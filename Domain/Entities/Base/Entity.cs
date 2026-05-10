namespace CarStoreManager.Domain.Base
{
    public abstract class Entity
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();

        public DateTime DataCriacao { get; protected set; } = DateTime.UtcNow;
    }
}
