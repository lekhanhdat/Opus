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
    using AvePoint.GCommon.Contract.Server.ExportAndImport;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.Tree.Object;
    #endregion

    public class UpgradeBrowseInfo
    {
        public EITreeNodeDto Node { get; set; }

        public PlatformType PlatformType { get; set; }

        public ProductVersion Version { get; set; }

        public ImportDataVersion DataVersion { get; set; }

        public List<LogicalDeviceDto> LogicalDevices { get; set; }

        public CacheSettingDto CacheSetting { get; set; }

        public override String ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("Upgrade Browse Info: ");
            stringBuilder.AppendFormat("Node: {0}, ", this.Node);
            stringBuilder.AppendFormat("Platform Type: {0}, ", this.PlatformType);
            stringBuilder.AppendFormat("Version: {0}, ", this.Version);
            stringBuilder.AppendFormat("Data Version: {0}", this.DataVersion);
            return stringBuilder.ToString();
        }
    }
}
