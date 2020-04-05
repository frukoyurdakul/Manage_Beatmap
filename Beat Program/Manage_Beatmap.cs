using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using FindIndex;
using MessageBoxMode;
using System.Threading;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;

namespace Manage_Beatmap
{
    public partial class Manage_Beatmap : Form
    {
        string path = string.Empty;
        string fileName = string.Empty;
        string lastUpdate = string.Empty;
        string[] lines, activeComboBoxItems, notActiveComboBoxItems;
        List<string> linesList = new List<string>();
        public static Language language;
        public static bool isHitsoundsOpen = false;
        List<string[]> undo = new List<string[]>();
        List<string[]> redo = new List<string[]>();
        DataTable table = new DataTable();
        public static SV_Changer_Content savedContent = null;
        int timingPointsIndex = -1;
        int offsetChange = 0;
        int nextTimingPointOffset = 0;
        bool isDefined = false;
        bool isHaveNotes = false;
        bool isActivated = false;
        bool isBackup = false;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                return cp;
            }
        }

        public Manage_Beatmap()
        {
            InitializeComponent();
            SetLanguage();
            ChangeControlTexts();
        }
        #region WinApi
        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(int vKey);
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        private string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();
            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }
        #endregion
        #region Base Functions
        private void manageLoad()
        {
            Text = language.LanguageContent[Language.manageBeatmapFormTitle] + " {" + fileName + "}";
            if (string.IsNullOrEmpty(path))
            {
                disableButtons();
                table.Columns.Clear();
                table.Clear();
                return;
            }
            else
                enableButtons();
            if (!isBackup)
            {
                lines = File.ReadAllLines(path);
                label2.Text = language.LanguageContent[Language.manageBeatmapTopLabel] + DateTime.Now.ToString(@"dd\/MM\/yyyy h\:mm\:ss");
            }
            if (table.Columns.Count == 0)
            {
                table.Columns.Add("Offset");
                table.Columns.Add("BPM");
                table.Columns.Add("Meter");
                table.Columns.Add("Volume");
                table.Columns.Add("Kiai");
            }
            prepareGridView();
            isBackup = false;
            dataGridView1.Focus();
            dataGridView1.ClearSelection();
        }
        private void prepareGridView()
        {
            List<int> offsetErrorIndexes = new List<int>();
            int colIndex = 0, dataGridViewRowIndex = 0;
            table.Clear();
            BPM_Changer.value = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] == "[TimingPoints]")
                {
                    timingPointsIndex = i;
                    break;
                }
            }
            if (timingPointsIndex == -1)
            {
                ShowMode.Error(language.LanguageContent[Language.noTimingPoints]);
                return;
            }
            for (int i = timingPointsIndex + 1; i < lines.Length - 1 && !string.IsNullOrWhiteSpace(lines[i]); i++)
            {
                DataRow dr = table.NewRow();
                string temp = lines[i];
                double offset = Convert.ToDouble(temp.Substring(0, temp.IndexOf(',')).Replace('.',','));
                if (offset != (int)offset)
                    offsetErrorIndexes.Add(dataGridViewRowIndex);
                dr[colIndex++] = offset.ToString() + "  (" + FormatString.getFormattedTimeString((int)offset) + ")";
                double bpmOrSV = Convert.ToDouble(temp.Substring(temp.IndexOfWithCount(',', 1), temp.IndexOfWithCount(',', 2) - temp.IndexOfWithCount(',', 1) - 1).Replace('.', ','));
                if (bpmOrSV < 0)
                    dr[colIndex++] = string.Format("{0:0.0000}", (-100) / bpmOrSV) + "x".Replace('.', ',');
                else
                    dr[colIndex++] = (60000 / bpmOrSV).ToString();
                if(temp.Substring(temp.IndexOfWithCount(',', 6), 1) == "1")
                    dr[colIndex++] = temp.Substring(temp.IndexOfWithCount(',', 2), temp.IndexOfWithCount(',', 3) - temp.IndexOfWithCount(',', 2) - 1) + "/4";
                else
                    dr[colIndex++] = "-";
                dr[colIndex++] = temp.Substring(temp.IndexOfWithCount(',', 5), temp.IndexOfWithCount(',', 6) - temp.IndexOfWithCount(',', 5) - 1) + "%";
                if (temp.Substring(temp.IndexOfWithCount(',', 7), 1) == "1")
                    dr[colIndex++] = "*";
                else
                    dr[colIndex++] = string.Empty;
                for (int t = 0; t < table.Rows.Count; t++)
                {
                    if (string.IsNullOrWhiteSpace(table.Rows[t][1].ToString())) table.Rows[t].Delete();
                }
                table.Rows.Add(dr);
                colIndex = 0;
                dataGridViewRowIndex++;
            }
            dataGridView1.DataSource = table;
            for (int i = 0; i < dataGridView1.Columns.Count; i++)
                dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns[4].DefaultCellStyle.Font = new Font("Times New Roman", 18F, FontStyle.Bold);
            for (int i = 0; i < offsetErrorIndexes.Count; i++)
            {
                dataGridView1.Rows[offsetErrorIndexes[i]].DefaultCellStyle.BackColor = Color.Red;
                for(int j = 0; j < dataGridView1.Rows[offsetErrorIndexes[i]].Cells.Count; j++)
                    dataGridView1.Rows[offsetErrorIndexes[i]].Cells[j].Style.ForeColor = Color.White;
            }
            if (offsetErrorIndexes.Count != 0)
            {
                ShowMode.Error(language.LanguageContent[Language.decimalOffsets]);
                dataGridView1.FirstDisplayedScrollingRowIndex = offsetErrorIndexes[0];
                disableButtons();
            }
            lastUpdate = DateTime.Now.ToString(@"dd\/MM\/yyyy h\:mm\:ss");
            label2.Text = language.LanguageContent[Language.manageBeatmapTopLabel] + lastUpdate;
            dataGridView1.ClearSelection();
        }
        private void disableButtons()
        {
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(notActiveComboBoxItems);
            button19.Enabled = false;
            button21.Enabled = false;
            button22.Enabled = false;
            button23.Enabled = false;
        }
        private void enableButtons()
        {
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(activeComboBoxItems);
            button19.Enabled = true;
            button21.Enabled = true;
            button22.Enabled = true;
            button23.Enabled = true;
        }
        private void AddBackup()
        {
            if (undo.Count == 20)
                undo.RemoveAt(0);
            undo.Add(lines);
            redo.Clear();
        }
        private void Undo()
        {
            if (undo.Count > 0)
            {
                redo.Add(lines);
                lines = undo[undo.Count - 1];
                undo.RemoveAt(undo.Count - 1);
                isBackup = true;
                manageLoad();
            }
        }
        private void Redo()
        {
            if (redo.Count > 0)
            {
                undo.Add(lines);
                lines = redo[redo.Count - 1];
                redo.RemoveAt(redo.Count - 1);
                isBackup = true;
                manageLoad();
            }
        }
        private void SaveFile()
        {
            if(ShowMode.QuestionWithYesNo(language.LanguageContent[Language.areYouSure]) == DialogResult.Yes)
            {
                File.WriteAllLines(path, lines);
                manageLoad();
            }
        }
        private void SetLanguage()
        {
            if (!File.Exists("config.cfg"))
            {
                File.WriteAllText("config.cfg", "Language=English");
                ShowMode.Information("Config.cfg file was missing. The default language is set to English.");
                language = new Language();
            }
            else
            {
                string[] config = File.ReadAllLines("config.cfg");
                if (config.Length == 0)
                {
                    ShowMode.Warning("Config.cfg file is empty. The default language is set to English.");
                    language = new Language();
                }
                else if (!config[0].Contains("Language=") || config[0].Length < 10)
                {
                    ShowMode.Warning("Config.cfg file is corrupted. The default language is set to English.");
                    language = new Language();
                }
                else if (config[0].Substring(config[0].IndexOf('=') + 1).Equals("English"))
                    language = new Language();
                else
                    language = new Language(config[0].Substring(config[0].IndexOf("=") + 1));
            }
        }
        private void SetComboBoxItems()
        {
            activeComboBoxItems = new string[19];
            notActiveComboBoxItems = new string[2];
            activeComboBoxItems[0] = language.LanguageContent[Language.selectFileButton];
            activeComboBoxItems[1] = language.LanguageContent[Language.addHitsoundsToFolderButton];
            activeComboBoxItems[2] = language.LanguageContent[Language.prepareMapToHitsoundingButton];
            activeComboBoxItems[3] = language.LanguageContent[Language.setAllWhistleToClapsButton];
            activeComboBoxItems[4] = language.LanguageContent[Language.positionNotesButton];
            activeComboBoxItems[5] = language.LanguageContent[Language.newTimingButton];
            activeComboBoxItems[6] = language.LanguageContent[Language.changeBPMofSelectedPointButton];
            activeComboBoxItems[7] = language.LanguageContent[Language.changeOffsetsOfSelectedPointsButton];
            activeComboBoxItems[8] = language.LanguageContent[Language.addInheritedPointsToChangeSVsmoothlyButton];
            activeComboBoxItems[9] = language.LanguageContent[Language.equalizeSVforAllTimingPointsButton];
            activeComboBoxItems[10] = language.LanguageContent[Language.increaseOrDecreaseSVsButton];
            activeComboBoxItems[11] = language.LanguageContent[Language.increaseSVstepByStepButton];
            activeComboBoxItems[12] = language.LanguageContent[Language.changeVolumesButton];
            activeComboBoxItems[13] = language.LanguageContent[Language.changeVolumesStepByStepButton];
            activeComboBoxItems[14] = language.LanguageContent[Language.deleteSelectedInheritedPointsButton];
            activeComboBoxItems[15] = language.LanguageContent[Language.deleteAllInheritedPointsButton];
            activeComboBoxItems[16] = language.LanguageContent[Language.deleteDuplicatePointsButton];
            activeComboBoxItems[17] = language.LanguageContent[Language.deleteUnneccessaryInheritedPointsButton];
            activeComboBoxItems[18] = language.LanguageContent[Language.copyTagsButton];
            notActiveComboBoxItems[0] = language.LanguageContent[Language.selectFileButton];
            notActiveComboBoxItems[1] = language.LanguageContent[Language.addHitsoundsToFolderButton];
        }
        private void ChangeControlTexts()
        {
            SetComboBoxItems();
            Text = language.LanguageContent[Language.manageBeatmapFormTitle] + " {" + fileName + "}";
            label2.Text = language.LanguageContent[Language.manageBeatmapTopLabel] + lastUpdate;
            label1.Text = language.LanguageContent[Language.manageBeatmapKeysLabel];
            if (comboBox1.Items.Count != 2)
            {
                comboBox1.Items[0] = language.LanguageContent[Language.selectFileButton];
                comboBox1.Items[1] = language.LanguageContent[Language.addHitsoundsToFolderButton];
                comboBox1.Items[2] = language.LanguageContent[Language.prepareMapToHitsoundingButton];
                comboBox1.Items[3] = language.LanguageContent[Language.setAllWhistleToClapsButton];
                comboBox1.Items[4] = language.LanguageContent[Language.positionNotesButton];
                comboBox1.Items[5] = language.LanguageContent[Language.newTimingButton];
                comboBox1.Items[6] = language.LanguageContent[Language.changeBPMofSelectedPointButton];
                comboBox1.Items[7] = language.LanguageContent[Language.changeOffsetsOfSelectedPointsButton];
                comboBox1.Items[8] = language.LanguageContent[Language.addInheritedPointsToChangeSVsmoothlyButton];
                comboBox1.Items[9] = language.LanguageContent[Language.equalizeSVforAllTimingPointsButton];
                comboBox1.Items[10] = language.LanguageContent[Language.increaseOrDecreaseSVsButton];
                comboBox1.Items[11] = language.LanguageContent[Language.increaseSVstepByStepButton];
                comboBox1.Items[12] = language.LanguageContent[Language.changeVolumesButton];
                comboBox1.Items[13] = language.LanguageContent[Language.changeVolumesStepByStepButton];
                comboBox1.Items[14] = language.LanguageContent[Language.deleteSelectedInheritedPointsButton];
                comboBox1.Items[15] = language.LanguageContent[Language.deleteAllInheritedPointsButton];
                comboBox1.Items[16] = language.LanguageContent[Language.deleteDuplicatePointsButton];
                comboBox1.Items[17] = language.LanguageContent[Language.deleteUnneccessaryInheritedPointsButton];
                comboBox1.Items[18] = language.LanguageContent[Language.copyTagsButton];
            }
            else
            {
                comboBox1.Items[0] = language.LanguageContent[Language.selectFileButton];
                comboBox1.Items[1] = language.LanguageContent[Language.addHitsoundsToFolderButton];
            }
            button16.Text = language.LanguageContent[Language.optionsFormTitle];
            button17.Text = language.LanguageContent[Language.undo];
            button18.Text = language.LanguageContent[Language.redo];
            button19.Text = language.LanguageContent[Language.save];
            applyFunctionButton.Text = language.LanguageContent[Language.applyFunctionButton];
        }
        private void RemoveInvisibleSelection()
        {
            if (dataGridView1.SelectedRows.Count == dataGridView1.Rows.Count)
            {
                for (int i = 0; i < dataGridView1.SelectedRows.Count; i++)
                    if (!dataGridView1.SelectedRows[i].Visible)
                        dataGridView1.SelectedRows[i--].Selected = false;
            }
        }
        private void Opening()
        {
            ShowMode.Information(language.LanguageContent[Language.osuManage]);
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = language.LanguageContent[Language.osuFiles] + " (*.osu,*.OSU) | *.osu;*.OSU";
            if (file.ShowDialog() == DialogResult.OK)
            {
                path = file.FileName;
                fileName = file.SafeFileName;
                manageLoad();
                timer1.Start();
            }
        }
        private void SetTimingOffsetsAndNewBpm()
        {
            double actualBpm = Convert.ToDouble(dataGridView1.SelectedRows[0].Cells[1].Value);
            double enteredBpm = BPM_Changer.value;
            double calculatedGridSnap = 0; // Will use it for checking the grid snap, multiplicated with resolution.
            string startingOffsetString = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();
            int startingOffset = Convert.ToInt32(startingOffsetString.Substring(0, startingOffsetString.IndexOf(' '))), newOffset;
            double resolution = 48; // Need an ekoka value for 1/16 and 1/12 which is 1/48.
            for (int i = 0; i < linesList.Count; i++)
            {
                if (linesList[i] == "[TimingPoints]")
                {
                    for (int j = i + 1; !string.IsNullOrWhiteSpace(linesList[j]); j++)
                    {
                        string selectedLine = linesList[j];
                        int selectedOffset = Convert.ToInt32(linesList[j].Substring(0, linesList[j].IndexOf(',')));
                        if (selectedOffset == startingOffset && selectedLine.Substring(selectedLine.IndexOfWithCount(',', 6), 1) == "1")
                        {
                            double bpmInTermsOfTime = 60000 / enteredBpm;
                            string bpmInTermsOfTimeString = bpmInTermsOfTime.ToString().Replace(',', '.');
                            selectedLine = selectedLine.Remove(selectedLine.IndexOf(',') + 1, selectedLine.IndexOfWithCount(',', 2) - selectedLine.IndexOf(',') - 2);
                            selectedLine = selectedLine.Insert(selectedLine.IndexOf(',') + 1, bpmInTermsOfTimeString);
                            linesList[j] = selectedLine;
                        }
                        else if (selectedOffset > startingOffset)
                        {
                            calculatedGridSnap = Convert.ToInt32((((selectedOffset - startingOffset) / (60000 / actualBpm)) * resolution));
                            newOffset = (int)(startingOffset + ((60000 / (enteredBpm * resolution)) * calculatedGridSnap));
                            if (selectedLine.Substring(selectedLine.IndexOfWithCount(',', 6), 1) == "1")
                            {
                                if (!isDefined)
                                {
                                    nextTimingPointOffset = selectedOffset;
                                    offsetChange = newOffset - selectedOffset;
                                    isDefined = true;
                                }
                            }
                            selectedLine = selectedLine.Remove(0, selectedLine.IndexOf(','));
                            if (offsetChange != 0)
                                selectedLine = selectedLine.Insert(0, (selectedOffset + offsetChange).ToString());
                            else
                                selectedLine = selectedLine.Insert(0, newOffset.ToString());
                            linesList[j] = selectedLine;
                        }
                    }
                }
            }
        }
        private string[] SetTimingOffsetsAndNewBpm(string[] lines)
        {
            double actualBpm = Convert.ToDouble(dataGridView1.SelectedRows[0].Cells[1].Value);
            double enteredBpm = BPM_Changer.value;
            double calculatedGridSnap = 0; // Will use it for checking the grid snap, multiplicated with resolution.
            string startingOffsetString = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();
            int startingOffset = Convert.ToInt32(startingOffsetString.Substring(0, startingOffsetString.IndexOf(' '))), newOffset;
            double resolution = 48; // Need an ekoka value for 1/16 and 1/12 which is 1/48.
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] == "[TimingPoints]")
                {
                    for (int j = i + 1; !string.IsNullOrWhiteSpace(lines[j]); j++)
                    {
                        string selectedLine = lines[j];
                        int selectedOffset = Convert.ToInt32(lines[j].Substring(0, lines[j].IndexOf(',')));
                        if (selectedOffset == startingOffset && selectedLine.Substring(selectedLine.IndexOfWithCount(',', 6), 1) == "1")
                        {
                            double bpmInTermsOfTime = 60000 / enteredBpm;
                            string bpmInTermsOfTimeString = bpmInTermsOfTime.ToString().Replace(',', '.');
                            selectedLine = selectedLine.Remove(selectedLine.IndexOf(',') + 1, selectedLine.IndexOfWithCount(',', 2) - selectedLine.IndexOf(',') - 2);
                            selectedLine = selectedLine.Insert(selectedLine.IndexOf(',') + 1, bpmInTermsOfTimeString);
                            lines[j] = selectedLine;
                        }
                        else if (selectedOffset > startingOffset)
                        {
                            calculatedGridSnap = Convert.ToInt32((((selectedOffset - startingOffset) / (60000 / actualBpm)) * resolution));
                            newOffset = (int)(startingOffset + ((60000 / (enteredBpm * resolution)) * calculatedGridSnap));
                            if (selectedLine.Substring(selectedLine.IndexOfWithCount(',', 6), 1) == "1")
                            {
                                if (!isDefined)
                                {
                                    nextTimingPointOffset = selectedOffset;
                                    offsetChange = newOffset - selectedOffset;
                                    isDefined = true;
                                }
                            }
                            selectedLine = selectedLine.Remove(0, selectedLine.IndexOf(','));
                            if (offsetChange != 0)
                                selectedLine = selectedLine.Insert(0, (selectedOffset + offsetChange).ToString());
                            else
                                selectedLine = selectedLine.Insert(0, newOffset.ToString());
                            lines[j] = selectedLine;
                        }
                    }
                }
            }
            return lines;
        }
        private void SetNewHitObjectOffsets()
        {
            double resolution = 48;
            double actualBpm = Convert.ToDouble(dataGridView1.SelectedRows[0].Cells[1].Value);
            double enteredBpm = BPM_Changer.value;
            double calculatedGridSnap = 0;
            int selectedIndex = -1;
            for (int i = 0; i < linesList.Count; i++)
            {
                if (linesList[i] == "[HitObjects]")
                {
                    selectedIndex = i + 1;
                    break;
                }
            }
            string startingOffsetString = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();
            int startingOffset = Convert.ToInt32(startingOffsetString.Substring(0, startingOffsetString.IndexOf(' ')));
            int selectedOffset, newOffset;
            if (selectedIndex != -1)
            {
                for (int i = selectedIndex; i < linesList.Count && !string.IsNullOrWhiteSpace(linesList[i]); i++)
                {
                    string currentLine = linesList[i];
                    selectedOffset = Convert.ToInt32(currentLine.Substring(currentLine.IndexOfWithCount(',', 2), currentLine.IndexOfWithCount(',', 3) - currentLine.IndexOfWithCount(',', 2) - 1));
                    if (selectedOffset > startingOffset)
                    {
                        if (currentLine.SearchCharCount(',') != 6)
                        {
                            if (selectedOffset > nextTimingPointOffset && isDefined)
                            {
                                currentLine = currentLine.Remove(currentLine.IndexOfWithCount(',', 2), currentLine.IndexOfWithCount(',', 3) - currentLine.IndexOfWithCount(',', 2) - 1);
                                currentLine = currentLine.Insert(currentLine.IndexOfWithCount(',', 2), (selectedOffset + offsetChange).ToString());
                                linesList[i] = currentLine;
                            }
                            else
                            {
                                calculatedGridSnap = Convert.ToInt32((((selectedOffset - startingOffset) / (60000 / actualBpm)) * resolution));
                                newOffset = (int)(startingOffset + ((60000 / (enteredBpm * resolution)) * calculatedGridSnap));
                                currentLine = currentLine.Remove(currentLine.IndexOfWithCount(',', 2), currentLine.IndexOfWithCount(',', 3) - currentLine.IndexOfWithCount(',', 2) - 1);
                                currentLine = currentLine.Insert(currentLine.IndexOfWithCount(',', 2), newOffset.ToString());
                                linesList[i] = currentLine;
                            }
                        }
                        else // means that it's a spinner
                        {
                            int spinnerEndOffset = Convert.ToInt32(currentLine.Substring(currentLine.IndexOfWithCount(',', 5), currentLine.IndexOfWithCount(',', 6) - currentLine.IndexOfWithCount(',', 5) - 1));
                            if (selectedOffset > nextTimingPointOffset && isDefined)
                            {
                                currentLine = currentLine.Remove(currentLine.IndexOfWithCount(',', 2), currentLine.IndexOfWithCount(',', 3) - currentLine.IndexOfWithCount(',', 2) - 1);
                                currentLine = currentLine.Insert(currentLine.IndexOfWithCount(',', 2), (selectedOffset + offsetChange).ToString());
                                currentLine = currentLine.Remove(currentLine.IndexOfWithCount(',', 5), currentLine.IndexOfWithCount(',', 6) - currentLine.IndexOfWithCount(',', 5) - 1);
                                currentLine = currentLine.Insert(currentLine.IndexOfWithCount(',', 5), (spinnerEndOffset + offsetChange).ToString());
                                linesList[i] = currentLine;
                            }
                            else
                            {
                                calculatedGridSnap = Convert.ToInt32((((selectedOffset - startingOffset) / (60000 / actualBpm)) * resolution));
                                newOffset = (int)(startingOffset + ((60000 / (enteredBpm * resolution)) * calculatedGridSnap));
                                currentLine = currentLine.Remove(currentLine.IndexOfWithCount(',', 2), currentLine.IndexOfWithCount(',', 3) - currentLine.IndexOfWithCount(',', 2) - 1);
                                currentLine = currentLine.Insert(currentLine.IndexOfWithCount(',', 2), newOffset.ToString());
                                calculatedGridSnap = Convert.ToInt32((((spinnerEndOffset - startingOffset) / (60000 / actualBpm)) * resolution));
                                newOffset = (int)(spinnerEndOffset + ((60000 / (enteredBpm * resolution)) * calculatedGridSnap));
                                currentLine = currentLine.Remove(currentLine.IndexOfWithCount(',', 5), currentLine.IndexOfWithCount(',', 6) - currentLine.IndexOfWithCount(',', 5) - 1);
                                currentLine = currentLine.Insert(currentLine.IndexOfWithCount(',', 5), newOffset.ToString());
                                linesList[i] = currentLine;
                            }
                        }
                        isHaveNotes = true;
                    }
                }
            }
            if (isHaveNotes)
                ShowMode.Information(language.LanguageContent[Language.noteTimingPositive]);
            else
                ShowMode.Warning(language.LanguageContent[Language.noteTimingNegative]);
        }
        private string[] SetNewHitObjectOffsets(string[] lines)
        {
            double resolution = 48;
            double actualBpm = Convert.ToDouble(dataGridView1.SelectedRows[0].Cells[1].Value);
            double enteredBpm = BPM_Changer.value;
            double calculatedGridSnap = 0;
            int selectedIndex = -1;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] == "[HitObjects]")
                {
                    selectedIndex = i + 1;
                    break;
                }
            }
            string startingOffsetString = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();
            int startingOffset = Convert.ToInt32(startingOffsetString.Substring(0, startingOffsetString.IndexOf(' ')));
            int selectedOffset, newOffset;
            if (selectedIndex != -1)
            {
                for (int i = selectedIndex; i < lines.Length && !string.IsNullOrWhiteSpace(lines[i]); i++)
                {
                    string currentLine = lines[i];
                    selectedOffset = Convert.ToInt32(currentLine.Substring(currentLine.IndexOfWithCount(',', 2), currentLine.IndexOfWithCount(',', 3) - currentLine.IndexOfWithCount(',', 2) - 1));
                    if (selectedOffset > startingOffset)
                    {
                        if (currentLine.SearchCharCount(',') != 6)
                        {
                            if (selectedOffset > nextTimingPointOffset && isDefined)
                            {
                                currentLine = currentLine.Remove(currentLine.IndexOfWithCount(',', 2), currentLine.IndexOfWithCount(',', 3) - currentLine.IndexOfWithCount(',', 2) - 1);
                                currentLine = currentLine.Insert(currentLine.IndexOfWithCount(',', 2), (selectedOffset + offsetChange).ToString());
                                lines[i] = currentLine;
                            }
                            else
                            {
                                calculatedGridSnap = Convert.ToInt32((((selectedOffset - startingOffset) / (60000 / actualBpm)) * resolution));
                                newOffset = (int)(startingOffset + ((60000 / (enteredBpm * resolution)) * calculatedGridSnap));
                                currentLine = currentLine.Remove(currentLine.IndexOfWithCount(',', 2), currentLine.IndexOfWithCount(',', 3) - currentLine.IndexOfWithCount(',', 2) - 1);
                                currentLine = currentLine.Insert(currentLine.IndexOfWithCount(',', 2), newOffset.ToString());
                                lines[i] = currentLine;
                            }
                        }
                        else // means that it's a spinner
                        {
                            int spinnerEndOffset = Convert.ToInt32(currentLine.Substring(currentLine.IndexOfWithCount(',', 5), currentLine.IndexOfWithCount(',', 6) - currentLine.IndexOfWithCount(',', 5) - 1));
                            if (selectedOffset > nextTimingPointOffset && isDefined)
                            {
                                currentLine = currentLine.Remove(currentLine.IndexOfWithCount(',', 2), currentLine.IndexOfWithCount(',', 3) - currentLine.IndexOfWithCount(',', 2) - 1);
                                currentLine = currentLine.Insert(currentLine.IndexOfWithCount(',', 2), (selectedOffset + offsetChange).ToString());
                                currentLine = currentLine.Remove(currentLine.IndexOfWithCount(',', 5), currentLine.IndexOfWithCount(',', 6) - currentLine.IndexOfWithCount(',', 5) - 1);
                                currentLine = currentLine.Insert(currentLine.IndexOfWithCount(',', 5), (spinnerEndOffset + offsetChange).ToString());
                                lines[i] = currentLine;
                            }
                            else
                            {
                                calculatedGridSnap = Convert.ToInt32((((selectedOffset - startingOffset) / (60000 / actualBpm)) * resolution));
                                newOffset = (int)(startingOffset + ((60000 / (enteredBpm * resolution)) * calculatedGridSnap));
                                currentLine = currentLine.Remove(currentLine.IndexOfWithCount(',', 2), currentLine.IndexOfWithCount(',', 3) - currentLine.IndexOfWithCount(',', 2) - 1);
                                currentLine = currentLine.Insert(currentLine.IndexOfWithCount(',', 2), newOffset.ToString());
                                calculatedGridSnap = Convert.ToInt32((((spinnerEndOffset - startingOffset) / (60000 / actualBpm)) * resolution));
                                newOffset = (int)(spinnerEndOffset + ((60000 / (enteredBpm * resolution)) * calculatedGridSnap));
                                currentLine = currentLine.Remove(currentLine.IndexOfWithCount(',', 5), currentLine.IndexOfWithCount(',', 6) - currentLine.IndexOfWithCount(',', 5) - 1);
                                currentLine = currentLine.Insert(currentLine.IndexOfWithCount(',', 5), newOffset.ToString());
                                lines[i] = currentLine;
                            }
                        }
                        isHaveNotes = true;
                    }
                }
            }
            if (!isHaveNotes)
                ShowMode.Warning(language.LanguageContent[Language.noteTimingNegative]);
            return lines;
        }
        private void CheckMinorShiftOccurences() // fixes the minor calculation errors depending on floating point issues by equalizing the green point - note offsets if between 5ms around.
        {
            lines = File.ReadAllLines(path);
            int timingPointsIndex = -1, hitObjectsIndex = -1;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] == "[TimingPoints]")
                    timingPointsIndex = i + 1;
                if (lines[i] == "[HitObjects]")
                {
                    hitObjectsIndex = i + 1;
                    break;
                }
            }
            if (timingPointsIndex == -1)
                ShowMode.Error(language.LanguageContent[Language.timingPointNotFound]);
            else if (hitObjectsIndex == -1)
                return;
            else
            {
                string currentTimingPointLine, currentHitObjectLine;
                int timingPointOffset, hitObjectOffset;
                for (int i = timingPointsIndex; i < hitObjectsIndex - 1 && !string.IsNullOrWhiteSpace(lines[i]); i++)
                {
                    currentTimingPointLine = lines[i];
                    timingPointOffset = Int32.Parse(currentTimingPointLine.Substring(0, currentTimingPointLine.IndexOf(',')));
                    for (int j = hitObjectsIndex; j < lines.Length; j++)
                    {
                        currentHitObjectLine = lines[j];
                        if (!string.IsNullOrWhiteSpace(currentHitObjectLine))
                        {
                            hitObjectOffset = Int32.Parse(currentHitObjectLine.Substring(
                            currentHitObjectLine.IndexOfWithCount(',', 2), currentHitObjectLine.IndexOfWithCount(',', 3) - currentHitObjectLine.IndexOfWithCount(',', 2) - 1));
                            if ((hitObjectOffset > timingPointOffset && hitObjectOffset < timingPointOffset + 6) ||  // timing     note      timing     note
                                (hitObjectOffset - 6 > timingPointOffset && hitObjectOffset < timingPointOffset))     // 3000  --  3005   ||  3005  --  3000
                            {
                                timingPointOffset = hitObjectOffset;
                                currentTimingPointLine = currentTimingPointLine.Remove(0, currentTimingPointLine.IndexOf(','));
                                currentTimingPointLine = currentTimingPointLine.Insert(0, timingPointOffset.ToString());
                                lines[i] = currentTimingPointLine;
                            }
                        }
                    }
                }
                File.WriteAllLines(path, lines);
            }
        }
        private void CheckMinorShiftOccurences(string path) // fixes the minor calculation errors depending on floating point issues by equalizing the green point - note offsets if between 5ms around.
        {
            string[] lines = File.ReadAllLines(path);
            int timingPointsIndex = -1, hitObjectsIndex = -1;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] == "[TimingPoints]")
                    timingPointsIndex = i + 1;
                if (lines[i] == "[HitObjects]")
                {
                    hitObjectsIndex = i + 1;
                    break;
                }
            }
            if (timingPointsIndex == -1)
            {
                ShowMode.Error(language.LanguageContent[Language.timingPointNotFound]);
                return;
            }
            else if (hitObjectsIndex == -1)
                return;
            else
            {
                string currentTimingPointLine, currentHitObjectLine;
                int timingPointOffset, hitObjectOffset;
                for (int i = timingPointsIndex; i < hitObjectsIndex - 1 && !string.IsNullOrWhiteSpace(lines[i]); i++)
                {
                    currentTimingPointLine = lines[i];
                    timingPointOffset = Int32.Parse(currentTimingPointLine.Substring(0, currentTimingPointLine.IndexOf(',')));
                    for (int j = hitObjectsIndex; j < lines.Length; j++)
                    {
                        currentHitObjectLine = lines[j];
                        if (!string.IsNullOrWhiteSpace(currentHitObjectLine))
                        {
                            hitObjectOffset = Int32.Parse(currentHitObjectLine.Substring(
                            currentHitObjectLine.IndexOfWithCount(',', 2), currentHitObjectLine.IndexOfWithCount(',', 3) - currentHitObjectLine.IndexOfWithCount(',', 2) - 1));
                            if ((hitObjectOffset > timingPointOffset && hitObjectOffset < timingPointOffset + 6) ||  // timing     note      timing     note
                                (hitObjectOffset - 6 > timingPointOffset && hitObjectOffset < timingPointOffset))     // 3000  --  3005   ||  3005  --  3000
                            {
                                timingPointOffset = hitObjectOffset;
                                currentTimingPointLine = currentTimingPointLine.Remove(0, currentTimingPointLine.IndexOf(','));
                                currentTimingPointLine = currentTimingPointLine.Insert(0, timingPointOffset.ToString());
                                lines[i] = currentTimingPointLine;
                            }
                        }
                    }
                }
                File.WriteAllLines(path, lines);
            }
        }
        private void WriteNewFile()
        {
            if (ShowMode.QuestionWithYesNo(language.LanguageContent[Language.askBackup]) == DialogResult.Yes)
            {
                string copyPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + fileName;
                if (File.Exists(copyPath))
                {
                    if (ShowMode.QuestionWithYesNo(language.LanguageContent[Language.overwriteBackup]) == DialogResult.Yes)
                        saveBackup(copyPath);
                }
                else
                    saveBackup(copyPath);
            }
            File.WriteAllLines(path, linesList.ToArray());
            if (isHaveNotes)
            {
                if (ShowMode.QuestionWithYesNo(language.LanguageContent[Language.notesShift]) == DialogResult.Yes)
                {
                    ShowMode.Warning(language.LanguageContent[Language.snapWarning]);
                    CheckMinorShiftOccurences();
                }
            }
            ShowMode.Information(language.LanguageContent[Language.processComplete]);
        }
        private void WriteNewFile(string path, string fileName, string[] lines)
        {
            File.WriteAllLines(path, lines);
            if (isHaveNotes)
            {
                if (ShowMode.QuestionWithYesNo(language.LanguageContent[Language.notesShift] + " " + language.LanguageContent[Language.currentFile] + fileName) == DialogResult.Yes)
                {
                    ShowMode.Warning(language.LanguageContent[Language.snapWarning]);
                    CheckMinorShiftOccurences(path);
                }
            }
        }
        private void saveBackup(string copyPath)
        {
            File.Copy(path, copyPath, true);
            ShowMode.Information(language.LanguageContent[Language.backupSaved]);
        }
        private void saveBackups(string[] paths, string[] fileNames)
        {
            int slashCount = paths[0].SearchCharCount('\\');
            string directoryName = paths[0].Substring(paths[0].IndexOfWithCount('\\', slashCount - 1),
                paths[0].IndexOfWithCount('\\', slashCount) - paths[0].IndexOfWithCount('\\', slashCount - 1) - 1);
            string directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + directoryName;
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
            ShowMode.Information(language.LanguageContent[Language.savedirectoryformultiplefiles] + "\"" + directoryName + "\"");
            for (int i = 0; i < paths.Length; i++)
                File.Copy(paths[i], directoryPath + "\\" + fileNames[i], true);
            ShowMode.Information(language.LanguageContent[Language.backupSaved]);
        }
        private void ChangeOffsets()
        {
            lines = File.ReadAllLines(path);
            string firstOffsetString = dataGridView1.SelectedRows[dataGridView1.SelectedRows.Count - 1].Cells[0].Value.ToString();
            string lastOffsetString = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();
            int firstOffset = Convert.ToInt32(firstOffsetString.Substring(0, firstOffsetString.IndexOf(' ')));
            int lastOffset;
            if (dataGridView1.SelectedRows.Contains(dataGridView1.Rows[0]))
                lastOffset = int.MaxValue;
            else
                lastOffset = Convert.ToInt32(lastOffsetString.Substring(0, lastOffsetString.IndexOf(' ')));
            string timeString = string.Empty;
            int timingPointsIndex = 0, hitObjectsIndex = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] == "[TimingPoints]")
                {
                    timingPointsIndex = i;
                    break;
                }
            }
            for (int i = timingPointsIndex + 1; i < lines.Length && !string.IsNullOrWhiteSpace(lines[i]); i++)
            {
                if (!lines[i].Contains(',')) continue;
                timeString = lines[i];
                int offset = Convert.ToInt32(timeString.Substring(0, timeString.IndexOf(',')));
                if (offset >= firstOffset && offset <= lastOffset)
                {
                    offset += (int)BPM_Changer.value;
                    timeString = timeString.Remove(0, lines[i].IndexOf(','));
                    timeString = timeString.Insert(0, offset.ToString());
                    lines[i] = timeString;
                }
                else if (offset >= lastOffset)
                    break;
            }
            for (int i = 0; i < lines.Length; i++)
                if (lines[i] == "[HitObjects]")
                {
                    hitObjectsIndex = i;
                    break;
                }
            for (int i = hitObjectsIndex + 1; i < lines.Length; i++)
            {
                timeString = lines[i];
                int offset = Convert.ToInt32(timeString.Substring(timeString.IndexOfWithCount(',', 2), timeString.IndexOfWithCount(',', 3) - timeString.IndexOfWithCount(',', 2) - 1));
                if(offset >= firstOffset && offset <= lastOffset)
                {
                    offset += (int)BPM_Changer.value;
                    timeString = timeString.Remove(timeString.IndexOfWithCount(',', 2), timeString.IndexOfWithCount(',', 3) - timeString.IndexOfWithCount(',', 2) - 1);
                    timeString = timeString.Insert(timeString.IndexOfWithCount(',', 2), offset.ToString());
                    lines[i] = timeString;
                }
            }
        }
        private string[] ChangeOffsets(string path, string[] lines)
        {
            string firstOffsetString = dataGridView1.SelectedRows[dataGridView1.SelectedRows.Count - 1].Cells[0].Value.ToString();
            string lastOffsetString = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();
            int firstOffset = Convert.ToInt32(firstOffsetString.Substring(0, firstOffsetString.IndexOf(' ')));
            int lastOffset;
            if (dataGridView1.SelectedRows.Contains(dataGridView1.Rows[0]))
                lastOffset = int.MaxValue;
            else
                lastOffset = Convert.ToInt32(lastOffsetString.Substring(0, lastOffsetString.IndexOf(' ')));
            string timeString = string.Empty;
            int timingPointsIndex = 0, hitObjectsIndex = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] == "[TimingPoints]")
                {
                    timingPointsIndex = i;
                    break;
                }
            }
            for (int i = timingPointsIndex + 1; i < lines.Length && !string.IsNullOrWhiteSpace(lines[i]); i++)
            {
                if (!lines[i].Contains(',')) continue;
                timeString = lines[i];
                int offset = Convert.ToInt32(timeString.Substring(0, timeString.IndexOf(',')));
                if (offset >= firstOffset && offset <= lastOffset)
                {
                    offset += (int)BPM_Changer.value;
                    timeString = timeString.Remove(0, lines[i].IndexOf(','));
                    timeString = timeString.Insert(0, offset.ToString());
                    lines[i] = timeString;
                }
                else if (offset >= lastOffset)
                    break;
            }
            for (int i = 0; i < lines.Length; i++)
                if (lines[i] == "[HitObjects]")
                {
                    hitObjectsIndex = i;
                    break;
                }
            for (int i = hitObjectsIndex + 1; i < lines.Length; i++)
            {
                timeString = lines[i];
                int offset = Convert.ToInt32(timeString.Substring(timeString.IndexOfWithCount(',', 2), timeString.IndexOfWithCount(',', 3) - timeString.IndexOfWithCount(',', 2) - 1));
                if (offset >= firstOffset && offset <= lastOffset)
                {
                    offset += (int)BPM_Changer.value;
                    timeString = timeString.Remove(timeString.IndexOfWithCount(',', 2), timeString.IndexOfWithCount(',', 3) - timeString.IndexOfWithCount(',', 2) - 1);
                    timeString = timeString.Insert(timeString.IndexOfWithCount(',', 2), offset.ToString());
                    lines[i] = timeString;
                }
            }
            return lines;
        }
        private void ChangeBookmarks_Offset()
        {
            int editorIndex = -1, bookmarksIndex = -1; for (int i = 0; i < linesList.Count; i++) if (linesList[i] == "[Editor]") { editorIndex = i; break; }
            if (editorIndex != -1)
            {
                string bookmarkString = string.Empty;
                for(int i = editorIndex + 1; !string.IsNullOrWhiteSpace(linesList[i]); i++)
                {
                    if (linesList[i].Contains("Bookmarks"))
                    {
                        bookmarkString = linesList[i].Substring(linesList[i].IndexOf(':') + 1).TrimStart().TrimEnd();
                        bookmarksIndex = i;
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(bookmarkString))
                {
                    List<int> bookmarks = new List<int>();
                    int bookmarkCount = bookmarkString.SearchCharCount(',') + 1;
                    if (bookmarkCount != 0)
                    {
                        for (int i = 0; i < bookmarkCount; i++)
                        {
                            if (i == 0)
                                bookmarks.Add(Convert.ToInt32(bookmarkString.Substring(0, bookmarkString.IndexOf(','))));
                            else if (i + 1 == bookmarkCount)
                                bookmarks.Add(Convert.ToInt32(bookmarkString.Substring(bookmarkString.IndexOfWithCount(',', i))));
                            else
                                bookmarks.Add(Convert.ToInt32(bookmarkString.Substring(
                                    bookmarkString.IndexOfWithCount(',', i), bookmarkString.IndexOfWithCount(',', i + 1) - bookmarkString.IndexOfWithCount(',', i) - 1)));
                        }
                        int startOffset, endOffset;
                        string currentLine = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();
                        startOffset = Convert.ToInt32(currentLine.Substring(0, currentLine.IndexOf(' ')));
                        currentLine = dataGridView1.SelectedRows[dataGridView1.SelectedRows.Count - 1].Cells[0].Value.ToString();
                        endOffset = Convert.ToInt32(currentLine.Substring(0, currentLine.IndexOf(' ')));
                        if (startOffset >= endOffset)
                        {
                            int temp = startOffset;
                            startOffset = endOffset;
                            endOffset = temp;
                        }
                        for (int i = 0; i < bookmarks.Count; i++)
                        {
                            if (bookmarks[i] >= startOffset && bookmarks[i] <= endOffset)
                                bookmarks[i] += (int)BPM_Changer.value;
                        }
                        bookmarkString = "Bookmarks: ";
                        for (int i = 0; i < bookmarks.Count; i++)
                        {
                            bookmarkString += bookmarks[i].ToString();
                            if (i + 1 == bookmarks.Count) { }
                            else bookmarkString += ",";
                        }
                        linesList[bookmarksIndex] = bookmarkString;
                    }
                }
                else
                    ShowMode.Error(language.LanguageContent[Language.noBookmarks]);
            }
        }
        private string[] ChangeBookmarks_Offset(string[] lines)
        {
            int editorIndex = -1, bookmarksIndex = -1; for (int i = 0; i < lines.Length; i++) if (lines[i] == "[Editor]") { editorIndex = i; break; }
            if (editorIndex != -1)
            {
                string bookmarkString = string.Empty;
                for (int i = editorIndex + 1; !string.IsNullOrWhiteSpace(lines[i]); i++)
                {
                    if (lines[i].Contains("Bookmarks"))
                    {
                        bookmarkString = lines[i].Substring(lines[i].IndexOf(':') + 1).TrimStart().TrimEnd();
                        bookmarksIndex = i;
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(bookmarkString))
                {
                    List<int> bookmarks = new List<int>();
                    int bookmarkCount = bookmarkString.SearchCharCount(',') + 1;
                    if (bookmarkCount != 0)
                    {
                        for (int i = 0; i < bookmarkCount; i++)
                        {
                            if (i == 0)
                                bookmarks.Add(Convert.ToInt32(bookmarkString.Substring(0, bookmarkString.IndexOf(','))));
                            else if (i + 1 == bookmarkCount)
                                bookmarks.Add(Convert.ToInt32(bookmarkString.Substring(bookmarkString.IndexOfWithCount(',', i))));
                            else
                                bookmarks.Add(Convert.ToInt32(bookmarkString.Substring(
                                    bookmarkString.IndexOfWithCount(',', i), bookmarkString.IndexOfWithCount(',', i + 1) - bookmarkString.IndexOfWithCount(',', i) - 1)));
                        }
                        int startOffset, endOffset;
                        string currentLine = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();
                        startOffset = Convert.ToInt32(currentLine.Substring(0, currentLine.IndexOf(' ')));
                        currentLine = dataGridView1.SelectedRows[dataGridView1.SelectedRows.Count - 1].Cells[0].Value.ToString();
                        endOffset = Convert.ToInt32(currentLine.Substring(0, currentLine.IndexOf(' ')));
                        if (startOffset >= endOffset)
                        {
                            int temp = startOffset;
                            startOffset = endOffset;
                            endOffset = temp;
                        }
                        for (int i = 0; i < bookmarks.Count; i++)
                        {
                            if (bookmarks[i] >= startOffset && bookmarks[i] <= endOffset)
                                bookmarks[i] += (int)BPM_Changer.value;
                        }
                        bookmarkString = "Bookmarks: ";
                        for (int i = 0; i < bookmarks.Count; i++)
                        {
                            bookmarkString += bookmarks[i].ToString();
                            if (i + 1 == bookmarks.Count) { }
                            else bookmarkString += ",";
                        }
                        lines[bookmarksIndex] = bookmarkString;
                    }
                }
                else
                    ShowMode.Error(language.LanguageContent[Language.noBookmarks]);
            }
            return lines;
        }
        private void ChangeBookmarks_BPM()
        {
            int editorIndex = -1, bookmarksIndex = -1; for (int i = 0; i < linesList.Count; i++) if (linesList[i] == "[Editor]") { editorIndex = i; break; }
            if (editorIndex != -1)
            {
                string bookmarkString = string.Empty;
                for (int i = editorIndex + 1; !string.IsNullOrWhiteSpace(linesList[i]); i++)
                {
                    if (linesList[i].Contains("Bookmarks") && linesList[i] != "Bookmarks: ")
                    {
                        bookmarkString = linesList[i].Substring(linesList[i].IndexOf(':') + 1).TrimStart().TrimEnd();
                        bookmarksIndex = i;
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(bookmarkString))
                {
                    List<int> bookmarks = new List<int>();
                    int bookmarkCount = bookmarkString.SearchCharCount(',') + 1;
                    if (bookmarkCount != 0)
                    {
                        for (int i = 0; i < bookmarkCount; i++)
                        {
                            if (i == 0)
                                bookmarks.Add(Convert.ToInt32(bookmarkString.Substring(0, bookmarkString.IndexOf(','))));
                            else if (i + 1 == bookmarkCount)
                                bookmarks.Add(Convert.ToInt32(bookmarkString.Substring(bookmarkString.IndexOfWithCount(',', i))));
                            else
                                bookmarks.Add(Convert.ToInt32(bookmarkString.Substring(
                                    bookmarkString.IndexOfWithCount(',', i), bookmarkString.IndexOfWithCount(',', i + 1) - bookmarkString.IndexOfWithCount(',', i) - 1)));
                        }
                        int startOffset, bookmarkStartingIndex = -1;
                        string currentLine = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();
                        startOffset = Convert.ToInt32(currentLine.Substring(0, currentLine.IndexOf(' ')));
                        for (int i = 0; i < bookmarks.Count; i++) if (bookmarks[i] >= startOffset) { bookmarkStartingIndex = i; break; }
                        if (bookmarkStartingIndex != -1)
                        {
                            List<int> timingPointOffsets = new List<int>();
                            double resolution = 192;
                            int timingPointsIndex = -1; for (int i = 0; i < linesList.Count; i++) if (linesList[i] == "[TimingPoints]") { timingPointsIndex = i; break; }
                            if (timingPointsIndex != -1)
                            {
                                double changedTimingPointBPM = 0;
                                for (int i = timingPointsIndex + 1; !string.IsNullOrWhiteSpace(linesList[i]); i++)
                                {
                                    currentLine = linesList[i];
                                    if (currentLine.Substring(currentLine.IndexOfWithCount(',', 6), 1) == "1")
                                    {
                                        timingPointOffsets.Add(Convert.ToInt32(currentLine.Substring(0, currentLine.IndexOf(','))));
                                        if (timingPointOffsets[timingPointOffsets.Count - 1] == startOffset)
                                        {
                                            string testString = currentLine.Substring(currentLine.IndexOfWithCount(',', 1), currentLine.IndexOfWithCount(',', 2) - currentLine.IndexOfWithCount(',', 1) - 1).Replace('.', ',');
                                            changedTimingPointBPM = Convert.ToDouble(testString);
                                        }
                                    }
                                }
                                int changedTimingPointIndex = timingPointOffsets.IndexOf(startOffset);
                                int offsetChange = 0;
                                if (timingPointOffsets.Count != 1)
                                {
                                    if (changedTimingPointIndex + 1 < timingPointOffsets.Count)
                                    {
                                        double totalSnap = (int)((timingPointOffsets[changedTimingPointIndex + 1] - timingPointOffsets[changedTimingPointIndex]) * resolution / (changedTimingPointBPM));
                                        double newOffset = timingPointOffsets[changedTimingPointIndex] + ((totalSnap / resolution) * (60000 / BPM_Changer.value));
                                        offsetChange = (int)newOffset - timingPointOffsets[changedTimingPointIndex + 1];
                                    }
                                }
                                if (changedTimingPointIndex != -1)
                                {
                                    for (int i = bookmarkStartingIndex; i < bookmarks.Count; i++)
                                    {
                                        int bookmarkOffset = bookmarks[i];
                                        int closestTimingPointIndex = -1;
                                        if (timingPointOffsets.Count > 1)
                                        {
                                            for (int j = timingPointOffsets.Count - 1; j >= 0; j--)
                                                if (timingPointOffsets[j] <= bookmarkOffset)
                                                { closestTimingPointIndex = j; break; }
                                        }
                                        else if (timingPointOffsets[0] < bookmarkOffset)
                                            closestTimingPointIndex = 0;
                                        else
                                            closestTimingPointIndex = -1;
                                        if (closestTimingPointIndex != -1)
                                        {
                                            if (closestTimingPointIndex == changedTimingPointIndex)
                                            {
                                                double snapValue = (int)(((bookmarkOffset - timingPointOffsets[closestTimingPointIndex]) / changedTimingPointBPM) * resolution);
                                                double newOffset = timingPointOffsets[closestTimingPointIndex] + ((snapValue / resolution) *  (60000 / BPM_Changer.value));
                                                bookmarks[i] = (int)newOffset;
                                            }
                                            else if (closestTimingPointIndex > changedTimingPointIndex)
                                            {
                                                if (offsetChange != 0)
                                                    bookmarks[i] += offsetChange;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    ShowMode.Error("On current timing points, nothing found on the list.");
                                    return;
                                }
                            }
                            else
                            {
                                ShowMode.Error(language.LanguageContent[Language.noTimingPoints]);
                                return;
                            }
                            bookmarkString = "Bookmarks: ";
                            for (int i = 0; i < bookmarks.Count; i++)
                            {
                                bookmarkString += bookmarks[i].ToString();
                                if (i + 1 == bookmarks.Count) { }
                                else bookmarkString += ",";
                            }
                            linesList[bookmarksIndex] = bookmarkString;
                        }
                        else
                            ShowMode.Error(language.LanguageContent[Language.nobookmarksfoundafterselectedpoint]);
                    }
                }
                else
                    ShowMode.Error(language.LanguageContent[Language.noBookmarks]);
            }
            else
                ShowMode.Error(language.LanguageContent[Language.editortagmissing]);
        }
        private string[] ChangeBookmarks_BPM(string[] lines)
        {
            int editorIndex = -1, bookmarksIndex = -1; for (int i = 0; i < lines.Length; i++) if (lines[i] == "[Editor]") { editorIndex = i; break; }
            if (editorIndex != -1)
            {
                string bookmarkString = string.Empty;
                for (int i = editorIndex + 1; !string.IsNullOrWhiteSpace(lines[i]); i++)
                {
                    if (lines[i].Contains("Bookmarks") && lines[i] != "Bookmarks: ")
                    {
                        bookmarkString = lines[i].Substring(lines[i].IndexOf(':') + 1).TrimStart().TrimEnd();
                        bookmarksIndex = i;
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(bookmarkString))
                {
                    List<int> bookmarks = new List<int>();
                    int bookmarkCount = bookmarkString.SearchCharCount(',') + 1;
                    if (bookmarkCount != 0)
                    {
                        for (int i = 0; i < bookmarkCount; i++)
                        {
                            if (i == 0)
                                bookmarks.Add(Convert.ToInt32(bookmarkString.Substring(0, bookmarkString.IndexOf(','))));
                            else if (i + 1 == bookmarkCount)
                                bookmarks.Add(Convert.ToInt32(bookmarkString.Substring(bookmarkString.IndexOfWithCount(',', i))));
                            else
                                bookmarks.Add(Convert.ToInt32(bookmarkString.Substring(
                                    bookmarkString.IndexOfWithCount(',', i), bookmarkString.IndexOfWithCount(',', i + 1) - bookmarkString.IndexOfWithCount(',', i) - 1)));
                        }
                        int startOffset, bookmarkStartingIndex = -1;
                        string currentLine = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();
                        startOffset = Convert.ToInt32(currentLine.Substring(0, currentLine.IndexOf(' ')));
                        for (int i = 0; i < bookmarks.Count; i++) if (bookmarks[i] >= startOffset) { bookmarkStartingIndex = i; break; }
                        if (bookmarkStartingIndex != -1)
                        {
                            List<int> timingPointOffsets = new List<int>();
                            double resolution = 192;
                            int timingPointsIndex = -1; for (int i = 0; i < lines.Length; i++) if (lines[i] == "[TimingPoints]") { timingPointsIndex = i; break; }
                            if (timingPointsIndex != -1)
                            {
                                double changedTimingPointBPM = 0;
                                for (int i = timingPointsIndex + 1; !string.IsNullOrWhiteSpace(lines[i]); i++)
                                {
                                    currentLine = lines[i];
                                    if (currentLine.Substring(currentLine.IndexOfWithCount(',', 6), 1) == "1")
                                    {
                                        timingPointOffsets.Add(Convert.ToInt32(currentLine.Substring(0, currentLine.IndexOf(','))));
                                        if (timingPointOffsets[timingPointOffsets.Count - 1] == startOffset)
                                        {
                                            string testString = currentLine.Substring(currentLine.IndexOfWithCount(',', 1), currentLine.IndexOfWithCount(',', 2) - currentLine.IndexOfWithCount(',', 1) - 1).Replace('.', ',');
                                            changedTimingPointBPM = Convert.ToDouble(testString);
                                        }
                                    }
                                }
                                int changedTimingPointIndex = timingPointOffsets.IndexOf(startOffset);
                                int offsetChange = 0;
                                if (timingPointOffsets.Count != 1)
                                {
                                    if (changedTimingPointIndex + 1 < timingPointOffsets.Count)
                                    {
                                        double totalSnap = (int)((timingPointOffsets[changedTimingPointIndex + 1] - timingPointOffsets[changedTimingPointIndex]) * resolution / (changedTimingPointBPM));
                                        double newOffset = timingPointOffsets[changedTimingPointIndex] + ((totalSnap / resolution) * (60000 / BPM_Changer.value));
                                        offsetChange = (int)newOffset - timingPointOffsets[changedTimingPointIndex + 1];
                                    }
                                }
                                if (changedTimingPointIndex != -1)
                                {
                                    for (int i = bookmarkStartingIndex; i < bookmarks.Count; i++)
                                    {
                                        int bookmarkOffset = bookmarks[i];
                                        int closestTimingPointIndex = -1;
                                        if (timingPointOffsets.Count > 1)
                                        {
                                            for (int j = timingPointOffsets.Count - 1; j >= 0; j--)
                                                if (timingPointOffsets[j] <= bookmarkOffset)
                                                { closestTimingPointIndex = j; break; }
                                        }
                                        else if (timingPointOffsets[0] < bookmarkOffset)
                                            closestTimingPointIndex = 0;
                                        else
                                            closestTimingPointIndex = -1;
                                        if (closestTimingPointIndex != -1)
                                        {
                                            if (closestTimingPointIndex == changedTimingPointIndex)
                                            {
                                                double snapValue = (int)(((bookmarkOffset - timingPointOffsets[closestTimingPointIndex]) / changedTimingPointBPM) * resolution);
                                                double newOffset = timingPointOffsets[closestTimingPointIndex] + ((snapValue / resolution) * (60000 / BPM_Changer.value));
                                                bookmarks[i] = (int)newOffset;
                                            }
                                            else if (closestTimingPointIndex > changedTimingPointIndex)
                                            {
                                                if (offsetChange != 0)
                                                    bookmarks[i] += offsetChange;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    ShowMode.Error("On current timing points, nothing found on the list.");
                                    return lines;
                                }
                            }
                            else
                            {
                                ShowMode.Error(language.LanguageContent[Language.noTimingPoints]);
                                return lines;
                            }
                            bookmarkString = "Bookmarks: ";
                            for (int i = 0; i < bookmarks.Count; i++)
                            {
                                bookmarkString += bookmarks[i].ToString();
                                if (i + 1 == bookmarks.Count) { }
                                else bookmarkString += ",";
                            }
                            lines[bookmarksIndex] = bookmarkString;
                        }
                        else
                            ShowMode.Error(language.LanguageContent[Language.nobookmarksfoundafterselectedpoint]);
                    }
                }
                else
                    ShowMode.Error(language.LanguageContent[Language.noBookmarks]);
            }
            else
                ShowMode.Error(language.LanguageContent[Language.editortagmissing]);
            return lines;
        }
        private string[] ChangeAllSnaps(string[] lines, List<string> timingContent, string basePath)
        {
            List<Notes> notes = new List<Notes>();
            List<TimingPoints> timingPoints = new List<TimingPoints>();
            List<TimingPoints> newTimingPoints = new List<TimingPoints>();
            List<int> errorOffsets = new List<int>();
            List<int> bookmarks = new List<int>();
            double resolution = 96;
            int timingPointsIndex = -1, hitObjectsIndex = -1;
            linesList = lines.ToList();
            for (int i = 0; i < linesList.Count; i++) if (linesList[i] == "[TimingPoints]") { timingPointsIndex = i; break; }
            for (int i = 0; i < linesList.Count; i++) if (linesList[i] == "[HitObjects]") { hitObjectsIndex = i; break; }
            if (timingPointsIndex == -1 || hitObjectsIndex == -1)
                ShowMode.Error(language.LanguageContent[Language.noTimingPointsOrNotes]);
            else
            {
                ShowMode.Information(language.LanguageContent[Language.openTimingDiff]);
                ShowMode.Warning(language.LanguageContent[Language.allNotesAndTimingPointsSnapped]);
                string currentLine;
                int offset, type;
                for(int i = timingPointsIndex + 1; !string.IsNullOrWhiteSpace(linesList[i]); i++)
                {
                    currentLine = linesList[i];
                    offset = Convert.ToInt32(currentLine.Substring(0, currentLine.IndexOf(',')));
                    type = Convert.ToInt32(currentLine.Substring(currentLine.IndexOfWithCount(',', 6), 1));
                    timingPoints.Add(new TimingPoints(offset, type, currentLine));
                }
                for (int i = hitObjectsIndex + 1; i < linesList.Count; i++)
                {
                    if(!string.IsNullOrWhiteSpace(linesList[i]))
                    {
                        currentLine = linesList[i];
                        offset = Convert.ToInt32(currentLine.Substring(currentLine.IndexOfWithCount(',', 2),
                            currentLine.IndexOfWithCount(',', 3) - currentLine.IndexOfWithCount(',', 2) - 1));
                        notes.Add(new Notes(offset, currentLine));
                    }
                }
                if (timingPoints.Count != 0 && notes.Count != 0)
                {
                    for (int i = 0; i < timingPoints.Count; i++)
                        for (int j = 0; j < i; j++)
                            if (timingPoints[j].Offset > timingPoints[i].Offset)
                            {
                                TimingPoints temp = timingPoints[i];
                                timingPoints[i] = timingPoints[j];
                                timingPoints[j] = temp;
                            }
                    for (int i = 0; i < notes.Count; i++)
                        for (int j = 0; j < i; j++)
                            if (notes[j].Offset > notes[i].Offset)
                            {
                                Notes temp = notes[i];
                                notes[i] = notes[j];
                                notes[j] = temp;
                            }
                    timingPoints[0].SnapValue = 0;
                    notes[0].SnapValue = 0;
                    double currentBPM = 0, snap = 0; if (timingPoints[0].Type == 1) currentBPM = timingPoints[0].BPM;
                    int currentTimingPointOffset = timingPoints[0].Offset, initialSnap = 0;
                    int offsetDifference, timingPointListIndex = 0, snapInt;
                    if (timingPoints[0].Type == 1)
                    {
                        for (int i = 1; i < timingPoints.Count; i++)
                        {
                            offsetDifference = timingPoints[i].Offset - currentTimingPointOffset;
                            snap = (offsetDifference / currentBPM) * resolution;
                            snapInt = Convert.ToInt32(snap);
                            if (snapInt % (resolution / 12) != 0 && snapInt % (resolution / 16) != 0)
                                errorOffsets.Add(timingPoints[i].Offset);
                            if (timingPoints[i].Type == 1)
                            {
                                currentBPM = timingPoints[i].BPM;
                                currentTimingPointOffset = timingPoints[i].Offset;
                                initialSnap = timingPoints[i].SnapValue;
                            }
                        }
                        initialSnap = 0;
                        int timingPointListIndexTemp = 0;
                        for (int i = 1; i < notes.Count; i++)
                        {
                            for (int j = timingPoints.Count - 1; j >= 0; j--)
                                if (timingPoints[j].Offset <= notes[i].Offset && timingPoints[j].Type == 1)
                                {
                                    timingPointListIndex = j;
                                    break;
                                }
                            offsetDifference = notes[i].Offset - timingPoints[timingPointListIndex].Offset;
                            snap = (offsetDifference / timingPoints[timingPointListIndex].BPM) * resolution;
                            snapInt = Convert.ToInt32(snap);
                            if (snapInt % (resolution / 12) != 0 && snapInt % (resolution / 16) != 0)
                                errorOffsets.Add(notes[i].Offset);
                            timingPointListIndexTemp = timingPointListIndex;
                        }
                        errorOffsets.Distinct();
                        if(errorOffsets.Count > 0)
                        {
                            errorOffsets.Sort();
                            if (ShowMode.QuestionWithYesNo(language.LanguageContent[Language.askForMultipleErrorSave]) == DialogResult.Yes)
                            {
                                string savePath = basePath + "\\" + fileName + " - Errors.html";
                                if (!Directory.Exists(basePath))
                                {
                                    ShowMode.Information(language.LanguageContent[Language.errorSavePath] + basePath);
                                    Directory.CreateDirectory(basePath);
                                }
                                if (File.Exists(savePath))
                                {
                                    if(ShowMode.QuestionWithYesNo(language.LanguageContent[Language.overwriteBackup]) == DialogResult.Yes)
                                    {
                                        using (StreamWriter writer = File.CreateText(savePath))
                                        {
                                            writer.WriteLine("<!DOCTYPE html>");
                                            writer.WriteLine("<html>");
                                            writer.WriteLine("<head>");
                                            writer.WriteLine("<title>Errors</title>");
                                            writer.WriteLine("</head>");
                                            writer.WriteLine("<body>");
                                            for(int i = 0; i < errorOffsets.Count; i++)
                                                writer.WriteLine("<a style=\"text-size: 20px;\" href=\"osu://edit/" + FormatString.getFormattedTimeString(errorOffsets[i]) + "\">Error " + (i + 1).ToString() + " (" + FormatString.getFormattedTimeString(errorOffsets[i]) + ")</a></br>");
                                            writer.WriteLine("</body>");
                                            writer.WriteLine("</html>");
                                        }
                                        ShowMode.Information(language.LanguageContent[Language.errorSaved]);
                                        return null;
                                    }
                                }
                                else
                                {
                                    using (StreamWriter writer = File.CreateText(savePath))
                                    {
                                        writer.WriteLine("<!DOCTYPE html>");
                                        writer.WriteLine("<html>");
                                        writer.WriteLine("<head>");
                                        writer.WriteLine("<title>Errors</title>");
                                        writer.WriteLine("</head>");
                                        writer.WriteLine("<body>");
                                        for (int i = 0; i < errorOffsets.Count; i++)
                                            writer.WriteLine("<a style=\"text-size: 20px;\" href=\"osu://edit/" + FormatString.getFormattedTimeString(errorOffsets[i]) + "\">Error " + (i + 1).ToString() + " (" + FormatString.getFormattedTimeString(errorOffsets[i]) + ")</a></br>");
                                        writer.WriteLine("</body>");
                                        writer.WriteLine("</html>");
                                    }
                                    ShowMode.Information(language.LanguageContent[Language.errorSaved]);
                                    return null;
                                }
                            }
                        }
                        else // means there is no error
                        {
                            errorOffsets = new List<int>();
                            currentTimingPointOffset = timingPoints[0].Offset;
                            currentBPM = timingPoints[0].BPM;
                            initialSnap = 0;
                            for (int i = 1; i < timingPoints.Count; i++)
                            {
                                offsetDifference = timingPoints[i].Offset - currentTimingPointOffset;
                                snap = (offsetDifference / currentBPM) * resolution;
                                snapInt = initialSnap + Convert.ToInt32(snap);
                                timingPoints[i].SnapValue = snapInt;
                                if (timingPoints[i].Type == 1)
                                {
                                    currentBPM = timingPoints[i].BPM;
                                    currentTimingPointOffset = timingPoints[i].Offset;
                                    initialSnap = timingPoints[i].SnapValue;
                                }
                            }
                            initialSnap = 0;
                            timingPointListIndexTemp = 0;
                            for (int i = 1; i < notes.Count; i++)
                            {
                                for (int j = timingPoints.Count - 1; j >= 0; j--)
                                    if (timingPoints[j].Offset <= notes[i].Offset && timingPoints[j].Type == 1)
                                    {
                                        timingPointListIndex = j;
                                        initialSnap = timingPoints[j].SnapValue;
                                        break;
                                    }
                                offsetDifference = notes[i].Offset - timingPoints[timingPointListIndex].Offset;
                                snap = (offsetDifference / timingPoints[timingPointListIndex].BPM) * resolution;
                                snapInt = initialSnap + Convert.ToInt32(snap);
                                notes[i].SnapValue = snapInt;
                                if(notes[i].HasEndOffset())
                                {
                                    int tempIndex = -1;
                                    for (int j = timingPoints.Count - 1; j >= 0; j--)
                                        if (timingPoints[j].Offset <= notes[i].EndOffset && timingPoints[j].Type == 1)
                                        {
                                            tempIndex = j;
                                            break;
                                        }
                                    if (tempIndex != -1)
                                    {
                                        offsetDifference = notes[i].EndOffset - timingPoints[tempIndex].Offset;
                                        snap = (offsetDifference / timingPoints[tempIndex].BPM) * resolution;
                                        snapInt = initialSnap + Convert.ToInt32(snap);
                                        notes[i].EndSnapValue = snapInt;
                                    }
                                }
                                timingPointListIndexTemp = timingPointListIndex;
                            }
                            for (int i = 0; i < timingContent.Count; i++)
                            {
                                currentLine = timingContent[i];
                                offset = Convert.ToInt32(currentLine.Substring(0, currentLine.IndexOf(',')));
                                newTimingPoints.Add(new TimingPoints(offset, 1, currentLine));
                            }
                            TimingPoints obj1, obj2;
                            newTimingPoints[0].SnapValue = 0;
                            for (int i = 0; i + 1 < newTimingPoints.Count; i++)
                            {
                                obj1 = newTimingPoints[i];
                                obj2 = newTimingPoints[i + 1];
                                offsetDifference = obj2.Offset - obj1.Offset;
                                snap = (offsetDifference / obj1.BPM) * resolution;
                                snapInt = obj1.SnapValue + Convert.ToInt32(snap);
                                if (snapInt % (resolution / 12) != 0 && snapInt % (resolution / 16) != 0)
                                    errorOffsets.Add(obj2.Offset);
                                else
                                    obj2.SnapValue = snapInt;
                            }
                            if (errorOffsets.Count > 1)
                            {
                                if(ShowMode.QuestionWithYesNo(language.LanguageContent[Language.timingPointContentError]) == DialogResult.Yes)
                                {
                                    string savePath = basePath + "\\" + fileName + " - Errors.html";
                                    if (!Directory.Exists(basePath))
                                    {
                                        ShowMode.Information(language.LanguageContent[Language.errorSavePath] + basePath);
                                        Directory.CreateDirectory(basePath);
                                    }
                                    if (File.Exists(savePath))
                                    {
                                        if (ShowMode.QuestionWithYesNo(language.LanguageContent[Language.overwriteBackup]) == DialogResult.Yes)
                                        {
                                            using (StreamWriter writer = File.CreateText(savePath))
                                            {
                                                writer.WriteLine("<!DOCTYPE html>");
                                                writer.WriteLine("<html>");
                                                writer.WriteLine("<head>");
                                                writer.WriteLine("<title>Errors</title>");
                                                writer.WriteLine("</head>");
                                                writer.WriteLine("<body>");
                                                for (int i = 0; i < errorOffsets.Count; i++)
                                                    writer.WriteLine("<a style=\"text-size: 20px;\" href=\"osu://edit/" + FormatString.getFormattedTimeString(errorOffsets[i]) + "\">Error " + (i + 1).ToString() + " (" + FormatString.getFormattedTimeString(errorOffsets[i]) + ")</a></br>");
                                                writer.WriteLine("</body>");
                                                writer.WriteLine("</html>");
                                            }
                                            ShowMode.Information(language.LanguageContent[Language.errorSaved]);
                                            return null;
                                        }
                                    }
                                    else
                                    {
                                        using (StreamWriter writer = File.CreateText(savePath))
                                        {
                                            writer.WriteLine("<!DOCTYPE html>");
                                            writer.WriteLine("<html>");
                                            writer.WriteLine("<head>");
                                            writer.WriteLine("<title>Errors</title>");
                                            writer.WriteLine("</head>");
                                            writer.WriteLine("<body>");
                                            for (int i = 0; i < errorOffsets.Count; i++)
                                                writer.WriteLine("<a style=\"text-size: 20px;\" href=\"osu://edit/" + FormatString.getFormattedTimeString(errorOffsets[i]) + "\">Error " + (i + 1).ToString() + " (" + FormatString.getFormattedTimeString(errorOffsets[i]) + ")</a></br>");
                                            writer.WriteLine("</body>");
                                            writer.WriteLine("</html>");
                                        }
                                        ShowMode.Information(language.LanguageContent[Language.errorSaved]);
                                        return null;
                                    }
                                }
                            }
                            else // means the pasted timing point content don't have errors too
                            {
                                int snapDifference = -1, newOffset = -1;
                                timingPointListIndex = -1;
                                newTimingPoints[0].SnapValue = 0;
                                for (int i = 0; i + 1 < newTimingPoints.Count; i++)
                                {
                                    obj1 = newTimingPoints[i];
                                    obj2 = newTimingPoints[i + 1];
                                    offsetDifference = obj2.Offset - obj1.Offset;
                                    snap = (offsetDifference / obj1.BPM) * resolution;
                                    snapInt = obj1.SnapValue + Convert.ToInt32(snap);
                                    newTimingPoints[i + 1].SnapValue = snapInt;
                                }
                                for (int i = 0; i < timingPoints.Count; i++)
                                {
                                    if (timingPoints[i].Type == 0)
                                    {
                                        for(int j = newTimingPoints.Count - 1; j >= 0; j--)
                                        {
                                            if(newTimingPoints[j].SnapValue <= timingPoints[i].SnapValue)
                                            {
                                                timingPointListIndex = j;
                                                break;
                                            }
                                        }
                                        snapDifference = timingPoints[i].SnapValue - newTimingPoints[timingPointListIndex].SnapValue;
                                        newOffset = newTimingPoints[timingPointListIndex].Offset + (int)(newTimingPoints[timingPointListIndex].BPM * (snapDifference / resolution));
                                        timingPoints[i].SetNewOffsetToDataString(newOffset);
                                    }
                                }
                                snapDifference = -1; newOffset = -1;
                                for (int i = 0; i < notes.Count; i++)
                                {
                                    Notes currentNote = notes[i];
                                    for (int j = newTimingPoints.Count - 1; j >= 0; j--)
                                    {
                                        if (newTimingPoints[j].SnapValue <= currentNote.SnapValue)
                                        {
                                            timingPointListIndex = j;
                                            break;
                                        }
                                    }
                                    snapDifference = currentNote.SnapValue - newTimingPoints[timingPointListIndex].SnapValue;
                                    newOffset = newTimingPoints[timingPointListIndex].Offset + (int)(newTimingPoints[timingPointListIndex].BPM * (snapDifference / resolution));
                                    currentNote.SetNewOffsetToDataString(newOffset);
                                    if (currentNote.HasEndOffset())
                                    {
                                        for (int j = newTimingPoints.Count - 1; j >= 0; j--)
                                        {
                                            if (newTimingPoints[j].SnapValue <= currentNote.EndSnapValue)
                                            {
                                                timingPointListIndex = j;
                                                break;
                                            }
                                        }
                                        snapDifference = currentNote.EndSnapValue - newTimingPoints[timingPointListIndex].SnapValue;
                                        newOffset = newTimingPoints[timingPointListIndex].Offset + (int)(newTimingPoints[timingPointListIndex].BPM * (snapDifference / resolution));
                                        currentNote.SetNewSpinnerEndOffsetToDataString(newOffset);
                                    }
                                }
                                for (int i = 0; i < timingPoints.Count; i++) if (timingPoints[i].Type == 1) timingPoints.RemoveAt(i--);
                                for (int i = 0; i < newTimingPoints.Count; i++) timingPoints.Add(newTimingPoints[i]);
                                for (int i = 0; i < timingPoints.Count; i++)
                                    for (int j = 0; j < i; j++)
                                    {
                                        if (timingPoints[j].Offset > timingPoints[i].Offset)
                                        {
                                            TimingPoints temp = timingPoints[i];
                                            timingPoints[i] = timingPoints[j];
                                            timingPoints[j] = temp;
                                        }
                                        else if(timingPoints[j].Offset == timingPoints[i].Offset && timingPoints[j].Type == 0 && timingPoints[i].Type == 1)
                                        {
                                            TimingPoints temp = timingPoints[i];
                                            timingPoints[i] = timingPoints[j];
                                            timingPoints[j] = temp;
                                        }
                                    }
                                for (int i = 0; i < notes.Count; i++)
                                    for (int j = 0; j < i; j++)
                                        if (notes[j].Offset > notes[i].Offset)
                                        {
                                            Notes temp = notes[i];
                                            notes[i] = notes[j];
                                            notes[j] = temp;
                                        }
                                for (int i = timingPointsIndex + 1; !string.IsNullOrWhiteSpace(linesList[i]); i++)
                                    linesList.RemoveAt(i--);
                                for (int i = 0; i < timingPoints.Count; i++)
                                    linesList.Insert(timingPointsIndex + 1 + i, timingPoints[i].DataString);
                                for (int i = 0; i < linesList.Count; i++) if (linesList[i] == "[HitObjects]") { hitObjectsIndex = i; break; }
                                for (int i = hitObjectsIndex + 1; i < linesList.Count; i++)
                                    if (!string.IsNullOrWhiteSpace(linesList[i]))
                                        linesList.RemoveAt(i--);
                                for (int i = 0; i < notes.Count; i++)
                                    linesList.Insert(hitObjectsIndex + 1 + i, notes[i].DataString);
                            }
                        }
                    }
                    else
                    {
                        ShowMode.Error(language.LanguageContent[Language.firstPointShouldBeTiming]);
                        return null;
                    }
                }
            }
            return linesList.ToArray();
        }
        private bool isMapHaveBookmarks()
        {
            for (int i = 0; i < lines.Length; i++)
                if (lines[i].Contains("Bookmarks:") && lines[i] != "Bookmarks:")
                    return true;
            return false;
        }
        private void changeNoteCoordinates(int x, int y, int i)
        {
            string currentLine = lines[i];
            currentLine = currentLine.Remove(0, currentLine.IndexOfWithCount(',', 1) - 1);
            currentLine = currentLine.Insert(0, x.ToString());
            currentLine = currentLine.Remove(currentLine.IndexOfWithCount(',', 1), currentLine.IndexOfWithCount(',', 2) - currentLine.IndexOfWithCount(',', 1) - 1);
            currentLine = currentLine.Insert(currentLine.IndexOfWithCount(',', 1), y.ToString());
            lines[i] = currentLine;
        }
        private void editSVs(double bpm, double SV)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                List<String> linesList = lines.ToList();
                List<int> selectedTimes = new List<int>();
                for (int i = 0; i < dataGridView1.SelectedRows.Count; i++)
                {
                    string offsetString = dataGridView1.SelectedRows[i].Cells[0].Value.ToString();
                    selectedTimes.Add(Convert.ToInt32(offsetString.Substring(0, offsetString.IndexOf(' '))));
                }
                selectedTimes = selectedTimes.ToList();
                selectedTimes.Sort();
                int timingPointsIndex = -1, currentTime, selectedTimesIndex = selectedTimes.Count - 1, minimumTime = selectedTimes.Min(), maximumTime = selectedTimes.Max();
                double currentValue = 0, currentBPM = 0, calculatedSV;
                string currentTimingPointValues = null, currentLine = null;
                bool isSuccessful = true;
                for (int i = 0; ; i++) if (linesList[i] == "[TimingPoints]") { timingPointsIndex = i; break; }
                for (int i = timingPointsIndex + 1; i < linesList.Count; i++)
                {
                    if (selectedTimes.Count == 0)
                        break;
                    if (!string.IsNullOrWhiteSpace(linesList[i]))
                    {
                        currentLine = linesList[i];
                        currentTime = Convert.ToInt32(currentLine.Substring(0, currentLine.IndexOf(',')));
                        if (currentTime >= minimumTime && currentTime <= maximumTime)
                        {
                            if(selectedTimes.Contains(currentTime))
                            {
                                int mode = Convert.ToInt32(currentLine.Substring(currentLine.IndexOfWithCount(',', 6),
                                    currentLine.IndexOfWithCount(',', 7) - currentLine.IndexOfWithCount(',', 6) - 1));
                                if (mode == 1)
                                {
                                    currentValue = Convert.ToDouble(currentLine.Substring(
                                         currentLine.IndexOfWithCount(',', 1),
                                         currentLine.IndexOfWithCount(',', 2) - currentLine.IndexOfWithCount(',', 1) - 1).Replace('.', ','));
                                    currentBPM = 60000 / currentValue;
                                }
                                else
                                {
                                    currentTimingPointValues = currentLine.Substring(
                                             linesList[i].IndexOfWithCount(',', 2));
                                    if (currentBPM != 0)
                                        calculatedSV = (-100 / SV) * (currentBPM / bpm);
                                    else
                                    {
                                        ShowMode.Error(language.LanguageContent[Language.onInheritedChangeTimingNotFound]);
                                        isSuccessful = false;
                                        break;
                                    }
                                    currentLine = currentLine.Remove(currentLine.IndexOfWithCount(',', 1),
                                    currentLine.IndexOfWithCount(',', 2) - currentLine.IndexOfWithCount(',', 1) - 1);
                                    currentLine = currentLine.Insert(currentLine.IndexOfWithCount(',', 1), calculatedSV.ToString().Replace(',', '.'));
                                    linesList[i] = currentLine;
                                }
                                selectedTimes.Remove(currentTime);
                            }
                        }
                    }
                }
                if (isSuccessful)
                {
                    if(ShowMode.QuestionWithYesNo(language.LanguageContent[Language.askBackup]) == DialogResult.Yes)
                    {
                        File.Copy(path, Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + fileName, true);
                        ShowMode.Information(language.LanguageContent[Language.processCompleteWithBackup]);
                    }
                    else
                        ShowMode.Information(language.LanguageContent[Language.processComplete]);
                    File.WriteAllLines(path, linesList.ToArray());
                    manageLoad();
                }
            }
            else
                ShowMode.Error(language.LanguageContent[Language.replyFromEdit]);
        }
        private void addSVs(double bpm)
        {
            List<String> linesList = lines.ToList();
            int timingPointsIndex = -1, currentTime;
            double currentValue, currentBPM, calculatedSV;
            string currentTimingPointValues = null;
            bool isGreenPointSpotted = false;
            for (int i = 0; ; i++) if (linesList[i] == "[TimingPoints]") { timingPointsIndex = i; break; }
            for (int i = timingPointsIndex + 1; !string.IsNullOrWhiteSpace(linesList[i]); i++)
            {
                if (linesList[i].Substring(linesList[i].IndexOfWithCount(',', 6), 1) == "0")
                {
                    ShowMode.Error(language.LanguageContent[Language.containsGreenPoints]);
                    isGreenPointSpotted = true;
                    break;
                }
                else
                {
                    currentTime = Convert.ToInt32(linesList[i].Substring(0, linesList[i].IndexOf(',')));
                    currentValue = Convert.ToDouble(linesList[i].Substring(
                    linesList[i].IndexOfWithCount(',', 1),
                    linesList[i].IndexOfWithCount(',', 2) - linesList[i].IndexOfWithCount(',', 1) - 1).Replace('.', ','));
                    currentTimingPointValues = linesList[i].Substring(
                        linesList[i].IndexOfWithCount(',', 2));
                    currentBPM = 60000 / currentValue;
                    calculatedSV = (-100) * (currentBPM / bpm);
                    currentTimingPointValues = currentTimingPointValues.Remove(currentTimingPointValues.IndexOfWithCount(',', 4), 1);
                    currentTimingPointValues = currentTimingPointValues.Insert(currentTimingPointValues.IndexOfWithCount(',', 4), "0");
                    linesList.Insert(i + 1, currentTime.ToString() + "," + calculatedSV.ToString().Replace(',', '.') + "," + currentTimingPointValues);
                    i++;
                }
            }
            if (!isGreenPointSpotted)
            {
                lines = linesList.ToArray();
                File.WriteAllLines(path, lines);
                ShowMode.Information(language.LanguageContent[Language.processComplete]);
                manageLoad();
            }
        }
        private void UndoFunction()
        {
            if (timer1.Enabled) timer1.Stop();
            Undo();
            if (!timer1.Enabled) timer1.Start();
        }
        private void RedoFunction()
        {
            if (timer1.Enabled) timer1.Stop();
            Redo();
            if (!timer1.Enabled) timer1.Start();
        }
        private void SaveFileFunction()
        {
            if (timer1.Enabled) timer1.Stop();
            SaveFile();
            if (!timer1.Enabled) timer1.Start();
        }
        private void Options()
        {
            if (timer1.Enabled) timer1.Stop();
            Options options = new Options();
            options.ShowDialog();
            if (options.IsLanguageChanged)
                ChangeControlTexts();
            if (!timer1.Enabled) timer1.Start();
        }
        private void FilterInherited()
        {
            dataGridView1.ClearSelection();
            CurrencyManager currencyManager1 = (CurrencyManager)BindingContext[dataGridView1.DataSource];
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                currencyManager1.SuspendBinding();
                if (dataGridView1.Rows[i].Cells[1].Value.ToString().Contains("x"))
                    dataGridView1.Rows[i].Visible = true;
                else
                    dataGridView1.Rows[i].Visible = false;
            }
            currencyManager1.ResumeBinding();
            int firstVisibleIndex;
            for (firstVisibleIndex = 0; firstVisibleIndex < dataGridView1.Rows.Count; firstVisibleIndex++)
            {
                if (dataGridView1.Rows[firstVisibleIndex].Visible)
                    break;
            }
            if (firstVisibleIndex != dataGridView1.Rows.Count)
                dataGridView1.FirstDisplayedScrollingRowIndex = firstVisibleIndex;
            dataGridView1.Focus();
        }
        private void FilterTiming()
        {
            dataGridView1.ClearSelection();
            CurrencyManager currencyManager1 = (CurrencyManager)BindingContext[dataGridView1.DataSource];
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                currencyManager1.SuspendBinding();
                if (dataGridView1.Rows[i].Cells[1].Value.ToString().Contains("x"))
                    dataGridView1.Rows[i].Visible = false;
                else
                    dataGridView1.Rows[i].Visible = true;
            }
            currencyManager1.ResumeBinding();
            int firstVisibleIndex;
            for (firstVisibleIndex = 0; firstVisibleIndex < dataGridView1.Rows.Count; firstVisibleIndex++)
            {
                if (dataGridView1.Rows[firstVisibleIndex].Visible)
                    break;
            }
            if (firstVisibleIndex != dataGridView1.Rows.Count)
                dataGridView1.FirstDisplayedScrollingRowIndex = firstVisibleIndex;
            dataGridView1.Focus();
        }
        private void RemoveFilter()
        {
            dataGridView1.ClearSelection();
            CurrencyManager currencyManager1 = (CurrencyManager)BindingContext[dataGridView1.DataSource];
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                currencyManager1.SuspendBinding();
                dataGridView1.Rows[i].Visible = true;
            }
            currencyManager1.ResumeBinding();
            int firstVisibleIndex;
            for (firstVisibleIndex = 0; firstVisibleIndex < dataGridView1.Rows.Count; firstVisibleIndex++)
            {
                if (dataGridView1.Rows[firstVisibleIndex].Visible)
                    break;
            }
            if (firstVisibleIndex != dataGridView1.Rows.Count)
                dataGridView1.FirstDisplayedScrollingRowIndex = firstVisibleIndex;
            dataGridView1.Focus();
        }
        #endregion
        #region Combo box functions
        private void PrepareMapToHitsounding()
        {
            if (timer1.Enabled) timer1.Stop();
            int hitObjectsIndex = -1, timingPointsIndex = -1;
            double sliderMultiplier = 0;
            for (int i = 0; i < lines.Length; i++) if (lines[i].Contains("SliderMultiplier") && lines[i] != "SliderMultiplier") { sliderMultiplier = Convert.ToDouble(lines[i].Substring(lines[i].IndexOf(':') + 1).Replace('.', ',')); break; }
            for (int i = 0; i < lines.Length; i++) if (lines[i] == "[TimingPoints]") { timingPointsIndex = i; break; }
            for (int i = 0; i < lines.Length; i++) if (lines[i] == "[HitObjects]") { hitObjectsIndex = i; break; }
            if (timingPointsIndex == -1 || hitObjectsIndex == -1)
                ShowMode.Error(language.LanguageContent[Language.noTimingPointsOrNotes]);
            else if (sliderMultiplier == 0)
                ShowMode.Error(language.LanguageContent[Language.noSliderMultiplierFound]);
            else if (ShowMode.QuestionWithYesNo(language.LanguageContent[Language.areYouSure]) == DialogResult.Yes)
            {
                AddBackup();
                List<string> linesList = lines.ToList();
                List<Notes> hitObjectOffsets = new List<Notes>();
                List<TimingPoints> oldTimingPoints = new List<TimingPoints>();
                List<TimingPoints> newTimingPoints = new List<TimingPoints>();
                for (int i = timingPointsIndex + 1; i < linesList.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(linesList[i]))
                    {
                        string currentLine = linesList[i];
                        oldTimingPoints.Add(new TimingPoints(
                                Convert.ToInt32(currentLine.Substring(0, currentLine.IndexOf(','))),
                                Convert.ToInt32(currentLine.Substring(currentLine.IndexOfWithCount(',', 6), 1)),
                                currentLine
                            ));
                    }
                    else
                        break;
                }
                for (int i = 0; i < oldTimingPoints.Count; i++)
                    for (int j = 0; j < i; j++)
                        if (oldTimingPoints[j].Offset > oldTimingPoints[i].Offset)
                        {
                            TimingPoints temp = oldTimingPoints[i];
                            oldTimingPoints[i] = oldTimingPoints[j];
                            oldTimingPoints[j] = temp;
                        }
                for (int i = hitObjectsIndex + 1; i < linesList.Count; i++)
                {
                    string currentLine = linesList[i];
                    if (!string.IsNullOrWhiteSpace(currentLine))
                    {
                        hitObjectOffsets.Add(new Notes((int)(Convert.ToDouble(currentLine.Substring(currentLine.IndexOfWithCount(',', 2), currentLine.IndexOfWithCount(',', 3) - currentLine.IndexOfWithCount(',', 2) - 1).Replace('.', ','))), currentLine));
                        Notes lastHitObject = hitObjectOffsets[hitObjectOffsets.Count - 1];
                        if (lastHitObject.IsSlider)
                        {
                            double beatDuration = 0, sliderVelocity = 0;
                            for (int j = oldTimingPoints.Count - 1; j >= 0; j--)
                                if(lastHitObject.Offset >= oldTimingPoints[j].Offset && oldTimingPoints[j].Type == 1)
                                {
                                    beatDuration = oldTimingPoints[j].BPM;
                                    break;
                                }
                            for (int j = oldTimingPoints.Count - 1; j >= 0; j--)
                                if (lastHitObject.Offset >= oldTimingPoints[j].Offset && oldTimingPoints[j].Type == 0)
                                {
                                    sliderVelocity = oldTimingPoints[j].SliderVelocity;
                                    break;
                                }
                            if (sliderVelocity == 0)
                                sliderVelocity = 1;
                            if (beatDuration != 0)
                                lastHitObject.SetEndOffset(currentLine, beatDuration, sliderVelocity, sliderMultiplier);
                            else
                                ShowMode.Error("Data returned wrong. Terminate program from executing from Task Manager or it might register wrong values.");
                        }
                    }
                }
                for (int i = 0; i + 1 < hitObjectOffsets.Count; i++)
                {
                    int pointOffset;
                    if (hitObjectOffsets[i].IsSlider || hitObjectOffsets[i].IsSpinner)
                        pointOffset = hitObjectOffsets[i].EndOffset + ((hitObjectOffsets[i + 1].Offset - hitObjectOffsets[i].EndOffset) / 2);
                    else
                        pointOffset = hitObjectOffsets[i].Offset + ((hitObjectOffsets[i + 1].Offset - hitObjectOffsets[i].Offset) / 2);
                    for (int j = oldTimingPoints.Count - 1; j >= 0; j--)
                        if (pointOffset >= oldTimingPoints[j].Offset)
                        {
                            string dataString = oldTimingPoints[j].DataString;
                            if (oldTimingPoints[j].Type == 1)
                            {
                                dataString = dataString.Remove(dataString.IndexOfWithCount(',', 1), dataString.IndexOfWithCount(',', 2) - dataString.IndexOfWithCount(',', 1) - 1);
                                dataString = dataString.Remove(dataString.IndexOfWithCount(',', 6), 1);
                                dataString = dataString.Insert(dataString.IndexOfWithCount(',', 1), "-100");
                                dataString = dataString.Insert(dataString.IndexOfWithCount(',', 6), "0");
                            }
                            newTimingPoints.Add(new TimingPoints(pointOffset, 0, dataString));
                            newTimingPoints[newTimingPoints.Count - 1].SetNewOffsetToDataString(pointOffset);
                            break;
                        }
                }
                oldTimingPoints.AddRange(newTimingPoints);
                for (int i = 0; i < oldTimingPoints.Count; i++)
                    for (int j = 0; j < i; j++)
                        if (oldTimingPoints[j].Offset > oldTimingPoints[i].Offset)
                        {
                            TimingPoints temp = oldTimingPoints[i];
                            oldTimingPoints[i] = oldTimingPoints[j];
                            oldTimingPoints[j] = temp;
                        }
                for (int i = 0; i < oldTimingPoints.Count; i++)
                    for (int j = 0; j < i; j++)
                        if (oldTimingPoints[j].Offset == oldTimingPoints[i].Offset && oldTimingPoints[j].Type == 0 && oldTimingPoints[i].Type == 1)
                        {
                            TimingPoints temp = oldTimingPoints[i];
                            oldTimingPoints[i] = oldTimingPoints[j];
                            oldTimingPoints[j] = temp;
                        }
                for (int i = timingPointsIndex + 1; !string.IsNullOrWhiteSpace(linesList[i]) && i < linesList.Count; i++)
                    linesList.RemoveAt(i--);
                for (int i = 0; i < oldTimingPoints.Count; i++)
                    linesList.Insert(timingPointsIndex + 1 + i, oldTimingPoints[i].DataString);
                File.WriteAllLines(path, linesList.ToArray());
                ShowMode.Information(language.LanguageContent[Language.processComplete]);
                manageLoad();
            }
            if (!timer1.Enabled) timer1.Start();
        }
        private void ChangeBPM()
        {
            if (timer1.Enabled) timer1.Stop();
            if (dataGridView1.SelectedRows.Count != 1)
                ShowMode.Error(language.LanguageContent[Language.oneRowOnly]);
            else
            {
                if (dataGridView1.SelectedRows[0].Cells[1].Value.ToString().Contains("x"))
                    ShowMode.Error(language.LanguageContent[Language.inheritedPointBPMError]);
                else
                {
                    string offsetString = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();
                    BPM_Changer BPM = new BPM_Changer();
                    BPM_Changer.Offset = Convert.ToInt32(offsetString.Substring(0, offsetString.IndexOf(' ')));
                    BPM.checkBox1.Enabled = isMapHaveBookmarks();
                    BPM.ShowDialog();
                    if (BPM_Changer.value != 0)
                    {
                        if (ShowMode.QuestionWithYesNo(language.LanguageContent[Language.offsetShift]) == DialogResult.Yes)
                        {
                            AddBackup();
                            linesList = lines.ToList();
                            if (BPM_Changer.ComboBoxSelectedIndex == 1)
                            {
                                ShowMode.Information(language.LanguageContent[Language.multiSelection]);
                                OpenFileDialog dialog = new OpenFileDialog();
                                dialog.InitialDirectory = path.Substring(0, path.LastIndexOf('\\'));
                                dialog.Multiselect = true;
                                dialog.Title = language.LanguageContent[Language.selectFiles];
                                dialog.Filter = language.LanguageContent[Language.osuFiles] + " (*.osu,*.OSU) | *.osu;*.OSU";
                                if (dialog.ShowDialog() == DialogResult.OK)
                                {
                                    string[] paths = dialog.FileNames;
                                    string[] fileNames = dialog.SafeFileNames;
                                    string[] lines;
                                    if (ShowMode.QuestionWithYesNo(language.LanguageContent[Language.multipleFileBackups]) == DialogResult.Yes)
                                        saveBackups(paths, fileNames);
                                    for (int i = 0; i < paths.Length; i++)
                                    {
                                        lines = File.ReadAllLines(paths[i]);
                                        if (BPM.checkBox1.Enabled)
                                            lines = ChangeBookmarks_BPM(lines);
                                        lines = SetTimingOffsetsAndNewBpm(lines);
                                        lines = SetNewHitObjectOffsets(lines);
                                        WriteNewFile(paths[i], fileNames[i], lines);
                                    }
                                    ShowMode.Information(language.LanguageContent[Language.processComplete]);
                                }
                                else
                                {
                                    ShowMode.Error(language.LanguageContent[Language.noFilesSelected]);
                                    if (!timer1.Enabled) timer1.Start();
                                    return;
                                }
                            }
                            else
                            {
                                if (BPM.checkBox1.Enabled)
                                    ChangeBookmarks_BPM();
                                SetTimingOffsetsAndNewBpm();
                                SetNewHitObjectOffsets();
                                WriteNewFile();
                            }
                            linesList.Clear();
                            manageLoad();
                        }
                    }
                }
            }
            if (!timer1.Enabled) timer1.Start();
        }
        private void ChangeOffset()
        {
            if (timer1.Enabled) timer1.Stop();
            RemoveInvisibleSelection();
            if (dataGridView1.SelectedRows.Count == 0)
                ShowMode.Error(language.LanguageContent[Language.oneRowRequired]);
            else
            {
                if (!dataGridView1.SelectedRows.Contains(dataGridView1.Rows[dataGridView1.Rows.Count - 1]))
                {
                    DialogResult res = ShowMode.QuestionWithYesNo(language.LanguageContent[Language.selectPointsWithLast]);
                    if (res == DialogResult.Yes)
                    {
                        BPM_Changer form = new BPM_Changer();
                        form.label1.Text = language.LanguageContent[Language.offsetChange];
                        form.label1.Location = new Point(133 - form.label1.Size.Width, form.label1.Location.Y);
                        form.Text = language.LanguageContent[Language.offsetChanger];
                        form.checkBox1.Enabled = isMapHaveBookmarks();
                        form.ShowDialog();
                        if (BPM_Changer.ComboBoxSelectedIndex == -1)
                        {
                            ShowMode.Warning(language.LanguageContent[Language.noFunctionSelected]);
                            return;
                        }
                        if (BPM_Changer.value != 0)
                        {
                            if (BPM_Changer.ComboBoxSelectedIndex == 1)
                            {
                                ShowMode.Information(language.LanguageContent[Language.multiSelection]);
                                OpenFileDialog dialog = new OpenFileDialog();
                                dialog.InitialDirectory = path.Substring(0, path.LastIndexOf('\\'));
                                dialog.Multiselect = true;
                                dialog.Title = language.LanguageContent[Language.selectFiles];
                                dialog.Filter = language.LanguageContent[Language.osuFiles] + " (*.osu,*.OSU) | *.osu;*.OSU";
                                if (dialog.ShowDialog() == DialogResult.OK)
                                {
                                    AddBackup();
                                    string[] paths = dialog.FileNames;
                                    string[] fileNames = dialog.SafeFileNames;
                                    string[] lines;
                                    if (ShowMode.QuestionWithYesNo(language.LanguageContent[Language.multipleFileBackups]) == DialogResult.Yes)
                                        saveBackups(paths, fileNames);
                                    for (int i = 0; i < paths.Length; i++)
                                    {
                                        lines = File.ReadAllLines(paths[i]);
                                        lines = ChangeOffsets(paths[i], lines);
                                        if (form.checkBox1.Checked)
                                            lines = ChangeBookmarks_Offset(lines);
                                        File.WriteAllLines(paths[i], lines);
                                    }
                                }
                                else
                                {
                                    ShowMode.Error(language.LanguageContent[Language.noFilesSelected]);
                                    if (!timer1.Enabled) timer1.Start();
                                    return;
                                }
                            }
                            else
                            {
                                AddBackup();
                                ChangeOffsets();
                                if (form.checkBox1.Checked)
                                    ChangeBookmarks_Offset();
                                File.WriteAllLines(path, lines);
                            }
                            ShowMode.Information(language.LanguageContent[Language.processComplete]);
                            ShowMode.Warning(language.LanguageContent[Language.overlappedTimingPointsWarning]);
                            manageLoad();
                        }
                    }
                }
                else
                {
                    BPM_Changer form = new BPM_Changer();
                    form.label1.Text = language.LanguageContent[Language.offsetChange];
                    form.label1.Location = new Point(133 - form.label1.Size.Width, form.label1.Location.Y);
                    form.Text = language.LanguageContent[Language.offsetChanger];
                    form.ShowDialog();
                    if (BPM_Changer.ComboBoxSelectedIndex == -1)
                    {
                        ShowMode.Warning(language.LanguageContent[Language.noFunctionSelected]);
                        return;
                    }
                    if (BPM_Changer.value != 0)
                    {
                        if (BPM_Changer.ComboBoxSelectedIndex == 1)
                        {
                            ShowMode.Information(language.LanguageContent[Language.multiSelection]);
                            OpenFileDialog dialog = new OpenFileDialog();
                            dialog.InitialDirectory = path.Substring(0, path.LastIndexOf('\\'));
                            dialog.Multiselect = true;
                            dialog.Title = language.LanguageContent[Language.selectFiles];
                            dialog.Filter = language.LanguageContent[Language.osuFiles] + " (*.osu,*.OSU) | *.osu;*.OSU";
                            if (dialog.ShowDialog() == DialogResult.OK)
                            {
                                AddBackup();
                                string[] paths = dialog.FileNames;
                                string[] fileNames = dialog.SafeFileNames;
                                string[] lines;
                                if (ShowMode.QuestionWithYesNo(language.LanguageContent[Language.multipleFileBackups]) == DialogResult.Yes)
                                    saveBackups(paths, fileNames);
                                for (int i = 0; i < paths.Length; i++)
                                {
                                    lines = File.ReadAllLines(paths[i]);
                                    ChangeOffsets(paths[i], lines);
                                    if (form.checkBox1.Checked)
                                        ChangeBookmarks_Offset(lines);
                                    File.WriteAllLines(paths[i], lines);
                                }
                            }
                            else
                            {
                                ShowMode.Error(language.LanguageContent[Language.noFilesSelected]);
                                if (!timer1.Enabled) timer1.Start();
                                return;
                            }
                        }
                        else
                        {
                            AddBackup();
                            ChangeOffsets();
                            if (form.checkBox1.Checked)
                                ChangeBookmarks_Offset();
                            File.WriteAllLines(path, lines);
                        }
                        ShowMode.Information(language.LanguageContent[Language.processComplete]);
                        manageLoad();
                    }
                }
            }
            if (!timer1.Enabled) timer1.Start();
        }
        private void SelectFile()
        {
            if (timer1.Enabled) timer1.Stop();
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = language.LanguageContent[Language.osuFiles] + " (*.osu,*.OSU) | *.osu;*.OSU";
            if (file.ShowDialog() == DialogResult.OK)
            {
                path = file.FileName;
                fileName = file.SafeFileName;
                lines = File.ReadAllLines(path);
            }
            else
            {
                path = string.Empty;
                fileName = string.Empty;
            }
            undo.Clear();
            redo.Clear();
            manageLoad();
            if (!timer1.Enabled) timer1.Start();
        }
        private void DeleteSelectedInheritedPoints()
        {
            if (timer1.Enabled) timer1.Stop();
            RemoveInvisibleSelection();
            if (dataGridView1.SelectedRows.Count == 0)
                ShowMode.Error(language.LanguageContent[Language.oneRowRequired]);
            else
            {
                for (int i = 0; i < dataGridView1.SelectedRows.Count; i++)
                {
                    if (!dataGridView1.SelectedRows[i].Cells[1].Value.ToString().Contains("x"))
                    {
                        ShowMode.Error(language.LanguageContent[Language.onlyInheritedPoint]);
                        if (!timer1.Enabled) timer1.Start();
                        return;
                    }
                }
                if (ShowMode.QuestionWithYesNo(language.LanguageContent[Language.areYouSure]) == DialogResult.Yes)
                {
                    AddBackup();
                    List<int> selectedTimes = new List<int>();
                    List<string> lines = this.lines.ToList();
                    int timingPointsIndex = -1, currentTime, mode;
                    string currentString;
                    for (int i = 0; i < dataGridView1.SelectedRows.Count; i++)
                    {
                        currentString = dataGridView1.SelectedRows[i].Cells[0].Value.ToString();
                        selectedTimes.Add(Convert.ToInt32(currentString.Substring(0, currentString.IndexOf(' '))));
                    }
                    selectedTimes.Sort();
                    for (int i = 0; i < lines.Count; i++)
                        if (lines[i] == "[TimingPoints]")
                        {
                            timingPointsIndex = i; break;
                        }
                    for (int i = timingPointsIndex + 1; !string.IsNullOrWhiteSpace(lines[i]); i++)
                    {
                        currentString = lines[i];
                        currentTime = Convert.ToInt32(currentString.Substring(0, currentString.IndexOf(',')));
                        mode = Convert.ToInt32(currentString.Substring(currentString.IndexOfWithCount(',', 6), 1));
                        if (selectedTimes.Contains(currentTime) && mode == 0)
                            lines.RemoveAt(i--);
                    }
                    File.WriteAllLines(path, lines.ToArray());
                    ShowMode.Information(language.LanguageContent[Language.processComplete]);
                    manageLoad();
                }
            }
            if (!timer1.Enabled) timer1.Start();
        }
        private void WhistleToClap()
        {
            if (timer1.Enabled) timer1.Stop();
            if (ShowMode.QuestionWithYesNo(language.LanguageContent[Language.areYouSure]) == DialogResult.Yes)
            {
                AddBackup();
                int index;
                int whistle = 2, clap = 8, finisher = 4;
                for (index = 0; lines[index] != "[HitObjects]"; index++) { }
                for (int i = index + 1; i < lines.Length; i++)
                {
                    if (lines[i].Contains(',') == false) continue;
                    int type = Convert.ToInt32(lines[i].Substring(lines[i].IndexOfWithCount(',', 4), lines[i].IndexOfWithCount(',', 5) - lines[i].IndexOfWithCount(',', 4) - 1));
                    if (type == whistle)
                    {
                        lines[i] = lines[i].Remove(lines[i].IndexOfWithCount(',', 4), lines[i].IndexOfWithCount(',', 5) - lines[i].IndexOfWithCount(',', 4) - 1);
                        lines[i] = lines[i].Insert(lines[i].IndexOfWithCount(',', 4), clap.ToString());
                    }
                    else if (type == whistle + finisher)
                    {
                        lines[i] = lines[i].Remove(lines[i].IndexOfWithCount(',', 4), lines[i].IndexOfWithCount(',', 5) - lines[i].IndexOfWithCount(',', 4) - 1);
                        lines[i] = lines[i].Insert(lines[i].IndexOfWithCount(',', 4), (clap + finisher).ToString());
                    }
                }
                File.WriteAllLines(path, lines);
                ShowMode.Information(language.LanguageContent[Language.processComplete]);
                manageLoad();
            }
            if (!timer1.Enabled) timer1.Start();
        }
        private void PositionNotes()
        {
            if (timer1.Enabled) timer1.Stop();
            string mode = string.Empty;
            for (int i = 0; i < lines.Length; i++)
                if (lines[i].Contains("Mode:")) { mode = lines[i]; break; }
            if (mode[mode.Length - 1] == '1')
            {
                if (ShowMode.QuestionWithYesNo(language.LanguageContent[Language.areYouSure]) == DialogResult.Yes)
                {
                    AddBackup();
                    int donX = 160;
                    int donY = 96;
                    int katX = 352;
                    int katY = 96;
                    int donWithFinisherX = 160;
                    int donWithFinisherY = 288;
                    int katWithFinisherX = 352;
                    int katWithFinisherY = 288;
                    int kat = 8;
                    int finisher = 4;
                    int don = 0;
                    for (int i = lines.Length - 1; lines[i] != "[HitObjects]"; i--)
                    {
                        if (lines[i].Contains(',') == false || lines[i].Any(char.IsLetter)) continue;
                        int type = Convert.ToInt32(lines[i].Substring(lines[i].IndexOfWithCount(',', 4), lines[i].IndexOfWithCount(',', 5) - lines[i].IndexOfWithCount(',', 4) - 1));
                        if (type == don)
                        {
                            changeNoteCoordinates(donX, donY, i);
                        }
                        else if (type == kat)
                        {
                            changeNoteCoordinates(katX, katY, i);
                        }
                        else if (type == don + finisher)
                        {
                            changeNoteCoordinates(donWithFinisherX, donWithFinisherY, i);
                        }
                        else if (type == kat + finisher)
                        {
                            changeNoteCoordinates(katWithFinisherX, katWithFinisherY, i);
                        }
                    }
                    File.WriteAllLines(path, lines);
                    ShowMode.Information(language.LanguageContent[Language.processComplete]);
                    manageLoad();
                }
            }
            else
                ShowMode.Error(language.LanguageContent[Language.notTaiko]);
            if (!timer1.Enabled) timer1.Start();
        }
        private void NewTiming()
        {
            if (timer1.Enabled) timer1.Stop();
            Timing_Manager manager = new Timing_Manager();
            manager.ShowDialog();
            if(manager.isValid && manager.timingContent.Count != 0)
            {
                if(ShowMode.QuestionWithYesNo(language.LanguageContent[Language.rememberSnappingNotes]) == DialogResult.Yes)
                {
                    if(manager.comboBox.SelectedIndex == 1)
                    {
                        ShowMode.Information(language.LanguageContent[Language.multiSelection]);
                        OpenFileDialog dialog = new OpenFileDialog();
                        dialog.InitialDirectory = path.Substring(0, path.LastIndexOf('\\'));
                        dialog.Multiselect = true;
                        dialog.Title = language.LanguageContent[Language.selectFiles];
                        dialog.Filter = language.LanguageContent[Language.osuFiles] + " (*.osu,*.OSU) | *.osu;*.OSU";
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            AddBackup();
                            string[] paths = dialog.FileNames;
                            string[] fileNames = dialog.SafeFileNames;
                            string[] lines;
                            int adjustedFileCount = 0;
                            if (ShowMode.QuestionWithYesNo(language.LanguageContent[Language.multipleFileBackups]) == DialogResult.Yes)
                                saveBackups(paths, fileNames);
                            for (int i = 0; i < paths.Length; i++)
                            {
                                lines = File.ReadAllLines(paths[i]);
                                lines = ChangeAllSnaps(lines, manager.timingContent, Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + fileNames[i]);
                                if(lines != null)
                                {
                                    File.WriteAllLines(paths[i], lines);
                                    adjustedFileCount++;
                                }
                            }
                            if (adjustedFileCount != fileNames.Length)
                                ShowMode.Warning(language.LanguageContent[Language.partiallyComplete]);
                            else
                                ShowMode.Information(language.LanguageContent[Language.processComplete]);
                            manageLoad();
                        }
                        else
                            ShowMode.Error(language.LanguageContent[Language.noFilesSelected]);
                    }
                    else
                    {
                        AddBackup();
                        string[] lines = File.ReadAllLines(path);
                        lines = ChangeAllSnaps(lines, manager.timingContent, Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                        if(lines != null)
                        {
                            File.WriteAllLines(path, lines);
                            ShowMode.Information(language.LanguageContent[Language.processComplete]);
                            manageLoad();
                        }
                    }
                }
            }
            if (!timer1.Enabled) timer1.Start();
        }
        private void SVchanger()
        {
            if (timer1.Enabled) timer1.Stop();
            RemoveInvisibleSelection();
            if (dataGridView1.SelectedRows.Count == 0)
                ShowMode.Error(language.LanguageContent[Language.oneRowRequired]);
            else
            {
                for (int i = 0; i < dataGridView1.SelectedRows.Count; i++)
                {
                    if (!dataGridView1.SelectedRows[i].Cells[1].Value.ToString().Contains('x'))
                    {
                        ShowMode.Error(language.LanguageContent[Language.onlyInheritedPoint]);
                        return;
                    }
                }
                BPM_Changer form = new BPM_Changer();
                form.label2.Dispose();
                form.comboBox1.Dispose();
                form.MinimumSize = new Size(form.Size.Width, form.Size.Height - 50);
                form.Size = new Size(form.Size.Width, form.Size.Height - 50);
                form.label1.Text = language.LanguageContent[Language.sliderVelocityChange];
                form.label1.Location = new Point(133 - form.label1.Size.Width, form.label1.Location.Y);
                form.Text = language.LanguageContent[Language.sliderVelocityChanger];
                form.checkBox1.Dispose();
                form.ShowDialog();
                if (BPM_Changer.value != 0)
                {
                    AddBackup();
                    for (int i = 0; i < dataGridView1.SelectedRows.Count; i++)
                    {
                        double value = Convert.ToDouble(dataGridView1.SelectedRows[i].Cells[1].Value.ToString().Substring(0, dataGridView1.SelectedRows[i].Cells[1].Value.ToString().IndexOf('x')));
                        value += BPM_Changer.value;
                        value = -(100 / value);
                        string offsetString = dataGridView1.SelectedRows[i].Cells[0].Value.ToString();
                        int data = Convert.ToInt32(offsetString.Substring(0, offsetString.IndexOf(' ')));
                        for (int j = timingPointsIndex + 1; j < lines.Length && !string.IsNullOrWhiteSpace(lines[j]); j++)
                        {
                            int time = Convert.ToInt32(lines[j].Substring(0, lines[j].IndexOf(',')));
                            if (data == time && lines[j].Substring(lines[j].IndexOfWithCount(',', 6), 1) != "1")
                            {
                                lines[j] = lines[j].Remove(lines[j].IndexOfWithCount(',', 1), lines[j].IndexOfWithCount(',', 2) - lines[j].IndexOfWithCount(',', 1) - 1);
                                lines[j] = lines[j].Insert(lines[j].IndexOfWithCount(',', 1), value.ToString().Replace(',', '.'));
                                break;
                            }
                        }
                    }
                    File.WriteAllLines(path, lines);
                    ShowMode.Information(language.LanguageContent[Language.processComplete]);
                    manageLoad();
                }
            }
            if (!timer1.Enabled) timer1.Start();
        }
        private void Hitsounds()
        {
            if (timer1.Enabled) timer1.Stop();
            Hitsounds obj;
            if (path == string.Empty)
                obj = new Hitsounds(string.Empty, timer1);
            else
                obj = new Hitsounds(path.Substring(path.LastIndexOf("\\")), timer1);
            Point location = DesktopLocation;
            obj.StartPosition = FormStartPosition.Manual;
            obj.Show(this);
            Hide();
        }
        private void SVstepbystep()
        {
            if (timer1.Enabled) timer1.Stop();
            RemoveInvisibleSelection();
            if (dataGridView1.SelectedRows.Count == 0)
                ShowMode.Error(language.LanguageContent[Language.oneRowRequired]);
            else
            {
                for (int i = 0; i < dataGridView1.SelectedRows.Count; i++)
                {
                    if (!dataGridView1.SelectedRows[i].Cells[1].Value.ToString().Contains('x'))
                    {
                        ShowMode.Error(language.LanguageContent[Language.onlyInheritedPoint]);
                        if (!timer1.Enabled) timer1.Start();
                        return;
                    }
                }
                BPM_Changer form = new BPM_Changer();
                form.label1.Text = language.LanguageContent[Language.lastSV];
                form.label1.Location = new Point(133 - form.label1.Size.Width, form.label1.Location.Y);
                form.label2.Dispose();
                form.comboBox1.Dispose();
                form.MinimumSize = new Size(form.Size.Width, form.Size.Height - 50);
                form.Size = new Size(form.Size.Width, form.Size.Height - 50);
                form.Text = language.LanguageContent[Language.sliderVelocityChanger];
                form.checkBox1.Dispose();
                form.ShowDialog();
                if (BPM_Changer.value != 0)
                {
                    AddBackup();
                    double firstValue = Convert.ToDouble(dataGridView1.SelectedRows[dataGridView1.SelectedRows.Count - 1].Cells[1].Value.ToString().Substring(0, dataGridView1.SelectedRows[dataGridView1.SelectedRows.Count - 1].Cells[1].Value.ToString().IndexOf('x')));
                    double lastValue = BPM_Changer.value;
                    double multiplier = (lastValue - firstValue) / (Convert.ToDouble(dataGridView1.SelectedRows.Count - 1));
                    for (int i = dataGridView1.SelectedRows.Count - 1; i >= 0; i--)
                    {
                        double value = firstValue + (multiplier * (dataGridView1.SelectedRows.Count - i - 1));
                        value = -(100 / value);
                        string offsetString = dataGridView1.SelectedRows[i].Cells[0].Value.ToString();
                        int data = Convert.ToInt32(offsetString.Substring(0, offsetString.IndexOf(' ')));
                        for (int j = timingPointsIndex + 1; j < lines.Length && !string.IsNullOrWhiteSpace(lines[j]); j++)
                        {
                            int time = Convert.ToInt32(lines[j].Substring(0, lines[j].IndexOf(',')));
                            if (data == time && lines[j].Contains('-'))
                            {
                                lines[j] = lines[j].Remove(lines[j].IndexOfWithCount(',', 1), lines[j].IndexOfWithCount(',', 2) - lines[j].IndexOfWithCount(',', 1) - 1);
                                lines[j] = lines[j].Insert(lines[j].IndexOfWithCount(',', 1), value.ToString().Replace(',', '.'));
                                break;
                            }
                        }
                    }
                    File.WriteAllLines(path, lines);
                    ShowMode.Information(language.LanguageContent[Language.processComplete]);
                    manageLoad();
                }
            }
            if (!timer1.Enabled) timer1.Start();
        }
        private void VolumeChanger()
        {
            if (timer1.Enabled) timer1.Stop();
            RemoveInvisibleSelection();
            if (dataGridView1.SelectedRows.Count == 0)
            {
                ShowMode.Error(language.LanguageContent[Language.oneRowRequired]);
                return;
            }
            BPM_Changer form = new BPM_Changer();
            form.label2.Dispose();
            form.comboBox1.Dispose();
            form.MinimumSize = new Size(form.Size.Width, form.Size.Height - 50);
            form.Size = new Size(form.Size.Width, form.Size.Height - 50);
            form.label1.Text = language.LanguageContent[Language.volumeChange];
            form.label1.Location = new Point(133 - form.label1.Size.Width, form.label1.Location.Y);
            form.Text = language.LanguageContent[Language.volumeChanger];
            form.checkBox1.Dispose();
            form.ShowDialog();
            if (BPM_Changer.value == 0)
            {
                if (!timer1.Enabled) timer1.Start();
                return;
            }
            else if ((BPM_Changer.value % 1) != 0)
            {
                ShowMode.Error(language.LanguageContent[Language.notFloatValue]);
                if (!timer1.Enabled) timer1.Start();
                return;
            }
            else
            {
                AddBackup();
                for (int i = 0; i < dataGridView1.SelectedRows.Count; i++)
                {
                    double value = Convert.ToDouble(dataGridView1.SelectedRows[i].Cells[3].Value.ToString().Substring(0, dataGridView1.SelectedRows[i].Cells[3].Value.ToString().IndexOf('%')));
                    value += BPM_Changer.value;
                    if (value > 100)
                        value = 100;
                    else if (value < 5)
                        value = 5;
                    string offsetString = dataGridView1.SelectedRows[i].Cells[0].Value.ToString();
                    int data = Convert.ToInt32(offsetString.Substring(0, offsetString.IndexOf(' ')));
                    for (int j = timingPointsIndex + 1; !string.IsNullOrWhiteSpace(lines[j]); j++)
                    {
                        int time = Convert.ToInt32(lines[j].Substring(0, lines[j].IndexOf(',')));
                        if (data == time)
                        {
                            lines[j] = lines[j].Remove(lines[j].IndexOfWithCount(',', 5), lines[j].IndexOfWithCount(',', 6) - lines[j].IndexOfWithCount(',', 5) - 1);
                            lines[j] = lines[j].Insert(lines[j].IndexOfWithCount(',', 5), value.ToString().Replace(',', '.'));
                        }
                        else if (time > data)
                            break;
                    }
                }
                File.WriteAllLines(path, lines);
                ShowMode.Information(language.LanguageContent[Language.processComplete]);
                manageLoad();
            }
            if (!timer1.Enabled) timer1.Start();
        }
        private void VolumeStepByStep()
        {
            if (timer1.Enabled) timer1.Stop();
            RemoveInvisibleSelection();
            if (dataGridView1.SelectedRows.Count == 0)
            {
                ShowMode.Error(language.LanguageContent[Language.oneRowRequired]);
                if (!timer1.Enabled) timer1.Start();
                return;
            }
            BPM_Changer form = new BPM_Changer();
            form.label2.Dispose();
            form.comboBox1.Dispose();
            form.MinimumSize = new Size(form.Size.Width, form.Size.Height - 50);
            form.Size = new Size(form.Size.Width, form.Size.Height - 50);
            form.label1.Text = language.LanguageContent[Language.lastVolume];
            form.label1.Location = new Point(133 - form.label1.Size.Width, form.label1.Location.Y);
            form.label1.AutoSize = true;
            form.label1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            form.Text = language.LanguageContent[Language.volumeChanger];
            form.checkBox1.Dispose();
            form.ShowDialog();
            if (BPM_Changer.value == 0)
            {
                if (!timer1.Enabled) timer1.Start();
                return;
            }
            else if ((BPM_Changer.value % 1) != 0)
            {
                ShowMode.Error(language.LanguageContent[Language.notFloatValue]);
                if (!timer1.Enabled) timer1.Start();
                return;
            }
            else
            {
                AddBackup();
                List<int> offsets = new List<int>();
                for (int i = 0; i < dataGridView1.SelectedRows.Count; i++)
                {
                    string currentLine = dataGridView1.SelectedRows[i].Cells[0].Value.ToString();
                    offsets.Add(Convert.ToInt32(currentLine.Substring(0, currentLine.IndexOf(' '))));
                }
                offsets = offsets.Distinct().ToList();
                offsets.Sort();
                string currentString = dataGridView1.SelectedRows[dataGridView1.SelectedRows.Count - 1].Cells[3].Value.ToString();
                double firstValue = Convert.ToDouble(currentString.Substring(0, currentString.IndexOf('%')));
                double lastValue = BPM_Changer.value;
                double multiplier = (lastValue - firstValue) / (Convert.ToDouble(offsets.Count - 1));
                double value = firstValue;
                for (int i = 0; i < offsets.Count; i++)
                {
                    int data = offsets[i];
                    value = Convert.ToInt32(firstValue + (multiplier * i));
                    if (value > 100)
                        value = 100;
                    else if (value < 5)
                        value = 5;
                    for (int j = timingPointsIndex + 1; !string.IsNullOrWhiteSpace(lines[j]); j++)
                    {
                        int time = Convert.ToInt32(lines[j].Substring(0, lines[j].IndexOf(',')));
                        if (data == time)
                        {
                            lines[j] = lines[j].Remove(lines[j].IndexOfWithCount(',', 5), lines[j].IndexOfWithCount(',', 6) - lines[j].IndexOfWithCount(',', 5) - 1);
                            lines[j] = lines[j].Insert(lines[j].IndexOfWithCount(',', 5), ((int)value).ToString());
                        }
                        else if (time > data)
                            break;
                    }
                }
                File.WriteAllLines(path, lines);
                ShowMode.Information(language.LanguageContent[Language.processComplete]);
                manageLoad();
            }
            if (!timer1.Enabled) timer1.Start();
        }
        private void SVadder()
        {
            if (timer1.Enabled) timer1.Stop();
            SV_Changer obj = new SV_Changer();
            obj.ShowDialog();
            if (obj.Status == DialogResult.Yes)
            {
                List<string> lines = this.lines.ToList();
                List<double> currentBPMs = new List<double>();
                List<int> redPointOffsets = new List<int>();
                double currentBPMvalue = 0, currentBPMvalueTemp = 0, snapTime, SVchange, firstSV, firstBPMvalue = 1, variableOffset = obj.FirstTimeInMilliSeconds;
                string currentLine;
                int timingPointsIndex = -1;
                for (int i = 0; i < lines.Count; i++)
                    if (lines[i] == "[TimingPoints]")
                    { timingPointsIndex = i + 1; break; }
                int hitObjectsIndex = -1;
                for (int i = 0; i < lines.Count; i++)
                    if (lines[i] == "[HitObjects]")
                    { hitObjectsIndex = i + 1; break; }
                int resolution = 48,
                    counter = 0,
                    kTemp = 0,
                    gridValue = (int)(obj.FirstGridValue * resolution / obj.LastGridValue);
                firstSV = obj.FirstSV;
                for (int i = timingPointsIndex; !lines[i].Contains("["); i++)
                {
                    currentLine = lines[i];
                    if (!string.IsNullOrWhiteSpace(currentLine))
                    {
                        if (currentLine.Substring(currentLine.IndexOfWithCount(',', 6), 1) == "1")
                        {
                            redPointOffsets.Add(int.Parse(currentLine.Substring(0, currentLine.IndexOf(','))));
                            currentBPMs.Add(double.Parse(currentLine.Substring(currentLine.IndexOfWithCount(',', 1), currentLine.IndexOfWithCount(',', 2) - currentLine.IndexOfWithCount(',', 1) - 1).Replace('.', ',')));
                        }
                    }
                }
                if (redPointOffsets.Count > 1)
                {
                    for (int j = redPointOffsets.Count - 1; j >= 0; j--)
                    {
                        if (obj.FirstTimeInMilliSeconds >= redPointOffsets[j])
                        {
                            currentBPMvalue = currentBPMs[j];
                            currentBPMvalueTemp = currentBPMvalue;
                            kTemp = j;
                            firstBPMvalue = currentBPMvalue;
                            break;
                        }
                    }
                }
                else if (redPointOffsets.Count < 1)
                {
                    ShowMode.Error(language.LanguageContent[Language.noTimingPoints]);
                    return;
                }
                else
                {
                    currentBPMvalue = currentBPMs[0];
                    currentBPMvalueTemp = currentBPMvalue;
                    firstBPMvalue = currentBPMvalue;
                }
                if (obj.TargetBPM != 0)
                    firstBPMvalue = 60000 / obj.TargetBPM;
                if (!obj.isNoteMode)
                {
                    SVchange = (obj.LastSV - obj.FirstSV) / obj.Count;
                    for (int i = timingPointsIndex; !lines[i].Contains("[") && counter < obj.Count; i++)
                    {
                        currentLine = lines[i];
                        if (!string.IsNullOrWhiteSpace(currentLine))
                        {
                            double tempSV = -100 * ((firstBPMvalue / currentBPMvalue) / (firstSV + (SVchange * (counter + 1))));
                            string temp = lines[i].Substring(lines[i].IndexOfWithCount(',', 2));
                            temp = temp.Remove(temp.IndexOfWithCount(',', 4), 1);
                            temp = temp.Insert(temp.IndexOfWithCount(',', 4), "0");
                            temp = temp.Insert(0, tempSV.ToString().Replace(',', '.') + ",");
                            temp = temp.Insert(0, (int)variableOffset + ",");
                            lines.Insert(i + 1, temp);
                            for (int j = 0; j < gridValue; j++)
                            {
                                snapTime = currentBPMvalue / resolution;
                                variableOffset += snapTime;
                                if (redPointOffsets.Count > 1)
                                {
                                    for (int k = redPointOffsets.Count - 1; k >= 0; k--)
                                    {
                                        if (variableOffset >= redPointOffsets[k])
                                        {
                                            currentBPMvalue = currentBPMs[k];
                                            if (k > kTemp)
                                                i++;
                                            currentBPMvalueTemp = currentBPMvalue;
                                            kTemp = k;
                                            break;
                                        }
                                    }
                                }
                            }
                            counter++;
                        }
                    }
                }
                else
                {
                    List<int> noteOffsets = new List<int>();
                    List<double> initialSVchanges = new List<double>();
                    double currentTime = obj.FirstTimeInMilliSeconds, 
                        tempSV = obj.FirstSV, 
                        lastSV = obj.LastSV;
                    int startTime,
                        endTime,
                        redPointOffset = -10000,
                        currentTimeTemp = (int)currentTime;
                    if (obj.isBetweenTimeMode)
                    {
                        for (int i = hitObjectsIndex; i < lines.Count; i++)
                        {
                            currentLine = lines[i];
                            if (!string.IsNullOrWhiteSpace(currentLine))
                            {
                                string offsetString = currentLine.Substring(currentLine.IndexOfWithCount(',', 2),
                                    currentLine.IndexOfWithCount(',', 3) - currentLine.IndexOfWithCount(',', 2) - 1);
                                int offset = int.Parse(offsetString);
                                if (offset >= obj.FirstTimeInMilliSeconds)
                                {
                                    noteOffsets.Add(offset);
                                    for (int j = i + 1; j < lines.Count; j++)
                                    {
                                        if (!string.IsNullOrWhiteSpace(lines[j]))
                                        {
                                            currentLine = lines[j];
                                            offsetString = currentLine.Substring(currentLine.IndexOfWithCount(',', 2),
                                                currentLine.IndexOfWithCount(',', 3) - currentLine.IndexOfWithCount(',', 2) - 1);
                                            offset = int.Parse(offsetString);
                                            if (offset <= obj.LastTimeInMilliSeconds)
                                                noteOffsets.Add(offset);
                                            else
                                                break;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = hitObjectsIndex; i < lines.Count; i++)
                        {
                            currentLine = lines[i];
                            if (!string.IsNullOrWhiteSpace(currentLine))
                            {
                                string offsetString = currentLine.Substring(currentLine.IndexOfWithCount(',', 2),
                                    currentLine.IndexOfWithCount(',', 3) - currentLine.IndexOfWithCount(',', 2) - 1);
                                int offset = int.Parse(offsetString);
                                if (offset >= obj.FirstTimeInMilliSeconds)
                                {
                                    int listCounter = 0;
                                    for (int j = i; listCounter < obj.Count && j < lines.Count; j++)
                                    {
                                        if (!string.IsNullOrWhiteSpace(lines[j]))
                                        {
                                            currentLine = lines[j];
                                            offsetString = currentLine.Substring(currentLine.IndexOfWithCount(',', 2),
                                                currentLine.IndexOfWithCount(',', 3) - currentLine.IndexOfWithCount(',', 2) - 1);
                                            offset = int.Parse(offsetString);
                                            noteOffsets.Add(offset);
                                            listCounter++;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    noteOffsets.Distinct();
                    startTime = noteOffsets.Min();
                    endTime = noteOffsets.Max();
                    double totalDifference = endTime - startTime;
                    int listIndex = 0, fileIndex = 0, selectedIndex = 0;
                    for (int i = hitObjectsIndex - 1; i > timingPointsIndex - 1; i--)
                    {
                        if (lines[i].IndexOf(',') != -1 && lines[i].SearchCharCount(',') == 7)
                        {
                            if (int.Parse(lines[i].Substring(0, lines[i].IndexOf(','))) <= startTime)
                            {
                                fileIndex = i;
                                break;
                            }
                            else
                                fileIndex = timingPointsIndex + 1;
                        }
                    }
                    selectedIndex = fileIndex;
                    for (int i = redPointOffsets.Count - 1; i >= 0; i--)
                    {
                        if (currentTime >= redPointOffsets[i])
                        {
                            currentBPMvalue = 60000 / currentBPMs[i];
                            currentBPMvalueTemp = currentBPMvalue;
                            break;
                        }
                    }
                    if (obj.TargetBPM != 0)
                        firstBPMvalue = obj.TargetBPM;
                    else
                        firstBPMvalue = currentBPMvalue;
                    double currentBPM;
                    int existingSvIndexInLines;
                    if (redPointOffset != -10000 || noteOffsets.Count != 0)
                    {
                        for (; currentTime <= endTime && listIndex < noteOffsets.Count;)
                        {
                            currentTime = noteOffsets[listIndex];
                            currentBPM = GetCurrentBPM(redPointOffsets, currentBPMs, currentTime);
                            tempSV = GetSvForTextByDifference(firstSV, lastSV, currentTime - noteOffsets[0],
                                totalDifference, currentBPM, firstBPMvalue);
                            existingSvIndexInLines = GetExistingSvIndexInLines(lines, timingPointsIndex, currentTime);
                            if (existingSvIndexInLines == -1)
                            {
                                string temp = lines[selectedIndex];
                                temp = temp.Substring(temp.IndexOfWithCount(',', 2));
                                temp = temp.Remove(temp.IndexOfWithCount(',', 4), 1);
                                temp = temp.Insert(temp.IndexOfWithCount(',', 4), "0");
                                temp = temp.Insert(0, tempSV.ToString().Replace(',', '.') + ",");
                                temp = temp.Insert(0, currentTime.ToString() + ",");
                                lines.Insert(fileIndex + 1, temp);
                            }
                            else
                            {
                                string temp = lines[selectedIndex];
                                temp = temp.Substring(temp.IndexOfWithCount(',', 2));
                                temp = temp.Remove(temp.IndexOfWithCount(',', 4), 1);
                                temp = temp.Insert(temp.IndexOfWithCount(',', 4), "0");
                                temp = temp.Insert(0, tempSV.ToString().Replace(',', '.') + ",");
                                temp = temp.Insert(0, currentTime.ToString() + ",");
                                lines[existingSvIndexInLines] = temp;
                            }
                            listIndex++;
                            fileIndex++;
                        }
                    }
                    else
                    {
                        ShowMode.Error(language.LanguageContent[Language.noTimingPointsOrNotes]);
                        if (!timer1.Enabled) timer1.Start();
                        return;
                    }
                }
                AddBackup();
                this.lines = lines.ToArray();
                File.WriteAllLines(path, this.lines);
                ShowMode.Information(language.LanguageContent[Language.SVchangesAdded]);
                if (!obj.isNoteMode)
                {
                    if (ShowMode.QuestionWithYesNo(language.LanguageContent[Language.mayNotBeSnapped]) == DialogResult.Yes)
                    {
                        CheckMinorShiftOccurences();
                        File.WriteAllLines(path, this.lines);
                        ShowMode.Information(language.LanguageContent[Language.allSVchangesSnapped]);
                    }
                }
                manageLoad();
            }
            if (obj.checkBox3.Checked)
                SVadder();
            else if (!timer1.Enabled) timer1.Start();
        }

        private double GetSvForTextByDifference(double firstSV, double lastSV, 
            double currentDifference, double totalDifference,
            double currentBPM, double targetBPM)
        {
            return -100 / GetSvValueByDifference(firstSV, lastSV, currentDifference, 
                totalDifference, currentBPM, targetBPM);
        }

        private double GetSvValueByDifference(double firstSV, double lastSV,
            double currentDifference, double totalDifference,
            double currentBPM, double targetBPM)
        {
            double ratio = currentDifference / totalDifference;
            double sv = (firstSV + ((lastSV - firstSV) * ratio)) / (currentBPM / targetBPM);
            return sv;
        }

        private double GetCurrentBPM(List<int> redPointOffsets, List<double> currentBPMs, double currentTime)
        {
            int index = -1;
            for (int i = redPointOffsets.Count - 1; i >= 0; i--)
            {
                if (redPointOffsets[i] <= currentTime)
                {
                    index = i;
                    break;
                }
            }
            if (index == -1)
                throw new ArgumentException("Closest BPM point was not found while searching by note offset.");
            return 60000d / currentBPMs[index];
        }

        private int GetExistingSvIndexInLines(List<string> fileLines, int timingPointIndex, double currentTime)
        {
            string line;
            string[] splitted;
            int existingSvIndex = -1;
            double pointTime;
            for (int i = timingPointIndex; i < fileLines.Count; i++)
            {
                line = fileLines[i].Trim();
                if (line == "[TimingPoints]")
                    continue;
                else if (line.StartsWith("[") || string.IsNullOrEmpty(line))
                    break;
                else
                {
                    splitted = line.Split(',');
                    pointTime = double.Parse(splitted[0]);
                    if (pointTime == currentTime && splitted[6] == "0")
                    {
                        existingSvIndex = i;
                        break;
                    }
                    else if (pointTime > currentTime)
                        break;
                }
            }
            return existingSvIndex;
        }

        private void EqualizeSV()
        {
            if (timer1.Enabled) timer1.Stop();
            SV_Equalizer obj = new SV_Equalizer();
            obj.ShowDialog();
            if (obj.IsValueSet)
            {
                if (obj.editType == 0)
                {
                    AddBackup();
                    addSVs(obj.Bpm_value);
                }
                else if (obj.editType == 1)
                {
                    AddBackup();
                    editSVs(obj.Bpm_value, obj.SV_value);
                }
                else
                    ShowMode.Error(language.LanguageContent[Language.noFunctionSelected]);
            }
            if (!timer1.Enabled) timer1.Start();
        }
        private void DeleteAllInheritedPoints()
        {
            if (timer1.Enabled) timer1.Stop();
            if (ShowMode.QuestionWithYesNo(language.LanguageContent[Language.deleteAllGreenPoints]) == DialogResult.Yes)
            {
                AddBackup();
                List<string> fileLines = lines.ToList();
                int timingPointIndex = -1; for (int i = 0; i < fileLines.Count; i++) if (fileLines[i] == "[TimingPoints]") { timingPointIndex = i + 1; break; }
                for (int i = timingPointIndex; !string.IsNullOrWhiteSpace(fileLines[i]) && i < fileLines.Count; i++)
                {
                    string currentLine = fileLines[i];
                    int mode = Int32.Parse(currentLine.Substring(currentLine.IndexOfWithCount(',', 6), 1));
                    if (mode == 0)
                        fileLines.RemoveAt(i--);
                }
                File.WriteAllLines(path, fileLines.ToArray());
                ShowMode.Information(language.LanguageContent[Language.processComplete]);
                manageLoad();
            }
            if (!timer1.Enabled) timer1.Start();
        }
        private void DeleteDuplicatePoints()
        {
            if (timer1.Enabled) timer1.Stop();
            if (ShowMode.QuestionWithYesNo(language.LanguageContent[Language.areYouSure]) == DialogResult.Yes)
            {
                AddBackup();
                List<string> linesList = lines.ToList();
                int timingPointsIndex = -1, currentTime = -1, currentTimeTemp = -1, mode = -1, modeTemp = -1;
                string currentLine;
                for (int i = 0; i < linesList.Count; i++)
                    if (linesList[i] == "[TimingPoints]")
                    {
                        timingPointsIndex = i;
                        break;
                    }
                for (int i = timingPointsIndex + 1; !string.IsNullOrWhiteSpace(linesList[i]); i++)
                {
                    currentLine = linesList[i];
                    currentTime = Convert.ToInt32(currentLine.Substring(0, currentLine.IndexOf(',')));
                    mode = Convert.ToInt32(currentLine.Substring(currentLine.IndexOfWithCount(',', 6), 1));
                    if (currentTime == currentTimeTemp && mode == modeTemp)
                    {
                        linesList.RemoveAt(i);
                        i--;
                    }
                    currentTimeTemp = currentTime;
                    modeTemp = mode;
                }
                File.WriteAllLines(path, linesList.ToArray());
                ShowMode.Information(language.LanguageContent[Language.processComplete]);
                manageLoad();
            }
            if (!timer1.Enabled) timer1.Start();
        }
        private void DeleteUnneccessaryInheritedPoints()
        {
            if (timer1.Enabled) timer1.Stop();
            int inheritedPointsCount = 0;
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                if (dataGridView1.Rows[i].Cells[1].Value.ToString().Contains("x"))
                    inheritedPointsCount++;
            if (inheritedPointsCount < 2)
                ShowMode.Error(language.LanguageContent[Language.twoInheritedPointRequired]);
            else if(ShowMode.QuestionWithYesNo(language.LanguageContent[Language.areYouSure]) == DialogResult.Yes)
            {
                AddBackup();
                List<string> linesList = lines.ToList();
                int timingPointsIndex = 0; for(int i = 0; i < linesList.Count; i++) if(linesList[i] == "[TimingPoints]") { timingPointsIndex = i; break; }
                for(int i = timingPointsIndex + 1; !string.IsNullOrWhiteSpace(linesList[i + 1]); i++)
                {
                    string firstLine = linesList[i];
                    string secondLine = linesList[i + 1];
                    if (firstLine.Substring(firstLine.IndexOfWithCount(',', 1)).Equals(secondLine.Substring(secondLine.IndexOfWithCount(',', 1))))
                    {
                        linesList.RemoveAt(i + 1);
                        i--;
                    }
                }
                File.WriteAllLines(path, linesList.ToArray());
                ShowMode.Information(language.LanguageContent[Language.processComplete]);
                manageLoad();
            }
            if (!timer1.Enabled) timer1.Start();
        }
        private void MetadataManager()
        {
            if (timer1.Enabled) timer1.Stop();
            Metadata_manager obj = new Metadata_manager(path, lines);
            obj.ShowDialog();
            if (obj.isSuccess)
            {
                SelectFile();
                manageLoad();
            }
            if (!timer1.Enabled) timer1.Start();
        }
        #endregion
        #region Form control functions
        private void Form5_Load(object sender, EventArgs e)
        {
            if(!isActivated)
            {
                ShowInTaskbar = false;
                ShowInTaskbar = true;
                Activate();
                isActivated = true;
            }
            manageLoad();
        }
        private void Manage_Beatmap_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                AddBackup();
                manageLoad();
            }
            else if (e.KeyCode == Keys.Escape)
                dataGridView1.ClearSelection();
            else if (e.KeyCode == Keys.Z && e.Control)
                Undo();
            else if (e.KeyCode == Keys.Y && e.Control)
                Redo();
        }
        private void Manage_Beatmap_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (timer1.Enabled) timer1.Stop();
            if (ShowMode.QuestionWithYesNo(language.LanguageContent[Language.areYouSureYouWantToExit]) == DialogResult.No)
            {
                e.Cancel = true;
                if (!timer1.Enabled) timer1.Start();
            }
            else
                timer1.Dispose();
        }
        private void Manage_Beatmap_MouseMove(object sender, MouseEventArgs e)
        {

        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataGridView1.Focus();
        }
        private void applyFunctionButton_Click(object sender, EventArgs e) // Apply Function
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    SelectFile();
                    break;
                case 1:
                    Hitsounds();
                    break;
                case 2:
                    PrepareMapToHitsounding();
                    break;
                case 3:
                    WhistleToClap();
                    break;
                case 4:
                    PositionNotes();
                    break;
                case 5:
                    NewTiming();
                    break;
                case 6:
                    ChangeBPM();
                    break;
                case 7:
                    ChangeOffset();
                    break;
                case 8:
                    SVadder();
                    break;
                case 9:
                    EqualizeSV();
                    break;
                case 10:
                    SVchanger();
                    break;
                case 11:
                    SVstepbystep();
                    break;
                case 12:
                    VolumeChanger();
                    break;
                case 13:
                    VolumeStepByStep();
                    break;
                case 14:
                    DeleteSelectedInheritedPoints();
                    break;
                case 15:
                    DeleteAllInheritedPoints();
                    break;
                case 16:
                    DeleteDuplicatePoints();
                    break;
                case 17:
                    DeleteUnneccessaryInheritedPoints();
                    break;
                case 18:
                    MetadataManager();
                    break;
                default:
                    ShowMode.Error(language.LanguageContent[Language.noFunctionSelected]);
                    break;
            }
        }
        private void optionsButton_Click(object sender, EventArgs e) // Options
        {
            Options();
        }
        private void undoButton_Click(object sender, EventArgs e) // Undo
        {
            UndoFunction();
        }
        private void redoButton_Click(object sender, EventArgs e) // Redo
        {
            RedoFunction();
        }
        private void saveButton_Click(object sender, EventArgs e) // Save
        {
            SaveFileFunction();
        }
        private void inheritedPointButton_Click(object sender, EventArgs e) // Inherited point filtering
        {
            FilterInherited();
        }
        private void timingPointButton_Click(object sender, EventArgs e) // Timing point filtering
        {
            FilterTiming();
        }

        private void allVisibleButton_Click(object sender, EventArgs e) // Make all visible
        {
            RemoveFilter();
        }

        private void Manage_Beatmap_Shown(object sender, EventArgs e)
        {
            Opening();
        }

        private void timer1_Tick(object sender, EventArgs e) // Timer
        {
            short ctrl = GetAsyncKeyState((int)Keys.ControlKey);
            short s = GetAsyncKeyState((int)Keys.S);
            bool isControlPressed = ((ctrl >> 15) & 0x0001) == 0x0001;
            bool isSPressed = ((s >> 15) & 0x0001) == 0x0001;

            bool wasControlPressed = ((ctrl >> 0) & 0x0001) == 0x0001;
            bool wasSPressed = ((s >> 15) & 0x0001) == 0x0001;
            if (isControlPressed && isSPressed)
            {
                if (!(wasControlPressed && wasSPressed))
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        string activeWindow = GetActiveWindowTitle();
                        if(!string.IsNullOrEmpty(activeWindow))
                        {
                            if (activeWindow.Contains("osu!") && activeWindow.Contains(fileName)) // Only enters when osu! is running and the same map is open on the editor.
                            {
                                Thread.Sleep(500);
                                AddBackup();
                                lines = File.ReadAllLines(path);
                                prepareGridView();
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}
