using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ProtoAttributor.Tests.Mocks
{
    [Serializable]
    [DataContract]
    [KnownType(typeof(string))]
    [KnownType(typeof(string))]
    public class TestRemoveDataAttributes
    {
        [Required]
        [DataMember(Order = 10, Name = "test")]
        public int MyProperty { get; set; }

        [DataMember(Order = 20)]
        public int MyProperty2 { get; set; }

        [Required]
        public int MyProperty3 { get; set; }

        [IgnoreDataMember]
        public int MyProperty4 { get; set; }

        public int MyProperty5 { get; set; }

        [DataMember(Name = "test12")]
        public int MyProperty6 { get; set; }
    }

    [DataContract]
    public enum TestRemoveEnumWithDataAttributes
    {
        [EnumMember]
        One,
        [EnumMember]
        Two,
        [EnumMember]
        Three,
        [EnumMember]
        Four,
        [EnumMember]
        Five
    }
}
