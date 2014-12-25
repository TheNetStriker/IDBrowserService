using CommandLine;
using IDBrowserServiceCode;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThumbnailGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                GenerateThumbnails();
            }
            catch (Exception e)
            {
                LogException(e);
                Console.WriteLine(e.ToString());
                Console.ReadLine();
            }
        }

        //private static void GenerateThumbnails(String type)
        //{
        //    Boolean keepAspectRatio = Boolean.Parse(ConfigurationManager.AppSettings["KeepAspectRatio"]);
        //    Boolean setGenericVideoThumbnailOnError = Boolean.Parse(ConfigurationManager.AppSettings["SetGenericVideoThumbnailOnError"]);

        //    IDImagerEntities db = new IDBrowserServiceCode.IDImagerEntities();
        //    IDImagerEntities dbThumbs = new IDBrowserServiceCode.IDImagerEntities(ConfigurationManager.ConnectionStrings["IDImagerThumbsEntities"].ConnectionString);
            
        //    List<String> imageFileExtensions = ConfigurationManager.AppSettings["ImageFileExtensions"].Split(new char[] {char.Parse(",")}).ToList();
        //    List<String> videoFileExtensions = ConfigurationManager.AppSettings["VideoFileExtensions"].Split(new char[] { char.Parse(",") }).ToList();

        //    List<String> thumbImageGuids = dbThumbs.idThumbs.Where(x => x.idType.Equals(type)).Select(x => x.ImageGUID).ToList();

        //    Console.WriteLine("Getting images without thumbnails....");
        //    var queryThumbnails = from catalogItem in db.idCatalogItem.Include("idFilePath")
        //                          where !thumbImageGuids.Contains(catalogItem.GUID) && (imageFileExtensions.Contains(catalogItem.idFileType) || videoFileExtensions.Contains(catalogItem.idFileType))
        //                          select catalogItem;

        //    int intCountCatalogItem = queryThumbnails.Count();
        //    int intCatalogItemCounter = 0;

        //    foreach (idCatalogItem catalogItem in queryThumbnails)
        //    {
        //        try
        //        {
        //            SaveImageThumbnailResult result = StaticFunctions.SaveImageThumbnail(catalogItem, ref db, ref dbThumbs, new List<String>() { type }, keepAspectRatio, setGenericVideoThumbnailOnError);
        //            foreach (Exception ex in result.Exceptions)
        //            {
        //                LogException(ex);
        //                Console.WriteLine(ex.ToString());
        //            }

        //            if (result.Exceptions.Count > 0)
        //            {
        //                LogFailedCatalogItem(catalogItem);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            LogException(ex);
        //            LogFailedCatalogItem(catalogItem);
        //            Console.WriteLine(ex.ToString());
        //        }

        //        intCatalogItemCounter += 1;
        //        Console.CursorLeft = 0;
        //        Console.Write(String.Format("{0} of {1} T thumbnails generated", intCatalogItemCounter, intCountCatalogItem));
        //    }
        //}

        private static void GenerateThumbnails()
        {
            Boolean keepAspectRatio = Boolean.Parse(ConfigurationManager.AppSettings["KeepAspectRatio"]);
            Boolean setGenericVideoThumbnailOnError = Boolean.Parse(ConfigurationManager.AppSettings["SetGenericVideoThumbnailOnError"]);

            var commandLineArguments = new CommandLineArguments();
            ICommandLineParser parser = new CommandLineParser();
            parser.ParseArguments(Environment.GetCommandLineArgs(), commandLineArguments);

            IDImagerEntities db = new IDBrowserServiceCode.IDImagerEntities();
            IDImagerEntities dbThumbs = new IDBrowserServiceCode.IDImagerEntities(ConfigurationManager.ConnectionStrings["IDImagerThumbsEntities"].ConnectionString);

            List<String> imageFileExtensions = ConfigurationManager.AppSettings["ImageFileExtensions"].Split(new char[] { char.Parse(",") }).ToList();
            List<String> videoFileExtensions = ConfigurationManager.AppSettings["VideoFileExtensions"].Split(new char[] { char.Parse(",") }).ToList();
            
            var queryCatalogItem = from catalogItem in db.idCatalogItem.Include("idFilePath")
                                   where imageFileExtensions.Contains(catalogItem.idFileType) || videoFileExtensions.Contains(catalogItem.idFileType)
                                   select catalogItem;

            if (commandLineArguments.FromDateTime != DateTime.MinValue)
                queryCatalogItem = queryCatalogItem.Where(x => x.idCreated >= commandLineArguments.FromDateTime);

            if (commandLineArguments.ToDateTime != DateTime.MinValue)
                queryCatalogItem = queryCatalogItem.Where(x => x.idCreated <= commandLineArguments.ToDateTime);

            if (commandLineArguments.FileFilter != null)
                queryCatalogItem = queryCatalogItem.Where(x => x.idFileType.ToLower().Equals(commandLineArguments.FileFilter.ToLower()));

            if (commandLineArguments.ImageGuid != null)
                queryCatalogItem = queryCatalogItem.Where(x => x.GUID.Equals(commandLineArguments.ImageGuid, StringComparison.OrdinalIgnoreCase));

            int intCountCatalogItem = queryCatalogItem.Count();
            int intCatalogItemCounter = 0;
            int intThumbnailsGenerated = 0;

            foreach (idCatalogItem catalogItem in queryCatalogItem)
            {
                List<String> typesToGenerate = new List<String>();
                List<idThumbs> idThumbsT = dbThumbs.idThumbs.Where(x => x.ImageGUID == catalogItem.GUID && x.idType.Equals("T")).ToList();
                List<idThumbs> idThumbsM = dbThumbs.idThumbs.Where(x => x.ImageGUID == catalogItem.GUID && x.idType.Equals("M")).ToList();
                List<idThumbs> idThumbsR = dbThumbs.idThumbs.Where(x => x.ImageGUID == catalogItem.GUID && x.idType.Equals("R")).ToList();

                if (idThumbsT.Count() == 0)
                    typesToGenerate.Add("T");

                if (idThumbsM.Count() == 0)
                    typesToGenerate.Add("M");

                if (catalogItem.idHasRecipe > 0 && idThumbsR.Count() == 0)
                    typesToGenerate.Add("R");
                
                if (commandLineArguments.Overwrite)
                {
                    foreach (idThumbs thumb in idThumbsT)
                        dbThumbs.DeleteObject(thumb);

                    foreach (idThumbs thumb in idThumbsM)
                        dbThumbs.DeleteObject(thumb);

                    foreach (idThumbs thumb in idThumbsR)
                        dbThumbs.DeleteObject(thumb);

                    typesToGenerate.Clear();
                    typesToGenerate.Add("T");
                    typesToGenerate.Add("M");
                    if (catalogItem.idHasRecipe > 0)
                        typesToGenerate.Add("R");
                }

                if (typesToGenerate.Count() > 0)
                {
                    try
                    {
                        SaveImageThumbnailResult result = StaticFunctions.SaveImageThumbnail(catalogItem, ref db, ref dbThumbs, typesToGenerate, keepAspectRatio, setGenericVideoThumbnailOnError);
                        foreach (Exception ex in result.Exceptions)
                        {
                            LogException(ex);
                            Console.WriteLine(ex.ToString());
                        }

                        if (result.Exceptions.Count > 0)
                            LogFailedCatalogItem(catalogItem);

                        intThumbnailsGenerated += result.ImageStreams.Count();
                    }
                    catch (Exception e)
                    {
                        LogException(e);
                        LogFailedCatalogItem(catalogItem);
                        Console.WriteLine(e.ToString());
                    }
                }
                
                intCatalogItemCounter += 1;
                
                Console.CursorLeft = 0;
                Console.Write(String.Format("{0} of {1} catalogitems checked. {2} thumbnails generated", intCatalogItemCounter, intCountCatalogItem, intThumbnailsGenerated));
            }
        }

        private static void LogException(Exception e)
        {
            System.IO.File.AppendAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ThumbnailGeneratorErrorLog.txt"), e.ToString() + "\r\n-----------------------------------\r\n");
        }

        private static void LogFailedCatalogItem(idCatalogItem catalogItem)
        {
            System.IO.File.AppendAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ThumbnailGeneratorFailedCatalogItems.txt"), catalogItem.GUID + "\r\n");
        }
    }
}
