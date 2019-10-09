using System;
using System.Drawing;

namespace SpringCard.LibCs.Windows.Controls.ColorWheel
{
    public class ColorChangedEventArgs : EventArgs
    {
        private RGB mRGB;
        private HSV mHSV;
        private Color mColor;

        public ColorChangedEventArgs(RGB RGB, HSV HSV)
        {
            mRGB = RGB;
            mHSV = HSV;
        }

        public ColorChangedEventArgs(Color color)
        {
           mColor = color;
        }

        public RGB RGB
        {
            get { return mRGB; }
        }

        public HSV HSV
        {
            get { return mHSV; }
        }

        public Color Color
        {
            get { return mColor; }
        }
    }
}