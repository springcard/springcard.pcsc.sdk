using System;

namespace SpringCard.LibCs.Windows.Controls.ColorWheel
{
    public struct HSV
    {
        private byte _hue;
        private byte _saturation;
        private byte _value;

        public HSV(byte hue, byte saturation, byte value)
        {
            this._hue = hue;
            this._saturation = saturation;
            this._value = value;
        }

        #region Properties

        public byte Hue
        {
            get { return this._hue; }
            internal set { this._hue = value; }
        }

        public byte Saturation
        {
            get { return this._saturation; }
            internal set { this._saturation = value; }
        }

        public byte Value
        {
            get { return this._value; }
            internal set { this._value = value; }
        }

        #endregion

        public override string ToString()
        {
            return String.Format("({0}, {1}, {2})", this._hue, this._saturation, this._value);
        }
    }
}