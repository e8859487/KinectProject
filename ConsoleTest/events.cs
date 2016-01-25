using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCollections
{
    using System.Collections;
    
    public delegate void ChangeEventHandler(object sender,EventArgs e);

    public class ListWithChangeEvent:ArrayList
    {
        public event ChangeEventHandler Changed;

        protected virtual void OnChanged(EventArgs e)
        {
            if (Changed != null)
            {
                Changed(this, e);
            }
        }

        public override int Add(object value)
        {
            OnChanged(EventArgs.Empty);
            return base.Add(value);
        }

        public override void Clear()
        {
            OnChanged(EventArgs.Empty);
            base.Clear();
        }

        public override object this[int index]
        {
            set
            {
                base[index] = value;
                OnChanged(EventArgs.Empty);
            }
        }
    }
}
