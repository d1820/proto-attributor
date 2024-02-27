using System.Runtime.Serialization;

namespace ProtoAttributor.Tests.Mocks
{
    // Models returned by MeController actions.
    [DataContract]
    public class TestDataIgnore
    {
        [DataMember(Order=1)]
        public string Hometown { get; set; }

        [DataMember(Order = 2)]
        public string Hometown1 { get; set; }

        [DataMember(Order = 14)]
        public string Hometown44 { get; set; }

        [IgnoreDataMember]
        public string Hometown55 { get; set; }

        [DataMember(Order=16)]
        public string Hometown66 { get; set; }

        [IgnoreDataMember]
        public string Hometown99 { get; set; }
    }
}
