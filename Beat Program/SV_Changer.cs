﻿using System;
using System.Drawing;
using System.Windows.Forms;
using MessageBoxMode;
using FindIndex;
using System.Text.RegularExpressions;
using System.Threading;
using System.Globalization;

namespace BeatmapManager
{
    public partial class SV_Changer
        #if DEBUG
            : ActionableForm<SV_Changer>
        #else
            : ActionableForm  
        #endif
    {
        public double FirstGridValue { get; internal set; }
        public double LastGridValue { get; internal set; }
        public double FirstSV { get; internal set; }
        public double LastSV { get; internal set; }
        public double TargetBPM { get; internal set; }
        public double ExpMultiplier { get; internal set; }
        public int FirstTimeInMilliSeconds { get; internal set; }
        public int LastTimeInMilliSeconds { get; internal set; }
        public int Count { get; internal set; }
        public int SvOffset { get; internal set; }
        public DialogResult Status { get; internal set; }
        public bool IsNoteMode { get; internal set; }
        public bool IsBetweenTimeMode { get; internal set; }
        private bool isTargetBpmEntered = false, isSvOffsetEntered = false, isExpMultiplierEntered = false, isMessageShown = false;

        public SV_Changer() : base()
        {

        }

        public SV_Changer(Action<SV_Changer> action) : base(action)
        {
            InitializeComponent();
            ChangeControlTexts();
            ChangeLabelPositions();
            if (MainForm.savedContent != null)
                FillSavedContent();
        }

        private void ChangeControlTexts()
        {
            Text = MainForm.language.LanguageContent[Language.SVchangerFormTitle];
            label4.Text = MainForm.language.LanguageContent[Language.copyTimeLabel];
            label1.Text = MainForm.language.LanguageContent[Language.setFirstSVlabel];
            label2.Text = MainForm.language.LanguageContent[Language.setLastSVlabel];
            label7.Text = MainForm.language.LanguageContent[Language.countLabel];
            label5.Text = MainForm.language.LanguageContent[Language.targetBPMlabel];
            label3.Text = MainForm.language.LanguageContent[Language.gridSnapLabel];
            label8.Text = MainForm.language.LanguageContent[Language.svOffset];
            bpmTextBox.Text = MainForm.language.LanguageContent[Language.optionalInitialIsFirst];
            checkBox1.Text = MainForm.language.LanguageContent[Language.checkBox];
            button.Text = MainForm.language.LanguageContent[Language.addInheritedPointsButton];
            checkBox2.Text = MainForm.language.LanguageContent[Language.activateBetweenTimeMode];
            checkBox2.Checked = true;
        }
        private void ChangeLabelPositions()
        {
            label4.Location = new Point(timeTextBox.Location.X - label4.Size.Width - 2, label4.Location.Y);
            label1.Location = new Point(firstTextBox.Location.X - label1.Size.Width - 2, label1.Location.Y);
            label2.Location = new Point(lastTextBox.Location.X - label2.Size.Width - 2, label2.Location.Y);
            label7.Location = new Point(countOrLastTimeTextBox.Location.X - label7.Size.Width - 2, label7.Location.Y);
            label5.Location = new Point(bpmTextBox.Location.X - label5.Size.Width - 2, label5.Location.Y);
            label3.Location = new Point(comboBox.Location.X - label3.Size.Width - 2, label3.Location.Y);
        }
        private void FillSavedContent()
        {
            isMessageShown = true;
            countOrLastTimeTextBox.Text = MainForm.savedContent.countOrLastTimeTextBox;
            timeTextBox.Text = MainForm.savedContent.timeTextBox;
            svOffsetTextBox.Text = MainForm.savedContent.svOffsetTextBox;
            firstTextBox.Text = MainForm.savedContent.firstSVTextBox;
            lastTextBox.Text = MainForm.savedContent.lastSVTextBox;
            checkBox1.Checked = MainForm.savedContent.putNotesBySnaps;
            checkBox2.Checked = MainForm.savedContent.betweenTimeModeCheckBox;
            comboBox.SelectedIndex = MainForm.savedContent.comboBoxSelectedIndex;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (checkVariables())
            {
                if (Status == DialogResult.Yes || Status == DialogResult.No)
                {
                    if (checkBox1.Checked)
                        IsNoteMode = true;
                    else
                        IsNoteMode = false;
                    MainForm.savedContent = new SV_Changer_Content
                             (
                                 checkBox2.Checked,
                                 checkBox1.Checked,
                                 comboBox.SelectedIndex,
                                 timeTextBox.Text,
                                 firstTextBox.Text,
                                 lastTextBox.Text,
                                 countOrLastTimeTextBox.Text,
                                 svOffsetTextBox.Text
                             );
                    InvokeAction();
                }
            }
        }
        private bool checkVariables()
        {
            foreach (Control c in this.Controls)
            {
                if (c is TextBox)
                {
                    if ((string.IsNullOrEmpty(c.Text) || string.IsNullOrWhiteSpace(c.Text)) && c.Name != "bpmTextBox")
                    {
                        ShowMode.Error(MainForm.language.LanguageContent[Language.oneOrMoreValuesEmpty]);
                        return false;
                    }
                }
            }
            if (timeTextBox.IsValidOffsetInput())
            {
                string decimalSeparator = Program.GetDecimalSeparator();
                if (firstTextBox.IsValidDecimalInput() && lastTextBox.IsValidDecimalInput())
                {
                    if(checkBox2.Checked)
                    {
                        if(!countOrLastTimeTextBox.IsValidOffsetInput())
                        {
                            ShowMode.Information(MainForm.language.LanguageContent[Language.timeExpressionDoesNotMatch]);
                            return false;
                        }
                    }
                    else
                    {
                        if (!Regex.IsMatch(countOrLastTimeTextBox.Text, @"^[1-9]+[0-9]*$"))
                        {
                            ShowMode.Error(MainForm.language.LanguageContent[Language.enterPositiveToCount]);
                            return false;
                        }
                    }
                    if (comboBox.SelectedIndex == -1 && !checkBox1.Checked)
                    {
                        ShowMode.Error(MainForm.language.LanguageContent[Language.selectGridSnap]);
                        return false;
                    }
                    if (!bpmTextBox.IsValidDecimalInput() && !string.IsNullOrWhiteSpace(bpmTextBox.Text) && bpmTextBox.Text != MainForm.language.LanguageContent[Language.optionalInitialIsFirst])
                    {
                        ShowMode.Error(MainForm.language.LanguageContent[Language.BPMwrong]);
                        return false;
                    }
                    string result = svOffsetTextBox.Text.Trim();
                    int svOffsetLocal;
                    try
                    {
                        if (string.IsNullOrWhiteSpace(result) || result == MainForm.language.LanguageContent[Language.optionalDefaultIsMinusThree])
                            svOffsetLocal = -3;
                        else
                            svOffsetLocal = Convert.ToInt32(result);
                    }
                    catch (FormatException)
                    {
                        ShowMode.Error("The value entered for SV Offset is incorrect.");
                        return false;
                    }
                    if (svOffsetLocal > 0 && checkBox1.Checked)
                    {
                        if (ShowMode.QuestionWithYesNo("The SV offset should be 0 or a negative value. Using a positive value will put the inherited points after the notes themselves. Are you sure you want to continue?") == DialogResult.No)
                            return false;
                    }

                    double expMultiplierLocal;
                    if (expMulTextBox.Text == MainForm.language.LanguageContent[Language.optionalDefaultIsOne])
                        expMultiplierLocal = 1;
                    else if (!expMulTextBox.IsValidDecimalInput())
                    {
                        ShowMode.Error("The exponential value should be a decimal value.");
                        return false;
                    }
                    else
                    {
                        expMultiplierLocal = Convert.ToDouble(expMulTextBox.Text);
                    }
                    if (expMultiplierLocal <= 0)
                    {
                        ShowMode.Error("The exponential multiplier should be bigger than 0.");
                        return false;
                    }

                    FirstTimeInMilliSeconds = (int) timeTextBox.GetOffsetMillis();
                    if (checkBox2.Checked)
                        LastTimeInMilliSeconds = (int) countOrLastTimeTextBox.GetOffsetMillis();
                    if(checkBox2.Checked && LastTimeInMilliSeconds <= FirstTimeInMilliSeconds)
                    {
                        ShowMode.Error(MainForm.language.LanguageContent[Language.lastTimeCannotBeSmaller]);
                        return false;
                    }

                    IsNoteMode = checkBox1.Checked;
                    IsBetweenTimeMode = checkBox2.Checked;
                    if(!IsNoteMode)
                    {
                        FirstGridValue = Convert.ToDouble(comboBox.Items[comboBox.SelectedIndex].ToString().Substring(0, comboBox.Items[comboBox.SelectedIndex].ToString().IndexOf('/')));
                        LastGridValue = Convert.ToDouble(comboBox.Items[comboBox.SelectedIndex].ToString().Substring(comboBox.Items[comboBox.SelectedIndex].ToString().IndexOf('/') + 1));
                    }
                    FirstSV = Convert.ToDouble(firstTextBox.Text);
                    LastSV = Convert.ToDouble(lastTextBox.Text);
                    if(!checkBox2.Checked)
                        Count = Convert.ToInt32(countOrLastTimeTextBox.Text);
                    if (bpmTextBox.Text != MainForm.language.LanguageContent[Language.optionalInitialIsFirst] && !string.IsNullOrWhiteSpace(bpmTextBox.Text))
                        TargetBPM = Double.Parse(bpmTextBox.Text);
                    else
                        TargetBPM = 0;
                    SvOffset = svOffsetLocal;
                    ExpMultiplier = expMultiplierLocal;
                }
                else
                {
                    ShowMode.Error(MainForm.language.LanguageContent[Language.SVchangesWrong]);
                    return false;
                }
            }
            else
            {
                ShowMode.Error(MainForm.language.LanguageContent[Language.timeExpressionDoesNotMatch]);
                return false;
            }
            if (checkBox1.Checked)
                Status = ShowMode.QuestionWithYesNoCancel(MainForm.language.LanguageContent[Language.rememberSnappingNotes]);
            else
                Status = ShowMode.QuestionWithYesNoCancel(MainForm.language.LanguageContent[Language.areYouSureToContinue]);
            return true;
        }

        private void SV_Changer_Resize(object sender, EventArgs e)
        {
            
        }

        private void bpmTextBox_Enter(object sender, EventArgs e)
        {
            if (!isTargetBpmEntered)
            {
                bpmTextBox.Text = string.Empty;
                isTargetBpmEntered = true;
            }
        }

        private void svOffsetTextBox_Enter(object sender, EventArgs e)
        {
            if (!isSvOffsetEntered)
            {
                svOffsetTextBox.Text = string.Empty;
                isSvOffsetEntered = true;
            }
        }

        private void expMulTextBox_Enter(object sender, EventArgs e)
        {
            if (!isExpMultiplierEntered)
            {
                expMulTextBox.Text = string.Empty;
                isExpMultiplierEntered = true;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                label7.Text = MainForm.language.LanguageContent[Language.copyLastTime];
                label7.Location = new Point(countOrLastTimeTextBox.Location.X - label7.Width - 2, countOrLastTimeTextBox.Location.Y + 3);
                comboBox.Enabled = false;
                comboBox.SelectedIndex = -1;
                checkBox1.Checked = true;
                checkBox1.Enabled = false;
            }
            else
            {
                label7.Text = MainForm.language.LanguageContent[Language.countLabel];
                label7.Location = new Point(countOrLastTimeTextBox.Location.X - label7.Width - 2, countOrLastTimeTextBox.Location.Y + 3);
                comboBox.Enabled = true;
                checkBox1.Enabled = true;
            }
        }

        private void SV_Changer_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                if (!isMessageShown && !checkBox2.Checked)
                {
                    ShowMode.Warning(MainForm.language.LanguageContent[Language.rememberSnappingNotesBetweenArea]);
                    isMessageShown = true;
                }
                comboBox.Enabled = false;
                comboBox.SelectedIndex = -1;
            }
            else
                comboBox.Enabled = true;
        }
    }
}
