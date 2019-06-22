using System.Collections.Generic;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Puzzle.HelperClasses
{
    public class thumbTag
    {
        public ImageBrush ib { get; set; }
        public int rotationAngle { get; set; }
        public int unionNr { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public List<Thumb> listName { get; set; }
    }
}
