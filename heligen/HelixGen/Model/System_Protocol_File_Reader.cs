// (c) Copyright 2017 HelixGen, All Rights Reserved.
// (c) Copyright 2017 Accel BioTech, All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using XmlDocument_Support_Utilities;
using HelixGen.Model;

//   Upon executing an automatic unreserve with a pipette attached, if the next DH specific operation is another Use, then the unreserve 
//         run-time logic should not flag a 'restore the last pipette' action on reacquisition of the DH by this cartridge's script.
//   Upon executing an automatic unreserve with a pipette attached, if there are no more DH specific operations following then the run-time
//         logic of the unreserve should not flag a 'restore the last pipette' status.

namespace HelixGen
{
    public enum eTestProcess_Statement_Type
    {
        kDisplayText,
        kPressureThreshold,
        kPump,
        kReadPressure,
        kSetPressure,
        kValve,
        kWait
    };

    public class CTestProcess_Protocol_Statement
    {
        public CTestProcess_Protocol_Statement(int lineNo, eTestProcess_Statement_Type typeIn)
        {
            m_eStatementType = typeIn;
            m_iLineNumber = lineNo;
        }

        public eTestProcess_Statement_Type m_eStatementType;
        public int m_iLineNumber;
    }

    public class CTestProcess_Protocol_DisplayText : CTestProcess_Protocol_Statement
    {
        public CTestProcess_Protocol_DisplayText(int lineNo, string textIn = "") : base(lineNo, eTestProcess_Statement_Type.kDisplayText)
        {
            _sMessage = textIn;
        }

        protected string _sMessage;

        /// <summary>
        /// The text of the message to be displayed.
        /// </summary>
        public string Message
        {
            get { return _sMessage; }
            set { _sMessage = value; }
        }
    }

    public class CTestProcess_Protocol_PressureThreshold : CTestProcess_Protocol_Statement
    {
        public CTestProcess_Protocol_PressureThreshold(int lineNo, double threshold = -1, string messageIn = "") : base(lineNo, eTestProcess_Statement_Type.kPressureThreshold)
        {
            _threshold = threshold;
            _message = messageIn;

        }

        protected double _threshold;
        protected string _message;

        /// <summary>
        /// The threshold to be considered.
        /// </summary>
        public double threshold
        {
            get { return _threshold; }
            set { _threshold = value; }
        }

        /// <summary>
        /// The message to be displayed if the threshold is not met.
        /// </summary>
        public string message
        {
            get { return _message; }
            set { _message = value; }
        }
    }

    public class CTestProcess_Protocol_Pump : CTestProcess_Protocol_Statement
    {
        public CTestProcess_Protocol_Pump(int lineNo, bool bOnIn) : base(lineNo, eTestProcess_Statement_Type.kPump)
        {
            _bOn = bOnIn;
        }

        /// <summary>
        /// True if the pump should be turned on, false otherwise.
        /// </summary>
        protected bool _bOn;

        public bool bOn
        {
            get { return _bOn; }
            set { _bOn = value; }
        }
    }

    public class CTestProcess_Protocol_ReadPressure : CTestProcess_Protocol_Statement
    {
        public CTestProcess_Protocol_ReadPressure(int lineNo) : base(lineNo, eTestProcess_Statement_Type.kReadPressure)
        {
        }
    }

    public class CTestProcess_Protocol_SetPressure : CTestProcess_Protocol_Statement
    {
        public CTestProcess_Protocol_SetPressure(int lineNo, double dPressureIn) : base(lineNo, eTestProcess_Statement_Type.kSetPressure)
        {
            _dPressure = dPressureIn;
        }

        protected double _dPressure;

        public double pressure
        {
            get { return _dPressure; }
            set { _dPressure = value; }
        }
    }

    public class CTestProcess_Protocol_Valve : CTestProcess_Protocol_Statement
    {
        public CTestProcess_Protocol_Valve(int lineNo, string valveName, bool bOpen) : base(lineNo, eTestProcess_Statement_Type.kValve)
        {
            _strValveName = valveName;
            _bOpen = bOpen;
        }

        protected string _strValveName;
        protected bool _bOpen;

        public string valveName
        {
            get { return _strValveName; }
            set { _strValveName = value; }
        }

        public bool Open
        {
            get { return _bOpen; }
            set { _bOpen = value; }
        }
    }

    public class CTestProcess_Protocol_Wait : CTestProcess_Protocol_Statement
    {
        public CTestProcess_Protocol_Wait(int lineNo, int waitTimeIn = -1) : base(lineNo, eTestProcess_Statement_Type.kWait)
        {
            _nWaitTimeInSeconds = waitTimeIn;
        }

        protected int _nWaitTimeInSeconds;

        /// <summary>
        /// The time to wait (in seconds obviously)
        /// </summary>
        public int waitTimeInSeconds
        {
            get { return _nWaitTimeInSeconds; }
            set { _nWaitTimeInSeconds = value; }
        }
    }


    public enum ePneumaticProcess_Statement_Type
    {
        kChip_Type,
        kProtocol_Name,
        kLoad,
        kPrime,
        kDigitize,
        kWait,
        kPCRProcess
    };

    public class CPneumaticProcess_Protocol_Statement
    {
        public CPneumaticProcess_Protocol_Statement(int iLineNumber, ePneumaticProcess_Statement_Type eStatementType)
        {
            m_eStatementType = eStatementType;
            m_iLineNumber = iLineNumber;
        }
        public ePneumaticProcess_Statement_Type m_eStatementType;
        public int m_iLineNumber;
    }

    public class CPneumaticProcess_Protocol_Wait_Statement : CPneumaticProcess_Protocol_Statement
    {
        public CPneumaticProcess_Protocol_Wait_Statement(int iLineNumber) : base(iLineNumber, ePneumaticProcess_Statement_Type.kWait)
        {
            // TBD
        }
        public uint m_uiWaitTimeInSeconds;
    }

    public class CPneumaticProcess_Protocol_Load_Statement : CPneumaticProcess_Protocol_Statement
    {
        public CPneumaticProcess_Protocol_Load_Statement(int iLineNumber, int iDeviceSlot, float fPressureInPSI, int iDurationInSeconds) : base(iLineNumber, ePneumaticProcess_Statement_Type.kLoad)
        {
            m_iDeviceSlot = iDeviceSlot;
            m_fPressureInPSI = fPressureInPSI;
            m_iDurationInSeconds = iDurationInSeconds;
        }
        public int m_iDeviceSlot;
        public float m_fPressureInPSI;
        public int m_iDurationInSeconds;
    }

    public class CPneumaticProcess_Protocol_Prime_Statement : CPneumaticProcess_Protocol_Statement
    {
        public CPneumaticProcess_Protocol_Prime_Statement(int iLineNumber, int iDeviceSlot, float fPressureInPSI, int iDurationInSeconds)
            : base(iLineNumber, ePneumaticProcess_Statement_Type.kPrime)
        {
            m_iDeviceSlot = iDeviceSlot;
            m_fPressureInPSI = fPressureInPSI;
            m_iDurationInSeconds = iDurationInSeconds;
        }
        public int m_iDeviceSlot;
        public float m_fPressureInPSI;
        public int m_iDurationInSeconds;
    }

    public class CPneumaticProcess_Protocol_Digitize_Statement : CPneumaticProcess_Protocol_Statement
    {
        public CPneumaticProcess_Protocol_Digitize_Statement(int iLineNumber, int iDeviceSlot, float fPressureInPSI, int iDurationInSeconds)
            : base(iLineNumber, ePneumaticProcess_Statement_Type.kDigitize)
        {
            m_iDeviceSlot = iDeviceSlot;
            m_fPressureInPSI = fPressureInPSI;
            m_iDurationInSeconds = iDurationInSeconds;
        }
        public int m_iDeviceSlot;
        public float m_fPressureInPSI;
        public int m_iDurationInSeconds;
    }

    public class CProtocol_PCRProcess_Statement : CPneumaticProcess_Protocol_Statement
    {
        public CProtocol_PCRProcess_Statement(int iLineNumber, ePneumaticProcess_Statement_Type eStepType = ePneumaticProcess_Statement_Type.kPCRProcess) : base(iLineNumber, ePneumaticProcess_Statement_Type.kPCRProcess)
        {
        }
        public string m_strPCRProcess;
    }

    public class CTest_Protocol_Process_Obj
    {
        public CTest_Protocol_Process_Obj()
        {
            m_TestProcess_Statements = new List<CTestProcess_Protocol_Statement>();
            m_uiCurrentStatementIndex = 0;
        }

        public CTest_Protocol_Process_Obj(CTest_Protocol_Process_Obj obj)
        {
            m_TestProcess_Statements = new List<CTestProcess_Protocol_Statement>(obj.m_TestProcess_Statements);
        }

        public List<CTestProcess_Protocol_Statement> m_TestProcess_Statements;
        public uint m_uiCurrentStatementIndex;
    }

    public class CPneumaticProcess_Protocol_Process_Obj
    {
        public CPneumaticProcess_Protocol_Process_Obj() { m_uiCurrentStatementIndex = 0; }
        public CPneumaticProcess_Protocol_Process_Obj(CPneumaticProcess_Protocol_Process_Obj obj)
        {
            this.m_strName = obj.m_strName;
            this.m_uiCurrentStatementIndex = obj.m_uiCurrentStatementIndex;
            this.m_PneumaticProcess_Statements = new List<CPneumaticProcess_Protocol_Statement>(obj.m_PneumaticProcess_Statements);
        }
        public string m_strName;
        public uint m_uiCurrentStatementIndex;
        public List<CPneumaticProcess_Protocol_Statement> m_PneumaticProcess_Statements;
    }

    public class CPCR_ProtocolMeasurementDefn
    {
        public CPCR_ProtocolMeasurementDefn()
        {
            m_fExposureTimeinMicroSeconds = -1.0F;  // Signifies not set.
            m_iLEDIntensity = -1;                   // Signifies not set.
        }


        // The Exposure time, in microseconds.  This is an optional parameter.
        // A value of -1.0F indicates that the field is not set.

        public float m_fExposureTimeinMicroSeconds;

        // The LED intensity setting, as a percentage of full power.  This is an optional parameter.
        // A value less than zero indicates that the field is not set.

        public int m_iLEDIntensity;
        public int[] m_filterindices;
    }

    public enum ePCR_ProtocolOperationType { kPCR_Step, kPCR_Cycle };

    /// <summary>
    /// Defines a valve operation.
    /// </summary>
    public class CPCR_ValveOp
    {
        /// <summary>
        /// The name of the valve, one of "vent", "in", or "out"
        /// </summary>
        protected string _valveName;

        /// <summary>
        /// True if the valve should be opened, false otherwise
        /// </summary>
        protected bool _bOpen;

        public CPCR_ValveOp(string nameIn = "", bool openIn = false)
        {
            _valveName = nameIn.ToUpper();
            _bOpen = openIn;
        }

        /// <summary>
        /// The name of the valve, one of "vent", "in", or "out"
        /// </summary>
        public string name
        {
            get { return _valveName; }
            set { _valveName = value.ToUpper(); }
        }

        /// <summary>
        /// True if the valve should be opened, false otherwise
        /// </summary>
        public bool open
        {
            get { return _bOpen; }
            set { _bOpen = value; }
        }
    }

    public class CPCR_ProtocolBaseStepDefn
    {
        public CPCR_ProtocolBaseStepDefn()
        {
            m_Pressure = -1;   // -1 indicates value not defined.
            bCameraEnabled = true;
            valveOps = new List<CPCR_ValveOp>();
        }
        public CPCR_ProtocolBaseStepDefn(ePCR_ProtocolOperationType ePCRStepType)
        {
            m_eStepType = ePCRStepType;
            m_Pressure = -1;    // -1 indicates value not defined.
            valveOps = new List<CPCR_ValveOp>();
        }
        virtual public ePCR_ProtocolOperationType m_eStepType { get; set; }
        virtual public string m_strName { get; set; }
        virtual public float m_fHoldTimeinSecs { get; set; }
        virtual public CPCR_ProtocolMeasurementDefn m_Measurement { get; set; }
        virtual public double m_Pressure { get; set; }    // Optional pressure setting. -1 if not set.
        public List<CPCR_ValveOp> valveOps; // Optional valve setting, blank if not set.
        virtual public bool bCameraEnabled { get; set; }
    }

    public class CPCR_ProtocolStepDefn : CPCR_ProtocolBaseStepDefn
    {
        public CPCR_ProtocolStepDefn(ePCR_ProtocolOperationType eStepType) : base(eStepType) { m_bIsCoolStep = false; }
        public float m_fTemperatureinC { get; set; }
        public bool m_bIsCoolStep { get; set; } // Temporary patch supporting protocol final cooling step. Disables LID Heater, and forces fans to run at 100% duty cycle.
    }

    public class CPCR_ProtocolCycleDefn : CPCR_ProtocolBaseStepDefn
    {
        public CPCR_ProtocolCycleDefn(ePCR_ProtocolOperationType eStepType) : base(eStepType) { }
        public uint m_uiRepetitions { get; set; }
        public CPCR_OperationDescriptor[] m_Operations;
        // other members TBD
    }

    public class CPCR_OperationDescriptor
    {
        public CPCR_OperationDescriptor(string strName, ePCR_ProtocolOperationType opType)
        {
            m_strOperationName = strName;
            m_OpType = opType;
        }
        public ePCR_ProtocolOperationType m_OpType;
        public string m_strOperationName;
    }

    public class CPCR_ProtocolProcess
    {
        public CPCR_ProtocolProcess(string strName)
        {
            m_strName = strName;
        }
        public string m_strName;
        public float m_fDataLoggingRateInSeconds;
        public CPCR_OperationDescriptor[] m_OperationSequence;
    }

    public class CSystem_Protocol_PCR_Process_Obj
    {
        public Dictionary<string, CPCR_ProtocolStepDefn> m_PCR_StepDefns;
        public Dictionary<string, CPCR_ProtocolCycleDefn> m_PCR_CycleDefns;
        public CPCR_ProtocolProcess m_PCR_ProtocolProcess;
    }

    public class CSystem_Protocol_Obj
    {
        public string m_strChipType;
        public CTest_Protocol_Process_Obj m_Test_Process;
        public CPneumaticProcess_Protocol_Process_Obj m_Pneumatic_Process;
        public CSystem_Protocol_PCR_Process_Obj m_PCR_Process;
    }

    public class CSystem_Protocol_File_Reader
    {
        protected HelixGenModel _model;

        public CSystem_Protocol_File_Reader()
        {
            _model = ((HelixGen.App)(App.Current)).Model;
        }


        private void ParseProtocolTestStatments(Positional_XmlElement test_protocol_statement_nodes,
            CTest_Protocol_Process_Obj test_protocolIn)
        {
            string sArg;
            bool bSuccess;
            double dPressure;
            int nWaitTimeInSeconds;

            foreach (Positional_XmlElement statement_node in test_protocol_statement_nodes)
            {
                if (statement_node.NodeType == XmlNodeType.Element)
                {
                    switch (statement_node.Name.ToUpper())
                    {
                        case "DISPLAYTEXT":
                            test_protocolIn.m_TestProcess_Statements.Add(
                                new CTestProcess_Protocol_DisplayText(statement_node.LineNumber, statement_node.InnerText)
                                );
                            break;

                        case "PRESSURETHRESHOLD":
                            {
                                // The arguments are of the form; threshold, message

                                string[] args = statement_node.InnerText.Split(',');

                                if (args.Length != 2)
                                {
                                    throw new Exception(string.Format("Protocol Parse Error. Test section, expecting two arguments for the {0} statement on line: {1}.",
                                         statement_node.Name,
                                         statement_node.LineNumber.ToString()));
                                }

                                // Validate the threshold argument.  It must be an integer and above zero.

                                double fThreshold = -1;

                                bSuccess = double.TryParse(args[0], out fThreshold);

                                if (!bSuccess)
                                {
                                    throw new Exception(string.Format("Protocol Parse Error. Test section, the threshold argument \"{0}\" for the {1} statement on line: {2} must be a valid positive real number.",
                                        args[0],
                                         statement_node.Name,
                                         statement_node.LineNumber.ToString()));
                                }

                                if (bSuccess && fThreshold < 0)
                                {
                                    throw new Exception(string.Format("Protocol Parse Error. Test section, the threshold argument \"{0}\" for the {1} statement on line: {2} must be a positive real number.",
                                        args[0],
                                         statement_node.Name,
                                         statement_node.LineNumber.ToString()));
                                }

                                test_protocolIn.m_TestProcess_Statements.Add(
                                    new CTestProcess_Protocol_PressureThreshold(statement_node.LineNumber, fThreshold, args[1])
                                    );
                            }
                            break;

                        case "PUMP":
                            // The argument can be either on or off.

                            sArg = statement_node.InnerText.Trim().ToUpper();

                            if (sArg != "ON" && sArg != "OFF")
                            {
                                throw new Exception(string.Format("Protocol Parse Error. Test section, the state argument \"{0}\" for the {1} statement on line: {2} must be either \"ON\" or \"OFF\" (case independent).",
                                    statement_node.InnerText,
                                     statement_node.Name,
                                     statement_node.LineNumber.ToString()));
                            }

                            test_protocolIn.m_TestProcess_Statements.Add(new CTestProcess_Protocol_Pump(statement_node.LineNumber, (sArg == "ON")));

                            break;

                        case "READPRESSURE":
                            test_protocolIn.m_TestProcess_Statements.Add(new CTestProcess_Protocol_ReadPressure(statement_node.LineNumber));
                            break;

                        case "SETPRESSURE":

                            string sValue = statement_node.InnerText.Trim();

                            if (sValue.ToUpper() == "OFF")
                                dPressure = 0;
                            else
                            {
                                bSuccess = double.TryParse(statement_node.InnerText.Trim(), out dPressure);

                                if (!bSuccess)
                                {
                                    throw new Exception(string.Format("Protocol Parse Error. Test section, the pressure argument \"{0}\" for the {1} statement on line: {2} must be a valid positive decimal number.",
                                        statement_node.InnerText,
                                         statement_node.Name,
                                         statement_node.LineNumber.ToString()));
                                }
                            }

                            test_protocolIn.m_TestProcess_Statements.Add(new CTestProcess_Protocol_SetPressure(statement_node.LineNumber, dPressure));

                            break;

                        case "VALVE":
                            {
                                string[] args = statement_node.InnerText.Split(',');

                                if (args.Length != 2)
                                {
                                    throw new Exception(string.Format("Protocol Parse Error. Test section, expecting two arguments for the {0} statement on line: {1}.",
                                         statement_node.Name,
                                         statement_node.LineNumber.ToString()));
                                }

                                if (args[0].ToUpper() != "IN" && args[0].ToUpper() != "OUT" && args[0].ToUpper() != "VENT")
                                {
                                    throw new Exception(string.Format("Protocol Parse Error. Test section, the valve name argument \"{0}\" for the {1} statement on line: {2} must be either \"IN\", \"OUT\" or \"VENT\" (case independent).",
                                        statement_node.InnerText,
                                         statement_node.Name,
                                         statement_node.LineNumber.ToString()));
                                }

                                if (args[1].ToUpper() != "OPEN" && args[1].ToUpper() != "CLOSE")
                                {
                                    throw new Exception(string.Format("Protocol Parse Error. Test section, the valve state argument \"{0}\" for the {1} statement on line: {2} must be either \"OPEN\" or \"CLOSE\" (case independent).",
                                        statement_node.InnerText,
                                         statement_node.Name,
                                         statement_node.LineNumber.ToString()));
                                }

                                test_protocolIn.m_TestProcess_Statements.Add(new CTestProcess_Protocol_Valve(statement_node.LineNumber, args[0], (args[1].ToUpper() == "OPEN")));
                            }

                            break;

                        case "WAIT":

                            bSuccess = Int32.TryParse(statement_node.InnerText.Trim(), out nWaitTimeInSeconds);

                            if (!bSuccess)
                            {
                                throw new Exception(string.Format("Protocol Parse Error. Test section, the time argument \"{0}\" for the {1} statement on line: {2} must be a valid positive integer.",
                                    statement_node.InnerText,
                                     statement_node.Name,
                                     statement_node.LineNumber.ToString()));
                            }

                            test_protocolIn.m_TestProcess_Statements.Add(new CTestProcess_Protocol_Wait(statement_node.LineNumber, nWaitTimeInSeconds));

                            break;

                        default:
                            throw new Exception(string.Format("Protocol Parse Error.  Test section, found an unexpected statement; \"{0}\' at line; {1}.",
                                statement_node.Name,
                                statement_node.LineNumber.ToString()));
#pragma warning disable // gag the unreachable code warning.  There is no way around it.
                            break;
#pragma warning restore
                    }
                }
            }
        }

        private void ParseProtocolPneumaticProcessStatements(Positional_XmlElement pneumaticprocess_protocol_statement_nodes, CPneumaticProcess_Protocol_Process_Obj pneumatic_protocol_obj, out string strChipType)
        {
            bool bRequiredPneumaticProcessNameFound = false;
            bool bRequiredChipTypeFound = false;
            //bool bOptionalPCRProcessStatementFound = false;

            strChipType = "";
            pneumatic_protocol_obj.m_PneumaticProcess_Statements = new List<CPneumaticProcess_Protocol_Statement>();

            foreach (Positional_XmlElement statement_node in pneumaticprocess_protocol_statement_nodes)
            {
                if (statement_node.NodeType == XmlNodeType.Element)
                {
                    switch (statement_node.Name.ToUpper())
                    {

                        case "CHIPTYPE":
                            {
                                if (bRequiredChipTypeFound)
                                {
                                    // Parsing error
                                    throw new Exception("Protocol Pneumatic Process parse Error. Duplicate <" + statement_node.Name + "> declaration at line # " + statement_node.LineNumber.ToString() + ".");
                                }
                                else
                                {
                                    bRequiredChipTypeFound = true;
                                }
                                strChipType = statement_node.InnerText;
                            }
                            break;
                        case "NAME":
                            {
                                if (bRequiredPneumaticProcessNameFound)
                                {
                                    // Parsing error
                                    throw new Exception("Protocol Pneumatic Process parse Error. Duplicate <Name> declaration at line # " + statement_node.LineNumber.ToString() + ".");
                                }
                                else
                                {
                                    bRequiredPneumaticProcessNameFound = true;
                                }
                                pneumatic_protocol_obj.m_strName = statement_node.InnerText;
                            }
                            break;
                        case "LOAD":
                            {
                                CPneumaticProcess_Protocol_Load_Statement LoadStatementDefn = new CPneumaticProcess_Protocol_Load_Statement(statement_node.LineNumber, 0, 0.0F, 0);

                                string[] args = statement_node.InnerText.Split(',');
                                // Verify required fields present
                                if (args.Length != 3)
                                {
                                    // Parsing error
                                    throw new Exception("Protocol Pneumatic LOAD parse Error. Invalid argument count for \"LOAD\" statement declaration at line # " + statement_node.LineNumber.ToString() + ".");
                                }
                                LoadStatementDefn.m_iDeviceSlot = 1;
                                LoadStatementDefn.m_fPressureInPSI = Convert.ToSingle(args[1]);
                                LoadStatementDefn.m_iDurationInSeconds = (int)Convert.ToUInt32(args[2]);
                                pneumatic_protocol_obj.m_PneumaticProcess_Statements.Add(LoadStatementDefn); // Add Load statement
                            }
                            break;
                        case "PRIME":
                            {
                                CPneumaticProcess_Protocol_Prime_Statement PrimeStatementDefn = new CPneumaticProcess_Protocol_Prime_Statement(statement_node.LineNumber, 0, 0.0F, 0);

                                string[] args = statement_node.InnerText.Split(',');
                                // Verify required fields present
                                if (args.Length != 3)
                                {
                                    // Parsing error
                                    throw new Exception("Protocol Pneumatic PRIME parse Error. Invalid argument count for \"PRIME\" statement declaration at line # " + statement_node.LineNumber.ToString() + ".");
                                }
                                PrimeStatementDefn.m_iDeviceSlot = 1;
                                PrimeStatementDefn.m_fPressureInPSI = Convert.ToSingle(args[1]);
                                PrimeStatementDefn.m_iDurationInSeconds = (int)Convert.ToUInt32(args[2]);
                                pneumatic_protocol_obj.m_PneumaticProcess_Statements.Add(PrimeStatementDefn); // Add PRIME statement
                            }
                            break;
                        case "DIGITIZE":
                            {
                                CPneumaticProcess_Protocol_Digitize_Statement DigitizeStatementDefn = new CPneumaticProcess_Protocol_Digitize_Statement(statement_node.LineNumber, 0, 0.0F, 0);

                                string[] args = statement_node.InnerText.Split(',');
                                // Verify required fields present
                                if (args.Length != 3)
                                {
                                    // Parsing error
                                    throw new Exception("Protocol Pneumatic PRIME parse Error. Invalid argument count for \"PRIME\" statement declaration at line # " + statement_node.LineNumber.ToString() + ".");
                                }
                                DigitizeStatementDefn.m_iDeviceSlot = 1;
                                DigitizeStatementDefn.m_fPressureInPSI = Convert.ToSingle(args[1]);
                                DigitizeStatementDefn.m_iDurationInSeconds = (int)Convert.ToUInt32(args[2]);
                                pneumatic_protocol_obj.m_PneumaticProcess_Statements.Add(DigitizeStatementDefn); // Add DIGITIZE statement
                            }
                            break;
                        case "PCR_PROCESS":
                            {
                                CProtocol_PCRProcess_Statement PCRProcessStatementDefn = new CProtocol_PCRProcess_Statement(statement_node.LineNumber);

                                string[] args = statement_node.InnerText.Split(',');
                                // Verify required fields present
                                if (args.Length == 1)
                                {
                                    PCRProcessStatementDefn.m_strPCRProcess = args[0]; // PCR Process Name
                                }
                                else
                                {
                                    // Parsing error
                                    throw new Exception("Protocol Pneumatic Process parse Error. Invalid argument count for \"PCR_Process\" statement declaration at line # " + statement_node.LineNumber.ToString() + ".");
                                }
                                pneumatic_protocol_obj.m_PneumaticProcess_Statements.Add(PCRProcessStatementDefn); // Add PCR_Process statement
                            }
                            break;
                        default:
                            {
                                // Parse error, first node after 'Steps' must be 'Step'
                                throw new Exception("Protocol Parse Error.  Expected valid sample preparation statement, instead found <" + statement_node.Name + ">.");
                            }
#pragma warning disable CS0162 // Unreachable code detected
                            break;
#pragma warning restore CS0162 // Unreachable code detected
                    }
                }
            }
        }

        private void ProtocolPneumaticProcessSemanticValidation(CPneumaticProcess_Protocol_Process_Obj protocol_obj, string strChipType, out string strWarning)
        {
            Dictionary<string, string> PneumaticProcess_ProtocolSemanticErrors = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            // Determine if errors detected.
            if (PneumaticProcess_ProtocolSemanticErrors.Values.Count() > 0)
            {
                string strErrors = "";
                string[] strSortedByLineNumberErrors = PneumaticProcess_ProtocolSemanticErrors.Keys.ToArray();
                Array.Sort(strSortedByLineNumberErrors);
                foreach (string strErrLineNumber in strSortedByLineNumberErrors)
                {
                    strErrors += PneumaticProcess_ProtocolSemanticErrors[strErrLineNumber];
                }
                throw new Exception(strErrors);
            }
            strWarning = "";
        }

        private void ParseProtocolPCRStepDefinitions(Positional_XmlElement step_nodes, CSystem_Protocol_PCR_Process_Obj protocol_obj)
        {
            foreach (Positional_XmlElement step_node in step_nodes)
            {
                if (step_node.NodeType == XmlNodeType.Element)
                {
                    if ("STEP" == step_node.Name.ToUpper())
                    {
                        bool bRequiredNameFound = false;
                        bool bRequiredTemperatureFound = false;
                        bool bRequiredHoldTimeFound = false;
                        bool bOptionalMeasurementFound = false;
                        bool bOptionalCoolAttributeFound = false;
                        CPCR_ProtocolStepDefn StepDefn = new CPCR_ProtocolStepDefn(ePCR_ProtocolOperationType.kPCR_Step);

                        foreach (Positional_XmlElement step_subnode in step_node)
                        {
                            if (step_subnode.NodeType == XmlNodeType.Element)
                            {
                                string tag_name = step_subnode.Name.ToUpper();

                                switch (tag_name)
                                {
                                    case "NAME":
                                        {
                                            if (bRequiredNameFound)
                                            {
                                                // Parsing error
                                                throw new Exception("Protocol Parse Error.  Step definition has duplicate <Name> entry.");
                                            }
                                            else
                                            {
                                                bRequiredNameFound = true; // Required definition.
                                            }
                                            StepDefn.m_strName = step_subnode.InnerText;
                                        }
                                        break;
                                    case "TEMPERATURE":
                                        {
                                            if (bRequiredTemperatureFound)
                                            {
                                                // Parsing error
                                                throw new Exception("Protocol Parse Error.  Step definition has duplicate <Temperature> entry.");
                                            }
                                            else
                                            {
                                                bRequiredTemperatureFound = true; // Required entry
                                            }
                                            StepDefn.m_fTemperatureinC = Convert.ToSingle(step_subnode.InnerText);
                                        }
                                        break;
                                    case "HOLDTIME":
                                        {
                                            if (bRequiredHoldTimeFound)
                                            {
                                                // Parsing error
                                                throw new Exception("Protocol Parse Error.  Step definition has duplicate <HoldTime> entry.");
                                            }
                                            else
                                            {
                                                bRequiredHoldTimeFound = true; // Required entry
                                            }
                                            StepDefn.m_fHoldTimeinSecs = Convert.ToSingle(step_subnode.InnerText);
                                        }
                                        break;
                                    case "CAMERA":
                                        {
                                            string cameraEnabledArg = step_subnode.InnerText.ToUpper();

                                            if (cameraEnabledArg != "DISABLE" && cameraEnabledArg != "ENABLE")
                                            {
                                                throw new Exception(string.Format(
                                                    "Protocol Parse Error.  Step definition camera statement value; \"{0}\" must be either \"ENABLE\" or \"DISABLE\" (case independent).",
                                                    step_subnode.InnerText));
                                            }

                                            StepDefn.bCameraEnabled = cameraEnabledArg == "ENABLE";
                                        }
                                        break;

                                    case "MEASUREMENT":
                                        {
                                            if (!bOptionalMeasurementFound)
                                            {
                                                bOptionalMeasurementFound = true;
                                                bool bRequiredFiltersFound = false;
                                                bool bOptionalExposureTimeFound = false;
                                                bool bOptionalLEDIntensityFound = false;
                                                StepDefn.m_Measurement = new CPCR_ProtocolMeasurementDefn();
                                                StepDefn.m_Measurement.m_fExposureTimeinMicroSeconds = -1.0F; // Indicates no user specified value in protocol (will instead use default)
                                                StepDefn.m_Measurement.m_iLEDIntensity = -1; // Indicates no user specified value in protocol (will instead use default)

                                                foreach (Positional_XmlElement measurement_subnode in step_subnode)
                                                {
                                                    if (measurement_subnode.NodeType == XmlNodeType.Element)
                                                    {
                                                        string subtag_name = measurement_subnode.Name.ToUpper();

                                                        switch (subtag_name)
                                                        {

                                                            case "EXPOSURETIMEINMICROSECONDS":
                                                                {
                                                                    if (bOptionalExposureTimeFound)
                                                                    {
                                                                        throw new Exception("Protocol Parse Error.  Step definition measurement field has duplicate <ExposureTimeinMicroSeconds> entry.");
                                                                    }
                                                                    else
                                                                    {
                                                                        bOptionalExposureTimeFound = true; // Required definition
                                                                    }
                                                                    try
                                                                    {
                                                                        StepDefn.m_Measurement.m_fExposureTimeinMicroSeconds = Convert.ToSingle(measurement_subnode.InnerText);
                                                                    }
                                                                    catch (Exception)
                                                                    {
                                                                        throw new Exception("Protocol Parse Error.  Step definition measurement field has malformed <ExposureTimeinMicroSeconds> value.");
                                                                    }
                                                                }
                                                                break;
                                                            case "FILTERS":
                                                                {
                                                                    if (bRequiredFiltersFound)
                                                                    {
                                                                        throw new Exception("Protocol Parse Error.  Step definition measurement field has duplicate <Filters> entry.");
                                                                    }
                                                                    else
                                                                    {
                                                                        bRequiredFiltersFound = true; // Required definition
                                                                    }
                                                                    StepDefn.m_Measurement.m_filterindices = new int[1]; // Initially allocate a filter array of length 1.  Will extend as filters are parsed.
                                                                    int iFilterOrderIndex = 0;
                                                                    foreach (Positional_XmlElement filter_node in measurement_subnode)
                                                                    {
                                                                        if (filter_node.NodeType == XmlNodeType.Element)
                                                                        {
                                                                            string filter_tag_name = filter_node.Name.ToUpper();
                                                                            if ("FILTER" == filter_tag_name)
                                                                            {
                                                                                if (iFilterOrderIndex > 0)
                                                                                {
                                                                                    Array.Resize<int>(ref StepDefn.m_Measurement.m_filterindices, iFilterOrderIndex + 1);
                                                                                }
                                                                                StepDefn.m_Measurement.m_filterindices[iFilterOrderIndex++] = Convert.ToInt32(filter_node.InnerText);
                                                                            }
                                                                            else
                                                                            {
                                                                                // Parsing error
                                                                                throw new Exception("Protocol Parse Error.  Filter tag expected following <Filters> tag, instead found <" + filter_tag_name + ">.");
                                                                            }
                                                                        }
                                                                    }
                                                                    if (iFilterOrderIndex == 0)
                                                                    {
                                                                        // Parsing error, must have at least one filter specified
                                                                        throw new Exception("Protocol Parse Error.  Filter definition expected following <Filters> tag.");
                                                                    }
                                                                }
                                                                break;
                                                            case "LEDINTENSITY":
                                                                {
                                                                    int nIntensity = 0;

                                                                    // swe;
                                                                    if (bOptionalLEDIntensityFound)
                                                                    {
                                                                        throw new Exception("Protocol Parse Error.  Step definition measurement field has duplicate <ledintensity> entry.");
                                                                    }
                                                                    else
                                                                    {
                                                                        bOptionalLEDIntensityFound = true;
                                                                    }
                                                                    try
                                                                    {
                                                                        nIntensity = Int32.Parse(measurement_subnode.InnerText);
                                                                    }
                                                                    catch (Exception)
                                                                    {
                                                                        throw new Exception(string.Format("Protocol Parse Error.  Step definition measurement field has malformed <ledintensity> value; \"{0}\" should be an integer.", measurement_subnode.InnerText));
                                                                    }

                                                                    // Check the range on the intensity.

                                                                    if (nIntensity < 0 || nIntensity > 100)
                                                                    {
                                                                        throw new Exception(string.Format("Protocol Parse Error.  Step definition measurement field <ledintensity> value is out of range.  The value; \"{0}\" should be between 0 and 100.", measurement_subnode.InnerText));
                                                                    }

                                                                    StepDefn.m_Measurement.m_iLEDIntensity = nIntensity;
                                                                }
                                                                break;
                                                            default:
                                                                {
                                                                    // Only 'Filters' tags defined - parse error
                                                                    throw new Exception("Protocol Parse Error.  Filters definition expected following <Measurement> tag.");
                                                                }
#pragma warning disable CS0162 // Unreachable code detected
                                                                break;
#pragma warning restore CS0162 // Unreachable code detected
                                                        }
                                                    }
                                                }
                                                // Verify required fields present
                                                if (!bRequiredFiltersFound)
                                                {
                                                    throw new Exception("Protocol Parse Error. <Filters> field required following <Step> <Measurement> tag.");
                                                }
                                            }
                                            else
                                            {
                                                // Parsing error
                                                throw new Exception("Protocol Parse Error.  Step definition has duplicate <Measurement> entry.");
                                            }
                                        }
                                        break;
                                    case "COOL":
                                        {
                                            if (bOptionalCoolAttributeFound)
                                            {
                                                // Parsing error
                                                throw new Exception("Protocol Parse Error.  Step definition has duplicate <Cool> entry.");
                                            }
                                            else
                                            {
                                                bOptionalCoolAttributeFound = true; // Require entry
                                            }
                                            StepDefn.m_bIsCoolStep = ((step_subnode.InnerText.ToUpper() == "ON") ? true : false);
                                        }
                                        break;
                                    case "VALVE":
                                        {
                                            string[] args = step_subnode.InnerText.Split(',');

                                            if (args.Length != 2)
                                            {
                                                throw new Exception(string.Format("Protocol Parse Error. Steps section, expecting two arguments for the {0} statement on line: {1}.",
                                                     step_subnode.Name,
                                                     step_subnode.LineNumber.ToString()));
                                            }

                                            if (args[0].ToUpper() != "IN" && args[0].ToUpper() != "OUT" && args[0].ToUpper() != "VENT")
                                            {
                                                throw new Exception(string.Format("Protocol Parse Error. Steps section, the valve name argument \"{0}\" for the {1} statement on line: {2} must be either \"IN\", \"OUT\" or \"VENT\" (case independent).",
                                                    step_subnode.InnerText,
                                                     step_subnode.Name,
                                                     step_subnode.LineNumber.ToString()));
                                            }

                                            if (args[1].ToUpper() != "OPEN" && args[1].ToUpper() != "CLOSE")
                                            {
                                                throw new Exception(string.Format("Protocol Parse Error. Steps section, the valve state argument \"{0}\" for the {1} statement on line: {2} must be either \"OPEN\" or \"CLOSE\" (case independent).",
                                                    step_subnode.InnerText,
                                                     step_subnode.Name,
                                                     step_subnode.LineNumber.ToString()));
                                            }

                                            StepDefn.valveOps.Add(new CPCR_ValveOp(args[0], args[1].ToUpper() == "OPEN"));
                                        }
                                        break;
                                    case "PRESSURE":
                                        {
                                            double dPressure;
                                            bool bSuccess = double.TryParse(step_subnode.InnerText.Trim(), out dPressure);

                                            if (!bSuccess)
                                            {
                                                throw new Exception(string.Format("Protocol Parse Error. Steps section, the pressure argument \"{0}\" for the {1} statement on line: {2} must be a valid positive integer.",
                                                    step_subnode.InnerText,
                                                     step_subnode.Name,
                                                     step_subnode.LineNumber.ToString()));
                                            }

                                            StepDefn.m_Pressure = dPressure;
                                        }
                                        break;
                                    default:
                                        {
                                            // Only 'Name', 'Temperature', 'HoldTime', 'Cool' and 'Measurement' tags are allowed - parse error
                                            throw new Exception("Protocol Parse Error.  Name, Temperature, HoldTime, Cool, and Measurement tags expected following <Step> tag.  Instead found " + tag_name + ".\n");
                                        }
#pragma warning disable CS0162 // Unreachable code detected
                                        break;
#pragma warning restore CS0162 // Unreachable code detected
                                }
                            }
                        }
                        // Verify required fields present
                        if (!(bRequiredNameFound && bRequiredTemperatureFound && bRequiredHoldTimeFound))
                        {
                            throw new Exception("Protocol Parse Error. <Name>,<Temperature>, and <HoldTime> must all be defined for <step> definition.");
                        }
                        if (protocol_obj.m_PCR_StepDefns == null)
                        {
                            protocol_obj.m_PCR_StepDefns = new Dictionary<string, CPCR_ProtocolStepDefn>(StringComparer.InvariantCultureIgnoreCase);
                        }
                        protocol_obj.m_PCR_StepDefns.Add(StepDefn.m_strName, StepDefn);
                    }
                    else
                    {
                        // Parse error, first node after 'Steps' must be 'Step'
                        throw new Exception("Protocol Parse Error.  Step definition expected following <Steps> tag.");
                    }
                }
            }
        }

        private void ParseProtocolPCRCycleDefinitions(Positional_XmlElement cycle_nodes, CSystem_Protocol_PCR_Process_Obj protocol_obj)
        {
            if (cycle_nodes == null)
            {
                return;
            }
            foreach (Positional_XmlElement cycle_node in cycle_nodes)
            {
                string strCycleName = "";
                if (cycle_node.NodeType == XmlNodeType.Element)
                {
                    if ("CYCLE" == cycle_node.Name.ToUpper())
                    {
                        bool bRequiredNameFound = false;
                        bool bRequiredRepetitionsFound = false;
                        bool bRequiredOperationsFound = false;
                        CPCR_ProtocolCycleDefn CycleStep = new CPCR_ProtocolCycleDefn(ePCR_ProtocolOperationType.kPCR_Cycle);

                        foreach (Positional_XmlElement cycle_subnode in cycle_node)
                        {
                            if (cycle_subnode.NodeType == XmlNodeType.Element)
                            {
                                string tag_name = cycle_subnode.Name.ToUpper();
                                switch (tag_name)
                                {
                                    case "NAME":
                                        {
                                            if (bRequiredNameFound)
                                            {
                                                // Parsing error
                                                throw new Exception("Protocol Parse Error.  Cycle definition has duplicate <Name> entry.");
                                            }
                                            else
                                            {
                                                bRequiredNameFound = true; // Required defintion.
                                            }
                                            if (protocol_obj.m_PCR_CycleDefns == null)
                                            {
                                                protocol_obj.m_PCR_CycleDefns = new Dictionary<string, CPCR_ProtocolCycleDefn>(StringComparer.InvariantCultureIgnoreCase);
                                            }
                                            strCycleName = CycleStep.m_strName = cycle_subnode.InnerText;
                                        }
                                        break;
                                    case "REPETITIONS":
                                        {
                                            if (bRequiredRepetitionsFound)
                                            {
                                                // Parsing error
                                                throw new Exception("Protocol Parse Error.  Cycle definition has duplicate <Repetitions> entry.");
                                            }
                                            else
                                            {
                                                bRequiredRepetitionsFound = true; // Required entry
                                            }
                                            CycleStep.m_uiRepetitions = Convert.ToUInt32(cycle_subnode.InnerText);
                                            if (CycleStep.m_uiRepetitions < 0)
                                            {
                                                throw new Exception("Protocol Parse Error.  Cycle definition has negative <Repetitions> entry.");
                                            }
                                        }
                                        break;
                                    case "OPERATIONS":
                                        {
                                            if (bRequiredOperationsFound)
                                            {
                                                // Parsing error
                                                throw new Exception("Protocol Parse Error.  Cycle definition has duplicate <Operations> entry.");
                                            }
                                            else
                                            {
                                                bRequiredOperationsFound = true;
                                            }
                                            CycleStep.m_Operations = new CPCR_OperationDescriptor[1]; // Start with an array of one operation.  Allocate additional ones as needed.
                                            int iOperationOrderIndex = 0;
                                            foreach (Positional_XmlElement operation_node in cycle_subnode)
                                            {
                                                if (operation_node.NodeType == XmlNodeType.Element)
                                                {
                                                    string cycle_entry_tag_name = operation_node.Name.ToUpper();
                                                    switch (cycle_entry_tag_name)
                                                    {
                                                        case "STEP":
                                                            {
                                                                if (iOperationOrderIndex > 0)
                                                                {
                                                                    Array.Resize<CPCR_OperationDescriptor>(ref CycleStep.m_Operations, iOperationOrderIndex + 1);
                                                                }
                                                                CycleStep.m_Operations[iOperationOrderIndex++] = new CPCR_OperationDescriptor(operation_node.InnerText, ePCR_ProtocolOperationType.kPCR_Step);
                                                            }
                                                            break;
                                                        case "CYCLE":
                                                            {
                                                                if (iOperationOrderIndex > 0)
                                                                {
                                                                    Array.Resize<CPCR_OperationDescriptor>(ref CycleStep.m_Operations, iOperationOrderIndex + 1);
                                                                }
                                                                protocol_obj.m_PCR_CycleDefns[strCycleName.ToUpper()].m_Operations[iOperationOrderIndex++] = new CPCR_OperationDescriptor(operation_node.InnerText, ePCR_ProtocolOperationType.kPCR_Cycle);
                                                            }
                                                            break;
                                                        default:
                                                            {
                                                                // Parsing error
                                                                throw new Exception("Protocol Parse Error.  Step tag expected following <Steps> tag, instead found <" + cycle_entry_tag_name + ">.");
                                                            }
#pragma warning disable CS0162 // Unreachable code detected
                                                            break;
#pragma warning restore CS0162 // Unreachable code detected
                                                    }
                                                }
                                            }
                                            if (iOperationOrderIndex == 0)
                                            {
                                                // Parsing error, must have at least one cycle step specified
                                                throw new Exception("Protocol Parse Error.  Step, Gradient, Cycle, or Meltcurve entry expected following <Steps> in cycle defintion.");
                                            }
                                        }
                                        break;
                                    default:
                                        {
                                            // Only 'Name', 'Repetitions', and 'Steps' permitted in Cycle definition - parse error
                                            throw new Exception("Protocol Parse Error.  Name, Repetitions, and Steps definition expected following <Cycle> tag.");
                                        }
#pragma warning disable CS0162 // Unreachable code detected
                                        break;
#pragma warning restore CS0162 // Unreachable code detected
                                }
                            }
                        }
                        // Verify required fields present
                        if (!(bRequiredNameFound && bRequiredRepetitionsFound && bRequiredOperationsFound))
                        {
                            throw new Exception("Protocol Parse Error. <Name>,<Repetitions>, and <Steps> must all be defined for <cycle> definition.");
                        }
                        protocol_obj.m_PCR_CycleDefns.Add(CycleStep.m_strName, CycleStep);
                    }
                    else
                    {
                        // Parse error, first node after 'Cycles' must be 'Cycle'
                        throw new Exception("Protocol Parse Error.  Cycle definition expected following <Cycles> tag.");
                    }
                }
            }
        }

        private void ParseProtocolPCRProcessSequenceDefinition(Positional_XmlElement process_sequence_node, CSystem_Protocol_PCR_Process_Obj protocol_obj)
        {
            if ("PROCESS_SEQUENCE" == process_sequence_node.Name.ToUpper())
            {
                bool bRequiredNameFound = false;
                bool bRequiredLoggingRateFound = false;
                bool bRequiredProcessStepsFound = false;
                string strProcessSequenceName = "";
                foreach (Positional_XmlElement process_sequence_subnode in process_sequence_node)
                {
                    if (process_sequence_subnode.NodeType == XmlNodeType.Element)
                    {
                        string tag_name = process_sequence_subnode.Name.ToUpper();
                        switch (tag_name)
                        {
                            case "NAME":
                                {
                                    if (bRequiredNameFound)
                                    {
                                        // Parsing error
                                        throw new Exception("Protocol Parse Error.  Process sequence definition has duplicate <Name> entry.");
                                    }
                                    else
                                    {
                                        bRequiredNameFound = true; // Required defintion.
                                    }
                                    protocol_obj.m_PCR_ProtocolProcess = new CPCR_ProtocolProcess(process_sequence_subnode.InnerText);
                                    strProcessSequenceName = process_sequence_subnode.InnerText;
                                }
                                break;
                            case "LOG_RATE_IN_SECS":
                                {
                                    if (bRequiredLoggingRateFound)
                                    {
                                        // Parsing error
                                        throw new Exception("Protocol Parse Error.  Process sequence definition has duplicate <log_rate_in_secs> entry.");
                                    }
                                    else
                                    {
                                        bRequiredLoggingRateFound = true; // Required defintion.
                                    }
                                    protocol_obj.m_PCR_ProtocolProcess.m_fDataLoggingRateInSeconds = Convert.ToSingle(process_sequence_subnode.InnerText);
                                    if (CSystem_Defns.cfPCR_Minimum_Measurement_Temperature_Sampling_Rate_In_Seconds > protocol_obj.m_PCR_ProtocolProcess.m_fDataLoggingRateInSeconds)
                                    {
                                        // Clamp to minimum allowed
                                        protocol_obj.m_PCR_ProtocolProcess.m_fDataLoggingRateInSeconds = CSystem_Defns.cfPCR_Minimum_Measurement_Temperature_Sampling_Rate_In_Seconds;
                                        // Log warning message - TBD
                                    }
                                }
                                break;
                            case "PROCESS_STEPS":
                                {
                                    if (bRequiredProcessStepsFound)
                                    {
                                        // Parsing error
                                        throw new Exception("Protocol Parse Error.  Process sequence definition has duplicate <Process_Steps> entry.");
                                    }
                                    else
                                    {
                                        bRequiredProcessStepsFound = true; // Required entry
                                    }
                                    protocol_obj.m_PCR_ProtocolProcess.m_OperationSequence = new CPCR_OperationDescriptor[1]; // Initally allocate a single operation entry.  Add others as parsed.
                                    int iOperationOrderIndex = 0;
                                    foreach (Positional_XmlElement operation_node in process_sequence_subnode)
                                    {
                                        if (operation_node.NodeType == XmlNodeType.Element)
                                        {
                                            string operation_tag_name = operation_node.Name.ToUpper();
                                            switch (operation_tag_name)
                                            {
                                                case "STEP":
                                                    {
                                                        if (iOperationOrderIndex > 0)
                                                        {
                                                            Array.Resize<CPCR_OperationDescriptor>(ref protocol_obj.m_PCR_ProtocolProcess.m_OperationSequence, iOperationOrderIndex + 1);
                                                        }
                                                        protocol_obj.m_PCR_ProtocolProcess.m_OperationSequence[iOperationOrderIndex++] = new CPCR_OperationDescriptor(operation_node.InnerText, ePCR_ProtocolOperationType.kPCR_Step);
                                                    }
                                                    break;
                                                case "CYCLE":
                                                    {
                                                        if (iOperationOrderIndex > 0)
                                                        {
                                                            Array.Resize<CPCR_OperationDescriptor>(ref protocol_obj.m_PCR_ProtocolProcess.m_OperationSequence, iOperationOrderIndex + 1);
                                                        }
                                                        protocol_obj.m_PCR_ProtocolProcess.m_OperationSequence[iOperationOrderIndex++] = new CPCR_OperationDescriptor(operation_node.InnerText, ePCR_ProtocolOperationType.kPCR_Cycle);
                                                    }
                                                    break;
                                                default:
                                                    {
                                                        // Parsing error
                                                        throw new Exception("Protocol Parse Error.  <Step>, <Cycle>, or <Iteration> tag expected following <Process_Steps> tag, instead found <" + operation_tag_name + ">.");
                                                    }
#pragma warning disable CS0162 // Unreachable code detected
                                                    break;
#pragma warning restore CS0162 // Unreachable code detected
                                            }
                                        }
                                    }
                                    if (iOperationOrderIndex == 0)
                                    {
                                        // Parsing error, must have at least one cycle, step, gradient, or meltcurve specified
                                        throw new Exception("Protocol Parse Error.  <Step>, <Cycle> <Meltcurve> or <Gradient> entry expected following <Process_steps> in process_sequence defintion.");
                                    }
                                }
                                break;
                            default:
                                {
                                    // Only 'Name', 'Process_Steps' permitted in Process_Sequence definition - parse error
                                    throw new Exception("Protocol Parse Error.  Only <Name>, <log_rate_in_secs>, and <Process_Steps> expected following <Process_Sequece> tag.");
                                }
                        }
                    }
                }
                // Verify required fields present
                if (!(bRequiredNameFound && bRequiredLoggingRateFound && bRequiredProcessStepsFound))
                {
                    throw new Exception("Protocol Parse Error. <Name>, <log_rate_in_secs> and <Process_Steps> must all be defined for <process_sequence> definition.");
                }
            }
            else
            {
                // Parse error, first node after 'Steps' and 'Cycles' and 'Iterations' must be 'Process_Sequence'
                throw new Exception("Protocol Parse Error.  Process_Sequence missing expected <Steps>, <Cycles> or <Meltcurves>.");
            }
        }

        private void ProtocolPCRSemanticValidation(CSystem_Protocol_PCR_Process_Obj protocol_obj, out string strWarning)
        {
            strWarning = "";

            // Verify all process sequence operations references are defined, and temperature limit and hold-times are acceptable.  Otherwise clamp.
            foreach (CPCR_OperationDescriptor process_step_entry in protocol_obj.m_PCR_ProtocolProcess.m_OperationSequence)
            {
                if (process_step_entry.m_OpType == ePCR_ProtocolOperationType.kPCR_Step)
                {
                    if (protocol_obj.m_PCR_StepDefns[process_step_entry.m_strOperationName.ToUpper()].m_strName == "")
                    {
                        throw new Exception("Protocol semantic Error. Process_Sequence \"" + protocol_obj.m_PCR_ProtocolProcess.m_strName + "\" references undefined Step \"" + process_step_entry.m_strOperationName + "\".");
                    }
                    CPCR_ProtocolStepDefn step = protocol_obj.m_PCR_StepDefns[process_step_entry.m_strOperationName];
                    //if (step.m_fTemperatureinC < _m_configurationObj.m_MiscellaneousConfiguration.m_fProtocolNonCycleStep_MinTemperature)
                        if (step.m_fTemperatureinC < _model.Config.m_MiscellaneousConfiguration.m_fProtocolNonCycleStep_MinTemperature)

                        {
                            step.m_fTemperatureinC = _model.Config.m_MiscellaneousConfiguration.m_fProtocolNonCycleStep_MinTemperature;
                        // Issue warning message to operator that step temperature temperature was clamped to specified minimum.
                        strWarning += "Protocol step \"" + process_step_entry.m_strOperationName + "\" has excessively low temperature.  Temperature clamped to minimum.\n";
                        if (step.m_fHoldTimeinSecs > _model.Config.m_MiscellaneousConfiguration.m_uiProtocolNonCycleStep_MinTemp_MaxHoldTimeinSeconds)
                        {
                            step.m_fHoldTimeinSecs = _model.Config.m_MiscellaneousConfiguration.m_uiProtocolNonCycleStep_MinTemp_MaxHoldTimeinSeconds;
                            // Issue warning message to operator that step hold-time has been clamped to specified maximum
                            strWarning += "Protocol step \"" + process_step_entry.m_strOperationName + "\" has excessively long hold-time for specified low temperature.  Hold-time clamped to maximum.\n";
                        }
                        protocol_obj.m_PCR_StepDefns[process_step_entry.m_strOperationName] = step; // Replace with updated step definition
                    }
                    if (step.m_fTemperatureinC > _model.Config.m_MiscellaneousConfiguration.m_fProtocolNonCycleStep_MaxTemperature)
                    {
                        step.m_fTemperatureinC = _model.Config.m_MiscellaneousConfiguration.m_fProtocolNonCycleStep_MaxTemperature;
                        protocol_obj.m_PCR_StepDefns[process_step_entry.m_strOperationName] = step; // Replace with updated step definition
                        // Issue warning message to operator that final step hold-time has been clamped to specified maximum
                        strWarning += "Protocol step \"" + process_step_entry.m_strOperationName + "\" has excessively high temperature.  Temperature clamped to maximum.\n";
                    }
                }
                else if (process_step_entry.m_OpType == ePCR_ProtocolOperationType.kPCR_Cycle)
                {
                    if (protocol_obj.m_PCR_CycleDefns[process_step_entry.m_strOperationName.ToUpper()].m_strName == "")
                    {
                        throw new Exception("Protocol semantic Error. Process_Sequence \"" + protocol_obj.m_PCR_ProtocolProcess.m_strName + "\" references undefined Cycle \"" + process_step_entry.m_strOperationName + "\".");
                    }
                }
            }

            // Verify all cycle step entry references are defined
            if (protocol_obj.m_PCR_CycleDefns != null)
            {
                foreach (KeyValuePair<string, CPCR_ProtocolCycleDefn> cycle_entry in protocol_obj.m_PCR_CycleDefns)
                {
                    // Verify any referenced operations are defined, and assure each cycle step's temperature limits are enforced.
                    foreach (CPCR_OperationDescriptor cycle_operation in cycle_entry.Value.m_Operations)
                    {
                        string cycle_operation_name = cycle_operation.m_strOperationName;
                        switch (cycle_operation.m_OpType)
                        {
                            case ePCR_ProtocolOperationType.kPCR_Step:
                                {
                                    CPCR_ProtocolStepDefn step;
                                    if (protocol_obj.m_PCR_StepDefns.TryGetValue(cycle_operation_name.ToUpper(), out step) == false)
                                    {
                                        throw new Exception("Protocol semantic Error. Cycle \"" + cycle_entry.Value.m_strName + "\" references undefined Step \"" + cycle_operation_name + "\".");
                                    }
                                    // Check if cycle embedded step has valid temperature set-points.  If needed, clamp them to specified limits.
                                    if (step.m_fTemperatureinC < _model.Config.m_MiscellaneousConfiguration.m_fProtocolCycleStep_MinTemperature)
                                    {
                                        step.m_fTemperatureinC = _model.Config.m_MiscellaneousConfiguration.m_fProtocolCycleStep_MinTemperature;
                                        protocol_obj.m_PCR_StepDefns[cycle_operation_name] = step; // Replace with clamped value
                                        strWarning += "Cycle \"" + cycle_entry.Value.m_strName + "\" references Step \"" + cycle_operation_name + "\" with excessively low temperature set-point.  Value has been clamped.\n.";
                                    }
                                    if (step.m_fTemperatureinC > _model.Config.m_MiscellaneousConfiguration.m_fProtocolCycleStep_MaxTemperature)
                                    {
                                        step.m_fTemperatureinC = _model.Config.m_MiscellaneousConfiguration.m_fProtocolCycleStep_MaxTemperature;
                                        protocol_obj.m_PCR_StepDefns[cycle_operation_name] = step; // Replace with clamped value
                                        strWarning += "Cycle \"" + cycle_entry.Value.m_strName + "\" references Step \"" + cycle_operation_name + "\" with excessively high temperature set-point. Value has been clamped.\n.";
                                    }
                                }
                                break;

                            case ePCR_ProtocolOperationType.kPCR_Cycle:
                                {
                                    CPCR_ProtocolCycleDefn cycle;
                                    if (protocol_obj.m_PCR_CycleDefns.TryGetValue(cycle_operation_name.ToUpper(), out cycle) == false)
                                    {
                                        throw new Exception("Protocol semantic Error. Cycle \"" + cycle_entry.Value.m_strName + "\" references undefined Cycle \"" + cycle_operation_name + "\".");
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }

        private void ProtocolSemanticValidation(CSystem_Protocol_Obj protocol_obj, out string strWarnings)
        {
            strWarnings = "";
            ProtocolPneumaticProcessSemanticValidation(protocol_obj.m_Pneumatic_Process, protocol_obj.m_strChipType, out strWarnings);
            ProtocolPCRSemanticValidation(protocol_obj.m_PCR_Process, out strWarnings);
        }

        public void ReadProtocolFile(string strFileNamePath, out CSystem_Protocol_Obj protocol, out string strWarnings)
        {
            CSystem_Protocol_Obj protocol_obj = new CSystem_Protocol_Obj();
            protocol_obj.m_Test_Process = new CTest_Protocol_Process_Obj();
            protocol_obj.m_PCR_Process = new CSystem_Protocol_PCR_Process_Obj();
            protocol_obj.m_Pneumatic_Process = new CPneumaticProcess_Protocol_Process_Obj();

            strWarnings = "";

            // Strip all comments prior to parsing, as derived Positional_XmlElement class with line numbers can't handle XmlComment nodes
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            XmlReader reader = XmlReader.Create(strFileNamePath, settings);
            Positional_XMLDocument doc = new Positional_XMLDocument();

            try
            {
                doc.Load(reader);
            }
            catch (Exception Exc)
            {
                string strErrorMsg = "Error parsing protocol script document \"" + strFileNamePath + "\\n." + Exc.ToString() + "\n";
                throw new CPCRInstrumentSystemException(strErrorMsg);
            }

            // Parse Sample Preparation section
            Positional_XmlElement protocol_node = (Positional_XmlElement)doc.DocumentElement.SelectSingleNode("/protocol");       // protocol top-level node

            // Parse the Test section.
            Positional_XmlElement test_nodes = (Positional_XmlElement)protocol_node.SelectSingleNode("test"); // protocol test node

            if (test_nodes != null)
                ParseProtocolTestStatments(test_nodes, protocol_obj.m_Test_Process);

            // Parse Pneumatic statements
            Positional_XmlElement pneumatic_statement_nodes = (Positional_XmlElement)protocol_node.SelectSingleNode("pneumatics"); // protocol pneumatics node

            ParseProtocolPneumaticProcessStatements(pneumatic_statement_nodes, protocol_obj.m_Pneumatic_Process, out protocol_obj.m_strChipType);

            // Parse PCR section
            Positional_XmlElement pcr_protocol_node = (Positional_XmlElement)protocol_node.SelectSingleNode("pcr"); // protocol pcr node

            // Parse PCR Step nodes
            Positional_XmlElement pcr_step_nodes = (Positional_XmlElement)pcr_protocol_node.SelectSingleNode("steps");
            ParseProtocolPCRStepDefinitions(pcr_step_nodes, protocol_obj.m_PCR_Process);

            // Parse PCR Cycle node entries
            Positional_XmlElement pcr_cycle_nodes = (Positional_XmlElement)pcr_protocol_node.SelectSingleNode("cycles");
            ParseProtocolPCRCycleDefinitions(pcr_cycle_nodes, protocol_obj.m_PCR_Process);

            // Parse PCR Process Sequence node entries
            Positional_XmlElement pcr_process_sequence_node = (Positional_XmlElement)pcr_protocol_node.SelectSingleNode("process_sequence");
            ParseProtocolPCRProcessSequenceDefinition(pcr_process_sequence_node, protocol_obj.m_PCR_Process);

            // Perform semantic checks to confirm validity of protocol
            ProtocolSemanticValidation(protocol_obj, out strWarnings);

            protocol = protocol_obj;
        }
    }
}