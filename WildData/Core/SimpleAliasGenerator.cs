﻿using System.Globalization;

namespace ModernRoute.WildData.Core
{
    public class SimpleAliasGenerator : IAliasGenerator
    {
        private int _Next;
        private string _Prefix;

        public SimpleAliasGenerator(string prefix = null)
        {
            _Prefix = prefix == null ? string.Empty : prefix;
            _Next = 0;
        }

        public string GenerateAlias()
        {
            return string.Concat(_Prefix, _Next++.ToString(CultureInfo.InvariantCulture));
        }
    }
}
