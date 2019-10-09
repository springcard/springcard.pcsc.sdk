using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SpringCard.LibCs.Windows.Controls.ColorWheel
{
    //http://www.vbfrance.com/codes/CONTROLES-COLOR-PICKER-WHEEL-COLOR-PICKER-SCREEN-COLOR_48207.aspx
    public partial class ScreenColorPicker : UserControl
    {
        private Bitmap m_capture;
        private Color m_selectedColor;
        private int m_zoom = 3;
        private bool bCapturing = false;

        /// <summary> 
        /// Evénement lors de changement de couleur. 
        /// </summary> 
        /// <remarks></remarks> 
        public event EventHandler<ColorChangedEventArgs> SelectedColorChanged;


        public ScreenColorPicker()
        {
            this.InitializeComponent();
        }

        /// <summary> 
        /// Obtient le centre d'un rectangle 
        /// </summary> 
        /// <param name="rect"></param> 
        /// <returns>Un point <see cref="PointF"></see></returns> 
        private PointF GetCenterPoint(RectangleF rect)
        {
            PointF pf = rect.Location;
            pf.Y += rect.Height / 2;
            pf.X += rect.Width / 2;
            return pf;
        }

        /// <summary> 
        /// Convertit un <see cref="Rectangle"></see> vers un <see cref="RectangleF"></see>. 
        /// </summary> 
        /// <param name="rect">Un <see cref="Rectangle"></see>.</param> 
        /// <returns>Un <see cref="RectangleF"></see>.</returns> 
        private RectangleF ConvertRectangleToFloating(Rectangle rect)
        {
            RectangleF rectF = new RectangleF();
            rectF.Y = Convert.ToSingle(rect.Y);
            rectF.X = Convert.ToSingle(rect.X);
            rectF.Width = Convert.ToSingle(rect.Width);
            rectF.Height = Convert.ToSingle(rect.Height);
            return rectF;
        }

        /// <summary> 
        /// Capture d'une zone entourant la souris. 
        /// </summary> 
        /// <remarks></remarks> 
        private void GetCapture()
        {
            Point MousePos = Control.MousePosition;

            //Définit la zone à capturer. 
            MousePos.X -= m_capture.Width / 2;
            MousePos.Y -= m_capture.Height / 2;

            using (Graphics g = Graphics.FromImage(m_capture))
            {
                //Capture d'écran de la taille de notre UC. 
                g.CopyFromScreen(MousePos, new Point(0, 0), m_capture.Size);

                //Force le UC à redessiner tout sont contenu. 
                this.Refresh();

                //Récupération de la partie qui entoure la souris. 
                PointF center = GetCenterPoint(new RectangleF(0, 0, m_capture.Size.Width, m_capture.Size.Height));

                //Récupération de la couleur pointé par la souris. 
                m_selectedColor = m_capture.GetPixel(Convert.ToInt32(Math.Round(center.X)), Convert.ToInt32(Math.Round(center.Y)));

                if (SelectedColorChanged != null) SelectedColorChanged(this, new ColorChangedEventArgs(m_selectedColor));
            }
        }

        /// <summary> 
        /// Mode capture. 
        /// </summary> 
        private void ScreenColorPicker_MouseDown(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                //Change le curseur pour indiquer le mode capture. 
                Cursor = Cursors.Cross;
                bCapturing = true;
                this.Invalidate();
            }
        }

        private void ScreenColorPicker_MouseMove(object sender, MouseEventArgs e)
        {
            //Prend des captures suivant la position de la souris. 
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                //Déclenche la récupération d'une capture + couleur du pixel poité par la souris. 
                GetCapture();
            }
        }

        /// <summary> 
        /// Arrêt capture au relachement du bouton de la souris. 
        /// </summary> 
        private void ScreenColorPicker_MouseUp(object sender, MouseEventArgs e)
        {
            //Redéfinit le curseur par défaut (flèche). 
            Cursor = Cursors.Arrow;
            bCapturing = false;
            this.Invalidate();
        }

        /// <summary> 
        /// Dessine la capture dans la zone cliente de notre UC. 
        /// </summary> 
        private void ScreenColorPicker_Paint(object sender, PaintEventArgs e)
        {
            Rectangle clientRect = this.ClientRectangle;

            if ((m_capture != null))
            {
                //Définit le mode d'interpolation voulu sur l'objet Graphics. 
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;

                //Créér le rectangle pour recevoir la capture (avec prise en charge d'un zoom). 
                RectangleF rectF = new RectangleF();
                rectF.Width = m_capture.Size.Width * m_zoom;
                rectF.Height = m_capture.Size.Height * m_zoom;
                rectF.X = 0;
                rectF.Y = 0;

                //Transfert la capture. 
                e.Graphics.DrawImage(m_capture, rectF);

                //Si on est en mode capture. 
                if (bCapturing)
                {
                    //Obtient le centre du rectangle. 
                    PointF centerPoint = GetCenterPoint(rectF);

                    //Créér le rectangle en se basant sur le point centrale. 
                    Rectangle centerRect = new Rectangle(new Point(Convert.ToInt32(centerPoint.X), Convert.ToInt32(centerPoint.Y)), new Size(0, 0));
                    centerRect.X -= (m_zoom / 2 - 1);
                    centerRect.Y -= (m_zoom / 2 - 1);
                    centerRect.Width = m_zoom;
                    centerRect.Height = m_zoom;

                    //Dessine le rectangle. 
                    e.Graphics.DrawRectangle(Pens.Black, centerRect);
                }
            }

            //Maintenant on dessine le sélecteur (carré noir). 
            Pen pen = new Pen(BackColor, 3);
            clientRect.Inflate(-1, -1);
            e.Graphics.DrawRectangle(pen, clientRect);
        }

        private void ScreenColorPicker_Resize(object sender, EventArgs e)
        {
            //Libère ressources utilisés pour le bitmap de capture. Si il existe déjà pour le recréer par la suite. 
            if (m_capture != null) m_capture.Dispose();
            RectangleF rectF = ConvertRectangleToFloating(this.ClientRectangle);
            int iHeight = Convert.ToInt32(Math.Floor(rectF.Height / m_zoom));
            int iWidth = Convert.ToInt32(Math.Floor(rectF.Width / m_zoom));

            m_capture = new Bitmap(iWidth, iHeight);
        }
    }
}