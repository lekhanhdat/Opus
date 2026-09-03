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
using AvePoint.RA.Common;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using OpenNLP.Tools.Util;
using RADownloadCentre.SettingExport.Contract;
using RADownloadCentre.SettingExport.Model;
using Level = AvePoint.RA.SharePoint.Common.Setting.Model.SettingLevel;

namespace RADownloadCentre.SettingExport.Helper
{
    public class TeamsSOSettingCsv(string filePath, BaseJobDto baseJobDto) : SettingCsv<ExportTeamsSOSettingData>(filePath, baseJobDto)
    {
        private readonly ILicenseHelperService LicenseHelperService = PlatformWindsorManager.GetService<ILicenseHelperService>();
        public const string TRUE = "TRUE";
        public const string FALSE = "FALSE";
        public const string FormatRule = "{0}, ({1}) {2}";
        private readonly List<Level> showDeleteAfterRestoreLevel = new List<Level> { Level.SiteCollection, Level.Container, Level.TeamsOrGroup };

        protected override List<string> AssembleSettingHeaderTittle()
        {
            var headers = new List<string>();
            AddHeaderPath(headers);
            AddHeaderGeneral(headers);
            AddHeaderArchivingSetting(headers);
            AddHeaderOther(headers);
            return headers;
        }

        private void AddHeaderOther(List<string> headers)
        {
            headers.AddRange([ExportSettingHeaderColumnContract.IsInherit]);
        }

        private void AddHeaderGeneral(List<string> headers)
        {
            headers.AddRange([ExportSettingHeaderColumnContract.EnableArchivingManagementCol, ExportSettingHeaderColumnContract.DeleteArchivedDataCol]);
        }

        private void AddHeaderArchivingSetting(List<string> headers)
        {
            headers.AddRange([ExportSettingHeaderColumnContract.RulesCol, ExportSettingHeaderColumnContract.IncludeTermStoreCol, ExportSettingHeaderColumnContract.DecryptIRMProtectedFilesCol,
              ExportSettingHeaderColumnContract.RemoveRetentionLabelBeforeArchivedCol,
              ExportSettingHeaderColumnContract.SupportLockedSiteCol,
            ]);
        }

        private void AddHeaderPath(List<string> headers)
        {
            headers.AddRange([ExportSettingHeaderColumnContract.ContainerCol,ExportSettingHeaderColumnContract.TeamsOrGroupCol , ExportSettingHeaderColumnContract.SiteCollectionCol, ExportSettingHeaderColumnContract.SiteCol,
                ExportSettingHeaderColumnContract.ListOrLibraryCol, ExportSettingHeaderColumnContract.FolderCol]);
        }
        protected override List<string> ConvertSettingToList(ExportTeamsSOSettingData setting)
        {

            var exportSetting = new List<string>();
            ConvertPath(exportSetting, setting);
            if (setting.IsEmptySetting)
            {
                if (setting.IsInheritSetting)
                {
                    exportSetting.AddRange(["", "", "", "", "", "", TRUE]);
                }
                return exportSetting;
            }
            ConvertGeneralSetting(exportSetting, setting);
            ConvertArchivingSetting(exportSetting, setting);
            ConvertOther(exportSetting, setting);
            return exportSetting;
        }

        private void ConvertOther(List<string> exportSetting, ExportTeamsSOSettingData setting)
        {
            string isInheritSetting = setting.IsInheritSetting ? TRUE : FALSE;
            exportSetting.AddRange([ProcessCol(isInheritSetting)]);
        }

        private void ConvertArchivingSetting(List<string> exportSetting, ExportTeamsSOSettingData setting)
        {
            string rule = ConvertRuleSetting(setting.Rules);
            string includeTermStore = setting.IncludeTermStore ? TRUE : FALSE;
            string decryptIRM = setting.DecryptIRMProtectedFile ? TRUE : FALSE;
            string removeRetentionLabel = setting.RemoveRetentionLabelBeforeArchived ? TRUE : FALSE;
            string supportLockedSite = setting.SupportLockedSite ? TRUE : FALSE;
            exportSetting.AddRange([ProcessCol(rule), ProcessCol(includeTermStore), ProcessCol(decryptIRM), ProcessCol(removeRetentionLabel), ProcessCol(supportLockedSite)]);
        }

        private string ConvertRuleSetting(List<ExportRuleInfo> rules)
        {
            List<string> exportRule = new();
            foreach (var rule in rules)
            {
                exportRule.Add(string.Format(FormatRule, rule.Index, rule.Level, rule.Name));
            }
            return string.Join(',', exportRule);
        }

        private void ConvertGeneralSetting(List<string> exportSetting, ExportTeamsSOSettingData setting)
        {
            string enableArchiver = setting.IsEnableArchiver ? TRUE : FALSE;
            string deleteArchiverData = LicenseHelperService.IsEnableDeleteRestoreDataFeature() && showDeleteAfterRestoreLevel.Contains(setting.NodeLevel) ? (setting.DeleteArchiverDataAfterRestored ? TRUE : FALSE) : string.Empty;
            exportSetting.AddRange([ProcessCol(enableArchiver), ProcessCol(deleteArchiverData)]);
        }

        private void ConvertPath(List<string> exportSetting, ExportTeamsSOSettingData setting)
        {
            exportSetting.AddRange([ProcessCol(setting.ContainerName), ProcessCol(setting.TeamsOrGroupName), ProcessCol(setting.SiteCollectionUrl), ProcessCol(setting.SiteUrl), ProcessCol(setting.ListUrl), ProcessCol(setting.FolderUrl)]);
        }

        private string ProcessCol(string column)
        {
            return column;
        }
    }
}
