using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace MultipleKinectMaster3D
{
    using HelixToolkit.Wpf;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Tools;
    using Microsoft.Win32;
    using MultipleKinectMaster3D.UISetting;
    using System.ComponentModel;
    using System.Threading;
    using System.Windows.Media.Media3D;
    using SpaceDepolyment;

    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    /// 

    public partial class MainWindow : Window, INotifyPropertyChanged, IDisposable
    {
        #region Private Property
        private KinectSensor kinectSensor = null;

        private string kinectStatusText = String.Empty;

        private bool isPlaying = false;

        private string lastFile = string.Empty;

        private delegate void OneArgDelegate(string arg);

        private delegate void NoArgDelegate();

        private string playBackFilePath;

        private MasterKinectProcessor3D masterKinectProcessor3D = null;

        /// <summary>
        /// 相機位置
        /// </summary>
        CameraPosMapper cameraPosMapper;
        SerialPortIO serialPortIO;
        #endregion
        public MainWindow()
        {
            InitializeComponent();

            //綁定相機位置與轉向
            cameraPosMapper = new CameraPosMapper();
            cameraPosMapper.AddDevice(new Point3D(50, 155, 10), 15);
            cameraPosMapper.AddDevice(new Point3D(252, 155, 570), 180);
            cameraPosMapper.AddDevice(new Point3D(-410, 155, 560), 108);


            //draw img
            this.drawWall();

            this.kinectSensor = KinectSensor.GetDefault();
            this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;
            this.kinectSensor.Open();
            this.KinectStatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                : Properties.Resources.SensorNotAvailableStatusText;

            this.masterKinectProcessor3D = new MasterKinectProcessor3D(kinectSensor, this.Dispatcher);
            this.masterKinectProcessor3D.SetMotionUI(LIB_Motion);
            this.masterKinectProcessor3D.SetCameraDeplyment(cameraPosMapper);


            ///*for IOT*/
            //serialPortIO = new SerialPortIO(9600, "COM7", LIB_Member, this.Dispatcher,  Doorstate);


            XmlManager.XmlReader reader = new XmlManager.XmlReader(@"./Setting/3DSetting.xml");
            playBackFilePath = reader.getNodeInnerText(@"/Root/SynchronousPlayPath");
            reader.Dispose();



            // set data context for display in UI

            this.DataContext = new
            {
                masterKinectProcessor = this.masterKinectProcessor3D,
                WallDeplyment = wallList,
                CameraPos1 = cameraPosMapper.GetCameraSetting(1),
                CameraPos2 = cameraPosMapper.GetCameraSetting(2),
                CameraPos3 = cameraPosMapper.GetCameraSetting(3),
                //DoorState= this.serialPortIO.DoorState
                //this.Lbl_TimeStamp.DataContext = this.masterKinectProcessor3D;
                //this.KinectStatus.DataContext = this;
            };
        }

        List<WallSetting> wallList = new List<WallSetting>();

        private void drawWall()
        {
            int WallLength_X = 444; //圍牆長
            int WallWidth_Y = 90;   //圍牆高   
            int WallHeight_Z = 10;  //圍牆厚
            int WallLength_X2 = 636;
            Brush wallBrush = Brushes.AliceBlue;
            //      7   1           —   —
            //    6   4    2      ｜   ｜  ｜
            //      5   3           __   __
            wallList.Add(new WallSetting(WallLength_X+5, WallWidth_Y, WallHeight_Z    , new Point3D((WallLength_X +5) / 2  , WallWidth_Y / 2, 0), wallBrush));                                       // 1

            wallList.Add(new WallSetting(WallHeight_Z, WallWidth_Y, WallLength_X2 -55, new Point3D(WallLength_X, WallWidth_Y / 2, (WallLength_X2 + 67) / 2), wallBrush));                 // 2

            wallList.Add(new WallSetting(WallLength_X, WallWidth_Y, WallHeight_Z, new Point3D(WallLength_X / 2, WallWidth_Y / 2, WallLength_X2), wallBrush));                            // 3

            wallList.Add(new WallSetting(WallHeight_Z, WallWidth_Y, WallLength_X2 - 150, new Point3D(WallHeight_Z / 2, WallWidth_Y / 2,( WallLength_X2 - 150) / 2), wallBrush));                       // 4

            wallList.Add(new WallSetting(WallLength_X, WallWidth_Y, WallHeight_Z, new Point3D(-WallLength_X / 2, WallWidth_Y / 2, WallLength_X2), wallBrush));                             // 5

            wallList.Add(new WallSetting(WallHeight_Z, WallWidth_Y, WallLength_X2, new Point3D(-WallLength_X + 5, WallWidth_Y / 2, (WallLength_X2) / 2), wallBrush));                        // 6

            wallList.Add(new WallSetting(WallLength_X, WallWidth_Y, WallHeight_Z    , new Point3D(-WallLength_X / 2, WallWidth_Y / 2, 0), wallBrush));                                        // 7



            Dictionary<string, DependencyProperty> dict_propertyPair = new Dictionary<string, DependencyProperty>();
            dict_propertyPair.Add("WallWidth", BoxVisual3D.WidthProperty);
            dict_propertyPair.Add("WallHeight", BoxVisual3D.HeightProperty);
            dict_propertyPair.Add("WallLength", BoxVisual3D.LengthProperty);
            dict_propertyPair.Add("WallCenter", BoxVisual3D.CenterProperty);
            dict_propertyPair.Add("WallBrush", BoxVisual3D.FillProperty);

            foreach (var wSetting in wallList)
            {
                BoxVisual3D bv3d = new BoxVisual3D();
                foreach (KeyValuePair<string, DependencyProperty> items in dict_propertyPair)
                {
                    Binding binding = new Binding();
                    binding.Source = wSetting;
                    binding.Path = new PropertyPath(items.Key);
                    BindingOperations.SetBinding(bv3d, items.Value, binding);
                }
                WallSetting.Children.Add(bv3d);
            }
        }

        public void Dispose()
        {
            if (this.masterKinectProcessor3D != null)
            {
                this.masterKinectProcessor3D.Dispose();
                this.masterKinectProcessor3D = null;
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
                masterKinectProcessor3D.ResetStartTime();
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
            OneArgDelegate playback = new OneArgDelegate(this.PlaybackClip);
            this.masterKinectProcessor3D.SendStartAsychronousPlay();
            //SocketPackage.SocketServer.SendStartAsychronousPlay();
            playback.BeginInvoke(playBackFilePath, null, null);
        }

        private void SetToUnknow_Click(object sender, RoutedEventArgs e)
        {
            masterKinectProcessor3D.SetMotionToUnknown();
        }

    }
}
