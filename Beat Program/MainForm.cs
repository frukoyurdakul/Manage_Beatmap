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

namespace BeatmapManager
{
    public partial class MainForm : Form
    {
        string path = string.Empty;
        string fileName = string.Empty;
        string lastUpdate = string.Empty;
        string[] lines;
        List<string> activeComboBoxItems, notActiveComboBoxItems;
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
        int buttonsEnabledState = 0;
        bool isDefined = false;
        bool isHaveNotes = false;
        bool isActivated = false;
        bool isBackup = false;

        private readonly decimal resolution = 5040m;
        private readonly SortedSet<decimal> snaps = new SortedSet<decimal>();

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                return cp;
            }
        }

        public MainForm()
        {
            InitializeComponent();
            SetLanguage();
            ChangeControlTexts();
            SetSnapResolutions();
        }

        private void SetSnapResolutions()
        {
            snaps.Add(0m);
            snaps.Add(315m);
            snaps.Add(420m);
            snaps.Add(560m);
            snaps.Add(630m);
            snaps.Add(720m);
            snaps.Add(840m);
            snaps.Add(945m);
            snaps.Add(1008m);
            snaps.Add(1120m);
            snaps.Add(1260m);
            snaps.Add(1440m);
            snaps.Add(1575m);
            snaps.Add(1680m);
            snaps.Add(1890m);
            snaps.Add(2016m);
            snaps.Add(2100m);
            snaps.Add(2160m);
            snaps.Add(2205m);
            snaps.Add(2240m);
            snaps.Add(2520m);
            snaps.Add(2800m);
            snaps.Add(2835m);
            snaps.Add(2880m);
            snaps.Add(2940m);
            snaps.Add(3024m);
            snaps.Add(3150m);
            snaps.Add(3360m);
            snaps.Add(3465m);
            snaps.Add(3600m);
            snaps.Add(3780m);
            snaps.Add(3920m);
            snaps.Add(4032m);
            snaps.Add(4095m);
            snaps.Add(4200m);
            snaps.Add(4320m);
            snaps.Add(4410m);
            snaps.Add(4480m);
            snaps.Add(4620m);
            snaps.Add(4725m);
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
            string decimalSeparator = Program.GetDecimalSeparator();
            string originalDecimalSeparator = Program.GetOriginalDecimalSeparator();
            for (int i = timingPointsIndex + 1; i < lines.Length - 1 && !string.IsNullOrWhiteSpace(lines[i]); i++)
            {
                DataRow dr = table.NewRow();
                string temp = lines[i];
                double offset = Convert.ToDouble(temp.Substring(0, temp.IndexOf(',')).Replace(originalDecimalSeparator, decimalSeparator));
                if (offset != (int)offset)
                    offsetErrorIndexes.Add(dataGridViewRowIndex);
                dr[colIndex++] = offset.ToString() + "  (" + FormatString.getFormattedTimeString((int)offset) + ")";
                double bpmOrSV = Convert.ToDouble(temp.Substring(temp.IndexOfWithCount(',', 1), temp.IndexOfWithCount(',', 2) - temp.IndexOfWithCount(',', 1) - 1).Replace(originalDecimalSeparator, decimalSeparator));
                if (bpmOrSV < 0)
                    dr[colIndex++] = string.Format("{0:0.0000}", (-100) / bpmOrSV) + "x".Replace(originalDecimalSeparator, decimalSeparator);
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
            {
                DataGridViewColumn column = dataGridView1.Columns[i];
                if (i == 0)
                    column.FillWeight = 4;
                else if (i == 1)
                    column.FillWeight = 3;
                else
                    column.FillWeight = 2;
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            dataGridView1.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns[4].DefaultCellStyle.Font = new Font("Arial Narrow", 12F, FontStyle.Bold);
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
            if (buttonsEnabledState == 1)
                return;

            buttonsEnabledState = 1;
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(notActiveComboBoxItems.ToArray());
            button19.Enabled = false;
            button21.Enabled = false;
            button22.Enabled = false;
            button23.Enabled = false;
        }
        private void enableButtons()
        {
            if (buttonsEnabledState == 2)
                return;

            buttonsEnabledState = 2;
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(activeComboBoxItems.ToArray());
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
            activeComboBoxItems = new List<string>();
            notActiveComboBoxItems = new List<string>();
            activeComboBoxItems.Add(language.LanguageContent[Language.selectFileButton]);
            // activeComboBoxItems[1] = language.LanguageContent[Language.addHitsoundsToFolderButton];
            // activeComboBoxItems[2] = language.LanguageContent[Language.prepareMapToHitsoundingButton];
            activeComboBoxItems.Add(language.LanguageContent[Language.setAllWhistleToClapsButton]);
            activeComboBoxItems.Add(language.LanguageContent[Language.positionNotesButton]);
            // activeComboBoxItems[5] = language.LanguageContent[Language.newTimingButton];
            activeComboBoxItems.Add(language.LanguageContent[Language.changeBPMofSelectedPointButton]);
            // activeComboBoxItems[7] = language.LanguageContent[Language.changeOffsetsOfSelectedPointsButton];
            activeComboBoxItems.Add(language.LanguageContent[Language.addInheritedPointsToChangeSVsmoothlyButton]);
            activeComboBoxItems.Add(language.LanguageContent[Language.equalizeSVforAllTimingPointsButton]);
            activeComboBoxItems.Add(language.LanguageContent[Language.increaseOrDecreaseSVsButton]);
            activeComboBoxItems.Add(language.LanguageContent[Language.increaseSVstepByStepButton]);
            activeComboBoxItems.Add(language.LanguageContent[Language.changeVolumesButton]);
            activeComboBoxItems.Add(language.LanguageContent[Language.changeVolumesStepByStepButton]);
            // activeComboBoxItems[14] = language.LanguageContent[Language.deleteSelectedInheritedPointsButton];
            // activeComboBoxItems[15] = language.LanguageContent[Language.deleteAllInheritedPointsButton];
            // activeComboBoxItems[16] = language.LanguageContent[Language.deleteDuplicatePointsButton];
            // activeComboBoxItems[17] = language.LanguageContent[Language.deleteUnneccessaryInheritedPointsButton];
            activeComboBoxItems.Add(language.LanguageContent[Language.copyTagsButton]);
            notActiveComboBoxItems.Add(language.LanguageContent[Language.selectFileButton]);
            // notActiveComboBoxItems[1] = language.LanguageContent[Language.addHitsoundsToFolderButton];
        }
        private void ChangeControlTexts()
        {
            SetComboBoxItems();
            Text = language.LanguageContent[Language.manageBeatmapFormTitle] + " {" + fileName + "}";
            label2.Text = language.LanguageContent[Language.manageBeatmapTopLabel] + lastUpdate;
            label1.Text = language.LanguageContent[Language.manageBeatmapKeysLabel];

            comboBox1.Items.Clear();
            if (!string.IsNullOrEmpty(path))
                comboBox1.Items.AddRange(activeComboBoxItems.ToArray());
            else
                comboBox1.Items.AddRange(notActiveComboBoxItems.ToArray());
            button16.Text = language.LanguageContent[Language.optionsFormTitle];
            button17.Text = language.LanguageContent[Language.undo];
            button18.Text = language.LanguageContent[Language.redo];
            button19.Text = language.LanguageContent[Language.save];
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
                SmoothSvChanger();
                timer1.Start();
            }
        }
        private void SetTimingOffsetsAndNewBpm()
        {
            lines = SetTimingOffsetsAndNewBpm(lines);
        }
        private string[] SetTimingOffsetsAndNewBpm(string[] lines)
        {
            decimal actualBpm = Convert.ToDecimal(dataGridView1.SelectedRows[0].Cells[1].Value);
            decimal enteredBpm = Convert.ToDecimal(BPM_Changer.value);
            decimal actualBpmMillis = 60000m / actualBpm;
            decimal enteredBpmMillis = 60000m / enteredBpm;

            decimal ratio1 = actualBpm / enteredBpm;
            decimal ratio2 = enteredBpmMillis / actualBpmMillis;

            string startingOffsetString = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();
            int startingOffset = Convert.ToInt32(startingOffsetString.Substring(0, startingOffsetString.IndexOf(' '))), newOffset;
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
                            decimal bpmInTermsOfTime = 60000m / enteredBpm;
                            string bpmInTermsOfTimeString = Convert.ToDouble(bpmInTermsOfTime).ToString().Replace(',', '.');
                            selectedLine = selectedLine.Remove(selectedLine.IndexOf(',') + 1, selectedLine.IndexOfWithCount(',', 2) - selectedLine.IndexOf(',') - 2);
                            selectedLine = selectedLine.Insert(selectedLine.IndexOf(',') + 1, bpmInTermsOfTimeString);
                            lines[j] = selectedLine;
                        }
                        else if (selectedOffset > startingOffset)
                        {
                            decimal diff = selectedOffset - startingOffset;
                            decimal closestResolution = GetClosestResolution(diff, actualBpmMillis);
                            decimal result = enteredBpmMillis * closestResolution / resolution;
                            newOffset = (int)(startingOffset + result);
                            if (selectedLine.Substring(selectedLine.IndexOfWithCount(',', 6), 1) == "1")
                            {
                                if (!isDefined)
                                {
                                    closestResolution = GetResolution(diff, actualBpmMillis);
                                    result = enteredBpmMillis * closestResolution / resolution;
                                    newOffset = (int)(startingOffset + result);

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
            lines = SetNewHitObjectOffsets(lines, false);
            if (isHaveNotes)
                ShowMode.Information(language.LanguageContent[Language.noteTimingPositive]);
            else
                ShowMode.Warning(language.LanguageContent[Language.noteTimingNegative]);
        }
        private string[] SetNewHitObjectOffsets(string[] lines, bool showWarning = false)
        {
            decimal actualBpm = Convert.ToDecimal(dataGridView1.SelectedRows[0].Cells[1].Value);
            decimal enteredBpm = Convert.ToDecimal(BPM_Changer.value);

            decimal actualBpmMillis = 60000m / actualBpm;
            decimal enteredBpmMillis = 60000m / enteredBpm;

            decimal ratio1 = actualBpm / enteredBpm;
            decimal ratio2 = enteredBpmMillis / actualBpmMillis;

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
                                decimal diff = selectedOffset - startingOffset;
                                decimal closestResolution = GetClosestResolution(diff, actualBpmMillis);
                                decimal result = enteredBpmMillis * closestResolution / resolution;
                                newOffset = (int)(startingOffset + result);
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
                                decimal diff = selectedOffset - startingOffset;
                                decimal closestResolution = GetClosestResolution(diff, actualBpmMillis);
                                decimal result = enteredBpmMillis * closestResolution / resolution;
                                newOffset = (int)(startingOffset + result);
                                currentLine = currentLine.Remove(currentLine.IndexOfWithCount(',', 2), currentLine.IndexOfWithCount(',', 3) - currentLine.IndexOfWithCount(',', 2) - 1);
                                currentLine = currentLine.Insert(currentLine.IndexOfWithCount(',', 2), newOffset.ToString());

                                diff = spinnerEndOffset - startingOffset;
                                closestResolution = GetClosestResolution(diff, actualBpmMillis);
                                result = enteredBpmMillis * closestResolution / resolution;
                                newOffset = (int)(startingOffset + result);
                                currentLine = currentLine.Remove(currentLine.IndexOfWithCount(',', 5), currentLine.IndexOfWithCount(',', 6) - currentLine.IndexOfWithCount(',', 5) - 1);
                                currentLine = currentLine.Insert(currentLine.IndexOfWithCount(',', 5), newOffset.ToString());
                                lines[i] = currentLine;
                            }
                        }
                        isHaveNotes = true;
                    }
                }
            }
            if (showWarning && !isHaveNotes)
                ShowMode.Warning(language.LanguageContent[Language.noteTimingNegative]);
            return lines;
        }
        private decimal GetClosestResolution(decimal diff, decimal bpm)
        {
            decimal resLocal = Convert.ToInt32(diff / bpm * resolution);
            decimal resModuled = resLocal % resolution;
            decimal resRoot = resLocal - resModuled;
            decimal closestRes = GetClosest(snaps, resModuled);
            return resRoot + closestRes;
        }

        private decimal GetResolution(decimal diff, decimal bpm)
        {
            return Convert.ToInt32(diff / bpm * resolution);
        }

        private decimal GetClosest(SortedSet<decimal> thisList, decimal thisValue)
        {
            // Check to see if we need to search the list.
            if (thisList == null || thisList.Count <= 0) { return 0; }
            if (thisList.Count == 1) { return thisList.ElementAt(0); }

            // Setup the variables needed to find the closest index
            int lower = 0;
            int upper = thisList.Count - 1;
            int index = (lower + upper) / 2;

            // Find the closest index (rounded down)
            bool searching = true;
            while (searching)
            {
                int comparisonResult = Decimal.Compare(thisValue, thisList.ElementAt(index));
                if (comparisonResult == 0) { return thisList.ElementAt(index); }
                else if (comparisonResult < 0) { upper = index - 1; }
                else { lower = index + 1; }

                index = (lower + upper) / 2;
                if (lower > upper) { searching = false; }
            }

            // Check to see if we are under or over the max values.
            if (index >= thisList.Count - 1) { return thisList.Max; }
            if (index < 0) { return thisList.Min; }

            // Check to see if we should have rounded up instead
            if (thisList.ElementAt(index + 1) - thisValue < thisValue - (thisList.ElementAt(index))) { index++; }

            // Return the correct/closest string
            return thisList.ElementAt(index);
        }

        private void CheckMinorShiftOccurences(int svOffset) // fixes the minor calculation errors depending on floating point issues by equalizing the green point - note offsets if between 5ms around.
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
                    timingPointOffset = Int32.Parse(currentTimingPointLine.Substring(0, currentTimingPointLine.IndexOf(','))) + svOffset;
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
                    CheckMinorShiftOccurences(0);
                }
            }
            ShowMode.Information(language.LanguageContent[Language.processComplete]);
        }
        private void WriteNewFileFromArray()
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
            File.WriteAllLines(path, lines);
            if (isHaveNotes)
            {
                if (ShowMode.QuestionWithYesNo(language.LanguageContent[Language.notesShift]) == DialogResult.Yes)
                {
                    ShowMode.Warning(language.LanguageContent[Language.snapWarning]);
                    CheckMinorShiftOccurences(0);
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
                                            string testString = currentLine.Substring(currentLine.IndexOfWithCount(',', 1), currentLine.IndexOfWithCount(',', 2) - currentLine.IndexOfWithCount(',', 1) - 1).ReplaceDecimalSeparator();
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
                                            string testString = currentLine.Substring(currentLine.IndexOfWithCount(',', 1), currentLine.IndexOfWithCount(',', 2) - currentLine.IndexOfWithCount(',', 1) - 1).ReplaceDecimalSeparator();
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
                                         currentLine.IndexOfWithCount(',', 2) - currentLine.IndexOfWithCount(',', 1) - 1).ReplaceDecimalSeparator());
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
                    linesList[i].IndexOfWithCount(',', 2) - linesList[i].IndexOfWithCount(',', 1) - 1).ReplaceDecimalSeparator());
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
            currencyManager1.SuspendBinding();
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
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
            currencyManager1.SuspendBinding();
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
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
            for (int i = 0; i < lines.Length; i++) if (lines[i].Contains("SliderMultiplier") && lines[i] != "SliderMultiplier") { sliderMultiplier = Convert.ToDouble(lines[i].Substring(lines[i].IndexOf(':') + 1).ReplaceDecimalSeparator()); break; }
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
                        hitObjectOffsets.Add(new Notes((int)(Convert.ToDouble(currentLine.Substring(currentLine.IndexOfWithCount(',', 2), currentLine.IndexOfWithCount(',', 3) - currentLine.IndexOfWithCount(',', 2) - 1).ReplaceDecimalSeparator())), currentLine));
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
            BPM_Changer changer = new BPM_Changer(BPM =>
            {
                if (dataGridView1.SelectedRows.Count != 1)
                    ShowMode.Error(language.LanguageContent[Language.oneRowOnly]);
                else
                {
                    if (dataGridView1.SelectedRows[0].Cells[1].Value.ToString().Contains("x"))
                        ShowMode.Error(language.LanguageContent[Language.inheritedPointBPMError]);
                    else
                    {
                        string offsetString = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();
                        BPM_Changer.Offset = Convert.ToInt32(offsetString.Substring(0, offsetString.IndexOf(' ')));
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
                                            lines = SetNewHitObjectOffsets(lines, true);
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
                                    WriteNewFileFromArray();
                                }
                                linesList.Clear();
                                manageLoad();
                            }
                        }
                    }
                }
            });

            changer.checkBox1.Checked = isMapHaveBookmarks();
            formHandlerPanel.SetForm(changer);
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
            RemoveInvisibleSelection();

            BPM_Changer form = new BPM_Changer(_ =>
            {
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
                    if (BPM_Changer.value != 0)
                    {
                        string[] lines = File.ReadAllLines(path);
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
            });

            form.AdjustFormForSingleInput(language.LanguageContent[Language.sliderVelocityChange]);
            formHandlerPanel.SetForm(form);
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
            RemoveInvisibleSelection();

            BPM_Changer form = new BPM_Changer(_ =>
            {
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

                    if (BPM_Changer.value != 0)
                    {
                        string[] lines = File.ReadAllLines(path);
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
            });

            form.AdjustFormForSingleInput(language.LanguageContent[Language.lastSV]);
            formHandlerPanel.SetForm(form);
        }
        private void VolumeChanger()
        {
            RemoveInvisibleSelection();
            BPM_Changer form = new BPM_Changer(_ =>
            {
                if (dataGridView1.SelectedRows.Count == 0)
                {
                    ShowMode.Error(language.LanguageContent[Language.oneRowRequired]);
                    return;
                }
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
                    string[] lines = File.ReadAllLines(path);
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
            });

            form.AdjustFormForSingleInput(language.LanguageContent[Language.volumeChange]);
            formHandlerPanel.SetForm(form);
        }
        private void VolumeStepByStep()
        {
            RemoveInvisibleSelection();
            BPM_Changer form = new BPM_Changer(_ =>
            {
                if (dataGridView1.SelectedRows.Count == 0)
                {
                    ShowMode.Error(language.LanguageContent[Language.oneRowRequired]);
                    if (!timer1.Enabled) timer1.Start();
                    return;
                }
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
                    string[] lines = File.ReadAllLines(path);
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
            });

            form.AdjustFormForSingleInput(language.LanguageContent[Language.lastVolume]);
            formHandlerPanel.SetForm(form);
        }

        private void SmoothSvChanger()
        {
            SV_Changer svChanger = new SV_Changer(form =>
            {
                AddBackup();
                this.lines = File.ReadAllLines(path);
                List<string> lines = this.lines.ToList();
                List<double> currentBPMs = new List<double>();
                List<int> redPointOffsets = new List<int>();
                SortedDictionary<int, int> greenPoints = new SortedDictionary<int, int>();
                List<int> greenPointOffsets = new List<int>();
                double currentBPMvalue = 0, currentBPMvalueTemp = 0, snapTime, SVchange, firstSV, firstBPMvalue = 1, variableOffset = form.FirstTimeInMilliSeconds;
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
                    gridValue = (int)(form.FirstGridValue * resolution / form.LastGridValue),
                    svOffset = form.SvOffset;
                firstSV = form.FirstSV;
                string pointType;
                for (int i = timingPointsIndex; !lines[i].Contains("["); i++)
                {
                    currentLine = lines[i];
                    if (!string.IsNullOrWhiteSpace(currentLine))
                    {
                        pointType = currentLine.Substring(currentLine.IndexOfWithCount(',', 6), 1);
                        if (pointType == "1")
                        {
                            redPointOffsets.Add(int.Parse(currentLine.Substring(0, currentLine.IndexOf(','))));
                            currentBPMs.Add(double.Parse(currentLine.Substring(currentLine.IndexOfWithCount(',', 1), currentLine.IndexOfWithCount(',', 2) - currentLine.IndexOfWithCount(',', 1) - 1).ReplaceDecimalSeparator()));
                        }
                        else if (pointType == "0")
                        {
                            int offset = int.Parse(currentLine.Substring(0, currentLine.IndexOf(',')));
                            greenPoints.Remove(offset);
                            greenPoints.Add(offset, i);
                            greenPointOffsets.Add(offset);
                        }
                    }
                }
                if (redPointOffsets.Count > 1)
                {
                    for (int j = redPointOffsets.Count - 1; j >= 0; j--)
                    {
                        if (form.FirstTimeInMilliSeconds >= redPointOffsets[j])
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
                if (form.TargetBPM != 0)
                    firstBPMvalue = 60000 / form.TargetBPM;
                if (!form.IsNoteMode)
                {
                    double count = form.Count;
                    double expMultiplier = form.ExpMultiplier;
                    SVchange = (form.LastSV - form.FirstSV) / count;
                    for (int i = timingPointsIndex; !lines[i].Contains("[") && counter < count; i++)
                    {
                        currentLine = lines[i];
                        if (!string.IsNullOrWhiteSpace(currentLine))
                        {
                            double target = counter + 1;
                            double percent = target / count;
                            double exponent = Math.Pow(percent, expMultiplier);
                            double result = target * exponent;
                            double tempSV = -100 * ((firstBPMvalue / currentBPMvalue) / (firstSV + (SVchange * result)));
                            string temp = lines[i].Substring(lines[i].IndexOfWithCount(',', 2));
                            temp = temp.Remove(temp.IndexOfWithCount(',', 4), 1);
                            temp = temp.Insert(temp.IndexOfWithCount(',', 4), "0");
                            temp = temp.Insert(0, tempSV.ToString().Replace(',', '.') + ",");
                            temp = temp.Insert(0, (int)variableOffset + svOffset + ",");
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
                    double currentTime = form.FirstTimeInMilliSeconds,
                        tempSV,
                        lastSV = form.LastSV;
                    int startTime,
                        endTime,
                        redPointOffset = -10000;
                    if (form.IsBetweenTimeMode)
                    {
                        for (int i = hitObjectsIndex; i < lines.Count; i++)
                        {
                            currentLine = lines[i];
                            if (!string.IsNullOrWhiteSpace(currentLine))
                            {
                                string offsetString = currentLine.Substring(currentLine.IndexOfWithCount(',', 2),
                                    currentLine.IndexOfWithCount(',', 3) - currentLine.IndexOfWithCount(',', 2) - 1);
                                int offset = int.Parse(offsetString);
                                if (offset >= form.FirstTimeInMilliSeconds)
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
                                            if (offset <= form.LastTimeInMilliSeconds)
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
                                if (offset >= form.FirstTimeInMilliSeconds)
                                {
                                    int listCounter = 0;
                                    for (int j = i; listCounter < form.Count && j < lines.Count; j++)
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
                    if (form.TargetBPM != 0)
                        firstBPMvalue = form.TargetBPM;
                    else
                        firstBPMvalue = currentBPMvalue;

                    double expMultiplier = form.ExpMultiplier;
                    double currentBPM;
                    int existingSvIndexInLines;
                    int selectedIndex2;
                    double svOffsetTemp;
                    double currentTimeRaw;
                    bool noteIsOnRedPoint;
                    bool noteIsOnGreenPoint;
                    bool isShiftingAsked = false;
                    bool isShiftingPoints = false;
                    bool isKiaiActive = false;
                    bool isKiaiActiveTemp = false;
                    bool firstLoop = true;
                    if (redPointOffset != -10000 || noteOffsets.Count != 0)
                    {
                        for (; currentTime <= endTime && listIndex < noteOffsets.Count;)
                        {
                            noteIsOnRedPoint = redPointOffsets.Contains(noteOffsets[listIndex]);
                            svOffsetTemp = noteIsOnRedPoint ? 0 : svOffset;

                            currentTime = noteOffsets[listIndex] + svOffsetTemp;
                            currentTimeRaw = noteOffsets[listIndex];
                            selectedIndex = GetClosestPointIndexInLines(lines, timingPointsIndex, currentTime);
                            selectedIndex2 = GetClosestPointIndexInLines(lines, timingPointsIndex, currentTimeRaw);

                            isKiaiActiveTemp = selectedIndex2 != -1 && lines[selectedIndex2].IsKiaiOpen();

                            // If we're checking for the first time, find the second closest from the line's
                            // offset and mark the kiai active value. Do not do this on other encounters.
                            if (firstLoop && selectedIndex != -1)
                            {
                                isKiaiActive = lines[selectedIndex].IsKiaiOpen();
                                firstLoop = false;
                            }

                            // Check if the point toggles kiai. If it does, do not apply offset.
                            if (isKiaiActiveTemp != isKiaiActive)
                            {
                                svOffsetTemp = 0;
                                currentTime = currentTimeRaw;
                                selectedIndex = selectedIndex2;
                            }

                            isKiaiActive = isKiaiActiveTemp;

                            if (svOffsetTemp != 0)
                            {
                                int noteOffset = noteOffsets[listIndex];
                                noteIsOnGreenPoint = greenPointOffsets.Contains(noteOffset) || greenPointOffsets.Contains((int)(noteOffset + svOffsetTemp));
                                if (noteIsOnGreenPoint && !greenPointOffsets.Contains((int)currentTime))
                                {
                                    if (!isShiftingAsked)
                                    {
                                        DialogResult result = ShowMode.QuestionWithYesNoCancel("The point at " + TimeSpan.FromMilliseconds(noteOffset).ToString(@"mm':'ss':'fff") + " will be duplicated with the defined SV offset. Are you sure you want to continue?\r\n\r\n\"Yes\" will shift the current offsets, \"No\" will put points which will potentially be duplicates, \"Cancel\" will cancel the whole operation.");
                                        if (result == DialogResult.Yes)
                                            isShiftingPoints = true;
                                        else if (result == DialogResult.No)
                                            isShiftingPoints = false;
                                        else
                                            return;
                                        isShiftingAsked = true;
                                    }
                                }
                            }

                            currentBPM = GetCurrentBPM(redPointOffsets, currentBPMs, currentTimeRaw);
                            tempSV = GetSvForTextByDifference(firstSV, lastSV, currentTimeRaw - startTime,
                                totalDifference, currentBPM, firstBPMvalue, expMultiplier);
                            existingSvIndexInLines = GetExistingSvIndexInLines(lines, timingPointsIndex, currentTime);
                            if (existingSvIndexInLines == -1 && isShiftingPoints)
                                existingSvIndexInLines = GetExistingSvIndexInLines(lines, timingPointsIndex, noteOffsets[listIndex]);

                            if (IsSvEqual(lines, selectedIndex, tempSV))
                            {
                                listIndex++;
                                continue;
                            }
                            else if (existingSvIndexInLines == -1)
                            {
                                string temp = lines[selectedIndex];
                                temp = temp.Substring(temp.IndexOfWithCount(',', 2));
                                temp = temp.Remove(temp.IndexOfWithCount(',', 4), 1);
                                temp = temp.Insert(temp.IndexOfWithCount(',', 4), "0");
                                temp = temp.Insert(0, tempSV.ToString().Replace(',', '.') + ",");
                                temp = temp.Insert(0, currentTime.ToString() + ",");
                                lines.Insert(selectedIndex + 1, temp);
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
                if (!form.IsNoteMode)
                {
                    if (ShowMode.QuestionWithYesNo(language.LanguageContent[Language.mayNotBeSnapped]) == DialogResult.Yes)
                    {
                        CheckMinorShiftOccurences(svOffset);
                        File.WriteAllLines(path, this.lines);
                        ShowMode.Information(language.LanguageContent[Language.allSVchangesSnapped]);
                    }
                }
                manageLoad();
            });
            formHandlerPanel.SetForm(svChanger);
        }

        private double GetOffsetOfLine(string line)
        {
            string offset = line.Substring(0, line.IndexOf(','));
            return double.Parse(offset);
        }

        private bool IsSvEqual(List<string> lines, int index, double targetSV)
        {
            // The index being smaller than 0 means there are no inherited points,
            // assume the SV is 1.0 at that point.
            if (index < 0)
                return targetSV == -100.0;

            string line = lines[index];

            // The passed point here can also be a red point. In that case, assume the SV
            // is 1.00x.
            bool isGreenPoint = line.IsPointInherited();
            if (!isGreenPoint)
                return targetSV == -100.0;

            double sv = double.Parse(SubstringWithCount(line, ',', 1, 2).ReplaceDecimalSeparator());
            return Math.Abs(targetSV - sv) < 0.000001;
        }

        public static string SubstringWithCount(string text, char searched, int from, int to)
        {
            if (from > to)
                throw new ArgumentException("\"from\" cannot be bigger than \"to\", from: " + from + ", to: " + to);

            int startIndex = text.IndexOfWithCount(searched, from);
            int endIndex = text.IndexOfWithCount(searched, to);
            string result;

            if (startIndex == -1 && from == 0 && endIndex - 1 >= 0)
                result = text.Substring(0, endIndex - 1);
            else if (startIndex >= 0 && endIndex - 1 >= 0)
                result = text.Substring(startIndex, endIndex - startIndex - 1);
            else
                result = "";
            return result;
        }

        private double GetSvForTextByDifference(double firstSV, double lastSV, 
            double currentDifference, double totalDifference,
            double currentBPM, double targetBPM, double expMultiplier)
        {
            return -100 / GetSvValueByDifference(firstSV, lastSV, currentDifference, 
                totalDifference, currentBPM, targetBPM, expMultiplier);
        }

        private double GetSvValueByDifference(double firstSV, double lastSV,
            double currentDifference, double totalDifference,
            double currentBPM, double targetBPM, double expMultiplier)
        {
            double ratio = currentDifference / totalDifference;
            double exponent, sv;
            if (lastSV < firstSV)
            {
                exponent = Math.Pow(1d - ratio, expMultiplier);
                sv = (lastSV + ((firstSV - lastSV) * exponent)) / (currentBPM / targetBPM);
            }
            else
            {
                exponent = Math.Pow(ratio, expMultiplier);
                sv = (firstSV + ((lastSV - firstSV) * exponent)) / (currentBPM / targetBPM);
            }
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
                    if (pointTime == currentTime)
                    {
                        if (splitted[6] == "0")
                        {
                            existingSvIndex = i;
                            break;
                        }
                    }
                    else if (pointTime > currentTime)
                        break;
                }
            }
            return existingSvIndex;
        }

        private int GetClosestPointIndexInLines(List<string> fileLines, int timingPointIndex, double currentTime)
        {
            string line;
            string[] splitted;
            int previousIndex = -1;
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
                    if (pointTime == currentTime)
                    {
                        // Return immediately if the point is an inherited point.
                        // Otherwise, it is a timing point. On the next loop,
                        // we might find an inherited point with the same offset,
                        // but we can also find a point with bigger offset.
                        // 
                        // In any case, in the next loop, we will probably return
                        // either this index, or the previous index.
                        if (line.IsPointInherited())
                            return i;
                    }
                    else if (pointTime > currentTime)
                        return previousIndex;
                }
                previousIndex = i;
            }
            return previousIndex;
        }

        private void SVequalizer()
        {
            formHandlerPanel.SetForm(new SV_Equalizer(equalizerForm =>
            {
                if (equalizerForm.ApplyToAllTaikoDiffs)
                {
                    DirectoryInfo folder = Directory.GetParent(path);
                    List<FileInfo> taikoDiffs = folder.GetFiles().Where(info => File.ReadAllLines(info.FullName).IsTaikoDifficulty()).ToList();
                    if (taikoDiffs.Count > 1)
                    {
                        DialogResult result = ShowMode.QuestionWithYesNoCancel("The beatmapset contains multiple taiko diffs and backups inside Manage Beatmap tool cannot be created.\n\nDo you want to save the backups to the Desktop?");
                        if (result == DialogResult.Cancel)
                            return;
                        else if (result == DialogResult.Yes)
                        {
                            // Save the current diffs to desktop here.
                            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + folder.Name;

                            // Check if the directory can be created.
                            if (Directory.CreateDirectory(desktopPath).Exists)
                            {
                                foreach (FileInfo diff in taikoDiffs)
                                {
                                    File.Copy(diff.FullName, desktopPath + "\\" + diff.Name);
                                }
                            }
                        }

                        // If we reach here, it means the SVs should be equalized.
                        int count = taikoDiffs.Count;
                        for (int i = 0; i < count; i++)
                        {
                            bool lastDiff = i == count - 1;
                            EqualizeSvInDiff(taikoDiffs[i].FullName, lastDiff);
                        }
                    }
                    else
                    {
                        // Add a backup and equalize SV.
                        AddBackup();
                        EqualizeSvInDiff(path, true);
                    }
                }
                else
                {
                    // Add a backup and equalize SV.
                    AddBackup();
                    EqualizeSvInDiff(path, true);
                }
            }));
        }

        private void EqualizeSvInDiff(string filePath, bool isLastDiff)
        {
            //TODO
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
            formHandlerPanel.SetForm(new Metadata_manager(path, lines, metadataManager =>
            {
                if (metadataManager.isSuccess)
                {
                    SelectFile();
                    manageLoad();
                }
            }));
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
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    SelectFile();
                    break;
                /*case 1:
                    Hitsounds();
                    break;*/
                /*case 2:
                    PrepareMapToHitsounding();
                    break;*/
                case 1:
                    WhistleToClap();
                    break;
                case 2:
                    PositionNotes();
                    break;
                /*case 5:
                    NewTiming();
                    break;*/
                case 3:
                    ChangeBPM();
                    break;
                /*case 7:
                    ChangeOffset();
                    break;*/
                case 4:
                    SmoothSvChanger();
                    break;
                case 5:
                    SVequalizer();
                    break;
                case 6:
                    SVchanger();
                    break;
                case 7:
                    SVstepbystep();
                    break;
                case 8:
                    VolumeChanger();
                    break;
                case 9:
                    VolumeStepByStep();
                    break;
                /*case 14:
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
                    break;*/
                case 10:
                    MetadataManager();
                    break;
                default:
                    ShowMode.Error(language.LanguageContent[Language.noFunctionSelected]);
                    break;
            }
        }
        private void applyFunctionButton_Click(object sender, EventArgs e) // Apply Function
        {
            
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
