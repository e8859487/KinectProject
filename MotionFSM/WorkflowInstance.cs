using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionFSM
{
    using System.Activities;
    using Microsoft.Activities.Extensions.Tracking;
    using System.ComponentModel;

    public class WorkflowInstance : INotifyPropertyChanged
    {
        internal static readonly Activity workflowDefinition = new Activity1();

        public StateMachineStateTracker StateTracker { get; private set; }

        public WorkflowApplication Host { get; set; }

        private readonly IWorkflowView view;

        public bool IsEventExist(string EventName)
        {
            //目前是事件為: S_Unknow 則所有事件都可以成立
            if (StateTracker.CurrentState == "S_UnKnow")
            {
                return true;
            }

            if (StateTracker != null) { 
            //Trusted transitions
            foreach (System.Activities.Statements.Transition ts in StateTracker.Transitions)
            {
                if (String.Equals(ts.DisplayName, EventName))
                {
                    return true;
                }
            }
            }
            //unknow transition
            return false;
        }


        public WorkflowInstance(IWorkflowView view, StateMachineStateTracker stateMachineStateTracker = null)
        {
            this.view = view;
            this.StateTracker = stateMachineStateTracker ?? new StateMachineStateTracker(workflowDefinition);
        }

        #region Public Events

        /// <summary>
        ///   The property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public void Run()
        {
            this.CreateWorkflowApplication();
            this.Host.Run();
        }

        public void Disposed()
        {
            this.Host.Terminate("Terminate");
        }

        public void ResumeBookmark(string bookmark, string info = null)
        {
            Host.ResumeBookmark(bookmark, info);
        }

        private void CreateWorkflowApplication()
        {
            this.Host = new WorkflowApplication(workflowDefinition)
            {
                Idle = this.view.OnIdle,
                //Aborted = this.view.OnAbort,
                //Completed = this.view.OnComplete,
                //InstanceStore = this.view.InstanceStore,
                //OnUnhandledException = this.view.OnUnhandledException,
                //PersistableIdle = this.view.PersistableIdle,
                //Unloaded = this.view.OnUnload
            };

            this.Host.Extensions.Add(this.StateTracker);
        }




        /// <summary>
        /// The on property changed.
        /// </summary>
        /// <param name="propertyName">
        /// The property name. 
        /// </param>
        private void NotifyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
