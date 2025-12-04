using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Battleship
{
    public class GradientPanel : Panel
    {
        private Color _colorTop = Color.FromArgb(25, 25, 112);
        private Color _colorBottom = Color.FromArgb(70, 130, 180);
        private LinearGradientMode _gradientMode = LinearGradientMode.Vertical;

        public Color ColorTop
        {
            get => _colorTop;
            set { _colorTop = value; Invalidate(); }
        }

        public Color ColorBottom
        {
            get => _colorBottom;
            set { _colorBottom = value; Invalidate(); }
        }

        public LinearGradientMode GradientMode
        {
            get => _gradientMode;
            set { _gradientMode = value; Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(
                ClientRectangle, _colorTop, _colorBottom, _gradientMode))
            {
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }
            base.OnPaint(e);
        }
    }
}