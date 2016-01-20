using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XmlManager
{
    public class XmlReader
    {
        XmlDocument xmlDoc=null;
        public XmlReader(string filePath){

            xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);
        
        }
        public string getNodeInnerText(string nodeName)
        {
            try
            {
                XmlNode node = xmlDoc.SelectSingleNode(nodeName);
                if (node != null)
                {
                    return node.InnerText;

                }
                else
                {
                    return "Couldn't find Node nameed: " + nodeName;
                }
            }
            catch(Exception ee)
            {
                return ee.ToString();
            }
        }
    }
}
