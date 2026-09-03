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

    internal class PhysicalMemory : System.ComponentModel.Component
    {
        private static string CreatedWmiNamespace = "root\\cimv2";
        private static string CreatedClassName = "Win32_PhysicalMemory";
        private static System.Management.ManagementScope statMgmtScope = null;
        private ManagementSystemProperties PrivateSystemProperties;
        private System.Management.ManagementObject PrivateLateBoundObject;
        private bool AutoCommitProp;
        private System.Management.ManagementBaseObject embeddedObj;
        private System.Management.ManagementBaseObject curObj;
        private bool isEmbedded;

        public PhysicalMemory()
        {
            this.InitializeObject(null, null, null);
        }

        public PhysicalMemory(string keyTag)
        {
            this.InitializeObject(null, new System.Management.ManagementPath(PhysicalMemory.ConstructPath(keyTag)), null);
        }

        public PhysicalMemory(System.Management.ManagementScope mgmtScope, string keyTag)
        {
            this.InitializeObject(((System.Management.ManagementScope)(mgmtScope)), new System.Management.ManagementPath(PhysicalMemory.ConstructPath(keyTag)), null);
        }

        public PhysicalMemory(System.Management.ManagementPath path, System.Management.ObjectGetOptions getOptions)
        {
            this.InitializeObject(null, path, getOptions);
        }

        public PhysicalMemory(System.Management.ManagementScope mgmtScope, System.Management.ManagementPath path)
        {
            this.InitializeObject(mgmtScope, path, null);
        }

        public PhysicalMemory(System.Management.ManagementPath path)
        {
            this.InitializeObject(null, path, null);
        }

        public PhysicalMemory(System.Management.ManagementScope mgmtScope, System.Management.ManagementPath path, System.Management.ObjectGetOptions getOptions)
        {
            this.InitializeObject(mgmtScope, path, getOptions);
        }

        public PhysicalMemory(System.Management.ManagementObject theObject)
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

        public PhysicalMemory(System.Management.ManagementBaseObject theObject)
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

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("A string identifying the physically labeled bank where the memory is located - fo" +
            "r example, \'Bank 0\' or \'Bank A\'.")]
        public string BankLabel
        {
            get
            {
                return ((string)(curObj["BankLabel"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsCapacityNull
        {
            get
            {
                if ((curObj["Capacity"] == null))
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
        [Description("The total capacity of this physical memory, in bytes.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public ulong Capacity
        {
            get
            {
                if ((curObj["Capacity"] == null))
                {
                    return System.Convert.ToUInt64(0);
                }
                return ((ulong)(curObj["Capacity"]));
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The Caption property is a short textual description (one-line string) of the obje" +
            "ct.")]
        public string Caption
        {
            get
            {
                return ((string)(curObj["Caption"]));
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("CreationClassName indicates the name of the class or the subclass used in the cre" +
            "ation of an instance. When used with the other key properties of this class, thi" +
            "s property allows all instances of this class and its subclasses to be uniquely " +
            "identified.")]
        public string CreationClassName
        {
            get
            {
                return ((string)(curObj["CreationClassName"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsDataWidthNull
        {
            get
            {
                if ((curObj["DataWidth"] == null))
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
        [Description("Data width of the physical memory, in bits. A data width of 0 and a total width o" +
            "f 8 would indicate that the memory is solely used to provide error correction bi" +
            "ts.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public ushort DataWidth
        {
            get
            {
                if ((curObj["DataWidth"] == null))
                {
                    return System.Convert.ToUInt16(0);
                }
                return ((ushort)(curObj["DataWidth"]));
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The Description property provides a textual description of the object. ")]
        public string Description
        {
            get
            {
                return ((string)(curObj["Description"]));
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The DeviceLocator property indicates the label of the socket or circuit board tha" +
            "t holds this memory.\nExample: SIMM 3")]
        public string DeviceLocator
        {
            get
            {
                return ((string)(curObj["DeviceLocator"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsFormFactorNull
        {
            get
            {
                if ((curObj["FormFactor"] == null))
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
        [Description("The implementation form factor for the chip.For example, values such as SIMM (7)," +
            " TSOP (9) or PGA (10) can be specified.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public FormFactorValues FormFactor
        {
            get
            {
                if ((curObj["FormFactor"] == null))
                {
                    return ((FormFactorValues)(System.Convert.ToInt32(24)));
                }
                return ((FormFactorValues)(System.Convert.ToInt32(curObj["FormFactor"])));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsHotSwappableNull
        {
            get
            {
                if ((curObj["HotSwappable"] == null))
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
        [Description(@"A physical component can be hot swapped if it is possible to replace the element with a physically different but equivalent one while the containing package has power applied to it (i.e., is 'on').  For example, a fan component may be designed to be hot swapped. All components that can be hot swapped are inherently removable and replaceable .")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public bool HotSwappable
        {
            get
            {
                if ((curObj["HotSwappable"] == null))
                {
                    return System.Convert.ToBoolean(0);
                }
                return ((bool)(curObj["HotSwappable"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsInstallDateNull
        {
            get
            {
                if ((curObj["InstallDate"] == null))
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
        [Description("The InstallDate property is datetime value indicating when the object was install" +
            "ed. A lack of a value does not indicate that the object is not installed.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public System.DateTime InstallDate
        {
            get
            {
                if ((curObj["InstallDate"] != null))
                {
                    return ToDateTime(((string)(curObj["InstallDate"])));
                }
                else
                {
                    return System.DateTime.MinValue;
                }
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsInterleaveDataDepthNull
        {
            get
            {
                if ((curObj["InterleaveDataDepth"] == null))
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
        [Description("The InterleaveDataDepth property indicates the maximum number of consecutive rows" +
            " of data that are accessed in a single interleaved transfer from the memory devi" +
            "ce. If the value is 0, then the memory is not interleaved.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public ushort InterleaveDataDepth
        {
            get
            {
                if ((curObj["InterleaveDataDepth"] == null))
                {
                    return System.Convert.ToUInt16(0);
                }
                return ((ushort)(curObj["InterleaveDataDepth"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsInterleavePositionNull
        {
            get
            {
                if ((curObj["InterleavePosition"] == null))
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
        [Description("The position of this physical memory in an interleave. 0 indicates non-interleave" +
            "d. 1 indicates the first position, 2 the second position and so on. For example," +
            " in a 2:1 interleave, a value of \'1\' would indicate that the memory is in the \'e" +
            "ven\' position.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint InterleavePosition
        {
            get
            {
                if ((curObj["InterleavePosition"] == null))
                {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["InterleavePosition"]));
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The name of the organization responsible for producing the physical element. This" +
            " may be the entity from whom the element is purchased, but this is not necessari" +
            "ly true. The latter information is contained in the Vendor property of CIM_Produ" +
            "ct.")]
        public string Manufacturer
        {
            get
            {
                return ((string)(curObj["Manufacturer"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsMemoryTypeNull
        {
            get
            {
                if ((curObj["MemoryType"] == null))
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
        [Description("The type of physical memory.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public MemoryTypeValues MemoryType
        {
            get
            {
                if ((curObj["MemoryType"] == null))
                {
                    return ((MemoryTypeValues)(System.Convert.ToInt32(22)));
                }
                return ((MemoryTypeValues)(System.Convert.ToInt32(curObj["MemoryType"])));
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The name by which the physical element is generally known.")]
        public string Model
        {
            get
            {
                return ((string)(curObj["Model"]));
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The Name property defines the label by which the object is known. When subclassed" +
            ", the Name property can be overridden to be a Key property.")]
        public string Name
        {
            get
            {
                return ((string)(curObj["Name"]));
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"OtherIdentifyingInfo captures additional data, beyond asset tag information, that could be used to identify a physical element. One example is bar code data associated with an element that also has an asset tag. Note that if only bar code data is available and is unique/able to be used as an element key, this property would be NULL and the bar code data used as the class key, in the tag property.")]
        public string OtherIdentifyingInfo
        {
            get
            {
                return ((string)(curObj["OtherIdentifyingInfo"]));
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The part number assigned by the organization responsible for producing or manufac" +
            "turing the physical element.")]
        public string PartNumber
        {
            get
            {
                return ((string)(curObj["PartNumber"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPositionInRowNull
        {
            get
            {
                if ((curObj["PositionInRow"] == null))
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
        [Description("Specifies the position of the physical memory in a \'row\'. For example, if it take" +
            "s two 8-bit memory devices to form a 16-bit row, then a value of \'2\'means that t" +
            "his memory is the second device. 0 is an invalid value for this property.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint PositionInRow
        {
            get
            {
                if ((curObj["PositionInRow"] == null))
                {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["PositionInRow"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPoweredOnNull
        {
            get
            {
                if ((curObj["PoweredOn"] == null))
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
        [Description("Boolean indicating that the physical element is powered on (TRUE), or is currentl" +
            "y off (FALSE).")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public bool PoweredOn
        {
            get
            {
                if ((curObj["PoweredOn"] == null))
                {
                    return System.Convert.ToBoolean(0);
                }
                return ((bool)(curObj["PoweredOn"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsRemovableNull
        {
            get
            {
                if ((curObj["Removable"] == null))
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
        [Description(@"A physical component is removable if it is designed to be taken in and out of the physical container in which it is normally found, without impairing the function of the overall packaging. A component can still be removable if power must be 'off' in order to perform the removal. If power can be 'on' and the component removed, then the element is removable and can be hot swapped. For example, an upgradeable processor chip is removable.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public bool Removable
        {
            get
            {
                if ((curObj["Removable"] == null))
                {
                    return System.Convert.ToBoolean(0);
                }
                return ((bool)(curObj["Removable"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsReplaceableNull
        {
            get
            {
                if ((curObj["Replaceable"] == null))
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
        [Description(@"A physical component is replaceable  if it is possible to replace (FRU or upgrade) the element with a physically different one.  For example, some computer systems allow the main processor chip to be upgraded to one of a higher clock rating. In this case, the processor is said to be replaceable . All removable components are inherently replaceable .")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public bool Replaceable
        {
            get
            {
                if ((curObj["Replaceable"] == null))
                {
                    return System.Convert.ToBoolean(0);
                }
                return ((bool)(curObj["Replaceable"]));
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("A manufacturer-allocated number used to identify the PhysicalElement.")]
        public string SerialNumber
        {
            get
            {
                return ((string)(curObj["SerialNumber"]));
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The stock keeping unit number for this physical element.")]
        public string SKU
        {
            get
            {
                return ((string)(curObj["SKU"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsSpeedNull
        {
            get
            {
                if ((curObj["Speed"] == null))
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
        [Description("The speed of the physical memory, in nanoseconds.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint Speed
        {
            get
            {
                if ((curObj["Speed"] == null))
                {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["Speed"]));
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"The Status property is a string indicating the current status of the object. Various operational and non-operational statuses can be defined. Operational statuses are ""OK"", ""Degraded"" and ""Pred Fail"". ""Pred Fail"" indicates that an element may be functioning properly but predicting a failure in the near future. An example is a SMART-enabled hard drive. Non-operational statuses can also be specified. These are ""Error"", ""Starting"", ""Stopping"" and ""Service"". The latter, ""Service"", could apply during mirror-resilvering of a disk, reload of a user permissions list, or other administrative work. Not all such work is on-line, yet the managed element is neither ""OK"" nor in one of the other states.")]
        public string Status
        {
            get
            {
                return ((string)(curObj["Status"]));
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The Tag property contains a string that uniquely identifies the physical memory d" +
            "evice represented by an instance of Win32_PhysicalMemory.\nExample: Physical Memo" +
            "ry 1")]
        public string Tag
        {
            get
            {
                return ((string)(curObj["Tag"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsTotalWidthNull
        {
            get
            {
                if ((curObj["TotalWidth"] == null))
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
        [Description("Total width, in bits, of the physical memory, including check or error correction" +
            " bits. If there are no error correction bits, the value in this property should " +
            "match that specified for the DataWidth property.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public ushort TotalWidth
        {
            get
            {
                if ((curObj["TotalWidth"] == null))
                {
                    return System.Convert.ToUInt16(0);
                }
                return ((ushort)(curObj["TotalWidth"]));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsTypeDetailNull
        {
            get
            {
                if ((curObj["TypeDetail"] == null))
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
        [Description("The TypeDetail property indicates the type of physical memory represented by Win3" +
            "2_PhysicalMemory.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public TypeDetailValues TypeDetail
        {
            get
            {
                if ((curObj["TypeDetail"] == null))
                {
                    return ((TypeDetailValues)(System.Convert.ToInt32(8192)));
                }
                return ((TypeDetailValues)(System.Convert.ToInt32(curObj["TypeDetail"])));
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("A string indicating the version of the physical element.")]
        public string Version
        {
            get
            {
                return ((string)(curObj["Version"]));
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

        private bool ShouldSerializeCapacity()
        {
            if ((this.IsCapacityNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeDataWidth()
        {
            if ((this.IsDataWidthNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeFormFactor()
        {
            if ((this.IsFormFactorNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeHotSwappable()
        {
            if ((this.IsHotSwappableNull == false))
            {
                return true;
            }
            return false;
        }

        // Converts a given datetime in DMTF format to System.DateTime object.
        static System.DateTime ToDateTime(string dmtfDate)
        {
            System.DateTime initializer = System.DateTime.MinValue;
            int year = initializer.Year;
            int month = initializer.Month;
            int day = initializer.Day;
            int hour = initializer.Hour;
            int minute = initializer.Minute;
            int second = initializer.Second;
            long ticks = 0;
            string dmtf = dmtfDate;
            System.DateTime datetime = System.DateTime.MinValue;
            string tempString = string.Empty;
            if ((dmtf == null))
            {
                throw new System.ArgumentOutOfRangeException();
            }
            if ((dmtf.Length == 0))
            {
                throw new System.ArgumentOutOfRangeException();
            }
            if ((dmtf.Length != 25))
            {
                throw new System.ArgumentOutOfRangeException();
            }
            try
            {
                tempString = dmtf.Substring(0, 4);
                if (("****" != tempString))
                {
                    year = int.Parse(tempString);
                }
                tempString = dmtf.Substring(4, 2);
                if (("**" != tempString))
                {
                    month = int.Parse(tempString);
                }
                tempString = dmtf.Substring(6, 2);
                if (("**" != tempString))
                {
                    day = int.Parse(tempString);
                }
                tempString = dmtf.Substring(8, 2);
                if (("**" != tempString))
                {
                    hour = int.Parse(tempString);
                }
                tempString = dmtf.Substring(10, 2);
                if (("**" != tempString))
                {
                    minute = int.Parse(tempString);
                }
                tempString = dmtf.Substring(12, 2);
                if (("**" != tempString))
                {
                    second = int.Parse(tempString);
                }
                tempString = dmtf.Substring(15, 6);
                if (("******" != tempString))
                {
                    ticks = (long.Parse(tempString) * ((long)((System.TimeSpan.TicksPerMillisecond / 1000))));
                }
                if (((((((((year < 0)
                            || (month < 0))
                            || (day < 0))
                            || (hour < 0))
                            || (minute < 0))
                            || (minute < 0))
                            || (second < 0))
                            || (ticks < 0)))
                {
                    throw new System.ArgumentOutOfRangeException();
                }
            }
            catch (System.Exception e)
            {
                throw new System.ArgumentOutOfRangeException(null, e.Message);
            }
            datetime = new System.DateTime(year, month, day, hour, minute, second, 0);
            datetime = datetime.AddTicks(ticks);
            System.TimeSpan tickOffset = System.TimeZone.CurrentTimeZone.GetUtcOffset(datetime);
            int UTCOffset = 0;
            int OffsetToBeAdjusted = 0;
            long OffsetMins = ((long)((tickOffset.Ticks / System.TimeSpan.TicksPerMinute)));
            tempString = dmtf.Substring(22, 3);
            if ((tempString != "******"))
            {
                tempString = dmtf.Substring(21, 4);
                try
                {
                    UTCOffset = int.Parse(tempString);
                }
                catch (System.Exception e)
                {
                    throw new System.ArgumentOutOfRangeException(null, e.Message);
                }
                OffsetToBeAdjusted = ((int)((OffsetMins - UTCOffset)));
                datetime = datetime.AddMinutes(((double)(OffsetToBeAdjusted)));
            }
            return datetime;
        }

        // Converts a given System.DateTime object to DMTF datetime format.
        static string ToDmtfDateTime(System.DateTime date)
        {
            string utcString = string.Empty;
            System.TimeSpan tickOffset = System.TimeZone.CurrentTimeZone.GetUtcOffset(date);
            long OffsetMins = ((long)((tickOffset.Ticks / System.TimeSpan.TicksPerMinute)));
            if ((System.Math.Abs(OffsetMins) > 999))
            {
                date = date.ToUniversalTime();
                utcString = "+000";
            }
            else
            {
                if ((tickOffset.Ticks >= 0))
                {
                    utcString = string.Concat("+", ((System.Int64)((tickOffset.Ticks / System.TimeSpan.TicksPerMinute))).ToString().PadLeft(3, '0'));
                }
                else
                {
                    string strTemp = ((System.Int64)(OffsetMins)).ToString();
                    utcString = string.Concat("-", strTemp.Substring(1, (strTemp.Length - 1)).PadLeft(3, '0'));
                }
            }
            string dmtfDateTime = ((System.Int32)(date.Year)).ToString().PadLeft(4, '0');
            dmtfDateTime = string.Concat(dmtfDateTime, ((System.Int32)(date.Month)).ToString().PadLeft(2, '0'));
            dmtfDateTime = string.Concat(dmtfDateTime, ((System.Int32)(date.Day)).ToString().PadLeft(2, '0'));
            dmtfDateTime = string.Concat(dmtfDateTime, ((System.Int32)(date.Hour)).ToString().PadLeft(2, '0'));
            dmtfDateTime = string.Concat(dmtfDateTime, ((System.Int32)(date.Minute)).ToString().PadLeft(2, '0'));
            dmtfDateTime = string.Concat(dmtfDateTime, ((System.Int32)(date.Second)).ToString().PadLeft(2, '0'));
            dmtfDateTime = string.Concat(dmtfDateTime, ".");
            System.DateTime dtTemp = new System.DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, 0);
            long microsec = ((long)((((date.Ticks - dtTemp.Ticks)
                        * 1000)
                        / System.TimeSpan.TicksPerMillisecond)));
            string strMicrosec = ((System.Int64)(microsec)).ToString();
            if ((strMicrosec.Length > 6))
            {
                strMicrosec = strMicrosec.Substring(0, 6);
            }
            dmtfDateTime = string.Concat(dmtfDateTime, strMicrosec.PadLeft(6, '0'));
            dmtfDateTime = string.Concat(dmtfDateTime, utcString);
            return dmtfDateTime;
        }

        private bool ShouldSerializeInstallDate()
        {
            if ((this.IsInstallDateNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeInterleaveDataDepth()
        {
            if ((this.IsInterleaveDataDepthNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeInterleavePosition()
        {
            if ((this.IsInterleavePositionNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeMemoryType()
        {
            if ((this.IsMemoryTypeNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializePositionInRow()
        {
            if ((this.IsPositionInRowNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializePoweredOn()
        {
            if ((this.IsPoweredOnNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeRemovable()
        {
            if ((this.IsRemovableNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeReplaceable()
        {
            if ((this.IsReplaceableNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeSpeed()
        {
            if ((this.IsSpeedNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeTotalWidth()
        {
            if ((this.IsTotalWidthNull == false))
            {
                return true;
            }
            return false;
        }

        private bool ShouldSerializeTypeDetail()
        {
            if ((this.IsTypeDetailNull == false))
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

        private static string ConstructPath(string keyTag)
        {
            string strPath = "root\\cimv2:Win32_PhysicalMemory";
            strPath = string.Concat(strPath, string.Concat(".Tag=", string.Concat("\"", string.Concat(keyTag, "\""))));
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
        public static PhysicalMemoryCollection GetInstances()
        {
            return GetInstances(null, null, null);
        }

        public static PhysicalMemoryCollection GetInstances(string condition)
        {
            return GetInstances(null, condition, null);
        }

        public static PhysicalMemoryCollection GetInstances(System.String[] selectedProperties)
        {
            return GetInstances(null, null, selectedProperties);
        }

        public static PhysicalMemoryCollection GetInstances(string condition, System.String[] selectedProperties)
        {
            return GetInstances(null, condition, selectedProperties);
        }

        public static PhysicalMemoryCollection GetInstances(System.Management.ManagementScope mgmtScope, System.Management.EnumerationOptions enumOptions)
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
            pathObj.ClassName = "Win32_PhysicalMemory";
            pathObj.NamespacePath = "root\\cimv2";
            System.Management.ManagementClass clsObject = new System.Management.ManagementClass(mgmtScope, pathObj, null);
            if ((enumOptions == null))
            {
                enumOptions = new System.Management.EnumerationOptions();
                enumOptions.EnsureLocatable = true;
            }
            return new PhysicalMemoryCollection(clsObject.GetInstances(enumOptions));
        }

        public static PhysicalMemoryCollection GetInstances(System.Management.ManagementScope mgmtScope, string condition)
        {
            return GetInstances(mgmtScope, condition, null);
        }

        public static PhysicalMemoryCollection GetInstances(System.Management.ManagementScope mgmtScope, System.String[] selectedProperties)
        {
            return GetInstances(mgmtScope, null, selectedProperties);
        }

        public static PhysicalMemoryCollection GetInstances(System.Management.ManagementScope mgmtScope, string condition, System.String[] selectedProperties)
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
            System.Management.ManagementObjectSearcher ObjectSearcher = new System.Management.ManagementObjectSearcher(mgmtScope, new SelectQuery("Win32_PhysicalMemory", condition, selectedProperties));
            System.Management.EnumerationOptions enumOptions = new System.Management.EnumerationOptions();
            enumOptions.EnsureLocatable = true;
            ObjectSearcher.Options = enumOptions;
            return new PhysicalMemoryCollection(ObjectSearcher.Get());
        }

        [Browsable(true)]
        public static PhysicalMemory CreateInstance()
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
            return new PhysicalMemory(tmpMgmtClass.CreateInstance());
        }

        [Browsable(true)]
        public void Delete()
        {
            PrivateLateBoundObject.Delete();
        }

        public enum FormFactorValues
        {

            Unknown0 = 0,

            Other0 = 1,

            SIP = 2,

            DIP = 3,

            ZIP = 4,

            SOJ = 5,

            Proprietary = 6,

            SIMM = 7,

            DIMM = 8,

            TSOP = 9,

            PGA = 10,

            RIMM = 11,

            SODIMM = 12,

            SRIMM = 13,

            SMD = 14,

            SSMP = 15,

            QFP = 16,

            TQFP = 17,

            SOIC = 18,

            LCC = 19,

            PLCC = 20,

            BGA = 21,

            FPBGA = 22,

            LGA = 23,

            NULL_ENUM_VALUE = 24,
        }

        public enum MemoryTypeValues
        {

            Unknown0 = 0,

            Other0 = 1,

            DRAM = 2,

            Synchronous_DRAM = 3,

            Cache_DRAM = 4,

            EDO = 5,

            EDRAM = 6,

            VRAM = 7,

            SRAM = 8,

            RAM = 9,

            ROM = 10,

            Flash = 11,

            EEPROM = 12,

            FEPROM = 13,

            EPROM = 14,

            CDRAM = 15,

            Val_3DRAM = 16,

            SDRAM = 17,

            SGRAM = 18,

            RDRAM = 19,

            DDR = 20,

            DDR_2 = 21,

            NULL_ENUM_VALUE = 22,
        }

        public enum TypeDetailValues
        {

            Reserved = 1,

            Other0 = 2,

            Unknown0 = 4,

            Fast_paged = 8,

            Static_column = 16,

            Pseudo_static = 32,

            RAMBUS = 64,

            Synchronous = 128,

            CMOS = 256,

            EDO = 512,

            Window_DRAM = 1024,

            Cache_DRAM = 2048,

            Non_volatile = 4096,

            NULL_ENUM_VALUE = 8192,
        }

        // Enumerator implementation for enumerating instances of the class.
        public class PhysicalMemoryCollection : object, ICollection
        {

            private ManagementObjectCollection privColObj;

            public PhysicalMemoryCollection(ManagementObjectCollection objCollection)
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
                    array.SetValue(new PhysicalMemory(((System.Management.ManagementObject)(array.GetValue(nCtr)))), nCtr);
                }
            }

            public virtual System.Collections.IEnumerator GetEnumerator()
            {
                return new PhysicalMemoryEnumerator(privColObj.GetEnumerator());
            }

            public class PhysicalMemoryEnumerator : object, System.Collections.IEnumerator
            {

                private ManagementObjectCollection.ManagementObjectEnumerator privObjEnum;

                public PhysicalMemoryEnumerator(ManagementObjectCollection.ManagementObjectEnumerator objEnum)
                {
                    privObjEnum = objEnum;
                }

                public virtual object Current
                {
                    get
                    {
                        return new PhysicalMemory(((System.Management.ManagementObject)(privObjEnum.Current)));
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
