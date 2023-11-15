﻿using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Penqueen.Tests.Domain.Manual.Configuration;

public static class PostEntityTypeConfigurationMixin
{
    public static EntityTypeBuilder<Penqueen.Tests.Domain.Manual.Post> ConfigureBackingFields(this EntityTypeBuilder<Penqueen.Tests.Domain.Manual.Post> builder)
    {
        return builder;
    }
}