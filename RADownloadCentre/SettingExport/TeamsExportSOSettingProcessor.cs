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
using Aspose.Email.Clients.Activity;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.SharePoint.Common.Setting;
using RADownloadCentre.SettingExport.Base;
using RADownloadCentre.SettingExport.Helper;
using RADownloadCentre.SettingExport.Model;
using Level = AvePoint.RA.SharePoint.Common.Setting.Model.SettingLevel;
using RuleType = AvePoint.RA.DB.Dao.Impl.RuleType;

namespace RADownloadCentre.SettingExport
{
    public class TeamsExportSOSettingProcessor : ExportSettingProcessor<ExportTeamsSOSettingData>
    {
        private RALogger Logger = RALogger.GetInstance(typeof(TeamsExportSOSettingProcessor));
        private AveContextHelper _aveContextHelper;
        private AveContextHelper AveContextHelper
        {
            get
            {
                if (_aveContextHelper == null)
                {
                    _aveContextHelper = new AveContextHelper();
                }
                return _aveContextHelper;
            }
        }
        private readonly SettingHelper _settingHelper;
        private readonly string _startFileName = "ES_TeamsSO_";

        private ITeamsSettingTreeService TeamsSettingTreeService => PlatformWindsorManager.GetService<ITeamsSettingTreeService>();
        private IRMArchiverSettingDao ArchiverSettingDao => PlatformWindsorManager.GetService<IRMArchiverSettingDao>();
        private IEXOSettingRuleDao EXOSettingRuleDao => PlatformWindsorManager.GetService<IEXOSettingRuleDao>();

        private List<RMSPSampleTreeNode> WebApplications = new List<RMSPSampleTreeNode>();

        private bool isExportAllSetting = false;

        public TeamsExportSOSettingProcessor(RMExportSettingJobMessage message) : base(message.JobID, message.JobType)
        {
            FilePath = JobReportUtility.GetDownloadReportDetailTempleFolder(BaseJobDto, BaseJobDto.Id.Replace("ETSOS", _startFileName), ".csv");
            exportSettingType = message.exportSettingType;
            isExportAllSetting = exportSettingType == AvePoint.RA.Contract.FunctionSetting.ExportSettingType.ExportAllSiteCollectionNodesAndCustomSettingNode;
            _settingHelper = new();
        }

        protected override string[][] AssembleSettingHeaderTittle(string[][] datas, string connectionName)
        {
            throw new NotImplementedException();
        }

        protected override Task<string[][]> ConvertSettingToArrayAsync(List<ExportTeamsSOSettingData> settings, string[][] datas)
        {
            throw new NotImplementedException();
        }

        protected override async Task GenerateDataAsync()
        {
            Logger.Info($"Current export setting type is {exportSettingType}");
            await using TeamsSOSettingCsv settingCsv = new(FilePath, BaseJobDto);
            await settingCsv.WriteHeaderAsync();
            foreach (var webApplication in WebApplications)
            {
                var webApplicationSetting = ArchiverSettingDao.LoadArchiverSettingByContentSource(new Guid(webApplication.Id), Guid.Empty, Guid.Empty, ContentSourceType.Teams);

                ExportTeamsSOSettingData containerExportSetting = null;
                if (webApplicationSetting == null)
                {
                    Logger.Info($"Current {webApplication.Id} container have null setting");
                    containerExportSetting = _settingHelper.ConvertTeamsSOEmptySetting(string.Empty, string.Empty, webApplication.Name, nodeLevel: Level.Container);
                    if (isExportAllSetting)
                    {
                        GenerateJobDetail(webApplication.Name, webApplication.Name);
                        await settingCsv.WriteAsync(containerExportSetting);
                    }
                }
                else
                {
                    containerExportSetting = HandleExportTeamsSettingData(webApplicationSetting, null, null, webApplication, nodeLevel: Level.Container);
                    await settingCsv.WriteAsync(containerExportSetting);
                }

                await foreach (var (teamsSetting, teamsNode) in HandleTeamsSettingData(webApplication, containerExportSetting))
                {
                    ExportTeamsSOSettingData inheritSetting = Clone(teamsSetting) ?? _settingHelper.ConvertTeamsSOEmptySetting(string.Empty, teamsNode.Name, webApplication.Name, nodeLevel: Level.TeamsOrGroup);
                    if (teamsSetting != null)
                    {
                        await settingCsv.WriteAsync(teamsSetting);
                    }
                    else
                    {
                        inheritSetting = Clone(containerExportSetting) ?? _settingHelper.ConvertTeamsSOEmptySetting(string.Empty, string.Empty, webApplication.Name, nodeLevel: Level.TeamsOrGroup);
                    }

                    inheritSetting.IsInheritSetting = true;
                    inheritSetting.TeamsOrGroupName = teamsNode.Name;

                    await foreach (var settingUnderSiteCollection in HandleSiteCollectionSettingData(webApplication, teamsNode, inheritSetting))
                    {
                        await settingCsv.WriteAsync(settingUnderSiteCollection);
                    }
                }
            }
        }

        private async IAsyncEnumerable<ExportTeamsSOSettingData> HandleSiteCollectionSettingData(RMSPSampleTreeNode webApplication, RMSPSampleTreeNode teamsNode, ExportTeamsSOSettingData inheritSetting)
        {
            var siteCollectionUnderTeams = await GetSiteCollectionNodeUnderTeams(teamsNode);
            foreach (var site in siteCollectionUnderTeams)
            {
                var settingUnderSiteCollection = ArchiverSettingDao.LoadArchiverSettingsUnderSite(new Guid(site.Id), ContentSourceType.Teams);
                if (!settingUnderSiteCollection.Any(_ => _.SiteId == _.SPObjectId) && isExportAllSetting)
                {
                    var siteCollectionSetting = Clone(inheritSetting) ?? _settingHelper.ConvertTeamsSOEmptySetting(site.FullPath, teamsNode.Name, webApplication.Name, nodeLevel: Level.SiteCollection);
                    siteCollectionSetting.SiteCollectionUrl = site.FullPath;
                    siteCollectionSetting.IsInheritSetting = true;
                    siteCollectionSetting.NodeLevel = Level.SiteCollection;
                    GenerateJobDetail(GetObjectName(site.FullPath), site.FullPath);
                    yield return siteCollectionSetting;
                }

                if (settingUnderSiteCollection.Any())
                {
                    var siteCollectionSetting = settingUnderSiteCollection.Where(_ => _.SiteId == _.SPObjectId).FirstOrDefault();
                    var orderExportSetting = new List<RMArchiverSetting>();
                    if (siteCollectionSetting != null) orderExportSetting.Add(siteCollectionSetting);
                    var settingWithoutSiteCollection = settingUnderSiteCollection.Where(_ => _.SiteId != _.SPObjectId).OrderBy(_ => _.Url);
                    if (settingWithoutSiteCollection.Any()) orderExportSetting.AddRange(settingWithoutSiteCollection);
                    foreach (var setting in orderExportSetting)
                    {
                        var siteCollectionUrl = site.FullPath;

                        var remoteSiteCollection = AveContextHelper.GetRemoteSite(siteCollectionUrl);
                        if (remoteSiteCollection == null)
                        {
                            GenerateJobDetail(GetObjectName(setting.Url), setting.Url,
                                "RM_JS_BCM_ImportSetting_NoSPObject", false);
                            continue;
                        }
                        var (siteUrl, listUrl, folderUrl, canProcess, level) = AveContextHelper.GetStructOfObject(siteCollectionUrl, setting.Url, await AveContextHelper.GetAveBPOSInfoAsync(remoteSiteCollection),
                            setting.SPObjectId == setting.SiteId, setting.SPObjectId);
                        if (canProcess)
                        {
                            var exportTeamsSettingData = HandleExportTeamsSettingData(setting, site, teamsNode, webApplication, siteUrl, listUrl, folderUrl, level);
                            yield return exportTeamsSettingData;
                        }
                        else
                        {
                            GenerateJobDetail(GetObjectName(setting.Url), setting.Url,
                                "RM_JS_BCM_ImportSetting_NoSPObject", false);
                        }
                    }
                }
            }
        }

        private string GetObjectName(string fullPath)
        {
            return fullPath.Substring(fullPath.LastIndexOf(@"/") + 1);
        }

        private async IAsyncEnumerable<(ExportTeamsSOSettingData?, RMSPSampleTreeNode)> HandleTeamsSettingData(RMSPSampleTreeNode webApplication, ExportTeamsSOSettingData inheritSetting)
        {
            var teamsNodes = await GetChildrenNode(webApplication);
            foreach (var teams in teamsNodes)
            {
                if (teams != null)
                {
                    var teamsSetting = ArchiverSettingDao.LoadArchiverSettingByContentSource(new Guid(teams.TeamsId), Guid.Empty, new Guid(teams.TeamsId), ContentSourceType.Teams);
                    if (teamsSetting == null)
                    {
                        if (isExportAllSetting)
                        {
                            GenerateJobDetail(teams.Name, teams.Name);
                            ExportTeamsSOSettingData teamsInheritSetting = Clone(inheritSetting) ?? _settingHelper.ConvertTeamsSOEmptySetting(string.Empty, teams.Name, webApplication.Name, nodeLevel: Level.TeamsOrGroup);
                            teamsInheritSetting.IsInheritSetting = true;
                            teamsInheritSetting.TeamsOrGroupName = teams.Name;
                            teamsInheritSetting.NodeLevel = Level.TeamsOrGroup;
                            yield return (teamsInheritSetting, teams);
                            continue;
                        }
                        yield return (null, teams);
                        continue;
                    }
                    var exportTeamsSetting = HandleExportTeamsSettingData(teamsSetting, null, teams, webApplication, nodeLevel: Level.TeamsOrGroup);
                    yield return (exportTeamsSetting, teams);
                }
            }
        }

        private async Task<List<RMSPSampleTreeNode>> GetSiteCollectionNodeUnderTeams(RMSPSampleTreeNode teams)
        {
            var virtualNode = (await GetChildrenNode(teams)).FirstOrDefault();
            if (virtualNode != null)
            {
                return await GetChildrenNode(virtualNode);
            }
            return new List<RMSPSampleTreeNode>();
        }

        private async Task<List<RMSPSampleTreeNode>> GetChildrenNode(RMSPSampleTreeNode parentNode)
        {
            return await TeamsSettingTreeService.BrowseSampleTreeAsync(parentNode);
        }

        protected override async Task GetListGroupHasSetting()
        {
            try
            {
                var farmNode = TeamsSettingTreeService.LoadFarmSampleTree().FirstOrDefault();
                if (farmNode == null)
                {
                    return;
                }

                farmNode.PageSize = int.MaxValue;

                var returnNode = await TeamsSettingTreeService.BrowseSampleTreeAsync(farmNode, true);
                farmNode.Children = returnNode;
                TeamsSettingTreeService.TransChildrenNodeName(farmNode);

                var containers = farmNode.Children;
                if (containers.IsNullOrEmpty())
                {
                    return;
                }

                var groupContainers = containers.GroupBy(item => item.Name).ToArray();

                var uniqueContainers = groupContainers.Where(g => g.Count() == 1).Select(g => g.First());
                groupContainers.Where(g => g.Count() > 1)
                    .ForEach(g => GenerateJobDetail(g.First().Name, "", "RM_JS_BCM_ImportSetting_DuplicateContainerName", false));
                WebApplications.AddRange(uniqueContainers);
            }
            catch (Exception ex)
            {
                Logger.Error($"error occured GetListGroupHasSetting in TeamsExportSOSettingProcessor,error : {ex}");
            }
        }

        private ExportTeamsSOSettingData HandleExportTeamsSettingData(RMArchiverSetting setting, RMSPSampleTreeNode? siteCollection, RMSPSampleTreeNode? teamsOrGroup, RMSPSampleTreeNode container, string siteUrl = "", string listUrl = "", string folderUrl = "", Level nodeLevel = Level.Container)
        {
            var exportTeamsSettingData = _settingHelper.ConvertExportTeamsSOSetting(setting, container.Name, teamsOrGroup is null ? "" : teamsOrGroup.Name, siteCollection is null ? "" : siteCollection.FullPath, siteUrl, listUrl, folderUrl, nodeLevel: nodeLevel);
            var archiverRule = EXOSettingRuleDao.GetArchiverMappingRules(setting.Id, (int)RuleType.Archiver);
            exportTeamsSettingData.Rules = _settingHelper.ConvertArchiverRuleMapping(archiverRule);
            if(setting.TeamsId == Guid.Empty)
            {
                GenerateJobDetail(container.Name, container.Name);
            }
            else
            {
                var objectName = setting.SiteId == Guid.Empty ? setting.Url : setting.Url.Substring(setting.Url.LastIndexOf(@"/") + 1);
                GenerateJobDetail(objectName, setting.Url);
            }
            return exportTeamsSettingData;
        }
    }
}
