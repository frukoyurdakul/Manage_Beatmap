﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FindIndex;

namespace Manage_Beatmap
{
    public static class Extensions
    {
        public static string ReplaceDecimalSeparator(this string input)
        {
            return input.Replace(Program.GetOriginalDecimalSeparator(), Program.GetDecimalSeparator());
        }

        public static double GetPointOffset(this string input)
        {
            int index = input.IndexOf(',');
            if (index >= 0)
                return double.Parse(input.Substring(0, index).ReplaceDecimalSeparator());
            else
                return -1;
        }

        public static bool IsPointInherited(this string input)
        {
            return input.GetBetween(',', 6, 7) == "0";
        }

        public static double GetHitObjectOffset(this string input)
        {
            string result = input.GetBetween(',', 2, 3).ReplaceDecimalSeparator();
            return double.Parse(result);
        }

        public static double GetPointValue(this string input)
        {
            string result = input.GetBetween(',', 1, 2).ReplaceDecimalSeparator();
            return double.Parse(result);
        }

        public static string GetBetween(this string input, char searched, int startCount, int endCount)
        {
            int startIndex = input.IndexOfWithCount(searched, startCount);
            int endIndex = input.IndexOfWithCount(searched, endCount);
            if (startIndex >= 0 && endIndex >= 0 && startIndex < endIndex)
                return input.Substring(startIndex, endIndex - startIndex - 1);
            else
                return "";
        }

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