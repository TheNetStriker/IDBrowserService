using Konsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IDBrowserServiceCore.Code
{
    public class ProgressTask
    {
        public Task Task { get; set; }
        private ProgressBar progressBar;
        public ProgressTaskFactory ProgressTaskFactory { get; private set; }

        private object progressBarLock;
        private int progressBarCurrent;
        private string progressBarText;

        public ProgressTask(ProgressTaskFactory progressTaskFactory, object progressBarLock, IConsole konsoleWindow, int maxProgress)
        {
            this.progressBarLock = progressBarLock;
            ProgressTaskFactory = progressTaskFactory;

            progressBarCurrent = 0;
            progressBarText = "";

            InitProgressBar(konsoleWindow, maxProgress);
        }

        private void InitProgressBar(IConsole konsoleWindow, int maxProgress)
        {
            progressBar = new ProgressBar(konsoleWindow, maxProgress);
        }

        /// <summary>
        /// Forces a redraw of the progress bar (e.g. in case of console size changed)
        /// </summary>
        /// <param name="konsoleWindow">IConsole window</param>
        /// <param name="maxProgress">Max progress</param>
        public void RedrawProgressBar(IConsole konsoleWindow, int maxProgress)
        {
            lock (progressBarLock)
            {
                InitProgressBar(konsoleWindow, maxProgress);
                progressBar.Refresh(progressBarCurrent, progressBarText);
            }
        }

        /// <summary>
        /// Updates progress bar
        /// </summary>
        /// <param name="current">Current percent</param>
        /// <param name="text">Status text</param>
        public void RefreshProgressBar(int current, string text)
        {
            progressBarCurrent = current;
            progressBarText = text;
            
            lock (progressBarLock)
            { 
                progressBar.Refresh(current, text); 
            }               
        }
    }
}
