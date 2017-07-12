using ModernRoute.WildData.Attributes;
using ModernRoute.WildData.Models;
using System;

namespace ModernRoute.WildData.Test.Models
{
    [Storage("ModelTable")]
    class Model : PreModel, IReadWriteModel<int>, IEquatable<Model>
    {
        private bool _IsPersistent = false;

        [VolatileOnStore]
        public int Id
        {
            get;
            set;
        }

        public int Property1
        {
            get;
            set;
        }

        [Column("Field2", 255)]
        public string Property2
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
