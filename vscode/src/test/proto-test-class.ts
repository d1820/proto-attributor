export const protoExisting = `using System;
using ProtoBuf;
namespace Sample;

[ProtoContract]
public class ProtoTesterFileScoped
{
    private string _test;

    [ProtoMember(2)]
    public int MyProperty { get; set; }

    [ProtoMember(1)]
    public int MyProperty1 { get; set; }

    public DateTime? NullDateTime { get; set; }

    public int? NullInt { get; set; }

    public int?[] NullIntArray { get; set; }

    public bool ExecuteWelcome() { ManWorker();  return true; }

    private string ManWorker() { return ""; }
}
`;

export const noProtoExisting = `using System;
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

export const protoEnum = `using System;
namespace Sample
{

  public enum MyEnum
  {
      [OtherAttribute]
      One,
      Two,
      ///<summary>
      /// Test
      ///</summary>
      Three
  }
}
`;

export const protoEnumExpected = `using System;
using ProtoBuf;

namespace Sample
{

  [ProtoContract]
  public enum MyEnum
  {
      [OtherAttribute]
      [ProtoEnum]
      One,
      [ProtoEnum]
      Two,
      ///<summary>
      /// Test
      ///</summary>
      [ProtoEnum]
      Three
  }
}
`;

export const protoReorderExisting = `using System;
using ProtoBuf;
namespace Sample;

[ProtoContract]
public class ProtoTesterFileScoped
{
    private string _test;

    [ProtoMember(2)]
    public int MyProperty { get; set; }

    [ProtoMember(23)]
    public int MyProperty1 { get; set; }

    [ProtoMember(34)]
    public DateTime? NullDateTime { get; set; }

    [ProtoMember(100)]
    public int? NullInt { get; set; }
}
`;

export const protoReorderExistingExpected = `using System;
using ProtoBuf;
namespace Sample;

[ProtoContract]
public class ProtoTesterFileScoped
{
    private string _test;

    [ProtoMember(1)]
    public int MyProperty { get; set; }

    [ProtoMember(2)]
    public int MyProperty1 { get; set; }

    [ProtoMember(3)]
    public DateTime? NullDateTime { get; set; }

    [ProtoMember(4)]
    public int? NullInt { get; set; }
}
`;
