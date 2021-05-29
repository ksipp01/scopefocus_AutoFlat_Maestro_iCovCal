using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using ASCOM.Utilities;
using ASCOM.scopefocus_AF_Maestro;

namespace ASCOM.scopefocus_AF_Maestro
{
    [ComVisible(false)]					// Form not registered for COM!
    public partial class SetupDialogForm : Form
    {
        TraceLogger tl; // Holder for a reference to the driver's trace logger
        static int flatPos;
        static string com;
        public SetupDialogForm(TraceLogger tlDriver)
        {
            InitializeComponent();

            // Save the provided trace logger for use within the setup dialogue
            tl = tlDriver;

            // Initialise current values of user settings from the ASCOM Profile
            InitUI();

            using (ASCOM.Utilities.Profile p = new Utilities.Profile())
            {
                p.DeviceType = "CoverCalibrator";
                com = p.GetValue(CoverCalibrator.driverID, "ComPort");
                //  openAngleTextBox.Text = p.GetValue(Switch.driverID, "OpenAngle");
                //  closedAngleTextBox.Text = p.GetValue(Switch.driverID, "ClosedAngle");
                string openAngle = p.GetValue(CoverCalibrator.driverID, "OpenAngle");
                string closedAngle = p.GetValue(CoverCalibrator.driverID, "ClosedAngle");
                string coverSettle = p.GetValue(CoverCalibrator.driverID, "CoverSettle");
                string calibSettle = p.GetValue(CoverCalibrator.driverID, "CalibSettle");
                string maxBrightnessValue = p.GetValue(CoverCalibrator.driverID, "MaxBrightnessValue");
                if (openAngle != "")
                    numericUpDown1.Value = Convert.ToInt16(openAngle);
                if (closedAngle != "")
                    numericUpDown2.Value = Convert.ToInt16(closedAngle);
                string flapServo = p.GetValue(CoverCalibrator.driverID, "flapServo");
                if (flapServo != "")
                    numericUpDown3.Value = Convert.ToInt16(flapServo);
                string levelServo = p.GetValue(CoverCalibrator.driverID, "levelServo");
                if (levelServo != "")
                    numericUpDown4.Value = Convert.ToInt16(levelServo);
                if (coverSettle != "")
                    numericUpDown5.Value = Convert.ToInt16(coverSettle);
                if (calibSettle != "")
                    numericUpDown6.Value = Convert.ToInt16(calibSettle);
                if (maxBrightnessValue != "")
                    numericUpDown7.Value = Convert.ToInt16(maxBrightnessValue);
                //string Id = p.GetValue(CoverCalibrator.driverID, "Id");
                //if (Id != "")
                //    numericUpDown3.Value = Convert.ToInt16(Id);


            }

            comboBoxComPort.Items.Clear();
            using (ASCOM.Utilities.Serial serial = new Utilities.Serial())

            {
                foreach (var item in serial.AvailableCOMPorts)
                {
                    comboBoxComPort.Items.Add(item);
                    if (item == com)
                        comboBoxComPort.SelectedItem = item;

                }
            }



        }

        private void cmdOK_Click(object sender, EventArgs e) // OK button event handler
        {
            // Place any validation constraint checks here
            // Update the state variables with results from the dialogue
            CoverCalibrator.comPort = (string)comboBoxComPort.SelectedItem;
            tl.Enabled = chkTrace.Checked;

            // added
            using (ASCOM.Utilities.Profile p = new Utilities.Profile())
            {
                p.DeviceType = "CoverCalibrator";
                p.WriteValue(CoverCalibrator.driverID, "ComPort", (string)comboBoxComPort.SelectedItem);
                p.WriteValue(CoverCalibrator.driverID, "OpenAngle", numericUpDown1.Value.ToString());
                p.WriteValue(CoverCalibrator.driverID, "ClosedAngle", numericUpDown2.Value.ToString());
                p.WriteValue(CoverCalibrator.driverID, "flapServo", numericUpDown3.Value.ToString());
                p.WriteValue(CoverCalibrator.driverID, "levelServo", numericUpDown4.Value.ToString());
                p.WriteValue(CoverCalibrator.driverID, "CoverSettle", numericUpDown5.Value.ToString());
                p.WriteValue(CoverCalibrator.driverID, "CalibSettle", numericUpDown6.Value.ToString());
                p.WriteValue(CoverCalibrator.driverID, "MaxBrightnessValue", numericUpDown7.Value.ToString());
                
                //    p.WriteValue(Switch.driverID, "Id", numericUpDown3.Value.ToString()); //***** added 3-7-14 may not be necessary.************
                // p.WriteValue(Switch.driverID, "Id", numericUpDown3.Value.ToString());

            }
            Dispose();

        }

        private void cmdCancel_Click(object sender, EventArgs e) // Cancel button event handler
        {
            Close();
        }

        private void BrowseToAscom(object sender, EventArgs e) // Click on ASCOM logo event handler
        {
            try
            {
                System.Diagnostics.Process.Start("https://ascom-standards.org/");
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    MessageBox.Show(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }

        private void InitUI()
        {
            chkTrace.Checked = tl.Enabled;
            // set the list of com ports to those that are currently available
            comboBoxComPort.Items.Clear();
            comboBoxComPort.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());      // use System.IO because it's static
            // select the current port if possible
            if (comboBoxComPort.Items.Contains(CoverCalibrator.comPort))
            {
                comboBoxComPort.SelectedItem = CoverCalibrator.comPort;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AutoFlatControl f1 = new AutoFlatControl();
            f1.Show();
        }
    }
}