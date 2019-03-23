using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DigitalCardBinder.Mk3.ComponentHandlers
{
    class PanelHandler
    {
        private Panel p;
        private int X = 0;
        private int Y = 0;

        public Control getComponent(Control sender, int x, int y)
        {
            p = (Panel)sender.Parent;
            X = x;
            Y = y;
            return containsLocation(p);
        }

        private Control containsLocation(Panel p)
        {
            foreach(Control c in p.Controls)
            {
                //Console.WriteLine(p.Controls.Count);
                if (X >= c.Location.X 
                    && X <= c.Location.X + c.Width
                    && Y >= c.Location.Y
                    && Y <= c.Location.Y + c.Height
                    && c.Name != "GhostCard")
                {
                    return c;
                }
            }
            return null;
        }
    }
}
