using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Penqueen.Tests.Domain.Manual.Configurations;

public static class BlogEntityTypeConfigurationMixin
{
    public static EntityTypeBuilder<Penqueen.Tests.Domain.Manual.Blog> ConfigureBackingFields(this EntityTypeBuilder<Penqueen.Tests.Domain.Manual.Blog> builder)
    {
        builder.Navigation(g => g.Posts).HasField("_posts").UsePropertyAccessMode(PropertyAccessMode.Field);
        return builder;
    }
}