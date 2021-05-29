//tabs=4
// --------------------------------------------------------------------------------
// TODO fill in this information for your driver, then remove this line!
//
// ASCOM CoverCalibrator driver for scopefocus_AF_Maestro
//
// Description:	Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam 
//				nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam 
//				erat, sed diam voluptua. At vero eos et accusam et justo duo 
//				dolores et ea rebum. Stet clita kasd gubergren, no sea takimata 
//				sanctus est Lorem ipsum dolor sit amet.
//
// Implements:	ASCOM CoverCalibrator interface version: <To be completed by driver developer>
// Author:		(XXX) Your N. Here <your@email.here>
//
// Edit Log:
//
// Date			Who	Vers	Description
// -----------	---	-----	-------------------------------------------------------
// dd-mmm-yyyy	XXX	6.0.0	Initial edit, created from ASCOM driver template
// --------------------------------------------------------------------------------
//


// This is used to define code in the template that is specific to one class implementation
// unused code can be deleted and this definition removed.
#define CoverCalibrator

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ASCOM;
using ASCOM.Astrometry;
using ASCOM.Astrometry.AstroUtils;
using ASCOM.Utilities;
using ASCOM.DeviceInterface;
using System.Globalization;
using System.Collections;

namespace ASCOM.scopefocus_AF_Maestro
{
    //
    // Your driver's DeviceID is ASCOM.scopefocus_AF_Maestro.CoverCalibrator
    //
    // The Guid attribute sets the CLSID for ASCOM.scopefocus_AF_Maestro.CoverCalibrator
    // The ClassInterface/None attribute prevents an empty interface called
    // _scopefocus_AF_Maestro from being created and used as the [default] interface
    //
    // TODO Replace the not implemented exceptions with code to implement the function or
    // throw the appropriate ASCOM exception.
    //

    /// <summary>
    /// ASCOM CoverCalibrator Driver for scopefocus_AF_Maestro.
    /// </summary>
    [Guid("df209d30-437e-45a6-a6c8-d3afa8a7c2bb")]
    [ClassInterface(ClassInterfaceType.None)]
    public class CoverCalibrator : ICoverCalibratorV1
    {
        /// <summary>
        /// ASCOM DeviceID (COM ProgID) for this driver.
        /// The DeviceID is used by ASCOM applications to load the driver at runtime.
        /// </summary>
        internal static string driverID = "ASCOM.scopefocus_AF_Maestro.CoverCalibrator";
        // TODO Change the descriptive string for your driver then remove this line
        /// <summary>
        /// Driver description that displays in the ASCOM Chooser.
        /// </summary>
        private static string driverDescription = "ASCOM CoverCalibrator Driver for scopefocus_AF_Maestro.";

        internal static string comPortProfileName = "COM Port"; // Constants used for Profile persistence
        internal static string comPortDefault = "COM1";
        internal static string traceStateProfileName = "Trace Level";
        internal static string traceStateDefault = "false";

        internal static string comPort; // Variables to hold the current device configuration

        // added
        internal static bool traceState;
        internal static int flatPos;
        internal static int flatOn;
           internal static int OpenAngle;
           internal static int ClosedAngle;
        internal static int flatLevel;
        internal static int flapServo;
        internal static int levelServo;
        private Serial serialPort;

        // end add

        // added from simulator example

        // Private simulator constants
     //   private const string DRIVER_PROGID = "ASCOM.Simulator.CoverCalibrator"; // ASCOM DeviceID (COM ProgID) for this driver.
    //    private const string DRIVER_DESCRIPTION = "ASCOM CoverCalibrator Simulator"; // Driver description that displays in the ASCOM Chooser.
     //   internal const double SYNCHRONOUS_BEHAVIOUR_LIMIT = 0.5; // Threshold (seconds) above which state changes will be handled asynchronously
        // Persistence constants
     //   private const string TRACE_STATE_PROFILE_NAME = "Trace State"; private const bool TRACE_STATE_DEFAULT = false;
    //    private const string MAX_BRIGHTNESS_PROFILE_NAME = "Maximum Brightness"; private const string MAX_BRIGHTNESS_DEFAULT = "100"; // Number of different brightness states
        private const string CALIBRATOR_STABILISATION_TIME_PROFILE_NAME = "Calibrator Stabilisation Time"; private const double CALIBRATOR_STABLISATION_TIME_DEFAULT = 2.0; // Seconds
        private const string CALIBRATOR_INITIALISATION_STATE_PROFILE_NAME = "Calibrator Initialisation State"; private const CalibratorStatus CALIBRATOR_INITIALISATION_STATE_DEFAULT = CalibratorStatus.Off;
        private const string COVER_OPENING_TIME_PROFILE_NAME = "Cover Opening Time"; private const double COVER_OPENING_TIME_DEFAULT = 4.0; // Seconds
        private const string COVER_INITIALISATION_STATE_PROFILE_NAME = "Cover Initialisation State"; private const CoverStatus COVER_INITIALISATION_STATE_DEFAULT = CoverStatus.Closed;

        // Simulator state variables
        private CoverStatus coverState; // The current cover status
        private CalibratorStatus calibratorState; // The current calibrator status
        private int brightnessValue; // The current brightness of the calibrator
        private CoverStatus targetCoverState; // The final cover status at the end of the current asynchronous command
        private CalibratorStatus targetCalibratorStatus; // The final calibrator status at the end of the current asynchronous command

        // User configuration variables
        internal static CalibratorStatus CalibratorStateInitialisationValue;
        internal static CoverStatus CoverStateInitialisationValue;
        //  internal static int MaxBrightnessValue;
        internal static double CoverOpeningTimeValue = 3;
        internal static double CalibratorStablisationTimeValue = 2;


        // ****move to profile settings
        internal static int MaxBrightnessValue = 160;
        internal static bool canDim;




     //   internal static double CoverOpeningTimeValue;
     //   internal static double CalibratorStablisationTimeValue;

        // Simulator components 
     //   private Util utilities; // ASCOM Utilities component
     //   internal TraceLogger TL; // ASCOM Trace Logger component
        private System.Timers.Timer coverTimer;
        private System.Timers.Timer calibratorTimer;

        // end sample add







        /// <summary>
        /// Private variable to hold the connected state
        /// </summary>
        private bool connectedState;

        /// <summary>
        /// Private variable to hold an ASCOM Utilities object
        /// </summary>
        private Util utilities;

        /// <summary>
        /// Private variable to hold an ASCOM AstroUtilities object to provide the Range method
        /// </summary>
        private AstroUtils astroUtilities;

        /// <summary>
        /// Variable to hold the trace logger object (creates a diagnostic log file with information that you specify)
        /// </summary>
        internal TraceLogger tl;

        /// <summary>
        /// Initializes a new instance of the <see cref="scopefocus_AF_Maestro"/> class.
        /// Must be public for COM registration.
        /// </summary>
        public CoverCalibrator()
        {

            try
            {

                tl = new TraceLogger("", "scopefocus_AF_Maestro");
                ReadProfile(); // Read device configuration from the ASCOM Profile store

                tl.LogMessage("CoverCalibrator", "Starting initialisation");

                connectedState = false; // Initialise connected to false
                utilities = new Util(); //Initialise util object
                astroUtilities = new AstroUtils(); // Initialise astro-utilities object



                // added
                calibratorTimer = new System.Timers.Timer();
                if (CalibratorStablisationTimeValue > 0.0)
                {
                    calibratorTimer.Interval = Convert.ToInt32(CalibratorStablisationTimeValue * 1000.0); // Set the timer interval in milliseconds from the stabilisation time in seconds
                }
                calibratorTimer.Elapsed += CalibratorTimer_Tick;
                tl.LogMessage("CoverCalibrator", $"Set calibrator timer to: {calibratorTimer.Interval}ms.");

                coverTimer = new System.Timers.Timer();
                if (CoverOpeningTimeValue > 0.0)
                {
                    coverTimer.Interval = Convert.ToInt32(CoverOpeningTimeValue * 1000.0); // Set the timer interval in milliseconds from the opening time in seconds
                }
                coverTimer.Elapsed += CoverTimer_Tick;
                tl.LogMessage("CoverCalibrator", $"Set cover timer to: {coverTimer.Interval}ms.");

                // Initialise internal start-up values
                //  IsConnected = false; // Initialise connected to false
                //  brightnessValue = 0; // Set calibrator brightness to 0 i.e. off
                coverState = CoverStateInitialisationValue; // Set the cover state as set by the user
                calibratorState = CalibratorStateInitialisationValue; // Set the calibration state as set by the user


               
                tl.LogMessage("CoverCalibrator", "Completed initialisation");




            }
            catch (Exception ex)
            {
                // Create a message to the user
                string message = $"Exception while creating CoverCalibrator simulator: \r\n{ex.ToString()}";

                // Attempt to log the message
                try
                {
                    tl.Enabled = true;
                    tl.LogMessageCrLf("Initialisation", message);
                }
                catch { } // Ignore any errors while attempting to log the error

                // Display the error to the user
                MessageBox.Show(message, "ASCOM CoverCalibrator Simulator Exception", MessageBoxButtons.OK, MessageBoxIcon.Error); // Display the message top the user
            }





            tl.LogMessage("CoverCalibrator", "Completed initialisation");
        }


        private void CoverTimer_Tick(object sender, EventArgs e)
        {
            coverState = targetCoverState;
            coverTimer.Stop();
            tl.LogMessage("OpenCover", $"End of cover asynchronous event - cover state is now: {coverState}.");
        }

        private void CalibratorTimer_Tick(object sender, EventArgs e)
        {
            calibratorState = targetCalibratorStatus;
            calibratorTimer.Stop();
            tl.LogMessage("OpenCover", $"End of cover asynchronous event - cover state is now: {coverState}.");
        }



        //
        // PUBLIC COM INTERFACE ICoverCalibratorV1 IMPLEMENTATION
        //

        #region Common properties and methods.

        /// <summary>
        /// Displays the Setup Dialog form.
        /// If the user clicks the OK button to dismiss the form, then
        /// the new settings are saved, otherwise the old values are reloaded.
        /// THIS IS THE ONLY PLACE WHERE SHOWING USER INTERFACE IS ALLOWED!
        /// </summary>
        public void SetupDialog()
        {
            // consider only showing the setup dialog if not connected
            // or call a different dialog if connected
            if (IsConnected)
                System.Windows.Forms.MessageBox.Show("Already connected, just press OK");

            using (SetupDialogForm F = new SetupDialogForm(tl))
            {
                var result = F.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    WriteProfile(); // Persist device configuration values to the ASCOM Profile store
                }
            }
        }

        public ArrayList SupportedActions
        {
            get
            {
                tl.LogMessage("SupportedActions Get", "Returning empty arraylist");
                return new ArrayList();
            }
        }

        public string Action(string actionName, string actionParameters)
        {
            LogMessage("", "Action {0}, parameters {1} not implemented", actionName, actionParameters);
            throw new ASCOM.ActionNotImplementedException("Action " + actionName + " is not implemented by this driver");
        }

        public void CommandBlind(string command, bool raw)
        {
            CheckConnected("CommandBlind");
            // TODO The optional CommandBlind method should either be implemented OR throw a MethodNotImplementedException
            // If implemented, CommandBlind must send the supplied command to the mount and return immediately without waiting for a response

            throw new ASCOM.MethodNotImplementedException("CommandBlind");
        }

        public bool CommandBool(string command, bool raw)
        {
            CheckConnected("CommandBool");
            // TODO The optional CommandBool method should either be implemented OR throw a MethodNotImplementedException
            // If implemented, CommandBool must send the supplied command to the mount, wait for a response and parse this to return a True or False value

            // string retString = CommandString(command, raw); // Send the command and wait for the response
            // bool retBool = XXXXXXXXXXXXX; // Parse the returned string and create a boolean True / False value
            // return retBool; // Return the boolean value to the client

            throw new ASCOM.MethodNotImplementedException("CommandBool");
        }

        // added
        private byte[] SetPositionCommand(int servo, int value)
        {
            value = value * 4;
            //the set target command (compact protocol) is 0x84, then servo number(in hex), then position x 4 in hex(2 bytes)
            byte[] bytearray = BitConverter.GetBytes(value);
            byte[] servobyte = BitConverter.GetBytes(servo);
            //  bytearray[0] = (byte)((bytearray[0]>>7) & 0x07);
            //  bytearray[1] = (byte)(bytearray[1] & 0x07);
            byte[] send = new byte[4];
            send[0] = 0x84;
            send[1] = servobyte[0];
            send[2] = (byte)(value & 0x7F);
            send[3] = (byte)((value >> 7) & 0x7F);

            // return new byte[] { 0x84, servobyte[0], bytearray[0], bytearray[1] };
            return send;
        }

        private byte[] GetPosition(int servo)
        {
            //get position command (compact protocol) is 0x90 folloed by servo number
            byte[] servobyte = BitConverter.GetBytes(servo);
            return new byte[] { 0x90, servobyte[0] };
        }


        public static byte[] ConvertToByteArray(string hexString)
        {
            hexString = hexString.Replace("-", "");//remove the "-" delimiter
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
            }

            byte[] HexAsBytes = new byte[hexString.Length / 2];
            for (int index = 0; index < hexString.Length; index += 2)
            {
                string byteValue = hexString.Substring(index, 2);
                HexAsBytes[index / 2] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                //   HexAsBytes[index] = byte.Parse(byteValue);
                //   HexAsBytes[index/2] = Convert.ToByte(byteValue);



            }

            return HexAsBytes;
        }
        // end add

        private void home()
        {
            //open cover
            int open = Convert.ToUInt16((OpenAngle * 17.8 + 64));
            CommandString(BitConverter.ToString(SetPositionCommand(flapServo, open)), false);
            System.Threading.Thread.Sleep(200);

            //goto mid brightness
            // int pos = Convert.ToUInt16(Brightness * 18 + 64);  //18 was 17.8
            if (canDim)
            {
                int pos = 1504;
                CommandString(BitConverter.ToString(SetPositionCommand(levelServo, pos)), false);
                System.Threading.Thread.Sleep(200);
            }
        }
        public string CommandString(string command, bool raw)
        {
            CheckConnected("CommandString");

            // add
            CheckConnected("CommandString");
            byte[] tempbyte = new byte[4];
            int tempint = 0;
            mutex.WaitOne();
            try
            {

                //change to maestro protocol
                serialPort.ClearBuffers();
                //   byte[] send = ConvertHexStringToByteArray(command);
                byte[] send = ConvertToByteArray(command);
                System.Threading.Thread.Sleep(50);
                //  serialPort.TransmitBinary(new byte[] {0x84,0x00,bytearray[0],bytearray[1]});
                serialPort.TransmitBinary(send);
                if (command.Substring(0, 2) == "84")
                {
                    serialPort.ClearBuffers();
                    return "";
                }
                // serialPort.Transmit(command);
                //  temp = serialPort.ReceiveTerminated("#");
                tempbyte = serialPort.ReceiveCountedBinary(2);

                serialPort.ClearBuffers();
                tempint = BitConverter.ToInt16(tempbyte, 0);

            }

            catch (Exception e)
            {
                tl.LogMessage("Command String Error", "sending" + command + "Exception " + e.Message);
            }
            finally
            {
                mutex.ReleaseMutex();
            }
            return tempint.ToString();
            //     return "9";
// end add

            // TODO The optional CommandString method should either be implemented OR throw a MethodNotImplementedException
            // If implemented, CommandString must send the supplied command to the mount and wait for a response before returning this to the client

            throw new ASCOM.MethodNotImplementedException("CommandString");
        }

        // added
        System.Threading.Mutex mutex = new System.Threading.Mutex();
        bool lastLink = false;
        long UPDATETICKS = (long)(1 * 10000000.0);
        long lastUpdate = 0;
        long lastL = 0;
        private void DoUpdate()
        {

            // only allow access for "gets" once per second.
            // if inside of 1 second the buffered value will be used.
            if (serialPort != null)
                if (DateTime.Now.Ticks > UPDATETICKS + lastUpdate)
                {
                    lastUpdate = DateTime.Now.Ticks;

                    flatPos = Convert.ToInt16(CommandString(BitConverter.ToString(GetPosition(flapServo)), false));
                    System.Threading.Thread.Sleep(5);
                    if (canDim)
                    flatLevel = Convert.ToInt16(CommandString(BitConverter.ToString(GetPosition(levelServo)), false));


                }
        }


        public void Dispose()
        {
            try
            {
                if (serialPort == null)
                    return;
                serialPort.Connected = false;
                serialPort.Dispose();
                serialPort = null;

                // Clean up the tracelogger and util objects
                tl.Enabled = false;
                tl.Dispose();
                tl = null;
                utilities.Dispose();
                utilities = null;
                astroUtilities.Dispose();
                astroUtilities = null;
            }

            catch (Exception e)
            {
                throw new Exception("Dispose Error");
            }

        }
        // end add





        //Orignal dispose

        //public void Dispose()
        //{
        //    // Clean up the trace logger and util objects
        //    tl.Enabled = false;
        //    tl.Dispose();
        //    tl = null;
        //    utilities.Dispose();
        //    utilities = null;
        //    astroUtilities.Dispose();
        //    astroUtilities = null;
        // }



        // add connected()
        public bool Connected
        {
            get
            {
                tl.LogMessage("Connected Get", IsConnected.ToString());
                return IsConnected;
            }
            set
            {
                tl.LogMessage("Connected Set", value.ToString());
                if (value == IsConnected)
                    return;

                if (value)
                {
                    /*                 
                   List<DeviceListItem> connectedDevices = Usc.getConnectedDevices();
                   foreach (DeviceListItem dli in connectedDevices)
                       {
                           Usc device = new Usc(dli);
                       }

                    */
                    connectedState = true;
                    tl.LogMessage("Connected Set", "Connecting to port " + comPort);

                    // TODO connect to the device
                    if (serialPort != null && serialPort.Connected)
                        return;
                    string portname;
                    using (ASCOM.Utilities.Profile p = new Profile())
                    {
                        p.DeviceType = "CoverCalibrator";
                        portname = p.GetValue(driverID, "ComPort");
                        //  Profile p = new Profile();
                        // p.DeviceType = "Switch";
                        flapServo = Convert.ToByte(p.GetValue(CoverCalibrator.driverID, "flapServo"));
                        levelServo = Convert.ToByte(p.GetValue(CoverCalibrator.driverID, "levelServo"));
                        OpenAngle = Convert.ToInt16(p.GetValue(CoverCalibrator.driverID, "OpenAngle"));
                        ClosedAngle = Convert.ToInt16(p.GetValue(CoverCalibrator.driverID, "ClosedAngle"));
                        CoverOpeningTimeValue = Convert.ToInt16(p.GetValue(CoverCalibrator.driverID, "CoverSettle"));
                        CalibratorStablisationTimeValue = Convert.ToInt16(p.GetValue(CoverCalibrator.driverID, "CalibSettle"));
                        MaxBrightnessValue = Convert.ToInt16(p.GetValue(CoverCalibrator.driverID, "MaxBrightnessValue"));
                        
                        

                    }
                    if (string.IsNullOrEmpty(portname))
                        throw new ASCOM.NotConnectedException("COM port no selected");
                    try
                    {
                        serialPort = new Serial();
                        serialPort.PortName = portname;
                        serialPort.Speed = SerialSpeed.ps9600;
                        serialPort.Parity = SerialParity.None;
                        serialPort.StopBits = SerialStopBits.One;
                        serialPort.DataBits = 8;
                        if (!serialPort.Connected)
                            serialPort.Connected = true;
                        serialPort.ClearBuffers();
                        System.Threading.Thread.Sleep(1000);
                    }
                    catch (Exception ex)
                    {
                        throw new ASCOM.NotConnectedException("Serial Port connection error");
                    }
                    if (MaxBrightnessValue == 1)
                    {
                        canDim = false;
                        tl.LogMessage("canDim", canDim.ToString());
                    }
                    else
                    {
                        canDim = true;
                        tl.LogMessage("canDim", canDim.ToString());
                    }
                    home();
                }
                else
                {
                    connectedState = false;
                    tl.LogMessage("Connected Set", "Disconnecting from port " + comPort);
                    // TODO disconnect from the device
                    if (serialPort != null && serialPort.Connected)
                    {
                        serialPort.Connected = false;
                        serialPort.Dispose();
                        serialPort = null;
                    }
                }
             //   home();
            }
            
        }

        // origianl connected ()
        //public bool Connected
        //{
        //    get
        //    {
        //        LogMessage("Connected", "Get {0}", IsConnected);
        //        return IsConnected;
        //    }
        //    set
        //    {
        //        tl.LogMessage("Connected", "Set {0}", value);
        //        if (value == IsConnected)
        //            return;

        //        if (value)
        //        {
        //            connectedState = true;
        //            LogMessage("Connected Set", "Connecting to port {0}", comPort);
        //            // TODO connect to the device
        //        }
        //        else
        //        {
        //            connectedState = false;
        //            LogMessage("Connected Set", "Disconnecting from port {0}", comPort);
        //            // TODO disconnect from the device
        //        }
        //    }
        //}

        public string Description
        {
            // TODO customise this device description
            get
            {
                tl.LogMessage("Description Get", driverDescription);
                return driverDescription;
            }
        }

        public string DriverInfo
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                // TODO customise this driver description
                string driverInfo = "Information about the driver itself. Version: " + String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
                tl.LogMessage("DriverInfo Get", driverInfo);
                return driverInfo;
            }
        }

        public string DriverVersion
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string driverVersion = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
                tl.LogMessage("DriverVersion Get", driverVersion);
                return driverVersion;
            }
        }

        public short InterfaceVersion
        {
            // set by the driver wizard
            get
            {
                LogMessage("InterfaceVersion Get", "1");
                return Convert.ToInt16("1");
            }
        }

        public string Name
        {
            get
            {
                string name = "scopefocus_AF_Maestro";
                tl.LogMessage("Name Get", name);
                return name;
            }
        }

        #endregion

        #region ICoverCalibrator Implementation

        /// <summary>
        /// Returns the state of the device cover, if present, otherwise returns "NotPresent"
        /// </summary>
        public CoverStatus CoverState
        {
            get
            {
                //tl.LogMessage("CoverState Get", "Not implemented");
                // throw new ASCOM.PropertyNotImplementedException("CoverState", false);

                if (IsConnected)
                {
                    LogMessage("CoverState Get", coverState.ToString());

                    int angle;

                    DoUpdate();
                    //   using (Usc device = connectToDevice())
                    //   {
                    //  ServoStatus[] servos;
                    //   device.getVariables(out servos);
                    //      pos = Convert.ToInt16(CommandString(BitConverter.ToString(GetPosition(0)), false));

                    //  pos = servos[Convert.ToInt16(flapServo)].position;
                    angle = Convert.ToInt16((((flatPos / 4) - 64) / 17.8));
                    if ((angle == ClosedAngle) || ((angle < ClosedAngle + 2) && (angle > ClosedAngle - 2)))
                    {
                        coverState = CoverStatus.Closed;
                        return coverState;
                    }
                    if ((angle == OpenAngle)|| ((angle < OpenAngle + 2) && (angle > OpenAngle - 2)))
                    {
                        coverState = CoverStatus.Open;
                        return coverState;
                    }

                    else
                        return coverState = CoverStatus.Moving;


                    
                }
                else
                {
                    LogMessage("CoverState Get", $"Not connected, returning CoverStatus.Unknown");
                    return CoverStatus.Unknown;
                }

            }
        }

        /// <summary>
        /// Initiates cover opening if a cover is present
        /// </summary>
        public void OpenCover()
        {
          //  if (coverState == CoverStatus.NotPresent) throw new MethodNotImplementedException("This device has no cover capability.");

            if (!IsConnected) throw new NotConnectedException("The driver is not connected, the OpenCover method is not available.");


            //comment below
            //Profile p = new Profile();
            //    p.DeviceType = "Cover";
            //    // byte flapServo =Convert.ToByte(p.GetValue(Switch.driverID, "flapServo"));
            //    int flapServo = Convert.ToInt16(p.GetValue(CoverCalibrator.driverID, "flapServo"));
            //    int OpenAngle = Convert.ToInt16(p.GetValue(CoverCalibrator.driverID, "OpenAngle"));
            // //   int ClosedAngle = Convert.ToInt16(p.GetValue(CoverCalibrator.driverID, "ClosedAngle"));
            //    //   int open = Convert.ToUInt16((OpenAngle * 17.8 + 64)*4);
            //    //   int closed = Convert.ToUInt16((ClosedAngle * 17.8 + 64)*4);
            // emd comment


            if (coverState != CoverStatus.Open)
            {
                coverState = CoverStatus.Moving;
                //targetCoverState = CoverStatus.Open;
                //coverTimer.Start();
                //tl.LogMessage("OpenCover", $"Starting asynchronous cover opening for {CoverOpeningTimeValue} seconds.");


                int open = Convert.ToUInt16((OpenAngle * 17.8 + 64));
                //   int closed = Convert.ToUInt16((ClosedAngle * 17.8 + 64));
               

                CommandString(BitConverter.ToString(SetPositionCommand(flapServo, open)), false);
                System.Threading.Thread.Sleep(100);
                //   tl.LogMessage("OpenCover", "Not implemented");
                //   throw new ASCOM.MethodNotImplementedException("OpenCover");

                WaitForCover(CoverOpeningTimeValue);

                //coomnet out after adding time
                //int angle = Convert.ToInt16((((flatPos / 4) - 64) / 17.8));
                //while (angle > OpenAngle + 2)
                //{
                //    System.Threading.Thread.Sleep(100);
                //    DoUpdate();
                //    angle = Convert.ToInt16((((flatPos / 4) - 64) / 17.8));

                //}

                if (!canDim)
                {
                    brightnessValue = 0;
                  //  calibratorState = CalibratorStatus.Ready;
                }
                tl.LogMessage("Cover Open", coverState.ToString());
            }
            else
                return;
        }

        /// <summary>
        /// Initiates cover closing if a cover is present
        /// </summary>
        public void CloseCover()
        {
            //copmment below
            //     Profile p = new Profile();
            //     p.DeviceType = "Cover";
            //     // byte flapServo =Convert.ToByte(p.GetValue(Switch.driverID, "flapServo"));
            //     int flapServo = Convert.ToInt16(p.GetValue(CoverCalibrator.driverID, "flapServo"));
            //  //   int OpenAngle = Convert.ToInt16(p.GetValue(CoverCalibrator.driverID, "OpenAngle"));
            //     int ClosedAngle = Convert.ToInt16(p.GetValue(CoverCalibrator.driverID, "ClosedAngle"));
            //     //   int open = Convert.ToUInt16((OpenAngle * 17.8 + 64)*4);
            //     //   int closed = Convert.ToUInt16((ClosedAngle * 17.8 + 64)*4);
            ////     int open = Convert.ToUInt16((OpenAngle * 17.8 + 64));

            // end comment
            if (!IsConnected) throw new NotConnectedException("The driver is not connected, the OpenCover method is not available.");

            if (coverState != CoverStatus.Closed)
            {

                coverState = CoverStatus.Moving;
                //targetCoverState = CoverStatus.Closed;
                //coverTimer.Start();
                //tl.LogMessage("OpenCover", $"Starting asynchronous cover opening for {CoverOpeningTimeValue} seconds.");



                int closed = Convert.ToUInt16((ClosedAngle * 17.8 + 64));
                CommandString(BitConverter.ToString(SetPositionCommand(flapServo, closed)), false);
                System.Threading.Thread.Sleep(100);

                WaitForCover(CoverOpeningTimeValue);
                //commnmet out ater adding timer

                //int angle = Convert.ToInt16((((flatPos / 4) - 64) / 17.8));
                //while (angle < ClosedAngle - 2)
                //{
                //    System.Threading.Thread.Sleep(100);
                //    DoUpdate();
                //    angle = Convert.ToInt16((((flatPos / 4) - 64) / 17.8));
                //}
                //tl.LogMessage("Cover Close", coverState.ToString());
                if (!canDim)
                    brightnessValue = 1;
            }
           
            else
                return;
          //  tl.LogMessage("CloseCover", "Not implemented");
         //   throw new ASCOM.MethodNotImplementedException("CloseCover");
        }

        /// <summary>
        /// Stops any cover movement that may be in progress if a cover is present and cover movement can be interrupted.
        /// </summary>
        public void HaltCover()
        {
            tl.LogMessage("HaltCover", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("HaltCover");
        }

        /// <summary>
        /// Returns the state of the calibration device, if present, otherwise returns "NotPresent"
        /// </summary>
        public CalibratorStatus CalibratorState
        {
            get
            {

                //if ((coverState == CoverStatus.Closed) && (Brightness == Convert.ToInt16((((flatLevel / 4) - 64) / 17.8))))
                if (calibratorState == CalibratorStatus.Off)
                    return calibratorState;
               // if ((coverState == CoverStatus.Closed) && (Brightness > 0))
                    if (coverState == CoverStatus.Closed) 
                    {
                    calibratorState = CalibratorStatus.Ready;
                    tl.LogMessage("CalibratorState Get", calibratorState.ToString());
                    return calibratorState;
                }

                if (coverState == CoverStatus.Open)
                {
                    calibratorState = CalibratorStatus.Off;
                    tl.LogMessage("CalibratorState Get", calibratorState.ToString());
                    return calibratorState;
                }
                //if ((coverState == CoverStatus.Closed) && (Brightness == 0))
                //{
                //    calibratorState = CalibratorStatus.Off;
                //    tl.LogMessage("CalibratorState Get", calibratorState.ToString());
                //    return calibratorState;
                //}

                else
                {
                    calibratorState = CalibratorStatus.NotReady;
                    tl.LogMessage("CalibratorState Get", calibratorState.ToString());
                    return calibratorState;
                    // LogMessage("CalibratorState Get", $"Not connected, returning CalibratorState.Unknown");
                    //  return CalibratorStatus.Unknown;
                }
                tl.LogMessage("CalibratorState Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("CalibratorState", false);
            }

        }

        /// <summary>
        /// Returns the current calibrator brightness in the range 0 (completely off) to <see cref="MaxBrightness"/> (fully on)
        /// </summary>
        public int Brightness
        {
            get
            {
              // if (calibratorState == CalibratorStatus.NotPresent) throw new PropertyNotImplementedException("Brightness", false);

                if (!IsConnected) throw new NotConnectedException("The simulator is not connected, the Brightness property is not available.");
                if (canDim)
                {
                    DoUpdate();
                    System.Threading.Thread.Sleep(100);
                    brightnessValue = ((flatLevel / 4) - 64) / 18;
                    //  tl.LogMessage("Brightness Get_ flatlevel", flatLevel.ToString());
                    tl.LogMessage("Brightness Get", brightnessValue.ToString());
                    return brightnessValue;
                }
                else
                {
                    tl.LogMessage("Brightness Get", brightnessValue.ToString());
                    return brightnessValue;
                }


                }
        }

        /// <summary>
        /// The Brightness value that makes the calibrator deliver its maximum illumination.
        /// </summary>
        public int MaxBrightness
        {
            get
            {
                return MaxBrightnessValue;
              //  tl.LogMessage("MaxBrightness Get", "Not implemented");
               // throw new ASCOM.PropertyNotImplementedException("MaxBrightness", false);
            }
        }

        /// <summary>
        /// Turns the calibrator on at the specified brightness if the device has calibration capability
        /// </summary>
        /// <param name="Brightness"></param>
        public void CalibratorOn(int Brightness)
        {
            //  if (calibratorState == CalibratorStatus.NotPresent) throw new MethodNotImplementedException("This device has no calibrator capability.");
            if ((Brightness < 0) || (Brightness > MaxBrightnessValue))
                throw new InvalidValueException("CalibratorOn", Brightness.ToString(), $"0 to {MaxBrightnessValue}");
            if (canDim)
            {
                if ((calibratorState == CalibratorStatus.Ready) || (calibratorState == CalibratorStatus.Off))
                {
                    calibratorState = CalibratorStatus.NotReady;
                    //targetCalibratorStatus = CalibratorStatus.Ready;
                    //calibratorTimer.Start();
                    //tl.LogMessage("CalibratorOn", $"Starting asynchronous calibrator turn on for {CalibratorStablisationTimeValue} seconds.");

                    // add
                    // brightnessValue = Brightness; // Set the assigned brightness
                    int pos = Convert.ToUInt16(Brightness * 18 + 64);  //18 was 17.8
                    CommandString(BitConverter.ToString(SetPositionCommand(levelServo, pos)), false);
                    System.Threading.Thread.Sleep(100);
                    WaitForCalib(CalibratorStablisationTimeValue);
                    tl.LogMessage("CalibratorOn - Brightness", Brightness.ToString());
                    calibratorState = CalibratorStatus.Ready;
                    //comment after adding timer

                    //int level = Convert.ToInt16((((flatLevel / 4) - 64) / 17.8));
                    //while (level != Brightness) 
                    //{
                    //    System.Threading.Thread.Sleep(100);
                    //    DoUpdate();
                    //    level = Convert.ToInt16(((flatLevel / 4) - 64) / 17.8);
                    //}
                    //calibratorState = CalibratorStatus.Ready;

                }
            }
            else
            {
                if (coverState == CoverStatus.Open)
                    CloseCover();

                brightnessValue = Brightness;
                tl.LogMessage("CalibratorOn - Brightness", Brightness.ToString());
                calibratorState = CalibratorStatus.Ready;
            }
            //  throw new ASCOM.MethodNotImplementedException("CalibratorOn");
        }

        /// <summary>
        /// Turns the calibrator off if the device has calibration capability
        /// </summary>
        public void CalibratorOff()
        {
            //  if (calibratorState == CalibratorStatus.NotPresent) throw new MethodNotImplementedException("This device has no calibrator capability.");
            if (canDim)
            {
                if (!IsConnected) throw new NotConnectedException("The simulator is not connected, the CalibratorOff method is not available.");

                calibratorState = CalibratorStatus.NotReady;
                //targetCalibratorStatus = CalibratorStatus.Off;
                //calibratorTimer.Start();
                //tl.LogMessage("CalibratorOff", $"Starting asynchronous calibrator turn off for {CalibratorStablisationTimeValue} seconds.");


                int pos = 64;
                // int pos = Convert.ToUInt16(Brightness * 18 + 64);  //18 was 17.8
                CommandString(BitConverter.ToString(SetPositionCommand(levelServo, pos)), false);
                System.Threading.Thread.Sleep(100);
                WaitForCalib(CalibratorStablisationTimeValue);

                // commneted after adding timer

                //int level = Convert.ToInt16((((flatLevel / 4) - 64) / 17.8));
                //while (level > 0)
                //{
                //    System.Threading.Thread.Sleep(100);
                //    DoUpdate();
                //    level = Convert.ToInt16(((flatLevel / 4) - 64) / 17.8);

                //}
            }
            else
            {
                OpenCover();
                calibratorState = CalibratorStatus.Off;
                brightnessValue = 0;
            }

            tl.LogMessage("CalibratorOff - Brightness", Brightness.ToString());
            calibratorState = CalibratorStatus.Off;
            // tl.LogMessage("CalibratorOff", "Not implemented");
            //  throw new ASCOM.MethodNotImplementedException("CalibratorOff");
        }

        #endregion

        #region Private properties and methods
        // here are some useful properties and methods that can be used as required
        // to help with driver development

        #region ASCOM Registration

        // Register or unregister driver for ASCOM. This is harmless if already
        // registered or unregistered. 
        //
        /// <summary>
        /// Register or unregister the driver with the ASCOM Platform.
        /// This is harmless if the driver is already registered/unregistered.
        /// </summary>
        /// <param name="bRegister">If <c>true</c>, registers the driver, otherwise unregisters it.</param>
        private static void RegUnregASCOM(bool bRegister)
        {
            using (var P = new ASCOM.Utilities.Profile())
            {
                P.DeviceType = "CoverCalibrator";
                if (bRegister)
                {
                    P.Register(driverID, driverDescription);
                }
                else
                {
                    P.Unregister(driverID);
                }
            }
        }

        /// <summary>
        /// This function registers the driver with the ASCOM Chooser and
        /// is called automatically whenever this class is registered for COM Interop.
        /// </summary>
        /// <param name="t">Type of the class being registered, not used.</param>
        /// <remarks>
        /// This method typically runs in two distinct situations:
        /// <list type="numbered">
        /// <item>
        /// In Visual Studio, when the project is successfully built.
        /// For this to work correctly, the option <c>Register for COM Interop</c>
        /// must be enabled in the project settings.
        /// </item>
        /// <item>During setup, when the installer registers the assembly for COM Interop.</item>
        /// </list>
        /// This technique should mean that it is never necessary to manually register a driver with ASCOM.
        /// </remarks>
        [ComRegisterFunction]
        public static void RegisterASCOM(Type t)
        {
            RegUnregASCOM(true);
        }

        /// <summary>
        /// This function unregisters the driver from the ASCOM Chooser and
        /// is called automatically whenever this class is unregistered from COM Interop.
        /// </summary>
        /// <param name="t">Type of the class being registered, not used.</param>
        /// <remarks>
        /// This method typically runs in two distinct situations:
        /// <list type="numbered">
        /// <item>
        /// In Visual Studio, when the project is cleaned or prior to rebuilding.
        /// For this to work correctly, the option <c>Register for COM Interop</c>
        /// must be enabled in the project settings.
        /// </item>
        /// <item>During uninstall, when the installer unregisters the assembly from COM Interop.</item>
        /// </list>
        /// This technique should mean that it is never necessary to manually unregister a driver from ASCOM.
        /// </remarks>
        [ComUnregisterFunction]
        public static void UnregisterASCOM(Type t)
        {
            RegUnregASCOM(false);
        }

        #endregion

        /// <summary>
        /// Returns true if there is a valid connection to the driver hardware
        /// </summary>
        /// 
      //  private bool IsConnected { get; set; }
        private bool IsConnected
        {
            get
            {
                // TODO check that the driver hardware connection exists and is connected to the hardware
                return connectedState;
            }
        }

        /// <summary>
        /// Use this function to throw an exception if we aren't connected to the hardware
        /// </summary>
        /// <param name="message"></param>
        private void CheckConnected(string message)
        {
            if (!IsConnected)
            {
                throw new ASCOM.NotConnectedException(message);
            }
        }

        /// <summary>
        /// Read the device configuration from the ASCOM Profile store
        /// </summary>
        internal void ReadProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "CoverCalibrator";
                tl.Enabled = Convert.ToBoolean(driverProfile.GetValue(driverID, traceStateProfileName, string.Empty, traceStateDefault));
                comPort = driverProfile.GetValue(driverID, comPortProfileName, string.Empty, comPortDefault);
            }
        }

        /// <summary>
        /// Write the device configuration to the  ASCOM  Profile store
        /// </summary>
        internal void WriteProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "CoverCalibrator";
                driverProfile.WriteValue(driverID, traceStateProfileName, tl.Enabled.ToString());
                driverProfile.WriteValue(driverID, comPortProfileName, comPort.ToString());
            }
        }

        /// <summary>
        /// Log helper function that takes formatted strings and arguments
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        internal void LogMessage(string identifier, string message, params object[] args)
        {
            var msg = string.Format(message, args);
            tl.LogMessage(identifier, msg);
        }

        private void WaitForCover(double duration)
        {
            DateTime endTime = DateTime.Now.AddSeconds(duration); // Calculate the end time
            do
            {
                System.Threading.Thread.Sleep(20);
                Application.DoEvents();
             //   tl.LogMessage("wiating for caver", endTime.ToString());
                //DoUpdate();
                //if (coverState != CoverStatus.Moving)
                //    break;
            } while (DateTime.Now < endTime);
        }
        private void WaitForCalib(double duration)
        {
            DateTime endTime = DateTime.Now.AddSeconds(duration); // Calculate the end time
            do
            {
                System.Threading.Thread.Sleep(20);
                Application.DoEvents();
             //   tl.LogMessage("wiating for calib", endTime.ToString());
                //DoUpdate();
                //if (calibratorState != CalibratorStatus.NotReady)
                //    break;
            } while (DateTime.Now < endTime);
        }

        #endregion
    }
}
