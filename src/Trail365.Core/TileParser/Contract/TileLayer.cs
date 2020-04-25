namespace Trail365.TileParser.Contract
{
    [ProtoBuf.ProtoContract(Name = @"layer")]
    public sealed class TileLayer : ProtoBuf.IExtensible
    {
        uint _version;
        [ProtoBuf.ProtoMember(15, IsRequired = true, Name = @"version", DataFormat = ProtoBuf.DataFormat.Default)]
        public uint Version
        {
            get { return _version; }
            set { _version = value; }
        }
        string _name;
        [ProtoBuf.ProtoMember(1, IsRequired = true, Name = @"name", DataFormat = ProtoBuf.DataFormat.Default)]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        readonly System.Collections.Generic.List<TileFeature> _features = new System.Collections.Generic.List<TileFeature>();
        [ProtoBuf.ProtoMember(2, Name = @"features", DataFormat = ProtoBuf.DataFormat.Default)]
        public System.Collections.Generic.List<TileFeature> Features
        {
            get { return _features; }
        }

        readonly System.Collections.Generic.List<string> _keys = new System.Collections.Generic.List<string>();
        [ProtoBuf.ProtoMember(3, Name = @"keys", DataFormat = ProtoBuf.DataFormat.Default)]
        public System.Collections.Generic.List<string> Keys
        {
            get { return _keys; }
        }

        readonly System.Collections.Generic.List<TileValue> _values = new System.Collections.Generic.List<TileValue>();
        [ProtoBuf.ProtoMember(4, Name = @"values", DataFormat = ProtoBuf.DataFormat.Default)]
        public System.Collections.Generic.List<TileValue> Values
        {
            get { return _values; }
        }

        uint _extent = 4096;
        [ProtoBuf.ProtoMember(5, IsRequired = false, Name = @"extent", DataFormat = ProtoBuf.DataFormat.TwosComplement)]
        [System.ComponentModel.DefaultValue((uint)4096)]
        public uint Extent
        {
            get { return _extent; }
            set { _extent = value; }
        }
        ProtoBuf.IExtension _extensionObject;
        ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
        { return ProtoBuf.Extensible.GetExtensionObject(ref _extensionObject, createIfMissing); }
    }
}
