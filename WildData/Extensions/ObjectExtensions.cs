using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernRoute.WildData.Extensions
{
    public static class ObjectExtensions
    {
        public static bool Is<T>(this object obj)
        {
            return obj is T;
        }

        public static T As<T>(this object obj) where T : class
        {
            return obj as T;
        }

        public static T Of<T>(this object obj)
        {
            return (T)obj;
        }
    }
}
