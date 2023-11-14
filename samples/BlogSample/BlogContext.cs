using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Proxies.Internal;
using Penqueen.Samples.BlogSample.Proxy;
using Penqueen.Types;

namespace Penqueen.Samples.BlogSample
{
    [GenerateProxies]
    public class BlogContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }

        public BlogContext()
        {
        }

        public BlogContext(DbContextOptions options) : base(options) {}
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseLazyLoadingProxies()
                .UseBlogContextProxies()
                .UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EfCoreTests;Integrated Security=True;")
                .LogTo(Console.WriteLine);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var navigation = modelBuilder.Entity<Blog>().Navigation(_ => _.Posts).HasField("_posts");
        }
        public Blog AddBlog(Guid id, string name, int? sample)
        {
            var entityType = Model.FindRuntimeEntityType(typeof(Blog));
            var proxy = new BlogProxy(this, entityType, this.GetService<ILazyLoader>(), id, name, sample);
            Blogs.Add(proxy);
            return proxy;
        }
    }


    public interface IQueryableCollection<T> : ICollection<T>, IQueryable<T>{}
}
