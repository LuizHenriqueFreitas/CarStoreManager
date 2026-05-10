// Application/Mappings/Oficina/ComponenteMapping.cs
using CarStoreManager.Application.DTOs.Oficina.Componente;
using Oficina.Domain.Entities;

namespace CarStoreManager.Application.Mappings.Oficina;

public static class ComponenteMapping
{
    // =========================
    // Entity → DTO
    // =========================
    public static ComponenteDTO ToDto(Componente entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

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
            Peso = entity.Peso,
            GarantiaDias = entity.GarantiaDias,
            Ativo = entity.Ativo
        };
    }

    // =========================
    // DTO → Entity (criação)
    // =========================
    public static Componente ToEntity(ComponenteDTO dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

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
        // Obs.: O construtor já define Ativo = true por padrão.
        // Se quiser permitir criar inativo, deve ajustar o construtor.
    }

    // =========================
    // DTO → Entity (atualização)
    // =========================
    public static void UpdateEntity(Componente entity, ComponenteDTO dto)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        // SKU não deve ser alterado após criação (é chave interna)
        // entity.SetSKUInterno(dto.SKUInterno); // comente se não quiser permitir

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
        
        if (dto.Ativo && !entity.Ativo)
            entity.Ativar();
        else if (!dto.Ativo && entity.Ativo)
            entity.Desativar();
    }
}