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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.TeamsSetting;
using AvePoint.RA.SharePoint.Common.Setting;
using RADownloadCentre.SettingExport.Base;
using RADownloadCentre.SettingExport.Helper;
using RADownloadCentre.SettingExport.Model;
using RuleType = AvePoint.RA.DB.Dao.Impl.RuleType;
using Level = AvePoint.RA.SharePoint.Common.Setting.Model.SettingLevel;


namespace RADownloadCentre.SettingExport
{
    public class SPExportSOSettingProcessor : ExportSettingProcessor<ExportSPSOSettingData>
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
        private readonly string _startFileName = "ES_SPSO_";

        private ISPSettingTreeService SPSettingTreeService => PlatformWindsorManager.GetService<ISPSettingTreeService>();
        private IRMArchiverSettingDao ArchiverSettingDao => PlatformWindsorManager.GetService<IRMArchiverSettingDao>();
        private IEXOSettingRuleDao EXOSettingRuleDao => PlatformWindsorManager.GetService<IEXOSettingRuleDao>();

        private List<RMSPSampleTreeNode> WebApplications = new List<RMSPSampleTreeNode>();

        public SPExportSOSettingProcessor(RMExportSettingJobMessage message) : base(message.JobID, message.JobType)
        {
            FilePath = JobReportUtility.GetDownloadReportDetailTempleFolder(BaseJobDto, BaseJobDto.Id.Replace("ESPSOS", _startFileName), ".csv");
            exportSettingType = message.exportSettingType;
            _settingHelper = new();
        }

        protected override string[][] AssembleSettingHeaderTittle(string[][] datas, string connectionName)
        {
            throw new NotImplementedException();
        }

        protected override Task<string[][]> ConvertSettingToArrayAsync(List<ExportSPSOSettingData> settings, string[][] datas)
        {
            throw new NotImplementedException();
        }

        protected override async Task GenerateDataAsync()
        {
            Logger.Info($"Current export setting type is {exportSettingType}");
            await using SharePointSOSettingCsv settingCsv = new(FilePath, BaseJobDto);
            await settingCsv.WriteHeaderAsync();
            foreach (var webApplication in WebApplications)
            {
                var webApplicationSetting = ArchiverSettingDao.LoadArchiverSettingByContentSource(new Guid(webApplication.Id), Guid.Empty, Guid.Empty, ContentSourceType.SharePoint);

                ExportSPSOSettingData containerExportSetting = null;
                if (webApplicationSetting == null)
                {
                    Logger.Info($"Current {webApplication.Name} container have null setting");
                    containerExportSetting = _settingHelper.ConvertSharePointSOEmptySetting(string.Empty, webApplication.Name, nodeLevel: Level.Container);
                    if (exportSettingType == AvePoint.RA.Contract.FunctionSetting.ExportSettingType.ExportAllSiteCollectionNodesAndCustomSettingNode)
                    {
                        GenerateJobDetail(webApplication.Name, webApplication.Name);
                        await settingCsv.WriteAsync(containerExportSetting);
                    }
                }
                else
                {
                    containerExportSetting = HandleExportSPSettingData(webApplicationSetting, null, webApplication, nodeLevel: Level.Container);
                    await settingCsv.WriteAsync(containerExportSetting);
                }

                await foreach (var settingUnderSiteCollection in HandleSiteCollectionSettingData(webApplication, containerExportSetting))
                {
                    await settingCsv.WriteAsync(settingUnderSiteCollection);
                }
            }
        }

        private async IAsyncEnumerable<ExportSPSOSettingData> HandleSiteCollectionSettingData(RMSPSampleTreeNode webApplication, ExportSPSOSettingData inheritSetting)
        {
            var siteCollectionUnderTeams = await GetChildrenNode(webApplication);
            foreach (var site in siteCollectionUnderTeams)
            {
                var settingUnderSiteCollections = ArchiverSettingDao.LoadArchiverSettingsUnderSite(new Guid(site.Id));
                if (!settingUnderSiteCollections.Any(_ => _.SiteId == _.SPObjectId) && exportSettingType == AvePoint.RA.Contract.FunctionSetting.ExportSettingType.ExportAllSiteCollectionNodesAndCustomSettingNode)
                {
                    var siteCollectionSetting = Clone(inheritSetting) ?? _settingHelper.ConvertSharePointSOEmptySetting(site.FullPath, webApplication.Name, true, Level.SiteCollection);
                    siteCollectionSetting.SiteCollectionUrl = site.FullPath;
                    siteCollectionSetting.IsInheritSetting = true;
                    siteCollectionSetting.NodeLevel = Level.SiteCollection;
                    GenerateJobDetail(GetObjectName(site.FullPath), site.FullPath);
                    yield return siteCollectionSetting;
                }

                if (settingUnderSiteCollections.Count != 0)
                {
                    var siteCollectionSetting = settingUnderSiteCollections.Where(_ => _.SiteId == _.SPObjectId).FirstOrDefault();
                    var orderExportSetting = new List<RMArchiverSetting>();
                    if (siteCollectionSetting != null) orderExportSetting.Add(siteCollectionSetting);
                    var settingWithoutSiteCollection = settingUnderSiteCollections.Where(_ => _.SiteId != _.SPObjectId).OrderBy(_ => _.Url);
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
                            var exportSPSettingData = HandleExportSPSettingData(setting, site, webApplication, siteUrl, listUrl, folderUrl, level);
                            yield return exportSPSettingData;
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

        private async IAsyncEnumerable<ExportSPSOSettingData> HandleExportStructContainerDoesNotSetting(RMSPSampleTreeNode webApplication)
        {
            GenerateJobDetail(webApplication.Name, webApplication.Name);
            yield return _settingHelper.ConvertTeamsSOEmptySetting(string.Empty, string.Empty, webApplication.Name);
            List<RMSPSampleTreeNode> siteCollections = await GetChildrenNode(webApplication);
            foreach (var site in siteCollections)
            {
                GenerateJobDetail(GetObjectName(site.FullPath), site.FullPath);
                yield return _settingHelper.ConvertSharePointSOEmptySetting(site.FullPath, webApplication.Name, true);
            }
        }

        private string GetObjectName(string fullPath)
        {
            return fullPath.Substring(fullPath.LastIndexOf(@"/") + 1);
        }

        private async Task<List<RMSPSampleTreeNode>> GetChildrenNode(RMSPSampleTreeNode parentNode)
        {
            return await SPSettingTreeService.BrowseSampleTreeAsync(parentNode);
        }

        protected override async Task GetListGroupHasSetting()
        {
            try
            {
                var farmNode = SPSettingTreeService.LoadFarmSampleTree().FirstOrDefault();
                if (farmNode == null)
                {
                    return;
                }

                farmNode.PageSize = int.MaxValue;

                var returnNode = await SPSettingTreeService.BrowseSampleTreeAsync(farmNode, true);
                farmNode.Children = returnNode;
                SPSettingTreeService.TransChildrenNodeName(farmNode);

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
                Logger.Error($"error occured GetListGroupHasSetting in SPExportSOSettingProcessor,error : {ex}");
            }
        }

        private ExportSPSOSettingData HandleExportSPSettingData(RMArchiverSetting setting, RMSPSampleTreeNode? siteCollection, RMSPSampleTreeNode container, string siteUrl = "", string listUrl = "", string folderUrl = "", Level nodeLevel = Level.Container)
        {
            var exportTeamsSettingData = _settingHelper.ConvertExportSPSOSetting(setting, container.Name, siteCollection is null ? "" : siteCollection.FullPath, siteUrl, listUrl, folderUrl, nodeLevel: nodeLevel);
            var archiverRule = EXOSettingRuleDao.GetArchiverMappingRules(setting.Id, (int)RuleType.Archiver);
            exportTeamsSettingData.Rules = _settingHelper.ConvertArchiverRuleMapping(archiverRule);
            if(siteCollection is null)
            {
                GenerateJobDetail(container.Name, container.Name);
            }
            else
            {
            var objectName = setting.SiteId == Guid.Empty ? setting.Url : GetObjectName(setting.Url);
            GenerateJobDetail(objectName, setting.Url);
            }
            return exportTeamsSettingData;
        }
    }
}
