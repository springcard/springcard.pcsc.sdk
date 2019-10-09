using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpringCard.LibCs.Windows.Controls.ColorWheel
{
    public partial class ColorChooser : UserControl
    {
        private ChangeStyle changeType = ChangeStyle.None;
        private Point selectedPoint;

        private ColorWheel myColorWheel;
        private RGB RGB;
        private HSV HSV;
        private bool isInUpdate = false;

        public delegate void ColorChangedEventHandler(object sender, ColorChangedEventArgs e);
        public event ColorChangedEventHandler ColorChanged;

        public ColorChooser()
        {
            InitializeComponent();
        }

        private enum ChangeStyle
        {
            MouseMove,
            RGB,
            HSV,
            None
        }

        private void ColorChooser_Load(object sender, System.EventArgs e)
        {
            // Turn on double-buffering, so the form looks better. 
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);

            // These properties are set in design view, as well, but they have to be set to false in order for the Paint
            // event to be able to display their contents. Never hurts to make sure they're invisible.
            pnlSelectedColor.Visible = false;
            pnlBrightness.Visible = false;
            pnlColor.Visible = false;

            // Calculate the coordinates of the three required regions on the form.
            Rectangle SelectedColorRectangle = new Rectangle(pnlSelectedColor.Location, pnlSelectedColor.Size);
            Rectangle BrightnessRectangle = new Rectangle(pnlBrightness.Location, pnlBrightness.Size);
            Rectangle ColorRectangle = new Rectangle(pnlColor.Location, pnlColor.Size);

            // Create the new ColorWheel class, indicating the locations of the color wheel itself, the
            // brightness area, and the position of the selected color.
            myColorWheel = new ColorWheel(ColorRectangle, BrightnessRectangle, SelectedColorRectangle);
            myColorWheel.ColorChanged += new ColorWheel.ColorChangedEventHandler(this.myColorWheel_ColorChanged);

            // Set the RGB and HSV values of the NumericUpDown controls.
            SetRGB(RGB);
            SetHSV(HSV);
        }

        private void HandleMouse(object sender, MouseEventArgs e)
        {
            // If you have the left mouse button down, then update the selectedPoint value and force a repaint of the color wheel.
            if (e.Button == MouseButtons.Left)
            {
                changeType = ChangeStyle.MouseMove;
                selectedPoint = new Point(e.X, e.Y);
                this.Invalidate();
            }
        }

        private void ColorChooser_MouseUp(object sender, MouseEventArgs e)
        {
            myColorWheel.SetMouseUp();
            changeType = ChangeStyle.None;
        }

        private void HandleRGBChange(object sender, System.EventArgs e)
        {

            // If the R, G, or B values change, use this code to update the HSV values and invalidate
            // the color wheel (so it updates the pointers). Check the isInUpdate flag to avoid recursive events
            // when you update the NumericUpdownControls.
            if (!isInUpdate)
            {
                changeType = ChangeStyle.RGB;
                RGB = new RGB((byte)nudRed.Value, (byte)nudGreen.Value, (byte)nudBlue.Value);
                SetHSV(ColorHelper.RGBtoHSV(RGB));
                this.Invalidate();
            }
        }

        private void HandleHSVChange(object sender, EventArgs e)
        {
            // If the H, S, or V values change, use this code to update the RGB values and invalidate
            // the color wheel (so it updates the pointers). Check the isInUpdate flag to avoid recursive events
            // when you update the NumericUpdownControls.
            if (!isInUpdate)
            {
                changeType = ChangeStyle.HSV;
                HSV = new HSV((byte)(nudHue.Value), (byte)(nudSaturation.Value), (byte)(nudBrightness.Value));
                SetRGB(ColorHelper.HSVtoRGB(HSV));
                this.Invalidate();
            }
        }

        private void SetRGB(RGB RGB)
        {
            // Update the RGB values on the form, but don't trigger the ValueChanged event of the form. The isInUpdate
            // variable ensures that the event procedures exit without doing anything.
            isInUpdate = true;
            RefreshValue(nudRed, RGB.Red);
            RefreshValue(nudBlue, RGB.Blue);
            RefreshValue(nudGreen, RGB.Green);
            isInUpdate = false;
        }

        private void SetHSV(HSV HSV)
        {
            // Update the HSV values on the form, but don't trigger the ValueChanged event of the form. The isInUpdate
            // variable ensures that the event procedures exit without doing anything.
            isInUpdate = true;
            RefreshValue(nudHue, HSV.Hue);
            RefreshValue(nudSaturation, HSV.Saturation);
            RefreshValue(nudBrightness, HSV.Value);
            isInUpdate = false;
        }

        private void RefreshValue(NumericUpDown nud, int value)
        {
            // Update the value of the NumericUpDown control, if the value is different than the current value.
            // Refresh the control, causing an immediate repaint.
            if (nud.Value != value)
            {
                nud.Value = value;
                nud.Refresh();
            }
        }

        public Color Color
        {
            // Get or set the color to be displayed in the color wheel.
            get { return myColorWheel != null ? myColorWheel.Color: Color.Gray; }

            set
            {
                // Indicate the color change type. Either RGB or HSV will cause the color wheel to update the position of the pointer.
                changeType = ChangeStyle.RGB;
                RGB = new RGB(value.R, value.G, value.B);
                HSV = ColorHelper.RGBtoHSV(RGB);
            }
        }

        private void myColorWheel_ColorChanged(object sender, ColorChangedEventArgs e)
        {
            if (this.ColorChanged != null) this.ColorChanged(sender, e);

            SetRGB(e.RGB);
            SetHSV(e.HSV);
        }

        private void ColorChooser_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            // Depending on the circumstances, force a repaint of the color wheel passing different information.
            switch (changeType)
            {
                case ChangeStyle.HSV:
                    myColorWheel.Draw(e.Graphics, HSV);
                    break;
                case ChangeStyle.MouseMove:
                case ChangeStyle.None:
                    myColorWheel.Draw(e.Graphics, selectedPoint);
                    break;
                case ChangeStyle.RGB:
                    myColorWheel.Draw(e.Graphics, RGB);
                    break;
            }
        }
    }
}