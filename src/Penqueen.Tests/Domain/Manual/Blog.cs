using Penqueen.CodeGenerators;
using Penqueen.Collections;

namespace Penqueen.Tests.Domain.Manual
{
    public class Blog
    {

        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual int? Sample { get; protected set; }
        public virtual SampleEnum EnumParam { get; protected set; }
        public virtual IQueryableCollection<Post> Posts { get; protected set; }

        protected Action? CollectionsInitialized;

        protected ICollection<Post>? _posts;

        protected Blog() { }

        protected Blog(Guid id, string name, SampleEnum enumParam = SampleEnum.Second, int? sample = null)
        {
            Id = id;
            Name = name;
            EnumParam = enumParam;
            Sample = sample;
        }

        protected Blog(Guid id, string name, SampleEnum enumParam = SampleEnum.Second, int? sample = null, IEnumerable<PostItem>? posts = null)
        {
            Id = id;
            Name = name;
            EnumParam = enumParam;
            Sample = sample;

            CollectionsInitialized =
                () =>
                {
                    foreach (var postData in posts)
                    {
                        ((IPostCollection) Posts).CreateNew(postData.Id, postData.Text, this);
                    }
                };
        }

        public Post AddPost(Guid id, string text)
        {
            var post = ((IPostCollection)Posts).CreateNew(id, text, this);
            return post;
        }
    }
}
