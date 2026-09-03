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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.Extension;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using RADownloadCentre.SettingExport.Contract;
using RADownloadCentre.SettingExport.Model;
using SettingModel = AvePoint.RA.SharePoint.Common.Setting.Model;
using Level = AvePoint.RA.SharePoint.Common.Setting.Model.SettingLevel;

namespace RADownloadCentre.SettingExport.Helper
{
    public class SettingHelper
    {
        private RALogger _logger = RALogger.GetInstance(typeof(SettingHelper));

        private readonly ITermDao _termDAO = PlatformWindsorManager.GetService<ITermDao>();
        public ExportTeamsSettingData ConvertExportTeamsSetting(RMTeamsSetting setting, string containerName, string teamsOrGroupName, bool isInherit = false)
        {
            return new ExportTeamsSettingData
            {
                Id = setting.Id,
                NodeInfo = setting.NodeInfo,
                TermId = setting.TermId,
                TermSetId = setting.TermSetId,
                DefaultTermId = setting.DefaultTermId,
                NeedCheckDefaultValue = setting.NeedCheckDefaultValue,
                IncludeDeclaredRecords = setting.IncludeDeclaredRecords,
                ApplyTermIncludeFolder = setting.ApplyTermIncludeFolder,
                ApplyExistType = setting.ApplyExistType,
                ApprovalType = setting.ApprovalType,
                EMailToRecordOwner = setting.EMailToRecordOwner,
                WorkflowReferenceId = setting.WorkflowReferenceId,
                DeployTermMethod = setting.DeployTermMethod,
                ContainerName = containerName,
                TeamsOrGroupName = teamsOrGroupName,
                FullPath = setting.FullPath,
                IsInheritSetting = false,
                TeamsId = setting.TeamsId.ToString(),
            };
        }

        public ExportSPSOSettingData ConvertExportSPSOSetting(RMArchiverSetting setting, string containerName, string siteCollectionUrl, string siteUrl, string listUrl, string folderUrl, bool isInherit = false, Level nodeLevel = Level.Container)
        {
            if (setting == null) return new ExportSPSOSettingData();
            var exportSetting = new ExportSPSOSettingData
            {
                Id = setting.Id,
                ContainerName = containerName,
                SiteCollectionUrl = siteCollectionUrl,
                SiteUrl = siteUrl,
                ListUrl = listUrl,
                FolderUrl = folderUrl,
                IsEnableArchiver = setting.EnableArchiverManagement == (int)EnableRecordManagementSetting.Enable,
                IncludeTermStore = setting.isIncludeManagedMetadataService,
                SupportLockedSite = setting.SupportLockedSite,
                DecryptIRMProtectedFile = setting.isEnableSuperUserDecrypt,
                RemoveRetentionLabelBeforeArchived = setting.isEnableRemoveRetentionLabel,
                IsInheritSetting = isInherit,
                NodeLevel = nodeLevel
            };

            if (setting.CleanRestoredOption == null)
            {
                exportSetting.DeleteArchiverDataAfterRestored = false;
            }
            else
            {
                var cleanRestoredOption = SerializerHelper.DeserializeByDataContractSerializer<CleanRestoredItemsExtension>(setting.CleanRestoredOption);
                exportSetting.DeleteArchiverDataAfterRestored = cleanRestoredOption.EnableDelArchivedData;
            }

            return exportSetting;
        }

        public ExportTeamsSOSettingData ConvertExportTeamsSOSetting(RMArchiverSetting setting, string containerName, string teamsOrGroupName, string siteCollectionUrl, string siteUrl, string listUrl, string folderUrl, bool isInherit = false, Level nodeLevel = Level.Container)
        {
            if(setting == null) return new ExportTeamsSOSettingData();
            var exportSetting = new ExportTeamsSOSettingData
            {
                Id = setting.Id,
                ContainerName = containerName,
                TeamsOrGroupName = teamsOrGroupName,
                SiteCollectionUrl = siteCollectionUrl,
                SiteUrl = siteUrl,
                ListUrl = listUrl,
                FolderUrl = folderUrl,
                IsEnableArchiver = setting.EnableArchiverManagement == (int)EnableRecordManagementSetting.Enable,
                IncludeTermStore = setting.isIncludeManagedMetadataService,
                SupportLockedSite = setting.SupportLockedSite,
                DecryptIRMProtectedFile = setting.isEnableSuperUserDecrypt,
                RemoveRetentionLabelBeforeArchived = setting.isEnableRemoveRetentionLabel,
                IsInheritSetting = isInherit,
                NodeLevel = nodeLevel
            };

            if (setting.CleanRestoredOption == null)
            {
                exportSetting.DeleteArchiverDataAfterRestored = false;
            }
            else
            {
                var cleanRestoredOption = SerializerHelper.DeserializeByDataContractSerializer<CleanRestoredItemsExtension>(setting.CleanRestoredOption);
                exportSetting.DeleteArchiverDataAfterRestored = cleanRestoredOption.EnableDelArchivedData;
            }

            return exportSetting;
        }

        public SettingModel.SettingLevel ConvertNodeLevelToSettingLevel(RMSPTreeNode node)
        {
            switch ((NodeLevel)node.Level)
            {
                case NodeLevel.Folder:
                case NodeLevel.DesignFolder:
                    return SettingModel.SettingLevel.Folder;
                case NodeLevel.List:
                    return SettingModel.SettingLevel.List;
                case NodeLevel.Site:
                    if (node.Name == ".")
                        return SettingModel.SettingLevel.RootWeb;
                    return SettingModel.SettingLevel.SubWeb;
                case NodeLevel.SiteCollection:
                    return SettingModel.SettingLevel.SiteCollection;
                default:
                    return SettingModel.SettingLevel.None;

            }
        }
        public (string SiteCollection, string Site, string List, string Folder) SplitFullPath(ExportTeamsSettingData setting)
        {
            (string SiteCollection, string Site, string List, string Folder) splitPath = ("", "", "", "");

            if (setting.IsInheritSetting || setting.IsEmptySetting)
            {
                return (setting.FullPath, "", "", "");
            }

            var node = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(setting.NodeInfo);
            do
            {
                var level = ConvertNodeLevelToSettingLevel(node);
                switch (level)
                {
                    case SettingModel.SettingLevel.Folder:
                        splitPath.Folder = string.IsNullOrEmpty(splitPath.Folder) ? node.FullPath.Substring(node.FullPath.LastIndexOf(@"/") + 1) : node.FullPath.Substring(node.FullPath.LastIndexOf(@"/") + 1) + "/" + splitPath.Folder;
                        break;
                    case SettingModel.SettingLevel.List:
                        splitPath.List = string.IsNullOrEmpty(splitPath.List) ? node.FullPath.Substring(node.FullPath.LastIndexOf(@"/") + 1) : node.FullPath.Substring(node.FullPath.LastIndexOf(@"/") + 1) + "/" + splitPath.List;
                        splitPath.List = node.FullPath.Contains("Lists") ? "Lists/" + splitPath.List : splitPath.List;
                        break;
                    case SettingModel.SettingLevel.SubWeb:
                        splitPath.Site = string.IsNullOrEmpty(splitPath.Site) ? node.FullPath.Substring(node.FullPath.LastIndexOf(@"/") + 1) : node.FullPath.Substring(node.FullPath.LastIndexOf(@"/") + 1) + "/" + splitPath.Site;
                        break;
                    case SettingModel.SettingLevel.RootWeb:
                        splitPath.Site = string.IsNullOrEmpty(splitPath.Site) ? "." : splitPath.Site;
                        break;
                    case SettingModel.SettingLevel.SiteCollection:
                        splitPath.SiteCollection = node.FullPath;
                        return splitPath;
                    default:
                        break;
                }
                node = node.Parent;
            } while (node != null);
            return splitPath;
        }
        public bool CheckTermAndColumnSettings(RMTeamsSetting nodeSetting, RMSPSampleTreeNode node)
        {
            if (nodeSetting == null)
            {
                _logger.Info($"The container {node.Name} setting is null");
                return false;
            }
            if (nodeSetting.IsUsingExistColumnName && !nodeSetting.SetDocLevelTermForExistColumn)
            {
                _logger.Info($"The container {node.Name} setting use the exist column");
                return false;
            }
            if (string.IsNullOrEmpty(nodeSetting.ColumnName) && !nodeSetting.IsUsingExistColumnName)
            {
                _logger.Info($"The container {node.Name} setting does not have column setting");
                return false;
            }
            if (nodeSetting.TermSetId == Guid.Empty)
            {
                _logger.Info($"The container {node.Name} setting does not have document level setting");
                return false;
            }
            return true;
        }
        public string CheckSetting(RMTeamsSetting setting, AvePoint.RA.Contract.FunctionSetting.ExportSettingType exportSettingType)
        {
            if (setting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
            {
                return "RM_JS_BCM_ExportSetting_DisableState";
            }
            if (setting.DeployTermMethod == (int)DeployTermMethod.UseAutoClassification)
            {
                return "RM_JS_BCM_ExportSetting_AutoClassificationSupport";
            }
            if (setting.DeployTermMethod == (int)DeployTermMethod.UseIntelligenceClassification)
            {
                return "RM_JS_BCM_ExportSetting_SmartClassificationSupport";
            }
            if (setting.ApprovalType == ApprovalType.ApprovalProcess && !Guid.TryParse(setting.WorkflowReferenceId, out Guid workflowReferenceId))
            {
                return "RM_JS_BCM_ExportSetting_WorkFlowProcessEmpty";
            }
            return string.Empty;
        }

        public ExportTeamsSettingData ConvertTeamsEmptySetting(string fullPath,string teamsOrGroupName, string containerName, bool isInheritSetting = false, string teamsId = "")
        {
            return new ExportTeamsSettingData
            {
                FullPath = fullPath,
                IsEmptySetting = true,
                ContainerName = containerName,
                TeamsOrGroupName = teamsOrGroupName,
                IsInheritSetting = isInheritSetting,
                TeamsId = teamsId
            };
        }

        public ExportTeamsSOSettingData ConvertTeamsSOEmptySetting(string siteCollection, string teamsOrGroupName, string containerName, bool isInheritSetting = false, Level nodeLevel = Level.Container)
        {
            return new ExportTeamsSOSettingData
            {
                SiteCollectionUrl = siteCollection,
                IsEmptySetting = true,
                ContainerName = containerName,
                TeamsOrGroupName = teamsOrGroupName,
                IsInheritSetting = isInheritSetting,
                NodeLevel = nodeLevel
            };
        }

        public ExportSPSOSettingData ConvertSharePointSOEmptySetting(string siteCollection, string containerName, bool isInheritSetting = false, Level nodeLevel = Level.Container)
        {
            return new ExportSPSOSettingData
            {
                SiteCollectionUrl = siteCollection,
                IsEmptySetting = true,
                ContainerName = containerName,
                IsInheritSetting = isInheritSetting,
                NodeLevel = nodeLevel
            };
        }

        public List<ExportRuleInfo> ConvertArchiverRuleMapping(List<RMSimpleRule> archiverRule)
        {
            if (!archiverRule.Any()) return new List<ExportRuleInfo>();
            return archiverRule.ConvertAll(_ => new ExportRuleInfo
            {
                Index = _.RuleOrder,
                Name = _.RuleName,
                Level = ConvertRuleLevel(_.IntRuleLevel)
            });
        }

        private string ConvertRuleLevel(int intRuleLevel)
        {
            switch (intRuleLevel)
            {
                case 2:
                    return RuleLevelContract.SiteCollection;
                case 4:
                    return RuleLevelContract.Site;
                case 8:
                    return RuleLevelContract.List;
                case 16:
                    return RuleLevelContract.Folder;
                case 32:
                    return RuleLevelContract.Item;
                case 128:
                    return RuleLevelContract.Attachment;
                case 256:
                    return RuleLevelContract.DocumentVersion;
                case 512:
                    return RuleLevelContract.ItemVersion;
                case 33554432:
                    return RuleLevelContract.Teams;
                case 64:
                default:
                    return RuleLevelContract.DocumentEmail;
            }
        }
    }
}
