using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace BeatmapManager
{
    public partial class Options : Form
    {
        public bool IsLanguageChanged { get; set; } = false;

        public Options()
        {
            InitializeComponent();
            ChangeControlTexts();
            ChangeLabelPositions();
        }

        private void ChangeControlTexts()
        {
            Text = MainForm.language.LanguageContent[Language.optionsFormTitle];
            label2.Text = MainForm.language.LanguageContent[Language.refresh] + ": F5";
            label3.Text = MainForm.language.LanguageContent[Language.undo] + ": Ctrl + Z";
            label4.Text = MainForm.language.LanguageContent[Language.redo] + ": Ctrl + Y";
        }

        private void ChangeLabelPositions()
        {
            
        }
    }
}
