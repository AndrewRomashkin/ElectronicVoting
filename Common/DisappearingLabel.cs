using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Common
{
    public class DisappearingLabel : Label
    {
        Timer timer = new Timer();

        public DisappearingLabel()
        {
            Interval = 1000;
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Text = "";
            timer.Stop();
        }

        public int Interval
        {
            set
            {
                timer.Interval = value;
            }

            get
            {
                return timer.Interval;
            }
        }

        public void Show(string message)
        {
            Text = message;
            timer.Start();
        }
    }
}
