using MessageBoxMode;
using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace BeatmapManager
{
    public partial class BPM_Changer : ActionableForm<BPM_Changer>
    {
        public static double value = 0;
        public static bool status = false;
        private bool formStatus = false; // Determines if close button is pressed or anything else like X button or ALT+F4.
        public static int Offset { get; set; }
        public static int ComboBoxSelectedIndex { get; internal set; } 

        public BPM_Changer(Action<BPM_Changer> action) : base(action)
        {
            InitializeComponent();
            ChangeControlTexts();
            ChangeLabelPositions();
            ComboBoxSelectedIndex = -1;
        }

        private void ChangeControlTexts()
        {
            Text = MainForm.language.LanguageContent[Language.BPMchangerFormTitle];
            label1.Text = MainForm.language.LanguageContent[Language.newBPMlabel];
            label2.Text = MainForm.language.LanguageContent[Language.changeType];
            button1.Text = MainForm.language.LanguageContent[Language.applyButton];
            checkBox1.Text = MainForm.language.LanguageContent[Language.adjustBookmarks];
        }

        private void ChangeLabelPositions()
        {
            label1.Location = new Point(textBox1.Location.X - label1.Size.Width - 2, label1.Location.Y);
            label2.Location = new Point(comboBox1.Location.X - label2.Size.Width - 2, label2.Location.Y);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Check();
        }

        private void Form6_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!formStatus)
                value = 0;
            formStatus = false;
        }

        private void Check()
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                ShowMode.Error(MainForm.language.LanguageContent[Language.oneOrMoreValuesEmpty]);
                return;
            }
            else if (textBox1.Text.Any(char.IsLetter))
            {
                ShowMode.Error(MainForm.language.LanguageContent[Language.onlyNumbersAndComma]);
                return;
            }
            else if (!comboBox1.IsDisposed && comboBox1.SelectedIndex == -1)
            {
                ShowMode.Error(MainForm.language.LanguageContent[Language.comboBoxSelectedIndex]);
                return;
            }
            DialogResult res = MessageBox.Show(MainForm.language.LanguageContent[Language.areYouSure], "Status", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (res == DialogResult.Yes)
            {
                value = Convert.ToDouble(textBox1.Text);
                formStatus = true;
                ComboBoxSelectedIndex = comboBox1.SelectedIndex;
                InvokeAction();
            }
            else if (res == DialogResult.Cancel)
            {
                value = 0;
            }
            else
            {
                value = 0;
                formStatus = true;
                InvokeAction();
            }
        }
    }
}
