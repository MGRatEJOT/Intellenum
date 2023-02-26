﻿using System;
using Microsoft.CodeAnalysis;

namespace Intellenum;

public readonly struct VogenConfiguration
{
    public VogenConfiguration(INamedTypeSymbol? underlyingType,
        INamedTypeSymbol? validationExceptionType,
        Conversions conversions,
        Customizations customizations,
        DeserializationStrictness deserializationStrictness,
        DebuggerAttributeGeneration debuggerAttributes)
    {
        UnderlyingType = underlyingType;
        ValidationExceptionType = validationExceptionType;
        Conversions = conversions;
        Customizations = customizations;
        DeserializationStrictness = deserializationStrictness;
        DebuggerAttributes = debuggerAttributes;
    }

    public static VogenConfiguration Combine(
        VogenConfiguration localValues,
        VogenConfiguration? globalValues,
        Func<INamedTypeSymbol>? funcForDefaultUnderlyingType = null)
    {
        var conversions = (localValues.Conversions, globalValues?.Conversions) switch
        {
            (Conversions.Default, null) => DefaultInstance.Conversions,
            (Conversions.Default, Conversions.Default) => DefaultInstance.Conversions,
            (Conversions.Default, var globalDefault) => globalDefault.Value,
            (var specificValue, _) => specificValue
        };

        var customizations = (localValues.Customizations, globalValues?.Customizations) switch
        {
            (Customizations.None, null) => DefaultInstance.Customizations,
            (Customizations.None, Customizations.None) => DefaultInstance.Customizations,
            (Customizations.None, var globalDefault) => globalDefault.Value,
            (var specificValue, _) => specificValue
        };

        var strictness = (localValues.DeserializationStrictness, globalValues?.DeserializationStrictness) switch
        {
            (DeserializationStrictness.Default, null) => DefaultInstance.DeserializationStrictness,
            (DeserializationStrictness.Default, DeserializationStrictness.Default) => DefaultInstance.DeserializationStrictness,
            (DeserializationStrictness.Default, var globalDefault) => globalDefault.Value,
            (var specificValue, _) => specificValue
        };

        var debuggerAttributes = (localValues.DebuggerAttributes, globalValues?.DebuggerAttributes) switch
        {
            (DebuggerAttributeGeneration.Default, null) => DefaultInstance.DebuggerAttributes,
            (DebuggerAttributeGeneration.Default, DebuggerAttributeGeneration.Default) => DefaultInstance.DebuggerAttributes,
            (DebuggerAttributeGeneration.Default, var globalDefault) => globalDefault.Value,
            (var specificValue, _) => specificValue
        };

        var validationExceptionType = localValues.ValidationExceptionType ?? globalValues?.ValidationExceptionType ?? DefaultInstance.ValidationExceptionType;
        var underlyingType = localValues.UnderlyingType ?? globalValues?.UnderlyingType ?? funcForDefaultUnderlyingType?.Invoke();

        return new VogenConfiguration(underlyingType, validationExceptionType, conversions, customizations, strictness, debuggerAttributes);
    }

    public INamedTypeSymbol? UnderlyingType { get; }
    
    public INamedTypeSymbol? ValidationExceptionType { get; }

    public Conversions Conversions { get; }
    
    public Customizations Customizations { get; }
    public DeserializationStrictness DeserializationStrictness { get; }
    
    public DebuggerAttributeGeneration DebuggerAttributes { get; }

    // the issue here is that without a physical 'symbol' in the source, we can't
    // get the namedtypesymbol
    // ReSharper disable once MemberCanBePrivate.Global
    public static readonly VogenConfiguration DefaultInstance = new(
        underlyingType: null,
        validationExceptionType: null,
        // ReSharper disable once RedundantNameQualifier
        conversions: Conversions.Default,
        customizations: Customizations.None,
        deserializationStrictness: DeserializationStrictness.Default,
        debuggerAttributes: DebuggerAttributeGeneration.Full);
}
