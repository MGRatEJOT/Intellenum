﻿using System;
using System.Runtime.CompilerServices;
using System.Text;
using Intellenum.Generators.Conversions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[assembly: InternalsVisibleTo("Intellenum.Tests")]

namespace Intellenum;


public static class Util
{
    static readonly IGenerateConversion[] _conversionGenerators =
    {
        new GenerateSystemTextJsonConversions(),
        new GenerateNewtonsoftJsonConversions(),
        new GenerateTypeConverterConversions(),
        new GenerateDapperConversions(),
        new GenerateEfCoreTypeConversions(),
        new GenerateLinqToDbConversions(),
    };


    public static string GenerateCallToValidateForDeserializing(VoWorkItem workItem)
    {
        StringBuilder sb = new StringBuilder();

        foreach (var eachInstance in workItem.InstanceProperties)
        {
            string escapedName = EscapeIfRequired(eachInstance.Name);
            sb.AppendLine($"        if(value == {escapedName}.Value) return {escapedName};");
        }

        return sb.ToString();
    }

    public static string EscapeIfRequired(string name)
    {
        bool match = SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None ||
                     SyntaxFacts.GetContextualKeywordKind(name) != SyntaxKind.None;

        return match ? "@" + name : name;
    }

    public static string GenerateModifiersFor(TypeDeclarationSyntax tds) => string.Join(" ", tds.Modifiers);

    public static string WriteStartNamespace(string @namespace)
    {
        if (string.IsNullOrEmpty(@namespace))
        {
            return string.Empty;
        }

        return @$"namespace {EscapeIfRequired(@namespace)}
{{
";
    }

    public static string WriteCloseNamespace(string @namespace)
    {
        if (string.IsNullOrEmpty(@namespace))
        {
            return string.Empty;
        }

        return @$"}}";
    }

    /// <summary>
    /// These are the attributes that are written to the top of the type, things like
    /// `TypeConverter`, `System.Text.JsonConverter` etc.
    /// </summary>
    /// <param name="tds"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public static string GenerateAnyConversionAttributes(TypeDeclarationSyntax tds, VoWorkItem item)
    {
        StringBuilder sb = new StringBuilder();

        foreach (var conversionGenerator in _conversionGenerators)
        {
            var attribute = conversionGenerator.GenerateAnyAttributes(tds, item);
            if (!string.IsNullOrEmpty(attribute))
            {
                sb.AppendLine(attribute);
            }
        }

        return sb.ToString();
    }

    public static string GenerateAnyConversionAttributesForDebuggerProxy(TypeDeclarationSyntax tds, VoWorkItem item) => item.Conversions.ToString();

    public static string GenerateAnyConversionBodies(TypeDeclarationSyntax tds, VoWorkItem item)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var conversionGenerator in _conversionGenerators)
        {
            sb.AppendLine(conversionGenerator.GenerateAnyBody(tds, item));
        }

        return sb.ToString();
    }

    public static string GenerateDebuggerProxyForStructs(TypeDeclarationSyntax tds, VoWorkItem item)
    {
        string code = $@"internal sealed class {item.VoTypeName}DebugView
        {{
            private readonly {item.VoTypeName} _t;

            {item.VoTypeName}DebugView({item.VoTypeName} t)
            {{
                _t = t;
            }}

            public global::System.Boolean IsInitialized => _t._isInitialized;
            public global::System.String UnderlyingType => ""{item.UnderlyingTypeFullName}"";
            public global::System.String Value => _t._isInitialized ? _t._value.ToString() : ""[not initialized]"" ;

            #if DEBUG
            public global::System.String CreatedWith => _t._stackTrace?.ToString() ?? ""the From method"";
            #endif

            public global::System.String Conversions => @""{Util.GenerateAnyConversionAttributesForDebuggerProxy(tds, item)}"";
                }}";

        return code;
    }

    public static string GenerateDebuggerProxyForClasses(TypeDeclarationSyntax tds, VoWorkItem item)
    {
        string code = $@"internal sealed class {item.VoTypeName}DebugView
        {{
            private readonly {item.VoTypeName} _t;

            {item.VoTypeName}DebugView({item.VoTypeName} t)
            {{
                _t = t;
            }}

            public global::System.String UnderlyingType => ""{item.UnderlyingTypeFullName}"";
            public {item.UnderlyingTypeFullName} Value => _t.Value ;

            public global::System.String Conversions => @""{Util.GenerateAnyConversionAttributes(tds, item)}"";
                }}";

        return code;
    }

    public static string GenerateYourAssemblyName() => typeof(Util).Assembly.GetName().Name!;
    public static string GenerateYourAssemblyVersion() => typeof(Util).Assembly.GetName().Version!.ToString();

    public static string GenerateToString(VoWorkItem item) =>
        item.HasToString ? string.Empty
            : $@"/// <summary>Returns the name of the enum.</summary>
    public override global::System.String ToString() => Name;";

    public static string GenerateIComparableImplementationIfNeeded(VoWorkItem item, TypeDeclarationSyntax tds)
    {
        INamedTypeSymbol? primitiveSymbol = item.UnderlyingType;
        if (!primitiveSymbol.ImplementsInterfaceOrBaseClass(typeof(IComparable<>)))
        {
            return string.Empty;
        }
    
        var primitive = tds.Identifier;
        var s = @$"public int CompareTo({primitive} other) => Value.CompareTo(other.Value);
        public int CompareTo(object other) {{
            if(other == null) return 1;
            if(other is {primitive} x) return CompareTo(x);
            throw new global::System.ArgumentException(""Cannot compare to object as it is not of type {primitive}"", nameof(other));
        }}";
    
         return s;
    }

    public static string GenerateDebugAttributes(VoWorkItem item, SyntaxToken className, string itemUnderlyingType)
    {
        var source = $$"""
[global::System.Diagnostics.DebuggerTypeProxyAttribute(typeof({{className}}DebugView))]
    [global::System.Diagnostics.DebuggerDisplayAttribute("Underlying type: {{itemUnderlyingType}}, Value = { _value }")]
""";
        if (item.DebuggerAttributes == DebuggerAttributeGeneration.Basic)
        {
            return $@"/* Debug attributes omitted because the 'debuggerAttributes' flag is set to {nameof(DebuggerAttributeGeneration.Basic)} on the Intellenum attribute.
This is usually set to avoid issues in Rider where it doesn't fully handle the attributes support by Visual Studio and
causes Rider's debugger to crash.

{source}

*/";
        }
    
        return source;
    }

    public static string GenerateIsDefinedImplementation(VoWorkItem item) =>
        """
    return TryFromValue(value, out _);
""";

    public static string GenerateFromValueImplementation(VoWorkItem item) =>
        """
    bool b = TryFromValue(value, out var ret);
    if(b) return ret;
    throw new global::System.InvalidOperationException($"No matching enums with a value of '{value}'");
""";

    public static string GenerateFromNameImplementation(VoWorkItem item) =>
        """
    bool b = TryFromName(name, out var ret);
    if(b) return ret;
    throw new global::System.InvalidOperationException($"No matching enums named '{name}'");
""";

    public static string GenerateTryFromValueImplementation(VoWorkItem item)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("""
switch (value) 
{
""");
        foreach (var eachInstance in item.InstanceProperties)
        {
            generate(eachInstance.Value, eachInstance.Name);
        }

        sb.AppendLine("""
    default:
        instance = default;
        return false;
}
""");

        return sb.ToString();

        void generate(object value, string name)
        {
            sb.AppendLine(
                $$"""
    case {{value}}:
        instance = {{item.VoTypeName}}.{{name}}; 
        return true;
""");

        }
    }

    public static string GenerateTryFromNameImplementation(VoWorkItem item)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("""
switch (name) 
{
""");

        foreach (var eachInstance in item.InstanceProperties)
        {
            generate(eachInstance.Name);
        }

        sb.AppendLine("""
    default:
        instance = default;
        return false;
}
""");

        return sb.ToString();

        void generate(string name)
            {
                sb.AppendLine(
                    $$"""
    case nameof({{item.VoTypeName}}.{{name}}):
        instance = {{item.VoTypeName}}.{{name}}; 
        return true;
""");
            }
    }
    
    public static string GenerateIsNameDefinedImplementation(VoWorkItem item) =>
        """
return TryFromName(name, out _);
""";
}