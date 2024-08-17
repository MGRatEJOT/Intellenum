﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

#if CODEANALYSIS_V3_OR_BETTER
using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.Diagnostics;
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Intellenum.Extensions;

public static class GeneralExtensions
{
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public static T? SingleOrDefault<T>(this IEnumerable<T?> source, string message)
    {
        try
        {
            return source.SingleOrDefault();
        }
        catch (Exception e)
        {
            try
            {
                throw new InvalidOperationException($"There were {source.Count()} item(s) found in the collection. {message}", e);
            }
            catch (Exception e2)
            {
                throw new InvalidOperationException("There were none or multiple items found in the collection. " + message, e2);

            }
        }
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public static T? SingleOrDefaultNoThrow<T>(this IEnumerable<T> source)
    {
        var first = source.ElementAtOrDefault(0);
        if (first is null)
        {
            return default;
        }

        var second = source.ElementAtOrDefault(1);
        return second is null ? first : default;
    }
}

/// <summary>
/// Provides extensions to <see cref="Compilation"/>.
/// </summary>
internal static class CompilationExtensions
{
    private static readonly byte[] mscorlibPublicKeyToken = new byte[]
        { 0xB7, 0x7A, 0x5C, 0x56, 0x19, 0x34, 0xE0, 0x89 };

#if CODEANALYSIS_V3_OR_BETTER
        private const string WebAppProjectGuidString = "{349C5851-65DF-11DA-9384-00065B846F21}";
        private const string WebSiteProjectGuidString = "{E24C65DC-7377-472B-9ABA-BC803B73C61A}";

        /// <summary>
        /// Gets a value indicating whether the project of the compilation is a Web SDK project based on project properties.
        /// </summary>
        internal static bool IsWebProject(this Compilation compilation, AnalyzerOptions options)
        {
            var propertyValue = options.GetMSBuildPropertyValue(MSBuildPropertyOptionNames.UsingMicrosoftNETSdkWeb, compilation);
            if (string.Equals(propertyValue?.Trim(), "true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            propertyValue = options.GetMSBuildPropertyValue(MSBuildPropertyOptionNames.ProjectTypeGuids, compilation);
            if (!RoslynString.IsNullOrEmpty(propertyValue) &&
                (propertyValue.Contains(WebAppProjectGuidString, StringComparison.OrdinalIgnoreCase) ||
                 propertyValue.Contains(WebSiteProjectGuidString, StringComparison.OrdinalIgnoreCase)))
            {
                var guids = propertyValue.Split(';').Select(g => g.Trim()).ToImmutableArray();
                return guids.Contains(WebAppProjectGuidString, StringComparer.OrdinalIgnoreCase) ||
                    guids.Contains(WebSiteProjectGuidString, StringComparer.OrdinalIgnoreCase);
            }

            return false;
        }
#endif

    // /// <summary>
    // /// Gets a type by its full type name and cache it at the compilation level.
    // /// </summary>
    // /// <param name="compilation">The compilation.</param>
    // /// <param name="fullTypeName">Namespace + type name, e.g. "System.Exception".</param>
    // /// <returns>The <see cref="INamedTypeSymbol"/> if found, null otherwise.</returns>
    // internal static INamedTypeSymbol? GetOrCreateTypeByMetadataName(this Compilation compilation, string fullTypeName) =>
    //     WellKnownTypeProvider.GetOrCreate(compilation).GetOrCreateTypeByMetadataName(fullTypeName);
    //
    // /// <summary>
    // /// Gets a type by its full type name and cache it at the compilation level.
    // /// </summary>
    // /// <param name="compilation">The compilation.</param>
    // /// <param name="fullTypeName">Namespace + type name, e.g. "System.Exception".</param>
    // /// <returns>The <see cref="INamedTypeSymbol"/> if found, null otherwise.</returns>
    // internal static bool TryGetOrCreateTypeByMetadataName(this Compilation compilation, string fullTypeName, out INamedTypeSymbol? namedTypeSymbol) =>
    //     WellKnownTypeProvider.GetOrCreate(compilation).TryGetOrCreateTypeByMetadataName(fullTypeName, out namedTypeSymbol);

    /// <summary>
    /// Gets a value indicating, whether the compilation of assembly targets .NET Framework.
    /// This method differentiates between .NET Framework and other frameworks (.NET Core, .NET Standard, .NET 5 in future).
    /// </summary>
    /// <param name="compilation">The compilation</param>
    /// <returns><c>True</c> if the compilation targets .NET Framework; otherwise <c>false</c>.</returns>
    internal static bool TargetsDotNetFramework(this Compilation compilation)
    {
        var objectType = compilation.GetSpecialType(SpecialType.System_Object);
        var assemblyIdentity = objectType.ContainingAssembly.Identity;
        if (assemblyIdentity.Name == "mscorlib" &&
            assemblyIdentity.IsStrongName &&
            (assemblyIdentity.Version == new System.Version(4, 0, 0, 0) || assemblyIdentity.Version == new System.Version(2, 0, 0, 0)) &&
            assemblyIdentity.PublicKeyToken.Length == mscorlibPublicKeyToken.Length)
        {
            for (int i = 0; i < mscorlibPublicKeyToken.Length; i++)
            {
                if (assemblyIdentity.PublicKeyToken[i] != mscorlibPublicKeyToken[i])
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }
}