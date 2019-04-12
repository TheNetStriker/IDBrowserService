using ImageMagick;
using System.Collections.Generic;

namespace IDBrowserServiceCore.Code.XmpRecipe
{
    public class XmpRecipeContainer
    {
        public bool HasValues
        {
            get
            {
                return Actions.Count > 0;
            }
        }

        public List<IXmpRecipeAction> Actions { get; set; }

        public XmpRecipeContainer()
        {
            Actions = new List<IXmpRecipeAction>();
        }
    }
}
