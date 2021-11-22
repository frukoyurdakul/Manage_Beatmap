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

        public ActionableForm()
        {
            formAction = null;
            DoubleBuffered = true;
        }

        public ActionableForm(Action<T> action)
        {
            formAction = action;
            DoubleBuffered = true;
        }

        protected virtual void InvokeAction()
        {
            formAction.Invoke(GetThis());
        }

        private T GetThis()
        {
            return (T) this;
        }
    }

    public abstract class ActionableForm: Form
    {

    }
}
