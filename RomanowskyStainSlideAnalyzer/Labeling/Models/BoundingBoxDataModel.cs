using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanowskyStainSlideAnalyzer.Labeling.Models
{
    public class BoundingBoxDataModel
    {
        public double X;
        public double Y;
        public double Width;
        public double Height;

        public BoundingBoxDataModel(double X, double Y, double Width, double Height)
        {
            this.X = X;
            this.Y = Y;
            this.Width = Width;
            this.Height = Height;
        }
    }
}
