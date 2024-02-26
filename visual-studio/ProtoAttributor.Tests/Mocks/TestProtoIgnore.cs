using ProtoBuf;

namespace ProtoAttributor.Tests.Mocks
{
    // Models returned by MeController actions.
    [ProtoContract]
    public class TestProtoIgnore
    {
        [ProtoMember(1)]
        public string Hometown { get; set; }

        [ProtoMember(2)]
        public string Hometown1 { get; set; }

        [ProtoMember(14)]
        public string Hometown44 { get; set; }

        [ProtoIgnore]
        public string Hometown55 { get; set; }

        [ProtoMember(16)]
        public string Hometown66 { get; set; }

        [ProtoIgnore]
        public string Hometown99 { get; set; }
    }
}
