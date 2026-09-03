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



namespace AvePoint.GCommon.Utility
{
    #region using directives
    using System.Collections;
    using System.ComponentModel;
    using System.Management;
    using System;
    #endregion

    internal class PerfFormattedData_PerfProc_Process : System.ComponentModel.Component
    {
        private static string CreatedWmiNamespace = "root\\cimv2";
        private static string CreatedClassName = "Win32_PerfFormattedData_PerfProc_Process";
        private static System.Management.ManagementScope statMgmtScope = null;
        private ManagementSystemProperties PrivateSystemProperties;
        private System.Management.ManagementObject PrivateLateBoundObject;
        private bool AutoCommitProp;
        private System.Management.ManagementBaseObject embeddedObj;
        private System.Management.ManagementBaseObject curObj;
        private bool isEmbedded;

        public PerfFormattedData_PerfProc_Process()
        {
            this.InitializeObject(null, null, null);
        }
        public PerfFormattedData_PerfProc_Process(string keyName)
        {
            this.InitializeObject(null, new System.Management.ManagementPath(PerfFormattedData_PerfProc_Process.ConstructPath(keyName)), null);
        }
        public PerfFormattedData_PerfProc_Process(System.Management.ManagementScope mgmtScope, string keyName)
        {
            this.InitializeObject(((System.Management.ManagementScope)(mgmtScope)), new System.Management.ManagementPath(PerfFormattedData_PerfProc_Process.ConstructPath(keyName)), null);
        }
        public PerfFormattedData_PerfProc_Process(System.Management.ManagementPath path, System.Management.ObjectGetOptions getOptions)
        {
            this.InitializeObject(null, path, getOptions);
        }
        public PerfFormattedData_PerfProc_Process(System.Management.ManagementScope mgmtScope, System.Management.ManagementPath path)
        {
            this.InitializeObject(mgmtScope, path, null);
        }
        public PerfFormattedData_PerfProc_Process(System.Management.ManagementPath path)
        {
            this.InitializeObject(null, path, null);
        }
        public PerfFormattedData_PerfProc_Process(System.Management.ManagementScope mgmtScope, System.Management.ManagementPath path, System.Management.ObjectGetOptions getOptions)
        {
            this.InitializeObject(mgmtScope, path, getOptions);
        }
        public PerfFormattedData_PerfProc_Process(System.Management.ManagementObject theObject)
        {
            Initialize();
            if ((CheckIfProperClass(theObject) == true))
            {
                PrivateLateBoundObject = theObject;
                PrivateSystemProperties = new ManagementSystemProperties(PrivateLateBoundObject);
                curObj = PrivateLateBoundObject;
            }
            else
            {
                throw new System.ArgumentException("Class name does not match.");
            }
        }

        public PerfFormattedData_PerfProc_Process(System.Management.ManagementBaseObject theObject)
        {
            Initialize();
            if ((CheckIfProperClass(theObject) == true))
            {
                embeddedObj = theObject;
                PrivateSystemProperties = new ManagementSystemProperties(theObject);
                curObj = embeddedObj;
                isEmbedded = true;
            }
            else
            {
                throw new System.ArgumentException("Class name does not match.");
            }
        }

       
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string OriginatingNamespace
        {
            get
            {
                return "root\\cimv2";
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ManagementClassName
        {
            get
            {
                string strRet = CreatedClassName;
                if ((curObj != null))
                {
                    if ((curObj.ClassPath != null))
                    {
                        strRet = ((string)(curObj["__CLASS"]));
                        if (((strRet == null)
                                    || (strRet == string.Empty)))
                        {
                            strRet = CreatedClassName;
                        }
                    }
                }
                return strRet;
            }
        }

        // Property pointing to an embedded object to get System properties of the WMI object.
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ManagementSystemProperties SystemProperties
        {
            get
            {
                return PrivateSystemProperties;
            }
        }

        // Property returning the underlying lateBound object.
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Management.ManagementBaseObject LateBoundObject
        {
            get
            {
                return curObj;
            }
        }

        // ManagementScope of the object.
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Management.ManagementScope Scope
        {
            get
            {
                if ((isEmbedded == false))
                {
                    return PrivateLateBoundObject.Scope;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if ((isEmbedded == false))
                {
                    PrivateLateBoundObject.Scope = value;
                }
            }
        }

        // Property to show the commit behavior for the WMI object. If true, WMI object will be automatically saved after each property modification.(ie. Put() is called after modification of a property).
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AutoCommit
        {
            get
            {
                return AutoCommitProp;
            }
            set
            {
                AutoCommitProp = value;
            }
        }

        // The ManagementPath of the underlying WMI object.
        [Browsable(true)]
        public System.Management.ManagementPath Path
        {
            get
            {
                if ((isEmbedded == false))
                {
                    return PrivateLateBoundObject.Path;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if ((isEmbedded == false))
                {
                    if ((CheckIfProperClass(null, value, null) != true))
                    {
                        throw new System.ArgumentException("Class name does not match.");
                    }
                    PrivateLateBoundObject.Path = value;
                }
            }
        }

        // Public static scope property which is used by the various methods.
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static System.Management.ManagementScope StaticScope
        {
            get
            {
                return statMgmtScope;
            }
            set
            {
                statMgmtScope = value;
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("A short textual description (one-line string) for the statistic or metric.")]
        public string Caption
        {
            get
            {
                return ((string)(curObj["Caption"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsCreatingProcessIDNull
        {
            get
            {
                if ((curObj["CreatingProcessID"] == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The Creating Process ID value is the Process ID of the process that created the p" +
            "rocess. The creating process may have terminated, so this value may no longer id" +
            "entify a running process.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint CreatingProcessID
        {
            get
            {
                if ((curObj["CreatingProcessID"] == null))
                {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["CreatingProcessID"]));
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("A textual description of the statistic or metric.")]
        public string Description
        {
            get
            {
                return ((string)(curObj["Description"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsElapsedTimeNull
        {
            get
            {
                if ((curObj["ElapsedTime"] == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The total elapsed time, in seconds, that this process has been running.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public ulong ElapsedTime
        {
            get
            {
                if ((curObj["ElapsedTime"] == null))
                {
                    return System.Convert.ToUInt64(0);
                }
                return ((ulong)(curObj["ElapsedTime"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsFrequency_ObjectNull
        {
            get
            {
                if ((curObj["Frequency_Object"] == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public ulong Frequency_Object
        {
            get
            {
                if ((curObj["Frequency_Object"] == null))
                {
                    return System.Convert.ToUInt64(0);
                }
                return ((ulong)(curObj["Frequency_Object"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsFrequency_PerfTimeNull
        {
            get
            {
                if ((curObj["Frequency_PerfTime"] == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public ulong Frequency_PerfTime
        {
            get
            {
                if ((curObj["Frequency_PerfTime"] == null))
                {
                    return System.Convert.ToUInt64(0);
                }
                return ((ulong)(curObj["Frequency_PerfTime"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsFrequency_Sys100NSNull
        {
            get
            {
                if ((curObj["Frequency_Sys100NS"] == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public ulong Frequency_Sys100NS
        {
            get
            {
                if ((curObj["Frequency_Sys100NS"] == null))
                {
                    return System.Convert.ToUInt64(0);
                }
                return ((ulong)(curObj["Frequency_Sys100NS"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsHandleCountNull
        {
            get
            {
                if ((curObj["HandleCount"] == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The total number of handles currently open by this process. This number is equal " +
            "to the sum of the handles currently open by each thread in this process.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint HandleCount
        {
            get
            {
                if ((curObj["HandleCount"] == null))
                {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["HandleCount"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsIDProcessNull
        {
            get
            {
                if ((curObj["IDProcess"] == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("ID Process is the unique identifier of this process. ID Process numbers are reuse" +
            "d, so they only identify a process for the lifetime of that process.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint IDProcess
        {
            get
            {
                if ((curObj["IDProcess"] == null))
                {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["IDProcess"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsIODataBytesPersecNull
        {
            get
            {
                if ((curObj["IODataBytesPersec"] == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The rate at which the process is reading and writing bytes in I/O operations. Thi" +
            "s counter counts all I/O activity generated by the process to include file, netw" +
            "ork and device I/Os.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public ulong IODataBytesPersec
        {
            get
            {
                if ((curObj["IODataBytesPersec"] == null))
                {
                    return System.Convert.ToUInt64(0);
                }
                return ((ulong)(curObj["IODataBytesPersec"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsIODataOperationsPersecNull
        {
            get
            {
                if ((curObj["IODataOperationsPersec"] == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The rate at which the process is issuing read and write I/O operations. This coun" +
            "ter counts all I/O activity generated by the process to include file, network an" +
            "d device I/Os.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public ulong IODataOperationsPersec
        {
            get
            {
                if ((curObj["IODataOperationsPersec"] == null))
                {
                    return System.Convert.ToUInt64(0);
                }
                return ((ulong)(curObj["IODataOperationsPersec"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsIOOtherBytesPersecNull
        {
            get
            {
                if ((curObj["IOOtherBytesPersec"] == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The rate at which the process is issuing bytes to I/O operations that do not invo" +
            "lve data such as control operations. This counter counts all I/O activity genera" +
            "ted by the process to include file, network and device I/Os.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public ulong IOOtherBytesPersec
        {
            get
            {
                if ((curObj["IOOtherBytesPersec"] == null))
                {
                    return System.Convert.ToUInt64(0);
                }
                return ((ulong)(curObj["IOOtherBytesPersec"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsIOOtherOperationsPersecNull
        {
            get
            {
                if ((curObj["IOOtherOperationsPersec"] == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The rate at which the process is issuing I/O operations that are neither read nor" +
            " write operations (for example, a control function). This counter counts all I/O" +
            " activity generated by the process to include file, network and device I/Os.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public ulong IOOtherOperationsPersec
        {
            get
            {
                if ((curObj["IOOtherOperationsPersec"] == null))
                {
                    return System.Convert.ToUInt64(0);
                }
                return ((ulong)(curObj["IOOtherOperationsPersec"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsIOReadBytesPersecNull
        {
            get
            {
                if ((curObj["IOReadBytesPersec"] == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The rate at which the process is reading bytes from I/O operations. This counter " +
            "counts all I/O activity generated by the process to include file, network and de" +
            "vice I/Os.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public ulong IOReadBytesPersec
        {
            get
            {
                if ((curObj["IOReadBytesPersec"] == null))
                {
                    return System.Convert.ToUInt64(0);
                }
                return ((ulong)(curObj["IOReadBytesPersec"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsIOReadOperationsPersecNull
        {
            get
            {
                if ((curObj["IOReadOperationsPersec"] == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The rate at which the process is issuing read I/O operations. This counter counts" +
            " all I/O activity generated by the process to include file, network and device I" +
            "/Os.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public ulong IOReadOperationsPersec
        {
            get
            {
                if ((curObj["IOReadOperationsPersec"] == null))
                {
                    return System.Convert.ToUInt64(0);
                }
                return ((ulong)(curObj["IOReadOperationsPersec"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsIOWriteBytesPersecNull
        {
            get
            {
                if ((curObj["IOWriteBytesPersec"] == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The rate at which the process is writing bytes to I/O operations. This counter co" +
            "unts all I/O activity generated by the process to include file, network and devi" +
            "ce I/Os.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public ulong IOWriteBytesPersec
        {
            get
            {
                if ((curObj["IOWriteBytesPersec"] == null))
                {
                    return System.Convert.ToUInt64(0);
                }
                return ((ulong)(curObj["IOWriteBytesPersec"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsIOWriteOperationsPersecNull
        {
            get
            {
                if ((curObj["IOWriteOperationsPersec"] == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The rate at which the process is issuing write I/O operations. This counter count" +
            "s all I/O activity generated by the process to include file, network and device " +
            "I/Os.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public ulong IOWriteOperationsPersec
        {
            get
            {
                if ((curObj["IOWriteOperationsPersec"] == null))
                {
                    return System.Convert.ToUInt64(0);
                }
                return ((ulong)(curObj["IOWriteOperationsPersec"]));
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The Name property defines the label by which the statistic or metric is known. Wh" +
            "en subclassed, the property can be overridden to be a Key property. ")]
        public string Name
        {
            get
            {
                return ((string)(curObj["Name"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPageFaultsPersecNull
        {
            get
            {
                if ((curObj["PageFaultsPersec"] == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"Page Faults/sec is the rate at which page faults by the threads executing in this process are occurring.  A page fault occurs when a thread refers to a virtual memory page that is not in its working set in main memory. This may not cause the page to be fetched from disk if it is on the standby list and hence already in main memory, or if it is in use by another process with whom the page is shared.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint PageFaultsPersec
        {
            get
            {
                if ((curObj["PageFaultsPersec"] == null))
                {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["PageFaultsPersec"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPageFileBytesNull
        {
            get
            {
                if ((curObj["PageFileBytes"] == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"Page File Bytes is the current amount of virtual memory, in bytes, that this process has reserved for use in the paging file(s). Paging files are used to store pages of memory used by the process that are not contained in other files. Paging files are shared by all processes, and the lack of space in paging files can prevent other processes from allocating memory. If there is no paging file, this counter reflects the current amount of virtual memory that the process has reserved for use in physical memory.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public ulong PageFileBytes
        {
            get
            {
                if ((curObj["PageFileBytes"] == null))
                {
                    return System.Convert.ToUInt64(0);
                }
                return ((ulong)(curObj["PageFileBytes"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPageFileBytesPeakNull
        {
            get
            {
                if ((curObj["PageFileBytesPeak"] == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"Page File Bytes Peak is the maximum amount of virtual memory, in bytes, that this process has reserved for use in the paging file(s). Paging files are used to store pages of memory used by the process that are not contained in other files.  Paging files are shared by all processes, and the lack of space in paging files can prevent other processes from allocating memory. If there is no paging file, this counter reflects the maximum amount of virtual memory that the process has reserved for use in physical memory.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public ulong PageFileBytesPeak
        {
            get
            {
                if ((curObj["PageFileBytesPeak"] == null))
                {
                    return System.Convert.ToUInt64(0);
                }
                return ((ulong)(curObj["PageFileBytesPeak"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPercentPrivilegedTimeNull
        {
            get
            {
                if ((curObj["PercentPrivilegedTime"] == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"% Privileged Time is the percentage of elapsed time that the process threads spent executing code in privileged mode. When a Windows system service is called, the service will often run in privileged mode to gain access to system-private data. Such data is protected from access by threads executing in user mode. Calls to the system can be explicit or implicit, such as page faults or interrupts. Unlike some early operating systems, Windows uses process boundaries for subsystem protection in addition to the traditional protection of user and privileged modes. Some work done by Windows on behalf of the application might appear in other subsystem processes in addition to the privileged time in the process.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public ulong PercentPrivilegedTime
        {
            get
            {
                if ((curObj["PercentPrivilegedTime"] == null))
                {
                    return System.Convert.ToUInt64(0);
                }
                return ((ulong)(curObj["PercentPrivilegedTime"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPercentProcessorTimeNull
        {
            get
            {
                if ((curObj["PercentProcessorTime"] == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"% Processor Time is the percentage of elapsed time that all of process threads used the processor to execution instructions. An instruction is the basic unit of execution in a computer, a thread is the object that executes instructions, and a process is the object created when a program is run. Code executed to handle some hardware interrupts and trap conditions are included in this count.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public ulong PercentProcessorTime
        {
            get
            {
                if ((curObj["PercentProcessorTime"] == null))
                {
                    return System.Convert.ToUInt64(0);
                }
                return ((ulong)(curObj["PercentProcessorTime"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPercentUserTimeNull
        {
            get
            {
                if ((curObj["PercentUserTime"] == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"% User Time is the percentage of elapsed time that the process threads spent executing code in user mode. Applications, environment subsystems, and integral subsystems execute in user mode. Code executing in user mode cannot damage the integrity of the Windows executive, kernel, and device drivers. Unlike some early operating systems, Windows uses process boundaries for subsystem protection in addition to the traditional protection of user and privileged modes. Some work done by Windows on behalf of the application might appear in other subsystem processes in addition to the privileged time in the process.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public ulong PercentUserTime
        {
            get
            {
                if ((curObj["PercentUserTime"] == null))
                {
                    return System.Convert.ToUInt64(0);
                }
                return ((ulong)(curObj["PercentUserTime"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPoolNonpagedBytesNull
        {
            get
            {
                if ((curObj["PoolNonpagedBytes"] == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"Pool Nonpaged Bytes is the size, in bytes, of the nonpaged pool, an area of system memory (physical memory used by the operating system) for objects that cannot be written to disk, but must remain in physical memory as long as they are allocated.  Memory\\Pool Nonpaged Bytes is calculated differently than Process\\Pool Nonpaged Bytes, so it might not equal Process\\Pool Nonpaged Bytes\\_Total.  This counter displays the last observed value only; it is not an average.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint PoolNonpagedBytes
        {
            get
            {
                if ((curObj["PoolNonpagedBytes"] == null))
                {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["PoolNonpagedBytes"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPoolPagedBytesNull
        {
            get
            {
                if ((curObj["PoolPagedBytes"] == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"Pool Paged Bytes is the size, in bytes, of the paged pool, an area of system memory (physical memory used by the operating system) for objects that can be written to disk when they are not being used.  Memory\\Pool Paged Bytes is calculated differently than Process\\Pool Paged Bytes, so it might not equal Process\\Pool Paged Bytes\\_Total. This counter displays the last observed value only; it is not an average.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint PoolPagedBytes
        {
            get
            {
                if ((curObj["PoolPagedBytes"] == null))
                {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["PoolPagedBytes"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPriorityBaseNull
        {
            get
            {
                if ((curObj["PriorityBase"] == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The current base priority of this process. Threads within a process can raise and" +
            " lower their own base priority relative to the process\' base priority.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint PriorityBase
        {
            get
            {
                if ((curObj["PriorityBase"] == null))
                {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["PriorityBase"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPrivateBytesNull
        {
            get
            {
                if ((curObj["PrivateBytes"] == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("Private Bytes is the current size, in bytes, of memory that this process has allo" +
            "cated that cannot be shared with other processes.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public ulong PrivateBytes
        {
            get
            {
                if ((curObj["PrivateBytes"] == null))
                {
                    return System.Convert.ToUInt64(0);
                }
                return ((ulong)(curObj["PrivateBytes"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsThreadCountNull
        {
            get
            {
                if ((curObj["ThreadCount"] == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The number of threads currently active in this process. An instruction is the bas" +
            "ic unit of execution in a processor, and a thread is the object that executes in" +
            "structions. Every running process has at least one thread.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint ThreadCount
        {
            get
            {
                if ((curObj["ThreadCount"] == null))
                {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["ThreadCount"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsTimestamp_ObjectNull
        {
            get
            {
                if ((curObj["Timestamp_Object"] == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public ulong Timestamp_Object
        {
            get
            {
                if ((curObj["Timestamp_Object"] == null))
                {
                    return System.Convert.ToUInt64(0);
                }
                return ((ulong)(curObj["Timestamp_Object"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsTimestamp_PerfTimeNull
        {
            get
            {
                if ((curObj["Timestamp_PerfTime"] == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public ulong Timestamp_PerfTime
        {
            get
            {
                if ((curObj["Timestamp_PerfTime"] == null))
                {
                    return System.Convert.ToUInt64(0);
                }
                return ((ulong)(curObj["Timestamp_PerfTime"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsTimestamp_Sys100NSNull
        {
            get
            {
                if ((curObj["Timestamp_Sys100NS"] == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public ulong Timestamp_Sys100NS
        {
            get
            {
                if ((curObj["Timestamp_Sys100NS"] == null))
                {
                    return System.Convert.ToUInt64(0);
                }
                return ((ulong)(curObj["Timestamp_Sys100NS"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsVirtualBytesNull
        {
            get
            {
                if ((curObj["VirtualBytes"] == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"Virtual Bytes is the current size, in bytes, of the virtual address space the process is using. Use of virtual address space does not necessarily imply corresponding use of either disk or main memory pages. Virtual space is finite, and the process can limit its ability to load libraries.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public ulong VirtualBytes
        {
            get
            {
                if ((curObj["VirtualBytes"] == null))
                {
                    return System.Convert.ToUInt64(0);
                }
                return ((ulong)(curObj["VirtualBytes"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsVirtualBytesPeakNull
        {
            get
            {
                if ((curObj["VirtualBytesPeak"] == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"Virtual Bytes Peak is the maximum size, in bytes, of virtual address space the process has used at any one time. Use of virtual address space does not necessarily imply corresponding use of either disk or main memory pages. However, virtual space is finite, and the process might limit its ability to load libraries.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public ulong VirtualBytesPeak
        {
            get
            {
                if ((curObj["VirtualBytesPeak"] == null))
                {
                    return System.Convert.ToUInt64(0);
                }
                return ((ulong)(curObj["VirtualBytesPeak"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsWorkingSetNull
        {
            get
            {
                if ((curObj["WorkingSet"] == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"Working Set is the current size, in bytes, of the Working Set of this process. The Working Set is the set of memory pages touched recently by the threads in the process. If free memory in the computer is above a threshold, pages are left in the Working Set of a process even if they are not in use.  When free memory falls below a threshold, pages are trimmed from Working Sets. If they are needed they will then be soft-faulted back into the Working Set before leaving main memory.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public ulong WorkingSet
        {
            get
            {
                if ((curObj["WorkingSet"] == null))
                {
                    return System.Convert.ToUInt64(0);
                }
                return ((ulong)(curObj["WorkingSet"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsWorkingSetPeakNull
        {
            get
            {
                if ((curObj["WorkingSetPeak"] == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"Working Set Peak is the maximum size, in bytes, of the Working Set of this process at any point in time. The Working Set is the set of memory pages touched recently by the threads in the process. If free memory in the computer is above a threshold, pages are left in the Working Set of a process even if they are not in use. When free memory falls below a threshold, pages are trimmed from Working Sets. If they are needed they will then be soft-faulted back into the Working Set before they leave main memory.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public ulong WorkingSetPeak
        {
            get
            {
                if ((curObj["WorkingSetPeak"] == null))
                {
                    return System.Convert.ToUInt64(0);
                }
                return ((ulong)(curObj["WorkingSetPeak"]));
            }
        }

        private bool CheckIfProperClass(System.Management.ManagementScope mgmtScope, System.Management.ManagementPath path, System.Management.ObjectGetOptions OptionsParam)
        {
            if (((path != null)
                        && (string.Compare(path.ClassName, this.ManagementClassName, StringComparison.OrdinalIgnoreCase) == 0)))
            {
                return true;
            }
            else
            {
                return CheckIfProperClass(new System.Management.ManagementObject(mgmtScope, path, OptionsParam));
            }
        }

        private bool CheckIfProperClass(System.Management.ManagementBaseObject theObj)
        {
            if (((theObj != null)
                        && (string.Compare(((string)(theObj["__CLASS"])), this.ManagementClassName, StringComparison.OrdinalIgnoreCase) == 0)))
            {
                return true;
            }
            else
            {
                System.Array parentClasses = ((System.Array)(theObj["__DERIVATION"]));
                if ((parentClasses != null))
                {
                    int count = 0;
                    for (count = 0; (count < parentClasses.Length); count = (count + 1))
                    {
                        if ((string.Compare(((string)(parentClasses.GetValue(count))), this.ManagementClassName, StringComparison.OrdinalIgnoreCase) == 0))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool ShouldSerializeCreatingProcessID()
        {
            if ((this.IsCreatingProcessIDNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeElapsedTime()
        {
            if ((this.IsElapsedTimeNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeFrequency_Object()
        {
            if ((this.IsFrequency_ObjectNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeFrequency_PerfTime()
        {
            if ((this.IsFrequency_PerfTimeNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeFrequency_Sys100NS()
        {
            if ((this.IsFrequency_Sys100NSNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeHandleCount()
        {
            if ((this.IsHandleCountNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeIDProcess()
        {
            if ((this.IsIDProcessNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeIODataBytesPersec()
        {
            if ((this.IsIODataBytesPersecNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeIODataOperationsPersec()
        {
            if ((this.IsIODataOperationsPersecNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeIOOtherBytesPersec()
        {
            if ((this.IsIOOtherBytesPersecNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeIOOtherOperationsPersec()
        {
            if ((this.IsIOOtherOperationsPersecNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeIOReadBytesPersec()
        {
            if ((this.IsIOReadBytesPersecNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeIOReadOperationsPersec()
        {
            if ((this.IsIOReadOperationsPersecNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeIOWriteBytesPersec()
        {
            if ((this.IsIOWriteBytesPersecNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeIOWriteOperationsPersec()
        {
            if ((this.IsIOWriteOperationsPersecNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializePageFaultsPersec()
        {
            if ((this.IsPageFaultsPersecNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializePageFileBytes()
        {
            if ((this.IsPageFileBytesNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializePageFileBytesPeak()
        {
            if ((this.IsPageFileBytesPeakNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializePercentPrivilegedTime()
        {
            if ((this.IsPercentPrivilegedTimeNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializePercentProcessorTime()
        {
            if ((this.IsPercentProcessorTimeNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializePercentUserTime()
        {
            if ((this.IsPercentUserTimeNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializePoolNonpagedBytes()
        {
            if ((this.IsPoolNonpagedBytesNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializePoolPagedBytes()
        {
            if ((this.IsPoolPagedBytesNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializePriorityBase()
        {
            if ((this.IsPriorityBaseNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializePrivateBytes()
        {
            if ((this.IsPrivateBytesNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeThreadCount()
        {
            if ((this.IsThreadCountNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeTimestamp_Object()
        {
            if ((this.IsTimestamp_ObjectNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeTimestamp_PerfTime()
        {
            if ((this.IsTimestamp_PerfTimeNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeTimestamp_Sys100NS()
        {
            if ((this.IsTimestamp_Sys100NSNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeVirtualBytes()
        {
            if ((this.IsVirtualBytesNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeVirtualBytesPeak()
        {
            if ((this.IsVirtualBytesPeakNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeWorkingSet()
        {
            if ((this.IsWorkingSetNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeWorkingSetPeak()
        {
            if ((this.IsWorkingSetPeakNull == false))
            {
                return true;
            }
            return false;
        }

        [Browsable(true)]
        public void CommitObject()
        {
            if ((isEmbedded == false))
            {
                PrivateLateBoundObject.Put();
            }
        }

        [Browsable(true)]
        public void CommitObject(System.Management.PutOptions putOptions)
        {
            if ((isEmbedded == false))
            {
                PrivateLateBoundObject.Put(putOptions);
            }
        }

        private void Initialize()
        {
            AutoCommitProp = true;
            isEmbedded = false;
        }

        private static string ConstructPath(string keyName)
        {
            string strPath = "root\\cimv2:Win32_PerfFormattedData_PerfProc_Process";
            strPath = string.Concat(strPath, string.Concat(".Name=", string.Concat("\"", string.Concat(keyName, "\""))));
            return strPath;
        }

        private void InitializeObject(System.Management.ManagementScope mgmtScope, System.Management.ManagementPath path, System.Management.ObjectGetOptions getOptions)
        {
            Initialize();
            if ((path != null))
            {
                if ((CheckIfProperClass(mgmtScope, path, getOptions) != true))
                {
                    throw new System.ArgumentException("Class name does not match.");
                }
            }
            PrivateLateBoundObject = new System.Management.ManagementObject(mgmtScope, path, getOptions);
            PrivateSystemProperties = new ManagementSystemProperties(PrivateLateBoundObject);
            curObj = PrivateLateBoundObject;
        }

        // Different overloads of GetInstances() help in enumerating instances of the WMI class.
        public static PerfFormattedData_PerfProc_ProcessCollection GetInstances()
        {
            return GetInstances(null, null, null);
        }

        public static PerfFormattedData_PerfProc_ProcessCollection GetInstances(string condition)
        {
            return GetInstances(null, condition, null);
        }

        public static PerfFormattedData_PerfProc_ProcessCollection GetInstances(System.String[] selectedProperties)
        {
            return GetInstances(null, null, selectedProperties);
        }

        public static PerfFormattedData_PerfProc_ProcessCollection GetInstances(string condition, System.String[] selectedProperties)
        {
            return GetInstances(null, condition, selectedProperties);
        }

        public static PerfFormattedData_PerfProc_ProcessCollection GetInstances(System.Management.ManagementScope mgmtScope, System.Management.EnumerationOptions enumOptions)
        {
            if ((mgmtScope == null))
            {
                if ((statMgmtScope == null))
                {
                    mgmtScope = new System.Management.ManagementScope();
                    mgmtScope.Path.NamespacePath = "root\\cimv2";
                }
                else
                {
                    mgmtScope = statMgmtScope;
                }
            }
            System.Management.ManagementPath pathObj = new System.Management.ManagementPath();
            pathObj.ClassName = "Win32_PerfFormattedData_PerfProc_Process";
            pathObj.NamespacePath = "root\\cimv2";
            System.Management.ManagementClass clsObject = new System.Management.ManagementClass(mgmtScope, pathObj, null);
            if ((enumOptions == null))
            {
                enumOptions = new System.Management.EnumerationOptions();
                enumOptions.EnsureLocatable = true;
            }
            return new PerfFormattedData_PerfProc_ProcessCollection(clsObject.GetInstances(enumOptions));
        }

        public static PerfFormattedData_PerfProc_ProcessCollection GetInstances(System.Management.ManagementScope mgmtScope, string condition)
        {
            return GetInstances(mgmtScope, condition, null);
        }

        public static PerfFormattedData_PerfProc_ProcessCollection GetInstances(System.Management.ManagementScope mgmtScope, System.String[] selectedProperties)
        {
            return GetInstances(mgmtScope, null, selectedProperties);
        }

        public static PerfFormattedData_PerfProc_ProcessCollection GetInstances(System.Management.ManagementScope mgmtScope, string condition, System.String[] selectedProperties)
        {
            if ((mgmtScope == null))
            {
                if ((statMgmtScope == null))
                {
                    mgmtScope = new System.Management.ManagementScope();
                    mgmtScope.Path.NamespacePath = "root\\cimv2";
                }
                else
                {
                    mgmtScope = statMgmtScope;
                }
            }
            System.Management.ManagementObjectSearcher ObjectSearcher = new System.Management.ManagementObjectSearcher(mgmtScope, new SelectQuery("Win32_PerfFormattedData_PerfProc_Process", condition, selectedProperties));
            System.Management.EnumerationOptions enumOptions = new System.Management.EnumerationOptions();
            enumOptions.EnsureLocatable = true;
            ObjectSearcher.Options = enumOptions;
            return new PerfFormattedData_PerfProc_ProcessCollection(ObjectSearcher.Get());
        }

        [Browsable(true)]
        public static PerfFormattedData_PerfProc_Process CreateInstance()
        {
            System.Management.ManagementScope mgmtScope = null;
            if ((statMgmtScope == null))
            {
                mgmtScope = new System.Management.ManagementScope();
                mgmtScope.Path.NamespacePath = CreatedWmiNamespace;
            }
            else
            {
                mgmtScope = statMgmtScope;
            }
            System.Management.ManagementPath mgmtPath = new System.Management.ManagementPath(CreatedClassName);
            System.Management.ManagementClass tmpMgmtClass = new System.Management.ManagementClass(mgmtScope, mgmtPath, null);
            return new PerfFormattedData_PerfProc_Process(tmpMgmtClass.CreateInstance());
        }

        [Browsable(true)]
        public void Delete()
        {
            PrivateLateBoundObject.Delete();
        }

        // Enumerator implementation for enumerating instances of the class.
        public class PerfFormattedData_PerfProc_ProcessCollection : object, ICollection
        {

            private ManagementObjectCollection privColObj;

            public PerfFormattedData_PerfProc_ProcessCollection(ManagementObjectCollection objCollection)
            {
                privColObj = objCollection;
            }

            public virtual int Count
            {
                get
                {
                    return privColObj.Count;
                }
            }

            public virtual bool IsSynchronized
            {
                get
                {
                    return privColObj.IsSynchronized;
                }
            }

            public virtual object SyncRoot
            {
                get
                {
                    return this;
                }
            }

            public virtual void CopyTo(System.Array array, int index)
            {
                privColObj.CopyTo(array, index);
                int nCtr;
                for (nCtr = 0; (nCtr < array.Length); nCtr = (nCtr + 1))
                {
                    array.SetValue(new PerfFormattedData_PerfProc_Process(((System.Management.ManagementObject)(array.GetValue(nCtr)))), nCtr);
                }
            }

            public virtual System.Collections.IEnumerator GetEnumerator()
            {
                return new PerfFormattedData_PerfProc_ProcessEnumerator(privColObj.GetEnumerator());
            }

            public class PerfFormattedData_PerfProc_ProcessEnumerator : object, System.Collections.IEnumerator
            {

                private ManagementObjectCollection.ManagementObjectEnumerator privObjEnum;

                public PerfFormattedData_PerfProc_ProcessEnumerator(ManagementObjectCollection.ManagementObjectEnumerator objEnum)
                {
                    privObjEnum = objEnum;
                }

                public virtual object Current
                {
                    get
                    {
                        return new PerfFormattedData_PerfProc_Process(((System.Management.ManagementObject)(privObjEnum.Current)));
                    }
                }

                public virtual bool MoveNext()
                {
                    return privObjEnum.MoveNext();
                }

                public virtual void Reset()
                {
                    privObjEnum.Reset();
                }
            }
        }

        // TypeConverter to handle null values for ValueType properties
        public class WMIValueTypeConverter : TypeConverter
        {

            private TypeConverter baseConverter;

            private System.Type baseType;

            public WMIValueTypeConverter(System.Type inBaseType)
            {
                baseConverter = TypeDescriptor.GetConverter(inBaseType);
                baseType = inBaseType;
            }

            public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type srcType)
            {
                return baseConverter.CanConvertFrom(context, srcType);
            }

            public override bool CanConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Type destinationType)
            {
                return baseConverter.CanConvertTo(context, destinationType);
            }

            public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                return baseConverter.ConvertFrom(context, culture, value);
            }

            public override object CreateInstance(System.ComponentModel.ITypeDescriptorContext context, System.Collections.IDictionary dictionary)
            {
                return baseConverter.CreateInstance(context, dictionary);
            }

            public override bool GetCreateInstanceSupported(System.ComponentModel.ITypeDescriptorContext context)
            {
                return baseConverter.GetCreateInstanceSupported(context);
            }

            public override PropertyDescriptorCollection GetProperties(System.ComponentModel.ITypeDescriptorContext context, object value, System.Attribute[] attributeVar)
            {
                return baseConverter.GetProperties(context, value, attributeVar);
            }

            public override bool GetPropertiesSupported(System.ComponentModel.ITypeDescriptorContext context)
            {
                return baseConverter.GetPropertiesSupported(context);
            }

            public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(System.ComponentModel.ITypeDescriptorContext context)
            {
                return baseConverter.GetStandardValues(context);
            }

            public override bool GetStandardValuesExclusive(System.ComponentModel.ITypeDescriptorContext context)
            {
                return baseConverter.GetStandardValuesExclusive(context);
            }

            public override bool GetStandardValuesSupported(System.ComponentModel.ITypeDescriptorContext context)
            {
                return baseConverter.GetStandardValuesSupported(context);
            }

            public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, System.Type destinationType)
            {
                if ((baseType.BaseType == typeof(System.Enum)))
                {
                    if ((value.GetType() == destinationType))
                    {
                        return value;
                    }
                    if ((((value == null)
                                && (context != null))
                                && (context.PropertyDescriptor.ShouldSerializeValue(context.Instance) == false)))
                    {
                        return "NULL_ENUM_VALUE";
                    }
                    return baseConverter.ConvertTo(context, culture, value, destinationType);
                }
                if (((baseType == typeof(bool))
                            && (baseType.BaseType == typeof(System.ValueType))))
                {
                    if ((((value == null)
                                && (context != null))
                                && (context.PropertyDescriptor.ShouldSerializeValue(context.Instance) == false)))
                    {
                        return "";
                    }
                    return baseConverter.ConvertTo(context, culture, value, destinationType);
                }
                if (((context != null)
                            && (context.PropertyDescriptor.ShouldSerializeValue(context.Instance) == false)))
                {
                    return "";
                }
                return baseConverter.ConvertTo(context, culture, value, destinationType);
            }
        }

        // Embedded class to represent WMI system Properties.
        [TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
        public class ManagementSystemProperties
        {

            private System.Management.ManagementBaseObject PrivateLateBoundObject;

            public ManagementSystemProperties(System.Management.ManagementBaseObject ManagedObject)
            {
                PrivateLateBoundObject = ManagedObject;
            }

            [Browsable(true)]
            public int GENUS
            {
                get
                {
                    return ((int)(PrivateLateBoundObject["__GENUS"]));
                }
            }

            [Browsable(true)]
            public string CLASS
            {
                get
                {
                    return ((string)(PrivateLateBoundObject["__CLASS"]));
                }
            }

            [Browsable(true)]
            public string SUPERCLASS
            {
                get
                {
                    return ((string)(PrivateLateBoundObject["__SUPERCLASS"]));
                }
            }

            [Browsable(true)]
            public string DYNASTY
            {
                get
                {
                    return ((string)(PrivateLateBoundObject["__DYNASTY"]));
                }
            }

            [Browsable(true)]
            public string RELPATH
            {
                get
                {
                    return ((string)(PrivateLateBoundObject["__RELPATH"]));
                }
            }

            [Browsable(true)]
            public int PROPERTY_COUNT
            {
                get
                {
                    return ((int)(PrivateLateBoundObject["__PROPERTY_COUNT"]));
                }
            }

            [Browsable(true)]
            public string[] DERIVATION
            {
                get
                {
                    return ((string[])(PrivateLateBoundObject["__DERIVATION"]));
                }
            }

            [Browsable(true)]
            public string SERVER
            {
                get
                {
                    return ((string)(PrivateLateBoundObject["__SERVER"]));
                }
            }

            [Browsable(true)]
            public string NAMESPACE
            {
                get
                {
                    return ((string)(PrivateLateBoundObject["__NAMESPACE"]));
                }
            }

            [Browsable(true)]
            public string PATH
            {
                get
                {
                    return ((string)(PrivateLateBoundObject["__PATH"]));
                }
            }
        }
    }
}
