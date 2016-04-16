using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionFSM
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

                sb.Append("Time,Event ,Inferred Motion, Real Motion \r\n");

                File.WriteAllText(this.filePath,sb.ToString());
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
