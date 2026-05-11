using CarStoreManager.Application.Services.Oficina.NotaFiscal;
using FluentAssertions;
using Xunit;

namespace CarStoreManager.Tests.Unidade.Services.NotaFiscal;

public class NotaFiscalXmlParserTest
{
    private const string XmlValido = """
        <?xml version="1.0" encoding="UTF-8"?>
        <nfeProc xmlns="http://www.portalfiscal.inf.br/nfe" versao="4.00">
          <NFe>
            <infNFe Id="NFe35260411222333000181550010000001231234567890" versao="4.00">
              <ide>
                <serie>1</serie>
                <nNF>123</nNF>
                <dhEmi>2026-05-10T10:00:00-03:00</dhEmi>
              </ide>
              <emit>
                <CNPJ>11222333000181</CNPJ>
                <xNome>Auto Pecas Brasil LTDA</xNome>
                <xFant>Auto Pecas BR</xFant>
              </emit>
              <dest>
                <CNPJ>99888777000166</CNPJ>
                <xNome>CarStore Oficina</xNome>
              </dest>
              <det nItem="1">
                <prod>
                  <cProd>PB-1234</cProd>
                  <xProd>Pastilha de Freio</xProd>
                  <NCM>87083010</NCM>
                  <CFOP>5102</CFOP>
                  <uCom>UN</uCom>
                  <qCom>10.0000</qCom>
                  <vUnCom>85.5000</vUnCom>
                </prod>
                <imposto>
                  <ICMS>
                    <ICMS00>
                      <CST>00</CST>
                      <pICMS>18.00</pICMS>
                      <vICMS>153.90</vICMS>
                    </ICMS00>
                  </ICMS>
                </imposto>
              </det>
              <total>
                <ICMSTot>
                  <vProd>855.00</vProd>
                  <vICMS>153.90</vICMS>
                  <vIPI>0.00</vIPI>
                  <vPIS>0.00</vPIS>
                  <vCOFINS>0.00</vCOFINS>
                  <vNF>855.00</vNF>
                </ICMSTot>
              </total>
            </infNFe>
          </NFe>
        </nfeProc>
        """;

    [Fact]
    public void Parse_XmlValido_ExtraiCabecalho()
    {
        var r = NotaFiscalXmlParser.Parse(XmlValido);

        r.IsSuccess.Should().BeTrue();
        var d = r.Value!;
        d.ChaveAcesso.Should().Be("35260411222333000181550010000001231234567890");
        d.Numero.Should().Be("123");
        d.Serie.Should().Be("1");
        d.EmitenteCnpj.Should().Be("11222333000181");
        d.EmitenteRazaoSocial.Should().Be("Auto Pecas Brasil LTDA");
        d.ValorProdutos.Should().Be(855.00m);
        d.ValorTotal.Should().Be(855.00m);
        d.ValorImpostos.Should().Be(153.90m);
    }

    [Fact]
    public void Parse_XmlValido_ExtraiItens()
    {
        var r = NotaFiscalXmlParser.Parse(XmlValido);
        r.IsSuccess.Should().BeTrue();

        var item = r.Value!.Itens.Single();
        item.CodigoProdutoFornecedor.Should().Be("PB-1234");
        item.DescricaoProdutoFornecedor.Should().Be("Pastilha de Freio");
        item.Ncm.Should().Be("87083010");
        item.Cfop.Should().Be("5102");
        item.Quantidade.Should().Be(10);
        item.ValorUnitario.Should().Be(85.50m);
        item.AliquotaIcms.Should().Be(18.00m);
        item.ValorIcms.Should().Be(153.90m);
        item.Cst.Should().Be("00");
    }

    [Fact]
    public void Parse_XmlVazio_RetornaFalha()
    {
        var r = NotaFiscalXmlParser.Parse("");
        r.IsSuccess.Should().BeFalse();
        r.Error.Should().Contain("vazio");
    }

    [Fact]
    public void Parse_XmlInvalido_RetornaFalha()
    {
        var r = NotaFiscalXmlParser.Parse("<not><valid");
        r.IsSuccess.Should().BeFalse();
        r.Error.Should().Contain("inválido");
    }

    [Fact]
    public void Parse_XmlSemInfNFe_RetornaFalha()
    {
        var r = NotaFiscalXmlParser.Parse("<root><a/></root>");
        r.IsSuccess.Should().BeFalse();
        r.Error.Should().Contain("infNFe");
    }

    [Fact]
    public void Parse_QuantidadeFracionaria_ArrendondaParaCima()
    {
        var xml = XmlValido.Replace("<qCom>10.0000</qCom>", "<qCom>2.5000</qCom>");
        var r = NotaFiscalXmlParser.Parse(xml);
        r.IsSuccess.Should().BeTrue();
        r.Value!.Itens.Single().Quantidade.Should().Be(3);
    }
}
