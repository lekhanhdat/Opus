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
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using RADownloadCentre.SettingExport.Base;
using RADownloadCentre.SettingExport.ContentSourceExportColumns;
using RADownloadCentre.SettingExport.ContentSourceExportColumns.DefaultExportColumns.Content;
using RADownloadCentre.SettingExport.ContentSourceExportColumns.DefaultExportColumns.Header;
using RADownloadCentre.SettingExport.ContentSourceExportColumns.Teams;
using RADownloadCentre.SettingExport.Model;

namespace RADownloadCentre.SettingExport.Helper
{
    public class TeamsSettingCsv(string filePath, BaseJobDto baseJobDto) : SettingCsv<ExportTeamsSettingData>(filePath, baseJobDto)
    {
        protected override List<string> AssembleSettingHeaderTittle()
        {
            BaseExportCsv headerColumns = new TeamsHeaderExportColumns();
            headerColumns = new TermDefaultHeaderColumns(headerColumns);
            headerColumns = new ApprovalDefaultHeaderColumns(headerColumns);
            headerColumns = new AddingHeaderColumns(headerColumns);
            return headerColumns.GetExportColumns();
        }

        protected override List<string> ConvertSettingToList(ExportTeamsSettingData setting)
        {
            var settingHelper = new SettingHelper();
            var splitPath = settingHelper.SplitFullPath(setting);
            BaseExportCsv contentColumns = new TeamsContentExportColumns(splitPath, setting);
            if (setting.IsEmptySetting)
            {
                if (setting.IsInheritSetting)
                {
                    var exportColumn = contentColumns.GetExportColumns();
                    exportColumn.AddRange(["", "", "", "", "", "", "", "", "","", "TRUE"]);
                    return exportColumn;
                }
                return contentColumns.GetExportColumns();
            }
            contentColumns = new TermDefaultContentColumns(contentColumns, setting);
            if(setting.DeployTermMethod == (int)DeployTermMethod.UseAutoClassification || setting.DeployTermMethod == (int)DeployTermMethod.UseIntelligenceClassification)
            {
                if (setting.IsInheritSetting)
                {
                    var exportColumn = contentColumns.GetExportColumns();
                    exportColumn.AddRange(["", "", "", "", "", "", "", "", "", "TRUE"]);
                    return exportColumn;
                }
                return contentColumns.GetExportColumns();
            }
            contentColumns = new ApprovalDefaultContentColumns(contentColumns, setting);
            contentColumns = new AddingContentColumn(contentColumns, setting);
            return contentColumns.GetExportColumns();
        }
    }
}
