using Microsoft.EntityFrameworkCore;

using Penqueen.Types;

namespace Penqueen.Tests.Domain.Generated
{
    [GenerateProxies(CustomProxies = false, ConfigurationMixins = true)]
    public class DummyContext : DbContext
    {
        public DbSet<DummyEntity> DummyEntities { get; set; }
    }

    public class DummyEntity
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }

        protected DummyEntity(){}

    }
}
