using MessageBoxMode;
using System.IO;
using System.Text;

namespace Manage_Beatmap
{
    public class Language
    {
        public string[] LanguageContent { get; }
        public string SelectedLanguage { get; set; }
        #region LanguageContentIndexes
        #region ErrorsAndWarnings
        #region ManageBeatmap
        public const int osuManage = 0;
        public const int osuFiles = 1;
        public const int noteTimingPositive = 2;
        public const int noteTimingNegative = 3;
        public const int timingPointNotFound = 4;
        public const int askBackup = 5;
        public const int overwriteBackup = 6;
        public const int notesShift = 7;
        public const int snapWarning = 8;
        public const int processComplete = 9;
        public const int backupSaved = 10;
        public const int offsetChanger = 11;
        public const int offsetChange = 12;
        public const int timingOffsetsChanged = 13;
        public const int noInputDetected = 14;
        public const int onInheritedChangeTimingNotFound = 15;
        public const int processCompleteWithBackup = 16;
        public const int replyFromEdit = 17;
        public const int containsGreenPoints = 18;
        public const int noTimingPoints = 19;
        public const int oneRowOnly = 20;
        public const int oneRowRequired = 21;
        public const int inheritedPointBPMError = 22;
        public const int offsetShift = 23;
        public const int processAborted = 24;
        public const int selectPointsWithLast = 25;
        public const int overlappedTimingPointsWarning = 26;
        public const int areYouSure = 27;
        public const int onlyInheritedPoint = 28;
        public const int sliderVelocityChanger = 29;
        public const int sliderVelocityChange = 30;
        public const int lastSV = 31;
        public const int volumeChanger = 32;
        public const int lastVolume = 33;
        public const int notFloatValue = 34;
        public const int noTimingPointsOrNotes = 35;
        public const int unsnappedNotesOrTimingPointsDetected = 36;
        public const int SVchangesAdded = 37;
        public const int mayNotBeSnapped = 38;
        public const int allSVchangesSnapped = 39;
        public const int noFunctionSelected = 40;
        public const int areYouSureYouWantToExit = 41;
        public const int deleteAllGreenPoints = 42;
        #endregion
        #region BPMchanger
        public const int onlyNumbersAndComma = 113;
        public const int sepearateWithComma = 114;
        #endregion
        #region Hitsounds
        public const int onlyWavAllowed = 43;
        public const int delete = 44;
        public const int buttonNotFound = 45;
        public const int hitsoundsSaved = 46;
        public const int noHitsounds = 47;
        public const int areYouSureToSaveHitsounds = 48;
        public const int saveSongFolder = 49;
        public const int selectTheFolder = 50;
        #endregion
        #region SVchanger
        public const int oneOrMoreValuesEmpty = 51;
        public const int enterPositiveToCount = 52;
        public const int selectGridSnap = 53;
        public const int BPMwrong = 54;
        public const int optionalInitialIsFirst = 55;
        public const int optionalDefaultIsMinusThree = 178;
        public const int SVchangesWrong = 56;
        public const int timeExpressionDoesNotMatch = 57;
        public const int rememberSnappingNotes = 58;
        public const int areYouSureToContinue = 59;
        public const int rememberSnappingNotesBetweenArea = 60;
        #endregion
        #region SVequalizer
        public const int BPMonlyNumberAndSeperatedComma = 61;
        public const int removeAllSVchanges = 62;
        public const int selectSVwithBPM = 63;
        public const int SVvalueFormat = 64;
        #endregion
        #endregion
        #region FormControlTexts
        #region ManageBeatmap
        public const int manageBeatmapTopLabel = 65;
        public const int manageBeatmapKeysLabel = 66;
        public const int selectFileButton = 67;
        public const int addHitsoundsToFolderButton = 68;
        public const int setAllWhistleToClapsButton = 69;
        public const int positionNotesButton = 70;
        public const int changeBPMofSelectedPointButton = 71;
        public const int changeOffsetsOfSelectedPointsButton = 72;
        public const int addInheritedPointsToChangeSVsmoothlyButton = 73;
        public const int equalizeSVforAllTimingPointsButton = 74;
        public const int increaseSVstepByStepButton = 75;
        public const int increaseOrDecreaseSVsButton = 76;
        public const int changeVolumesButton = 77;
        public const int changeVolumesStepByStepButton = 78;
        public const int deleteAllInheritedPointsButton = 79;
        public const int deleteSelectedInheritedPointsButton = 80;
        public const int deleteDuplicatePointsButton = 81;
        public const int copyTagsButton = 82;
        public const int manageBeatmapFormTitle = 83;
        #endregion
        #region BPMchanger
        public const int newBPMlabel = 84;
        public const int applyButton = 85;
        public const int BPMchangerFormTitle = 86;
        #endregion
        #region Hitsounds
        public const int hitsoundsModeLabel = 87;
        public const int clearHitsoundsButton = 88;
        public const int hitsoundsExplanationLabel = 89;
        public const int saveHitsoundsButton = 90;
        public const int hitsoundLabel = 91;
        public const int hitsoundTypeLabel = 92;
        public const int hitsoundSampleLabel = 93;
        public const int playHitsoundButton = 94;
        public const int stopHitsoundButton = 95;
        public const int HitsoundsFormTitle = 96;
        #endregion
        #region SVchanger
        public const int SVchangerTopLabel = 97;
        public const int copyTimeLabel = 98;
        public const int setFirstSVlabel = 99;
        public const int setLastSVlabel = 100;
        public const int countLabel = 101;
        public const int targetBPMlabel = 102;
        public const int gridSnapLabel = 103;
        public const int checkBox = 104;
        public const int addInheritedPointsButton = 105;
        public const int SVchangerFormTitle = 106;
        #endregion
        #region SVequalizer
        public const int enterBPMlabel = 107;
        public const int enterSVlabel = 108;
        public const int typeLabel = 109;
        public const int addComboBox = 110;
        public const int editComboBox = 111;
        public const int SVequalizerFormTitle = 112;
        #endregion
        #region Options
        public const int optionsFormTitle = 115;
        public const int languageLabel = 116;
        public const int firstLanguage = 117;
        public const int secondLanguage = 118;
        public const int thirdLanguage = 119;
        // more languages will be added here
        #endregion
        #region Additionals
        public const int notTaiko = 120;
        public const int undo = 121;
        public const int redo = 122;
        public const int save = 123;
        public const int refresh = 124;
        public const int copyLastTime = 125;
        public const int lastTimeCannotBeSmaller = 126;
        public const int activateBetweenTimeMode = 127;
        public const int selectFiles = 128;
        public const int multiSelectionTags = 129;
        public const int cannotUndo = 130;
        public const int sourceEmpty = 131;
        public const int tagsEmpty = 132;
        public const int sourceAndTagsEmpty = 133;
        public const int partiallyComplete = 134;
        public const int comboBoxSelectedIndex = 135;
        public const int multiSelection = 136;
        public const int volumeChange = 137;
        public const int currentFile = 138;
        public const int manageCurrentFile = 139;
        public const int noFilesSelected = 140;
        public const int msnIntegration = 141;
        public const int msnIntegrationError = 142;
        public const int osuError = 143;
        public const int decimalOffsets = 144;
        public const int notesSaved = 145;
        public const int adjustBookmarks = 146;
        public const int noBookmarks = 147;
        public const int copyMetadata = 148;
        public const int artistOrTitleMissing = 149;
        public const int metadatatagmissing = 150;
        public const int wrongbookmarkformat = 151;
        public const int nobookmarksfoundafterselectedpoint = 152;
        public const int editortagmissing = 153;
        public const int multipleFileBackups = 154;
        public const int savedirectoryformultiplefiles = 155;
        public const int newTimingButton = 156;
        public const int applyFunctionButton = 157;
        public const int timingManagerFormTitle = 158;
        public const int changeType = 159;
        public const int timingContent = 160;
        public const int pasteTimingHere = 161;
        public const int applyTiming = 162;
        public const int timingFormatWrong = 163;
        public const int onlyTimingPoints = 164;
        public const int openTimingDiff = 165;
        public const int firstPointShouldBeTiming = 166;
        public const int allNotesAndTimingPointsSnapped = 167;
        public const int askForMultipleErrorSave = 168;
        public const int errorSavePath = 169;
        public const int timingPointContentError = 170;
        public const int errorSaved = 171;
        public const int reOpenWindow = 172;
        public const int deleteUnneccessaryInheritedPointsButton = 173;
        public const int twoInheritedPointRequired = 174;
        public const int prepareMapToHitsoundingButton = 175;
        public const int noSliderMultiplierFound = 176;
        public const int svOffset = 177;
        #endregion
        #endregion
        #endregion
        private const int size = 179;
        public Language(string lang)
        {
            if (File.Exists("Languages\\" + lang + ".txt"))
            {
                LanguageContent = new string[size];
                LanguageContent = File.ReadAllLines("Languages\\" + lang + ".txt", Encoding.UTF8);
                if (LanguageContent.Length != size)
                {
                    LanguageContent = new string[size];
                    ShowMode.Error("Language file is corrupted. Default is set to English.");
                    SetEnglishLanguage();
                }
                else
                {
                    ExtractValues();
                    SaveToConfig(lang);
                    if (lang.Equals("Turkish"))
                        SelectedLanguage = "Türkçe";
                }
            }
            else
            {
                ShowMode.Error("File not found. Default is set to English.");
                LanguageContent = new string[size];
                SetEnglishLanguage();
            }
        }
        public Language() { LanguageContent = new string[size]; SetEnglishLanguage(); }
        private void SaveToConfig(string lang)
        {
            File.WriteAllText("config.cfg", "Language=" + lang);
        }
        public void SetEnglishLanguage()
        {
            LanguageContent[0] = "Please, select the .osu file you want to manage.";
            LanguageContent[1] = "osu! files";
            LanguageContent[2] = "Notes' timings has changed successfully. However, there might be some notes which aren't snapped, especially spinners and notes that placed before the first timing point, if edited. Always check aimod.";
            LanguageContent[3] = "There are no notes found after selected timing point's offset.";
            LanguageContent[4] = "The map doesn't contain any timing points. Process aborted.";
            LanguageContent[5] = "This kind of change may require a backup. Do you want a backup on your desktop?";
            LanguageContent[6] = "The same beatmap has a backup on Desktop. Do you want to overwrite?";
            LanguageContent[7] = "Some of the notes may have shifted ± 2ms maximum, alongside with green points. If you exit, then open again, then press \"Resnap All Notes\" at 1/12 grid snap divisior and save the map, this function can edit the green lines that snapped on notes themselves. Do you want the adjustment?";
            LanguageContent[8] = "Remember to exit, open it in editor again and \"save the beatmap first\" after you resnapped all notes.";
            LanguageContent[9] = "Process complete.";
            LanguageContent[10] = "A backup of this beatmap has been saved to Desktop.";
            LanguageContent[11] = "Offset changer";
            LanguageContent[12] = "Offset change:";
            LanguageContent[13] = "The timing offsets has been changed successfully.";
            LanguageContent[14] = "No input detected or the value was 0, process aborted.";
            LanguageContent[15] = "One of the inherited points didn't have a BPM value to calculate SV on it. Process aborted.";
            LanguageContent[16] = "Process complete, the backup has been saved.";
            LanguageContent[17] = "Reply from edit: No \"rows\" selected. Rows that contain BPM can also be selected, the program will only edit the inherited points.";
            LanguageContent[18] = "The map contains green points, delete all of them, press F5 and start the process again.";
            LanguageContent[19] = "An error has occured during the search of \"[TimingPoints]\" or there have been no timing points found. Please check the .osu file.";
            LanguageContent[20] = "On BPM Changes, you must select only one row to process.";
            LanguageContent[21] = "You have to select at least one row.";
            LanguageContent[22] = "The inherited points' BPM cannot be changed.";
            LanguageContent[23] = "Beware that 1ms unsnapped objects may cause unsnapped object results alongside with the green notes. Make sure that all of the snaps are correct. Are you sure you want to continue?";
            LanguageContent[24] = "Process aborted.";
            LanguageContent[25] = "It is highly recommended to select timing points with the last timing point in the map, otherwise the bottom non-selected ones won't shift. Are you sure you want to continue?";
            LanguageContent[26] = "Check if the timing points overlapped because of the selection.";
            LanguageContent[27] = "Are you sure?";
            LanguageContent[28] = "You have to select only the rows of an inherited point.";
            LanguageContent[29] = "Slider Velocity Changer";
            LanguageContent[30] = "SV Change:";
            LanguageContent[31] = "Last SV:";
            LanguageContent[32] = "Volume Changer";
            LanguageContent[33] = "Last volume:";
            LanguageContent[34] = "The value should not be a float value.";
            LanguageContent[35] = "This map has no timing points or notes. Process aborted.";
            LanguageContent[36] = "Unsnapped notes or timing points detected. Process aborted. You can go to the position by clicking \"Yes\" at ";
            LanguageContent[37] = "The SV changes are added.";
            LanguageContent[38] = "However, some of them may not correctly snapped. If you have notes on the snaps already, the program will fix the snappings related to the notes. Do you want to run that function?";
            LanguageContent[39] = "All SV changes are snapped.";
            LanguageContent[40] = "No function selected, process aborted.";
            LanguageContent[41] = "Are you sure you want to exit?";
            LanguageContent[42] = "Warning: This function will delete all green points. Are you sure you want to continue?";
            LanguageContent[43] = "Only \".wav\" extension is allowed. The rest of the files are not included.";
            LanguageContent[44] = "Delete";
            LanguageContent[45] = "Button not found.";
            LanguageContent[46] = "The hitsounds are saved.";
            LanguageContent[47] = "There are no custom hitsounds available.";
            LanguageContent[48] = "Are you sure to save the hitsounds?";
            LanguageContent[49] = "Do you want to save the hitsounds to the song folder?";
            LanguageContent[50] = "Please select the folder.";
            LanguageContent[51] = "One or more values are empty.";
            LanguageContent[52] = "Please enter a positive value to the count section.";
            LanguageContent[53] = "Please select a grid snap.";
            LanguageContent[54] = "BPM value has entered wrong. Example: 120 or 175,25";
            LanguageContent[55] = "optional, initial is first";
            LanguageContent[56] = "One of the SV changes are wrong. Example: 1 or 1,25 (dot or comma, depending on the decimal separator of your language.)";
            LanguageContent[57] = "The time expression does not match. Required parts: \"00:00:000\" or \"0:00:00:000\". You can use \"05:21:234 - \" for example.";
            LanguageContent[58] = "Important: Remember to snap all the notes correctly to get an accurate result. Are you sure you want to continue?";
            LanguageContent[59] = "Are you sure you want to continue?";
            LanguageContent[60] = "Remember to snap all the notes correctly in that area. Otherwise the change may not be smooth.";
            LanguageContent[61] = "The BPM should be only number, seperated by decimal separator. A value should be selected from drop down list as well.";
            LanguageContent[62] = "Warning: Before starting this process, remove all the SV changes. Are you sure you want to continue?";
            LanguageContent[63] = "Select SVs with the BPM values above (at least one BPM), so the program will calculate correctly. Are you sure you want to continue?";
            LanguageContent[64] = "SV value format is wrong. Example: 1 or 1,05 (localized decimal separator works.)";
            LanguageContent[65] = "If you want to increase or decrease step by step from bottom to top, select the related rows from bottom to top.It will take the first selected value as the base value and calculates the related values automatically based on your input at the other window. \"Always select as rows, otherwise you'll get an error (except hitsounds and add smooth SV change button).\" Normally the program detects Ctrl + S while osu! is open with editor mode, but it should be checked. Last update: ";
            LanguageContent[66] = "The key combinations are in Options.";
            LanguageContent[67] = "Select File";
            LanguageContent[68] = "Add hitsounds to the folder";
            LanguageContent[69] = "Set all whistle sounds to claps";
            LanguageContent[70] = "Position the notes (Taiko mode)";
            LanguageContent[71] = "Change BPM of the selected timing point";
            LanguageContent[72] = "Change offsets of the selected timing points";
            LanguageContent[73] = "Add inherited points to change SV smoothly";
            LanguageContent[74] = "Equalize the SV for all timing points";
            LanguageContent[75] = "Increase SV step by step of selected inherited points";
            LanguageContent[76] = "Increase or decrease SV's of selected inherited points";
            LanguageContent[77] = "Change volumes of selected points";
            LanguageContent[78] = "Increase volume step by step of selected rows";
            LanguageContent[79] = "Delete all inherited points";
            LanguageContent[80] = "Delete selected inherited points";
            LanguageContent[81] = "Delete duplicate timing and inherited points";
            LanguageContent[82] = "Copy metadata to the selected files";
            LanguageContent[83] = "Manage Beatmap";
            LanguageContent[84] = "New BPM: ";
            LanguageContent[85] = "Apply!";
            LanguageContent[86] = "BPM Changer";
            LanguageContent[87] = "Mode:";
            LanguageContent[88] = "Clear hitsounds";
            LanguageContent[89] = "Drop files here. It'll play once after you drop one. It is highly recommended to lower the volume if you are thinking about dropping multiple hitsounds, because they'll play concurrently. Additionally: This tool is created for creating hitsounds only, not editing. The program will save the hitsounds based on the maximum value. For example: you have normal-hitnormal, normal-hitnormal2 and normal-hitnormal 5. The program will save the normal hitsounds starting from normal-hitnormal6.";
            LanguageContent[90] = "Save!";
            LanguageContent[91] = "Sound: ";
            LanguageContent[92] = "Hitsound type: ";
            LanguageContent[93] = "Sample: ";
            LanguageContent[94] = "Play!";
            LanguageContent[95] = "Stop!";
            LanguageContent[96] = "Hitsound Manager";
            LanguageContent[97] = "It is highly recommended to selecting the grid snap relatively. For example; if you are on 1/8 snap don't use 4/1, if you do it the results will be wrong.\r\n\r\nThe SV offset is for shifting the resulting inherited points by that amount of milliseconds.";
            LanguageContent[98] = "Copy the time:";
            LanguageContent[99] = "Set first SV:";
            LanguageContent[100] = "Set last SV:";
            LanguageContent[101] = "Count:";
            LanguageContent[102] = "Target BPM:";
            LanguageContent[103] = "Grid snap:";
            LanguageContent[104] = "Put points by note snaps";
            LanguageContent[105] = "Add inherited points";
            LanguageContent[106] = "Slider Velocity Changer";
            LanguageContent[107] = "Enter BPM:";
            LanguageContent[108] = "Enter SV:";
            LanguageContent[109] = "Type:";
            LanguageContent[110] = "Add (remove all SV)";
            LanguageContent[111] = "Edit (select points before enter)";
            LanguageContent[112] = "Slider Velocity Equalizer";
            LanguageContent[113] = "The value cannot contain anything but numbers and decimal separator.";
            LanguageContent[114] = "This is a mistake, please point out to me when this happens.";
            LanguageContent[optionsFormTitle] = "Options";
            LanguageContent[languageLabel] = "Language: ";
            LanguageContent[firstLanguage] = "English";
            LanguageContent[secondLanguage] = "Turkish";
            LanguageContent[thirdLanguage] = "French";
            LanguageContent[notTaiko] = "The selected file is not a taiko map.";
            LanguageContent[undo] = "Undo";
            LanguageContent[redo] = "Redo";
            LanguageContent[save] = "Save";
            LanguageContent[refresh] = "Refresh";
            LanguageContent[copyLastTime] = "Copy last time:";
            LanguageContent[lastTimeCannotBeSmaller] = "The 2nd time cannot be smaller than or equal the first time.";
            LanguageContent[activateBetweenTimeMode] = "Activate between time mode";
            LanguageContent[selectFiles] = "Select files";
            LanguageContent[multiSelectionTags] = "Select the files to copy the metadata and bookmarks.";
            LanguageContent[cannotUndo] = "Beware that this operation will not be undone, so if you are unsure, just click Cancel to abort the process.";
            LanguageContent[sourceEmpty] = "Source is empty, are you sure to set the other file sources as empty? Yes to apply, No to abort the process, Cancel to skip this file.";
            LanguageContent[tagsEmpty] = "Tags are empty, are you sure to set the other file tags as empty? Yes to apply, No to abort the process, Cancel to skip this file.";
            LanguageContent[sourceAndTagsEmpty] = "Source and tags are empty, are you sure to set the other file sources and tags as empty? Yes to apply, No to abort the process, Cancel to skip this file.";
            LanguageContent[partiallyComplete] = "The process has partially completed. The changed files count is: ";
            LanguageContent[comboBoxSelectedIndex] = "An item must be selected from drop down list.";
            LanguageContent[multiSelection] = "Select the files. (Multi selection is available.)";
            LanguageContent[volumeChange] = "Volume change:";
            LanguageContent[currentFile] = "The current file is: ";
            LanguageContent[manageCurrentFile] = "The program will manage the current file first.";
            LanguageContent[noFilesSelected] = "No files selected, process aborted.";
            LanguageContent[msnIntegration] = "Do you want to use MSN integration? (Open MSN integration from Options and reopen the song from song list.)";
            LanguageContent[msnIntegrationError] = "Remember to open MSN integration from Options and reopen the song from song list.";
            LanguageContent[osuError] = "osu! needs to be open to apply this process.";
            LanguageContent[decimalOffsets] = "There are some decimal timing point offsets which need to be fixed in order to use the program. After changing it from the file, press F5 to refresh.";
            LanguageContent[notesSaved] = "Hitsound notes have been saved as \"notes.txt\" in the same folder. Do you want to open it?";
            LanguageContent[adjustBookmarks] = "Adjust bookmarks";
            LanguageContent[noBookmarks] = "No bookmarks found on the map.";
            LanguageContent[copyMetadata] = "Copy metadata and bookmarks";
            LanguageContent[artistOrTitleMissing] = "Artist and/or title are missing.";
            LanguageContent[metadatatagmissing] = "On the current file, metadata tag is missing.";
            LanguageContent[wrongbookmarkformat] = "Bookmark format is wrong, process aborted.";
            LanguageContent[nobookmarksfoundafterselectedpoint] = "No bookmarks found after the selected point. No change has been done on bookmarks.";
            LanguageContent[editortagmissing] = "On the current file, metadata tag is missing.";
            LanguageContent[multipleFileBackups] = "You have selected the multiple file editing option. Undo for the files (except the current open one) are not possible after this process. Do you want backups to the desktop?";
            LanguageContent[savedirectoryformultiplefiles] = "The files will be saved at this location: ";
            LanguageContent[newTimingButton] = "Insert new timing";
            LanguageContent[applyFunctionButton] = "Apply function";
            LanguageContent[timingManagerFormTitle] = "Timing Manager";
            LanguageContent[changeType] = "Change Type:";
            LanguageContent[timingContent] = "Timing Content";
            LanguageContent[pasteTimingHere] = "Paste timing points here. They should only contain red points.";
            LanguageContent[applyTiming] = "Apply timing";
            LanguageContent[timingFormatWrong] = "The pasted timing format is wrong. It only should contain numbers, dot and comma. The first mistaken line is at: ";
            LanguageContent[onlyTimingPoints] = "Only timing points are allowed. There is an inherited point at: ";
            LanguageContent[openTimingDiff] = "It is recommended to create a timing diff specific for this case, if any unsnapped notes or timing points found the program will able to redirect its position.";
            LanguageContent[firstPointShouldBeTiming] = "The first point of the map should be a red point. Process aborted.";
            LanguageContent[allNotesAndTimingPointsSnapped] = "All of the timing points, inherited points and notes should be snapped, including the edited timing version. The program will warn you where the unsnapped object is, so you can fix it.";
            LanguageContent[askForMultipleErrorSave] = "There are multiple unsnapped objects detected throughout the map. By pressing \"Yes\" you can save the errors as editor links to the desktop.";
            LanguageContent[errorSavePath] = "The errors will be saved to: ";
            LanguageContent[timingPointContentError] = "Inserted timing point content has snapping errors. If you have the timing difficulty open on osu!, click Yes to generate errors.";
            LanguageContent[errorSaved] = "Errors have been saved.";
            LanguageContent[reOpenWindow] = "Re-open the window with same values";
            LanguageContent[deleteUnneccessaryInheritedPointsButton] = "Delete unneccessary inherited points";
            LanguageContent[twoInheritedPointRequired] = "At least two inherited points are required on the map.";
            LanguageContent[prepareMapToHitsoundingButton] = "Prepare the map to hitsounding";
            LanguageContent[noSliderMultiplierFound] = "The property \"SliderMultiplier\" has not been found on the file. Process aborted.";
            LanguageContent[optionalDefaultIsMinusThree] = "optional, default is -3";
            LanguageContent[svOffset] = "SV Offset:";
            SelectedLanguage = "English";
            SaveToConfig(SelectedLanguage);
        }
        private void ExtractValues()
        {
            for(int i=0;i<LanguageContent.Length;i++)
                LanguageContent[i] = LanguageContent[i].Substring(LanguageContent[i].IndexOf("=") + 1);
        }
    }
}
