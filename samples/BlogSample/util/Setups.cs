using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Penqueen.Samples.BlogSample;

public record PostItem(Guid Id, string Text);
public record BlogItem(Guid Id, string Name, int? Sample, PostItem[] Posts);

public static class Setups
{
    public static BlogItem[] Blogs =
    {
        new(
            Guid.NewGuid(),
            "Blog 1",
            0,
            new PostItem[]
            {
                new(Guid.NewGuid(), "Post 1 text"),
                new(Guid.NewGuid(), "Post 2 text"),
                new(Guid.NewGuid(), "Post 3 text"),
            }
        ),
        new(
            Guid.NewGuid(),
            "Blog 2",
            0,
            new PostItem[]
            {
                new(Guid.NewGuid(), "Post 3 text"),
                new(Guid.NewGuid(), "Post 4 text"),
                new(Guid.NewGuid(), "Post 5 text"),
                new(Guid.NewGuid(), "Post 6 text"),
            }
        )
    };

    public static BlogContext SetupBlogsAndPosts()
    {
        var context = new BlogContext();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        foreach (BlogItem blogItem in Blogs)
        {
            var blog = context.AddBlog(blogItem.Id, blogItem.Name, blogItem.Sample);
            foreach (PostItem postItem in blogItem.Posts)
            {
                var post = blog.AddPost(postItem.Id, postItem.Text);
            }

            context.Blogs.Add(blog);
        }

        return context;
    }

    public static (BlogContext context, CountingCommandInterceptor interceptor) WithInterceptor()
    {
        var interceptor = new CountingCommandInterceptor();
        var options = new DbContextOptionsBuilder()
            .AddInterceptors(interceptor)
            .Options;

        var context = new BlogContext(options);
        return (context, interceptor);
    }
}

public class CountingCommandInterceptor : DbCommandInterceptor
{
    public int Counter { get; private set; }

    public void Reset()
    {
        Counter = 0;
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        Counter++;

        return result;
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        Counter++;

        return new ValueTask<InterceptionResult<DbDataReader>>(result);
    }

    public override InterceptionResult<object> ScalarExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
    {
        Counter++;
        return base.ScalarExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<object> result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        Counter++;
        return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
    }
}
