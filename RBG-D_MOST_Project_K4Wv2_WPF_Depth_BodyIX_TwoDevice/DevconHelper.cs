using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RBG_D_MOST_Project_K4Wv2_WPF_Depth_BodyIX_TwoDevice
{
    class DevconHelper
    {
        Dictionary<string, string> deviceDict = null;//save device name and it instance ID
        ProcessStartInfo cmdStartInfo = null;
        Process cmdProcess = null;
        StringBuilder sbOutputDataReceive = null;
        StringBuilder sbErrorDataEeceive = null;

        public DevconHelper()
        {
            deviceDict = new Dictionary<string, string>();
            sbOutputDataReceive = new StringBuilder();
            sbErrorDataEeceive = new StringBuilder();

            cmdStartInfo = new ProcessStartInfo();
            cmdStartInfo.FileName = @"C:\Windows\System32\cmd.exe";
            cmdStartInfo.RedirectStandardOutput = true;     //重定向標準輸出
            cmdStartInfo.RedirectStandardError = true;      //重定向錯誤輸出
            cmdStartInfo.RedirectStandardInput = true;      //重定向標準輸入
            cmdStartInfo.UseShellExecute = false;           //關閉Shell的使用
            cmdStartInfo.CreateNoWindow = true;             //設置不顯示視窗
        }

        /// <summary>
        /// add Device. 
        /// </summary>
        /// <param name="deviceName">Name </param>
        /// <param name="deviceInstanceID">Device instance ID </param>
        public void AddDevice(string deviceName, string deviceInstanceID)
        {
            deviceDict.Add(deviceName, deviceInstanceID);
        }

        /// <summary>
        /// Remove Device.
        /// </summary>
        /// <param name="deviceName"></param>
        public void RemoveDevice(string deviceName)
        {
            deviceDict.Remove(deviceName);
        }

        /// <summary>
        /// Enable a Device by DeviceName
        /// </summary>
        /// <param name="DeviceName"></param>
        public void EnableDecive(string DeviceName)
        {
            if (deviceDict.ContainsKey(DeviceName) && CheckDeviceAvailable(DeviceName))
            {
                sbErrorDataEeceive.Clear();
                sbOutputDataReceive.Clear();
                DevconProcesser(DeviceName, "enable");  //enable Device

                if (sbOutputDataReceive.Length > 0)
                {
                    if (sbOutputDataReceive.ToString().Contains("restart"))
                    {
                        DevconProcesser(DeviceName, "restart"); //Restart Device
                        DevconProcesser(DeviceName, "enable"); //Enable Device
                    }
                    //Show error message when it failed. 
                    if (sbOutputDataReceive.ToString().Contains("Enabled on reboot"))
                    {
                        MessageBox.Show("Output Data : " + sbOutputDataReceive.ToString());
                    }
                }
                 if (sbErrorDataEeceive.Length > 4)
                {
                    MessageBox.Show("Error output Data : " + sbErrorDataEeceive.ToString());
                }
            }
        }

        /// <summary>
        /// Disable a Device by Device Name.
        /// </summary>
        /// <param name="DeviceName"></param>
        public void DisableDevice(string DeviceName)
        {
            if (deviceDict.ContainsKey(DeviceName) && CheckDeviceAvailable(DeviceName))
            {
                sbErrorDataEeceive.Clear();
                sbOutputDataReceive.Clear();
                DevconProcesser(DeviceName, "disable"); //Disable Device

                if (sbOutputDataReceive.Length > 0)
                {
                    //When it should be restarted, just restart and then disable it.
                    if (sbOutputDataReceive.ToString().Contains("restart"))
                    {
                        DevconProcesser(DeviceName, "restart"); //Restart Device
                        DevconProcesser(DeviceName, "disable"); //Disable Device
                    }
                    //Show error message when it failed. 
                    if (sbOutputDataReceive.ToString().Contains("Disabled on reboot"))
                    {
                        MessageBox.Show("Output Data : " + sbOutputDataReceive.ToString());
                    }
                }
                 if (sbErrorDataEeceive.Length > 4)
                {
                    MessageBox.Show("Error output Data : " + sbErrorDataEeceive.ToString());
                }
            }
        }

        /// <summary>
        /// Restart a Device by Device Name. 
        /// </summary>
        /// <param name="DeviceName"></param>
        public void RestartDevice(string DeviceName)
        {
            if (deviceDict.ContainsKey(DeviceName) && CheckDeviceAvailable(DeviceName))
            {
                sbErrorDataEeceive.Clear();
                sbOutputDataReceive.Clear();
                DevconProcesser(DeviceName, "restart"); //restart Device

                if (sbOutputDataReceive.Length > 0)
                {
                    MessageBox.Show("Output Data : " + sbOutputDataReceive.ToString());

                }
                 if (sbErrorDataEeceive.Length > 4)
                {
                    MessageBox.Show("Error output Data : " + sbErrorDataEeceive.ToString());
                }
            }
        }

        /// <summary>
        /// Show A Device Status By Device Name. 
        /// </summary>
        /// <param name="DeviceName"></param>
        public void CheckDeviceStatus(string DeviceName)
        {
            if (deviceDict.ContainsKey(DeviceName) && CheckDeviceAvailable(DeviceName))
            {
                sbErrorDataEeceive.Clear();
                sbOutputDataReceive.Clear();
                DevconProcesser(DeviceName, "status");  //ckeck Device device

                if (sbOutputDataReceive.Length > 0)
                {
                    MessageBox.Show("DeviceName : " + DeviceName  + "\n" + "Output Data : " + sbOutputDataReceive.ToString());
                }
                if (sbErrorDataEeceive.Length > 4)//原始字串內涵 \r\n  \r\n\r\n
                {
                    MessageBox.Show("Error output Data : " + sbErrorDataEeceive.ToString());
                }
            }
        }



        /// <summary>
        /// List All Devices 
        /// </summary>
        public void ShowAllDevice()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var str in deviceDict)
            {
                sb.Append("Key : " + str.Key + "Value : " + str.Value + "\n");
            }
            MessageBox.Show(sb.ToString());
        }


        public void Dispose()
        {
            if (cmdProcess != null)
            {
                cmdProcess.Dispose();
                cmdProcess.Close();
            }
            
        }

        /// <summary>
        /// Check Device are ready or not
        /// </summary>
        /// <param name="DeviceName"></param>
        /// <returns></returns>
        private Boolean CheckDeviceAvailable(string DeviceName)
        {
            sbErrorDataEeceive.Clear();
            sbOutputDataReceive.Clear();
            DevconProcesser(DeviceName, "status");

            if (sbOutputDataReceive.Length > 0)
            {
                if (sbOutputDataReceive.ToString().Contains("not present"))
                {
                    MessageBox.Show(DeviceName + " not Present");
                    return false;
                }
            }

            if (sbErrorDataEeceive.Length > 4)
            {
                MessageBox.Show(DeviceName + " : UnKnow Error : " + sbErrorDataEeceive.ToString());
                return false;
            }
            return true;
        }


        /// <summary>
        /// Process Devcon Command to comtrol Device
        /// </summary>
        /// <param name="DeviceName">Device Name</param>
        /// <param name="CMD">Devcon's Command </param>
        private void DevconProcesser(string DeviceName, string CMD)
        {
            cmdProcess = new Process();
            cmdProcess.StartInfo = cmdStartInfo;
          //  cmdProcess.ErrorDataReceived += cmd_Error;
            cmdProcess.OutputDataReceived += cmd_DataReceived;
            cmdProcess.EnableRaisingEvents = true;
            cmdProcess.Start();
            cmdProcess.BeginOutputReadLine();
            cmdProcess.BeginErrorReadLine();

            cmdProcess.StandardInput.WriteLine("devcon " + CMD + " \"" + deviceDict[DeviceName] + "\" & exit");

            cmdProcess.WaitForExit();
            //cmdProcess.Kill();
            cmdProcess.Close();
            cmdProcess.Dispose();
        }

        private void cmd_DataReceived(object sender, DataReceivedEventArgs e)
        {
            sbOutputDataReceive.AppendLine(e.Data);
        }

        private void cmd_Error(object sender, DataReceivedEventArgs e)
        {
            sbErrorDataEeceive.AppendLine(e.Data);
        }

    }
}
