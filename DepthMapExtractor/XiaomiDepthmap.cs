// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild

using System.Collections.Generic;

namespace Kaitai
{
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
            _fixedHeader = new MagicConstant(m_io, this, m_root);
            _imageInfo1 = new Info(m_io, this, m_root);
            _padding = new Padding80(m_io, this, m_root);
            _imageInfo2 = new Info2(m_io, this, m_root);
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
        public partial class MagicConstant : KaitaiStruct
        {
            public static MagicConstant FromFile(string fileName)
            {
                return new MagicConstant(new KaitaiStream(fileName));
            }

            public MagicConstant(KaitaiStream p__io, XiaomiDepthmap p__parent = null, XiaomiDepthmap p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _magic = m_io.ReadBytes(4);
                if (!((KaitaiStream.ByteArrayCompare(Magic, new byte[] { 80, 77, 80, 68 }) == 0)))
                {
                    throw new ValidationNotEqualError(new byte[] { 80, 77, 80, 68 }, Magic, M_Io, "/types/magic_constant/seq/0");
                }
                _magic2 = m_io.ReadBytes(7);
                if (!((KaitaiStream.ByteArrayCompare(Magic2, new byte[] { 2, 0, 100, 0, 1, 0, 0 }) == 0)))
                {
                    throw new ValidationNotEqualError(new byte[] { 2, 0, 100, 0, 1, 0, 0 }, Magic2, M_Io, "/types/magic_constant/seq/1");
                }
            }
            private byte[] _magic;
            private byte[] _magic2;
            private XiaomiDepthmap m_root;
            private XiaomiDepthmap m_parent;
            public byte[] Magic { get { return _magic; } }
            public byte[] Magic2 { get { return _magic2; } }
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
                _filler = m_io.ReadBytes(128);
                _data0 = m_io.ReadBytes(1024);
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
            private byte[] _filler;
            private byte[] _data0;
            private XiaomiDepthmap m_root;
            private XiaomiDepthmap m_parent;
            public byte[] Filler { get { return _filler; } }
            public byte[] Data0 { get { return _data0; } }
            public XiaomiDepthmap M_Root { get { return m_root; } }
            public XiaomiDepthmap M_Parent { get { return m_parent; } }
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
                _int1 = m_io.ReadU4le();
                _int2 = m_io.ReadU4le();
                _junk1 = m_io.ReadBytes(5);
                _int3 = m_io.ReadU4le();
                _junk2 = m_io.ReadU4le();
            }
            private uint _int1;
            private uint _int2;
            private byte[] _junk1;
            private uint _int3;
            private uint _junk2;
            private XiaomiDepthmap m_root;
            private XiaomiDepthmap m_parent;
            public uint Int1 { get { return _int1; } }
            public uint Int2 { get { return _int2; } }
            public byte[] Junk1 { get { return _junk1; } }
            public uint Int3 { get { return _int3; } }
            public uint Junk2 { get { return _junk2; } }
            public XiaomiDepthmap M_Root { get { return m_root; } }
            public XiaomiDepthmap M_Parent { get { return m_parent; } }
        }
        public partial class Info2 : KaitaiStruct
        {
            public static Info2 FromFile(string fileName)
            {
                return new Info2(new KaitaiStream(fileName));
            }

            public Info2(KaitaiStream p__io, XiaomiDepthmap p__parent = null, XiaomiDepthmap p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _padding0 = m_io.ReadBytes(4);
                _sizeW = m_io.ReadU4le();
                _int1 = m_io.ReadU4le();
                _data0 = m_io.ReadBytes(1012);
            }
            private byte[] _padding0;
            private uint _sizeW;
            private uint _int1;
            private byte[] _data0;
            private XiaomiDepthmap m_root;
            private XiaomiDepthmap m_parent;
            public byte[] Padding0 { get { return _padding0; } }
            public uint SizeW { get { return _sizeW; } }
            public uint Int1 { get { return _int1; } }
            public byte[] Data0 { get { return _data0; } }
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
                _junk = m_io.ReadBytes(1024);
            }
            private byte[] _junk;
            private XiaomiDepthmap m_root;
            private XiaomiDepthmap m_parent;
            public byte[] Junk { get { return _junk; } }
            public XiaomiDepthmap M_Root { get { return m_root; } }
            public XiaomiDepthmap M_Parent { get { return m_parent; } }
        }
        private MagicConstant _fixedHeader;
        private Info _imageInfo1;
        private Padding80 _padding;
        private Info2 _imageInfo2;
        private List<Sector> _confidenceMap;
        private byte[] _depthmap;
        private XiaomiDepthmap m_root;
        private KaitaiStruct m_parent;
        public MagicConstant FixedHeader { get { return _fixedHeader; } }
        public Info ImageInfo1 { get { return _imageInfo1; } }
        public Padding80 Padding { get { return _padding; } }
        public Info2 ImageInfo2 { get { return _imageInfo2; } }
        public List<Sector> ConfidenceMap { get { return _confidenceMap; } }
        public byte[] Depthmap { get { return _depthmap; } }
        public XiaomiDepthmap M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
    }
}
