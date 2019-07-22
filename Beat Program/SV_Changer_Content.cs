using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manage_Beatmap
{
    public class SV_Changer_Content
    {
        public bool reOpenCheckBox { get; set; } = false;
        public bool betweenTimeModeCheckBox { get; set; } = false;
        public bool putNotesBySnaps { get; set; } = false;
        public int comboBoxSelectedIndex { get; set; } = -1;
        public string timeTextBox { get; set; } = string.Empty;
        public string firstSVTextBox { get; set; } = string.Empty;
        public string lastSVTextBox { get; set; } = string.Empty;
        public string countOrLastTimeTextBox { get; set; } = string.Empty;
        public SV_Changer_Content(bool betweenTimeModeCheckBox, bool putNotesBySnaps, int comboBoxSelectedIndex, string timeTextBox, string firstSVTextBox, string lastSVTextBox, string countOrLastTimeTextBox)
        {
            this.betweenTimeModeCheckBox = betweenTimeModeCheckBox;
            this.putNotesBySnaps = putNotesBySnaps;
            this.comboBoxSelectedIndex = comboBoxSelectedIndex;
            this.timeTextBox = timeTextBox;
            this.firstSVTextBox = firstSVTextBox;
            this.lastSVTextBox = lastSVTextBox;
            this.countOrLastTimeTextBox = countOrLastTimeTextBox;
        }
        public SV_Changer_Content(SV_Changer_Content content)
        {
            betweenTimeModeCheckBox = content.betweenTimeModeCheckBox;
            putNotesBySnaps = content.putNotesBySnaps;
            comboBoxSelectedIndex = content.comboBoxSelectedIndex;
            timeTextBox = content.timeTextBox;
            firstSVTextBox = content.firstSVTextBox;
            lastSVTextBox = content.lastSVTextBox;
            countOrLastTimeTextBox = content.countOrLastTimeTextBox;
        }
    }
}
