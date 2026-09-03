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



namespace AvePoint.GCommon.Utility.VSS
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Management;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;

    [AveVersion("$Revision:  $")]
    [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "iSCSIUtility is unmodifiable as the cause of being referenced.")]
    public sealed class iSCSIUtility
    {
        /// <summary>
        /// Retrieves the iSCSI Target name by path, returns the corresponding target name or null while the input path contains no mount point.
        /// </summary>
        public static string GetiSCSITargetNameByPath(string path)
        {
            string target = null;

            try
            {
                string name = GetDriveLetterByMountPointPath(path) ?? path.Substring(0, 2);
                uint index = LogicalDiskToPartition.Single(ldtop => ldtop.Dependent.Equals(name)).Antecedent;
                string pnpId = DiskDrive.Single(drive => drive.Index.Equals(index)).PNPDeviceID;
                target = iSCSISession.Single(
                    session => session.Devices.Any(device => device.DeviceInterfaceName.Equals(pnpId))).TargetName;
            }
            catch (InvalidOperationException)
            {
            }

            return target;
        }

        /// <summary>
        /// Retrieves all mount points on local machine, each mount point contains a directory path and a volume id, a driver letter root directory also considered as a mount point.
        /// </summary>
        public static IEnumerable<MountPoint> MountPoint
        {
            get
            {
                return from ManagementObject mo in new ManagementClass(@"\root\CIMV2:Win32_MountPoint").GetInstances() select new MountPoint(trim(mo.Properties["Directory"].Value), trim(mo.Properties["Volume"].Value));
            }
        }

        /// <summary>
        /// Retrieves the mapping of all logical disks and partitions.
        /// </summary>
        public static IEnumerable<LogicalDiskToPartition> LogicalDiskToPartition
        {
            get
            {
                return from ManagementObject mo in new ManagementClass(@"\root\CIMV2:Win32_LogicalDiskToPartition").GetInstances() select new LogicalDiskToPartition(diskIndex(trim(mo.Properties["Antecedent"].Value)), trim(mo.Properties["Dependent"].Value));
            }
        }

        /// <summary>
        /// Retrieves all volumes on local machine.
        /// </summary>
        public static IEnumerable<Volume> Volume
        {
            get
            {
                return from ManagementObject mo in new ManagementClass(@"\root\CIMV2:Win32_Volume").GetInstances() select new Volume(mo);
            }
        }

        /// <summary>
        /// Retrieves all logical disks on local machine.
        /// </summary>
        public static IEnumerable<LogicalDisk> LogicalDisk
        {
            get
            {
                return from ManagementObject mo in new ManagementClass(@"\root\CIMV2:Win32_LogicalDisk").GetInstances() select new LogicalDisk(mo);
            }
        }

        /// <summary>
        /// Retrieves all disk drive on local machine.
        /// </summary>
        public static IEnumerable<DiskDrive> DiskDrive
        {
            get
            {
                return from ManagementObject mo in new ManagementClass(@"\root\CIMV2:Win32_DiskDrive").GetInstances() select new DiskDrive(mo);
            }
        }

        /// <summary>
        /// Retrieves all iSCSI sessions on local machine.
        /// </summary>
        public static IEnumerable<iSCSISession> iSCSISession
        {
            get
            {
                return from ManagementObject mo in new ManagementClass(@"\root\WMI:MSIscsiInitiator_SessionClass").GetInstances() select new iSCSISession(mo);
            }
        }

        /// <summary>
        /// Retrieves the common initiator node name that is used when establishing sessions from the local machine.
        /// </summary>
        public static string IscsiInitiatorNodeName
        {
            get
            {
                byte[] buffer = new byte[iscsidsc_dll.MAX_ISCSI_NAME_LEN + 1];
                string name = null;

                if (iscsidsc_dll.ERROR_SUCCESS == iscsidsc_dll.GetIScsiInitiatorNodeNameA(buffer))
                {
                    name = Encoding.ASCII.GetString(buffer, 0, Array.IndexOf<byte>(buffer, 0));
                }
                else
                {
                    throw new InvalidOperationException();
                }

                return name;
            }
        }

        /// <summary>
        /// Retrieves the list of targets that the iSCSI initiator service has discovered, and can also instruct the iSCSI initiator service to refresh the list.
        /// </summary>
        public static string[] IscsiTargetsOnLocalMachine
        {
            get
            {
                uint contentSize = 0;
                string[] targets = new string[0];
                iscsidsc_dll.ReportIScsiTargetsA(iscsidsc_dll.TRUE, ref contentSize, null);

                if (contentSize > 0)
                {
                    byte[] buffer = new byte[contentSize];

                    if (iscsidsc_dll.ERROR_SUCCESS == iscsidsc_dll.ReportIScsiTargetsA(iscsidsc_dll.TRUE, ref contentSize, buffer))
                    {
                        targets = DoubleNullTerminatedToStringArray(buffer);
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                }

                return targets;
            }
        }

        /// <summary>
        /// Retrieves the list of initiator Host Bus Adapters that are running on the machine.
        /// </summary>
        public static string[] IscsiInitiatorsOnLocalMachine
        {
            get
            {
                uint contentSize = 0;
                string[] targets = new string[0];
                iscsidsc_dll.ReportIScsiInitiatorListA(ref contentSize, null);

                if (contentSize > 0)
                {
                    byte[] buffer = new byte[contentSize];

                    if (iscsidsc_dll.ERROR_SUCCESS == iscsidsc_dll.ReportIScsiInitiatorListA(ref contentSize, buffer))
                    {
                        targets = DoubleNullTerminatedToStringArray(buffer);
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                }

                return targets;
            }
        }

        /// <summary>
        /// Retrieves information about the initiator version.
        /// </summary>
        public static Version IscsiVersionInformation
        {
            get
            {
                Version version = null;
                iscsidsc_dll.ISCSI_VERSION_INFO info = new iscsidsc_dll.ISCSI_VERSION_INFO();

                if (iscsidsc_dll.ERROR_SUCCESS == iscsidsc_dll.GetIScsiVersionInformation(ref info))
                {
                    version = new Version((int)info.MajorVersion, (int)info.MinorVersion, (int)info.BuildNumber);
                }
                else
                {
                    throw new InvalidOperationException();
                }

                return version;
            }
        }

        #region Tool Helpers

        private static string trim(object src)
        {
            string s = src as string;

            if (null != s)
            {
                s = s.Substring(s.IndexOf('=') + 1).Replace(@"\\", @"\").Trim('\"');
            }

            return s ?? string.Empty;
        }

        private static uint diskIndex(string src)
        {
            int i = src.IndexOf('#') + 1;
            return uint.Parse(src.Substring(i, src.IndexOf(',') - i));
        }

        private static string GetDriveLetterByMountPointPath(string path)
        {
            string drive = null;

            try
            {
                Regex driveLetter = new Regex(regxDriveLetter);
                MountPoint[] cacheMountPoint = MountPoint.ToArray();
                IEnumerable<string> mountPoints = from point in cacheMountPoint where !driveLetter.IsMatch(point.Directory) select point.Directory;
                string explicitPoint = mountPoints.Single(path.StartsWith);
                string volume = cacheMountPoint.Single(point => point.Directory.Equals(explicitPoint)).Volume;
                drive = cacheMountPoint.Single(point => point.Volume.Equals(volume) && driveLetter.IsMatch(point.Directory)).Directory;
            }
            catch (InvalidOperationException)
            {
            }

            return drive;
        }

        private static readonly string regxVolumeId = @"^\\\\\?\\Volume\{[a-f0-9]{8}(-[a-f0-9]{4}){3}-[a-f0-9]{12}\}\\$";
        private static readonly string regxDriveLetter = @"^[A-Z]\:(\\)?$";

        private static string[] DoubleNullTerminatedToStringArray(byte[] rawBuffer)
        {
            List<string> result = new List<string>();
            int bufferIndex = 0;

            while (rawBuffer[bufferIndex] != 0)
            {
                int correntRecordLength = Array.IndexOf<byte>(rawBuffer, 0, bufferIndex) - bufferIndex;
                result.Add(Encoding.ASCII.GetString(rawBuffer, bufferIndex, correntRecordLength));
                bufferIndex += correntRecordLength + 1;
            }

            return result.ToArray();
        }

        private sealed class iscsidsc_dll
        {
            #region Constants and structs defined in iscsidsc.h

            public const UInt32 ERROR_SUCCESS = 0;
            public const UInt16 MAX_ISCSI_NAME_LEN = 223;
            public const Byte TRUE = 1;

            [StructLayout(LayoutKind.Sequential)]
            public struct ISCSI_VERSION_INFO
            {
                public UInt32 MajorVersion;
                public UInt32 MinorVersion;
                public UInt32 BuildNumber;
            }

            #endregion

            [DllImport("iscsidsc.dll")]
            public static extern UInt32 ReportIScsiTargetsA(Byte ForceUpdate, ref UInt32 BufferSize, Byte[] Buffer);

            [DllImport("iscsidsc.dll")]
            public static extern UInt32 ReportIScsiInitiatorListA(ref UInt32 BufferSize, Byte[] Buffer);

            [DllImport("iscsidsc.dll")]
            public static extern UInt32 GetIScsiInitiatorNodeNameA(Byte[] InitiatorNodeName);

            [DllImport("iscsidsc.dll")]
            public static extern UInt32 GetIScsiVersionInformation(ref ISCSI_VERSION_INFO VersionInfo);
        }

        #endregion
    }

    [AveVersion("$Revision:  $")]
    public sealed class MountPoint
    {
        public MountPoint(string dir, string vol)
        {
            directory = dir;
            volume = vol;
        }

        public string Directory { get { return directory.TrimEnd('\\'); } }
        public string Volume { get { return volume; } }

        private readonly string directory = null;
        private readonly string volume = null;
    }

    [AveVersion("$Revision:  $")]
    public sealed class LogicalDiskToPartition
    {
        public LogicalDiskToPartition(uint ant, string dep)
        {
            antecedent = ant;
            dependent = dep;
        }

        public uint Antecedent { get { return antecedent; } }
        public string Dependent { get { return dependent; } }

        private readonly uint antecedent = uint.MaxValue;
        private readonly string dependent = null;
    }

    [AveVersion("$Revision:  $")]
    public sealed class Volume : ManagementAdaptBase
    {
        public Volume(ManagementBaseObject obj)
            : base(obj)
        {
        }

        public string Name
        {
            get
            {
                return PropertyValue<string>("Name").TrimEnd('\\');
            }
        }

        public string DeviceID
        {
            get
            {
                return PropertyValue<string>("DeviceID");
            }
        }

        public string DriveLetter
        {
            get
            {
                return PropertyValue<string>("DriveLetter");
            }
        }
    }

    [AveVersion("$Revision:  $")]
    public sealed class LogicalDisk : ManagementAdaptBase
    {
        public LogicalDisk(ManagementBaseObject obj)
            : base(obj)
        {
        }

        public string DeviceID
        {
            get
            {
                return PropertyValue<string>("DeviceID");
            }
        }
    }

    [AveVersion("$Revision:  $")]
    public sealed class DiskDrive : ManagementAdaptBase
    {
        public DiskDrive(ManagementBaseObject obj)
            : base(obj)
        {
        }

        public uint Index
        {
            get
            {
                return PropertyValue<uint>("Index");
            }
        }

        public string PNPDeviceID
        {
            get
            {
                return PropertyValue<string>("PNPDeviceID");
            }
        }
    }

    [AveVersion("$Revision:  $")]
    public sealed class DiskPartition : ManagementAdaptBase
    {
        public DiskPartition(ManagementBaseObject obj)
            : base(obj)
        {
        }

        public uint DiskIndex
        {
            get
            {
                return PropertyValue<uint>("DiskIndex");
            }
        }
    }

    [AveVersion("$Revision:  $")]
    public sealed class iSCSISession : ManagementAdaptBase
    {
        public iSCSISession(ManagementBaseObject obj)
            : base(obj)
        {
        }

        public string InitiatorName
        {
            get
            {
                return PropertyValue<string>("InitiatorName");
            }
        }

        public string TargetName
        {
            get
            {
                return PropertyValue<string>("TargetName");
            }
        }

        /// <summary>
        /// Retrieves all devices belong to this session.
        /// </summary>
        public IEnumerable<iSCSIDevice> Devices
        {
            get
            {
                foreach (ManagementBaseObject mbo in PropertyValue<ManagementBaseObject[]>("Devices"))
                {
                    yield return new iSCSIDevice(mbo);
                }
            }
        }
    }

    [AveVersion("$Revision:  $")]
    public sealed class iSCSIDevice : ManagementAdaptBase
    {
        public iSCSIDevice(ManagementBaseObject obj)
            : base(obj)
        {
        }

        public string DeviceInterfaceName
        {
            get
            {
                string name = PropertyValue<string>("DeviceInterfaceName");
                return name.Substring(4, name.Length - 43).ToUpperInvariant().Replace('#', '\\');
            }
        }

        public string TargetName
        {
            get
            {
                return PropertyValue<string>("TargetName");
            }
        }
    }

    [AveVersion("$Revision:  $")]
    public sealed class iSCSITarget : ManagementAdaptBase
    {
        public iSCSITarget(ManagementBaseObject obj)
            : base(obj)
        {
        }

        public string TargetName
        {
            get
            {
                return PropertyValue<string>("TargetName");
            }
        }
    }

    [AveVersion("$Revision:  $")]
    public abstract class ManagementAdaptBase
    {
        protected ManagementAdaptBase(ManagementBaseObject obj)
        {
            handler = obj;
        }

        protected TValue PropertyValue<TValue>(string property)
        {
            return (TValue)(handler.Properties[property].Value);
        }

        private readonly ManagementBaseObject handler = null;
    }
}
