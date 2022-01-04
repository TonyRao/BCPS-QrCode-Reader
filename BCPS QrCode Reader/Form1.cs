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
        private FilterInfoCollection CaptureDevice;
        private VideoCaptureDevice FinalFrame;
        private static readonly HttpClient client = new HttpClient();
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
            if (FinalFrame.IsRunning == true)
            {
                FinalFrame.Stop();
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            FinalFrame = new VideoCaptureDevice(CaptureDevice[comboBox1.SelectedIndex].MonikerString);
            FinalFrame.NewFrame += new NewFrameEventHandler(FinalFrame_NewFrame);
            FinalFrame.Start();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            timer1.Start();
            Console.WriteLine("Scanner Started");
        }

        private async void timer1_Tick(object sender, EventArgs e)
        {
            BarcodeReader Reader = new BarcodeReader();
            Result result = Reader.Decode((Bitmap)pictureBox1.Image);
            try
            {
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
                    //packs the vlaues
                    var content = new FormUrlEncodedContent(values);
                    //awaits response from server
                    var response = await client.PostAsync("https://broward.focusschoolsoftware.com/focus/mobileApps/checkIn/index.php", content);
                    //Converts data to Json Object
                    JObject data = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                    //Displayes the output message
                    MessageBox.Show($"{decoded} : {data.SelectToken("m").ToString()}");
                    //Form2 form = new Form2();
                    //form.Show();
                    //this.Hide();

                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
