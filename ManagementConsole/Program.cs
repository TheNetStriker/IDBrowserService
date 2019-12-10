using IDBrowserServiceCore.Code;
using IDBrowserServiceCore.Data.IDImager;
using IDBrowserServiceCore.Data.PostgresHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ManagementConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            string strConnection = "Host=172.17.2.17;Database=photosupreme_mad;Username=idimager_main;Password=idi_main_2606;";

            var optionsBuilder = new DbContextOptionsBuilder<IDImagerDB>();

            optionsBuilder.UseNpgsql(strConnection);
            optionsBuilder.ReplaceService<ISqlGenerationHelper, PostgresSqlGenerationHelper>();

            IDImagerDB db = new IDImagerDB(optionsBuilder.Options);

            List<string> videoFileTypes = new List<string>() { 
                "M2TS",
                "3GP",
                "AVI",
                "TIF",
                "M4V",
                "MPG",
                "MTS",
                "MP4",
                "MOV",};

            var query = db.idCatalogItem
                .Include(x => x.idFilePath)
                .Where(x => videoFileTypes.Contains(x.idFileType));

            string strTranscodeDirectory = "\\\\QNAPNAS01\\Multimedia\\VideoTranscode\\mad";
            string strVideoSize = "Hd480";
            int intTotalCount = query.Count();
            int intCounter = 0;

            foreach (idCatalogItem catalogItem in query)
            {
                String strFilePath = StaticFunctions.GetImageFilePath(catalogItem, null);
                string strTranscodedFile = StaticFunctions.TranscodeVideo(strFilePath, catalogItem.GUID,
                    strTranscodeDirectory, strVideoSize).Result;

                intCounter++;

                Console.WriteLine(string.Format("{0}/{1}: {2}", intCounter, intTotalCount, strFilePath));
            }
        }
    }
}
