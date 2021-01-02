// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild

using System.Collections.Generic;

namespace Kaitai
{

    /// <summary>
    /// Parser for Depthmaps embedded in photos taken with last models of Xiaomi phones.
    /// Still WIP.
    /// See https://github.com/VincenzoLaSpesa/XiaomiDepthMapExtractor for more info.
    /// </summary>
    public partial class XiaomiDepthmap : KaitaiStruct
    {
        public static XiaomiDepthmap FromFile(string fileName)
        {
            return new XiaomiDepthmap(new KaitaiStream(fileName));
        }

        public XiaomiDepthmap(KaitaiStream p__io, KaitaiStruct p__parent = null, XiaomiDepthmap p__root = null) : base(p__io)
        {
            m_parent = p__parent;
            m_root = p__root ?? this;
            _read();
        }
        private void _read()
        {
            _magic = m_io.ReadBytes(4);
            if (!((KaitaiStream.ByteArrayCompare(Magic, new byte[] { 80, 77, 80, 68 }) == 0)))
            {
                throw new ValidationNotEqualError(new byte[] { 80, 77, 80, 68 }, Magic, M_Io, "/seq/0");
            }
            _imageInfo = new Info(m_io, this, m_root);
            _padding = new Padding80(m_io, this, m_root);
            _depthmapInfo = new TypeDepthmapInfo(m_io, this, m_root);
            _confidenceMap = new List<Sector>();
            {
                var i = 0;
                Sector M_;
                do {
                    M_ = new Sector(m_io, this, m_root);
                    _confidenceMap.Add(M_);
                    i++;
                } while (!(!(M_.HasNext)));
            }
            _depthmap = m_io.ReadBytesFull();
        }
        public partial class Info : KaitaiStruct
        {
            public static Info FromFile(string fileName)
            {
                return new Info(new KaitaiStream(fileName));
            }

            public Info(KaitaiStream p__io, XiaomiDepthmap p__parent = null, XiaomiDepthmap p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _unknown1 = m_io.ReadU2le();
                _unknown2 = m_io.ReadU2le();
                _unknown3 = m_io.ReadU2le();
                _unknown4 = m_io.ReadU1();
                _unknown5 = m_io.ReadU4le();
                _unknown6 = m_io.ReadU4le();
                _padding1 = m_io.ReadBytes(5);
                _unknown7 = m_io.ReadU4le();
                _padding2 = m_io.ReadU4le();
            }
            private ushort _unknown1;
            private ushort _unknown2;
            private ushort _unknown3;
            private byte _unknown4;
            private uint _unknown5;
            private uint _unknown6;
            private byte[] _padding1;
            private uint _unknown7;
            private uint _padding2;
            private XiaomiDepthmap m_root;
            private XiaomiDepthmap m_parent;
            public ushort Unknown1 { get { return _unknown1; } }
            public ushort Unknown2 { get { return _unknown2; } }
            public ushort Unknown3 { get { return _unknown3; } }
            public byte Unknown4 { get { return _unknown4; } }
            public uint Unknown5 { get { return _unknown5; } }
            public uint Unknown6 { get { return _unknown6; } }
            public byte[] Padding1 { get { return _padding1; } }
            public uint Unknown7 { get { return _unknown7; } }
            public uint Padding2 { get { return _padding2; } }
            public XiaomiDepthmap M_Root { get { return m_root; } }
            public XiaomiDepthmap M_Parent { get { return m_parent; } }
        }
        public partial class Padding80 : KaitaiStruct
        {
            public static Padding80 FromFile(string fileName)
            {
                return new Padding80(new KaitaiStream(fileName));
            }

            public Padding80(KaitaiStream p__io, XiaomiDepthmap p__parent = null, XiaomiDepthmap p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _padding = m_io.ReadBytes(1024);
            }
            private byte[] _padding;
            private XiaomiDepthmap m_root;
            private XiaomiDepthmap m_parent;
            public byte[] Padding { get { return _padding; } }
            public XiaomiDepthmap M_Root { get { return m_root; } }
            public XiaomiDepthmap M_Parent { get { return m_parent; } }
        }
        public partial class TypeDepthmapInfo : KaitaiStruct
        {
            public static TypeDepthmapInfo FromFile(string fileName)
            {
                return new TypeDepthmapInfo(new KaitaiStream(fileName));
            }

            public TypeDepthmapInfo(KaitaiStream p__io, XiaomiDepthmap p__parent = null, XiaomiDepthmap p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _padding = m_io.ReadBytes(4);
                _depthmapWidth = m_io.ReadU4le();
                _unknown1 = m_io.ReadU4le();
                _data = m_io.ReadBytes(1012);
            }
            private byte[] _padding;
            private uint _depthmapWidth;
            private uint _unknown1;
            private byte[] _data;
            private XiaomiDepthmap m_root;
            private XiaomiDepthmap m_parent;
            public byte[] Padding { get { return _padding; } }
            public uint DepthmapWidth { get { return _depthmapWidth; } }
            public uint Unknown1 { get { return _unknown1; } }
            public byte[] Data { get { return _data; } }
            public XiaomiDepthmap M_Root { get { return m_root; } }
            public XiaomiDepthmap M_Parent { get { return m_parent; } }
        }
        public partial class Sector : KaitaiStruct
        {
            public static Sector FromFile(string fileName)
            {
                return new Sector(new KaitaiStream(fileName));
            }

            public Sector(KaitaiStream p__io, XiaomiDepthmap p__parent = null, XiaomiDepthmap p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_lookaheadData0 = false;
                f_hasNext = false;
                _read();
            }
            private void _read()
            {
                _padding = m_io.ReadBytes(128);
                _data = m_io.ReadBytes(1024);
            }
            private bool f_lookaheadData0;
            private byte[] _lookaheadData0;
            public byte[] LookaheadData0
            {
                get
                {
                    if (f_lookaheadData0)
                        return _lookaheadData0;
                    long _pos = m_io.Pos;
                    m_io.Seek(M_Io.Pos);
                    _lookaheadData0 = m_io.ReadBytes(128);
                    m_io.Seek(_pos);
                    f_lookaheadData0 = true;
                    return _lookaheadData0;
                }
            }
            private bool f_hasNext;
            private bool _hasNext;
            public bool HasNext
            {
                get
                {
                    if (f_hasNext)
                        return _hasNext;
                    _hasNext = (bool) ((KaitaiStream.ByteArrayCompare(LookaheadData0, new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127 }) == 0));
                    f_hasNext = true;
                    return _hasNext;
                }
            }
            private byte[] _padding;
            private byte[] _data;
            private XiaomiDepthmap m_root;
            private XiaomiDepthmap m_parent;
            public byte[] Padding { get { return _padding; } }
            public byte[] Data { get { return _data; } }
            public XiaomiDepthmap M_Root { get { return m_root; } }
            public XiaomiDepthmap M_Parent { get { return m_parent; } }
        }
        private byte[] _magic;
        private Info _imageInfo;
        private Padding80 _padding;
        private TypeDepthmapInfo _depthmapInfo;
        private List<Sector> _confidenceMap;
        private byte[] _depthmap;
        private XiaomiDepthmap m_root;
        private KaitaiStruct m_parent;
        public byte[] Magic { get { return _magic; } }
        public Info ImageInfo { get { return _imageInfo; } }
        public Padding80 Padding { get { return _padding; } }
        public TypeDepthmapInfo DepthmapInfo { get { return _depthmapInfo; } }
        public List<Sector> ConfidenceMap { get { return _confidenceMap; } }
        public byte[] Depthmap { get { return _depthmap; } }
        public XiaomiDepthmap M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
    }
}
