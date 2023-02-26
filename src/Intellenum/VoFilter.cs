﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Intellenum;

internal static class VoFilter
{
    /// <summary>
    /// This is stage 1 in the pipeline - the 'quick filter'.  We find out is it a type declaration and does it have any attributes? - don't allocate anything
    /// here as this is called a **lot** (every time the editor is changed, i.e. key-presses).
    /// </summary>
    /// <param name="syntaxNode"></param>
    /// <returns></returns>
    public static bool IsTarget(SyntaxNode syntaxNode) =>
        syntaxNode is TypeDeclarationSyntax {AttributeLists.Count: > 0};

    // We return all value object attributes here. There can only be one, but we report
    // it later with the location from the syntax.
    public static IEnumerable<AttributeData> TryGetIntellenumAttributes(INamedTypeSymbol voSymbolInformation)
    {
        var attrs = voSymbolInformation.GetAttributes();

        return attrs.Where(
                a => a.AttributeClass?.FullName() == "Intellenum.IntellenumAttribute"
                     || a.AttributeClass?.BaseType?.FullName() == "Intellenum.IntellenumAttribute"
                     || a.AttributeClass?.BaseType?.BaseType?.FullName() == "Intellenum.IntellenumAttribute");
    }

    // This is stage 2 in the pipeline - we filter down to just 1 target
    public static VoTarget? TryGetTarget(GeneratorSyntaxContext context)
    {
        var voSyntaxInformation = (TypeDeclarationSyntax) context.Node;

        var semanticModel = context.SemanticModel;

        var voSymbolInformation = (INamedTypeSymbol) semanticModel.GetDeclaredSymbol(context.Node)!;

        var attributeData = TryGetIntellenumAttributes(voSymbolInformation).ToImmutableArray();

        if(attributeData.Length > 0)
        {
            return new VoTarget(
                semanticModel,
                voSyntaxInformation,
                semanticModel.GetDeclaredSymbol(context.Node)!.ContainingType,
                voSymbolInformation,
                attributeData);
        }

        return null;
    }

    public static bool IsTarget(INamedTypeSymbol? voClass)
    {
        if (voClass == null)
        {
            return false;
        }

        ImmutableArray<AttributeData> attributes = voClass.GetAttributes();

        if (attributes.Length == 0)
        {
            return false;
        }

        AttributeData? voAttribute =
            attributes.SingleOrDefault(a => a.AttributeClass?.FullName() is "Intellenum.IntellenumAttribute");

        return voAttribute is not null;
    }
}
