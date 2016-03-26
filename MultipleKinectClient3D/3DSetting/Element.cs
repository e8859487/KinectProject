// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Element.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   Represents an element.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MultipleKinectClient3D
{
    using System.ComponentModel;
    using System.Windows.Media;
    using System.Windows.Media.Media3D;

    /// <summary>
    /// Represents an element.
    /// </summary>
    public class Element : System.ComponentModel.INotifyPropertyChanged
    {

        private Point3D _Point1;
        /// <summary>
        /// Gets or sets the position point1 for pipe.
        /// </summary>
        public Point3D Point1 { 
            get {
                return _Point1;
            }
            set {
                _Point1 = value;
                RaisePropertyChanged("Point1");
            }
        }
        /// <summary>
        /// Gets or sets the position point2 for pipe.
        /// </summary>
        private Point3D _Point2;
        public Point3D Point2
        {
            get
            {
                return _Point2;
            }
            set
            {
                _Point2 = value;
                RaisePropertyChanged("Point2");
            }
        }

        private Point3D _position;

        /// <summary>
        /// Gets or sets the position for sphere3d.
        /// </summary>
        public Point3D Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
                RaisePropertyChanged("Position");
            }
        }

        /// <summary>
        /// Gets or sets the radius.
        /// </summary>
        public double Radius { get; set; }

        /// <summary>
        /// Gets or sets the Brush
        /// </summary>
        public Brush Brushes { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string property)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}