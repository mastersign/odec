using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace de.mastersign.odec.utils
{
    internal static class ObjectUtils
    {
        public static bool AreEqual(byte[] a, byte[] b)
        {
            if (a == null)
            {
                return b == null;
            }
            if (b == null)
            {
                return false;
            }
            if (a.Length != b.Length)
            {
                return false;
            }
            return !a.Where((aValue, i) => aValue != b[i]).Any();
        }

        public static bool AreEqual(int[] a, int[] b)
        {
            if (a == null)
            {
                return b == null;
            }
            if (b == null)
            {
                return false;
            }
            if (a.Length != b.Length)
            {
                return false;
            }
            return !a.Where((aValue, i) => aValue != b[i]).Any();
        }

        public static bool AreEqual<T>(IEquatable<T>[] a, IEquatable<T>[] b)
        {
            if (a == null)
            {
                return b == null;
            }
            if (b == null)
            {
                return false;
            }
            if (a.Length != b.Length)
            {
                return false;
            }
            return !a.Where((aValue, i) => !aValue.Equals(b[i])).Any();
        }

        public static int GetHashCode(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            return data.Aggregate(7, (current, v) => current*v + 23);
        }

        public static int GetHashCode(int[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            return data.Aggregate(7, (current, v) => current*v + 23);
        }
    }
}