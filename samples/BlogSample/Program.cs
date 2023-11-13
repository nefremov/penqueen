
using Penqueen.Samples.BlogSample;

using (var context1 = Setups.SetupBlogsAndPosts())
{
    context1.SaveChanges();
}

using (var context6 = new BlogContext())
{
    var blog3 = context6.Blogs.First(b => b.Id == Setups.Blogs.First().Id);

    var post = blog3.Posts.AsQueryable().First(p => p.Id == Setups.Blogs.First().Posts.First().Id);
}


