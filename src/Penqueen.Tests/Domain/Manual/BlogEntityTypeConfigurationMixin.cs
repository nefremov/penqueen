using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Penqueen.Tests.Domain.Manual.Configuration;

public static class BlogEntityTypeConfigurationMixin
{
    public static EntityTypeBuilder<Penqueen.Tests.Domain.Manual.Blog> ConfigureBackingFields(this EntityTypeBuilder<Penqueen.Tests.Domain.Manual.Blog> builder)
    {
        builder.Navigation(g => g.Posts).HasField("_posts");
        return builder;
    }
}