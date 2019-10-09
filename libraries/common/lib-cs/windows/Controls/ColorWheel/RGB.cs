using System;

namespace SpringCard.LibCs.Windows.Controls.ColorWheel
{
    public struct RGB
    {
        private byte _red;
        private byte _green;
        private byte _blue;

        public RGB(byte red, byte green, byte blue)
        {
            this._red = red;
            this._green = green;
            this._blue = blue;
        }

        #region Properties

        public byte Red
        {
            get { return this._red; }
        }

        public byte Green
        {
            get { return this._green; }
        }

        public byte Blue
        {
            get { return this._blue; }
        }

        #endregion

        public override string ToString()
        {
            return String.Format("({0}, {1}, {2})", this._red, this._green, this._blue);
        }
    }
}