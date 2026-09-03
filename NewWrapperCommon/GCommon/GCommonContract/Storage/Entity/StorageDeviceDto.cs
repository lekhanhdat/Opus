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
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AvePoint.GCommon.Contract.Storage.Entity
{
    public class StorageDeviceDto
    {
        public bool DAOMigrated { set; get; }
        public bool IsAveStorage { set; get; }
        public string DAOStoragePolicyId { get; set; }
        public string DAOLogicalDeviceId { get; set; }
        public string DAOPhysicalDeviceId { get; set; }

        public string Id { get; set; }
        public string AuditId { get; set; }
        public int Type { get; set; }
        public string Name { get; set; }
        public string ConnectionString { get; set; }

        public string Description { get; set; }

        private long storageDeviceSpace = -1;
        public long ModifyTime { get; set; }  //Physical device的修改时间。
        public int Status { get; set; }  //判断是否是删除的Physical device,以及是否是修改Physical device后新建立的Physical Device
        public long FreeSpace { get; set; }
        public bool IsEncryptPassword { get; set; }
        public string BackupPhysicalDeviceId { get; set; }
        public string LastModifiedTime { get; set; }
        public string LastArchivedTime { get; set; }
        public bool IsUsingDevice { get; set; }
        public long StorageDeviceSpace
        {
            get
            {
                return this.storageDeviceSpace;
            }
            set
            {
                this.storageDeviceSpace = value;
            }
        }
        private const string PASSWORD_PATTERN = "([^&]*)secret=([^&]*)";
        private readonly static Regex r = new Regex(PASSWORD_PATTERN);
        private List<string> password;
        public List<string> Password
        {
            get
            {
                lock (r)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(ConnectionString))
                        {
                            Match m = r.Match(ConnectionString);
                            if (m.Success)
                            {
                                password = new List<string>();
                            }
                            while (m.Success)
                            {
                                password.Add("&" + m.Groups[0].Value);
                                m = m.NextMatch();

                            }
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.Message, e);
                    }
                }
                return password;
            }
        }
        public StorageDeviceExtension Extension { get; set; }

        private float useSpace = -1;
        public float UseSpace
        {
            get
            {
                return this.useSpace;
            }
            set
            {
                this.useSpace = value;
            }
        }
        public int SpaceType { get; set; }
        public UIXRI mCurrentXRI { get; set; }

        public bool SetupDataRetention { set; get; }

        public ScheduleDto Schedule { get; set; }

        public string NotificationId { get; set; }

        public List<RetentionRule> ArchiveRetentionRules { get; set; }

        public int CompressionSpeed { get; set; }
        public bool UseCompression { get; set; }
        public bool UseEncryption { get; set; }
        public string EncryptionProfileId { get; set; }
        public bool IsSystemStorage { get; set; }
        public void UpdatePassword(List<string> newPassword)
        {
            for (int i = 0; i < newPassword.Count; i++)
            {
                ConnectionString = ConnectionString.Replace(password[i], newPassword[i]);
            }
            password = newPassword;
        }

        public string BuildValidateXRI()
        {
            string xri = BuildXRI(true);
            if (!xri.Contains("&isvalidate="))
            {
                xri += "&isvalidate=true";
            }
            return xri;
        }
        private string BuildXRI(bool creation)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(ConnectionString);
            if (!sb.ToString().Contains("&id="))
            {
                sb.Append("&id=");
                sb.Append(Id);
            }
            //add space param
            if (SpaceType == 0 && StorageDeviceSpace >= 0)
            {
                if (!sb.ToString().Contains("&spaceThresholdUnit=".ToLower()))
                {
                    sb.Append("&spaceThresholdUnit=".ToLower());
                    sb.Append(1);
                }
                if (!sb.ToString().Contains("&spaceThreshold=".ToLower()))
                {
                    sb.Append("&spaceThreshold=".ToLower());
                    sb.Append(StorageDeviceSpace);
                }
            }
            else if (SpaceType == 1 && UseSpace >= 0)
            {
                if (!sb.ToString().Contains("&spaceThresholdUnit=".ToLower()))
                {
                    sb.Append("&spaceThresholdUnit=".ToLower());
                    sb.Append(2);
                }
                if (!sb.ToString().Contains("&spaceThreshold=".ToLower()))
                {
                    sb.Append("&spaceThreshold=".ToLower());
                    sb.Append(UseSpace);
                }
            }
            if (!sb.ToString().Contains("&modifyTime=".ToLower()))
            {
                sb.Append("&modifyTime=".ToLower());
                sb.Append(ModifyTime);
            }
            if (!sb.ToString().Contains("&creation="))
            {
                sb.Append("&creation=");
                sb.Append(creation);
            }

            if (!sb.ToString().Contains("&assignedspace=") && Extension != null && Extension.TotalSpace > 0)
            {
                sb.Append("&assignedspace=");
                sb.Append(Extension.TotalSpace);
                sb.Append("&usedspace=");
                sb.Append(Extension.UsedSpace);
            }
            return sb.ToString();
        }
        public string BuildXRI()
        {
            return BuildXRI(true);
        }
        public class XRIUtil
        {

            public static string ValueEncode(string value)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return value;
                }
                return value.Replace("%", "%25").Replace("&", "%26").Replace("=", "%3D").Replace("^", "%5e");
                //return value.Replace("%", "%25").Replace("&", "%26").Replace("=", "%3D");
            }

            public static string ValueDecode(string value)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return value;
                }
                return value.Replace("%3D", "=").Replace("%26", "&").Replace("%25", "%").Replace("%5e", "^");
                //return value.Replace("%3D", "=").Replace("%26", "&").Replace("%25", "%");
            }

        }
    }

    public class StorageDeviceExtension
    {
        [DataMember]
        public long UsedSpace { get; set; }
        [DataMember]
        public long TotalSpace { get; set; }
    }
}
