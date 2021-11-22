using System;
using System.Drawing;
using System.Windows.Forms;
using MessageBoxMode;
using FindIndex;
using System.Text.RegularExpressions;
using System.Threading;
using System.Globalization;

namespace Manage_Beatmap
{
    public partial class SV_Changer
        #if DEBUG
            : ActionableForm<SV_Changer>
        #else
            : ActionableForm  
        #endif
    {
        public double FirstGridValue { get; set; }
        public double LastGridValue { get; set; }
        public double FirstSV { get; set; }
        public double LastSV { get; set; }
        public double TargetBPM { get; set; }
        public int FirstTimeInMilliSeconds { get; set; }
        public int LastTimeInMilliSeconds { get; set; }
        public int Count { get; set; }
        public int SvOffset { get; set; }
        public DialogResult Status { get; set; }
        public bool isNoteMode { get; set; }
        public bool isBetweenTimeMode { get; set; }
        private bool isTargetBpmEntered = false, isSvOffsetEntered = false, isButtonClicked = false, isMessageShown = false;

        public SV_Changer() : base()
        {

        }

        public SV_Changer(Action<SV_Changer> action) : base(action)
        {
            InitializeComponent();
            ChangeControlTexts();
            ChangeLabelPositions();
            if (Manage_Beatmap.savedContent != null)
                FillSavedContent();
        }

        private void ChangeControlTexts()
        {
            Text = Manage_Beatmap.language.LanguageContent[Language.SVchangerFormTitle];
            label6.Text = Manage_Beatmap.language.LanguageContent[Language.SVchangerTopLabel];
            label4.Text = Manage_Beatmap.language.LanguageContent[Language.copyTimeLabel];
            label1.Text = Manage_Beatmap.language.LanguageContent[Language.setFirstSVlabel];
            label2.Text = Manage_Beatmap.language.LanguageContent[Language.setLastSVlabel];
            label7.Text = Manage_Beatmap.language.LanguageContent[Language.countLabel];
            label5.Text = Manage_Beatmap.language.LanguageContent[Language.targetBPMlabel];
            label3.Text = Manage_Beatmap.language.LanguageContent[Language.gridSnapLabel];
            label8.Text = Manage_Beatmap.language.LanguageContent[Language.svOffset];
            bpmTextBox.Text = Manage_Beatmap.language.LanguageContent[Language.optionalInitialIsFirst];
            checkBox1.Text = Manage_Beatmap.language.LanguageContent[Language.checkBox];
            button.Text = Manage_Beatmap.language.LanguageContent[Language.addInheritedPointsButton];
            checkBox2.Text = Manage_Beatmap.language.LanguageContent[Language.activateBetweenTimeMode];
            checkBox3.Text = Manage_Beatmap.language.LanguageContent[Language.reOpenWindow];
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
            countOrLastTimeTextBox.Text = Manage_Beatmap.savedContent.countOrLastTimeTextBox;
            timeTextBox.Text = Manage_Beatmap.savedContent.timeTextBox;
            svOffsetTextBox.Text = Manage_Beatmap.savedContent.svOffsetTextBox;
            firstTextBox.Text = Manage_Beatmap.savedContent.firstSVTextBox;
            lastTextBox.Text = Manage_Beatmap.savedContent.lastSVTextBox;
            checkBox1.Checked = Manage_Beatmap.savedContent.putNotesBySnaps;
            checkBox2.Checked = Manage_Beatmap.savedContent.betweenTimeModeCheckBox;
            checkBox3.Checked = true;
            comboBox.SelectedIndex = Manage_Beatmap.savedContent.comboBoxSelectedIndex;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (checkVariables())
            {
                if (Status == DialogResult.Yes || Status == DialogResult.No)
                {
                    if (checkBox1.Checked)
                        isNoteMode = true;
                    else
                        isNoteMode = false;
                    if (checkBox3.Checked)
                    {
                        Manage_Beatmap.savedContent = new SV_Changer_Content
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
                    }
                    else
                        Manage_Beatmap.savedContent = null;
                    isButtonClicked = true;
                    Close();
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
                        ShowMode.Error(Manage_Beatmap.language.LanguageContent[Language.oneOrMoreValuesEmpty]);
                        return false;
                    }
                }
            }
            if (Regex.IsMatch(timeTextBox.Text, @"\d{2}[:]\d{2}[:]\d{3}") || Regex.IsMatch(timeTextBox.Text, @"[1-9]+[0-9]*[:]\d{2}[:]\d{2}[:]\d{3}"))
            {
                string decimalSeparator = Program.GetDecimalSeparator();
                if (Regex.IsMatch(firstTextBox.Text, @"^[0-9]+$") ||
                Regex.IsMatch(firstTextBox.Text, @"^[0-9]+[" + decimalSeparator + "][0-9]+$") ||
                Regex.IsMatch(lastTextBox.Text, @"^[0-9]+$") ||
                Regex.IsMatch(lastTextBox.Text, @"^[0-9]+[" + decimalSeparator + "][0-9]+$"))
                {
                    if(checkBox2.Checked)
                    {
                        if(!(Regex.IsMatch(countOrLastTimeTextBox.Text, @"\d{2}[:]\d{2}[:]\d{3}") || 
                            Regex.IsMatch(countOrLastTimeTextBox.Text, @"[1-9]+[0-9]*[:]\d{2}[:]\d{2}[:]\d{3}")))
                        {
                            ShowMode.Information(Manage_Beatmap.language.LanguageContent[Language.timeExpressionDoesNotMatch]);
                            return false;
                        }
                    }
                    else
                    {
                        if (!Regex.IsMatch(countOrLastTimeTextBox.Text, @"^[1-9]+[0-9]*$"))
                        {
                            ShowMode.Error(Manage_Beatmap.language.LanguageContent[Language.enterPositiveToCount]);
                            return false;
                        }
                    }
                    if (comboBox.SelectedIndex == -1 && !checkBox1.Checked)
                    {
                        ShowMode.Error(Manage_Beatmap.language.LanguageContent[Language.selectGridSnap]);
                        return false;
                    }
                    if (!Regex.IsMatch(bpmTextBox.Text, @"^[1-9]+[0-9]*$") && !Regex.IsMatch(bpmTextBox.Text, @"^[1-9]+[0-9]*[,][0-9]+$") && !string.IsNullOrWhiteSpace(bpmTextBox.Text) && bpmTextBox.Text != Manage_Beatmap.language.LanguageContent[Language.optionalInitialIsFirst])
                    {
                        ShowMode.Error(Manage_Beatmap.language.LanguageContent[Language.BPMwrong]);
                        return false;
                    }
                    string result = svOffsetTextBox.Text.Trim();
                    int svOffsetLocal;
                    try
                    {
                        if (string.IsNullOrWhiteSpace(result) || result == Manage_Beatmap.language.LanguageContent[Language.optionalDefaultIsMinusThree])
                            svOffsetLocal = -3;
                        else
                            svOffsetLocal = Convert.ToInt32(svOffsetTextBox.Text.Trim());
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
                    if (timeTextBox.Text.SearchCharCount(':') == 2)
                    {
                        string first = timeTextBox.Text.Substring(0, 2);
                        string second = timeTextBox.Text.Substring(timeTextBox.Text.IndexOfWithCount(':', 1), 2);
                        string third = timeTextBox.Text.Substring(timeTextBox.Text.IndexOfWithCount(':', 2), 3);
                        FirstTimeInMilliSeconds = Convert.ToInt32(first) * 60000 + Convert.ToInt32(second) * 1000 + Convert.ToInt32(third);
                        if(checkBox2.Checked)
                        {
                            first = countOrLastTimeTextBox.Text.Substring(0, 2);
                            second = countOrLastTimeTextBox.Text.Substring(countOrLastTimeTextBox.Text.IndexOfWithCount(':', 1), 2);
                            third = countOrLastTimeTextBox.Text.Substring(countOrLastTimeTextBox.Text.IndexOfWithCount(':', 2), 3);
                            LastTimeInMilliSeconds = Convert.ToInt32(first) * 60000 + Convert.ToInt32(second) * 1000 + Convert.ToInt32(third);
                        }
                    }
                    else
                    {
                        string first = timeTextBox.Text.Substring(0, timeTextBox.Text.IndexOf(':'));
                        string second = timeTextBox.Text.Substring(timeTextBox.Text.IndexOfWithCount(':', 1), 2);
                        string third = timeTextBox.Text.Substring(timeTextBox.Text.IndexOfWithCount(':', 2), 2);
                        string fourth = timeTextBox.Text.Substring(timeTextBox.Text.IndexOfWithCount(':',3), 3);
                        FirstTimeInMilliSeconds = Convert.ToInt32(first) * 3600000 + Convert.ToInt32(second) * 60000 + Convert.ToInt32(third) * 1000 + Convert.ToInt32(fourth);
                        if(checkBox2.Checked)
                        {
                            first = countOrLastTimeTextBox.Text.Substring(0, timeTextBox.Text.IndexOf(':'));
                            second = countOrLastTimeTextBox.Text.Substring(countOrLastTimeTextBox.Text.IndexOfWithCount(':', 1), 2);
                            third = countOrLastTimeTextBox.Text.Substring(countOrLastTimeTextBox.Text.IndexOfWithCount(':', 2), 2);
                            fourth = countOrLastTimeTextBox.Text.Substring(countOrLastTimeTextBox.Text.IndexOfWithCount(':', 3), 3);
                            LastTimeInMilliSeconds = Convert.ToInt32(first) * 3600000 + Convert.ToInt32(second) * 60000 + Convert.ToInt32(third) * 1000 + Convert.ToInt32(fourth);
                        }
                    }
                    if(checkBox2.Checked && LastTimeInMilliSeconds <= FirstTimeInMilliSeconds)
                    {
                        ShowMode.Error(Manage_Beatmap.language.LanguageContent[Language.lastTimeCannotBeSmaller]);
                        return false;
                    }
                    isNoteMode = checkBox1.Checked;
                    isBetweenTimeMode = checkBox2.Checked;
                    if(!isNoteMode)
                    {
                        FirstGridValue = Convert.ToDouble(comboBox.Items[comboBox.SelectedIndex].ToString().Substring(0, comboBox.Items[comboBox.SelectedIndex].ToString().IndexOf('/')));
                        LastGridValue = Convert.ToDouble(comboBox.Items[comboBox.SelectedIndex].ToString().Substring(comboBox.Items[comboBox.SelectedIndex].ToString().IndexOf('/') + 1));
                    }
                    FirstSV = Convert.ToDouble(firstTextBox.Text);
                    LastSV = Convert.ToDouble(lastTextBox.Text);
                    if(!checkBox2.Checked)
                        Count = Convert.ToInt32(countOrLastTimeTextBox.Text);
                    if (bpmTextBox.Text != Manage_Beatmap.language.LanguageContent[Language.optionalInitialIsFirst] && !string.IsNullOrWhiteSpace(bpmTextBox.Text))
                        TargetBPM = Double.Parse(bpmTextBox.Text);
                    else
                        TargetBPM = 0;
                    SvOffset = svOffsetLocal;
                }
                else
                {
                    ShowMode.Error(Manage_Beatmap.language.LanguageContent[Language.SVchangesWrong]);
                    return false;
                }
            }
            else
            {
                ShowMode.Information(Manage_Beatmap.language.LanguageContent[Language.timeExpressionDoesNotMatch]);
                return false;
            }
            if (checkBox1.Checked)
                Status = ShowMode.QuestionWithYesNoCancel(Manage_Beatmap.language.LanguageContent[Language.rememberSnappingNotes]);
            else
                Status = ShowMode.QuestionWithYesNoCancel(Manage_Beatmap.language.LanguageContent[Language.areYouSureToContinue]);
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

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                label7.Text = Manage_Beatmap.language.LanguageContent[Language.copyLastTime];
                label7.Location = new Point(countOrLastTimeTextBox.Location.X - label7.Width - 2, countOrLastTimeTextBox.Location.Y + 3);
                comboBox.Enabled = false;
                comboBox.SelectedIndex = -1;
                checkBox1.Checked = true;
                checkBox1.Enabled = false;
            }
            else
            {
                label7.Text = Manage_Beatmap.language.LanguageContent[Language.countLabel];
                label7.Location = new Point(countOrLastTimeTextBox.Location.X - label7.Width - 2, countOrLastTimeTextBox.Location.Y + 3);
                comboBox.Enabled = true;
                checkBox1.Enabled = true;
            }
        }

        private void SV_Changer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (checkBox3.Checked)
            {
                Manage_Beatmap.savedContent = new SV_Changer_Content
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
            }
            else
                Manage_Beatmap.savedContent = null;
            if (!isButtonClicked)
                checkBox3.Checked = false;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                if (!isMessageShown && !checkBox2.Checked)
                {
                    ShowMode.Warning(Manage_Beatmap.language.LanguageContent[Language.rememberSnappingNotesBetweenArea]);
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
