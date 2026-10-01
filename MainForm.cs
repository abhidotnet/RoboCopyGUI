using System.Diagnostics;
using System.Text;

namespace RoboCopyGui;

public sealed class MainForm : Form
{
    private readonly ToolTip _toolTip = new()
    {
        AutoPopDelay = 14000,
        InitialDelay = 400,
        ReshowDelay = 200
    };

    private readonly TextBox _sourceTextBox = new();
    private readonly TextBox _destinationTextBox = new();
    private readonly TextBox _filesTextBox = new() { Text = "*.*" };
    private readonly TextBox _excludeFilesTextBox = new();
    private readonly TextBox _excludeDirectoriesTextBox = new();

    private readonly CheckBox _copySubdirectoriesCheckBox = Option("Copy subdirectories (/S)");
    private readonly CheckBox _copyEmptyDirectoriesCheckBox = Option("Copy empty subdirectories too (/E)");
    private readonly CheckBox _purgeCheckBox = Option("Delete destination files that are gone from the source (/PURGE)", Color.DarkRed);
    private readonly CheckBox _mirrorCheckBox = Option("Mirror source to destination (/MIR)", Color.DarkRed);
    private readonly CheckBox _moveFilesCheckBox = Option("Move files, then delete them from the source (/MOV)", Color.DarkRed);
    private readonly CheckBox _moveFilesAndDirectoriesCheckBox = Option("Move files and directories, then delete them from the source (/MOVE)", Color.DarkRed);
    private readonly CheckBox _restartableCheckBox = Option("Restartable mode (/Z)");
    private readonly CheckBox _backupModeCheckBox = Option("Backup mode (/B)");
    private readonly CheckBox _restartableBackupCheckBox = Option("Restartable mode, then backup mode if access is denied (/ZB)");
    private readonly CheckBox _unbufferedCheckBox = Option("Unbuffered I/O, for large files (/J)");
    private readonly CheckBox _efsRawCheckBox = Option("Copy encrypted files in EFS raw mode (/EFSRAW)");
    private readonly CheckBox _createOnlyCheckBox = Option("Create the directory tree and zero-length files only (/CREATE)");
    private readonly CheckBox _fatNamesCheckBox = Option("Use 8.3 FAT file names (/FAT)");
    private readonly CheckBox _disableLongPathsCheckBox = Option("Turn off paths longer than 256 characters (/256)");
    private readonly CheckBox _symlinksCheckBox = Option("Copy symbolic links as links (/SL)");
    private readonly CheckBox _junctionsCheckBox = Option("Copy junctions as junctions (/SJ)");
    private readonly CheckBox _noOffloadCheckBox = Option("Do not use Windows copy offload (/NOOFFLOAD)");
    private readonly CheckBox _compressCheckBox = Option("Request network compression (/COMPRESS)");
    private readonly CheckBox _noCloneCheckBox = Option("Do not use block cloning (/NOCLONE)");

    private readonly NumericUpDown _levelsNumeric = new()
    {
        Minimum = 0,
        Maximum = 1000,
        Value = 0,
        Width = 90
    };

    private readonly ComboBox _sparseCombo = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Width = 360
    };

    private readonly CheckBox _archiveOnlyCheckBox = Option("Only files with the archive attribute (/A)");
    private readonly CheckBox _archiveAndResetCheckBox = Option("Only archive files, then clear the attribute (/M)");
    private readonly CheckBox _excludeChangedCheckBox = Option("Exclude changed files (/XC)");
    private readonly CheckBox _excludeNewerCheckBox = Option("Exclude newer files (/XN)");
    private readonly CheckBox _excludeOlderCheckBox = Option("Exclude older files (/XO)");
    private readonly CheckBox _excludeExtraCheckBox = Option("Exclude extra destination files and directories (/XX)");
    private readonly CheckBox _excludeLonelyCheckBox = Option("Exclude lonely files and directories (/XL)");
    private readonly CheckBox _includeSameCheckBox = Option("Include files that are already the same (/IS)");
    private readonly CheckBox _includeTweakedCheckBox = Option("Include tweaked files (/IT)");
    private readonly CheckBox _includeModifiedCheckBox = Option("Include files whose change time differs (/IM)");
    private readonly CheckBox _fatFileTimesCheckBox = Option("Assume FAT file times, with 2-second granularity (/FFT)");
    private readonly CheckBox _dstCheckBox = Option("Compensate for a one-hour DST difference (/DST)");
    private readonly CheckBox _excludeLinksCheckBox = Option("Exclude symbolic links and junctions (/XJ)");
    private readonly CheckBox _excludeDirLinksCheckBox = Option("Exclude directory links and junctions (/XJD)");
    private readonly CheckBox _excludeFileLinksCheckBox = Option("Exclude file symbolic links (/XJF)");

    private readonly TextBox _includeAttributesTextBox = new();
    private readonly TextBox _excludeAttributesTextBox = new();
    private readonly TextBox _maxSizeTextBox = new();
    private readonly TextBox _minSizeTextBox = new();
    private readonly TextBox _maxAgeTextBox = new();
    private readonly TextBox _minAgeTextBox = new();
    private readonly TextBox _maxLastAccessTextBox = new();
    private readonly TextBox _minLastAccessTextBox = new();

    private readonly CheckBox _copyAllCheckBox = Option("Copy all file information (/COPYALL)");
    private readonly CheckBox _copySecurityCheckBox = Option("Copy security as well as data, attributes and timestamps (/SEC)");
    private readonly CheckBox _secFixCheckBox = Option("Fix security on every file, including skipped files (/SECFIX)");
    private readonly CheckBox _timFixCheckBox = Option("Fix timestamps on every file, including skipped files (/TIMFIX)");
    private readonly CheckBox _noCopyCheckBox = Option("Do not copy file information (/NOCOPY)");
    private readonly CheckBox _noDirectoryCopyCheckBox = Option("Do not copy directory information (/NODCOPY)");

    private readonly CheckBox _copyDataCheckBox = FlagOption("Data (D)", true);
    private readonly CheckBox _copyAttributesCheckBox = FlagOption("Attributes (A)", true);
    private readonly CheckBox _copyTimestampsCheckBox = FlagOption("Timestamps (T)", true);
    private readonly CheckBox _copySecurityFlagCheckBox = FlagOption("Security (S)");
    private readonly CheckBox _copyOwnerCheckBox = FlagOption("Owner (O)");
    private readonly CheckBox _copyAuditingCheckBox = FlagOption("Auditing (U)");
    private readonly CheckBox _copySkipStreamsCheckBox = FlagOption("Skip alternate streams (X)");

    private readonly CheckBox _directoryDataCheckBox = FlagOption("Data (D)", true);
    private readonly CheckBox _directoryAttributesCheckBox = FlagOption("Attributes (A)", true);
    private readonly CheckBox _directoryTimestampsCheckBox = FlagOption("Timestamps (T)");
    private readonly CheckBox _directoryEasCheckBox = FlagOption("Extended attributes (E)");
    private readonly CheckBox _directorySkipStreamsCheckBox = FlagOption("Skip alternate streams (X)");

    private readonly TextBox _addAttributesTextBox = new();
    private readonly TextBox _removeAttributesTextBox = new();

    private readonly NumericUpDown _retryCountNumeric = new()
    {
        Minimum = 0,
        Maximum = 1000000,
        Value = 1,
        Width = 90
    };

    private readonly NumericUpDown _retryWaitNumeric = new()
    {
        Minimum = 0,
        Maximum = 1000000,
        Value = 1,
        Width = 90
    };

    private readonly CheckBox _saveRetryDefaultsCheckBox = Option("Save these retry settings as the defaults in the registry (/REG)", Color.DarkRed);
    private readonly CheckBox _waitForShareCheckBox = Option("Wait for share names to be defined, retrying error 67 (/TBD)");
    private readonly CheckBox _lowFreeSpaceCheckBox = Option("Pause when destination free space gets low (/LFSM)");
    private readonly TextBox _lowFreeSpaceFloorTextBox = new() { Width = 140 };
    private readonly CheckBox _multiThreadCheckBox = Option("Copy with multiple threads (/MT)");
    private readonly NumericUpDown _threadCountNumeric = new()
    {
        Minimum = 1,
        Maximum = 128,
        Value = 8,
        Width = 90,
        Enabled = false
    };
    private readonly NumericUpDown _interPacketGapNumeric = new()
    {
        Minimum = 0,
        Maximum = 1000000,
        Value = 0,
        Width = 90
    };
    private readonly NumericUpDown _monitorChangesNumeric = new()
    {
        Minimum = 0,
        Maximum = 1000000,
        Value = 0,
        Width = 90
    };
    private readonly NumericUpDown _monitorMinutesNumeric = new()
    {
        Minimum = 0,
        Maximum = 1000000,
        Value = 0,
        Width = 90
    };
    private readonly TextBox _runHoursTextBox = new() { Width = 140 };
    private readonly CheckBox _perFileRunHoursCheckBox = Option("Apply run hours to each file (/PF)");
    private readonly TextBox _ioMaxSizeTextBox = new() { Width = 140 };
    private readonly TextBox _ioRateTextBox = new() { Width = 140 };
    private readonly TextBox _thresholdTextBox = new() { Width = 140 };

    private readonly CheckBox _listOnlyCheckBox = Option("List only — do not copy, stamp or delete (/L)");
    private readonly CheckBox _verboseCheckBox = Option("Verbose output, including skipped files (/V)");
    private readonly CheckBox _reportExtraCheckBox = Option("Report all extra files, not only selected ones (/X)");
    private readonly CheckBox _timestampsCheckBox = Option("Include source timestamps (/TS)");
    private readonly CheckBox _fullPathCheckBox = Option("Include the full file path (/FP)");
    private readonly CheckBox _bytesCheckBox = Option("Print sizes in bytes (/BYTES)");
    private readonly CheckBox _noSizeCheckBox = Option("Do not log file sizes (/NS)");
    private readonly CheckBox _noClassCheckBox = Option("Do not log file classes (/NC)");
    private readonly CheckBox _noFileListCheckBox = Option("Do not log file names (/NFL)");
    private readonly CheckBox _noDirectoryListCheckBox = Option("Do not log directory names (/NDL)");
    private readonly CheckBox _showProgressCheckBox = Option("Show estimated time remaining (/ETA)");
    private readonly CheckBox _noProgressCheckBox = Option("Hide Robocopy's own percentage (/NP)");
    private readonly CheckBox _teeCheckBox = Option("Write output to the console and the log (/TEE)");
    private readonly CheckBox _noJobHeaderCheckBox = Option("Hide the job header (/NJH)");
    private readonly CheckBox _noJobSummaryCheckBox = Option("Hide the job summary (/NJS)");
    private readonly CheckBox _unicodeOutputCheckBox = Option("Write status as Unicode (/UNICODE)");

    private readonly TextBox _logFileTextBox = new();
    private readonly ComboBox _logModeCombo = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Width = 320
    };

    private readonly TextBox _jobNameTextBox = new();
    private readonly TextBox _saveJobTextBox = new();
    private readonly CheckBox _quitCheckBox = Option("Quit after reading the command, without copying (/QUIT)");
    private readonly CheckBox _noSourceDirectoryCheckBox = Option("Job file already contains the source (/NOSD)");
    private readonly CheckBox _noDestinationDirectoryCheckBox = Option("Job file already contains the destination (/NODD)");
    private readonly TextBox _advancedOptionsTextBox = new()
    {
        Multiline = true,
        ScrollBars = ScrollBars.Vertical,
        Height = 70
    };

    private readonly TextBox _commandPreviewTextBox = new()
    {
        Multiline = true,
        ReadOnly = true,
        ScrollBars = ScrollBars.Vertical,
        Height = 64,
        Dock = DockStyle.Top,
        Font = new Font(FontFamily.GenericMonospace, 9)
    };

    private readonly RichTextBox _outputTextBox = new()
    {
        ReadOnly = true,
        Dock = DockStyle.Fill,
        Font = new Font(FontFamily.GenericMonospace, 9),
        BackColor = Color.Black,
        ForeColor = Color.Gainsboro
    };

    private readonly ProgressBar _progressBar = new()
    {
        Minimum = 0,
        Maximum = 10000,
        Value = 0,
        Dock = DockStyle.Fill,
        Height = 22,
        Style = ProgressBarStyle.Continuous,
        AccessibleName = "Copy progress"
    };

    private readonly Label _progressPercentLabel = new()
    {
        Text = "0%",
        AutoSize = false,
        Width = 72,
        TextAlign = ContentAlignment.MiddleRight,
        Dock = DockStyle.Fill,
        Font = new Font(SystemFonts.MessageBoxFont!, FontStyle.Bold),
        AccessibleName = "Copy percentage"
    };

    private readonly Label _progressDetailLabel = new()
    {
        Text = "Ready",
        AutoSize = false,
        AutoEllipsis = true,
        Dock = DockStyle.Fill,
        TextAlign = ContentAlignment.MiddleLeft,
        Height = 22,
        AccessibleName = "Copy progress detail"
    };

    private readonly Button _runButton = new()
    {
        Text = "Run Robocopy",
        Width = 130,
        Height = 34
    };

    private readonly Button _cancelButton = new()
    {
        Text = "Cancel",
        Width = 100,
        Height = 34,
        Enabled = false
    };

    private readonly Button _copyCommandButton = new()
    {
        Text = "Copy command",
        Width = 120,
        Height = 34
    };

    private readonly RobocopyProgressTracker _tracker = new();
    private Process? _process;
    private CancellationTokenSource? _scanCancellation;
    private bool _cancelRequested;
    private bool _updatingPreview;

    public MainForm()
    {
        Text = "Robocopy GUI";
        Width = 1180;
        Height = 1020;
        MinimumSize = new Size(980, 760);
        StartPosition = FormStartPosition.CenterScreen;
        AcceptButton = _runButton;

        _sparseCombo.Items.AddRange([
            "Sparse files: leave the default",
            "Keep files sparse (/SPARSE:Y)",
            "Do not keep files sparse (/SPARSE:N)"
        ]);
        _sparseCombo.SelectedIndex = 0;

        _logModeCombo.Items.AddRange([
            "Overwrite the log (/LOG)",
            "Append to the log (/LOG+)",
            "Overwrite a Unicode log (/UNILOG)",
            "Append to a Unicode log (/UNILOG+)"
        ]);
        _logModeCombo.SelectedIndex = 0;

        BuildInterface();
        HookEvents();
        UpdateCommandPreview();
    }

    private void BuildInterface()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 1,
            RowCount = 4
        };

        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 270));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        root.Controls.Add(BuildPathsGroup(), 0, 0);
        root.Controls.Add(BuildOptionsTabs(), 0, 1);
        root.Controls.Add(BuildCommandAndButtonsPanel(), 0, 2);
        root.Controls.Add(BuildOutputGroup(), 0, 3);

        Controls.Add(root);
    }

    private GroupBox BuildPathsGroup()
    {
        var group = new GroupBox
        {
            Text = "Source and destination",
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(10)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 3,
            RowCount = 5
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));

        AddPathRow(layout, 0, "Source:", _sourceTextBox, SelectSourceFolder);
        AddPathRow(layout, 1, "Destination:", _destinationTextBox, SelectDestinationFolder);
        AddTextRow(layout, 2, "Files:", _filesTextBox,
            "File names or wildcards, separated by spaces. Example: *.docx *.xlsx. The default *.* copies every file.");
        AddTextRow(layout, 3, "Exclude files:", _excludeFilesTextBox,
            "Optional names, paths or wildcards. Passed as /XF.");
        AddTextRow(layout, 4, "Exclude folders:", _excludeDirectoriesTextBox,
            "Optional names, paths or wildcards. Passed as /XD.");

        group.Controls.Add(layout);
        return group;
    }

    private TabControl BuildOptionsTabs()
    {
        var tabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Multiline = true
        };

        tabs.TabPages.Add(BuildCopyTab());
        tabs.TabPages.Add(BuildSelectionTab());
        tabs.TabPages.Add(BuildAttributesTab());
        tabs.TabPages.Add(BuildPerformanceTab());
        tabs.TabPages.Add(BuildLoggingTab());
        tabs.TabPages.Add(BuildAdvancedTab());
        return tabs;
    }

    private TabPage BuildCopyTab()
    {
        var page = CreateTabPage("Copy");
        page.Controls.Add(Stack(
            CheckboxGrid(
                _copySubdirectoriesCheckBox,
                _copyEmptyDirectoriesCheckBox,
                _mirrorCheckBox,
                _purgeCheckBox,
                _moveFilesCheckBox,
                _moveFilesAndDirectoriesCheckBox,
                _restartableCheckBox,
                _backupModeCheckBox,
                _restartableBackupCheckBox,
                _unbufferedCheckBox,
                _efsRawCheckBox,
                _createOnlyCheckBox,
                _fatNamesCheckBox,
                _disableLongPathsCheckBox,
                _symlinksCheckBox,
                _junctionsCheckBox,
                _noOffloadCheckBox,
                _compressCheckBox,
                _noCloneCheckBox),
            FieldGrid(
                ("Only the top n folder levels (/LEV):", _levelsNumeric, "0 copies the whole tree. 1 copies only the source folder."),
                ("Sparse files:", _sparseCombo, "Controls whether the sparse state is kept during the copy.")),
            Note("Mirror, purge and move can delete files. Use List only on the Logging tab before running them.")));
        return page;
    }

    private TabPage BuildSelectionTab()
    {
        var page = CreateTabPage("Selection");
        page.Controls.Add(Stack(
            CheckboxGrid(
                _archiveOnlyCheckBox,
                _archiveAndResetCheckBox,
                _excludeChangedCheckBox,
                _excludeNewerCheckBox,
                _excludeOlderCheckBox,
                _excludeExtraCheckBox,
                _excludeLonelyCheckBox,
                _includeSameCheckBox,
                _includeTweakedCheckBox,
                _includeModifiedCheckBox,
                _fatFileTimesCheckBox,
                _dstCheckBox,
                _excludeLinksCheckBox,
                _excludeDirLinksCheckBox,
                _excludeFileLinksCheckBox),
            FieldGrid(
                ("Include attributes (/IA):", _includeAttributesTextBox, "Letters from RASHCNETO. Example: RA copies read-only or archive files."),
                ("Exclude attributes (/XA):", _excludeAttributesTextBox, "Letters from RASHCNETO. Example: SH skips system and hidden files."),
                ("Maximum size in bytes (/MAX):", _maxSizeTextBox, "Skip files larger than this many bytes."),
                ("Minimum size in bytes (/MIN):", _minSizeTextBox, "Skip files smaller than this many bytes."),
                ("Maximum age (/MAXAGE):", _maxAgeTextBox, "Skip files older than this many days, or a date as YYYYMMDD."),
                ("Minimum age (/MINAGE):", _minAgeTextBox, "Skip files newer than this many days, or a date as YYYYMMDD."),
                ("Maximum last access (/MAXLAD):", _maxLastAccessTextBox, "Skip files not used since this many days, or a date as YYYYMMDD."),
                ("Minimum last access (/MINLAD):", _minLastAccessTextBox, "Skip files used since this many days, or a date as YYYYMMDD."))));
        return page;
    }

    private TabPage BuildAttributesTab()
    {
        var page = CreateTabPage("Attributes");
        page.Controls.Add(Stack(
            CheckboxGrid(
                _copyAllCheckBox,
                _copySecurityCheckBox,
                _secFixCheckBox,
                _timFixCheckBox,
                _noCopyCheckBox,
                _noDirectoryCopyCheckBox),
            SectionLabel("File information (/COPY)"),
            HorizontalFlags(
                _copyDataCheckBox,
                _copyAttributesCheckBox,
                _copyTimestampsCheckBox,
                _copySecurityFlagCheckBox,
                _copyOwnerCheckBox,
                _copyAuditingCheckBox,
                _copySkipStreamsCheckBox),
            SectionLabel("Directory information (/DCOPY)"),
            HorizontalFlags(
                _directoryDataCheckBox,
                _directoryAttributesCheckBox,
                _directoryTimestampsCheckBox,
                _directoryEasCheckBox,
                _directorySkipStreamsCheckBox),
            FieldGrid(
                ("Add attributes (/A+):", _addAttributesTextBox, "Letters from RASHCNET added to copied files. Example: RH"),
                ("Remove attributes (/A-):", _removeAttributesTextBox, "Letters from RASHCNETO removed from copied files. Example: S")),
            Note("When several file-information options are selected, /NOCOPY is used first, then /COPYALL, then a custom /COPY selection, then /SEC. The command preview shows which one will run.")));
        return page;
    }

    private TabPage BuildPerformanceTab()
    {
        Tip(_multiThreadCheckBox, "n is from 1 to 128. The default is 8. This cannot be combined with /IPG, /EFSRAW or /LFSM. The progress bar is less precise while several files are copied at once.");
        Tip(_interPacketGapNumeric, "Milliseconds to wait between packets, which leaves bandwidth free on a slow link.");
        Tip(_runHoursTextBox, "Times when new copies may start, as hhmm-hhmm. Example: 2200-0600");
        Tip(_ioMaxSizeTextBox, "Maximum read or write size per cycle. A K, M or G suffix is allowed. Example: 8M");
        Tip(_ioRateTextBox, "Requested I/O rate. A K, M or G suffix is allowed. Example: 10M");
        Tip(_thresholdTextBox, "Only throttle files at least this large. A K, M or G suffix is allowed.");
        Tip(_lowFreeSpaceFloorTextBox, "Optional floor, for example 10G. Leave this blank to use 10 percent of the destination volume.");

        var page = CreateTabPage("Performance");
        page.Controls.Add(Stack(
            CheckboxGrid(
                _multiThreadCheckBox,
                _lowFreeSpaceCheckBox,
                _waitForShareCheckBox,
                _saveRetryDefaultsCheckBox,
                _perFileRunHoursCheckBox),
            FieldGrid(
                ("Threads (/MT):", _threadCountNumeric, null),
                ("Gap between packets in ms (/IPG):", _interPacketGapNumeric, null),
                ("Retries for failed copies (/R):", _retryCountNumeric, "Robocopy's own default is one million. This program sends 1 unless you change it."),
                ("Wait between retries in seconds (/W):", _retryWaitNumeric, "Robocopy's own default is 30 seconds. This program sends 1 unless you change it."),
                ("Low free space floor (/LFSM):", _lowFreeSpaceFloorTextBox, null),
                ("Run again after this many changes (/MON):", _monitorChangesNumeric, "0 disables monitoring. Robocopy keeps watching the source and runs again."),
                ("Run again after this many minutes (/MOT):", _monitorMinutesNumeric, "0 disables the timer. Robocopy runs again if the source changed."),
                ("Run hours (/RH):", _runHoursTextBox, null),
                ("Maximum I/O size (/IoMaxSize):", _ioMaxSizeTextBox, null),
                ("I/O rate (/IoRate):", _ioRateTextBox, null),
                ("Throttle files at least this size (/Threshold):", _thresholdTextBox, null))));
        return page;
    }

    private TabPage BuildLoggingTab()
    {
        var page = CreateTabPage("Logging");
        page.Controls.Add(Stack(
            CheckboxGrid(
                _listOnlyCheckBox,
                _verboseCheckBox,
                _reportExtraCheckBox,
                _timestampsCheckBox,
                _fullPathCheckBox,
                _bytesCheckBox,
                _noSizeCheckBox,
                _noClassCheckBox,
                _noFileListCheckBox,
                _noDirectoryListCheckBox,
                _showProgressCheckBox,
                _noProgressCheckBox,
                _teeCheckBox,
                _noJobHeaderCheckBox,
                _noJobSummaryCheckBox,
                _unicodeOutputCheckBox),
            FieldGrid(
                ("Log file:", LogFileEditor(), "Leave blank to keep the output in this window only."),
                ("Log mode:", _logModeCombo, "Used only when a log file is selected.")),
            Note("The progress bar reads Robocopy's percentage. /NP hides that output, so the bar then advances one file at a time. /NFL and /NDL hide names the bar uses to measure the job.")));
        return page;
    }

    private TabPage BuildAdvancedTab()
    {
        var page = CreateTabPage("Advanced");
        page.Controls.Add(Stack(
            CheckboxGrid(
                _quitCheckBox,
                _noSourceDirectoryCheckBox,
                _noDestinationDirectoryCheckBox),
            FieldGrid(
                ("Load job file (/JOB):", _jobNameTextBox, "Reads options from a Robocopy job file."),
                ("Save job file (/SAVE):", _saveJobTextBox, "Writes the current options to a job file. Add /QUIT to save without copying.")),
            SectionLabel("Other switches"),
            _advancedOptionsTextBox,
            Note("Anything typed here is passed to robocopy.exe as written. Use this for a switch that is not listed above, such as /IF. Check the generated command before running it.")));
        return page;
    }

    private Control BuildCommandAndButtonsPanel()
    {
        var container = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(0, 8, 0, 4),
            ColumnCount = 1,
            RowCount = 2
        };

        container.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        container.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var commandGroup = new GroupBox
        {
            Text = "Generated command",
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(8)
        };
        commandGroup.Controls.Add(_commandPreviewTextBox);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(0, 8, 0, 0)
        };

        buttonPanel.Controls.Add(_runButton);
        buttonPanel.Controls.Add(_cancelButton);
        buttonPanel.Controls.Add(_copyCommandButton);

        container.Controls.Add(commandGroup, 0, 0);
        container.Controls.Add(buttonPanel, 0, 1);
        return container;
    }

    private GroupBox BuildOutputGroup()
    {
        var group = new GroupBox
        {
            Text = "Robocopy output",
            Dock = DockStyle.Fill,
            Padding = new Padding(8)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 78));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(_progressBar, 0, 0);
        layout.Controls.Add(_progressPercentLabel, 1, 0);
        layout.Controls.Add(_progressDetailLabel, 0, 1);
        layout.SetColumnSpan(_progressDetailLabel, 2);
        layout.Controls.Add(_outputTextBox, 0, 2);
        layout.SetColumnSpan(_outputTextBox, 2);

        group.Controls.Add(layout);
        return group;
    }

    private Control LogFileEditor()
    {
        var panel = new TableLayoutPanel
        {
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            Height = 28,
            Margin = new Padding(0)
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));

        _logFileTextBox.Dock = DockStyle.Fill;
        var browseButton = new Button
        {
            Text = "Browse...",
            Dock = DockStyle.Fill
        };
        browseButton.Click += (_, _) => SelectLogFile();

        panel.Controls.Add(_logFileTextBox, 0, 0);
        panel.Controls.Add(browseButton, 1, 0);
        return panel;
    }

    private static TabPage CreateTabPage(string title)
    {
        return new TabPage(title)
        {
            AutoScroll = true,
            Padding = new Padding(4)
        };
    }

    private static TableLayoutPanel Stack(params Control[] sections)
    {
        var stack = new TableLayoutPanel
        {
            ColumnCount = 1,
            RowCount = sections.Length,
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(2)
        };
        stack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        for (var index = 0; index < sections.Length; index++)
        {
            stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            sections[index].Dock = DockStyle.Fill;
            sections[index].Margin = new Padding(0, 2, 0, 2);
            stack.Controls.Add(sections[index], 0, index);
        }

        return stack;
    }

    private static TableLayoutPanel CheckboxGrid(params CheckBox[] boxes)
    {
        var grid = new TableLayoutPanel
        {
            ColumnCount = 2,
            RowCount = (boxes.Length + 1) / 2,
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(6, 4, 6, 4)
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        for (var index = 0; index < boxes.Length; index++)
        {
            boxes[index].Anchor = AnchorStyles.Left;
            boxes[index].Margin = new Padding(4, 3, 8, 3);
            grid.Controls.Add(boxes[index], index % 2, index / 2);
        }

        return grid;
    }

    private TableLayoutPanel FieldGrid(params (string Label, Control Input, string? Tip)[] fields)
    {
        var grid = new TableLayoutPanel
        {
            ColumnCount = 2,
            RowCount = fields.Length,
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(8, 2, 8, 6)
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 290));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        for (var index = 0; index < fields.Length; index++)
        {
            grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var (labelText, input, tip) = fields[index];
            var label = new Label
            {
                Text = labelText,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(4, 6, 4, 6)
            };
            input.Margin = new Padding(4, 3, 4, 3);
            if (input is TextBox or ComboBox)
            {
                input.Dock = DockStyle.Fill;
            }
            else
            {
                input.Anchor = AnchorStyles.Left;
            }

            if (!string.IsNullOrWhiteSpace(tip))
            {
                Tip(label, tip);
                Tip(input, tip);
            }

            grid.Controls.Add(label, 0, index);
            grid.Controls.Add(input, 1, index);
        }

        return grid;
    }

    private static FlowLayoutPanel HorizontalFlags(params CheckBox[] boxes)
    {
        var panel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top,
            Padding = new Padding(8, 0, 8, 4)
        };

        foreach (var box in boxes)
        {
            box.Margin = new Padding(4, 2, 12, 2);
            panel.Controls.Add(box);
        }

        return panel;
    }

    private static Label SectionLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Font = new Font(SystemFonts.MessageBoxFont!, FontStyle.Bold),
            Margin = new Padding(10, 8, 4, 2),
            Anchor = AnchorStyles.Left
        };
    }

    private static Label Note(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            MaximumSize = new Size(1040, 0),
            ForeColor = Color.DimGray,
            Margin = new Padding(10, 4, 10, 8)
        };
    }

    private static CheckBox Option(string text, Color? foreColor = null)
    {
        return new CheckBox
        {
            Text = text,
            AutoSize = true,
            ForeColor = foreColor ?? SystemColors.ControlText
        };
    }

    private static CheckBox FlagOption(string text, bool isChecked = false)
    {
        return new CheckBox
        {
            Text = text,
            AutoSize = true,
            Checked = isChecked
        };
    }

    private static void AddPathRow(
        TableLayoutPanel layout,
        int row,
        string label,
        TextBox textBox,
        EventHandler browseHandler)
    {
        textBox.Dock = DockStyle.Fill;

        var browseButton = new Button
        {
            Text = "Browse...",
            AutoSize = true
        };
        browseButton.Click += browseHandler;

        layout.Controls.Add(new Label
        {
            Text = label,
            AutoSize = true,
            Anchor = AnchorStyles.Left
        }, 0, row);

        layout.Controls.Add(textBox, 1, row);
        layout.Controls.Add(browseButton, 2, row);
    }

    private void AddTextRow(
        TableLayoutPanel layout,
        int row,
        string label,
        TextBox textBox,
        string tooltipText)
    {
        Tip(textBox, tooltipText);
        textBox.Dock = DockStyle.Fill;

        layout.Controls.Add(new Label
        {
            Text = label,
            AutoSize = true,
            Anchor = AnchorStyles.Left
        }, 0, row);

        layout.Controls.Add(textBox, 1, row);
        layout.SetColumnSpan(textBox, 2);
    }

    private void Tip(Control control, string text)
    {
        _toolTip.SetToolTip(control, text);
    }

    private void HookEvents()
    {
        foreach (Control control in GetAllControls(this))
        {
            switch (control)
            {
                case TextBox textBox when textBox != _commandPreviewTextBox:
                    textBox.TextChanged += (_, _) => UpdateCommandPreview();
                    break;

                case CheckBox checkBox:
                    checkBox.CheckedChanged += (_, _) => UpdateCommandPreview();
                    break;

                case NumericUpDown numeric:
                    numeric.ValueChanged += (_, _) => UpdateCommandPreview();
                    break;

                case ComboBox combo:
                    combo.SelectedIndexChanged += (_, _) => UpdateCommandPreview();
                    break;
            }
        }

        _runButton.Click += async (_, _) => await RunRobocopyAsync();
        _cancelButton.Click += (_, _) => CancelRobocopy();
        _copyCommandButton.Click += (_, _) =>
        {
            if (!string.IsNullOrWhiteSpace(_commandPreviewTextBox.Text))
            {
                Clipboard.SetText(_commandPreviewTextBox.Text);
            }
        };
    }

    private static IEnumerable<Control> GetAllControls(Control root)
    {
        foreach (Control child in root.Controls)
        {
            yield return child;

            foreach (Control descendant in GetAllControls(child))
            {
                yield return descendant;
            }
        }
    }

    private void SelectSourceFolder(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select the Robocopy source folder",
            UseDescriptionForTitle = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _sourceTextBox.Text = dialog.SelectedPath;
        }
    }

    private void SelectDestinationFolder(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select the Robocopy destination folder",
            UseDescriptionForTitle = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _destinationTextBox.Text = dialog.SelectedPath;
        }
    }

    private void SelectLogFile()
    {
        using var dialog = new SaveFileDialog
        {
            Title = "Select a Robocopy log file",
            Filter = "Log files (*.log)|*.log|Text files (*.txt)|*.txt|All files (*.*)|*.*",
            DefaultExt = "log"
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _logFileTextBox.Text = dialog.FileName;
        }
    }

    private void UpdateCommandPreview()
    {
        if (_updatingPreview)
        {
            return;
        }

        _updatingPreview = true;
        try
        {
            _threadCountNumeric.Enabled = _multiThreadCheckBox.Checked;
            _lowFreeSpaceFloorTextBox.Enabled = _lowFreeSpaceCheckBox.Checked;
            _commandPreviewTextBox.Text = BuildDisplayCommand();
        }
        finally
        {
            _updatingPreview = false;
        }
    }

    private string BuildDisplayCommand()
    {
        return $"robocopy.exe {BuildArguments()}";
    }

    private string BuildArguments()
    {
        var arguments = new List<string>();

        AddQuotedArgument(arguments, _sourceTextBox.Text.Trim());
        AddQuotedArgument(arguments, _destinationTextBox.Text.Trim());

        foreach (var pattern in SplitArguments(_filesTextBox.Text))
        {
            arguments.Add(QuoteIfNeeded(pattern));
        }

        if (_copyEmptyDirectoriesCheckBox.Checked)
        {
            arguments.Add("/E");
        }
        else if (_copySubdirectoriesCheckBox.Checked)
        {
            arguments.Add("/S");
        }

        AddNumericSwitch(arguments, "LEV", _levelsNumeric.Value);
        AddSwitch(arguments, _restartableBackupCheckBox, "/ZB");
        if (!_restartableBackupCheckBox.Checked)
        {
            AddSwitch(arguments, _restartableCheckBox, "/Z");
            AddSwitch(arguments, _backupModeCheckBox, "/B");
        }

        AddSwitch(arguments, _unbufferedCheckBox, "/J");
        AddSwitch(arguments, _efsRawCheckBox, "/EFSRAW");
        AddFileCopyArguments(arguments);
        AddSwitch(arguments, _secFixCheckBox, "/SECFIX");
        AddSwitch(arguments, _timFixCheckBox, "/TIMFIX");
        AddSwitch(arguments, _purgeCheckBox, "/PURGE");
        AddSwitch(arguments, _mirrorCheckBox, "/MIR");
        AddSwitch(arguments, _moveFilesCheckBox, "/MOV");
        AddSwitch(arguments, _moveFilesAndDirectoriesCheckBox, "/MOVE");
        AddAttributeChange(arguments, "A+", _addAttributesTextBox.Text);
        AddAttributeChange(arguments, "A-", _removeAttributesTextBox.Text);
        AddSwitch(arguments, _createOnlyCheckBox, "/CREATE");
        AddSwitch(arguments, _fatNamesCheckBox, "/FAT");
        AddSwitch(arguments, _disableLongPathsCheckBox, "/256");
        AddNumericSwitch(arguments, "MON", _monitorChangesNumeric.Value);
        AddNumericSwitch(arguments, "MOT", _monitorMinutesNumeric.Value);
        AddTextSwitch(arguments, "RH", _runHoursTextBox.Text);
        AddSwitch(arguments, _perFileRunHoursCheckBox, "/PF");
        AddNumericSwitch(arguments, "IPG", _interPacketGapNumeric.Value);
        AddSwitch(arguments, _junctionsCheckBox, "/SJ");
        AddSwitch(arguments, _symlinksCheckBox, "/SL");

        if (_multiThreadCheckBox.Checked)
        {
            arguments.Add($"/MT:{_threadCountNumeric.Value}");
        }

        AddDirectoryCopyArguments(arguments);
        AddSwitch(arguments, _noOffloadCheckBox, "/NOOFFLOAD");
        AddSwitch(arguments, _compressCheckBox, "/COMPRESS");

        if (_sparseCombo.SelectedIndex == 1)
        {
            arguments.Add("/SPARSE:Y");
        }
        else if (_sparseCombo.SelectedIndex == 2)
        {
            arguments.Add("/SPARSE:N");
        }

        AddSwitch(arguments, _noCloneCheckBox, "/NOCLONE");
        AddTextSwitch(arguments, "IoMaxSize", _ioMaxSizeTextBox.Text);
        AddTextSwitch(arguments, "IoRate", _ioRateTextBox.Text);
        AddTextSwitch(arguments, "Threshold", _thresholdTextBox.Text);
        AddSwitch(arguments, _archiveOnlyCheckBox, "/A");
        AddSwitch(arguments, _archiveAndResetCheckBox, "/M");
        AddTextSwitch(arguments, "IA", _includeAttributesTextBox.Text);
        AddTextSwitch(arguments, "XA", _excludeAttributesTextBox.Text);
        AddExclusion(arguments, "/XF", _excludeFilesTextBox.Text);
        AddExclusion(arguments, "/XD", _excludeDirectoriesTextBox.Text);
        AddSwitch(arguments, _excludeChangedCheckBox, "/XC");
        AddSwitch(arguments, _excludeNewerCheckBox, "/XN");
        AddSwitch(arguments, _excludeOlderCheckBox, "/XO");
        AddSwitch(arguments, _excludeExtraCheckBox, "/XX");
        AddSwitch(arguments, _excludeLonelyCheckBox, "/XL");
        AddSwitch(arguments, _includeSameCheckBox, "/IS");
        AddSwitch(arguments, _includeTweakedCheckBox, "/IT");
        AddTextSwitch(arguments, "MAX", _maxSizeTextBox.Text);
        AddTextSwitch(arguments, "MIN", _minSizeTextBox.Text);
        AddTextSwitch(arguments, "MAXAGE", _maxAgeTextBox.Text);
        AddTextSwitch(arguments, "MINAGE", _minAgeTextBox.Text);
        AddTextSwitch(arguments, "MAXLAD", _maxLastAccessTextBox.Text);
        AddTextSwitch(arguments, "MINLAD", _minLastAccessTextBox.Text);
        AddSwitch(arguments, _fatFileTimesCheckBox, "/FFT");
        AddSwitch(arguments, _dstCheckBox, "/DST");
        AddSwitch(arguments, _excludeLinksCheckBox, "/XJ");
        AddSwitch(arguments, _excludeDirLinksCheckBox, "/XJD");
        AddSwitch(arguments, _excludeFileLinksCheckBox, "/XJF");
        AddSwitch(arguments, _includeModifiedCheckBox, "/IM");
        arguments.Add($"/R:{_retryCountNumeric.Value}");
        arguments.Add($"/W:{_retryWaitNumeric.Value}");
        AddSwitch(arguments, _saveRetryDefaultsCheckBox, "/REG");
        AddSwitch(arguments, _waitForShareCheckBox, "/TBD");

        if (_lowFreeSpaceCheckBox.Checked)
        {
            var floor = _lowFreeSpaceFloorTextBox.Text.Trim();
            arguments.Add(string.IsNullOrEmpty(floor) ? "/LFSM" : $"/LFSM:{floor}");
        }

        AddSwitch(arguments, _listOnlyCheckBox, "/L");
        AddSwitch(arguments, _reportExtraCheckBox, "/X");
        AddSwitch(arguments, _verboseCheckBox, "/V");
        AddSwitch(arguments, _timestampsCheckBox, "/TS");
        AddSwitch(arguments, _fullPathCheckBox, "/FP");
        AddSwitch(arguments, _bytesCheckBox, "/BYTES");
        AddSwitch(arguments, _noSizeCheckBox, "/NS");
        AddSwitch(arguments, _noClassCheckBox, "/NC");
        AddSwitch(arguments, _noFileListCheckBox, "/NFL");
        AddSwitch(arguments, _noDirectoryListCheckBox, "/NDL");
        AddSwitch(arguments, _noProgressCheckBox, "/NP");
        AddSwitch(arguments, _showProgressCheckBox, "/ETA");
        AddLogArgument(arguments);
        AddSwitch(arguments, _teeCheckBox, "/TEE");
        AddSwitch(arguments, _noJobHeaderCheckBox, "/NJH");
        AddSwitch(arguments, _noJobSummaryCheckBox, "/NJS");
        AddSwitch(arguments, _unicodeOutputCheckBox, "/UNICODE");
        AddTextSwitch(arguments, "JOB", _jobNameTextBox.Text);
        AddTextSwitch(arguments, "SAVE", _saveJobTextBox.Text);
        AddSwitch(arguments, _quitCheckBox, "/QUIT");
        AddSwitch(arguments, _noSourceDirectoryCheckBox, "/NOSD");
        AddSwitch(arguments, _noDestinationDirectoryCheckBox, "/NODD");

        if (!string.IsNullOrWhiteSpace(_advancedOptionsTextBox.Text))
        {
            var flattened = string.Join(
                " ",
                _advancedOptionsTextBox.Text.Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries));
            if (!string.IsNullOrWhiteSpace(flattened))
            {
                arguments.Add(flattened);
            }
        }

        return string.Join(" ", arguments);
    }

    private void AddFileCopyArguments(List<string> arguments)
    {
        if (_noCopyCheckBox.Checked)
        {
            arguments.Add("/NOCOPY");
            return;
        }

        if (_copyAllCheckBox.Checked)
        {
            arguments.Add("/COPYALL");
            return;
        }

        var flags = CollectFlags(
            (_copyDataCheckBox, 'D'),
            (_copyAttributesCheckBox, 'A'),
            (_copyTimestampsCheckBox, 'T'),
            (_copySecurityFlagCheckBox, 'S'),
            (_copyOwnerCheckBox, 'O'),
            (_copyAuditingCheckBox, 'U'),
            (_copySkipStreamsCheckBox, 'X'));

        if (flags.Length > 0 && flags != "DAT")
        {
            arguments.Add("/COPY:" + flags);
            return;
        }

        AddSwitch(arguments, _copySecurityCheckBox, "/SEC");
    }

    private void AddDirectoryCopyArguments(List<string> arguments)
    {
        if (_noDirectoryCopyCheckBox.Checked)
        {
            arguments.Add("/NODCOPY");
            return;
        }

        var flags = CollectFlags(
            (_directoryDataCheckBox, 'D'),
            (_directoryAttributesCheckBox, 'A'),
            (_directoryTimestampsCheckBox, 'T'),
            (_directoryEasCheckBox, 'E'),
            (_directorySkipStreamsCheckBox, 'X'));

        if (flags.Length > 0 && flags != "DA")
        {
            arguments.Add("/DCOPY:" + flags);
        }
    }

    private void AddLogArgument(List<string> arguments)
    {
        if (string.IsNullOrWhiteSpace(_logFileTextBox.Text))
        {
            return;
        }

        var prefix = _logModeCombo.SelectedIndex switch
        {
            1 => "/LOG+:",
            2 => "/UNILOG:",
            3 => "/UNILOG+:",
            _ => "/LOG:"
        };

        arguments.Add(prefix + QuoteIfNeeded(_logFileTextBox.Text.Trim()));
    }

    private static void AddSwitch(List<string> arguments, CheckBox checkBox, string value)
    {
        if (checkBox.Checked)
        {
            arguments.Add(value);
        }
    }

    private static void AddNumericSwitch(List<string> arguments, string name, decimal value)
    {
        if (value > 0)
        {
            arguments.Add($"/{name}:{value}");
        }
    }

    private static void AddTextSwitch(List<string> arguments, string name, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            arguments.Add($"/{name}:{value.Trim()}");
        }
    }

    private static void AddAttributeChange(List<string> arguments, string name, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            arguments.Add($"/{name}:{value.Trim()}");
        }
    }

    private static void AddExclusion(List<string> arguments, string name, string value)
    {
        var parts = SplitArguments(value).ToList();
        if (parts.Count == 0)
        {
            return;
        }

        arguments.Add(name);
        foreach (var part in parts)
        {
            arguments.Add(QuoteIfNeeded(part));
        }
    }

    private static string CollectFlags(params (CheckBox Box, char Flag)[] items)
    {
        var flags = new StringBuilder();
        foreach (var (box, flag) in items)
        {
            if (box.Checked)
            {
                flags.Append(flag);
            }
        }

        return flags.ToString();
    }

    private static void AddQuotedArgument(List<string> arguments, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            arguments.Add(QuoteIfNeeded(value));
        }
    }

    private static string QuoteIfNeeded(string value)
    {
        if (value.Contains(' ') || value.Contains('\t') || value.Contains('"'))
        {
            return $"\"{value.Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
        }

        return value;
    }

    private static IEnumerable<string> SplitArguments(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return [];
        }

        var values = new List<string>();
        var current = new StringBuilder();
        var insideQuotes = false;

        foreach (var character in input.Trim())
        {
            if (character == '"')
            {
                insideQuotes = !insideQuotes;
                continue;
            }

            if (char.IsWhiteSpace(character) && !insideQuotes)
            {
                if (current.Length > 0)
                {
                    values.Add(current.ToString());
                    current.Clear();
                }

                continue;
            }

            current.Append(character);
        }

        if (current.Length > 0)
        {
            values.Add(current.ToString());
        }

        return values;
    }

    private async Task RunRobocopyAsync()
    {
        if (string.IsNullOrWhiteSpace(_sourceTextBox.Text) ||
            string.IsNullOrWhiteSpace(_destinationTextBox.Text))
        {
            MessageBox.Show(
                this,
                "Select both a source folder and a destination folder.",
                "Source and destination required",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        if (!Directory.Exists(_sourceTextBox.Text))
        {
            MessageBox.Show(
                this,
                "The selected source folder does not exist.",
                "Invalid source folder",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        if (_multiThreadCheckBox.Checked &&
            (_interPacketGapNumeric.Value > 0 || _efsRawCheckBox.Checked || _lowFreeSpaceCheckBox.Checked))
        {
            MessageBox.Show(
                this,
                "Multi-threaded copy (/MT) cannot be combined with a packet gap (/IPG), EFS raw mode (/EFSRAW), or low free space mode (/LFSM).",
                "Incompatible options",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        var risky = new List<string>();
        if (_mirrorCheckBox.Checked)
        {
            risky.Add("/MIR");
        }

        if (_purgeCheckBox.Checked)
        {
            risky.Add("/PURGE");
        }

        if (_moveFilesCheckBox.Checked)
        {
            risky.Add("/MOV");
        }

        if (_moveFilesAndDirectoriesCheckBox.Checked)
        {
            risky.Add("/MOVE");
        }

        if (!_listOnlyCheckBox.Checked && risky.Count > 0)
        {
            var confirmation = MessageBox.Show(
                this,
                "These options can delete files or folders: " + string.Join(", ", risky) + ".\n\n" +
                "List only (/L) shows what would happen without changing anything.\n\n" +
                "Do you want to continue?",
                "Potentially destructive operation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirmation != DialogResult.Yes)
            {
                return;
            }
        }

        _outputTextBox.Clear();
        AppendOutput($"Running: {BuildDisplayCommand()}{Environment.NewLine}{Environment.NewLine}");

        _cancelRequested = false;
        _tracker.Reset();
        _tracker.MarkRunning();
        RefreshProgress();

        _runButton.Enabled = false;
        _cancelButton.Enabled = true;

        _scanCancellation?.Cancel();
        _scanCancellation?.Dispose();
        _scanCancellation = new CancellationTokenSource();
        var scanTask = Task.Run(() => RobocopyWorkEstimator.Estimate(CreateScanRequest(), _scanCancellation.Token), _scanCancellation.Token);
        var observeScanTask = ObserveScanAsync(scanTask);

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "robocopy.exe",
                Arguments = BuildArguments(),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            _process = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            _process.OutputDataReceived += (_, eventArgs) =>
            {
                if (eventArgs.Data is null)
                {
                    return;
                }

                var hideFromLog = _tracker.ApplyOutputLine(eventArgs.Data);
                if (!hideFromLog)
                {
                    AppendOutput(eventArgs.Data + Environment.NewLine);
                }

                RefreshProgress();
            };

            _process.ErrorDataReceived += (_, eventArgs) =>
            {
                if (eventArgs.Data is not null)
                {
                    AppendOutput(eventArgs.Data + Environment.NewLine, Color.OrangeRed);
                }
            };

            if (!_process.Start())
            {
                throw new InvalidOperationException("Could not start robocopy.exe.");
            }

            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();

            await _process.WaitForExitAsync();

            var exitCode = _process.ExitCode;
            if (_cancelRequested)
            {
                AppendOutput($"{Environment.NewLine}Robocopy process cancelled.{Environment.NewLine}", Color.Gold);
            }
            else
            {
                var message = InterpretRobocopyExitCode(exitCode);
                _tracker.MarkFinished(exitCode < 8);
                AppendOutput(
                    $"{Environment.NewLine}Finished. Exit code: {exitCode}. {message}{Environment.NewLine}",
                    exitCode >= 8 ? Color.OrangeRed : Color.LightGreen);
            }
        }
        catch (Exception exception)
        {
            _tracker.MarkFinished(success: false);
            AppendOutput(
                $"{Environment.NewLine}Error: {exception.Message}{Environment.NewLine}",
                Color.OrangeRed);

            MessageBox.Show(
                this,
                $"Robocopy could not be run.\n\n{exception.Message}",
                "Execution error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            _scanCancellation.Cancel();
            try
            {
                await observeScanTask;
            }
            catch (OperationCanceledException)
            {
                // The size estimate is optional.
            }

            RefreshProgress();

            _process?.Dispose();
            _process = null;

            _runButton.Enabled = true;
            _cancelButton.Enabled = false;
        }
    }

    private ScanRequest CreateScanRequest()
    {
        var recurse = _copySubdirectoriesCheckBox.Checked ||
                      _copyEmptyDirectoriesCheckBox.Checked ||
                      _mirrorCheckBox.Checked ||
                      _moveFilesAndDirectoriesCheckBox.Checked;

        return new ScanRequest
        {
            Source = _sourceTextBox.Text.Trim(),
            Recurse = recurse,
            MaxLevels = (int)_levelsNumeric.Value,
            ArchiveOnly = _archiveOnlyCheckBox.Checked || _archiveAndResetCheckBox.Checked,
            SkipDirectoryLinks = _excludeLinksCheckBox.Checked || _excludeDirLinksCheckBox.Checked,
            SkipFileLinks = _excludeLinksCheckBox.Checked || _excludeFileLinksCheckBox.Checked,
            MaxBytes = TryParseByteCount(_maxSizeTextBox.Text),
            MinBytes = TryParseByteCount(_minSizeTextBox.Text),
            IncludePatterns = SplitArguments(_filesTextBox.Text).ToList(),
            ExcludeFilePatterns = SplitArguments(_excludeFilesTextBox.Text).ToList(),
            ExcludeDirectoryPatterns = SplitArguments(_excludeDirectoriesTextBox.Text).ToList()
        };
    }

    private static long? TryParseByteCount(string text)
    {
        if (RobocopyProgressTracker.TryParseSize(text, out var bytes) && !string.IsNullOrWhiteSpace(text))
        {
            return bytes;
        }

        return null;
    }

    private async Task ObserveScanAsync(Task<WorkEstimate> scanTask)
    {
        try
        {
            var estimate = await scanTask.ConfigureAwait(false);
            _tracker.SetEstimate(estimate);
            RefreshProgress();
        }
        catch (OperationCanceledException)
        {
            // A new run, or closing the window, cancelled the estimate.
        }
        catch (Exception)
        {
            // The copy can continue without a total size.
        }
    }

    private void CancelRobocopy()
    {
        _cancelRequested = true;
        _tracker.MarkCancelled();
        RefreshProgress();
        _scanCancellation?.Cancel();

        if (_process is null || _process.HasExited)
        {
            return;
        }

        try
        {
            _process.Kill(entireProcessTree: true);
        }
        catch (Exception exception)
        {
            AppendOutput(
                $"{Environment.NewLine}Could not cancel process: {exception.Message}{Environment.NewLine}",
                Color.OrangeRed);
        }
    }

    private void RefreshProgress()
    {
        if (IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            try
            {
                BeginInvoke(RefreshProgress);
            }
            catch (InvalidOperationException)
            {
                // The window is closing.
            }

            return;
        }

        var snapshot = _tracker.Snapshot();
        var shownBytes = snapshot.CompletedBytes;
        if (!snapshot.CurrentFileAccounted)
        {
            shownBytes += (long)Math.Round(snapshot.CurrentFileBytes * snapshot.CurrentFileFraction);
        }

        double fraction;
        if (snapshot.ForceComplete)
        {
            fraction = 1;
        }
        else if (snapshot.TotalBytes > 0)
        {
            fraction = shownBytes / (double)snapshot.TotalBytes;
        }
        else if (snapshot.EstimatedFiles > 0)
        {
            double filesDone = snapshot.CompletedFiles;
            if (!snapshot.CurrentFileAccounted)
            {
                filesDone += snapshot.CurrentFileFraction;
            }

            fraction = filesDone / snapshot.EstimatedFiles;
        }
        else
        {
            fraction = snapshot.CurrentFileFraction;
        }

        if (!snapshot.ForceComplete && fraction >= 1)
        {
            fraction = 0.995;
        }

        fraction = Math.Clamp(fraction, 0, 1);

        var waitingForMeasurement = snapshot.Running &&
                                    fraction <= 0 &&
                                    snapshot.TotalBytes == 0 &&
                                    snapshot.EstimatedFiles == 0;

        if (waitingForMeasurement)
        {
            _progressBar.Style = ProgressBarStyle.Marquee;
            _progressPercentLabel.Text = "...";
        }
        else
        {
            if (_progressBar.Style != ProgressBarStyle.Continuous)
            {
                _progressBar.Style = ProgressBarStyle.Continuous;
            }

            _progressBar.Value = (int)Math.Round(fraction * _progressBar.Maximum);
            _progressPercentLabel.Text = FormatPercent(fraction);
        }

        _progressDetailLabel.Text = DescribeProgress(snapshot, shownBytes);
        _progressDetailLabel.ForeColor = snapshot.Cancelled
            ? Color.DarkGoldenrod
            : snapshot.Failed
                ? Color.Firebrick
                : snapshot.ForceComplete
                    ? Color.DarkGreen
                    : SystemColors.ControlText;
    }

    private string DescribeProgress(ProgressSnapshot snapshot, long shownBytes)
    {
        if (snapshot.Cancelled)
        {
            return "Cancelled";
        }

        if (snapshot.Failed)
        {
            return "Finished with failures";
        }

        if (snapshot.ForceComplete)
        {
            return snapshot.TotalBytes > 0
                ? $"Finished — {FormatBytes(snapshot.TotalBytes)}"
                : "Finished";
        }

        if (!snapshot.Running && !snapshot.Finished)
        {
            return "Ready";
        }

        var action = _listOnlyCheckBox.Checked ? "Listing" : "Copying";
        var detail = new StringBuilder(action);

        if (!string.IsNullOrWhiteSpace(snapshot.CurrentFileName))
        {
            detail.Append(' ');
            detail.Append(snapshot.CurrentFileName);
            if (!snapshot.CurrentFileAccounted && snapshot.CurrentFileFraction > 0)
            {
                detail.Append(" (");
                detail.Append(FormatPercent(snapshot.CurrentFileFraction));
                detail.Append(" of this file)");
            }
        }

        if (!string.IsNullOrWhiteSpace(snapshot.Eta))
        {
            detail.Append(" — ETA ");
            detail.Append(snapshot.Eta);
        }

        if (snapshot.TotalBytes > 0)
        {
            detail.Append(" — ");
            detail.Append(FormatBytes(Math.Min(shownBytes, snapshot.TotalBytes)));
            detail.Append(" of ");
            detail.Append(FormatBytes(snapshot.TotalBytes));
        }
        else if (snapshot.EstimatedFiles > 0)
        {
            detail.Append(" — ");
            detail.Append(snapshot.CompletedFiles);
            detail.Append(" of ");
            detail.Append(snapshot.EstimatedFiles);
            detail.Append(" files");
        }
        else if (snapshot.Running)
        {
            detail.Append(" — measuring the copy");
        }

        return detail.ToString();
    }

    private static string FormatPercent(double fraction)
    {
        var percent = Math.Clamp(fraction, 0, 1) * 100;
        return percent >= 100 ? "100%" : $"{percent:0.0}%";
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double value = bytes;
        var unit = 0;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return unit == 0 ? $"{bytes} B" : $"{value:0.0} {units[unit]}";
    }

    private void AppendOutput(string text, Color? colour = null)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => AppendOutput(text, colour));
            return;
        }

        _outputTextBox.SelectionStart = _outputTextBox.TextLength;
        _outputTextBox.SelectionColor = colour ?? _outputTextBox.ForeColor;
        _outputTextBox.AppendText(text);
        _outputTextBox.SelectionColor = _outputTextBox.ForeColor;
        _outputTextBox.ScrollToCaret();
    }

    private static string InterpretRobocopyExitCode(int exitCode)
    {
        return exitCode switch
        {
            0 => "No files were copied; no failures were reported.",
            1 => "Files were copied successfully.",
            2 => "Extra files or directories were detected at the destination.",
            3 => "Files were copied and extra destination files or directories were detected.",
            4 => "Mismatched files or directories were detected.",
            5 => "Files were copied and mismatches were detected.",
            6 => "Extra destination files or directories and mismatches were detected.",
            7 => "Files were copied; extra files or directories and mismatches were detected.",
            _ when exitCode >= 8 => "Robocopy reported at least one copy failure.",
            _ => "Robocopy returned an unrecognised exit code."
        };
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_process is not null && !_process.HasExited)
        {
            var result = MessageBox.Show(
                this,
                "Robocopy is still running. Cancel it and exit?",
                "Confirm exit",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }

            CancelRobocopy();
        }

        _scanCancellation?.Cancel();
        base.OnFormClosing(e);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _toolTip.Dispose();
            _scanCancellation?.Dispose();
        }

        base.Dispose(disposing);
    }
}
