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




using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.Wrapper.Common;
using System.Collections.Generic;
using System.Linq;
using AvePoint.Wrapper.Restore;
using System;
namespace AvePoint.Item.Restore
{
    public enum RestoreStatus
    {
        Success,
        Failed,
        Skipped,
        Exception = 4,
        ContainerFailed = 5
    }

    public class AveRestoreReportDto
    {
        public JobReportDetailEntityType EntityType { get; set; }
        /// <summary>
        /// Only for configuration details, configuration name, remark3
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Only for configuration details, object name responding with the configuration
        /// </summary>
        public string RelatedObjectTitle { get; set; }

        public string Path { get; set; }
        
        public string Title { get; set; }

        public string SourcePath { get; set; }

        public string Type { get; set; }

        public string Version { get; set; }

        /// <summary>
        /// Only for object details
        /// </summary>
        public long Size { get; set; }

        public RestoreStatus Status { get; set; }

        public string ErrorMessage { get; set; }

        public int ConflictResolution { get; set; }

        public int PolicyLevel { get; set; }

        public string PathMD5 { get; set; }
        public string DestinationUrl { get; set; }

        public long StartTime { get; set; } // for restore migration report

        /// <summary>
        /// New Created, Replaced, Skipped, Overwriten, Appended
        /// </summary>
        private RestoreReportOption option;

        public string Option 
        {
            get
            {
                if (this.Status == RestoreStatus.Skipped)
                {
                    return RestoreReportOption.Skipped.ToString();
                }
                return option.GetResourceString();
            }
        }

        public bool IsFailedNode
        {
            get { return EntityType == JobReportDetailEntityType.NormalInfo && Status == RestoreStatus.Failed; }
        }

        public bool IsSuccessNode
        {
            get { return EntityType == JobReportDetailEntityType.NormalInfo && Status == RestoreStatus.Success; }
        }

        public AveRestoreReportDto()
        {
            EntityType = JobReportDetailEntityType.NormalInfo;
            StartTime = DateTime.UtcNow.Ticks;
        }

        public AveRestoreReportDto(AveWrapperReportDto dto, RestoreContentDto contentDto)
        {
            this.EntityType = JobReportDetailEntityType.Configuration;
            this.Name = dto.Name;
            this.RelatedObjectTitle = dto.RelatedObjectTitle;
            this.Type = dto.Type;
            this.Status = (RestoreStatus)dto.Status;
            this.Path = contentDto.Name;
            this.Title = contentDto.Name;
            this.SourcePath = contentDto.SrcUrl;
            this.ErrorMessage = dto.ErrorMessage;
            this.PathMD5 = contentDto.ItemPathMd5;
        }

        public static IEnumerable<AveRestoreReportDto> Parse(IEnumerable<AveWrapperReportDto> dtos, RestoreContentDto content)
        {
            return dtos.Select(wrapperReportDto => new AveRestoreReportDto(wrapperReportDto, content));
        }

        /// <summary>
        /// Set Option
        /// </summary>
        /// <param name="restoreMode">AveRestoreMode: only [Replace, Append, AppendANewVersion, OverWrite, OverWriteByModifiedTime, Skipped] are valid</param>
        /// <param name="isExist">Is SPObject exist in destination. If it is null, it means restore has encountered some error.</param>
        public void SetOption(AveRestoreMode restoreMode, bool? isExist, RestoreStatus status)
        {
            if (status == RestoreStatus.Skipped)
            {
                this.option = RestoreReportOption.Skipped;
                return;
            }
            if (!isExist.HasValue)
            {
                this.option = status == RestoreStatus.Failed ? default(RestoreReportOption) : RestoreReportOption.Skipped;
                return;
            }
            if (isExist == false && restoreMode != AveRestoreMode.Append)
            {
                this.option = RestoreReportOption.NewCreated;
                return;
            }
            this.option = GetMappedReportOption(restoreMode);
        }

        private RestoreReportOption GetMappedReportOption(AveRestoreMode restoreMode)
        {
            RestoreReportOption option = default(RestoreReportOption);
            switch (restoreMode)
            {
                case AveRestoreMode.Replace:
                    option = RestoreReportOption.Replaced;
                    break;
                case AveRestoreMode.Append:
                case AveRestoreMode.AppendANewVersion:
                    option = RestoreReportOption.Appended;
                    break;
                case AveRestoreMode.OverWrite:
                case AveRestoreMode.OverWriteByModifiedTime:
                case AveRestoreMode.OverWrite | AveRestoreMode.RestoreProperty:
                    option = RestoreReportOption.Overwritten;
                    break;
                case AveRestoreMode.Default:
                    option = RestoreReportOption.Skipped;
                    break;
                default:
                    option = RestoreReportOption.None;
                    break;
            }
            return option;
        }

        public void GetConflictResolution(AveRestoreMode restoreMode)
        {
            if (restoreMode == AveRestoreMode.Default)
            {
                if (this.option == RestoreReportOption.NewCreated)
                {
                    this.ConflictResolution = (int)RestoreReportOption.None;
                    return;
                }
                this.ConflictResolution = (int)RestoreReportOption.Skipped;
                return;
            }

            if (restoreMode == AveRestoreMode.OverWrite)
            {
                if(this.option == RestoreReportOption.Skipped)
                {
                     this.ConflictResolution = (int)RestoreReportOption.Skipped;
                     return;
                }
                else if (this.option == RestoreReportOption.NewCreated)
                {
                    this.ConflictResolution = (int)RestoreReportOption.Overwritten;
                    return;
                }
            }

            if ((restoreMode == AveRestoreMode.Append || restoreMode == AveRestoreMode.AppendANewVersion) 
                && this.option == RestoreReportOption.Appended)
            {
                this.ConflictResolution = (int)RestoreReportOption.Appended;
            }
        }
    }
}
