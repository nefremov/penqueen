using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Proxies.Internal;

using Penqueen.Tests.Domain.Generated.Proxy;
using Penqueen.Types;

using System.Diagnostics;
using Penqueen.CodeGenerators;

namespace Penqueen.Tests.Domain.Generated
{
    [GenerateProxies(CustomProxies = true, ConfigurationMixins = true)]
    public class BlogContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }


        private readonly IServiceProvider _internalServiceProvider;

        public BlogContext() { }
        public BlogContext(DbContextOptions options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                //.UseInternalServiceProvider(_internalServiceProvider)
                .UseChangeTrackingProxies()
                .UseLazyLoadingProxies()
                .UseBlogContextProxies()
                .LogTo(message => Debug.WriteLine(message))
                .UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EfCoreTests;Integrated Security=True;");
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.Entity<Blog>().HasMany(_ => _.Posts).WithOne(_=>_.Blog)...HasField("_posts"));
            var navigation = modelBuilder.Entity<Blog>().Navigation(_ => _.Posts).HasField("_posts");//.UsePropertyAccessMode(PropertyAccessMode.Field);

            //navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
        }

        public Blog AddBlog(Guid id, string name, SampleEnum enumProp, int? sample)
        {
            var entityType = Model.FindRuntimeEntityType(typeof(Blog));
            var proxy = new BlogProxy(this, entityType, this.GetService<ILazyLoader>(), id, name, enumProp, sample);
            Blogs.Add(proxy);
            return proxy;
        }
        public Blog AddBlog(Guid id, string name, SampleEnum enumProp, int? sample, IEnumerable<PostItem> posts)
        {
            var entityType = Model.FindRuntimeEntityType(typeof(Blog));
            var proxy = new BlogProxy(this, entityType, this.GetService<ILazyLoader>(), id, name, enumProp, sample, posts);
            Blogs.Add(proxy);
            return proxy;
        }
    }
}
