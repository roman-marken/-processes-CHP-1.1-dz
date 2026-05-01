using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ProcessesChpDz
{
    public sealed class MainForm : Form
    {
        private readonly DataGridView processGrid;
        private readonly NumericUpDown intervalInput;
        private readonly Button applyIntervalButton;
        private readonly Button refreshButton;
        private readonly Button killButton;
        private readonly Button runNotepadButton;
        private readonly Button runCalcButton;
        private readonly Button runPaintButton;
        private readonly Button runCustomButton;
        private readonly TextBox customProgramInput;
        private readonly Label selectedNameLabel;
        private readonly Label selectedIdLabel;
        private readonly Label startTimeLabel;
        private readonly Label cpuTimeLabel;
        private readonly Label threadsLabel;
        private readonly Label copiesLabel;
        private readonly Timer refreshTimer;

        private List<ProcessInfo> currentProcesses = new List<ProcessInfo>();
        private int? selectedProcessId;

        public MainForm()
        {
            Text = "Processes CHP 1.1 DZ";
            MinimumSize = new Size(980, 620);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9F);

            processGrid = CreateProcessGrid();
            intervalInput = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 3600,
                Value = 5,
                Width = 80
            };

            applyIntervalButton = new Button { Text = "Застосувати", Width = 110 };
            refreshButton = new Button { Text = "Оновити зараз", Width = 115 };
            killButton = new Button { Text = "Завершити процес", Width = 150, Enabled = false };

            runNotepadButton = new Button { Text = "Блокнот", Width = 95 };
            runCalcButton = new Button { Text = "Калькулятор", Width = 105 };
            runPaintButton = new Button { Text = "Paint", Width = 85 };
            runCustomButton = new Button { Text = "Запустити", Width = 100 };
            customProgramInput = new TextBox { Width = 260 };

            selectedNameLabel = CreateValueLabel();
            selectedIdLabel = CreateValueLabel();
            startTimeLabel = CreateValueLabel();
            cpuTimeLabel = CreateValueLabel();
            threadsLabel = CreateValueLabel();
            copiesLabel = CreateValueLabel();

            refreshTimer = new Timer();
            refreshTimer.Interval = (int)intervalInput.Value * 1000;

            BuildLayout();
            WireEvents();
            ClearDetails();
            RefreshProcessList();
            refreshTimer.Start();
        }

        private DataGridView CreateProcessGrid()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            grid.Columns.Add("Id", "PID");
            grid.Columns.Add("Name", "Назва процесу");
            grid.Columns.Add("Memory", "Пам'ять");
            grid.Columns.Add("Threads", "Потоки");

            grid.Columns["Id"].FillWeight = 18;
            grid.Columns["Name"].FillWeight = 47;
            grid.Columns["Memory"].FillWeight = 20;
            grid.Columns["Threads"].FillWeight = 15;

            return grid;
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(12)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));

            var leftPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };
            leftPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            leftPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var topBar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = true,
                Padding = new Padding(0, 0, 0, 8)
            };
            topBar.Controls.Add(new Label
            {
                Text = "Інтервал оновлення, сек:",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 7, 8, 0)
            });
            topBar.Controls.Add(intervalInput);
            topBar.Controls.Add(applyIntervalButton);
            topBar.Controls.Add(refreshButton);

            leftPanel.Controls.Add(topBar, 0, 0);
            leftPanel.Controls.Add(processGrid, 0, 1);

            var rightPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1,
                Padding = new Padding(12, 0, 0, 0)
            };
            rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            rightPanel.Controls.Add(CreateDetailsGroup(), 0, 0);
            rightPanel.Controls.Add(CreateActionsGroup(), 0, 1);
            rightPanel.Controls.Add(CreateLauncherGroup(), 0, 2);

            root.Controls.Add(leftPanel, 0, 0);
            root.Controls.Add(rightPanel, 1, 0);
            Controls.Add(root);
        }

        private GroupBox CreateDetailsGroup()
        {
            var group = new GroupBox
            {
                Text = "Детальна інформація",
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(12)
            };

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                RowCount = 6
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            AddDetailRow(table, 0, "Процес:", selectedNameLabel);
            AddDetailRow(table, 1, "PID:", selectedIdLabel);
            AddDetailRow(table, 2, "Час старту:", startTimeLabel);
            AddDetailRow(table, 3, "CPU час:", cpuTimeLabel);
            AddDetailRow(table, 4, "Потоки:", threadsLabel);
            AddDetailRow(table, 5, "Копій процесу:", copiesLabel);

            group.Controls.Add(table);
            return group;
        }

        private GroupBox CreateActionsGroup()
        {
            var group = new GroupBox
            {
                Text = "Керування процесом",
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(12),
                Margin = new Padding(0, 12, 0, 0)
            };

            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true
            };
            panel.Controls.Add(killButton);
            group.Controls.Add(panel);
            return group;
        }

        private GroupBox CreateLauncherGroup()
        {
            var group = new GroupBox
            {
                Text = "Запуск програм",
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(12),
                Margin = new Padding(0, 12, 0, 0)
            };

            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1,
                RowCount = 3
            };

            var commonButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                WrapContents = true
            };
            commonButtons.Controls.Add(runNotepadButton);
            commonButtons.Controls.Add(runCalcButton);
            commonButtons.Controls.Add(runPaintButton);

            var customPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                WrapContents = true,
                Margin = new Padding(0, 10, 0, 0)
            };
            customPanel.Controls.Add(new Label
            {
                Text = "Своя програма:",
                AutoSize = true,
                Margin = new Padding(0, 7, 8, 0)
            });
            customPanel.Controls.Add(customProgramInput);
            customPanel.Controls.Add(runCustomButton);

            panel.Controls.Add(commonButtons, 0, 0);
            panel.Controls.Add(customPanel, 0, 1);
            group.Controls.Add(panel);
            return group;
        }

        private static Label CreateValueLabel()
        {
            return new Label
            {
                AutoSize = true,
                MaximumSize = new Size(330, 0),
                Margin = new Padding(3, 6, 3, 6)
            };
        }

        private static void AddDetailRow(TableLayoutPanel table, int row, string title, Control value)
        {
            table.Controls.Add(new Label
            {
                Text = title,
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Margin = new Padding(3, 6, 3, 6)
            }, 0, row);
            table.Controls.Add(value, 1, row);
        }

        private void WireEvents()
        {
            refreshTimer.Tick += delegate { RefreshProcessList(); };
            applyIntervalButton.Click += delegate { ApplyRefreshInterval(); };
            refreshButton.Click += delegate { RefreshProcessList(); };
            killButton.Click += delegate { KillSelectedProcess(); };

            runNotepadButton.Click += delegate { StartProgram("notepad.exe"); };
            runCalcButton.Click += delegate { StartProgram("calc.exe"); };
            runPaintButton.Click += delegate { StartProgram("mspaint.exe"); };
            runCustomButton.Click += delegate { StartProgram(customProgramInput.Text.Trim()); };

            processGrid.SelectionChanged += delegate { ShowSelectedProcessDetails(); };
        }

        private void ApplyRefreshInterval()
        {
            refreshTimer.Interval = (int)intervalInput.Value * 1000;
            RefreshProcessList();
        }

        private void RefreshProcessList()
        {
            var previousId = selectedProcessId;
            var processes = Process.GetProcesses()
                .Select(ProcessInfo.FromProcess)
                .OrderBy(process => process.Name, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(process => process.Id)
                .ToList();

            currentProcesses = processes;
            processGrid.Rows.Clear();

            foreach (var process in currentProcesses)
            {
                int rowIndex = processGrid.Rows.Add(
                    process.Id,
                    process.Name,
                    FormatBytes(process.WorkingSetBytes),
                    process.ThreadCount);
                processGrid.Rows[rowIndex].Tag = process.Id;
            }

            RestoreSelection(previousId);
            ShowSelectedProcessDetails();
        }

        private void RestoreSelection(int? processId)
        {
            processGrid.ClearSelection();
            if (!processId.HasValue)
            {
                return;
            }

            foreach (DataGridViewRow row in processGrid.Rows)
            {
                if ((int)row.Tag == processId.Value)
                {
                    row.Selected = true;
                    processGrid.CurrentCell = row.Cells[0];
                    return;
                }
            }
        }

        private void ShowSelectedProcessDetails()
        {
            if (processGrid.SelectedRows.Count == 0)
            {
                selectedProcessId = null;
                ClearDetails();
                return;
            }

            int processId = (int)processGrid.SelectedRows[0].Tag;
            selectedProcessId = processId;

            Process process = null;
            try
            {
                process = Process.GetProcessById(processId);
                string processName = process.ProcessName;
                selectedNameLabel.Text = processName;
                selectedIdLabel.Text = process.Id.ToString();
                startTimeLabel.Text = TryRead(() => process.StartTime.ToString("yyyy-MM-dd HH:mm:ss"));
                cpuTimeLabel.Text = TryRead(() => process.TotalProcessorTime.ToString());
                threadsLabel.Text = TryRead(() => process.Threads.Count.ToString());
                copiesLabel.Text = CountProcessesByName(processName).ToString();
                killButton.Enabled = true;
            }
            catch (Exception ex)
            {
                if (!(ex is ArgumentException || ex is InvalidOperationException))
                {
                    throw;
                }

                ClearDetails();
                selectedNameLabel.Text = "Процес вже завершено";
            }
            finally
            {
                if (process != null)
                {
                    process.Dispose();
                }
            }
        }

        private void ClearDetails()
        {
            selectedNameLabel.Text = "-";
            selectedIdLabel.Text = "-";
            startTimeLabel.Text = "-";
            cpuTimeLabel.Text = "-";
            threadsLabel.Text = "-";
            copiesLabel.Text = "-";
            killButton.Enabled = false;
        }

        private void KillSelectedProcess()
        {
            if (!selectedProcessId.HasValue)
            {
                return;
            }

            int processId = selectedProcessId.Value;
            string processName = selectedNameLabel.Text;

            var answer = MessageBox.Show(
                "Завершити процес \"" + processName + "\" з PID " + processId + "?",
                "Підтвердження",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (answer != DialogResult.Yes)
            {
                return;
            }

            try
            {
                using (var process = Process.GetProcessById(processId))
                {
                    process.Kill();
                }
                RefreshProcessList();
            }
            catch (Exception ex)
            {
                if (!(ex is ArgumentException ||
                    ex is InvalidOperationException ||
                    ex is System.ComponentModel.Win32Exception))
                {
                    throw;
                }

                MessageBox.Show(
                    "Не вдалося завершити процес: " + ex.Message,
                    "Помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                RefreshProcessList();
            }
        }

        private void StartProgram(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                MessageBox.Show("Вкажіть шлях або назву програми.", "Запуск програми",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = command,
                    UseShellExecute = true
                };

                if (File.Exists(command))
                {
                    startInfo.WorkingDirectory = Path.GetDirectoryName(command);
                }

                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                if (!(ex is InvalidOperationException ||
                    ex is System.ComponentModel.Win32Exception ||
                    ex is FileNotFoundException))
                {
                    throw;
                }

                MessageBox.Show(
                    "Не вдалося запустити програму: " + ex.Message,
                    "Помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static string TryRead(Func<string> reader)
        {
            try
            {
                return reader();
            }
            catch (Exception ex)
            {
                if (!(ex is InvalidOperationException ||
                    ex is System.ComponentModel.Win32Exception ||
                    ex is NotSupportedException))
                {
                    throw;
                }

                return "Немає доступу";
            }
        }

        private static int CountProcessesByName(string processName)
        {
            try
            {
                return Process.GetProcessesByName(processName).Length;
            }
            catch
            {
                return 0;
            }
        }

        private static string FormatBytes(long bytes)
        {
            double value = bytes;
            string[] units = { "B", "KB", "MB", "GB" };
            int unitIndex = 0;

            while (value >= 1024 && unitIndex < units.Length - 1)
            {
                value /= 1024;
                unitIndex++;
            }

            return value.ToString("0.##") + " " + units[unitIndex];
        }

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    public sealed class ProcessInfo
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public long WorkingSetBytes { get; private set; }
        public int ThreadCount { get; private set; }

        public static ProcessInfo FromProcess(Process process)
        {
            var info = new ProcessInfo();
            info.Id = process.Id;
            info.Name = SafeRead(() => process.ProcessName, "Unknown");
            info.WorkingSetBytes = SafeRead(() => process.WorkingSet64, 0L);
            info.ThreadCount = SafeRead(() => process.Threads.Count, 0);
            return info;
        }

        private static T SafeRead<T>(Func<T> reader, T fallback)
        {
            try
            {
                return reader();
            }
            catch
            {
                return fallback;
            }
        }
    }
}
