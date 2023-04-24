﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Proxies.Internal;

namespace Penqueen.Tests.Domain.Manual
{
    //[GenerateProxies]
    public class BlogContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }


        private readonly IServiceProvider _internalServiceProvider;
        public BlogContext()
        {
            //            _internalServiceProvider
            //                = new ServiceCollection()
            ////                    .AddEntityFrameworkSqlServer()
            //                    .AddEntityFrameworkProxies()
            //                    .BuildServiceProvider(validateScopes: true);

        }

        public BlogContext(DbContextOptions options) : base(options) {}
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                //               .UseInternalServiceProvider(_internalServiceProvider)
                //               .UseChangeTrackingProxies()
                .ReplaceService<IProxyFactory, BlogContextProxyFactory>()
                .UseChangeTrackingProxies()
                .UseLazyLoadingProxies()
                .UseSqlServer(
                    "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EfCoreTests;Integrated Security=True;");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.Entity<Blog>().HasMany(_ => _.Posts).WithOne(_=>_.Blog)...HasField("_posts"));
            var navigation = modelBuilder.Entity<Blog>().Navigation(_ => _.Posts).HasField("_posts");//.UsePropertyAccessMode(PropertyAccessMode.Field);

            //navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
        }

        public Blog AddBlog(Guid id, string name, int? sample)
        {
            var entityType = Model.FindRuntimeEntityType(typeof(Blog));
            var proxy = new BlogProxy(id, name, sample, this, entityType, this.GetService<ILazyLoader>());
            Blogs.Add(proxy);
            return proxy;
        }
    }

}
