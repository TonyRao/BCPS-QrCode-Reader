using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZXing;
using System.Net.Http;
using Newtonsoft.Json;
using System.Json;
using Newtonsoft.Json.Linq;

namespace BCPS_QrCode_Reader
{
    public partial class Form1 : Form
    {
        Image pic;
        private FilterInfoCollection CaptureDevice;
        private VideoCaptureDevice FinalFrame;
        private static readonly HttpClient client = new HttpClient();
        bool ToggleState = false;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CaptureDevice = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo Device in CaptureDevice)
            {
                comboBox1.Items.Add(Device.Name);
            }

            comboBox1.SelectedIndex = 0;
            FinalFrame = new VideoCaptureDevice();
        }
        private void FinalFrame_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            pictureBox1.Image = (Bitmap)eventArgs.Frame.Clone();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            killCam();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Toggler();
        }

        private async void timer1_Tick(object sender, EventArgs e)
        {
            //defines barcode
            BarcodeReader Reader = new BarcodeReader();
            //
            pic = pictureBox1.Image;
            if(pic == null){return;}
            Result result;
            try
            {
                result = Reader.Decode((Bitmap)pic);
            }
            catch
            {
                return;
            }
            if (result == null) { return; }
            string decoded = result.ToString().Trim();
            Console.WriteLine(decoded);
            if (decoded != "")
            {
                //Stops Timer
                timer1.Stop();
                //Values to post to api
                var values = new Dictionary<string, string>
                {
                    { "f", "gSI" },
                    { "s", decoded }
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
                    return;
                }
                //Converts data to Json Object
                JObject data = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                //Displays the output message
                MessageBox.Show($"{decoded} : {data.SelectToken("m").ToString()}");
                timer1.Start();
            }
        }
        //kills camera
        private void killCam()
        {
            if (FinalFrame.IsRunning == true)
            {
                FinalFrame.Stop();
                button1.Text = "Start";
                pictureBox1.Image  = null;
            }
        }
        //toggles on and off
        private void Toggler()
        {
            //on
            if (ToggleState == false)
            {
                comboBox1.Enabled = false;
                FinalFrame = new VideoCaptureDevice(CaptureDevice[comboBox1.SelectedIndex].MonikerString);
                FinalFrame.NewFrame += new NewFrameEventHandler(FinalFrame_NewFrame);
                FinalFrame.Start();
                timer1.Enabled = true;
                timer1.Start();
                Console.WriteLine("Scanner Started");
                button1.Text = "Stop";
                ToggleState = true;
            }
            else //off
            {
                comboBox1.Enabled = true;
                killCam();
                button1.Text = "Start";
                ToggleState = false;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
