using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IDBrowserServiceCore.Settings
{
    public class SiteSettings
    {
        public ConnectionStringSettings ConnectionStrings { get; set; }
        public ServiceSettings ServiceSettings { get; set; }
    }
}
