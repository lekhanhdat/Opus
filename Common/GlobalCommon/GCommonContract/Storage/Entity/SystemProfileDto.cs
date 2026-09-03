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



using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.GCommon.Contract.Storage.Entity
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot("SystemProfileDto")]
    public class SystemProfileDto : IProfileContent
    {
        [DataMember]
        [XmlAttribute("id")]
        public string Id { get; set; }

        [DataMember]
        [XmlAttribute("name")]
        public string Name { get; set; }

        [DataMember]
        [XmlAttribute("description")]
        public string Description { get; set; }

        [DataMember]
        [XmlAttribute("systemAddress")]
        public string SystemAddress { get; set; }

        [DataMember]
        [XmlAttribute("username")]
        public string Username { get; set; }

        [DataMember]
        [XmlAttribute("password")]
        public string Password { get; set; }

        [DataMember]
        [XmlAttribute("connectionType")]
        public ConnectionType ConnType { get; set; }

        [DataMember]
        [XmlAttribute("port")]
        public int Port { get; set; }

        [DataMember]
        [XmlAttribute("mode7")]
        public bool Mode7 { get; set; }

        [DataMember]
        [XmlAttribute("modeC")]
        public bool ModeC { get; set; }

        [DataMember]
        [XmlAttribute("preferredIP")]
        public string PreferredIP { get; set; }

        [DataMember]
        [XmlAttribute("isUseTunneling")]
        public bool IsUseTunneling { get; set; }

        [DataMember]
        [XmlAttribute("tnneling")]
        public string Tunneling { get; set; }

        [DataMember]
        [XmlAttribute("version")]
        public string Version { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot("OntapItemInfo")]
    public class OntapItemInfo
    {
        [DataMember]
        [XmlAttribute("location")]
        public string Location { get; set; }

        [DataMember]
        [XmlAttribute("name")]
        public string Name { get; set; }

        [DataMember]
        [XmlAttribute("description")]
        public string Description { get; set; }

        [DataMember]
        [XmlAttribute("freeSpace")]
        public long FreeSpace { get; set; }

        [DataMember]
        [XmlAttribute("Lun")]
        public int Lun { get; set; }

        [DataMember]
        [XmlAttribute("totalSize")]
        public long TotalSize { get; set; }

        [DataMember]
        [XmlAttribute("LunPath")]
        public string LunPath { get; set; }

        [DataMember]
        [XmlAttribute("StorageSystem")]
        public string StorageSystem { get; set; }

        [DataMember]
        [XmlAttribute("StorageSystemPath")]
        public string StorageSystemPath { get; set; }

        [DataMember]
        [XmlAttribute("Type")]
        public string Type { get; set; }

        [DataMember]
        [XmlAttribute("DiskSerialNumber")]
        public string DiskSerialNumber { get; set; }

        [DataMember]
        [XmlAttribute("BackedBySnapshot")]
        public string BackedBySnapshot { get; set; }

        [DataMember]
        [XmlAttribute("Shared")]
        public bool Shared { get; set; }

        [DataMember]
        [XmlAttribute("SCSIPort")]
        public int SCSIPort { get; set; }

        [DataMember]
        [XmlAttribute("Bus")]
        public int Bus { get; set; }

        [DataMember]
        [XmlAttribute("Target")]
        public int Target { get; set; }

        [DataMember]
        [XmlAttribute("UNCPath")]
        public string UNCPath { get; set; }

        [DataMember]
        [XmlAttribute("BootOrSystemDisk")]
        public bool BootOrSystemDisk { get; set; }

        [DataMember]
        [XmlAttribute("Status")]
        public bool Status { get; set; }

        [DataMember]
        [XmlAttribute("IsSnaplock")]
        public bool IsSnaplock { get; set; }

        [DataMember]
        [XmlAttribute("Readonly")]
        public bool Readonly { get; set; }

        [DataMember]
        [XmlAttribute("SnapmirrorSource")]
        public bool SnapmirrorSource { get; set; }

        [DataMember]
        [XmlAttribute("SnapvaultPrimary")]
        public bool SnapvaultPrimary { get; set; }

        [DataMember]
        [XmlAttribute("DiskPartitionStyle")]
        public string DiskPartitionStyle { get; set; }

        [DataMember]
        [XmlAttribute("CloneSplitRestoreStatus")]
        public string CloneSplitRestoreStatus { get; set; }

        [DataMember]
        [XmlAttribute("DiskID")]
        public int DiskID { get; set; }

        [DataMember]
        [XmlAttribute("VolumeName")]
        public string VolumeName { get; set; }

        [DataMember]
        [XmlAttribute("MountPoints")]
        public List<string> MountPoints { get; set; }

        [DataMember]
        [XmlAttribute("IPAddresses")]
        public string IPAddresses { get; set; }

        [DataMember]
        [XmlAttribute("iSCSIInitiator")]
        public string iSCSIInitiator { get; set; }

    }

    [DataContract]
    public enum ConnectionType
    {
        [EnumMember]
        None = -1,
        [EnumMember]
        HTTP = 0,
        [EnumMember]
        HTTPS = 1,
        [EnumMember]
        RPC = 2,
    }

    [DataContract]
    public class LUNMonitorDto
    {
        [DataMember]
        public ServiceType ServiceType { get; set; }

        [DataMember]
        public ServiceDto Service { get; set; }
    }

    [DataContract]
    public class ErrorMessageKey
    {
        public const string NAME_KEY = "ErrorName";
        public const string SYSTEM_ADRESS_KEY = "ErrorSystemAdress";
        public const string USERNAME_KEY = "ErrorUsername";
        public const string PASSWORD_KEY = "ErrorPassword";
        public const string PORT_KEY = "ErrorPort";
        public const string PREFERRED_ID_KEY = "ErrorPreferredId";
        public const string TUNNELING = "ErrorTunneling";
        public const string TEST_MESSAGE_KEY = "ErrorTestMessage";
        public const string VERSION = "Version";
    }

    [DataContract]
    public class SystemProfileDetailDto
    {
        [DataMember]
        public PhysicalDeviceDto PhysicalDevice { get; set; }
        [DataMember]
        public long TotalSpace { get; set; }
        [DataMember]
        public long UseSpase { get; set; }
        [DataMember]
        public long FreeSpace { get; set; }    //剩余空间
    }

}
