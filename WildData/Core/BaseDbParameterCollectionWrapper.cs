using System;

namespace ModernRoute.WildData.Core
{
    public abstract class BaseDbParameterCollectionWrapper : IDbParameterCollectionWrapper
    {
        public string Postfix
        {
            get;
            set;
        }

        private string GetParamName(string nameBase)
        {
            if (nameBase == null)
            {
                throw new ArgumentNullException(nameof(nameBase));
            }

            return string.Concat(nameBase, Postfix ?? string.Empty);
        }

        public abstract void AddParam(string name, DateTime? value);
        public abstract void AddParam(string name, double? value);
        public abstract void AddParam(string name, int? value);
        public abstract void AddParam(string name, long? value);
        public abstract void AddParam(string name, Guid? value);
        public abstract void AddParam(string name, short? value);
        public abstract void AddParam(string name, decimal? value);
        public abstract void AddParam(string name, bool? value);
        public abstract void AddParam(string name, float? value);
        public abstract void AddParam(string name, DateTimeOffset? value);
        public abstract void AddParam(string name, byte? value);
        public abstract void AddParam(string name, byte[] value, int size);
        public abstract void AddParam(string name, string value, int size);
        public abstract void AddParamNotNull(string name, DateTime value);
        public abstract void AddParamNotNull(string name, double value);
        public abstract void AddParamNotNull(string name, int value);
        public abstract void AddParamNotNull(string name, long value);
        public abstract void AddParamNotNull(string name, Guid value);
        public abstract void AddParamNotNull(string name, decimal value);
        public abstract void AddParamNotNull(string name, bool value);
        public abstract void AddParamNotNull(string name, short value);
        public abstract void AddParamNotNull(string name, float value);
        public abstract void AddParamNotNull(string name, DateTimeOffset value);
        public abstract void AddParamNotNull(string name, byte value);
        public abstract void AddParamNotNull(string name, byte[] value, int size);
        public abstract void AddParamNotNull(string name, string value, int size);

        public void AddParamBase(string nameBase, short? value)
        {
            AddParam(GetParamName(nameBase), value);
        }

        public void AddParamBase(string nameBase, long? value)
        {
            AddParam(GetParamName(nameBase), value);
        }

        public void AddParamBase(string nameBase, int? value)
        {
            AddParam(GetParamName(nameBase), value);
        }

        public void AddParamBase(string nameBase, float? value)
        {
            AddParam(GetParamName(nameBase), value);
        }

        public void AddParamBase(string nameBase, bool? value)
        {
            AddParam(GetParamName(nameBase), value);
        }

        public void AddParamBase(string nameBase, decimal? value)
        {
            AddParam(GetParamName(nameBase), value);
        }

        public void AddParamBase(string nameBase, double? value)
        {
            AddParam(GetParamName(nameBase), value);
        }

        public void AddParamBase(string nameBase, DateTime? value)
        {
            AddParam(GetParamName(nameBase), value);
        }

        public void AddParamBase(string nameBase, DateTimeOffset? value)
        {
            AddParam(GetParamName(nameBase), value);
        }

        public void AddParamBase(string nameBase, byte? value)
        {
            AddParam(GetParamName(nameBase), value);
        }

        public void AddParamBase(string nameBase, byte[] value, int size)
        {
            AddParam(GetParamName(nameBase), value, size);
        }

        public void AddParamBase(string nameBase, Guid? value)
        {
            AddParam(GetParamName(nameBase), value);
        }

        public void AddParamBase(string nameBase, string value, int size)
        {
            AddParam(GetParamName(nameBase), value, size);
        }      

        public void AddParamNotNullBase(string nameBase, short value)
        {
            AddParamNotNull(GetParamName(nameBase), value);
        }

        public void AddParamNotNullBase(string nameBase, long value)
        {
            AddParamNotNull(GetParamName(nameBase), value);
        }

        public void AddParamNotNullBase(string nameBase, int value)
        {
            AddParamNotNull(GetParamName(nameBase), value);
        }

        public void AddParamNotNullBase(string nameBase, Guid value)
        {
            AddParamNotNull(GetParamName(nameBase), value);
        }

        public void AddParamNotNullBase(string nameBase, bool value)
        {
            AddParamNotNull(GetParamName(nameBase), value);
        }

        public void AddParamNotNullBase(string nameBase, decimal value)
        {
            AddParamNotNull(GetParamName(nameBase), value);
        }

        public void AddParamNotNullBase(string nameBase, float value)
        {
            AddParamNotNull(GetParamName(nameBase), value);
        }

        public void AddParamNotNullBase(string nameBase, double value)
        {
            AddParamNotNull(GetParamName(nameBase), value);
        }

        public void AddParamNotNullBase(string nameBase, DateTime value)
        {
            AddParamNotNull(GetParamName(nameBase), value);
        }

        public void AddParamNotNullBase(string nameBase, DateTimeOffset value)
        {
            AddParamNotNull(GetParamName(nameBase), value);
        }

        public void AddParamNotNullBase(string nameBase, byte value)
        {
            AddParamNotNull(GetParamName(nameBase), value);
        }

        public void AddParamNotNullBase(string nameBase, byte[] value, int size)
        {
            AddParamNotNull(GetParamName(nameBase), value, size);
        }

        public void AddParamNotNullBase(string nameBase, string value, int size)
        {
            AddParamNotNull(GetParamName(nameBase), value, size);
        }
    }
}
