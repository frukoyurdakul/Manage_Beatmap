using Manage_Beatmap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BeatmapManager
{
    public class ActionableForm<T>: ActionableForm where T: ActionableForm<T>
    {
        private readonly Action<T> formAction;
        private bool willUseLoading = false;
        private Form parent;

        public ActionableForm() : base()
        {
            formAction = null;
        }

        public ActionableForm(Action<T> action) : base()
        {
            formAction = action;
        }

        public virtual T UseLoading(Form parent)
        {
            this.parent = parent;
            willUseLoading = true;
            return GetThis();
        }

        protected virtual void InvokeAction()
        {
            LoadingForm loadingForm = null;
            if (willUseLoading)
            {
                loadingForm = new LoadingForm();
                loadingForm.StartPosition = FormStartPosition.CenterParent;
                loadingForm.Show();
                loadingForm.Top = parent.Top + ((parent.Height / 2) - (loadingForm.Height / 2));
                loadingForm.Left = parent.Left + ((parent.Width / 2) - (loadingForm.Width / 2));
                loadingForm.Refresh();
            }
            formAction.Invoke(GetThis());
            if (willUseLoading)
                loadingForm.Close();
        }

        private T GetThis()
        {
            return (T) this;
        }
    }

    public abstract class ActionableForm: Form
    {
        public ActionableForm()
        {
            TopLevel = false;
            FormBorderStyle = FormBorderStyle.None;
            DoubleBuffered = true;
        }
    }
}
