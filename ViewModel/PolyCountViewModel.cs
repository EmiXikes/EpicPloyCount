using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using CADHelper;
using System.Runtime.InteropServices;
using System.Diagnostics;
using DistriBo;

namespace EpicPloyCount.ViewModel
{
    public class PolyCountViewModel : Model.INPC
    {
        public PolyCountViewModel()
        {
            // Constructor
            MainWindowInstance = (MainWindow)App.Current.MainWindow;
            PolyCountData = new Model.PolyCountModel();

            // Buttons
            ContinueMeasurementSequence = new RCommand(continueMeasurementSequence);
            AddMeasurementToStatic = new RCommand(addMeasurementToStatic);
            StartNew = new RCommand(startNew);
            ClearStatic = new RCommand(clearStatic);
            ClearAll = new RCommand(clearAll);
            WriteAttribute = new RCommand(writeAttribute);
            CloseApplication = new RCommand(closeApplication);
            WindowLoaded = new RCommand(windowLoaded);

            ValueToClipboard = new RCommand(valueToClipboard);

            // Start Launching

            // Mouse & KB hooks
            MouseHookUnsubscribe();
            MouseHookSubscribe(Hook.GlobalEvents());


            // Initial values
            isLockedToCursor = true;
            IsControlsVisible = false;
            CopyTagVisible = false;
            PolyCountData.ActiveMeasurement = 0;
            PolyCountData.StaticMeasurement = 0;
            ConversionMultiplier = 0.001;
            //TopMost = true;
            //startMeasurementSequence();
        }



        Model.PolyCountModel PolyCountData;
        MainWindow MainWindowInstance;


        #region Bindables

        public bool IsControlsVisible
        {
            get => isControlsVisible; set
            {
                isControlsVisible = value;
                MyPropertyChanged(nameof(IsControlsVisible));
            }
        }
        public bool CopyTagVisible
        {
            get => copyTagVisible; set
            {
                copyTagVisible = value;
                MyPropertyChanged(nameof(CopyTagVisible));
            }
        }

        public string DisplayedResult
        {
            get 
            {
                return ComposedResult(); 
            }
        }
        public string DisplayedDetailedResult
        {
            get 
            { 
                return DetailedResult(); 
            }
        }
        public ICommand ContinueMeasurementSequence { get; set; }
        public ICommand AddMeasurementToStatic { get; set; }
        public ICommand StartNew { get; set; }
        public ICommand ValueToClipboard { get; set; }
        public ICommand ClearStatic { get; set; }
        public ICommand ClearAll { get; set; }
        public ICommand WriteAttribute { get; set; }
        public ICommand CloseApplication { get; set; }
        public ICommand WindowLoaded { get; set; }
        public bool TopMost
        {
            get => topMost; set
            {
                topMost = value;
                MyPropertyChanged(nameof(TopMost));
            }
        }

        private bool topMost;



        #endregion

        #region Helpers and privates

        // Private fields for Bindable properties

        private bool isControlsVisible;
        private bool copyTagVisible;

        // Button Methods

        private void continueMeasurementSequence(object param)
        {
            isLockedToCursor = true;
            IsControlsVisible = false;
            startMeasurementSequence();
        }
        private void addMeasurementToStatic(object obj)
        {
            PolyCountData.StaticMeasurement = PolyCountData.ActiveMeasurement;
            PolyCountData.ActiveMeasurement = 0;
            updateProperties();
        }
        private void startNew(object obj)
        {
            isLockedToCursor = true;
            IsControlsVisible = false;
            PolyCountData.ActiveMeasurement = 0;
            updateProperties();

            startMeasurementSequence();
        }
        private void clearStatic(object obj)
        {
            PolyCountData.StaticMeasurement = 0;
            updateProperties();
        }
        private void clearAll(object obj)
        {
            PolyCountData.ActiveMeasurement = 0;
            PolyCountData.StaticMeasurement = 0;
            updateProperties();
        }
        private void writeAttribute(object obj)
        {
            //isLockedToCursor = true;
            //IsControlsVisible = false;

            CADHelper.ShadowEngineGlobal.InitShadowEngine();

            TopMost = false;
            AcadProcess = xWinHelper.xWinHelper.xLastProcByName("acad");
            TopMost = true;

            ddeThreadID = ShadowEngineGlobal.xDDEcontainedInProcess(AcadProcess);

            // xWinHelper.xWinHelper.xActivateWin(AcadProcess.MainWindowHandle);

            Commands.SendDDEString(ddeThreadID, "LOGFILEON\n");
            Commands.SendDDEString(ddeThreadID, "LOGFILEPATH\n" + "c:\\epic\\log\\\n");

            Commands.SeqAttributeEdit(ddeThreadID, Incr, SeqStop, NewAttrValue, false);

            xWinHelper.xWinHelper.xActivateWin(AcadProcess.MainWindowHandle);
           
        }
        private void valueToClipboard(object obj)
        {
            Clipboard.SetText(obj.ToString());
            CopyTagVisible = true;
        }
        private string NewAttrValue()
        {
            return DisplayedResult;
        }
        private string SeqStop(MacroFailState FailSeverity, string Message)
        {
            //isLockedToCursor = false;
            //IsControlsVisible = true;
            return "";
            throw new NotImplementedException();
        }
        private int Incr()
        {
            return 0;
            throw new NotImplementedException();
        }

        private void closeApplication(object obj)
        {
            MouseHookUnsubscribe();
            App.Current.Shutdown();
        }
        private void windowLoaded(object obj)
        {
            startMeasurementSequence();
        }


        // Sequence Stuff

        private Process AcadProcess;
        private uint ddeThreadID;
        private IntPtr WinHandle;

        private double ConversionMultiplier;
        private string ComposedResult()
        {
            double Result;
            Result = PolyCountData.ActiveMeasurement + PolyCountData.StaticMeasurement;
            Result = Result * ConversionMultiplier;
            Result = Math.Round(Result, 0);

            double subR = Result / 5;
            double subRdecimals = subR - Math.Truncate(subR);
            if (subRdecimals != 0)
            {
                subR = Math.Truncate(subR) + 1;
            }
            else
            {
                subR = Math.Truncate(subR);
            }

            Result = subR * 5;


            return Result.ToString();
        }
        private string DetailedResult()
        {
            string Result;
            Result = "[ " + Math.Round(PolyCountData.StaticMeasurement * ConversionMultiplier, 2).ToString() + " ]" + " + " +
                Math.Round(PolyCountData.ActiveMeasurement * ConversionMultiplier, 2).ToString();

            return Result;
        }
        private void updateProperties()
        {
            MyPropertyChanged(nameof(DisplayedResult));
            MyPropertyChanged(nameof(DisplayedDetailedResult));
        }


        private void startMeasurementSequence()
        {
            CADHelper.ShadowEngineGlobal.InitShadowEngine();

            TopMost = false;
            AcadProcess = xWinHelper.xWinHelper.xLastProcByName("acad");
            TopMost = true;

            ddeThreadID = ShadowEngineGlobal.xDDEcontainedInProcess(AcadProcess);

            

            Commands.SendDDEString(ddeThreadID, "LOGFILEON\n");
            Commands.SendDDEString(ddeThreadID, "LOGFILEPATH\n" + "c:\\epic\\log\\\n");

            Commands.SetVar(ddeThreadID, "INSUNITS", "", getUnits, null);
            //Commands.SeqPolyLengthDelete(ddeThreadID, SequenceStop, SelectedPolyLength);

            //xWinHelper.xWinHelper.xActivateWin(AcadProcess.MainWindowHandle);

            //CADHelper.ACADinfo.ActivateLastAutoCAD();
        }

        private string getUnits(string VarValue)
        {
            if (VarValue == "6")
            {
                ConversionMultiplier = 1;
            }
            else
            {
                ConversionMultiplier = 0.001;
            }
            
            Commands.SeqPolyLengthDelete(ddeThreadID, SequenceStop, SelectedPolyLength);
            xWinHelper.xWinHelper.xActivateWin(AcadProcess.MainWindowHandle);

            return VarValue;
        }

        private string SelectedPolyLength(double Length)
        {
            PolyCountData.ActiveMeasurement = PolyCountData.ActiveMeasurement + Length;
            updateProperties();
            return "";
        }
        private string SequenceStop(MacroFailState FailSeverity, string Message)
        {
            isLockedToCursor = false;
            IsControlsVisible = true;
            return "";
        }

        #endregion

        #region Mouse & Keyboard hooks

        private bool isLockedToCursor;
        private IKeyboardMouseEvents m_Events;

        private void MouseHookSubscribe(IKeyboardMouseEvents events)
        {
            m_Events = events;
            m_Events.MouseMove += HookManager_MouseMove;
            m_Events.MouseClick += HookManager_MouseClick;
            m_Events.KeyPress += HookManager_KeyPress;
        }

        private void HookManager_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                isLockedToCursor = false;
                IsControlsVisible = true;
            }
        }

        private void MouseHookUnsubscribe()
        {
            if (m_Events == null) return;
            m_Events.MouseMove -= HookManager_MouseMove;
            m_Events.KeyPress -= HookManager_KeyPress;
            m_Events.Dispose();
            m_Events = null;
        }
        private void HookManager_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)27)
            {
                isLockedToCursor = false;
                IsControlsVisible = true;
            }             
            
        }
        private void HookManager_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            CopyTagVisible = false;

            if(isLockedToCursor == true)
            {
                MainWindowInstance.Top = e.Y + 10;
                MainWindowInstance.Left = e.X + 5;
            }
        }

        #endregion
    }
}
