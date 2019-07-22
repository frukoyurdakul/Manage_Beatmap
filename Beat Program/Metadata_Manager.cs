using FindIndex;
using MessageBoxMode;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Manage_Beatmap
{
    public partial class Metadata_manager : Form
    {
        public bool isSuccess { get; internal set; } = false;
        string path;
        List<string> lines = new List<string>();
        public Metadata_manager(string path, string[] lines)
        {
            this.lines = lines.ToList();
            this.path = path;
            InitializeComponent();
        }

        #region Main functions
        private string[] setValues(List<string> lines, string title, string titleUnicode, string artist, string artistUnicode, string bookmarks, string source, string tags)
        {
            bool success = false, isBookmarkTabExist = false;
            int editorIndex = -1, metadataIndex = -1;
            for (int i = 0; i < lines.Count; i++) if(lines[i] == "[Editor]") { editorIndex = i; break; }
            for (int i = 0; i < lines.Count; i++) if (lines[i] == "[Metadata]") { metadataIndex = i; break; }
            if (metadataIndex != -1)
            {
                if (editorIndex != -1)
                {
                    for (int i = editorIndex + 1; !string.IsNullOrWhiteSpace(lines[i]); i++)
                    {
                        if (lines[i].Contains("Bookmarks:") && !string.IsNullOrEmpty(bookmarks))
                        {
                            lines[i] = "Bookmarks: " + bookmarks;
                            isBookmarkTabExist = true;
                            break;
                        }
                    }
                }
                if (!isBookmarkTabExist && !string.IsNullOrEmpty(bookmarks) && editorIndex != -1)
                    lines.Insert(editorIndex + 1, "Bookmarks: " + bookmarks);
                for(int i = metadataIndex + 1; !string.IsNullOrWhiteSpace(lines[i]); i++)
                {
                    if (lines[i].Contains("TitleUnicode:"))
                        lines[i] = "TitleUnicode:" + titleUnicode;
                    else if (lines[i].Contains("Title"))
                        lines[i] = "Title:" + title;
                    else if (lines[i].Contains("ArtistUnicode:"))
                        lines[i] = "ArtistUnicode:" + artistUnicode;
                    else if (lines[i].Contains("Artist:"))
                        lines[i] = "Artist:" + artist;
                    else if (lines[i].Contains("Source:"))
                        lines[i] = "Source:" + source;
                    else if (lines[i].Contains("Tags:"))
                        lines[i] = "Tags:" + tags; 
                }
                success = true;
            }
            if (success)
                return lines.ToArray();
            else
                return null;
        }
        #endregion
        #region Form control functions
        private void titleTextBox_TextChanged(object sender, EventArgs e)
        {
            romanisedTitleTextBox.Enabled = false;
            for(int i = 0; i < titleTextBox.Text.Length; i++)
            {
                if (titleTextBox.Text[i] < 32 || titleTextBox.Text[i] > 126)
                {
                    romanisedTitleTextBox.Enabled = true;
                    break;
                }
            }
            if (titleTextBox.Text[titleTextBox.Text.Length - 1] == ' ')
                romanisedTitleTextBox.Enabled = true;
            if (romanisedTitleTextBox.Enabled == false)
                romanisedTitleTextBox.Text = titleTextBox.Text;
        }

        private void artistTextBox_TextChanged(object sender, EventArgs e)
        {
            romanisedArtistTextBox.Enabled = false;
            for (int i = 0; i < artistTextBox.Text.Length; i++)
            {
                if (artistTextBox.Text[i] < 32 || artistTextBox.Text[i] > 126)
                {
                    romanisedArtistTextBox.Enabled = true;
                    break;
                }
            }
            if(artistTextBox.Text[artistTextBox.Text.Length - 1] == ' ')
                romanisedArtistTextBox.Enabled = true;
            if(romanisedArtistTextBox.Enabled == false)
                romanisedArtistTextBox.Text = artistTextBox.Text;
        }

        private void Metadata_manager_Load(object sender, EventArgs e)
        {
            button.Text = Manage_Beatmap.language.LanguageContent[Language.copyMetadata];
            int counter = 0;
            for(int i = 0; i < lines.Count; i++)
            {
                if (counter == 7)
                    break;
                if (lines[i].Contains("Bookmarks:"))
                {
                    if (lines[i] != "Bookmarks:")
                        bookmarksTextBox.Text = lines[i].Substring(lines[i].IndexOf(':') + 2);
                    else
                        bookmarksTextBox.Text = string.Empty;
                    counter++;
                }
                else if (lines[i].Contains("TitleUnicode:"))
                {
                    if (lines[i] != "TitleUnicode:")
                        romanisedTitleTextBox.Text = lines[i].Substring(lines[i].IndexOf(':') + 1);
                    else
                        romanisedTitleTextBox.Text = string.Empty;
                    counter++;
                }
                else if (lines[i].Contains("Title:"))
                {
                    if (lines[i] != "Title:")
                        titleTextBox.Text = lines[i].Substring(lines[i].IndexOf(':') + 1);
                    else
                        titleTextBox.Text = string.Empty;
                    counter++;
                }
                else if (lines[i].Contains("ArtistUnicode:"))
                {
                    if (lines[i] != "ArtistUnicode:")
                        romanisedArtistTextBox.Text = lines[i].Substring(lines[i].IndexOf(':') + 1);
                    else
                        romanisedArtistTextBox.Text = string.Empty;
                    counter++;
                }
                else if (lines[i].Contains("Artist:"))
                {
                    if (lines[i] != "Artist:")
                        artistTextBox.Text = lines[i].Substring(lines[i].IndexOf(':') + 1);
                    else
                        artistTextBox.Text = string.Empty;
                    counter++;
                }
                else if (lines[i].Contains("Source:"))
                {
                    if (lines[i] != "Source:")
                        sourceTextBox.Text = lines[i].Substring(lines[i].IndexOf(':') + 1);
                    else
                        sourceTextBox.Text = string.Empty;
                    counter++;
                }
                else if (lines[i].Contains("Tags:"))
                {
                    if (lines[i] != "Tags:")
                        tagsTextBox.Text = lines[i].Substring(lines[i].IndexOf(':') + 1);
                    else
                        tagsTextBox.Text = string.Empty;
                    counter++;
                }
            }
        }

        private void button_Click(object sender, EventArgs e)
        {
            string title, titleUnicode, artist, artistUnicode, bookmarks, source, tags;
            bookmarks = bookmarksTextBox.Text.Replace(" ", "");
            if(bookmarks[0] >= 48 && bookmarks[0] <= 57 && bookmarks[bookmarks.Length - 1] >= 48 && bookmarks[bookmarks.Length - 1] <= 57)
            {
                for(int i = 0; i < bookmarks.Length; i++)
                {
                    if (bookmarks[i] >= 48 && bookmarks[i] <= 57) { }
                    else if (bookmarks[i] == ',') { }
                    else
                    {
                        ShowMode.Error(Manage_Beatmap.language.LanguageContent[Language.wrongbookmarkformat]);
                        return;
                    }
                }
            }
            else
            {
                ShowMode.Error(Manage_Beatmap.language.LanguageContent[Language.wrongbookmarkformat]);
                return;
            }
            title = titleTextBox.Text.TrimStart().TrimEnd();
            titleUnicode = romanisedTitleTextBox.Text.TrimStart().TrimEnd();
            artist = artistTextBox.Text.TrimStart().TrimEnd();
            artistUnicode = romanisedArtistTextBox.Text.TrimStart().TrimEnd();
            source = sourceTextBox.Text.TrimStart().TrimEnd();
            tags = tagsTextBox.Text.TrimStart().TrimEnd();
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(artist))
                ShowMode.Error(Manage_Beatmap.language.LanguageContent[Language.artistOrTitleMissing]);
            else
            {
                string[] lines;
                ShowMode.Information(Manage_Beatmap.language.LanguageContent[Language.multiSelectionTags]);
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.InitialDirectory = path.Substring(0, path.LastIndexOf('\\'));
                dialog.Filter = Manage_Beatmap.language.LanguageContent[Language.osuFiles] + " (*.osu,*.OSU) | *.osu;*.OSU";
                dialog.Multiselect = true;
                dialog.Title = Manage_Beatmap.language.LanguageContent[Language.selectFiles];
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string[] paths = dialog.FileNames;
                    string[] fileNames = dialog.SafeFileNames;
                    List<string> mapper = new List<string>();
                    List<string> version = new List<string>();
                    for (int i = 0; i < fileNames.Length; i++)
                    {
                        string currentFileName = fileNames[i];
                        if (currentFileName.IndexOf('(') != -1 && currentFileName.IndexOf(')') != -1)
                            mapper.Add(currentFileName.Substring(currentFileName.IndexOf('(') + 1, currentFileName.IndexOf(')') - currentFileName.IndexOf('(') - 1));
                        if (currentFileName.IndexOf('[') != -1 && currentFileName.IndexOf(']') != -1)
                            version.Add(currentFileName.Substring(currentFileName.IndexOf('[') + 1, currentFileName.IndexOf(']') - currentFileName.IndexOf('[') - 1));
                    }
                    int slashCount = paths[0].SearchCharCount('\\');
                    string directoryFolder = paths[0].Substring(0, paths[0].LastIndexOf('\\'));
                    for (int i = 0; i < paths.Length; i++)
                    {
                        lines = File.ReadAllLines(paths[i]);
                        lines = setValues(lines.ToList(), title, titleUnicode, artist, artistUnicode, bookmarks, source, tags);
                        if (lines != null)
                            File.WriteAllLines(paths[i], lines);
                        else
                            ShowMode.Error(Manage_Beatmap.language.LanguageContent[Language.metadatatagmissing] + " " +
                                Manage_Beatmap.language.LanguageContent[Language.currentFile] + paths[i]);
                    }
                    Thread.Sleep(500);
                    if (mapper.Count == paths.Length && version.Count == paths.Length)
                    {
                        for (int i = 0; i < paths.Length; i++)
                        {
                            File.Copy(paths[i],
                                directoryFolder + "\\" + artistUnicode + " - " + titleUnicode + " (" + mapper[i] + ") [" + version[i] + "].osu", true);
                        }
                        Thread.Sleep(500);
                        for (int i = 0; i < paths.Length; i++)
                            try
                            {
                                File.Delete(paths[i]);
                            }
                            catch (InvalidOperationException ex)
                            {
                                ShowMode.Error(ex.Message);
                            }
                        ShowMode.Information(Manage_Beatmap.language.LanguageContent[Language.processComplete]);
                        isSuccess = true;
                        Close();
                    }
                }
                else
                    ShowMode.Error(Manage_Beatmap.language.LanguageContent[Language.noFilesSelected]);
            }
        }

        private void Metadata_manager_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.UserClosing)
                e.Cancel = true;
        }

        private void textBox_Enter(object sender, EventArgs e)
        {
            TextBox obj = sender as TextBox;
            obj.Select(obj.Text.Length, 0);
        }
        #endregion
    }
}
