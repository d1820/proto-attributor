export const dataContractExisting = `using System;
using System.Runtime.Serialization;
namespace Sample;

[DataContract]
public class ProtoTesterFileScoped
{
    private string _test;

    [DataMember(Order = 1)]
    public int MyProperty { get; set; }

    [DataMember(Order = 2)]
    public int MyProperty1 { get; set; }

    public DateTime? NullDateTime { get; set; }

    public int? NullInt { get; set; }

    public int?[] NullIntArray { get; set; }

    public bool ExecuteWelcome() { ManWorker();  return true; }

    private string ManWorker() { return ""; }
}
`;

export const noDataContractExisting = `using System;
namespace Sample;

public class ProtoTesterFileScoped
{
    private string _test;

    public int MyProperty { get; set; }

    public int MyProperty1 { get; set; }

    public DateTime? NullDateTime { get; set; }

    public int? NullInt { get; set; }

    public int?[] NullIntArray { get; set; }

    public bool ExecuteWelcome() { ManWorker();  return true; }

    private string ManWorker() { return ""; }
}
`;
