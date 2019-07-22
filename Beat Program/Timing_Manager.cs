using FindIndex;
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
    public partial class Timing_Manager : Form
    {
        public List<string> timingContent { get; internal set; } = new List<string>();
        public bool isValid { get; internal set; } = false;
        public Timing_Manager()
        {
            InitializeComponent();
            ChangeControlTexts();
            ChangeLabelPositions();
        }
        private void ChangeControlTexts()
        {
            Text = Manage_Beatmap.language.LanguageContent[Language.timingManagerFormTitle];
            label1.Text = Manage_Beatmap.language.LanguageContent[Language.timingContent];
            label2.Text = Manage_Beatmap.language.LanguageContent[Language.changeType];
            timingTextBox.Text = Manage_Beatmap.language.LanguageContent[Language.pasteTimingHere];
        }

        private void ChangeLabelPositions()
        {
            label2.Location = new Point(comboBox.Location.X - label2.Size.Width - 2, label2.Location.Y);
        }

        private void timingTextBox_Enter(object sender, EventArgs e)
        {
            if (timingTextBox.Text == Manage_Beatmap.language.LanguageContent[Language.pasteTimingHere])
                timingTextBox.Text = string.Empty;
        }

        private void applyTimingButton_Click(object sender, EventArgs e)
        {
            List<string> timingContent = timingTextBox.Lines.ToList();
            bool isFormatValid = true;
            for (int i = 0; i < timingContent.Count; i++)
                if (string.IsNullOrWhiteSpace(timingContent[i]))
                    timingContent.RemoveAt(i--);
            if (timingContent.Count != 0)
            {
                for (int i = 0; i < timingContent.Count; i++)
                {
                    string currentLine = timingContent[i];
                    for (int j = 0; j < currentLine.Length; j++)
                    {
                        if (!((currentLine[j] >= '0' && currentLine[j] <= '9') || currentLine[j] == '.' || currentLine[j] == ',' || currentLine[j] == '\n' || currentLine[j] == '-'))
                        {
                            ShowMode.Error(Manage_Beatmap.language.LanguageContent[Language.timingFormatWrong] + (j + 1).ToString());
                            timingTextBox.Focus();
                            if (!string.IsNullOrEmpty(timingTextBox.Text))
                                timingTextBox.Select(getSelectionLine(timingContent, j), timingTextBox.Lines[j].Length);
                            isFormatValid = false;
                            break;
                        }
                    }
                    if (!isFormatValid)
                        break;
                }
                if (isFormatValid)
                {
                    if (comboBox.SelectedIndex != -1)
                    {
                        bool isAllTimingPoints = true;
                        for (int i = 0; i < timingContent.Count; i++)
                        {
                            string currentLine = timingContent[i];
                            if (currentLine.Substring(currentLine.IndexOfWithCount(',', 6), 1) == "0")
                            {
                                ShowMode.Error(Manage_Beatmap.language.LanguageContent[Language.onlyTimingPoints] + (i + 1).ToString());
                                timingTextBox.Focus();
                                if (!string.IsNullOrEmpty(timingTextBox.Text))
                                    timingTextBox.Select(getSelectionLine(timingContent, i), timingTextBox.Lines[i].Length);
                                isAllTimingPoints = false;
                                break;
                            }
                        }
                        if (isAllTimingPoints)
                        {
                            if (ShowMode.QuestionWithYesNo(Manage_Beatmap.language.LanguageContent[Language.areYouSure]) == DialogResult.Yes)
                            {
                                this.timingContent = timingContent;
                                isValid = true;
                                Close();
                            }
                        }
                    }
                    else
                        ShowMode.Error(Manage_Beatmap.language.LanguageContent[Language.noFunctionSelected]);
                }
            }
            else
                ShowMode.Error(Manage_Beatmap.language.LanguageContent[Language.noInputDetected]);
        }

        private int getSelectionLine(List<string> timingContent, int lineIndex)
        {
            int index = 0;
            for (int i = 0; i < lineIndex; i++)
                index += timingContent[i].Length;
            return index + 1;
        }

        private void Timing_Manager_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isValid)
                timingContent = new List<string>();
        }
    }
}
