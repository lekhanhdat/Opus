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




namespace AvePoint.Media.Service.ArchiverBackup
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using AvePoint.GCommon;
    using AvePoint.Media.Service.DomainModel;
    using Merged18NResources.MediaServiceArchiverBackup;
    using AvePoint.Media.Common;
    #endregion

    public class EndUserBrowserIndexService
        : ArchiverIndexServiceBase
        , IEndUserBrowserIndexService
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        Dictionary<String, Int64> retentionTimeSpanMap = new Dictionary<String, Int64>();

        public ArchiverBasicIndex GetCurrentIndex(String pathMd5)
        {
            return this.HeadAndBodyService.GetIndex(pathMd5);
        }

        public ArchiverBasicIndex GetParentIndex(String pathMd5)
        {
            return this.HeadAndBodyService.GetParentIndex(pathMd5);
        }

        public List<ArchiverBasicIndex> GetChildIndexList(String pathMd5)
        {
            var result = this.HeadAndBodyService.GetChildIndexList(pathMd5);
            foreach (ArchiverBasicIndex index in result)
            {
                var startTime = new DateTime(index.ArchiveTime);
                if (index.Attributes != null && index.Attributes.Contains("TimeZoneID"))
                {
                    var utcTime = new DateTime(index.ArchiveTime);
                    var timeZoneId = index.Attributes.Substring(index.Attributes.IndexOfIgnoreCase("TimeZoneID") + 11);
                    index.TimeZoneId = timeZoneId.Remove(timeZoneId.IndexOf(ServiceConstants.ExtraChar));
                }
                var retentionTime = this.GetRetentionTimeSpan(index.JobId);
                var finalTime = startTime.AddSeconds(retentionTime);
                index.FinalDisposition = finalTime.Ticks;
                if (retentionTime == -1)
                    index.FinalDisposition = -1;
                this.logger.Info(MediaServiceArchiverBackupResource.EndUserBrowserIndexServiceGetChildIndexListInfo, startTime, finalTime, index.ArchiveTime);
            }
            return result;
        }

        public List<ArchiverBasicIndex> GetChildIndexList(ArchiverIndexInfo info)
        {
            var result = new List<ArchiverBasicIndex>();
            var indexList = this.HeadAndBodyService.GetChildIndexList(info.PathMD5);
            if (info.Length != 0)
            {
                for (Int32 i = info.OffSet; i < info.OffSet + info.Length && i < indexList.Count && i >= info.OffSet; i++)
                { result.Add(indexList[i]); }
            }
            else
                result = indexList;
            foreach (ArchiverBasicIndex index in result)
            {
                var startTime = new DateTime(index.ArchiveTime);
                if (index.Attributes != null && index.Attributes.Contains("TimeZoneID"))
                {
                    var utcTime = new DateTime(index.ArchiveTime);
                    var timeZoneId = index.Attributes.Substring(index.Attributes.IndexOfIgnoreCase("TimeZoneID") + 11);
                    index.TimeZoneId = timeZoneId.Remove(timeZoneId.IndexOf(ServiceConstants.ExtraChar));
                    //var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                    //startTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, timeZoneInfo);
                }
                var retentionTime = this.GetRetentionTimeSpan(index.JobId);
                var finalTime = startTime.AddSeconds(retentionTime);
                index.FinalDisposition = finalTime.Ticks;
                if (retentionTime == -1)
                    index.FinalDisposition = -1;
                this.logger.Info(MediaServiceArchiverBackupResource.EndUserBrowserIndexServiceGetChildIndexListInfo, startTime, finalTime, index.ArchiveTime);
            }
            return result;
        }

        public Int64 GetChildCount(String pathMd5)
        {
            return this.HeadAndBodyService.GetChildCount(pathMd5);
        }

        Int64 GetRetentionTimeSpan(String jobId)
        {
            var result = default(Int64);
            if (!this.retentionTimeSpanMap.ContainsKey(jobId))
            {
                result = this.SiteMasterService.GetRetentionTimeSpanByJobId(jobId);
                this.retentionTimeSpanMap.Add(jobId, result);
            }
            else
                result = this.retentionTimeSpanMap[jobId];
            this.logger.Info(MediaServiceArchiverBackupResource.EndUserBrowserIndexServiceGetRetentionTimeSpanInfo, result);
            return result;
        }
    }
}