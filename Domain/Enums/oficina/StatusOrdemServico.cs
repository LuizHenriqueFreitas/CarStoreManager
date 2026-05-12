namespace CarStoreManager.Domain.Enums;

public enum StatusOrdemServico
{
    Pendente = 1,                          // recém criada pelo recepcionista (rascunho de orçamento)
    EmAnalise = 2,                         // recepcionista enviou pra revisão do mecânico
    BuscandoPecasParaOrcamento = 8,        // mecânico requisitou peça que não existe — admin precisa cotar/encomendar
    AguardandoCliente = 7,                 // mecânico revisou; recepcionista vai apresentar ao cliente
    Aprovada = 3,                          // cliente aprovou; pronta pra iniciar trabalho
    EmAndamento = 4,                       // mecânico iniciou o serviço
    Pausada = 9,                           // mecânico emitiu alerta — aguarda decisão do cliente sobre escopo aumentado
    Finalizada = 5,                        // mecânico terminou o trabalho técnico — aguarda cobrança/entrega pela recepção
    Entregue = 10,                         // recepção cobrou e entregou ao cliente (terminal feliz)
    Cancelada = 6                          // cancelada a qualquer momento
}
