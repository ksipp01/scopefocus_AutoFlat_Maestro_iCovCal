using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ASCOM.Utilities;
using System.IO.Ports;
using System.Threading;

namespace ASCOM.scopefocus_AF_Maestro
{
    public partial class AutoFlatControl : Form
    {
        internal static string driverID = "ASCOM.scopefocus_AF_Maestro.CoverCalibrator";
        private Serial serialPort;

        private static int flapServo;
        private static int levelServo;
        private double flatPos;
        private double flatLevel;
        private string comPort;
        public string ComPort
        {
            get { return this.comPort; }
            
        }
        public AutoFlatControl()
        {

           
            InitializeComponent();
           
           // string com;
            /*
            using (ASCOM.Utilities.Profile p = new Utilities.Profile())
            {
                p.DeviceType = "Switch";
                com = p.GetValue(Switch.driverID, "ComPort");
            //    string openAngle = p.GetValue(Switch.driverID, "OpenAngle");
              //  string closedAngle = p.GetValue(Switch.driverID, "ClosedAngle");
            
                /*
                if (openAngle != "")
                    numericUpDown1.Value = Convert.ToInt16(openAngle);
                if (closedAngle != "")
                    numericUpDown2.Value = Convert.ToInt16(closedAngle);
                 * */
         //   }
            
            comboBox1.Items.Clear();
            using (ASCOM.Utilities.Serial serial = new Utilities.Serial())
            {
                foreach (var item in serial.AvailableCOMPorts)
                {
                    comboBox1.Items.Add(item);
                 //   if (item == com)
                 //   comboBox1.SelectedItem = item;
            
                }
            }
            
        }

        private void button3_Click(object sender, EventArgs e)// works if first connect to driver then go back and use control app
        {
            if (button3.Text == "Disconnect")
            {
                serialPort.Dispose();
                button3.Text = "Connect";
                button3.BackColor = System.Drawing.Color.WhiteSmoke;
                return;
            }
            else
            {
                /*
                string portName;
                using (ASCOM.Utilities.Profile p = new Profile())
                {

                    p.DeviceType = "Switch";
                    portName = p.GetValue(driverID, "ComPort");

                    if (string.IsNullOrEmpty(portName))
                    {
                        
                    //    SerialConnection = new ArduinoSerial(StopBits.One, 9600, comboBox1.SelectedItem.ToString(), true);

                     //   SerialConnection.CommandQueueReady += new ArduinoSerial.CommandQueueReadyEventHandler(SerialConnection_CommandQueueReady);

                        
                        p.DeviceType = "Switch";
                        p.WriteValue(Switch.driverID, "ComPort", (string)comboBox1.SelectedItem);
                        portName = (string)comboBox1.SelectedItem;
                    }
                    // report a problem with the port name
                    //  throw new ASCOM.NotConnectedException("no Com port selected");
                }
                // try to connect using the port
        */
                try
                {
                    serialPort = new Serial();
                  //  serialPort.PortName = portName;
                    serialPort.PortName = comPort;
                    serialPort.Speed = SerialSpeed.ps9600;
                    serialPort.Connected = true;
                    Thread.Sleep(250);
                   // SerialConnection = new ArduinoSerial();
                  //  SerialConnection.CommandQueueReady += new ArduinoSerial.CommandQueueReadyEventHandler(SerialConnection_CommandQueueReady);
              //      GetAngle();
                    button3.BackColor = System.Drawing.Color.Lime;
                    button3.Text = "Disconnect";
                 //   Thread.Sleep(500);
                 //   GetAngle();
                }


                catch (Exception ex)
                {
                    // report any error
                    throw new ASCOM.NotConnectedException("Serial port connection error", ex);
                }
            }           
    }

        private void button1_Click(object sender, EventArgs e)
        {
          //  SerialConnection.SendCommand(ArduinoSerial.SerialCommand.FlatToggle, flatPos + 1);
            serialPort.Transmit("T " + Convert.ToString(flatPos + 1) + "\n");
            GetAngle();
        }
       private void GetAngle()
       {
            Thread.Sleep(100);
            serialPort.ClearBuffers();
            //   serialPort.Transmit(ArduinoSerial.SerialCommand.Position);
            serialPort.Transmit("G\n");
            // serialPort.ClearBuffers();
           // serialPort.ClearBuffers();

            string p = "";
            p = serialPort.ReceiveTerminated("#");

            // flatPos = int.Parse(com_args[1]);
            //   flatPos = int.Parse(p.Trim("\r".ToCharArray()));
            flatPos = Convert.ToDouble(p.Replace("#", ""));
         //  SerialConnection.SendCommand(ArduinoSerial.SerialCommand.Position);//this is one behind
        //   Thread.Sleep(100);
           textBox1.Text = flatPos.ToString();
        }
       private void GetLevel()
       {
           Thread.Sleep(100);
           serialPort.ClearBuffers();
           //   serialPort.Transmit(ArduinoSerial.SerialCommand.Position);
           serialPort.Transmit("B\n");
           // serialPort.ClearBuffers();
           // serialPort.ClearBuffers();

           string p = "";
           p = serialPort.ReceiveTerminated("#");

           // flatPos = int.Parse(com_args[1]);
           //   flatPos = int.Parse(p.Trim("\r".ToCharArray()));
           flatLevel = Convert.ToDouble(p.Replace("#", ""));
           //  SerialConnection.SendCommand(ArduinoSerial.SerialCommand.Position);//this is one behind
           //   Thread.Sleep(100);
           textBox2.Text = flatLevel.ToString();
       }

       private void SetLevel(int level)
       {
           Thread.Sleep(100);
           serialPort.ClearBuffers();
           serialPort.Transmit("L " + level.ToString() + "\n");
       }
 private void buttonNeg_Click(object sender, EventArgs e)
 {
    // SerialConnection.SendCommand(ArduinoSerial.SerialCommand.FlatToggle, flatPos - 1); 
     serialPort.Transmit("T " + Convert.ToString(flatPos - 1) + "\n");
     GetAngle();
 }

 private void button1_Click_1(object sender, EventArgs e)
 {
     //SerialConnection.SendCommand(ArduinoSerial.SerialCommand.FlatToggle, flatPos + 10);
     serialPort.Transmit("T " + Convert.ToString(flatPos + 10) + "\n");
     GetAngle();
 }

 private void button2_Click(object sender, EventArgs e)
 {
    // SerialConnection.SendCommand(ArduinoSerial.SerialCommand.FlatToggle, flatPos - 10);
     serialPort.Transmit("T " + Convert.ToString(flatPos - 10) + "\n");
     GetAngle();
 }

 private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
 {
     comPort = comboBox1.SelectedItem.ToString();
 }

 private void trackBar1_Scroll(object sender, EventArgs e)
 {
    // SetLevel(trackBar1.Value);
 }

 private void trackBar1_Leave(object sender, EventArgs e)
 {
    //
 }

 private void trackBar1_MouseUp(object sender, MouseEventArgs e)
 {
     SetLevel(trackBar1.Value);
     Thread.Sleep(500);
     GetLevel();
 }


    }
}
