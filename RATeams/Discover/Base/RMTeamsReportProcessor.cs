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
using AvePoint.RA.Common.Extension;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RACommonUtility.CAMLHelper.CAML;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.RACommonUtility.Extension;
using AvePoint.RA.RACommonUtility.Model;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client;
using System.Collections.Concurrent;
using System.Net;

namespace RATeams.Discover.Base;

public abstract class RMTeamsReportProcessor
{
    protected readonly RALogger Logger;
    private const string SITES = "Sites";
    private const string LISTS = "Lists";

    protected Guid siteId;

    protected IRMReportManager ReportManager;

    protected readonly BaseJobDto BaseJobDto;

    private List<string> _designLists = [];

    protected string BCSColumnInternalName;

    protected string JobId;

    protected bool JobHasException = false;

    protected bool JobHasStopped = false;

    protected string BCSColumnName = string.Empty;

    protected readonly ITeamsSettingDao _teamsSettingDao = PlatformWindsorManager.GetService<ITeamsSettingDao>();

    protected readonly IRMReportService ReportService = PlatformWindsorManager.GetService<IRMReportService>();

    protected readonly List<NodeItem> SiteCollectionNodeItems = [];

    protected readonly List<RMSPTreeNode> SiteCollectionSPTreeNodeItems = [];

    private IRMSubJobDao _subJobDao = PlatformWindsorManager.GetService<IRMSubJobDao>();

    protected Dictionary<Guid, string> BCSColumnNameDics = new();

    protected ITenantInfoDao TenantInfoDao = PlatformWindsorManager.GetService<ITenantInfoDao>();
    protected readonly ISPSettingTreeService SPTreeService = PlatformWindsorManager.GetService<ISPSettingTreeService>();


    protected ClientContextHelper ClientContextHelper = new ClientContextHelper();
    protected TeamsSettingHelper _teamsSettingHelper = new TeamsSettingHelper();

    private AveObjectModelFactory _factory = null;
    protected IAveSite Site { get; set; }

    protected List<Guid> FitRuleFoldersInDisposalJob = [];

    private ConcurrentDictionary<Guid, int> _termWssidMappingsOfSite;

    private List<TermTreeNode> _groupTermTreeNodes;

    private const int _queryConditionMaxCount = 100;

    private readonly object lockTermWssidMappingsOfSiteObj = new object();
    protected Dictionary<Guid, RMTeamsSetting> mSiteSettingsCache = new();

    protected RMTeamsReportProcessor(string jobId, JobType jobType, bool IsOrphanedTermReport)
    {
        JobId = jobId;
        Logger = new RALogger(GetType());
        _groupTermTreeNodes = IsOrphanedTermReport ? ReportService.GetRATermTreeNodeOfOrphanedTermAsync().Result : ReportService.GetRATermTreeNodesAsync().Result;
        ReportManager = ReportMangerFactory.Instance.ReportManager;
        ReportManager.StartUpdateJobProgress();
        BaseJobDto = new BaseJobDto()
        {
            Id = jobId,
            JobType = (int)jobType
        };
        _designLists = WebUtil.GetDesignLists(TenantInfoDao.IsEnableCSD(TenantLocalValue.LogonGroupId));
        RMSubJob subJobWithContext = _subJobDao.GetSubJob(jobId, true);
        Init(subJobWithContext?.JobContext?.Settings);
    }

    private void Init(string? jobContext)
    {
        var tempSiteCollections = string.IsNullOrWhiteSpace(jobContext)
            ? []
            : SerializerHelper.DeserializeByDataContractSerializer<List<RMSPTreeNode>>(jobContext);
        foreach (var tempSiteCollection in tempSiteCollections)
        {
            var teams = tempSiteCollection.GetTeamsNode();
            var tempNode = tempSiteCollection.Clone();
            RMTeamsSetting setting = null;
            while (tempNode != null)
            {
                setting = _teamsSettingHelper.GetTeamsSetting(tempSiteCollection);
                if (setting != null)
                {
                    break;
                }
                tempNode = tempNode.Parent;
            }

            if (setting is { EnableRecordManagement: (int)EnableRecordManagementSetting.Enable })
            {
                var groupNode = tempSiteCollection.GetGroupNode();
                SiteCollectionNodeItems.Add(new NodeItem(tempSiteCollection, new NodeItem(groupNode)));
                mSiteSettingsCache.Add(new Guid(tempSiteCollection.Id), setting);
            }
            else
            {
                Logger.Info("node is disable {0}", tempSiteCollection.FullPath);
            }
        }

        if (tempSiteCollections is { Count: > 0 })
        {
            BCSColumnNameDics = InitBcsColumnNames(tempSiteCollections);
        }
    }

    public abstract Task RunAsync();

    private List<Guid> GetTermIds(IAveTaxonomyField taxonomyField)
    {
        List<Guid> subTermIds;
        Guid anchordGuid;
        string anchordId = taxonomyField.GetProperty("AnchorId");
        if (!string.IsNullOrEmpty(anchordId) && anchordId != "00000000-0000-0000-0000-000000000000")
        {
            anchordGuid = new Guid(anchordId);
            subTermIds = GetSubTermIds(anchordGuid);
        }
        else
        {
            subTermIds = GetSubTermIds(taxonomyField.TermSetId);
        }

        return subTermIds;
    }

    private List<Guid> GetSubTermIds(Guid termNodeId)
    {
        var termIds = new List<Guid> { termNodeId }; // include self
        var termNode = FindTermTreeNode(termNodeId);
        AddChildTerms(termNode, termIds);
        return termIds;
    }

    private void AddChildTerms(TermTreeNode termNode, List<Guid> termIds)
    {
        if (termNode?.Children != null)
        {
            foreach (var child in termNode.Children)
            {
                termIds.Add(child.Key);
                AddChildTerms(child.Value, termIds);
            }
        }
    }

    private TermTreeNode FindTermTreeNode(Guid termNodeId)
    {
        foreach (var groupNode in _groupTermTreeNodes)
        {
            var node = SearchTermTree(groupNode, termNodeId);
            if (node != null)
            {
                return node;
            }
        }
        return null;
    }

    private TermTreeNode SearchTermTree(TermTreeNode current, Guid termNodeId)
    {
        if (current.ID == termNodeId)
        {
            return current;
        }

        foreach (var child in current.Children.Values)
        {
            var found = SearchTermTree(child, termNodeId);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private IAveTaxonomyField GetTaxonomyField(IAveFieldCollection fields, string rmFieldTitle)
    {
        return fields.GetRecordTaxonomyField(rmFieldTitle);
    }

    protected string MakeFullUrl(string webUrl, string relativeUrl)
    {
        if (webUrl == null)
        {
            throw new ArgumentNullException("webUrl");
        }
        if (relativeUrl == null)
        {
            throw new ArgumentNullException("relativeUrl");
        }
        relativeUrl = relativeUrl.Trim();
        StringBuilder stringBuilder = new StringBuilder(512);
        if (relativeUrl.StartsWith("/"))
        {
            stringBuilder.Append(webUrl);
            stringBuilder.Append(relativeUrl);
        }
        else
        {
            stringBuilder.Append(webUrl);
            if (relativeUrl != "")
            {
                stringBuilder.Append("/");
                stringBuilder.Append(relativeUrl);
            }
        }
        if (stringBuilder[stringBuilder.Length - 1] == '/')
        {
            stringBuilder.Remove(stringBuilder.Length - 1, 1);
        }
        return stringBuilder.ToString();
    }

    protected void SendJobReportDetails(NodeItem item, JobDetailsStatus status, string comments = "")
    {
        if (this is RMTeamsCreationAndDestroyedFileReportProcessor)
        {
            return;
        }
        JMReportJobDetails detail = new JMReportJobDetails();
        detail.Type = JobReportUtility.ConvertItemTypeForDetails(item.NodeLevel);
        detail.TitleOrName = item.NameOrTitle;
        detail.Url = item.FullPath;
        detail.Status = status;
        detail.Comment = comments;
        ReportManager.SendJobDetail(detail);
    }

    protected void CheckNodeLevel(NodeItem node, NodeLevel expected)
    {
        if (!node.NodeLevel.Equals(expected))
        {
            throw new Exception(string.Format("Node expected level is {0}, but current node type is {1}. Node full path: {2}.", expected.ToString(), node.NodeLevel.ToString(), node.FullPath));
        }
    }

    protected void ClearChildren(NodeItem node)
    {
        node.Children.Clear();
    }

    protected static void SafeDisposeObject(object obj)
    {
        if (obj == null)
        {
            return;
        }

        var disposeObj = (obj as IDisposable);

        if (disposeObj != null)
        {
            disposeObj.Dispose();
        }
    }

    private Dictionary<Guid, string> InitBcsColumnNames(List<RMSPTreeNode> tempSiteCollections)
    {
        var result = new Dictionary<Guid, string>();
        foreach (var siteCollection in tempSiteCollections)
        {
            if (!siteCollection.IsEnableHoldPhyical)
            {
                var container = siteCollection.GetGroupNode();
                var tempColumnName = _teamsSettingDao.GetMedataColumn(new Guid(container.Id));
                if (!string.IsNullOrEmpty(tempColumnName))
                {
                    result.Add(new Guid(siteCollection.Id), tempColumnName);
                }
            }
        }
        return result;
    }

    public virtual async Task ProcessAsync(NodeItem node)
    {
        siteId = node.Id;
        try
        {
            if (node.NodeLevel == NodeLevel.SiteCollection)
            {
                await ProcessSiteAsync(node);
            }
        }
        catch (JobStopException ex)
        {
            throw new JobStopException("This Job is stopped.");
        }
        catch (Exception ex)
        {
            Logger.Error("An error occurred while ProcessAsync. fullPath: [{0}], error message : {1}.", node.FullPath, ex.ToString());
            throw;
        }
    }
    protected virtual async Task ProcessSiteAsync(NodeItem site)
    {
        using (PerformanceScope scope = new PerformanceScope("RMTeamsReportProcessor.ProcessSite", $"RMTeamsReportProcessor.ProcessSite.[{site.NameOrTitle}]", addToStatistics: true))
        {
            IAveWeb discoverWeb = null;
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    Logger.Info("Start Site process. fullPath: [{0}], isIncludeNew : [{1}].", site.FullPath, site.IncludeNew);
                    var remoteSite = RABrowserClient.GetSiteNode(site.FullPath);
                    var bposInfo = await CommonPoolUserUtil.GetBPOSInfoAsync(remoteSite);
                    _factory = MultiAppUtil.CreateAveObjectModelFactory(site.FullPath, bposInfo, AveContextKind.ClientObjectModel);
                    Site = _factory.CreateSite(site.FullPath);

                    var totalWebs = Site.AllWebs.Count;
                    ReportManager.IncreaseBase(totalWebs);

                    try
                    {
                        ClientContextHelper.InitClientContext(site, bposInfo);
                    }
                    catch (Exception ce)
                    {
                        Logger.Warn("Get Context Error {0}", ce.ToString());
                    }
                    _termWssidMappingsOfSite = new ConcurrentDictionary<Guid, int>();
                    discoverWeb = Site.RootWeb;
                    site.DiscoverObj = Site;
                    site.NameOrTitle = discoverWeb.Title;
                    ClientContextHelper.SPWebTimeZone = discoverWeb.RegionalSettings.TimeZone;
                    NodeItem rootWebNode;
                    if (site.HasCheckedChildren)
                    {
                        rootWebNode = site.Children.Values[0];
                        rootWebNode.DiscoverObj = discoverWeb;
                        rootWebNode.NameOrTitle = discoverWeb.Title;
                    }
                    else
                    {
                        rootWebNode = new NodeItem()
                        {
                            Id = discoverWeb.ID,
                            NameOrTitle = discoverWeb.Title,
                            DiscoverObj = discoverWeb,
                            FullPath = site.FullPath,
                            NodeLevel = NodeLevel.Site,
                            Parent = site,
                            IncludeNew = true,
                            IsChecked = true
                        };
                    }
                    rootWebNode.TeamsName = site.TeamsName;

                    SendJobReportDetails(site, JobDetailsStatus.Successful);
                    await ProcessWebAsync(rootWebNode);
                }
            }
            catch (JobStopException ex)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch (WebException we)
            {
                JobHasException = true;
                SendJobReportDetails(site, JobDetailsStatus.Failed, "RM_JM_Details_Failed_UnexpectedError");
                Logger.Error("An error occurred while prosess sitecollection, FullPath is :{0}, error message: {1}.", site.FullPath, we.ToString());
            }
            catch (Exception e)
            {
                JobHasException = true;
                SendJobReportDetails(site, JobDetailsStatus.Failed, "RM_JM_Details_Failed_UnexpectedError");
                Logger.Error("An error occurred while prosess sitecollection, id is :{0}, error message: {1}.", site.FullPath, e.ToString());
            }
            finally
            {
                SafeDisposeObject(Site);
                ClearChildren(site);
                DisposeContext();
            }
        }
    }

    protected virtual async Task ProcessWebAsync(NodeItem web, bool IsProcessLists = true)
    {
        using (PerformanceScope scope = new PerformanceScope("RMReportProcessor.ProcessWeb", $"RMReportProcessor.ProcessWeb.[{web.NameOrTitle}]", addToStatistics: true))
        {
            try
            {
                ReportManager.Increase();
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    CheckNodeLevel(web, NodeLevel.Site);
                    SendJobReportDetails(web, JobDetailsStatus.Successful);

                    try
                    {
                        ClientContextHelper.GetRegionalSetting((web.DiscoverObj as IAveWeb).ServerRelativeUrl);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn("Get Regional settings error {0}", ex.ToString());
                    }
                    if (web.Children.Count == 0)
                    {
                        if (IsProcessLists)
                        {
                            var treeNodeLists = new NodeItem()
                            {
                                NodeLevel = NodeLevel.Lists,
                                IsChecked = true,
                                FullPath = LISTS,
                                NameOrTitle = LISTS,
                                Parent = web,
                                DiscoverObj = web.DiscoverObj,
                                TeamsName = web.TeamsName
                            };
                            await ProcessListsAsync(treeNodeLists);
                        }

                        //Sites节点
                        var treeNodeSites = new NodeItem()
                        {
                            NodeLevel = NodeLevel.Sites,
                            IsChecked = true,
                            FullPath = SITES,
                            NameOrTitle = SITES,
                            Parent = web,
                            DiscoverObj = web.DiscoverObj,
                            TeamsName = web.TeamsName
                        };
                        await ProcessWebsAsync(treeNodeSites);
                    }
                    else
                    {
                        foreach (var childNode in web.Children.Values.OrderBy(n => n.NodeLevel))
                        {
                            if (AreThereProcessedChildren(childNode))
                            {
                                childNode.TeamsName = web.TeamsName;
                                childNode.DiscoverObj = web.DiscoverObj;
                                switch (childNode.NodeLevel)
                                {
                                    case NodeLevel.Lists:
                                        if (IsProcessLists)
                                        {
                                            await ProcessListsAsync(childNode);
                                        }
                                        break;
                                    case NodeLevel.Sites:
                                        await ProcessWebsAsync(childNode);
                                        break;

                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch (JobStopException ex)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception e)
            {
                JobHasException = true;
                Logger.Error("An error occurred while processing web: {0}, error message: {1}.", web.FullPath, e.ToString());
            }
            finally
            {
                SafeDisposeObject(web.DiscoverObj);
                ClearChildren(web);
            }
        }
    }
    protected virtual async Task ProcessWebsAsync(NodeItem sitesNode)
    {
        using (PerformanceScope scope = new PerformanceScope("RMTeamsReportProcessor.ProcessWebs", $"RMTeamsReportProcessor.ProcessWebs.[{sitesNode.NameOrTitle}]", addToStatistics: true))
        {
            try
            {
                CheckNodeLevel(sitesNode, NodeLevel.Sites);
                var parentWeb = sitesNode.DiscoverObj as IAveWeb;
                NodeItem tempWebNode;
                foreach (var subWeb in parentWeb.Webs)
                {
                    ReportManager.Increase();
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        if (sitesNode.Children.TryGetValue(subWeb.ID, out tempWebNode))
                        {
                            tempWebNode.DiscoverObj = subWeb;
                            if (AreThereProcessedChildren(tempWebNode))
                            {
                                tempWebNode.TeamsName = sitesNode.TeamsName;
                                await ProcessWebAsync(tempWebNode);
                            }
                            else if (tempWebNode.IsChecked)
                            {
                                SendJobReportDetails(tempWebNode, JobDetailsStatus.Successful);
                            }
                            sitesNode.Children.Remove(subWeb.ID);
                        }
                        else if (sitesNode.IsChecked)
                        {
                            tempWebNode = new NodeItem()
                            {
                                Id = subWeb.ID,
                                NameOrTitle = subWeb.Name,
                                DiscoverObj = subWeb,
                                FullPath = subWeb.Url,
                                NodeLevel = NodeLevel.Site,
                                Parent = sitesNode,
                                IsChecked = true,
                                TeamsName = sitesNode.TeamsName,
                            };
                            await ProcessWebAsync(tempWebNode);
                        }
                    }
                }

                if (sitesNode.Children.Count > 0)
                {
                    foreach (var node in sitesNode.Children.Values)
                    {
                        if (node.IsChecked)
                        {
                            JobHasException = true;
                            SendJobReportDetails(node, JobDetailsStatus.Failed, "RM_JM_Details_Failed_NodeDeleted");
                        }
                    }
                }
            }
            catch (JobStopException ex)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception e)
            {
                JobHasException = true;
                Logger.Error("An error occurred while processing sites level node, error message: {0}.", e.ToString());
            }
            finally
            {
                ClearChildren(sitesNode);
            }
        }
    }
    private async Task ProcessListAsync(NodeItem listsNode, IAveWeb parentWeb, IAveList discoverList, CancellationTokenSource cts = null)
    {
        try
        {
            using (CheckJobStopScope jScope = new CheckJobStopScope())
            {
                ReportManager.Increase();
                NodeItem tempListNode;
                Logger.Info("list rootfolder url {0}", discoverList.RootFolder.Name);
                int listTemplate = (int)discoverList.BaseTemplate;
                if (listTemplate == 600)
                {
                    Logger.Info("Skip external list {0}", discoverList.RootFolder.Name);
                    return;
                }
                if (CheckIsDesignList(discoverList.RootFolder.Name + listTemplate.ToString()) || discoverList.Hidden)
                {
                    Logger.Info("Skip the design list & system list{0}", discoverList.RootFolder.Name);
                    return;
                }
                if (listsNode.Children.TryGetValue(discoverList.ID, out tempListNode) && tempListNode.IsChecked)
                {
                    tempListNode.TeamsName = listsNode.TeamsName;
                    tempListNode.DiscoverObj = discoverList;
                    await ProcessListAsync(tempListNode);
                    //mProcessedLists++;
                    //UpdateJobProgressByList();
                    listsNode.Children.SafeRemove(discoverList.ID);
                }
                else if (listsNode.IsChecked)
                {
                    if (!listsNode.Children.TryGetValue(discoverList.ID, out tempListNode))
                    {
                        tempListNode = new NodeItem()
                        {
                            NodeLevel = NodeLevel.List,
                            Id = discoverList.ID,
                            NameOrTitle = discoverList.Title,
                            FullPath = MakeFullUrl(parentWeb.Url, discoverList.RootFolder.Url),  //discoverList.RootFolder.ServerRelativeUrl,
                            NodeType = discoverList.BaseType == AveBaseType.DocumentLibrary ? NodeType.DocumentLibrary : NodeType.GenericList,
                            DiscoverObj = discoverList,
                            Parent = listsNode,
                            IncludeNew = true,
                            IsChecked = true,
                            TeamsName = listsNode.TeamsName,
                        };
                        if (tempListNode.NameOrTitle.Equals("{System Folder}", StringComparison.OrdinalIgnoreCase))
                        {
                            tempListNode.NodeType = NodeType.DocumentLibrary;
                        }
                        if (tempListNode != null && tempListNode.IsChecked)
                        {
                            await ProcessListAsync(tempListNode);
                        }
                        else
                        {
                            Logger.Warn("Temp list node is null.");
                        }
                    }
                }
            }
        }
        catch (JobStopException ex)
        {
            cts?.Cancel();
            throw ex;
        }
    }
    private bool CheckIsDesignList(string listInfo)
    {
        bool isDesignList = false;
        try
        {
            if (this._designLists.Contains(listInfo))
            {
                return true;
            }
        }
        catch (Exception e)
        {
            Logger.Warn($"An error has occurred when CheckIsDesignList, message:{e.Message}");
        }
        return isDesignList;
    }
    private async Task ProcessListsAsync(IAveWeb parentWeb, NodeItem listsNode)
    {
        if (parentWeb.Lists.Count == 0) return;
        foreach (var discoverList in parentWeb.Lists)
        {
            await ProcessListAsync(listsNode, parentWeb, discoverList);
        }
    }
    protected async Task ProcessListsAsync(NodeItem listsNode)
    {
        using (PerformanceScope scope = new PerformanceScope("RMTeamsReportProcessor.ProcessLists", $"RMTeamsReportProcessor.ProcessLists.[{listsNode.NameOrTitle}]", addToStatistics: true))
        {
            try
            {
                CheckNodeLevel(listsNode, NodeLevel.Lists);
                var parentWeb = listsNode.DiscoverObj as IAveWeb;
                //NodeItem tempListNode;
                ReportManager.IncreaseBase(parentWeb.Lists.Count);
                await ProcessListsAsync(parentWeb, listsNode);
                if (listsNode.Children.Count > 0)
                {
                    foreach (var node in listsNode.Children.Values)
                    {
                        if (node.IsChecked)
                        {
                            string webRelativeUrl = parentWeb.ServerRelativeUrl;
                            if (node.FullPath.StartsWith(webRelativeUrl, StringComparison.OrdinalIgnoreCase))
                            {
                                node.FullPath = node.FullPath.Substring(webRelativeUrl.Length);
                                node.FullPath = MakeFullUrl(parentWeb.Url, node.FullPath);
                            }
                            JobHasException = true;
                            SendJobReportDetails(node, JobDetailsStatus.Failed, "RM_JM_Details_Failed_NodeDeleted");
                        }
                    }
                }
            }
            catch (JobStopException ex)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception e)
            {
                JobHasException = true;
                Logger.Error("An error occurred while processing lists level node, error message: {0}.", e.ToString());
            }
            finally
            {
                ClearChildren(listsNode);
            }
        }
    }
    protected virtual async Task ProcessListAsync(NodeItem list)
    {
        using (PerformanceScope scope0 = new PerformanceScope("RMTeamsReportProcessor.ProcessList", $"RMTeamsReportProcessor.ProcessList.[{list.NameOrTitle}]", addToStatistics: true))
        {
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    CheckNodeLevel(list, NodeLevel.List);
                    var discoverList = list.DiscoverObj as IAveList;
                    var rootFolder = discoverList.RootFolder;
                    var discoverWeb = discoverList.ParentWeb;

                    list.FullPath = MakeFullUrl(discoverWeb.Url, rootFolder.Url);
                    list.NameOrTitle = discoverList.Title;
                    var listFields = discoverList.Fields;
                    IAveTaxonomyField mmsField = null;
                    try
                    {
                        mmsField = GetTaxonomyField(listFields, BCSColumnName);
                        BCSColumnInternalName = mmsField.InternalName;
                        Logger.Info("List Metadata coloumn {0}", BCSColumnName);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn("The list doesn't have BCS Column. List url: {0}, Error message: {1}.", list.FullPath, ex.ToString());
                        SendJobReportDetails(list, JobDetailsStatus.Successful, "RM_JM_Details_Sucess_NoBCSColumns");
                        return;
                    }
                    var termIds = GetTermIds(mmsField);
                    long total = 0;
                    int rowLimit = Site.GetMaxItemsPerThrottledOperation();
                    var cms = GetCAMLManagerList(listFields, mmsField, termIds, discoverWeb, discoverList);
                    if (cms.Count != 0)
                    {
                        ReportManager.Increase();

                        Logger.Debug($"begin to query list:{list.FullPath}, cms count:{cms.Count}, termIdsCount:{termIds.Count}");
                        var endIndex = SpCommonUtility.GetLastItemFolderId(discoverList, discoverList.RootFolder);

                        foreach (CAMLManager cm in cms)
                        {
                            total += SpCommonUtility.ConfigItemsByQueryInfo(new SPQueryInfo()
                            {
                                List = discoverList,
                                CAML = cm,
                                RowLimit = rowLimit,
                                MaxItemId = endIndex,
                                CurrentFolder = discoverList.RootFolder,
                                ScopeType = Types.ScopeTypes.RecursiveAll
                            }, items =>
                            {
                                return ProcessItems(discoverWeb, discoverList, items, list.TeamsName);
                            });
                        }
                    }

                    SendJobReportDetails(list, JobDetailsStatus.Successful, total > 0 ? "" : "RM_JM_Details_Sucess_NoMachedList");
                }
            }
            catch (JobStopException ex)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception e)
            {
                JobHasException = true;
                SendJobReportDetails(list, JobDetailsStatus.Failed, "RM_JM_Details_Failed_UnexpectedError");
                Logger.Error("An error occurred while processing list level node: {0}, error message: {1}.", list.FullPath, e.ToString());
            }
            finally
            {
                SafeDisposeObject(list.DiscoverObj);
                ClearChildren(list);
            }
        }
    }
  
    
    private List<CAMLManager> GetCAMLManagerList(IAveFieldCollection listFields, IAveTaxonomyField mmsField, List<Guid> termIds, IAveWeb web, IAveList list)
    {
        List<CAMLManager> cms = new List<CAMLManager>();
        if (termIds.Count < _queryConditionMaxCount)
        {
            CAMLManager cm = InitCamlQuery(listFields, mmsField, termIds, web, list);
            if (cm != null)
            {
                cms.Add(cm);
            }
        }
        else
        {
            int index = 0;
            while (termIds.Skip(index).Take(_queryConditionMaxCount) != null && termIds.Skip(index).Take(_queryConditionMaxCount).Count() != 0)
            {
                var queryIds = termIds.Skip(index).Take(_queryConditionMaxCount).ToList();
                index += _queryConditionMaxCount;
                if (queryIds.Count() != 0)
                {
                    CAMLManager cm = InitCamlQuery(listFields, mmsField, queryIds, web, list);
                    if (cm != null)
                    {
                        cms.Add(cm);
                    }
                }
            }
        }
        return cms;
    }

    private void GetSubTermIds(TermTreeNode termNode, ref List<Guid> termIds)
    {
        if (termNode != null)
        {
            foreach (var item in termNode.Children)
            {
                termIds.Add(item.Key);
                GetSubTermIds(item.Value, ref termIds);
            }
        }
    }
    private TermTreeNode GetTermTreeNode(TermTreeNode sourceNode, Guid termNodeId)
    {
        TermTreeNode tempNode = null;
        if (sourceNode != null && sourceNode.ID != termNodeId)
        {
            if (sourceNode.Children != null && sourceNode.Children.Count > 0 && !sourceNode.Children.TryGetValue(termNodeId, out tempNode))
            {
                foreach (var node in sourceNode.Children.Values)
                {
                    tempNode = GetTermTreeNode(node, termNodeId);
                    if (tempNode != null)
                    {
                        break;
                    }
                }
            }
        }

        return tempNode;
    }

    protected string GetSingleUserFieldValue(IAveListItem item, string fieldName)
    {
        return item.GetSingleUserFieldValue(fieldName);
    }

    protected void SendJobReportItemDetails(IAveListItem item, NodeLevel nodeLevel, JobDetailsStatus status, string comments = "")
    {
        try
        {
            JMReportJobDetails detail = new JMReportJobDetails();
            detail.Type = JobReportUtility.ConvertItemTypeForDetails(nodeLevel);
            detail.TitleOrName = item.Name;
            detail.Url = item.FullPath();
            detail.Status = status;
            detail.Comment = comments;
            ReportManager.SendJobDetail(detail);
        }
        catch (Exception e)
        {
            Logger.Error("An error occurred while sending item detail, error:{0}", e.ToString());
        }
    }
    protected string GetListItemName(IAveListItem item)
    {
        var itemName = item.Name;
        if (!string.IsNullOrEmpty(itemName))
        {
            return itemName;
        }
        switch (item.ParentList.BaseTemplate)
        {
            case AveListTemplateType.DocumentLibrary:
            case AveListTemplateType.RecordLib:
                itemName = item.Name;
                break;
            case AveListTemplateType.Links:
                if (AveListTemplateType.Links == item.ParentList.BaseTemplate)
                {
                    IAveFieldUrlValue filedUrlValue = _factory.CreateFieldUrlValue(item.FieldValues["URL"].ToString());
                    itemName = filedUrlValue.Url;
                }
                break;
            default:
                itemName = item.Title;
                break;
        }
        return itemName;
    }

    protected bool AreThereProcessedChildren(NodeItem node)
    {
        return node.HasCheckedChildren || node.IncludeNew || (node.IsChecked && node.Children.Count == 0);
    }

    protected abstract CAMLManager InitCamlQuery(IAveFieldCollection listFields, IAveTaxonomyField taxonomyField, List<Guid> termIds, IAveWeb web, IAveList list);

    protected abstract int ProcessItems(IAveWeb web, IAveList list, List<RMDiscoverItem> items, string teamsName);

    protected bool GetWssidOfTerm(IAveTaxonomyField taxonomyField, Guid termId, out int wssid)
    {
        bool result = false;
        InitTermWssidMappingsOfSite();
        if (!_termWssidMappingsOfSite.TryGetValue(termId, out wssid))
        {
            try
            {
                wssid = int.Parse(GetWssIDForTerm(termId));
                if (wssid > 0)
                {
                    result = true;
                    _termWssidMappingsOfSite.TryAdd(termId, wssid);
                }
            }
            catch (Exception ex)
            {
                Logger.Warn("Get TermId And WssId Mapping failed! Term id: {0}. Error message: {1}.", termId, ex.ToString());
            }
        }
        else if (wssid != 0)
        {
            return true;
        }
        return result;
    }

    protected string GetWssIDForTerm(Guid termId)
    {
        try
        {
            string result = "-1";
            List taxonomyList = ClientContextHelper.Context.Web.Lists.GetByTitle("TaxonomyHiddenList");
            CamlQuery camlQueryForTerm = new CamlQuery();
            camlQueryForTerm.ViewXml = @"
            <View>
                <Query>
                    <Where>
                        <Eq>
                            <FieldRef Name='IdForTerm' />
                            <Value Type='Text'>" + termId + @"</Value>
                        </Eq>
                    </Where>
                </Query>       
            </View>";
            ListItemCollection termItems = taxonomyList.GetItems(camlQueryForTerm);
            ClientContextHelper.Context.Load(termItems);
            ClientContextHelper.Context.ExecuteQuery();
            if (termItems?.FirstOrDefault() != null)
            {
                result = termItems?.First()["ID"].ToString();
            }
            return result;
        }
        catch (Exception e1)
        {
            Logger.Warn("get wwsid for term error: {0}", e1.ToString());
            return "-1";
        }
    }
    private void InitTermWssidMappingsOfSite()
    {
        if (_termWssidMappingsOfSite == null)
        {
            lock (lockTermWssidMappingsOfSiteObj)
            {
                if (_termWssidMappingsOfSite == null)
                {
                    _termWssidMappingsOfSite = new ConcurrentDictionary<Guid, int>();
                }
            }
        }
    }

    private void DisposeContext()
    {
        try
        {
            if (ClientContextHelper.Context != null)
            {
                ClientContextHelper.Context.Dispose();
            }
        }
        catch (Exception e)
        {
            Logger.Warn("Dispose context error {0}", e.ToString());
        }
    }
  
  
    protected bool GetSingleTaxonomyFieldValue(IAveListItem item, string fieldName, out Guid termId, out string termName)
    {
        return item.GetSingleTaxonomyFieldValue(fieldName, out termId, out termName);
    }
}