using BeatmapManager;
using MessageBoxMode;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Manage_Beatmap
{
    public partial class ResnapObjectsForm: ActionableForm<ResnapObjectsForm>
    {
        public class Members
        {
            public double StartOffset { get; internal set; } = 0;
            public double EndOffset { get; internal set; } = 0;

            public bool IsSnapGreenLines { get; internal set; } = true;
            public bool IsWholeMap { get; internal set; } = true;
            public bool IsAllTaikoDiffs { get; internal set; } = false;

            internal void Reset()
            {
                StartOffset = 0;
                EndOffset = 0;
                IsSnapGreenLines = true;
                IsWholeMap = true;
                IsAllTaikoDiffs = false;
            }
        }

        // cannot believe I had to rename the class
        // because the variable could not have the same name
        // holy shit c# you prove yourself to be worse than java
        // day by day
        public Members Values { get; } = new Members();

        public ResnapObjectsForm(Action<ResnapObjectsForm> action) : base(action)
        {
            InitializeComponent();
            startOffsetTextBox.Focus();
        }

        public ResnapObjectsForm() : base()
        {
            InitializeComponent();
            startOffsetTextBox.Focus();
        }

        private void wholeMapCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (wholeMapCheckBox.Checked)
            {
                startOffsetTextBox.Clear();
                startOffsetTextBox.Enabled = false;
                endOffsetTextBox.Clear();
                endOffsetTextBox.Enabled = false;
            }
            else
            {
                startOffsetTextBox.Enabled = true;
                endOffsetTextBox.Enabled = true;
                startOffsetTextBox.Focus();
            }
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            if (validate())
                InvokeAction();
        }

        private bool validate()
        {
            double startOffset, endOffset;
            if (!wholeMapCheckBox.Checked)
            {
                if (!startOffsetTextBox.IsValidOffsetInput())
                {
                    ShowMode.Error("Start offset: " + MainForm.language.LanguageContent[Language.timeExpressionDoesNotMatch]);
                    Values.Reset();
                    return false;
                }
                else if (!endOffsetTextBox.IsValidOffsetInput())
                {
                    ShowMode.Error("End offset: " + MainForm.language.LanguageContent[Language.timeExpressionDoesNotMatch]);
                    Values.Reset();
                    return false;
                }
                else
                {
                    startOffset = startOffsetTextBox.GetOffsetMillis();
                    endOffset = endOffsetTextBox.GetOffsetMillis();
                }
            }
            else
            {
                startOffset = 0;
                endOffset = 0;
            }

            Values.StartOffset = startOffset;
            Values.EndOffset = endOffset;
            Values.IsAllTaikoDiffs = allTaikoDiffsCheckBox.Checked;
            Values.IsWholeMap = wholeMapCheckBox.Checked;
            Values.IsSnapGreenLines = snapGreenLinesCheckBox.Checked;
            return true;
        }
    }
}
