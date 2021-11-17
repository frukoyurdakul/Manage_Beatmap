using System;
using System.Windows.Forms;
using MessageBoxMode;
using System.Threading;
using System.Globalization;
using System.Drawing;

namespace Manage_Beatmap
{
    public partial class SV_Equalizer : Form
    {
        public double Bpm_value { get; set; } = -1;
        public double SV_value { get; set; } = 1;
        public bool IsValueSet { get; set; } = false;
        public int editType { get; set; } = -1;
        public SV_Equalizer()
        {
            InitializeComponent();
            ChangeControlTexts();
            ChangeLabelPositions();
        }

        private void ChangeControlTexts()
        {
            Text = Manage_Beatmap.language.LanguageContent[Language.SVequalizerFormTitle];
            label1.Text = Manage_Beatmap.language.LanguageContent[Language.enterBPMlabel];
            label2.Text = Manage_Beatmap.language.LanguageContent[Language.enterSVlabel];
            label3.Text = Manage_Beatmap.language.LanguageContent[Language.typeLabel];
            comboBox1.Items[0] = Manage_Beatmap.language.LanguageContent[Language.addComboBox];
            comboBox1.Items[1] = Manage_Beatmap.language.LanguageContent[Language.editComboBox];
            button1.Text = Manage_Beatmap.language.LanguageContent[Language.applyButton];
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
                ShowMode.Error(Manage_Beatmap.language.LanguageContent[Language.BPMwrong]);
            else
            {
                if (textBox2.Text != "1" && textBox2.Text != "Default is 1.00x")
                {
                    if (!decimal.TryParse(textBox2.Text, out f))
                        ShowMode.Error(Manage_Beatmap.language.LanguageContent[Language.SVvalueFormat]);
                    else if (comboBox1.SelectedIndex == 0)
                    {
                        if (ShowMode.QuestionWithYesNo(Manage_Beatmap.language.LanguageContent[Language.removeAllSVchanges]) == DialogResult.Yes)
                        {
                            Bpm_value = (double)d;
                            if (f != 0) SV_value = (double)f;
                            IsValueSet = true;
                            editType = comboBox1.SelectedIndex;
                            this.Close();
                        }
                    }
                    else
                    {
                        if ((ShowMode.QuestionWithYesNo(Manage_Beatmap.language.LanguageContent[Language.selectSVwithBPM]) == DialogResult.Yes))
                        {
                            Bpm_value = (double)d;
                            if (f != 0) SV_value = (double)f;
                            IsValueSet = true;
                            editType = comboBox1.SelectedIndex;
                            this.Close();
                        }
                    }
                }
                else if (comboBox1.SelectedIndex == 0)
                {
                    if (ShowMode.QuestionWithYesNo(Manage_Beatmap.language.LanguageContent[Language.removeAllSVchanges]) == DialogResult.Yes)
                    {
                        Bpm_value = (double)d;
                        SV_value = 1;
                        IsValueSet = true;
                        editType = comboBox1.SelectedIndex;
                        this.Close();
                    }
                }
                else
                {
                    if ((ShowMode.QuestionWithYesNo(Manage_Beatmap.language.LanguageContent[Language.selectSVwithBPM]) == DialogResult.Yes))
                    {
                        Bpm_value = (double)d;
                        SV_value = 1;
                        IsValueSet = true;
                        editType = comboBox1.SelectedIndex;
                        this.Close();
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
