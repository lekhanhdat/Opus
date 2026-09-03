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




namespace AvePoint.GCommon.Contract.Media.Object
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;

    #endregion using directives

    public class PlatformRetentionInfo
    {
        public Boolean IsNeedJob { get; set; }

        public String PlanId { get; set; }

        public String FarmName { get; set; }

        public String PlanName { get; set; }

        public String BatchFilePath { get; set; }

        public List<String> JobId { get; set; }

        public String RetentionJobId { get; set; }

        public CacheSettingDto Cache { get; set; }

        public Int32 RetentionJobType { get; set; }

        public Int32 RetentionJobWeight { get; set; }

        public PlatformType PlatformType { get; set; }

        public ProductVersion ProductVersion { get; set; }

        public LogicalDeviceDto LogicalDevice { get; set; }

        public LogicalDeviceDto DestinationDevice { get; set; }

        public Dictionary<String, String> StorageInfoMap { get; set; }

        public Dictionary<String, String> BlobIndexGuidMap { get; set; }

        public Dictionary<String, String> StorageInfoExtensionMap { get; set; }

        public override String ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("Platform Retention Info: ");
            stringBuilder.AppendFormat("Plan Id: {0}, ", this.PlanId);
            stringBuilder.AppendFormat("Retention Job Id: {0}, ", this.RetentionJobId);
            stringBuilder.AppendFormat("Retention Job Type: {0}, ", this.RetentionJobType);
            stringBuilder.AppendFormat("Cache: {0}, ", this.Cache);
            stringBuilder.AppendFormat("Logical Device: {0}", this.LogicalDevice);
            return stringBuilder.ToString();
        }
    }
}