﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IDBrowserServiceCore.Settings
{
    public class ServiceSettings
    {
        public bool CreateThumbnails { get; set; }
        public int MThumbmailWidth { get; set; }
        public int MThumbnailHeight { get; set; }
        public bool KeepAspectRatio { get; set; }
        public bool SetGenericVideoThumbnailOnError { get; set; }
        public FilePathReplaceSettings FilePathReplace { get; set; }
        
    }
}