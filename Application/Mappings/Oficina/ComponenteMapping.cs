using CarStoreManager.Application.DTOs.Oficina.Componente;
using CarStoreManager.Domain.Entities.Oficina;

namespace CarStoreManager.Application.Mappings.Oficina;

public static class ComponenteMapping
{
    // =========================
    // Entity → DTO
    // =========================
    public static ComponenteDTO ToDto(Componente entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        return new ComponenteDTO
        {
            Id = entity.Id,
            SKUInterno = entity.SKUInterno,
            Nome = entity.Nome,
            Descricao = entity.Descricao,
            MarcaFabricante = entity.MarcaFabricante,
            PartNumber = entity.PartNumber,
            CodigoOEM = entity.CodigoOEM,
            CodigoBarras = entity.CodigoBarras,
            NCM = entity.NCM,
            CEST = entity.CEST,
            Categoria = entity.Categoria,
            Unidade = entity.Unidade,
            Sistema = entity.Sistema?.ToString() ?? string.Empty,
            Peso = entity.Peso,
            GarantiaDias = entity.GarantiaDias,
            Ativo = entity.Ativo,
            CustoUnitario = entity.CustoUnitario,
            MargemLucroPct = entity.MargemLucroPct,
            ValorVenda = entity.ValorVenda
        };
    }

    public static ComponenteListaDTO ToListaDto(Componente entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        return new ComponenteListaDTO
        {
            Id = entity.Id,
            SKUInterno = entity.SKUInterno,
            Nome = entity.Nome,
            MarcaFabricante = entity.MarcaFabricante,
            PartNumber = entity.PartNumber,
            CodigoOEM = entity.CodigoOEM,
            CodigoBarras = entity.CodigoBarras,
            Categoria = entity.Categoria,
            Sistema = entity.Sistema?.ToString() ?? string.Empty,
            Ativo = entity.Ativo
        };
    }

    public static Componente FromCriarDto(CriarComponenteDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        return new Componente(
            skuInterno: dto.SKUInterno,
            nome: dto.Nome,
            descricao: dto.Descricao,
            marcaFabricante: dto.MarcaFabricante,
            partNumber: dto.PartNumber,
            codigoOEM: dto.CodigoOEM,
            codigoBarras: dto.CodigoBarras,
            ncm: dto.NCM,
            cest: dto.CEST,
            categoria: dto.Categoria,
            unidade: dto.Unidade,
            peso: dto.Peso,
            garantiaDias: dto.GarantiaDias
        );
    }

    public static void ApplyAtualizarDto(Componente entity, AtualizarComponenteDTO dto)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        entity.SetNome(dto.Nome);
        entity.SetDescricao(dto.Descricao);
        entity.SetMarcaFabricante(dto.MarcaFabricante);
        entity.SetPartNumber(dto.PartNumber);
        entity.SetCodigoOEM(dto.CodigoOEM);
        entity.SetCodigoBarras(dto.CodigoBarras);
        entity.SetNCM(dto.NCM);
        entity.SetCEST(dto.CEST);
        entity.SetCategoria(dto.Categoria);
        entity.SetUnidade(dto.Unidade);
        entity.SetPeso(dto.Peso);
        entity.SetGarantiaDias(dto.GarantiaDias);

        if (dto.Ativo && !entity.Ativo) entity.Ativar();
        else if (!dto.Ativo && entity.Ativo) entity.Desativar();
    }

    // =========================
    // Para criação a partir do ComponenteDTO genérico (compat)
    // =========================
    public static Componente ToEntity(ComponenteDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        return new Componente(
            skuInterno: dto.SKUInterno,
            nome: dto.Nome,
            descricao: dto.Descricao,
            marcaFabricante: dto.MarcaFabricante,
            partNumber: dto.PartNumber,
            codigoOEM: dto.CodigoOEM,
            codigoBarras: dto.CodigoBarras,
            ncm: dto.NCM,
            cest: dto.CEST,
            categoria: dto.Categoria,
            unidade: dto.Unidade,
            peso: dto.Peso,
            garantiaDias: dto.GarantiaDias
        );
    }

    public static void UpdateEntity(Componente entity, ComponenteDTO dto)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        entity.SetNome(dto.Nome);
        entity.SetDescricao(dto.Descricao);
        entity.SetMarcaFabricante(dto.MarcaFabricante);
        entity.SetPartNumber(dto.PartNumber);
        entity.SetCodigoOEM(dto.CodigoOEM);
        entity.SetCodigoBarras(dto.CodigoBarras);
        entity.SetNCM(dto.NCM);
        entity.SetCEST(dto.CEST);
        entity.SetCategoria(dto.Categoria);
        entity.SetUnidade(dto.Unidade);
        entity.SetPeso(dto.Peso);
        entity.SetGarantiaDias(dto.GarantiaDias);

        if (dto.Ativo && !entity.Ativo) entity.Ativar();
        else if (!dto.Ativo && entity.Ativo) entity.Desativar();
    }
}
