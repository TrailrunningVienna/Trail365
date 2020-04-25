using System.Collections.Generic;
using ProtoBuf;

namespace Trail365.TileParser.Contract
{

    [ProtoBuf.ProtoContract(Name = @"tile")]
    public sealed class Tile : IExtensible
    {
        private readonly List<TileLayer> _layers = new List<TileLayer>();

        [ProtoMember(3, Name = @"layers", DataFormat = ProtoBuf.DataFormat.Default)]
        public System.Collections.Generic.List<TileLayer> Layers
        {
            get { return _layers; }
        }

        IExtension _extensionObject;
        ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
        { return ProtoBuf.Extensible.GetExtensionObject(ref _extensionObject, createIfMissing); }
    }
}
