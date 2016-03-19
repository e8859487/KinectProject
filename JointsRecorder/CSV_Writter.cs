using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JointsRecorder
{
    public class CSV_Writter
    {
        private string filePath = string.Empty;
        public CSV_Writter(string filePath)
        {
            this.filePath = filePath;
            if (!File.Exists(this.filePath))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Time,UserId,Motions,SpineBase.x,SpineBase.y,SpineBase.z, SpineMid.x,SpineMid.y,SpineMid.z, Neck.x,Neck.y,Neck.z, Head.x,Head.y,Head.z, ShoulderLeft.x,ShoulderLeft.y,ShoulderLeft.z, ElbowLeft.x,ElbowLeft.y,ElbowLeft.z, WristLeft.x,WristLeft.y,WristLeft.z, HandLeft.x,HandLeft.y,HandLeft.z, ShoulderRight.x,ShoulderRight.y,ShoulderRight.z, ElbowRight.x,ElbowRight.y,ElbowRight.z, WristRight.x,WristRight.y,WristRight.z, HandRight.x,HandRight.y,HandRight.z, HipLeft.x,HipLeft.y,HipLeft.z, KneeLeft.x,KneeLeft.y,KneeLeft.z, AnkleLeft.x,AnkleLeft.y,AnkleLeft.z, FootLeft.x,FootLeft.y,FootLeft.z, HipRight.x,HipRight.y,HipRight.z, KneeRight.x,KneeRight.y,KneeRight.z, AnkleRight.x,AnkleRight.y,AnkleRight.z, FootRight.x,FootRight.y,FootRight.z, SpineShoulder.x,SpineShoulder.y,SpineShoulder.z, HandTipLeft.x,HandTipLeft.y,HandTipLeft.z, ThumbLeft.x,ThumbLeft.y,ThumbLeft.z, HandTipRight.x,HandTipRight.y,HandTipRight.z, ThumbRight.x,ThumbRight.y,ThumbRight.z");
                sb.Append(",,SpineBase, SpineMid, Neck, Head, ShoulderLeft, ElbowLeft, WristLeft, HandLeft, ShoulderRight, ElbowRight, WristRight, HandRight, HipLeft, KneeLeft, AnkleLeft, FootLeft, HipRight, KneeRight, AnkleRight, FootRight, SpineShoulder, HandTipLeft, ThumbLeft, HandTipRight, ThumbRight\r\n");
                File.WriteAllText(this.filePath,sb.ToString() );
            
            }
        }

        public void WriteLine(string data)
        {
            StreamWriter sw;
            using ( sw = new StreamWriter(filePath,true))
            {
                sw.WriteLine(data);
            }
            sw.Close();
        }
    
    }
}
