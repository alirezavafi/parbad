﻿// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persian.Plus.PaymentGateway.Storage.EntityFrameworkCore.Options;

namespace Persian.Plus.PaymentGateway.Storage.EntityFrameworkCore.Configuration
{
    internal static class EntityTypeConfigurationExtensions
    {
        public static EntityTypeBuilder ToTable(this EntityTypeBuilder entityTypeBuilder, TableOptions options, string defaultSchema)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            var schema = string.IsNullOrWhiteSpace(options.Schema) ? defaultSchema : options.Schema;

            if (string.IsNullOrWhiteSpace(options.Name))
            {
                throw new Exception("Table name cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(schema))
            {
                entityTypeBuilder.ToTable(options.Name);
            }
            else
            {
                entityTypeBuilder.ToTable(options.Name, schema);
            }

            return entityTypeBuilder;
        }
    }
}
