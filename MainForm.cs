using System.Diagnostics;
using System.Text;

namespace RoboCopyGui;

public sealed class MainForm : Form
{
    private readonly TextBox _sourceTextBox = new();
    private readonly TextBox _destinationTextBox = new();
    private readonly TextBox _filesTextBox = new() { Text = "*.*" };
    private readonly TextBox _excludeFilesTextBox = new();
    private readonly TextBox _excludeDirectoriesTextBox = new();

    private readonly CheckBox _copySubdirectoriesCheckBox = new()
    {
        Text = "Copy subdirectories (/S)"
    };

    private readonly CheckBox _copyEmptyDirectoriesCheckBox = new()
    {
        Text = "Copy empty subdirectories too (/E)"
    };

    private readonly CheckBox _mirrorCheckBox = new()
    {
        Text = "Mirror source to destination (/MIR)",
        ForeColor = Color.DarkRed
    };

    private readonly CheckBox _moveFilesCheckBox = new()
    {
        Text = "Move files after copying (/MOV)",
        ForeColor = Color.DarkRed
    };

    private readonly CheckBox _moveFilesAndDirectoriesCheckBox = new()
    {
        Text = "Move files and directories after copying (/MOVE)",
        ForeColor = Color.DarkRed
    };

    private readonly CheckBox _copyAllCheckBox = new()
    {
        Text = "Copy all file information (/COPYALL)"
    };

    private readonly CheckBox _restartableCheckBox = new()
    {
        Text = "Restartable mode (/Z)"
    };

    private readonly CheckBox _backupModeCheckBox = new()
    {
        Text = "Backup mode (/B)"
    };

    private readonly CheckBox _restartableBackupCheckBox = new()
    {
        Text = "Restartable backup mode (/ZB)"
    };

    private readonly CheckBox _copySecurityCheckBox = new()
    {
        Text = "Copy security information (/SEC)"
    };

    private readonly CheckBox _copySecurityOwnerAuditCheckBox = new()
    {
        Text = "Copy security, owner and audit information (/SECFIX)"
    };

    private readonly NumericUpDown _retryCountNumeric = new()
    {
        Minimum = 0,
        Maximum = 1000000,
        Value = 1
    };

    private readonly NumericUpDown _retryWaitNumeric = new()
    {
        Minimum = 0,
        Maximum = 1000000,
        Value = 1
    };

    private readonly CheckBox _verboseCheckBox = new()
    {
        Text = "Verbose output (/V)"
    };

    private readonly CheckBox _listOnlyCheckBox = new()
    {
        Text = "List only — do not copy (/L)"
    };

    private readonly CheckBox _showProgressCheckBox = new()
    {
        Text = "Show progress (/ETA)"
    };

    private readonly CheckBox _noProgressCheckBox = new()
    {
        Text = "Do not show progress (/NP)"
    };

    private readonly CheckBox _teeCheckBox = new()
    {
        Text = "Write output to console and log (/TEE)"
    };

    private readonly TextBox _logFileTextBox = new();
    private readonly TextBox _advancedOptionsTextBox = new()
    {
        Multiline = true,
        ScrollBars = ScrollBars.Vertical,
        Height = 80
    };

    private readonly TextBox _commandPreviewTextBox = new()
    {
        Multiline = true,
        ReadOnly = true,
        ScrollBars = ScrollBars.Vertical,
        Height = 70,
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

    private Process? _process;

    public MainForm()
    {
        Text = "Robocopy GUI";
        Width = 1120;
        Height = 820;
        MinimumSize = new Size(900, 650);
        StartPosition = FormStartPosition.CenterScreen;

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
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
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

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));

        AddPathRow(layout, 0, "Source:", _sourceTextBox, SelectSourceFolder);
        AddPathRow(layout, 1, "Destination:", _destinationTextBox, SelectDestinationFolder);
        AddTextRow(layout, 2, "Files:", _filesTextBox,
            "Optional file pattern(s), separated by spaces. Example: *.docx *.xlsx");
        AddTextRow(layout, 3, "Exclude files:", _excludeFilesTextBox,
            "Optional. Passed as /XF.");
        AddTextRow(layout, 4, "Exclude folders:", _excludeDirectoriesTextBox,
            "Optional. Passed as /XD.");

        group.Controls.Add(layout);
        return group;
    }

    private TabControl BuildOptionsTabs()
    {
        var tabs = new TabControl
        {
            Dock = DockStyle.Top,
            Height = 255
        };

        var copyTab = new TabPage("Copy");
        var copyLayout = CreateFlowLayout();

        copyLayout.Controls.Add(_copySubdirectoriesCheckBox);
        copyLayout.Controls.Add(_copyEmptyDirectoriesCheckBox);
        copyLayout.Controls.Add(_mirrorCheckBox);
        copyLayout.Controls.Add(_moveFilesCheckBox);
        copyLayout.Controls.Add(_moveFilesAndDirectoriesCheckBox);
        copyLayout.Controls.Add(_restartableCheckBox);
        copyLayout.Controls.Add(_backupModeCheckBox);
        copyLayout.Controls.Add(_restartableBackupCheckBox);

        copyTab.Controls.Add(copyLayout);

        var securityTab = new TabPage("Security");
        var securityLayout = CreateFlowLayout();

        securityLayout.Controls.Add(_copyAllCheckBox);
        securityLayout.Controls.Add(_copySecurityCheckBox);
        securityLayout.Controls.Add(_copySecurityOwnerAuditCheckBox);

        securityTab.Controls.Add(securityLayout);

        var retryTab = new TabPage("Retries and output");
        var retryLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 2,
            RowCount = 7
        };

        retryLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250));
        retryLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        retryLayout.Controls.Add(new Label
        {
            Text = "Retries for failed copies (/R):",
            AutoSize = true,
            Anchor = AnchorStyles.Left
        }, 0, 0);
        retryLayout.Controls.Add(_retryCountNumeric, 1, 0);

        retryLayout.Controls.Add(new Label
        {
            Text = "Wait between retries in seconds (/W):",
            AutoSize = true,
            Anchor = AnchorStyles.Left
        }, 0, 1);
        retryLayout.Controls.Add(_retryWaitNumeric, 1, 1);

        retryLayout.Controls.Add(_verboseCheckBox, 0, 2);
        retryLayout.SetColumnSpan(_verboseCheckBox, 2);

        retryLayout.Controls.Add(_listOnlyCheckBox, 0, 3);
        retryLayout.SetColumnSpan(_listOnlyCheckBox, 2);

        retryLayout.Controls.Add(_showProgressCheckBox, 0, 4);
        retryLayout.SetColumnSpan(_showProgressCheckBox, 2);

        retryLayout.Controls.Add(_noProgressCheckBox, 0, 5);
        retryLayout.SetColumnSpan(_noProgressCheckBox, 2);

        retryLayout.Controls.Add(_teeCheckBox, 0, 6);
        retryLayout.SetColumnSpan(_teeCheckBox, 2);

        retryTab.Controls.Add(retryLayout);

        var advancedTab = new TabPage("Logging and advanced");
        var advancedLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 2,
            RowCount = 4
        };

        advancedLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125));
        advancedLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        advancedLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        advancedLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        advancedLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        advancedLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var browseLogButton = new Button
        {
            Text = "Browse...",
            AutoSize = true
        };
        browseLogButton.Click += (_, _) => SelectLogFile();

        var logPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = false
        };
        _logFileTextBox.Width = 480;
        logPanel.Controls.Add(_logFileTextBox);
        logPanel.Controls.Add(browseLogButton);

        advancedLayout.Controls.Add(new Label
        {
            Text = "Log file (/LOG):",
            AutoSize = true,
            Anchor = AnchorStyles.Left
        }, 0, 0);
        advancedLayout.Controls.Add(logPanel, 1, 0);

        advancedLayout.Controls.Add(new Label
        {
            Text = "Advanced switches:",
            AutoSize = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        }, 0, 1);
        advancedLayout.Controls.Add(new Label
        {
            Text = "Enter additional valid Robocopy switches exactly as required. " +
                   "These are passed directly to robocopy.exe.",
            AutoSize = true,
            MaximumSize = new Size(650, 0)
        }, 1, 1);

        advancedLayout.Controls.Add(new Label
        {
            Text = "Options:",
            AutoSize = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        }, 0, 2);
        advancedLayout.Controls.Add(_advancedOptionsTextBox, 1, 2);

        advancedLayout.Controls.Add(new Label
        {
            Text = "Warning:",
            AutoSize = true,
            ForeColor = Color.DarkRed,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        }, 0, 3);
        advancedLayout.Controls.Add(new Label
        {
            Text = "/MIR, /PURGE, /MOV and /MOVE can delete files or folders. " +
                   "Use /L first to review what Robocopy would do.",
            AutoSize = true,
            ForeColor = Color.DarkRed,
            MaximumSize = new Size(650, 0)
        }, 1, 3);

        advancedTab.Controls.Add(advancedLayout);

        tabs.TabPages.Add(copyTab);
        tabs.TabPages.Add(securityTab);
        tabs.TabPages.Add(retryTab);
        tabs.TabPages.Add(advancedTab);

        return tabs;
    }

    private Control BuildCommandAndButtonsPanel()
    {
        var container = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(0, 10, 0, 10),
            ColumnCount = 1,
            RowCount = 2
        };

        container.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        container.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var commandGroup = new GroupBox
        {
            Text = "Generated command",
            Dock = DockStyle.Top,
            AutoSize = true
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

        group.Controls.Add(_outputTextBox);
        return group;
    }

    private static FlowLayoutPanel CreateFlowLayout()
    {
        return new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true
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

    private static void AddTextRow(
        TableLayoutPanel layout,
        int row,
        string label,
        TextBox textBox,
        string tooltipText)
    {
        var tooltip = new ToolTip();
        tooltip.SetToolTip(textBox, tooltipText);

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

    private void HookEvents()
    {
        foreach (Control control in GetAllControls(this))
        {
            switch (control)
            {
                case TextBox textBox:
                    textBox.TextChanged += (_, _) => UpdateCommandPreview();
                    break;

                case CheckBox checkBox:
                    checkBox.CheckedChanged += (_, _) => UpdateCommandPreview();
                    break;

                case NumericUpDown numeric:
                    numeric.ValueChanged += (_, _) => UpdateCommandPreview();
                    break;
            }
        }

        _runButton.Click += async (_, _) => await RunRobocopyAsync();
        _cancelButton.Click += (_, _) => CancelRobocopy();
        _copyCommandButton.Click += (_, _) =>
        {
            Clipboard.SetText(_commandPreviewTextBox.Text);
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
            Description = "Select the Robocopy source folder"
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
            Description = "Select the Robocopy destination folder"
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
        _commandPreviewTextBox.Text = BuildDisplayCommand();
    }

    private string BuildDisplayCommand()
    {
        var arguments = BuildArguments();

        return $"robocopy.exe {arguments}";
    }

    private string BuildArguments()
    {
        var arguments = new List<string>();

        AddQuotedArgument(arguments, _sourceTextBox.Text.Trim());
        AddQuotedArgument(arguments, _destinationTextBox.Text.Trim());

        // Robocopy accepts zero or more file patterns after source and destination.
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

        if (_mirrorCheckBox.Checked)
        {
            arguments.Add("/MIR");
        }

        if (_moveFilesCheckBox.Checked)
        {
            arguments.Add("/MOV");
        }

        if (_moveFilesAndDirectoriesCheckBox.Checked)
        {
            arguments.Add("/MOVE");
        }

        if (_copyAllCheckBox.Checked)
        {
            arguments.Add("/COPYALL");
        }

        if (_restartableCheckBox.Checked)
        {
            arguments.Add("/Z");
        }

        if (_backupModeCheckBox.Checked)
        {
            arguments.Add("/B");
        }

        if (_restartableBackupCheckBox.Checked)
        {
            arguments.Add("/ZB");
        }

        if (_copySecurityCheckBox.Checked)
        {
            arguments.Add("/SEC");
        }

        if (_copySecurityOwnerAuditCheckBox.Checked)
        {
            arguments.Add("/SECFIX");
        }

        if (!string.IsNullOrWhiteSpace(_excludeFilesTextBox.Text))
        {
            arguments.Add("/XF");

            foreach (var file in SplitArguments(_excludeFilesTextBox.Text))
            {
                arguments.Add(QuoteIfNeeded(file));
            }
        }

        if (!string.IsNullOrWhiteSpace(_excludeDirectoriesTextBox.Text))
        {
            arguments.Add("/XD");

            foreach (var directory in SplitArguments(_excludeDirectoriesTextBox.Text))
            {
                arguments.Add(QuoteIfNeeded(directory));
            }
        }

        arguments.Add($"/R:{_retryCountNumeric.Value}");
        arguments.Add($"/W:{_retryWaitNumeric.Value}");

        if (_verboseCheckBox.Checked)
        {
            arguments.Add("/V");
        }

        if (_listOnlyCheckBox.Checked)
        {
            arguments.Add("/L");
        }

        if (_showProgressCheckBox.Checked)
        {
            arguments.Add("/ETA");
        }

        if (_noProgressCheckBox.Checked)
        {
            arguments.Add("/NP");
        }

        if (_teeCheckBox.Checked)
        {
            arguments.Add("/TEE");
        }

        if (!string.IsNullOrWhiteSpace(_logFileTextBox.Text))
        {
            arguments.Add($"/LOG:{QuoteIfNeeded(_logFileTextBox.Text.Trim())}");
        }

        // This enables every other supported Robocopy switch to be passed through.
        if (!string.IsNullOrWhiteSpace(_advancedOptionsTextBox.Text))
        {
            arguments.Add(_advancedOptionsTextBox.Text.Trim());
        }

        return string.Join(" ", arguments);
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
            return $"\"{value.Replace("\"", "\\\"")}\"";
        }

        return value;
    }

    private static IEnumerable<string> SplitArguments(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Enumerable.Empty<string>();
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

        if (_mirrorCheckBox.Checked ||
            _moveFilesCheckBox.Checked ||
            _moveFilesAndDirectoriesCheckBox.Checked)
        {
            var confirmation = MessageBox.Show(
                this,
                "The selected options may delete files or folders. " +
                "Use List only (/L) first if you have not reviewed the changes.\n\n" +
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

        _runButton.Enabled = false;
        _cancelButton.Enabled = true;

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
                if (eventArgs.Data is not null)
                {
                    AppendOutput(eventArgs.Data + Environment.NewLine);
                }
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
            var message = InterpretRobocopyExitCode(exitCode);

            AppendOutput($"{Environment.NewLine}Finished. Exit code: {exitCode}. {message}{Environment.NewLine}",
                exitCode >= 8 ? Color.OrangeRed : Color.LightGreen);
        }
        catch (Exception exception)
        {
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
            _process?.Dispose();
            _process = null;

            _runButton.Enabled = true;
            _cancelButton.Enabled = false;
        }
    }

    private void CancelRobocopy()
    {
        if (_process is null || _process.HasExited)
        {
            return;
        }

        try
        {
            _process.Kill(entireProcessTree: true);
            AppendOutput($"{Environment.NewLine}Robocopy process cancelled.{Environment.NewLine}", Color.Gold);
        }
        catch (Exception exception)
        {
            AppendOutput(
                $"{Environment.NewLine}Could not cancel process: {exception.Message}{Environment.NewLine}",
                Color.OrangeRed);
        }
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
        // Robocopy does not use conventional 0 = success / non-zero = failure rules.
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

        base.OnFormClosing(e);
    }
}