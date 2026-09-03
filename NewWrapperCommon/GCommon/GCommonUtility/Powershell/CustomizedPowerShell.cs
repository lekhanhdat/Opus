/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */

namespace AvePoint.Adonis.PowerShell
{
    using AvePoint.GCommon;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Runspaces;
    using System.Reflection;
    using System.Text;

    [AveVersion("$Revision: 262573 $")]
    public class CustomizedPowerShell : IDisposable
    {
        #region Fields
        private List<Command> mCommands;
        private CustomizedPSHost mCustomizedPSHost;
        private RunspaceConfiguration mRunspaceConfiguration;
        private InitialSessionState mInitialSessionState;
        private PSSnapInException mSnapInEx;
        private StringBuilder mErrorMsg;
        private Runspace mRunspace;
        private Pipeline mPipeline;
        private bool mIsImportModule;
        private Collection<PSObject> mResultObjs;
        private Collection<Object> mErrorObjs;
        private PipelineState mPipelineState;
        #endregion

        #region Properties
        public bool IsImportModule
        {
            get { return mIsImportModule; }
        }

        /// <summary>
        /// Pipeline State
        /// </summary>
        public PipelineState PipelineState
        {
            get
            {
                return mPipelineState;
            }
        }

        /// <summary>
        /// Output from the CustomizedPSHost.
        /// </summary>
        public string CustomizedPSHostOutput
        {
            get
            {
                if (null != mCustomizedPSHost)
                {
                    return (mCustomizedPSHost.UI as CustomizedPSHostUserInterface).OutPut;
                }
                return string.Empty;
            }
        }
        
        /// <summary>
        /// Errors from pipeline invoke.
        /// </summary>
        public Collection<Object> Errors
        {
            get { return mErrorObjs; }
        }

        /// <summary>
        /// Results from pipeline invoke.
        /// </summary>
        public Collection<PSObject> Results
        {
            get { return mResultObjs; }
        }

        /// <summary>
        /// Error message about the whole operation.
        /// </summary>
        public string ErrorMsg
        {
            get { return mErrorMsg.ToString(); }
        }

        /// <summary>
        /// Used to modify RunspaceConfiguration entries for PSSnapIn.
        /// </summary>
        public RunspaceConfiguration RunspaceConfiguration
        {
            get { return mRunspace.RunspaceConfiguration; }
        }

        /// <summary>
        /// Used to modify InitialSessionState entries for Module.
        /// </summary>
        public InitialSessionState InitialSessionState
        {
            get { return mRunspace.InitialSessionState; }
        }
        #endregion

        #region Constructor
        public CustomizedPowerShell()
        {
            mErrorMsg = new StringBuilder(1024);
            mCommands = new List<Command>();
            mResultObjs = new Collection<PSObject>();
            mErrorObjs = new Collection<object>();
            mRunspaceConfiguration = RunspaceConfiguration.Create();
            mInitialSessionState = InitialSessionState.CreateDefault();
            mCustomizedPSHost = new CustomizedPSHost();
        }

        /// <summary>
        ///  Constructor for AddSnapIn
        /// </summary>
        /// <param name="snapInOrModule">SnapIn object</param>
        /// <param name="customizedPSHost">Customized PSHost, used for Stream output</param>
        public CustomizedPowerShell(string snapIn)
            : this()
        {
            try
            {
                AddSnapIn(snapIn);
            }
            catch (Exception ex)
            {
                mErrorMsg.AppendLine(string.Format("Check the registered PSSnapIn: {0}. Add PSSnapIn failed. Error: {1}.", snapIn, ex.Message));
            }
            // Create a run space that uses the host object and run the 
            // script using a PowerShell object.
            mRunspace = RunspaceFactory.CreateRunspace(mCustomizedPSHost, mRunspaceConfiguration);     
        }

        /// <summary>
        /// Constructor for ImportModule
        /// </summary>
        /// <param name="moduleNames">Module objects</param>
        /// <param name="customizedPSHost">Customized PSHost, used for Stream output</param>
        public CustomizedPowerShell(string[] moduleNames)
            : this()
        {
            mInitialSessionState.ImportPSModule(moduleNames);
            mRunspace = RunspaceFactory.CreateRunspace(mCustomizedPSHost, mInitialSessionState);
            mIsImportModule = true;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Add SnapIn
        /// </summary>
        /// <param name="snapIn">SnapIn Object</param>
        public void AddSnapIn(string snapIn)
        {
            mRunspaceConfiguration.AddPSSnapIn(snapIn, out mSnapInEx);
            if (mSnapInEx != null)
            {
                mErrorMsg.Append(mSnapInEx.ToString());
            }
        }

        /// <summary>
        /// Import Module
        /// </summary>
        /// <param name="moduleNames">Module objects</param>
        public void ImportModule(string[] moduleNames)
        {
            mInitialSessionState.ImportPSModule(moduleNames);
            mIsImportModule = true;
        }

        #region Add Customized EventHandler
        /// <summary>
        /// DataReady event for pipeline Output or Error
        /// </summary>
        /// <param name="eventType">Event type</param>
        /// <param name="eventObject">Event object</param>
        public void DataReadyEventAttach(EventType eventType, EventHandler eventObject)
        {
            if (null != mPipeline)
            {
                switch (eventType)
                {
                    case EventType.OutputDataReady:
                        mPipeline.Output.DataReady += eventObject;
                        break;
                    case EventType.ErrorDataReady:
                        mPipeline.Error.DataReady += eventObject;
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Event for pipeline state changed
        /// </summary>
        /// <param name="eventObject"></param>
        public void StateChangedEventAttach(EventHandler<PipelineStateEventArgs> eventObject)
        {
            if (null != mPipeline)
            {
                mPipeline.StateChanged += eventObject;
            }
        }
        #endregion

        #region Add or Clear Commands
        /// <summary>
        /// Add Scripts
        /// </summary>
        /// <param name="scripts">Scripts to add</param>
        public void AddCommands(List<string> scripts)
        {
            scripts.ForEach(item =>
                {
                    Command tempCommand = new Command(item, true);
                    mCommands.Add(tempCommand);
                });
        }

        /// <summary>
        /// Add Commands
        /// </summary>
        /// <param name="commands">Commands to add</param>
        public void AddCommands(List<Command> commands)
        {
            mCommands.AddRange(commands);
        }

        /// <summary>
        /// Add a command or script
        /// </summary>
        /// <param name="cmdlet">command or script</param>
        /// <param name="parameters">parameters for the command</param>
        /// <param name="isScript">Whether it's a script</param>
        public void AddCommand(string cmdlet, Dictionary<string, object> parameters, bool isScript = false)
        {
            Command tempCommand = null;
            if (!isScript)
            {
                tempCommand = new Command(cmdlet);
                if (parameters != null && parameters.Count > 0)
                {
                    foreach (KeyValuePair<string, object> parameter in parameters)
                    {
                        tempCommand.Parameters.Add(parameter.Key, parameter.Value);
                    }
                }
            }
            else
            {
                tempCommand = new Command(cmdlet, isScript);
            }
            mCommands.Add(tempCommand);
        }

        /// <summary>
        /// Clear Commands
        /// </summary>
        public void ClearCommands()
        {
            if (mCommands.Count > 0)
            {
                mCommands.Clear();
            }
        }
        #endregion

        #region Open and Run Commands
        /// <summary>
        /// After modify configuration or initialSessionState, open runspace and create pipeline
        /// </summary>
        public void Open()
        {
            if (null != mRunspace)
            {
                mRunspace.Open();
                mPipeline = mRunspace.CreatePipeline();
            }
        }

        /// <summary>
        /// Run PowerShell commands and return the output of CustomizedPSHost
        /// </summary>
        public string RunPSScript()
        {
            PrepareOperation();
            try
            {
                //Get pipeline realtime state
                mPipeline.StateChanged += Pipeline_StateChanged;
                mResultObjs = mPipeline.Invoke();
                if (mPipeline.Error != null && mPipeline.Error.Count > 0)
                {
                    mErrorObjs = mPipeline.Error.ReadToEnd();
                }
            }
            finally
            {
                Dispose();
            }
            return (mCustomizedPSHost.UI as CustomizedPSHostUserInterface).OutPut;
        }

        /// <summary>
        /// Run PowerShell commands async
        /// </summary>
        public void RunPSScriptAsync()
        {
            PrepareOperation();
            //Used to add items to the result collection or error collection and 
            //get pipeline realtime state
            mPipeline.Output.DataReady += Output_DataReady;
            mPipeline.Error.DataReady += Error_DataReady;
            mPipeline.StateChanged += Pipeline_StateChanged;

            mPipeline.InvokeAsync();
        }

        /// <summary>
        /// Help to create and Open Runspace, then create pipeline.
        /// Add commands to pipeline and then close pipeline input
        /// </summary>
        private void PrepareOperation()
        {
            if (null == mRunspace)
            {
                if (!IsImportModule)
                {
                    mRunspace = RunspaceFactory.CreateRunspace(mCustomizedPSHost, mRunspaceConfiguration);
                }
                else
                {
                    mRunspace = RunspaceFactory.CreateRunspace(mCustomizedPSHost, mInitialSessionState);
                }
            }
            if(null == mPipeline)
            {
                Open();
            }

            //Add Scripts and Commands
            foreach (Command itemCommand in mCommands)
            {
                mPipeline.Commands.Add(itemCommand);
            }
            mPipeline.Input.Close();
        }
        #endregion

        #region Default EventHandler for Invoke and Async Invoke
        private void Pipeline_StateChanged(object sender, PipelineStateEventArgs e)
        {
            mPipelineState = e.PipelineStateInfo.State;
        }

        private void Error_DataReady(object sender, EventArgs e)
        {
            Collection<Object> data = mPipeline.Error.NonBlockingRead();
            foreach (PSObject item in data)
            {
                mErrorObjs.Add(item);
            }
        }

        private void Output_DataReady(object sender, EventArgs e)
        {
            Collection<PSObject> data = mPipeline.Output.NonBlockingRead();
            foreach (PSObject item in data)
            {
                mResultObjs.Add(item);
            }
        }
        #endregion

        #region Release resources
        /// <summary>
        /// Release resources
        /// </summary>
        public void Dispose()
        {
            if (null != mPipeline)
            {
                mPipeline.Stop();
                mPipeline.Dispose();
                mPipeline = null;
            }
            if (null != mRunspace)
            {
                mRunspace.Dispose();
                mRunspace = null;
            }
        }
        #endregion

        #endregion
    }

    [AveVersion("$Revision: 262573 $")]
    public enum EventType
    {
        Unknown = 0,
        OutputDataReady,
        ErrorDataReady,
    }

    [AveVersion("$Revision: 262573 $")]
    public class CustomizedPSHost : PSHost
    {
        private CustomizedPSHostUserInterface ui;

        private CultureInfo originalCultureInfo =
            System.Threading.Thread.CurrentThread.CurrentCulture;

        private CultureInfo originalUICultureInfo =
            System.Threading.Thread.CurrentThread.CurrentUICulture;

        private Guid myId = Guid.NewGuid();

        public CustomizedPSHost()
        {
            ui = new CustomizedPSHostUserInterface();
        }

        public CustomizedPSHost(OutputFormat format)
        {
            ui = new CustomizedPSHostUserInterface(format);
        }

        public override System.Globalization.CultureInfo CurrentCulture
        {
            get { return this.originalCultureInfo; }
        }

        public override System.Globalization.CultureInfo CurrentUICulture
        {
            get { return this.originalUICultureInfo; }
        }

        public override Guid InstanceId
        {
            get { return this.myId; }
        }

        public override string Name
        {
            get { return "CustomizedPSHost"; }
        }

        public override PSHostUserInterface UI
        {
            get { return ui; }
        }

        public override Version Version
        {
            get { return new Version(6, 0); }
        }

        public override void EnterNestedPrompt()
        {
            throw new NotImplementedException(
                "The method or operation is not implemented.");
        }

        public override void ExitNestedPrompt()
        {
            throw new NotImplementedException(
                "The method or operation is not implemented.");
        }

        public override void NotifyBeginApplication()
        {
            return;
        }

        public override void NotifyEndApplication()
        {
            return;
        }

        public override void SetShouldExit(int exitCode)
        {
            return;
        }

        public string GetCustomizedPSAllOutput()
        {
            return ui.OutPut;
        }
    }
    [AveVersion("$Revision: 262573 $")]
    internal class CustomizedPSHostUserInterface : PSHostUserInterface
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private StringBuilder mResult;
        private CustomizedPSHostRawUserInterface mRawUserInterface;
        private OutputFormat mCustomizedFormat;

        public CustomizedPSHostUserInterface()
        {
            mResult = new StringBuilder();
            mRawUserInterface = new CustomizedPSHostRawUserInterface();
        }

        public CustomizedPSHostUserInterface(OutputFormat customizedFormat)
            : this()
        {
            mCustomizedFormat = customizedFormat;
        }

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            mResult.Append(value);
        }

        public override void Write(string value)
        {
            mResult.Append(value);
        }

        public override void WriteDebugLine(string message)
        {
            if (mCustomizedFormat == OutputFormat.SMHVFormat)
            {
                mResult.AppendLine(string.Format("[{0}]DEBUG: {1} ", DateTime.Now.TimeOfDay.ToString(), message));
            }
            else
            {
                mResult.AppendLine("DEBUG: " + message);
            }
        }

        public override void WriteErrorLine(string value)
        {
            if (mCustomizedFormat == OutputFormat.SMHVFormat)
            {
                mResult.AppendLine(string.Format("[{0}]ERROR: {1} ", DateTime.Now.TimeOfDay.ToString(), value));
            }
            else
            {
                mResult.AppendLine("ERROR: " + value);
            }
        }

        public override void WriteLine(string value)
        {
            mResult.AppendLine(value);
        }

        public override void WriteVerboseLine(string message)
        {
            if (mCustomizedFormat == OutputFormat.SMHVFormat)
            {
                mResult.AppendLine(string.Format("[{0}]VERBOSE: {1} ", DateTime.Now.TimeOfDay.ToString(), message));
            }
            else
            {
                mResult.AppendLine("VERBOSE: " + message);
            }
        }

        public override void WriteWarningLine(string message)
        {
            if (mCustomizedFormat == OutputFormat.SMHVFormat)
            {
                mResult.AppendLine(string.Format("[{0}]WARN: {1} ", DateTime.Now.TimeOfDay.ToString(), message));
            }
            else
            {
                mResult.AppendLine("WARN: " + message);
            }
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            return;
        }

        public string OutPut
        {
            get { return mResult.ToString(); }
        }

        public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
        {
            throw new NotImplementedException();
        }

        public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
        {
            //throw new NotImplementedException();
            try
            {
                mLog.Log(AveLogLevel.INFO, "PSHost: prompt for choice, caption: {0}; message: {1}; choice: {2}.", caption, message, choices[defaultChoice].Label);
            }
            catch (Exception e)
            {
                mLog.Debug(e.ToString());
            }
            return defaultChoice;
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
        {
            throw new NotImplementedException();
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName,
            PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            throw new NotImplementedException();
        }

        public override PSHostRawUserInterface RawUI
        {
            get { return mRawUserInterface; }
        }

        public override string ReadLine()
        {
            throw new NotImplementedException();
        }

        public override System.Security.SecureString ReadLineAsSecureString()
        {
            throw new NotImplementedException();
        }
    }
    [AveVersion("$Revision: 262573 $")]
    internal class CustomizedPSHostRawUserInterface : PSHostRawUserInterface
    {
        public override ConsoleColor BackgroundColor
        {
            get { return Console.BackgroundColor; }
            set { Console.BackgroundColor = value; }
        }

        public override Size BufferSize
        {
            get { return new Size(Console.BufferWidth, Console.BufferHeight); }
            set { Console.SetBufferSize(value.Width, value.Height); }
        }

        public override Coordinates CursorPosition
        {
            get
            {
                return new Coordinates(Console.CursorLeft, Console.CursorTop);
            }
            set
            {
                Console.SetCursorPosition(value.X, value.Y);
            }
        }

        public override int CursorSize
        {
            get { return Console.CursorSize; }
            set { Console.CursorSize = value; }
        }

        public override ConsoleColor ForegroundColor
        {
            get { return Console.ForegroundColor; }
            set { Console.ForegroundColor = value; }
        }

        public override bool KeyAvailable
        {
            get { return Console.KeyAvailable; }
        }

        public override Size MaxPhysicalWindowSize
        {
            get { return new Size(Console.LargestWindowWidth, Console.LargestWindowHeight); }
        }

        public override Size MaxWindowSize
        {
            get { return new Size(Console.LargestWindowWidth, Console.LargestWindowHeight); }
        }

        public override Coordinates WindowPosition
        {
            get { return new Coordinates(Console.WindowLeft, Console.WindowTop); }
            set { Console.SetWindowPosition(value.X, value.Y); }
        }

        public override Size WindowSize
        {
            get { return new Size(Console.WindowWidth, Console.WindowHeight); }
            set { Console.SetWindowSize(value.Width, value.Height); }
        }

        public override string WindowTitle
        {
            get { return Console.Title; }
            set { Console.Title = value; }
        }

        public override void FlushInputBuffer()
        {
        }

        public override BufferCell[,] GetBufferContents(Rectangle rectangle)
        {
            throw new NotImplementedException(
                     "The method or operation is not implemented.");
        }

        public override KeyInfo ReadKey(ReadKeyOptions options)
        {
            throw new NotImplementedException(
                      "The method or operation is not implemented.");
        }

        public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill)
        {
            throw new NotImplementedException(
                      "The method or operation is not implemented.");
        }

        public override void SetBufferContents(Coordinates origin,
                                               BufferCell[,] contents)
        {
            throw new NotImplementedException(
                      "The method or operation is not implemented.");
        }

        public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
        {
            throw new NotImplementedException(
                      "The method or operation is not implemented.");
        }

    }

    [AveVersion("$Revision: 262573 $")]
    public enum OutputFormat
    {
        UnDefined,
        SMHVFormat
    }
}
