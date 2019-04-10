using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDBrowserServiceCore
{
    public class SaveImageThumbnailResult
    {
        public List<Stream> ImageStreams { get; set; }
        public List<Exception> Exceptions { get; set; }

        public SaveImageThumbnailResult()
        {
            ImageStreams = new List<Stream>();
            Exceptions = new List<Exception>();
        }
    }
}
