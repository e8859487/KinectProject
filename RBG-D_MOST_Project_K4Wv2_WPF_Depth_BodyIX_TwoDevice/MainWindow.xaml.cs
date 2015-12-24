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
using System.Windows.Threading;

namespace RBG_D_MOST_Project_K4Wv2_WPF_Depth_BodyIX_TwoDevice
{
     /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged, IDisposable
    {
        /// <summary> Indicates if a playback is currently in progress </summary>
        private bool isPlaying = false;

        private string lastFile = string.Empty;

        private delegate void NoArgDelegate();

        private delegate void OneArgDelegate(string arg);


        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

        /// <summary>
        /// Current kinect sensor status text to display
        /// </summary>
        private string kinectStatusText = string.Empty;

        private KinectBodyView kinectBodyView = null;
        private KinectBodyView kinectBodyView_B = null;

        DispatcherTimer timer = null;
        private Boolean timerFlag = false;

        private int deviceIndex = 1;
        private const string DEV_A = "Dev_A";
        private const string DEV_B = "Dev_B";
        private string myDev = DEV_B;
        
        private DevconHelper devHelper = null;

        public MainWindow()
        {
            //Must Disable All Device Before Enter Main Loop
            devHelper = new DevconHelper();
            devHelper.AddDevice(DEV_A, @"@USB\VID_045E&PID_02C4&MI_00\7&6D76B01&0&0000");
            devHelper.AddDevice(DEV_B, @"@USB\VID_045E&PID_02C4&MI_00\7&1FC7402&0&0000");
            devHelper.DisableDevice(DEV_A);
            devHelper.DisableDevice(DEV_B);

            this.InitializeComponent();
            this.kinectSensor = KinectSensor.GetDefault();
            //this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;
            this.kinectSensor.Open();
            this.kinectStatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.SensorNotAvailableStatusText;
            this.kinectBodyView = new KinectBodyView(this.kinectSensor);
            this.kinectBodyView_B = new KinectBodyView(this.kinectSensor);

            // set data context for display in UI

            this.DataContext = this;
            this.kinectBodyViewbox.DataContext = this.kinectBodyView;
            this.kinectBodyViewbox_B.DataContext = this.kinectBodyView_B;

            this.timer = new DispatcherTimer();
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

        public void Dispose()
        {
            if (this.kinectBodyView != null)
            {
                this.kinectBodyView.Dispose();
                this.kinectBodyView = null;
            }
            //if (this.timer != null)
            //{
            //    this.timer.Stop();
            //}
            //if (this.devHelper != null)
            //{
            //    //this.devHelper.dispose();
            //}
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.kinectBodyView != null)
            {
                this.kinectBodyView.Dispose();
                this.kinectBodyView = null;
            }
            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }

        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            this.KinectStatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.SensorNotAvailableStatusText;
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

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            CloseKinectSensor();
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            OpenKinectSensor();
        }

        private void StartSwtichButton_Click(object sender, RoutedEventArgs e)
        {
            this.timer.Interval = TimeSpan.FromMilliseconds(1500);
            this.timer.Tick += timer_Tick;
            if (timerFlag == false)
            {
                this.timer.Start();
                timerFlag = true;
                MessageBox.Show("Timer Open");
            }
            else
            {
                this.timer.Stop();
                timerFlag = false;
                MessageBox.Show("Timer Close");
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            //only switch
            if (deviceIndex == 1)
            {
                CloseKinectSensor();
                devHelper.DisableDevice(DEV_A);
                devHelper.EnableDecive(DEV_B);
                Thread.Sleep(50);
                OpenKinectSensor();
                this.kinectBodyView_B.CloseSourceListener();
                this.kinectBodyView.OpenSourceListener();
                deviceIndex = 2;
            }
            else if (deviceIndex == 2)
            {
                CloseKinectSensor();
                devHelper.DisableDevice(DEV_B);
                devHelper.EnableDecive(DEV_A);
                Thread.Sleep(50);
                OpenKinectSensor();
                this.kinectBodyView.CloseSourceListener();
                this.kinectBodyView_B.OpenSourceListener();
                deviceIndex = 1;
            }
        }

        private void SamllSwtichButton_Click(object sender, RoutedEventArgs e)
        {
            if (deviceIndex == 1)
            {
                CloseKinectSensor();

                devHelper.DisableDevice(DEV_A);
                devHelper.EnableDecive(DEV_B);
                Thread.Sleep(1000);
                OpenKinectSensor();
                this.kinectBodyView_B.CloseSourceListener();
                this.kinectBodyView.OpenSourceListener();
                deviceIndex = 2;
            }
            else if (deviceIndex == 2)
            {
                CloseKinectSensor();

                 devHelper.DisableDevice(DEV_B);
                 devHelper.EnableDecive(DEV_A);
                 Thread.Sleep(1000);
                
                OpenKinectSensor();
                this.kinectBodyView.CloseSourceListener();
                this.kinectBodyView_B.OpenSourceListener();
                deviceIndex = 1;
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

        private void PlaybackClip(string filePath)
        {
            using (KStudioClient client = KStudio.CreateClient())
            {
                client.ConnectToService();
                using (KStudioPlayback playback = client.CreatePlayback(filePath))
                {
                    playback.LoopCount = 1;
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
            }
            else
            {
                this.PlayButton.IsEnabled = true;
            }
        }

        private void OpenKinectSensor()
        {
            if (this.kinectSensor == null)
            {
                this.kinectSensor = KinectSensor.GetDefault();
                this.kinectSensor.Open();
            }
        }

        private void CloseKinectSensor()
        {
            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }
 
        private void DisableButton_Click(object sender, RoutedEventArgs e)
        {
            devHelper.DisableDevice(myDev);
        }

        private void EnableButton_Click(object sender, RoutedEventArgs e)
        {
            devHelper.EnableDecive(myDev);
        }

        private void StatusButton_Click(object sender, RoutedEventArgs e)
        {
            devHelper.CheckDeviceStatus(myDev);
        }

        private void DevRestartButton_Click(object sender, RoutedEventArgs e)
        {
            devHelper.RestartDevice(myDev);
        }

        private void SwitchDevButton_Click(object sender, RoutedEventArgs e)
        {
            if (myDev == DEV_A)
            {
                myDev = DEV_B;
            }
            else
            {
                myDev = DEV_A;
            }
        }
    }
}
