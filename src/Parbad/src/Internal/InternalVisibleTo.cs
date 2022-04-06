// Copyright (c) Parbad.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Runtime.CompilerServices;

[assembly:
    InternalsVisibleTo("Parbad.Core.Owin"),
    InternalsVisibleTo("Parbad.Core.Mvc"),
    InternalsVisibleTo("Parbad.Core.AspNetCore"),
    InternalsVisibleTo("Parbad.Core.Storage.Cache"),
    InternalsVisibleTo("Parbad.Core.Storage.EntityFrameworkCore"),
    InternalsVisibleTo("Parbad.Core.Tests")]
