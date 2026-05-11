using System.Globalization;
using System.Xml.Linq;
using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.NotaFiscal;

namespace CarStoreManager.Application.Services.Oficina.NotaFiscal;

/// <summary>
/// Parser para NF-e modelo 55 (versão 4.00). Lê o envelope <c>nfeProc</c>
/// padrão da SEFAZ ou o <c>NFe</c> raiz quando o XML não está envelopado.
///
/// Não faz validação de assinatura nem consulta SEFAZ — só extrai o conteúdo
/// estrutural para o service decidir o que fazer.
/// </summary>
public static class NotaFiscalXmlParser
{
    private static readonly XNamespace Ns = "http://www.portalfiscal.inf.br/nfe";

    public static Result<XmlNotaFiscalParseado> Parse(string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            return Result<XmlNotaFiscalParseado>.Fail("XML vazio.");

        XDocument doc;
        try
        {
            doc = XDocument.Parse(xml);
        }
        catch (Exception ex)
        {
            return Result<XmlNotaFiscalParseado>.Fail($"XML inválido: {ex.Message}");
        }

        var infNFe = doc.Descendants(Ns + "infNFe").FirstOrDefault();
        if (infNFe is null)
            return Result<XmlNotaFiscalParseado>.Fail(
                "Estrutura inválida: elemento <infNFe> não encontrado. Esperado NF-e modelo 55 v4.00.");

        try
        {
            var resultado = new XmlNotaFiscalParseado
            {
                XmlOriginal = xml,
                ChaveAcesso = ExtrairChaveAcesso(infNFe)
            };

            ExtrairIdentificacao(infNFe, resultado);
            ExtrairEmitente(infNFe, resultado);
            ExtrairDestinatario(infNFe, resultado);
            ExtrairItens(infNFe, resultado);
            ExtrairTotais(infNFe, resultado);

            if (resultado.Itens.Count == 0)
                return Result<XmlNotaFiscalParseado>.Fail("NF-e sem itens (det).");

            if (string.IsNullOrEmpty(resultado.EmitenteCnpj))
                return Result<XmlNotaFiscalParseado>.Fail("CNPJ do emitente ausente no XML.");

            return Result<XmlNotaFiscalParseado>.Ok(resultado);
        }
        catch (Exception ex)
        {
            return Result<XmlNotaFiscalParseado>.Fail($"Falha ao interpretar XML: {ex.Message}");
        }
    }

    private static string ExtrairChaveAcesso(XElement infNFe)
    {
        // Id="NFe" + 44 dígitos.
        var idAttr = infNFe.Attribute("Id")?.Value ?? string.Empty;
        if (idAttr.StartsWith("NFe", StringComparison.OrdinalIgnoreCase) && idAttr.Length == 47)
            return idAttr[3..];
        // Fallback: tentar via protNFe/infProt/chNFe.
        var chNFe = infNFe.Document?
            .Descendants(Ns + "chNFe")
            .FirstOrDefault()?.Value;
        return chNFe ?? string.Empty;
    }

    private static void ExtrairIdentificacao(XElement infNFe, XmlNotaFiscalParseado r)
    {
        var ide = infNFe.Element(Ns + "ide");
        if (ide is null) return;

        r.Numero = ide.Element(Ns + "nNF")?.Value ?? "";
        r.Serie = ide.Element(Ns + "serie")?.Value ?? "";
        var dhEmi = ide.Element(Ns + "dhEmi")?.Value
                 ?? ide.Element(Ns + "dEmi")?.Value
                 ?? "";
        if (DateTime.TryParse(dhEmi, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var dt))
            r.DataEmissao = dt;
    }

    private static void ExtrairEmitente(XElement infNFe, XmlNotaFiscalParseado r)
    {
        var emit = infNFe.Element(Ns + "emit");
        if (emit is null) return;

        r.EmitenteCnpj = OnlyDigits(emit.Element(Ns + "CNPJ")?.Value ?? "");
        r.EmitenteRazaoSocial = (emit.Element(Ns + "xNome")?.Value ?? "").Trim();
        r.EmitenteNomeFantasia = (emit.Element(Ns + "xFant")?.Value ?? r.EmitenteRazaoSocial).Trim();

        // E-mail e telefone não são obrigatórios na NF-e — usamos placeholders
        // se ausentes para satisfazer os ValueObjects do Fornecedor.
        var ender = emit.Element(Ns + "enderEmit");
        r.EmitenteTelefone = ender?.Element(Ns + "fone")?.Value ?? "";
        r.EmitenteEmail = emit.Element(Ns + "email")?.Value ?? "";
    }

    private static void ExtrairDestinatario(XElement infNFe, XmlNotaFiscalParseado r)
    {
        var dest = infNFe.Element(Ns + "dest");
        if (dest is null) return;
        r.DestinatarioCnpj = OnlyDigits(
            dest.Element(Ns + "CNPJ")?.Value
            ?? dest.Element(Ns + "CPF")?.Value
            ?? "");
        r.DestinatarioRazaoSocial = (dest.Element(Ns + "xNome")?.Value ?? "").Trim();
    }

    private static void ExtrairItens(XElement infNFe, XmlNotaFiscalParseado r)
    {
        foreach (var det in infNFe.Elements(Ns + "det"))
        {
            var prod = det.Element(Ns + "prod");
            if (prod is null) continue;

            var imposto = det.Element(Ns + "imposto");
            var (cst, csosn, aliq, vIcms) = ExtrairIcms(imposto);

            var qComStr = prod.Element(Ns + "qCom")?.Value ?? "0";
            var qCom = decimal.Parse(qComStr, CultureInfo.InvariantCulture);

            // Estoque do sistema é em unidades inteiras — arredonda pra cima
            // se for fracionário (não fica < 1) e o admin revisa antes de aprovar.
            var quantidade = (int)Math.Ceiling(qCom);
            if (quantidade < 1) quantidade = 1;

            var item = new XmlItemNotaFiscalParseado
            {
                CodigoProdutoFornecedor = prod.Element(Ns + "cProd")?.Value ?? "",
                DescricaoProdutoFornecedor = prod.Element(Ns + "xProd")?.Value ?? "",
                Ncm = prod.Element(Ns + "NCM")?.Value ?? "",
                Unidade = prod.Element(Ns + "uCom")?.Value ?? "UN",
                Quantidade = quantidade,
                ValorUnitario = decimal.Parse(
                    prod.Element(Ns + "vUnCom")?.Value ?? "0",
                    CultureInfo.InvariantCulture),
                Cfop = prod.Element(Ns + "CFOP")?.Value ?? "",
                Cst = cst,
                Csosn = csosn,
                AliquotaIcms = aliq,
                ValorIcms = vIcms
            };

            // Rastreabilidade — bloco rastro é opcional mas comum em medicamentos/peças.
            var rastro = prod.Element(Ns + "rastro");
            if (rastro is not null)
            {
                item.NumeroLoteFornecedor = rastro.Element(Ns + "nLote")?.Value;
                if (DateTime.TryParse(rastro.Element(Ns + "dFab")?.Value, out var df))
                    item.DataFabricacao = df;
                if (DateTime.TryParse(rastro.Element(Ns + "dVal")?.Value, out var dv))
                    item.DataValidade = dv;
            }

            r.Itens.Add(item);
        }
    }

    private static (string? cst, string? csosn, decimal aliq, decimal vIcms) ExtrairIcms(XElement? imposto)
    {
        if (imposto is null) return (null, null, 0m, 0m);
        var icmsRoot = imposto.Element(Ns + "ICMS");
        if (icmsRoot is null) return (null, null, 0m, 0m);

        // O nó filho varia (ICMS00, ICMS10, ICMSSN102, etc.) — pegamos o primeiro.
        var icms = icmsRoot.Elements().FirstOrDefault();
        if (icms is null) return (null, null, 0m, 0m);

        var cst = icms.Element(Ns + "CST")?.Value;
        var csosn = icms.Element(Ns + "CSOSN")?.Value;
        var aliq = decimal.Parse(
            icms.Element(Ns + "pICMS")?.Value ?? "0",
            CultureInfo.InvariantCulture);
        var vIcms = decimal.Parse(
            icms.Element(Ns + "vICMS")?.Value ?? "0",
            CultureInfo.InvariantCulture);
        return (cst, csosn, aliq, vIcms);
    }

    private static void ExtrairTotais(XElement infNFe, XmlNotaFiscalParseado r)
    {
        var icmsTot = infNFe.Element(Ns + "total")?.Element(Ns + "ICMSTot");
        if (icmsTot is null) return;

        r.ValorProdutos = decimal.Parse(
            icmsTot.Element(Ns + "vProd")?.Value ?? "0",
            CultureInfo.InvariantCulture);

        // vNF é o valor final da nota; soma de impostos é a diferença.
        r.ValorTotal = decimal.Parse(
            icmsTot.Element(Ns + "vNF")?.Value ?? "0",
            CultureInfo.InvariantCulture);

        // Soma componentes de imposto explicitamente (mais auditável que vNF - vProd).
        var vIcms = decimal.Parse(icmsTot.Element(Ns + "vICMS")?.Value ?? "0", CultureInfo.InvariantCulture);
        var vIpi = decimal.Parse(icmsTot.Element(Ns + "vIPI")?.Value ?? "0", CultureInfo.InvariantCulture);
        var vPis = decimal.Parse(icmsTot.Element(Ns + "vPIS")?.Value ?? "0", CultureInfo.InvariantCulture);
        var vCofins = decimal.Parse(icmsTot.Element(Ns + "vCOFINS")?.Value ?? "0", CultureInfo.InvariantCulture);
        r.ValorImpostos = vIcms + vIpi + vPis + vCofins;
    }

    private static string OnlyDigits(string s)
        => new string(s.Where(char.IsDigit).ToArray());
}
