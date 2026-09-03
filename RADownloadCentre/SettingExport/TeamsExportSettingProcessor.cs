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
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.SharePoint.Common.Setting;
using RADownloadCentre.SettingExport.Base;
using RADownloadCentre.SettingExport.Helper;
using RADownloadCentre.SettingExport.Model;
using SettingModel = AvePoint.RA.SharePoint.Common.Setting.Model;

namespace RADownloadCentre.SettingExport
{
    public class TeamsExportSettingProcessor : ExportSettingProcessor<ExportTeamsSettingData>
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(TeamsExportSettingProcessor));

        private readonly ITeamsSettingTreeService _teamsSettingTreeService = PlatformWindsorManager.GetService<ITeamsSettingTreeService>();
        private readonly IBrowseTreeService _browseTreeService = PlatformWindsorManager.GetService<IBrowseTreeService>();
        private readonly ITeamsSettingDao _teamsSettingDao = PlatformWindsorManager.GetService<ITeamsSettingDao>();
        private readonly ITermDao _termDAO = PlatformWindsorManager.GetService<ITermDao>();
        private readonly IRMWorkflowDefinitionDao _workflowDefinitionDAO = PlatformWindsorManager.GetService<IRMWorkflowDefinitionDao>();
        private readonly IRecordOwnerDao _recordOwnerDao = PlatformWindsorManager.GetService<IRecordOwnerDao>();

        private readonly AveContextHelper _aveContextHelper;
        private readonly SettingHelper _settingHelper;

        private readonly List<RMSPSampleTreeNode> _containers = [];

        private readonly string _startFileName = "ES_Teams_";

        public TeamsExportSettingProcessor(RMExportSettingJobMessage jobMsg) : base(jobMsg.JobID, jobMsg.JobType)
        {
            FilePath = JobReportUtility.GetDownloadReportDetailTempleFolder(BaseJobDto, BaseJobDto.Id.Replace("EST", _startFileName), ".csv");
            _aveContextHelper = new();
            _settingHelper = new();
            exportSettingType = jobMsg.exportSettingType;
        }

        protected override string[][] AssembleSettingHeaderTittle(string[][] datas, string connectionName)
        {
            throw new NotImplementedException();
        }

        protected override Task<string[][]> ConvertSettingToArrayAsync(List<ExportTeamsSettingData> settings, string[][] datas)
        {
            throw new NotImplementedException();
        }

        protected override async Task GenerateDataAsync()
        {
            await using TeamsSettingCsv settingCsv = new(FilePath, BaseJobDto);

            await settingCsv.WriteHeaderAsync();

            foreach (var container in _containers)
            {

                var containerSetting = _teamsSettingDao.LoadTeamsSetting(Guid.Parse(container.SPObjectId), Guid.Empty, Guid.Empty, true);

                if (!_settingHelper.CheckTermAndColumnSettings(containerSetting, container))
                {
                    if (exportSettingType == AvePoint.RA.Contract.FunctionSetting.ExportSettingType.ExportAllSiteCollectionNodesAndCustomSettingNode)
                    {
                        await foreach (var setting in HandleContainerInvalidSetting(container))
                        {
                            await settingCsv.WriteAsync(setting);
                        }
                    }
                    continue;
                }

                var containerData = await HandleContainer(containerSetting, container, false);
                if (containerData == default)
                {
                    if(exportSettingType == AvePoint.RA.Contract.FunctionSetting.ExportSettingType.ExportAllSiteCollectionNodesAndCustomSettingNode)
                    {
                        containerData = _settingHelper.ConvertExportTeamsSetting(containerSetting, container.Name, string.Empty);
                    }
                    else
                    {
                        continue;
                    }
                }
                if(containerData != default)
                {
                    GenerateJobDetail(container.Name, container.Name);
                    await settingCsv.WriteAsync(containerData);
                }
                container.PageSize = int.MaxValue;

                var listTeamsOrGroupOfContainer = (await _browseTreeService.BrowseSPOTreeAsync(container, RMBrowseTreeNodeSourceType.Teams, true)).Children;
                if (exportSettingType == AvePoint.RA.Contract.FunctionSetting.ExportSettingType.ExportAllSiteCollectionNodesAndCustomSettingNode)
                {
                    await HandleExportAllSiteCollectionNodeSettings(containerData, listTeamsOrGroupOfContainer, container, settingCsv);
                }
                else
                {
                    await foreach (var setting in HandleTeamsOrGroupInContainer(listTeamsOrGroupOfContainer, container))
                    {
                        await settingCsv.WriteAsync(setting);
                    };

                    await foreach (var setting in HandleSite(listTeamsOrGroupOfContainer, container))
                    {
                        await settingCsv.WriteAsync(setting);
                    };
                }
            }
        }

        private async Task HandleExportAllSiteCollectionNodeSettings(ExportTeamsSettingData containerSetting, List<RMSPSampleTreeNode> listTeamsOrGroupOfContainer, RMSPSampleTreeNode container, TeamsSettingCsv settingCsv)
        {
            await foreach (var setting in HandleTeamsOrGroupInContainerForExportAll(listTeamsOrGroupOfContainer, container, containerSetting))
            {
                ExportTeamsSettingData inheritSetting = Clone(setting);
                if (setting.IsInheritSetting)
                {
                    inheritSetting = Clone(containerSetting) ?? _settingHelper.ConvertTeamsEmptySetting(string.Empty, string.Empty, container.Name);
                }
                await settingCsv.WriteAsync(setting);
                inheritSetting.IsInheritSetting = true;
                var currentTeamsNode = listTeamsOrGroupOfContainer.FirstOrDefault(_ => _.TeamsId.Equals(setting.TeamsId));
                if (currentTeamsNode != null)
                {
                    inheritSetting.TeamsOrGroupName = currentTeamsNode.Name;
                    await foreach (var siteSetting in HandleSiteForExportAll(currentTeamsNode, container, inheritSetting))
                    {
                        await settingCsv.WriteAsync(siteSetting);
                    }
                }
            }
        }

        private async IAsyncEnumerable<ExportTeamsSettingData> HandleTeamsOrGroupInContainerForExportAll(List<RMSPSampleTreeNode> listTeamsOrGroupInContainer, RMSPSampleTreeNode container, ExportTeamsSettingData inheritSetting)
        {
            using CheckJobStopScope jScope = new();
            foreach (var teams in listTeamsOrGroupInContainer)
            {
                var setting = _teamsSettingDao.LoadTeamsSetting(new Guid(teams.TeamsId), new Guid(teams.TeamsId), Guid.Empty);

                if(setting == null)
                {
                    GenerateJobDetail(teams.Name, teams.Name);
                    ExportTeamsSettingData teamsInheritSetting = Clone(inheritSetting) ?? _settingHelper.ConvertTeamsEmptySetting(string.Empty, teams.Name, container.Name);
                    teamsInheritSetting.TeamsId = teams.TeamsId;
                    teamsInheritSetting.TeamsOrGroupName = teams.Name;
                    teamsInheritSetting.FullPath = string.Empty;
                    teamsInheritSetting.IsInheritSetting = true;
                    yield return teamsInheritSetting;
                    continue;
                }

                if(setting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
                {
                    GenerateJobDetail(teams.Name, teams.Name);
                    yield return _settingHelper.ConvertTeamsEmptySetting(string.Empty, teams.Name, container.Name, teamsId: teams.TeamsId);
                    continue;
                }

                if (!ValidateSetting(setting, false))
                {
                    GenerateJobDetail(teams.Name, teams.Name);
                    yield return _settingHelper.ConvertExportTeamsSetting(setting, container.Name, teams.Name);
                    continue;
                }

                yield return await HandleExportTeamsSettingData(setting, teams, container.Name);
            }
        }

        private async IAsyncEnumerable<ExportTeamsSettingData> HandleContainerInvalidSetting(RMSPSampleTreeNode webApplication)
        {
            GenerateJobDetail(webApplication.Name, webApplication.Name);
            yield return _settingHelper.ConvertTeamsEmptySetting(string.Empty,string.Empty, webApplication.Name);
            webApplication.PageSize = int.MaxValue;
            var teamsUnderWebApplication = (await _browseTreeService.BrowseSPOTreeAsync(webApplication, RMBrowseTreeNodeSourceType.Teams, true)).Children;
            if (teamsUnderWebApplication.Any())
            {
                foreach (var teams in teamsUnderWebApplication)
                {
                    GenerateJobDetail(teams.Name, teams.Name);
                    yield return _settingHelper.ConvertTeamsEmptySetting(string.Empty, teams.Name, webApplication.Name, true);
                    var virtualNode = (await _browseTreeService.BrowseSPOTreeAsync(teams, RMBrowseTreeNodeSourceType.Teams, true)).Children.FirstOrDefault();
                    if (virtualNode != null)
                    {
                        virtualNode.PageSize = int.MaxValue;
                        var siteUnderTeams = (await _browseTreeService.BrowseSPOTreeAsync(virtualNode, RMBrowseTreeNodeSourceType.Teams, true)).Children;
                        if (siteUnderTeams.Any())
                        {
                            foreach (var site in siteUnderTeams)
                            {
                                GenerateJobDetail(GetObjectName(site.FullPath), site.FullPath);
                                yield return _settingHelper.ConvertTeamsEmptySetting(site.FullPath, teams.Name, webApplication.Name, true);
                            }
                        }
                    }
                }
            }
        }

        private async IAsyncEnumerable<ExportTeamsSettingData> HandleSiteForExportAll(RMSPSampleTreeNode teamsNode,
            RMSPSampleTreeNode container, ExportTeamsSettingData inheritSetting)
        {
            using CheckJobStopScope jScope = new();

            var virtualNode = (await _browseTreeService.BrowseSPOTreeAsync(teamsNode, RMBrowseTreeNodeSourceType.Teams, true)).Children.FirstOrDefault();
            if (virtualNode != null)
            {
                virtualNode.PageSize = int.MaxValue;
                var siteUnderTeams = ((await _browseTreeService.BrowseSPOTreeAsync(virtualNode, RMBrowseTreeNodeSourceType.Teams, true)).Children);
                foreach (var siteCollection in siteUnderTeams)
                {
                    var settingUnderSiteCollection = _teamsSettingDao.LoadSettingsUnderSite(new Guid(container.Id), new Guid(teamsNode.TeamsId), new Guid(siteCollection.SPObjectId));
                    if (!settingUnderSiteCollection.Any(_ => _.WebId == Guid.Empty))
                    {
                        GenerateJobDetail(GetObjectName(siteCollection.FullPath), siteCollection.FullPath);
                        var exportTeamsSettingData = Clone(inheritSetting) ?? _settingHelper.ConvertTeamsEmptySetting(string.Empty, teamsNode.Name, container.Name);
                        exportTeamsSettingData.FullPath = siteCollection.FullPath;
                        yield return exportTeamsSettingData;
                    }
                    if (settingUnderSiteCollection.Any())
                    {
                        var orderExportSetting = settingUnderSiteCollection.OrderBy(_ => _.FullPath).ThenBy(_ => _.WebId).ToList();
                        foreach (var setting in orderExportSetting)
                        {
                            if (!ValidateSetting(setting, setting.WebId != Guid.Empty) && setting.WebId != Guid.Empty)
                            {
                                continue;
                            }

                            var siteCollectionUrl = siteCollection.FullPath;

                            var remoteSiteCollection = _aveContextHelper.GetRemoteSite(siteCollection.FullPath);
                            if (remoteSiteCollection == null)
                            {
                                GenerateJobDetail(GetObjectName(setting.FullPath), setting.FullPath,
                                    "RM_JS_BCM_ImportSetting_NoSPObject", false);
                                continue;
                            }
                            var node = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(setting.NodeInfo);
                            SettingModel.SettingLevel level = _settingHelper.ConvertNodeLevelToSettingLevel(node);
                            var canProcess = _aveContextHelper.CheckAveTeamsObjectIsExist(setting.FullPath, level, siteCollectionUrl,
                                await _aveContextHelper.GetAveBPOSInfoAsync(remoteSiteCollection), setting.ScopeId);
                            if (canProcess)
                            {
                                if(setting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
                                {
                                    var objectName = setting.FullPath.Substring(setting.FullPath.LastIndexOf(@"/") + 1);
                                    GenerateJobDetail(objectName, setting.FullPath);
                                    yield return _settingHelper.ConvertTeamsEmptySetting(setting.FullPath, teamsNode.Name, container.Name);
                                    continue;
                                }

                                var exportTeamsSettingData = await HandleExportTeamsSettingData(setting, teamsNode, container);
                                yield return exportTeamsSettingData;
                            }
                            else
                            {
                                GenerateJobDetail(GetObjectName(setting.FullPath), setting.FullPath,
                                    "RM_JS_BCM_ImportSetting_NoSPObject", false);
                            }
                        }
                    }
                }
            }
        }

        private string GetObjectName(string fullPath)
        {
            return fullPath.Substring(fullPath.LastIndexOf(@"/") + 1);
        }

        protected override async Task GetListGroupHasSetting()
        {
            try
            {
                var farmNode = _teamsSettingTreeService.LoadFarmSampleTree().FirstOrDefault();
                if (farmNode == null)
                {
                    return;
                }

                farmNode.PageSize = int.MaxValue;
                farmNode.SourceType = (int)SourceFlag.Teams;

                var returnNode = await _browseTreeService.BrowseSPOTreeAsync(farmNode, RMBrowseTreeNodeSourceType.Teams, true);
                _teamsSettingTreeService.TransChildrenNodeName(returnNode);

                var containers = returnNode?.Children;
                if (containers.IsNullOrEmpty())
                {
                    return;
                }

                var groupContainers = containers.GroupBy(item => item.Name).ToArray();

                var uniqueContainers = groupContainers.Where(g => g.Count() == 1).Select(g => g.First());
                groupContainers.Where(g => g.Count() > 1)
                    .ForEach(g => GenerateJobDetail(g.First().Name, "", "RM_JS_BCM_ImportSetting_DuplicateContainerName", false));
                _containers.AddRange(uniqueContainers);
            }
            catch (Exception ex)
            {
                _logger.Error($"error occured GetListGroupHasSetting in TeamsExportSettingProcessor,error : {ex}");
            }
        }

        private bool ValidateSetting(RMTeamsSetting setting, bool isShowDetail = true)
        {
            var comment = _settingHelper.CheckSetting(setting, exportSettingType);
            if (comment != string.Empty)
            {
                if(isShowDetail)
                {
                    var deployDoNotSupportMethods = new[] { (int)DeployTermMethod.UseIntelligenceClassification, (int)DeployTermMethod.UseAutoClassification };
                    string objectName = setting.FullPath.Substring(setting.FullPath.LastIndexOf(@"/") + 1);
                    if (deployDoNotSupportMethods.Contains(setting.DeployTermMethod))
                        GenerateJobDetailWithStatus(objectName, setting.FullPath, AvePoint.RA.Contract.RMWeb.JobMonitor.JobDetailsStatus.Skipped, comment);
                    else
                        GenerateJobDetail(objectName, setting.FullPath, comment, false);
                }
                return false;
            }

            return true;
        }

        private async IAsyncEnumerable<ExportTeamsSettingData> HandleTeamsOrGroupInContainer(List<RMSPSampleTreeNode> listTeamsOrGroupInContainer, RMSPSampleTreeNode container)
        {
            using CheckJobStopScope jScope = new();
            var teamsIds = listTeamsOrGroupInContainer.Select(item => Guid.Parse(item.TeamsId)).ToList();
            var settings = _teamsSettingDao.LoadTeamsSettings(teamsIds, teamsIds);

            foreach (var setting in settings)
            {
                if (!ValidateSetting(setting))
                {
                    continue;
                }

                var teamsOrGroup = listTeamsOrGroupInContainer.First(item => item.TeamsId == setting.TeamsId.ToString());

                yield return await HandleExportTeamsSettingData(setting, teamsOrGroup, container);
            }
        }

        private async IAsyncEnumerable<ExportTeamsSettingData> HandleSite(List<RMSPSampleTreeNode> teamsOrGroups,
            RMSPSampleTreeNode container)
        {
            using CheckJobStopScope jScope = new();
            var teamsIds = teamsOrGroups.Select(item => new Guid(item.TeamsId)).ToList();

            var settingUnderSite = _teamsSettingDao.LoadSettingsUnderTeams(new Guid(container.Id), teamsIds);
            if (!settingUnderSite.Any())
            {
                yield break;
            }

            var siteCollectionAndUrl = _teamsSettingDao.GetSiteCollectionIdAndUrlAsync(settingUnderSite.Select(item => item.SiteId.ToString()));
            var orderExportSetting = settingUnderSite.OrderBy(_ => _.TeamsId).ThenBy(_ => _.SiteId).ThenBy(_ => _.FullPath).ThenBy(_ => _.WebId).ToList();
            foreach (var setting in orderExportSetting)
            {
                if (!ValidateSetting(setting) || !siteCollectionAndUrl.ContainsKey(setting.SiteId))
                {
                    continue;
                }

                var teamsOrGroup = teamsOrGroups.First(item => item.TeamsId == setting.TeamsId.ToString());

                var siteCollectionUrl = siteCollectionAndUrl[setting.SiteId];

                var remoteSiteCollection = _aveContextHelper.GetRemoteSite(siteCollectionUrl);
                if (remoteSiteCollection == null)
                {
                    GenerateJobDetail(setting.FullPath.Substring(setting.FullPath.LastIndexOf(@"/") + 1), setting.FullPath,
                        "RM_JS_BCM_ImportSetting_NoSPObject", false);
                    continue;
                }
                var node = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(setting.NodeInfo);
                SettingModel.SettingLevel level = _settingHelper.ConvertNodeLevelToSettingLevel(node);
                var canProcess = _aveContextHelper.CheckAveTeamsObjectIsExist(setting.FullPath, level, siteCollectionUrl,
                    await _aveContextHelper.GetAveBPOSInfoAsync(remoteSiteCollection), setting.ScopeId);
                if (canProcess)
                {
                    var exportTeamsSettingData = await HandleExportTeamsSettingData(setting, teamsOrGroup, container);
                    yield return exportTeamsSettingData;
                }
                else
                {
                    GenerateJobDetail(GetObjectName(setting.FullPath), setting.FullPath,
                                    "RM_JS_BCM_ImportSetting_NoSPObject", false);
                }
            }

        }

        private async Task GetTermScopeTermDefaultWorkflowUsernames(ExportTeamsSettingData setting)
        {
            setting.TermScopeNamePath = setting.TermId != Guid.Empty
                            ? _termDAO.GetTermNamesPathByTermId(setting.TermId)
                            : _termDAO.GetTermSetNamesPathByTermSetId(setting.TermSetId);
            setting.TermDefaultNamePath = setting.DefaultTermId == Guid.Empty ? string.Empty
                        : _termDAO.GetTermNamesPathByTermId(setting.DefaultTermId).Replace(setting.TermScopeNamePath, "").Replace('/', PathSeparator);

            if (setting.ApprovalType == ApprovalType.ApprovalProcess)
            {
                var workflowReference = _workflowDefinitionDAO.GetWorkflowByReferenceId(new Guid(setting.WorkflowReferenceId));
                if (workflowReference == null)
                {
                    GenerateJobDetail(setting.FullPath.Substring(setting.FullPath.LastIndexOf(@"\") + 1), setting.FullPath,
                            "RM_JS_BCM_ExportSetting_WorkFlowProcessEmpty", false);
                    setting.WorkflowInfomation = null;
                }
                else
                {
                    setting.WorkflowInfomation = new WorkflowInfomation
                    {
                        Id = workflowReference.Id,
                        Name = workflowReference.Name,
                    };
                }
            }
            else if (setting.ApprovalType == ApprovalType.RecordOwners)
            {
                setting.UserName = (await _recordOwnerDao.GetRecordOwnerAccountsAsync(setting.Id, RecordOwnerSettingType.Teams)).Select(item => item.UserPrincipalName).ToList();
            }
        }

        private async Task<ExportTeamsSettingData> HandleContainer(RMTeamsSetting containerSetting, RMSPSampleTreeNode container, bool isShowJobDetail = true)
        {
            if (!ValidateSetting(containerSetting, exportSettingType != AvePoint.RA.Contract.FunctionSetting.ExportSettingType.ExportAllSiteCollectionNodesAndCustomSettingNode))
            {
                return default;
            }
            return await HandleExportTeamsSettingData(containerSetting, null, container, isShowJobDetail);
        }

        private async Task<ExportTeamsSettingData> HandleExportTeamsSettingData(RMTeamsSetting setting, RMSPSampleTreeNode? teamsOrGroup, string containerName, bool isInherit = false)
        {
            var exportTeamsSettingData = _settingHelper.ConvertExportTeamsSetting(setting, containerName, teamsOrGroup is null ? "" : teamsOrGroup.Name, isInherit);
            await GetTermScopeTermDefaultWorkflowUsernames(exportTeamsSettingData);
            if (setting.TeamsId == Guid.Empty)
            {
                GenerateJobDetail(containerName, containerName);
            }
            else
            {
                var objectName = setting.SiteId == Guid.Empty ? setting.FullPath : setting.FullPath.Substring(setting.FullPath.LastIndexOf(@"/") + 1);
                GenerateJobDetail(objectName, setting.FullPath);
            }
            return exportTeamsSettingData;
        }

        private async Task<ExportTeamsSettingData> HandleExportTeamsSettingData(RMTeamsSetting setting, RMSPSampleTreeNode teamsOrGroup, RMSPSampleTreeNode container, bool isShowJobDetail = true)
        {
            var exportTeamsSettingData = _settingHelper.ConvertExportTeamsSetting(setting, container.Name, teamsOrGroup is null ? "" : teamsOrGroup.Name);
            await GetTermScopeTermDefaultWorkflowUsernames(exportTeamsSettingData);
            if(isShowJobDetail)
            {
                if (setting.TeamsId == Guid.Empty)
                {
                    GenerateJobDetail(container.Name, container.Name);
                }
                else
                {
                    var objectName = setting.SiteId == Guid.Empty ? setting.FullPath : setting.FullPath.Substring(setting.FullPath.LastIndexOf(@"/") + 1);
                    GenerateJobDetail(objectName, setting.FullPath);
                }
            }
            return exportTeamsSettingData;
        }
    }
}
