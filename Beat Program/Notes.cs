using System;
using FindIndex;

namespace Manage_Beatmap
{
    class Notes
    {
        public int SnapValue { get; set; }
        public int EndSnapValue { get; set; }
        public int Offset { get; set; }
        public int EndOffset { get; set; }
        public bool IsSlider { get; internal set; }
        public bool IsSpinner { get; internal set; }
        public string DataString { get; set; }

        public Notes(int offset, string dataString)
        {
            Offset = offset;
            SnapValue = -1;
            EndSnapValue = -1;
            DataString = dataString;
            IsSlider = isSlider(dataString);
            IsSpinner = isSpinner(dataString);
            if (IsSpinner)
                SetEndOffset(dataString);
            else
                EndOffset = -1;
        }

        public void SetEndOffset(string dataString, double beatDuration, double sliderVelocity, double sliderMultiplier)
        {
            int repeat = Convert.ToInt32(dataString.Substring(dataString.IndexOfWithCount(',', 6), dataString.IndexOfWithCount(',', 7) - dataString.IndexOfWithCount(',', 6) - 1));
            double pixelLength;
            if (dataString.SearchCharCount(',') < 8)
                pixelLength = Convert.ToDouble(dataString.Substring(dataString.IndexOfWithCount(',', 7)).ReplaceDecimalSeparator());
            else
                pixelLength = Convert.ToDouble(dataString.Substring(dataString.IndexOfWithCount(',', 7), dataString.IndexOfWithCount(',', 8) - dataString.IndexOfWithCount(',', 7) - 1).ReplaceDecimalSeparator());
            EndOffset = Offset + (int)((pixelLength / sliderVelocity) / (100 * sliderMultiplier) * beatDuration * repeat);
        }

        public void SetEndOffset(string dataString)
        {
            EndOffset = Convert.ToInt32(dataString.Substring(dataString.IndexOfWithCount(',', 5), dataString.IndexOfWithCount(',', 6) - dataString.IndexOfWithCount(',', 5) - 1));
        }

        private bool isSlider(string dataString)
        {
            int type = Convert.ToInt32(dataString.Substring(dataString.IndexOfWithCount(',', 3), dataString.IndexOfWithCount(',', 4) - dataString.IndexOfWithCount(',', 3) - 1));
            string bits = Convert.ToString(type, 2).PadLeft(8, '0');
            return bits[bits.Length - 2] == '1';
        }

        private bool isSpinner(string dataString)
        {
            int type = Convert.ToInt32(dataString.Substring(dataString.IndexOfWithCount(',', 3), dataString.IndexOfWithCount(',', 4) - dataString.IndexOfWithCount(',', 3) - 1));
            string bits = Convert.ToString(type, 2).PadLeft(8, '0');
            return bits[bits.Length - 4] == '1';
        }

        public bool HasEndOffset()
        {
            return EndOffset > -1;
        }

        public void SetNewOffsetToDataString(int newOffset)
        {
            DataString = DataString.Remove(DataString.IndexOfWithCount(',', 2), DataString.IndexOfWithCount(',', 3) - DataString.IndexOfWithCount(',', 2) - 1);
            DataString = DataString.Insert(DataString.IndexOfWithCount(',', 2), newOffset.ToString());
        }

        public void SetNewSpinnerEndOffsetToDataString(int newOffset)
        {
            DataString = DataString.Remove(DataString.IndexOfWithCount(',', 5), DataString.IndexOfWithCount(',', 6) - DataString.IndexOfWithCount(',', 5) - 1);
            DataString = DataString.Insert(DataString.IndexOfWithCount(',', 5), newOffset.ToString());
        }
    }
}
