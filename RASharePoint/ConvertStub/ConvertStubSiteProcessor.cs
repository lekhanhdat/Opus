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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.DBLocker;
using AvePoint.RA.SharePoint.Archiver;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.Wrapper.Backup;
using AvePoint.Wrapper.Common;
using HSMCommon;
using Microsoft.SharePoint.News.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.ConvertStub
{
    public class ConvertStubSiteProcessor
    {
        private static readonly AveLogger s_logger = AveLogger.GetInstance(typeof(ConvertStubJobHandler));
        private static readonly Guid s_recordFeatureId = new Guid("da2e115b-07e4-49d9-bb2c-35e93bb9fca9");

        private HSMConnector _hsmConnectorInstance = null;
        public HSMConnector HSMConnector
        {
            get
            {
                _hsmConnectorInstance ??= new HSMConnector(_configuration);
                return _hsmConnectorInstance;
            }
        }

        private ScheduleConfiguration _configuration;

        private Dictionary<Guid, AveSPWeb> _aveSPWebCache = [];
        private Dictionary<Guid, StubListNode> _listStubCache = [];
        private AveSPSite _aveSPSite;
        Dictionary<string, ArchiverConvertStubIndexService> _scAndConvertStubServiceDic = [];
        private readonly object _syncLock = new object();

        public ConvertStubSiteProcessor(ScheduleConfiguration configuration, StubSiteNode siteNode, Dictionary<string, ArchiverConvertStubIndexService> scAndConvertStubServiceDic)
        {
            _configuration = new ScheduleConfiguration(configuration.JobId)
            {
                IsConvertStubJob = true,
                JobReportDto = configuration.JobReportDto,
                ProgressDto = configuration.ProgressDto,
                currentRule = configuration.currentRule,
                NeedConvertStubType = configuration.NeedConvertStubType,
                isConvertSameTypeStub = configuration.isConvertSameTypeStub,
                RuleNameByJobIdDic = configuration.RuleNameByJobIdDic,
                user = configuration.user,
            };
            ConfigForProcessingSite(siteNode);
            _listStubCache = siteNode.StubListNodeCache;
            _scAndConvertStubServiceDic = scAndConvertStubServiceDic;
        }

        private void ConfigForProcessingSite(StubSiteNode siteNode)
        {
            _aveSPSite = siteNode.AveSPSite;
            _configuration.SiteCollectionUrl = siteNode.SiteUrl;
            _configuration.SiteCollectionID = _aveSPSite.SPSite.ID;
            _configuration.siteUrlSchemeAndHost = new Uri(siteNode.SiteUrl).Scheme + @"://" + new Uri(siteNode.SiteUrl).Authority;
            _configuration.aveObjectModelFactory = siteNode.AveObjectModelFactory;
            if (_configuration.StubUserInfos == null || _configuration.StubUserInfos.Count == 0)
            {
                _configuration.StubUserInfos = _aveSPSite.GetUsers();
            }
            if (_configuration.StubGroupInfos == null || _configuration.StubGroupInfos.Count == 0)
            {
                _configuration.StubGroupInfos = _aveSPSite.GetGroups();
            }
            _aveSPSite.SPSite.EnsureRecordFeatureEnabled(s_recordFeatureId);
            if (_aveSPSite.SPSite.DenyAddAndCustomizePagesStatus)
            {
                try
                {
                    _aveSPSite.SPSite.DenyAddAndCustomizePagesStatus = false;
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("OneDrive"))
                    {
                        s_logger.Warn($"Skip setting DenyAddAndCustomizePagesStatus for OneDrive site. Site {siteNode.SiteUrl}, E: {e}");
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            _aveSPSite.SPSite.EnsureWebDeclarationSetting();
        }

        public async Task<int> Run()
        {
            await AddStubsToQueue();

            ProcessStubCreationAndDeletion();

            var updatedIndexRecordCount = UpdateStubInfoes();

            HSMConnector.UploadDataToReportLocation();

            await _configuration.FlushStubFileRecords();

            return updatedIndexRecordCount;
        }

        private async Task AddStubsToQueue()
        {
            using var _ = new PerformanceScope("ConvertStubSiteProcessor:AddStubsToQueue", $"AddStubsToQueue for site {_configuration.SiteCollectionUrl}", true);
            try
            {
                using (new CheckJobStopScope()) { }
                s_logger.Info($"Start add stubs to queue: {_listStubCache.Count}");

                if (_listStubCache.Count == 0)
                {
                    s_logger.Warn("Stub Cache empty");
                    return;
                }

                const int BATCH_SIZE = 50;
                foreach (var (listId, stubListNode) in _listStubCache)
                {
                    try
                    {
                        using (new CheckJobStopScope()) { }

                        if (!_aveSPWebCache.TryGetValue(stubListNode.WebId, out var aveSPWeb))
                        {
                            aveSPWeb = new AveSPWeb(_aveSPSite, stubListNode.WebId, "");
                            _aveSPWebCache[stubListNode.WebId] = aveSPWeb;
                        }

                        var aveSPList = new AveSPList(aveSPWeb, listId, "");
                        stubListNode.ListRootFolderPath = aveSPList.RootFolderPath;

                        using var ps = new PerformanceScope("ConvertStubSiteProcessor:AddStubsToQueue", $"Add to queue found stubs in list: {aveSPList.SPList.RootFolder.Url}", true);

                        HSMConnector.Add2Queue(new HSMListInfo { ListObject = aveSPList.SPList });

                        var allStubUniqueIds = stubListNode.StubFileNodeCache.Keys.ToHashSet();
                        foreach (var batchIds in allStubUniqueIds.Chunk(BATCH_SIZE))
                        {
                            using (new CheckJobStopScope()) { }
                            try
                            {
                                var spItemsMap = BatchGetItemsByUniqueIds(aveSPList, batchIds).ToDictionary(x => x.UniqueId);
                                Parallel.ForEach(batchIds, new ParallelOptions { MaxDegreeOfParallelism = 10 }, stubId =>
                                {
                                    s_logger.Debug($"Start Processing stubId: {stubId} in list {listId}");
                                    if (!stubListNode.StubFileNodeCache.TryGetValue(stubId, out var nodeCache)) return;
                                    if (spItemsMap.TryGetValue(stubId, out var spItem))
                                    {
                                        try
                                        {
                                            ProcessStubFile(_aveSPSite, aveSPList, spItem, nodeCache.FileIndex, nodeCache.IsSkipUpdateIndex);
                                        }
                                        catch (Exception e)
                                        {
                                            s_logger.Error($"Error processing item {stubId}: {e.Message}");
                                            _configuration.JobReportDto.AddRecordReport(nodeCache.FileIndex.Url, ConvertStubAction.Scan, JobDetailsStatus.Failed, e.Message);
                                        }
                                    }
                                    else
                                    {
                                        s_logger.Warn($"Item {stubId} not found in SharePoint list via Batch Fetch.");
                                        _configuration.JobReportDto.AddRecordReport(nodeCache.FileIndex.Url, ConvertStubAction.Scan, JobDetailsStatus.Failed, "Item not found in list");
                                    }
                                    s_logger.Debug($"Finished Processing stubId: {stubId} in list {listId}");
                                });
                            }
                            catch (Exception batchEx)
                            {
                                s_logger.Error($"Error processing batch for list {listId}. E: {batchEx}");
                            }
                        }
                    }
                    catch (JobStopException) { throw; }
                    catch (Exception e)
                    {
                        s_logger.Error($"An error occurred while processing List {listId}. E: {e}");
                        _configuration.JobReportDto.AddRecordReport($"{_configuration.siteUrlSchemeAndHost}/{stubListNode.ListRootFolderPath}", ConvertStubAction.Scan, JobDetailsStatus.Failed, e.Message);
                    }
                }
            }
            catch (JobStopException) { throw; }
            catch (Exception e)
            {
                s_logger.Error($"An error occured while AddStubsToQueue. E: {e}");
                _configuration.ProgressDto.HasErrorNode = true;
                throw;
            }
            finally
            {
                HSMConnector.Finish();
                HSMConnector.WaitingQueueFinshed();
            }
        }

        private void ProcessStubFile(AveSPSite aveSite, AveSPList aveList, IAveListItem spFile, ArchiverBasicIndex basicIndex, bool isSkipUpdateIndex)
        {
            s_logger.Info($"Start process stub file: {basicIndex.NodeGuid}, stubId: {spFile.UniqueId}");

            basicIndex.Name = _configuration.isConvertSameTypeStub
                ? $"{_configuration.JobId}_{basicIndex.Name}"
                : basicIndex.Name;


            var aveFile = new Wrapper.Backup.AveSPItem(
                spFile.UniqueId,
                spFile.ID,
                spFile.File.UIVersion,
                AveItemType.Document,
                spFile.File.ParentFolder.UniqueId,
                aveSite.SPSite.ID,
                aveList,
                aveList.Sender,
                aveList.QueryService,
                aveList.Fields
            );

            using var _ = new PerformanceScope("ConvertStubSiteProcessor:ProcessStubFile", $"Add to queue found stub file: {aveFile.SPListItem.UniqueId}", true);

            var stubFileInfo = new HSMFileInfo
            {
                FileObject = aveFile.Item.File,
                PathMD5 = basicIndex.PathMD5
            };

            var metadata = new AveSPDocumentMetadataDto();
            if (aveFile.RowId > 0)
            {
                metadata.UserDataInfo = aveFile.GetUserData();
                metadata.ItemTPGUIDofLookupValue = aveFile.GetLookupFieldGuidValue();
            }
            aveFile.CachePrincipalFromDatajunction();
            metadata.DocDataJunction = aveFile.GetUserDataJunction();
            metadata.DocInfo_Old = aveFile.GetDocInfo();
            stubFileInfo.MetadataDto = metadata;

            if (aveFile.HasUniqueRoleAssignments)
            {
                stubFileInfo.RoleAssignment = AveRoleAssignments.CreateInstance(aveFile).GetRoleAssignments();
            }


            stubFileInfo.StubId = basicIndex.StubId;

            stubFileInfo.FileServerRelatedUrl = $"{spFile.File.ParentFolder.ServerRelativeUrl}/{basicIndex.Name}";
            stubFileInfo.ArchiverFileIndex = basicIndex;

            HSMConnector.Add2Queue(stubFileInfo);

            lock (_syncLock)
            {
                _configuration.StubCache.Add(aveFile.Id.ToString(), new()
                {
                    BackupFileId = basicIndex.NodeGuid,
                    StubRealId = stubFileInfo.StubId,
                    StubTypeStr = _configuration.currentRule.LeaveStubType.ToString(),
                    SiteUrl = basicIndex.SitePath,
                    Status = JobDetailsStatus.None,
                    IndexRecordId = basicIndex.Id,
                    IsSkipUpdateIndex = isSkipUpdateIndex
                });
            }
        }

        private void ProcessStubCreationAndDeletion()
        {
            s_logger.Info($"start process stub creation and deletion");
            using var _ = new PerformanceScope("ConvertStubSiteProcessor:ProcessStubCreationAndDeletion", $"ProcessStubCreationAndDeletion for site: {_configuration.SiteCollectionUrl}", true);
            try
            {
                foreach (var stubList in _listStubCache)
                {
                    using (new CheckJobStopScope()) { }
                    stubList.Value.StubFileNodeCache.ForEach(item => { HSMConnector.DBForHSMStub.UpdateRecordStatusToVerified(item.Key.ToString()); });
                    var containerIds = HSMConnector.DBForHSMStub.GetContainerIds(_configuration.currentRule.Id, stubList.Key.ToString());
                    s_logger.Info($"containers found for list {stubList.Value.ListRootFolderPath}, listId: {stubList.Key}, count: {containerIds.Count}");

                    foreach (var containerId in containerIds)
                    {
                        using (new CheckJobStopScope()) { }
                        var stubs = HSMConnector.DBForHSMStub.GetRecords(_configuration.currentRule.Id, stubList.Key.ToString(), containerId);
                        s_logger.Info($"stubs found for containerId {containerId}, count: {stubs.Count} in list {stubList.Value.ListRootFolderPath}, listId: {stubList.Key}");
                        using var ps = new PerformanceScope("ConvertStubSiteProcessor:ProcessStubCreationAndDeletion:ProcessContainer",
                            $"AddImportJobTask for container: {containerId}, in list: {stubList.Value.ListRootFolderPath}", true);
                        var failedStubs = stubs.FindAll(s => s.Status != StubExportStauts.Verified);
                        if (failedStubs.Count > 0)
                        {
                            HSMConnector.RebuildJobManifestXML(containerId, failedStubs);
                        }

                        var list = from s in stubs select s.Conver2RestoreFileInfo(_configuration.JobId);
                        HSMConnector.AddImportJobTask(_aveSPSite.SPSite, stubList.Value.WebId, stubList.Key, containerId, false, stubList.Value.ListRootFolderPath, list.ToList());
                        
                    }
                }
            }
            catch (JobStopException)
            {
                throw;
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occured while ProcessStubCreationAndDeletion. E: {e}");
                throw;
            }
            finally
            {
                HSMConnector.WatingCompleted();
            }
        }

        private int UpdateStubInfoes()
        {
            s_logger.Info($"Start update stub info. Count: {_configuration.StubCache.Count}");
            var updatedIndexRecordCount = 0;
            using var _ = new PerformanceScope("ConvertStubSiteProcessor:UpdateStubInfoes", $"UpdateStubInfoes for site {_configuration.SiteCollectionUrl}", true);
            if (_configuration.isConvertSameTypeStub)
            {
                s_logger.Info($"Convert same stub type. No need to update index db");
                return updatedIndexRecordCount;
            }
            foreach (var stubRecord in _configuration.StubCache)
            {
                try
                {
                    using (new CheckJobStopScope()) { }

                    if (stubRecord.Value.Status == JobDetailsStatus.Successful && !stubRecord.Value.IsSkipUpdateIndex)
                    {
                        using var ps = new PerformanceScope("ConvertStubSiteProcessor:UpdateStubInfoes:UpdateFileStubInfo", $"UpdateStubInfo for file {stubRecord.Value.IndexRecordId}", true);
                        var stubInfo = ConvertStubUtility.GenerateStubInfo(stubRecord.Value.StubTypeStr, stubRecord.Value.StubRealId);
                        s_logger.Debug($"Update stub info for file {stubRecord.Value.IndexRecordId} stubinfo: {stubInfo}");
                        _scAndConvertStubServiceDic[stubRecord.Value.SiteUrl].UpdateStubInfo(stubRecord.Value.IndexRecordId, stubInfo);
                        updatedIndexRecordCount++;
                    }
                }
                catch (JobStopException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    s_logger.Error($"An error occured while UpdateStubInfo. FileId: {stubRecord.Value.BackupFileId} E: {e}");
                    _configuration.ProgressDto.HasErrorNode = true;
                    throw;
                }
            }

            return updatedIndexRecordCount;
        }

        private IAveListItemCollection BatchGetItemsByUniqueIds(AveSPList aveList, Guid[] uniqueIds)
        {
            if (uniqueIds == null || uniqueIds.Length == 0) return null;

            //var valuesXml = string.Join("", uniqueIds.Select(id => $"<Value Type='Guid'>{id}</Value>"));

            //AveCamlQuery query = new AveCamlQuery();
            //query.ViewXml = $@"
            //<View Scope='RecursiveAll'>
            //    <Query>
            //        <Where>
            //            <In>
            //                <FieldRef Name='UniqueId' />
            //                <Values>{valuesXml}</Values>
            //            </In>
            //        </Where>
            //    </Query>
            //    <ViewFields>
            //        <FieldRef Name='File' />
            //        <FieldRef Name='FileLeafRef' />
            //        <FieldRef Name='FileDirRef' />
            //        <FieldRef Name='Modified' />
            //        <FieldRef Name='Editor' />
            //        <FieldRef Name='Author' />
            //        <FieldRef Name='UniqueId' />
            //        <FieldRef Name='ID' />
            //    </ViewFields>
            //</View>";

            //return aveList.SPList.GetItems(query);
            try
            {
                return aveList.SPList.GetItemsByUniqueIds(uniqueIds);
            }
            catch (Exception e)
            {
                s_logger.Error($"Error in BatchGetItemsByUniqueIds. E: {e}");
                throw;
            }
        }
    }
}
