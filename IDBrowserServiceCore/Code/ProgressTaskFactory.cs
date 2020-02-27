using Konsole;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IDBrowserServiceCore.Code
{
    public class ProgressTaskFactory
    {
        private readonly List<ProgressTask> taskPool;
        private IConsole mainWindow;
        private IConsole topWindow;
        private IConsole bottomWindow;
        private readonly List<string> logs;
        private object progressBarLock;
        private ILogger logger;
        private int concurrentTasks;

        public ProgressTaskFactory(int concurrentTasks, int maxProgress, int progressBarTextWidth, ILogger logger)
        {
            this.concurrentTasks = concurrentTasks;
            this.logger = logger;
            progressBarLock = new object();
            taskPool = new List<ProgressTask>();
            logs = new List<string>();

            InitConsoleWindows();

            for (int i = 0; i < concurrentTasks; i++)
            {
                taskPool.Add(new ProgressTask(this, progressBarLock, topWindow, maxProgress, progressBarTextWidth));
            }
        }

        private void InitConsoleWindows()
        {
            Console.Clear();

            int intMainWindowWidth = Console.WindowWidth;
            int intMainWindowHeight = Console.WindowHeight - 2;
            int intTopWindowHeight = concurrentTasks + 3; // We have to add an additional line here or this will trigger an error under Linux
            int intTopWindowWidth = intMainWindowWidth - 2;
            int intBottomWindowHeight = intMainWindowHeight - intTopWindowHeight - 2;
            int intBottomWindowWidth = intMainWindowWidth - 2;

            mainWindow = Window.OpenBox("Transcoding videos", intMainWindowWidth, intMainWindowHeight);
            topWindow = mainWindow.OpenBox("Tasks", intTopWindowWidth, intTopWindowHeight);
            bottomWindow = mainWindow.OpenBox("Log", 0, intTopWindowHeight, intBottomWindowWidth, intBottomWindowHeight);
        }

        private void RedrawConsoleWindows()
        {
            lock (mainWindow)
            {
                InitConsoleWindows();

                // Redraw logs
                for (int i = 0; i < bottomWindow.WindowHeight; i++)
                {
                    if (logs.Count <= i)
                        return;

                    bottomWindow.WriteLine(logs[i]);
                }
            }
        }

        /// <summary>
        /// Returns the first idle ProgressTask
        /// </summary>
        /// <returns>Idle ProgressTask</returns>
        public ProgressTask GetIdleProgressTask()
        {
            return taskPool.FirstOrDefault(x => x.Task == null);
        }

        /// <summary>
        /// Returns all active tasks
        /// </summary>
        /// <returns>Active tasks</returns>
        public List<Task> GetTasks()
        {
            return taskPool
                .Where(x => x.Task != null)
                .Select(x => x.Task)
                .ToList();
        }

        /// <summary>
        /// Wait's until any task is completed and returns this task.
        /// </summary>
        /// <returns>Completed task</returns>
        public async Task<ProgressTask> WaitForAnyTask()
        {
            Task completedTask = await Task.WhenAny(GetTasks());
            ProgressTask completedProgressTask = taskPool.Single(x => x.Task == completedTask);
            completedProgressTask.Task = null;
            return completedProgressTask;
        }

        /// <summary>
        /// Wait's until all tasks are completed.
        /// </summary>
        public void WaitForAllTasks()
        {
            Task.WhenAll(GetTasks()).Wait();
        }

        /// <summary>
        /// Forces a redraw on all console windows (e.g. in case of console size changed)
        /// </summary>
        /// <param name="maxProgress">Max progress for progress bar's</param>
        /// <param name="progressBarTextWidth">Progress bar text width</param>
        public void RedrawConsoleWindows(int maxProgress, int progressBarTextWidth)
        {
            lock (progressBarLock)
            {
                RedrawConsoleWindows();

                foreach (ProgressTask progressTask in taskPool)
                {
                    progressTask.RedrawProgressBar(topWindow, maxProgress, progressBarTextWidth);
                }
            }
        }

        /// <summary>
        /// Writes a log to the console window and to the supplied Serilog Logger
        /// </summary>
        /// <param name="text">Text to write</param>
        /// <param name="writeToConsole">If true output will be written to console</param>
        /// <param name="logLevel">Serilog log level</param>
        public void WriteLog(string text, bool writeToConsole, LogEventLevel logLevel)
        {
            logger.Write(logLevel, text);

            if (writeToConsole)
            {
                logs.Insert(0, text);

                lock (mainWindow)
                {
                    bottomWindow.WriteLine(text);
                }
            }
        }
    }
}
