﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Intellenum
{
    [Generator]
    public class IntellenumGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValueProvider<(ImmutableArray<VoTarget> Left, ImmutableArray<AttributeSyntax> Right)> targets = GetTargets(context);

            IncrementalValueProvider<(Compilation Left, (ImmutableArray<VoTarget> Left, ImmutableArray<AttributeSyntax> Right) Right)> compilationAndValues
                = context.CompilationProvider.Combine(targets);

            context.RegisterSourceOutput(compilationAndValues,
                static (spc, source) => Execute(
                    source.Left, 
                    source.Right.Left, 
                    source.Right.Right,
                    spc));
        }

        private static IncrementalValueProvider<(ImmutableArray<VoTarget> Left, ImmutableArray<AttributeSyntax> Right)> GetTargets(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValuesProvider<VoTarget> voFilter = context.SyntaxProvider.CreateSyntaxProvider(
                    predicate: static (s, _) => IntellenumFilter.IsTarget(s),
                    transform: static (ctx, _) => IntellenumFilter.TryGetTarget(ctx))
                .Where(static m => m is not null)!;

            IncrementalValuesProvider<AttributeSyntax> globalConfigFilter = context.SyntaxProvider.CreateSyntaxProvider(
                    predicate: static (s, _) => IsTarget(s),
                    transform: static (ctx, _) => ManageAttributes.TryGetAssemblyLevelDefaultsAttribute(ctx))
                .Where(static m => m is not null)!;

            IncrementalValueProvider<(ImmutableArray<VoTarget> Left, ImmutableArray<AttributeSyntax> Right)> targetsAndDefaultAttributes
                = voFilter.Collect().Combine(globalConfigFilter.Collect());

            return targetsAndDefaultAttributes;
        }

        static void Execute(
            Compilation compilation, 
            ImmutableArray<VoTarget> typeDeclarations,
            ImmutableArray<AttributeSyntax> globalConfigAttributes,
            SourceProductionContext context)
        {
            if (typeDeclarations.IsDefaultOrEmpty)
            {
                return;
            }
            
            // if there are some, get the
            var buildResult =
                ManageAttributes.GetDefaultConfigFromGlobalAttribute(globalConfigAttributes, compilation);
            
            foreach (var diagnostic in buildResult.Diagnostics)
            {
                context.ReportDiagnostic(diagnostic);
            }

            IntellenumConfiguration? globalConfig = buildResult.ResultingConfiguration;

            // get all of the ValueObject types found.
            List<VoWorkItem> workItems = GetWorkItems(typeDeclarations, context, globalConfig, compilation).ToList();

            if (workItems.Count > 0)
            {
                foreach (var eachWorkItem in workItems)
                {
                    WriteWorkItems.WriteVo(eachWorkItem, context);
                }
            }
        }

        static IEnumerable<VoWorkItem> GetWorkItems(ImmutableArray<VoTarget> targets,
            SourceProductionContext context,
            IntellenumConfiguration? globalConfig,
            Compilation compilation)
        {
            if (targets.IsDefaultOrEmpty)
            {
                yield break;
            }

            foreach (VoTarget? eachTarget in targets)
            {
                if (eachTarget is null)
                {
                    continue;
                }
                
                var ret = BuildWorkItems.TryBuild(eachTarget, context, globalConfig, compilation);
                
                if (ret is not null)
                {
                    yield return ret;
                }
            }
        }

        private static bool IsTarget(SyntaxNode node) =>
            node is AttributeListSyntax attributeList
            && attributeList.Target is not null
            && attributeList.Target.Identifier.IsKind(SyntaxKind.AssemblyKeyword);
    }
}