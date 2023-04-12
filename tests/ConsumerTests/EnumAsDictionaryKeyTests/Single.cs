﻿#if NET6_0_OR_GREATER
#nullable disable

using System.Text.Json;
using Intellenum.Tests.Types;

namespace ConsumerTests.EnumAsDictionaryKeyTests;

[Intellenum(typeof(Single))]
[Member("Manager", 1)]
[Member("Operator", 2)]
public partial class EmployeeTypeSingle
{
}

public class SingleTests
{
    [Fact]
    public void int_can_serialize_intellenum_as_key_of_dictionary()
    {
        Dictionary<EmployeeTypeSingle, List<Employee>> d = new()
        {
            { EmployeeTypeSingle.Manager, new List<Employee> { new Employee("John Smith", 30) } },
            { EmployeeTypeSingle.Operator, new List<Employee> { new Employee("Dave Angel", 42) } }
        };

        var json = JsonSerializer.Serialize(d);

        var d2 = JsonSerializer.Deserialize<Dictionary<EmployeeTypeSingle, List<Employee>>>(json);

        d2.Should().ContainKey(EmployeeTypeSingle.Manager);
        d2.Should().ContainKey(EmployeeTypeSingle.Operator);

        d2[EmployeeTypeSingle.Manager].Should().Contain(new Employee("John Smith", 30));
        d2[EmployeeTypeSingle.Operator].Should().Contain(new Employee("Dave Angel", 42));
    }
}

#endif