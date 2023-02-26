﻿using System;
using System.Collections.Immutable;
using System.IO;
using Intellenum;
using Microsoft.CodeAnalysis.Testing;

namespace AnalyzerTests
{
    public static class References
    {
        static readonly string _loc = typeof(IntellenumAttribute).Assembly.Location;

        public static Lazy<ReferenceAssemblies> Net70AndOurs = new(() =>
            new ReferenceAssemblies(
                    "net7.0",
                    new PackageIdentity(
                        "Microsoft.NETCore.App.Ref",
                        "7.0.0-rc.1.22426.10"),
                    Path.Combine("ref", "net7.0"))
                .AddAssemblies(
                    ImmutableArray.Create("Vogen", "Vogen.SharedTypes", _loc.Replace(".dll", string.Empty))));
    }
}