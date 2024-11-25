
using ImageMagick;

namespace IDBrowserServiceCore.Code.XmpRecipe.Actions
{
    public class XmpResize : IXmpRecipeAction
    {
        public uint Width { get; set; }
        public uint Height { get; set; }
    }
}
