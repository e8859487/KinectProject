using Microsoft.Kinect;
using Microsoft.Kinect.Tools;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MultipleKinectClient3D
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged, IDisposable
    {

        private KinectSensor kinectSensor = null;

        private string kinectStatusText = String.Empty;

        private bool isPlaying = false;

        private string lastFile = string.Empty;

        private delegate void OneArgDelegate(string arg);

        private delegate void NoArgDelegate();

        private string playBackFilePath;

        private ClientKinectProcessor3D clientKinectProcessor3D = null;
        public MainWindow()
        {
            InitializeComponent();

            this.kinectSensor = KinectSensor.GetDefault();
            this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;
            this.kinectSensor.Open();
            this.KinectStatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                : Properties.Resources.SensorNotAvailableStatusText;

            this.clientKinectProcessor3D = new ClientKinectProcessor3D(kinectSensor);
            this.clientKinectProcessor3D.PlayerStateChanged += new clientEventHandler(StatusChange);


            // set data context for display in UI
            this.DataContext = this.clientKinectProcessor3D;
            this.KinectStatus.DataContext = this;

            //讀取同步播放檔案路徑
            XmlManager.XmlReader reader = new XmlManager.XmlReader(@"./Setting/3DSetting.xml");
            playBackFilePath = reader.getNodeInnerText(@"/Root/SynchronousPlayPath");
            reader.Dispose();
        }

        public void Dispose()
        {
            if (this.clientKinectProcessor3D != null)
            {
                this.clientKinectProcessor3D.Dispose();
                this.clientKinectProcessor3D = null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string KinectStatusText
        {
            get
            {
                return this.kinectStatusText;
            }
            set
            {
                if (this.kinectStatusText != value)
                {
                    this.kinectStatusText = value;

                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("KinectStatusText"));
                    }
                }
            }
        }

        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            this.KinectStatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                  : Properties.Resources.SensorNotAvailableStatusText;
        }

        /// <summary>
        /// 處理Server端傳過來的資訊 ，同步控制撥放或是錄影
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StatusChange(object sender, SocketPackage.StatusEventArgs e)
        {
            //control Record
            if (e.Status == SocketPackage.TRANSMIT_STATUS.StartPlaybackClip || e.Status == SocketPackage.TRANSMIT_STATUS.StopPlaybackClip)
            {
                OneArgDelegate playback = new OneArgDelegate(this.PlaybackClip);
                playback.BeginInvoke(playBackFilePath, null, null);     
                //MessageBox.Show(playBackFilePath);

            }
        }


        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = this.OpenFileForPlayBack();

            if (!string.IsNullOrEmpty(filePath))
            {
                this.lastFile = filePath;
                this.isPlaying = true;
                this.UpdateState();

                // Start running the playback asynchronously
                OneArgDelegate playback = new OneArgDelegate(this.PlaybackClip);
                playback.BeginInvoke(filePath, null, null);
            }
        }

        private void PlaybackClip(string filePath)
        {
            using (KStudioClient client = KStudio.CreateClient())
            {
                client.ConnectToService();
                using (KStudioPlayback playback = client.CreatePlayback(filePath))
                {
                    playback.Start();
                    while (playback.State == KStudioPlaybackState.Playing)
                    {
                        Thread.Sleep(500);
                    }
                }
                client.DisconnectFromService();
            }
            this.isPlaying = false;
            this.Dispatcher.BeginInvoke(new NoArgDelegate(UpdateState));
        }

        private void UpdateState()
        {
            if (this.isPlaying)
            {
                this.PlayButton.IsEnabled = false;
                this.SynchronousPlay.IsEnabled = false;
            }
            else
            {
                this.PlayButton.IsEnabled = true;
                this.SynchronousPlay.IsEnabled = true;
            }
        }

        private string OpenFileForPlayBack()
        {
            string fileName = string.Empty;
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.FileName = this.lastFile;
            dlg.DefaultExt = Properties.Resources.XefExtension;
            dlg.Filter = Properties.Resources.EventFileDescription + " " + Properties.Resources.EventFileFilter; // Filter files by extension 
            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                fileName = dlg.FileName;
            }
            return fileName;
        }

        private void SynChronousButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SendTest_Click(object sender, RoutedEventArgs e)
        {
            clientKinectProcessor3D.SendTestMsg();
        }
    }
}
