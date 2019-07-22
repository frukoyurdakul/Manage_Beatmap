using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using MessageBoxMode;
using FindIndex;
using System.Text.RegularExpressions;
using System.Drawing;
using System.ComponentModel;
using System.Diagnostics;

namespace Manage_Beatmap
{
    public partial class Hitsounds : Form
    {
        List<string> paths = new List<string>();
        List<string> taikoHitsounds = new List<string>();
        List<string> stdOrCtbHitsounds = new List<string>();
        List<string> notes = new List<string>();
        List<Buttons> buttons = new List<Buttons>();
        int[,] hitSoundCounts = new int[3, 4];
        string beatmapPath = string.Empty;
        int currentButtonCount = 0;
        Timer timer;
        public Hitsounds(string path, Timer timer)
        {
            InitializeComponent();
            ChangeControlTexts();
            ChangeLabelPositions();
            comboBox1.SelectedIndex = 0;
            for (int i = 0; i < 4; i++) for (int j = 0; j < 3; j++) hitSoundCounts[j, i] = 1;
            beatmapPath = path;
            this.timer = timer;
        }
        private void ChangeControlTexts()
        {
            Text = Manage_Beatmap.language.LanguageContent[Language.HitsoundsFormTitle];
            label1.Text = Manage_Beatmap.language.LanguageContent[Language.hitsoundsModeLabel];
            button2.Text = Manage_Beatmap.language.LanguageContent[Language.clearHitsoundsButton];
            label2.Text = Manage_Beatmap.language.LanguageContent[Language.hitsoundsExplanationLabel];
            button1.Text = Manage_Beatmap.language.LanguageContent[Language.saveHitsoundsButton];
        }
        private void ChangeLabelPositions()
        {
            label1.Location = new Point(comboBox1.Location.X - label1.Size.Width - 2, label1.Location.Y);
        }
        private void panel2_DragDrop(object sender, DragEventArgs e)
        {
            bool isShown = false;
            if (!label2.IsDisposed) label2.Dispose();
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                paths.AddRange((string[])e.Data.GetData(DataFormats.FileDrop));
            for (int i = 0; i < paths.Count; i++)
            {
                if (!(paths[i].ToLower().Contains(".wav")))
                {
                    if (!isShown)
                    {
                        ShowMode.Warning(Manage_Beatmap.language.LanguageContent[Language.onlyWavAllowed]);
                        isShown = true;
                    }
                    paths.Remove(paths[i--]);
                }
            }
            for (int i = currentButtonCount; i < paths.Count; i++)
            {
                buttons.Add(new Buttons(i + 1, paths[i]));
                buttons[i].Controls.Add(addDeleteButton());
                panel2.Controls.AddRange(buttons[i].Controls.ToArray());
                panel2.SetFlowBreak(buttons[i].Controls[buttons[i].Controls.Count - 1], true);
                currentButtonCount++;
            }
        }
        private void panel2_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }
        private void Hitsounds_FormClosing(object sender, FormClosingEventArgs e)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].RemovePlayer();
            }
            buttons.Clear();
            timer.Start();
        }
        private Control addDeleteButton()
        {
            Button button = new Button();

            //button specifications
            button.Size = new Size(80, 28);
            button.Text = Manage_Beatmap.language.LanguageContent[Language.delete];
            button.Visible = true;
            button.BackColor = SystemColors.ControlLightLight;
            button.FlatStyle = FlatStyle.Flat;
            button.Click += new EventHandler(deleteButton_Click);

            return button;
        }
        private void deleteButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int index = panel2.Controls.IndexOf(button), buttonRowCount = 8, deleteValueCounter = 0;
            if (index != -1)
            {
                for (int i = index; deleteValueCounter < buttonRowCount; i--)
                {
                    panel2.Controls.RemoveAt(i);
                    deleteValueCounter++;
                }
                buttons.RemoveAt(index / buttonRowCount);
                paths.RemoveAt(index / buttonRowCount);
                currentButtonCount--;
            }
            else
                ShowMode.Error(Manage_Beatmap.language.LanguageContent[Language.buttonNotFound]);
        }
        private void SaveHitSounds(string path)
        {
            bool isChecked = false;
            for (int i = 0; i < buttons.Count; i++)
            {
                string saveLine = string.Empty;
                if (comboBox1.SelectedIndex == 0)
                {
                    string sample = panel2.Controls[4 + i * 8].Text;
                    string hitSoundType = panel2.Controls[2 + i * 8].Text;
                    string noteLine = panel2.Controls[6 + i * 8].Text.TrimStart().TrimEnd();
                    string extension = paths[i].Substring(paths[i].Length - 4);
                    int box1SelectedIndex = -1, box2SelectedIndex = -1;
                    if (!isChecked)
                    {
                        var currentHitsoundsVar = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly).
                            Where(s => s.EndsWith(".wav"));
                        string[] currentHitsounds = currentHitsoundsVar.ToArray();
                        if (currentHitsounds.Length != 0)
                        {
                            for (int j = 0; j < currentHitsounds.Length; j++)
                            {
                                if (!currentHitsounds[j].Contains("taiko"))
                                {
                                    stdOrCtbHitsounds.Add(currentHitsounds[j]);
                                }
                            }
                        }
                        if (stdOrCtbHitsounds.Count != 0)
                        {
                            for (int j = 0; j < stdOrCtbHitsounds.Count; j++)
                            {
                                string safeFileName = stdOrCtbHitsounds[j].Substring(stdOrCtbHitsounds[j].LastIndexOf('\\') + 1);
                                string firstPart = safeFileName.Substring(0, safeFileName.IndexOf('-'));
                                string secondPart = safeFileName.Substring(safeFileName.IndexOf('-') + 1, safeFileName.IndexOf('.') - safeFileName.IndexOf('-'));
                                int number;
                                if (secondPart.Any(char.IsDigit))
                                    number = int.Parse(Regex.Match(secondPart, @"\d+").Value);
                                else
                                    number = 1;
                                if (firstPart == "normal")
                                    box1SelectedIndex = 0;
                                else if (firstPart == "soft")
                                    box1SelectedIndex = 1;
                                else if (firstPart == "drum")
                                    box1SelectedIndex = 2;

                                if (secondPart.Contains("hitnormal"))
                                    box2SelectedIndex = 0;
                                else if (secondPart.Contains("hitclap"))
                                    box2SelectedIndex = 1;
                                else if (secondPart.Contains("hitwhistle"))
                                    box2SelectedIndex = 2;
                                else if (secondPart.Contains("hitfinisher"))
                                    box2SelectedIndex = 3;
                                if (hitSoundCounts[box1SelectedIndex, box2SelectedIndex] < number) hitSoundCounts[box1SelectedIndex, box2SelectedIndex] = number;
                            }
                        }
                        box1SelectedIndex = -1;
                        box2SelectedIndex = -1;
                        isChecked = true;
                    }
                    if (sample == "Normal")
                        box1SelectedIndex = 0;
                    else if (sample == "Soft")
                        box1SelectedIndex = 1;
                    else if (sample == "Drum")
                        box1SelectedIndex = 2;

                    if (hitSoundType == "Normal")
                        box2SelectedIndex = 0;
                    else if (hitSoundType == "Clap")
                        box2SelectedIndex = 1;
                    else if (hitSoundType == "Whistle")
                        box2SelectedIndex = 2;
                    else if (hitSoundType == "Finisher")
                        box2SelectedIndex = 3;
                    saveLine = sample.ToLower() + "-hit" + hitSoundType.ToLower();
                    if (hitSoundCounts[box1SelectedIndex, box2SelectedIndex] != 1)
                        saveLine += hitSoundCounts[box1SelectedIndex, box2SelectedIndex].ToString();
                    saveLine += extension;
                    if (!string.IsNullOrWhiteSpace(noteLine))
                        notes.Add(saveLine + ": " + noteLine);
                    File.Copy(paths[i], path + "\\" + saveLine, true);
                    hitSoundCounts[box1SelectedIndex, box2SelectedIndex]++;
                }
                else if (comboBox1.SelectedIndex == 1)
                {
                    string sample = panel2.Controls[4 + i * 8].Text;
                    string hitSoundType = panel2.Controls[2 + i * 8].Text;
                    string noteLine = panel2.Controls[6 + i * 8].Text.TrimStart().TrimEnd();
                    string extension = paths[i].Substring(paths[i].Length - 4);
                    int box1SelectedIndex = -1, box2SelectedIndex = -1;
                    if (!isChecked)
                    {
                        var currentHitsoundsVar = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).
                            Where(s => s.EndsWith(".ogg") ||
                                  s.EndsWith(".wav"));
                        string[] currentHitsounds = currentHitsoundsVar.ToArray();
                        if (currentHitsounds.Length != 0)
                        {
                            for (int j = 0; j < currentHitsounds.Length; j++)
                            {
                                if (currentHitsounds[j].Contains("taiko"))
                                {
                                    taikoHitsounds.Add(currentHitsounds[j]);
                                }
                            }
                        }
                        if (taikoHitsounds.Count != 0)
                        {
                            for (int j = 0; j < taikoHitsounds.Count; j++)
                            {
                                string safeFileName = taikoHitsounds[j].Substring(taikoHitsounds[j].LastIndexOf('\\') + 1);
                                string firstPart = safeFileName.Substring(safeFileName.IndexOf('-') + 1, safeFileName.IndexOfWithCount('-', 2) - safeFileName.IndexOf('-') - 2);
                                string secondPart = safeFileName.Substring(safeFileName.IndexOfWithCount('-',2), safeFileName.IndexOf('.') - safeFileName.IndexOfWithCount('-', 2));
                                int number;
                                if (secondPart.Any(char.IsDigit))
                                    number = Int32.Parse(Regex.Match(secondPart, @"\d+").Value);
                                else
                                    number = 1;
                                if (firstPart == "normal")
                                    box1SelectedIndex = 0;
                                else if (firstPart == "soft")
                                    box1SelectedIndex = 1;
                                else if (firstPart == "drum")
                                    box1SelectedIndex = 2;

                                if (secondPart.Contains("hitnormal"))
                                    box2SelectedIndex = 0;
                                else if (secondPart.Contains("hitclap"))
                                    box2SelectedIndex = 1;
                                else if (secondPart.Contains("hitwhistle"))
                                    box2SelectedIndex = 2;
                                else if (secondPart.Contains("hitfinisher"))
                                    box2SelectedIndex = 3;
                                if (hitSoundCounts[box1SelectedIndex, box2SelectedIndex] < number) hitSoundCounts[box1SelectedIndex, box2SelectedIndex] = number + 1;
                            }
                        }
                        box1SelectedIndex = -1;
                        box2SelectedIndex = -1;
                        isChecked = true;
                    }
                    if (sample == "Normal")
                        box1SelectedIndex = 0;
                    else if (sample == "Soft")
                        box1SelectedIndex = 1;
                    else if (sample == "Drum")
                        box1SelectedIndex = 2;

                    if (hitSoundType == "Normal")
                        box2SelectedIndex = 0;
                    else if (hitSoundType == "Clap")
                        box2SelectedIndex = 1;
                    else if (hitSoundType == "Whistle")
                        box2SelectedIndex = 2;
                    else if (hitSoundType == "Finisher")
                        box2SelectedIndex = 3;
                    saveLine = "taiko-" + sample.ToLower() + "-hit" + hitSoundType.ToLower();
                    if (hitSoundCounts[box1SelectedIndex, box2SelectedIndex] != 1)
                        saveLine += hitSoundCounts[box1SelectedIndex, box2SelectedIndex].ToString();
                    saveLine += extension;
                    if (!string.IsNullOrWhiteSpace(noteLine))
                        notes.Add(saveLine + ": " + noteLine);
                    try
                    {
                        File.Copy(paths[i], path + "\\" + saveLine, true);
                    }
                    catch (FileNotFoundException e)
                    {
                        ShowMode.Error(e.Message);
                    }
                    hitSoundCounts[box1SelectedIndex, box2SelectedIndex]++;
                }
            }
            ShowMode.Information(Manage_Beatmap.language.LanguageContent[Language.hitsoundsSaved]);
            if(notes.Count > 0)
            {
                File.WriteAllLines(path + "\\notes.txt", notes.ToArray());
                if (ShowMode.QuestionWithYesNo(Manage_Beatmap.language.LanguageContent[Language.notesSaved]) == DialogResult.Yes)
                    Process.Start(path + "\\notes.txt");
            }
            for (int i = 0; i < 4; i++) for (int j = 0; j < 3; j++) hitSoundCounts[j, i] = 1;
            stdOrCtbHitsounds.Clear();
            taikoHitsounds.Clear();
            notes.Clear();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (panel2.Controls.Count < 5)
            {
                ShowMode.Error(Manage_Beatmap.language.LanguageContent[Language.noHitsounds]);
                return;
            }
            if (ShowMode.QuestionWithYesNo(Manage_Beatmap.language.LanguageContent[Language.areYouSureToSaveHitsounds]) == DialogResult.Yes)
            {
                if(beatmapPath != string.Empty)
                {
                    DialogResult res = ShowMode.QuestionWithYesNoCancel(Manage_Beatmap.language.LanguageContent[Language.saveSongFolder]);
                    if (res == DialogResult.Yes)
                        SaveHitSounds(beatmapPath);
                    else if (res == DialogResult.No)
                    {
                        ShowMode.Information(Manage_Beatmap.language.LanguageContent[Language.selectTheFolder]);
                        FolderBrowserDialog dialog = new FolderBrowserDialog();
                        if (dialog.ShowDialog() == DialogResult.OK)
                            SaveHitSounds(dialog.SelectedPath);
                    }
                }
                else
                {
                    ShowMode.Information(Manage_Beatmap.language.LanguageContent[Language.selectTheFolder]);
                    FolderBrowserDialog dialog = new FolderBrowserDialog();
                    if (dialog.ShowDialog() == DialogResult.OK)
                        SaveHitSounds(dialog.SelectedPath);
                }
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (ShowMode.QuestionWithYesNo(Manage_Beatmap.language.LanguageContent[Language.areYouSure]) == DialogResult.Yes)
            {
                for (int i = 0; i < buttons.Count; i++) buttons[i].RemovePlayer();
                buttons.Clear();
                taikoHitsounds.Clear();
                stdOrCtbHitsounds.Clear();
                panel2.Controls.Clear();
                paths.Clear();
                notes.Clear();
                currentButtonCount = 0;
            }
        }

        private void Hitsounds_Load(object sender, EventArgs e)
        {
            Location = Owner.Location;
            Left += Owner.ClientSize.Width / 2 - Width / 2;
            Top += Owner.ClientSize.Height / 2 - Height / 2;
        }

        private void Hitsounds_FormClosed(object sender, FormClosedEventArgs e)
        {
            Owner.Show();
        }
    }
}