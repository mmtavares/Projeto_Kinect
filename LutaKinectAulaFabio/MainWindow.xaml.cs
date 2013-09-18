using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Media;
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
using Coding4Fun.Kinect.Wpf;
using Microsoft.Samples.Kinect.WpfViewers;
using Microsoft.Kinect;

namespace LutaKinectAulaFabio
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        
        bool closing = false;
        bool PontoInicial = false;
        const int Contador = 6;
        Skeleton[] TodosEsqueletos = new Skeleton[Contador];
        KinectSensor SensorKinect;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            kinectSensorChooser1.KinectSensorChanged += new DependencyPropertyChangedEventHandler(kinectSensorChooser1_KinectSensorChanged);
        }
        
        void kinectSensorChooser1_KinectSensorChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            KinectSensor old = (KinectSensor)e.OldValue;
            StopKinect(old);
            KinectSensor sensor = (KinectSensor)e.NewValue;
            if (sensor == null)
            {
                return;
            }

            sensor.SkeletonStream.Enable();

            sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(sensor_AllFramesReady);
            sensor.DepthStream.Enable();
            sensor.ColorStream.Enable();
            try
            {
                sensor.Start();
                SensorKinect = sensor;
            }
            catch (System.IO.IOException)
            {
                kinectSensorChooser1.AppConflictOccurred();
            }
        }
        
        void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            if (closing)
            {
                return;
            }
            Skeleton first = GetFirstSkeleton(true, e);
            if (!PontoInicial && first != null)
            {
                SoundPlayer simpleSound = new SoundPlayer(@"C:/Users/Marciano/Downloads/ProjetoAulaFabio/LutaKinectAulaFabio/EyeTiger.wav");
                simpleSound.Play();
                PontoInicial = true;
            }
            if (first == null)
            {
                PontoInicial = false;
                return;
            }
            GetCameraPoint(first, true, e);
            using (SkeletonFrame skeletonFrameData = e.OpenSkeletonFrame())
            {
                try
                {
                    (from s in TodosEsqueletos where s.TrackingState == SkeletonTrackingState.Tracked select s).ElementAt(1);
                }
                catch (ArgumentOutOfRangeException)
                {
                    return;
                }
                Skeleton second = GetSecondSkeleton(false, e);
                GetCameraPoint2(second, false, e);
            }
            
            PlayerOneHit(P1Lhand, P2Chest);
            PlayerOneHit(P1Rhand, P2Chest);
            PlayerOneHit(P1Lhand, P2Head);
            PlayerOneHit(P1Rhand, P2Head);
            PlayerTwoHit(P2Lhand, P1Chest);
            PlayerTwoHit(P2Rhand, P1Chest);
            PlayerTwoHit(P2Lhand, P1Head);
            PlayerTwoHit(P2Rhand, P1Head);
            winner();
        }

        private void winner()
        {
            
            if (P1Health.Value.Equals(0))
            {
                p2wins.Visibility = Visibility.Visible;
                SoundPlayer simpleSound = new SoundPlayer(@"C:/Users/Marciano/Downloads/ProjetoAulaFabio/LutaKinectAulaFabio/EyeTiger.wav");
                simpleSound.Play();
                closing = true;
            }
            if (P2Health.Value.Equals(0))
            {
                p1wins.Visibility = Visibility.Visible;
                SoundPlayer simpleSound = new SoundPlayer(@"C:/Users/Marciano/Downloads/ProjetoAulaFabio/LutaKinectAulaFabio/EyeTiger.wav");
                simpleSound.Play();
                closing = true;
            }
        }
        
        void PlayerOneHit(Image a, Image b)
        {
            double P1x = Canvas.GetLeft(a) + a.Width;
            double P1y = Canvas.GetTop(a) + a.Height;
            double P2x = Canvas.GetLeft(b) + b.Width;
            double P2y = Canvas.GetTop(b) + b.Height;
            double raio = a.Height + b.Height;
            double distancia = Math.Sqrt(Math.Pow(P1x - P2x, 2) + Math.Pow(P1y - P2y, 2));
            if (distancia < raio)
            {   
                P2Health.Value -= .2;
                SoundPlayer hitSound = new SoundPlayer(@"C:/Users/Marciano/Downloads/ProjetoAulaFabio/LutaKinectAulaFabio/Hit.wav");
                hitSound.Play();
            }
        }
        
        void PlayerTwoHit(Image c, Image d)
        {
            double P2x = Canvas.GetLeft(c) + c.Width;
            double P2y = Canvas.GetTop(c) + c.Height;
            double P1x = Canvas.GetLeft(d) + d.Width;
            double P1y = Canvas.GetTop(d) + d.Height;
            double raio = c.Height + d.Height;
            double distancia = Math.Sqrt(Math.Pow(P2x - P1x, 2) + Math.Pow(P2y - P1y, 2));
            if (distancia < raio)
            {
                P1Health.Value -= .2;
                SoundPlayer hitSound = new SoundPlayer(@"C:/Users/Marciano/Downloads/ProjetoAulaFabio/LutaKinectAulaFabio/Hit.wav");
                hitSound.Play();
            }
        }

        void GetCameraPoint(Skeleton first, bool PontoInicial, AllFramesReadyEventArgs e)
        {
            using (DepthImageFrame depth = e.OpenDepthImageFrame())
            {
                if (depth == null || kinectSensorChooser1.Kinect == null)
                {
                    return;
                }
                DepthImagePoint headDepthPointP1 = SensorKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.Head].Position, DepthImageFormat.Resolution320x240Fps30);
                DepthImagePoint leftWristDepthPointP1 = SensorKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.WristLeft].Position, DepthImageFormat.Resolution320x240Fps30);
                DepthImagePoint rightWristDepthPointP1 = SensorKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.WristRight].Position, DepthImageFormat.Resolution320x240Fps30);
                DepthImagePoint leftElbowDepthPointP1 = SensorKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.ElbowLeft].Position, DepthImageFormat.Resolution320x240Fps30);
                DepthImagePoint rightElbowDepthPointP1 = SensorKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.ElbowRight].Position, DepthImageFormat.Resolution320x240Fps30);
                DepthImagePoint leftHandDepthPointP1 = SensorKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.HandLeft].Position, DepthImageFormat.Resolution320x240Fps30);
                DepthImagePoint rightHandDepthPointP1 = SensorKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.HandRight].Position, DepthImageFormat.Resolution320x240Fps30);
                DepthImagePoint leftHipDepthPointP1 = SensorKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.HipLeft].Position, DepthImageFormat.Resolution320x240Fps30);
                DepthImagePoint rightHipDepthPointP1 = SensorKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.HipRight].Position, DepthImageFormat.Resolution320x240Fps30);
                DepthImagePoint leftShoulderDepthPointP1 = SensorKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.ShoulderLeft].Position, DepthImageFormat.Resolution320x240Fps30);
                DepthImagePoint rightShoulderDepthPointP1 = SensorKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.ShoulderRight].Position, DepthImageFormat.Resolution320x240Fps30);
                DepthImagePoint leftKneeDepthPointP1 = SensorKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.KneeLeft].Position, DepthImageFormat.Resolution320x240Fps30);
                DepthImagePoint rightKneeDepthPointP1 = SensorKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.KneeRight].Position, DepthImageFormat.Resolution320x240Fps30);
                DepthImagePoint leftAnkleDepthPointP1 = SensorKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.AnkleLeft].Position, DepthImageFormat.Resolution320x240Fps30);
                DepthImagePoint rightAnkleDepthPointP1 = SensorKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.AnkleRight].Position, DepthImageFormat.Resolution320x240Fps30);
                DepthImagePoint midDepthPointP1 = SensorKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.Spine].Position, DepthImageFormat.Resolution320x240Fps30);

                ColorImagePoint headColorPointP1 = SensorKinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, headDepthPointP1, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint leftHandColorPointP1 = SensorKinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, leftHandDepthPointP1, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint rightHandColorPointP1 = SensorKinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, rightHandDepthPointP1, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint leftElbowColorPointP1 = SensorKinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, leftElbowDepthPointP1, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint rightElbowColorPointP1 = SensorKinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, rightElbowDepthPointP1, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint leftWristColorPointP1 = SensorKinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, leftWristDepthPointP1, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint rightWristColorPointP1 = SensorKinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, rightWristDepthPointP1, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint leftHipColorPointP1 = SensorKinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, leftHipDepthPointP1, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint rightHipColorPointP1 = SensorKinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, rightHipDepthPointP1, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint leftShoulderColorPointP1 = SensorKinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, leftShoulderDepthPointP1, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint rightShoulderColorPointP1 = SensorKinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, rightShoulderDepthPointP1, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint leftKneeColorPointP1 = SensorKinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, leftKneeDepthPointP1, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint rightKneeColorPointP1 = SensorKinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, rightKneeDepthPointP1, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint leftAnkleColorPointP1 = SensorKinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, leftAnkleDepthPointP1, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint rightAnkleColorPointP1 = SensorKinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, rightAnkleDepthPointP1, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint midColorPointP1 = SensorKinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, midDepthPointP1, ColorImageFormat.RgbResolution640x480Fps30);
                CameraPosition(P1Head, headColorPointP1);
                CameraPosition(P1Rhand, rightHandColorPointP1);
                CameraPosition(P1Lhand, leftHandColorPointP1);
                CameraPosition(P1Chest, midColorPointP1);
                CameraPosition(P1Rfoot, rightAnkleColorPointP1);
                CameraPosition(P1Lfoot, leftAnkleColorPointP1);
            }
        }
        
        Skeleton GetFirstSkeleton(bool PontoInicial, AllFramesReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrameData = e.OpenSkeletonFrame())
            {
                if (skeletonFrameData == null)
                {
                    return null;
                }
                skeletonFrameData.CopySkeletonDataTo(TodosEsqueletos);
                Skeleton first = (from s in TodosEsqueletos where s.TrackingState == SkeletonTrackingState.Tracked select s).FirstOrDefault();
                return first;
            }
        }
        
        void GetCameraPoint2(Skeleton second, bool PontoInicial, AllFramesReadyEventArgs e)
        {
            using (DepthImageFrame depth = e.OpenDepthImageFrame())
            {
                if (depth == null ||
                    kinectSensorChooser1.Kinect == null)
                {
                    return;
                }
                DepthImagePoint headDepthPointP2 = SensorKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(second.Joints[JointType.Head].Position, DepthImageFormat.Resolution320x240Fps30);
                DepthImagePoint leftWristDepthPointP2 = SensorKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(second.Joints[JointType.WristLeft].Position, DepthImageFormat.Resolution320x240Fps30);
                DepthImagePoint rightWristDepthPointP2 = SensorKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(second.Joints[JointType.WristRight].Position, DepthImageFormat.Resolution320x240Fps30);
                DepthImagePoint leftElbowDepthPointP2 = SensorKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(second.Joints[JointType.ElbowLeft].Position, DepthImageFormat.Resolution320x240Fps30);
                DepthImagePoint rightElbowDepthPointP2 = SensorKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(second.Joints[JointType.ElbowRight].Position, DepthImageFormat.Resolution320x240Fps30);
                DepthImagePoint leftHandDepthPointP2 = SensorKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(second.Joints[JointType.HandLeft].Position, DepthImageFormat.Resolution320x240Fps30);
                DepthImagePoint rightHandDepthPointP2 = SensorKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(second.Joints[JointType.HandRight].Position, DepthImageFormat.Resolution320x240Fps30);
                DepthImagePoint leftHipDepthPointP2 = SensorKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(second.Joints[JointType.HipLeft].Position, DepthImageFormat.Resolution320x240Fps30);
                DepthImagePoint rightHipDepthPointP2 = SensorKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(second.Joints[JointType.HipRight].Position, DepthImageFormat.Resolution320x240Fps30);
                DepthImagePoint leftShoulderDepthPointP2 = SensorKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(second.Joints[JointType.ShoulderLeft].Position, DepthImageFormat.Resolution320x240Fps30);
                DepthImagePoint rightShoulderDepthPointP2 = SensorKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(second.Joints[JointType.ShoulderRight].Position, DepthImageFormat.Resolution320x240Fps30);
                DepthImagePoint leftKneeDepthPointP2 = SensorKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(second.Joints[JointType.KneeLeft].Position, DepthImageFormat.Resolution320x240Fps30);
                DepthImagePoint rightKneeDepthPointP2 = SensorKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(second.Joints[JointType.KneeRight].Position, DepthImageFormat.Resolution320x240Fps30);
                DepthImagePoint leftAnkleDepthPointP2 = SensorKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(second.Joints[JointType.AnkleLeft].Position, DepthImageFormat.Resolution320x240Fps30);
                DepthImagePoint rightAnkleDepthPointP2 = SensorKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(second.Joints[JointType.AnkleRight].Position, DepthImageFormat.Resolution320x240Fps30);
                DepthImagePoint midDepthPointP2 = SensorKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(second.Joints[JointType.Spine].Position, DepthImageFormat.Resolution320x240Fps30);

                ColorImagePoint headColorPointP2 = SensorKinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, headDepthPointP2, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint leftHandColorPointP2 = SensorKinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, leftHandDepthPointP2, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint rightHandColorPointP2 = SensorKinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, rightHandDepthPointP2, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint leftElbowColorPointP2 = SensorKinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, leftElbowDepthPointP2, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint rightElbowColorPointP2 = SensorKinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, rightElbowDepthPointP2, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint leftWristColorPointP2 = SensorKinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, leftWristDepthPointP2, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint rightWristColorPointP2 = SensorKinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, rightWristDepthPointP2, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint leftHipColorPointP2 = SensorKinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, leftHipDepthPointP2, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint rightHipColorPointP2 = SensorKinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, rightHipDepthPointP2, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint leftShoulderColorPointP2 = SensorKinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, leftShoulderDepthPointP2, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint rightShoulderColorPointP2 = SensorKinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, rightShoulderDepthPointP2, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint leftKneeColorPointP2 = SensorKinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, leftKneeDepthPointP2, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint rightKneeColorPointP2 = SensorKinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, rightKneeDepthPointP2, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint leftAnkleColorPointP2 = SensorKinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, leftAnkleDepthPointP2, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint rightAnkleColorPointP2 = SensorKinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, rightAnkleDepthPointP2, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint midColorPointP2 = SensorKinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, midDepthPointP2, ColorImageFormat.RgbResolution640x480Fps30);
                CameraPosition2(P2Head, headColorPointP2);
                CameraPosition2(P2Rhand, rightHandColorPointP2);
                CameraPosition2(P2Lhand, leftHandColorPointP2);
                CameraPosition2(P2Chest, midColorPointP2);
                CameraPosition2(P2Rfoot, rightAnkleColorPointP2);
                CameraPosition2(P2Lfoot, leftAnkleColorPointP2);
            }
        }
        
        Skeleton GetSecondSkeleton(bool PontoInicial, AllFramesReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrameData = e.OpenSkeletonFrame())
            {
                if (skeletonFrameData == null)
                {
                    return null;
                }
                skeletonFrameData.CopySkeletonDataTo(TodosEsqueletos);
                Skeleton second = (from s in TodosEsqueletos where s.TrackingState == SkeletonTrackingState.Tracked select s).LastOrDefault();
                if (!PontoInicial)
                {
                    second = (from s in TodosEsqueletos where s.TrackingState == SkeletonTrackingState.Tracked select s).ElementAt(1);
                }
                return second;
            }
        }
        
        private void StopKinect(KinectSensor sensor)
        {
            if (sensor != null)
            {
                if (sensor.IsRunning)
                {
                    sensor.Stop();
                }
            }
        }
        
        private void CameraPosition(FrameworkElement element, ColorImagePoint point)
        {
            Canvas.SetLeft(element, point.X - element.Width / 2);
            Canvas.SetTop(element, point.Y - element.Height / 2);
        }
        
        private void CameraPosition2(FrameworkElement element, ColorImagePoint point)
        {
            Canvas.SetLeft(element, point.X - element.Width / 2);
            Canvas.SetTop(element, point.Y - element.Height / 2);
        }
                
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            closing = true;
            StopKinect(kinectSensorChooser1.Kinect);
        }
    }
}
