namespace CarStoreManager.Domain.Enums;

public enum StatusOrdemServico
{
    Pendente = 1,                          // recém criada pelo recepcionista (rascunho de orçamento)
    EmVistoria = 2,                        // recepcionista está vistoriando o veículo e montando o contrato da OS
    BuscandoPecasParaOrcamento = 8,        // mecânico requisitou peça que não existe — admin precisa cotar/encomendar
    AguardandoCliente = 7,                 // vistoria concluída; recepcionista vai apresentar ao cliente
    Aprovada = 3,                          // cliente aprovou; pronta pra iniciar trabalho
    EmAndamento = 4,                       // mecânico iniciou o serviço
    Pausada = 9,                           // mecânico emitiu alerta — aguarda decisão do cliente sobre escopo aumentado
    PagamentoPendente = 11,                // mecânico terminou o trabalho técnico — falta receber pelo serviço
    Finalizada = 5,                        // trabalho técnico terminado E já pago — pronta pra entrega
    Entregue = 10,                         // recepção cobrou e entregou ao cliente (terminal feliz)
    Cancelada = 6                          // cancelada a qualquer momento
}
