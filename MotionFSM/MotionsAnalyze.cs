using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocketPackage;


namespace MotionFSM
{
    using XmlManager;

    public  class MotionsAnalyze
    {
        internal readonly Dictionary<string, int> THS;



        public MotionsAnalyze()
        {
            THS = new Dictionary<string, int>();

            XmlReader reader = new XmlReader(@"./Setting.xml");
            THS.Add("T1", int.Parse(reader.getNodeInnerText(@"/Root/T1")));
            THS.Add("T2", int.Parse(reader.getNodeInnerText(@"/Root/T2")));
            THS.Add("T3", int.Parse(reader.getNodeInnerText(@"/Root/T3")));
            THS.Add("T4", int.Parse(reader.getNodeInnerText(@"/Root/T4")));
            THS.Add("T5", int.Parse(reader.getNodeInnerText(@"/Root/T5")));
            THS.Add("T6", int.Parse(reader.getNodeInnerText(@"/Root/T6")));
            THS.Add("T7", int.Parse(reader.getNodeInnerText(@"/Root/T7")));
            THS.Add("T8", int.Parse(reader.getNodeInnerText(@"/Root/T8")));
            THS.Add("T9_1", int.Parse(reader.getNodeInnerText(@"/Root/T9_1")));
            THS.Add("T9_2", int.Parse(reader.getNodeInnerText(@"/Root/T9_2")));

            reader.Dispose();


        }


        public static void EventAnalyze(MyBody pre,MyBody now,MyBody Init,int fallDownThreshold){

        }



    }
}
