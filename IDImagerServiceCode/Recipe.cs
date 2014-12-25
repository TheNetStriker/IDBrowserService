using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace IDBrowserServiceCode
{
    public class Recipe
    {
        private static XName xNameDescription = XName.Get("Description", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");

        //iles namespaces
        private static XName xNameIles = XName.Get("iles", "http://www.w3.org/2000/xmlns/");

        //Rotate namespaces
        private static XName xNameIlesRotate = XName.Get("Rotate", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesRotate_RecipeEnabled = XName.Get("RecipeEnabled", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesRotate_FriendlyName = XName.Get("FriendlyName", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesRotate_Opacity = XName.Get("Opacity", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesRotate_BlendMode = XName.Get("BlendMode", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesRotate_Angle = XName.Get("Angle", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesRotate_Crop = XName.Get("Crop", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesRotate_BackgroundColor = XName.Get("BackgroundColor", "http://ns.idimager.com/iles/1.0/");

        //Resize namespaces
        private static XName xNameIlesResizeFixed = XName.Get("ResizeFixed", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesResizeFixed_RecipeEnabled = XName.Get("RecipeEnabled", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesResizeFixed_FriendlyName = XName.Get("FriendlyName", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesResizeFixed_GUID = XName.Get("GUID", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesResizeFixed_Enabled = XName.Get("Enabled", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesResizeFixed_Width = XName.Get("Width", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesResizeFixed_Height = XName.Get("Height", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesResizeFixed_WidthDisplayUnit = XName.Get("WidthDisplayUnit", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesResizeFixed_HeightDisplayUnit = XName.Get("HeightDisplayUnit", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesResizeFixed_AdjustResolution = XName.Get("AdjustResolution", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesResizeFixed_Resolution = XName.Get("Resolution", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesResizeFixed_ResolutionDisplayUnit = XName.Get("ResolutionDisplayUnit", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesResizeFixed_Proportional = XName.Get("Proportional", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesResizeFixed_ProportionalSetting = XName.Get("ProportionalSetting", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesResizeFixed_EnlargeSmallImages = XName.Get("EnlargeSmallImages", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesResizeFixed_MaintainRatio = XName.Get("MaintainRatio", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesResizeFixed_Interpolated = XName.Get("Interpolated", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesResizeFixed_Filter = XName.Get("Filter", "http://ns.idimager.com/iles/1.0/");

        //Crop namespaces
        private static XName xNameIlesCrop = XName.Get("Crop", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesCrop_RecipeEnabled = XName.Get("RecipeEnabled", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesCrop_FriendlyName = XName.Get("FriendlyName", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesCrop_Opacity = XName.Get("Opacity", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesCrop_BlendMode = XName.Get("BlendMode", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesCrop_Left = XName.Get("Left", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesCrop_Top = XName.Get("Top", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesCrop_Right = XName.Get("Right", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesCrop_Bottom = XName.Get("Bottom", "http://ns.idimager.com/iles/1.0/");

        //Straighten namespaces
        private static XName xNameIlesStraighten = XName.Get("Straighten", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesStraighten_RecipeEnabled = XName.Get("RecipeEnabled", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesStraighten_FriendlyName = XName.Get("FriendlyName", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesStraighten_Opacity = XName.Get("Opacity", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesStraighten_BlendMode = XName.Get("BlendMode", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesStraighten_Angle = XName.Get("Angle", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesStraighten_Crop = XName.Get("Crop", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesStraighten_BackgroundColor = XName.Get("BackgroundColor", "http://ns.idimager.com/iles/1.0/");

        //Flip namespaces
        private static XName xNameIlesFlip = XName.Get("Flip", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesFlip_RecipeEnabled = XName.Get("RecipeEnabled", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesFlip_FriendlyName = XName.Get("FriendlyName", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesFlip_Opacity = XName.Get("Opacity", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesFlip_BlendMode = XName.Get("BlendMode", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesFlip_FlipVertical = XName.Get("FlipVertical", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesFlip_FlipHorizontal = XName.Get("FlipHorizontal", "http://ns.idimager.com/iles/1.0/");

        //Frame namespaces
        private static XName xNameIlesFrame = XName.Get("Frame", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesFrame_RecipeEnabled = XName.Get("RecipeEnabled", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesFrame_FriendlyName = XName.Get("FriendlyName", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesFrame_Opacity = XName.Get("Opacity", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesFrame_BlendMode = XName.Get("BlendMode", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesFrame_FrameStream = XName.Get("FrameStream", "http://ns.idimager.com/iles/1.0/");

        //Title namespaces
        private static XName xNameIlesTitle = XName.Get("Title", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesTitle_RecipeEnabled = XName.Get("RecipeEnabled", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesTitle_FriendlyName = XName.Get("FriendlyName", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesTitle_TitleStream = XName.Get("TitleStream", "http://ns.idimager.com/iles/1.0/");

        //Watermarks namespaces
        private static XName xNameIlesWatermarks = XName.Get("Watermarks", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesWatermarks_RecipeEnabled = XName.Get("RecipeEnabled", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesWatermarks_FriendlyName = XName.Get("FriendlyName", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesWatermarks_Opacity = XName.Get("Opacity", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesWatermarks_BlendMode = XName.Get("BlendMode", "http://ns.idimager.com/iles/1.0/");
        private static XName xNameIlesWatermarks_WatermarksStream = XName.Get("WatermarksStream", "http://ns.idimager.com/iles/1.0/");

        //public static Stream ApplyXmpRecipe(XDocument xdocument, Stream imageStream, String fileType)
        //{
        //    BitmapFrame bitmapFrame = StaticFunctions.getBitmapFrameFromImageStream(imageStream, fileType);
        //    TransformGroup transformGroup = GetRecipeTransformGroup(xdocument, bitmapFrame);

        //    if (transformGroup.Children.Count > 0)
        //    {
        //        TransformedBitmap tb = new TransformedBitmap();
        //        tb.BeginInit();
        //        tb.Source = bitmapFrame;
        //        tb.Transform = transformGroup;
        //        tb.EndInit();

        //        bitmapFrame = BitmapFrame.Create(tb);

        //        JpegBitmapEncoder encoder = new JpegBitmapEncoder();
        //        encoder.Frames.Add(bitmapFrame);
        //        imageStream = new System.IO.MemoryStream();
        //        encoder.Save(imageStream);
        //        imageStream.Position = 0;
        //    }

        //    return imageStream;
        //}

        public static Stream ApplyXmpRecipe(XDocument xdocument, Stream imageStream, String fileType)
        {
            BitmapSource bitmapSource = StaticFunctions.GetBitmapFrameFromImageStream(imageStream, fileType);

            if (ApplyXmpRecipe(xdocument, ref bitmapSource, new TransformGroup()))
            {
                BitmapFrame transformedBitmapFrame = BitmapFrame.Create(bitmapSource);

                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(transformedBitmapFrame);
                imageStream = new System.IO.MemoryStream();
                encoder.Save(imageStream);
                imageStream.Position = 0;
            }

            return imageStream;
        }

        public static bool ApplyXmpRecipe(XDocument xdocument, ref BitmapSource bitmapSource, TransformGroup transformGroup) 
        {
            IEnumerable<XNode> recipeNodes = null;
            bool changed = false;

            if (xdocument != null)
            {
                var xmpQuery = from descriptions in xdocument.Descendants(xNameDescription)
                               where descriptions.Attribute(xNameIles) != null
                               select descriptions;

                recipeNodes = xmpQuery.First().Nodes();
            }

            if (recipeNodes != null)
            {
                //Process simple transformations
                foreach (XElement element in recipeNodes.OfType<XElement>().Where(x => x.Name.Equals(xNameIlesRotate)))
                {
                    XElement currentRecipe = element.Descendants().First();

                    XElement recipeEnabledElement = getXElementByXName(currentRecipe, xNameIlesRotate_RecipeEnabled);
                    XElement friendlyNameElement = getXElementByXName(currentRecipe, xNameIlesRotate_FriendlyName);
                    XElement opacityElement = getXElementByXName(currentRecipe, xNameIlesRotate_Opacity);
                    XElement blendModeElement = getXElementByXName(currentRecipe, xNameIlesRotate_BlendMode);
                    XElement angleElement = getXElementByXName(currentRecipe, xNameIlesRotate_Angle);
                    XElement cropElement = getXElementByXName(currentRecipe, xNameIlesRotate_Crop);
                    XElement backgroundColorElement = getXElementByXName(currentRecipe, xNameIlesRotate_BackgroundColor);

                    if (recipeEnabledElement.Value.Equals("1") & angleElement != null)
                    {
                        double angle = Double.Parse(angleElement.Value);

                        //Only 90, 180 and 270 degrees are supported
                        if (angle == 90 | angle == 180 | angle == 270)
                        {
                            RotateTransform rotateTransform = new RotateTransform();
                            rotateTransform.Angle = angle;
                            transformGroup.Children.Add(rotateTransform);
                        }
                    }
                }

                foreach (XElement element in recipeNodes.OfType<XElement>().Where(x => x.Name.Equals(xNameIlesResizeFixed)))
                {
                    XElement currentRecipe = element.Descendants().First();

                    XElement recipeEnabledElement = getXElementByXName(currentRecipe, xNameIlesResizeFixed_RecipeEnabled);
                    XElement friendlyNameElement = getXElementByXName(currentRecipe, xNameIlesResizeFixed_FriendlyName);
                    XElement guidElement = getXElementByXName(currentRecipe, xNameIlesResizeFixed_GUID);
                    XElement resizeFixedEnabledElement = getXElementByXName(currentRecipe, xNameIlesResizeFixed_Enabled);
                    XElement widthElement = getXElementByXName(currentRecipe, xNameIlesResizeFixed_Width);
                    XElement heightElement = getXElementByXName(currentRecipe, xNameIlesResizeFixed_Height);
                    XElement widthDisplayUnitElement = getXElementByXName(currentRecipe, xNameIlesResizeFixed_WidthDisplayUnit);
                    XElement heightDisplayUnitElement = getXElementByXName(currentRecipe, xNameIlesResizeFixed_HeightDisplayUnit);
                    XElement adjustResolutionElement = getXElementByXName(currentRecipe, xNameIlesResizeFixed_AdjustResolution);
                    XElement resolutionElement = getXElementByXName(currentRecipe, xNameIlesResizeFixed_Resolution);
                    XElement resolutionDisplayUnitElement = getXElementByXName(currentRecipe, xNameIlesResizeFixed_ResolutionDisplayUnit);
                    XElement proportionalElement = getXElementByXName(currentRecipe, xNameIlesResizeFixed_Proportional);
                    XElement proportionalSettingElement = getXElementByXName(currentRecipe, xNameIlesResizeFixed_ProportionalSetting);
                    XElement enlargeSmallImagesElement = getXElementByXName(currentRecipe, xNameIlesResizeFixed_EnlargeSmallImages);
                    XElement maintainRatioElement = getXElementByXName(currentRecipe, xNameIlesResizeFixed_MaintainRatio);
                    XElement interpolatedElement = getXElementByXName(currentRecipe, xNameIlesResizeFixed_Interpolated);
                    XElement filterElement = getXElementByXName(currentRecipe, xNameIlesResizeFixed_Filter);

                    if (recipeEnabledElement.Value.Equals("1"))
                    {
                        int intWidth = Int32.Parse(widthElement.Value);
                        int intHeight = Int32.Parse(heightElement.Value);
                        double scaleX;
                        double scaleY;

                        if (bitmapSource.PixelWidth > bitmapSource.PixelHeight)
                        {
                            scaleX = (double)intWidth / (double)bitmapSource.PixelWidth;
                            scaleY = (double)intHeight / (double)bitmapSource.PixelHeight;
                        }
                        else
                        {
                            scaleX = (double)intHeight / (double)bitmapSource.PixelHeight;
                            scaleY = (double)intWidth / (double)bitmapSource.PixelWidth;
                        }

                        ScaleTransform scaleTransform = new ScaleTransform(scaleX, scaleY, 0, 0);
                        transformGroup.Children.Add(scaleTransform);
                    }
                }

                foreach (XElement element in recipeNodes.OfType<XElement>().Where(x => x.Name.Equals(xNameIlesStraighten)))
                {
                    XElement currentRecipe = element.Descendants().First();

                    XElement recipeEnabledElement = getXElementByXName(currentRecipe, xNameIlesStraighten_RecipeEnabled);
                    XElement friendlyNameElement = getXElementByXName(currentRecipe, xNameIlesStraighten_FriendlyName);
                    XElement opacityElement = getXElementByXName(currentRecipe, xNameIlesStraighten_Opacity);
                    XElement blendModeElement = getXElementByXName(currentRecipe, xNameIlesStraighten_BlendMode);
                    XElement angleElement = getXElementByXName(currentRecipe, xNameIlesStraighten_Angle);
                    XElement cropElement = getXElementByXName(currentRecipe, xNameIlesStraighten_Crop);
                    XElement BackgroundColorElement = getXElementByXName(currentRecipe, xNameIlesStraighten_BackgroundColor);

                    if (recipeEnabledElement.Value.Equals("1") & angleElement != null)
                    {
                        double angle = Double.Parse(angleElement.Value);

                        //Only 90, 180 and 270 degrees are supported
                        if (angle == 90 | angle == 180 | angle == 270)
                        {
                            RotateTransform rotateTransform = new RotateTransform();
                            rotateTransform.Angle = angle;
                            transformGroup.Children.Add(rotateTransform);
                        }
                    }
                }

                double flipScaleX = 1;
                double flipScaleY = 1;
                foreach (XElement element in recipeNodes.OfType<XElement>().Where(x => x.Name.Equals(xNameIlesFlip)))
                {
                    XElement currentRecipe = element.Descendants().First();

                    XElement recipeEnabledElement = getXElementByXName(currentRecipe, xNameIlesFlip_RecipeEnabled);
                    XElement friendlyNameElement = getXElementByXName(currentRecipe, xNameIlesFlip_FriendlyName);
                    XElement opacityElement = getXElementByXName(currentRecipe, xNameIlesFlip_Opacity);
                    XElement blendModeElement = getXElementByXName(currentRecipe, xNameIlesFlip_BlendMode);
                    XElement flipVerticalElement = getXElementByXName(currentRecipe, xNameIlesFlip_FlipVertical);
                    XElement flipHorizontalElement = getXElementByXName(currentRecipe, xNameIlesFlip_FlipHorizontal);

                    if (recipeEnabledElement.Value.Equals("1"))
                    {
                        flipScaleX = (flipVerticalElement != null && flipVerticalElement.Value.Equals("1")) ? -1 : flipScaleX;
                        flipScaleY = (flipHorizontalElement != null && flipHorizontalElement.Value.Equals("1")) ? -1 : flipScaleY;
                    }
                }

                if (flipScaleX != 1 | flipScaleY != 1)
                {
                    ScaleTransform scaleTransform = new ScaleTransform(flipScaleX, flipScaleY, 0, 0);
                    transformGroup.Children.Add(scaleTransform);
                }

                foreach (XElement element in recipeNodes.OfType<XElement>().Where(x => x.Name.Equals(xNameIlesFrame)))
                {
                    XElement currentRecipe = element.Descendants().First();

                    XElement recipeEnabledElement = getXElementByXName(currentRecipe, xNameIlesFrame_RecipeEnabled);
                    XElement friendlyNameElement = getXElementByXName(currentRecipe, xNameIlesFrame_FriendlyName);
                    XElement opacityElement = getXElementByXName(currentRecipe, xNameIlesFrame_Opacity);
                    XElement blendModeElement = getXElementByXName(currentRecipe, xNameIlesFrame_BlendMode);
                    XElement frameStreamElement = getXElementByXName(currentRecipe, xNameIlesFrame_FrameStream);
                }

                foreach (XElement element in recipeNodes.OfType<XElement>().Where(x => x.Name.Equals(xNameIlesTitle)))
                {
                    XElement currentRecipe = element.Descendants().First();

                    XElement recipeEnabledElement = getXElementByXName(currentRecipe, xNameIlesTitle_RecipeEnabled);
                    XElement friendlyNameElement = getXElementByXName(currentRecipe, xNameIlesTitle_FriendlyName);
                    XElement titleStreamElement = getXElementByXName(currentRecipe, xNameIlesTitle_TitleStream);
                }

                foreach (XElement element in recipeNodes.OfType<XElement>().Where(x => x.Name.Equals(xNameIlesWatermarks)))
                {
                    XElement currentRecipe = element.Descendants().First();

                    XElement recipeEnabledElement = getXElementByXName(currentRecipe, xNameIlesWatermarks_RecipeEnabled);
                    XElement friendlyNameElement = getXElementByXName(currentRecipe, xNameIlesWatermarks_FriendlyName);
                    XElement opacityElement = getXElementByXName(currentRecipe, xNameIlesWatermarks_Opacity);
                    XElement blendModeElement = getXElementByXName(currentRecipe, xNameIlesWatermarks_BlendMode);
                    XElement watermarksStreamElement = getXElementByXName(currentRecipe, xNameIlesWatermarks_WatermarksStream);
                }
            }

            var scaleTransformQuery = from scale in transformGroup.Children.OfType<ScaleTransform>()
                                      where scale.ScaleX > 0 & scale.ScaleY > 0
                                      orderby scale.ScaleX, scale.ScaleY
                                      select scale;
                
            //check if multiple ScaleTransform transformations are added and only take the smallest
            if (scaleTransformQuery.Count() > 1)
            {
                ScaleTransform smallestScaleTransform = scaleTransformQuery.First();

                foreach (ScaleTransform existingScaleTransform in transformGroup.Children.OfType<ScaleTransform>().ToList())
                    transformGroup.Children.Remove(existingScaleTransform);

                transformGroup.Children.Add(smallestScaleTransform);
            }

            if (transformGroup != null && transformGroup.Children.Count > 0)
            {
                TransformedBitmap tb = new TransformedBitmap();
                tb.BeginInit();
                tb.Source = bitmapSource;
                tb.Transform = transformGroup;
                tb.EndInit();

                bitmapSource = tb;
                changed = true;
            }
                        
            if (recipeNodes != null && xdocument != null)
            {
                //Process complex transformations
                foreach (XElement element in recipeNodes)
                {
                    XElement currentRecipe = element.Descendants().First();

                    if (element.Name.Equals(xNameIlesCrop))
                    {
                        XElement recipeEnabledElement = getXElementByXName(currentRecipe, xNameIlesCrop_RecipeEnabled);
                        XElement friendlyNameElement = getXElementByXName(currentRecipe, xNameIlesCrop_FriendlyName);
                        XElement opacityElement = getXElementByXName(currentRecipe, xNameIlesCrop_Opacity);
                        XElement blendModeElement = getXElementByXName(currentRecipe, xNameIlesCrop_BlendMode);
                        XElement leftElement = getXElementByXName(currentRecipe, xNameIlesCrop_Left);
                        XElement topElement = getXElementByXName(currentRecipe, xNameIlesCrop_Top);
                        XElement rightElement = getXElementByXName(currentRecipe, xNameIlesCrop_Right);
                        XElement bottomElement = getXElementByXName(currentRecipe, xNameIlesCrop_Bottom);

                        if (recipeEnabledElement.Value.Equals("1"))
                        {
                            double left = double.Parse(leftElement.Value);
                            double top = double.Parse(topElement.Value);
                            double right = double.Parse(rightElement.Value);
                            double bottom = double.Parse(bottomElement.Value);

                            int intLeftPixel = Convert.ToInt32(bitmapSource.PixelWidth * left);
                            int intTopPixel = Convert.ToInt32(bitmapSource.PixelHeight * top);
                            int intRightPixel = Convert.ToInt32(bitmapSource.PixelWidth * right);
                            int intBottomPixel = Convert.ToInt32(bitmapSource.PixelHeight * bottom);
                            int intWidth = intRightPixel - intLeftPixel;
                            int intHeight = intBottomPixel - intTopPixel;

                            bitmapSource = new CroppedBitmap(bitmapSource, new Int32Rect(intLeftPixel, intTopPixel, intWidth, intHeight));
                            changed = true;
                        }
                    }
                } 
            }

            return changed;
        }

        private static XElement getXElementByXName(XElement xelement, XName xname)
        {
            return xelement.Nodes().OfType<XElement>().SingleOrDefault(x => x.Name.LocalName.Equals(xname.LocalName));
        }
    }
}
