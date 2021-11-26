﻿using System;
using System.Windows.Forms;
using MessageBoxMode;
using System.Threading;
using System.Globalization;
using System.Drawing;

namespace BeatmapManager
{
    public partial class SV_Equalizer : ActionableForm<SV_Equalizer>
    {
        public double Bpm_value { get; set; } = -1;
        public double SV_value { get; set; } = 1;
        public bool IsValueSet { get; set; } = false;
        public int editType { get; set; } = -1;
        public SV_Equalizer(Action<SV_Equalizer> action) : base(action)
        {
            InitializeComponent();
            ChangeControlTexts();
            ChangeLabelPositions();
        }

        private void ChangeControlTexts()
        {
            Text = MainForm.language.LanguageContent[Language.SVequalizerFormTitle];
            label1.Text = MainForm.language.LanguageContent[Language.enterBPMlabel];
            label2.Text = MainForm.language.LanguageContent[Language.enterSVlabel];
            label3.Text = MainForm.language.LanguageContent[Language.typeLabel];
            comboBox1.Items[0] = MainForm.language.LanguageContent[Language.addComboBox];
            comboBox1.Items[1] = MainForm.language.LanguageContent[Language.editComboBox];
            button1.Text = MainForm.language.LanguageContent[Language.applyButton];
        }
        private void ChangeLabelPositions()
        {
            label1.Location = new Point(textBox1.Location.X - label1.Size.Width - 2, label1.Location.Y);
            label2.Location = new Point(textBox2.Location.X - label2.Size.Width - 2, label2.Location.Y);
            label3.Location = new Point(comboBox1.Location.X - label3.Size.Width - 2, label3.Location.Y);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            decimal d = 0;
            decimal f = 0;
            if (!Decimal.TryParse(textBox1.Text, out d) && comboBox1.SelectedIndex == -1)
                ShowMode.Error(MainForm.language.LanguageContent[Language.BPMwrong]);
            else
            {
                if (textBox2.Text != "1" && textBox2.Text != "Default is 1.00x")
                {
                    if (!decimal.TryParse(textBox2.Text, out f))
                        ShowMode.Error(MainForm.language.LanguageContent[Language.SVvalueFormat]);
                    else if (comboBox1.SelectedIndex == 0)
                    {
                        if (ShowMode.QuestionWithYesNo(MainForm.language.LanguageContent[Language.removeAllSVchanges]) == DialogResult.Yes)
                        {
                            Bpm_value = (double)d;
                            if (f != 0) SV_value = (double)f;
                            IsValueSet = true;
                            editType = comboBox1.SelectedIndex;
                            InvokeAction();
                        }
                    }
                    else
                    {
                        if ((ShowMode.QuestionWithYesNo(MainForm.language.LanguageContent[Language.selectSVwithBPM]) == DialogResult.Yes))
                        {
                            Bpm_value = (double)d;
                            if (f != 0) SV_value = (double)f;
                            IsValueSet = true;
                            editType = comboBox1.SelectedIndex;
                            InvokeAction();
                        }
                    }
                }
                else if (comboBox1.SelectedIndex == 0)
                {
                    if (ShowMode.QuestionWithYesNo(MainForm.language.LanguageContent[Language.removeAllSVchanges]) == DialogResult.Yes)
                    {
                        Bpm_value = (double)d;
                        SV_value = 1;
                        IsValueSet = true;
                        editType = comboBox1.SelectedIndex;
                        InvokeAction();
                    }
                }
                else
                {
                    if ((ShowMode.QuestionWithYesNo(MainForm.language.LanguageContent[Language.selectSVwithBPM]) == DialogResult.Yes))
                    {
                        Bpm_value = (double)d;
                        SV_value = 1;
                        IsValueSet = true;
                        editType = comboBox1.SelectedIndex;
                        InvokeAction();
                    }
                }
            }
        }
        private void textBox2_Enter(object sender, EventArgs e)
        {
            textBox2.Text = "";
        }
    }
}
