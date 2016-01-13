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

namespace MultipleKinectClient
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

        private ClientKinectProcessor kinectBodyView = null;



        public MainWindow()
        {
            this.InitializeComponent();
            this.kinectSensor = KinectSensor.GetDefault();
            this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;
            this.kinectSensor.Open();
            this.kinectStatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.SensorNotAvailableStatusText;
            this.kinectBodyView = new ClientKinectProcessor(this.kinectSensor);

            // set data context for display in UI

            this.DataContext = this;
            this.kinectBodyViewbox.DataContext = this.kinectBodyView;
            this.BodyIndex.DataContext = this.kinectBodyView;
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
        }

        #region Private Method

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
        #endregion

        #region Button_Event
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

        private void SendMeg_Click(object sender, RoutedEventArgs e)
        {
            kinectBodyView.SendImgToServer();
        } 
        #endregion

        private void CatchImg_Click(object sender, RoutedEventArgs e)
        {
            kinectBodyView.CatchImgBitmap();
        }

        private void Render_Click(object sender, RoutedEventArgs e)
        {
            kinectBodyView.RenderBitmap();
        }

    }
}
