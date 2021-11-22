using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BeatmapManager
{
    public class DoubleBufferedGridView : DataGridView
    {
        public DoubleBufferedGridView()
        {
            DoubleBuffered = true;
        }
    }
}
