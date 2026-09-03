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

namespace RADownloadCentre.SettingExport.Base;

public abstract class BaseDefaultExportColumns(BaseExportCsv exportCsv) : BaseExportCsv
{
    protected const string ManuallyChooseATerm = "Manually choose a term";
    protected const string SetADefaultTerm = "Set a default term";
    protected const string AutoPopulate = "Auto populate a term based on criteria (Doesn't support import)";
    protected const string SmartClassification = "Smart classification (Doesn't support import)";
    protected const string NoManualSetting = "No manual setting";
    protected const string WorkflowProcess = "Workflow process";
    protected const string RecordOwner = "Record owner";
    protected const string AutoApprove = "Skip manual review for this location";
    protected static readonly string ApplyTermByColumn = I18NEntity.GetString("RM_JS_BCM_Export_ApplyTermByColumn");
    protected static readonly string TermScopeColumn = I18NEntity.GetString("RM_JS_BCM_Export_TermScopeColumn");
    protected static readonly string DefaultTermColumn = I18NEntity.GetString("RM_JS_BCM_Export_DefaultTermColumn");
    protected static readonly string ApplyToExistingDocumentsColumn = I18NEntity.GetString("RM_JS_BCM_Export_ApplyToExistingDocumentsColumn");
    protected static readonly string ApplyToExistingDeclaredRecordsColumn = I18NEntity.GetString("RM_JS_BCM_Export_ApplyToExistingDeclaredRecordsColumn");
    protected static readonly string ApplyToDocumentSetsAndFoldersColumn = I18NEntity.GetString("RM_JS_BCM_Export_ApplyToDocumentSetsAndFoldersColumn");
    protected static readonly string SendEmailForPersonColumn = I18NEntity.GetString("RM_JS_BCM_Export_SendEmailForPersonColumn");
    protected static readonly string SendEmailNotificationColumn = I18NEntity.GetString("RM_JS_BCM_Export_SendEmailNotificationColumn");
    protected static readonly string OverwriteTheExistingTermColumn = I18NEntity.GetString("RM_JS_BCM_Export_OverwriteTheExistingTermColumn");
    protected static readonly string ManualApprovalTypeColumn = $"\"{I18NEntity.GetString("RM_JS_BCM_Export_ManualApprovalTypeColumn")}\"";
    protected static readonly string IsInheritSetting = I18NEntity.GetString("RM_JS_BCM_Export_IsInheritSettingColumn");

    protected BaseExportCsv BaseExportCsv = exportCsv;
    public abstract override List<string> GetExportColumns();
}