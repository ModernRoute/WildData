using System;
using System.Text;

namespace ModernRoute.WildData.Linq
{
    public class RandomAliasGenerator : IAliasGenerator
    {
        private const string _Alphabet = "0123456789abcdef";
        private const int _AliasLength = 32;

        private string _Prefix;

        private Random _Random = new Random();

        public RandomAliasGenerator(string prefix = null)
        {
            _Prefix = prefix == null ? string.Empty : prefix;
        }

        public string GenerateAlias()
        {
            StringBuilder builder = new StringBuilder(_Prefix, _AliasLength + _Prefix.Length);

            for (int i = 0; i < _AliasLength; i++)
            {
                builder.Append(_Alphabet[_Random.Next(_Alphabet.Length)]);
            }

            return builder.ToString();
        }
    }
}
