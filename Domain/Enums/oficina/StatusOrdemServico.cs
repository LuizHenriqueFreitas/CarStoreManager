namespace CarStoreManager.Domain.Enums;

public enum StatusOrdemServico
{
    Pendente = 1,            // recém criada pelo recepcionista (rascunho de orçamento)
    EmAnalise = 2,           // recepcionista enviou pra revisão do mecânico
    AguardandoCliente = 7,   // mecânico revisou; recepcionista vai apresentar ao cliente
    Aprovada = 3,            // cliente aprovou; pronta pra iniciar trabalho
    EmAndamento = 4,         // mecânico iniciou o serviço
    Finalizada = 5,          // serviço concluído
    Cancelada = 6            // cancelada a qualquer momento
}