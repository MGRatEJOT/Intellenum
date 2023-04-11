using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Intellenum.Extensions;
using Microsoft.CodeAnalysis;

namespace Intellenum;

internal static class TryParseGeneration
{
    public static string GenerateTryParseIfNeeded(VoWorkItem item)
    {
        INamedTypeSymbol primitiveSymbol = item.UnderlyingType;

        try
        {
            var found = FindMatches(primitiveSymbol).ToList();

            if (found.Count == 0) return string.Empty;

            StringBuilder sb = new StringBuilder();
                
            foreach (var eachSymbol in found)
            {
                BuildMethod(eachSymbol, sb, item);
            }

            return sb.ToString();
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Cannot parse {primitiveSymbol} - {e}", e);

        }
    }

    private static void BuildMethod(IMethodSymbol methodSymbol, StringBuilder sb, VoWorkItem item)
    {
        string parameters = BuildParameters(methodSymbol);
        string parameterNames = BuildParameterNames(methodSymbol);

        var inheritDocRef = methodSymbol.ToString()!.Replace("<", "{").Replace(">", "}");
            
        var ret =
            @$"
    /// <inheritdoc cref=""{inheritDocRef}""/>
    /// <summary>
    /// </summary>
    /// <returns>
    /// The value created via the <see cref=""From""/> method.
    /// </returns>
    public static global::System.Boolean TryParse({parameters}, {GenerateNotNullWhenAttribute()} out {item.VoTypeName} result) {{
        if({item.UnderlyingTypeFullName}.TryParse({parameterNames}, out var r)) {{
            return TryFromValue(r, out result);
        }}

        result = default;
        return false;
    }}";

        sb.AppendLine(ret);
    }

    private static string GenerateNotNullWhenAttribute()
    {
        return @"
#if NETCOREAPP3_0_OR_GREATER
[global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
#endif
";

    }

    private static string BuildParameters(IMethodSymbol methodSymbol)
    {
        List<string> l = new();

        for (var index = 0; index < methodSymbol.Parameters.Length-1; index++)
        {
            IParameterSymbol eachParameter = methodSymbol.Parameters[index];
                
            string refKind = BuildRefKind(eachParameter.RefKind);

            string type = eachParameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            string name = Util.EscapeIfRequired(eachParameter.Name);

            l.Add($"{refKind}{type} {name}");
        }

        return string.Join(", ", l);
    }

    private static string BuildRefKind(RefKind refKind) =>
        refKind switch
        {
            RefKind.In => "in ",
            RefKind.Out => "out ",
            RefKind.Ref => "ref ",
            _ => ""
        };

    private static string BuildParameterNames(IMethodSymbol methodSymbol)
    {
        List<string> l = new();
        for (var index = 0; index < methodSymbol.Parameters.Length-1; index++)
        {
            var eachParameter = methodSymbol.Parameters[index];
            l.Add($"{eachParameter.Name}");
        }

        return string.Join(", ", l);
    }

    private static IEnumerable<IMethodSymbol> FindMatches(INamedTypeSymbol primitiveSymbol)
    {
        ImmutableArray<ISymbol> members = primitiveSymbol.GetMembers("TryParse");

        if (members.Length == 0) yield break;
            
        foreach (ISymbol eachMember in members)
        {
            if (eachMember is not IMethodSymbol s)
            {
                continue;
            }

            if (!s.IsStatic)
            {
                continue;
            }

            var ps = s.GetParameters();

            if (s.ReturnType.Name != nameof(Boolean))
            {
                continue;
            }

            if (!SymbolEqualityComparer.Default.Equals(ps[ps.Length-1].Type, primitiveSymbol))
            {
                continue;
            }

            yield return s;
        }
    }
}