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




namespace AvePoint.Media.Service.ArchiverBackup
{
    #region using directives

    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.Media.Common;
    using Merged18NResources.MediaServiceArchiverBackup;
    using AvePoint.Media.Service.DomainModel;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Storage;
    using AvePoint.GCommon.Contract.CommonFilter;
    using AvePoint.GCommon.Contract.Server.Common;
    using AvePoint.Media.Core.Index;
    using AvePoint.RA.Common;
    using Newtonsoft.Json;
    using AvePoint.RA.CommonUtil;
    using Microsoft.SharePoint.Client;
    using AvePoint.RA.DB.Model;
    using global::Media.Common.ClassicStorageApi;
    using AvePoint.GCommon.Utility;
    using AvePoint.GCommon.Utility.Exceptions;
    using global::Media.Service.DomainModel;
    using AvePoint.RA.Contract.RMWeb;
    using System.Xml;
    using AvePoint.RA.Contract.RMWeb.CP;
    using global::Media.Service.ArchiverBackup.Index.IndexService.FunctionIndexServiceIntention;
    using global::Media.Service.ArchiverBackup.Index.IndexService.FunctionIndexServiceIntentionImpl;
    using global::Media.Service.ArchiverBackup.Index.IndexService.TableIndexServiceIntentionImpl;
    using global::Media.Service.ArchiverBackup.Index;
    using AvePoint.GCommon.Contract.Media.Object;
    using System.Diagnostics;

    #endregion using directives

    public class ArchiverAdvancedSearchService
        : AdvancedSearchServiceBase<ArchiverAdvancedSearchInfo, ArchiverAdvancedSearchResult>
        , IAdvancedSearchService
    {
        RALogger logger = RALogger.GetInstance(typeof(ArchiverAdvancedSearchService));
        IXSystem indexLogicalDevice;
        //public IIndexService<ArchiverIndexServiceOpenParameter> IndexService { get; set; }
        public ArchiverIndexService _IndexService { get; set; }
        public IIndexService<ArchiverIndexServiceOpenParameter> IndexService
        {
            get
            {
                if (_IndexService == null)
                {
                    _IndexService = new ArchiverIndexService()
                    {
                        IndexProcessor = new IndexProcessor<ArchiverIndexProcessorParameter>(),
                        IndexSynchronizer = new IndexDatabaseSynchronizer()
                    };
                    return _IndexService;
                }
                else
                {
                    return _IndexService;
                }
            }
            set { }
        }
        private GDriveArchiverIndexService _indexGoogleService { get; set; }
        public IIndexService<GDriveIndexServiceOpenParameter> IndexGoogleService
        {
            get
            {
                if (_indexGoogleService == null)
                {
                    _indexGoogleService = new GDriveArchiverIndexService()
                    {
                        IndexProcessor = new IndexProcessor<GDriveArchiverIndexProcessorParameter>(),
                        IndexSynchronizer = new IndexDatabaseSynchronizer()
                    };
                    return _indexGoogleService;
                }
                else
                {
                    return _indexGoogleService;
                }
            }
            set { }
        }
        //public IArchiverAdvancedSearchIndexService AdvanceSearchIndexService { get; set; }
        public IArchiverAdvancedSearchIndexService _AdvanceSearchIndexService { get; set; }
        public IArchiverAdvancedSearchIndexService AdvanceSearchIndexService
        {
            get
            {
                if (_AdvanceSearchIndexService == null)
                {
                    _AdvanceSearchIndexService = new ArchiverAdvancedSearchIndexService() { HeadAndBodyService = new ArchiverHeadAndBodyIndexService() { IndexProcessor = _IndexService.IndexProcessor } };
                    return _AdvanceSearchIndexService;
                }
                else
                {
                    return _AdvanceSearchIndexService;
                }
            }
            set { }
        }
        private IGDriveArchiverAdvancedSearchIndexService _gDriveArchiverAdvancedSearchIndexService { get; set; }
        public IGDriveArchiverAdvancedSearchIndexService GDriveArchiverAdvancedSearchIndexService
        {
            get
            {
                return _gDriveArchiverAdvancedSearchIndexService ??= new GDriveArchiverAdvancedSearchIndexService
                {
                    HeadAndBodyService = new GDriveArchiverHeadAndBodyIndexService
                    {
                        IndexProcessor = _indexGoogleService.IndexProcessor
                    }
                };
            }
            set { }
        }

        //public IArchiverAdvancedSearchIndexService AdvanceSearchIndexService => PlatformWindsorManager.GetService<IArchiverAdvancedSearchIndexService>();

        public override List<TreeNode> Search(ArchiverAdvancedSearchInfo searchInfo, ArchiverRestoreOrderBy orderBy)
        {
            logger.Info($"ArchiverAdvancedSearchService.Search.NodeInfos count:{searchInfo.NodeInfos.Count}.");
            foreach (var node in searchInfo.NodeInfos)
            {
                try
                {
                    ListModeResult = new List<TreeNode>();
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    Open(node.BrowseInfo, searchInfo.OpenIndexDbTimeoutInMs);
                    sw.Stop();
                    logger.Info($"ArchiverAdvancedSearchService.Search.OpenIndexDbTimeoutInMs:OpenTime:{sw.ElapsedMilliseconds}ms");
                    return SearchSiteCollection(searchInfo, node, orderBy);
                }
                catch (Exception ex)
                {
                    logger.Error(@"Looks up a localized string similar to An error occurred while searching site collection {0}:{1}..", node.BrowseInfo.SiteUrl, ex.ToString());
                    if(ex is OpenIndexDbTimeoutException)
                    {
                        throw;
                    }
                }
                finally
                {
                    Close();
                }
            }
            return null;
        }
        public List<ArchiverBasicIndex> SearchForFS(ArchiverAdvancedSearchInfo searchInfo, ArchiverRestoreOrderBy orderBy)
        {
            logger.Info($"ArchiverAdvancedSearchService.Search.NodeInfos count:{searchInfo.NodeInfos.Count}.");
            foreach (var node in searchInfo.NodeInfos)
            {
                try
                {
                    ListModeResult = new List<TreeNode>();
                    Open(node.BrowseInfo, searchInfo.OpenIndexDbTimeoutInMs);
                    return SearchConnection(searchInfo, node, orderBy);
                }
                catch (Exception ex)
                {
                    logger.Error(@"Looks up a localized string similar to An error occurred while searching site collection {0}:{1}..", node.BrowseInfo.SiteUrl, ex.ToString());
                    if (ex is OpenIndexDbTimeoutException)
                    {
                        throw;
                    }
                }
                finally
                {
                    Close();
                }
            }
            return null;
        }
        public List<TreeNode> SearchForGoogle(GDriveArchiverAdvancedSearchInfo searchInfo, ArchiverRestoreOrderBy orderBy)
        {
            logger.Info($"ArchiverAdvancedSearchService.Search.NodeInfos count:{searchInfo.NodeInfos.Count}.");
            foreach (var node in searchInfo.NodeInfos)
            {
                try
                {
                    ListModeResult = new List<TreeNode>();
                    Open(node.BrowseInfo, searchInfo.OpenIndexDbTimeoutInMs);
                    return SearchGoogle(searchInfo, node, orderBy);
                }
                catch (Exception ex)
                {
                    logger.Error(@"Looks up a localized string similar to An error occurred while searching site collection {0}:{1}..", node.BrowseInfo.DriveName, ex.ToString());
                    if (ex is OpenIndexDbTimeoutException)
                    {
                        throw;
                    }
                }
                finally
                {
                    Close();
                }
            }
            return null;
        }
        public List<ArchiverBasicIndex> SearchForExport(ArchiverAdvancedSearchInfo searchInfo)
        {
            foreach (var node in searchInfo.NodeInfos)
            {
                try
                {
                    ListModeResult = new List<TreeNode>();
                    Open(node.BrowseInfo, searchInfo.OpenIndexDbTimeoutInMs);
                    List<ArchiverBasicIndex> indexes = this.AdvanceSearchIndexService.Search(searchInfo.FilterInfors, node.BrowseInfo, null, out _);
                    return indexes;
                }
                catch (Exception ex)
                {
                    logger.Error(@"Looks up a localized string similar to An error occurred while searching site collection {0}:{1}..", node.BrowseInfo.SiteUrl, ex.ToString());
                    throw;
                }
                finally
                {
                    Close();
                }
            }
            return null;
        }
        public List<RMArchiveSiteInfo> SearchForJob(ArchiverAdvancedSearchInfo searchInfo)
        {
            var resultList = new List<RMArchiveSiteInfo>();
            foreach (var node in searchInfo.NodeInfos)
            {
                try
                {                 
                    Open(node.BrowseInfo);
                    DashBoardInfo dashBoardInfo =  this.AdvanceSearchIndexService.SearchForJobV2();
                    //var indexs = SearchSiteCollectionForJob(searchInfo, node);
                    //var versionIntCount = indexs.Where(index => index.Name.Contains(':')).Count();
                    //var versionNumber = (double)indexs.Where(index => index.Name.Contains(':')).Count() / 1000;
                    //var fileNumber = (double)(indexs.Count - versionIntCount) / 1000;
                    var result = new RMArchiveSiteInfo() { 
                        Id = Guid.NewGuid().ToString(),
                        SiteId = node.SiteId, 
                        SiteUrl = node.BrowseInfo.SiteUrl, 
                        FileNumber = dashBoardInfo.FileNumber, 
                        VersionNumber = dashBoardInfo.VersionNumber
                    };
                    resultList.Add(result);
                }
                catch (Exception ex)
                {
                    logger.Error(@"Looks up a localized string similar to An error occurred while searching site collection {0}:{1}..", node.BrowseInfo.SiteUrl, ex.ToString());
                }
                finally
                {
                    Close();
                    DeleteFile(node.BrowseInfo);
                }
            }
            return resultList;
        }
        public List<RMArchiveGDriveInfo> SearchForGoogleJob(GDriveArchiverAdvancedSearchInfo searchInfo)
        {
            var resultList = new List<RMArchiveGDriveInfo>();
            foreach (var node in searchInfo.NodeInfos)
            {
                try
                {
                    Open(node.BrowseInfo);
                    DashBoardInfo dashBoardInfo = this.GDriveArchiverAdvancedSearchIndexService.SearchForJob();
                    var result = new RMArchiveGDriveInfo()
                    {
                        Id = Guid.NewGuid().ToString(),
                        DriveId = node.SiteId,
                        DriveName = node.BrowseInfo.DriveName,
                        FileNumber = dashBoardInfo.FileNumber,
                        VersionNumber = dashBoardInfo.VersionNumber,
                        TenantId = node.BrowseInfo.TenantGroupId,
                    };
                    resultList.Add(result);
                }
                catch (Exception ex)
                {
                    logger.Error(@"Looks up a localized string similar to An error occurred while searching site collection {0}:{1}..", node.BrowseInfo.SiteUrl, ex.ToString());
                }
                finally
                {
                    CloseIndexGoogleService();
                    DeleteFile(node.BrowseInfo);
                }
            }
            return resultList;
        }
        public async IAsyncEnumerable<ArchiverBasicIndex> SearchForExportItems(ArchiverBrowseInfo info, RA.Contract.Archiver.TimeRange timeRange)
        {
            Open(info);
            try
            {
                IAsyncEnumerable<ArchiverBasicIndex> stream = timeRange switch
                {
                    RA.Contract.Archiver.TimeRange.All => this.AdvanceSearchIndexService.SearchForExportAllItemAsync(),
                    RA.Contract.Archiver.TimeRange.Custom => this.AdvanceSearchIndexService.SearchForExportAllItemOnSpecificTimeRangeAsync(info),
                    _ => AsyncEnumerable.Empty<ArchiverBasicIndex>()
                };

                await foreach (var item in stream)
                {
                    yield return item;
                }
            }
            finally
            {
                Close();
            }
        }
        public List<GoogleBasicIndex> SearchForGDriveExportItems(GDriveBrowseInfo info, RA.Contract.Archiver.TimeRange timeRange)
        {
            List<GoogleBasicIndex> result = new List<GoogleBasicIndex>();
            Open(info);
            switch (timeRange)
            {
                case RA.Contract.Archiver.TimeRange.All:
                    result = this.GDriveArchiverAdvancedSearchIndexService.SearchForExportAllItem();
                    break;
                case RA.Contract.Archiver.TimeRange.Custom:
                    result = this.GDriveArchiverAdvancedSearchIndexService.SearchForExportAllItemOnSpecificTimeRange(info);
                    break;
                case RA.Contract.Archiver.TimeRange.None:
                default:
                    logger.Error(@$"No search method was executed, info:{info},timeRange:{timeRange} ");
                    break;
            }
            return result;
        }

        public long SearchArchivedSizeForExportSubSites(ArchiverBrowseInfo info, string subSiteUrl, RA.Contract.Archiver.TimeRange timeRange)
        {
            Open(info);
            switch (timeRange)
            {
                case RA.Contract.Archiver.TimeRange.All:
                    return this.AdvanceSearchIndexService.SearchArchivedSizeForExportSubSite(subSiteUrl, null);
                case RA.Contract.Archiver.TimeRange.Custom:
                    return this.AdvanceSearchIndexService.SearchArchivedSizeForExportSubSite(subSiteUrl, info);
                case RA.Contract.Archiver.TimeRange.None:
                default:
                    logger.Error(@$"No search method was executed, info:{info},timeRange:{timeRange} ");
                    break;
            }
            return 0;
        }

        public List<ArchiverBasicIndex> SearchSubSiteForExportSubSites(ArchiverBrowseInfo info, RA.Contract.Archiver.TimeRange timeRange)
        {
            Open(info);
            switch (timeRange)
            {
                case RA.Contract.Archiver.TimeRange.All:
                    return this.AdvanceSearchIndexService.SearchSubsitesForExportAllSubSites(null);
                case RA.Contract.Archiver.TimeRange.Custom:
                    return this.AdvanceSearchIndexService.SearchSubsitesForExportAllSubSites(info);
                case RA.Contract.Archiver.TimeRange.None:
                default:
                    logger.Error(@$"No search method was executed, info:{info},timeRange:{timeRange} ");
                    break;
            }
            return new List<ArchiverBasicIndex>();
        }

        public override void ProcessException(Exception e)
        {
            this.logger.Error(MediaServiceArchiverBackupResource.ArchiverAdvancedSearchServiceProcessExceptionArchiverAdvancedSearchException, e.ToString());
        }

        /// <summary>
        /// 将每个site collection分开search，防止互相影响
        /// </summary>
        /// <param name="filterPolicy">过滤条件</param>
        /// <param name="resultNodes">结果</param>
        /// <param name="nodeInfo">结点信息</param>
        private List<TreeNode> SearchSiteCollection(ArchiverAdvancedSearchInfo searchInfo, ArchiverSearchNodeInfo nodeInfo, ArchiverRestoreOrderBy orderBy)
        {
            //logger.Info(MediaServiceArchiverBackupResource.ArchiverAdvancedSearchServiceSearchSiteCollectionBegin, nodeInfo.BrowseInfo.Path);
            List<TreeNode> nodeChildren = ProcessNodeForTreeMode(nodeInfo.BrowseInfo, searchInfo.FilterInfors, orderBy);
            return nodeChildren;
        }
        private List<ArchiverBasicIndex> SearchConnection(ArchiverAdvancedSearchInfo searchInfo, ArchiverSearchNodeInfo nodeInfo, ArchiverRestoreOrderBy orderBy)
        {
            List<ArchiverBasicIndex> indexes = new List<ArchiverBasicIndex>();
            string rootNodeName = GetRootNodeName(nodeInfo.BrowseInfo);

            List<TreeNode> list = new List<TreeNode>();
            nodeInfo.BrowseInfo.Path = rootNodeName;
            indexes = this.AdvanceSearchIndexService.SearchForFS(searchInfo.FilterInfors, nodeInfo.BrowseInfo, orderBy);
            return indexes;
        }
        private List<TreeNode> SearchGoogle(GDriveArchiverAdvancedSearchInfo searchInfo, GDriveArchiverSearchNodeInfo nodeInfo, ArchiverRestoreOrderBy orderBy)
        {
            var searchNode = nodeInfo.BrowseInfo;
            var list = new List<TreeNode>();
            //searchNode.Path = searchNode.Path;
            var indexes = this.GDriveArchiverAdvancedSearchIndexService.SearchForGoogle(searchInfo.FilterInfors, searchNode, orderBy);
            logger.Info($"search indexes result count is {indexes.Count}");

            foreach (var index in indexes)
            {
                if (searchInfo.FilterInfors.PathMD5List != null && searchInfo.FilterInfors.PathMD5List.Count > 0)
                {
                    if (!searchInfo.FilterInfors.PathMD5List.Contains(index.PathMD5))
                    {
                        continue;
                    }
                }
                if (searchNode.Path.Equals(index.Name, StringComparison.OrdinalIgnoreCase)) { continue; }
                list.Add(AssembleGoogleBranch(index, searchNode.DriveId));
            }
            return list;
        }
        /// <summary>
        /// 对选中的结点进行判断，然后走不同的处理方法
        /// </summary>
        /// <param name="searchNode">用户选中的结点的信息</param>
        /// <param name="filterPolicy">用户设置的所有过滤条件</param>
        /// <returns>用户选中结点的children</returns>
        private List<TreeNode> ProcessNodeForTreeMode(ArchiverBrowseInfo searchNode, ArchiverRestoreFilter filters, ArchiverRestoreOrderBy orderBy)
        {
            List<TreeNode> resultChildren = new List<TreeNode>();
            var resultBranches = GetBranchesByFilters(searchNode, filters, orderBy);
            return resultBranches;
        }        
        /// <summary>
        /// 核心方法，对低于SiteCollection级别的结点下的数据，根据用户设置的
        /// 过滤条件（不包含SiteCollection级别的）做search，并组装branches
        /// </summary>
        /// <param name="searchNode">用户选中的结点的信息</param>
        /// <param name="filterPolicy">用户输入的过滤条件</param>
        /// <param name="siteUrl">search的SiteCollection的url</param>
        /// <returns>key是用户设置的filter的序号，value是符合过滤条件的branches</returns>
        private List<TreeNode> GetBranchesByFilters(ArchiverBrowseInfo searchNode, ArchiverRestoreFilter filter, ArchiverRestoreOrderBy orderBy)
        {
            List<ArchiverBasicIndex> indexes = new List<ArchiverBasicIndex>();
            string rootNodeName = GetRootNodeName(searchNode);
            //string rootSiteTitle = GetRootSiteTitle(searchNode);
            //List<TreeNode> list = new List<TreeNode>();
            //searchNode.Path = rootNodeName;
            //indexes = this.AdvanceSearchIndexService.Search(filter, searchNode);
            //return indexes;

            List<TreeNode> list = new List<TreeNode>();
            searchNode.Path = rootNodeName;
            indexes = this.AdvanceSearchIndexService.Search(filter, searchNode, orderBy, out long totalCount);
            logger.Info($"serch indexes result count is {indexes.Count}");
            List<string> md5 = new List<string>();
            Dictionary<string, List<TreeNode>> temptree = new Dictionary<string, List<TreeNode>>();

            foreach (var index in indexes)
            {
                if (filter.PathMD5List != null && filter.PathMD5List.Count > 0)
                {
                    if (!filter.PathMD5List.Contains(index.PathMD5))
                    {
                        continue;
                    }
                }
                if (index.Type == "E")
                {
                    list.Add(AssembleIndexToDto(index, searchNode.SiteUrl, true));
                    break;
                }
                if (rootNodeName.Equals(index.Name, StringComparison.OrdinalIgnoreCase)) { continue; }
                list.Add(AssembleBranch(index, null, searchNode.SiteUrl, rootNodeName == string.Empty ? searchNode.SiteUrl : rootNodeName));
            }
            if(list.Count > 0)
            {
                list[0].TotalCount = totalCount;
            }
            //}
            return list;
        }

        private String GetRootNodeName(ArchiverBrowseInfo searchNode)
        {
            String result = String.Empty;
            result = searchNode.Path.Contains("\\") ? searchNode.Path.Substring(searchNode.SiteUrl.Length + 1) : string.Empty;
            if (searchNode.Level == TreeNodeLevel.List)
            {
                var position = result.Contains("\\") ? result.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase) : result.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
                var tempName = result.Substring(position + 1);
                var decodeTempName = AveConverter.EncodeSpecialChar(tempName);
                result = result.Substring(0, position + 1) + decodeTempName;
            }
            return result;
        }

        /// <summary>
        /// 通过查找出的符合条件的叶结点，用递归算法，组装整个branch， 一直组装到用户选中结点，
        /// 如果选中结点高于SiteCollection级别，则组装到SiteCollection为止，对于RootSite（“.”）结点有特殊处理
        /// 因为对RootSite做search相当于对SiteCollection做search
        /// </summary>
        /// <param name="index">过滤出来的符合条件的叶节点数据信息</param>
        /// <param name="node">上次递归得到的parent结点</param>
        /// <param name="root">用户选中的结点名</param>
        /// <returns>递归结束后，返回完整的branch</returns>
        private TreeNode AssembleBranch(ArchiverBasicIndex index, TreeNode node, string siteUrl, string root)
        {
            TreeNode resultNode;
            ArchiverBasicIndex parentIndex = this.AdvanceSearchIndexService.GetParentFolder(index);
            TreeNode parentNode;
            if (parentIndex.Name == siteUrl && index.Name != "." && root == ".")
            {
                resultNode = node ?? AssembleIndexToDto(index, siteUrl, true);
                resultNode.Parent = AssembleIndexToDto(parentIndex, siteUrl, true);
            }
            else
            {
                parentNode = AssembleIndexToDto(parentIndex, siteUrl);
                var temp = AssembleIndexToDto(index, siteUrl, true);
                parentNode.Children.Add(node ?? temp);
                resultNode = parentIndex.Name != root ? AssembleBranch(parentIndex, parentNode, siteUrl, root) : parentNode ?? AssembleIndexToDto(index, siteUrl);
            }
            resultNode.Count = int.MaxValue;
            return resultNode;
        }
        private TreeNode AssembleGoogleBranch(GoogleBasicIndex index, string driveId, TreeNode node = null)
        {
            var parentIndex = this.GDriveArchiverAdvancedSearchIndexService.GetParentFolder(index);
            //if (parentIndex == null) return null;
            TreeNode resultNode;
            TreeNode parentNode;
            //if (parentIndex.Type == (int)GDriveDataType.Drive)
            //{
            //    resultNode = node ?? AssembleGoogleIndexToDto(index);
            //    resultNode.Parent = AssembleGoogleIndexToDto(parentIndex, true);
            //}
            //else
            {
                parentNode = AssembleGoogleIndexToDto(parentIndex);
                var temp = AssembleGoogleIndexToDto(index, true);
                parentNode.Children.Add(node ?? temp);
                resultNode = parentIndex.Type == (int)GDriveDataType.MyDrive || parentIndex.Type == (int)GDriveDataType.SharedDrive ? parentNode : AssembleGoogleBranch(parentIndex, driveId, parentNode);
            }
            resultNode.Count = index.PlatFormType;
            return resultNode;
        }



        /// <summary>
        /// 将ArchiverBasicIndex对象转化为TreeNode对象
        /// </summary>
        /// <param name="index">要转化的ArchiverBasicIndex对象</param>
        /// <returns>转化成的TreeNode对象</returns>
        private TreeNode AssembleIndexToDto(ArchiverBasicIndex index, string siteUrl, bool isLeafNode = false)
        {
            TreeNode treeNodeDto = new TreeNode();
            treeNodeDto.TreeNodeLevel = index.Type.ToNodeLevelByMediaDataTypeString().ToString().ToEnum<TreeNodeLevel>();
            if (treeNodeDto.TreeNodeLevel == TreeNodeLevel.List && index.ListType != 0) { treeNodeDto.Type = TreeNodeType.DocumentLibrary; }
            else { treeNodeDto.Type = TreeNodeType.GenericList; }
            int position = index.Name.Contains("\\") ? index.Name.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase) : index.Name.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
            string tempName = treeNodeDto.TreeNodeLevel == TreeNodeLevel.SiteCollection ? index.Name : index.Name.Substring(position + 1);
            treeNodeDto.Name = tempName;
            treeNodeDto.DisplayName = tempName;
            treeNodeDto.FullPath = GetFullPath(index, siteUrl);
            treeNodeDto.FullPathForUI = index.Url;
            treeNodeDto.FarmName = FarmName;
            treeNodeDto.FarmId = FarmId;
            treeNodeDto.ID = Guid.NewGuid().ToString();
            treeNodeDto.Expanded = true;
            treeNodeDto.SitePath = index.SitePath;
            treeNodeDto.ChildrenLoaded = true;
            treeNodeDto.CanChildrenBeLoaded = true;
            treeNodeDto.ModifiedTime = index.ModifyTime;
            treeNodeDto.CreatedTime = index.CreateTime;
            treeNodeDto.ArchivedTime = index.ArchiveTime;
            treeNodeDto.PathMD5 = index.PathMD5;
            treeNodeDto.ParentPathMD5 = index.ParentPathMD5;
            treeNodeDto.Id = index.Id;
            treeNodeDto.NodeGuid = index.NodeGuid;
            treeNodeDto.TypeInIndex = index.Type;
            treeNodeDto.ModifiedBy = index.Editor;
            treeNodeDto.JobId = index.JobId;
            treeNodeDto.Author = index.Author;
            treeNodeDto.IsArchiveTier = index.FlagExtend == (int)Storage.AccessTierType.Archive;
            treeNodeDto.ContentLenth = index.ContentLength;
            treeNodeDto.IsSoftDeleted = index.RetentionStatus == (int)FilterDeletedType.Soft;
            treeNodeDto.BackupTime = index.ArchiveTime;
            if (index.Type.Equals("W", StringComparison.OrdinalIgnoreCase) && !index.Attributes.Equals(String.Empty))
            {
                var tempTitle = index.Attributes.Substring(index.Attributes.IndexOfIgnoreCase("Title:") + 6);
                treeNodeDto.Title = tempTitle.Remove(tempTitle.IndexOf(ServiceConstants.ExtraChar));
            }
            if (index.Attributes.Contains(ServiceConstants.Delimiter.ToString()))
                treeNodeDto.Description = index.Attributes.Replace(ServiceConstants.ExtraChar.ToString(), Environment.NewLine).Replace(ServiceConstants.Delimiter.ToString(), ":");
            else
                treeNodeDto.Description = index.Attributes.Replace(ServiceConstants.ExtraChar.ToString(), Environment.NewLine);
            if (!isLeafNode)
                treeNodeDto.SelectorHidden = true;
            else
                ListModeResult.Add(treeNodeDto);
            return treeNodeDto;
        }
        private TreeNode AssembleGoogleIndexToDto(GoogleBasicIndex index, bool isLeafNode = false)
        {
            TreeNode treeNodeDto = new TreeNode();
            treeNodeDto.TreeNodeLevel = ConvertToNodeLevel(index.Type);
            treeNodeDto.Name = index.Name;
            treeNodeDto.DisplayName = (int)GDriveDataType.MyDrive == index.Type ? index.DriveName : index.Name;
            treeNodeDto.FullPath = index.Path;
            treeNodeDto.FullPathForUI = index.Path;
            treeNodeDto.FarmName = "";
            treeNodeDto.FarmId = "";
            treeNodeDto.ID = (int)GDriveDataType.MyDrive == index.Type ? index.DriveId : index.ItemId;
            treeNodeDto.Expanded = true;
            treeNodeDto.SitePath = index.DriveId;
            treeNodeDto.ChildrenLoaded = true;
            treeNodeDto.CanChildrenBeLoaded = true;
            treeNodeDto.ModifiedTime = index.ModifyTime;
            treeNodeDto.CreatedTime = index.CreateTime;
            treeNodeDto.ArchivedTime = index.ArchiveTime;
            treeNodeDto.PathMD5 = index.PathMD5;
            treeNodeDto.ParentPathMD5 = index.ParentPathMD5;
            treeNodeDto.TypeInIndex = index.Type.ToString();
            treeNodeDto.ModifiedBy = "";
            treeNodeDto.Title = index.Name;
            //treeNodeDto.IsArchiveTier = index.FlagExtend == (int)Storage.AccessTierType.Archive;
            treeNodeDto.ContentLenth = index.ContentLength;
            treeNodeDto.IsSoftDeleted = index.RetentionStatus == (int)FilterDeletedType.Soft;
            //if (index.Attributes.Contains(ServiceConstants.Delimiter.ToString()))
            //    treeNodeDto.Description = index.Attributes.Replace(ServiceConstants.ExtraChar.ToString(), Environment.NewLine).Replace(ServiceConstants.Delimiter.ToString(), ":");
            //else
            //    treeNodeDto.Description = index.Attributes.Replace(ServiceConstants.ExtraChar.ToString(), Environment.NewLine);
            if (!isLeafNode)
                treeNodeDto.SelectorHidden = true;
            else
                ListModeResult.Add(treeNodeDto);
            return treeNodeDto;
        }
        private TreeNodeLevel ConvertToNodeLevel(int nodeType)
        {
            return nodeType switch
            {
                (int)GDriveDataType.MyDrive => TreeNodeLevel.GoogleMyDrive,
                (int)GDriveDataType.SharedDrive => TreeNodeLevel.GoogleSharedDrive,
                (int)GDriveDataType.Folder => TreeNodeLevel.GoogleDriveFolder,
                (int)GDriveDataType.File => TreeNodeLevel.GoogleDriveFile,
                (int)GDriveDataType.FileVersion => TreeNodeLevel.GoogleDriveFile,
                _ => TreeNodeLevel.Undefined,
            };
        }

        /// <summary>
        /// 获取一个结点数据的全路径
        /// </summary>
        /// <param name="index">结点数据</param>
        /// <returns>传入结点的全路径</returns>
        private string GetFullPath(ArchiverBasicIndex index, string siteUrl)
        {
            string fullPath = default(string);
            switch (index.Type)
            {
                case "E":
                    fullPath = index.Name;
                    break;
                case "W":
                case "L":
                case "F":
                    fullPath = siteUrl + "\\" + index.Name;
                    break;
                case "D":
                case "I":
                case "A":
                case "U":
                case "V":
                    string parentPath = this.AdvanceSearchIndexService.GetParentFolder(index).Name;
                    fullPath = siteUrl + "\\" + parentPath + "\\" + index.Name;
                    break;
                default:
                    throw new UnknownFileTypeException(String.Format(MediaServiceArchiverBackupResource.ArchiverAdvancedSearchServiceGetFullPathException, index.Type));
            }
            return fullPath;
        }

        private void Open(ArchiverBrowseInfo browseInfo, int openIndexTimeoutInMs = 0)
        {
            IndexDatabaseHelper.isNoNeedUploadIndex = true;
            CacheManager = PlatformWindsorManager.GetService<ICacheService>();
            indexLogicalDevice = XFactoryCommon.InstanceSystem(browseInfo.IndexLogicalDevice.ConnectionString);
            indexLogicalDevice.Open();
            CacheManager.Open(browseInfo.CacheSetting, indexLogicalDevice.IsDirectSystem);
            var openParam = new ArchiverIndexServiceOpenParameter(browseInfo, CacheManager.CacheSystem, indexLogicalDevice);
            if(openIndexTimeoutInMs > 0)
            {
                openParam.WaitIndexLockerTimeOutInMs = openIndexTimeoutInMs;
            }
            IndexService.Open(openParam);
        }
        private void Open(GDriveBrowseInfo browseInfo, int openIndexTimeoutInMs = 0)
        {
            IndexDatabaseHelper.isNoNeedUploadIndex = true;
            CacheManager = PlatformWindsorManager.GetService<ICacheService>();
            indexLogicalDevice = XFactoryCommon.InstanceSystem(browseInfo.IndexLogicalDevice.ConnectionString);
            indexLogicalDevice.Open();
            CacheManager.Open(browseInfo.CacheSetting, indexLogicalDevice.IsDirectSystem);
            var openParam = new GDriveIndexServiceOpenParameter(browseInfo, CacheManager.CacheSystem, indexLogicalDevice);
            if (openIndexTimeoutInMs > 0)
            {
                openParam.WaitIndexLockerTimeOutInMs = openIndexTimeoutInMs;
            }
            IndexGoogleService.Open(openParam);
        }

        private void DeleteFile(ArchiverBrowseInfo browseInfo)
        {
            IndexDatabaseHelper.isNoNeedUploadIndex = true;
            CacheManager = PlatformWindsorManager.GetService<ICacheService>();
            indexLogicalDevice = XFactory.InstanceSystem(browseInfo.IndexLogicalDevice.ConnectionString);
            indexLogicalDevice.Open();
            CacheManager.Open(browseInfo.CacheSetting, indexLogicalDevice.IsDirectSystem);
            var openParam = new ArchiverIndexServiceOpenParameter(browseInfo, CacheManager.CacheSystem, indexLogicalDevice);
            IndexService.DeleteFileForJob(openParam);
        }
        private void DeleteFile(GDriveBrowseInfo browseInfo)
        {
            IndexDatabaseHelper.isNoNeedUploadIndex = true;
            CacheManager = PlatformWindsorManager.GetService<ICacheService>();
            indexLogicalDevice = XFactory.InstanceSystem(browseInfo.IndexLogicalDevice.ConnectionString);
            indexLogicalDevice.Open();
            CacheManager.Open(browseInfo.CacheSetting, indexLogicalDevice.IsDirectSystem);
            var openParam = new GDriveIndexServiceOpenParameter(browseInfo, CacheManager.CacheSystem, indexLogicalDevice);
            IndexGoogleService.DeleteFileForJob(openParam);
        }

        private void Close()
        {
            try
            {
                IndexService.Close();
                if (indexLogicalDevice != null)
                {
                    indexLogicalDevice.Close();
                }
                CacheManager.Close();
            }
            catch (Exception ex)
            {
                logger.Warn(MediaServiceArchiverBackupResource.ArchiverAdvancedSearchServiceCloseException, ex.ToString());
            }
        }
        private void CloseIndexGoogleService()
        {
            try
            {
                IndexGoogleService.Close();
                if (indexLogicalDevice != null)
                {
                    indexLogicalDevice.Close();
                }
                CacheManager.Close();
            }
            catch (Exception ex)
            {
                logger.Warn(MediaServiceArchiverBackupResource.ArchiverAdvancedSearchServiceCloseException, ex.ToString());
            }
        }

        public override void Dispose() { }
    }
}