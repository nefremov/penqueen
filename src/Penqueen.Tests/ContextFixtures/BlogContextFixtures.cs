using Microsoft.EntityFrameworkCore;

//using Penqueen.Tests.Domain.Manual;
using Penqueen.Tests.Domain.Generated;

using Xunit.Sdk;

namespace Penqueen.Tests
{
    public class BlogContextFixtures
    {
        [Fact]
        public void CanAddBlog()
        {
            Guid blogId = Guid.NewGuid();
            var (context, interceptor) = Setups.WithInterceptor();
            using (context)
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                Blog blog = context.AddBlog(blogId, "test", 1);
                context.SaveChanges();
            }

            using var context1 = new BlogContext();
            var actual = context1.Blogs.Count(b => b.Id == blogId);
            Assert.Equal(1, actual);
            Assert.Equal(1, interceptor.Counter);
        }
        [Fact]
        public void CanAddPostToExistingBlog()
        {
            Guid blogId = Guid.NewGuid();
            Guid postId = Guid.NewGuid();
            using var context1 = new BlogContext();
            context1.Database.EnsureDeleted();
            context1.Database.EnsureCreated();
            context1.AddBlog(blogId, "test", 1);
            context1.SaveChanges();

            var (context2, interceptor) = Setups.WithInterceptor();
            using (context2)
            {
                var blog = context2.Blogs.First(b => b.Id == blogId);
                blog.AddPost(postId, "Post Text");
                context2.SaveChanges();
            }

            using var context3 = new BlogContext();
            var actual = context3.Posts.Count(p => p.Id == postId);
            Assert.Equal(1, actual);
            Assert.Equal(2, interceptor.Counter); // read blog + write post
        }

        [Fact]
        public void CanAddPostToExistingBlogWithPosts()
        {
            Guid blogId = Guid.NewGuid();
            Guid postId = Guid.NewGuid();
            using var context1 = Setups.SetupBlogsAndPosts();
            context1.SaveChanges();

            var (context2, interceptor) = Setups.WithInterceptor();
            using (context2)
            {
                var blog = context2.Blogs.First(b => b.Id == Setups.Blogs.First().Id);
                blog.AddPost(postId, "New Post Text");
                context2.SaveChanges();
            }

            using var context3 = new BlogContext();
            var actual = context3.Posts.Count(p => p.Id == postId);
            Assert.Equal(1, actual); 
            actual = context3.Posts.Count(p => p.BlogId == Setups.Blogs.First().Id);
            Assert.Equal(Setups.Blogs.First().Posts.Length + 1, actual);
            Assert.Equal(2, interceptor.Counter); // read blog + write post
        }
    }
}