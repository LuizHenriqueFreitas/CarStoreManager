namespace CarStoreManager.Application.Common;

/*
    Abstração de um arquivo enviado para upload.
    Permite que Application receba arquivos sem depender de AspNetCore (IFormFile).
    A camada Web é responsável por adaptar IFormFile → ArquivoUpload.
*/
public sealed class ArquivoUpload
{
    public string NomeArquivo { get; }
    public long Tamanho { get; }
    public string ContentType { get; }
    public Func<Stream> AbrirStream { get; }

    public ArquivoUpload(string nomeArquivo, long tamanho, string contentType, Func<Stream> abrirStream)
    {
        if (string.IsNullOrWhiteSpace(nomeArquivo))
            throw new ArgumentException("Nome do arquivo é obrigatório.", nameof(nomeArquivo));
        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Content-Type é obrigatório.", nameof(contentType));
        if (abrirStream is null)
            throw new ArgumentNullException(nameof(abrirStream));

        NomeArquivo = nomeArquivo;
        Tamanho = tamanho;
        ContentType = contentType;
        AbrirStream = abrirStream;
    }
}
