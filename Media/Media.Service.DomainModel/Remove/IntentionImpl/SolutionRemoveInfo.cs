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
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using global::Media.Common;
    #endregion

    public class SolutionRemoveInfo
        : IRemoveInfo
    {
        public String JobId { get; set; }
        public String PlanId { get; set; }
        public LogicalDeviceDto LogicalDevice { get; set; }
        public CacheSettingDto CacheSetting { get; set; }
        public List<SolutionFile> SolutionFiles { get; set; }

        public override String ToString()
        {
            return String.Format("JobId: {0}, SolutionFiles: {1}",
                this.JobId,
                this.TextSolutionList());
        }

        String TextSolutionList()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in this.SolutionFiles)
            {
                sb.Append(item.ToString()).Append("  ");
            }
            return sb.ToString();
        }

        public SolutionRemoveInfo()
        { }

        public SolutionRemoveInfo(SolutionCenterRemoveDataParamDto param)
        {
            JobId = param.JobId;
            LogicalDevice = param.LogicalDevice;
            CacheSetting = param.CacheSetting;
            PlanId = param.PlanId;
            var solutionFiles = new List<SolutionFile>();
            param.SolutionFiles.ForEach(historyVersion =>
            {
                var solutionFile = new SolutionFile()
                {
                    Name = historyVersion.Name,
                    IsChecked = historyVersion.IsChecked,
                    Version = historyVersion.Version,
                    CreateTime = historyVersion.CreateTime,
                    Description = historyVersion.Description,
                    Path = historyVersion.Path,
                    Type = EnumConverter.ToEnum<TreeNodeLevel>(historyVersion.Type.ToString()),
                };
                solutionFiles.Add(solutionFile);
            });
            SolutionFiles = solutionFiles;
        }
    }
}