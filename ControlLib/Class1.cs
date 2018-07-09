using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;


namespace ControlLib
{
    public class TextBoxUser:TextBox
    {
        private const int WM_PAINT = 0x000F;

        private string backGroundText = "请输入手机号";

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
                    if (string.IsNullOrEmpty(Text))
                    {
                        SizeF size = g.MeasureString(backGroundText, Font);
                        //draw background text   
                        g.DrawString(backGroundText, Font, Brushes.LightGray, new PointF(0, (Height - size.Height-5) / 2));
                    }
                }
            }
        }
    }

    public class TextBoxPwd : TextBox
    {
        private const int WM_PAINT = 0x000F;

        private string backGroundText = "请输入密码";

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
