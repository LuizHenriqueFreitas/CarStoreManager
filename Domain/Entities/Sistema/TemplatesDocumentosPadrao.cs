namespace CarStoreManager.Domain.Entities.Sistema;

/// <summary>
/// Textos padrão (genéricos, mas formais) usados para pré-preencher os campos
/// de documento em texto livre do sistema — termo de entrega e contrato de
/// consignação — na primeira inicialização de <see cref="ConfiguracaoSistema"/>.
/// O administrador pode editá-los livremente na aba "Documentos" das
/// Configurações; isso só define o ponto de partida antes da customização.
/// </summary>
public static class TemplatesDocumentosPadrao
{
    public const string TermoEntrega = """
        TERMO DE ENTREGA DE VEÍCULO

        Pelo presente termo, [NOME DA LOJA/CONCESSIONÁRIA], entrega ao(à)
        Sr(a). [NOME DO CLIENTE], portador(a) do CPF nº [CPF DO CLIENTE], o
        veículo abaixo descrito, nas condições apresentadas no momento da venda:

        Marca/Modelo: [MARCA E MODELO]
        Ano: [ANO DE FABRICAÇÃO/MODELO]
        Cor: [COR]
        Placa: [PLACA]
        RENAVAM: [RENAVAM]
        Quilometragem na entrega: [QUILOMETRAGEM] km
        Valor da venda: R$ [VALOR]

        O(A) comprador(a) declara ter vistoriado o veículo no ato da entrega e
        confirma que:

        1. O veículo foi entregue em condições de uso, com os itens obrigatórios
           de segurança (estepe, macaco, triângulo, extintor, quando aplicável).
        2. Foram entregues os seguintes documentos e itens: manual do
           proprietário, [NÚMERO] chave(s), CRLV/documento de transferência,
           nota fiscal ou recibo de compra e venda.
        3. Eventuais avarias, defeitos ou pendências identificadas na vistoria
           estão descritas abaixo (ou "nenhuma", caso não haja):
           [DESCREVER PENDÊNCIAS, SE HOUVER]
        4. O(A) comprador(a) está ciente das condições de garantia oferecidas,
           conforme descrito abaixo:
           [CONDIÇÕES DE GARANTIA, SE HOUVER]

        Local e data: [CIDADE], [DATA]


        _______________________________          _______________________________
        Assinatura do(a) Comprador(a)             Assinatura do(a) Representante
        [NOME DO CLIENTE]                         [NOME DO VENDEDOR/REPRESENTANTE]
        """;

    public const string ContratoConsignacao = """
        CONTRATO DE CONSIGNAÇÃO PARA VENDA DE VEÍCULO

        Pelo presente instrumento particular, de um lado [NOME DA LOJA/
        CONCESSIONÁRIA], inscrita no CNPJ nº [CNPJ DA LOJA], doravante
        denominada CONSIGNATÁRIA, e de outro lado [NOME DO PROPRIETÁRIO],
        portador(a) do CPF nº [CPF DO PROPRIETÁRIO], doravante denominado(a)
        CONSIGNANTE, têm entre si justo e acordado o seguinte:

        1. OBJETO
        O CONSIGNANTE entrega à CONSIGNATÁRIA, em regime de consignação, o
        veículo abaixo descrito, para fins exclusivos de exposição e venda:

        Marca/Modelo: [MARCA E MODELO]
        Ano: [ANO DE FABRICAÇÃO/MODELO]
        Cor: [COR]
        Placa: [PLACA]
        RENAVAM: [RENAVAM]
        Quilometragem na entrada: [QUILOMETRAGEM] km

        2. PRAZO
        Este contrato vigora pelo prazo de [PRAZO EM DIAS] dias corridos,
        contados a partir da data de assinatura, podendo ser renovado por
        igual período mediante acordo entre as partes.

        3. VALOR E COMISSÃO
        O CONSIGNANTE espera obter, com a venda do veículo, o valor líquido de
        R$ [VALOR ESPERADO PELO PROPRIETÁRIO]. A CONSIGNATÁRIA fará jus a uma
        comissão de [PERCENTUAL DE COMISSÃO]% sobre o valor recebido na venda
        (ou ao valor fixo de R$ [VALOR FIXO DE COMISSÃO], conforme acordado).

        4. RESPONSABILIDADES DA CONSIGNATÁRIA
        4.1. Zelar pela guarda e conservação do veículo enquanto estiver em seu
             poder;
        4.2. Expor e anunciar o veículo pelos meios usuais de venda;
        4.3. Repassar ao CONSIGNANTE o valor da venda, deduzida a comissão
             acordada, em até [PRAZO DE REPASSE] dias úteis após o recebimento
             do pagamento do comprador.

        5. RESPONSABILIDADES DO CONSIGNANTE
        5.1. Garantir que o veículo está livre de ônus, dívidas, multas e
             restrições judiciais/administrativas não informadas previamente;
        5.2. Entregar a documentação necessária para a transferência (CRLV,
             recibo de venda assinado, com firma reconhecida quando exigido).

        6. RESCISÃO
        Qualquer das partes pode rescindir este contrato antes do prazo final,
        mediante aviso prévio de [PRAZO DE AVISO PRÉVIO] dias, desde que o
        veículo ainda não esteja em processo de venda com terceiro.

        Local e data: [CIDADE], [DATA]


        _______________________________          _______________________________
        Assinatura do(a) CONSIGNANTE               Assinatura da CONSIGNATÁRIA
        [NOME DO PROPRIETÁRIO]                     [NOME DO REPRESENTANTE DA LOJA]
        """;

    /// <summary>
    /// Roteiro de campos pra o vendedor preencher com o que a financiadora
    /// informou por fora do sistema (ligação, e-mail, portal do parceiro) —
    /// não é uma simulação nem um cálculo, só um ponto de partida pra não
    /// esquecer nenhum dado na hora de anotar a resposta.
    /// </summary>
    public const string RespostaFinanciadora = """
        Financiadora: [NOME DA FINANCIADORA]
        Contato: [NOME DO ATENDENTE / TELEFONE / E-MAIL]
        Data do retorno: [DATA]

        Condições propostas:
        - Valor financiado: R$ [VALOR]
        - Número de parcelas: [PARCELAS]
        - Valor aproximado da parcela: R$ [VALOR DA PARCELA]
        - Taxa de juros informada: [TAXA]
        - Carência/primeira parcela: [PRAZO]

        Observações da financiadora: [CONDIÇÕES ADICIONAIS, EXIGÊNCIAS DE DOCUMENTAÇÃO, VALIDADE DA PROPOSTA, ETC.]
        """;
}
