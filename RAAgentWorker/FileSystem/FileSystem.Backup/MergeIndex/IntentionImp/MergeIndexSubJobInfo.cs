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
    #region directives
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.Media.Service.DomainModel.DocAve6x;
    using System;
    using System.Collections.Generic;
    using System.Text;
    #endregion

    public class MergeIndexSubJobInfo
        : IMergeIndexSubJobInfo
    {
        public String ConnectionName { get; set; }
        public String ConnectionId { get; set; }
        public String FarmName { get; set; }
        public String WebAppName { get; set; }
        public String IndexVolume { get; set; }
        public BaseJobDto JobDto { get; set; }
        public LogicalDeviceDto IndexLogicalDevice { get; set; }
        public CacheSettingDto CacheSetting { get; set; }
        public List<MergeIndexJobState> MergeIndexJobsState { get; set; }
        public Boolean IgnoreUpdateJobState { get; set; }

        public MergeIndexSubJobInfo()
        { }

        public MergeIndexSubJobInfo(MergeIndexJobInfo info)
        {
            this.FarmName = info.FarmName;
            this.ConnectionName = info.ConnectionName;
            this.CacheSetting = info.CacheLocation;
            this.IndexLogicalDevice = info.IndexLogicalDevice;
            this.WebAppName = info.WebAppName;
            this.JobDto = info.JobDto;
            this.ConnectionId = info.ConnectionId;
            this.MergeIndexJobsState = info.MergeIndexJobsState;
            var volumeGenerator = new ArchiverVolumeGenerator();
            var volumeParam = new VolumeParameter() { ConnectionId = this.ConnectionId,ConnectionName = this.ConnectionName};
            this.IndexVolume = volumeGenerator.GenerateIndexVolume(volumeParam);

        }

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("MergeIndexSubJobInfo: JobId: ");
            sb.Append(MergeIndexJobsState[0].JobId ?? "fake Id");
            sb.Append(" FarmName: ");
            sb.Append(FarmName);
            sb.Append(" WebAppName: ");
            sb.Append(WebAppName);
            sb.Append(" SiteUrl: ");
            //sb.Append(SiteUrl);
            return sb.ToString();
        }
    }
}