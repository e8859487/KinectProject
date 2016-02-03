using Microsoft.Kinect;
using Microsoft.Kinect.Tools;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;

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

        private ClientKinectProcessor clientKinectProcessor = null;

        private bool isRecording;

        private KStudioRecording recording;

        private string playBackFilePath;

        public MainWindow()
        {
            this.InitializeComponent();
            this.kinectSensor = KinectSensor.GetDefault();
            this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;
            this.kinectSensor.Open();
            this.kinectStatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.SensorNotAvailableStatusText;
            this.clientKinectProcessor = new ClientKinectProcessor(this.kinectSensor);
            this.clientKinectProcessor.changed += new clientRecordEventHandler(StatusChange);
            // set data context for display in UI

            this.DataContext = this;
            this.kinectBodyViewbox.DataContext = this.clientKinectProcessor;
            this.BodyIndex.DataContext = this.clientKinectProcessor;


            //讀取同步播放檔案路徑
            XmlManager.XmlReader reader = new XmlManager.XmlReader(@"./Setting.xml");
            playBackFilePath = reader.getNodeInnerText(@"/Root/SynchronousPlayPath");
            reader.Dispose();

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
            if (this.clientKinectProcessor != null)
            {
                this.clientKinectProcessor.Dispose();
                this.clientKinectProcessor = null;
            }
        }

        #region Private Method

        /// <summary>
        /// 處理Server端傳過來的資訊 ，同步控制撥放或是錄影
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StatusChange(object sender, SocketPackage.StatusEventArgs e)
        {
            //control Record
            if (e.Status == SocketPackage.TRANSMIT_STATUS.StartRecord || e.Status == SocketPackage.TRANSMIT_STATUS.StopRecord)
            {
                Record_Click(null, null);
            }
            else if(e.Status == SocketPackage.TRANSMIT_STATUS.StartPlaybackClip || e.Status == SocketPackage.TRANSMIT_STATUS.StopPlaybackClip){
                OneArgDelegate playback = new OneArgDelegate(this.PlaybackClip);
                playback.BeginInvoke(playBackFilePath, null, null);
            }
        }



        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.clientKinectProcessor != null)
            {
                this.clientKinectProcessor.Dispose();
                this.clientKinectProcessor = null;
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

        private void RecordClip(string filePath)
        {
            using (KStudioClient client = KStudio.CreateClient())
            {
                client.ConnectToService();

                //Specify which streams should be recorded
                KStudioEventStreamSelectorCollection streamCollection = new KStudioEventStreamSelectorCollection();
                streamCollection.Add(KStudioEventStreamDataTypeIds.Ir);
                streamCollection.Add(KStudioEventStreamDataTypeIds.Depth);
                streamCollection.Add(KStudioEventStreamDataTypeIds.Body);

                 recording = client.CreateRecording(filePath, streamCollection);
                    recording.Start();
                    while (recording.State == KStudioRecordingState.Recording)
                    {
                        Thread.Sleep(500);
                    }

                client.DisconnectFromService();
            }

            //Update UI after the background recording task has completed
            this.isRecording = false;
            this.Dispatcher.BeginInvoke(new NoArgDelegate(UpdateState));
        }

        private string SaveRecordingAs()
        {
            string fileName = string.Empty;
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "recordAndPalaybackBasics.xef";
            dlg.DefaultExt = Properties.Resources.XefExtension;
            dlg.AddExtension = true;
            dlg.Filter = Properties.Resources.EventFileDescription + " " + Properties.Resources.EventFileFilter;
            dlg.CheckPathExists = true;
            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                fileName = dlg.FileName;
            }

            return fileName;
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
 
        private void Record_Click(object sender, RoutedEventArgs e)
        {
            if (isRecording == false)
            {
               string filePath = "D:\\clientSynRecord1.xef";
               // string filePath = this.SaveRecordingAs();
               // C:\\recordAndPalaybackBasics2.xef

                if (!string.IsNullOrEmpty(filePath))
                {
                    this.lastFile = filePath;
                    this.isRecording = true;
                    //this.RecordPlaybackStatusText = Properties.Resources.RecordingInProgressText;
                    //this.UpdateState();

                    //Start runing the plalyback asychronously
                    OneArgDelegate recording = new OneArgDelegate(this.RecordClip);
                    recording.BeginInvoke(filePath, null, null);
                }
            }
            else
            {
                this.isRecording = false;
                // this.UpdateState();
                //this.RecordPlaybackStatusText = "";

                if (recording != null)
                {
                    if (recording.State == KStudioRecordingState.Recording)
                    {
                        recording.Stop();
                        recording.Dispose();
                    }
                }
            }
        }
        #endregion

    }
}
