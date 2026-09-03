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

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Handler.Object
{
    [KnownType(typeof(EDSearchRequest))]
    [KnownType(typeof(CrawlSettingsRequest))]
    [KnownType(typeof(SearchLocationDto))]
    [KnownType(typeof(EDSyncHoldItemDto))]
    [KnownType(typeof(EDSyncFileSettingsDto))]
    [KnownType(typeof(EDSyncSettingsRequest))]
    [KnownType(typeof(EDHoldManagerRequest))]
    [KnownType(typeof(CplDBSettingsRequest))]
    [KnownType(typeof(HoldItemRequest))]
    [KnownType(typeof(EDRealTimeRequest))]
    [KnownType(typeof(DeleteInexistentWebAppRequest))]
    [KnownType(typeof(ContentSourceRequest))]
    [KnownType(typeof(CrawlStatusRequest))]
    [KnownType(typeof(DataExportRequest))]
    [KnownType(typeof(EDExportLocationRequest))]
    [KnownType(typeof(EDConfigRequest))]
    [KnownType(typeof(EDFullTextIndexSearchRequest))]
    [KnownType(typeof(EDPlanRequest))]
    [KnownType(typeof(ArchiveDataRequest))]
    [KnownType(typeof(EDOffLineSearchResultPagingRequest))]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EDiscoveryRequest
    {
        public EDiscoveryRequest()
        {
            CheckPermissions = new List<string>();
        }

        [DataMember]
        public EDService EDService { get; set; }

        [DataMember]
        public List<string> CheckPermissions { get; set; }
    }

    /// <summary>
    /// 请求的服务类型.  eDiscovery Service Registered Id
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EDService : uint
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Search = 1,
        [EnumMember]
        CrawlSettings = 2,
        [EnumMember]
        HoldManager = 3,
        [EnumMember]
        EDSyncSettings = 4,
        [EnumMember]
        CplDBSettings = 5,
        [EnumMember]
        HoldItem = 6,
        [EnumMember]
        CrawlStatus = 7,
        [EnumMember]
        DeleteInexistentWebApp = 8,
        [EnumMember]
        PlanManager = 9,
        [EnumMember]
        DataExportService = 10,
        [EnumMember]
        ExportLocatoinService = 11,
        [EnumMember]
        EDConfig = 12,
        [EnumMember]
        ArchiveDataService = 13
    }
}
