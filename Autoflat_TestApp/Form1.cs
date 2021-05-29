using System;
using System.Windows.Forms;

namespace ASCOM.scopefocus_AF_Maestro
{
    public partial class Form1 : Form
    {

        private ASCOM.DriverAccess.CoverCalibrator driver;

        public Form1()
        {
            InitializeComponent();
            SetUIState();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsConnected)
                driver.Connected = false;

            Properties.Settings.Default.Save();
        }

        private void buttonChoose_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.DriverId = ASCOM.DriverAccess.CoverCalibrator.Choose(Properties.Settings.Default.DriverId);
            SetUIState();
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                driver.Connected = false;
            }
            else
            {
                driver = new ASCOM.DriverAccess.CoverCalibrator(Properties.Settings.Default.DriverId);
                driver.Connected = true;
            }
            SetUIState();
            timer1.Start();
        }

        private void SetUIState()
        {
            buttonConnect.Enabled = !string.IsNullOrEmpty(Properties.Settings.Default.DriverId);
            buttonChoose.Enabled = !IsConnected;
            buttonConnect.Text = IsConnected ? "Disconnect" : "Connect";
        }
        private void SetStatus()
        {
            if (IsConnected)
            {
                System.Threading.Thread.Sleep(100);

                //      bool on = driver.GetSwitch(id);
                //   bool on = true;
                //      textBoxAngle.Text = Convert.ToInt16(driver.GetSwitchValue(id)).ToString();//conver to int first to round
                textBoxAngle.Text = driver.Brightness.ToString();

                textBoxStatus.Text = driver.CoverState.ToString();
                textBox1.Text = driver.CalibratorState.ToString();
            //    trackBar1.Value = driver.Brightness;
                //if (on)
                //    textBoxStatus.Text = "On";
                //else
                //    textBoxStatus.Text = "Off";
                //     Thread.Sleep(200);
            }
            else
                return;
        }
        private bool IsConnected
        {
            get
            {
                return ((this.driver != null) && (driver.Connected == true));
            }
        }

        private void trackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            //if (driver.CoverState.ToString() == "Open")
            //{
            //    MessageBox.Show("Cover must be closed to adjust brightness");
            //    return;
            //}

            //else
            //{
            //    driver.CalibratorOn(trackBar1.Value);
            //}
        }

        private void button1_Click(object sender, EventArgs e)
        {
            driver.CloseCover();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            driver.OpenCover();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MessageBox.Show(PublishVersion.ToString());
        }
        public string PublishVersion
        {
            get
            {
                if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
                {
                    Version ver = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion;
                    return string.Format("{0}.{1}.{2}.{3}", ver.Major, ver.Minor, ver.Build, ver.Revision);
                }
                else
                    return "Not Published";
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            SetStatus();
        }

       
        // cant update status while changing  value
       

       

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (driver.CoverState.ToString() == "Open")
            {
                MessageBox.Show("Cover must be closed to adjust brightness");
         
                return;
            }

            else
            {
                if (((int)numericUpDown1.Value >= 0) || ((int)numericUpDown1.Value > driver.MaxBrightness))
                    driver.CalibratorOn((int)numericUpDown1.Value);
                else
                {
                    MessageBox.Show("Brightness out of Range");
                    numericUpDown1.Value = 0;

                }
            }
        }
    }
    
}
