﻿// ReSharper disable MemberInitializerValueIgnored
// ReSharper disable UnusedType.Global

// ReSharper disable UnusedParameter.Local

using System;

namespace Intellenum
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class IntellenumDefaultsAttribute : Attribute
    {
        /// <summary>
        /// Creates a new instance of a type that represents the default
        /// values used for value object generation.
        /// </summary>
        /// <param name="underlyingType">The primitive underlying type.</param>
        /// <param name="conversions">Any conversions that need to be done for this type, e.g. to be serialized etc.</param>
        /// <param name="throws">The type of exception that is thrown when validation fails.</param>
        /// <param name="customizations">Any customizations, for instance, treating numbers in [de]serialization as strings.</param>
        /// <param name="debuggerAttributes">Controls how debugger attributes are generated. This is useful in Rider where the attributes crash Rider's debugger.</param>
        public IntellenumDefaultsAttribute(
            Type? underlyingType = null,
            Conversions conversions = Conversions.Default,
            Type? throws = null,
            Customizations customizations = Customizations.None,
            DebuggerAttributeGeneration debuggerAttributes = DebuggerAttributeGeneration.Default)
        {
            // UnderlyingType = underlyingType ?? typeof(int);
            // TypeOfValidationException = throws ?? typeof(ValueObjectValidationException);
            // Conversions = conversions;
            // Customizations = customizations;
            // DeserializationStrictness = deserializationStrictness;
            // OmitDebugAttributes = omitDebugAttributes;
        }

        // public Type UnderlyingType { get;  } = typeof(int);
        //
        // public Type TypeOfValidationException { get; } = typeof(ValueObjectValidationException);
        //
        // public Conversions Conversions { get; } = Conversions.Default;
        //
        // public Customizations Customizations { get; }
        //
        // public DeserializationStrictness DeserializationStrictness { get; } =
        //     DeserializationStrictness.AllowValidAndKnownInstances;
        //
        // public bool? OmitDebugAttributes { get; }
    }
}

