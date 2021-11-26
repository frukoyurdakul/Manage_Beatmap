using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using WMPLib;

namespace BeatmapManager
{
    class Buttons
    {
        public List<Control> Controls { get; set; }
        public WindowsMediaPlayer wmp = new WindowsMediaPlayer();
        string path;
        Button button; ComboBox box; ComboBox box2; Label label; Label label2; Label label3; TextBox textBox;
        public Buttons(int count, string path)
        {
            this.path = path;
            button = new Button();
            box = new ComboBox();
            box2 = new ComboBox();
            label = new Label();
            label2 = new Label();
            label3 = new Label();
            textBox = new TextBox();
            wmp.URL = path;
            wmp.PlayStateChange += Wmp_PlayStateChange;

            //label

            label.AutoSize = true;
            label.Text = count.ToString() + ". " + MainForm.language.LanguageContent[Language.hitsoundLabel];
            label.Visible = true;

            //label2

            label2.AutoSize = true;
            label2.Text = MainForm.language.LanguageContent[Language.hitsoundTypeLabel];
            label2.Visible = true;

            //label3

            label3.AutoSize = true;
            label3.Text = MainForm.language.LanguageContent[Language.hitsoundSampleLabel];
            label3.Visible = true;

            //comboBox

            box.Size = new Size(121, 27);
            box.DropDownStyle = ComboBoxStyle.DropDownList;
            box.Items.Add("Normal");
            box.Items.Add("Clap");
            box.Items.Add("Whistle");
            box.Items.Add("Finisher");
            box.SelectedIndex = 0;
            box.Visible = true;

            //combobox2

            box2.Size = new Size(121, 27);
            box2.DropDownStyle = ComboBoxStyle.DropDownList;
            box2.Items.Add("Normal");
            box2.Items.Add("Soft");
            box2.Items.Add("Drum");
            box2.SelectedIndex = 0;
            box2.Visible = true;

            //button

            button.Size = new Size(80, 28);
            button.Text = "Play!";
            button.Visible = true;
            button.BackColor = SystemColors.ControlLightLight;
            button.FlatStyle = FlatStyle.Flat;
            button.Click += new EventHandler(button_Click);

            //add controls
            Controls = new List<Control>();
            Controls.Add(label);
            Controls.Add(label2);
            Controls.Add(box);
            Controls.Add(label3);
            Controls.Add(box2);
            Controls.Add(button);
            Controls.Add(textBox);
        }
        private void Wmp_PlayStateChange(int NewState)
        {
            if (wmp.playState == WMPPlayState.wmppsPlaying)
                button.Text = MainForm.language.LanguageContent[Language.stopHitsoundButton];
            else if (wmp.playState == WMPPlayState.wmppsStopped)
                button.Text = MainForm.language.LanguageContent[Language.playHitsoundButton];
        }
        void button_Click(object sender, EventArgs e)
        {
            if (wmp.playState == WMPPlayState.wmppsPlaying)
            {
                button.Text = MainForm.language.LanguageContent[Language.playHitsoundButton];
                wmp.controls.stop();
            }
            else if (wmp.playState == WMPPlayState.wmppsStopped)
            {
                button.Text = MainForm.language.LanguageContent[Language.stopHitsoundButton];
                wmp.controls.play();
            }
        }
        public void RemovePlayer()
        {
            wmp.controls.stop();
        }
    }
}
