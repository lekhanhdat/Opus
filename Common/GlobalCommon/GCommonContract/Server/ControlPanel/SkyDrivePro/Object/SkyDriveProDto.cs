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


using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.SharePointBrowser;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.SkyDrivePro.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SkyDriveProTestResult
    {
        [DataMember]
        public SiteCollectionState State { get; set; }
        [DataMember]
        public BPOSMould BPOSMould { get; set; }
        [DataMember]
        public String Url { get; set; }
        [DataMember]
        public String UserName { get; set; }
        [DataMember]
        public String SPVersion { get; set; }
        [DataMember]
        public String TemplateName { get; set; }
        [DataMember]
        public String TemplateTitle { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SkyDriveProImportResult
    {
        [DataMember]
        public SkyDriveProImportState ResultState { get; set; }
        [DataMember]
        public int UpAgentCount { get; set; }
        [DataMember]
        public int RegisteredCount { get; set; }
        [DataMember]
        public List<SkyDriveProTestResult> Results { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReconnectSkyDrivesResult
    {
        [DataMember]
        public SkyDriveProReconnectState ResultState { get; set; }
        [DataMember]
        public List<RemoteSiteCollection> SiteList { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ScanMySitesResult
    {
        [DataMember]
        public ScanSitesResultState ResultState { get; set; }
        [DataMember]
        public List<SkyDriveProTestResult> SiteResults { get; set; }
        [DataMember]
        public string FileName { get; set; }
        [DataMember]
        public long TimeOut { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SkyDriveProImportState
    {
        [EnumMember]
        NoSkyDrivePro,
        //[EnumMember]
        //NoAvailableAgent,
        [EnumMember]
        SkyDriveProAccessSome,
        [EnumMember]
        ReadFileError,
        [EnumMember]
        AllSkyDriveProsExist,
        [EnumMember]
        NotGlobalAdmin,
        [EnumMember]
        AdminCenterUrlInvalid
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SkyDriveProReconnectState
    {
        [EnumMember]
        AccessAll,
        [EnumMember]
        AccessSome,
        [EnumMember]
        NotGlobalAdmin,
        [EnumMember]
        AdminCenterUrlInvalid
    }
}
