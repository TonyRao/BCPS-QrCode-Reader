using System;
using System.Windows.Forms;

namespace BCPS_QrCode_Reader
{
    public partial class Box : Form
    {
        public Box()
        {
            InitializeComponent();
        }
        private void Box_Load(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            timer1.Start();
        }
        public string TextboxValue
        {
            get { return textBox1.Text; }
            set { textBox1.Text = value; }
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
