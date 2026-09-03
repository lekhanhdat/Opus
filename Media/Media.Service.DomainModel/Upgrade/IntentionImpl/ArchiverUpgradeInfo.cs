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




namespace AvePoint.Media.Service.DomainModel
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using global::Media.Common;
    #endregion

    public class ArchiverUpgradeInfo
        : UpgradeInfoBase
    {
        public Int32 JobType { set; get; }
        public String JobId { set; get; }
        public String PlanId { set; get; }
        public List<ArchiverUpgradeSubInfo> ImportInfos { set; get; }
        public LogicalDeviceDto OldIndexLogicalDevice { set; get; }
        public Dictionary<string, LogicalDeviceDto> NewIndexLogicalDevices { set; get; }
        public List<StoragePolicyDto> DataStoragePolicies { set; get; }
        public PlatformType PlatformType { set; get; }
        public ProductVersion ProductVersion { set; get; }
        public List<String> FailedSiteCollections { set; get; }
        public Dictionary<String, List<StoragePolicyDto>> JobStoragePolicies { set; get; }

        public ArchiverUpgradeInfo()
        { }

        public ArchiverUpgradeInfo(ArchiverUpgradeDto param)
        {
            JobId = param.JobId;
            PlanId = param.PlanId;
            JobType = param.JobType;
            ImportInfos = new List<ArchiverUpgradeSubInfo>();
            foreach (var subItem in param.ImportInfos)
            {
                ImportInfos.Add(new ArchiverUpgradeSubInfo(subItem));
            }
            OldIndexLogicalDevice = param.OldIndexLogicalDevice;
            NewIndexLogicalDevices = param.NewIndexLogicalDevices;
            DataStoragePolicies = param.DataStoragePolicies;
            PlatformType = EnumConverter.ToEnum<PlatformType>(param.PlatformType.ToString());
            ProductVersion = param.ProductVersion;
            FailedSiteCollections = param.FailedSiteCollections;
            JobStoragePolicies = param.JobStoragePolicies;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ArchiverImportSubInfo: ");
            sb.Append(JobId.ToString());
            sb.Append(" ");
            sb.Append(ImportInfos.ToString());
            sb.Append(" ");
            sb.Append(OldIndexLogicalDevice.ToString());
            sb.Append(" ");
            sb.Append(NewIndexLogicalDevices.ToString());
            sb.Append(" ");
            sb.Append(PlatformType.ToString());
            return sb.ToString();
        }
    }
}