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

using AvePoint.Api.Contract.Job;
using DocAveOnline.WebApi.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Archiver.Scan.Base
{
    public class EndUserArchiveContainerConfig
    {
        public string JobId { get; set; }
        public string ContainerId;

        public HashSet<EndUserArchiveSiteCollectionConfig> SiteCollectionConfigs;
    }

    public class EndUserArchiveSiteCollectionConfig
    {
        public EndUserArchiveSiteCollectionConfig() 
        {
            FileInfoList = new List<EndUserFileInfo>();
            SkipFileInfoList = new List<EndUserFileInfo>();
            ExceptionFileInfoList = new List<EndUserFileInfo>();
        }
        public string Office365TenantId { get; set; }
        public string SiteCollectionId { get; set; }
        public List<EndUserFileInfo> FileInfoList { get; set; } //file info
        public List<EndUserFileInfo> SkipFileInfoList { get; set; }
        public List<EndUserFileInfo> ExceptionFileInfoList { get; set; }
        public ApiRuleAction RuleAction { get; set; } //rule action, ex: Archive, DeleteOnly
        public string SiteCollectionErrorMessage { get; set; }
    }

    public class EndUserFileInfo
    {
        public Guid SiteCollectionId { get; set; }
        public Guid WebId { get; set; }
        public int Id { get; set; }
        public string FullPath { get; set; }
        public bool FullPathAlreadyUriDecoded { get; set; }
        public JobDetailsStatus Status { get; set; }
        public string ErrorMessage { get; set; }

        public string GetDecodedFullPath()
        {
            return (FullPathAlreadyUriDecoded ? FullPath : Uri.UnescapeDataString(FullPath)).Trim().Replace('\\', '/');
        }
    }

    public enum ApiRuleAction
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Archive = 1024,
        [EnumMember]
        DeleteOnly = 16384,
    }
}
