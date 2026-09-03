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
using AvePoint.RA.I18N.Core;

namespace RADownloadCentre.SettingExport.Contract
{
    internal class ExportSettingHeaderColumnContract
    {
        public static readonly string ContainerCol = I18NEntity.GetString("RM_JS_BCM_Export_ContainerColumn");
        public static readonly string TeamsOrGroupCol = I18NEntity.GetString("RM_JS_BCM_Export_TeamsOrGroupColumn");
        public static readonly string SiteCollectionCol = I18NEntity.GetString("RM_JS_BCM_Export_SiteCollectionColumn");
        public static readonly string SiteCol = I18NEntity.GetString("RM_JS_BCM_Export_SiteColumn");
        public static readonly string ListOrLibraryCol = I18NEntity.GetString("RM_JS_BCM_Export_LibraryColumn");
        public static readonly string FolderCol = I18NEntity.GetString("RM_JS_BCM_Export_FolderColumn");
        public static readonly string EnableArchivingManagementCol = I18NEntity.GetString("RM_JS_BCM_Export_EnableArchivingManagementColumn");
        public static readonly string DeleteArchivedDataCol = I18NEntity.GetString("RM_JS_BCM_Export_DeleteArchivedDataColumn");
        public static readonly string RulesCol = I18NEntity.GetString("RM_JS_BCM_Export_RulesColumn");
        public static readonly string IncludeTermStoreCol = I18NEntity.GetString("RM_JS_BCM_Export_IncludeTermStoreColumn");
        public static readonly string DecryptIRMProtectedFilesCol = I18NEntity.GetString("RM_JS_BCM_Export_DecryptIRMProtectedFilesColumn");
        public static readonly string RemoveRetentionLabelBeforeArchivedCol = I18NEntity.GetString("RM_JS_BCM_Export_RemoveRetentionLabelBeforeArchivedColumn");
        public static readonly string SupportLockedSiteCol = I18NEntity.GetString("RM_JS_BCM_Export_SupportLockedSiteColumn");
        public static readonly string IsInherit = I18NEntity.GetString("RM_JS_BCM_Export_IsInheritSettingColumn");
    }
}
