using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using FindIndex;

namespace BeatmapManager
{
    public static class Extensions
    {
        public static string ReplaceDecimalSeparator(this string input)
        {
            return input.Replace(Program.GetOriginalDecimalSeparator(), Program.GetDecimalSeparator());
        }

        public static string ToOffsetString(this double input)
        {
            return TimeSpan.FromMilliseconds(input).ToString(@"mm':'ss':'fff");
        }

        public static string ToOffsetString(this int input)
        {
            return TimeSpan.FromMilliseconds(input).ToString(@"mm':'ss':'fff");
        }

        public static int GetIntegerPart(this string input)
        {
            return int.Parse(new string(input.Where(char.IsDigit).ToArray()));
        }

        public static decimal RoundIfTooClose(this decimal input)
        {
            string inputString = input.ToString();
            int precisionIndex = inputString.IndexOf(Program.GetDecimalSeparator());
            if (precisionIndex == -1 || precisionIndex == inputString.Length - 1)
                return input;

            string floatingPointValue = inputString.Substring(precisionIndex + 1);
            if (floatingPointValue.Length > 11 && floatingPointValue.Substring(0, 11).IsAllEqualTo('9'))
                return Math.Round(input);
            else
                return input;
        }

        public static bool IsAllEqualTo(this string input, char searched)
        {
            for (int i = 0; i < input.Length; i++)
                if (input[i] != searched)
                    return false;

            return true;
        }

        public static double GetPointOffset(this string input)
        {
            int index = input.IndexOf(',');
            if (index >= 0)
                return double.Parse(input.Substring(0, index).ReplaceDecimalSeparator());
            else
                return -1;
        }

        public static double GetPointValue(this string input)
        {
            string result = input.GetBetween(',', 1, 2).ReplaceDecimalSeparator();
            return double.Parse(result);
        }

        public static bool IsPointInherited(this string input)
        {
            return input.GetBetween(',', 6, 7) == "0";
        }

        public static bool IsKiaiOpen(this string input)
        {
            string searched = input.GetAfter(',', 7);
            return (int.Parse(searched) & 1) == 1;
        }

        public static bool IsHitObjectNormal(this string input)
        {
            return (Convert.ToInt16(input.GetBetween(',', 3, 4)) & 1) == 1;
        }

        public static bool IsHitObjectSpinner(this string input)
        {
            return (Convert.ToInt16(input.GetBetween(',', 3, 4)) & 8) != 0;
        }

        public static bool IsHitObjectSlider(this string input)
        {
            return (Convert.ToInt16(input.GetBetween(',', 3, 4)) & 2) != 0;
        }

        public static double GetHitObjectOffset(this string input)
        {
            string result = input.GetBetween(',', 2, 3).ReplaceDecimalSeparator();
            return double.Parse(result);
        }

        public static double GetHitObjectSpinnerOffset(this string input)
        {
            string result = input.GetBetween(',', 5, 6).ReplaceDecimalSeparator();
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

        public static string GetBefore(this string input, char searched, int startCount)
        {
            int startIndex = 0;
            int endIndex = input.IndexOfWithCount(searched, startCount);
            if (startIndex >= 0 && endIndex >= 0 && startIndex < endIndex)
                return input.Substring(startIndex, endIndex - startIndex);
            else
                return "";
        }

        public static string GetAfter(this string input, char searched, int startCount)
        {
            int startIndex = input.IndexOfWithCount(searched, startCount);
            int endIndex = input.Length;
            if (startIndex >= 0 && endIndex >= 0 && startIndex < endIndex)
                return input.Substring(startIndex, endIndex - startIndex);
            else
                return "";
        }

        public static string SetPointValue(this string input, double value)
        {
            return SetBetween(input, ',', 1, 2, value.ToString().Replace(',', '.'));
        }

        public static string SetPointOffset(this string input, double value)
        {
            return SetBefore(input, ',', 1, value.ToString().Replace(',', '.'));
        }

        public static string SetPointMeasure(this string input, int value)
        {
            return SetBetween(input, ',', 2, 3, value.ToString());
        }

        public static string SetPointVolume(this string input, int value)
        {
            return SetBetween(input, ',', 5, 6, value.ToString());
        }

        public static string SetPointType(this string input, bool isTimingPoint)
        {
            return SetBetween(input, ',', 6, 7, isTimingPoint ? "1" : "0");
        }

        public static string SetPointKiaiOpen(this string input, bool isKiaiOpen)
        {
            int value = input.GetAfter(',', 7).GetIntegerPart();
            if (isKiaiOpen)
                value |= 0x1;
            else
                value &= ~0x1;
            return input.SetAfter(',', 7, value.ToString());
        }

        public static string SetPointOmitBarline(this string input, bool isBarlineOmitted)
        {
            int value = input.GetAfter(',', 7).GetIntegerPart();
            if (isBarlineOmitted)
                value |= 8;
            else
                value &= ~8;
            return input.SetAfter(',', 7, value.ToString());
        }

        public static string SetHitObjectOffset(this string input, double value)
        {
            return input.SetBetween(',', 2, 3, value.ToString().Replace(',', '.'));
        }

        public static string SetHitObjectPos(this string input, int x, int y)
        {
            return input.SetBefore(',', 1, x.ToString())
                .SetBetween(',', 1, 2, y.ToString());
        }

        public static string SetBetween(this string input, char searched, int startCount, int endCount, string text)
        {
            int startIndex = startCount == 0 ? 0 : input.IndexOfWithCount(searched, startCount);
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

        public static string SetBefore(this string input, char searched, int endCount, string text)
        {
            int startIndex = 0;
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

        public static string SetAfter(this string input, char searched, int startCount, string text)
        {
            int startIndex = startCount == 0 ? 0 : input.IndexOfWithCount(searched, startCount);
            int endIndex = input.Length;
            if (startIndex >= 0 && endIndex >= 0 && startIndex < endIndex)
            {
                string result = input.Remove(startIndex, endIndex - startIndex - 1);
                result = result.Insert(startIndex, text);
                return result;
            }
            else
                return "";
        }

        public static bool IsValidDecimalInput(this TextBox textBox)
        {
            string decimalSeparator = Program.GetDecimalSeparator();
            return (Regex.IsMatch(textBox.Text, @"^[0-9]+$") ||
                Regex.IsMatch(textBox.Text, @"^[0-9]+[" + decimalSeparator + "][0-9]+$"));
        }

        public static bool IsValidOffsetInput(this TextBox textBox)
        {
            return Regex.IsMatch(textBox.Text, @"\d{2}[:]\d{2}[:]\d{3}") || Regex.IsMatch(textBox.Text, @"[1-9]+[0-9]*[:]\d{2}[:]\d{2}[:]\d{3}");
        }

        public static double GetOffsetMillis(this TextBox textBox)
        {
            if (IsValidOffsetInput(textBox))
            {
                if (textBox.Text.SearchCharCount(':') == 2)
                {
                    string first = textBox.Text.Substring(0, 2);
                    string second = textBox.Text.Substring(textBox.Text.IndexOfWithCount(':', 1), 2);
                    string third = textBox.Text.Substring(textBox.Text.IndexOfWithCount(':', 2), 3);
                    return Convert.ToInt32(first) * 60000 + Convert.ToInt32(second) * 1000 + Convert.ToInt32(third);
                }
                else
                {
                    string first = textBox.Text.Substring(0, textBox.Text.IndexOf(':'));
                    string second = textBox.Text.Substring(textBox.Text.IndexOfWithCount(':', 1), 2);
                    string third = textBox.Text.Substring(textBox.Text.IndexOfWithCount(':', 2), 2);
                    string fourth = textBox.Text.Substring(textBox.Text.IndexOfWithCount(':', 3), 3);
                    return Convert.ToInt32(first) * 3600000 + Convert.ToInt32(second) * 60000 + Convert.ToInt32(third) * 1000 + Convert.ToInt32(fourth);
                }
            }
            else
                throw new ArgumentException("Invalid offset format: " + textBox.Text);
        }

        public static int GetTimingPointsStartIndex(this string[] input)
        {
            int length = input.Length;
            for (int i = 0; i < length; i++)
            {
                string line = input[i];
                if (line.Contains("[TimingPoints]") && i + 1 < length && !string.IsNullOrEmpty(input[i + 1]))
                    return i + 1;
            }
            return -1;
        }

        public static int GetTimingPointsStartIndex(this List<string> input)
        {
            int length = input.Count;
            for (int i = 0; i < length; i++)
            {
                string line = input[i];
                if (line.Contains("[TimingPoints]") && i + 1 < length && !string.IsNullOrEmpty(input[i + 1]))
                    return i + 1;
            }
            return -1;
        }

        public static int GetHitObjectsStartIndex(this string[] input)
        {
            int length = input.Length;
            for (int i = 0; i < length; i++)
            {
                string line = input[i];
                if (line.Contains("[HitObjects]") && i + 1 < length && !string.IsNullOrEmpty(input[i + 1]))
                    return i + 1;
            }
            return -1;
        }

        public static int GetHitObjectsStartIndex(this List<string> input)
        {
            int length = input.Count;
            for (int i = 0; i < length; i++)
            {
                string line = input[i];
                if (line.Contains("[HitObjects]") && i + 1 < length && !string.IsNullOrEmpty(input[i + 1]))
                    return i + 1;
            }
            return -1;
        }

        public static double FindMaxPointOffset(this string[] input)
        {
            // Surely there can't be a map with -30000 ms timing point offset, right?
            double offset = -30000;
            for (int i = input.GetTimingPointsStartIndex(); i >= 0 && i < input.Length && !string.IsNullOrWhiteSpace(input[i]); i++)
            {
                offset = Math.Max(offset, input[i].GetPointOffset());
            }
            return offset;
        }

        public static double FindMaxPointOffset(this List<string> input)
        {
            // Surely there can't be a map with -30000 ms timing point offset, right?
            double offset = -30000;
            for (int i = input.GetTimingPointsStartIndex(); i >= 0 && i < input.Count && !string.IsNullOrWhiteSpace(input[i]); i++)
            {
                offset = Math.Max(offset, input[i].GetPointOffset());
            }
            return offset;
        }

        public static double FindMinPointOffset(this string[] input)
        {
            double offset = double.MaxValue;
            for (int i = input.GetTimingPointsStartIndex(); i >= 0 && i < input.Length && !string.IsNullOrWhiteSpace(input[i]); i++)
            {
                offset = Math.Min(offset, input[i].GetPointOffset());
            }
            return offset;
        }

        public static double FindMinPointOffset(this List<string> input)
        {
            double offset = double.MaxValue;
            for (int i = input.GetTimingPointsStartIndex(); i >= 0 && i < input.Count && !string.IsNullOrWhiteSpace(input[i]); i++)
            {
                offset = Math.Min(offset, input[i].GetPointOffset());
            }
            return offset;
        }

        public static double FindMaxHitObjectOffset(this string[] input)
        {
            double offset = 0;
            for (int i = input.GetHitObjectsStartIndex(); i >= 0 && i < input.Length && !string.IsNullOrWhiteSpace(input[i]); i++)
            {
                offset = Math.Max(offset, input[i].GetHitObjectOffset());
            }
            return offset;
        }

        public static double FindMaxHitObjectOffset(this List<string> input)
        {
            double offset = 0;
            for (int i = input.GetHitObjectsStartIndex(); i >= 0 && i < input.Count && !string.IsNullOrWhiteSpace(input[i]); i++)
            {
                offset = Math.Max(offset, input[i].GetHitObjectOffset());
            }
            return offset;
        }

        public static double FindMinHitObjectOffset(this string[] input)
        {
            double offset = double.MaxValue;
            for (int i = input.GetHitObjectsStartIndex(); i >= 0 && i < input.Length && !string.IsNullOrWhiteSpace(input[i]); i++)
            {
                offset = Math.Min(offset, input[i].GetHitObjectOffset());
            }
            return offset;
        }

        public static double FindMinHitObjectOffset(this List<string> input)
        {
            double offset = double.MaxValue;
            for (int i = input.GetHitObjectsStartIndex(); i >= 0 && i < input.Count && !string.IsNullOrWhiteSpace(input[i]); i++)
            {
                offset = Math.Min(offset, input[i].GetHitObjectOffset());
            }
            return offset;
        }

        public static bool IsTaikoDifficulty(this string[] input)
        {
            int length = input.Length;
            for (int i = 0; i < length; i++)
            {
                string line = input[i];
                if (line.Contains("Mode"))
                    return line.GetIntegerPart() == 1;
            }
            throw new ArgumentException("The beatmap file is incomplete. Mode of the difficulty could not be determined.");
        }

        public static bool IsTaikoDifficulty(this List<string> input)
        {
            int length = input.Count;
            for (int i = 0; i < length; i++)
            {
                string line = input[i];
                if (line.Contains("Mode"))
                    return line.GetIntegerPart() == 1;
            }
            throw new ArgumentException("The beatmap file is incomplete. Mode of the difficulty could not be determined.");
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
