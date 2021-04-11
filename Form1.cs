using System;
using System.Drawing;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using Emgu.CV;
using Emgu.CV.Structure;

namespace WebcamFaceDetection
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        FilterInfoCollection filter;
        VideoCaptureDevice device;
        //Training data from opencv;https://github.com/opencv/opencv/blob/master/data/haarcascades/haarcascade_frontalface_alt_tree.xml
        static readonly CascadeClassifier cascadeClassifier = new CascadeClassifier("haarcascade_frontalface_alt_tree.xml");
        private void Form1_Load(object sender, EventArgs e)
        {
            filter = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo device in filter)
                cboDevice.Items.Add(device.Name);
            cboDevice.SelectedIndex = 0;
            device = new VideoCaptureDevice();
        }

        private void btnDetect_Click(object sender, EventArgs e)
        {
            device = new VideoCaptureDevice(filter[cboDevice.SelectedIndex].MonikerString);
            device.NewFrame += Device_NewFrame;
            device.Start();
        }

        
        /*
         * Summary:
         *  When detect button is clicked a new frame will generate from the video source.
         *  We'll use the cascade classifier to detect multiscale image,
         *  Then use a pen to draw a red box over the face.
         */
        private void Device_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
            Image<Bgr, byte> grayImage = new Image<Bgr, byte>(bitmap); //defining an image with Bgr color type and depth of byte
            Rectangle[] rectangles = cascadeClassifier.DetectMultiScale(grayImage, 1.2, 1);//image,scalefactor, minNeighbors
            foreach (Rectangle rectangle in rectangles)
            {
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    using (Pen pen = new Pen(Color.Red, 1))
                    {
                        graphics.DrawRectangle(pen, rectangle);
                    }
                }
            }
            pic.Image = bitmap;
        }

        /*
         * Summary:
         *  When the form closes, stop the device
         */
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (device.IsRunning)
                device.Stop();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var savedPic = pic.Image;
            var saveFile = new SaveFileDialog();
            saveFile.Title = "Save your photo";
            if(saveFile.ShowDialog() == DialogResult.OK)
            {
                savedPic.Save(saveFile.FileName + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                MessageBox.Show("Image saved");
            }
        }
    }
}
