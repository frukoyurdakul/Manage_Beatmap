using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BeatmapManager
{
    public class FormHandlerPanel : Panel
    {
        private ActionableForm form;
        private int originalFormHeight;

        public FormHandlerPanel()
        {
            InitializeComponent();
        }

        public void SetForm<T>(ActionableForm<T> form) where T: ActionableForm<T>
        {
            if (this.form != null)
            {
                Controls.Remove(this.form);
                this.form.Dispose();
            }

            this.form = form;
            originalFormHeight = form.Height;

            // Arrange the form to show in panel
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Padding = new Padding(0, 0, 0, 0);
            form.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            Width = form.Width + 30;
            Height = form.Height -10;
            form.Location = new System.Drawing.Point(15, form.Location.Y - 5);

            Controls.Add(form);
            form.Show();
        }

        private AnchorStyles GetStyles()
        {
            if (form != null)
            {
                if (Height > form.Height)
                    return AnchorStyles.Left | AnchorStyles.Right;
                else
                    return AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            }
            else
                return AnchorStyles.None;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            HorizontalScroll.Maximum = 0;
            AutoScroll = false;
            VerticalScroll.Visible = false;
            AutoScroll = true;

            // 
            // FormHandlerPanel
            // 
            this.SizeChanged += new System.EventHandler(this.FormHandlerPanel_SizeChanged);
            this.ResumeLayout(false);
        }

        private void FormHandlerPanel_SizeChanged(object sender, EventArgs e)
        {
            if (form != null)
            {
                int y = Math.Max((Height - form.Height) / 2, 0);
                if (form.Location.Y != y)
                    form.Location = new System.Drawing.Point(form.Location.X, y);
                if (form.Size.Height != originalFormHeight)
                    form.Size = new System.Drawing.Size(form.Size.Width, originalFormHeight);
                if (Size.Height > form.Size.Height)
                    Size = new System.Drawing.Size(Size.Width, form.Size.Height);
                if (Parent != null)
                {
                    int locationY = Math.Max((Parent.Height - Height) / 2, 0);
                    if (Location.Y != locationY)
                        Location = new System.Drawing.Point(Location.X, locationY);
                }
            }
        }
    }
}
