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
    using AvePoint.GCommon.Contract.Media.Object.Exchange;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    #endregion

    public class ExchangePruningJob
    {
        public String RetentionJobId { get; set; }

        public Int32 RetentionJobWeight { get; set; }

        public Int32 RetentionJobType { get; set; }

        public String UserAddress { get; set; }

        public String PlanName { get; set; }

        public String PlanId { get; set; }

        public String CycleId { get; set; }

        public String BatchFilePath { get; set; }

        public List<String> EmailAddressList { get; set; }

        public Dictionary<String, String> StorageInfoMap { get; set; }

        public List<String> JobId { get; set; }

        public CacheSettingDto cache { get; set; }

        public LogicalDeviceDto logicalDevice { get; set; }

        public LogicalDeviceDto DestinationDevice { get; set; }

        public ExchangeOnlineBackupDataPruningMsg PruningMsg { get; set; }

        public RetentionType OperationType { get; set; }

        public string GroupId { get; set; }

        public string TenantGroupOwner { get; set; }

        public Boolean DeleteIndexAndDate { get; set; }

        public override String ToString()
        {
            return String.Format("Retention Job Id: {0}", this.RetentionJobId);
        }
    }
}
