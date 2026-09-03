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
using AvePoint.RA.CommonUtil;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Common.CAMLHelper.CAML;
using AvePoint.RA.SharePoint.RMSharePointTaxnomy;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Configuration;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.SharePoint.RelatedRecords;
using Microsoft.SharePoint.Client.DocumentManagement;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.Contract.Tenant;

namespace AvePoint.RA.SharePoint.RMSharePointColumn
{
    public class RMSharePointColumn : IDisposable
    {

        private RALogger logger = RALogger.GetInstance(typeof(RMSharePointColumn));

        #region construct method
        public RMSharePointColumn(RMSPTreeNode sitenode)
        {
            //RMSPTreeService = PlatformWindsorManager.GetService(typeof(ISPSettingTreeService)) as ISPSettingTreeService;
            //SharePointSettingDao = PlatformWindsorManager.GetService(typeof(ISharePointSettingDao)) as ISharePointSettingDao;
            JobService = PlatformWindsorManager.GetService(typeof(IJobMonitorService)) as IJobMonitorService;
            //TermSetDao = PlatformWindsorManager.GetService(typeof(ITermSetDao)) as ITermSetDao;
            //TermDao = PlatformWindsorManager.GetService(typeof(ITermDao)) as ITermDao;
            this.siteCollectionTreeNode = sitenode;
            InitContext(sitenode);
        }
        public RMSharePointColumn()
        {
            //RMSPTreeService = PlatformWindsorManager.GetService(typeof(ISPSettingTreeService)) as ISPSettingTreeService;
            //SharePointSettingDao = PlatformWindsorManager.GetService(typeof(ISharePointSettingDao)) as ISharePointSettingDao;
            JobService = PlatformWindsorManager.GetService(typeof(IJobMonitorService)) as IJobMonitorService;
            //TermSetDao = PlatformWindsorManager.GetService(typeof(ITermSetDao)) as ITermSetDao;
            //TermDao = PlatformWindsorManager.GetService(typeof(ITermDao)) as ITermDao;
        }
        private RMSPTreeNode siteCollectionTreeNode { get; set; }//current sitecollection node
        private string columnDisplayName = string.Empty;
        private string classification = string.Empty;

        #endregion

        #region SharePoint Client Object
        private List<string> DesignLists = new List<string>();
        private Site mSite { get; set; }
        private Web mWeb { get; set; }// column web root web. 
        private ClientContext mClientContext { get; set; }
        private RMSharePointTaxonomy mTaxonomy { get; set; }
        #endregion

        #region property for set default value
        private Term m_term;
        private string m_internalName;
        private TaxonomyField taxField;
        private uint limitedCount;
        private bool m_needCheckDefaultVaule;
        #endregion

        #region property for job 
        public JobType jobType { get; set; }
        //private int processListsCount;
        //public int totalListCounts;
        //public int currentjobProgress;
        // public int nextJobProgress;
        public Dictionary<Guid, int> settingResults = new Dictionary<Guid, int>();
        public string jobId;
        public List<JMGlobalSettingJobDetails> SPSettingJobDetails = new List<JMGlobalSettingJobDetails>();
        #endregion

        #region const string value
        private Guid RevIMClassificationColumnID
        {
            get
            {
                return new Guid("20f84bba906045b4af568ee102a52dcb");
            }
        }
        private const string BCSColumnName = "RevIMBCS";



        private const string RAI18N_UpdatePhysicalColumnFailed = "RM_SS_UpdatePhysicalFieldFailed";
        private string RAI18N_ConfigPhysicalAction
        {
            get { return I18NEntity.GetString("RM_SS_ConfigPhysicalAction"); }
        }
        private Guid relatedColumnId = new Guid("b40273fb-26d2-40e8-9a34-dd20bc9ca1d7");
        private const string relatedColumnInternalName = "RecordsRelated";
        private  string relatedColumnDisplayName
        {
            get { return I18NEntity.GetString("RM_SS_RelatedRecords"); }
        }
        #endregion

        #region Dao Interface
        private IContainerDao mContainerDao { get; set; }
        public IContainerDao ContainerDao
        {
            get
            {
                if (mContainerDao == null)
                {
                    mContainerDao = PlatformWindsorManager.GetService(typeof(IContainerDao)) as IContainerDao;
                    return mContainerDao;
                }
                else
                {
                    return mContainerDao;
                }
            }
        }
        public ISPSettingTreeService RMSPTreeService => PlatformWindsorManager.GetService<ISPSettingTreeService>();
        private ISharePointSettingDao SharePointSettingDao => PlatformWindsorManager.GetService<ISharePointSettingDao>();
        public IJobMonitorService JobService { get; set; }
        public ITermSetDao TermSetDao => PlatformWindsorManager.GetService<ITermSetDao>();

        public ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        #endregion

        #region Job Progress & Detail
        #region remove progress logic
        //private int CalculateProgress(int numerator, int total)
        //{
        //    double progressCount = 0;
        //    if (numerator >= total)
        //    {
        //        progressCount = total;
        //    }
        //    else
        //    {
        //        progressCount = (double)numerator / (double)total * (nextJobProgress - currentjobProgress) + currentjobProgress;
        //    }
        //    return (int)progressCount;
        //}
        //public int GetListCount(RMSPTreeNode node)
        //{
        //    int result = 0;
        //    logger.Info("begin get lists count");
        //    try
        //    {
        //        switch (node.Level)
        //        {
        //            case (int)NodeLevel.SiteCollection:
        //                Web rootWeb = this.mSite.RootWeb;
        //                this.mClientContext.Load(rootWeb, r => r.Webs, r => r.Lists);
        //                this.mClientContext.ExecuteQuery();
        //                result += rootWeb.Lists.Count();
        //                foreach (var web in rootWeb.Webs)
        //                {
        //                    result += GetListCount(web);
        //                }
        //                break;
        //            case (int)NodeLevel.Site:
        //                Web rweb = this.mSite.OpenWebById(new Guid(node.Parent.Parent.SPObjectId));
        //                result += GetListCount(rweb);
        //                break;
        //            case (int)NodeLevel.Library:
        //            case (int)NodeLevel.List:
        //                result++;
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Info("Get Total list count error {0}", e.ToString());
        //    }
        //    logger.Info("end get lists count {0}", result);
        //    return result;
        //}
        //private int GetListCount(Web web)
        //{
        //    int result = 0;
        //    this.mClientContext.Load(web, w => w.Webs, w => w.Lists);
        //    this.mClientContext.ExecuteQuery();
        //    result += web.Lists.Count();
        //    foreach (var subWeb in web.Webs)
        //    {
        //        result += GetListCount(subWeb);
        //    }
        //    return result;
        //}
        #endregion
        private void AddDetailToList(string objectName, string sourceURL, string Action, JobDetailsStatus status, string message)
        {
            JMGlobalSettingJobDetails detail = new JMGlobalSettingJobDetails();
            if (objectName == sourceURL)
            {
                detail.ObjectName = mWeb.Title;
            }
            else
            {
                detail.ObjectName = objectName;
            }
            detail.SourceURL = sourceURL;
            detail.ColumnName = this.columnDisplayName;
            detail.Action = Action;
            detail.Status = status;
            detail.Comment = message;
            detail.Classification = this.classification;
            this.SPSettingJobDetails.Add(detail);
        }
        private void SetSettingStatus(Guid SettingScopeId, int failedType)
        {
            if (settingResults.ContainsKey(SettingScopeId))
            {
                var fType = settingResults[SettingScopeId];
                settingResults[SettingScopeId] = fType | failedType;
            }
            else
            {
                settingResults.Add(SettingScopeId, failedType);
            }
        }
        private string GetFullUrl(RMSPTreeNode node)
        {
            if (node.Level == (int)NodeLevel.Folder)
            {
                return WebUtil.MakeFullUrl(GetSiteCollectionNode(node).FullPath, node.FullPath);
            }
            return node.FullPath;
        }
        #endregion

        #region logic method
        /// <summary>
        /// validate site collection node is in Group or not.
        /// </summary>
        /// <param name="siteCollectionUrl"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public bool ValidateSiteCollection(RMSPTreeNode node, string groupId, Guid scopeId)
        {
            try
            {
                //DAOAPIClientV1 test = new DAOAPIClientV1();
                //RemoteSiteCollection site = test.GetRemoteSiteCollectionByUrl(node.FullPath);
                RemoteSiteCollection site = RABrowserClient.GetRemoteSiteCollectionByUrl(node.FullPath);
                if (site == null)
                {
                    logger.Info("Site was removed in DAO Register sites {0}", node.FullPath);
                    this.AddDetailToList(node.Name, node.FullPath, string.Empty, JobDetailsStatus.Skipped, "RM_SS_SiteRemovedFromDAO");
                    SetSettingStatus(scopeId, (int)BCSSettingFailedType.None);
                    return false;
                }
                else
                {
                    if (!site.parentId.Equals(groupId, StringComparison.OrdinalIgnoreCase))
                    {
                        logger.Info("Site was removed from SharePoint Groups {0} : {1}", node.FullPath, groupId);
                        this.AddDetailToList(node.Name, node.FullPath, string.Empty, JobDetailsStatus.Skipped, "RM_SS_SiteRemovedFromSPGroups");
                        SetSettingStatus(scopeId, (int)BCSSettingFailedType.None);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Info("Validate site collection failed {0} : {1}", node.FullPath, ex.ToString());
                this.AddDetailToList(node.Name, node.FullPath, string.Empty, JobDetailsStatus.Skipped, "RM_SS_SiteRemovedFromDAO");
                SetSettingStatus(scopeId, (int)BCSSettingFailedType.None);
                return false;
            }
        }
        public bool ValidataGroup(RMSPTreeNode node)
        {
            try
            {
                //DAOAPIClientV1 test = new DAOAPIClientV1();
                //RemoteWebApplication webApp = test.GetWebApplicationById(node.Id);
                RemoteWebApplication webApp = RABrowserClient.GetWebApplicationById(node.Id);
                if (webApp == null)
                {
                    logger.Info("Group was removed in DAO Register Groups {0}", node.FullPath);
                    this.AddDetailToList(node.Name, node.FullPath, string.Empty, JobDetailsStatus.Skipped, "RM_SS_GroupRemovedFromDAO");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Info("Validate group failed {0} : {1}", node.FullPath, ex.ToString());
                return false;
            }
        }
        /// <summary>
        /// get the site id for DocAve.
        /// </summary>
        /// <returns></returns>
        public Guid GetSiteId()
        {
            try
            {
                return new Guid(this.siteCollectionTreeNode.SPObjectId);
            }
            catch
            {
                return Guid.Empty;
            }
        }
        public Guid GetReadSiteId()
        {
            try
            {
                return this.mSite.Id;
            }
            catch
            {
                return Guid.Empty;
            }
        }
        public RMSPTreeNode GetListNode(RMSPTreeNode node)
        {
            while (node.Level != (int)NodeLevel.List)
            {
                node = node.Parent;
            }
            return node;
        }
        public RMSPTreeNode GetWebNode(RMSPTreeNode node)
        {
            while (node.Level != (int)NodeLevel.Site)
            {
                node = node.Parent;
            }
            return node;
        }
        public RMSPTreeNode GetSiteCollectionNode(RMSPTreeNode node)
        {
            while (node.Level != (int)NodeLevel.SiteCollection)
            {
                node = node.Parent;
            }
            return node;
        }
        public RMSPTreeNode GetWebAppNode(RMSPTreeNode node)
        {
            while (node.Level != (int)NodeLevel.WebApplication)
            {
                node = node.Parent;
            }
            return node;
        }
        public RMSPTreeNode GetCustomSettingsNode(RMSPTreeNode node, string url, string userName, string password)//, ref RMSPTreeNode parentSiteColl)
        {
            AveBPOSAccountInfo accountInfo = new AveBPOSAccountInfo()
            {
                //Domain = site.BposInfo.UserAccountInfo.Domain,
                UserName = userName,
                Password = CspCommunicationWrapper.UnWrapKeyToSecureString(Convert.FromBase64String(password))
            };
            //var serviceFactory = AveObjectModelFactory.CreateObjectModelFactory(url, accountInfo, AveContextKind.ClientObjectModel);
            //IAveSiteServiceHelper siteServiceHelper = serviceFactory.CreateSiteServiceHelper();
            //url = siteServiceHelper.TryToRectifySiteUrl(url, accountInfo);

            // var serviceSite = serviceFactory.CreateSite(url);
            // var web = serviceSite.OpenWeb();
            //IAveList list = null;
            InitContext(node);
            mClientContext.Load(mSite);
            mClientContext.Load(mWeb);
            mClientContext.ExecuteQuery();
            var web = this.mClientContext.Web;

            mClientContext.Load(web);
            mClientContext.ExecuteQuery();

            if (url.Equals(this.mSite.Url, StringComparison.OrdinalIgnoreCase))
            {
                node.Level = (int)NodeLevel.SiteCollection;
                node.FullPath = url;
                node.SPObjectId = node.Id;//this.mSite.Id.ToString();
                node.Title = this.mWeb.Title;
            }
            else if (url.Equals(web.Url, StringComparison.OrdinalIgnoreCase))
            {
                node.Level = (int)NodeLevel.Site;
                node.FullPath = url;
                node.SPObjectId = web.Id.ToString();
                node.Title = web.Title;
                node.SPObjectId = web.Id.ToString();
                node.WebId = web.Id;
            }
            else
            {
                //list = web.GetListFromUrl(url);
                int start = url.LastIndexOf('/') + 1;
                var libName = url.Substring(start, url.Length - start);
                var libUrl = web.ServerRelativeUrl + "/" + libName;
                logger.Info("lib url,{0}", libUrl);
                var list = web.GetList(libUrl);
                mClientContext.Load(list);
                mClientContext.ExecuteQuery();
                node.Level = (int)NodeLevel.Library;
                node.FullPath = url;
                node.SPObjectId = list.Id.ToString();
                node.Title = list.Title;
                node.ListId = list.Id;
                node.WebId = web.Id;
            }
            return node;
        }
        public void InitSiteObject(RMSPTreeNode node, bool needCheckDefaultVaule, bool isCustomSetting)
        {
            m_needCheckDefaultVaule = needCheckDefaultVaule;
            if (needCheckDefaultVaule)
            {
                limitedCount = GetMaxItemsPerThrottledOperation(mSite);
            }
        }
        private async System.Threading.Tasks.Task SetDefaultValueAsync(RMSPTreeNode node, Web web, List list)
        {
            using (CheckJobStopScope jScope = new CheckJobStopScope())
            {
                try
                {
                    if (InitFiled(node, list))
                    {
                        var items = QueryItems(list, node);
                        await SetValueAsync(list, items, true);
                    }
                    //}
                }
                catch (Exception ex)
                {
                    logger.Error("an error occurred while set default value,siteUrl:{0}, listTitle:{1},ERROR:{2}", list != null ? web.Url : string.Empty, list != null ? list.Title : string.Empty, ex.ToString());
                    throw ex;
                }
            }
        }
        private async System.Threading.Tasks.Task SetDefaultValueAsync(RMSPTreeNode node, Web web, List list, Folder folder)
        {
            using (CheckJobStopScope jScope = new CheckJobStopScope())
            {
                try
                {
                    if (InitFiled(node, list, folder))
                    {
                        var items = QueryItems(list, folder, node);
                        await SetValueAsync(list, items);
                    }
                    //}
                }
                catch (Exception ex)
                {
                    logger.Error("an error occurred while set default value,siteUrl:{0}, listTitle:{1},ERROR:{2}", list != null ? web.Url : string.Empty, list != null ? list.Title : string.Empty, ex.ToString());
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 整体对List下面所有的Folder赋值，都赋值成lib/list的default term
        /// </summary>
        /// <param name="node"></param>
        /// <param name="web"></param>
        /// <param name="list"></param>
        private async System.Threading.Tasks.Task SetDefaultValueForFolderAsync(RMSPTreeNode node, Web web, List list)
        {
            using (CheckJobStopScope jScope = new CheckJobStopScope())
            {
                try
                {
                    if (InitFiled(node, list))
                    {
                        var items = QueryItemsForFolder(list, node);
                        await SetValueAsync(list, items);
                    }
                    //}
                }
                catch (Exception ex)
                {
                    logger.Error("an error occurred while set default value,siteUrl:{0}, listTitle:{1},ERROR:{2}", list != null ? web.Url : string.Empty, list != null ? list.Title : string.Empty, ex.ToString());
                    throw ex;
                }
            }
        }

        /*  old code
        private void SetDefaultValue(RMSPTreeNode node, Guid webId, Guid listId)//, bool isCustomSetting)
        {
            List list = null;
            Web web = null;
            try
            {
                //if (m_needCheckDefaultVaule)
                //{
                web = this.mSite.OpenWebById(webId);
                mClientContext.Load(web);
                list = web.Lists.GetById(listId);
                mClientContext.Load(list, l => l.Fields, l => l.RootFolder, l => l.ParentWeb);
                mClientContext.Load(list);
                mClientContext.ExecuteQuery();

                //m_aveWeb = m_Site.OpenWeb(webId.ToString());
                //list = m_aveWeb.Lists.GetById(listId);
                if (InitFiled(node, list))
                {
                    var items = QueryItems(list, node);
                    SetValue(list, items);
                }
                //}
            }
            catch (Exception ex)
            {
                logger.Error("an error occurred while set default value,siteUrl:{0}, listTitle:{1},ERROR:{2}", list != null ? web.Url : string.Empty, list != null ? list.Title : string.Empty, ex.ToString());
                throw ex;
            }

        }
        */
        private bool InitFiled(RMSPTreeNode node, List list)//, bool isCustomSetting)
        {
            bool success = false;
            try
            {
                var field = list.Fields.GetById(RevIMClassificationColumnID);
                this.mClientContext.Load(field);
                this.mClientContext.ExecuteQuery();
                if (field != null)
                {
                    taxField = mClientContext.CastTo<TaxonomyField>(field);
                    m_internalName = taxField.InternalName;
                    var defaultVal = taxField.DefaultValue;
                    if (!string.IsNullOrEmpty(defaultVal))
                    {
                        var termGuid = defaultVal.Substring(defaultVal.IndexOf("|") + 1);

                        var taxonomySession = TaxonomySession.GetTaxonomySession(mClientContext);
                        mClientContext.Load(taxonomySession);
                        mClientContext.ExecuteQuery();

                        mClientContext.Load(taxonomySession.TermStores);
                        mClientContext.ExecuteQuery();
                        var termStore = taxonomySession.TermStores[0];
                        m_term = termStore.GetTerm(Guid.Parse(termGuid));
                        success = true;
                    }
                    logger.Info("begin to init field, field internalName:{0}, defaultValue:{1}, listTitle:{2},siteUrl:{3}", m_internalName, defaultVal, list.Title, mWeb.Url);
                }
                else
                {
                    logger.Warn("init field, field not exist in the list,siteUrl:{0},listTitle:{1}, fieldId:{2}", mWeb.Url, list.Title, RevIMClassificationColumnID);
                }

            }
            catch (Exception ex)
            {
                logger.Warn("error occurred while init field,ERROR:{0}", ex.ToString());
            }
            return success;


        }
        
        private bool InitFiled(RMSPTreeNode node, List list, Folder folder)//, bool isCustomSetting)
        {

            bool success = false;
            try
            {
                var field = list.Fields.GetById(RevIMClassificationColumnID);
                this.mClientContext.Load(field);
                this.mClientContext.ExecuteQuery();
                if (field != null)
                {
                    taxField = mClientContext.CastTo<TaxonomyField>(field);
                    m_internalName = taxField.InternalName;
                    var defaultVal = taxField.DefaultValue;
                    defaultVal = node.DefaultTermName;
                    if (!string.IsNullOrEmpty(defaultVal))
                    {
                        var termGuid = node.DefaultTermId;

                        var taxonomySession = TaxonomySession.GetTaxonomySession(mClientContext);
                        mClientContext.Load(taxonomySession);
                        mClientContext.ExecuteQuery();

                        mClientContext.Load(taxonomySession.TermStores);
                        mClientContext.ExecuteQuery();
                        var termStore = taxonomySession.TermStores[0];
                        m_term = termStore.GetTerm(termGuid);
                        success = true;
                    }
                    logger.Info("begin to init field, field internalName:{0}, defaultValue:{1}, listTitle:{2},siteUrl:{3}", m_internalName, defaultVal, list.Title, mWeb.Url);
                }
                else
                {
                    logger.Warn("init field, field not exist in the list,siteUrl:{0},listTitle:{1}, fieldId:{2}", mWeb.Url, list.Title, RevIMClassificationColumnID);
                }

            }
            catch (Exception ex)
            {
                logger.Warn("error occurred while init field,ERROR:{0}", ex.ToString());
            }
            return success;
        }
        private bool CheckClassificationSetting(RMSPTreeNode node)
        {
            if (!node.IsUsingExistColumnName && !mTaxonomy.ValidateTermIds(node.TermSetId, node.TermId != null && node.TermId != Guid.Empty ? node.TermId.ToString() : string.Empty,
    node.DefaultTermId != null && node.DefaultTermId != Guid.Empty ? node.DefaultTermId.ToString() : string.Empty))
            {
                this.columnDisplayName = node.ColumnName;
                this.classification = node.TermNameOfContainer;
                this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Action_CheckColumnSetting", JobDetailsStatus.Failed, "RM_SS_ConfigureColumnFailed");
                if (node.isEnableClassification && !mTaxonomy.ValidateTermId(node.TermIdOfContainer))
                {
                    this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Action_CheckClassificationSetting", JobDetailsStatus.Failed, "RM_SS_ConfigureClassificationFailed");
                }
                logger.Warn(I18NEntity.GetString("RM_SS_ConfigureColumnFailed"));
                SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSColumnFailed | (int)BCSSettingFailedType.AddBCSPropertyFailed);
                return false;
            }
            return true;
        }

        private ListItemCollection QueryItems(List list, RMSPTreeNode node)
        {
            ListItemCollection items = null;
            try
            {
                CamlQuery query = CamlQuery.CreateAllItemsQuery();
                if (node.ApplyExistType == (int)ApplyExistingTermType.OverWrite)
                {
                    query.ViewXml = string.Format("<View Scope=\"RecursiveAll\"><Query><Where><Eq><FieldRef Name='FSObjType'></FieldRef><Value Type='Text'>0</Value></Eq></Where></Query><RowLimit>{0}</RowLimit></View>", limitedCount);
                }
                else
                {
                    query.ViewXml = @"
                    <View Scope='RecursiveAll'>
                        <Query>
                            <Where>
                                <And>
                                    <IsNull>
                                        <FieldRef Name='" + m_internalName + @"'/>
                                    </IsNull>
                                    <Eq>
                                        <FieldRef Name='FSObjType'></FieldRef>
                                        <Value Type='Text'>0</Value>
                                    </Eq>
                                </And>
                            </Where>
                        </Query>
                        <RowLimit>" + limitedCount + @"</RowLimit>
                    </View>";
                }
                items = list.GetItems(query);
            }
            catch (Exception ex)
            {
                logger.Error("an error occurred while Query And Set DefaultVaule,ERROR:{0}", ex.ToString());
            }
            mClientContext.Load(items);
            mClientContext.ExecuteQuery();
            return items;
        }

        private ListItemCollection QueryItemsForFolder(List list, RMSPTreeNode node)
        {
            ListItemCollection items = null;
            try
            {
                CamlQuery query = CamlQuery.CreateAllItemsQuery();
                if (node.ApplyExistType == (int)ApplyExistingTermType.OverWrite)
                {
                    query.ViewXml = string.Format("<View Scope=\"RecursiveAll\"><Query><Where><Eq><FieldRef Name='FSObjType'></FieldRef><Value Type='Text'>1</Value></Eq></Where></Query><RowLimit>{0}</RowLimit></View>", limitedCount);
                }
                else
                {
                    query.ViewXml = @"
                    <View Scope='RecursiveAll'>
                        <Query>
                            <Where>
                                <And>
                                    <IsNull>
                                        <FieldRef Name='" + m_internalName + @"'/>
                                    </IsNull>
                                    <Eq>
                                        <FieldRef Name='FSObjType'></FieldRef>
                                        <Value Type='Text'>1</Value>
                                    </Eq>
                                </And>
                            </Where>
                        </Query>
                        <RowLimit>" + limitedCount + @"</RowLimit>
                    </View>";
                }
                items = list.GetItems(query);
            }
            catch (Exception ex)
            {
                logger.Error("an error occurred while Query And Set DefaultVaule,ERROR:{0}", ex.ToString());
            }
            mClientContext.Load(items);
            mClientContext.ExecuteQuery();
            return items;
        }

        private ListItemCollection QueryItems(List list, Folder folder, RMSPTreeNode node)
        {
            ListItemCollection items = null;
            try
            {
                CamlQuery query = CamlQuery.CreateAllItemsQuery();
                if (node.ApplyExistType == (int)ApplyExistingTermType.OverWrite)
                {
                    query.ViewXml = string.Format("<View Scope=\"FilesOnly\"><RowLimit>{0}</RowLimit></View>", limitedCount);
                }
                else
                {
                    query.ViewXml = @"
                    <View Scope='FilesOnly'>
                        <Query>
                            <Where>
                                <IsNull>
                                   <FieldRef Name='" + BCSColumnName + @"'/>
                                </IsNull>
                            </Where>
                        </Query>
                        <RowLimit>" + limitedCount + @"</RowLimit>
                    </View>";
                }
                query.FolderServerRelativeUrl = folder.ServerRelativeUrl;
                items = list.GetItems(query);

            }
            catch (Exception ex)
            {
                logger.Error("an error occurred while Query And Set DefaultVaule,ERROR:{0}", ex.ToString());
            }
            mClientContext.Load(items);
            mClientContext.ExecuteQuery();
            return items;
        }
        private async System.Threading.Tasks.Task SetValueAsync(List list, ListItemCollection items, bool needChedkFileSystemObjectType = false)
        {
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            if (items != null)
            {
                var itemCount = 0;
                int tempcounter = 0;

                mClientContext.Load(list, l => l.Fields);
                mClientContext.ExecuteQuery();

                var textField = list.Fields.GetById(taxField.TextField);
                mClientContext.Load(textField);
                mClientContext.ExecuteQuery();
                var textFieldName = textField.InternalName;
                mClientContext.Load(m_term, t => t.Id, t => t.Name, t => t.TermStore.WorkingLanguage);
                mClientContext.Load(list);
                mClientContext.ExecuteQuery();

                int lcid = 1033;
                if (items.Count > 0)
                {
                    lcid = m_term.TermStore.WorkingLanguage;
                }
                foreach (var item in items)
                {
                    try
                    {
                        using (CheckJobStopScope jScope = new CheckJobStopScope())
                        {
                            if (needChedkFileSystemObjectType && item["FSObjType"].ToString() == "1")
                            {
                                logger.Info("skip item is Folder, itemId: {0}", item.Id);
                                continue;
                            }

                            item[taxField.InternalName] = m_term.Name + "|" + m_term.Id.ToString();
                            item[textFieldName] = m_term.Name + "|" + m_term.Id.ToString();
                            item.SystemUpdate();
                            mClientContext.ExecuteQuery();
                        }
                    }
                    catch (JobStopException ex)
                    {
                        throw new JobStopException("This Job is stopped.");
                    }
                    catch (ServerUnauthorizedAccessException ae)
                    {
                        logger.Error("Apply to the existing documents error:{0}", ae.ToString());
                    }
                    catch (Exception ex)
                    {
                        logger.Error("an error occurred while update default value,listTitle:{0},itemId:{1},ERROR:{2}", list.Title, item.Id, ex.ToString());
                    }
                    tempcounter++;
                    if (tempcounter >= 100)
                    {
                        await JobService.UpdateJobWithoutProgressChangeAsync(jobId);
                        tempcounter = 0;
                    }
                }

                if (itemCount > 0 && this.SPSettingJobDetails.Count > 0)
                {
                    if (this.SPSettingJobDetails.Last().Status != JobDetailsStatus.Failed)
                    {
                        //this.SPSettingJobDetails.Last().Status = JobDetailsStatus.Successful;
                        //this.SPSettingJobDetails.Last().Action = "Update";
                        //this.SPSettingJobDetails.Last().Comment = "";
                        this.AddDetailToList(this.SPSettingJobDetails.Last().ObjectName,
                            this.SPSettingJobDetails.Last().SourceURL, "Set Default Value", JobDetailsStatus.Successful, "");
                    }
                }

            }
            stopWatch.Stop();
            TimeSpan timer = stopWatch.Elapsed;
            logger.Info("Stop Watch SetValue 5000 Items ,time : {0} s ", timer.TotalSeconds);
        }

        protected uint GetMaxItemsPerThrottledOperation(Site discoverSite)
        {
            uint maxItemsPer = 5000;
            maxItemsPer = discoverSite.MaxItemsPerThrottledOperation;
            return maxItemsPer;
        }
        /*private async Task<List<RMSPTreeNode>> GetRegisteredSPSitesAsync()
        {
            List<RMSPTreeNode> returnList = new List<RMSPTreeNode>();
            List<RMSPTreeNode> registeredSites = RMSPTreeService.LoadFarm();
            var defaultSites = await RMSPTreeService.BrowseAsync(registeredSites[0]);
            foreach (var defaultSite in defaultSites)
            {
                returnList.AddRange(await RMSPTreeService.BrowseAsync(defaultSite));
            }
            return returnList;
        }*/
        private bool CheckIsDesignList(string listInfo)
        {
            bool isDesignList = false;
            try
            {
                if (this.DesignLists.Contains(listInfo))
                {
                    return true;
                }
            }
            catch(Exception e)
            {
                logger.Warn($"An error has occurred when CheckIsDesignList, message:{e.Message}");
            }
            return isDesignList;
        }
        private List<string> GetDesignLists()
        {
            return WebUtil.GetDesignLists(TenantService.IsCSDTenant());
        }
        public Guid GetTermStoreId()
        {
            return mTaxonomy.GetDefaultTermStoreId();
        }
        public string GetPhysicalLibraryPath(RMSharePointSetting setting)
        {
            string path = string.Empty;
            SharePointSettingUtility spUtility = new SharePointSettingUtility();
            var node = spUtility.GetRemoteSiteCollection(setting.SiteId.ToString());
            if (node != null)
            {
                CommonClientContext commonClientContext = new CommonClientContext();
                var context = commonClientContext.InitClientContext(node);
                Site mSite = context.Site;

                context.Load(mSite);
                context.ExecuteQuery();
                Web web = null;
                List list = null;
                if (setting.WebId == null || setting.WebId == Guid.Empty)
                {
                    web = mSite.RootWeb;
                    path += node.url;
                }
                else
                {
                    web = mSite.OpenWebById(setting.WebId);
                    context.Load(web, w => w.Url);
                    context.ExecuteQuery();
                    path += web.Url;
                }
                try
                {
                    context.Load(web, w => w.Webs, w => w.Lists);
                    context.ExecuteQuery();
                }
                catch (Exception e)
                {
                    logger.Info("physical web may have been deleted, ,webid is:{0},message:{1}", setting.WebId, e.Message.ToString());
                    return string.Empty;
                }

                try
                {
                    if (setting.ListId == null || setting.ListId == Guid.Empty)
                    {
                        var libName = Common.Util.GetAppSettingValue("RevIMHoldPhysicalLibraryName");
                        list = web.Lists.GetByTitle(libName);
                    }
                    else
                    {
                        list = web.Lists.GetById(setting.ListId);
                    }
                    context.Load(list, l => l.Title);
                    context.ExecuteQuery();
                    path += "/" + list.Title;
                }
                catch (Exception e)
                {
                    logger.Info("physical library may have been deleted, libraryid is:{0},message:{1}", setting.ListId, e.Message.ToString());
                    return string.Empty;
                }
            }

            return path;
        }
        public void InitContext(RMSPTreeNode node)
        {
            CommonClientContext commonClientContext = new CommonClientContext();
            mClientContext = commonClientContext.InitClientContext(node);
            
            this.mSite = mClientContext.Site;
            this.mWeb = mClientContext.Site.RootWeb;
            this.DesignLists = GetDesignLists();
            mClientContext.Load(mSite);
            mClientContext.Load(mWeb);
            mClientContext.ExecuteQuery();
            var BPOSInfo = PoolUserUtil.GetAveBPOSAccountInfo(node.BposInfo, node.FullPath);//new debug...
            var tokenProvider = BPOSInfo.Convert2TokenProvider();
            RelatedRecordsAppUtility utility = new RelatedRecordsAppUtility(mClientContext,tokenProvider, mSite, mWeb, mWeb.Url);
            mTaxonomy = new RMSharePointTaxonomy(mClientContext);
        }
        public Guid GetSiteCollectionRevIMColumnID()
        {
            try
            {
                mClientContext.Load(mWeb, w => w.Fields);
                Field metadataField = mWeb.Fields.GetById(RevIMClassificationColumnID);
                mClientContext.Load(metadataField);
                //之前设置过Global Settings，但是由于一些原因在这个site collection中没有创建出来column，所以web中get不到，会报异常
                mClientContext.ExecuteQuery();
                logger.Info("check site column id");
                return RevIMClassificationColumnID;
            }
            catch (Exception ex)
            {
                logger.Info("Site not config metadata column {0}", ex.ToString());
            }
            return Guid.Empty;
        }

        public Guid CheckListColumnID(List list)
        {
            try
            {
                logger.Info("Check list column id");
                mClientContext.Load(list, l => l.Fields);
                Field metadataField = list.Fields.GetById(RevIMClassificationColumnID);
                mClientContext.Load(metadataField);
                //之前设置过Global Settings，但是由于一些原因在List中没有创建出来column，所以list中get不到，会报异常,例如新建的subsite
                mClientContext.ExecuteQuery();
                return RevIMClassificationColumnID;
            }
            catch (Exception ex)
            {
                logger.Info("List not config metadata column {0}", ex.ToString());
            }
            return Guid.Empty;

        }
        public Guid GetLocationTermSetUniqueId()
        {
            return TermSetDao.GetRMTermSet((int)TermSetType.Physical).UniqueId;
        }

        #endregion

        #region Physical Settings
        /// <summary>
        /// 按照原来的逻辑此方法不需要对sharepoint操作，只需要添加一条detail记录即可
        /// </summary>
        /// <param name="node"></param>
        public void CancelPhysicalFlagForSPNode(RMSPTreeNode node)
        {
            using (CheckJobStopScope jScope = new CheckJobStopScope())
            {
                var oldSetting = SharePointSettingDao.LoadSharePointSetting(new Guid(node.SPObjectId), GetSiteId(), true);
                if (oldSetting != null && oldSetting.IsEnableHoldPhyical)
                {
                    this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Action_CancelPhysical", JobDetailsStatus.Successful, null);
                }
            }
        }
        public void AddPhysicalFlagForSPNode(RMSPTreeNode node)
        {
            string libPhyName = Common.Util.GetAppSettingValue("RevIMHoldPhysicalLibraryName");
            string colPhyName = Common.Util.GetAppSettingValue("RevIMHomeLocationName");
            string contentTypeNames = Common.Util.GetAppSettingValue("RevIMWorkflowContentTypes");
            List<string> needRemoveBCSContentTypes = new List<string>();
            if (!string.IsNullOrEmpty(contentTypeNames))
            {
                needRemoveBCSContentTypes = contentTypeNames.Split(';').ToList();
            }
            //string RAI18N_NotSetPhysicalInConfigFile = "RM_SS_NotSetPhysicalInConfigFile";
            string RAI18N_ConfigPhysicalAction = I18NEntity.GetString("RM_SS_ConfigPhysicalAction");
            Web web = null;
            if ((node.NodeType == 0 && node.Level == 300) || node.TemplateId == 600)//need to filter the system list  
            {
                this.AddDetailToList(node.Name, GetFullUrl(node), RAI18N_ConfigPhysicalAction, JobDetailsStatus.Skipped, "RM_SS_NoMarkPhysicalForListNode");
                logger.Info("list node no can AddPhysical settting , Node fullPath is {0}", node.FullPath);
                return;
            }

            if (string.IsNullOrEmpty(libPhyName) || string.IsNullOrEmpty(colPhyName))
            {
                //this.AddDetailToList(node.Name, GetFullUrl(node), RAI18N_ConfigPhysicalAction, JobDetailsStatus.Failed, RAI18N_NotSetPhysicalInConfigFile);
                //logger.Warn(I18NEntity.GetString(RAI18N_NotSetPhysicalInConfigFile));
                SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddPhysicalPropertyFailed);
                return;
            }
            try
            {
                logger.Info("Begin Add a Hold Physical label for the node");
                switch (node.Level)
                {
                    case (int)NodeLevel.SiteCollection:
                        web = this.mSite.OpenWeb("");
                        break;
                    case (int)NodeLevel.Site:
                        web = this.mSite.OpenWebById(new Guid(node.SPObjectId));
                        break;
                    case (int)NodeLevel.Library:
                    case (int)NodeLevel.List:
                        if (node.WebId != null && node.WebId != Guid.Empty)
                        {
                            web = this.mSite.OpenWebById(node.WebId);
                        }
                        else
                        {
                            web = this.mSite.OpenWebById(new Guid(node.Parent.Parent.SPObjectId));
                        }
                        break;
                }
                try
                {
                    mClientContext.Load(web, w => w.Id);
                    mClientContext.ExecuteQuery();
                }
                catch (Exception)
                {
                    this.AddDetailToList(node.Name, GetFullUrl(node), RAI18N_ConfigPhysicalAction, JobDetailsStatus.Failed, "^Open Web occur Error");
                    logger.Error("Open Web occur Error , Node fullPath is {0}", node.FullPath);
                    SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddPhysicalPropertyFailed);
                    return;
                }
                libPhyName = Common.Util.GetAppSettingValue("RevIMHoldPhysicalLibraryName");
                List physicalLib = null;
                mClientContext.Load(web);
                ArgumentCheck.CheckNotNull(web);
                if ((int)NodeLevel.List == node.Level || (int)NodeLevel.Library == node.Level)
                {
                    physicalLib = web?.Lists.GetById(new Guid(node.SPObjectId));
                }
                else
                {
                    physicalLib = web?.Lists.GetByTitle(libPhyName);
                }
                string requestListName = Common.Util.GetAppSettingValue("RevIMRequestListName");
                List requestList = web.Lists.GetByTitle(requestListName);
                RMSPTreeNode requestListNode = CreateRequestListNode(node);
                //if (CheckIsDesignList(physicalLib.RootFolder.Name + physicalLib.BaseTemplate.ToString()))// to do job detail
                //{
                //    logger.Info("Skip the design list {0}", physicalLib.RootFolder.Name);
                //    this.AddDetailToList(node.Name, GetFullUrl(node), RAI18N_ConfigPhysicalAction, JobDetailsStatus.Skipped, "^list node no can set");
                //    return;
                ////}
                //RemoveBCSColumn(web);
                RemoveBCSColumn(physicalLib, needRemoveBCSContentTypes);
                if (node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Site)
                {
                    try
                    {
                        mClientContext.Load(physicalLib, l => l.Title);
                        mClientContext.ExecuteQuery();
                        if (libPhyName.Equals(physicalLib.Title))
                        {
                            if (string.IsNullOrEmpty(node.Name))
                            {
                                node.Name = web.Title;
                            }
                            //CreatePhysicalListTaxonomyField(physicalLib, node);
                            UpdatePhysicalColumnForLibrary(physicalLib, node);
                            UpdateBoxTypeColumn(physicalLib, node);
                            logger.Info("Update request list column.");
                            UpdatePhysicalColumnForLibrary(requestList, requestListNode);
                            UpdateBoxTypeColumn(requestList, requestListNode);
                        }
                    }
                    catch (JobStopException ex)
                    {
                        throw new JobStopException("This Job is stopped.");
                    }
                    catch (Exception e)
                    {
                        this.AddDetailToList(node.Name, GetFullUrl(node), RAI18N_ConfigPhysicalAction, JobDetailsStatus.Failed, "RM_SS_NoFoundPhysicalLibrary");
                        logger.Error("Get Physical List From SharePoint Error , Node fullPath is {0},message:{1}", node.FullPath, e.Message);
                        SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddPhysicalPropertyFailed);
                    }
                }
                else
                {
                    try
                    {
                        mClientContext.Load(physicalLib, l => l.Id);
                        mClientContext.ExecuteQuery();

                        if (physicalLib.Id != null)
                        {
                            //CreatePhysicalListTaxonomyField(physicalLib, node);
                            UpdatePhysicalColumnForLibrary(physicalLib, node);
                            UpdateBoxTypeColumn(physicalLib, node);
                            logger.Info("Update request list column.");
                            UpdatePhysicalColumnForLibrary(requestList, requestListNode);
                            UpdateBoxTypeColumn(requestList, requestListNode);
                        }
                    }
                    catch (JobStopException ex)
                    {
                        throw new JobStopException("This Job is stopped.");
                    }
                    catch (Exception e)
                    {
                        this.AddDetailToList(node.Name, GetFullUrl(node), RAI18N_ConfigPhysicalAction, JobDetailsStatus.Failed, "RM_SS_NoFoundPhysicalLibrary");
                        logger.Error("Get Physical List From SharePoint Error , Node fullPath is {0},message:{1}", node.FullPath, e.Message);
                        SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddPhysicalPropertyFailed);
                    }
                }

            }
            catch (JobStopException ex)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception e)
            {
                //this.AddDetailToList(node.Name, GetFullUrl(node), RAI18N_ConfigPhysicalAction, JobDetailsStatus.Failed, "RM_SS_AddPhysicalFlagForSPNodeErrorTip");
                logger.Error("Add a Hold Physical label for the node has a error,message is :{0} ", e.Message);
                SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddPhysicalPropertyFailed);
            }
            finally
            {
                if (CheckJobStatusUtility.isStopping)
                {
                    SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSColumnFailed | (int)BCSSettingFailedType.AddBCSPropertyFailed);
                }
                else
                {
                    SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.None);
                }
            }
        }
        private RMSPTreeNode CreateRequestListNode(RMSPTreeNode physicalLibNode)
        {
            using (CheckJobStopScope jScope = new CheckJobStopScope())
            {
                RMSPTreeNode requestListNode = physicalLibNode.Clone();
                string requestListName = Common.Util.GetAppSettingValue("RevIMRequestListName");
                string fullPath = requestListNode.FullPath;
                if (requestListNode.Level == (int)NodeLevel.Library || requestListNode.Level == (int)NodeLevel.List)
                {
                    fullPath = fullPath.Substring(0, fullPath.LastIndexOf('/'));
                }
                requestListNode.Name = requestListName;
                requestListNode.FullPath = fullPath + "/Lists/" + requestListName;
                return requestListNode;
            }
        }
        private void UpdateBoxTypeColumn(List lib, RMSPTreeNode node)
        {
            using (CheckJobStopScope jScope = new CheckJobStopScope())
            {
                string RAI18N_UpdateBoxTypeFailed = "RM_SS_UpdateBoxTypeColumn";
                string RAI18N_ConfigBoxTypeAction = I18NEntity.GetString("RM_SS_ConfigBoxTypeColumnAction");
                Field boxField = null;
                bool isSkipped = false;
                try
                {
                    var colPhyName = Common.Util.GetAppSettingValue("RevIMBoxTypeName");
                    mClientContext.Load(lib, l => l.Title, l => l.Fields);
                    mClientContext.ExecuteQuery();
                    try
                    {
                        boxField = lib.Fields.GetByTitle(colPhyName);
                        this.mClientContext.Load(boxField, col => col.Title);
                        this.mClientContext.ExecuteQuery();
                    }
                    catch (Exception e)
                    {
                        this.AddDetailToList(node.Name, GetFullUrl(node), RAI18N_ConfigBoxTypeAction, JobDetailsStatus.Failed, I18NEntity.GetString("RM_SS_NotFoundBoxTypeFiled"));
                        logger.Error("Get Physical BoxType Field From SharePoint Error , Node fullPath is {0},message:{1}", node.FullPath, e.Message);
                        SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddPhysicalPropertyFailed);
                        return;
                    }
                    if (boxField.Title.Equals(colPhyName))
                    {
                        FieldChoice choiceField = this.mClientContext.CastTo<FieldChoice>(boxField);
                        this.mClientContext.Load(choiceField, f => f.DefaultValue, f => f.Choices);
                        this.mClientContext.ExecuteQuery();
                        //get choice from RMDB
                        List<RMContainer> containers = ContainerDao.GetAllContainers();
                        string defaultValue = string.Empty;
                        List<string> containerNames = new List<string>();
                        foreach (var container in containers)
                        {
                            if (container.IsDefault)
                            {
                                defaultValue = container.TypeName;
                            }
                            if (!container.IsRemoved)
                            {
                                containerNames.Add(container.TypeName);
                            }
                        }
                        var fieldValues = containerNames.ToArray();
                        if ((choiceField.DefaultValue.Equals(defaultValue) && choiceField.Choices.Equals(fieldValues)) || containers.Count == 0)
                        {
                            isSkipped = true;
                            this.AddDetailToList(node.Name, GetFullUrl(node), RAI18N_ConfigBoxTypeAction, JobDetailsStatus.Skipped, null);
                        }
                        if (!isSkipped)
                        {
                            this.mClientContext.Load(choiceField);
                            #region modify the column schema
                            choiceField.DefaultValue = defaultValue;
                            choiceField.Choices = fieldValues;
                            choiceField.UpdateAndPushChanges(true);
                            this.mClientContext.ExecuteQuery();
                            #endregion
                            this.AddDetailToList(node.Name, GetFullUrl(node), RAI18N_ConfigBoxTypeAction, JobDetailsStatus.Successful, null);
                        }
                    }
                }
                catch (Exception e)
                {
                    this.AddDetailToList(node.Name, GetFullUrl(node), RAI18N_ConfigBoxTypeAction, JobDetailsStatus.Failed, RAI18N_UpdateBoxTypeFailed);
                    logger.Warn("Update Physical box type Column Error , node is : {0},message:{1}", node.FullPath, e.Message);
                    SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddPhysicalPropertyFailed);
                }
            }
        }
        private void UpdatePhysicalColumnForLibrary(List lib, RMSPTreeNode node)
        {
            using (CheckJobStopScope jScope = new CheckJobStopScope())
            {
                Field f = null;
                bool isSkipped = false;
                try
                {
                    var colPhyName = Common.Util.GetAppSettingValue("RevIMHomeLocationName");
                    Guid locationTermSetid = GetLocationTermSetUniqueId();
                    mClientContext.Load(lib, l => l.Title, l => l.Fields);
                    mClientContext.ExecuteQuery();
                    try
                    {
                        f = lib.Fields.GetByTitle(colPhyName);
                        this.mClientContext.Load(f, col => col.Title);
                        this.mClientContext.ExecuteQuery();
                    }
                    catch (Exception e)
                    {
                        this.AddDetailToList(node.Name, GetFullUrl(node), RAI18N_ConfigPhysicalAction, JobDetailsStatus.Failed, "RM_SS_NotFoundPhysicalFiled");
                        logger.Error("Get Physical Filed From SharePoint Error , Node fullPath is {0},message:{1}", node.FullPath, e.Message);
                        return;
                    }
                    if (f.Title.Equals(colPhyName))
                    {
                        TaxonomyField taxField = this.mClientContext.CastTo<TaxonomyField>(f);
                        this.mClientContext.Load(taxField, t => t.SspId, t => t.TermSetId);
                        this.mClientContext.ExecuteQuery();
                        if (this.GetTermStoreId().Equals(taxField.SspId) && locationTermSetid.Equals(taxField.TermSetId))
                        {
                            isSkipped = true;
                            this.AddDetailToList(node.Name, GetFullUrl(node), RAI18N_ConfigPhysicalAction, JobDetailsStatus.Skipped, null);
                        }
                        if (!isSkipped)
                        {
                            this.mClientContext.Load(taxField);
                            #region modify the column schema
                            taxField.SspId = this.GetTermStoreId();
                            taxField.TermSetId = locationTermSetid;
                            taxField.EnforceUniqueValues = false;
                            taxField.AllowMultipleValues = false;
                            taxField.DefaultValue = string.Empty;
                            taxField.Title = colPhyName;
                            taxField.Indexed = true;
                            taxField.Required = true;
                            taxField.Description = string.Empty;
                            taxField.Update();
                            //lib.Update();
                            this.mClientContext.ExecuteQuery();
                            #endregion
                            this.AddDetailToList(node.Name, GetFullUrl(node), RAI18N_ConfigPhysicalAction, JobDetailsStatus.Successful, null);
                        }
                    }
                }
                catch (Exception e)
                {
                    this.AddDetailToList(node.Name, GetFullUrl(node), RAI18N_ConfigPhysicalAction, JobDetailsStatus.Failed, RAI18N_UpdatePhysicalColumnFailed);
                    logger.Warn("Update Physical Column SspId Error , node is : {0},message:{1}", node.FullPath, e.Message);
                    SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddPhysicalPropertyFailed);
                }
            }
        }
        private void RemoveBCSColumn(List list, List<string> contentTypeNames)
        {
            this.mClientContext.Load(list, w => w.ContentTypes);
            this.mClientContext.ExecuteQuery();
            foreach (var contentType in list.ContentTypes)
            {
                try
                {
                    if (contentTypeNames.Contains(contentType.Name))
                    {
                        var fieldLink = contentType.FieldLinks.GetById(this.RevIMClassificationColumnID);
                        this.mClientContext.Load(fieldLink);
                        this.mClientContext.ExecuteQuery();
                        fieldLink.DeleteObject();
                        contentType.Update(true);
                        list.Update();
                        this.mClientContext.ExecuteQuery();
                    }
                }
                catch (Exception e)
                {
                    logger.Info("Remove Field link error {0}", e.ToString());
                }
            }
        }
        #endregion

        #region SharePoint Classification Setting

        private TaxonomyField CreateNewSiteCollectionBCSColumn(RMSPTreeNode node)//, Guid oldFieldId)
        {
            using (CheckJobStopScope jScope = new CheckJobStopScope())
            {
                Field classificationField = null;
                bool isFieldExist = false;
                try
                {
                    Field dispalyNameField = this.mWeb.Fields.GetByTitle(node.ColumnName);
                    mClientContext.Load(dispalyNameField);
                    mClientContext.ExecuteQuery();
                    string getTitle = dispalyNameField.Title;
                    isFieldExist = true;
                }
                catch (Exception e)
                {
                    logger.Info("Not get same column from site {0} : {1}", node.FullPath, e.ToString());
                }
                if (isFieldExist)
                {
                    logger.Warn("Have same name column in the SiteCollection");
                    throw new Exception("Have same name column in the SiteCollection");
                }
                try
                {
                    //创建新的column，会报异常，但是报异常后web的Fields可以通过就ID取出来了。
                    classificationField = this.mWeb.Fields.AddFieldAsXml("<Field Type='" + "TaxonomyFieldType" + "'   Name='" + "RevIMBCS" + "' ID='" + RevIMClassificationColumnID + "' DisplayName='" + node.ColumnName + "'  ShowField='Term1033' StaticName='RevIMBCS' />", true, AddFieldOptions.AddFieldInternalNameHint | AddFieldOptions.AddFieldToDefaultView | AddFieldOptions.AddToAllContentTypes);
                    this.mClientContext.Load(classificationField);
                    this.mClientContext.ExecuteQuery();
                }
                catch
                {
                    classificationField = this.mWeb.Fields.GetById(RevIMClassificationColumnID);
                    this.mClientContext.Load(classificationField);
                    this.mClientContext.ExecuteQuery();
                }

                TaxonomyField taxField = this.mClientContext.CastTo<TaxonomyField>(classificationField);
                this.mClientContext.Load(taxField);
                taxField.SspId = mTaxonomy.GetDefaultTermStoreId();
                node.TermStoreId = taxField.SspId;
                taxField.EnforceUniqueValues = false;
                taxField.AllowMultipleValues = false;
                taxField.TermSetId = node.TermSetId;
                if (node.TermId != null && node.TermId != Guid.Empty)
                {
                    taxField.AnchorId = node.TermId;
                }
                if (node.DefaultTermId != null && node.DefaultTermId != Guid.Empty)
                {
                    taxField.DefaultValue = "-1" + ";#" + node.DefaultTermName + "|" + node.DefaultTermId;
                }
                taxField.Indexed = true;
                taxField.Required = true;
                if (node.Description == null)
                {
                    taxField.Description = string.Empty;
                }
                else
                {
                    taxField.Description = node.Description;
                }
                if (node.IsDisplyaTermPath)
                {
                    taxField.IsPathRendered = true;
                }
                taxField.Update();
                this.mWeb.Update();//to do next , validate update Web or not
                mClientContext.Load(taxField);
                this.mClientContext.ExecuteQuery();
                logger.Info("Create SiteCollection Classification Success, The internal name of this column is {0}", taxField.InternalName);
                return taxField;
            }
        }
        private void AddBCSColumnToSiteCollection(RMSPTreeNode node)//, bool isCustomSetting = false)
        {
            bool isUpdate = true;
            bool isSkipped = true;
            this.columnDisplayName = node.ColumnName;
            this.classification = node.TermNameOfContainer;
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    Guid siteCollColumnID = GetSiteCollectionRevIMColumnID();//current logic to init all classification column to same Id & internalname
                    if (siteCollColumnID != Guid.Empty)
                    {
                        try
                        {
                            #region update sitecollection classification column
                            bool isFieldExist = false;
                            try
                            {
                                Field dispalyNameField = this.mWeb.Fields.GetByTitle(node.ColumnName);
                                mClientContext.Load(dispalyNameField);
                                mClientContext.ExecuteQuery();
                                string getTitle = dispalyNameField.Title;
                                if (dispalyNameField.Id != RevIMClassificationColumnID)
                                {
                                    isFieldExist = true;
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Info("Not get same column from site {0} : {1}", node.FullPath, e.ToString());
                            }
                            if (isFieldExist)
                            {
                                logger.Warn("Have same name column in the SiteCollection");
                                throw new NameConflictException("Have same name column in the SiteCollection");
                            }

                            mClientContext.Load(mWeb, w => w.Fields);
                            Field siteColumn = mWeb.Fields.GetById(siteCollColumnID);
                            mClientContext.Load(siteColumn);//之前设置过Global Settings，但是由于一些原因在这个site collection中没有创建出来column，所以web中get不到，会报异常
                            mClientContext.ExecuteQuery();
                            UpdateSiteCollectionBCSColumn(node, siteCollColumnID, siteColumn, ref isSkipped);
                            node.isFailedConfigMetaDataColumn = false;
                            //SharePointSettingDao.AddOrUpdateColumnInfo(m_ClientContext.Site.Id, Guid.Empty, Guid.Empty, fieldId, node);
                            if (isSkipped)
                            {
                                //string skipComment = (!isCustomSetting && null != SharePointSettingDao.LoadSharePointSetting(new Guid(node.SPObjectId), GetSiteId())) ? I18NEntity.GetString("RM_JS_JMD_Comment_ConfiguredCustomSettings") : null;//to do next
                                this.AddDetailToList(mWeb.Title, GetFullUrl(node), "RM_JS_JMD_Status_SkipSiteCollectionColumn", JobDetailsStatus.Skipped, null);
                            }
                            else
                            {
                                this.AddDetailToList(mWeb.Title, GetFullUrl(node), "RM_JS_JMD_Status_UpdateSiteCollectionColumn", JobDetailsStatus.Successful, null);
                            }
                            #endregion
                        }
                        catch (JobStopException ex)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (NameConflictException sne)
                        {
                            throw new Exception(sne.Message);
                        }
                        catch (Exception e)
                        {
                            //之前设置过Global Settings，但是由于一些原因在这个site collection中没有创建出来column，所以web中get不到，会报异常
                            logger.Info("Maybe the column in site is deleted , detail message {0}", e.ToString());
                            logger.Info("Need create new site column Path {0}", node.FullPath);
                            isUpdate = false;
                            #region create new site column
                            InitContext(node);
                            mClientContext.Load(mWeb);
                            TaxonomyField taxField = CreateNewSiteCollectionBCSColumn(node);
                            node.isFailedConfigMetaDataColumn = false;
                            //SharePointSettingDao.AddOrUpdateColumnInfo(m_ClientContext.Site.Id, Guid.Empty, Guid.Empty, taxField.Id, node);
                            this.AddDetailToList(mWeb.Title, GetFullUrl(node), "RM_JS_JMD_Status_AddSiteCollectionColumn", JobDetailsStatus.Successful, null);
                            #endregion
                        }
                    }
                    else
                    {
                        logger.Info("Need create new site column Path {0}", node.FullPath);
                        isUpdate = false;
                        #region create new site column
                        InitContext(node);
                        mClientContext.Load(mWeb);
                        TaxonomyField taxField = CreateNewSiteCollectionBCSColumn(node);
                        //SharePointSettingDao.AddOrUpdateColumnInfo(m_ClientContext.Site.Id, Guid.Empty, Guid.Empty, taxField.Id, node);
                        this.AddDetailToList(mWeb.Title, GetFullUrl(node), "RM_JS_JMD_Status_AddSiteCollectionColumn", JobDetailsStatus.Successful, null);
                        #endregion
                    }
                }
            }
            catch (JobStopException ex)
            {
                SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSColumnFailed | (int)BCSSettingFailedType.AddBCSPropertyFailed);
                throw new JobStopException("This Job is stopped.");
            }
            catch (ServerException se)
            {
                logger.Error("Update or Create new site column error Path {0}:{1}", node.FullPath, se.ToString());
                if (isUpdate)
                {
                    this.AddDetailToList(mWeb.Title, GetFullUrl(node), "RM_JS_JMD_Status_UpdateSiteCollectionColumn", JobDetailsStatus.Failed, "RM_SS_SCAddiOrNameRepeat");
                }
                else
                {
                    this.AddDetailToList(mWeb.Title, GetFullUrl(node), "RM_JS_JMD_Status_AddSiteCollectionColumn", JobDetailsStatus.Failed, "RM_SS_SCAddiOrNameRepeat");
                }
                node.isFailedConfigMetaDataColumn = true;
                SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSColumnFailed);
            }
            catch (Exception e)
            {
                logger.Error("Update or Create new site column error Path {0}:{1}", node.FullPath, e.ToString());
                if (isUpdate)
                {
                    this.AddDetailToList(mWeb.Title, GetFullUrl(node), "RM_JS_JMD_Status_UpdateSiteCollectionColumn", JobDetailsStatus.Failed, e.Message);
                }
                else
                {
                    this.AddDetailToList(mWeb.Title, GetFullUrl(node), "RM_JS_JMD_Status_AddSiteCollectionColumn", JobDetailsStatus.Failed, e.Message);
                }
                node.isFailedConfigMetaDataColumn = true;
                SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSColumnFailed);
                //SharePointSettingDao.AddOrUpdateColumnInfo(m_ClientContext.Site.Id, Guid.Empty, Guid.Empty, Guid.Empty, node);
            }
        }
        private async System.Threading.Tasks.Task<BaseType> AddBCSColumnToListAsync(RMSPTreeNode node)
        {
            BaseType baseType = BaseType.None;
            using (CheckJobStopScope jScope = new CheckJobStopScope())
            {
                if (node.IsUsingExistColumnName)
                {
                    return baseType;
                }
                this.columnDisplayName = node.ColumnName;
                this.classification = node.TermNameOfContainer;
                try
                {

                    Web web = this.mSite.OpenWebById(new Guid(node.Parent.Parent.SPObjectId));
                    mClientContext.Load(web);
                    List list = web.Lists.GetById(new Guid(node.SPObjectId));
                    mClientContext.Load(mWeb);
                    this.mClientContext.ExecuteQuery();

                    mClientContext.Load(list, l => l.Fields, l => l.RootFolder, l => l.BaseTemplate, l => l.Title, l => l.BaseType);
                    mClientContext.ExecuteQuery();

                    baseType = list.BaseType;
                    if (list.BaseTemplate == 600)
                    {
                        logger.Info("Skip the design list & system list{0}", list.RootFolder.Name);
                        return baseType;
                    }
                    if (CheckIsDesignList(list.RootFolder.Name + list.BaseTemplate.ToString()))
                    {
                        logger.Info("Skip the design list {0}", list.RootFolder.Name);
                        return baseType;
                    }


                    bool isSiteCollFieldExist = false;
                    try
                    {
                        Field dispalyNameField = this.mWeb.Fields.GetByTitle(node.ColumnName);
                        mClientContext.Load(dispalyNameField);
                        mClientContext.ExecuteQuery();
                        string getTitle = dispalyNameField.Title;
                        if (dispalyNameField.Id != RevIMClassificationColumnID)
                        {
                            isSiteCollFieldExist = true;
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Info("Not get same column from site {0} : {1}", node.FullPath, e.ToString());
                    }
                    if (isSiteCollFieldExist)
                    {
                        logger.Error("Have same name column in the SiteCollection");
                        this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_UpdateListColumn", JobDetailsStatus.Failed, "Have same name column in the SiteCollection");
                        throw new Exception("Have same name column in the SiteCollection");
                    }

                    bool isListFieldExist = false;
                    try
                    {
                        Field dispalyNameField = list.Fields.GetByTitle(node.ColumnName);
                        mClientContext.Load(dispalyNameField);
                        mClientContext.ExecuteQuery();
                        string getTitle = dispalyNameField.Title;
                        if (dispalyNameField.Id != RevIMClassificationColumnID)
                        {
                            isListFieldExist = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("get field by title error,{0}", ex.ToString());
                    }
                    if (isListFieldExist)
                    {
                        logger.Error("Have same name column in the list");
                        this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_UpdateListColumn", JobDetailsStatus.Failed, "Have same name column in the list");
                        throw new Exception("Have same name column in the list");
                    }
                    
                

                    if (!validateTerm(node, false, "RM_JS_JMD_Status_AddListColumn"))
                    {
                        return baseType;
                    }

                    Guid fieldId = GetSiteCollectionRevIMColumnID();
                    if (fieldId == Guid.Empty)
                    {
                        logger.Info("Site collection not config column {0}", web.Url);
                        var siteCollectionNode = GetSiteCollectionNode(node);
                        var groupNode = GetWebAppNode(node);
                        var globalSetting = SharePointSettingDao.LoadSharePointSetting(new Guid(groupNode.SPObjectId), Guid.Empty);
                        InitTreeNodeSettings(siteCollectionNode, globalSetting);
                        AddBCSColumnToSiteCollection(siteCollectionNode);
                        fieldId = GetSiteCollectionRevIMColumnID();
                        //reload context 
                        InitContext(node);
                        web = this.mSite.OpenWebById(new Guid(node.Parent.Parent.SPObjectId));
                        mClientContext.Load(web);
                        list = web.Lists.GetById(new Guid(node.SPObjectId));
                        mClientContext.Load(mWeb);
                        mClientContext.Load(list, l => l.Fields, l => l.RootFolder, l => l.BaseTemplate, l => l.Title);
                        mClientContext.ExecuteQuery();
                    }
                    Field field = this.mWeb.Fields.GetById(fieldId);
                    //Field field = this.m_Web.Fields.GetByInternalNameOrTitle(fieldName);
                    mClientContext.Load(field);
                    mClientContext.ExecuteQuery();
                    Guid listColumnId = CheckListColumnID(list);
                    if (listColumnId != Guid.Empty)
                    {
                        //更新list column，因为该节点的祖先节点可能设置了custom setting，所以需要更新
                        //（在把column添加到list里时已经打了detail,所以这里不再需要添加detail）
                        UpdateListBCSField(web, list, fieldId, node);
                    }
                    else
                    {
                        AddBCSColumnToList(list, field, node);
                        //UpdateListBCSField(web, list, fieldId, node, false);
                    }
                    //list.BaseTemplate = ListTemplateType
                    if (node.NeedCheckDefaultValue)
                    {
                        if (baseType == BaseType.DocumentLibrary)
                        {
                            //Document Library类型Browse Folder，按照Folder进行设置DefaultValue
                            await SetDefaultValueAsync(node, web, list, list.RootFolder);
                            await SetDefaultValueForFolderAsync(node, web, list);
                        }
                        else
                        {
                            //Generic List类型不进行Browse Folder，按照List进行设置DefaultValue
                            await SetDefaultValueAsync(node, web, list);
                        }
                    }

                }
                catch (ServerUnauthorizedAccessException se)
                {
                    logger.Error("Add site column on list error Path :{0}, {1}", node.FullPath, se.ToString());
                    this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_AddListColumn", JobDetailsStatus.Failed, "RM_SS_DocLibraryAccessDeny");
                    SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSColumnFailed);

                }
                catch (ServerException ex)
                {
                    if (ex.Message.Contains("List does not exist"))
                    {
                        logger.Error("Add site column on list error Path :{0}, error detail {1}", node.FullPath, ex.ToString());
                        this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_AddListColumn", JobDetailsStatus.Failed, "RM_SS_ListIsNotExist");//to do next
                    }
                    else
                    {
                        logger.Error("Add site column on list error Path :{0}, error detail {1}", node.FullPath, ex.ToString());
                        this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_AddListColumn", JobDetailsStatus.Failed, string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), ex.Message));
                    }
                    SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSColumnFailed);
                }
                catch (JobStopException je)
                {
                    logger.Error("While add site column on list[{0}], {1} ", node.FullPath, je.ToString());
                    SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSColumnFailed | (int)BCSSettingFailedType.AddBCSPropertyFailed);
                }
                catch (Exception e)
                {
                    if (!e.Message.Equals("Have same name column in the list", StringComparison.OrdinalIgnoreCase) && !e.Message.Equals("Have same name column in the SiteCollection", StringComparison.OrdinalIgnoreCase))
                    {
                        logger.Error("Add site column on list error Path :{0}, error detail {1}", node.FullPath, e.ToString());
                        this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_AddListColumn", JobDetailsStatus.Failed, string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), e.Message));
                    }
                    SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSColumnFailed);
                }
                return baseType;
            }
        }
        private async System.Threading.Tasks.Task AddBCSColumnToFolderAsync(RMSPTreeNode node)
        {
            using (CheckJobStopScope jScope = new CheckJobStopScope())
            {
                if (node.IsUsingExistColumnName)
                {
                    return;
                }
                this.columnDisplayName = node.ColumnName;
                this.classification = node.TermNameOfContainer;
                try
                {
                    Web web = this.mSite.OpenWebById(new Guid(GetWebNode(node).SPObjectId));
                    mClientContext.Load(web);
                    List list = web.Lists.GetById(new Guid(GetListNode(node).SPObjectId));
                    mClientContext.Load(mWeb);
                    this.mClientContext.ExecuteQuery();

                    mClientContext.Load(list, l => l.RootFolder, l => l.BaseTemplate, l => l.Title);
                    mClientContext.ExecuteQuery();

                    if (list.BaseTemplate == 600)
                    {
                        logger.Info("Skip the design list & system list{0}", list.RootFolder.Name);
                        return;
                    }
                    if (CheckIsDesignList(list.RootFolder.Name + list.BaseTemplate.ToString()))
                    {
                        logger.Info("Skip the design list {0}", list.RootFolder.Name);
                        return;
                    }

                    bool isSiteCollFieldExist = false;
                    try
                    {
                        Field dispalyNameField = this.mWeb.Fields.GetByTitle(node.ColumnName);
                        mClientContext.Load(dispalyNameField);
                        mClientContext.ExecuteQuery();
                        string getTitle = dispalyNameField.Title;
                        if (dispalyNameField.Id != RevIMClassificationColumnID)
                        {
                            isSiteCollFieldExist = true;
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Info("Not get same column from site {0} : {1}", node.FullPath, e.ToString());
                    }
                    if (isSiteCollFieldExist)
                    {
                        logger.Error("Have same name column in the SiteCollection");
                        this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_AddFolderColumn", JobDetailsStatus.Failed, "Have same name column in the SiteCollection");
                        throw new Exception("Have same name column in the SiteCollection");
                    }

                    bool isListFieldExist = false;
                    try
                    {
                        Field dispalyNameField = list.Fields.GetByTitle(node.ColumnName);
                        mClientContext.Load(dispalyNameField);
                        mClientContext.ExecuteQuery();
                        string getTitle = dispalyNameField.Title;
                        if (dispalyNameField.Id != RevIMClassificationColumnID)
                        {
                            isListFieldExist = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("get field by title error,{0}", ex.ToString());
                    }
                    if (isListFieldExist)
                    {
                        logger.Error("Have same name column in the list");
                        this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_AddFolderColumn", JobDetailsStatus.Failed, "Have same name column in the list");
                        throw new Exception("Have same name column in the list");
                    }

                    if (!validateTerm(node, false, "RM_JS_JMD_Status_AddFolderColumn"))
                    {
                        return;
                    }

                    Guid fieldId = GetSiteCollectionRevIMColumnID();
                    if (fieldId == Guid.Empty)
                    {
                        logger.Info("Site collection not config column {0}", web.Url);
                        var siteCollectionNode = GetSiteCollectionNode(node);
                        var groupNode = GetWebAppNode(node);
                        var globalSetting = SharePointSettingDao.LoadSharePointSetting(new Guid(groupNode.SPObjectId), Guid.Empty);
                        InitTreeNodeSettings(siteCollectionNode, globalSetting);
                        AddBCSColumnToSiteCollection(siteCollectionNode);
                        fieldId = GetSiteCollectionRevIMColumnID();
                        //reload context 
                        InitContext(node);
                        web = this.mSite.OpenWebById(new Guid(node.Parent.Parent.SPObjectId));
                        mClientContext.Load(web);
                        list = web.Lists.GetById(new Guid(node.SPObjectId));
                        mClientContext.Load(mWeb);
                        mClientContext.Load(list, l => l.Fields, l => l.RootFolder, l => l.BaseTemplate, l => l.Title);
                        mClientContext.ExecuteQuery();
                    }

                    Field field = this.mWeb.Fields.GetById(fieldId);
                    mClientContext.Load(field);
                    mClientContext.ExecuteQuery();
                    Guid listColumnId = CheckListColumnID(list);
                    if (listColumnId == Guid.Empty)
                    {
                        logger.Info("list not config column {0}", list.RootFolder.ServerRelativeUrl);
                        var listNode = GetListNode(node);
                        AddBCSColumnToList(list, field, listNode);
                        //AddBCSColumnToList(listNode);
                    }


                    try
                    {
                        Folder folder = null;

                        folder = list.ParentWeb.GetFolderByServerRelativeUrl(node.FullPath);
                        mClientContext.Load(folder);
                        mClientContext.ExecuteQuery();


                        if (node.DefaultTermId == Guid.Empty)
                        {
                            try
                            {
                                var defaultValues = GetDefaultFolderValues(list, folder);
                                if (!string.IsNullOrEmpty(defaultValues))
                                {
                                    logger.Warn("'/forms/client_LocationBasedDefaults.html' is not exist.");
                                    var defaultsXmlDoc = new XmlDocument();
                                    defaultsXmlDoc.LoadXml(defaultValues.EncodeAmpersandInHref());
                                    defaultsXmlDoc = RemoveFieldDefault(defaultsXmlDoc, folder.ServerRelativeUrl, BCSColumnName);
                                    UpdateDefaultFolderValues(list, defaultsXmlDoc.OuterXml);
                                }
                                this.AddDetailToList(folder.Name, GetFullUrl(node), "RM_JS_JMD_Status_UpdateFolderColumn", JobDetailsStatus.Successful, null);
                            }
                            catch (Exception e)
                            {
                                logger.Warn("Remove Folder Default Value From '/forms/client_LocationBasedDefaults.html' error:{0}", e.ToString());
                                this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_UpdateFolderColumn", JobDetailsStatus.Failed, string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), e.Message));
                            }
                        }
                        else
                        {
                            var existFolderDefaultValue = string.Empty;
                            try
                            {
                                var defaultValues = GetDefaultFolderValues(list, folder);
                                var defaultsXmlDoc = new XmlDocument();
                                defaultsXmlDoc.LoadXml(defaultValues.EncodeAmpersandInHref());
                                XmlNode xmlNode = SelectSingleFieldDefaultNode(defaultsXmlDoc, folder.ServerRelativeUrl, BCSColumnName);
                                existFolderDefaultValue = xmlNode.InnerText;
                            }
                            catch (Exception)
                            {
                                logger.Warn("Get Field Default Value error, do add logic,path: {0}", node.FullPath);
                            }


                            string wssId = GetTermWssId(node.DefaultTermName, node.DefaultTermId);
                            string folderDefaultValue = wssId + ";#" + node.DefaultTermName + "|" + node.DefaultTermId;

                            if (folderDefaultValue == existFolderDefaultValue)
                            {
                                this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_SkipFolderColumn", JobDetailsStatus.Skipped, null);
                            }
                            else
                            {
                                MetadataDefaults mDefaults = new MetadataDefaults(mClientContext, list);
                                mDefaults.SetFieldDefault(folder, BCSColumnName, folderDefaultValue);
                                mDefaults.Update();
                                mClientContext.ExecuteQuery();
                                if (string.IsNullOrEmpty(existFolderDefaultValue))
                                {
                                    this.AddDetailToList(folder.Name, GetFullUrl(node), "RM_JS_JMD_Status_AddFolderColumn", JobDetailsStatus.Successful, null);
                                }
                                else
                                {
                                    this.AddDetailToList(folder.Name, GetFullUrl(node), "RM_JS_JMD_Status_UpdateFolderColumn", JobDetailsStatus.Successful, null);
                                }
                            }

                            if (node.NeedCheckDefaultValue)
                            {
                                await SetDefaultValueAsync(node, web, list, folder);
                            }
                        }
                    }
                    //catch (ServerUnauthorizedAccessException se)
                    //{
                    //    logger.Error("Add folder column on list error Path :{0}, {1}", node.FullPath, se.ToString());
                    //    this.AddDetailToList(node.Name, GetFullUrl(node), I18NEntity.GetString("RM_JS_JMD_Status_AddFolderColumn"), JobDetailsStatus.Failed, "RM_SS_DocLibraryAccessDeny");
                    //    SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSColumnFailed);

                    //}
                    catch (ServerException ex)
                    {
                        if (ex.Message.Contains("File Not Found"))
                        {
                            logger.Error("Add folder default value error Path :{0}, error detail {1}", node.FullPath, ex.ToString());
                            this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_AddFolderColumn", JobDetailsStatus.Failed, "RM_SS_FolderIsNotExist");//to do next
                        }
                        else
                        {
                            logger.Error("Add folder default value error Path :{0}, error detail {1}", node.FullPath, ex.ToString());
                            this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_AddFolderColumn", JobDetailsStatus.Failed, string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), ex.Message));
                        }
                        SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSColumnFailed);
                    }
                    catch (JobStopException je)
                    {
                        logger.Error("While add folder default value[{0}], {1} ", node.FullPath, je.ToString());
                        SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSColumnFailed | (int)BCSSettingFailedType.AddBCSPropertyFailed);
                    }
                    catch (Exception e)
                    {
                        logger.Error("Add folder default value Path :{0}, error detail {1}", node.FullPath, e.ToString());
                        this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_AddFolderColumn", JobDetailsStatus.Failed, string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), e.Message));
                        SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSColumnFailed);
                    }

                }
                catch (ServerUnauthorizedAccessException se)
                {
                    logger.Error("Add site column on list error Path :{0}, {1}", node.FullPath, se.ToString());
                    this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_AddFolderColumn", JobDetailsStatus.Failed, "RM_SS_DocLibraryAccessDeny");
                    SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSColumnFailed);

                }
                catch (ServerException ex)
                {
                    if (ex.Message.Contains("List does not exist"))
                    {
                        logger.Error("Add site column on list error Path :{0}, error detail {1}", node.FullPath, ex.ToString());
                        this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_AddFolderColumn", JobDetailsStatus.Failed, "RM_SS_ListIsNotExist");//to do next
                    }
                    else
                    {
                        logger.Error("Add site column on list error Path :{0}, error detail {1}", node.FullPath, ex.ToString());
                        this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_AddFolderColumn", JobDetailsStatus.Failed, string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), ex.Message));
                    }
                    SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSColumnFailed);
                }
                catch (JobStopException je)
                {
                    logger.Error("While add site column on list[{0}], {1} ", node.FullPath, je.ToString());
                    SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSColumnFailed | (int)BCSSettingFailedType.AddBCSPropertyFailed);
                }
                catch (Exception e)
                {
                    if (!e.Message.Equals("Have same name column in the list", StringComparison.OrdinalIgnoreCase) && !e.Message.Equals("Have same name column in the SiteCollection", StringComparison.OrdinalIgnoreCase))
                    {
                        logger.Error("Add site column on list error Path :{0}, error detail {1}", node.FullPath, e.ToString());
                        this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_AddFolderColumn", JobDetailsStatus.Failed, string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), e.Message));
                    }
                    SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSColumnFailed);
                }
            }
        }
        private void UpdateSiteCollectionBCSColumn(RMSPTreeNode node, Guid fieldId, Field metadataField, ref bool isSkipped)
        {
            using (CheckJobStopScope jScope = new CheckJobStopScope())
            {
                string nodeDescription = string.Empty;
                if (node.Description != null)
                {
                    nodeDescription = node.Description;
                }
                if (!metadataField.Title.Equals(node.ColumnName))
                {
                    isSkipped = false;
                }
                else if (!nodeDescription.Equals(metadataField.Description))
                {
                    isSkipped = false;
                }
                metadataField.StaticName = "RevIMBCS";
                metadataField.Title = node.ColumnName;
                metadataField.Update();
                mClientContext.ExecuteQuery();//to do next to validate update one time
                TaxonomyField taxField = this.mClientContext.CastTo<TaxonomyField>(metadataField);
                mClientContext.Load(taxField);
                mClientContext.ExecuteQuery();
                node.TermStoreId = mTaxonomy.GetDefaultTermStoreId();
                if (taxField.SspId != node.TermStoreId)
                {
                    isSkipped = false;
                    taxField.SspId = node.TermStoreId;
                }
                taxField.TermSetId = node.TermSetId;
                if (node.TermId != null && node.TermId != Guid.Empty)
                {
                    if (taxField.AnchorId == null || !taxField.AnchorId.Equals(node.TermId))
                    {
                        isSkipped = false;
                    }
                    taxField.AnchorId = node.TermId;
                }
                else
                {
                    taxField.AnchorId = Guid.Empty;
                }
                if (node.DefaultTermId != null && node.DefaultTermId != Guid.Empty)
                {
                    string newDefaultValue = "-1;#" + node.DefaultTermName + "|" + node.DefaultTermId.ToString();
                    if (string.IsNullOrEmpty(taxField.DefaultValue))
                    {
                        isSkipped = false;
                        taxField.DefaultValue = newDefaultValue;
                    }
                    else if (!taxField.DefaultValue.Contains(node.DefaultTermId.ToString()))
                    {
                        isSkipped = false;
                        taxField.DefaultValue = newDefaultValue;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(taxField.DefaultValue) && (node.DefaultTermId == null || node.DefaultTermId == Guid.Empty))
                    {
                        isSkipped = true;
                    }
                    else
                    {
                        isSkipped = false;
                        taxField.DefaultValue = string.Empty;
                    }
                }
                taxField.EnforceUniqueValues = false;
                taxField.AllowMultipleValues = false;
                taxField.Indexed = true;
                if (node.Description == null)
                {
                    taxField.Description = string.Empty;
                }
                else
                {
                    taxField.Description = node.Description;
                }
                if (taxField.IsPathRendered != node.IsDisplyaTermPath)
                {
                    isSkipped = false;
                    taxField.IsPathRendered = node.IsDisplyaTermPath;
                }
                taxField.Required = true;
                taxField.Update();
                mWeb.Update();//to do next validate need update Web or not.
                mClientContext.Load(taxField);
                mClientContext.ExecuteQuery();
            }
        }
        private string GetTermWssId(string term, Guid termId)
        {
            try
            {
                string result = "-1";
                List taxonomyList = this.mClientContext.Web.Lists.GetByTitle("TaxonomyHiddenList");

                CamlQuery camlQueryForTerm = new CamlQuery();
                camlQueryForTerm.ViewXml = @"<View>
          <Query>
          <Where>
            <Eq>
            <FieldRef Name='Term'/>
            <Value Type='Text'>" + term + @"</Value>
            </Eq>
          </Where>
          </Query>
        </View>";

                ListItemCollection termItems = taxonomyList.GetItems(camlQueryForTerm);

                this.mClientContext.Load(termItems);
                this.mClientContext.ExecuteQuery();
                foreach (var termItem in termItems)
                {
                    string taxId = termItem["IdForTerm"].ToString();
                    if (taxId.Equals(termId.ToString()))
                    {
                        return termItem["ID"].ToString();
                    }
                }
                return result;
            }
            catch (Exception e1)
            {
                logger.Warn("get wssid term:{0}, id:{1}, error:{2}", term, termId, e1.ToString());
                return "-1";
            }
        }
        private void UpdateBCSColumnDefaultValue(Web web, List list, RMSPTreeNode node, TaxonomyField listTaxField)
        {
            mClientContext.Load(listTaxField);
            string wssId = GetTermWssId(node.DefaultTermName, node.DefaultTermId);
            if (wssId == "-1")
            {
                try
                {
                    ListItemCreationInformation info = new ListItemCreationInformation()
                    {
                        UnderlyingObjectType = FileSystemObjectType.Folder,
                        FolderUrl = string.Concat("Temporary_Folder_For_WssId_Creation_", DateTime.Now.ToFileTime().ToString())
                    };
                    var item = list.AddItem(info);
                    web.Context.ExecuteQuery();
                    listTaxField.ValidateSetValue(item, node.DefaultTermName + "|" + node.DefaultTermId);
                    web.Context.ExecuteQuery();
                    web.Context.Load(item);
                    dynamic val = item[node.ColumnName];
                    //The folder has now served it's purpose and can safely be removed
                    item.DeleteObject();
                }
                catch (Exception ex)
                {
                    logger.Warn("add item for get wssid error:{0}", ex.ToString());
                }
                wssId = GetTermWssId(node.DefaultTermName, node.DefaultTermId);
                if (wssId == "-1")
                {
                    throw new Exception(string.Format("term not found in the term store,termStoreId:{0},TermSet:{1},TermName:{2}", node.TermStoreId, node.TermSetName, node.DefaultTermName));
                }
                listTaxField.DefaultValue = string.Format("{0};#{1}|{2}", wssId, node.DefaultTermName, node.DefaultTermId);
                listTaxField.Update();
            }
            else
            {
                listTaxField.DefaultValue = wssId + ";#" + node.DefaultTermName + "|" + node.DefaultTermId;
                listTaxField.Update();
            }
            this.mClientContext.ExecuteQuery();
        }
        //private TaxonomyField CreateNewListBCSField(Web web, Guid listId, Guid globalFieldId, RMSPTreeNode node)
        //{
        //    using (CheckJobStopScope jScope = new CheckJobStopScope())
        //    {
        //        string wssId = string.Empty;
        //        mClientContext.Load(web);
        //        var list = web.Lists.GetById(listId);
        //        mClientContext.Load(list, l => l.Fields);
        //        bool isFieldExist = false;
        //        try
        //        {
        //            Field dispalyNameField = list.Fields.GetByTitle(node.ColumnName);
        //            mClientContext.Load(dispalyNameField);
        //            mClientContext.ExecuteQuery();
        //            string getTitle = dispalyNameField.Title;
        //            isFieldExist = true;
        //        }
        //        catch
        //        {

        //        }
        //        if (isFieldExist)
        //        {
        //            logger.Warn("Have same name column in the list");
        //            throw new Exception("Have same name column in the list");
        //        }
        //        Field newListField = null;
        //        try
        //        {
        //            Field f = this.mWeb.Fields.GetById(globalFieldId);
        //            mClientContext.Load(f);
        //            this.mClientContext.ExecuteQuery();
        //            newListField = list.Fields.AddFieldAsXml(f.SchemaXml, true, AddFieldOptions.AddFieldInternalNameHint | AddFieldOptions.AddFieldToDefaultView | AddFieldOptions.AddToAllContentTypes);
        //            this.mClientContext.Load(newListField);
        //            this.mClientContext.ExecuteQuery();
        //        }
        //        catch (Exception e)
        //        {
        //            logger.Info("Custom setting and library is create after sharePoint setting.");
        //            newListField = list.Fields.AddFieldAsXml("<Field Type='" + "TaxonomyFieldType" + "'   Name='" + node.ColumnName + "' DisplayName='" + node.ColumnName + "'  ShowField='Term1033' />", true, AddFieldOptions.AddFieldInternalNameHint | AddFieldOptions.AddFieldToDefaultView | AddFieldOptions.AddToAllContentTypes);
        //            this.mClientContext.Load(newListField);
        //            this.mClientContext.ExecuteQuery();
        //        }

        //        TaxonomyField taxField = this.mClientContext.CastTo<TaxonomyField>(newListField);
        //        this.mClientContext.Load(taxField);
        //        taxField.SspId = mTaxonomy.GetDefaultTermStoreId();
        //        node.TermStoreId = taxField.SspId;
        //        taxField.EnforceUniqueValues = false;
        //        taxField.AllowMultipleValues = false;
        //        taxField.TermSetId = node.TermSetId;
        //        if (node.TermId != null)
        //        {
        //            taxField.AnchorId = node.TermId;
        //        }
        //        if (node.DefaultTermId != null && node.DefaultTermId != Guid.Empty)
        //        {
        //            taxField.DefaultValue = "-1;#" + node.DefaultTermName + "|" + node.DefaultTermId;
        //        }
        //        else
        //        {
        //            taxField.DefaultValue = string.Empty;
        //        }
        //        taxField.Indexed = true;
        //        taxField.Required = true;
        //        if (node.Description == null)
        //        {
        //            taxField.Description = string.Empty;
        //        }
        //        else
        //        {
        //            taxField.Description = node.Description;
        //        }
        //        taxField.Update();
        //        list.Update();
        //        this.mClientContext.ExecuteQuery();
        //        if (node.DefaultTermId != null && node.DefaultTermId != Guid.Empty && taxField.DefaultValue.StartsWith("-1"))
        //        {
        //            UpdateBCSColumnDefaultValue(web, list, node, taxField);
        //        }
        //        return taxField;
        //    }
        //}
        private TaxonomyField UpdateListBCSField(Web web, List list, Guid fieldId, RMSPTreeNode node, bool addDetail = true)
        {
            using (CheckJobStopScope jScope = new CheckJobStopScope())
            {
                bool isSkipped = true;
                Field metadataField = list.Fields.GetById(fieldId);
                mClientContext.Load(metadataField);
                mClientContext.ExecuteQuery();
                string nodeDescription = string.Empty;
                if (node.Description != null)
                {
                    nodeDescription = node.Description;
                }
                if (!nodeDescription.Equals(metadataField.Description))
                {
                    isSkipped = false;
                }
                else if (!metadataField.Title.Equals(node.ColumnName))
                {
                    isSkipped = false;
                }
                metadataField.Title = node.ColumnName;
                metadataField.StaticName = "RevIMBCS";
                try
                {
                    metadataField.Update();
                    mClientContext.ExecuteQuery();
                }
                catch (ServerUnauthorizedAccessException sue)
                {
                    logger.Warn("Process 'UpdateListTaxonomyField' method has ServerUnauthorizedAccessException,error message {0}", sue.ToString());
                    this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_UpdateListColumn", JobDetailsStatus.Failed, "RM_SS_DocLibraryAccessDeny");
                    SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSColumnFailed);
                    return this.mClientContext.CastTo<TaxonomyField>(metadataField);
                }
                catch (Exception ex)
                {
                    logger.Warn("Process 'UpdateListTaxonomyField' method has error,error message {0}", ex.ToString());
                    this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_UpdateListColumn", JobDetailsStatus.Failed, "RM_SS_SCAdditions");
                    SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSColumnFailed);
                    return this.mClientContext.CastTo<TaxonomyField>(metadataField);
                }
                mClientContext.Load(list, l => l.Fields);
                string wssId = string.Empty;
                TaxonomyField taxField = this.mClientContext.CastTo<TaxonomyField>(metadataField);
                this.mClientContext.Load(taxField);
                node.TermStoreId = mTaxonomy.GetDefaultTermStoreId();
                if (node.TermStoreId != taxField.SspId)
                {
                    isSkipped = false;
                    taxField.SspId = node.TermStoreId;
                }
                taxField.EnforceUniqueValues = false;
                taxField.AllowMultipleValues = false;
                taxField.TermSetId = node.TermSetId;
                if (node.TermId != null)
                {
                    if (taxField.AnchorId != node.TermId)
                    {
                        isSkipped = false;
                    }
                    taxField.AnchorId = node.TermId;
                }
                if (node.DefaultTermId != null && node.DefaultTermId != Guid.Empty)
                {
                    if (string.IsNullOrEmpty(taxField.DefaultValue))
                    {
                        isSkipped = false;
                    }
                    else if (!taxField.DefaultValue.Contains(node.DefaultTermId.ToString()))
                    {
                        isSkipped = false;
                    }
                    taxField.DefaultValue = "-1" + ";#" + node.DefaultTermName + "|" + node.DefaultTermId;
                }
                else
                {
                    if (!string.IsNullOrEmpty(taxField.DefaultValue))
                    {
                        isSkipped = false;
                        taxField.DefaultValue = string.Empty;
                    }
                }
                taxField.Indexed = true;
                taxField.Required = true;
                if (node.Description == null)
                {
                    taxField.Description = string.Empty;
                }
                else
                {
                    taxField.Description = node.Description;
                }
                if (taxField.IsPathRendered != node.IsDisplyaTermPath)
                {
                    isSkipped = false;
                    taxField.IsPathRendered = node.IsDisplyaTermPath;
                }
                taxField.Update();
                list.Update();
                this.mClientContext.Load(taxField);
                this.mClientContext.ExecuteQuery();
                if (node.DefaultTermId != null && node.DefaultTermId != Guid.Empty && taxField.DefaultValue.StartsWith("-1"))
                {
                    UpdateBCSColumnDefaultValue(web, list, node, taxField);
                }
                if (addDetail)
                {
                    if (isSkipped)
                    {
                        this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_SkipListColumn", JobDetailsStatus.Skipped, null);
                    }
                    else
                    {
                        this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_UpdateListColumn", JobDetailsStatus.Successful, null);
                    }
                }
                return taxField;
            }
        }
        private void AddBCSColumnToList(List list, Field field, RMSPTreeNode node)
        {
            using (CheckJobStopScope jScope = new CheckJobStopScope())
            {
                mClientContext.Load(list, l => l.Fields, l => l.RootFolder, l => l.BaseTemplate, l => l.Title);
                mClientContext.ExecuteQuery();
                bool isFieldExist = false;
                try
                {
                    Field dispalyNameField = list.Fields.GetByTitle(node.ColumnName);
                    mClientContext.Load(dispalyNameField);
                    mClientContext.ExecuteQuery();
                    string getTitle = dispalyNameField.Title;
                    isFieldExist = true;
                }
                catch(Exception e)
                {
                    logger.Warn($"An error has occurred when AddBCSColumnToList, message:{e.Message}");
                }
                if (isFieldExist)
                {
                    logger.Warn("Have same name column in the list");
                    throw new Exception("Have same name column in the list");
                }
                if (list.BaseTemplate == 600)
                {
                    logger.Info("Skip the design list & system list{0}", list.RootFolder.Name);
                    return;
                }
                if (CheckIsDesignList(list.RootFolder.Name + list.BaseTemplate.ToString()))
                {
                    logger.Info("Skip the design list {0}", list.RootFolder.Name);
                    return;
                }
                Field newListField = list.Fields.AddFieldAsXml(field.SchemaXml, true, AddFieldOptions.AddFieldInternalNameHint | AddFieldOptions.AddFieldToDefaultView | AddFieldOptions.AddToAllContentTypes);
                if (node.Description == null)
                {
                    newListField.Description = string.Empty;
                }
                else
                {
                    newListField.Description = node.Description;
                }

                newListField.Update();

                #region modified RECO-312
                var newTaxField = mClientContext.CastTo<TaxonomyField>(newListField);
                this.mClientContext.Load(newTaxField);
                this.mClientContext.ExecuteQuery();
                var isNeedUpdate = false;
                if (newTaxField.AnchorId != node.TermId)
                {
                    isNeedUpdate = true;
                    newTaxField.SspId = this.mTaxonomy.GetDefaultTermStoreId();
                    newTaxField.EnforceUniqueValues = false;
                    newTaxField.AllowMultipleValues = false;
                    newTaxField.TermSetId = node.TermSetId;
                    if (node.TermId != null)
                    {
                        newTaxField.AnchorId = node.TermId;
                    }
                    if (node.DefaultTermId != null && node.DefaultTermId != Guid.Empty)
                    {
                        newTaxField.DefaultValue = "-1" + ";#" + node.DefaultTermName + "|" + node.DefaultTermId;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(newTaxField.DefaultValue))
                        {
                            newTaxField.DefaultValue = string.Empty;
                        }
                    }
                    newTaxField.Indexed = true;
                    newTaxField.Required = true;
                }
                if (newTaxField.IsPathRendered != node.IsDisplyaTermPath)
                {
                    isNeedUpdate = true;
                    newTaxField.IsPathRendered = node.IsDisplyaTermPath;
                }
                if (isNeedUpdate)
                {
                    newTaxField.Update();
                }
                #endregion

                this.mClientContext.Load(newListField);
                this.mClientContext.ExecuteQuery();
                Field listField = list.Fields.GetById(field.Id);
                var webTaxField = this.mClientContext.CastTo<TaxonomyField>(field);
                var listTaxField = this.mClientContext.CastTo<TaxonomyField>(listField);
                this.mClientContext.Load(listTaxField, f => f.DefaultValue, f => f.AnchorId);
                this.mClientContext.ExecuteQuery();
                if (node.TermId != null && node.TermId != Guid.Empty)
                {
                    listTaxField.AnchorId = node.TermId;
                    listTaxField.Update();
                    this.mClientContext.Load(listTaxField);
                    this.mClientContext.ExecuteQuery();
                }
                if (!string.IsNullOrEmpty(node.DefaultTermName) && node.DefaultTermId != Guid.Empty)
                {
                    if (listTaxField.DefaultValue != null && listTaxField.DefaultValue.StartsWith("-1")) //+ node.DefaultTermName + "|" + node.DefaultTermId))
                    {
                        UpdateBCSColumnDefaultValue(this.mWeb, list, node, listTaxField);
                    }
                    if (webTaxField.DefaultValue != null && webTaxField.DefaultValue.StartsWith("-1") && webTaxField.DefaultValue.Contains(node.DefaultTermId.ToString()))
                    {
                        //hanlde SiteCollection Default Value....
                        UpdateBCSColumnDefaultValue(this.mWeb, list, node, webTaxField);
                    }
                }

                this.AddDetailToList(list.Title, GetFullUrl(node), "RM_JS_JMD_Status_AddListColumn", JobDetailsStatus.Successful, null);
            }
        }
        #endregion

        #region update container Classification property

        private void AddOrUpdateWebProperty(Web web, RMSPTreeNode node)//, bool rootNodeIsSite = false)//currentlogic now ,no need to check the level 
        {
            using (CheckJobStopScope jScope = new CheckJobStopScope())
            {
                #region init Resx
                string strAddClassificationResx, strUpdateClassificationResx, strSkipClassificationResx = "";
                Web rootWeb = this.mSite.RootWeb;
                mClientContext.Load(rootWeb, r => r.Id);
                mClientContext.Load(web, w => w.AllProperties, w => w.Id, w => w.Title);
                mClientContext.ExecuteQuery();

                if (rootWeb.Id.Equals(web.Id))
                {
                    logger.Info("web is rootweb so show resx sitecollection");
                    strAddClassificationResx = I18NEntity.GetString("RM_JS_JMD_Status_AddSiteCollectionClassification");
                    strUpdateClassificationResx = I18NEntity.GetString("RM_JS_JMD_Status_UpdateSiteCollectionClassification");
                    strSkipClassificationResx = I18NEntity.GetString("RM_JS_JMD_Status_SkipSiteCollectionClassification");
                }
                else
                {
                    strAddClassificationResx = I18NEntity.GetString("RM_JS_JMD_Status_AddWebClassification");
                    strUpdateClassificationResx = I18NEntity.GetString("RM_JS_JMD_Status_UpdateWebClassification");
                    strSkipClassificationResx = I18NEntity.GetString("RM_JS_JMD_Status_SkipWebClassification");
                }
                #endregion
                try
                {
                    if (!validateTerm(node, true, strAddClassificationResx))
                    {
                        return;
                    }
                    if (!node.TermIdOfContainer.Equals(Guid.Empty))
                    {
                        if (!string.IsNullOrEmpty(web.AllProperties["RevIM"].ToString()) && !mTaxonomy.ValidateTermId(node.TermIdOfContainer))
                        {
                            this.AddDetailToList(web.Title, GetFullUrl(node), strUpdateClassificationResx, JobDetailsStatus.Failed, "RM_SS_ConfigureClassificationFailed");
                            logger.Warn("Update web level classification error causing of term not exist");
                            SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSPropertyFailed);
                            return;
                        }
                    }
                    //if (!rootNodeIsSite)
                    //{
                    //    RMSharePointSetting siteCustomSetting = SharePointSettingDao.LoadSharePointSetting(web.Id, GetSiteId());
                    //    if (siteCustomSetting != null)
                    //    {
                    //        this.AddDetailToList(web.Title, GetFullUrl(node), strSkipClassificationResx, JobDetailsStatus.Skipped, "RM_JS_JMD_Comment_ConfiguredCustomSettings");
                    //        return;
                    //    }
                    //}
                    if (!web.AllProperties["RevIM"].ToString().Equals(node.TermIdOfContainer.ToString()))
                    {
                        web.AllProperties["RevIM"] = node.TermIdOfContainer.ToString();
                        web.Update();
                        mClientContext.ExecuteQuery();
                        this.AddDetailToList(web.Title, GetFullUrl(node), strUpdateClassificationResx, JobDetailsStatus.Successful, null);
                    }
                    else
                    {
                        if (node.TermIdOfContainer.Equals(Guid.Empty)) { return; }
                        this.AddDetailToList(web.Title, GetFullUrl(node), strSkipClassificationResx, JobDetailsStatus.Skipped, null);
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        if (node.TermIdOfContainer.Equals(Guid.Empty))
                        {
                            return;
                        }
                        if (!mTaxonomy.ValidateTermId(node.TermIdOfContainer))
                        {
                            this.AddDetailToList(node.Title, GetFullUrl(node), strAddClassificationResx, JobDetailsStatus.Failed, "RM_SS_ConfigureClassificationFailed");
                            logger.Warn("Add web level classification error causing of term not exist");
                            SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddPhysicalPropertyFailed);
                            return;
                        }

                        logger.Info("Add or update Web Property {0}:{1} reset new", node.FullPath, ex.ToString());
                        web.AllProperties["RevIM"] = node.TermIdOfContainer.ToString();
                        web.Update();
                        mClientContext.ExecuteQuery();
                        this.AddDetailToList(web.Title, GetFullUrl(node), strAddClassificationResx, JobDetailsStatus.Successful, null);
                    }
                    catch (Exception ex1)
                    {
                        logger.Error("Add or update List Classification failed {0}", ex1.ToString());
                        this.AddDetailToList(web.Title, GetFullUrl(node), strAddClassificationResx, JobDetailsStatus.Failed, ex1.Message);
                        SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddPhysicalPropertyFailed);
                    }
                }
            }
        }
        private void AddOrUpdateListProperty(List list, RMSPTreeNode node)//, bool rootNodeIsList = true)
        {
            using (CheckJobStopScope jScope = new CheckJobStopScope())
            {
                try
                {
                    mClientContext.Load(list);
                    mClientContext.Load(list, l => l.RootFolder, l => l.Id, l => l.BaseTemplate);
                    mClientContext.ExecuteQuery();
                }
                catch (Exception e)
                {
                    logger.Info("load root folder error {0}", e.ToString());
                }
                Folder rootFolder = list.RootFolder;

                mClientContext.Load(rootFolder, f => f.Properties, f => f.Name);
                mClientContext.ExecuteQuery();
                if (list.BaseTemplate == 600)
                {
                    logger.Info("Skip the design list & system list{0}", list.RootFolder.Name);
                    return;
                }
                if (CheckIsDesignList(list.RootFolder.Name + list.BaseTemplate.ToString()))
                {
                    logger.Info("Skip the design list {0}", list.RootFolder.Name);
                    return;
                }
                if (!validateTerm(node, true, "RM_JS_JMD_Status_AddListClassification"))
                {
                    return;
                }
                try
                {
                    //if (!rootNodeIsList)
                    //{
                    //    RMSharePointSetting libCustomSetting = SharePointSettingDao.LoadSharePointSetting(list.Id, GetSiteId());
                    //    if (libCustomSetting != null)
                    //    {
                    //        this.AddDetailToList(node.Name, GetFullUrl(node), I18NEntity.GetString("RM_JS_JMD_Status_SkipListClassification"), JobDetailsStatus.Skipped, "RM_JS_JMD_Comment_ConfiguredCustomSettings");
                    //        return;
                    //    }
                    //}

                    if (!node.TermIdOfContainer.Equals(Guid.Empty))
                    {
                        if (!string.IsNullOrEmpty(rootFolder.Properties["RevIM"].ToString()) && !mTaxonomy.ValidateTermId(node.TermIdOfContainer))
                        {
                            this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_UpdateListClassification", JobDetailsStatus.Failed, "RM_SS_ConfigureClassificationFailed");
                            logger.Warn("Update list level classification error causing of term not exist");
                            SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSPropertyFailed);
                            return;
                        }
                    }
                    if (!rootFolder.Properties["RevIM"].ToString().Equals(node.TermIdOfContainer.ToString()))
                    {
                        rootFolder.Properties["RevIM"] = node.TermIdOfContainer.ToString();
                        rootFolder.Update();
                        mClientContext.ExecuteQuery();
                        this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_UpdateListClassification", JobDetailsStatus.Successful, null);
                    }
                    else
                    {
                        if (node.TermIdOfContainer.Equals(Guid.Empty)) { return; }
                        this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_SkipListClassification", JobDetailsStatus.Skipped, null);
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        if (node.TermIdOfContainer.Equals(Guid.Empty))
                        {
                            return;
                        }
                        if (!mTaxonomy.ValidateTermId(node.TermIdOfContainer))
                        {
                            this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_AddListClassification", JobDetailsStatus.Failed, "RM_SS_ConfigureClassificationFailed");
                            logger.Warn("Add list level classification error causing of term not exist");
                            SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSPropertyFailed);
                            return;
                        }

                        logger.Info("Add or update list Property {0}:{1} reset new", node.FullPath, ex.ToString());
                        rootFolder.Properties["RevIM"] = node.TermIdOfContainer.ToString();
                        rootFolder.Update();
                        mClientContext.ExecuteQuery();
                        this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_AddListClassification", JobDetailsStatus.Successful, null);
                    }
                    catch (Exception ex1)
                    {
                        logger.Error("Add or update List Classification failed {0}", ex1.ToString());
                        this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_AddListClassification", JobDetailsStatus.Failed, ex1.Message);
                        SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSPropertyFailed);
                    }
                }
            }
        }
        private void AddOrUpdateFolderProperty(List list, Folder folder, RMSPTreeNode node)//, bool rootNodeIsList = true)
        {
            using (CheckJobStopScope jScope = new CheckJobStopScope())
            {
                try
                {
                    mClientContext.Load(list);
                    mClientContext.Load(list, l => l.RootFolder, l => l.Id, l => l.BaseTemplate);
                    mClientContext.ExecuteQuery();
                }
                catch (Exception e)
                {
                    logger.Info("load root folder error {0}", e.ToString());
                }

                mClientContext.Load(folder, f => f.Properties, f => f.Name);
                mClientContext.ExecuteQuery();
                if (list.BaseTemplate == 600)
                {
                    logger.Info("Skip the design list & system list{0}", list.RootFolder.Name);
                    return;
                }
                if (CheckIsDesignList(list.RootFolder.Name + list.BaseTemplate.ToString()))
                {
                    logger.Info("Skip the design list {0}", list.RootFolder.Name);
                    return;
                }
                if (!validateTerm(node, true, "RM_JS_JMD_Status_AddFolderClassification"))
                {
                    return;
                }
                try
                {
                    if (!node.TermIdOfContainer.Equals(Guid.Empty))
                    {
                        if (!mTaxonomy.ValidateTermId(node.TermIdOfContainer))
                        {
                            this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_UpdateFolderClassification", JobDetailsStatus.Failed, "RM_SS_ConfigureClassificationFailed");
                            logger.Warn("Update list level classification error causing of term not exist");
                            SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSPropertyFailed);
                            return;
                        }
                    }
                    //folder.Properties["RevIMBCS"] = node.TermNameOfContainer + "|" + node.TermIdOfContainer.ToString();
                    //folder.Update();
                    //mClientContext.ExecuteQuery();
                    //this.AddDetailToList(node.Name, GetFullUrl(node), I18NEntity.GetString("RM_JS_JMD_Status_UpdateFolderClassification"), JobDetailsStatus.Successful, null);


                    if (!node.TermIdOfContainer.Equals(Guid.Empty))
                    {
                        if (!string.IsNullOrEmpty(folder.Properties["RevIM"].ToString()) && !mTaxonomy.ValidateTermId(node.TermIdOfContainer))
                        {
                            this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_UpdateFolderClassification", JobDetailsStatus.Failed, "RM_SS_ConfigureClassificationFailed");
                            logger.Warn("Update list level classification error causing of term not exist");
                            SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSPropertyFailed);
                            return;
                        }
                    }
                    if (!folder.Properties["RevIM"].ToString().Equals(node.TermIdOfContainer.ToString()))
                    {
                        folder.Properties["RevIM"] = node.TermIdOfContainer.ToString();
                        folder.Update();
                        mClientContext.ExecuteQuery();
                        this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_UpdateFolderClassification", JobDetailsStatus.Successful, null);
                    }
                    else
                    {
                        if (node.TermIdOfContainer.Equals(Guid.Empty)) { return; }
                        this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_SkipFolderClassification", JobDetailsStatus.Skipped, null);
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        if (node.TermIdOfContainer.Equals(Guid.Empty))
                        {
                            return;
                        }
                        if (!mTaxonomy.ValidateTermId(node.TermIdOfContainer))
                        {
                            this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_AddFolderClassification", JobDetailsStatus.Failed, "RM_SS_ConfigureClassificationFailed");
                            logger.Warn("Add list level classification error causing of term not exist");
                            SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSPropertyFailed);
                            return;
                        }

                        logger.Info("Add or update list Property {0}:{1} reset new", node.FullPath, ex.ToString());
                        folder.Properties["RevIM"] = node.TermIdOfContainer.ToString();
                        folder.Update();
                        mClientContext.ExecuteQuery();
                        this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_AddFolderClassification", JobDetailsStatus.Successful, null);
                    }
                    catch (Exception ex1)
                    {
                        logger.Error("Add or update List Classification failed {0}", ex1.ToString());
                        this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_AddFolderClassification", JobDetailsStatus.Failed, ex1.Message);
                        SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSPropertyFailed);
                    }
                }
            }
        }
        public void UpdateWebClassification(RMSPTreeNode node)
        {
            using (CheckJobStopScope jScope = new CheckJobStopScope())
            {
                this.columnDisplayName = node.ColumnName;
                this.classification = node.TermNameOfContainer;
                Web web = this.mSite.OpenWebById(new Guid(node.SPObjectId));
                AddOrUpdateWebProperty(web, node);
            }
        }
        public void UpdateListClassification(RMSPTreeNode node)
        {
            using (CheckJobStopScope jScope = new CheckJobStopScope())
            {
                this.columnDisplayName = node.ColumnName;
                this.classification = node.TermNameOfContainer;
                Web web = this.mSite.OpenWebById(new Guid(node.Parent.Parent.SPObjectId));//to do next Replace to node.WebID?
                mClientContext.Load(web);
                List list = web.Lists.GetById(new Guid(node.SPObjectId));
                AddOrUpdateListProperty(list, node);
            }
        }
        public void UpdateFolderClassification(RMSPTreeNode node)
        {
            using (CheckJobStopScope jScope = new CheckJobStopScope())
            {
                this.columnDisplayName = node.ColumnName;
                this.classification = node.TermNameOfContainer;

                Web web = this.mSite.OpenWebById(new Guid(GetWebNode(node).SPObjectId));
                List list = web.Lists.GetById(new Guid(GetListNode(node).SPObjectId));
                //var folder = list.RootFolder.Folders.GetByUrl(node.FullPath);
                var folder = list.ParentWeb.GetFolderByServerRelativeUrl(node.FullPath);
                //mClientContext.Load(web);
                mClientContext.Load(list);
                mClientContext.Load(mWeb);
                this.mClientContext.ExecuteQuery();

                AddOrUpdateFolderProperty(list, folder, node);
            }
        }


        #endregion

        #region app column and setting

        private void AddRelatedColumnToSiteCollection(RMSPTreeNode node)
        {
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    Guid siteCollColumnID = GetSiteCollectionRelatedColumnID();//current logic to init all classification column to same Id & internalname
                    if (siteCollColumnID != Guid.Empty)
                    {
                        this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_AddSiteCollectionRelatedColumnSkip", JobDetailsStatus.Skipped, null);
                    }
                    else
                    {
                        logger.Info("Need create new site column Path {0}", node.FullPath);
                        #region create new site column
                        InitContext(node);
                        mClientContext.Load(mWeb);
                        string columnSchema = "<Field Type=\"Note\" DisplayName='" + relatedColumnDisplayName + "' RichText=\"TRUE\" RichTextMode=\"FullHtml\" Group=\"Custom Columns\"  ID=\"{b40273fb-26d2-40e8-9a34-dd20bc9ca1d7}\"   Name='" + relatedColumnInternalName + "' ShowInDisplayForm='TRUE' ShowInEditForm='FALSE' ShowInNewForm='FALSE' ShowInFileDlg='FALSE' ShowInListSettings='FALSE' ShowInVersionHistory='TRUE' ShowInViewForms='TRUE' UnlimitedLengthInDocumentLibrary=\"TRUE\"  />";
                        mWeb.Fields.AddFieldAsXml(columnSchema, true, AddFieldOptions.AddFieldInternalNameHint | AddFieldOptions.AddFieldToDefaultView | AddFieldOptions.AddToAllContentTypes);
                        mWeb.Update();
                        mClientContext.ExecuteQuery();
                        this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_AddSiteCollectionRelatedColumn", JobDetailsStatus.Successful, null);
                        #endregion
                    }
                }
            }
            catch (JobStopException ex)
            {
                //SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSColumnFailed | (int)BCSSettingFailedType.AddBCSPropertyFailed);
                this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_AddSiteCollectionRelatedColumn", JobDetailsStatus.Failed, "RM_SS_DocLibraryAccessDeny");
                throw new JobStopException("This Job is stopped.");
            }
            catch (ServerException se)
            {
                logger.Error("Update or Create new site app column error Path {0}:{1}", node.FullPath, se.ToString());
                //SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSColumnFailed);
                this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_AddSiteCollectionRelatedColumn", JobDetailsStatus.Failed, "RM_SS_ListIsNotExist");
            }
            catch (Exception e)
            {
                logger.Error("Update or Create new site app column error Path {0}:{1}", node.FullPath, e.ToString());

                //node.isFailedConfigMetaDataColumn = true;
                //SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSColumnFailed);
                this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_AddSiteCollectionRelatedColumn", JobDetailsStatus.Failed, string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), e.Message));
            }
        }

        private void AddRelatedColumnToList(RMSPTreeNode node)
        {
            using (CheckJobStopScope jScope = new CheckJobStopScope())
            {
                try
                {
                    Web web = this.mSite.OpenWebById(new Guid(node.Parent.Parent.SPObjectId));
                    mClientContext.Load(web);
                    List list = web.Lists.GetById(new Guid(node.SPObjectId));
                    mClientContext.Load(mWeb);
                    this.mClientContext.ExecuteQuery();

                    mClientContext.Load(list, l => l.Fields, l => l.RootFolder, l => l.BaseTemplate, l => l.Title);
                    mClientContext.ExecuteQuery();

                    if (list.BaseTemplate == 600)
                    {
                        logger.Info("Skip the design list & system list{0}", list.RootFolder.Name);
                        return;
                    }
                    if (CheckIsDesignList(list.RootFolder.Name + list.BaseTemplate.ToString()))
                    {
                        logger.Info("Skip the design list {0}", list.RootFolder.Name);
                        return;
                    }

                    Guid fieldId = GetSiteCollectionRelatedColumnID();
                    if (fieldId == Guid.Empty)
                    {
                        logger.Info("Site collection not config app column {0}", web.Url);
                        var siteCollectionNode = GetSiteCollectionNode(node);
                        var groupNode = GetWebAppNode(node);
                        var globalSetting = SharePointSettingDao.LoadSharePointSetting(new Guid(groupNode.SPObjectId), Guid.Empty);
                        InitTreeNodeSettings(siteCollectionNode, globalSetting);
                        AddRelatedColumnToSiteCollection(siteCollectionNode);
                        fieldId = GetSiteCollectionRelatedColumnID();
                        //reload context 
                        InitContext(siteCollectionNode);
                        web = this.mSite.OpenWebById(new Guid(node.Parent.Parent.SPObjectId));
                        mClientContext.Load(web);
                        list = web.Lists.GetById(new Guid(node.SPObjectId));
                        mClientContext.Load(mWeb);
                        mClientContext.Load(list, l => l.Fields, l => l.RootFolder, l => l.BaseTemplate, l => l.Title);
                        mClientContext.ExecuteQuery();
                    }
                    Field field = this.mWeb.Fields.GetById(fieldId);
                    mClientContext.Load(field);
                    mClientContext.ExecuteQuery();
                    Guid listColumnId = CheckRelatedColumnID(list);
                    if (listColumnId != Guid.Empty)
                    {
                        this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_AddListRelatedColumnSkip", JobDetailsStatus.Skipped, null);
                    }
                    else
                    {
                        mClientContext.Load(list, l => l.Fields);
                        mClientContext.ExecuteQuery();
                        list.Fields.AddFieldAsXml(field.SchemaXml, true, AddFieldOptions.AddFieldInternalNameHint | AddFieldOptions.AddFieldToDefaultView | AddFieldOptions.AddToAllContentTypes);
                        list.Update();
                        mClientContext.ExecuteQuery();
                        this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_AddListRelatedColumn", JobDetailsStatus.Successful, null);
                    }

                }
                catch (ServerUnauthorizedAccessException se)
                {
                    logger.Error("Add site column on list error Path :{0}, {1}", node.FullPath, se.ToString());
                    this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_AddListRelatedColumn", JobDetailsStatus.Failed, "RM_SS_DocLibraryAccessDeny");
                    //SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSColumnFailed);

                }
                catch (ServerException ex)
                {
                    if (ex.Message.Contains("List does not exist"))
                    {
                        logger.Error("Add site column on list error Path :{0}, error detail {1}", node.FullPath, ex.ToString());
                        this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_AddListRelatedColumn", JobDetailsStatus.Failed, "RM_SS_ListIsNotExist");//to do next
                    }
                    else
                    {
                        logger.Error("Add site column on list error Path :{0}, error detail {1}", node.FullPath, ex.ToString());
                        this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_AddListRelatedColumn", JobDetailsStatus.Failed, string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), ex.Message));
                    }
                    //SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSColumnFailed);
                }
                catch (JobStopException je)
                {
                    logger.Error("While add site column on list[{0}], {1} ", node.FullPath, je.ToString());
                    //SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSColumnFailed | (int)BCSSettingFailedType.AddBCSPropertyFailed);
                }
                catch (Exception e)
                {
                    logger.Error("Add site column on list error Path :{0}, error detail {1}", node.FullPath, e.ToString());
                    this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_JMD_Status_AddListRelatedColumn", JobDetailsStatus.Failed, string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), e.Message));
                    //SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.AddBCSColumnFailed);
                }
            }
        }

        public Guid GetSiteCollectionRelatedColumnID()
        {
            try
            {
                mClientContext.Load(mWeb, w => w.Fields);
                Field metadataField = mWeb.Fields.GetById(relatedColumnId);
                mClientContext.Load(metadataField);
                mClientContext.ExecuteQuery();
                logger.Info("check site app column id");
                return relatedColumnId;
            }
            catch (Exception ex)
            {
                logger.Info("Site not config app column {0}", ex.ToString());
            }
            return Guid.Empty;
        }

        public Guid CheckRelatedColumnID(List list)
        {
            try
            {
                logger.Info("Check list column id");
                mClientContext.Load(list, l => l.Fields);
                Field metadataField = list.Fields.GetById(relatedColumnId);
                mClientContext.Load(metadataField);
                mClientContext.ExecuteQuery();
                return relatedColumnId;
            }
            catch (Exception ex)
            {
                logger.Info("List not config metadata app column {0}", ex.ToString());
            }
            return Guid.Empty;

        }

        public void AddWebApp(RMSPTreeNode node)
        {
            try
            {
                if (node.Level == (int)NodeLevel.SiteCollection)
                {
                    var BPOSInfo = PoolUserUtil.GetAveBPOSAccountInfo(node.BposInfo, node.FullPath);
                    var tokenProvider = BPOSInfo.Convert2TokenProvider();
                    RelatedRecordsAppUtility util = new RelatedRecordsAppUtility(mClientContext, tokenProvider, mSite, mWeb, node.FullPath);
                    util.AddAnApp(new Guid("e1fa5ab5-0db3-4a7b-91b6-322b28de4116"));
                }
                else if (node.Level == (int)NodeLevel.Site)
                {
                    var siteNode = GetSiteCollectionNode(node);
                    var BPOSInfo = PoolUserUtil.GetAveBPOSAccountInfo(siteNode.BposInfo, siteNode.FullPath);
                    var tokenProvider = BPOSInfo.Convert2TokenProvider();
                    var web = this.mSite.OpenWebById(new Guid(node.SPObjectId));
                    mClientContext.Load(web);
                    mClientContext.ExecuteQuery();

                    if (web.Id == mWeb.Id)
                    {
                        logger.Info("add app skip root web");
                    }
                    RelatedRecordsAppUtility util = new RelatedRecordsAppUtility(mClientContext, tokenProvider, mSite, web, node.FullPath);
                    util.AddAnApp(new Guid("e1fa5ab5-0db3-4a7b-91b6-322b28de4116"));
                }
            }
            catch (Exception ex)
            {
                logger.Warn("add web app error:{0}", ex.ToString());
                this.AddDetailToList(node.Name, node.FullPath, "RM_JMD_AddApp", JobDetailsStatus.Failed, "RM_SS_AddAppError");
            }
        }

        public void UnInstallWebApp(RMSPTreeNode node)
        {
            try
            {
                if (node.Level == (int)NodeLevel.SiteCollection)
                {
                    var BPOSInfo = PoolUserUtil.GetAveBPOSAccountInfo(node.BposInfo, node.FullPath);
                    var tokenProvider = BPOSInfo.Convert2TokenProvider();
                    RelatedRecordsAppUtility util = new RelatedRecordsAppUtility(mClientContext, tokenProvider, mSite, mWeb, node.FullPath);
                    util.UninstallApp(new Guid("e1fa5ab5-0db3-4a7b-91b6-322b28de4116"));
                }
                else if (node.Level == (int)NodeLevel.Site)
                {
                    var siteNode = GetSiteCollectionNode(node);
                    var BPOSInfo = PoolUserUtil.GetAveBPOSAccountInfo(siteNode.BposInfo, siteNode.FullPath);
                    var tokenProvider = BPOSInfo.Convert2TokenProvider();
                    var web = this.mSite.OpenWebById(new Guid(node.SPObjectId));
                    mClientContext.Load(web);
                    mClientContext.ExecuteQuery();

                    if (web.Id == mWeb.Id)
                    {
                        logger.Info("uninstall app skip root web");
                    }
                    RelatedRecordsAppUtility util = new RelatedRecordsAppUtility(mClientContext, tokenProvider, mSite, web, node.FullPath);
                    util.UninstallApp(new Guid("e1fa5ab5-0db3-4a7b-91b6-322b28de4116"));                   
                }
            }
            catch (Exception ex)
            {
                logger.Warn("uninstall web app error:{0}", ex.ToString());
            }
        }
        #endregion

        public void ConfigSiteCollectionSetting(RMSPTreeNode node)
        {
            using (CheckJobStopScope jScope = new CheckJobStopScope())
            {
                if (!CheckClassificationSetting(node))
                {
                    return;
                }
                this.columnDisplayName = node.ColumnName;
                if (node.isEnableClassification)
                {
                    this.classification = node.TermNameOfContainer;
                }
                if (!validateTerm(node, false, "RM_JS_JMD_Status_AddSiteCollectionColumn"))
                {
                    return;
                }
                if (!node.IsUsingExistColumnName)
                {
                    AddBCSColumnToSiteCollection(node);
                }
                if (node.EnableRelatedRecords)
                {
                    AddRelatedColumnToSiteCollection(node);
                    AddWebApp(node);
                }
                else
                {
                    UnInstallWebApp(node);
                }
                //if (node.isEnableClassification)
                //{
                //    this.classification = node.TermNameOfContainer;
                //}
            }
        }

        public async System.Threading.Tasks.Task ConfigSubNodeSettingsAsync(RMSPTreeNode node, bool isApplySettingJob = false)//node level is Site or List.
        {
            using (CheckJobStopScope jScope = new CheckJobStopScope())
            { //check list have column
                if ((node.Level == (int)NodeLevel.List || node.Level == (int)NodeLevel.Library) && !node.Hidden)
                {
                    await JobService.UpdateJobWithoutProgressChangeAsync(jobId);
                    try
                    {
                        bool hasCustomSetting = false;
                        var listSetting = SharePointSettingDao.LoadSharePointSetting(new Guid(node.SPObjectId), GetSiteId(), true);
                        if (isApplySettingJob && listSetting != null && listSetting.SettingTime != 0)
                        {
                            logger.Info("No need to set list node {0}", node.FullPath);
                            return;
                        }

                        if (listSetting != null)
                        {
                            hasCustomSetting = true;
                            logger.Info("list have custom setting {0}", node.FullPath);
                            InitTreeNodeSettings(node, listSetting);
                        }

                        if (hasCustomSetting && listSetting.IsEnableHoldPhyical)
                        {
                            AddPhysicalFlagForSPNode(node);
                        }
                        if (!(hasCustomSetting && listSetting.TermId == Guid.Empty) || !hasCustomSetting)//Equals Guid Empty only set physical setting
                        {
                            if (!node.isEnableClassification)
                            {
                                node.TermIdOfContainer = Guid.Empty;
                                node.TermNameOfContainer = "";
                                node.IsInheritParentTerm = false;
                            }
                            BaseType baseType = 
                            await AddBCSColumnToListAsync(node);
                            UpdateListClassification(node);
                            if (node.EnableRelatedRecords)
                            {
                                AddRelatedColumnToList(node);
                                AddWebApp(node);
                            }
                            else
                            {
                                UnInstallWebApp(node);
                            }
                            if (baseType == BaseType.DocumentLibrary)
                            {
                                //For browse Folder node.
                                foreach (var subNode in await RMSPTreeService.BrowseAsync(node))
                                {
                                    InitTreeNodeSettings(subNode, node);
                                    await ConfigSubNodeSettingsAsync(subNode);
                                }
                            }
                        }
                    }
                    catch (JobStopException ex)
                    {
                        throw new JobStopException("This Job is stopped.");
                    }
                    catch (Exception ex)
                    {
                        logger.Info("Set list Setting error {0}", ex.ToString());
                    }
                }
                else if (node.Level == (int)NodeLevel.Site)
                {
                    try
                    {
                        var siteSetting = SharePointSettingDao.LoadSharePointSetting(new Guid(node.SPObjectId), GetSiteId());
                        if (isApplySettingJob && siteSetting != null && siteSetting.SettingTime != 0)
                        {
                            logger.Info("No need to browse sub node {0}", node.FullPath);
                            return;
                        }
                        if (siteSetting != null)
                        {
                            logger.Info("Group Id is {0},node url:{1}", node.Parent.Parent.Parent.Parent.SPObjectId, node.FullPath);
                            siteSetting.ColumnName = SharePointSettingDao.GetMedataColumn(new Guid(node.Parent.Parent.Parent.Parent.SPObjectId));
                            InitTreeNodeSettings(node, siteSetting);
                        }
                        if (!node.isEnableClassification)
                        {
                            node.TermIdOfContainer = Guid.Empty;
                            node.TermNameOfContainer = "";
                            node.IsInheritParentTerm = false;
                        }
                        UpdateWebClassification(node);//to do next no need to check the EnableClassification or not
                        if (node.EnableRelatedRecords)
                        {
                            AddWebApp(node);
                        }
                        else
                        {
                            UnInstallWebApp(node);
                        }
                        foreach (var subNode in await RMSPTreeService.BrowseAsync(node))
                        {
                            InitTreeNodeSettings(subNode, node);
                            await ConfigSubNodeSettingsAsync(subNode);
                        }
                    }
                    catch (JobStopException jex)
                    {
                        throw new JobStopException("This Job is stopped.");
                    }
                    catch (ServerException se)
                    {
                        logger.Info("Error code {0}:Error Message {1}", se.ServerErrorCode, se.Message);
                        if (se.Message.Contains("File Not Found"))
                        {
                            this.AddDetailToList(node.Name, node.FullPath, string.Empty, JobDetailsStatus.Failed, "RM_SS_WebIsNotExist");
                        }
                        SetSettingStatus(node.SettingScopeId, (int)FailedType.ConfigClassification);
                    }
                    catch (Exception ex1)
                    {
                        SetSettingStatus(node.SettingScopeId, (int)FailedType.ConfigClassification);
                        this.AddDetailToList(node.Name, node.FullPath, string.Empty, JobDetailsStatus.Failed, ex1.Message);
                        logger.Info("Set web setting error {0}", ex1.ToString());
                    }
                }
                //for vir node.
                else if (node.Level == (int)NodeLevel.Sites || node.Level == (int)NodeLevel.Lists || node.Level == (int)NodeLevel.Folders || node.Level == (int)NodeLevel.RootFolder)
                {
                    foreach (var subNode in await RMSPTreeService.BrowseAsync(node))
                    {
                        InitTreeNodeSettings(subNode, node);
                        await ConfigSubNodeSettingsAsync(subNode);
                    }
                }
                else if (node.Level == (int)NodeLevel.Folder)
                {
                    try
                    {
                        var folderSetting = SharePointSettingDao.LoadSharePointSetting(new Guid(node.SPObjectId), GetSiteId());
                        if (isApplySettingJob && folderSetting != null && folderSetting.SettingTime != 0)
                        {
                            logger.Info("No need to browse sub node {0}", node.FullPath);
                            return;
                        }
                        if (folderSetting != null)
                        {
                            logger.Info("Group Id is {0},node url:{1}", node.Parent.Parent.Parent.Parent.SPObjectId, node.FullPath);
                            folderSetting.ColumnName = SharePointSettingDao.GetMedataColumn(new Guid(node.Parent.Parent.Parent.Parent.SPObjectId));
                            InitTreeNodeSettings(node, folderSetting);
                        }
                        if (!node.isEnableClassification)
                        {
                            node.TermIdOfContainer = Guid.Empty;
                            node.TermNameOfContainer = "";
                            node.IsInheritParentTerm = false;
                        }
                        await AddBCSColumnToFolderAsync(node);
                        //UpdateFolderClassification(node);
                        foreach (var subNode in await RMSPTreeService.BrowseAsync(node))
                        {
                            InitTreeNodeSettings(subNode, node);
                            await ConfigSubNodeSettingsAsync(subNode);
                        }
                    }
                    catch (JobStopException jex)
                    {
                        throw new JobStopException("This Job is stopped.");
                    }
                    catch (ServerException se)
                    {
                        logger.Info("Error code {0}:Error Message {1}", se.ServerErrorCode, se.Message);
                        if (se.Message.Contains("File Not Found"))
                        {
                            this.AddDetailToList(node.Name, node.FullPath, string.Empty, JobDetailsStatus.Failed, "RM_SS_WebIsNotExist");
                        }
                        SetSettingStatus(node.SettingScopeId, (int)FailedType.ConfigClassification);
                    }
                    catch (Exception ex1)
                    {
                        SetSettingStatus(node.SettingScopeId, (int)FailedType.ConfigClassification);
                        this.AddDetailToList(node.Name, node.FullPath, string.Empty, JobDetailsStatus.Failed, ex1.Message);
                        logger.Info("Set web setting error {0}", ex1.ToString());
                    }
                }
            }
        }

        private bool validateTerm(RMSPTreeNode node, bool isClassification, string action)
        {
            bool result = true;
            string errorMsgKey = isClassification ? "RM_SS_ConfigureClassificationFailed" : "RM_SS_ConfigureColumnFailed";
            action = I18NEntity.GetString(action);
            var faildType = isClassification ? BCSSettingFailedType.AddBCSPropertyFailed : BCSSettingFailedType.AddBCSColumnFailed;
            try
            {
                Guid termId = isClassification ? node.TermIdOfContainer : node.DefaultTermId;

                termId = !isClassification && node.DefaultTermId == Guid.Empty ? node.TermId : termId;
                if (termId != Guid.Empty)
                {
                    logger.Info("validate term, FullPath:{0}, termName:{1}, termId:{2}, classificationTerm:{3},classificationTermId:{4}", node.FullPath, node.DefaultTermName, termId);
                    var dbTerm = TermDao.GetRMTermByGuId(termId);
                    if (dbTerm != null && (dbTerm.IsRemoved || dbTerm.IsDeprecated))
                    {
                        logger.Warn("faild to config sp setting, term is not valid, dbtermremoved:{0}, dbtermdeparecated:{1}, defaultTermId:{2}", dbTerm.IsRemoved, dbTerm.IsDeprecated, termId);
                        if (node.Level == (int)NodeLevel.Site)
                        {
                            this.AddDetailToList(node.Title, GetFullUrl(node), action, JobDetailsStatus.Failed, errorMsgKey);
                        }
                        else
                        {
                            this.AddDetailToList(node.Name, GetFullUrl(node), action, JobDetailsStatus.Failed, errorMsgKey);
                        }
                        SetSettingStatus(node.SettingScopeId, (int)faildType);
                        result = false;
                    }
                    else
                    {
                        var term = mTaxonomy.GetTermFromMMS(termId.ToString());
                        if (term == null || term.IsDeprecated)
                        {
                            AvePoint.GCommon.Utility.ArgumentCheck.NotNull(term, nameof(term));
                            logger.Warn("faild to config sp setting, term is not valid, sptermdeparecated:{0}, defaultTermId:{1}", term.IsDeprecated, termId);
                            if (node.Level == (int)NodeLevel.Site)
                            {
                                this.AddDetailToList(node.Title, GetFullUrl(node), action, JobDetailsStatus.Failed, errorMsgKey);
                            }
                            else
                            {
                                this.AddDetailToList(node.Name, GetFullUrl(node), action, JobDetailsStatus.Failed, errorMsgKey);
                            }
                            SetSettingStatus(node.SettingScopeId, (int)faildType);
                            result = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred while config SP Setting, Term is invaild,ERROR:{0}", ex.ToString());
                if (node.Level == (int)NodeLevel.SiteCollection)
                {
                    this.AddDetailToList(node.Title, GetFullUrl(node), action, JobDetailsStatus.Failed, errorMsgKey);
                }
                else
                {
                    this.AddDetailToList(node.Name, GetFullUrl(node), action, JobDetailsStatus.Failed, errorMsgKey);
                }
                SetSettingStatus(node.SettingScopeId, (int)faildType);
                result = false;
            }
            return result;
        }
        public void InitTreeNodeSettings(RMSPTreeNode node, RMSPTreeNode parentNode)
        {
            node.SettingScopeId = parentNode.SettingScopeId;
            SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.None);
            node.ColumnName = parentNode.ColumnName;
            node.Description = parentNode.Description;
            node.DefaultTermId = parentNode.DefaultTermId;
            node.DefaultTermName = parentNode.DefaultTermName;
            node.TermId = parentNode.TermId;
            node.TermSetId = parentNode.TermSetId;
            node.TermSetName = parentNode.TermSetName;
            node.TermStoreId = parentNode.TermStoreId;
            node.TermNameOfContainer = parentNode.TermNameOfContainer;
            node.TermIdOfContainer = parentNode.TermIdOfContainer;
            node.IsInheritParentTerm = parentNode.IsInheritParentTerm;
            node.isEnableClassification = parentNode.isEnableClassification;
            node.ExistColumnName = parentNode.ExistColumnName;
            node.IsUsingExistColumnName = parentNode.IsUsingExistColumnName;
            node.NeedCheckDefaultValue = parentNode.NeedCheckDefaultValue;
            node.IsDisplyaTermPath = parentNode.IsDisplyaTermPath;
            node.ApplyExistType = parentNode.ApplyExistType;
            node.EnableRelatedRecords = parentNode.EnableRelatedRecords;
            if (parentNode.IsUsingExistColumnName)
            {
                node.ColumnName = parentNode.ExistColumnName;
            }
        }
        public void InitTreeNodeSettings(RMSPTreeNode node, RMSharePointSetting setting, bool isGloablSetting = false)
        {
            //add this for update SharePoint Setting Table
            node.SettingScopeId = setting.ScopeId;
            SetSettingStatus(node.SettingScopeId, (int)BCSSettingFailedType.None);
            if (isGloablSetting)
            {
                node.ColumnName = setting.ColumnName;
            }
            node.Description = setting.Description;
            node.DefaultTermId = setting.DefaultTermId;
            node.DefaultTermName = setting.DefaultTermName;
            node.TermId = setting.TermId;
            node.TermSetId = setting.TermSetId;
            node.TermSetName = setting.TermSetName;
            node.TermStoreId = setting.TermStoreId;
            node.TermNameOfContainer = setting.TermNameOfContainer;
            node.TermIdOfContainer = setting.TermIdOfContainer;
            node.IsInheritParentTerm = setting.IsInheritParentTerm;
            node.isEnableClassification = setting.isEnableClassification;
            node.IsEnableHoldPhyical = setting.IsEnableHoldPhyical;
            node.ExistColumnName = setting.ExistColumnName;
            node.IsUsingExistColumnName = setting.IsUsingExistColumnName;
            node.NeedCheckDefaultValue = setting.NeedCheckDefaultValue;
            node.IsDisplyaTermPath = setting.IsDisplyaTermPath;
            node.ApplyExistType = setting.ApplyExistType;
            node.EnableRelatedRecords = setting.EnableRelatedRecords;
            if (setting.IsUsingExistColumnName)
            {
                node.ColumnName = setting.ExistColumnName;
            }

        }

        public void Dispose()
        {
            try
            {
                if (mClientContext != null)
                {
                    mClientContext.Dispose();
                }
                mSite = null;
                mWeb = null;
            }
            catch (Exception ex)
            {
                logger.Warn("dispose the site context error:{0}", ex.ToString());
            }
            try
            {
                this.SPSettingJobDetails.Clear();
            }
            catch (Exception ex1)
            {
                logger.Warn("dispose spsettings details error:{0}", ex1.ToString());
            }

            foreach (var scopeId in settingResults.Keys)
            {
                try
                {
                    //update SharePoint Setting Table for Run job time & setting status
                    int result = settingResults[scopeId];
                    bool isFailedConfigColumn = false;
                    bool isFailedConfigProperty = false;
                    //bool isFailedConfigPhysical = false;
                    isFailedConfigColumn = (result & (int)BCSSettingFailedType.AddBCSColumnFailed) == (int)BCSSettingFailedType.AddBCSColumnFailed;
                    isFailedConfigProperty = (result & (int)BCSSettingFailedType.AddBCSPropertyFailed) == (int)BCSSettingFailedType.AddBCSPropertyFailed;
                    //isFailedConfigPhysical = (result & (int)BCSSettingFailedType.AddPhysicalPropertyFailed) == (int)BCSSettingFailedType.AddPhysicalPropertyFailed;
                    if (CheckJobStatusUtility.isStopping)
                    {
                        SharePointSettingDao.SetSettingJobTimeAsync(scopeId, true, true).Wait();
                    }
                    else
                    {
                        SharePointSettingDao.SetSettingJobTimeAsync(scopeId, isFailedConfigColumn, isFailedConfigProperty).Wait();
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn("Update Job Setting Result Error:{0}", ex.ToString());
                }
            }
        }


        #region old logic before RevIM 3.2.2 & RevIm Online
        //    private void AddMetadataColumnToList(RMSPTreeNode treeNode, bool isCustomSetting = false)
        //    {
        //        List<RMSPTreeNode> nodes = RMSPTreeService.Browse(treeNode);
        //        int listCount = 0;
        //        foreach (var node in nodes)
        //        {
        //            AddNodeProperty(node, treeNode);
        //            if (isCustomSetting || !node.IsUsingExistColumnName)
        //            {
        //                if (SharePointSettingDao.LoadSharePointSetting(new Guid(node.SPObjectId), GetSiteId()) != null && !isCustomSetting)
        //                {
        //                    if ((node.Level == (int)NodeLevel.List || node.Level == (int)NodeLevel.Library) && (!node.Hidden))
        //                    {
        //                        #region for large data logic
        //                        listCount++;
        //                        if (listCount > 500)
        //                        {
        //                            try
        //                            {
        //                                logger.Info("Re- init client content to release memory ");
        //                                this.mSite = null;
        //                                this.mWeb = null;
        //                                this.mClientContext.Dispose();
        //                                listCount = 0;
        //                            }
        //                            catch (Exception e)
        //                            {
        //                                listCount = 0;
        //                                logger.Info("Release context error {0}", e.ToString());
        //                            }
        //                            InitContext(node);
        //                        }
        //                        #endregion
        //                        try
        //                        {
        //                            Web rweb = this.mSite.OpenWebById(new Guid(node.Parent.Parent.SPObjectId));
        //                            RenameTaxnomyField(treeNode.ColumnName, new Guid(node.SPObjectId), rweb, node, isCustomSetting);
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            logger.Warn("Rename the column name error {0} {1}", node.FullPath, ex.ToString());
        //                        }
        //                    }
        //                    else
        //                    {
        //                        RenameMetadataColumnToList(node, true);
        //                    }
        //                    processListsCount++;
        //                    logger.Info("Skip the current node having custom setting {0}", node.FullPath);
        //                    continue;
        //                }
        //            }

        //            if ((node.Level == (int)NodeLevel.List || node.Level == (int)NodeLevel.Library) && (!node.Hidden))
        //            {
        //                Web web = this.mSite.OpenWebById(new Guid(node.Parent.Parent.SPObjectId));
        //                mClientContext.Load(web);
        //                mClientContext.ExecuteQuery();
        //                if (isCustomSetting || !node.IsUsingExistColumnName)
        //                {
        //                    try
        //                    {
        //                        #region for performance
        //                        listCount++;
        //                        if (listCount > 500)
        //                        {
        //                            try
        //                            {
        //                                logger.Info("Re- init client content to release memory ");
        //                                this.mSite = null;
        //                                this.mWeb = null;
        //                                this.mClientContext.Dispose();
        //                                listCount = 0;
        //                            }
        //                            catch (Exception e)
        //                            {
        //                                listCount = 0;
        //                                logger.Info("Release context error {0}", e.ToString());
        //                            }
        //                            InitContext(node);
        //                        }
        //                        #endregion
        //                        var listId = new Guid(node.SPObjectId);
        //                        var webId = new Guid(node.Parent.Parent.SPObjectId);
        //                        AddOrUpdateTaxonomyColumn(node.ColumnName, listId, web, node, isCustomSetting);
        //                        SetDefaultValue(node, webId, listId, isCustomSetting);
        //                        try
        //                        {
        //                            processListsCount++;
        //                            JobService.UpdateJobProgress(jobId, CalculateProgress(processListsCount, totalListCounts));
        //                        }
        //                        catch (Exception e)
        //                        {
        //                            logger.Warn("Update job progress on library error {0}", e.ToString());
        //                        }
        //                    }
        //                    catch (Exception e)
        //                    {
        //                        logger.Error("add list column for global settings error, Path{0} :{1}", node.FullPath, e.ToString());
        //                        this.AddDetailToList(node.Name, GetFullUrl(node), I18NEntity.GetString("RM_JS_JMD_Status_AddListColumn"), JobDetailsStatus.Failed, "RM_SS_ContactAdmin");
        //                    }
        //                }
        //                if (!node.isEnableClassification)
        //                {
        //                    node.TermIdOfContainer = Guid.Empty;
        //                    node.TermNameOfContainer = "";
        //                }
        //                web = this.mSite.OpenWebById(new Guid(node.Parent.Parent.SPObjectId));
        //                AddOrUpdateListProperty(web, new Guid(node.SPObjectId), node, false);
        //            }
        //            else if ((node.Level == (int)NodeLevel.List || node.Level == (int)NodeLevel.Library))
        //            {
        //                logger.Info("Skip the system list & custom list {0}", node.FullPath);
        //                processListsCount++;
        //            }
        //            else
        //            {
        //                if (node.Level == (int)NodeLevel.Site)
        //                {
        //                    Web web = this.mSite.OpenWebById(new Guid(node.SPObjectId));
        //                    if (!node.isEnableClassification)
        //                    {
        //                        node.TermIdOfContainer = Guid.Empty;
        //                        node.TermNameOfContainer = "";
        //                    }
        //                    AddOrUpdateWebProperty(web, node);
        //                }

        //                if (jobType.Equals(JobType.SharePointInheritSetting) || jobType.Equals(JobType.SharePointCustomSetting))
        //                {
        //                    if (node.Level == (int)NodeLevel.Site && node.Parent.Level != (int)NodeLevel.SiteCollection)
        //                    {
        //                        Web web = this.mSite.OpenWebById(new Guid(node.SPObjectId));
        //                        mClientContext.Load(web, w => w.Id, w => w.Title);
        //                        mClientContext.ExecuteQuery();
        //                        RMSharePointSetting siteCustomSetting = SharePointSettingDao.LoadSharePointSetting(web.Id, GetSiteId());
        //                        if (siteCustomSetting == null)
        //                        {
        //                            AddMetadataColumnToList(node, isCustomSetting);
        //                        }
        //                        else
        //                        {
        //                            this.AddDetailToList(web.Title, GetFullUrl(node), I18NEntity.GetString("RM_JS_JMD_Status_SkipSiteCollectionColumn"), JobDetailsStatus.Skipped, "RM_JS_JMD_Comment_ConfiguredCustomSettings");
        //                        }
        //                    }
        //                    else
        //                    {
        //                        AddMetadataColumnToList(node, isCustomSetting);
        //                    }
        //                }
        //                else
        //                {
        //                    AddMetadataColumnToList(node, isCustomSetting);
        //                }
        //            }
        //        }
        //    }
        //    public void AddCustomColumn(RMSPTreeNode node, bool rootNodeIsList = true)
        //    {
        //        if (!mTaxonomy.ValidateTermIds(node.TermSetId, node.TermId != null ? node.TermId.ToString() : string.Empty,
        //node.DefaultTermId != null ? node.DefaultTermId.ToString() : string.Empty))
        //        {
        //            this.columnDisplayName = node.ColumnName;
        //            this.classification = node.TermNameOfContainer;
        //            this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_Common_Pending", JobDetailsStatus.Failed, "RM_SS_ConfigureColumnFailed");
        //            if (node.isEnableClassification)
        //            {
        //                if (node.isEnableClassification && !mTaxonomy.ValidateTermId(node.TermIdOfContainer))
        //                {
        //                    this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_Common_Pending", JobDetailsStatus.Failed, "RM_SS_ConfigureClassificationFailed");
        //                }
        //            }
        //            logger.Warn(I18NEntity.GetString("RM_SS_ConfigureColumnFailed"));
        //            return;
        //        }
        //        this.columnDisplayName = node.ColumnName;
        //        this.classification = node.TermNameOfContainer;
        //        if (node.Level == (int)NodeLevel.SiteCollection)
        //        {
        //            ConfigSiteCollectionSetting(node);
        //        }
        //        else if ((node.Level == (int)NodeLevel.List || node.Level == (int)NodeLevel.Library) && (!node.Hidden))
        //        {
        //            Web web = null;
        //            if (node.WebId != null && node.WebId != Guid.Empty)
        //            {
        //                web = this.mSite.OpenWebById(node.WebId);
        //            }
        //            else
        //            {
        //                web = this.mSite.OpenWebById(new Guid(node.Parent.Parent.SPObjectId));
        //            }

        //            logger.Info("Add list column Path: {0}", node.FullPath);
        //            AddCustomSettingColumn(node.ColumnName, new Guid(node.SPObjectId), web, node);
        //            if (!node.isEnableClassification)
        //            {
        //                node.TermIdOfContainer = Guid.Empty;
        //                node.TermNameOfContainer = "";
        //            }
        //            if (node.WebId != null && node.WebId != Guid.Empty)
        //            {
        //                web = this.mSite.OpenWebById(node.WebId);
        //            }
        //            else
        //            {
        //                web = this.mSite.OpenWebById(new Guid(node.Parent.Parent.SPObjectId));
        //            }//reload the web object.
        //            AddOrUpdateListProperty(web, new Guid(node.SPObjectId), node, rootNodeIsList);
        //            try
        //            {
        //                processListsCount++;
        //                JobService.UpdateJobProgress(jobId, CalculateProgress(processListsCount, totalListCounts));
        //            }
        //            catch (Exception e)
        //            {
        //                logger.Warn("Update job progress on library error {0}", e.ToString());
        //            }
        //        }
        //        else if ((node.Level == (int)NodeLevel.List || node.Level == (int)NodeLevel.Library))
        //        {
        //            logger.Info("Skip the system list & custom list {0}", node.FullPath);
        //            processListsCount++;
        //        }
        //        else
        //        {
        //            if (node.Level == (int)NodeLevel.Site)
        //            {
        //                Web web = this.mSite.OpenWebById(new Guid(node.SPObjectId));
        //                if (!node.isEnableClassification)
        //                {
        //                    node.TermIdOfContainer = Guid.Empty;
        //                    node.TermNameOfContainer = "";
        //                }
        //                AddOrUpdateWebProperty(web, node, true);
        //            }
        //            foreach (var subNode in RMSPTreeService.Browse(node))
        //            {
        //                #region init term property
        //                AddNodeProperty(subNode, node);
        //                if (SharePointSettingDao.LoadSharePointSetting(new Guid(subNode.SPObjectId), GetSiteId()) != null)
        //                {
        //                    //SetDefaultValue(new Guid(node.Parent.Parent.SPObjectId), new Guid(node.SPObjectId), true);
        //                    this.AddDetailToList(subNode.Name, GetFullUrl(subNode), "Skipped", JobDetailsStatus.Skipped, null);
        //                    try
        //                    {
        //                        processListsCount++;
        //                        JobService.UpdateJobProgress(jobId, CalculateProgress(processListsCount, totalListCounts));
        //                    }
        //                    catch (Exception e)
        //                    {
        //                        logger.Warn("Update job progress on library error {0}", e.ToString());
        //                    }
        //                    logger.Info("skip node has custom setting {0}", node.FullPath);
        //                    continue;
        //                }
        //                #endregion
        //                AddCustomColumn(subNode, false);
        //            }
        //        }

        //    }
        //    public void BreakCustomColumn(RMSPTreeNode node)
        //    {
        //        if (!mTaxonomy.ValidateTermIds(node.TermSetId, node.TermId != null ? node.TermId.ToString() : string.Empty,
        //node.DefaultTermId != null ? node.DefaultTermId.ToString() : string.Empty))
        //        {
        //            this.columnDisplayName = node.ColumnName;
        //            this.classification = node.TermNameOfContainer;
        //            this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_Common_Pending", JobDetailsStatus.Failed, "RM_SS_ConfigureColumnFailed");
        //            logger.Warn(I18NEntity.GetString("RM_SS_ConfigureColumnFailed"));
        //            return;
        //        }
        //        this.columnDisplayName = node.ColumnName;
        //        this.classification = node.TermNameOfContainer;
        //        if (node.Level == (int)NodeLevel.SiteCollection)
        //        {
        //            ConfigSiteCollectionSetting(node);
        //        }
        //        else if ((node.Level == (int)NodeLevel.List || node.Level == (int)NodeLevel.Library) && (!node.Hidden))
        //        {
        //            Web web = this.mSite.OpenWebById(new Guid(node.Parent.Parent.SPObjectId));
        //            logger.Info("Add list column Path: {0}", node.FullPath);
        //            if (!node.isEnableClassification)
        //            {
        //                node.TermIdOfContainer = Guid.Empty;
        //                node.TermNameOfContainer = "";
        //            }
        //            AddOrUpdateTaxonomyColumnForBreak(node.ColumnName, new Guid(node.SPObjectId), web, node);
        //            web = this.mSite.OpenWebById(new Guid(node.Parent.Parent.SPObjectId));
        //            //AddOrUpdateListProperty(web, new Guid(node.SPObjectId), node);
        //            try
        //            {
        //                processListsCount++;
        //                JobService.UpdateJobProgress(jobId, CalculateProgress(processListsCount, totalListCounts));
        //            }
        //            catch (Exception e)
        //            {
        //                logger.Warn("Update job progress on library error {0}", e.ToString());
        //            }
        //        }
        //        else if ((node.Level == (int)NodeLevel.List || node.Level == (int)NodeLevel.Library))
        //        {
        //            logger.Info("Skip the system list & custom list {0}", node.FullPath);
        //            processListsCount++;
        //        }
        //        else
        //        {
        //            if (node.Level == (int)NodeLevel.Site)
        //            {
        //                Web web = this.mSite.OpenWebById(new Guid(node.SPObjectId));
        //                if (!node.isEnableClassification)
        //                {
        //                    node.TermIdOfContainer = Guid.Empty;
        //                    node.TermNameOfContainer = "";
        //                }
        //                AddOrUpdateWebProperty(web, node, true);
        //            }
        //            foreach (var subNode in RMSPTreeService.Browse(node))
        //            {
        //                #region init term property
        //                AddNodeProperty(subNode, node);
        //                #endregion
        //                if (subNode.Level == (int)NodeLevel.Site)
        //                {
        //                    Web web = this.mSite.OpenWebById(new Guid(subNode.SPObjectId));
        //                    mClientContext.Load(web, w => w.Id);
        //                    mClientContext.ExecuteQuery();
        //                    RMSharePointSetting siteCustomSetting = SharePointSettingDao.LoadSharePointSetting(web.Id, GetSiteId());
        //                    if (siteCustomSetting == null)
        //                    {
        //                        BreakCustomColumn(subNode);
        //                    }
        //                }
        //                else
        //                {
        //                    BreakCustomColumn(subNode);
        //                }
        //            }
        //        }

        //    }
        //    public void RenameSiteColumn(RMSPTreeNode node)
        //    {
        //        this.columnDisplayName = node.ColumnName;
        //        this.classification = node.TermNameOfContainer;
        //        RenameSiteMetadataColumn(node);
        //        RenameMetadataColumnToList(node);
        //    }
        //    private void RenameMetadataColumnToList(RMSPTreeNode treeNode, bool isCustomSetting = false)
        //    {
        //        List<RMSPTreeNode> nodes = RMSPTreeService.Browse(treeNode);
        //        int listCount = 0;
        //        foreach (var node in nodes)
        //        {
        //            AddNodeProperty(node, treeNode);
        //            if ((node.Level == (int)NodeLevel.List || node.Level == (int)NodeLevel.Library) && (!node.Hidden))
        //            {
        //                #region for performance
        //                listCount++;
        //                if (listCount > 500)
        //                {
        //                    try
        //                    {
        //                        logger.Info("Re- init client content to release memory ");
        //                        this.mSite = null;
        //                        this.mWeb = null;
        //                        this.mClientContext.Dispose();
        //                        listCount = 0;
        //                    }
        //                    catch (Exception e)
        //                    {
        //                        listCount = 0;
        //                        logger.Info("Release context error {0}", e.ToString());
        //                    }
        //                    InitContext(node);
        //                }
        //                #endregion
        //                try
        //                {
        //                    Web web = this.mSite.OpenWebById(new Guid(node.Parent.Parent.SPObjectId));
        //                    mClientContext.Load(web);
        //                    mClientContext.ExecuteQuery();
        //                    if (isCustomSetting)
        //                    {
        //                        RenameTaxnomyField(node.ColumnName, new Guid(node.SPObjectId), web, node, isCustomSetting);
        //                    }
        //                    else
        //                    {
        //                        RenameSiteTaxomyField(node.ColumnName, new Guid(node.SPObjectId), web, node);
        //                    }
        //                    try
        //                    {
        //                        processListsCount++;
        //                        JobService.UpdateJobProgress(jobId, CalculateProgress(processListsCount, totalListCounts));
        //                    }
        //                    catch (Exception e)
        //                    {
        //                        logger.Warn("Update job progress on library error {0}", e.ToString());
        //                    }
        //                }
        //                catch (Exception e)
        //                {
        //                    logger.Error("add list column for global settings error Path{0} :{1}", node.FullPath, e.ToString());
        //                    this.AddDetailToList(node.Name, GetFullUrl(node), I18NEntity.GetString("RM_JS_JMD_Status_UpdateListColumn"), JobDetailsStatus.Failed, e.Message);
        //                }
        //            }
        //            else if ((node.Level == (int)NodeLevel.List || node.Level == (int)NodeLevel.Library))
        //            {
        //                logger.Info("Skip the system list & custom list {0}", node.FullPath);
        //                processListsCount++;
        //            }
        //            else
        //            {
        //                RenameMetadataColumnToList(node);
        //            }
        //        }
        //    }
        //    private void RenameSiteMetadataColumn(RMSPTreeNode node)
        //    {
        //        try
        //        {
        //            Guid fieldId = RevIMClassificationColumnID;
        //            mClientContext.Load(mWeb, w => w.Fields);
        //            Field metadataField = mWeb.Fields.GetById(fieldId);
        //            mClientContext.Load(metadataField);
        //            mClientContext.ExecuteQuery();
        //            metadataField.StaticName = node.ColumnName;
        //            metadataField.Title = node.ColumnName;
        //            if (node.Description == null)
        //            {
        //                metadataField.Description = string.Empty;
        //            }
        //            else
        //            {
        //                metadataField.Description = node.Description;
        //            }
        //            metadataField.Update();
        //            mWeb.Update();
        //            mClientContext.Load(metadataField);
        //            mClientContext.ExecuteQuery();
        //            node.isFailedConfigMetaDataColumn = false;
        //            SharePointSettingDao.UpdateGlobalSetting(node);
        //            this.AddDetailToList(node.Name, GetFullUrl(node), I18NEntity.GetString("RM_JS_JMD_Status_UpdateSiteCollectionColumn"), JobDetailsStatus.Successful, null);
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Rename site column error {0}:{1}", node.FullPath, ex.ToString());
        //            this.AddDetailToList(node.Name, GetFullUrl(node), I18NEntity.GetString("RM_JS_JMD_Status_UpdateSiteCollectionColumn"), JobDetailsStatus.Skipped, I18NEntity.GetString("RM_JS_JMD_Comment_ConfiguredCustomSettings"));
        //            node.isFailedConfigMetaDataColumn = true;
        //            SharePointSettingDao.UpdateGlobalSetting(node);
        //        }
        //    }
        //    private void AddOrUpdateTaxonomyColumn(string fieldName, Guid listId, Web web, RMSPTreeNode node, bool isCustomSetting = false)
        //    {
        //        mClientContext.Load(web);
        //        List list = web.Lists.GetById(listId);
        //        mClientContext.Load(list, l => l.Fields, l => l.RootFolder, l => l.BaseTemplate);
        //        mClientContext.Load(list);
        //        mClientContext.ExecuteQuery();
        //        if (list.BaseTemplate == 600)
        //        {
        //            logger.Info("Skip the design list & system list{0}", list.RootFolder.Name);
        //            return;
        //        }
        //        if (CheckIsDesignList(list.RootFolder.Name + list.BaseTemplate.ToString()))
        //        {
        //            logger.Info("Skip the design list {0}", list.RootFolder.Name);
        //            return;
        //        }
        //        try
        //        {
        //            #region Get list column
        //            //Guid fieldId = SharePointSettingDao.GetSiteColumnId(m_ClientContext.Site.Id);
        //            //if (fieldId == Guid.Empty)
        //            //{
        //            Guid fieldId = GetSiteCollectionRevIMColumnID();
        //            //}
        //            //Guid fieldListId = SharePointSettingDao.GetListColumnId(m_ClientContext.Site.Id, web.Id, list.Id);
        //            RMSharePointSetting libSetting = SharePointSettingDao.LoadSharePointSetting(list.Id, GetSiteId());
        //            if (isCustomSetting && libSetting != null && string.Equals(node.ColumnName, libSetting.ColumnName)) //check if column name changed for change gourp or other change.
        //            {
        //                this.AddDetailToList(node.Name, GetFullUrl(node), I18NEntity.GetString("RM_JS_JMD_Status_SkipListColumn"), JobDetailsStatus.Skipped, "Already set custom column.");
        //                return;
        //            }
        //            else if (isCustomSetting && libSetting != null)
        //            {
        //                RenameTaxnomyField(node.ColumnName, list.Id, web, node, isCustomSetting);
        //                return;
        //            }

        //            if (fieldId != Guid.Empty)
        //            {
        //                UpdateListTaxonomyField(web, list, fieldId, node);
        //            }
        //            else
        //            {
        //                //fieldId = Guid.Empty 说明Site Collection级别的Column没有创建出来，所以list级别的column也不会被创建出来
        //                this.AddDetailToList(node.Name, GetFullUrl(node), I18NEntity.GetString("RM_JS_JMD_Status_AddListColumn"), JobDetailsStatus.Failed, "RM_SS_SCcolumnFail");
        //                return;
        //            }
        //            #endregion
        //        }
        //        catch
        //        {
        //            logger.Info("Need Add Site column,{0}", node.FullPath);
        //            try
        //            {
        //                //Guid fieldId = SharePointSettingDao.GetSiteColumnId(m_ClientContext.Site.Id);
        //                //if (fieldId == Guid.Empty)
        //                //{
        //                Guid fieldId = GetSiteCollectionRevIMColumnID();
        //                //}
        //                Field field = this.mWeb.Fields.GetById(fieldId);
        //                //Field field = this.m_Web.Fields.GetByInternalNameOrTitle(fieldName);
        //                mClientContext.Load(field);
        //                this.mClientContext.ExecuteQuery();
        //                AddSiteColumnToList(list, field, node);
        //                //添加完column要进行更新，避免365 library访问不了的问题(REC-1642)
        //                logger.Info("Update list taxonomy filed after column created.");
        //                UpdateListTaxonomyField(web, list, fieldId, node, false);
        //            }
        //            catch (ServerUnauthorizedAccessException se)
        //            {
        //                logger.Warn("Add site column on list error Path :{0}, {1}", node.FullPath, se.ToString());
        //                this.AddDetailToList(node.Name, GetFullUrl(node), I18NEntity.GetString("RM_JS_JMD_Status_AddListColumn"), JobDetailsStatus.Failed, "RM_SS_DocLibraryAccessDeny");
        //            }
        //            catch (Exception e)
        //            {
        //                if (!e.Message.Equals("Have same name column in the list", StringComparison.OrdinalIgnoreCase))
        //                {
        //                    logger.Warn("Add site column on list error Path :{0}, error detail {1}", node.FullPath, e.ToString());
        //                    this.AddDetailToList(node.Name, GetFullUrl(node), I18NEntity.GetString("RM_JS_JMD_Status_AddListColumn"), JobDetailsStatus.Failed, "RM_JM_Details_Failed_UnexpectedError");
        //                }
        //            }
        //        }
        //    }
        //    private void AddOrUpdateTaxonomyColumnForBreak(string fieldName, Guid listId, Web web, RMSPTreeNode node)
        //    {
        //        bool isUpdate = true;
        //        mClientContext.Load(web);
        //        List list = web.Lists.GetById(listId);
        //        mClientContext.Load(list, l => l.Fields);
        //        mClientContext.Load(list);
        //        mClientContext.ExecuteQuery();
        //        try
        //        {
        //            #region Get list column
        //            //Guid siteFieldId = SharePointSettingDao.GetSiteColumnId(m_ClientContext.Site.Id);
        //            //if (siteFieldId == Guid.Empty)
        //            //{
        //            Guid siteFieldId = GetSiteCollectionRevIMColumnID();
        //            // }
        //            Guid fieldId = CheckListColumnID(list);//SharePointSettingDao.GetListColumnId(m_ClientContext.Site.Id, web.Id, list.Id);
        //            if (fieldId != Guid.Empty)
        //            {
        //                UpdateListTaxonomyField(web, list, fieldId, node);
        //                AddOrUpdateListProperty(list, node);
        //            }
        //            else
        //            {
        //                isUpdate = false;
        //                Field field = this.mWeb.Fields.GetById(siteFieldId);
        //                mClientContext.Load(field);
        //                this.mClientContext.ExecuteQuery();
        //                AddSiteColumnToList(list, field, node);
        //                AddOrUpdateListProperty(list, node);
        //            }
        //            #endregion
        //            logger.Info("Delete custom setting");
        //            //SharePointSettingDao.DeleteCustomSetting(m_ClientContext.Site.Id, web.Id, list.Id);
        //        }
        //        catch (Exception e)
        //        {
        //            logger.Info("Need Add Site column,{0}, error message {1}", node.FullPath, e.ToString());
        //            if (isUpdate)
        //            {
        //                this.AddDetailToList(node.Name, GetFullUrl(node), I18NEntity.GetString("RM_JS_JMD_Status_UpdateListColumn"), JobDetailsStatus.Failed, e.Message);
        //            }
        //            else
        //            {
        //                this.AddDetailToList(node.Name, GetFullUrl(node), I18NEntity.GetString("RM_JS_JMD_Status_AddListColumn"), JobDetailsStatus.Failed, e.Message);
        //            }
        //        }
        //    }
        //    private void RenameSiteTaxomyField(string fieldName, Guid listId, Web web, RMSPTreeNode node)
        //    {
        //        bool needUpdate = false;
        //        mClientContext.Load(web);
        //        List list = web.Lists.GetById(listId);
        //        mClientContext.Load(list, l => l.Fields, l => l.RootFolder, l => l.BaseTemplate);
        //        mClientContext.Load(list);
        //        mClientContext.ExecuteQuery();
        //        if (list.BaseTemplate == 600)
        //        {
        //            logger.Info("Skip the design list & system list{0}", list.RootFolder.Name);
        //            return;
        //        }
        //        if (CheckIsDesignList(list.RootFolder.Name + list.BaseTemplate.ToString()))
        //        {
        //            logger.Info("Skip the design list {0}", list.RootFolder.Name);
        //            return;
        //        }
        //        //Guid fieldId = SharePointSettingDao.GetSiteColumnId(m_ClientContext.Site.Id);
        //        //if (fieldId == Guid.Empty)
        //        //{
        //        Guid fieldId = GetSiteCollectionRevIMColumnID();
        //        //}
        //        //Guid GlobalFieldId = SharePointSettingDao.GetSiteColumnId(m_ClientContext.Site.Id);
        //        if (fieldId != Guid.Empty)
        //        {
        //            Field f = null;
        //            try
        //            {
        //                f = list.Fields.GetById(fieldId);
        //                this.mClientContext.Load(f);
        //                f.Title = fieldName;
        //                f.StaticName = fieldName;
        //                f.Update();
        //                this.mClientContext.ExecuteQuery();
        //            }
        //            catch (Exception e)
        //            {
        //                //list在之前创建column的时候就没有创建出来，导致修改name的时候get不到，所以会报异常,例如新建的subsite
        //                logger.Warn("Current list column is not exist {0}, detail message {1}", node.Name, e.ToString());
        //                //this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_Common_Pending", JobDetailsStatus.Failed, "RM_SS_SSColumnInList");                    
        //                this.AddDetailToList(node.Name, GetFullUrl(node), "RM_JS_Common_Pending", JobDetailsStatus.Skipped, "RM_SS_SSColumnInList");
        //                //node.isFailedConfigMetaDataColumn = true;
        //                //SharePointSettingDao.UpdateColumnInfo(m_ClientContext.Site.Id, web.Id, listId, fieldId, node);
        //                return;
        //            }
        //            try
        //            {
        //                string nodeDescription = string.Empty;
        //                if (node.Description != null)
        //                {
        //                    nodeDescription = node.Description;
        //                }
        //                if (!f.Title.Equals(fieldName))
        //                {
        //                    needUpdate = true;
        //                }
        //                else if (!nodeDescription.Equals(f.Description))
        //                {
        //                    needUpdate = true;
        //                }
        //                f.Title = fieldName;
        //                f.StaticName = fieldName;
        //                if (node.Description == null)
        //                {
        //                    f.Description = string.Empty;
        //                }
        //                else
        //                {
        //                    f.Description = node.Description;
        //                }
        //                f.Update();
        //                web.Update();
        //                mClientContext.Load(f);
        //                this.mClientContext.ExecuteQuery();
        //                if (needUpdate)
        //                {
        //                    this.AddDetailToList(node.Name, GetFullUrl(node), I18NEntity.GetString("RM_JS_JMD_Status_UpdateListColumn"), JobDetailsStatus.Successful, null);
        //                }
        //                else
        //                {
        //                    RMSharePointSetting listCustomSetting = SharePointSettingDao.LoadSharePointSetting(listId, GetSiteId());
        //                    this.AddDetailToList(node.Name, GetFullUrl(node), I18NEntity.GetString("RM_JS_JMD_Status_SkipListColumn"), JobDetailsStatus.Skipped, listCustomSetting != null ? I18NEntity.GetString("RM_JS_JMD_Comment_ConfiguredCustomSettings") : null);
        //                }
        //                node.isFailedConfigMetaDataColumn = false;
        //                // SharePointSettingDao.UpdateColumnInfo(m_ClientContext.Site.Id, web.Id, listId, fieldId, node);
        //            }
        //            catch (Exception)
        //            {
        //                node.isFailedConfigMetaDataColumn = true;
        //                //SharePointSettingDao.UpdateColumnInfo(m_ClientContext.Site.Id, web.Id, listId, fieldId, node);
        //                throw;
        //            }

        //        }
        //    }

        //    private void RenameTaxnomyField(string fieldName, Guid listId, Web web, RMSPTreeNode node, bool isCustomSetting = false)
        //    {
        //        List list = web.Lists.GetById(listId);
        //        Guid fieldId = CheckListColumnID(list);//SharePointSettingDao.GetListColumnId(m_ClientContext.Site.Id, new Guid(node.Parent.Parent.SPObjectId), listId);
        //        //Guid GlobalFieldId = SharePointSettingDao.GetSiteColumnId(m_ClientContext.Site.Id);
        //        if (fieldId != Guid.Empty)
        //        {
        //            try
        //            {
        //                bool needUpdate = false;
        //                mClientContext.Load(web);

        //                mClientContext.Load(list, l => l.Fields);
        //                Field f = list.Fields.GetById(fieldId);
        //                this.mClientContext.Load(f);
        //                this.mClientContext.ExecuteQuery();
        //                string nodeDescription = string.Empty;
        //                if (node.Description != null)
        //                {
        //                    nodeDescription = node.Description;
        //                }
        //                if (!f.Title.Equals(fieldName))
        //                {
        //                    needUpdate = true;
        //                }
        //                else if (!nodeDescription.Equals(f.Description))
        //                {
        //                    needUpdate = true;
        //                }
        //                f.Title = fieldName;
        //                f.StaticName = fieldName;
        //                if (node.Description == null)
        //                {
        //                    f.Description = string.Empty;
        //                }
        //                else
        //                {
        //                    f.Description = node.Description;
        //                }
        //                f.Update();
        //                web.Update();
        //                mClientContext.Load(f);
        //                this.mClientContext.ExecuteQuery();
        //                if (needUpdate)
        //                {
        //                    this.AddDetailToList(node.Name, GetFullUrl(node), I18NEntity.GetString("RM_JS_JMD_Status_UpdateListColumn"), JobDetailsStatus.Successful, null);
        //                }
        //                else
        //                {
        //                    string sikpComment = !isCustomSetting && (SharePointSettingDao.LoadSharePointSetting(listId, GetSiteId()) != null) ? I18NEntity.GetString("RM_JS_JMD_Comment_ConfiguredCustomSettings") : null;
        //                    this.AddDetailToList(node.Name, GetFullUrl(node), I18NEntity.GetString("RM_JS_JMD_Status_SkipListColumn"), JobDetailsStatus.Skipped, sikpComment);
        //                }
        //                node.isFailedConfigMetaDataColumn = false;
        //                //SharePointSettingDao.UpdateColumnInfo(m_ClientContext.Site.Id, web.Id, listId, fieldId, node);
        //            }
        //            catch (Exception)
        //            {
        //                node.isFailedConfigMetaDataColumn = true;
        //                //SharePointSettingDao.UpdateColumnInfo(m_ClientContext.Site.Id, web.Id, listId, fieldId, node);
        //                throw;
        //            }
        //        }
        //    }
        //    private void AddSiteColumnToList(List list, Field field, RMSPTreeNode node)
        //    {
        //        mClientContext.Load(list, l => l.Fields, l => l.RootFolder, l => l.BaseTemplate, l => l.Title);
        //        mClientContext.ExecuteQuery();
        //        bool isFieldExist = false;
        //        try
        //        {
        //            Field dispalyNameField = list.Fields.GetByTitle(node.ColumnName);
        //            mClientContext.Load(dispalyNameField);
        //            mClientContext.ExecuteQuery();
        //            string getTitle = dispalyNameField.Title;
        //            isFieldExist = true;
        //        }
        //        catch
        //        {

        //        }
        //        if (isFieldExist)
        //        {
        //            logger.Warn("Have same name column in the list");
        //            throw new Exception("Have same name column in the list");
        //        }
        //        if (list.BaseTemplate == 600)
        //        {
        //            logger.Info("Skip the design list & system list{0}", list.RootFolder.Name);
        //            return;
        //        }
        //        if (CheckIsDesignList(list.RootFolder.Name + list.BaseTemplate.ToString()))
        //        {
        //            logger.Info("Skip the design list {0}", list.RootFolder.Name);
        //            return;
        //        }
        //        Field newListField = list.Fields.AddFieldAsXml(field.SchemaXml, true, AddFieldOptions.AddFieldInternalNameHint | AddFieldOptions.AddFieldToDefaultView | AddFieldOptions.AddToAllContentTypes);
        //        if (node.Description == null)
        //        {
        //            newListField.Description = string.Empty;
        //        }
        //        else
        //        {
        //            newListField.Description = node.Description;
        //        }
        //        newListField.Update();
        //        mClientContext.Load(newListField);
        //        this.mClientContext.ExecuteQuery();
        //        Field listField = list.Fields.GetById(field.Id);
        //        var webTaxField = this.mClientContext.CastTo<TaxonomyField>(field);
        //        var listTaxField = this.mClientContext.CastTo<TaxonomyField>(listField);
        //        this.mClientContext.Load(listTaxField, f => f.DefaultValue);
        //        this.mClientContext.ExecuteQuery();
        //        if (listTaxField.DefaultValue.Equals("-1;#" + node.DefaultTermName + "|" + node.DefaultTermId))
        //        {
        //            UpdateTaxnomyDefaultValue(this.mWeb, list, node, webTaxField);
        //            UpdateTaxnomyDefaultValue(this.mWeb, list, node, listTaxField);
        //        }
        //        this.AddDetailToList(list.Title, GetFullUrl(node), I18NEntity.GetString("RM_JS_JMD_Status_AddListColumn"), JobDetailsStatus.Successful, null);
        //    }
        //    private void AddCustomSettingColumn(string fieldName, Guid listId, Web web, RMSPTreeNode node)
        //    {
        //        mClientContext.Load(web);
        //        List list = web.Lists.GetById(listId);
        //        Guid fieldId = CheckListColumnID(list);//SharePointSettingDao.GetListColumnId(m_ClientContext.Site.Id, new Guid(node.Parent.Parent.SPObjectId), listId);
        //        Guid GlobalFieldId = GetSiteCollectionRevIMColumnID();//SharePointSettingDao.GetSiteColumnId(m_ClientContext.Site.Id);
        //        mClientContext.Load(list, l => l.Fields, l => l.RootFolder, l => l.BaseTemplate);
        //        mClientContext.ExecuteQuery();
        //        if (list.BaseTemplate == 600)
        //        {
        //            logger.Info("Skip the design list & system list{0}", list.RootFolder.Name);
        //            return;
        //        }
        //        if (CheckIsDesignList(list.RootFolder.Name + list.BaseTemplate.ToString()))
        //        {
        //            logger.Info("Skip the design list {0}", list.RootFolder.Name);
        //            return;
        //        }
        //        //Global settings not config,need config global first
        //        if (GlobalFieldId == Guid.Empty)
        //        {
        //            RMSPTreeNode siteNode = GetSiteCollectionNode(node);
        //            RMSharePointSetting setting = SharePointSettingDao.LoadSharePointSetting(new Guid(siteNode.Parent.SPObjectId), Guid.Empty);
        //            if (setting != null)
        //            {
        //                siteNode.ColumnName = setting.ColumnName;
        //                siteNode.Description = setting.Description;
        //                siteNode.DefaultTermId = setting.DefaultTermId;
        //                siteNode.DefaultTermName = setting.DefaultTermName;
        //                siteNode.TermId = setting.TermId;
        //                siteNode.TermName = setting.TermName;
        //                siteNode.TermSetId = setting.TermSetId;
        //                siteNode.TermSetName = setting.TermSetName;
        //                ConfigSiteCollectionSetting(siteNode);
        //                GlobalFieldId = RevIMClassificationColumnID;
        //                web = this.mSite.OpenWebById(new Guid(node.Parent.Parent.SPObjectId));
        //                mClientContext.Load(web);
        //                list = web.Lists.GetById(listId);
        //                mClientContext.Load(list, l => l.Fields);
        //            }
        //        }

        //        try
        //        {
        //            var webId = new Guid(node.Parent.Parent.SPObjectId);
        //            if (GlobalFieldId != Guid.Empty && fieldId == Guid.Empty)
        //            {
        //                TaxonomyField taxField = UpdateListTaxonomyField(web, list, GlobalFieldId, node);
        //                //SharePointSettingDao.AddOrUpdateColumnInfo(m_ClientContext.Site.Id, webId, listId, taxField.Id, node);
        //                SetDefaultValue(node, webId, listId, true);
        //                //#endregion
        //            }
        //            else if (fieldId != Guid.Empty)
        //            {
        //                #region update field
        //                string wssId = string.Empty;
        //                mClientContext.Load(web);
        //                list = web.Lists.GetById(listId);
        //                mClientContext.Load(list, l => l.Fields);
        //                TaxonomyField taxField = UpdateListTaxonomyField(web, list, fieldId, node);
        //                SetDefaultValue(node, webId, listId, true);
        //                node.isFailedConfigMetaDataColumn = false;
        //                //SharePointSettingDao.AddOrUpdateColumnInfo(m_ClientContext.Site.Id, new Guid(node.Parent.Parent.SPObjectId), listId, taxField.Id, node);
        //                #endregion
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            logger.Info("Create new Custom column {0}", node.FullPath);
        //            #region
        //            try
        //            {
        //                node.isFailedConfigMetaDataColumn = false;
        //                TaxonomyField taxField = CreateNewListTaxonomyField(web, listId, GlobalFieldId, node);
        //                //SharePointSettingDao.AddOrUpdateColumnInfo(m_ClientContext.Site.Id, new Guid(node.Parent.Parent.SPObjectId), listId, taxField.Id, node);
        //                SetDefaultValue(node, new Guid(node.Parent.Parent.SPObjectId), listId, true);
        //                this.AddDetailToList(node.Name, GetFullUrl(node), I18NEntity.GetString("RM_JS_JMD_Status_AddListColumn"), JobDetailsStatus.Successful, null);
        //            }
        //            catch (Exception ex)
        //            {
        //                logger.Warn("Create new Custom column has some error {0}", ex.ToString());
        //                this.AddDetailToList(node.Name, GetFullUrl(node), I18NEntity.GetString("RM_JS_JMD_Status_AddListColumn"), JobDetailsStatus.Failed, "RM_SS_ContactAdmin");
        //                Guid taxFieldId = Guid.Empty;
        //                node.isFailedConfigMetaDataColumn = true;
        //                //SharePointSettingDao.AddOrUpdateColumnInfo(m_ClientContext.Site.Id, new Guid(node.Parent.Parent.SPObjectId), listId, taxFieldId, node);
        //            }

        //            #endregion
        //        }
        //    }
        #endregion

        #region Get/Remove Field Default Value in Folder
        private XmlNode SelectSingleFieldDefaultNode(XmlDocument defaultsXml, string folderPath, string fieldName)
        {
            return defaultsXml.DocumentElement.SelectSingleNode(string.Format(System.Globalization.CultureInfo.InvariantCulture, "/MetadataDefaults/a[@href='{0}']/DefaultValue[@FieldName='{1}']", new object[]
            {
        Microsoft.SharePoint.Client.Utilities.HttpUtility.UrlPathEncode(folderPath, false),
        fieldName
            }));
        }
        private XmlNode SelectSingleFolderNode(XmlDocument defaultsXml, string folderPath)
        {
            return defaultsXml.DocumentElement.SelectSingleNode(string.Format(System.Globalization.CultureInfo.InvariantCulture, "/MetadataDefaults/a[@href='{0}']", new object[]
            {
        Microsoft.SharePoint.Client.Utilities.HttpUtility.UrlPathEncode(folderPath, false)
            }));
        }
        private string GetDefaultFolderValues(List list, Folder sourcefolder)
        {

            Folder formsFolder = list.ParentWeb.GetFolderByServerRelativeUrl(list.RootFolder.ServerRelativeUrl + "/forms");

            mClientContext.Load(formsFolder, f => f.Files);
            mClientContext.ExecuteQuery();

            var clientLocationBasedDefaultsFile =
                formsFolder.Files.FirstOrDefault(
                    f => f.Name.ToLowerInvariant() == "client_LocationBasedDefaults.html".ToLowerInvariant());

            if (clientLocationBasedDefaultsFile != null)
            {
                return ReadFileContent(clientLocationBasedDefaultsFile);
            }

            return null;
        }
        private XmlDocument RemoveFieldDefault(XmlDocument defaultsXml, string folderPath, string fieldName)
        {
            XmlNode xmlNode = this.SelectSingleFolderNode(defaultsXml, folderPath);
            if (xmlNode == null)
            {
                return defaultsXml;
            }
            XmlNode xmlNode2 = xmlNode.SelectSingleNode(string.Format(System.Globalization.CultureInfo.InvariantCulture, "./DefaultValue[@FieldName='{0}']", new object[]
            {
        fieldName
            }));
            if (xmlNode2 == null)
            {
                return defaultsXml;
            }
            xmlNode.RemoveChild(xmlNode2);
            if (!xmlNode.HasChildNodes)
            {
                defaultsXml.DocumentElement.RemoveChild(xmlNode);
            }
            return defaultsXml;
        }
        private string ReadFileContent(File file)
        {
            ClientResult<System.IO.Stream> stream = file.OpenBinaryStream();
            file.Context.ExecuteQuery();

            using (System.IO.StreamReader reader = new System.IO.StreamReader(stream.Value, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// For after remove update
        /// </summary>
        /// <param name="list"></param>
        /// <param name="defaultValues"></param>
        private void UpdateDefaultFolderValues(List list, string defaultValues)
        {
            Folder formsFolder =
                mClientContext.Web.GetFolderByServerRelativeUrl(
                    list.RootFolder.ServerRelativeUrl + "/forms");

            var fci = new FileCreationInformation();
            fci.Content = Encoding.UTF8.GetBytes(defaultValues);
            fci.Url = "client_LocationBasedDefaults.html";
            fci.Overwrite = true;
            var metaDataFile = formsFolder.Files.Add(fci);

            mClientContext.Load(metaDataFile);
            mClientContext.ExecuteQuery();
        }

        #endregion
    }
}
