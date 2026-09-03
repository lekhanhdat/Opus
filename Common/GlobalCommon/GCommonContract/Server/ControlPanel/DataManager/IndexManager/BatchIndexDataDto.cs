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



using System.Collections.Generic;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.DataManager.IndexManager
{

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BatchIndexDataDto
    {
        [DataMember]
        public List<RunType> RunTypeList { set; get; }

        [DataMember]
        public List<BatchCrawlIndexFarmDto> FarmDtoList { set; get; }

        [DataMember]
        public BatchCrawlIndexJobDto JobDto { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BatchCrawlIndexFarmDto
    {
        [DataMember]
        public FarmDto Farm { set; get; }

        [DataMember]
        public List<BatchCrawlIndexWebDto> WebDtoList { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BatchCrawlIndexWebDto
    {
        [DataMember]
        public string WebUrl { set; get; }

        [DataMember]
        public List<BatchCrawlIndexSiteDto> SiteDtoList { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BatchCrawlIndexSiteDto
    {
        [DataMember]
        public string SiteUrl { set; get; }

        [DataMember]
        public string CrawlIndexProfileName { set; get; }

        [DataMember]
        public string CrawlIndexProfileId { set; get; }

        [DataMember]
        public UpdateIndexSettingResult Result { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BatchCrawlIndexJobDto
    {
        [DataMember]
        public string FullTextIndexJobId { get; set; }

        [DataMember]
        public UpdateIndexSettingResult Result { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RunType
    {
        [EnumMember]
        Upgrade5X = 0,
        [EnumMember]
        DocAve60 = 1,
        [EnumMember]
        DocAve61NoIndex = 2,
        [EnumMember]
        DocAve61IndexFailed = 3,
        [EnumMember]
        DocAve61IndexSuccess = 4
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum UpdateIndexSettingResult
    {
        [EnumMember]
        None,
        [EnumMember]
        NoExistWebUrl,
        [EnumMember]
        NoExistSiteUrl,
        [EnumMember]
        ExistSameSiteUrl,
        [EnumMember]
        NoExistIndexProfile,
        [EnumMember]
        ExistRunningJob,
        [EnumMember]
        ExistDeletingJob,
        [EnumMember]
        NoExistFullTextIndexJobId,
        [EnumMember]
        Other,
    }
}