using System;
using FindIndex;

namespace Manage_Beatmap
{
    class TimingPoints
    {
        public int Offset { get; set; }
        public double BPM { get; set; }
        public double SliderVelocity { get; set; }
        public int SnapValue { get; set; }
        public int Type { get; set; }
        public string DataString { get; set; }
        public TimingPoints(int offset, int type, string dataString)
        {
            Offset = offset;
            Type = type;
            DataString = dataString;
            SnapValue = -1;
            if (Type == 0)
            {
                SliderVelocity = (-100) / setSecondAttribute(dataString);
                BPM = -1;
            }
            else
            {
                BPM = setSecondAttribute(dataString);
                SliderVelocity = -1;
            }
        }

        private double setSecondAttribute(string dataString)
        {
            double value = Convert.ToDouble(dataString.Substring(dataString.IndexOfWithCount(',', 1), dataString.IndexOfWithCount(',', 2) - dataString.IndexOfWithCount(',', 1) - 1).Replace('.', ','));
            return value;
        }

        public void SetNewOffsetToDataString(int newOffset)
        {
            DataString = DataString.Remove(0, DataString.IndexOf(','));
            DataString = DataString.Insert(0, newOffset.ToString());
        }
    }
}
