using ComponentAce.Compression.Libs.zlib;
using IDBrowserServiceCore.Data;
using IDBrowserServiceCore.Data.IDImager;
using IDBrowserServiceCore.Data.IDImagerThumbs;
using ImageMagick;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using IDBrowserServiceCore.Code.XmpRecipe;
using IDBrowserServiceCore.Settings;
using FFmpeg.NET;
using FFmpeg.NET.Enums;

namespace IDBrowserServiceCore.Code
{
    public class StaticFunctions
    {
        public static FileStream GetImageFileStream(string filePath)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return fileStream;
        }

        public static String GetImageFilePath(idCatalogItem catalogItem, List<FilePathReplaceSettings> filePathReplaceSettings)
        {
            string strFilePath = catalogItem.idFilePath.FilePath;

            if (filePathReplaceSettings != null)
            {
                foreach (FilePathReplaceSettings settings in filePathReplaceSettings)
                {
                    string strPathMatch = settings.PathMatch;
                    string strPathReplace = settings.PathReplace;
                    if (!string.IsNullOrEmpty(strPathMatch) && !string.IsNullOrEmpty(strPathReplace))
                        strFilePath = strFilePath.Replace(strPathMatch, strPathReplace, StringComparison.CurrentCultureIgnoreCase);

                    if (Path.DirectorySeparatorChar != '\\')
                        strFilePath = strFilePath.Replace('\\', Path.DirectorySeparatorChar);
                }
            }

            return Path.Combine(strFilePath, catalogItem.FileName);
        }

        public async static Task<SaveImageThumbnailResult> SaveImageThumbnail(idCatalogItem catalogItem, IDImagerDB db, IDImagerThumbsDB dbThumbs,
            List<String> types, Boolean keepAspectRatio, Boolean setGenericVideoThumbnailOnError, ServiceSettings serviceSettings)
        {
            SaveImageThumbnailResult result = new SaveImageThumbnailResult();
            Stream imageStream = null;
            String filePath = null;

            try
            {
                filePath = GetImageFilePath(catalogItem, serviceSettings.FilePathReplace);
                imageStream = GetImageFileStream(filePath);

                foreach (String type in types)
                {
                    int imageWidth;
                    int imageHeight;

                    if (type.Equals("T"))
                    {
                        imageWidth = 160;
                        imageHeight = 120;
                    }
                    else
                    {
                        imageWidth = serviceSettings.MThumbmailWidth;
                        imageHeight = serviceSettings.MThumbnailHeight;
                    }

                    XmpRecipeContainer xmpRecipeContainer = null;
                    if (type.Equals("T") || type.Equals("R"))
                    {
                        if (catalogItem.idHasRecipe > 0)
                        {
                            XDocument recipeXDocument = await GetRecipeXDocument(db, catalogItem);
                            xmpRecipeContainer = XmpRecipeHelper.ParseXmlRecepie(recipeXDocument);
                        }
                    }

                    MemoryStream resizedImageStream = new MemoryStream();
                    MagickReadSettings magickReadSettings = null;

                    if (Enum.TryParse<MagickFormat>(catalogItem.idFileType, true, out MagickFormat magickFormat))
                    {
                        magickReadSettings = new MagickReadSettings { Format = magickFormat };
                    }

                    imageStream.Position = 0;

                    MagickImage image = new MagickImage(imageStream, magickReadSettings)
                    {
                        Format = MagickFormat.Jpeg,
                    }; 

                    image.Resize(imageWidth, imageHeight);

                    if (xmpRecipeContainer != null)
                        XmpRecipeHelper.ApplyXmpRecipe(xmpRecipeContainer, image);

                    image.Write(resizedImageStream);
                    resizedImageStream.Position = 0;

                    bool boolThumbExists = await dbThumbs.idThumbs
                        .AnyAsync(x => x.ImageGUID == catalogItem.GUID  && x.idType == type);

                    if (!boolThumbExists)
                    {
                        idThumbs newThumb = new idThumbs
                        {
                            GUID = Guid.NewGuid().ToString().ToUpper(),
                            ImageGUID = catalogItem.GUID,
                            idThumb = StreamToByteArray(resizedImageStream),
                            idType = type
                        };

                        dbThumbs.idThumbs.Add(newThumb);
                    }

                    result.ImageStreams.Add(resizedImageStream);
                }

                if (imageStream != null) { imageStream.Close(); }
                await dbThumbs.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (imageStream != null) { imageStream.Close(); }
                result.Exceptions.Add(new Exception(String.Format("Error generating thumbnail for imageGUID {0} file {1}", catalogItem.GUID, filePath), ex));
            }

            return result;
        }

        public async static Task<XDocument> GetRecipeXDocument(IDImagerDB db, idCatalogItem catalogItem)
        {
            if (catalogItem.idHasRecipe > 0)
            {
                idImageData imageData = await db.idImageData.SingleOrDefaultAsync(x => x.ImageGUID.Equals(catalogItem.GUID) && x.DataName.Equals("XMP"));

                if (imageData != null)
                {
                    MemoryStream compressedXmpStream = new MemoryStream(imageData.idData);
                    MemoryStream decompressedXmpStream = new MemoryStream();
                    ZOutputStream outZStream = new ZOutputStream(decompressedXmpStream);

                    compressedXmpStream.CopyTo(outZStream);
                    decompressedXmpStream.Position = 0;
                    XDocument xdocument = XDocument.Load(decompressedXmpStream);
                    compressedXmpStream.Close();
                    decompressedXmpStream.Close();

                    return xdocument;
                }
            }

            return null;
        }

        public static byte[] StreamToByteArray(Stream inputStream)
        {
            if (!inputStream.CanRead)
                throw new ArgumentException();

            if (inputStream.CanSeek)
                inputStream.Seek(0, SeekOrigin.Begin);

            byte[] output = new byte[inputStream.Length];
            int bytesRead = inputStream.Read(output, 0, output.Length);

            if (inputStream.CanSeek)
                inputStream.Seek(0, SeekOrigin.Begin);

            return output;
        }

        public static void Resize(MagickImage image, int width, int height)
        {
            if (image.Width > width && image.Height > height)
            {
                image.Resize(width, height);
            }
        }

        public async static Task<string> TranscodeVideo(string filePath, string guid, string transcodeDirectory, string videosize)
        {
            string strTranscodeDirectory = Path.Combine(transcodeDirectory, videosize, guid.Substring(0, 2));
            string strTranscodeFilePath = Path.Combine(strTranscodeDirectory, guid + ".mp4");

            if (!File.Exists(strTranscodeFilePath))
            {
                var inputFile = new MediaFile(filePath);
                var outputFile = new MediaFile(strTranscodeFilePath);
                VideoSize videoSize = (VideoSize)Enum.Parse(typeof(VideoSize), videosize);

                if (!Directory.Exists(strTranscodeDirectory))
                    Directory.CreateDirectory(strTranscodeDirectory);

                var conversionOptions = new ConversionOptions
                {
                    VideoSize = videoSize
                };

                var ffmpeg = new Engine();
                await ffmpeg.ConvertAsync(inputFile, outputFile, conversionOptions);
            }

            return strTranscodeFilePath;
        }

        //public static Rotation Rotate(ref BitmapSource bitmapSource, ref TransformGroup transformGroup)
        //{
        //    Rotation rotation = StaticFunctions.GetRotation(bitmapSource);
        //    if (rotation != Rotation.Rotate0)
        //    {
        //        RotateTransform rotateTransform = new RotateTransform();

        //        switch (rotation)
        //        {
        //            case Rotation.Rotate90:
        //                rotateTransform.Angle = 90;
        //                break;
        //            case Rotation.Rotate180:
        //                rotateTransform.Angle = 180;
        //                break;
        //            case Rotation.Rotate270:
        //                rotateTransform.Angle = 270;
        //                break;
        //        }

        //        transformGroup.Children.Add(rotateTransform);
        //    }

        //    return rotation;
        //}


        public const string OrientationQuery = "System.Photo.Orientation";

        //public static Rotation GetRotation(BitmapSource bitmapSource)
        //{
        //    BitmapMetadata bitmapMetadata = bitmapSource.Metadata as BitmapMetadata;

        //    if ((bitmapMetadata != null) && (bitmapMetadata.ContainsQuery(OrientationQuery)))
        //    {
        //        object o = bitmapMetadata.GetQuery(OrientationQuery);

        //        if (o != null)
        //        {
        //            //refer to http://www.impulseadventure.com/photo/exif-orientation.html for details on orientation values
        //            switch ((ushort)o)
        //            {
        //                case 6:
        //                    return Rotation.Rotate90;
        //                case 3:
        //                    return Rotation.Rotate180;
        //                case 8:
        //                    return Rotation.Rotate270;
        //            }
        //        }
        //    }

        //    return Rotation.Rotate0;
        //}
    }
}
