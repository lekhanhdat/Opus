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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMRelatedRecord;
using AvePoint.RA.Contract.RMRelatedRecord.BrowserObjInfo;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.SharePoint.RelatedRecords.RelatedRecordsBrowser;
using AvePoint.RA.Web.Extentions.Authorize;
using AvePoint.RA.Web.Models.RelatedRecords;
using AvePoint.RA.Web.Models.Resource;
using AvePoint.Records.Core.Utilities.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.RelatedRecords
{
    public class RelatedRecordsApiController : ControllerBase
    {
        public IExplorerService ExplorerService
        {
            get
            {
                return (IExplorerService)PlatformWindsorManager.GetService(typeof(IExplorerService));
            }
        }
        public ITemplateManagementService TemplateManagementService
        {
            get
            {
                return (ITemplateManagementService)PlatformWindsorManager.GetService(typeof(ITemplateManagementService));
            }
        }
        public IUserService UserService
        {
            get
            {
                return (IUserService)PlatformWindsorManager.GetService(typeof(IUserService));
            }
        }
        public IPermissionManagementService PermissionManagementService
        {
            get
            {
                return (IPermissionManagementService)PlatformWindsorManager.GetService(typeof(IPermissionManagementService));
            }
        }

        public IGeneralSettingService GeneralSettingService
        {
            get
            {
                return (IGeneralSettingService)PlatformWindsorManager.GetService(typeof(IGeneralSettingService));
            }
        }

        public IRMSecurityTrimmingHelper SecurityTrimmingHelper
        {
            get
            {
                return (IRMSecurityTrimmingHelper)PlatformWindsorManager.GetService(typeof(IRMSecurityTrimmingHelper));
            }
        }
        private RALogger logger = RALogger.GetInstance(typeof(RelatedRecordsApiController));

        [HttpPost]
        public string Browser([FromBody] SPTreePage tree)//[FromBody]TreePage tree)
        {
            //current url
            #region init context token
            //var requestContext = Request.Properties["MS_HttpContext"] as HttpContextWrapper;
            Dictionary<string, string> parms = GetRequestTokenParm();
            string accessToken = ReletedRecordsAppTokenHelper.GetAccessToken(parms);
            string url = string.Empty;
            if (tree != null)
            {
                url = tree.WebUrl;
            }
            else
            {
                url = parms["?hu"];
            }
            SPTreePage page = new SPTreePage();
            #endregion
            using (BrowserWorker worker = new BrowserWorker(url, accessToken))
            {
                try
                {
                    List<BrowserSPObjInfo> infos = new List<BrowserSPObjInfo>();
                    AvePoint.GCommon.Utility.ArgumentCheck.NotNull(tree, nameof(tree));
                    logger.Info("browser sp object {0}:{1} webid {2} listid {3} folderid {4} pageinfo {5}", url, tree.NodeLevel, tree.WebId, tree.ListId, tree.FolderId, tree.pageInfo);
                    switch (tree.NodeLevel)
                    {
                        #region browser sp object
                        case (int)NodeLevel.Site:
                            {
                                var info = worker.BrowserCurrentWeb();
                                var sites = new BrowserSPObjInfo();
                                sites.NodeLevel = (int)NodeLevel.Sites;
                                sites.id = info.id;
                                sites.WebId = info.id;
                                sites.name = "sites";
                                sites.WebUrl = info.WebUrl;
                                var lists = new BrowserSPObjInfo();
                                lists.NodeLevel = (int)NodeLevel.Lists;
                                lists.id = info.id;
                                lists.WebId = info.id;
                                lists.name = "lists";
                                lists.WebUrl = info.WebUrl;
                                infos.Add(sites);
                                infos.Add(lists);

                                page.ChildrenCount = 2;
                                page.infos = infos;
                                break;
                            }
                        case (int)NodeLevel.Sites:
                            int sitesCount = 0;
                            if (tree.PageIndex == null)
                            {
                                tree.PageIndex = 0;
                            }
                            else
                            {
                                tree.PageIndex = tree.PageIndex - 1;
                            }

                            List<RecordsWebBrowserInfo> siteInfos = worker.BrowserSites(tree.WebId, (int)tree.PageIndex * (int)tree.PageSize, (uint)tree.PageSize, ref sitesCount);
                            infos.AddRange(siteInfos);
                            page.ChildrenCount = sitesCount;
                            page.infos = infos;
                            break;
                        case (int)NodeLevel.Lists:
                            int listsCount = 0;
                            if (tree.PageIndex == null)
                            {
                                tree.PageIndex = 0;
                            }
                            else
                            {
                                tree.PageIndex = tree.PageIndex - 1;
                            }
                            List<RecordsListBrowserInfo> listInfos = worker.BrowserLists(tree.WebId, (int)tree.PageIndex * (int)tree.PageSize, (uint)tree.PageSize, ref listsCount);
                            infos.AddRange(listInfos);
                            page.ChildrenCount = listsCount;
                            page.infos = infos;
                            break;
                        case (int)NodeLevel.List:
                            {
                                List<RecordsFolderBrowserInfo> folderInfos = worker.BrowserFolders(tree.WebId, tree.ListId, tree.FolderId, tree.ServerRelativeUrl, false);
                                infos.AddRange(folderInfos);
                                page.ChildrenCount = folderInfos.Count;
                                page.infos = infos;
                            }
                            break;
                        case (int)NodeLevel.Folder:
                            {
                                List<RecordsFolderBrowserInfo> folderInfos = worker.BrowserFolders(tree.WebId, tree.ListId, tree.FolderId, tree.ServerRelativeUrl, false);
                                infos.AddRange(folderInfos);
                                page.ChildrenCount = folderInfos.Count;
                                page.infos = infos;
                                break;
                            }
                        default:
                            {
                                var info = worker.BrowserCurrentWeb();
                                var sites = new BrowserSPObjInfo();
                                sites.NodeLevel = (int)NodeLevel.Sites;
                                sites.id = info.id;
                                sites.WebId = info.id;
                                sites.name = "sites";
                                sites.WebUrl = info.WebUrl;
                                var lists = new BrowserSPObjInfo();
                                lists.NodeLevel = (int)NodeLevel.Lists;
                                lists.id = info.id;
                                lists.WebId = info.id;
                                lists.name = "lists";
                                lists.WebUrl = info.WebUrl;
                                infos.Add(sites);
                                infos.Add(lists);

                                page.ChildrenCount = 2;
                                page.infos = infos;
                                break;
                            }
                            #endregion
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("Browser objects failed {0}", e.ToString());
                    throw;
                }
            }
            return JsonConvert.SerializeObject(page);
        }
        [HttpPost]
        public string GetItems([FromBody] SPTreePage tree)
        {
            //var requestContext = Request.Properties["MS_HttpContext"] as HttpContextWrapper;
            Dictionary<string, string> parms = GetRequestTokenParm();
            string accessToken = ReletedRecordsAppTokenHelper.GetAccessToken(parms);
            string url = string.Empty;
            if (tree != null)
            {
                url = tree.WebUrl;
            }
            else
            {
                url = parms["?hu"];
            }
            SPTreePage page = new SPTreePage();
            using (BrowserWorker worker = new BrowserWorker(url, accessToken))
            {
                logger.Info("browser items {0}:{1} webid {2} listid {3} folderid {4} pageinfo {5}", url, tree.NodeLevel, tree.WebId, tree.ListId, tree.FolderId, tree.pageInfo);
                try
                {
                    List<BrowserSPObjInfo> infos = new List<BrowserSPObjInfo>();
                    int childrenCount = 0;
                    string pageInfo = string.Empty;
                    if (tree.pageInfo != null)
                    {
                        var pageObj = tree.pageInfo.Find(t => t.pageIndex.Equals(tree.PageIndex));
                        if (pageObj != null)
                        {
                            pageInfo = pageObj.pageInfo;
                        }
                        else
                        {
                            try
                            {
                                //get page info
                                if (tree.pageInfo.Count > 0 && tree.PageIndex != 1)
                                {
                                    int currentPage = tree.pageInfo[tree.pageInfo.Count - 1].pageIndex;
                                    pageInfo = tree.pageInfo[tree.pageInfo.Count - 1].pageInfo;
                                    int pageCount = (int)tree.PageIndex - currentPage;
                                    for (int i = 1; i <= pageCount; i++)
                                    {
                                        worker.GetPageInfo(tree.WebId, tree.ListId, tree.FolderId, tree.ServerRelativeUrl, ref pageInfo, (uint)tree.PageSize, ref childrenCount);
                                        PageInfo pInfo = new PageInfo() { pageIndex = currentPage + i, pageInfo = pageInfo };
                                        tree.pageInfo.Add(pInfo);
                                    }
                                }
                            }
                            catch (Exception pe)
                            {
                                logger.Warn("Init pageinfo error {0}", pe.ToString());
                            }
                        }
                    }
                    List<RecordsItemBrowserInfo> itemInfos = worker.BrowserItems(tree.WebId, tree.ListId, tree.FolderId, tree.ServerRelativeUrl, ref pageInfo, (uint)tree.PageSize, ref childrenCount);
                    if (tree.pageInfo == null)
                    {
                        PageInfo pInfo = new PageInfo() { pageIndex = 2, pageInfo = pageInfo };
                        tree.pageInfo = new List<PageInfo>();
                        tree.pageInfo.Add(pInfo);
                    }
                    else
                    {
                        PageInfo pInfo = new PageInfo() { pageIndex = (int)tree.PageIndex + 1, pageInfo = pageInfo };
                        tree.pageInfo.Add(pInfo);
                    }
                    infos.AddRange(itemInfos);
                    page.ChildrenCount = childrenCount;
                    page.infos = infos;
                    page.pageInfo = tree.pageInfo;
                    page.WebId = tree.WebId;
                    page.ListId = tree.ListId;
                    page.FolderId = tree.FolderId;
                    page.WebUrl = tree.WebUrl;
                    page.ServerRelativeUrl = tree.ServerRelativeUrl;
                }
                catch (Exception e)
                {
                    logger.Warn("Browser items failed {0}", e.ToString());
                    throw;
                }
            }
            return JsonConvert.SerializeObject(page);
        }

        [HttpPost]
        public string SubmitRelatedItems([FromBody] List<RecordsItemBrowserInfo> browserObjs)
        {
            //var requestContext = Request.Properties["MS_HttpContext"] as HttpContextWrapper;
            Dictionary<string, string> parms = GetRequestTokenParm();
            string hostUrl = parms[SPAppConstants.ParamHostUrl];
            string accessToken = ReletedRecordsAppTokenHelper.GetAccessToken(parms);
            Guid listId = new Guid(parms[SPAppConstants.ParamListId]);
            int id = Convert.ToInt32(parms[SPAppConstants.ParamItemId]);
            try
            {
                using (var utility = new RelatedRecordsUtility(hostUrl, accessToken, listId, id))
                {
                    List<RMRelatedItemInfo> infos = new List<RMRelatedItemInfo>();
                    foreach (var browserObj in browserObjs)
                    {

                        RecordsItemBrowserInfo iteminfo = browserObj as RecordsItemBrowserInfo;
                        SORelativeDataArchiverNodeLevel itemLevel = iteminfo.ListBaseType == 0 ? SORelativeDataArchiverNodeLevel.Item : SORelativeDataArchiverNodeLevel.Document;
                        if (itemLevel == SORelativeDataArchiverNodeLevel.Item)
                        {
                            iteminfo.url = WebUtil.GetListItemRealPath(iteminfo.url);
                        }
                        RMRelatedItemInfo info = new RMRelatedItemInfo()
                        {
                            DocLibRowId = iteminfo.DocLibRowId,
                            WebUrl = iteminfo.WebUrl,
                            ListId = iteminfo.ListId,
                            url = iteminfo.url,
                            name = iteminfo.name,
                            NeedDelete = iteminfo.NeedDelete,
                            id = iteminfo.id,
                            FolderId = iteminfo.FolderId,
                            level = itemLevel,
                            ParentFolderIsRootFolder = iteminfo.ParentFolderIsRootFolder,
                            SiteId = iteminfo.SiteId,
                            SiteUrl = iteminfo.SiteUrl,
                            WebId = iteminfo.WebId,
                            WebServerRelativeUrl = iteminfo.WebServerRelativeUrl,
                            ListUrl = iteminfo.ListUrl,
                            FolderUrl = iteminfo.FolderUrl,
                            ItemUrl = iteminfo.ItemUrl,
                            SourceFlag = (int)SourceFlag.SharePoint,
                            NodeType = iteminfo.NodeLevel,
                        };
                        infos.Add(info);
                    }
                    utility.UpdateRelatedPropertiesForApp(infos, []);
                    var relatedInfos = utility.GetRelatedProperties();
                    //logger.Info("Submit success navigate to original location:{0}", utility.folderUrl);
                    //requestContext.Response.Redirect(utility.folderUrl);
                    if (relatedInfos != null)
                    {
                        return JsonConvert.SerializeObject(relatedInfos);
                    }
                    else
                    {
                        List<RMRelatedItemInfo> emptyInfos = new List<RMRelatedItemInfo>();
                        return JsonConvert.SerializeObject(emptyInfos);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("submit related records failed {0}", ex.ToString());
                throw;
            }
            //return string.Empty;
        }

        [HttpPost]
        public async Task<RelatedRouterInfos> IsRelatedRecord()
        {
            RelatedRouterInfos RelatedRouterInfos = null;
            try
            {
                //var requestContext = Request.Properties["MS_HttpCo7ntext"] as HttpContextWrapper;
                string redirectUrl = "";
                try
                {
                    redirectUrl = WebUtil.GetRedirectRecodsSSOLoginUrlForRelateApp();
                }
                catch (Exception ex1)
                {
                    logger.Error($"int related record re:{ex1}");
                }
                var param = GetRequestTokenParm();
                if (param.Count > 0 
                    && param.ContainsKey(SPAppConstants.ParamRelateRedirectSign) 
                    && param.ContainsKey(SPAppConstants.ParamRelateToken)
                )
                {
                    RelatedRouterInfos = new RelatedRouterInfos();
                    RelatedRouterInfos.QueryParameters = Request.GetTypedHeaders().Referer.Query;
                    RelatedRouterInfos.ResourceViaPermission = JsonConvert.SerializeObject(new UIResourceItem() { Name = Models.ResourceKeys.RelatedRecords, Value = Models.ResourceKeys.RelatedRecords.ToUrl() });
                    RelatedRouterInfos.RedirectUrl = redirectUrl;
                    RelatedRouterInfos.TimeSettingModel = await InitSettingAsync();
                }

            }
            catch (Exception ex)
            {
                logger.Error($"check related record error:{ex.ToString()}");
            }

            return RelatedRouterInfos;
        }

        private async Task<string> InitSettingAsync()
        {
            try
            {
                //var requestContext = Request.Properties["MS_HttpContext"] as HttpContextWrapper;
                Dictionary<string, string> parms = GetRequestTokenParm();
                string accessToken = string.Empty;
                string hostUrl = parms[SPAppConstants.ParamHostUrl];
                using (new PerformanceScope("RelatedRecordsController--GetAccessToken"))
                {
                    accessToken = ReletedRecordsAppTokenHelper.GetAccessToken(parms);
                }
                Guid listId = new Guid(parms[SPAppConstants.ParamListId]);
                int id = Convert.ToInt32(parms[SPAppConstants.ParamItemId]);
                RelatedRecordsInfos relatedRecordsInfos = new RelatedRecordsInfos();
                using (new PerformanceScope("RelatedRecordsController--RelatedRecordsUtility"))
                {
                    using (var utility = new RelatedRecordsUtility(hostUrl, accessToken, listId, id))
                    {
                        var tenantId = parms.ContainsKey(SPAppConstants.ParamTenantId) ? parms[SPAppConstants.ParamTenantId] : string.Empty;
                        if (string.IsNullOrEmpty(tenantId))
                        {
                            logger.Info($"try to get tenantId by web prop");
                            tenantId = utility.GetTenantId();
                        }
                        ThrowUtil.ThrowIfNullOrEmpty(tenantId, "tenant Id empty.");
                        TenantLocalValue.LogonGroupId = tenantId;
                        var tsm = await GeneralSettingService.GetTimeSettingModelAsync(tenantId);
                        return JsonConvert.SerializeObject(tsm);
                    };
                }
            }
            catch (Exception ex)
            {
                logger.Error("init setting error:{0}", ex.ToString());
                return string.Empty;
            }
            
        }

        [HttpPost]
        public RelatedRecordsInfos GetRelatedRecordsInfos()
        {
            //var requestContext = Request.Properties["MS_HttpContext"] as HttpContextWrapper;
            Dictionary<string, string> parms = GetRequestTokenParm();
            string accessToken = string.Empty;
            string hostUrl = parms[SPAppConstants.ParamHostUrl];
            using (new PerformanceScope("RelatedRecordsController--GetAccessToken"))
            {
                accessToken = ReletedRecordsAppTokenHelper.GetAccessToken(parms);
            }
            Guid listId = new Guid(parms[SPAppConstants.ParamListId]);
            int id = Convert.ToInt32(parms[SPAppConstants.ParamItemId]);
            RelatedRecordsInfos relatedRecordsInfos = new RelatedRecordsInfos();
            using (new PerformanceScope("RelatedRecordsController--RelatedRecordsUtility"))
            {
                using (var utility = new RelatedRecordsUtility(hostUrl, accessToken, listId, id))
                {
                    var relatedInfos = utility.GetRelatedProperties();
                    if (relatedInfos != null)
                    {
                        relatedRecordsInfos.RelatedInfos = JsonConvert.SerializeObject(relatedInfos);
                    }
                    var folderUrl = utility.folderUrl;
                    if (!string.IsNullOrEmpty(folderUrl))
                    {
                        relatedRecordsInfos.NavigateUrl = folderUrl;
                    }
                    var currItemName = utility.GetCurrentItemName();
                    if (!string.IsNullOrEmpty(currItemName))
                    {
                        relatedRecordsInfos.CurrentItemName = currItemName;
                    }
                    if (!string.IsNullOrEmpty(hostUrl))
                    {
                        relatedRecordsInfos.HostUrl = hostUrl;
                    }
                    return relatedRecordsInfos;
                };
            }
        }

        [HttpPost]
        public async Task<string> GetRelatedRecords()
        {
            string result = string.Empty;
            try
            {
                #region init Parm
                //var requestContext = Request.Properties["MS_HttpContext"] as HttpContextWrapper;
                Dictionary<string, string> parms = GetRequestTokenParm();
                string tenantId = parms.ContainsKey(SPAppConstants.ParamTenantId) ? parms[SPAppConstants.ParamTenantId] : string.Empty;
                string accessToken = ReletedRecordsAppTokenHelper.GetAccessToken(parms);
                Guid listId = TryParseGuid(parms[SPAppConstants.ParamListId]);
                Guid webId = Guid.Empty;
                int itemId = int.Parse(parms[SPAppConstants.ParamItemId]);
                string siteUrl = parms[SPAppConstants.ParamHostUrl];
                //var o365DomainName = parms[SPAppConstants.ParamDomain];
                //var o365DomainName = parms.ContainsKey(SPAppConstants.ParamDomain) ? parms[SPAppConstants.ParamDomain] : null;
                #endregion
                RecordsItemBrowserInfo info = null;
                List<int> scopePermissionIds = new List<int>();
                bool isEnduser = false;
                using (BrowserWorker worker = new BrowserWorker(siteUrl, accessToken))
                {
                    if (string.IsNullOrEmpty(tenantId))
                    {
                        logger.Info($"try to get tenantId by web prop => get related");
                        tenantId = worker.GetTenantId();
                    }


                    try
                    {
                        var loginUserName = worker.GetLoginName();
                        var user = await TenantUtil.RunUnderTenantAsync(tenantId, "related", GetUserByloginNameAsync, new List<string> { loginUserName });
                        isEnduser = await TenantUtil.RunUnderTenantAsync(tenantId, "related", IsPhysicalEnduserAsync, new List<string> { user.UserId });
                        var userAndGroupIds = await TenantUtil.RunUnderTenantAsync (tenantId, "related", GerUserAndGroupIdsAsync, new List<string> { user.UserId });

                        if (isEnduser)
                        {
                            scopePermissionIds = TenantUtil.RunUnderTenant(tenantId, "related", GetScopePermissionIds, userAndGroupIds);
                        }
                        logger.Info($"Related request {loginUserName} isEndUser:{isEnduser} ");
                        //var account = AOSApi
                    }
                    catch (Exception e)
                    {
                        logger.Info($"Validate physical User failed {e.ToString()}");
                    }
                    ThrowUtil.ThrowIfNullOrEmpty(tenantId, "tenant Id empty.");
                    info = worker.BrowserItemInfo(listId, itemId);
                }
                var recordId = GetRecordId(info.SiteId, info.id);
                var recordParm = new GetRelatedRecordsParm() { IsAdmin = !isEnduser, RecordId = recordId, ScopePermissions = scopePermissionIds };
                return TenantUtil.RunUnderTenant(tenantId, "related", GetRelatedRecordById, new List<GetRelatedRecordsParm>() { recordParm });

            }
            catch (Exception ex)
            {
                logger.Error("get related records failed {0}", ex.ToString());
            }
            return result;


        }
        public Task<bool> IsPhysicalEnduserAsync(List<string> accountId)
        {
            return TenantUtil.RunUnderTenantAsync(new TenantContext(TenantLocalValue.LogonGroupId, accountId[0], TenantLocalValue.LogonUserEmail),
                        async () =>
                        {
                            var isAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(Contract.RoleAssignments.RMPermissionMasks.PhysicalAdmin);
                            return !isAdmin && (await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(Contract.RoleAssignments.RMPermissionMasks.PhysicalEndUser));
                        });

        }
        public Task<List<int>> GerUserAndGroupIdsAsync(List<string> accountIds)
        {
            return UserService.GetUserAndGroupIdsAsync(accountIds[0]);
        }
        public Task<AccountDto> GetUserByloginNameAsync(List<string> userName)
        {
            return UserService.GetUserByNameAsync(userName[0]);
        }
        public List<int> GetScopePermissionIds(List<int> userAndGroupIds)
        {
            return PermissionManagementService.GetScopePermissionIds(userAndGroupIds);
        }

        [HttpPost]
        public async Task<string> GetRelatedRecordDetail([FromBody] RelatedReordsParm itemInfo)
        {
            string result = string.Empty;
            try
            {
                #region init Parm
                //var requestContext = Request.Properties["MS_HttpContext"] as HttpContextWrapper;
                Dictionary<string, string> parms = GetRequestTokenParm();
                string tenantId = parms.ContainsKey(SPAppConstants.ParamTenantId) ? parms[SPAppConstants.ParamTenantId] : string.Empty;
                string accessToken = ReletedRecordsAppTokenHelper.GetAccessToken(parms);
                Guid listId = TryParseGuid(parms[SPAppConstants.ParamListId]);
                Guid webId = Guid.Empty;
                int itemId = int.Parse(parms[SPAppConstants.ParamItemId]);
                string siteUrl = parms[SPAppConstants.ParamHostUrl];

                using (BrowserWorker worker = new BrowserWorker(siteUrl, accessToken))
                {
                    logger.Info($"try to get tenantId by web prop => get related detail");
                    if (string.IsNullOrEmpty(tenantId))
                    {
                        tenantId = worker.GetTenantId();
                    }
                    ThrowUtil.ThrowIfNullOrEmpty(tenantId, "tenant Id empty.");
                    //try
                    //{
                    //    var loginUserName = worker.GetLoginName();
                    //    var user = RMAosApiClient.ValidateUser(loginUserName, tenantId);
                    //    var userAndGroupIds = UserService.GetUserAndGroupIds(user.Account.Id);
                    //    var scopePermissionIds = PermissionManagementService.GetScopePermissionIds(userAndGroupIds);
                    //    itemInfo.ScopePermissions = scopePermissionIds;
                    //    //var account = AOSApi
                    //}
                    //catch (Exception e)
                    //{
                    //    logger.Info($"Validate physical User failed {e.ToString()}");
                    //}
                }

                //get physical permission scopes

                #endregion
                List<RelatedReordsParm> args = new List<RelatedReordsParm>() { itemInfo };
                result = await TenantUtil.RunUnderTenantAsync(tenantId, "related", GetDetailByTypeAsync, args);
            }
            catch (Exception ex)
            {
                logger.Error("get related record detail failed {0}", ex.ToString());
            }
            return result;

        }

        private async Task<string> GetDetailByTypeAsync(List<RelatedReordsParm> args)
        {
            string result = string.Empty;
            var itemInfo = args[0];
            var recordId = TryParseGuid(itemInfo.Id);
            switch (itemInfo.SourceFlag)
            {
                case (int)SourceFlag.SharePoint:
                    var spItem = await ExplorerService.LoadDetailByKeyAsync(0, recordId, ExplorerDetailTab.All);
                    result = JsonConvert.SerializeObject(spItem);
                    break;
                case (int)SourceFlag.Physical:
                    PhysicalObjectDto phyItem = null;
                    if (recordId != Guid.Empty)
                    {
                        phyItem = await ExplorerService.GetPhysicalObjectByIdAsync(recordId);
                        phyItem.HomeLocationFullPath = ExplorerService.GetPhysicalObjectFullPath(recordId);
                        if (phyItem.MetaInfo == null)
                        {
                            phyItem.MetaInfo = new Dictionary<string, string>();
                        }
                        if (phyItem.Id != Guid.Empty)
                        {
                            phyItem.Template = await TemplateManagementService.LoadTemplateDtoAsync(phyItem.TemplateId);

                        }
                        await ExplorerService.ConvertDateTimeColumnValueTimeZoneAsync(phyItem);
                    }

                    else
                    {
                        logger.Error($"Load physical object info, current id seems is not in correct format, id value: [{recordId}].");
                    }

                    //result = SerializerHelper.SerializeByJsonSerializer(phyItem);
                    result = JsonConvert.SerializeObject(phyItem);
                    break;
                default:
                    throw new NotSupportedException("invalid type");
            }

            return result;

        }

        private Guid TryParseGuid(string parm)
        {
            Guid result = Guid.Empty;
            Guid.TryParse(parm, out result);
            return result;
        }

        private Dictionary<string, string> GetRequestTokenParm()
        {
            var parmString = Request.GetTypedHeaders().Referer.Query;
            Dictionary<string, string> parmDic = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(parmString))
            {
                return parmDic;
            }
            if(parmString.Length > 0)
            {
                var parmArray = parmString.Substring(1).Split('&');
                string[] tempArr = null;
                foreach (var parm in parmArray)
                {
                    tempArr = parm.Split('=');
                    if(tempArr.Length > 1)
                    {
                        string key = WebUtility.UrlDecode(tempArr[0]);
                        string value = WebUtility.UrlDecode(tempArr[1]);
                        parmDic.Add(key, value);
                        logger.Info($"get request param key:{key}, value:{value}");
                    }
                }
            }
            

            if(Request.Cookies.TryGetValue(SPAppConstants.ParamRelateToken, out var token))
            {
                parmDic.Add(SPAppConstants.ParamRelateToken, token);
            }

            return parmDic;
        }
        private Guid GetRecordId(Guid scopeId, Guid nodeId)
        {
            return (scopeId.ToString().ToLowerInvariant() + nodeId.ToString().ToLowerInvariant()).ToMd5();
        }

        private string GetRelatedRecordById(List<GetRelatedRecordsParm> parms)
        {
            var recordParm = parms[0];
            if (recordParm.IsAdmin)
            {
                var relatedInfo = ExplorerService.GetRelatedRecoredsBaseInfo(recordParm.RecordId);
                return JsonConvert.SerializeObject(relatedInfo);
            }
            else
            {
                var relatedInfo = ExplorerService.GetRelatedRecoredsBaseInfoForStandardUser(recordParm.RecordId, recordParm.ScopePermissions);
                return JsonConvert.SerializeObject(relatedInfo);
            }
        }
    }
    public class RelatedRecordsInfos
    {
        public string RelatedInfos { get; set; }

        public string NavigateUrl { get; set; }

        public string CurrentItemName { get; set; }

        public string HostUrl { get; set; }
    }
    public class RelatedRouterInfos
    {
        public string ResourceViaPermission { get; set; }

        public string QueryParameters { get; set; }

        public string RedirectUrl { get; set; }

        public string TimeSettingModel { get; set; }
    }

}
