using IDBrowserServiceCore.Code.XmpRecipe.Actions;
using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace IDBrowserServiceCore.Code.XmpRecipe
{
    public class XmpRecipeHelper
    {
        private static XName xNameDescription = XName.Get("Description", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");

        //iles namespace
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

        public static XmpRecipeContainer ParseXmlRecepie(XDocument xdocument)
        {
            XmpRecipeContainer xmpRecipeContainer = new XmpRecipeContainer();
            IEnumerable<XNode> recipeNodes = null;

            if (xdocument != null)
            {
                var xmpQuery = from descriptions in xdocument.Descendants(xNameDescription)
                               where descriptions.Attribute(xNameIles) != null
                               select descriptions;

                recipeNodes = xmpQuery.First().Nodes();
            }

            if (recipeNodes != null)
            {
                foreach (XElement element in recipeNodes.OfType<XElement>())
                {
                    XElement currentRecipe = element.Descendants().First();

                    if (element.Name.Equals(xNameIlesRotate))
                    {                        
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

                            XmpRotate xmpRotate = new XmpRotate
                            {
                                Angle = angle
                            };
                            xmpRecipeContainer.Actions.Add(xmpRotate);
                        }
                    }
                    else if (element.Name.Equals(xNameIlesResizeFixed))
                    {
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

                            XmpResize xmpResize = new XmpResize
                            {
                                Width = intWidth,
                                Height = intHeight
                            };

                            xmpRecipeContainer.Actions.Add(xmpResize);
                        }
                    }
                    else if (element.Name.Equals(xNameIlesStraighten))
                    {
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
                            bool crop = cropElement != null && cropElement.Value.Equals("1") ? true : false;

                            XmpStraighten xmpStraighten = new XmpStraighten
                            {
                                Angle = angle,
                                Crop = crop
                            };
                            xmpRecipeContainer.Actions.Add(xmpStraighten);
                        }
                    }
                    else if (element.Name.Equals(xNameIlesFlip))
                    {
                        XElement recipeEnabledElement = getXElementByXName(currentRecipe, xNameIlesFlip_RecipeEnabled);
                        XElement friendlyNameElement = getXElementByXName(currentRecipe, xNameIlesFlip_FriendlyName);
                        XElement opacityElement = getXElementByXName(currentRecipe, xNameIlesFlip_Opacity);
                        XElement blendModeElement = getXElementByXName(currentRecipe, xNameIlesFlip_BlendMode);
                        XElement flipVerticalElement = getXElementByXName(currentRecipe, xNameIlesFlip_FlipVertical);
                        XElement flipHorizontalElement = getXElementByXName(currentRecipe, xNameIlesFlip_FlipHorizontal);

                        XmpFlip xmpFlip = new XmpFlip
                        {
                            FlipVertical = flipVerticalElement != null && flipVerticalElement.Value.Equals("1") ? true : false,
                            FlipHorizontal = flipHorizontalElement != null && flipHorizontalElement.Value.Equals("1") ? true : false,
                        };
                        xmpRecipeContainer.Actions.Add(xmpFlip);
                    }
                    else if (element.Name.Equals(xNameIlesFrame))
                    {
                        XElement recipeEnabledElement = getXElementByXName(currentRecipe, xNameIlesFrame_RecipeEnabled);
                        XElement friendlyNameElement = getXElementByXName(currentRecipe, xNameIlesFrame_FriendlyName);
                        XElement opacityElement = getXElementByXName(currentRecipe, xNameIlesFrame_Opacity);
                        XElement blendModeElement = getXElementByXName(currentRecipe, xNameIlesFrame_BlendMode);
                        XElement frameStreamElement = getXElementByXName(currentRecipe, xNameIlesFrame_FrameStream);

                        XmpWatermark xmpWatermark = new XmpWatermark
                        {
                            Watermark = Convert.FromBase64String(frameStreamElement.Value)
                        };
                        xmpRecipeContainer.Actions.Add(xmpWatermark);
                        //File.WriteAllBytes("d:\\Frame.png", xmpWatermark.Watermark);
                    }
                    else if (element.Name.Equals(xNameIlesTitle))
                    {
                        XElement recipeEnabledElement = getXElementByXName(currentRecipe, xNameIlesTitle_RecipeEnabled);
                        XElement friendlyNameElement = getXElementByXName(currentRecipe, xNameIlesTitle_FriendlyName);
                        XElement titleStreamElement = getXElementByXName(currentRecipe, xNameIlesTitle_TitleStream);

                        XmpWatermark xmpWatermark = new XmpWatermark
                        {
                            Watermark = Convert.FromBase64String(titleStreamElement.Value)
                        };
                        xmpRecipeContainer.Actions.Add(xmpWatermark);
                        //File.WriteAllBytes("d:\\Title.png", xmpWatermark.Watermark);
                    }
                    else if (element.Name.Equals(xNameIlesWatermarks))
                    {
                        XElement recipeEnabledElement = getXElementByXName(currentRecipe, xNameIlesWatermarks_RecipeEnabled);
                        XElement friendlyNameElement = getXElementByXName(currentRecipe, xNameIlesWatermarks_FriendlyName);
                        XElement opacityElement = getXElementByXName(currentRecipe, xNameIlesWatermarks_Opacity);
                        XElement blendModeElement = getXElementByXName(currentRecipe, xNameIlesWatermarks_BlendMode);
                        XElement watermarksStreamElement = getXElementByXName(currentRecipe, xNameIlesWatermarks_WatermarksStream);

                        XmpWatermark xmpWatermark = new XmpWatermark
                        {
                            Watermark = Convert.FromBase64String(watermarksStreamElement.Value)
                        };
                        xmpRecipeContainer.Actions.Add(xmpWatermark);
                        //File.WriteAllBytes("d:\\Watermark.png", xmpWatermark.Watermark);
                    }
                    else if (element.Name.Equals(xNameIlesCrop))
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
                            XmpCrop xmpCrop = new XmpCrop()
                            {
                                Left = double.Parse(leftElement.Value),
                                Top = double.Parse(topElement.Value),
                                Right = double.Parse(rightElement.Value),
                                Bottom = double.Parse(bottomElement.Value)
                            };

                            xmpRecipeContainer.Actions.Add(xmpCrop);
                        }
                    }
                }
            }

            return xmpRecipeContainer;
        }

        public static void ApplyXmpRecipe(XmpRecipeContainer xmpRecipeContainer, MagickImage image) 
        {
            foreach (IXmpRecipeAction action in xmpRecipeContainer.Actions)
            {
                if (action.GetType() == typeof(XmpRotate))
                {
                    XmpRotate xmpRotate = (XmpRotate)action;
                    image.VirtualPixelMethod = VirtualPixelMethod.Black;
                    image.Rotate(xmpRotate.Angle);
                }
                else if (action.GetType() == typeof(XmpResize))
                {
                    XmpResize xmpResize = (XmpResize)action;

                    if (image.Width > xmpResize.Width && image.Height > xmpResize.Height)
                        image.Resize(xmpResize.Width, xmpResize.Height);
                }
                else if (action.GetType() == typeof(XmpCrop))
                {
                    XmpCrop xmpCrop = (XmpCrop)action;

                    int intLeftPixel = Convert.ToInt32(image.Width * xmpCrop.Left);
                    int intTopPixel = Convert.ToInt32(image.Height * xmpCrop.Top);
                    int intRightPixel = Convert.ToInt32(image.Width * xmpCrop.Right);
                    int intBottomPixel = Convert.ToInt32(image.Height * xmpCrop.Bottom);
                    int intWidth = intRightPixel - intLeftPixel;
                    int intHeight = intBottomPixel - intTopPixel;

                    MagickGeometry magickGeometry = new MagickGeometry(intLeftPixel, intTopPixel, intWidth, intHeight);
                    image.Crop(magickGeometry);
                }
                else if (action.GetType() == typeof(XmpFlip))
                {
                    XmpFlip xmpFlip = (XmpFlip)action;

                    if (xmpFlip.FlipVertical)
                        image.Flop();

                    if (xmpFlip.FlipHorizontal)
                        image.Flip();
                }
                else if (action.GetType() == typeof(XmpStraighten))
                {
                    XmpStraighten xmpStraighten = (XmpStraighten)action;
                    image.VirtualPixelMethod = VirtualPixelMethod.Black;

                    if (xmpStraighten.Crop)
                    {
                        //http://www.imagemagick.org/Usage/distorts/#rotate_methods

                        int w = image.Width;
                        int h = image.Height;
                        double aa = xmpStraighten.Angle * Math.PI / 180;
                        double srt = (w * Math.Abs(Math.Sin(aa)) + h * Math.Abs(Math.Cos(aa))) / Math.Min(w,h);
                        
                        image.Distort(DistortMethod.ScaleRotateTranslate, srt, xmpStraighten.Angle);
                    }
                    else
                    {
                        image.Rotate(xmpStraighten.Angle);
                    }
                }
            }
        }

        private static XElement getXElementByXName(XElement xelement, XName xname)
        {
            return xelement.Nodes().OfType<XElement>().SingleOrDefault(x => x.Name.LocalName.Equals(xname.LocalName));
        }
    }
}
