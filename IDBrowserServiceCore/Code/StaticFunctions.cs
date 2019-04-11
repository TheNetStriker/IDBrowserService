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

namespace IDBrowserServiceCore.Code
{
    public class StaticFunctions
    {
        private static List<String> imageFileExtensions;
        private static List<String> videoFileExtensions;

        public static IConfiguration Configuration { get; set; }

        public static List<String> ImageFileExtensions
        {
            get
            {
                if (imageFileExtensions == null)
                {
                    imageFileExtensions = Configuration["IDBrowserServiceSettings:ImageFileExtensions"]
                        .Split(new char[] { char.Parse(",") }).ToList();
                };
                return imageFileExtensions;
            }
        }

        public static List<String> VideoFileExtensions
        {
            get
            {
                if (videoFileExtensions == null)
                {
                    videoFileExtensions = Configuration["IDBrowserServiceSettings:VideoFileExtensions"]
                        .Split(new char[] { char.Parse(",") }).ToList();
                };
                return videoFileExtensions;
            }
        }

        public static FileStream GetImageFileStream(string filePath)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return fileStream;
        }

        public static String GetImageFilePath(idCatalogItem catalogItem)
        {
            string strPathMatch = Configuration["IDBrowserServiceSettings:FilePathReplace:PathMatch"];
            string strPathReplace = Configuration["IDBrowserServiceSettings:FilePathReplace:PathReplace"];
            string strFilePath = catalogItem.idFilePath.FilePath;

            if (!string.IsNullOrEmpty(strPathMatch) && !string.IsNullOrEmpty(strPathReplace))
                strFilePath = strFilePath.Replace(strPathMatch, strPathReplace, StringComparison.CurrentCultureIgnoreCase);

            if (Path.DirectorySeparatorChar != '\\')
                strFilePath = strFilePath.Replace('\\', Path.DirectorySeparatorChar);

            return Path.Combine(strFilePath, catalogItem.FileName);
        }

        public static SaveImageThumbnailResult SaveImageThumbnail(idCatalogItem catalogItem, IDImagerDB db, IDImagerThumbsDB dbThumbs,
            List<String> types, Boolean keepAspectRatio, Boolean setGenericVideoThumbnailOnError)
        {
            SaveImageThumbnailResult result = new SaveImageThumbnailResult();
            Stream imageStream = null;
            String filePath = null;

            try
            {
                filePath = GetImageFilePath(catalogItem);

                //if (ImageFileExtensions.Contains(catalogItem.idFileType))
                //{
                imageStream = GetImageFileStream(filePath);
                //}
                //else if (VideoFileExtensions.Contains(catalogItem.idFileType))
                //{
                    //try
                    //{
                    //    bitmapFrame = BitmapFrame.Create((BitmapSource)GenerateVideoThumbnail(filePath, new TimeSpan(0, 0, 0)));
                    //}
                    //catch (Exception ex)
                    //{
                    //    if (setGenericVideoThumbnailOnError)
                    //    {
                    //        result.Exceptions.Add(new Exception(String.Format("Video thumbnail generation for imageGUID {0} file {1} failed. Generic thumbnails has been set.", catalogItem.GUID, filePath), ex));

                    //        Assembly assembly = Assembly.GetExecutingAssembly();
                    //        Stream genericVideoThumbnailStream = assembly.GetManifestResourceStream(@"IDBrowserServiceCode.Images.image_ph2.png");
                    //        bitmapFrame = BitmapFrame.Create(genericVideoThumbnailStream);
                    //    }
                    //    else
                    //    {
                    //        result.Exceptions.Add(new Exception(String.Format("Video thumbnail generation for imageGUID {0} file {1} failed.", catalogItem.GUID, filePath), ex));
                    //        return result;
                    //    }
                    //}
                //}
                //else
                //{
                //    throw new Exception(String.Format("File type {0} not supported", catalogItem.idFileType));
                //}

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
                        imageWidth = Int32.Parse(Configuration["IDBrowserServiceSettings:MThumbmailWidth"]);
                        imageHeight = Int32.Parse(Configuration["IDBrowserServiceSettings:MThumbnailHeight"]);
                    }

                    XDocument recipeXDocument = null;
                    if (type.Equals("T") || type.Equals("R"))
                    {
                        if (catalogItem.idHasRecipe > 0)
                            recipeXDocument = GetRecipeXDocument(db, catalogItem);
                    }

                    MemoryStream resizedImageStream = new MemoryStream();

                    imageStream.Position = 0;
                    MagickImage image = new MagickImage(imageStream, new MagickReadSettings { Format = MagickFormat.Mp4 });

                    image.Format = MagickFormat.Jpeg;
                    image.Resize(imageWidth, imageHeight);
                    image.Write(resizedImageStream);
                    resizedImageStream.Position = 0;

                    //if (Recipe.ApplyXmpRecipe(recipeXDocument, ref bitmapSource, transformGroup))
                    //{
                    //    BitmapFrame transformedBitmapFrame = BitmapFrame.Create(bitmapSource);

                    //    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    //    encoder.Frames.Add(transformedBitmapFrame);
                    //    resizedImageStream = new System.IO.MemoryStream();
                    //    encoder.Save(resizedImageStream);
                    //    resizedImageStream.Position = 0;
                    //}
                    
                    lock (dbThumbs)
                    {
                        bool boolThumbExists = dbThumbs.idThumbs
                            .Any(x => x.ImageGUID == catalogItem.GUID  && x.idType == type);

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
                    }

                    result.ImageStreams.Add(resizedImageStream);
                }

                if (imageStream != null) { imageStream.Close(); }
                dbThumbs.SaveChanges();
            }
            catch (Exception ex)
            {
                if (imageStream != null) { imageStream.Close(); }
                result.Exceptions.Add(new Exception(String.Format("Error generating thumbnail for imageGUID {0} file {1}", catalogItem.GUID, filePath), ex));
            }

            return result;
        }

        public static XDocument GetRecipeXDocument(IDImagerDB db, idCatalogItem catalogItem)
        {
            if (catalogItem.idHasRecipe > 0)
            {
                idImageData imageData = db.idImageData.SingleOrDefault(x => x.ImageGUID.Equals(catalogItem.GUID) && x.DataName.Equals("XMP"));

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

        //public static void Resize(ref BitmapSource bitmapSource, ref TransformGroup transformGroup, int width, int height)
        //{
        //    if (bitmapSource.PixelWidth > width && bitmapSource.PixelHeight > height)
        //    {
        //        double scale;

        //        foreach (ScaleTransform existingScaleTransform in transformGroup.Children.OfType<ScaleTransform>().ToList())
        //            transformGroup.Children.Remove(existingScaleTransform);

        //        if (bitmapSource.PixelWidth > bitmapSource.PixelHeight)
        //        {
        //            scale = (double)width / (double)bitmapSource.PixelWidth;
        //        }
        //        else
        //        {
        //            scale = (double)height / (double)bitmapSource.PixelHeight;
        //        }

        //        ScaleTransform scaleTransform = new ScaleTransform(scale, scale, 0, 0);
        //        transformGroup.Children.Add(scaleTransform);
        //    }
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
