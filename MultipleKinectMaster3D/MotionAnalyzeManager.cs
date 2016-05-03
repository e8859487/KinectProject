using MotionFSM;
using SocketPackage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MultipleKinectMaster3D
{
    class MotionAnalyzeManager : IWorkflowView
    {
        public Dictionary<ulong, MotionsAnalyze> motionDict = new Dictionary<ulong, MotionsAnalyze>();

        System.Windows.Controls.ListBox listbox;

        public MotionAnalyzeManager(System.Windows.Controls.ListBox listbox)
        {
            this.listbox = listbox;
        }

        /// <summary>
        /// 新增指定 ID 的 Tracker
        /// </summary>
        /// <param name="TrackingId"></param>
        public void AddMotionAnalyzer(ulong TrackingId)
        {
            if (!motionDict.ContainsKey(TrackingId))
            {
               MotionsAnalyze motionAnalyze =  new MotionsAnalyze(this);
               motionDict.Add(TrackingId, motionAnalyze);
               this.AddNewTextbox(TrackingId,motionAnalyze);
            }
            else
            {
                Debug.Print(" >> MotionAnalyzeManager.AddMotionAnalyzer already exixt TrackingId " + TrackingId.ToString());
            }

        }

     

        /// <summary>
        /// 移除指定 ID 的 Tracker
        /// </summary>
        /// <param name="TrackingId"></param>
        public void RemoveMotionAnalyzer(ulong TrackingId)
        {
            if (motionDict.ContainsKey(TrackingId))
            {
                motionDict[TrackingId].Dispose();
                motionDict.Remove(TrackingId);
                this.RemoveTextbox(TrackingId);
            }
            else
            {
                Debug.Print(" >> MotionAnalyzeManager.RemoveMotionAnalyzer not find!! TrackingId " + TrackingId.ToString());
            }
        }

        Dictionary<ulong, TextBox> textBoxDict = new Dictionary<ulong, TextBox>();

        private void AddNewTextbox(ulong TrackingId, MotionsAnalyze motionAnalyze)
        {
            TextBox tb = new TextBox();

            textBoxDict.Add(TrackingId, tb);

            tb.SetBinding(TextBox.TextProperty, new Binding()
            {
                Source = motionAnalyze,
                Path = new PropertyPath("CurrentMotions")
            });
            listbox.Items.Add(tb);
            Debug.Print(">>List Numbers {0}", listbox.Items.Count);

        }

        private void RemoveTextbox(ulong TrackingId)
        {
            TextBox tb = textBoxDict[TrackingId];

            listbox.Items.Remove(tb);

            textBoxDict.Remove(TrackingId);
        }


        /// <summary>
        /// 給ID, 動作分析
        /// </summary>
        /// <param name="TrackingId"></param>
        /// <param name="body"></param>
        public void EventAnalyze(ulong TrackingId,MyBody body)
        {
            if (motionDict.ContainsKey(TrackingId))
            {
                motionDict[TrackingId].EventAnalyze(body);
            }
            else
            {
                Debug.Print(" >> MotionAnalyzeManager.EventAnalyze not find!! TrackingId " + TrackingId.ToString());
            }
        }

 


        public void OnIdle(System.Activities.WorkflowApplicationIdleEventArgs args)
        {
            //throw new NotImplementedException();
        }

        public System.Activities.UnhandledExceptionAction OnUnhandledException(System.Activities.WorkflowApplicationUnhandledExceptionEventArgs args)
        {
            throw new NotImplementedException();
         
        }
    }
}
