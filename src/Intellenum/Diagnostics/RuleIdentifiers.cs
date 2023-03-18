﻿namespace Intellenum.Diagnostics;

internal static class RuleIdentifiers
{
    public const string AddValidationMethod = "AddValidationMethod";

    public const string TypeCannotBeNested = "VOG001";
    public const string UnderlyingTypeMustNotBeSameAsValueObject = "VOG002";
    public const string UnderlyingTypeCannotBeCollection = "VOG003";
    public const string InstanceMethodCannotHaveNullArgumentName = "VOG006";
    public const string InstanceMethodCannotHaveNullArgumentValue = "VOG007";
    public const string CannotHaveUserConstructors = "VOG008";
    public const string DoNotUseDefault = "VOG009";
    public const string DoNotUseNew = "VOG010";
    public const string InvalidConversions = "VOG011";
    public const string CustomExceptionMustDeriveFromException = "VOG012";
    public const string CustomExceptionMustHaveValidConstructor = "VOG013";
    public const string TypeCannotBeAbstract = "VOG017";
    public const string InvalidCustomizations = "VOG019";
    public const string RecordToStringOverloadShouldBeSealed = "VOG020";
    public const string TypeShouldBePartial = "VOG021";
    public const string InstanceValueCannotBeConverted = "VOG023";
    public const string DuplicateTypesFound = "VOG024";
    public const string DoNotUseReflection = "VOG025";
    public const string MustHaveInstances = "VOG026";
}