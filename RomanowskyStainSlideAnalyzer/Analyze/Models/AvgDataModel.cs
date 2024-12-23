using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanowskyStainSlideAnalyzer.Analyze.Models
{
    public class AvgDataModel
    {
        public Color color { get; set; }
        public float a { get; set; }
        public float r { get; set; }
        public float g { get; set; }
        public float b { get; set; }
        public float brightness { get; set; }
        public float hue { get; set; }
        public float saturation { get; set; }
        public float total { get; set; }
        public float width { get; set; }
        public float height { get; set; }
        public float size { get; set; }

        public AvgDataModel(
            Color color = new Color(),
            float a = 255f,
            float r = 255f,
            float g = 0f,
            float b = 0f,
            float brightness = 255f,
            float hue = 255f,
            float saturation = 255f,
            float total = 255f,
            float width = 255f,
            float height = 255f,
            float size = 255f
        )
        {
            this.color = color;
            this.a = a;
            this.r = r;
            this.g = g;
            this.b = b;
            this.brightness = brightness;
            this.hue = hue;
            this.saturation = saturation;
            this.total = total;
            this.width = width;
            this.height = height;
            this.size = size;
        }

        public void calculateHSB(float? total = null)
        {
            brightness /= total == null ? this.total : (float) total;
            hue /= total == null ? this.total : (float)total;
            saturation /= total == null ? this.total : (float)total;

            brightness = brightness > 255 ? 255 : brightness;
            brightness = brightness < 0 ? 0 : brightness;

            hue = hue > 255 ? 255 : hue;
            hue = hue < 0 ? 0 : hue;

            saturation = saturation > 255 ? 255 : saturation;
            saturation = saturation < 0 ? 0 : saturation;
        }

        public void calculateSize(float? total = null)
        {
            width /= total == null ? this.total : (float)total;
            height /= total == null ? this.total : (float)total;
            size /= total == null ? this.total : (float)total;
        }

        public void setARGB()
        {
            color = Color.FromArgb(Convert.ToInt32(a), Convert.ToInt32(r), Convert.ToInt32(g), Convert.ToInt32(b));
        }
    }
}
