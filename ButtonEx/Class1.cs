using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ButtonEx
{
    public class ButtonEx:Button
    {
        private const int WM_PAINT = 0x000F;

        private string backGroundText = "";

        [Description("BackGround Text")]
        public string BackGroundText
        {
            get { return backGroundText; }
            set { backGroundText = value; }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_PAINT)
            {
                using (Graphics g = CreateGraphics())
                {
                    if (string.IsNullOrEmpty(Text) && !Focused)
                    {
                        SizeF size = g.MeasureString(backGroundText, Font);
                        //draw background text   
                        g.DrawString(backGroundText, Font, Brushes.LightGray, new PointF(0, (Height - size.Height) / 2));
                    }
                }
            }
        }
    }
}
