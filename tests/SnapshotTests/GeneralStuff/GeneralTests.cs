﻿using System.Threading.Tasks;
using Intellenum;
using VerifyXunit;

namespace SnapshotTests.GeneralStuff
{
    [UsesVerify]
    public class GeneralTests
    {
        [Fact]
        public Task Partial_struct_created_successfully()
        {
            var source = @"using Intellenum;
namespace Whatever;

[Intellenum(typeof(int))]
public partial struct CustomerId
{
}";

            return RunTest(source);
        }

        private static Task RunTest(string source) =>
            new SnapshotRunner<IntellenumGenerator>()
                .WithSource(source)
                .RunOnAllFrameworks();

        [Fact]
        public Task No_namespace() =>
            RunTest(@"using Intellenum;

[Intellenum(typeof(int))]
public partial struct CustomerId
{
}");


        [Fact]
        public Task Produces_instances() =>
            RunTest(@"using Intellenum;

namespace Whatever;

[Intellenum(typeof(int))]
[Instance(name: ""Unspecified"", value: -1, tripleSlashComment: ""a short description that'll show up in intellisense"")]
[Instance(name: ""Unspecified1"", value: -2)]
[Instance(name: ""Unspecified2"", value: -3, tripleSlashComment: ""<some_xml>whatever</some_xml"")]
[Instance(name: ""Unspecified3"", value: -4)]
[Instance(name: ""Cust42"", value: 42)]
public partial struct CustomerId
{
}
");

        [Fact]
        public Task Validation_with_PascalCased_validate_method() =>
            RunTest(@"using Intellenum;

namespace Whatever;

[Intellenum(typeof(int))]
public partial struct CustomerId
{
    private static Validation Validate(int value)
    {
        if (value > 0)
            return Validation.Ok;

        return Validation.Invalid(""must be greater than zero"");
    }
}
");

        [Fact]
        public Task Validation_with_camelCased_validate_method() =>
            RunTest(@"using Intellenum;

namespace Whatever;

[Intellenum(typeof(int))]
public partial struct CustomerId
{
    private static Validation validate(int value)
    {
        if (value > 0)
            return Validation.Ok;

        return Validation.Invalid(""must be greater than zero"");
    }
}
");


        [Fact]
        public Task Namespace_names_can_have_reserved_keywords() =>
            RunTest(@"using Intellenum;

namespace @double;

[Intellenum]
[Instance(name: ""@struct"", value: 42)]
[Instance(name: ""@double"", value: 52)]
[Instance(name: ""@event"", value: 69)]
[Instance(name: ""@void"", value: 666)]
public partial struct @class
{
    private static Validation validate(int value)
    {
        if (value > 0)
            return Validation.Ok;

        return Validation.Invalid(""must be greater than zero"");
    }
}
");
    }
}