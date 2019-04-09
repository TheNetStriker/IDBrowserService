using ComponentAce.Compression.Libs.zlib;
using IDBrowserServiceCore.Data;
using System;
using System.IO;
using System.Linq;

namespace IDBrowserServiceCore
{
    public class StaticFunctions
    {
        public static FileStream GetImageFileStream(string filePath)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return fileStream;
        }

        public static String GetImageFilePath(idCatalogItem catalogItem, string pathMatch, string pathReplace)
        {
            string strFilePath = catalogItem.idFilePath.FilePath;

            if (!string.IsNullOrEmpty(pathMatch) && !string.IsNullOrEmpty(pathReplace))
                strFilePath = strFilePath.Replace(pathMatch, pathReplace, StringComparison.CurrentCultureIgnoreCase);

            if (Path.DirectorySeparatorChar != '\\')
                strFilePath = strFilePath.Replace('\\', Path.DirectorySeparatorChar);

            return Path.Combine(strFilePath, catalogItem.FileName);
        }

        //public static BitmapFrame GetBitmapFrameFromImageStream(Stream imageStream, String fileType)
        //{
        //    BitmapFrame bitmapFrame;
        //    BitmapDecoder bitmapDecoder;

        //    if (fileType.Equals("TIF", StringComparison.OrdinalIgnoreCase))
        //    {
        //        bitmapDecoder = new TiffBitmapDecoder(imageStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
        //    }
        //    else if (fileType.Equals("PNG", StringComparison.OrdinalIgnoreCase))
        //    {
        //        bitmapDecoder = new PngBitmapDecoder(imageStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
        //    }
        //    else if (fileType.Equals("JPG", StringComparison.OrdinalIgnoreCase) || fileType.Equals("JPEG", StringComparison.OrdinalIgnoreCase))
        //    {
        //        bitmapDecoder = new JpegBitmapDecoder(imageStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
        //    }
        //    else
        //    {
        //        throw new Exception(String.Format("Not supported file type: {0}", fileType));
        //    }

        //    bitmapFrame = bitmapDecoder.Frames[0];
        //    return bitmapFrame;
        //}

        //public static SaveImageThumbnailResult SaveImageThumbnail(idCatalogItem catalogItem, ref IDImagerDB db, ref IDImagerDB dbThumbs,
        //    List<String> types, Boolean keepAspectRatio, Boolean setGenericVideoThumbnailOnError)
        //{
        //    SaveImageThumbnailResult result = new SaveImageThumbnailResult();
        //    Stream imageStream = null;
        //    String filePath = null;

        //    try
        //    {
        //        filePath = GetImageFilePath(catalogItem);
        //        BitmapFrame bitmapFrame;

        //        if (ImageFileExtensions.Contains(catalogItem.idFileType))
        //        {
        //            imageStream = GetImageFileStream(filePath);
        //            bitmapFrame = GetBitmapFrameFromImageStream(imageStream, catalogItem.idFileType);
        //        }
        //        else if (VideoFileExtensions.Contains(catalogItem.idFileType))
        //        {
        //            try
        //            {
        //                bitmapFrame = BitmapFrame.Create((BitmapSource)GenerateVideoThumbnail(filePath, new TimeSpan(0, 0, 0)));
        //            }
        //            catch (Exception ex)
        //            {
        //                if (setGenericVideoThumbnailOnError)
        //                {
        //                    result.Exceptions.Add(new Exception(String.Format("Video thumbnail generation for imageGUID {0} file {1} failed. Generic thumbnails has been set.", catalogItem.GUID, filePath), ex));

        //                    Assembly assembly = Assembly.GetExecutingAssembly();
        //                    Stream genericVideoThumbnailStream = assembly.GetManifestResourceStream(@"IDBrowserServiceCode.Images.image_ph2.png");
        //                    bitmapFrame = BitmapFrame.Create(genericVideoThumbnailStream);
        //                }
        //                else
        //                {
        //                    result.Exceptions.Add(new Exception(String.Format("Video thumbnail generation for imageGUID {0} file {1} failed.", catalogItem.GUID, filePath), ex));
        //                    return result;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            throw new Exception(String.Format("File type {0} not supported", catalogItem.idFileType));
        //        }

        //        foreach (String type in types)
        //        {
        //            int imageWidth;
        //            int imageHeight;

        //            if (type.Equals("T"))
        //            {
        //                imageWidth = 160;
        //                imageHeight = 120;
        //            }
        //            else
        //            {
        //                imageWidth = Int32.Parse(ConfigurationManager.AppSettings["MThumbmailWidth"]);
        //                imageHeight = Int32.Parse(ConfigurationManager.AppSettings["MThumbnailHeight"]);
        //            }

        //            XDocument recipeXDocument = null;
        //            if (type.Equals("T") || type.Equals("R"))
        //            {
        //                if (catalogItem.idHasRecipe > 0)
        //                    recipeXDocument = GetRecipeXDocument(db, catalogItem);
        //            }

        //            TransformGroup transformGroup = new TransformGroup();

        //            if (bitmapFrame.PixelWidth > imageWidth && bitmapFrame.PixelHeight > imageHeight)
        //            {
        //                double scaleX;
        //                double scaleY;

        //                foreach (ScaleTransform existingScaleTransform in transformGroup.Children.OfType<ScaleTransform>().ToList())
        //                    transformGroup.Children.Remove(existingScaleTransform);

        //                if (bitmapFrame.PixelWidth > bitmapFrame.PixelHeight)
        //                {
        //                    scaleX = (double)imageWidth / (double)bitmapFrame.PixelWidth;
        //                    scaleY = (double)imageHeight / (double)bitmapFrame.PixelHeight;
        //                }
        //                else
        //                {
        //                    scaleX = (double)imageHeight / (double)bitmapFrame.PixelHeight;
        //                    scaleY = (double)imageWidth / (double)bitmapFrame.PixelWidth;
        //                }

        //                ScaleTransform scaleTransform = new ScaleTransform(scaleX, scaleY, 0, 0);
        //                transformGroup.Children.Add(scaleTransform);
        //            }

        //            Rotation rotation = StaticFunctions.GetRotation(bitmapFrame);
        //            if (rotation != Rotation.Rotate0)
        //            {
        //                RotateTransform rotateTransform = new RotateTransform();

        //                switch (rotation)
        //                {
        //                    case Rotation.Rotate90:
        //                        rotateTransform.Angle = 90;
        //                        break;
        //                    case Rotation.Rotate180:
        //                        rotateTransform.Angle = 180;
        //                        break;
        //                    case Rotation.Rotate270:
        //                        rotateTransform.Angle = 270;
        //                        break;
        //                }

        //                transformGroup.Children.Add(rotateTransform);
        //            }

        //            Stream resizedImageStream = imageStream;
        //            BitmapSource bitmapSource = bitmapFrame;

        //            if (Recipe.ApplyXmpRecipe(recipeXDocument, ref bitmapSource, transformGroup))
        //            {
        //                BitmapFrame transformedBitmapFrame = BitmapFrame.Create(bitmapSource);

        //                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
        //                encoder.Frames.Add(transformedBitmapFrame);
        //                resizedImageStream = new System.IO.MemoryStream();
        //                encoder.Save(resizedImageStream);
        //                resizedImageStream.Position = 0;
        //            }

        //            lock (dbThumbs)
        //            {
        //                idThumbs newThumb = new idThumbs();
        //                newThumb.GUID = Guid.NewGuid().ToString().ToUpper();
        //                newThumb.ImageGUID = catalogItem.GUID;
        //                newThumb.idThumb = StreamToByteArray(resizedImageStream);
        //                newThumb.idType = type;

        //                dbThumbs.idThumbs.Add(newThumb);
        //            }

        //            result.ImageStreams.Add(resizedImageStream);
        //        }

        //        if (imageStream != null) { imageStream.Close(); }
        //        dbThumbs.SaveChanges();
        //    }
        //    catch (Exception ex)
        //    {
        //        if (imageStream != null) { imageStream.Close(); }
        //        result.Exceptions.Add(new Exception(String.Format("Error generating thumbnail for imageGUID {0} file {1}", catalogItem.GUID, filePath), ex));
        //    }

        //    return result;
        //}

        public static System.Xml.Linq.XDocument GetRecipeXDocument(IDImagerDB db, idCatalogItem catalogItem)
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
                    System.Xml.Linq.XDocument xdocument = System.Xml.Linq.XDocument.Load(decompressedXmpStream);
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

        //public static ImageSource GenerateVideoThumbnail(String fileName, TimeSpan position)
        //{
        //    MediaPlayer mediaPlayer = new MediaPlayer();
        //    mediaPlayer.ScrubbingEnabled = true;
        //    mediaPlayer.Open(new Uri(fileName));
        //    mediaPlayer.Position = position;

        //    int intTimeoutStep = 10;
        //    int intTimeout = 0;

        //    while (mediaPlayer.NaturalVideoWidth == 0 && mediaPlayer.NaturalVideoHeight == 0)
        //    {
        //        System.Threading.Thread.Sleep(intTimeoutStep);
        //        intTimeout += intTimeoutStep;
        //        if (intTimeout > 5000)
        //        {
        //            throw new Exception("Could not load video");
        //        };
        //    }

        //    uint[] framePixels = new uint[mediaPlayer.NaturalVideoWidth * mediaPlayer.NaturalVideoHeight];
        //    uint[] previousFramePixels = new uint[framePixels.Length];

        //    // Render the current frame into a bitmap
        //    var drawingVisual = new DrawingVisual();
        //    var renderTargetBitmap = new RenderTargetBitmap(mediaPlayer.NaturalVideoWidth, mediaPlayer.NaturalVideoHeight, 96, 96, PixelFormats.Default);
        //    using (var drawingContext = drawingVisual.RenderOpen())
        //    {
        //        drawingContext.DrawVideo(mediaPlayer, new Rect(0, 0, mediaPlayer.NaturalVideoWidth, mediaPlayer.NaturalVideoHeight));
        //    }
        //    renderTargetBitmap.Render(drawingVisual);

        //    // Copy the pixels to the specified location
        //    renderTargetBitmap.CopyPixels(framePixels, mediaPlayer.NaturalVideoWidth * 4, 0);
        //    mediaPlayer.Close();

        //    return renderTargetBitmap;
        //}
    }
}
