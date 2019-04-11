using ImageMagick;

namespace IDBrowserServiceCore.Code
{
    public class XmpReceipe
    {
        public bool HasValues { get; private set; }

        private double? rotate;
        public double? Rotate
        {
            get
            {
                return rotate;
            }
            set
            {
                rotate = value;
                HasValues = rotate != null && rotate != 0;
            }
        }

        private MagickGeometry resize;
        public MagickGeometry Resize
        {
            get
            {
                return resize;
            }
            set
            {
                resize = value;
                HasValues = resize != null;
            }
        }

        private XmpCrop crop;
        public XmpCrop Crop
        {
            get
            {
                return crop;
            }
            set
            {
                crop = value;
                HasValues = crop != null;
            }
        }
    }
}
