using CarStoreManager.Domain.Base;
using Oficina.Domain.Enums;

namespace Oficina.Domain.Entities;

public class ComponenteEquivalente : Entity
{
    public Guid ComponenteOriginalId { get; private set; }

    public Componente ComponenteOriginal { get; private set; } = null!;

    public Guid ComponenteEquivalenteId { get; private set; }

    public Componente ComponenteEquivalenteRelacionado { get; private set; } = null!;

    public TipoEquivalencia TipoEquivalencia { get; private set; }

    protected ComponenteEquivalente() {}

    public ComponenteEquivalente(
        Guid pecaOriginalId,
        Guid pecaEquivalenteId,
        TipoEquivalencia tipoEquivalencia)
    {
        ComponenteOriginalId = pecaOriginalId;
        ComponenteEquivalenteId = pecaEquivalenteId;
        TipoEquivalencia = tipoEquivalencia;
    }
}