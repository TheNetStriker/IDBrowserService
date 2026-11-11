using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Terminal.Gui.App;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace IDBrowserServiceCore.Code
{
    public class ProgressTaskFactory : IDisposable
    {
        private readonly List<ProgressTask> taskPool;
        private readonly ObservableCollection<string> logLines;
        private readonly ILogger logger;
        private readonly int concurrentTasks;
        private readonly CancellationTokenSource cancellationTokenSource;

        // Terminal.Gui views — only accessed on the UI thread via app.Invoke()
        private IApplication app;
        private Window mainWindow;
        private FrameView tasksFrame;
        private FrameView logFrame;
        private ListView logListView;

        // Parallel arrays indexed by task slot — populated during RunUiThread
        private ProgressBar[] progressBars;
        private Label[] fileLabels;

        // Signals that the Terminal.Gui app is fully initialised and views are ready
        private readonly ManualResetEventSlim uiReady = new ManualResetEventSlim(false);

        // Keeps the UI thread alive until we call Dispose()
        private Thread uiThread;

        public ProgressTaskFactory(int concurrentTasks, int maxProgress, int progressBarTextWidth, ILogger logger,
            CancellationTokenSource cancellationTokenSource)
        {
            this.concurrentTasks = concurrentTasks;
            this.logger = logger;
            this.cancellationTokenSource = cancellationTokenSource;
            logLines = new ObservableCollection<string>();
            taskPool = new List<ProgressTask>();

            StartUiThread(maxProgress, progressBarTextWidth);

            // Wait until Terminal.Gui is ready before we create ProgressTask wrappers that
            // reference the view objects.
            uiReady.Wait();

            for (int i = 0; i < concurrentTasks; i++)
            {
                taskPool.Add(new ProgressTask(this, i));
            }
        }

        // ── UI thread ────────────────────────────────────────────────────────────

        private void StartUiThread(int maxProgress, int progressBarTextWidth)
        {
            uiThread = new Thread(() => RunUiThread(maxProgress, progressBarTextWidth))
            {
                IsBackground = false,
                Name = "Terminal.Gui UI Thread"
            };
            uiThread.Start();
        }

        private void RunUiThread(int maxProgress, int progressBarTextWidth)
        {
            app = Application.Create();
            app.Init();

            mainWindow = new Window
            {
                Title = "Transcoding videos",
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            // Tasks panel — one row per progress bar plus border lines
            int tasksHeight = concurrentTasks + 2;

            tasksFrame = new FrameView
            {
                Title = "Tasks",
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = tasksHeight
            };

            // Add one ProgressBar + one Label per task slot
            // ProgressBar occupies the left 40%, Label the remaining 60%
            const int progressBarPercent = 40;

            progressBars = new ProgressBar[concurrentTasks];
            fileLabels = new Label[concurrentTasks];

            for (int i = 0; i < concurrentTasks; i++)
            {
                var pb = new ProgressBar
                {
                    X = 0,
                    Y = i,
                    Width = Dim.Percent(progressBarPercent),
                    Height = 1,
                    Fraction = 0f,
                    Text = string.Empty
                };

                var lbl = new Label
                {
                    X = Pos.Percent(progressBarPercent) + 1,
                    Y = i,
                    Width = Dim.Fill(),
                    Height = 1,
                    Text = string.Empty
                };

                progressBars[i] = pb;
                fileLabels[i] = lbl;

                tasksFrame.Add(pb, lbl);
            }

            // Log panel — fills the remaining vertical space, using ListView for scrollable lines
            logFrame = new FrameView
            {
                Title = "Log",
                X = 0,
                Y = tasksHeight,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            logListView = new ListView
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            logListView.SetSource(logLines);

            logFrame.Add(logListView);
            mainWindow.Add(tasksFrame, logFrame);

            // Intercept Ctrl+C inside Terminal.Gui (which owns the terminal and suppresses the
            // normal Console.CancelKeyPress signal) and cancel the transcoding operation.
            app.Keyboard.KeyDown += (sender, key) =>
            {
                if (key == Key.C.WithCtrl)
                {
                    cancellationTokenSource.Cancel();
                    key.Handled = true;
                }
            };

            // Signal that all views exist and can be referenced safely
            uiReady.Set();

            app.Run(mainWindow);

            // app.Run blocks until RequestStop is called (from Dispose)
            app.Dispose();
        }

        // ── Internal helpers ──────────────────────────────────────────────────────

        internal ProgressBar GetProgressBar(int index) => progressBars[index];

        internal Label GetFileLabel(int index) => fileLabels[index];

        internal void InvokeOnUiThread(Action action)
        {
            app?.Invoke(action);
        }

        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>Returns the first idle ProgressTask.</summary>
        public ProgressTask GetIdleProgressTask()
        {
            return taskPool.FirstOrDefault(x => x.Task == null);
        }

        /// <summary>Returns all currently active tasks.</summary>
        public List<Task> GetTasks()
        {
            return taskPool
                .Where(x => x.Task != null)
                .Select(x => x.Task)
                .ToList();
        }

        /// <summary>Waits until any task completes and returns it.</summary>
        public async Task<ProgressTask> WaitForAnyTask()
        {
            Task completedTask = await Task.WhenAny(GetTasks());
            ProgressTask completedProgressTask = taskPool.Single(x => x.Task == completedTask);
            completedProgressTask.Task = null;
            return completedProgressTask;
        }

        /// <summary>Waits until all tasks complete.</summary>
        public void WaitForAllTasks()
        {
            Task.WhenAll(GetTasks()).Wait();
        }

        /// <summary>
        /// Writes a log message to the Serilog logger and optionally to the console log panel.
        /// </summary>
        /// <param name="text">Text to write.</param>
        /// <param name="writeToConsole">Whether to append to the on-screen log panel.</param>
        /// <param name="logLevel">Serilog log level.</param>
        public void WriteLog(string text, bool writeToConsole, LogEventLevel logLevel)
        {
            logger.Write(logLevel, text);

            if (!writeToConsole)
                return;

            string line = $"[{DateTime.Now:HH:mm:ss}] {text}";

            app.Invoke(() =>
            {
                // Insert at top so newest messages appear first
                logLines.Insert(0, line);
                logListView.SetNeedsDraw();
            });
        }

        public void Dispose()
        {
            app?.Invoke(() => app.RequestStop());
            uiThread?.Join(TimeSpan.FromSeconds(5));
            uiReady.Dispose();
        }
    }
}
