namespace Penqueen.Tests.Domain.Manual
{
    public partial class Blog
    {

        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual int? Sample { get; set; }
        public virtual ICollection<Post> Posts { get; protected set; }

        protected Blog() { }

        protected Blog(Guid id, string name, int? sample)
        {
            Id = id;
            Name = name;
            Sample = sample;
        }

        public Post AddPost(Guid id, string text)
        {
            var post = ((Blog.IPostCollection)Posts).CreateNew(id, text, this);
            _posts.Add(post);
            return post;
        }
    }
}
