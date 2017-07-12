using ModernRoute.WildData.Attributes;
using ModernRoute.WildData.Models;
using System;

namespace ModernRoute.WildData.Test.Models
{
    [Storage("all_types_table", "all_types_schema")]
    class AllTypesModel : IReadOnlyModel
    {
        private bool _IsPersistent;

        [Column("int")]
        public int Int
        {
            get;
            set;
        }

        [Column("int_null")]
        public int? IntNull
        {
            get;
            set;
        }

        [Column("byte")]
        public byte Byte
        {
            get;
            set;
        }

        [Column("byte_null")]
        public byte? ByteNull
        {
            get;
            set;
        }

        [Column("long")]
        public long Long
        {
            get;
            set;
        }

        [Column("long_null")]
        public long? LongNull
        {
            get;
            set;
        }

        [Column("short")]
        public short Short
        {
            get;
            set;
        }

        [Column("short_null")]
        public short? ShortNull
        {
            get;
            set;
        }

        [Column("boolean")]
        public bool Bool
        {
            get;
            set;
        }

        [Column("boolean_null")]
        public bool? BoolNull
        {
            get;
            set;
        }

        [Column("guid")]
        public Guid Guid
        {
            get;
            set;
        }

        [Column("guid_null")]
        public Guid? GuidNull
        {
            get;
            set;
        }

        [Column("string")]
        [NotNull]
        public string String
        {
            get;
            set;
        }

        [Column("string_null")]
        public string StringNull
        {
            get;
            set;
        }
        
        [Column("datetime")]
        public DateTime DateTime
        {
            get;
            set;
        }

        [Column("datetime_null")]
        public DateTime? DateTimeNull
        {
            get;
            set;
        }

        [Column("datetimeoffset")]
        public DateTimeOffset DateTimeOffset
        {
            get;
            set;
        }

        [Column("datetimeoffset_null")]
        public DateTimeOffset? DateTimeOffsetNull
        {
            get;
            set;
        }

        [Column("float")]
        public float Float
        {
            get;
            set;
        }

        [Column("float_null")]
        public float? FloatNull
        {
            get;
            set;
        }

        [Column("double")]
        public double Double
        {
            get;
            set;
        }

        [Column("double_null")]
        public double? DoubleNull
        {
            get;
            set;
        }


        [Column("binary")]
        [NotNull]
        public byte[] Binary
        {
            get;
            set;
        }

        [Column("binary_null")]
        public byte[] BinaryNull
        {
            get;
            set;
        }

        public bool IsPersistent()
        {
            return _IsPersistent;
        }

        public void SetPersistent(bool value)
        {
            _IsPersistent = value;
        }
    }
}
