using CarStoreManager.Domain.Base;

namespace CarStoreManager.Domain.Entities;

/*
    Entidade genérica de foto vinculada a outras entidades por
    EntidadeTipo + EntidadeId (ex.: "VeiculoVenda", "Componente").
    Mantém referência opcional a VeiculoVendaId para permitir o
    relacionamento HasMany existente com VeiculoVenda no EF.
*/
public class Foto : Entity
{
    public string EntidadeTipo { get; private set; } = null!;
    public Guid EntidadeId { get; private set; }

    // FK opcional para o relacionamento direto com VeiculoVenda no EF.
    public Guid? VeiculoVendaId { get; private set; }

    public string Url { get; private set; } = null!;
    public string NomeArquivo { get; private set; } = null!;
    public long TamanhoBytes { get; private set; }
    public string ContentType { get; private set; } = null!;
    public DateTime DataUpload { get; private set; }
    public int Ordem { get; private set; }

    protected Foto() { }

    public Foto(
        string entidadeTipo,
        Guid entidadeId,
        string url,
        string nomeArquivo,
        long tamanhoBytes,
        string contentType,
        int ordem)
    {
        if (string.IsNullOrWhiteSpace(entidadeTipo))
            throw new ArgumentException("EntidadeTipo é obrigatório.", nameof(entidadeTipo));
        if (entidadeId == Guid.Empty)
            throw new ArgumentException("EntidadeId é obrigatório.", nameof(entidadeId));
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Url é obrigatória.", nameof(url));

        EntidadeTipo = entidadeTipo;
        EntidadeId = entidadeId;
        Url = url;
        NomeArquivo = nomeArquivo ?? string.Empty;
        TamanhoBytes = tamanhoBytes;
        ContentType = contentType ?? string.Empty;
        DataUpload = DateTime.UtcNow;
        Ordem = ordem;

        // Mantém compatibilidade com o relacionamento de VeiculoVenda.
        if (string.Equals(entidadeTipo, "VeiculoVenda", StringComparison.OrdinalIgnoreCase))
            VeiculoVendaId = entidadeId;
    }

    // Construtor de conveniência para uso direto a partir de VeiculoVenda.AdicionarFoto(url).
    public Foto(Guid veiculoVendaId, string url, int ordem)
        : this("VeiculoVenda", veiculoVendaId, url, string.Empty, 0, string.Empty, ordem)
    { }

    public void AtualizarOrdem(int novaOrdem) => Ordem = novaOrdem;
}
