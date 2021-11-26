using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FindIndex;

namespace BeatmapManager
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

        public static string SetPointValue(this string input, double value)
        {
            return SetBetween(input, ',', 1, 2, value.ToString().Replace(',', '.'));
        }

        public static string SetBetween(this string input, char searched, int startCount, int endCount, string text)
        {
            int startIndex = input.IndexOfWithCount(searched, startCount);
            int endIndex = input.IndexOfWithCount(searched, endCount);
            if (startIndex >= 0 && endIndex >= 0 && startIndex < endIndex)
            {
                string result = input.Remove(startIndex, endIndex - startIndex - 1);
                result = result.Insert(startIndex, text);
                return result;
            }
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

        public static void AdjustFormForSingleInput(this BPM_Changer form, string text)
        {
            form.label1.Text = text;
            form.Size = new Size(form.Width, 115);
            form.label2.Dispose();
            form.label1.Location = new Point(form.label1.Location.X, form.label1.Location.Y + 12);
            form.textBox1.Location = new Point(form.textBox1.Location.X, form.textBox1.Location.Y + 12);

            form.comboBox1.Dispose();
            form.checkBox1.Dispose();
        }
    }
}
