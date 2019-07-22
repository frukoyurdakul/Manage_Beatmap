using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Manage_Beatmap
{
    public partial class Options : Form
    {
        public bool IsLanguageChanged { get; set; } = false;
        private int comboBoxInitialIndex;
        public Options()
        {
            InitializeComponent();
            AddCurrentLanguages();
            ChangeControlTexts();
            ChangeLabelPositions();
            comboBox1.SelectedIndex = comboBox1.Items.IndexOf(Manage_Beatmap.language.SelectedLanguage);
            comboBoxInitialIndex = comboBox1.SelectedIndex;
        }
        private void AddCurrentLanguages()
        {
            if(Directory.Exists("Languages"))
            {
                var currentLanguagesVar = Directory.GetFiles("Languages", "*.*", SearchOption.AllDirectories).
                            Where(s => s.EndsWith(".txt"));
                string[] currentLanguages = currentLanguagesVar.ToArray();
                for (int i = 0; i < currentLanguages.Length; i++)
                    comboBox1.Items.Add(currentLanguages[i].Substring(currentLanguages[i].IndexOf('\\') + 1, currentLanguages[i].IndexOf('.') - currentLanguages[i].IndexOf('\\') - 1));
            }
        }
        private void ChangeControlTexts()
        {
            Text = Manage_Beatmap.language.LanguageContent[Language.optionsFormTitle];
            label1.Text = Manage_Beatmap.language.LanguageContent[Language.languageLabel];
            if(comboBox1.Items.Count > 0)
                comboBox1.Items[0] = Manage_Beatmap.language.LanguageContent[Language.firstLanguage];
            if(comboBox1.Items.Count > 1)
                comboBox1.Items[1] = Manage_Beatmap.language.LanguageContent[Language.secondLanguage];
            if (comboBox1.Items.Count > 2)
                comboBox1.Items[2] = Manage_Beatmap.language.LanguageContent[Language.thirdLanguage];
            label2.Text = Manage_Beatmap.language.LanguageContent[Language.refresh] + ": F5";
            label3.Text = Manage_Beatmap.language.LanguageContent[Language.undo] + ": Ctrl + Z";
            label4.Text = Manage_Beatmap.language.LanguageContent[Language.redo] + ": Ctrl + Y";
        }
        private void ChangeLabelPositions()
        {
            label1.Location = new System.Drawing.Point(comboBox1.Location.X - label1.Size.Width - 2, label1.Location.Y);
        }
        private void comboBox1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
                Manage_Beatmap.language = new Language();
            else if (comboBox1.SelectedIndex == 1)
                Manage_Beatmap.language = new Language("Turkish");
            else if (comboBox1.SelectedIndex == 2)
                Manage_Beatmap.language = new Language("French");
            ChangeControlTexts();
            ChangeLabelPositions();
            IsLanguageChanged = comboBox1.SelectedIndex != comboBoxInitialIndex;
        }
    }
}
