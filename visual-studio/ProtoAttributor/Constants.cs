namespace ProtoAttributor
{
    public static class Constants
    {
        public static class Proto
        {
            public const string PROPERTY_ATTRIBUTE_NAME = "ProtoMember";
            public const string PROPERTY_IGNORE_ATTRIBUTE_NAME = "ProtoIgnore";
            public const string CLASS_ATTRIBUTE_NAME = "ProtoContract";
            public const string ENUM_ATTRIBUTE_NAME = "ProtoContract";
            public const string ENUM_MEMBER_NAME = "ProtoEnum";
            public const string USING_STATEMENT = "ProtoBuf";
            public const string BASE_PROP_NAME = "Proto";
        }

        public static class Data
        {
            public const string PROPERTY_ATTRIBUTE_NAME = "DataMember";
            public const string PROPERTY_IGNORE_ATTRIBUTE_NAME = "IgnoreDataMember";
            public const string CLASS_ATTRIBUTE_NAME = "DataContract";
            public const string ENUM_ATTRIBUTE_NAME = "DataContract";
            public const string ENUM_MEMBER_NAME = "EnumMember";
            public const string USING_STATEMENT = "System.Runtime.Serialization";
            public const string BASE_PROP_NAME = "Data";
            public const string BASE_PROPIGNORE_NAME = "IgnoreData";
            public const string BASE_KNOWN_TYPE_NAME = "KnownType";
        }
    }
}
