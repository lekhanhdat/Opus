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
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using DocAveOnline.WebApi.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.GCommon.Contract.Server.EndUserRestoreSetting
{
    [DataContract]
    public class EndUserRestoreSettingUIDto
    {
        [DataMember]
        public bool IsRestoreArchivedTier { get; set; }
        [DataMember]
        public bool IsCustomizeStubRestorePage { get; set; }
        [DataMember]
        public bool IsIncludeSharedLinks { get; set; }
        [DataMember]
        public string Logo { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public string Footer { get; set; }
        [DataMember]
        public bool IsAllowRestore { get; set; }
        [DataMember]
        public EndUserPermissionSetting PermissionSetting { get; set; }

    }

    public class EndUserPermissionSetting
    {
        [DataMember]
        public GroupOrTeamSitePermissionSetting TeamsAndGroup { get; set; }
        [DataMember]
        public SharePointSitePermissionSetting SiteCollection { get; set; }
        [DataMember]
        public string SiteCollectionSpecialGroupNames { get; set; }
        [DataMember]
        public bool IsRestoreGroupTeamSite { get; set; }
        [DataMember]
        public bool IsExportGroupTeamSite { get; set; }
        [DataMember]
        public bool IsRestoreSiteCollection { get; set; }
        [DataMember]
        public bool IsExportSiteCollection { get; set; }
        [DataMember]
        public bool IsRestoreStubLink { get; set; }
        [DataMember]
        public bool IsExportStubLink { get; set; }
        [DataMember]
        public bool? IsSearchGroupTeamSite { get; set; }
        [DataMember]
        public bool? IsSearchSiteCollection { get; set; }
        [DataMember]
        public StubOopRestoreSetting StubOopRestoreSetting { get; set; }
    }

    [DataContract]
    public class StubOopRestoreSetting
    {
        [DataMember]
        public bool IsEnableStubOopRestore { get; set; }
        [DataMember]
        public bool IsEnableSearchStubLocation { get; set; }
        [DataMember]
        public bool IsEnableManualInputDesStubLocation { get; set; }
    }

    public enum SharePointSitePermissionSetting
    {
        [DataMember]
        SiteOwner,
        [DataMember]
        SiteOwnerAndSiteMemberGroup,
        [DataMember]
        SiteOwnerAndSpecialGroup,
        [DataMember]
        SiteOwnerAndSiteMemberGroupAndSiteVisitor
    }

    public enum GroupOrTeamSitePermissionSetting
    {
        [EnumMember]
        Owner,
        [EnumMember]
        OwnerOrMembler
    }
}
