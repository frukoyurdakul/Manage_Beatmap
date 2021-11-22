using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Manage_Beatmap
{
    public class FormHandlerPanel : Panel
    {
        private ActionableForm form;

        public void SetForm<T>(ActionableForm<T> form) where T: ActionableForm<T>
        {
            if (this.form != null)
            {
                Controls.Remove(this.form);
                this.form.Dispose();
            }

            this.form = form;

            // Arrange the form to show in panel
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
            form.MinimumSize = new System.Drawing.Size(0, form.MinimumSize.Height);
            form.Width = Width;

            Controls.Add(form);
            form.Show();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ResumeLayout(false);

        }
    }
}
