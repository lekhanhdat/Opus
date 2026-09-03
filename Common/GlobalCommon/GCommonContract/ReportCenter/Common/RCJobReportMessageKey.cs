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


namespace AvePoint.GCommon.Contract.ReportCenter.Common
{
    public class RCJobReportMessageKey
    {
      
        /// <summary>
        /// Cannot find available agent.
        /// args = null
        /// </summary>
        public const string NoAvailableAgent = "NoAvailableAgent";

        /// <summary>
        /// Unsupported tree level {0} for export to each site.
        /// args[0] = nodel level
        /// </summary>
        public const string UnsupportedTreeLevelForExportToEachSite = "UnsupportedTreeLevelForExportToEachSite";

        /// <summary>
        /// AuditorDB is over quota,please prune some audit data and try again.
        /// args = null
        /// </summary>
        public const string AuditorDBIsOverQuota = "AuditorDBIsOverQuota";

        /// <summary>
        /// Insufficient free space. Please delete some jobs in job monitor to get more available size
        /// args = null
        /// </summary>
        public const string AuditorCacheDBIsOverQuota = "AuditorCacheDBIsOverQuota";

        /// <summary>
        /// Change audit from {0} to {1}
        /// args[0] = orginal action
        /// args[1] = target action
        /// </summary>
        public const string ChangeAuditSetting = "ChangeAuditSetting";

        /// <summary>
        /// Change audit from {0} to None
        /// args[0] = orginal action
        /// </summary>
        public const string ChangeAuditSettingToNone = "ChangeAuditSettingToNone";

        /// <summary>
        /// Change audit from None to {0}
        /// args[0] = orginal action
        /// </summary>
        public const string ChangeAuditSettingFromNone = "ChangeAuditSettingFromNone";

        /// <summary>
        /// Change audit from None to None
        /// </summary>
        public const string ChangeAuditSettingFromNoneToNone = "ChangeAuditSettingFromNoneToNone";

        /// <summary>
        /// SharePoint Audit does not support to audit the actions for the site collections that are using the template same with this site collection.
        /// </summary>
        public const string AuditNotSupportedSiteCollection = "AuditNotSupportedSiteCollection";

        /// <summary>
        /// This Site Collection has been filter.
        /// </summary>
        public const string AuditFilterSiteCollection = "AuditFilterSiteCollection";

        /// <summary>
        /// An error occured ，exception message：{0}
        /// args[0] = exception message
        /// </summary>
        public const string ExceptionOccurred = "ExceptionOccurred";

        /// <summary>
        /// The publish report location is only support document library.
        /// </summary>
        public const string OnlySupportDocumentlibrary = "OnlySupportDocumentlibrary";

        /// <summary>
        /// The export location is unavailable.This library is not exists.
        /// </summary>
        public const string UnavailableExportLibrary = "UnavailableExportLibrary";

        /// <summary>
        /// There is another job with the same selected node is running.
        /// </summary>
        public const string SkipJobForSameNode = "SkipJobForSameNode";

        public const string UnavailableCredential = "UnavailableCredential";



        /// <summary>
        /// An error occured then apply {0} rule.
        /// </summary>
        public const string ApplyRuleException = "ApplyRuleException";

        #region configuration report
        /// <summary>
        /// process node {0} error.
        /// </summary>
        public const string ProcessNodeError = "ProcessNodeError";

        public const string AR_SC_GeneralSettings_Exception = "AR_SC_GeneralSettings_Exception";
        public const string AR_SC_SearchSettings_Exception = "AR_SC_SearchSettings_Exception";
        public const string AR_SC_SharePointStorageReport_Exception = "AR_SC_SharePointStorageReport_Exception";
        public const string AR_SC_SiteCollectionFeatures_Exception = "AR_SC_SiteCollectionFeatures_Exception";
        public const string AR_SC_ContentAnalysis_Exception = "AR_SC_ContentAnalysis_Exception";
        public const string AR_SC_NotSupportTemplate = "AR_SC_NotSupportTemplate";

        public const string AR_Web_GeneralSettings_Exception = "AR_Web_GeneralSettings_Exception";
        public const string AR_Web_SecuritySettings_Exception = "AR_Web_SecuritySettings_Exception";
        public const string AR_Web_Search_Exception = "AR_Web_Search_Exception";
        public const string AR_Web_RegionalSettings_Exception = "AR_Web_RegionalSettings_Exception";
        public const string AR_Web_Properties_Exception = "AR_Web_Properties_Exception";
        public const string AR_Web_RSSSettings_Exception = "AR_Web_RSSSettings_Exception";
        public const string AR_Web_ListAndDocumentLibraryInformation_Exception = "AR_Web_ListAndDocumentLibraryInformation_Exception";
        public const string AR_Web_SubSitesAndPageInformation_Exception = "AR_Web_SubSitesAndPageInformation_Exception";
        public const string AR_Web_SharePointStorageReport_Exception = "AR_Web_SharePointStorageReport_Exception";
        public const string AR_Web_ContentAnalysis_Exception = "AR_Web_ContentAnalysis_Exception";
        public const string AR_Web_SiteFeatures_Exception = "AR_Web_SiteFeatures_Exception";
        public const string AR_Web_NotSupportTemplate = "AR_Web_NotSupportTemplate";

        public const string AR_List_GeneralSettings_Exception = "AR_List_GeneralSettings_Exception";
        public const string AR_List_SecuritySettings_Exception = "AR_List_SecuritySettings_Exception";
        public const string AR_List_SharePointStorageReport_Exception = "AR_List_SharePointStorageReport_Exception";
        #endregion

        /// <summary>
        /// Export to Report Location failed info
        /// </summary>
        public const string FUFileNotFoundException = "FU_FileNotFoundException";
        public const string FUNotSupportLibraryTemplateException = "FU_NotSupportLibraryTemplateException";
        public const string FUUnavailableLibraryException = "FU_UnavailableLibraryException";
        public const string FUExceptionOccurred = "FU_ExceptionOccurred";

        public const string JobSummary_AllSitesHaveBeenDeleted = "JobSummary_AllSitesHaveBeenDeleted";

        public const string JobSummary_NotFindConnStr = "JobSummary_NotFindConnStr";

        public const string JObSummary_NotSupportExportEachSite = "JObSummary_NotSupportExportEachSite";

        /// <summary>
        /// unknown error occured, please refer to log.
        /// </summary>
        public const string JobSummary_GetConnStrException = "JobSummary_GetConnStrException";

        /// <summary>
        /// There is a same job already running.
        /// </summary>
        public const string SkipJobForSameJob = "SkipJobForSameJob";

        /// <summary>
        /// Finish but No Item In List.
        /// </summary>
        //public const string FinishWithNoItem = "FinishWithNoItem";

        /// <summary>
        /// ApplyRule No object meet the conditions
        /// </summary>
        public const string SummaryComment_ApplyRule_SkipWithNoSCFilted = "SummaryComment_ApplyRule_SkipWithNoSCFilted";

        public const string AppTokenTreeNode_NotSupportRetrieve = "AppTokenTreeNode_NotSupportRetrieve";

        public const string SelectEmptyNodes_RunJob = "SelectEmptyNodes_RunJob";
    }
}
