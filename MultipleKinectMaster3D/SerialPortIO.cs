using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.IO.Ports;
using System.Diagnostics;
using System.ComponentModel;
using System.Timers;
using System.Windows.Threading;
using System.Windows.Controls;

namespace MultipleKinectMaster3D
{
    class SerialPortIO : INotifyPropertyChanged
    {

        private SerialPort myport;
        static Dictionary<long, bool> idDict;
        System.Windows.Controls.ListBox listbox;
        Timer t;
        Dispatcher dispatcher;
        Slider slider;
        public SerialPortIO(int baudRate, string PortName, System.Windows.Controls.ListBox LIB_Motion, Dispatcher dispatcher ,Slider slider)
        {
            this.slider = slider;
            this.dispatcher = dispatcher;
            try
            {
                listbox = LIB_Motion;
                myport = new SerialPort();
                myport.BaudRate = baudRate;
                myport.PortName = PortName;
                myport.Open();
                Console.WriteLine("start read");
                myport.DtrEnable = true;﻿
            myport.DataReceived += myport_DataReceived;
            idDict = new Dictionary<long, bool>();
            Debug.Print("SerialPort Start Success");
            }
            catch
            {
                Debug.Print("SerialPort Start Fail");
            }

            t = new Timer(3000);
            t.Elapsed += t_Elapsed;


            DoorState = 0;

        }

        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
                            dispatcher.Invoke(() =>
                    {
            slider.Value = 0;
                    });
        }

        void myport_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data = myport.ReadLine();
            long value;

            if (long.TryParse(data, out value))
            {
                if (!idDict.ContainsKey(value))
                {
                    idDict.Add(value, true);
                    dispatcher.Invoke(() =>
                    {
                        listbox.Items.Add(data.Substring(0,data.Length-2));
                        slider.Value = 1;

                    });
 
                    t.Start();
                    Debug.Print("SerialPortIO new user");

                }
                else
                {
                    idDict.Remove(value);
                    dispatcher.Invoke(() =>
                    {
                        listbox.Items.Remove(data.Substring(0, data.Length - 2));
                        slider.Value = 1;

                    });
                }
            }
            else
            {
                Debug.Print("SerialPortIO error");
            }

        }
        public event PropertyChangedEventHandler PropertyChanged;

        public void AddMember()
        {
            listbox.Items.Add("123");
        }
        private double doorState = 0;
        public double DoorState
        {
            get
            {
                return doorState;
            }
            set
            {
                //if (this.doorState != value)
                {
                    this.doorState = value;
                    if (this.PropertyChanged != null)
                    {
                        Debug.Print("yoi change!!");
                        this.PropertyChanged(this, new PropertyChangedEventArgs("DoorState"));
                    }
                }
            }
        }
    }
}