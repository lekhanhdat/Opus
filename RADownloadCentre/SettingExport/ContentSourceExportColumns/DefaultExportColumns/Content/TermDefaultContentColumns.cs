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
using AvePoint.RA.Contract.Object;
using RADownloadCentre.SettingExport.Base;
using RADownloadCentre.SettingExport.Model;

namespace RADownloadCentre.SettingExport.ContentSourceExportColumns.DefaultExportColumns.Content;

public class TermDefaultContentColumns(BaseExportCsv exportCsv, ExportTeamsSettingData setting) : BaseDefaultExportColumns(exportCsv)
{
    public override List<string> GetExportColumns()
    {
        var result = AddTermInformation(BaseExportCsv.GetExportColumns());
        return result;
    }
    
    private List<string> AddTermInformation(List<string> result)
    {
        var applyTermByColumn = setting.DeployTermMethod switch
        {
            (int)DeployTermMethod.NoDefaultTerm => ProcessCol(ManuallyChooseATerm),
            (int)DeployTermMethod.UseDefaultTerm => ProcessCol(SetADefaultTerm),
            (int)DeployTermMethod.UseAutoClassification => ProcessCol(AutoPopulate),
            (int)DeployTermMethod.UseIntelligenceClassification => ProcessCol(SmartClassification)
        };
        if(setting.DeployTermMethod == (int)DeployTermMethod.UseAutoClassification || setting.DeployTermMethod == (int)DeployTermMethod.UseIntelligenceClassification)
        {
            result.Add(applyTermByColumn);
            return result;
        }
        var termScopeColumn =
            ProcessCol(setting.TermScopeNamePath.Replace('/', PathSeparator).Replace(@"""", "\"\""));
        var defaultTermColumn =
            ProcessCol(setting.TermDefaultNamePath.TrimStart(PathSeparator).Replace(@"""", "\"\""));
        var applyToExistingDocumentsColum = ProcessCol(setting.NeedCheckDefaultValue ? "TRUE" : "");
        var applyToExistingDeclaredRecordsColumn = ProcessCol(setting.IncludeDeclaredRecords ? "TRUE" : "");
        var applyToDocumentSetsAndFoldersColumn = ProcessCol(setting.ApplyTermIncludeFolder switch
        {
            null => "",
            true => "TRUE",
            _ => ""
        });
        var overwriteTheExistingTermColumn =
            ProcessCol(setting.ApplyExistType == (int)ApplyExistingTermType.OverWrite &&
                       (setting.NeedCheckDefaultValue || setting.ApplyTermIncludeFolder == true)
                ? "TRUE"
                : "");
        result.AddRange(
        [
            applyTermByColumn,
            termScopeColumn,
            defaultTermColumn,
            applyToExistingDocumentsColum,
            applyToExistingDeclaredRecordsColumn,
            applyToDocumentSetsAndFoldersColumn,
            overwriteTheExistingTermColumn
        ]);
        return result;
    }
}