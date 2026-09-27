using CarStoreManager.Domain.Base;

namespace CarStoreManager.Domain.Entities.Oficina;

/// <summary>
/// Vistoria de entrada da OS — o recepcionista registra o estado do veículo
/// na chegada e o que será feito, na presença do cliente. É o próprio
/// "contrato da OS": texto editável (pode partir de um template de
/// Configurações → Documentos, via SeletorTemplateDocumento) + fotos
/// anexadas (Foto.EntidadeTipo="VistoriaOrdemServico", EntidadeId=Id desta
/// vistoria). Ciclo de vida em 2 fases, mesmo padrão de
/// Concessionaria.Vistoria:
///   1) Recepcionista clica "iniciar vistoria" → cria com Concluida=false,
///      pode editar o texto e anexar fotos.
///   2) Recepcionista clica "concluir vistoria" → Concluida=true, texto
///      trava; a OS avança pra AguardandoCliente.
/// Sem assinatura eletrônica própria — o aceite do cliente continua sendo
/// o "Cliente aprovou" já existente (AguardandoCliente → Aprovada).
/// </summary>
public class VistoriaOrdemServico : Entity
{
    public Guid OrdemServicoId { get; private set; }
    public Guid RecepcionistaId { get; private set; }
    public string TextoContrato { get; private set; } = "";
    public DateTime DataInicio { get; private set; }
    public DateTime? DataConclusao { get; private set; }
    public bool Concluida { get; private set; }

    protected VistoriaOrdemServico() { }

    public VistoriaOrdemServico(Guid ordemServicoId, Guid recepcionistaId)
    {
        if (ordemServicoId == Guid.Empty) throw new ArgumentException("Ordem de serviço inválida.", nameof(ordemServicoId));
        if (recepcionistaId == Guid.Empty) throw new ArgumentException("Recepcionista inválido.", nameof(recepcionistaId));

        OrdemServicoId = ordemServicoId;
        RecepcionistaId = recepcionistaId;
        DataInicio = DateTime.UtcNow;
        Concluida = false;
    }

    public void EditarTexto(string novoTexto)
    {
        if (Concluida)
            throw new InvalidOperationException("Vistoria já concluída não pode ser editada.");

        TextoContrato = novoTexto?.Trim() ?? "";
    }

    public void Concluir(string textoFinal)
    {
        if (Concluida)
            throw new InvalidOperationException("Vistoria já foi concluída.");
        if (string.IsNullOrWhiteSpace(textoFinal))
            throw new ArgumentException("O texto do contrato é obrigatório para concluir a vistoria.", nameof(textoFinal));

        TextoContrato = textoFinal.Trim();
        Concluida = true;
        DataConclusao = DateTime.UtcNow;
    }
}
