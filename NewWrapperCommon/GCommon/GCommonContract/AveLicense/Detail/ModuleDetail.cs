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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Server.Common;

namespace AvePoint.GCommon.Contract.AveLicense.Detail
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot("LicenseModuleDetail")]
    public class LicenseModuleDetail
    {
        public static readonly int ServerQuantityUnlimited = -1;
        public static readonly int Office365QuantityUnlimited = -1;
        public static readonly long MigrationQuantiyUnlimited = -1;
        public static readonly DateTime ExpirationUnlimited = DateTime.MaxValue;
        public static readonly int DurationUnlimited = -1;
        public static readonly int UserQuantityUnlimited = -1;

        [DataMember]
        [XmlAttribute("Name")]
        public ModuleName Name { get; set; }

        [DataMember]
        [XmlAttribute("ExpireTime")]
        public DateTime ExpireTime { get; set; }

        [DataMember]
        [XmlAttribute("LicenseType")]
        public LicenseType LicenseType { get; set; }

        [DataMember]
        [XmlAttribute("EnableTime")]
        public DateTime EnableTime { get; set; }

        [DataMember]
        [XmlAttribute("IsSharedMigration")]
        public bool IsSharedMigration { get; set; }

        [DataMember]
        [XmlAttribute("HasEverRegisteredEnterpirse")]
        public bool HasEverRegisteredEnterpirse { get; set; }

        [DataMember]
        [XmlAttribute("ServerQuantity")]
        public int ServerQuantity { get; set; }

        [DataMember]
        [XmlAttribute("MigrationQuantity")]
        public long MigrationQuantity { get; set; }

        [DataMember]
        [XmlAttribute("Office365Quantity")]
        public int Office365Quantity { get; set; }

        [XmlAttribute("IsUsingSharepointTime")]
        public bool IsUsingSharepointTime { get; set; }

        /// <summary>
        /// 判断模块是否含有扩展性功能
        /// </summary>
        [DataMember]
        [XmlAttribute("HasEnforcer")]
        public bool HasEnforcer { get; set; }

        /// <summary>
        /// License 计费方式
        /// </summary>
        [DataMember]
        [XmlAttribute("LicensePayType")]
        public LicensePayType LicensePayType { get; set; }

        [DataMember]
        [XmlAttribute("UserSeatQuantity")]
        public int UserSeatQuantity { get; set; }

        [DataMember]
        [XmlIgnore]
        public List<FarmDto> RegisteredFarms { get; set; }

        [DataMember]
        [XmlIgnore]
        public long UsedMigrationQuantity { get; set; }

        [DataMember]
        [XmlIgnore]
        public int UsedServerQuantity { get; set; }

        [DataMember]
        public int? EffectiveDays { get; set; }

        [DataMember]
        [XmlIgnore]
        public int UsedUserSeatQuantity { get; set; }

        public bool IsEnterprised
        {
            get
            {
                return this.LicenseType == LicenseType.Enterprise;
            }
        }

        public ModuleContainer Container
        {
            get
            {
                if (Name.ToString().StartsWith("AD", StringComparison.OrdinalIgnoreCase)) return ModuleContainer.Administration;
                if (Name.ToString().StartsWith("CP", StringComparison.OrdinalIgnoreCase)) return ModuleContainer.Compliance;
                if (Name.ToString().StartsWith("DP", StringComparison.OrdinalIgnoreCase)) return ModuleContainer.DataProtection;
                if (Name.ToString().StartsWith("GA", StringComparison.OrdinalIgnoreCase)) return ModuleContainer.GavornanceAutomation;
                if (Name.ToString().StartsWith("MG", StringComparison.OrdinalIgnoreCase)) return ModuleContainer.Migration;
                if (Name.ToString().StartsWith("RC", StringComparison.OrdinalIgnoreCase)) return ModuleContainer.ReportCenter;
                if (Name.ToString().StartsWith("SO", StringComparison.OrdinalIgnoreCase)) return ModuleContainer.StorageOptimzation;
                if (Name.ToString().StartsWith("OF", StringComparison.OrdinalIgnoreCase)) return ModuleContainer.Office365;
                return default(ModuleContainer);
            }
        }

        public bool IsComplaint
        {
            get
            {
                bool complaint = true;
                if (this.Name == ModuleName.DP_VMBackup && this.ServerQuantity != ServerQuantityUnlimited)
                {
                    return UsedServerQuantity <= ServerQuantity;
                }
                if (this.LicensePayType == LicensePayType.UserSeat
                    && this.Container != ModuleContainer.Migration
                    && !(this.Container == ModuleContainer.StorageOptimzation && this.Name == ModuleName.SO_FSArchiver)
                    && this.UserSeatQuantity != ServerQuantityUnlimited
                    && this.UsedUserSeatQuantity > this.UserSeatQuantity)
                {
                    return false;
                }
                if (this.LicensePayType == LicensePayType.Server
                    && Container != ModuleContainer.Migration
                    && !(this.Container == ModuleContainer.StorageOptimzation && this.Name == ModuleName.SO_FSArchiver)
                    && Container != ModuleContainer.Office365
                    && RegisteredFarms != null
                    && this.ServerQuantity != ServerQuantityUnlimited)
                {
                    int registeredServers = 0;
                    RegisteredFarms.ForEach(f => { if (f.FarmService != null) registeredServers += f.FarmService.FarmServiceCount; });
                    complaint = registeredServers <= ServerQuantity;
                }

                return complaint;
            }
        }

        public void SetEnableTime(DateTime time)
        {
            this.EnableTime = time;
            if (EffectiveDays.HasValue)
            {
                this.ExpireTime = this.EffectiveDays == DurationUnlimited ? ExpirationUnlimited :
                    this.EnableTime + new TimeSpan(this.EffectiveDays.Value, 0, 0, 0);
            }
        }

        public bool IsModuleExpired(DateTime systemTime)
        {
            bool isExpired = false;
            if (this.ExpireTime != ExpirationUnlimited && systemTime.Ticks - this.ExpireTime.Ticks > 0)
            {
                isExpired = true;
            }
            return isExpired;
        }

    }
}
