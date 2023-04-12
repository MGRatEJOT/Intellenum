﻿#if NET6_0_OR_GREATER
#nullable disable

using System.Text.Json;
using Intellenum.Tests.Types;

namespace ConsumerTests.EnumAsDictionaryKeyTests;

[Intellenum(typeof(DateTime))]
[Member("Manager", 1)]
[Member("Operator", 2)]
public partial class EmployeeTypeDateTime
{
}

public class DateTimeTests
{
    [Fact]
    public void int_can_serialize_intellenum_as_key_of_dictionary()
    {
        Dictionary<EmployeeTypeDateTime, List<Employee>> d = new()
        {
            { EmployeeTypeDateTime.Manager, new List<Employee> { new Employee("John Smith", 30) } },
            { EmployeeTypeDateTime.Operator, new List<Employee> { new Employee("Dave Angel", 42) } }
        };

        var json = JsonSerializer.Serialize(d);

        var d2 = JsonSerializer.Deserialize<Dictionary<EmployeeTypeDateTime, List<Employee>>>(json);

        d2.Should().ContainKey(EmployeeTypeDateTime.Manager);
        d2.Should().ContainKey(EmployeeTypeDateTime.Operator);

        d2[EmployeeTypeDateTime.Manager].Should().Contain(new Employee("John Smith", 30));
        d2[EmployeeTypeDateTime.Operator].Should().Contain(new Employee("Dave Angel", 42));
    }
}

#endif