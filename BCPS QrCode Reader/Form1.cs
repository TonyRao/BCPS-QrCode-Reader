using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using ZXing;

namespace BCPS_QrCode_Reader
{
    public partial class Form1 : System.Windows.Forms.Form
    {
        Image pic;
        private FilterInfoCollection CaptureDevice;
        private VideoCaptureDevice FinalFrame;
        private static readonly HttpClient client = new HttpClient();
        private Bitmap CamFeed;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //get cameras
            CaptureDevice = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            //push cameras to list
            foreach (FilterInfo Device in CaptureDevice)
            {
                comboBox1.Items.Add(Device.Name);
            }
            comboBox1.SelectedIndex = 0;
            FinalFrame = new VideoCaptureDevice();
            //gets capture devices
            FinalFrame = new VideoCaptureDevice(CaptureDevice[comboBox1.SelectedIndex].MonikerString);
            //displays frame
            FinalFrame.NewFrame += new NewFrameEventHandler(FinalFrame_NewFrame);
            FinalFrame.Start();
            timer1.Enabled = true;
            timer1.Start();
            Console.WriteLine("Scanner Started");
        }
        private void FinalFrame_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            CamFeed = (Bitmap)eventArgs.Frame.Clone();
            //makes picturebox1 = video camera feed
            pictureBox1.Image = CamFeed;
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            killCam();
            Environment.Exit(0);
        }

        private async void timer1_Tick(object sender, EventArgs e)
        {
            //defines barcode
            Bitmap pic = CamFeed;
            if(pic == null){return;}
            Result result;
            try { result = new BarcodeReader().Decode(pic); } catch { return; }
            if (result == null)return;
            if (result.ToString() != "")
            {
                //Stops Timer
                timer1.Stop();
                //Values to post to api
                var values = new Dictionary<string, string>
                {
                    { "f", "gSI" },
                    { "s", result.ToString() }
                };
                //packs the values
                var content = new FormUrlEncodedContent(values);
                //defines variable outside try
                HttpResponseMessage response;
                //awaits response from server
                try
                {
                    response = await client.PostAsync("https://broward.focusschoolsoftware.com/focus/mobileApps/checkIn/index.php", content);
                }
                catch
                {
                    MessageBox.Show("There was an error communicating with api. make sure your connected to the internet.");
                    timer2.Start();
                    return;
                }
                try
                {
                    //Converts data to Json Object
                    JObject data = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                    //Displays the output message
                    Box box = new Box();
                    box.TextboxValue = $"{result} {Environment.NewLine} { data.SelectToken("m")}";
                    box.Show();
                    timer2.Start();
                }
                catch
                {
                    timer2.Start();
                }
            }
        }
        //kills camera
        private void killCam()
        {
            if (FinalFrame == null) { return; }
            if (FinalFrame.IsRunning == true){FinalFrame.SignalToStop();return; }
        }
        private void timer2_Tick(object sender, EventArgs e)
        {
            timer1.Start();
            timer2.Stop();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            timer1.Stop();
            if (FinalFrame != null)
            {
                if (FinalFrame.IsRunning == true) 
                { 
                    FinalFrame.SignalToStop();
                }
            }
            FinalFrame = new VideoCaptureDevice(CaptureDevice[comboBox1.SelectedIndex].MonikerString);
            FinalFrame.NewFrame += new NewFrameEventHandler(FinalFrame_NewFrame);
            FinalFrame.Start();
            timer1.Start();
        }
    }
}
