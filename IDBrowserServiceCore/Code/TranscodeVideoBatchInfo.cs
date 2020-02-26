using System.IO;

namespace IDBrowserServiceCore.Code
{
    public class TranscodeVideoBatchInfo
    {
        public string GUID { get; private set; }
        public int VideoWidth { get; private set; }
        public int VideoHeight { get; private set; }
        public FileInfo VideoFileInfo { get; private set; }
        public FileInfo TranscodeFileInfo { get; private set; }

        public TranscodeVideoBatchInfo(string guid, int videoWidth, int videoHeight, string videoFilePath, string transcodeFilePath)
        {
            this.GUID = guid;
            this.VideoWidth = videoWidth;
            this.VideoHeight = VideoHeight;
            this.VideoFileInfo = new FileInfo(videoFilePath);
            this.TranscodeFileInfo = new FileInfo(transcodeFilePath);
        }
    }
}
