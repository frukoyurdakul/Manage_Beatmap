using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manage_Beatmap
{
    public static class ListExtensions
    {
        public static T GetClosest<T>(this List<T> list, T item)
        {
            if (list.Count == 0)
                throw new ArgumentException("Cannot get closest value on an empty list.");

            int index = list.BinarySearch(item);
            if (index == -1)
                return list[0];
            else
            {
                int targetIndex = ~index - 1;
                if (targetIndex >= 0 && targetIndex < list.Count)
                    return list[targetIndex];
                else
                    throw new ArgumentException("BinarySearch returned index: " + targetIndex + ", count: " + list.Count);
            }
        }
    }
}
