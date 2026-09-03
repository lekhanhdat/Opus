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
using AvePoint.RA.Common.RMRuleManagement;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.Archiver.Scan.Base;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common.JobExecutionProcess;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using DocumentFormat.OpenXml.Spreadsheet;
using Google.Api.Gax.ResourceNames;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Archiver.Scan.Base
{
    public class EndUserSharePointScanner : ArchiverSharePointScanner
    {
        #region Fields & Constructor

        private static readonly RALogger mLog = RALogger.GetInstance(typeof(EndUserSharePointScanner));

        private EndUserArchiveSiteCollectionConfig EndUserArchiveSiteCollectionConfigtoreConfig;

        internal ArchiverNodeItem root = new();

        private Dictionary<Guid, ArchiverNodeItem> mWebCache = new();
        private Dictionary<Guid, ArchiverNodeItem> mListCache = new();
        private Dictionary<string, ArchiverNodeItem> mFolderCache = new(StringComparer.OrdinalIgnoreCase);
        private HashSet<Guid> mExceptionWebIds = new();
        private HashSet<ArchiverNodeItem> mExcpetionNodes = new();


        #region Properties

        private IDiscoverNodeWorker mDiscoverWorker = null;
        public override IDiscoverNodeWorker discoverWorker
        {
            get
            {
                if (mDiscoverWorker == null)
                {
                    mDiscoverWorker = new ScanDiscovrerNodeWorker(jobSettings, mConfiguration, mDependencyObjs, false);
                }
                return mDiscoverWorker;
            }
            set { }
        }

        #endregion

        public EndUserSharePointScanner(ScanJobSettings scanJobSettings) : base(scanJobSettings)
        {
            mLog.Info("EndUserSharePointScanner constructor started.");
            EndUserArchiveSiteCollectionConfigtoreConfig = scanJobSettings.Configuration.EndUserArchiveSiteCollectionConfig;
            if (EndUserArchiveSiteCollectionConfigtoreConfig == null || string.IsNullOrWhiteSpace(mConfiguration.SiteCollectionUrl))
            {
                mLog.Error("Site collection information missing in restore configuration.");
                throw new InvalidOperationException("Site collection information is missing in restore configuration.");
            }
            if (EndUserArchiveSiteCollectionConfigtoreConfig.FileInfoList == null || EndUserArchiveSiteCollectionConfigtoreConfig.FileInfoList.Count == 0)
            {
                mLog.Error("No files specified for processing in restore configuration.");
                throw new InvalidOperationException("No files specified for processing in restore configuration.");
            }
            EndUserArchiveSiteCollectionConfigtoreConfig.FileInfoList = EndUserArchiveSiteCollectionConfigtoreConfig.FileInfoList
                .Where(file => file != null && !string.IsNullOrWhiteSpace(file.GetDecodedFullPath()))
                .GroupBy(file => file.Id + '/' + file.GetDecodedFullPath(), StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First()).ToList();
            mLog.Info("EndUserSharePointScanner constructor finished.");

            mLog.Info($"[EndUserArchiveSiteCollectionConfig] FileInfoList count: {EndUserArchiveSiteCollectionConfigtoreConfig.FileInfoList?.Count}");
            foreach (var fileInfo in EndUserArchiveSiteCollectionConfigtoreConfig.FileInfoList)
            {
                mLog.Info($"[FileInfo] Id: {fileInfo?.Id}, WebId: {fileInfo?.WebId}");
            }
            mLog.Info($"[EndUserArchiveSiteCollectionConfig] JobConflictFileInfoList count: {EndUserArchiveSiteCollectionConfigtoreConfig.SkipFileInfoList?.Count}");
            foreach (var skippedNode in EndUserArchiveSiteCollectionConfigtoreConfig.SkipFileInfoList)
            {
                mLog.Info($"[SkippedFileInfo] Id: {skippedNode?.Id}, WebId: {skippedNode?.WebId}");
            }
        }

        #endregion

        #region Public Interface & Main Flow

        public override async Task RunAsync()
        {
            mLog.Info("RunAsync started.");
            try
            {
                JobExecutionProcessStatisticExecutor.Instance.StartCalculateRuleAndSummary(NodeLevel.Item.ToString(), mConfiguration.SiteCollectionUrl);
                discoverWorker.Init(null);
                mConfiguration.ProgressDto.SetBaseCount4Phase(EndUserArchiveSiteCollectionConfigtoreConfig.FileInfoList.Count());

                BuildTree();
                mConfiguration.ProgressDto.SetBaseCount4Phase(NeedProcessNodeCount(root));
                await ProcessTree(root);
                discoverWorker.Flush();
            }
            finally
            {
                JobExecutionProcessStatisticExecutor.Instance.EndCalculateRuleAndScanSummary(NeedProcessNodeCount(root), null);
                mExcpetionNodes.Clear();
                mWebCache.Clear();
                mListCache.Clear();
                mFolderCache.Clear();
                root.Dispose();
                root = null;
                mLog.Info("RunAsync finished and resources cleaned up.");
            }
        }

        public int NeedProcessNodeCount(ArchiverNodeItem node)
        {
            int res = 0;
            if (node == null)
            {
                return 0;
            }
            res += node.Children.Values.Where(sub => sub?.Status == JobDetailsStatus.Successful).Count();
            foreach(var subNode in node.Children.Values.Where(sub => sub?.Status == JobDetailsStatus.Successful))
            {
                res += NeedProcessNodeCount(subNode);
            }
            return res;
        }

        public IAveSite BuildSite()
        {
            try
            {
                mLog.Info("BuildSite started.");
                IAveSite site = mConfiguration.aveObjectModelFactory.CreateSite(mConfiguration.SiteCollectionUrl);
                root.SiteId = site.ID;
                root.SiteUrl = site.Url;
                root.FullPath = site.Url;
                root.Name = site.Url;
                root.ID = site.ID;
                root.DiscoverSPObject = new AveDiscoverSite(site, GetBposInfoBySite(mConfiguration.SiteCollectionUrl), AveDiscoveryKind.API, DiscoverModule.Archive);
                mLog.Info("BuildSite finished.");
                return site;
            }
            catch (Exception ex) 
            {
                mLog.Error($"Fail build Site,ex:{ex}");
                root.Status = JobDetailsStatus.Failed;
                root.SiteUrl = mConfiguration?.SiteCollectionUrl;
                root.Name = mConfiguration?.SiteCollectionUrl;
                root.FullPath = mConfiguration?.SiteCollectionUrl;
                return null;
            }
            finally
            {
                root.Cache_NodeType = (int)CacheNodeType.SiteCollection;
                root.SPNodeLevel = NodeLevel.SiteCollection;
                root.ReportLevel = (int)SPNodeLevel.SiteCollection;
            }
            
        }

        public void BuildTree()
        {
            mLog.Info("BuildTree started.");
            IAveSite site = BuildSite();
            foreach (var fileInfo in EndUserArchiveSiteCollectionConfigtoreConfig.FileInfoList)
            {
                try
                {
                    BuildChainForFile(fileInfo, (AveDiscoverSite)root.DiscoverSPObject, site);
                }
                catch (Exception ex)
                {
                    mLog.Warn($"Failed to build nodes for file '{fileInfo.GetDecodedFullPath()}'. {ex}");
                    string exceptionMessage = I18NEntity.HasKey(ex.InnerException?.Message) ? ex.InnerException.Message : "StorageOptimization_EndUserArchive_ParentNodeProcessException";
                    exceptionMessage = I18NEntity.HasKey(ex.Message) ? ex.Message : exceptionMessage;
                    mExcpetionNodes.Add(CreateExceptionItemNode(fileInfo, JobDetailsStatus.Exception, exceptionMessage));
                }
            }
            foreach (var exceptionNode in mExcpetionNodes)
            {
                AttachToNearestParent(exceptionNode);
            }
            foreach (var skippedNode in EndUserArchiveSiteCollectionConfigtoreConfig.SkipFileInfoList)
            {
                AttachToNearestParent(CreateExceptionItemNode(skippedNode, JobDetailsStatus.Skipped, skippedNode.ErrorMessage));
            }
            foreach (var ExceptionFileInfo in EndUserArchiveSiteCollectionConfigtoreConfig.ExceptionFileInfoList)
            {
                AttachToNearestParent(CreateExceptionItemNode(ExceptionFileInfo, JobDetailsStatus.Exception, ExceptionFileInfo.ErrorMessage));
            }
            UpdateNodeScanStatus(root);
            mLog.Info("BuildTree finished.");
        }

        #endregion

        #region Tree Processing

        public async Task ProcessTree(ArchiverNodeItem node)
        {
            if (node.SPNodeLevel < NodeLevel.Item)
            {
                await ProcessContainer(node);
            }
            else
            {
                await ProcessItem(node);
            }
        }

        private async Task ProcessContainer(ArchiverNodeItem node)
        {
            mLog.Info($"ProcessContainer started for node level: {node?.SPNodeLevel}, nodeId :{node?.ID}");
            if (node.SPNodeLevel == NodeLevel.List)
            {
                mDependencyObjs.PutIn(node.SPList, (int)NodeLevel.List, false);
            }
            await discoverWorker.ProcessContainerAsync(node, ProcessType.NoNeedProcess);
            mConfiguration.JobReportDto.AddScanReport(GetFullPath(node.FullPath), 0, (int)node.Cache_NodeType, node.RuleName, node.Status, node.ExceptionMessage);
            foreach (var child in node.Children.Values)
            {
                await ProcessTree(child);
            }
        }

        private string GetFullPath(string path)
        {
            if(string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }
            if(path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }
            Uri siteUri = new Uri(mConfiguration.SiteCollectionUrl);
            return siteUri.Scheme + "://" + siteUri.Host + '/' + path.Trim('/');
        }

        private async Task ProcessItem(ArchiverNodeItem node)
        {
            mLog.Info($"ProcessItem started for node level: {node?.SPNodeLevel}, nodeId:{node?.ID}");
            if (node.Status == JobDetailsStatus.Successful)
            {
                AveDiscoverItem item = node.DiscoverSPObject as AveDiscoverItem;
                AveDiscoverFolder parentFolder = node.Parent.DiscoverSPObject as AveDiscoverFolder;
                await ProcessVersionAndAttachmentsAsync(item, parentFolder, node.Parent, discoverWorker);
            }
            else
            {
                mConfiguration.JobReportDto.AddScanReport(GetFullPath(node.FullPath), 0, (int)node.Cache_NodeType, node.RuleName, node.Status, node.ExceptionMessage);
            }
        }

        private void UpdateNodeScanStatus(ArchiverNodeItem node)
        {
            if (node == null)
            {
                return;
            }

            if (node.Parent != null && node?.Parent.Status != JobDetailsStatus.Successful && node.Status == JobDetailsStatus.Successful)
            {
                node.Status = node.Parent.Status;
            }

            if (node.Parent?.Status == JobDetailsStatus.Skipped)
            {
                node.Status = JobDetailsStatus.Skipped;
            }

            foreach (var child in node.Children.Values)
            {
                UpdateNodeScanStatus(child);
            }

            /*if (node.Children.All(subNode => subNode.Value.Status == JobDetailsStatus.Exception))
            {
                node.Status = JobDetailsStatus.Exception;
            }*/
            node.ForcedNotReport = true;
        }

        #endregion

        #region Tree Construction Helpers

        private void BuildChainForFile(
            EndUserFileInfo fileInfo,
            AveDiscoverSite discoverSite,
            IAveSite site)
        {
            mLog.Info("BuildChainForFile started.");
            ArchiverNodeItem webNode = EnsureWebNodeWrapper(fileInfo.WebId, discoverSite);
            IAveFile aveFile = GetFileWrapper(fileInfo, webNode);
            IAveList parentList = aveFile.ParentList;
            ArchiverNodeItem listNode = EnsureListNodeWrapper(webNode, parentList, discoverSite);
            ArchiverNodeItem folderNode = EnsureFolderChainWrapper(listNode, parentList, aveFile.ParentFolder, site, webNode?.DiscoverSPObject as AveDiscoverWeb);
            CreateItemNode(folderNode, listNode, aveFile, parentList, site, discoverSite, fileInfo);
            mLog.Info("BuildChainForFile finished.");
        }

        private ArchiverNodeItem CreateExceptionItemNode(EndUserFileInfo fileInfo, JobDetailsStatus status, string exceptionMessage)
        {
            return new ArchiverNodeItem()
            {
                FullPath = fileInfo.FullPath,
                Name = Path.GetFileName(fileInfo.FullPath),
                Status = status,
                WebId = fileInfo.WebId,
                ExceptionMessage = I18NEntity.HasKey(exceptionMessage) ? exceptionMessage : "",
                Cache_NodeType = (int)CacheNodeType.Item,
                SPNodeLevel = NodeLevel.Item,
                RuleName = "N/A",
            };
        }

        private void AttachToNearestParent(ArchiverNodeItem node)
        {
            if (node == null)
            {
                return;
            }

            if (node.ID == Guid.Empty)
            {
                node.ID = Guid.NewGuid();
            }

            ArchiverNodeItem parent = FindNearestParent(root, node);
            AttachChildNode(parent ?? root, node);
            node.Parent = parent;
        }

        private ArchiverNodeItem FindNearestParent(ArchiverNodeItem current, ArchiverNodeItem target)
        {
            foreach (var child in current.Children.Values)
            {
                if (child.SPNodeLevel < NodeLevel.Item && IsParentPath(child.FullPath, target.FullPath))
                {
                    return FindNearestParent(child, target);
                }
            }
            return current;
        }

        #endregion

        #region Node Retrieval Wrappers

        private IAveFile GetFileWrapper(EndUserFileInfo fileInfo, ArchiverNodeItem webNode)
        {
            try
            {
                var file = GetFile(fileInfo, webNode);
                if (file == null || !file.Exists)
                {
                    mLog.Warn($"Failed to resolve file '{fileInfo.GetDecodedFullPath()}'. File is null.");
                    throw new Exception($"StorageOptimization_SOARRecordManagerFileNotExist");
                }
                if (file.Item.ID != fileInfo.Id)
                {
                    mLog.Error($"GetFileWrapper: File ID mismatch for '{fileInfo.GetDecodedFullPath()}'. Expected ID: {fileInfo.Id}, Actual ID: {file.Item.ID}.");
                    throw new Exception($"StorageOptimization_EndUserArchive_ItemIdException{I18NEntity.Separator}{file.Item.ID}{I18NEntity.Separator}{fileInfo.Id}");
                }
                return file;
            }
            catch (Exception ex)
            {
                mLog.Warn($"Failed to resolve file '{fileInfo.GetDecodedFullPath()}'. {ex}");
                throw new Exception($"Failed to resolve file '{fileInfo.GetDecodedFullPath()}'.", ex);
            }
        }

        private ArchiverNodeItem EnsureWebNodeWrapper(Guid webId, AveDiscoverSite discoverSite)
        {
            try
            {
                var webNode = EnsureWebNode(webId, discoverSite);
                if (webNode == null)
                {
                    mLog.Warn($"Failed to resolve web node '{webId}'. Node is null.");
                    throw new Exception($"Failed to resolve web node '{webId}'. Node is null.");
                }
                if (webNode.Status != JobDetailsStatus.Successful)
                {
                    mLog.Error($"EnsureWebNodeWrapper: Status={webNode.Status}, ExceptionMessage={webNode.ExceptionMessage}");
                    throw new Exception($"Failed to resolve web node '{webId}'. Status: {webNode.Status}. Message: {webNode.ExceptionMessage}");
                }
                return webNode;
            }
            catch (Exception ex)
            {
                mLog.Warn($"Failed to resolve web node '{webId}'. {ex}");
                mExceptionWebIds.Add(webId);
                throw new Exception($"Failed to resolve web node '{webId}'.", ex);
            }
        }

        private ArchiverNodeItem EnsureListNodeWrapper(
            ArchiverNodeItem webNode,
            IAveList spList,
            AveDiscoverSite discoverSite)
        {
            try
            {
                var listNode = EnsureListNode(webNode, spList, discoverSite);
                if (listNode == null)
                {
                    mLog.Warn($"Failed to resolve list node '{spList.Title}'. Node is null.");
                    throw new Exception($"Failed to resolve list node '{spList.Title}'. Node is null.");
                }
                if (listNode.Status == JobDetailsStatus.Skipped)
                {
                    throw new Exception($"List '{spList.Title}' is skipped due to job settings.");
                }
                else if (listNode.Status != JobDetailsStatus.Successful)
                {
                    mLog.Error($"EnsureListNodeWrapper: Status={listNode.Status}, ExceptionMessage={listNode.ExceptionMessage}");
                    throw new Exception($"Failed to resolve list node '{spList.Title}'. Status: {listNode.Status}. Message: {listNode.ExceptionMessage}");
                }
                return listNode;
            }
            catch (Exception ex)
            {
                mLog.Warn($"Failed to resolve list node '{spList.Title}'. {ex}");
                throw new Exception($"Failed to resolve list node '{spList.Title}'.", ex);
            }
        }

        private ArchiverNodeItem EnsureFolderChainWrapper(
            ArchiverNodeItem listNode,
            IAveList spList,
            IAveFolder targetFolder,
            IAveSite site,
            AveDiscoverWeb discoverWeb)
        {
            try
            {
                var folderNode = EnsureFolderChain(listNode, spList, targetFolder, site, discoverWeb);
                if (folderNode == null)
                {
                    mLog.Warn($"Failed to resolve folder chain for '{targetFolder?.ServerRelativeUrl}'. Node is null.");
                    throw new Exception($"Failed to resolve folder chain for '{targetFolder?.ServerRelativeUrl}'. Node is null.");
                }
                if (folderNode.Status != JobDetailsStatus.Successful)
                {
                    mLog.Error($"EnsureFolderChainWrapper: Status={folderNode.Status}, ExceptionMessage={folderNode.ExceptionMessage}");
                    throw new Exception($"Failed to resolve folder chain for '{targetFolder?.ServerRelativeUrl}'. Status: {folderNode.Status}. Message: {folderNode.ExceptionMessage}");
                }
                return folderNode;
            }
            catch (Exception ex)
            {
                mLog.Warn($"Failed to resolve folder chain for '{targetFolder?.ServerRelativeUrl}'. {ex}");
                throw new Exception($"Failed to resolve folder chain for '{targetFolder?.ServerRelativeUrl}'.", ex);
            }
        }

        #endregion

        #region Core Node Retrieval

        private IAveFile GetFile(EndUserFileInfo fileInfo, ArchiverNodeItem webNode)
        {
            AveDiscoverWeb discoverWeb = webNode?.DiscoverSPObject as AveDiscoverWeb;
            IAveFile aveFile;
            try
            {
                aveFile = discoverWeb.AveWeb.GetFileByFullPath(fileInfo.GetDecodedFullPath());
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load file '{fileInfo.GetDecodedFullPath()}' from SharePoint. {ex}");
            }
            return aveFile;
        }

        private ArchiverNodeItem EnsureWebNode(Guid webId, AveDiscoverSite discoverSite)
        {
            if (mExceptionWebIds.Contains(webId))
            {
                throw new Exception($"web {webId} exist in exception web ids");
            }

            if (mWebCache.TryGetValue(webId, out ArchiverNodeItem cached))
            {
                return cached;
            }

            AveDiscoverWeb discoverWeb;
            try
            {
                discoverWeb = discoverSite.GetWeb(webId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to resolve web '{webId}'. {ex}");
            }

            ArchiverNodeItem parentNode = discoverWeb.AveWeb.IsRootWeb
                ? root
                : EnsureWebNode(discoverWeb.AveWeb.ParentWebId, discoverSite) ?? root;

            ArchiverNodeItem webNode = parentNode.GenerateSiteNodeItem(discoverWeb, mConfiguration, discoverWeb.AveWeb.IsRootWeb);
            webNode.DiscoverSPObject = discoverWeb;
            AttachChildNode(parentNode, webNode);
            mWebCache[webId] = webNode;
            return webNode;
        }

        private ArchiverNodeItem EnsureListNode(
            ArchiverNodeItem webNode,
            IAveList spList,
            AveDiscoverSite discoverSite)
        {
            if (mListCache.TryGetValue(spList.ID, out ArchiverNodeItem cached))
            {
                return cached;
            }            

            AveDiscoverList discoverList = discoverSite.GetDiscoverList(spList.ParentWeb.Site, spList.ParentWeb, spList.RootFolder.ServerRelativeUrl);
            ArchiverNodeItem listNode = webNode.GenerateListNodeItem(discoverList, spList);
            listNode.ListId = spList.ID;
            listNode.WebId = webNode.WebId;
            listNode.SPList = spList;
            AttachChildNode(webNode, listNode);
            mListCache[spList.ID] = listNode;
            if (ListSkipCheck(listNode))
            {
                listNode.Status = JobDetailsStatus.Skipped;
                listNode.ExceptionMessage = "StorageOptimization_SOARSOArchiveIsSystemObject";
            }
            return listNode;
        }

        private ArchiverNodeItem EnsureFolderChain(
            ArchiverNodeItem listNode,
            IAveList spList,
            IAveFolder targetFolder,
            IAveSite site,
            AveDiscoverWeb discoverWeb)
        {
            string rootFolderUrl = spList.RootFolder.ServerRelativeUrl.TrimEnd('/');
            ArchiverNodeItem current = EnsureFolderNode(listNode, spList, site, discoverWeb, rootFolderUrl, NodeLevel.RootFolder);
            string currentPath = rootFolderUrl;

            if (targetFolder == null)
            {
                return current;
            }

            string targetUrl = targetFolder.ServerRelativeUrl.TrimEnd('/');
            if (!targetUrl.StartsWith(rootFolderUrl, StringComparison.OrdinalIgnoreCase))
            {
                targetUrl = rootFolderUrl;
            }

            foreach (string segment in GetFolderSegments(rootFolderUrl, targetUrl))
            {
                if (string.IsNullOrEmpty(segment))
                {
                    throw new InvalidOperationException("Invalid folder segment encountered during folder chain construction.");
                }

                currentPath = ($"{currentPath}/{segment}").Replace("//", "/");
                current = EnsureFolderNode(current, spList, site, discoverWeb, currentPath, NodeLevel.Folder);
            }

            return current;
        }

        private ArchiverNodeItem EnsureFolderNode(
            ArchiverNodeItem parentNode,
            IAveList spList,
            IAveSite site,
            AveDiscoverWeb discoverWeb,
            string folderServerRelativeUrl,
            NodeLevel level)
        {
            folderServerRelativeUrl = folderServerRelativeUrl.TrimEnd('/');
            string cacheKey = $"{spList.ID:D}:{folderServerRelativeUrl}";
            if (mFolderCache.TryGetValue(cacheKey, out ArchiverNodeItem cached))
            {
                return cached;
            }

            var discoverFolder = new AveDiscoverFolder(site, discoverWeb.WebID, folderServerRelativeUrl, DiscoverModule.Archive, mConfiguration.aveObjectModelFactory, spList.ID, discoverWeb.AveWeb);
            ArchiverNodeItem folderNode = parentNode.GenerateFolderNodeItem(discoverFolder, level, site.Url, mConfiguration);
            AttachChildNode(parentNode, folderNode);
            mFolderCache[cacheKey] = folderNode;
            return folderNode;
        }

        private void CreateItemNode(
            ArchiverNodeItem folderNode,
            ArchiverNodeItem listNode,
            IAveFile aveFile,
            IAveList parentList,
            IAveSite site,
            AveDiscoverSite discoverSite,
            EndUserFileInfo fileInfo)
        {

            AveDiscoverItem discoverItem = ResolveDiscoverItem(folderNode, listNode, aveFile, parentList, discoverSite, fileInfo);

            ArchiverNodeItem itemNode = new ArchiverNodeItem
            {
                ID = aveFile.UniqueId,
                ItemId = aveFile.UniqueId,
                FolderId = folderNode.ID,
                Name = aveFile.Name,
                FullPath = aveFile.ServerRelativeUrl,
                Parent = folderNode,
                SPNodeLevel = NodeLevel.Item,
                Cache_NodeType = (int)CacheNodeType.Item,
                ItemType = parentList?.BaseTemplate == AveListTemplateType.DocumentLibrary
                    ? ArchiverCommon.ItemType.DOCUMENT
                    : ArchiverCommon.ItemType.ITEM_TYPE,
                DiscoverSPObject = discoverItem,
                SiteUrl = site.Url,
                SiteId = site.ID,
                WebId = folderNode.WebId,
                ListId = listNode.ID,
                WebApplicationId = site.WebApplication?.ID ?? Guid.Empty,
                DocumentSize = aveFile.Length,
                Created = aveFile.TimeCreated.Ticks,
                Modified = aveFile.TimeLastModified.Ticks,
                Author = aveFile.Author?.LoginName,
                Editor = aveFile.ModifiedBy?.LoginName,
                LibRowID = discoverItem?.ID ?? aveFile.Item?.ID ?? -1,
                DoDelete = true
            };

            if (discoverItem == null)
            {
                mLog.Warn($"Skip file '{aveFile.ServerRelativeUrl}' because discovery metadata cannot be resolved.");
                itemNode.Status = JobDetailsStatus.Exception;
                itemNode.ExceptionMessage = "";
            }
            else if (LinkFileCommon.StubFileNameSuffixList.Contains(System.IO.Path.GetExtension(discoverItem.LeafName)) && discoverItem.CurrentItem != null
            && discoverItem.CurrentItem.FieldValues.ContainsKey(LinkFileCommon.LinkFileFieldName)
            && discoverItem.CurrentItem.FieldValues[LinkFileCommon.LinkFileFieldName] != null
            && discoverItem.CurrentItem.FieldValues[LinkFileCommon.LinkFileFieldName].ToString().Length > 0)
            {
                mLog.Warn($"Skip file '{aveFile.ServerRelativeUrl}' because it is stub file");
                itemNode.Status = JobDetailsStatus.Skipped;
                itemNode.ExceptionMessage = "StorageOptimization_EndUserArchive_SkipStubFile";
            }
            else
            {
                discoverItem.Length = aveFile.Length;
            }
            AttachChildNode(folderNode, itemNode);
        }

        private AveDiscoverItem ResolveDiscoverItem(
            ArchiverNodeItem folderNode,
            ArchiverNodeItem listNode,
            IAveFile aveFile,
            IAveList parentList,
            AveDiscoverSite discoverSite,
            EndUserFileInfo fileInfo)
        {
            string dirName = NormalizeItemDirectory(folderNode.FullPath, aveFile.ServerRelativeUrl);
            string leafName = aveFile.Name;
            bool isListItem = parentList?.BaseTemplate != AveListTemplateType.DocumentLibrary;
            try
            {
                AveDiscoverItem discoverItem = discoverSite.GetItemExist(fileInfo.WebId, listNode.ID, aveFile.UniqueId, dirName, leafName, isListItem, folderNode?.DiscoverSPObject as AveDiscoverFolder);
                if (discoverItem != null)
                {
                    return discoverItem;
                }
            }
            catch (Exception ex)
            {
                mLog.Warn($"GetItemExist failed for '{aveFile.ServerRelativeUrl}'. {ex}");
            }

            try
            {
                return (folderNode?.DiscoverSPObject as AveDiscoverFolder).GetItemsWithoutCache()
                    ?.FirstOrDefault(item => item.DocID == aveFile.UniqueId
                        || string.Equals(item.FullUrl, aveFile.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                mLog.Warn($"Fallback discovery lookup failed for '{aveFile.ServerRelativeUrl}'. {ex}");
            }

            return null;
        }

        #endregion

        #region Utility Methods

        private static string NormalizeItemDirectory(string folderPath, string filePath)
        {
            if (!string.IsNullOrWhiteSpace(folderPath))
            {
                return folderPath.TrimEnd('/');
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                return string.Empty;
            }

            int lastSlash = filePath.LastIndexOf('/');
            if (lastSlash <= 0)
            {
                return "/";
            }

            return filePath.Substring(0, lastSlash);
        }

        private static void AttachChildNode(ArchiverNodeItem parent, ArchiverNodeItem child)
        {
            if (parent == null || child == null)
            {
                return;
            }

            if (parent.Children.ContainsKey(child.ID))
            {
                parent.Children[child.ID] = child;
            }
            else
            {
                parent.Children.Add(child.ID, child);
            }
        }

        private static IEnumerable<string> GetFolderSegments(string rootFolderUrl, string targetFolderUrl)
        {
            if (string.IsNullOrEmpty(rootFolderUrl) || string.IsNullOrEmpty(targetFolderUrl))
            {
                yield break;
            }

            string normalizedRoot = rootFolderUrl.Trim('/');
            string normalizedTarget = targetFolderUrl.Trim('/');

            if (!normalizedTarget.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
            {
                yield break;
            }

            string relative = normalizedTarget.Length == normalizedRoot.Length
                ? string.Empty
                : normalizedTarget.Substring(normalizedRoot.Length).Trim('/');

            if (string.IsNullOrEmpty(relative))
            {
                yield break;
            }

            foreach (string segment in relative.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
            {
                yield return segment;
            }
        }

        private bool IsParentPath(string parentPath, string childPath)
        {
            parentPath = parentPath.TrimEnd('/').ToLowerInvariant() + "/";
            childPath = childPath.TrimEnd('/').ToLowerInvariant() + "/";
            return childPath.StartsWith(parentPath, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

    }
}
