using System;
using System.Drawing;

namespace SpringCard.LibCs.Windows.Controls.ColorWheel
{
    public static class ColorHelper
    {
        public static RGB HSVtoRGB(byte hue, byte saturation, byte value)
        {
            return HSVtoRGB(new HSV(hue, saturation, value));
        }

        public static Color HSVtoColor(HSV hsv)
        {
            RGB rgb = HSVtoRGB(hsv);
            return Color.FromArgb(rgb.Red, rgb.Green, rgb.Blue);
        }

        public static Color HSVtoColor(byte hue, byte saturation, byte value)
        {
            return HSVtoColor(new HSV(hue, saturation, value));
        }

        public static RGB HSVtoRGB(HSV hsv)
        {
            // HSV contains values scaled as in the color wheel: that is, all from 0 to 255. 
            // for ( this code to work, HSV.Hue needs to be scaled from 0 to 360 (it//s the angle of the selected
            // point within the circle). HSV.Saturation and HSV.value must be scaled to be between 0 and 1.
            double h = 0D, s = 0D, v = 0D;
            double r = 0D, g = 0D, b = 0D;

            // Scale Hue to be between 0 and 360. Saturation and value scale to be between 0 and 1.
            h = ((double)hsv.Hue / 255 * 360) % 360;
            s = (double)hsv.Saturation / 255;
            v = (double)hsv.Value / 255;

            if (s == 0)
            {
                // If s is 0, all colors are the same. This is some flavor of gray.
                r = v;
                g = v;
                b = v;
            }
            else
            {
                double p = 0D, q = 0D, t = 0D;

                double fractionalSector;
                double sectorPos;
                int sectorNumber;

                // The color wheel consists of 6 sectors. Figure out which sector you//re in.
                sectorPos = h / 60;
                sectorNumber = (int)(Math.Floor(sectorPos));

                // get the fractional part of the sector. That is, how many degrees into the sector are you?
                fractionalSector = sectorPos - sectorNumber;

                // Calculate values for the three axes of the color. 
                p = v * (1 - s);
                q = v * (1 - (s * fractionalSector));
                t = v * (1 - (s * (1 - fractionalSector)));

                // Assign the fractional colors to r, g, and b based on the sector the angle is in.
                switch (sectorNumber)
                {
                    case 0:
                        r = v;
                        g = t;
                        b = p;
                        break;
                    case 1:
                        r = q;
                        g = v;
                        b = p;
                        break;
                    case 2:
                        r = p;
                        g = v;
                        b = t;
                        break;
                    case 3:
                        r = p;
                        g = q;
                        b = v;
                        break;
                    case 4:
                        r = t;
                        g = p;
                        b = v;
                        break;
                    case 5:
                        r = v;
                        g = p;
                        b = q;
                        break;
                }
            }
            // return an RGB structure, with values scaled to be between 0 and 255.
            return new RGB((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }

        public static HSV RGBtoHSV(RGB RGB)
        {
            // In this function, R, G, and B values must be scaled to be between 0 and 1.
            // HSV.Hue will be a value between 0 and 360, and HSV.Saturation and value are between 0 and 1.
            // The code must scale these to be between 0 and 255 for the purposes of this application.
            double min = 0D, max = 0D, delta = 0D;
            double h = 0D, s = 0D, v = 0D;

            double r = (double)RGB.Red / 255;
            double g = (double)RGB.Green / 255;
            double b = (double)RGB.Blue / 255;

            min = Math.Min(Math.Min(r, g), b);
            v = max = Math.Max(Math.Max(r, g), b);
            delta = max - min;

            if (max == 0 || delta == 0)
            {
                // R, G, and B must be 0, or all the same. In this case, S is 0, and H is undefined.
                // Using H = 0 is as good as any...
                s = 0;
                h = 0;
            }
            else
            {
                s = delta / max;
                if (r == max)
                {
                    // Between Yellow and Magenta
                    h = (g - b) / delta;
                }
                else if (g == max)
                {
                    // Between Cyan and Yellow
                    h = 2 + (b - r) / delta;
                }
                else
                {
                    // Between Magenta and Cyan
                    h = 4 + (r - g) / delta;
                }

            }
            // Scale h to be between 0 and 360. This may require adding 360, if the value
            // is negative.
            h *= 60;
            if (h < 0) h += 360;

            // Scale to the requirements of this application. All values are between 0 and 255.
            return new HSV((byte)(h / 360 * 255), (byte)(s * 255), (byte)(v * 255));
        }
    }
}