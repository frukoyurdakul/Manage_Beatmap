using System;
using System.Windows.Forms;
using MessageBoxMode;
using System.Threading;
using System.Globalization;
using System.Drawing;

namespace BeatmapManager
{
    public partial class SV_Equalizer : ActionableForm<SV_Equalizer>
    {
        private const string defaultText = "Default is 1.00x";

        public double BpmValue { get; set; } = -1;
        public double SvValue { get; internal set; } = 1;
        public double StartOffset { get; internal set; } = 1;
        public double EndOffset { get; internal set; } = 1;
        public bool ApplyToWholeRange { get; internal set; }
        public bool ApplyToAllTaikoDiffs { get; internal set; }
   
        public SV_Equalizer(Action<SV_Equalizer> action) : base(action)
        {
            InitializeComponent();
            ChangeControlTexts();
        }

        private void ChangeControlTexts()
        {
            Text = MainForm.language.LanguageContent[Language.SVequalizerFormTitle];
            label1.Text = MainForm.language.LanguageContent[Language.enterBPMlabel];
            label2.Text = MainForm.language.LanguageContent[Language.enterSVlabel];
            label3.Text = MainForm.language.LanguageContent[Language.typeLabel];
            button1.Text = MainForm.language.LanguageContent[Language.applyButton];
        }
       
        private void button1_Click(object sender, EventArgs e)
        {
            double bpmValue = 0;
            double svValue = 0;
            double startOffset = 0;
            double endOffset = 0;

            if (!bpmTextBox.IsValidDecimalInput())
            {
                ShowMode.Error("The BPM format is not correct. Example: \"120\" or \"135" + Program.GetDecimalSeparator() + "46\".");
                return;
            }
            else
                bpmValue = Convert.ToDouble(bpmTextBox.Text.Trim());

            if (!svTextBox.IsValidDecimalInput())
            {
                if (svTextBox.Text == defaultText)
                    svValue = 1;
                else
                {
                    ShowMode.Error("The SV format is not correct. Example: \"1\" or \"1" + Program.GetDecimalSeparator() + "35\".");
                    return;
                }
            }
            else
                svValue = Convert.ToDouble(svTextBox.Text.Trim());

            if (!applyFullyCheckBox.Checked)
            {
                if (!startOffsetTextBox.IsValidOffsetInput())
                {
                    ShowMode.Error("The start offset is wrong. " + MainForm.language.LanguageContent[Language.timeExpressionDoesNotMatch]);
                    return;
                }
                else
                    startOffset = startOffsetTextBox.GetOffsetMillis();

                if (!endOffsetTextBox.IsValidOffsetInput())
                {
                    ShowMode.Error("The end offset is wrong. " + MainForm.language.LanguageContent[Language.timeExpressionDoesNotMatch]);
                    return;
                }
                else
                    endOffset = endOffsetTextBox.GetOffsetMillis();
            }
            else
            {
                startOffset = 0;
                endOffset = int.MaxValue;
            }

            string questionText = applyTaikoMapsCheckBox.Checked
                ? "This operation will equalize the SV for the selected region, for all taiko difficulties in the mapset. Are you sure you want to continue?"
                : "This operation will equalize the SV for the selected region. Are you sure you want to continue?";

            // If we made it here, it means all values are correct.
            // Ask the confirmation and continue if agreed.
            if (ShowMode.QuestionWithYesNo(questionText) == DialogResult.Yes)
            {
                BpmValue = bpmValue;
                SvValue = svValue;
                StartOffset = startOffset;
                EndOffset = endOffset;
                ApplyToAllTaikoDiffs = applyTaikoMapsCheckBox.Checked;
                ApplyToWholeRange = applyFullyCheckBox.Checked;
                InvokeAction();
            }
        }
        private void textBox2_Enter(object sender, EventArgs e)
        {
            svTextBox.Text = "";
        }

        private void applyFullyCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (applyFullyCheckBox.Checked)
            {
                startOffsetTextBox.Clear();
                endOffsetTextBox.Clear();
                startOffsetTextBox.Enabled = false;
                endOffsetTextBox.Enabled = false;
            }
            else
            {
                startOffsetTextBox.Enabled = true;
                endOffsetTextBox.Enabled = true;
            }
        }
    }
}
