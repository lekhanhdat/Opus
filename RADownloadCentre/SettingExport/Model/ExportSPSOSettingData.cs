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
using NodeLevel = AvePoint.RA.SharePoint.Common.Setting.Model.SettingLevel;

namespace RADownloadCentre.SettingExport.Model
{
    public class ExportSPSOSettingData
    {
        public Guid Id { get; internal set; }
        public string ContainerName { get; set; }
        public string SiteCollectionUrl { get; set; }
        public string SiteUrl { get; set; }
        public string ListUrl { get; set; }
        public string FolderUrl { get; set; }
        public bool IsEnableArchiver { get; set; }
        public bool DeleteArchiverDataAfterRestored { get; set; }
        public List<ExportRuleInfo> Rules { get; set; }
        public bool IncludeTermStore { get; set; }
        public bool DecryptIRMProtectedFile { get; set; }
        public bool RemoveRetentionLabelBeforeArchived { get; set; }
        public bool IsEmptySetting { get; set; }
        public bool IsInheritSetting { get; set; }
        public bool SupportLockedSite { get; set; }
        public NodeLevel NodeLevel { get; set; }
    }
    public class ExportRuleInfo
    {
        public string Name { get; set; }
        public string Level { get; set; }
        public int Index { get; set; }
    }
}
