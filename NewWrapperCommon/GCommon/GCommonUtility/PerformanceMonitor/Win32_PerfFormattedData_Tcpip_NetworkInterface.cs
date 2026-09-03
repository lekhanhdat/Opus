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
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Management;
    #endregion

    internal class PerfFormattedData_Tcpip_NetworkInterface : System.ComponentModel.Component
    {
        private static string CreatedWmiNamespace = "root\\cimv2";
        private static string CreatedClassName = "Win32_PerfFormattedData_Tcpip_NetworkInterface";
        private static System.Management.ManagementScope statMgmtScope = null;
        private ManagementSystemProperties PrivateSystemProperties;
        private System.Management.ManagementObject PrivateLateBoundObject;
        private bool AutoCommitProp;
        private System.Management.ManagementBaseObject embeddedObj;
        private System.Management.ManagementBaseObject curObj;
        private bool isEmbedded;

        public PerfFormattedData_Tcpip_NetworkInterface()
        {
            this.InitializeObject(null, null, null);
        }
        public PerfFormattedData_Tcpip_NetworkInterface(string keyName)
        {
            this.InitializeObject(null, new System.Management.ManagementPath(PerfFormattedData_Tcpip_NetworkInterface.ConstructPath(keyName)), null);
        }
        public PerfFormattedData_Tcpip_NetworkInterface(System.Management.ManagementScope mgmtScope, string keyName)
        {
            this.InitializeObject(((System.Management.ManagementScope)(mgmtScope)), new System.Management.ManagementPath(PerfFormattedData_Tcpip_NetworkInterface.ConstructPath(keyName)), null);
        }
        public PerfFormattedData_Tcpip_NetworkInterface(System.Management.ManagementPath path, System.Management.ObjectGetOptions getOptions)
        {
            this.InitializeObject(null, path, getOptions);
        }
        public PerfFormattedData_Tcpip_NetworkInterface(System.Management.ManagementScope mgmtScope, System.Management.ManagementPath path)
        {
            this.InitializeObject(mgmtScope, path, null);
        }
        public PerfFormattedData_Tcpip_NetworkInterface(System.Management.ManagementPath path)
        {
            this.InitializeObject(null, path, null);
        }
        public PerfFormattedData_Tcpip_NetworkInterface(System.Management.ManagementScope mgmtScope, System.Management.ManagementPath path, System.Management.ObjectGetOptions getOptions)
        {
            this.InitializeObject(mgmtScope, path, getOptions);
        }

        public PerfFormattedData_Tcpip_NetworkInterface(System.Management.ManagementObject theObject)
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

        public PerfFormattedData_Tcpip_NetworkInterface(System.Management.ManagementBaseObject theObject)
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

        // Property returns the namespace of the WMI class.
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

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsBytesReceivedPersecNull
        {
            get
            {
                if ((curObj["BytesReceivedPersec"] == null))
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
        [Description("Bytes Received/sec is the rate at which bytes are received over each network adap" +
            "ter, including framing characters. Network Interface\\\\Bytes Received/sec is a su" +
            "bset of Network Interface\\\\Bytes Total/sec.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public UInt64 BytesReceivedPersec
        {
            get
            {
                if ((curObj["BytesReceivedPersec"] == null))
                {
                    return System.Convert.ToUInt64(0);
                }
                return (System.Convert.ToUInt64(curObj["BytesReceivedPersec"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsBytesSentPersecNull
        {
            get
            {
                if ((curObj["BytesSentPersec"] == null))
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
        [Description("Bytes Sent/sec is the rate at which bytes are sent over each each network adapter" +
            ", including framing characters. Network Interface\\\\Bytes Sent/sec is a subset of" +
            " Network Interface\\\\Bytes Total/sec.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public UInt64 BytesSentPersec
        {
            get
            {
                if ((curObj["BytesSentPersec"] == null))
                {
                    return System.Convert.ToUInt64(0);
                }
                return (System.Convert.ToUInt64(curObj["BytesSentPersec"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsBytesTotalPersecNull
        {
            get
            {
                if ((curObj["BytesTotalPersec"] == null))
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
        [Description("Bytes Total/sec is the rate at which bytes are sent and received over each networ" +
            "k adapter, including framing characters. Network Interface\\\\Bytes Received/sec i" +
            "s a sum of Network Interface\\\\Bytes Received/sec and Network Interface\\\\Bytes Se" +
            "nt/sec.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public UInt64 BytesTotalPersec
        {
            get
            {
                if ((curObj["BytesTotalPersec"] == null))
                {
                    return System.Convert.ToUInt64(0);
                }
                return (Convert.ToUInt64(curObj["BytesTotalPersec"]));
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
        public bool IsCurrentBandwidthNull
        {
            get
            {
                if ((curObj["CurrentBandwidth"] == null))
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
        [Description("Current Bandwidth is an estimate of the current bandwidth of the network interfac" +
            "e in bits per second (BPS).  For interfaces that do not vary in bandwidth or for" +
            " those where no accurate estimation can be made, this value is the nominal bandw" +
            "idth.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public String CurrentBandwidth
        {
            get
            {
                if ((curObj["CurrentBandwidth"] == null))
                {
                    return "0";
                }
                return curObj["CurrentBandwidth"].ToString();
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
        public bool IsOutputQueueLengthNull
        {
            get
            {
                if ((curObj["OutputQueueLength"] == null))
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
        [Description(@"Output Queue Length is the length of the output packet queue (in packets). If this is longer than two, there are delays and the bottleneck should be found and eliminated, if possible. Since the requests are queued by the Network Driver Interface Specification (NDIS) in this implementation, this will always be 0.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint OutputQueueLength
        {
            get
            {
                if ((curObj["OutputQueueLength"] == null))
                {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["OutputQueueLength"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPacketsOutboundDiscardedNull
        {
            get
            {
                if ((curObj["PacketsOutboundDiscarded"] == null))
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
        [Description("Packets Outbound Discarded is the number of outbound packets that were chosen to " +
            "be discarded even though no errors had been detected to prevent transmission. On" +
            "e possible reason for discarding packets could be to free up buffer space.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint PacketsOutboundDiscarded
        {
            get
            {
                if ((curObj["PacketsOutboundDiscarded"] == null))
                {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["PacketsOutboundDiscarded"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPacketsOutboundErrorsNull
        {
            get
            {
                if ((curObj["PacketsOutboundErrors"] == null))
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
        [Description("Packets Outbound Errors is the number of outbound packets that could not be trans" +
            "mitted because of errors.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint PacketsOutboundErrors
        {
            get
            {
                if ((curObj["PacketsOutboundErrors"] == null))
                {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["PacketsOutboundErrors"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPacketsPersecNull
        {
            get
            {
                if ((curObj["PacketsPersec"] == null))
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
        [Description("Packets/sec is the rate at which packets are sent and received on the network int" +
            "erface.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint PacketsPersec
        {
            get
            {
                if ((curObj["PacketsPersec"] == null))
                {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["PacketsPersec"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPacketsReceivedDiscardedNull
        {
            get
            {
                if ((curObj["PacketsReceivedDiscarded"] == null))
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
        [Description(@"Packets Received Discarded is the number of inbound packets that were chosen to be discarded even though no errors had been detected to prevent their delivery to a higher-layer protocol.  One possible reason for discarding packets could be to free up buffer space.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint PacketsReceivedDiscarded
        {
            get
            {
                if ((curObj["PacketsReceivedDiscarded"] == null))
                {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["PacketsReceivedDiscarded"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPacketsReceivedErrorsNull
        {
            get
            {
                if ((curObj["PacketsReceivedErrors"] == null))
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
        [Description("Packets Received Errors is the number of inbound packets that contained errors pr" +
            "eventing them from being deliverable to a higher-layer protocol.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint PacketsReceivedErrors
        {
            get
            {
                if ((curObj["PacketsReceivedErrors"] == null))
                {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["PacketsReceivedErrors"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPacketsReceivedNonUnicastPersecNull
        {
            get
            {
                if ((curObj["PacketsReceivedNonUnicastPersec"] == null))
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
        [Description("Packets Received Non-Unicast/sec is the rate at which non-unicast (subnet broadca" +
            "st or subnet multicast) packets are delivered to a higher-layer protocol.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint PacketsReceivedNonUnicastPersec
        {
            get
            {
                if ((curObj["PacketsReceivedNonUnicastPersec"] == null))
                {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["PacketsReceivedNonUnicastPersec"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPacketsReceivedPersecNull
        {
            get
            {
                if ((curObj["PacketsReceivedPersec"] == null))
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
        [Description("Packets Received/sec is the rate at which packets are received on the network int" +
            "erface.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint PacketsReceivedPersec
        {
            get
            {
                if ((curObj["PacketsReceivedPersec"] == null))
                {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["PacketsReceivedPersec"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPacketsReceivedUnicastPersecNull
        {
            get
            {
                if ((curObj["PacketsReceivedUnicastPersec"] == null))
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
        [Description("Packets Received Unicast/sec is the rate at which (subnet) unicast packets are de" +
            "livered to a higher-layer protocol.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint PacketsReceivedUnicastPersec
        {
            get
            {
                if ((curObj["PacketsReceivedUnicastPersec"] == null))
                {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["PacketsReceivedUnicastPersec"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPacketsReceivedUnknownNull
        {
            get
            {
                if ((curObj["PacketsReceivedUnknown"] == null))
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
        [Description("Packets Received Unknown is the number of packets received through the interface " +
            "that were discarded because of an unknown or unsupported protocol.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint PacketsReceivedUnknown
        {
            get
            {
                if ((curObj["PacketsReceivedUnknown"] == null))
                {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["PacketsReceivedUnknown"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPacketsSentNonUnicastPersecNull
        {
            get
            {
                if ((curObj["PacketsSentNonUnicastPersec"] == null))
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
        [Description("Packets Sent Non-Unicast/sec is the rate at which packets are requested to be tra" +
            "nsmitted to non-unicast (subnet broadcast or subnet multicast) addresses by high" +
            "er-level protocols.  The rate includes the packets that were discarded or not se" +
            "nt.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint PacketsSentNonUnicastPersec
        {
            get
            {
                if ((curObj["PacketsSentNonUnicastPersec"] == null))
                {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["PacketsSentNonUnicastPersec"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPacketsSentPersecNull
        {
            get
            {
                if ((curObj["PacketsSentPersec"] == null))
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
        [Description("Packets Sent/sec is the rate at which packets are sent on the network interface.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint PacketsSentPersec
        {
            get
            {
                if ((curObj["PacketsSentPersec"] == null))
                {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["PacketsSentPersec"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPacketsSentUnicastPersecNull
        {
            get
            {
                if ((curObj["PacketsSentUnicastPersec"] == null))
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
        [Description("Packets Sent Unicast/sec is the rate at which packets are requested to be transmi" +
            "tted to subnet-unicast addresses by higher-level protocols.  The rate includes t" +
            "he packets that were discarded or not sent.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint PacketsSentUnicastPersec
        {
            get
            {
                if ((curObj["PacketsSentUnicastPersec"] == null))
                {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["PacketsSentUnicastPersec"]));
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

        private bool ShouldSerializeBytesReceivedPersec()
        {
            if ((this.IsBytesReceivedPersecNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeBytesSentPersec()
        {
            if ((this.IsBytesSentPersecNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeBytesTotalPersec()
        {
            if ((this.IsBytesTotalPersecNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeCurrentBandwidth()
        {
            if ((this.IsCurrentBandwidthNull == false))
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

        private bool ShouldSerializeOutputQueueLength()
        {
            if ((this.IsOutputQueueLengthNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializePacketsOutboundDiscarded()
        {
            if ((this.IsPacketsOutboundDiscardedNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializePacketsOutboundErrors()
        {
            if ((this.IsPacketsOutboundErrorsNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializePacketsPersec()
        {
            if ((this.IsPacketsPersecNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializePacketsReceivedDiscarded()
        {
            if ((this.IsPacketsReceivedDiscardedNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializePacketsReceivedErrors()
        {
            if ((this.IsPacketsReceivedErrorsNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializePacketsReceivedNonUnicastPersec()
        {
            if ((this.IsPacketsReceivedNonUnicastPersecNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializePacketsReceivedPersec()
        {
            if ((this.IsPacketsReceivedPersecNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializePacketsReceivedUnicastPersec()
        {
            if ((this.IsPacketsReceivedUnicastPersecNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializePacketsReceivedUnknown()
        {
            if ((this.IsPacketsReceivedUnknownNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializePacketsSentNonUnicastPersec()
        {
            if ((this.IsPacketsSentNonUnicastPersecNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializePacketsSentPersec()
        {
            if ((this.IsPacketsSentPersecNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializePacketsSentUnicastPersec()
        {
            if ((this.IsPacketsSentUnicastPersecNull == false))
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
            string strPath = "root\\cimv2:Win32_PerfFormattedData_Tcpip_NetworkInterface";
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
        public static PerfFormattedData_Tcpip_NetworkInterfaceCollection GetInstances()
        {
            return GetInstances(null, null, null);
        }

        public static PerfFormattedData_Tcpip_NetworkInterfaceCollection GetInstances(string condition)
        {
            return GetInstances(null, condition, null);
        }

        public static PerfFormattedData_Tcpip_NetworkInterfaceCollection GetInstances(System.String[] selectedProperties)
        {
            return GetInstances(null, null, selectedProperties);
        }

        public static PerfFormattedData_Tcpip_NetworkInterfaceCollection GetInstances(string condition, System.String[] selectedProperties)
        {
            return GetInstances(null, condition, selectedProperties);
        }

        public static PerfFormattedData_Tcpip_NetworkInterfaceCollection GetInstances(System.Management.ManagementScope mgmtScope, System.Management.EnumerationOptions enumOptions)
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
            pathObj.ClassName = "Win32_PerfFormattedData_Tcpip_NetworkInterface";
            pathObj.NamespacePath = "root\\cimv2";
            System.Management.ManagementClass clsObject = new System.Management.ManagementClass(mgmtScope, pathObj, null);
            if ((enumOptions == null))
            {
                enumOptions = new System.Management.EnumerationOptions();
                enumOptions.EnsureLocatable = true;
            }
            return new PerfFormattedData_Tcpip_NetworkInterfaceCollection(clsObject.GetInstances(enumOptions));
        }

        public static PerfFormattedData_Tcpip_NetworkInterfaceCollection GetInstances(System.Management.ManagementScope mgmtScope, string condition)
        {
            return GetInstances(mgmtScope, condition, null);
        }

        public static PerfFormattedData_Tcpip_NetworkInterfaceCollection GetInstances(System.Management.ManagementScope mgmtScope, System.String[] selectedProperties)
        {
            return GetInstances(mgmtScope, null, selectedProperties);
        }

        public static PerfFormattedData_Tcpip_NetworkInterfaceCollection GetInstances(System.Management.ManagementScope mgmtScope, string condition, System.String[] selectedProperties)
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
            System.Management.ManagementObjectSearcher ObjectSearcher = new System.Management.ManagementObjectSearcher(mgmtScope, new SelectQuery("Win32_PerfFormattedData_Tcpip_NetworkInterface", condition, selectedProperties));
            System.Management.EnumerationOptions enumOptions = new System.Management.EnumerationOptions();
            enumOptions.EnsureLocatable = true;
            ObjectSearcher.Options = enumOptions;
            return new PerfFormattedData_Tcpip_NetworkInterfaceCollection(ObjectSearcher.Get());
        }

        [Browsable(true)]
        public static PerfFormattedData_Tcpip_NetworkInterface CreateInstance()
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
            return new PerfFormattedData_Tcpip_NetworkInterface(tmpMgmtClass.CreateInstance());
        }

        [Browsable(true)]
        public void Delete()
        {
            PrivateLateBoundObject.Delete();
        }

        // Enumerator implementation for enumerating instances of the class.
        public class PerfFormattedData_Tcpip_NetworkInterfaceCollection : object, ICollection
        {

            private ManagementObjectCollection privColObj;

            public PerfFormattedData_Tcpip_NetworkInterfaceCollection(ManagementObjectCollection objCollection)
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
                    array.SetValue(new PerfFormattedData_Tcpip_NetworkInterface(((System.Management.ManagementObject)(array.GetValue(nCtr)))), nCtr);
                }
            }

            public virtual System.Collections.IEnumerator GetEnumerator()
            {
                return new PerfFormattedData_Tcpip_NetworkInterfaceEnumerator(privColObj.GetEnumerator());
            }

            public class PerfFormattedData_Tcpip_NetworkInterfaceEnumerator : object, System.Collections.IEnumerator
            {

                private ManagementObjectCollection.ManagementObjectEnumerator privObjEnum;

                public PerfFormattedData_Tcpip_NetworkInterfaceEnumerator(ManagementObjectCollection.ManagementObjectEnumerator objEnum)
                {
                    privObjEnum = objEnum;
                }

                public virtual object Current
                {
                    get
                    {
                        return new PerfFormattedData_Tcpip_NetworkInterface(((System.Management.ManagementObject)(privObjEnum.Current)));
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
