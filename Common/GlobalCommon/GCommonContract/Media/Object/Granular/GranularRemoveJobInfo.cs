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
    #endregion

    public class GranularRemoveJobInfo
    {
        public Boolean IsCycle { get; set; }
        public String FarmName { get; set; }
        public String PlanId { get; set; }
        public String CycleId { get; set; }
        public List<String> JobId { get; set; }
        public List<String> SiteUrls { get; set; }
        public CacheSettingDto Cache { get; set; }
        public LogicalDeviceDto LogicalDevice { get; set; }
        public Dictionary<String, String> StorageInfoMap { get; set; }

        public override String ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("Farm Name: {0}, ", this.FarmName);
            stringBuilder.AppendFormat("Plan Id: {0}, ", this.PlanId);
            stringBuilder.AppendFormat("Cache: {0}, ", this.Cache);
            stringBuilder.AppendFormat("Logical Device: {0}, ", this.LogicalDevice);
            return stringBuilder.ToString();
        }
    }
}
