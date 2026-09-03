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



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
//using Microsoft.SharePoint;
using AvePoint.Wrapper.Discovery;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon.Contract.Server.Job.Object;
using System.Collections;
using System.Reflection;
using AvePoint.GCommon;
using System.Globalization;
using System.Xml;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.Contract;
using AvePoint.RA.DB.Model;
using Newtonsoft.Json;
using RAArchiverCommon;
using AvePoint.RA.Common;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.RA.SharePoint.ExplorerSync.Cache;
using AvePoint.RA.Contract.RMWeb.JobMonitor;

namespace AvePoint.RA.SharePoint.Archiver
{
    public class ArchiverNodeItem : IDisposable
    {
        #region Private Vars
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private Char delimiter = (Char)0x12;
        private Guid mId = Guid.Empty;
        private string mFullPath = string.Empty;
        private bool mIsSystemObject = false;
        private int mCacheNodeType = -1;
        private SortedList<Guid, ArchiverNodeItem> mChildren = null;
        private int mLibRowId = 0;
        private Guid attachementID = Guid.Empty;
        #endregion

        #region for enduser archive
        public JobDetailsStatus Status { get; set; }
        public string ExceptionMessage { get; set; }
        #endregion

        #region Properties
        public Guid ID { get { return mId; } set { mId = value; } }
        public string Title { get; set; }
        public string Name { get; set; }
        public byte Level
        {
            get
            {
                byte result = byte.MinValue;
                if (DiscoverSPObject is AveDiscoverItem)
                {
                    result = ((AveDiscoverItem)DiscoverSPObject).Level;
                }
                else if (DiscoverSPObject is AveVersionObject)
                {
                    result = ((AveVersionObject)DiscoverSPObject).Level;
                }
                else if (DiscoverSPObject is AveItemObject)
                {
                    result = ((AveItemObject)DiscoverSPObject).Level;
                }
                else if (DiscoverSPObject is AveDiscoverFolder)
                {
                    result = ((AveDiscoverFolder)DiscoverSPObject).Level;
                }
                else if (DiscoverSPObject is IAveListItem)
                {
                    //result = (byte)((IAveListItem)DiscoverSPObject).File.Level;
                    result = Convert.ToByte(((IAveListItem)DiscoverSPObject)["_Level"]);
                }
                else if (DiscoverSPObject is IAveListItemVersion)
                {
                    result = (byte)((IAveListItemVersion)DiscoverSPObject).Level;
                }

                return result;
            }
        }
        public int UIVersion
        {
            get
            {
                int result = -1;
                if (DiscoverSPObject is AveDiscoverItem)
                {
                    result = ((AveDiscoverItem)DiscoverSPObject).Uiversion;
                }
                else if (DiscoverSPObject is AveVersionObject)
                {
                    result = ((AveVersionObject)DiscoverSPObject).Uiversion;
                }
                else if (DiscoverSPObject is AveItemObject)
                {
                    result = ((AveItemObject)DiscoverSPObject).Uiversion;
                }
                else if (DiscoverSPObject is AveDiscoverFolder)
                {
                    result = ((AveDiscoverFolder)DiscoverSPObject).Uiversion;
                }
                else if (DiscoverSPObject is AveDiscoverSite)
                {
                    result = -1;
                }
                else if (DiscoverSPObject is AveDiscoverWeb)
                {
                    result = -1;
                }
                else if (DiscoverSPObject is AveDiscoverList)
                {
                    result = -1;
                }
                else if (DiscoverSPObject is IAveListItem)
                {
                    //result = ((IAveListItem)DiscoverSPObject).File.UIVersion;
                    result = Convert.ToInt32(((IAveListItem)DiscoverSPObject)["_UIVersion"]);
                }
                else if (DiscoverSPObject is IAveListItemVersion)
                {
                    result = ((IAveListItemVersion)DiscoverSPObject).VersionId;
                }

                return result;
            }
        }
        public string FullPath
        {
            get
            {
                //If fullpath is empty, it means this is a root sitecollection. So we need to use Webapplication URL as SiteCollection's
                return string.IsNullOrEmpty(mFullPath) ? WebApplicationUrl?.TrimEnd('/') ?? "" : mFullPath;
            }
            set
            {
                mFullPath = value;
            }
        }
        public bool ArchiveLevel { get; set; }
        public bool ApproveStatus { get; set; }
        public string RuleId { get; set; }

        public int RulePolicyLevel { get; set; }
        public string RuleName { get; set; }
        public string RuleArchiverAction { get; set; }
        public int ReportLevel { get; set; }

        public List<int> ItemIDs { get; set; }
        /// <summary>
        /// This is the same as cacheNode Level. This is only for the web and folder ,because their scructure is not clear.
        /// </summary>
        public int Cache_NodeType
        {
            get
            {
                if (mCacheNodeType == -1)
                {
                    switch (this.SPNodeLevel)
                    {
                        case NodeLevel.SiteCollection:
                            mCacheNodeType = (int)CacheNodeType.SiteCollection;
                            break;
                        case NodeLevel.Site:
                            mCacheNodeType = (int)CacheNodeType.Web;
                            break;
                        case NodeLevel.List:
                        case NodeLevel.Library:
                            mCacheNodeType = (int)CacheNodeType.List;
                            break;
                        case NodeLevel.Folder:
                            mCacheNodeType = (int)CacheNodeType.Folder;
                            break;
                        default:
                            mCacheNodeType = 0;
                            break;
                    }
                }
                return mCacheNodeType;
            }
            set
            {
                mCacheNodeType = value;
            }
        }
        public ArchiverCommon.NodeType NodeType { get; set; }
        public ArchiverCommon.ItemType ItemType { get; set; }
        /// <summary>
        /// NodeLevel from Tree Node
        /// </summary>
        public NodeLevel SPNodeLevel { get; set; }
        public object DiscoverSPObject { get; set; }
        public bool ShouldDoArchive { get; set; }
        public bool? IsRecord { get; set; }
        public int? holdAndRecordStatus { get; set; }
        public Guid AttachementID { get { return attachementID; } }

        /// <summary>
        /// This is used at Debug: output the scan result to a local folder which name is scheduleId;
        /// </summary>
        public string ScheduleId { get; set; }

        public Guid WebApplicationId { get; set; }
        public string WebApplicationUrl { get; set; }
        public string SiteUrl { get; set; }
        public Guid SiteId { get; set; }
        public Guid WebId { get; set; }
        public Guid ListId { get; set; }
        public int ListType { get; set; }
        /// <summary>
        /// This is Only Use for EndUserArchive
        /// </summary>
        public Guid FolderId { get; set; }
        /// <summary>
        /// This is Only Use for EndUserArchive
        /// </summary>
        public Guid ItemId { get; set; }
        /// <summary>
        /// This is Only Use for EndUserArchive
        /// </summary>
        public bool IsRootFolder { get; set; }
        public bool IsSystemObject
        {
            get
            {
                return mIsSystemObject ? mIsSystemObject : ((this.SPNodeLevel == NodeLevel.Library || this.SPNodeLevel == NodeLevel.List) && this.ID.Equals(Guid.Empty));
            }
            set
            {
                mIsSystemObject = value;
            }
        }
        public int RuleMatchStatus { get; set; }
        /// <summary>
        /// Only the archive level Node ,this property is availiable;
        /// </summary>
        public RuleCollection RuleCollection { get; set; }
        public ArchiverNodeItem Parent { get; set; }
        public SortedList<Guid, ArchiverNodeItem> Children
        {
            get
            {
                if (mChildren == null)
                {
                    mChildren = new SortedList<Guid, ArchiverNodeItem>();
                }
                return mChildren;
            }
            private set { mChildren = value; }
        }
        public Dictionary<string, RuleNodeContract> BreakInheritNodes { get; private set; }

        /// <summary>
        /// add properties for test run
        /// </summary>
        /// 
        public long DocumentSize { get; set; }
        public long Created { get; set; }
        public string CreatedBy { get; set; }
        public long Modified { get; set; }
        public string ModifiedBy { get; set; }
        private string _author;
        public string Author
        {
            get { return _author; }
            set { _author = HandleAveUserName(value); }
        }
        private string _editor;
        public string Editor 
        {
            get { return _editor; }
            set { _editor = HandleAveUserName(value); }
        }
        public bool ActionTaken { set; get; }

        /// <summary>
        /// add properties for newsfeed
        /// </summary>
        /// 
        public bool IsMicroFeed { get; set; }
        public bool IsMicroFeedRef { get; set; }
        public MicroBlogType MicroType { get; set; }
        public Guid MicroFeedListID { get; set; }
        public string RootPostID { get; set; }
        public string PostID { get; set; }
        public bool IsAppData { get; set; }
        public string AppDataName { get; set; }
        public int LibRowID
        {
            get { return mLibRowId; }
            set { mLibRowId = value; }
        }


        public Hashtable ItemDisplayColumns { get; set; }

        public IAveList SPList { get; set; }

        public string SiteTitle { get; set; }

        public int HasRelatedDocument { get; set; }//记录是否存在RelatedRecord ， >1 表示存在，0 表示不存在

        public int DeleteRelatedRecords { get; set; }//标记是否在删除文件的同时，删除RelatedRecord, 1 means delated related record,  0 means skip, 

        public string RelatedRecordInfo { get; set; }

        public bool DoDelete { get; set; }//标记文件后续是否要执行action 

        public bool ForcedReport { get; set; }

        public bool ForcedNotReport { get; set; }

        public bool IsInheritContainerTerm { get; set; } // Inherit container level term for unclassified docs

        public Guid ContainerLevelTermId { get; set; } // container level term for folder and docs will be the same with their parent list

        public ManifestDocumentSnapshot ManifestSnapshot { get; set; }
        #endregion

        #region Constructor

        public ArchiverNodeItem()
        {
        }

        public ArchiverNodeItem(RuleNodeContract root)
            : this()
        {
            BuildNodeItemTree(root);
        }

       /* public void AddChild(RuleNodeContract node)
        {
            foreach (RuleNodeContract tmp in node.Children)
            {
                AddChild(new ArchiverNodeItem(tmp));
            }
        }*/

        private void BuildNodeItemTree(RuleNodeContract root)
        {
            CopyTreeNodeAtrributeToNodeItem(root);
        }

        /// <summary>
        /// Convert TreeNode to NodeItem
        /// </summary>
        /// <param name="node"></param>
        private void CopyTreeNodeAtrributeToNodeItem(RuleNodeContract node)
        {
            FullPath = node.FullPath == null ? string.Empty : node.FullPath.TrimStart('/');
            ID = new Guid(node.NodeId);
            if (node.NodeLevel == NodeLevel.Site && !FullPath.Equals(node.SiteUrl, StringComparison.OrdinalIgnoreCase))
            {
                Name = node.FullPath.Substring(node.SiteUrl.Length + 1);
            }
            else
            {
                Name = node.NodeName;
            }
            SPNodeLevel = node.NodeLevel;
            if (node.NodeLevel == NodeLevel.Folder)
            {
                Title = node.ListTitle;
            }
            else
            {
                Title = node.NodeName;
            }
            if (node.RuleCollection != null)
            {
                RuleCollection = node.RuleCollection;
            }
            BreakInheritNodes = node.BreakInheritNodesEncryptBySha1;
            //WebApplicationUrl = node.WebAppUrl;
            //WebApplicationId = new Guid(node.WebAppId);
            SiteUrl = node.SiteUrl;
            SiteId = string.IsNullOrEmpty(node.SiteId) ? Guid.Empty : new Guid(node.SiteId);
            WebId = string.IsNullOrEmpty(node.WebId) ? Guid.Empty : new Guid(node.WebId);
            ListId = string.IsNullOrEmpty(node.ListId) ? Guid.Empty : new Guid(node.ListId);
        }

        public ArchiverNodeItem(ArchiveApproveReport report)
        {
            //ApproveReport result = new ApproveReport();
            //result.ScanTime = DateTime.UtcNow.Ticks;//arthur: maybe need pass this value from outside
            //result.FullPath = this.FullPath;
            //result.LeafName = this.Name;
            //result.LibRowId = this.mLibRowId;
            //result.NodeId = this.ID.ToString();
            ////result.NodeType = this.Cache_NodeType;
            //result.NodeType = this.Cache_NodeType >= 10000 ? (int)this.ItemType : (int)this.NodeType;
            //result.CacheNodeType = this.Cache_NodeType;
            //result.ParentId = this.Parent == null ? Guid.Empty.ToString() : this.Parent.ID.ToString();
            //result.UIVersion = this.UIVersion;
            //result.ArchiveLevel = this.ArchiveLevel;


            //result.Approval = true;
            //result.Level = this.Level;
            ////result.StorageId = this.StoragePolicyId==string.Empty? null:this.StoragePolicyId;
            //result.RuleId = this.RuleId == null ? null : this.RuleId.ToString();
            FullPath = report.FullPath;
            Name = report.LeafName;
            mLibRowId = report.LibRowId;
            ID = new Guid(report.NodeId);
            Cache_NodeType = report.CacheNodeType;
            SPNodeLevel = (NodeLevel)report.SPNodeLevel;
            ReportLevel = report.ArchiveLevel;
            ItemIDs = report.ItemIDs;
        }

        #endregion

        #region override .
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{{[Name:{0}], [Level:{1}], [Children:{2}}}",
                Name, SPNodeLevel, Children == null ? 0 : Children.Count);
            return sb.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            ArchiverNodeItem other = obj as ArchiverNodeItem;
            return this.SPNodeLevel == other.SPNodeLevel && this.ID.Equals(other.ID);
        }

        public override int GetHashCode()
        {
            return ((Name == null ? 0 : Name.GetHashCode()) + (int)SPNodeLevel);
        }
        #endregion

        #region Public Functions


        public ArchiverNodeItem GenerateWebappNodeItem()
        {
            ArchiverNodeItem result = new ArchiverNodeItem();
            result.ID = this.WebApplicationId;
            result.FullPath = this.WebApplicationUrl;
            result.Cache_NodeType = (int)CacheNodeType.WebApplication;
            result.NodeType = ArchiverCommon.NodeType.WebApp;
            result.Name = this.WebApplicationUrl;
            result.SPNodeLevel = NodeLevel.WebApplication;
            this.Parent = result;
            return result;
        }

        /// <summary>
        /// Only support: Webapplication NodeItem call this function
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public ArchiverNodeItem GenerateSiteCollectionNodeItem(IAveSite site, ScheduleConfiguration config)
        {
            ArchiverNodeItem siteNode = new ArchiverNodeItem();
            siteNode.SPNodeLevel = NodeLevel.SiteCollection;

            siteNode = new ArchiverNodeItem();
            siteNode.ID = site.ID;
            siteNode.WebApplicationId = this.ID;
            siteNode.WebApplicationUrl = this.WebApplicationUrl;
            siteNode.Name = site.Url;
            siteNode.Title = site.Url;
            siteNode.FullPath = site.Url;//site.ServerRelativeUrl.StartsWith("/",StringComparison.OrdinalIgnoreCase) ? site.ServerRelativeUrl.Substring(1) : site.ServerRelativeUrl;
            siteNode.RuleCollection = this.RuleCollection;
            siteNode.NodeType = ArchiverCommon.NodeType.Site;
            siteNode.SPNodeLevel = NodeLevel.SiteCollection;
            siteNode.SiteUrl = site.Url;
            try
            {
                if (config.IsILMode && config.RuleCollection != null &&
                    config.RuleCollection[1].ProfileType == GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRuleForRevIM)//RevIM job获取metadata
                {
                    mLog.Info("Current rule is ArchiverRuleForRevIM job and need get site collection information, site collection url is :{0}.", siteNode.FullPath);
                    Hashtable columnCollectionOfDisplayName = new Hashtable(StringComparer.OrdinalIgnoreCase);
                    try
                    {
                        columnCollectionOfDisplayName["author"] = site.Owner.LoginName;
                        columnCollectionOfDisplayName["editor"] = site.RootWeb.CurrentUser.Name;
                        siteNode.ItemDisplayColumns = columnCollectionOfDisplayName;
                    }
                    catch (Exception e)
                    {
                        mLog.Warn("Get Version Properties Error{0}", e.ToString());
                    }
                }

            }
            catch (Exception exp)
            {
                mLog.Warn("Error in Get item columns : " + exp.ToString());
            }
            siteNode.IsInheritContainerTerm = SetInheritContainerTerm(config, new Guid(config.AveSiteId), siteNode.SPNodeLevel);
            mLog.Info($"Get inherit term flag for node: {siteNode.FullPath}, level: {siteNode.SPNodeLevel}, IsInheritContainerTerm: {siteNode.IsInheritContainerTerm} ");
            return siteNode;
        }

        public ArchiverNodeItem GenerateSiteNodeItem(AveDiscoverWeb web, ScheduleConfiguration config, bool isOrParentIsRooWeb = false)
        {
            //web.name 不是web 的相对url，是web url 最后一个斜杠后的字符串，既是web 的title,这里需要的webname是为index 用的，所以要重新组装成sub01/subsite1/subsubsite1，sub01是rootweb下的subweb。
            string webName = web.Name;
            if (!isOrParentIsRooWeb)
            {
                if (web.AveWeb.Site.ServerRelativeUrl.Equals("/"))
                {
                    webName = web.FullUrl.Substring(web.AveWeb.Site.ServerRelativeUrl.Length);
                }
                else
                {
                    webName = web.FullUrl.Substring(web.AveWeb.Site.ServerRelativeUrl.Length + 1);
                }
            }
            ArchiverNodeItem result = new ArchiverNodeItem()
            {
                SPNodeLevel = NodeLevel.Site,
                ID = web.WebID,
                WebApplicationId = this.WebApplicationId,
                SiteId = this.ID,
                WebApplicationUrl = this.WebApplicationUrl,
                FullPath = web.FullUrl,
                Name = webName,
                Title = web.Title,
                SiteTitle = web.Title,
                Parent = this,
                DiscoverSPObject = web,
                Cache_NodeType = this.Cache_NodeType == (int)CacheNodeType.SiteCollection ? (int)CacheNodeType.Web : (this.Cache_NodeType + 1),
                NodeType = ArchiverCommon.NodeType.Web,
                SiteUrl = web.AveWeb.Site.Url,
                WebId = web.WebID,
                IsAppData = web.AveWeb.IsAppWeb,
            };
            if (this.ShouldDoArchive)
            {
                result.ShouldDoArchive = true;
            }
            if (result.IsAppData)
            {
                result.AppDataName = web.Name;
            }
            try
            {
                if (config.IsILMode && config.RuleCollection != null &&
                    config.RuleCollection[1].ProfileType == GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRuleForRevIM)//RevIM job获取metadata
                {
                    mLog.Info("Current rule is ArchiverRuleForRevIM job and need get site information, site collection url is :{0}.", result.FullPath);
                    Hashtable columnCollectionOfDisplayName = new Hashtable(StringComparer.OrdinalIgnoreCase);
                    try
                    {
                        if (web.AveWeb.IsRootWeb)
                        {
                            columnCollectionOfDisplayName["author"] = web.AveWeb.Site.Owner.LoginName;
                        }
                        else
                        {
                            columnCollectionOfDisplayName["author"] = web.AveWeb.Author.LoginName;
                        }
                        columnCollectionOfDisplayName["editor"] = web.AveWeb.CurrentUser.Name;
                        result.ItemDisplayColumns = columnCollectionOfDisplayName;
                    }
                    catch (Exception e)
                    {
                        mLog.Warn("Get Version Properties Error{0}", e.ToString());
                    }
                }

            }
            catch (Exception exp)
            {
                mLog.Warn("Error in Get item columns : " + exp.ToString());
            }
            result.IsInheritContainerTerm = SetInheritContainerTerm(config, web.WebID, result.SPNodeLevel);
            mLog.Info($"Get inherit term flag for node: {result.ID}, level: {result.SPNodeLevel}, IsInheritContainerTerm: {result.IsInheritContainerTerm}");
            return result;
        }

        public void SetInheritContainerTerm4CurrentList(ScheduleConfiguration config, bool needInitInfo)
        {
            try
            {
                // currently only enforce rule job support inherit container level term
                if (config == null || !config.IsILMode) return;
                mLog.Info($"Get inherit term flag for node: {ID}, level: {SPNodeLevel}");
                if (!needInitInfo)
                {
                    var (hasSetting, isInheritParentTerm) = config.TryGetIsEnableInheritTerm(ID, SPNodeLevel);
                    if (hasSetting)
                    {
                        this.IsInheritContainerTerm = isInheritParentTerm;
                    }
                    else
                    {
                        this.IsInheritContainerTerm = Parent?.IsInheritContainerTerm ?? false;
                    }
                }
                
                if (this.IsInheritContainerTerm)
                {
                    this.ContainerLevelTermId = GetTermIdFromContainer4List(this.SPList);
                    mLog.Info($"{SPNodeLevel} node: {FullPath} has inherit container term, ContainerLevelTermId: {ContainerLevelTermId}");
                }
            }
            catch (Exception e)
            {
                mLog.Warn($"Error in SetInheritContainerTerm4CurrentList for list node: {FullPath}, Ex: {e}");
            }
        }

        private Guid GetTermIdFromContainer4List(IAveList list)
        {
            var containerTermId = Guid.Empty;
            try
            {
                // check List
                if (!TryGetTermFromProps(list?.RootFolder?.ServerRelativeUrl, list?.RootFolder?.Properties, ref containerTermId))
                {
                    // check current Web => parent web... => root web (site collection)
                    var currentWeb = list.ParentWeb;
                    while (currentWeb != null)
                    {
                        if (TryGetTermFromProps(currentWeb.ServerRelativeUrl, currentWeb.AllProperties, ref containerTermId))
                            break;

                        currentWeb = currentWeb.ParentWeb;
                    }
                }
                mLog.Info($"Container level term found for list {list.RootFolder.Url}, termId: {containerTermId}");
            }
            catch (Exception e)
            {
                mLog.Error($"Error while getting container level term settings for list {list.RootFolder.Url}, EX: {e}");
            }

            return containerTermId;
        }

        private bool TryGetTermFromProps(string path, Hashtable props, ref Guid containerTermId)
        {
            try
            {
                if (props == null || !props.ContainsKey(RcordsBuiltInColumn.CONTAINER_BCS_NAME)) return false;

                if (!Guid.TryParse(props[RcordsBuiltInColumn.CONTAINER_BCS_NAME]?.ToString(), out containerTermId))
                {
                    mLog.Warn($"container [{path}] has BCS property but value is not valid. Value: {props[RcordsBuiltInColumn.CONTAINER_BCS_NAME]}");
                    return false;
                }

                var termName = RMSPExplorerDataCache.Instance.Terms.TryGetValue(containerTermId, out var term)
                    ? term.Name
                    : string.Empty;

                mLog.Info($"Container {path} uses container level classification. termId: {containerTermId}, termName: {termName}");
                return true;
            }
            catch (Exception e)
            {
                mLog.Error($"Error while getting container level term settings for container [{path}], EX: {e}");
                return false;
            }
        }

        private bool SetInheritContainerTerm(ScheduleConfiguration config, Guid SPObjectId, NodeLevel nodeLevel, bool isUsingParent = false)
        {
            try
            {
                // currently only enforce rule job support inherit container level term
                if (config == null || !config.IsILMode) return false;
                mLog.Info($"Get inherit term flag for node: {SPObjectId}, level: {nodeLevel}, isUsingParent: {isUsingParent}");
                if (isUsingParent) return this.IsInheritContainerTerm;
                var (hasSetting, isInheritParentTerm) = config.TryGetIsEnableInheritTerm(SPObjectId, nodeLevel);
                if (hasSetting) return isInheritParentTerm;
                else return this.IsInheritContainerTerm; //If not have setting, the parent node's value is used.
            }
            catch (Exception e)
            {
                mLog.Warn($"Error in SetInheritContainerTerm for node: {SPObjectId}, Ex: {e}");
                return false;
            }
        }

        public ArchiverNodeItem GenerateWebAppDefinitionNodeItem(AveDiscoverAppDefinition appDefinition, IAveAppInstance appInstance)
        {
            ArchiverNodeItem result = new ArchiverNodeItem()
            {
                SPNodeLevel = NodeLevel.App,
                ID = appDefinition.ProductId,
                WebApplicationId = this.WebApplicationId,
                SiteId = this.SiteId,
                WebApplicationUrl = this.WebApplicationUrl,
                FullPath = this.FullPath + '/' + appInstance.Title,
                Name = appDefinition.Name,
                Title = appInstance.Title,
                Parent = this,
                DiscoverSPObject = appDefinition,
                Cache_NodeType = (int)CacheNodeType.APP,
                NodeType = ArchiverCommon.NodeType.App,
                SiteUrl = this.SiteUrl,
                WebId = this.ID,
                ShouldDoArchive = true
            };
            if(result.FullPath != null && !result.FullPath.StartWithIgnoreCase("http") 
                && this.SiteUrl != null && this.SiteUrl.Contains('/') && Uri.TryCreate(this.SiteUrl, new UriCreationOptions(), out Uri uri))
            {
                result.FullPath = $"{uri.Scheme}://{uri.Host}/{result.FullPath.Trim('/')}";
            }

            if (!String.IsNullOrEmpty(appDefinition.AppFullUrl))
            {
                result.AppDataName = appDefinition.AppFullUrl.Substring(appDefinition.AppFullUrl.LastIndexOf("/") + 1);
            }
            return result;
        }

        public ArchiverNodeItem GenerateListNodeItem(AveDiscoverList list, IAveList spList)
        {
            bool recordFlag = false;
            if (spList != null)
            {
                recordFlag = ArchiverCommonStaticMethod.CheckListRecord(spList);
            }
            ArchiverNodeItem result = new ArchiverNodeItem
            {
                SPNodeLevel = list.Type == 1 ? NodeLevel.Library : NodeLevel.List,
                ListType = list.Type,
                ID = list.ListId,
                Name = list.Name,
                Title = list.Title,
                WebApplicationId = this.WebApplicationId,
                SiteId = this.SiteId,
                WebId = this.ID,
                WebApplicationUrl = this.WebApplicationUrl,
                FullPath = list.RootFolderUrl,
                Parent = this,
                DiscoverSPObject = list,
                NodeType = ArchiverCommon.NodeType.List,
                ListId = this.ListId,
                Cache_NodeType = (int)CacheNodeType.List,
                IsRecord = recordFlag,
                SiteUrl = spList == null ? this.SiteUrl : spList.ParentWeb.Site.Url,
                IsAppData = this.IsAppData,
                SPList = spList
            };
            if (result.IsAppData)
            {
                result.AppDataName = this.AppDataName + "\\" + result.Name;
            }
            if (this.ShouldDoArchive)
            {
                result.ShouldDoArchive = true;
            }
            if ((list.ListId == Guid.Empty) || list.Type == 1)
            {
                result.NodeType = ArchiverCommon.NodeType.DocList;
            }

            // use IsInheritContainerTerm from parent for needInitInfo process, otherwise will set it later
            result.IsInheritContainerTerm = this.IsInheritContainerTerm;
            //result.IsInheritContainerTerm = SetInheritContainerTerm(config, list.ListId, result.SPNodeLevel);
            return result;
        }

        public ArchiverNodeItem GenerateFolderNodeItem(AveDiscoverFolder folder, NodeLevel nodeLevel)
        {
            ArchiverNodeItem result = new ArchiverNodeItem
            {
                ID = folder.DocID,
                Name = folder.LeafName,
                FullPath = folder.FullUrl,
                SPNodeLevel = nodeLevel,
                WebApplicationId = this.WebApplicationId,
                WebApplicationUrl = this.WebApplicationUrl,
                SiteId = this.SiteId,
                WebId = this.WebId,
                ListId = this.ListId,
                DiscoverSPObject = folder,
                ListType = this.ListType,
                Cache_NodeType = (int)CacheNodeType.Folder,
                Parent = this,
                NodeType = ArchiverCommon.NodeType.Folder,
                mCacheNodeType = this.mCacheNodeType + 1,
                mLibRowId = folder.ID == null ? -1 : folder.ID.Value,
                IsRecord = this.IsRecord,
                SiteUrl = folder.AveFolder.ParentWeb.Site.Url,
            };
            if (nodeLevel == NodeLevel.RootFolder)
            {
                result.Name = AveConstants.ROOT_FOLDER;
                //For Server Farm NewsFeed, DiscoverItem ListId = Guid.Empty
                result.ListId = this.ID;
            }
            return result;
        }
        //由于旧的函数在获取SiteUrl的时候，底层调用GetLists()导致效率降低，直接传入siteUrl可以提高效率
        public ArchiverNodeItem GenerateFolderNodeItem(AveDiscoverFolder folder, NodeLevel nodeLevel, string siteUrl, ScheduleConfiguration config)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.GenerateFolderNodeItem"))
            {
                ArchiverNodeItem result = new ArchiverNodeItem
                {
                    ID = folder.DocID,
                    Name = folder.LeafName,
                    FullPath = folder.FullUrl,
                    SPNodeLevel = nodeLevel,
                    WebApplicationId = this.WebApplicationId,
                    WebApplicationUrl = this.WebApplicationUrl,
                    SiteId = this.SiteId,
                    WebId = this.WebId,
                    ListId = this.ListId,
                    DiscoverSPObject = folder,
                    ListType = this.ListType,
                    Cache_NodeType = (int)CacheNodeType.Folder,
                    Parent = this,
                    NodeType = ArchiverCommon.NodeType.Folder,
                    mCacheNodeType = this.mCacheNodeType + 1,
                    mLibRowId = folder.ID == null ? -1 : folder.ID.Value,
                    IsRecord = this.IsRecord,
                    SiteUrl = siteUrl,
                    IsAppData = this.IsAppData,
                    SPList = this.SPList,
                    Modified = folder.TimeLastModified.Ticks,
                    Created = folder.TimeCreated.Ticks
                };
                if (result.IsAppData)
                {
                    result.AppDataName = this.AppDataName + "\\" + result.Name.TrimStart('\\');
                    result.AppDataName = result.AppDataName.TrimEnd('\\');  //防止{system folder}后面多“\\”;
                }
                if (nodeLevel == NodeLevel.RootFolder)
                {
                    result.Name = AveConstants.ROOT_FOLDER;
                    //For Server Farm NewsFeed, DiscoverItem ListId = Guid.Empty
                    result.ListId = this.ID;
                }
                if (this.ShouldDoArchive)
                {
                    result.ShouldDoArchive = true;
                }
                try
                {
                    //RA Job Use RADisplayColumns and if DisplayColumns have column need Concat & Distinct with RADisplayColumns.ADO-190744
                    //RevIM job获取metadata
                    if (config.IsILMode
                     &&config.RuleCollection != null
                     && config.RuleCollection[1].ProfileType == GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRuleForRevIM
                     && folder.ID != null
                     && !folder.ID.Equals(default(int)))
                    {
                        using (AvePerformanceScope pc1 = new AvePerformanceScope("ArchiverScan.GenerateFolderNodeItem.GetItemColumns"))
                        {
                            IAveListItem item = SPList.GetItemById((int)folder.ID);
                            result.ItemDisplayColumns = GetItemColumns(item, config.BackgroundSettings.RADisplayColumns, true, config.BCSColumnName);
                        }
                    }

                }
                catch (Exception exp)
                {
                    mLog.Warn("Error in Get item columns : " + exp.ToString());
                }
                result.IsInheritContainerTerm = SetInheritContainerTerm(config, folder.DocID, result.SPNodeLevel, true);
                if (result.IsInheritContainerTerm && this.ContainerLevelTermId != Guid.Empty)
                {
                    result.ContainerLevelTermId = this.ContainerLevelTermId;
                }
                mLog.Info($"Get inherit term flag for node: {result.ID}, level: {result.SPNodeLevel}, IsInheritContainerTerm: {result.IsInheritContainerTerm}, ContainerLevelTermId: {result.ContainerLevelTermId}");
                return result;
            }
        }

        public ArchiverNodeItem GenerateItemNodeItem(AveDiscoverItem item, AveDiscoverFolder folder, ScheduleConfiguration config)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.GenerateItemNodeItem"))
            {
                long tempmodified = 0;
                tempmodified = item.TimeLastModified.Ticks;
                long tempcreated = item.TimeCreated.Ticks;
                MicroBlogType microBlogType = MicroBlogType.NotMicroFeed;
                string rootPostId = string.Empty;
                string postId = string.Empty;
                Guid microFeedListId = Guid.Empty;
                bool isMicroFeed = false;
                bool isMicroFeedRef = false;
                //List: {System Folder}'s  item.CurrentItem throw an Exception in Office365 and this List do not need to check NewsFeed
                if (this.ListId != Guid.Empty)
                {
                    if (item.CurrentItem != null && item.CurrentItem.ParentList.BaseTemplate == AveListTemplateType.MicroFeed)
                    {
                        mLog.Info("CurrentItem is MicroFeed.ItemName:{0}.", item.ID);
                        item.ObjType = Wrapper.Common.ItemType.MicroFeedItem;
                        //DefinitionId不为1是Mirco Feed List第二个Folder中的Item，是自动生成的关联信息，不进行处理。 
                        if (item.CurrentItem["DefinitionId"] != null && !item.CurrentItem["DefinitionId"].ToString().Equals("1"))
                        {
                            mLog.Info("CurrentItem is MicroFeedRef.ItemName:{0}.", item.ID);
                            isMicroFeedRef = true;
                        }
                        isMicroFeed = true;
                        microBlogType = item.CurrentItem["MicroBlogType"] != null ? (MicroBlogType)item.CurrentItem["MicroBlogType"] : MicroBlogType.NotMicroFeed;
                        microFeedListId = item.CurrentItem.ParentList.ID;
                        rootPostId = item.CurrentItem["RootPostID"] != null ? item.CurrentItem["RootPostID"].ToString() : string.Empty;
                        postId = item.CurrentItem["ID"] != null ? item.CurrentItem["ID"].ToString() : string.Empty;
                    }
                }
                ArchiverNodeItem result = new ArchiverNodeItem
                {
                    ID = item.DocID,
                    Name = item.ItemName,
                    FullPath = string.Format("{0}\\{1}", folder.FullUrl.TrimEnd('/'), item.LeafName),  //SAAS-23282 处理full url 使得{system folder}下面的document可以正常删除
                    SPNodeLevel = NodeLevel.Item,
                    //For O365 ,the system file's item.ID ==0 ,for Server Farm, the system file's item.ID == null
                    ItemType = ((int)this.ListType != 1 && (item.ID != null) && item.ID != 0) ? ArchiverCommon.ItemType.ITEM_TYPE : ArchiverCommon.ItemType.DOCUMENT,
                    DiscoverSPObject = item,
                    ListType = this.ListType,
                    Cache_NodeType = (int)CacheNodeType.Item,
                    Parent = this,
                    IsSystemObject = item.ID == null || item.ID == 0,
                    mLibRowId = item.ID == null ? -1 : item.ID.Value,
                    Modified = tempmodified,
                    Created = tempcreated,
                    IsMicroFeed = isMicroFeed,
                    IsMicroFeedRef = isMicroFeedRef,
                    MicroType = microBlogType,
                    MicroFeedListID = microFeedListId,
                    RootPostID = rootPostId,
                    PostID = postId,
                    SiteUrl = this.SiteUrl,
                    WebId = this.WebId,
                    ListId = this.ListId,
                    IsAppData = this.IsAppData,
                    SPList = this.SPList,
                    DocumentSize = item.Length,
                    Author = item.Author,
                    Editor = item.Editor
                };
                if (result.IsAppData)
                {
                    result.AppDataName = this.AppDataName + "\\" + result.Name;
                }

                try
                {
                    //RA Job Use RADisplayColumns and if DisplayColumns have column need Concat & Distinct with RADisplayColumns.ADO-190744
                    //message中标识job的属性为IsManualApproval，将config.isRAjob修改为config.currentRule.IsManualApproval
                    if (config.IsILMode && config.RuleCollection != null &&
                    config.RuleCollection[1].ProfileType == GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRuleForRevIM)//RevIM job获取metadata
                    {
                        using (AvePerformanceScope pc1 = new AvePerformanceScope("ArchiverScan.GenerateItemNodeItem.GetItemColumns"))
                        {
                            result.ItemDisplayColumns = GetItemColumns(item.CurrentItem, config.BackgroundSettings.RADisplayColumns, true, config.BCSColumnName);
                        }
                        if (item.CurrentItem != null && item.CurrentItem.Fields != null && item.CurrentItem.Fields.ContainsField("RecordsRelated"))
                        {
                            try
                            {
                                var recordRelatedValue = item.CurrentItem["RecordsRelated"]?.ToString();
                                if (!string.IsNullOrEmpty(recordRelatedValue))
                                {
                                    var relatedItems = RelatedRecordsUtility.GetRelatedProperties(recordRelatedValue);
                                    result.HasRelatedDocument = relatedItems.Count;
                                    result.RelatedRecordInfo = recordRelatedValue;
                                }
                            }
                            catch (Exception ex)
                            {
                                mLog.Warn("Error in get related document column:" + ex.Message);
                                result.HasRelatedDocument = 0;
                            }
                        }
                    }
                }
                catch (Exception exp)
                {
                    mLog.Warn("Error in Get item columns : " + exp.ToString());
                }

                result.IsInheritContainerTerm = SetInheritContainerTerm(config, item.DocID, result.SPNodeLevel, true);
                if (result.IsInheritContainerTerm && this.ContainerLevelTermId != Guid.Empty)
                {
                    result.ContainerLevelTermId = this.ContainerLevelTermId;
                }
                mLog.Info($"Get inherit term flag for node: {result.ID}, level: {result.SPNodeLevel}, IsInheritContainerTerm: {result.IsInheritContainerTerm}, ContainerLevelTermId: {result.ContainerLevelTermId}");
                return result;
            }
        }
        public ArchiverNodeItem GenerateItemNodeItemV2(IAveListItem item, AveDiscoverFolder folder, ScheduleConfiguration config)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.GenerateItemNodeItemV2"))
            {
                long tempmodified = 0;
                tempmodified = Convert.ToDateTime(item.FieldValues["Last_x0020_Modified"].ToString()).Ticks;
                MicroBlogType microBlogType = MicroBlogType.NotMicroFeed;
                string rootPostId = string.Empty;
                string postId = string.Empty;
                Guid microFeedListId = Guid.Empty;
                bool isMicroFeed = false;
                bool isMicroFeedRef = false;
                //List: {System Folder}'s  item.CurrentItem throw an Exception in Office365 and this List do not need to check NewsFeed
                if (this.ListId != Guid.Empty)
                {
                    if (item != null && item.ParentList.BaseTemplate == AveListTemplateType.MicroFeed)
                    {
                        mLog.Info("CurrentItem is MicroFeed.ItemName:{0}.", item.ID);
                        //item.ty = Wrapper.Common.ItemType.MicroFeedItem;
                        //DefinitionId不为1是Mirco Feed List第二个Folder中的Item，是自动生成的关联信息，不进行处理。 
                        if (item["DefinitionId"] != null && !item["DefinitionId"].ToString().Equals("1"))
                        {
                            mLog.Info("CurrentItem is MicroFeedRef.ItemName:{0}.", item.ID);
                            isMicroFeedRef = true;
                        }
                        isMicroFeed = true;
                        microBlogType = item["MicroBlogType"] != null ? (MicroBlogType)item["MicroBlogType"] : MicroBlogType.NotMicroFeed;
                        microFeedListId = item.ParentList.ID;
                        rootPostId = item["RootPostID"] != null ? item["RootPostID"].ToString() : string.Empty;
                        postId = item["ID"] != null ? item["ID"].ToString() : string.Empty;
                    }
                }
                ArchiverNodeItem result = new ArchiverNodeItem
                {
                    ID = item.UniqueId,
                    Name = item.Name,
                    FullPath = string.Format("{0}\\{1}", folder.FullUrl.TrimEnd('/'), item.Name),  //SAAS-23282 处理full url 使得{system folder}下面的document可以正常删除
                    SPNodeLevel = NodeLevel.Item,
                    //For O365 ,the system file's item.ID ==0 ,for Server Farm, the system file's item.ID == null
                    ItemType = ((int)this.ListType != 1 && (item.ID != null) && item.ID != 0) ? ArchiverCommon.ItemType.ITEM_TYPE : ArchiverCommon.ItemType.DOCUMENT,
                    DiscoverSPObject = item,
                    ListType = this.ListType,
                    Cache_NodeType = (int)CacheNodeType.Item,
                    Parent = this,
                    IsSystemObject = item.ID == null || item.ID == 0,
                    mLibRowId = item.ID,
                    Modified = tempmodified,
                    IsMicroFeed = isMicroFeed,
                    IsMicroFeedRef = isMicroFeedRef,
                    MicroType = microBlogType,
                    MicroFeedListID = microFeedListId,
                    RootPostID = rootPostId,
                    PostID = postId,
                    SiteUrl = this.SiteUrl,
                    WebId = this.WebId,
                    ListId = this.ListId,
                    IsAppData = this.IsAppData,
                    SPList = this.SPList
                };
                if (item.Fields != null && item.Fields.ContainsField("File_x0020_Size"))
                {
                    result.DocumentSize = Convert.ToInt64(item["File_x0020_Size"]);
                }
                if (result.IsAppData)
                {
                    result.AppDataName = this.AppDataName + "\\" + result.Name;
                }

                try
                {
                    //RA Job Use RADisplayColumns and if DisplayColumns have column need Concat & Distinct with RADisplayColumns.ADO-190744
                    //message中标识job的属性为IsManualApproval，将config.isRAjob修改为config.currentRule.IsManualApproval

                    if (config.IsILMode && config.RuleCollection != null &&
                        config.RuleCollection[1].ProfileType == GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRuleForRevIM)//RevIM job获取metadata
                    {
                        using (AvePerformanceScope pc1 = new AvePerformanceScope("ArchiverScan.GenerateItemNodeItem.GetItemColumns"))
                        {
                            result.ItemDisplayColumns = GetItemColumns(item, config.BackgroundSettings.RADisplayColumns, true, config.BCSColumnName);
                        }
                        if (item != null && item.Fields != null && item.Fields.ContainsField("RecordsRelated"))
                        {
                            try
                            {
                                var recordRelatedValue = item["RecordsRelated"].ToString();
                                if (!string.IsNullOrEmpty(recordRelatedValue))
                                {
                                    var relatedItems = RelatedRecordsUtility.GetRelatedProperties(recordRelatedValue);
                                    result.HasRelatedDocument = relatedItems.Count;
                                    result.RelatedRecordInfo = recordRelatedValue;
                                }
                            }
                            catch (Exception ex)
                            {
                                mLog.Warn("Error in get related document column:" + ex.Message);
                                result.HasRelatedDocument = 0;
                            }
                        }
                    }
                }
                catch (Exception exp)
                {
                    mLog.Warn("Error in Get item columns : " + exp.ToString());
                }

                result.IsInheritContainerTerm = SetInheritContainerTerm(config, item.UniqueId, result.SPNodeLevel, true);
                if (result.IsInheritContainerTerm && this.ContainerLevelTermId != Guid.Empty)
                {
                    result.ContainerLevelTermId = this.ContainerLevelTermId;
                }
                mLog.Info($"Get inherit term flag for node: {result.ID}, level: {result.SPNodeLevel}, IsInheritContainerTerm: {result.IsInheritContainerTerm}, ContainerLevelTermId: {result.ContainerLevelTermId}");
                return result;
            }
        }

        public ArchiverNodeItem GenerateAttachmentNodeItem(AveItemObject attachment, AveDiscoverFolder folder)
        {
            ArchiverNodeItem result = new ArchiverNodeItem
            {
                ID = attachment.DocID,
                Name = string.Format("{0}:{1}", this.Name, attachment.LeafName),
                FullPath = attachment.FullUrl,//folder.AveFolder.ParentWeb.GetFile("/" + attachment.FullUrl.TrimStart('/')).ServerRelativeUrl,// string.Format("{0}\\{1}", folder.FullUrl, ((AveDiscoverItem)this.DiscoverSPObject).LeafName),
                SPNodeLevel = NodeLevel.Item,
                ItemType = ((int)this.ListType != 1 && (((AveDiscoverItem)this.DiscoverSPObject).ID != null)) ? ArchiverCommon.ItemType.ATTACHMENT : ArchiverCommon.ItemType.ITEM_TYPE,
                DiscoverSPObject = attachment,
                ListType = this.ListType,
                Cache_NodeType = (int)CacheNodeType.Attachment,
                Parent = this,
                SiteUrl = this.SiteUrl,
                SiteId = this.SiteId,
                WebId = this.WebId,
                ListId = this.ListId,
                mLibRowId = attachment.ID == null ? -1 : attachment.ID.Value,
                attachementID = attachment.ParentID,
                IsAppData = this.IsAppData,
                Modified = attachment.TimeLastModified.Ticks,
                DocumentSize = attachment.Length,
                Created = attachment.TimeCreated.Ticks,
                Author = attachment.Author,
                Editor = attachment.Author //Attachment has no Editor info, use Author instead
            };
            if (result.IsAppData)
            {
                result.AppDataName = this.AppDataName + "\\" + result.Name;
            }
            return result;
        }

        public ArchiverNodeItem GenerateAttachmentNodeItemV2(IAveAttachment attachment, AveDiscoverFolder folder)
        {
            ArchiverNodeItem result = new ArchiverNodeItem
            {
                ID = attachment.ROWID,
                Name = string.Format("{0}:{1}", this.Name, attachment.FileName),
                FullPath = attachment.ServerRelativeUrl,//folder.AveFolder.ParentWeb.GetFile("/" + attachment.FullUrl.TrimStart('/')).ServerRelativeUrl,// string.Format("{0}\\{1}", folder.FullUrl, ((AveDiscoverItem)this.DiscoverSPObject).LeafName),
                SPNodeLevel = NodeLevel.Item,
                ItemType = ((int)this.ListType != 1 && (((IAveListItem)this.DiscoverSPObject).ID != null)) ? ArchiverCommon.ItemType.ATTACHMENT : ArchiverCommon.ItemType.ITEM_TYPE,
                DiscoverSPObject = attachment,
                ListType = this.ListType,
                Cache_NodeType = (int)CacheNodeType.Attachment,
                Parent = this,
                SiteUrl = this.SiteUrl,
                SiteId = this.SiteId,
                WebId = this.WebId,
                ListId = this.ListId,
                mLibRowId = -1,
                attachementID = attachment.GetParentId(),
                IsAppData = this.IsAppData
            };
            if (result.IsAppData)
            {
                result.AppDataName = this.AppDataName + "\\" + result.Name;
            }
            return result;
        }

        public ArchiverNodeItem GenerateAttachmentNodeFolder(AveItemObject attachment, AveDiscoverFolder folder)
        {
            ArchiverNodeItem result = new ArchiverNodeItem
            {
                ID = attachment.DocID,
                Name = string.Format("{0}_.{1}:{2}", folder.ID, this.Name, attachment.LeafName),
                FullPath = string.Format("{0}\\{1}", folder.FullUrl, ((AveDiscoverFolder)this.DiscoverSPObject).LeafName),
                SPNodeLevel = NodeLevel.Item,
                ItemType = ((int)this.ListType != 1 && (((AveDiscoverFolder)this.DiscoverSPObject) != null)) ? ArchiverCommon.ItemType.ATTACHMENT : ArchiverCommon.ItemType.FOLDER_DOCLIB,
                DiscoverSPObject = attachment,
                ListType = this.ListType,
                Cache_NodeType = (int)CacheNodeType.Attachment,
                Parent = this,
                SiteUrl = this.SiteUrl,
                SiteId = this.SiteId,
                WebId = this.WebId,
                ListId = this.ListId,
                mLibRowId = folder.ID == null ? -1 : folder.ID.Value,
                attachementID = attachment.ParentID,
                IsAppData = this.IsAppData
            };
            if (result.IsAppData)
            {
                result.AppDataName = this.AppDataName + "\\" + result.Name;
            }
            return result;
        }

        public ArchiverNodeItem GenerateAttachmentNodeFolderV2(IAveAttachment attachment, AveDiscoverFolder folder)
        {
            ArchiverNodeItem result = new ArchiverNodeItem
            {
                ID = attachment.ROWID,
                Name = string.Format("{0}_.{1}:{2}", folder.ID, this.Name, attachment.FileName),
                FullPath = string.Format("{0}\\{1}", folder.FullUrl, ((AveDiscoverFolder)this.DiscoverSPObject).LeafName),
                SPNodeLevel = NodeLevel.Item,
                ItemType = ((int)this.ListType != 1 && (((AveDiscoverFolder)this.DiscoverSPObject) != null)) ? ArchiverCommon.ItemType.ATTACHMENT : ArchiverCommon.ItemType.FOLDER_DOCLIB,
                DiscoverSPObject = attachment,
                ListType = this.ListType,
                Cache_NodeType = (int)CacheNodeType.Attachment,
                Parent = this,
                SiteUrl = this.SiteUrl,
                SiteId = this.SiteId,
                WebId = this.WebId,
                ListId = this.ListId,
                mLibRowId = folder.ID == null ? -1 : folder.ID.Value,
                attachementID = attachment.GetParentId(),
                IsAppData = this.IsAppData
            };
            if (result.IsAppData)
            {
                result.AppDataName = this.AppDataName + "\\" + result.Name;
            }
            return result;
        }

        public ArchiverNodeItem GenerateItemVersionNodeItem(AveVersionObject version, ArchiverNodeItem item, ScheduleConfiguration config)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.GenerateItemVersionNodeItem"))
            {
                ArchiverNodeItem result = new ArchiverNodeItem
                {
                    ID = this.ID,
                    Name = string.Format("{0}:{1}", ((AveDiscoverItem)this.DiscoverSPObject).LeafName, this.GenerateVersionLable(version.Uiversion)),
                    FullPath = item.FullPath + ":" + this.GenerateVersionLable(version.Uiversion),
                    SPNodeLevel = NodeLevel.Item,
                    ItemType = ((int)this.ListType != 1 && (((AveDiscoverItem)this.DiscoverSPObject).ID != null)) ? ArchiverCommon.ItemType.ITEM_VERSION : ArchiverCommon.ItemType.DOCUMENT_VER,
                    DiscoverSPObject = version,
                    ListType = this.ListType,
                    Cache_NodeType = (int)CacheNodeType.ItemVersion,
                    Parent = this,
                    SiteUrl = this.SiteUrl,
                    SiteId = this.SiteId,
                    WebId = this.WebId,
                    ListId = this.ListId,
                    mLibRowId = item.mLibRowId,
                    IsAppData = this.IsAppData,
                    //
                    //Modified = this.Modified,//Version暂时使用Parent Modified Time，用来检查文件Modified time是否改变
                    DocumentSize = version.Size,
                    Modified = version.TimeLastModified.Ticks,
                    Created = this.Created,
                    Author = this.Author,
                    Editor = this.Editor
                };
                if (result.IsAppData)
                {
                    result.AppDataName = this.AppDataName + "\\" + result.Name;
                }
                if (config.IsILMode && config.RuleCollection != null &&
                    config.RuleCollection[1].ProfileType == GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRuleForRevIM)//RevIM job获取metadata
                {
                    using (AvePerformanceScope pc1 = new AvePerformanceScope("ArchiverScan.GenerateItemVersionNodeItem.GetItemVersionColumns"))
                    {
                        result.ItemDisplayColumns = item.ItemDisplayColumns;
                        //mLog.Info("Current rule is ArchiverRuleForRevIM job and need get version information,Item Path:{0}.", result.FullPath);
                        //Hashtable columnCollectionOfDisplayName = new Hashtable(StringComparer.OrdinalIgnoreCase);
                        //try
                        //{
                        //    IAveListItemVersion aveVersion = ((AveDiscoverItem)item.DiscoverSPObject).CurrentItem.Versions.GetVersionFromID(version.Uiversion);
                        //    columnCollectionOfDisplayName["content type"] = aveVersion.ListItem.ContentType.Name.ToString();
                        //    columnCollectionOfDisplayName["author"] = GetVersionAuthorOrEditorInfo(aveVersion, true);
                        //    columnCollectionOfDisplayName["editor"] = GetVersionAuthorOrEditorInfo(aveVersion, false);
                        //    result.ItemDisplayColumns = columnCollectionOfDisplayName;
                        //}
                        //catch (Exception e)
                        //{
                        //    mLog.Warn("Get Version Properties Error:{0}.", e.ToString());
                        //}
                    }
                }
                return result;
            }
        }

        public ArchiverNodeItem GenerateItemVersionNodeItemV2(IAveListItemVersion version, ArchiverNodeItem item, ScheduleConfiguration config)
        {

            ArchiverNodeItem result = new ArchiverNodeItem
            {
                ID = this.ID,
                Name = string.Format("{0}:{1}", ((IAveListItem)this.DiscoverSPObject).Name, this.GenerateVersionLable(version.VersionId)),
                FullPath = item.FullPath + ":" + this.GenerateVersionLable(version.VersionId),
                SPNodeLevel = NodeLevel.Item,
                ItemType = ((int)this.ListType != 1 && (((IAveListItem)this.DiscoverSPObject).ID != null)) ? ArchiverCommon.ItemType.ITEM_VERSION : ArchiverCommon.ItemType.DOCUMENT_VER,
                DiscoverSPObject = version,
                ListType = this.ListType,
                Cache_NodeType = (int)CacheNodeType.ItemVersion,
                Parent = this,
                SiteUrl = this.SiteUrl,
                SiteId = this.SiteId,
                WebId = this.WebId,
                ListId = this.ListId,
                mLibRowId = item.mLibRowId,
                IsAppData = this.IsAppData,
                //Modified = this.Modified,//Version暂时使用Parent Modified Time，用来检查文件Modified time是否改变
                Modified = version.Created.Ticks,
                //DocumentSize = ,|
            };
            if (result.IsAppData)
            {
                result.AppDataName = this.AppDataName + "\\" + result.Name;
            }
            if (config.IsILMode && config.RuleCollection != null &&
                config.RuleCollection[1].ProfileType == GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRuleForRevIM)//RevIM job获取metadata
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.GenerateItemVersionNodeItem.GetItemVersionColumns"))
                {
                    result.ItemDisplayColumns = item.ItemDisplayColumns;
                    //mLog.Info("Current rule is ArchiverRuleForRevIM job and need get version information,Item Path:{0}.", result.FullPath);
                    //Hashtable columnCollectionOfDisplayName = new Hashtable(StringComparer.OrdinalIgnoreCase);
                    //try
                    //{
                    //    IAveListItemVersion aveVersion = ((AveDiscoverItem)item.DiscoverSPObject).CurrentItem.Versions.GetVersionFromID(version.Uiversion);
                    //    columnCollectionOfDisplayName["content type"] = aveVersion.ListItem.ContentType.Name.ToString();
                    //    columnCollectionOfDisplayName["author"] = GetVersionAuthorOrEditorInfo(aveVersion, true);
                    //    columnCollectionOfDisplayName["editor"] = GetVersionAuthorOrEditorInfo(aveVersion, false);
                    //    result.ItemDisplayColumns = columnCollectionOfDisplayName;
                    //}
                    //catch (Exception e)
                    //{
                    //    mLog.Warn("Get Version Properties Error:{0}.", e.ToString());
                    //}
                }
            }
            return result;
        }

        /*private static string GetVersionAuthorOrEditorInfo(IAveListItemVersion version, bool authorOrEditor)
        {
            string ModifieName = String.Empty;
            try
            {
                string columnName = authorOrEditor ? "Author" : "Editor";
                string itemUserInfo = version[columnName].ToString();
                string[] sArray = itemUserInfo.Split(new Char[] { ';', '#' }, StringSplitOptions.RemoveEmptyEntries);
                //result.ModifiedByTitle = sArray[1].ToString();
                IAveUser user = version.ListItem.ParentList.ParentWeb.SiteUsers.GetByID(int.Parse(sArray[0].ToString()));
                if (user != null)
                {
                    ModifieName = user.Name;
                    //result.ModifiedByTitle = user.Name;
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Can not GetVersionAuthorOrEditorInfo,Message:{0}.", ex.ToString());
            }
            return ModifieName;
        }*/

        public ArchiverNodeItem GenerateFolderVersionNodeItem(AveVersionObject version, NodeLevel nodeLevel, AveDiscoverFolder disFolder)
        {
            ArchiverNodeItem result = new ArchiverNodeItem()
            {
                ID = this.ID,
                Name = disFolder.LeafName + ":" + GenerateVersionLable(version.Uiversion),
                FullPath = this.FullPath,
                SPNodeLevel = nodeLevel,
                WebApplicationId = this.WebApplicationId,
                WebApplicationUrl = this.WebApplicationUrl,
                SiteUrl = this.SiteUrl,
                SiteId = this.SiteId,
                WebId = this.WebId,
                ListId = this.ListId,
                DiscoverSPObject = version,
                ListType = this.ListType,
                Cache_NodeType = this.Cache_NodeType,
                Parent = this,
                NodeType = ArchiverCommon.NodeType.Folder,
                ShouldDoArchive = true,
                ArchiveLevel = false,
                mLibRowId = disFolder.ID == null ? -1 : disFolder.ID.Value,
                IsAppData = this.IsAppData
                // mCacheNodeType = this.Cache_NodeType+1
            };
            if (result.IsAppData)
            {
                result.AppDataName = this.AppDataName + "\\" + result.Name;
            }
            return result;
        }

        public ApproveReport ConvertToApproveReport()
        {
            ApproveReport result = new ApproveReport();
            result.ScanTime = DateTime.UtcNow.Ticks;//arthur: maybe need pass this value from outside
            result.FullPath = this.FullPath;
            result.LeafName = this.Name == null ? "null" : this.Name;
            result.LibRowId = this.mLibRowId;
            result.NodeId = this.ID.ToString();
            //result.NodeType = this.Cache_NodeType;
            result.NodeType = this.Cache_NodeType >= 10000 ? (int)this.ItemType : (int)this.NodeType;
            result.SPNodeLevel = (int)this.SPNodeLevel;
            result.CacheNodeType = this.Cache_NodeType;
            result.ParentId = this.Parent == null ? Guid.Empty.ToString() : this.Parent.ID.ToString();
            result.UIVersion = this.UIVersion;
            result.ArchiveLevel = this.ArchiveLevel;



            result.Approval = false;
            result.Level = this.Level;
            //result.StorageId = this.StoragePolicyId==string.Empty? null:this.StoragePolicyId;
            result.RuleId = this.RuleId == null ? null : this.RuleId.ToString();
            result.RuleName = this.RuleName == null ? null : this.RuleName;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ArchiveApproveReport ConvertToArchiveApproveReport(SOApproveDBStatus status = SOApproveDBStatus.Approved)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.NodeItem.ConvertToArchiveApproveReport"))
            {
                ArchiveApproveReport result = new ArchiveApproveReport();
                result.ScanTime = DateTime.UtcNow.Ticks;//arthur: maybe need pass this value from outside
                result.FullPath = this.FullPath;
                result.LeafName = this.Name == null ? "null" : this.Name;
                result.LibRowId = this.mLibRowId;
                result.NodeId = this.ID.ToString();
                //result.NodeType = this.Cache_NodeType;
                result.NodeType = this.Cache_NodeType >= 10000 ? (int)this.ItemType : (int)this.NodeType;
                //result.SPNodeLevel = (int)this.SPNodeLevel;
                //区分item,document及它们的version，SPNodeLevel：item为500，document为505，item version为550，document version为555.
                switch (this.ItemType)
                {
                    case ArchiverCommon.ItemType.ITEM_TYPE:
                        {
                            result.SPNodeLevel = 500;
                            if (this.DiscoverSPObject is AveDiscoverItem)
                            {
                                result.ItemObject = (this.DiscoverSPObject as AveDiscoverItem).CurrentItem;
                            }
                            break;
                        }
                    case ArchiverCommon.ItemType.ITEM_VERSION:
                        {
                            result.SPNodeLevel = 550;
                            break;
                        }
                    case ArchiverCommon.ItemType.DOCUMENT:
                        {
                            result.SPNodeLevel = 505;
                            if (this.DiscoverSPObject is AveDiscoverItem)
                            {
                                result.ItemObject = (this.DiscoverSPObject as AveDiscoverItem).CurrentItem;
                            }
                            break;
                        }
                    case ArchiverCommon.ItemType.DOCUMENT_VER:
                        {
                            result.SPNodeLevel = 555;
                            break;
                        }
                    default:
                        {
                            result.SPNodeLevel = (int)this.SPNodeLevel;
                            break;
                        }
                }
                result.CacheNodeType = this.Cache_NodeType;
                result.ParentId = this.Parent == null ? Guid.Empty.ToString() : this.Parent.ID.ToString();
                result.UIVersion = this.UIVersion;
                //result.ArchiveLevel = this.ArchiveLevel;


                result.Status = status;
                result.Level = this.Level;
                //result.StorageId = this.StoragePolicyId==string.Empty? null:this.StoragePolicyId;
                result.RuleId = this.RuleId == null ? null : this.RuleId;
                result.RuleName = this.RuleName == null ? null : this.RuleName;
                result.RuleArchiverAction = this.RuleArchiverAction == null ? null : this.RuleArchiverAction;
                result.IsAppData = this.IsAppData;
                result.AppDataName = this.AppDataName;
                result.SourceFlag = (int)SOSourceFlag.SharePoint;
                result.HasRelatedDocument = this.HasRelatedDocument;
                result.DeleteRelatedRecords = this.DeleteRelatedRecords;
                result.RelatedRecordInfo = this.RelatedRecordInfo;

                //add for test run job
                result.DocumentSize = this.DocumentSize;
                result.Created = this.Created;
                result.CreatedBy = this.CreatedBy;
                result.Modified = this.Modified;
                result.ModifiedBy = this.ModifiedBy;
                result.ActionTaken = this.ActionTaken;
                result.SiteUrl = this.SiteUrl == null ? string.Empty : this.SiteUrl;
                result.WebID = this.WebId;
                result.ListID = this.ListId;
                result.IsAppData = this.IsAppData;
                result.JsonMeta = GetJsonMeta(result);
                result.SiteTitle = this.SiteTitle;
                result.DoDelete = this.DoDelete;
                result.Author = this.Author;
                result.Editor = this.Editor;
                result.ManifestDocumentSnapshot = this.ManifestSnapshot;
                if (ForcedNotReport)
                {
                    result.ShouldAddDetails = false;
                }
                else if (ForcedReport)
                {
                    result.ShouldAddDetails = true;
                }
                else if (this.Parent != null && !string.IsNullOrEmpty(this.Parent.RuleId) && this.Parent.DoDelete)
                {
                    result.ShouldAddDetails = false;
                }
                else
                {
                    result.ShouldAddDetails = true;
                }

                if (RulePolicyLevel == (int)PolicyLevel.Teams)
                {
                    result.ShouldAddDetails = false;
                }

                return result;
            }
        }

        private string GetJsonMeta(ArchiveApproveReport report)
        {
            try
            {
                ArchiverSharePointDto dto = new ArchiverSharePointDto()
                {
                    LeafName = this.Name,
                    Path = this.FullPath,
                    ArchivedTime = DateTime.UtcNow,
                    Metadata = GetMetaData(),
                    ScopeID = this.ListId,
                    SPNodeLevel = report.SPNodeLevel,
                    CreatedTime = Created,
                    CDLastModifiedTime = Modified,
                    FileType = GetItemExtension(Name),
                };

                string recordsId = string.Empty;
                if (this.ItemDisplayColumns!=null && this.ItemDisplayColumns.ContainsKey(SPColumnConstants.DocumentId))
                {
                    recordsId = this.ItemDisplayColumns[SPColumnConstants.DocumentId]?.ToString();
                }
                else if (this.ItemDisplayColumns != null && this.ItemDisplayColumns.ContainsKey(RcordsBuiltInColumn.UNIQUEID_NAME))
                {
                    recordsId = this.ItemDisplayColumns[RcordsBuiltInColumn.UNIQUEID_NAME]?.ToString();
                }
                dto.RecordsId = recordsId;

                return JsonConvert.SerializeObject(dto);
            }
            catch (Exception e)
            {
                mLog.Warn($"GetJsonMeta error: {e}");
                return "";
            }
        }

        private string GetItemExtension(string objectName)
        {
            try
            {
                if (this.SPList == null)
                {
                    return string.Empty;
                }
                if (this.ItemType is ArchiverCommon.ItemType.ITEM_TYPE or ArchiverCommon.ItemType.DOCUMENT)
                {
                    var result = string.Empty;
                    if (this.SPList.BaseType == AveBaseType.DocumentLibrary)
                    {
                        var ext = System.IO.Path.GetExtension(objectName);
                        result = ext.IndexOf(".") >= 0 ? ext.Substring(1) : "RM_RDM_RecordDetails_DataType_FileNull";
                    }
                    else
                    {
                        result = "RM_RDM_RecordDetails_DataType_SPItem";
                    }
                    return result;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception e)
            {
                mLog.Warn($"GetItemExtension error: {e}");
                return string.Empty;
            }
        }

        public string GetMetaData()
        {
            Hashtable columns = this.ItemDisplayColumns;
            if (columns != null && columns.Count > 0)
            {
                XmlDocument doc = new XmlDocument();
                XmlElement xe = doc.CreateElement("MetaData");
                foreach (var column in columns.Keys)
                {
                    XmlElement colXe = doc.CreateElement("Column");
                    colXe.SetAttribute("Name", column.ToString());
                    string value = columns[column].ToString();
                    if (value.Contains(delimiter))
                    {
                        string[] values = value.Split(delimiter);
                        colXe.SetAttribute("Value", values[0].ToString());
                        colXe.SetAttribute("ExtendValue", values[1].ToString());
                    }
                    else
                    {
                        colXe.SetAttribute("Value", columns[column].ToString());
                    }
                    xe.AppendChild(colXe);
                }
                return xe.OuterXml;
            }
            return null;
        }

        /// <summary>
        /// 1.获取Item相关Column Value，可以通过Display Name获取，也可以通过Internal Name获取
        /// 2.先通过Display Name获取，如果Display Name获取不到则通过Internal Name获取
        /// 3.通过不同Name获取，返回不同Name的Key+value
        /// 4.RA Job need get BCS Column by BCSColumnID Default.
        /// </summary>
        public Hashtable GetItemColumns(IAveListItem item, List<string> fieldNames, bool isRAJob, string bcsColumnName)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.GetItemColumns"))
            {
                Hashtable columnCollectionOfDisplayName = new Hashtable(StringComparer.OrdinalIgnoreCase);
                if (item != null)
                {
                    #region get RA BCSColumn
                    if (isRAJob)
                    {
                        try
                        {
                            IAveField field = null;
                            if (!string.IsNullOrEmpty(bcsColumnName))
                            {
                                field = item.Fields.GetField(bcsColumnName);
                            }
                            //如果为空，就取BCS Column
                            if (field == null)
                            {
                                string BCSColumnID = "20f84bba906045b4af568ee102a52dcb";
                                field = item.Fields.GetFieldById(new Guid(BCSColumnID), false);
                            }
                            if (field.Type == AveFieldType.Invalid)
                            {
                                var fileObj = item[field.ID];
                                if (fileObj != null)
                                {
                                    if (fileObj.GetType() != typeof(string))
                                    {
                                        var dic = ((Dictionary<string, object>)item[field.ID]);
                                        var termName = dic["Label"].ToString();
                                        var termId = new Guid(dic["TermGuid"].ToString());
                                        columnCollectionOfDisplayName[bcsColumnName] = field.GetFieldValueAsText(item[field.ID]) + delimiter.ToString() + termName + "|" + termId;
                                    }
                                    else
                                    {
                                        columnCollectionOfDisplayName[bcsColumnName] = field.GetFieldValueAsText(item[field.ID]) + delimiter.ToString() + item[field.ID];
                                    }
                                }
                                else
                                {
                                    mLog.Info($"Can not get RA BCS Column property when get item columns.bcsColumnName:{bcsColumnName}.");
                                }
                            }
                            else
                            {
                                mLog.Info("BCSColumnID exist but column type is not Invalid.Field Type:{0}.", field.Type.ToString());
                            }
                        }
                        catch (Exception ex)
                        {
                            mLog.Info($"Can not get RA BCS Column property when get item columns.bcsColumnName:{bcsColumnName}.Message:{ex.ToString()}.");
                        }
                    }
                    #endregion
                    foreach (var fieldName in fieldNames)
                    {
                        bool isGetColumnByInternalName = false;
                        IAveField field = null;
                        try
                        {
                            if (fieldName.Equals("Content Type", StringComparison.OrdinalIgnoreCase) || fieldName.Equals("ContentType", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    columnCollectionOfDisplayName[fieldName.ToLower(CultureInfo.CurrentCulture)] = item.ContentType.Name;
                                }
                                catch (Exception ex)
                                {
                                    mLog.Info("Can not get content type property when get item columns.Message:{0}.", ex.Message);
                                }
                                continue;
                            }
                            field = item.Fields[fieldName];
                        }
                        catch (Exception e)
                        {
                            try
                            {
                                field = item.Fields.GetFieldByInternalName(fieldName);
                                isGetColumnByInternalName = true;
                            }
                            catch (Exception ex)
                            {
                                mLog.Info("Can not get field by internal name when get item columns.FieldName:{0}.Message:{1}.", fieldName, ex.Message);
                                columnCollectionOfDisplayName[fieldName.ToLower(CultureInfo.CurrentCulture)] = string.Empty;
                                continue;
                            }
                        }
                        try
                        {
                            string fieldTitle = field.Title.ToLower(CultureInfo.CurrentCulture);//RA Need Lower
                            string fieldInternalName = field.InternalName.ToLower(CultureInfo.InvariantCulture);
                            if (field.Hidden)
                            {
                                mLog.Info("Current field is hidden, field Name:{0}.", fieldTitle);
                                continue;
                            }
                            if (item[field.ID] == null)
                            {
                                if (field.Type != AveFieldType.Number && field.Type != AveFieldType.DateTime && field.Type != AveFieldType.Boolean && field.Type != AveFieldType.Integer)
                                {//text match * need this.        
                                    columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = string.Empty;
                                }
                                continue;
                            }
                            switch (field.Type)
                            {
                                //在rule判断时，会判断数据类型。
                                case AveFieldType.Boolean:
                                case AveFieldType.Number:
                                    columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = item[field.ID];
                                    break;
                                case AveFieldType.Counter:
                                    columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = Convert.ToDouble(item[field.ID]);
                                    break;
                                case AveFieldType.DateTime:
                                    columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = ToUniversalTimeWithTimeZone((DateTime)item[field.ID], item.Web);
                                    break;
                                case AveFieldType.User:
                                    var value = item[field.ID];
                                    var stringVlue = value as string;
                                    if (stringVlue != null)
                                    {
                                        columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = stringVlue.Substring(stringVlue.IndexOf('#') + 1);
                                    }
                                    else if (value is IEnumerable)
                                    {
                                        StringBuilder users = new StringBuilder();
                                        foreach (var userinfo in (value as IEnumerable))
                                        {
                                            var user = userinfo.ToString();
                                            users.Append(user.Substring(user.IndexOf('#') + 1));
                                            users.Append(';');
                                        }
                                        users.Length = Math.Max(0, users.Length - 1);
                                        columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = users.ToString();
                                    }
                                    else
                                    {
                                        columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = value;
                                    }
                                    break;
                                case AveFieldType.Lookup:
                                    var lookupValue = item[field.ID];
                                    var realValue = lookupValue as IAveFieldLookupValue;
                                    if (realValue != null)
                                    {
                                        columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = realValue.LookupValue;
                                    }
                                    else if (string.Equals(field.TypeAsString, "Lookup", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(field.TypeAsString, "LookupMulti", StringComparison.OrdinalIgnoreCase))
                                    {
                                        columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = field.GetFieldValueAsText(lookupValue);
                                    }
                                    else
                                    {
                                        columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = lookupValue;
                                    }
                                    break;
                                case AveFieldType.Invalid:
                                    if (string.Equals(field.TypeAsString, "TaxonomyFieldType", StringComparison.OrdinalIgnoreCase) ||
                                        string.Equals(field.TypeAsString, "TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                                    {
                                        columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = field.GetFieldValueAsText(item[field.ID]) + delimiter.ToString() + item[field.ID].ToString();
                                    }
                                    else
                                    {
                                        columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = item[field.ID];
                                    }
                                    break;
                                case AveFieldType.ModStat:
                                    columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = field.GetFieldValueAsText(item[field.ID]);
                                    break;
                                default:
                                    columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = field.GetFieldValueAsText(item[field.ID]);
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            mLog.Info(string.Format("Get the metadata of item error.Field Name:{0}.Exception:{1}", field.Title, ex.Message));
                        }
                    }
                }
                return columnCollectionOfDisplayName;
            }
        }

        private DateTime ToUniversalTimeWithTimeZone(DateTime datetime, IAveWeb web)
        {
            if (datetime.Kind != DateTimeKind.Utc)
            {
                datetime = web.RegionalSettings.TimeZone.LocalTimeToUTC(datetime);
            }
            return datetime;
        }

        public bool CheckIsBlockDeleteOnlyRecord(IAveList tmpList)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.CheckIsBlockDeleteOnlyRecord"))
            {
                if (null == holdAndRecordStatus)
                {
                    IAveListItem spitem = tmpList.GetItemById(this.LibRowID);
                    int result = 0;
                    try
                    {
                        object obj = spitem[new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E")];
                        if (obj != null && !int.TryParse(obj.ToString(), out result)) result = 0;
                    }
                    catch (ArgumentException ex)
                    {
                        mLog.Info(ex.ToString());
                        result = 0;
                    }
                    holdAndRecordStatus = result;
                }
                return ArchiverCommonStaticMethod.IsBlockDeleteOnlyRecord(holdAndRecordStatus ?? 0);
            }
        }


        public bool CheckisRecord(IAveList tmpList)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.CheckisRecord"))
            {
                if (null == holdAndRecordStatus)
                {
                    IAveListItem spitem = tmpList.GetItemById(this.LibRowID);
                    int result = 0;
                    try
                    {
                        object obj = spitem[new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E")];
                        if (obj != null && !int.TryParse(obj.ToString(), out result)) result = 0;
                    }
                    catch (ArgumentException ex)
                    {
                        mLog.Info(ex.ToString());
                        result = 0;
                    }
                    holdAndRecordStatus = result;
                }
                return ArchiverCommonStaticMethod.IsRecord(holdAndRecordStatus ?? 0);
            }
        }

        public bool CheckIsHoldOnly(IAveList tmpList)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.CheckIsHoldOnly"))
            {
                if (null == holdAndRecordStatus)
                {
                    IAveListItem spitem = tmpList.GetItemById(this.LibRowID);
                    int result = 0;
                    try
                    {
                        object obj = spitem[new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E")];
                        if (obj != null && !int.TryParse(obj.ToString(), out result)) result = 0;
                    }
                    catch (ArgumentException ex)
                    {
                        mLog.Info(ex.ToString());
                        result = 0;
                    }
                    holdAndRecordStatus = result;
                }
                return ArchiverCommonStaticMethod.IsHoldOnly(holdAndRecordStatus ?? 0);
            }
        }

        public bool CheckHasHold(IAveList tmpList)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.CheckIsHoldOnly"))
            {
                if (null == holdAndRecordStatus)
                {
                    IAveListItem spitem = tmpList.GetItemById(this.LibRowID);
                    int result = 0;
                    try
                    {
                        object obj = spitem[new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E")];
                        if (obj != null && !int.TryParse(obj.ToString(), out result)) result = 0;
                    }
                    catch (ArgumentException ex)
                    {
                        mLog.Info(ex.ToString());
                        result = 0;
                    }
                    holdAndRecordStatus = result;
                }
                return ArchiverCommonStaticMethod.IsOnHold(holdAndRecordStatus ?? 0);
            }
        }
        #endregion

        #region private functions
        private string GenerateVersionLable(int version)
        {
            return string.Format("{0}.{1}", (version / 512), (version % 512));
        }
        
        private string HandleAveUserName(string userLoginName)
        {
            if (string.IsNullOrEmpty(userLoginName))
                return userLoginName;

            if (userLoginName.Contains("|"))
            {
                int targetIndex = userLoginName.LastIndexOf("|");
                return userLoginName.Substring(targetIndex + 1);
            }
            else if (userLoginName.Contains(";#"))
            {
                return userLoginName.Split(";#")[0];
            }
            return userLoginName;
        }

        #endregion
        public void Dispose()
        {
            //dispose discoverspobject
            if (DiscoverSPObject != null)
            {
                if (DiscoverSPObject is AveDiscoverItem)
                {
                    using ((AveDiscoverItem)DiscoverSPObject) { }
                }
            }
        }
    }

    /// <summary>
    /// This is used for Micro Feed Archiver
    /// </summary>
    public enum MicroBlogType
    {
        NotMicroFeed = 0,
        Post = 2,
        Reply = 4,
        Ref = 32,
    }
}