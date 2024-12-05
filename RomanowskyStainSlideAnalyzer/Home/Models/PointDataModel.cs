using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanowskyStainSlideAnalyzer.Home.Models
{
    public class PointDataModel
    {
        public double X;
        public double Y;
        public ClassTypeModel ClassType;
        public Ellipse ellipse;

        public PointDataModel(double X, double Y, ClassTypeModel ClassType, Ellipse ellipse)
        {
            this.X = X;
            this.Y = Y;
            this.ClassType = ClassType;
            this.ellipse = ellipse;
        }
    }
}
