using System.Threading.Tasks;
using Terminal.Gui.Views;

namespace IDBrowserServiceCore.Code
{
    public class ProgressTask
    {
        public Task Task { get; set; }
        public ProgressTaskFactory ProgressTaskFactory { get; private set; }

        private readonly int index;
        private int progressBarCurrent;
        private string progressBarText;

        public ProgressTask(ProgressTaskFactory progressTaskFactory, int index)
        {
            ProgressTaskFactory = progressTaskFactory;
            this.index = index;
            progressBarCurrent = 0;
            progressBarText = string.Empty;
        }

        /// <summary>
        /// Updates the progress bar with the current percentage and label text.
        /// </summary>
        /// <param name="current">Progress percentage 0–100.</param>
        /// <param name="text">Label text to display on the bar.</param>
        public void RefreshProgressBar(int current, string text)
        {
            progressBarCurrent = current;
            progressBarText = text;

            float fraction = current / 100f;

            ProgressTaskFactory.InvokeOnUiThread(() =>
            {
                ProgressBar pb = ProgressTaskFactory.GetProgressBar(index);
                pb.Fraction = fraction;
                pb.SetNeedsDraw();

                Label lbl = ProgressTaskFactory.GetFileLabel(index);
                lbl.Text = text;
                lbl.SetNeedsDraw();
            });
        }
    }
}
