using ModernRoute.WildData.Attributes;
using ModernRoute.WildData.Core;
using ModernRoute.WildData.Helpers;
using ModernRoute.WildData.Linq;
using ModernRoute.WildData.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ModernRoute.WildData.Test.Core
{
    [TestFixture]
    class ReadWriteRepositoryHelperTest
    {
        [Test]
        public void Ctor()
        {
            TestRepository testRepository = new TestRepository();

            Model model = new Model
            {
                Id = 5,
                Property1 = 54,
                Property2 = "value1",
                Property3 = "value2",
                Property4 = "value3",
                Property5 = "value4",
                Property7 = "value5",
                Property9 = "value7",
                Property12 = "value8",
                Field16 = "value9",
                Field18 = "value10"
            };

            Model expectedStoredModel = model.Clone();
            expectedStoredModel.Id = TestRepository.RandomId;
            expectedStoredModel.Field18 = TestRepository.RandomStringValue;

            Model expectedUpdatedModel = model.Clone();
            expectedUpdatedModel.Field18 = TestRepository.RandomStringValue;

            Model modelToStore = model.Clone();
            testRepository.StoreModel(modelToStore);
            Assert.AreEqual(expectedStoredModel, modelToStore);
            Model actualModel = testRepository.GetModel();
            Assert.AreEqual(expectedStoredModel, actualModel);

            Model modelToUpdate = model.Clone();
            testRepository.UpdateModel(modelToUpdate);
            Assert.AreEqual(expectedUpdatedModel, modelToUpdate);
            actualModel = testRepository.GetModel();
            Assert.AreEqual(expectedUpdatedModel, actualModel);
        }
    }

    class TestRepository 
    {
        public const int RandomId = 109683325;
        public const string RandomStringValue = "QLqsxzXzPu4FaxLmwLlc";

        private ReadWriteRepositoryHelper<Model, int> _Repository;

        private Wrapper _Wrapper;        

        public TestRepository()
        {
            _Repository = new ReadWriteRepositoryHelper<Model, int>();
            _Wrapper = new Wrapper(_Repository.MemberColumnMap);
        }

        public void StoreModel(Model model)
        {
            _Repository.SetParametersFromObject(_Wrapper, model);
            _Wrapper.AddParamNotNull(nameof(model.Id), RandomId);
            string value = RandomStringValue;
            _Wrapper.AddParam(nameof(model.Field18), value, value.Length);
            _Repository.UpdateVolatileColumnsOnStore(_Wrapper, model);
        }

        public void UpdateModel(Model model)
        {
            _Repository.SetParametersFromObject(_Wrapper, model);
            string value = RandomStringValue;
            _Wrapper.AddParam(nameof(model.Field18), value, value.Length);
            _Repository.UpdateVolatileColumnsOnUpdate(_Wrapper, model);
        }

        public Model GetModel()
        {
            return _Repository.ReadSingleObject(_Wrapper);
        }
    }

    class Wrapper : IReaderWrapper, IDbParameterCollectionWrapper
    {
        private Regex regex = new Regex("@_(.*)_[0-9a-f]+", RegexOptions.Compiled);
        private IDictionary<string, int> _MemberNameColumnIndexMap;
        private IDictionary<int, object> _Values;

        public Wrapper(IReadOnlyDictionary<string, ColumnDescriptor> memberColumnMap)
        {
            _MemberNameColumnIndexMap = memberColumnMap.ToDictionary(i => i.Key, i => i.Value.ColumnIndex);
            _Values = new Dictionary<int, object>();
        }

        private string CutTheParamName(string name)
        {
            Match match = regex.Match(name);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return name;
        }

        private int GetParamIndex(string name)
        {
            name = CutTheParamName(name);
            return _MemberNameColumnIndexMap[name]; ;
        }

        public void AddParam(string name, DateTimeOffset? value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParam(string name, float? value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParam(string name, DateTime? value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParam(string name, short? value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParam(string name, long? value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParam(string name, decimal? value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParam(string name, bool? value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParam(string name, int? value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParam(string name, double? value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParam(string name, byte? value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParam(string name, byte[] value, int size)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParam(string name, Guid? value, int size)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParam(string name, string value, int size)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParamNotNull(string name, bool value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParamNotNull(string name, float value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParamNotNull(string name, Guid value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParamNotNull(string name, DateTime value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParamNotNull(string name, long value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParamNotNull(string name, decimal value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParamNotNull(string name, int value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParamNotNull(string name, DateTimeOffset value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParamNotNull(string name, short value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParamNotNull(string name, double value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParamNotNull(string name, byte value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParamNotNull(string name, byte[] value, int size)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParamNotNull(string name, string value, int size)
        {
            _Values[GetParamIndex(name)] = value;
        }

        private T GetValue<T>(int columnIndex)
        {
            object value = _Values[columnIndex];

            if (value is T)
            {
                return (T)value;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public bool GetBoolean(int columnIndex)
        {
            return GetValue<bool>(columnIndex);
        }

        public bool? GetBooleanNullable(int columnIndex)
        {
            return GetValue<bool?>(columnIndex);
        }

        public byte GetByte(int columnIndex)
        {
            return GetValue<byte>(columnIndex);
        }

        public byte? GetByteNullable(int columnIndex)
        {
            return GetValue<byte?>(columnIndex);
        }

        public byte[] GetBytes(int columnIndex)
        {
            return GetValue<byte[]>(columnIndex);
        }

        public byte[] GetBytesNullable(int columnIndex)
        {
            return GetValue<byte[]>(columnIndex);
        }

        public DateTime GetDateTime(int columnIndex)
        {
            return GetValue<DateTime>(columnIndex);
        }

        public DateTime? GetDateTimeNullable(int columnIndex)
        {
            return GetValue<DateTime?>(columnIndex);
        }

        public DateTimeOffset GetDateTimeOffset(int columnIndex)
        {
            return GetValue<DateTimeOffset>(columnIndex);
        }

        public DateTimeOffset? GetDateTimeOffsetNullable(int columnIndex)
        {
            return GetValue<DateTimeOffset?>(columnIndex);
        }

        public decimal GetDecimal(int columnIndex)
        {
            return GetValue<decimal>(columnIndex);
        }

        public decimal? GetDecimalNullable(int columnIndex)
        {
            return GetValue<decimal?>(columnIndex);
        }

        public double GetDouble(int columnIndex)
        {
            return GetValue<double>(columnIndex);
        }

        public double? GetDoubleNullable(int columnIndex)
        {
            return GetValue<double?>(columnIndex);
        }

        public float GetFloat(int columnIndex)
        {
            return GetValue<float>(columnIndex);
        }

        public float? GetFloatNullable(int columnIndex)
        {
            return GetValue<float?>(columnIndex);
        }

        public Guid GetGuid(int columnIndex)
        {
            return GetValue<Guid>(columnIndex);
        }

        public Guid? GetGuidNullable(int columnIndex)
        {
            return GetValue<Guid?>(columnIndex);
        }

        public int GetInt(int columnIndex)
        {
            return GetValue<int>(columnIndex);
        }

        public int? GetIntNullable(int columnIndex)
        {
            return GetValue<int?>(columnIndex);
        }

        public long GetLong(int columnIndex)
        {
            return GetValue<long>(columnIndex);
        }

        public long? GetLongNullable(int columnIndex)
        {
            return GetValue<long?>(columnIndex);
        }

        public short GetShort(int columnIndex)
        {
            return GetValue<short>(columnIndex);
        }

        public short? GetShortNullable(int columnIndex)
        {
            return GetValue<short?>(columnIndex);
        }

        public string GetString(int columnIndex)
        {
            return GetValue<string>(columnIndex);
        }

        public string GetStringNullable(int columnIndex)
        {
            return GetValue<string>(columnIndex);
        }
    }



    [Storage("ModelTable")]
    class Model : IReadWriteModel<int>, IEquatable<Model>
    {
        [VolatileOnStore]
        public int Id
        {
            get;
            set;
        }

        public bool IsNew
        {
            get
            {
                return Id <= 0;
            }
        }

        public int Property1
        {
            get;
            set;
        }

        [Column("Field2",255)]
        public string Property2
        {
            get;
            set;
        }

        [Column("Field3", 255)]
        public string Property3
        {
            get;
            set;
        }

        [Attributes.Ignore]
        public string Property4
        {
            get;
            set;
        }

        [NotNull]
        public string Property5
        {
            get;
            set;
        }

        public string Property6
        {
            get;
            private set;
        }

        public string Property7
        {
            private get;
            set;
        }

        public string Property8
        {
            get;
        }

        public string Property9
        {
            set { }
        }

        protected string Property10
        {
            get;
            set;
        }

        private string Property11
        {
            get;
            set;
        }

        internal string Property12
        {
            get;
            set;
        }
        public static string Property13
        {
            get;
            set;
        }

        protected string Field14 = string.Empty;

        private string Field15 = string.Empty;

        internal string Field16 = string.Empty;

        public static string Field17 = string.Empty;

        [VolatileOnUpdate]
        [VolatileOnStore]
        public string Field18 = string.Empty;

        public override bool Equals(object obj)
        {
            Model model = obj as Model;
            
            if (model == null)
            {
                return false;
            } 

            return Equals(model);
        }

        public override int GetHashCode()
        {
            return
                Id.GetHashCode() ^ 
                Property1.GetHashCode() ^
                (Property2?.GetHashCode() ?? 0) ^
                (Property3?.GetHashCode() ?? 0) ^
                (Property5?.GetHashCode() ?? 0) ^
                (Field18?.GetHashCode() ?? 0);
        }

        public bool Equals(Model other)
        {
            return
                Id == other.Id &&
                Property1 == other.Property1 &&
                Property2 == other.Property2 &&
                Property3 == other.Property3 &&
                Property5 == other.Property5 &&
                Field18 == other.Field18;
        }

        public Model Clone()
        {
            return new Model
            {
                Id = Id,
                Property1 = Property1,
                Property2 = Property2,
                Property3 = Property3,
                Property4 = Property4,
                Property5 = Property5,
                Property6 = Property6,
                Property7 = Property7,
                Property10 = Property10,
                Property11 = Property11,
                Property12 = Property12,
                Field14 = Field14,
                Field15 = Field15,
                Field16 = Field16,
                Field18 = Field18
            };
        }
    }
}
