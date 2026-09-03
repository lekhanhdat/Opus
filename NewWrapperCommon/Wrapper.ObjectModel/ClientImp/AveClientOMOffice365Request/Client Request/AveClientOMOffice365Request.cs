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
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client.UserProfiles;
using AvePoint.GCommon;
using Microsoft.SharePoint.Client;
using System.Collections;
using Microsoft.Online.SharePoint.TenantAdministration;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using AveClientRequest.Common;
using Microsoft.SharePoint.Client.Application;
using Microsoft.SharePoint.Client.Taxonomy;
using AvePoint.ObjectModel.WebService;
using AvePoint.Wrapper.Resource.Client;
using ClientFile = Microsoft.SharePoint.Client.File;
using System.IO;
using Microsoft.SharePoint.Client.Utilities;
using System.Web.Script.Serialization;
using Microsoft.SharePoint.Client.WorkflowServices;
using Microsoft.SharePoint.Client.Workflow;
using System.Xml;
using System.Globalization;
using AvePoint.Office365.Api;
using AvePoint.ObjectModel.O365;

namespace AvePoint.ObjectModel.ClientOM
{
    public partial class AveClientOMOffice365Request : AveClientOM2019Request
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveClientOMOffice365Request));
        private bool? haveAddAndCustomizePagesPermission = null;
        private FederationToken tokenProviders;
        public AveClientOMOffice365Request(string url, AveBPOSAccountInfo userAccountInfo, List<ITokenProvider> tokenProviders, string serverVersion)
            : base(url, userAccountInfo, null, serverVersion)
        {
            this.tokenProviders = new FederationToken(tokenProviders);
            Type = AveClientRequestType.AveClientOMOffice365Request;
            ITokenProvider IDCLRToken = this.tokenProviders.GetProviderByType(TokenType.IDCLR);
            // nintex中都是使用的页面请求，需要使用IDCLR类型的Token。
            nintexAPIProcessor = new AveNintexAPIProcessor(url, IDCLRToken, Nintex.O365API.APIMethod.HTTP);
            if (IDCLRToken != null)
            {
                mRequestCommon = new AveHttpWebRequestCommonOffice365(mWebUrl, mObj, IDCLRToken);
                mWebServiceRequest.TokenProvider = IDCLRToken;
            }
            else
            {
                mRequestCommon = new AveHttpWebRequestCommonEmpty();
            }
        }

        protected override AveClientContext InitClientObject(string url)
        {
            var context = new AveO365ClientContext(url);
            context.RequestTimeout = WrapperConfiguration.BPOS_S.HttpWebRequestTimeout;//ten miniutes
            SetContextInfo(context);
            return context;
        }

        private void SetContextInfo(ClientContext context)
        {
            if (tokenProviders != null && (tokenProviders.MainTokenProvider.TokenType == TokenType.Bearer))
            {
                context.FormDigestHandlingEnabled = false;
            }
            else
            {
                context.SetFormDigest();
            }
            context.SetTokenProvider(tokenProviders == null ? null : tokenProviders.MainTokenProvider);
        }

        internal override AveClientContext CreateContext()
        {
            return CreateContext(mWebUrl);
        }

        protected override AveClientContext CreateContext(string weburl)
        {
            AveClientContext context = InitClientObject(weburl);
            return context;
        }

        private AveRetryClientContext CreateRetryContext()
        {
            return InitRetryClientContext(mWebUrl);
        }

        private AveRetryClientContext CreateRetryContext(string url)
        {
            return InitRetryClientContext(url);
        }

        private AveRetryClientContext InitRetryClientContext(string url)
        {
            var context = new AveRetryClientContext(url);
            //set timeout to 20 mins
            context.RequestTimeout = WrapperConfiguration.BPOS_S.HttpWebRequestTimeout * 2;
            SetContextInfo(context);
            context.RefreshToken(() =>
            {
                context.ResetContext(tokenProviders.MainTokenProvider);
            });
            context.RequestTimeout = WrapperConfiguration.BPOS_S.HttpWebRequestTimeout;
            return context;
        }


        [NoAPI]
        public override Dictionary<string, object> UpdateKeyWord(string term, int localId, int calendarType, Dictionary<string, object> keyWordProp)
        {
            return base.UpdateKeyWord(term, localId, calendarType, keyWordProp);
        }

        [NoAPI]
        public override void UpdateScopeDisplayGroup(int groupId, string groupName, Dictionary<string, object> updateProp)
        {
            base.UpdateScopeDisplayGroup(groupId, groupName, updateProp);
        }
        protected override bool IsSpecialLibrary(AveClientContext context, string webUrl, Guid webId, Guid listId, out List list)
        {
            list = null;
            try
            {
                var web = webId == Guid.Empty ? context.Site.OpenWeb(webUrl) : context.Site.OpenWebById(webId);
                list = web.Lists.GetById(listId);
                //Only webservice has unused version.
                //context.Load(list, l => l.BaseType, l => l.MajorWithMinorVersionsLimit);
                //context.ExecuteQuery();
                //if (list.BaseType == BaseType.DocumentLibrary && list.MajorWithMinorVersionsLimit > 0)
                //{
                //    return true;
                //}
            }
            catch (Exception e)
            {
                mLogger.Warn("An error occurred while confirm whether this list is special library. WebURl: {0}, WebId: {1} ListId: {2},  Error: {3}", webUrl, webId, listId, e);
            }
            return false;
        }

        public override Dictionary<string, object> GetItemVersionsForBrowser(string webServerRelativeUrl, string listId, int itemId, Dictionary<string, string> fields)
        {
            Dictionary<string, object> listItemVersionsProperties = new Dictionary<string, object>();
            List<Dictionary<string, object>> itemVersionPropertiesList = new List<Dictionary<string, object>>();
            using (ClientContext context = CreateContext())
            {
                var web = context.Site.OpenWeb(webServerRelativeUrl);
                var list = web.Lists.GetById(new Guid(listId));
                var item = list.GetItemById(itemId);
                context.Load(item.Versions, version => version.Include(v => v.VersionLabel));
                context.ExecuteQuery();

                foreach (var version in item.Versions)
                {
                    var listItemVersionProperties = new Dictionary<string, object>();
                    listItemVersionProperties["VersionLabel"] = version.VersionLabel;
                    itemVersionPropertiesList.Add(listItemVersionProperties);
                }
                listItemVersionsProperties.Add("ChildrenProperties", itemVersionPropertiesList);
                return listItemVersionsProperties;
            }
        }
        private static object lockObj = new object();
        private Dictionary<string, object> GetNeedLoadFields(Dictionary<string, object> fieldValues, Dictionary<string, string> needLoadFields)
        {
            // needLoadFields是list上的属性，在多线程的情况下，多个item多线程的时候会出现问题。参考CI-41498
            lock (lockObj)
            {
                Dictionary<string, object> result = new Dictionary<string, object>();
                if (!needLoadFields.ContainsKey("Modified"))
                {
                    needLoadFields.Add("Modified", "DateTime");
                }
                if (!needLoadFields.ContainsKey("Editor"))
                {
                    needLoadFields.Add("Editor", "User");
                }
                var values = new Dictionary<string, object>();
                result.Add("FieldValues", values);
                Dictionary<string, string> KeyMapping = new Dictionary<string, string>();
                KeyMapping["_UIVersion"] = "VersionId";
                KeyMapping["_UIVersionString"] = "VersionLabel";
                //KeyMapping["ID"] = "VersionId";
                KeyMapping["_IsCurrentVersion"] = "IsCurrentVersion";
                KeyMapping["FileRef"] = "Url";
                KeyMapping["File_x0020_Size"] = "Length";
                KeyMapping["_ModerationStatus"] = "ModerationStatus";
                KeyMapping["Created_x0020_By"] = "CreatedBy" + AveObjectModelConstant.ObjectPropertySuffix;
                KeyMapping["_Level"] = "Level";
                foreach (var needLoadField in needLoadFields)
                {
                    var columnNameKey = needLoadField.Key;
                    if (string.Equals(columnNameKey, "Created", StringComparison.Ordinal))
                    {
                        columnNameKey = "Created_x0020_Date";
                    }
                    if (fieldValues.ContainsKey(columnNameKey))
                    {
                        var value = fieldValues[columnNameKey];
                        if (value == null)
                        {
                            continue;
                        }
                        if (string.Equals(needLoadField.Key, "_Level", StringComparison.Ordinal))
                        {
                            try
                            {
                                value = Byte.Parse(value.ToString());
                            }
                            catch (Exception ex)
                            {
                                mLogger.Warn("Failed to convert Level value to byte, value: {0}, exception: {1}.", value.ToString(), ex);
                            }
                        }
                        //ADO-198874 On-premise 对于MultiChoice 获取的field  value是;#value;# 这种格式的，OnlineAPI 获取的是string[] 格式，为了保持和on-premise一致，此处需要修改
                        if (string.Equals(needLoadField.Value, "MultiChoice", StringComparison.OrdinalIgnoreCase) && value is string[])
                        {
                            string temp = ";#";
                            foreach (var tempVlaue in (string[])value)
                            {
                                temp += string.Format("{0};#", tempVlaue);
                            }
                            value = temp;
                        }
                        var mappedFieldName = KeyMapping.ContainsKey(needLoadField.Key) ? KeyMapping[needLoadField.Key] : needLoadField.Key;
                        result[mappedFieldName] = value;
                        values[needLoadField.Key] = value;
                    }
                }
                return result;
            }
        }

        protected override Dictionary<string, object> QueryItemVersionsForDiscover(string webRelativeUrl, string listRelativeUrl, string listId, int itemId, string itemUrl, CultureInfo cultureInfo, Dictionary<string, string> needLoadFields)
        {
            if (WrapperConfiguration.BPOS_S.BackupItemVersionByAPI)
            {
                var fileVersions = new Dictionary<int, FileVersion>();
                Dictionary<string, object> listItemVersionsProperties = new Dictionary<string, object>();
                List<Dictionary<string, object>> itemVersionPropertiesList = new List<Dictionary<string, object>>();
                using (ClientContext context = CreateContext())
                {
                    var web = context.Site.OpenWeb(webRelativeUrl);
                    var list = web.Lists.GetById(new Guid(listId));
                    var item = list.GetItemById(itemId);
                    context.Load(item, i => i.Versions.Include(
                    v => v["Modified"],
                    v => v["Editor"],
                    v => v["_Level"],
                    v => v["_UIVersion"]));
                    context.ExecuteQuery();
                    if (item.Versions.Count <= 0)
                    {
                        listItemVersionsProperties["HasVersion"] = false;
                    }
                    foreach (var version in item.Versions)
                    {
                        Dictionary<string, object> fieldValues = new Dictionary<string, object>();
                        var listItemVersionProperties = new Dictionary<string, object>();
                        foreach (KeyValuePair<string, object> fieldValue in version.FieldValues)
                        {
                            var value = fieldValue.Value;
                            // 
                            if (string.Equals(fieldValue.Key, "Created_x0020_Date", StringComparison.Ordinal))
                            {
                                value = DateTime.Parse(value.ToString(), null, DateTimeStyles.AdjustToUniversal);
                            }
                            AssembleItemProperties(fieldValues, value, fieldValue.Key);
                        }
                        itemVersionPropertiesList.Add(GetNeedLoadFields(fieldValues, needLoadFields));
                    }
                    listItemVersionsProperties.Add("ChildrenProperties", itemVersionPropertiesList);
                    return listItemVersionsProperties;
                }
            }
            else
            {
                return mWebServiceRequest.GetItemVersions(webRelativeUrl, listRelativeUrl, listId, itemId, itemUrl, cultureInfo, needLoadFields);
            }
        }





        private string GetPageInfo(AveBrowserOption option)
        {
            if (option.StartIndex == 0 || string.IsNullOrEmpty(option.PageInfo))
            {
                return string.Empty;
            }
            var data = option.PageInfo.Trim(',').Split(',');
            var index = option.StartIndex / 10 - 1;
            return string.Format("Paged=TRUE&p_ID={0}", data[index]);
        }

        protected List<Folder> QueryFoldersForLargeListV5(ClientContext context, List list, string folderUrl)
        {
            List<Folder> folders = new List<Folder>();
            var worker = new LargeListQueryWorker(context, list, folderUrl, this.MaxItemsPerThrottledOperation, GetFolderByAPI, null);
            worker.BeforeQueryAction += (contextArg, listItemsArg) =>
            {
                contextArg.Load(listItemsArg, items => items.ListItemCollectionPosition,
                                        items => items.IncludeWithDefaultProperties(item => item.Folder.ListItemAllFields.HasUniqueRoleAssignments,
                        item => item.Folder.ListItemAllFields.Id,
                        item => item.Folder.UniqueId,
                        item => item.Folder.Name,
                        item => item.Folder.ServerRelativeUrl));
            };
            worker.AfterQueryAction += (contextArg, itemArg, isLibraryArg) =>
            {
                folders.Add(itemArg.Folder);
            };
            worker.ExceptionWhenQueryAction = () =>
            {
                folders.Clear();
            };
            worker.SetCamlQueryUrl = SetCamlQueryFolderUrl;
            mLogger.Debug("Begin discover folders in large list, list.ItemCount:{0}, folder URL:{1}.", list.ItemCount, folderUrl);
            worker.Run();
            context.ExecuteQuery();
            mLogger.Debug("Finish discover folders in large list, {0} folders in folder {1}", folders.Count, folderUrl);
            return folders;
        }

        private void LoadBrowserFolderProperty(ClientContext context, ListItemCollection listItemsArg)
        {
            context.Load(listItemsArg, items => items.ListItemCollectionPosition,
                                        items => items.IncludeWithDefaultProperties(item => item.Folder.ListItemAllFields.HasUniqueRoleAssignments,
                        item => item.Folder.ListItemAllFields.Id,
                        item => item.Folder.UniqueId,
                        item => item.Folder.Name,
                        item => item.Folder.ServerRelativeUrl));
        }

        private List<Folder> QueryFoldersForLargeList(ClientContext context, List list, string folderUrl, CamlQuery query)
        {
            List<Folder> folders = new List<Folder>();
            var worker = new LargeListQueryWorker(context, list, folderUrl, this.MaxItemsPerThrottledOperation, GetFolderByAPI, query);
            worker.BeforeQueryAction += LoadBrowserFolderProperty;
            worker.AfterQueryAction += (contextArg, itemArg, isLibraryArg) =>
            {
                folders.Add(itemArg.Folder);
            };
            worker.ExceptionWhenQueryAction = () =>
            {
                folders.Clear();
            };
            worker.SetCamlQueryUrl = SetCamlQueryFolderUrl;
            mLogger.Debug("Begin browse folders in large list, list.ItemCount:{0}, folder URL:{1}.", list.ItemCount, folderUrl);
            worker.Run();
            context.ExecuteQuery();
            mLogger.Debug("Finish browse folders in large list, {0} folders in folder {1}", folders.Count, folderUrl);
            return folders;
        }

        protected int QuerySubFoldersCountForLargeList(ClientContext context, List list, string folderUrl, CamlQuery query, ref string pageInfo)
        {
            int itemCount = 0;
            string Ids = string.Empty;
            var worker = new LargeListQueryWorker(context, list, folderUrl, this.MaxItemsPerThrottledOperation, GetFolderByAPI, query);
            worker.BeforeQueryAction += (contextArg, listItemsArg) =>
            {
                contextArg.Load(listItemsArg, items => items.ListItemCollectionPosition, items => items.Include(i => i.Id));
            };
            worker.AfterQueryAction += (contextArg, itemArg, isLibraryArg) =>
            {
                //每10个item 记录一次Id,对应browser界面 一页10个item
                itemCount++;
                if (itemCount % 10 == 0)
                {
                    Ids = string.Format("{0},{1}", Ids, itemArg.Id);
                }
            };
            worker.ExceptionWhenQueryAction = () =>
            {
                itemCount = 0;
                Ids = string.Empty;
            };
            worker.SetCamlQueryUrl = SetCamlQueryFolderUrl;
            worker.Run();
            context.ExecuteQuery();
            pageInfo = Ids;
            return itemCount;
        }
        protected override void CreateUserProfile(string userName)
        {
            try
            {
                //经研究发现如果用site 的url创建context是无法创建出user profile的，必须使用CA的url来创建context，才能创建出user profile
                using (var context = CreateContext(AveUrlUtility.GetTenantAdminSiteUrl(mWebUrl)))
                {
                    var loader = ProfileLoader.GetProfileLoader(context);
                    loader.CreatePersonalSiteEnqueueBulk(new string[] { userName });
                    loader.Context.ExecuteQuery();
                }
            }
            catch (Exception e)
            {
                mLogger.Error("An error occurred while create user profile. User name: {0}, error: {1}", userName, e.ToString());
            }
        }



        /// <summary>
        /// Online 支持通过API的方式来SetVersion Setting，
        /// 因此不再需要通过HttpRequest的方式更新VersionSetting，
        /// 对于Online 该Method 空跑
        /// </summary>
        /// <param name="versionLimitedProperties"></param>
        /// <param name="listProperties"></param>
        protected override void SetVersionSetting(Dictionary<string, object> versionLimitedProperties, Dictionary<string, object> listProperties)
        {
            object count;
            listProperties.TryGetValue("MajorVersionLimit", out count);// O365 change this setting default value, it can not be set to 0; it must be between 1 and 50000
            var versionLimitCount = (int)count;
            listProperties["MajorVersionLimit"] = versionLimitCount == 0 ? 50000 : versionLimitCount;
        }

        /// <summary>
        /// Online API 支持SiteLogoUrl与SiteLogoDescription的set与get，因此需要执行HttpRequest 
        /// </summary>
        /// <param name="webProperties"></param>
        /// <returns></returns>
        protected override bool NeedUpdateWebLogo(Dictionary<string, object> webProperties)
        {
            return false;
        }








        /// <summary>
        /// 创建模拟site。
        /// </summary>
        /// <param name="CAUrl"></param>
        /// <param name="compatibilityLevel"></param>
        /// <param name="lcid"></param>
        /// <param name="owner"></param>
        /// <param name="storageQuota"></param>
        /// <param name="template"></param>
        /// <param name="timeZoneId"></param>
        /// <param name="title"></param>
        /// <param name="url"></param>
        /// <param name="resourceQuota"></param>
        /// <returns></returns>
        private string AddSimulationSite(string CAUrl, int compatibilityLevel, uint lcid, string owner, long storageQuota, string template, int timeZoneId, string title, string url, double resourceQuota)
        {
            mWebServiceRequest.AddSite(CAUrl, compatibilityLevel, lcid, owner, storageQuota, template, timeZoneId, title, url, resourceQuota);
            return string.Empty;
        }

        public override void ApplySiteDesign(string webUrl, Guid siteDesignId)
        {
            using (AveClientContext context = CreateContext())
            {
                Tenant tenant = new Tenant(context);
                var result = tenant.ApplySiteDesign(webUrl, siteDesignId);
                context.ExecuteQuery();
            }
        }


        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "user name suffix")]
        public override SiteStatus GetSiteStatus(string siteUrl, Func<AveBPOSAccountInfo, string, string> GetAdminUrl)
        {
            string adminSiteUrl = GetAdminUrl(mUserAccountInfo, siteUrl);
            Dictionary<string, object> storageProperties = new Dictionary<string, object>();
            SiteStatus status = SiteStatus.Normal;
            using (var context = CreateContext(adminSiteUrl))
            {
                Tenant tenant = new Tenant(context);

                //var properties = tenant.GetSitePropertiesByUrl(siteUrl, true);
                //The new api can not throw exception.
                var properties = tenant.GetSitePropertiesFromSharePointByFilter(string.Format("Url -eq '{0}'", siteUrl), "0", true);
                context.Load(properties);
                context.ExecuteQuery();
                if (properties.Count == 0)
                {
                    status = SiteStatus.InRecycleBin;
                    mLogger.Debug("Site:'{0}' can not be found by filter method.", siteUrl);
                    try
                    {
                        var deleteSiteProperties = tenant.GetDeletedSitePropertiesByUrl(siteUrl);
                        context.Load(deleteSiteProperties);
                        context.ExecuteQuery();
                    }
                    catch (Exception e)
                    {
                        mLogger.Debug("Site:'{0}' do not in recycle bin. Message:{1}", siteUrl, e.ToString());
                        status = SiteStatus.Deleted;
                    }
                }
            }
            return status;
        }

        protected override void LoadAppsInfo(AveClientContext context, ClientObjectList<AppInstance> apps, Web web)
        {
            context.Load(web.AppTiles);
            context.Load(apps);
        }

        protected override void AssembleAppsProperties(string webServerRelativeUrl, Web web, ClientObjectList<AppInstance> apps, List<Dictionary<string, object>> appPropertyList)
        {
            if (apps.Count > 0)
            {
                Dictionary<Guid, AppTile> appTileMapping = new Dictionary<Guid, AppTile>();
                foreach (var appTile in web.AppTiles)
                {
                    appTileMapping[appTile.AppId] = appTile;
                }

                foreach (AppInstance app in apps)
                {
                    Dictionary<string, object> appInstanceProperties = new Dictionary<string, object>();
                    CopyProperty(appInstanceProperties, app);
                    if (!string.IsNullOrEmpty(app.AppWebFullUrl))
                    {
                        appInstanceProperties["AppWebFullUrl"] = new Uri(app.AppWebFullUrl);
                    }

                    Uri startPage = null;
                    if (Uri.TryCreate(app.StartPage, UriKind.RelativeOrAbsolute, out startPage))
                    {
                        appInstanceProperties["StartPage"] = startPage;
                    }

                    Dictionary<string, object> appProperties = new Dictionary<string, object>();
                    appProperties["ProductId"] = app.ProductId;

                    AppTile appTile;
                    if (appTileMapping.TryGetValue(app.Id, out appTile))
                    {
                        appProperties["Source"] = (AveAppSource)(int)appTile.AppSource;
                    }
                    else
                    {
                        appProperties["Source"] = AveAppSource.InvalidSource;
                    }

                    appInstanceProperties["App"] = appProperties;
                    appPropertyList.Add(appInstanceProperties);
                }
            }
        }






        private Dictionary<string, object> AssembleSkyDriveProProperties(PersonProperties prop, string username = null)
        {
            Dictionary<string, object> skyDriveProp = new Dictionary<string, object>();
            bool isUsernameExists = prop.ServerObjectIsNull.HasValue && prop.ServerObjectIsNull == false;
            skyDriveProp["Exists"] = isUsernameExists;
            skyDriveProp["PersonalUrl"] = isUsernameExists ? prop.PersonalUrl : string.Empty;
            if (isUsernameExists)
            {
                Uri personalUrl = new Uri(prop.PersonalUrl, UriKind.RelativeOrAbsolute);

                if ((personalUrl.IsAbsoluteUri
                    && !personalUrl.GetLeftPart(UriPartial.Path).EndsWith("Person.aspx", StringComparison.OrdinalIgnoreCase)
                    && !personalUrl.GetLeftPart(UriPartial.Path).EndsWith("PersonImmersive.aspx", StringComparison.OrdinalIgnoreCase)
                    ))
                {
                    skyDriveProp["PersonalSpace"] = prop.PersonalUrl;
                }
                else
                {
                    skyDriveProp["PersonalSpace"] = string.Empty;
                }
            }
            else
            {
                skyDriveProp["PersonalSpace"] = string.Empty;
            }
            skyDriveProp["UserName"] = username;
            skyDriveProp["Version"] = prop.Context.ServerLibraryVersion.ToString();
            return skyDriveProp;
        }

        protected override bool UpdateLinks(ContentType contentType, Dictionary<Guid, Dictionary<string, object>> fieldLinks)
        {
            bool changed = false;
            if (fieldLinks != null)
            {
                foreach (KeyValuePair<Guid, Dictionary<string, object>> fieldlinkInterator in fieldLinks)
                {
                    FieldLink fieldLink = null;

                    foreach (var currentFieldlink in contentType.FieldLinks)
                    {
                        if (currentFieldlink.Id == fieldlinkInterator.Key)
                        {
                            fieldLink = currentFieldlink;
                        }
                    }

                    if (fieldLink == null)
                    {
                        continue;
                    }

                    if (fieldlinkInterator.Value.ContainsKey("Hidden"))
                    {
                        fieldLink.Hidden = Convert.ToBoolean(fieldlinkInterator.Value["Hidden"]);
                        changed = true;
                    }
                    if (fieldlinkInterator.Value.ContainsKey("Required"))
                    {
                        fieldLink.Required = Convert.ToBoolean(fieldlinkInterator.Value["Required"]);
                        changed = true;
                    }
                    if (fieldlinkInterator.Value.ContainsKey("DisplayName"))
                    {
                        fieldLink.DisplayName = fieldlinkInterator.Value["DisplayName"].ToString();
                        changed = true;
                    }
                    if (fieldlinkInterator.Value.ContainsKey("ReadOnly"))
                    {
                        fieldLink.ReadOnly = Convert.ToBoolean(fieldlinkInterator.Value["ReadOnly"]);
                        changed = true;
                    }
                    if (fieldlinkInterator.Value.ContainsKey("ShowInDisplayForm"))
                    {
                        fieldLink.ShowInDisplayForm = Convert.ToBoolean(fieldlinkInterator.Value["ShowInDisplayForm"]);
                        changed = true;
                    }
                }
            }
            return changed;
        }
        protected override void AddContentTypeFieldLink(ContentType contentType, Field field, Dictionary<string, object> fieldLinkProp)
        {
            FieldLinkCreationInformation Info = new FieldLinkCreationInformation();
            Info.Field = field;
            contentType.FieldLinks.Add(Info);
            int fieldLinksCount = contentType.FieldLinks.Count;
            FieldLink fieldLink = contentType.FieldLinks[fieldLinksCount - 1];
            if (fieldLinkProp.ContainsKey("Hidden"))
            {
                fieldLink.Hidden = bool.Parse(fieldLinkProp["Hidden"].ToString());
            }
            if (fieldLinkProp.ContainsKey("Required"))
            {
                fieldLink.Required = bool.Parse(fieldLinkProp["Required"].ToString());
            }
            if (fieldLinkProp.ContainsKey("DisplayName"))
            {
                fieldLink.DisplayName = fieldLinkProp["DisplayName"].ToString();
            }
            if (fieldLinkProp.ContainsKey("ReadOnly"))
            {
                fieldLink.ReadOnly = bool.Parse(fieldLinkProp["ReadOnly"].ToString());
            }
            if (fieldLinkProp.ContainsKey("ShowInDisplayForm"))
            {
                fieldLink.ShowInDisplayForm = bool.Parse(fieldLinkProp["ShowInDisplayForm"].ToString());
            }
        }


        protected override void SetEditorReadOnly(List list, bool readOnly) {/*do nothing.or 2013 will throw exception when add a file version.*/}
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Rad is a part of Value")]
        protected override void SetAdvancedSetting(Dictionary<string, object> advancedSettingProp, Dictionary<string, object> listProperties)
        {
            if (listProperties.ContainsKey("DisableGridEditing"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$AllowGridEditingSection$ctl02$AllowGrid"] = (bool)listProperties["DisableGridEditing"] ? "RadAllowGridNo" : "RadAllowGridYes";
                listProperties.Remove("DisableGridEditing");
            }
            if (listProperties.ContainsKey("NavigateForFormsPages"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$DialogForFormsPagesSection$ctl03$DialogForFormsPages"] = (bool)listProperties["NavigateForFormsPages"] ? "RadDialogForFormsPagesNo" : "RadDialogForFormsPagesYes";
                listProperties.Remove("NavigateForFormsPages");
            }
            if (listProperties.ContainsKey("IsSiteAssetsLibrary"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$AttachmentLibrarySection$ctl02$AttachmentLibrary"] = (bool)listProperties["IsSiteAssetsLibrary"] ? "RadAttachmentLibraryYes" : "RadAttachmentLibraryNo";
                listProperties.Remove("IsSiteAssetsLibrary");
            }
            if (listProperties.ContainsKey("DefaultItemOpenUseListSetting") && !(bool)listProperties["DefaultItemOpenUseListSetting"])
            {
                advancedSettingProp["ctl00$PlaceHolderMain$OpenDocumentSection$ctl01$DefaultItemOpen"] = "RadDefaultItemOpenServerSetting";
                listProperties.Remove("DefaultItemOpenUseListSetting");
            }
            else if (listProperties.ContainsKey("DefaultItemOpen"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$OpenDocumentSection$ctl01$DefaultItemOpen"] = (int)listProperties["DefaultItemOpen"] == 0 ? "RadDefaultItemOpenPreferClient" : "RadDefaultItemOpenBrowser";
                listProperties.Remove("DefaultItemOpen");
            }
            if (listProperties.ContainsKey("SendToLocationName"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$SendToSection$ctl01$TxtSendToLocationName"] = listProperties["SendToLocationName"];
                listProperties.Remove("SendToLocationName");
            }
            if (listProperties.ContainsKey("SendToLocationUrl"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$SendToSection$ctl02$TxtSendToLocationUrl"] = listProperties["SendToLocationUrl"];
                listProperties.Remove("SendToLocationUrl");
            }
            if (listProperties.ContainsKey("EnableManagedIndexes"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$ManagedIndexesSection$ctl02$AllowManagedIndex"] = (bool)listProperties["EnableManagedIndexes"] ? "RadManagedIndexesYes" : "RadManagedIndexesNo";
                listProperties.Remove("EnableManagedIndexes");
            }
            if (listProperties.ContainsKey("EnableAttachments"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$AttachmentsSection$ctl02$DisableAttachments"] = (bool)listProperties["EnableAttachments"] ? "RadAttachmentsEnabled" : "RadAttachmentsDisabled";
                listProperties.Remove("EnableAttachments");
            }
        }

        protected override void LoadWeb(Web web, ClientContext context)
        {
            context.Load(web);
            context.Load(web, w => w.SiteLogoDescription,
                              w => w.RequestAccessEmail,
                              w => w.MembersCanShare,
                              w => w.AccessRequestSiteDescription,
                              w => w.UseAccessRequestDefault);//该属性需要主动load，不然load不出来
            ExceptionHandlingScope memberGroupCondition = new ExceptionHandlingScope(context);
            using (memberGroupCondition.StartScope())
            {
                using (memberGroupCondition.StartTry())
                {
                    context.Load(web, w => w.AssociatedMemberGroup, w => w.AssociatedMemberGroup.Users, w => w.AssociatedMemberGroup.Owner.Id, w => w.AssociatedMemberGroup.Owner.PrincipalType);
                }
                using (memberGroupCondition.StartCatch())
                {
                    context.Load(web, w => w.AssociatedMemberGroup);
                }
            }
            ExceptionHandlingScope ownerGroupCondition = new ExceptionHandlingScope(context);
            using (ownerGroupCondition.StartScope())
            {
                using (ownerGroupCondition.StartTry())
                {
                    context.Load(web, w => w.AssociatedOwnerGroup, w => w.AssociatedOwnerGroup.Users, w => w.AssociatedOwnerGroup.Owner.Id, w => w.AssociatedOwnerGroup.Owner.PrincipalType);
                }
                using (ownerGroupCondition.StartCatch())
                {
                    context.Load(web, w => w.AssociatedOwnerGroup);
                }
            }
            ExceptionHandlingScope visitorGroupCondition = new ExceptionHandlingScope(context);
            using (visitorGroupCondition.StartScope())
            {
                using (visitorGroupCondition.StartTry())
                {
                    context.Load(web, w => w.AssociatedVisitorGroup, w => w.AssociatedVisitorGroup.Users, w => w.AssociatedVisitorGroup.Owner.Id, w => w.AssociatedVisitorGroup.Owner.PrincipalType);
                }
                using (visitorGroupCondition.StartCatch())
                {
                    context.Load(web, w => w.AssociatedVisitorGroup);
                }
            }
            ExceptionHandlingScope normalProperty = new ExceptionHandlingScope(context);
            using (normalProperty.StartScope())
            {
                using (normalProperty.StartTry())
                {
                    context.Load(web, w => w.CurrentUser, w => w.RootFolder, w => w.AllProperties, w => w.Navigation.TopNavigationBar, w => w.Navigation.QuickLaunch, w => w.HasUniqueRoleAssignments, w => w.SupportedUILanguageIds, w => w.AllowDesignerForCurrentUser, w => w.AllowAutomaticASPXPageIndexing);
                }
                using (normalProperty.StartCatch())
                {
                    context.Load(web, w => w.CurrentUser, w => w.RootFolder, w => w.AllProperties, w => w.Navigation.TopNavigationBar, w => w.Navigation.QuickLaunch, w => w.HasUniqueRoleAssignments);
                }
            }
        }

        //public override ListItem InternUpdate(List list, int itemid, Dictionary<string, object> itemProperties, ExceptionHandlingScope excepScope)
        //{
        //    Dictionary<string, object> itemFieldValues = itemProperties["ChangedFieldValues"] as Dictionary<string, object>;
        //    ListItem tempListItem = new ListItem(list.Context, new ObjectPathMethod(list.Context, list.Path, "GetItemById", new object[] { itemid }));
        //    bool isCurrentCheckOut = itemProperties.ContainsKey("IsCurrentCheckOut") ? (bool)itemProperties["IsCurrentCheckOut"] : false;
        //    bool changed = AveListItemRestore.SetFieldValues(tempListItem, itemFieldValues);
        //    if (changed)
        //    {
        //        string itemTitle = itemFieldValues.ContainsKey("FileLeafRef") ? itemFieldValues["FileLeafRef"] as string : string.Empty;
        //        itemFieldValues.Remove("FileLeafRef");
        //        IList<ListItemFormUpdateValue> values = new List<ListItemFormUpdateValue>();
        //        values.Add(new ListItemFormUpdateValue() { FieldName = "FileLeafRef", FieldValue = itemTitle });
        //        // ADO-169105 office文件的EnterpriseKeyword使用更新column的方法无法更新成功，需要使用ValidateUpdateListItem来更新。
        //        if (itemFieldValues.ContainsKey("TaxKeyword"))
        //        {
        //            string taxKeyword = itemFieldValues["TaxKeyword"] as string;
        //            itemFieldValues.Remove("TaxKeyword");
        //            values.Add(new ListItemFormUpdateValue() { FieldName = "TaxKeyword", FieldValue = taxKeyword });
        //        }
        //        tempListItem.SystemUpdate();
        //        list.Context.Load(tempListItem);
        //        list.Context.Load(tempListItem, it => it.HasUniqueRoleAssignments);
        //    }
        //    return tempListItem;
        //}


        public override bool UpdateTermGroupUserInfo(TermGroup group, Dictionary<string, object> needUpdateGroupProperties)
        {
            bool change = false;
            if (needUpdateGroupProperties.ContainsKey("AddContributor"))
            {
                foreach (string principalName in needUpdateGroupProperties["AddContributor"] as List<string>)
                {
                    group.AddContributor(principalName);
                    change |= true;
                }
            }
            if (needUpdateGroupProperties.ContainsKey("AddGroupManager"))
            {
                foreach (string principalName in needUpdateGroupProperties["AddGroupManager"] as List<string>)
                {
                    group.AddGroupManager(principalName);
                    change |= true;
                }
            }
            return change;
        }

        protected override List<string> UpdateListNormalProperties
        {
            get
            {
                return new List<string> { "NoCrawl", "ReadSecurity", "WriteSecurity" };
            }
        }


        public override Stream RetryGetFileStream(string webServerRelativeUrl, string fileServerRelativeUrl, string source)
        {
            Stream stream = null;
            try
            {
                string tempWebServerRelativeUrl = string.Empty;
                if (source.Equals("File", StringComparison.OrdinalIgnoreCase))
                {
                    string filePath = fileServerRelativeUrl;
                    if (!fileServerRelativeUrl.StartsWith(webServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        filePath = AveUrlUtility.CombineUrl(webServerRelativeUrl, fileServerRelativeUrl);
                    }
                    tempWebServerRelativeUrl = AveUrlUtility.CombineUrl(webServerRelativeUrl, "_layouts/15/download.aspx?SourceUrl=");
                }
                AveClientTaskRetryHelper retryHelper = new AveClientTaskRetryHelper(3, new KeyValuePair<string, string>("WebException", "Unable to connect to the remote server"),
                                                                               new KeyValuePair<string, string>("WebException", "The remote server returned an error: (500) Internal Server Error"),
                                                                               new KeyValuePair<string, string>("WebException", "The operation has timed out"),
                                                                               new KeyValuePair<string, string>("IOException", "Received an unexpected EOF or 0 bytes from the transport stream"));
                retryHelper.ShouldRetryCommonConnectionExceptions = true;
                retryHelper.ExecuteWithRetryMechanism(() =>
                {
                    stream = mWebServiceRequest.GetFileStream(tempWebServerRelativeUrl, fileServerRelativeUrl, source);
                });
            }
            catch (Exception e)
            {
                mLogger.Error("Get file throught WebService failed. File:{0} Web:{1} Error:{2}", fileServerRelativeUrl, webServerRelativeUrl, e);
            }
            return stream;
        }

        protected override Stream GetFileStreamByRestApi(string webUrl, string fileServerRelativeUrl)
        {
            string methodCmd = string.Format("getfilebyserverrelativeurl('{0}')", fileServerRelativeUrl);
            string request = string.Format("{0}/_api/Web/{1}/$value", webUrl.TrimEnd('/'), methodCmd);
            mLogger.Info("Rest api request: {0}", request);

            Stream stream = null;
            AveClientTaskRetryHelper retryHelper = new AveClientTaskRetryHelper(3, new KeyValuePair<string, string>("WebException", "Unable to connect to the remote server"),
                                                                               new KeyValuePair<string, string>("WebException", "The remote server returned an error: (500) Internal Server Error"),
                                                                               new KeyValuePair<string, string>("WebException", "The operation has timed out"),
                                                                               new KeyValuePair<string, string>("IOException", "Received an unexpected EOF or 0 bytes from the transport stream"));
            retryHelper.ExecuteWithRetryMechanism(() =>
            {
                stream = GetContentStream(request, "RApiFS");
            });
            return stream;
        }


        public override ListItem InternUpdateAPI(List list, ListItem item, Dictionary<string, object> itemProperties, ExceptionHandlingScope excepScope)
        {
            Dictionary<string, object> itemFieldValues = itemProperties["ChangedFieldValues"] as Dictionary<string, object>;
            bool isCurrentCheckOut = itemProperties.ContainsKey("IsCurrentCheckOut") ? (bool)itemProperties["IsCurrentCheckOut"] : false;
            bool changed = AveListItemRestore.SetFieldValues(item, itemFieldValues);
            if (changed)
            {
                item.SystemUpdate();
            }
            return item;
        }








        private void Backup13ModeStartOption(ClientContext context, WorkflowStartOptionCache cache, WorkflowSubscriptionService service, WorkflowSubscriptionCollection collection)
        {
            if (collection.Count == 0)
            {
                return;
            }

            foreach (var workflow in collection)
            {
                if (workflow.EventTypes.Contains("ItemAdded", StringComparer.OrdinalIgnoreCase) ||
                    workflow.EventTypes.Contains("ItemUpdated", StringComparer.OrdinalIgnoreCase))
                {

                    string cacheKeyName = WorkflowStartOptionCache.ListWorkflow;
                    if (!string.IsNullOrEmpty(workflow.ParentContentTypeId))
                    {
                        cacheKeyName = workflow.ParentContentTypeId;
                    }
                    if (!cache.SP2013ModeWorkflowAutoStartCache.ContainsKey(cacheKeyName))
                    {
                        cache.SP2013ModeWorkflowAutoStartCache.Add(cacheKeyName, new List<WorkflowStartOption>());
                    }
                    var listCache = cache.SP2013ModeWorkflowAutoStartCache[cacheKeyName];

                    var option = new WorkflowStartOption()
                    {
                        DefinitionId = workflow.Id,
                        ItemAdded = workflow.EventTypes.Contains("ItemAdded", StringComparer.OrdinalIgnoreCase),
                        ItemUpdated = workflow.EventTypes.Contains("ItemUpdated", StringComparer.OrdinalIgnoreCase)
                    };
                    listCache.Add(option);
                    mLogger.Debug("Change auto start option for 2013 mode workflow:{0}:{1},AutoStart:{2} to {3},AutoChange:{4} to {5}",
                              workflow.Name, workflow.Id, option.ItemAdded, false, option.ItemUpdated, false);
                    var clonedSubscription = CloneSubscription(context, workflow);
                    var eventList = clonedSubscription.EventTypes.ToList();
                    eventList.Remove("ItemAdded");
                    eventList.Remove("ItemUpdated");
                    clonedSubscription.EventTypes = eventList.ToArray();
                    service.PublishSubscription(clonedSubscription);
                }
            }
        }

        private WorkflowSubscription CloneSubscription(ClientContext context, WorkflowSubscription subscription)
        {
            WorkflowSubscription workflowSubscription = new WorkflowSubscription(context);
            workflowSubscription.DefinitionId = subscription.DefinitionId;
            workflowSubscription.EventSourceId = subscription.EventSourceId;
            workflowSubscription.Id = subscription.Id;
            workflowSubscription.Name = subscription.Name;
            if (subscription.PropertyDefinitions != null)
            {
                foreach (KeyValuePair<string, string> keyValuePair in subscription.PropertyDefinitions)
                {
                    workflowSubscription.SetProperty(keyValuePair.Key, keyValuePair.Value);
                }
            }
            workflowSubscription.StatusFieldName = subscription.StatusFieldName;
            string eventTypeStr = string.Empty;
            List<string> eventTypes = new List<string>();
            foreach (string eventType in subscription.EventTypes)
            {
                if (!eventTypes.Contains(eventType))
                {
                    eventTypeStr += eventType + "#;";
                    eventTypes.Add(eventType);
                }
            }
            workflowSubscription.EventTypes = eventTypes.ToArray();
            return workflowSubscription;
        }


        protected override void LoadItemsProperty(ClientContext context, ListItemCollection items)
        {
            ExceptionHandlingScope scope = new ExceptionHandlingScope(context);
            using (scope.StartScope())
            {
                //ADO-157190 365 CommunitySite中自带的Disscussion List中的ListItem load DisplayName时会出异常
                using (scope.StartTry())
                {
                    context.Load(items);
                    context.Load(items, its => its.Include(t => t.HasUniqueRoleAssignments, t => t.DisplayName, t => t.Properties));
                }
                using (scope.StartCatch())
                {
                    context.Load(items);
                    context.Load(items, its => its.Include(t => t.HasUniqueRoleAssignments));
                }
            }
        }





        protected override void HandleMetaInfoField(AveClientContext context, ListItem item, Dictionary<string, object> itemFieldValues)
        {
            if (itemFieldValues.ContainsKey("Properties"))
            {
                LoadItemProperty(context, item);
                context.ExecuteQuery();
                var properties = itemFieldValues["Properties"] as Hashtable;
                foreach (DictionaryEntry et in properties)
                {
                    item.Properties[et.Key.ToString()] = et.Value;
                }
            }
            else
            {
                base.HandleMetaInfoField(context, item, itemFieldValues);
            }
        }

        protected override void LoadContentType(ClientContext context, ContentType contentType)
        {
            context.Load(contentType, c => c.Id, c => c.SchemaXml, c => c.FieldLinks, c => c.SchemaXmlWithResourceTokens);
        }




        /// <summary>
        /// ADO-201408 经研究发现 API 无法更新list level xmldocuments，可以成功更新web level xmldocuments
        /// </summary>
        /// <param name="context"></param>
        /// <param name="contentType"></param>
        /// <param name="needUpdateContentTypeProperties"></param>
        /// <param name="newProp"></param>
        /// <returns>true: 使用API update 成功</returns>
        private bool UpdateContentTypeAddedDocumentsByAPI(ClientContext context, ContentType contentType, Dictionary<string, object> needUpdateContentTypeProperties, Dictionary<string, object> newProp, bool updateChildren)
        {
            if (context.HasPendingRequest)
            {
                context.ExecuteQuery();
            }
            try
            {
                var doc = new XmlDocument();
                if (!string.IsNullOrEmpty(contentType.SchemaXmlWithResourceTokens))
                {
                    doc.LoadXml(contentType.SchemaXmlWithResourceTokens);
                }
                else
                {
                    doc.LoadXml(contentType.SchemaXml);
                }

                Dictionary<string, string> XmlDocumentData = (Dictionary<string, string>)needUpdateContentTypeProperties["AddedDocuments"];

                var tags = doc.GetElementsByTagName("XmlDocuments");
                XmlNode node = null;
                if (tags.Count > 0)
                {
                    node = tags[0];
                }
                else
                {
                    node = doc.DocumentElement.AppendChild(doc.CreateElement("XmlDocuments"));
                }
                foreach (string str in XmlDocumentData.Keys)
                {
                    var element = doc.CreateElement("XmlDocument");
                    element.SetAttribute("NamespaceURI", str);
                    string str2 = XmlDocumentData[str];
                    UTF8Encoding encoding = new UTF8Encoding();
                    byte[] buffer = encoding.GetBytes(str2);
                    element.InnerText = Convert.ToBase64String(buffer, 0, buffer.Length);
                    node.AppendChild(element);
                }
                contentType.SchemaXmlWithResourceTokens = doc.OuterXml;
                contentType.Update(updateChildren);
                context.ExecuteQuery();
                newProp["SchemaXmlWithResourceTokens"] = contentType.SchemaXmlWithResourceTokens;
                return true;
            }
            catch (Exception ex)
            {
                mLogger.Warn("An error occurred while update content type AddedDocuments property, content type name: {0}, error: {1}", contentType.Name, ex);
                return false;
            }




        }
        private Dictionary<string, object> GetNeedUpdateContentTypePropertiesForWebService(Dictionary<string, object> needUpdateContentTypeProperties)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            object temp;
            if (needUpdateContentTypeProperties.TryGetValue("NewDocumentControl", out temp))
            {
                properties["NewDocumentControl"] = temp;
            }
            if (needUpdateContentTypeProperties.TryGetValue("RequireClientRenderingOnNew", out temp))
            {
                properties["RequireClientRenderingOnNew"] = temp;
            }
            return properties;
        }

        private void LoadProperty(AveClientContext context, Action loadProperty)
        {
            ExceptionHandlingScope excepScope = new ExceptionHandlingScope(context);

            using (excepScope.StartScope())
            {
                using (excepScope.StartTry())
                {
                    loadProperty();
                }
                using (excepScope.StartCatch()) ;
            }
        }


        private Dictionary<string, object> LoadAlertProprty(AveClientContext context, Web web, Guid alertId)
        {
            var alert = web.Alerts.GetById(alertId);
            context.Load(alert);
            context.ExecuteQuery();
            LoadAlertSpecialProperty(context, alert);
            context.ExecuteQuery();
            return LoadAlertProprty(alert);
        }


        internal override bool UpdateWebUserResource(Web web, Dictionary<string, object> changeProperties)
        {
            bool change = false;
            change |= UpdateUserResource(web.TitleResource, AveUserResourceConstants.TITLE_RESOUCE, changeProperties);
            change |= UpdateUserResource(web.DescriptionResource, AveUserResourceConstants.DESCRIPTION_RESOUCE, changeProperties);
            return change;
        }
        internal override bool UpdateListUserResource(List list, Dictionary<string, object> changeProperties)
        {
            bool change = false;
            change |= UpdateUserResource(list.TitleResource, AveUserResourceConstants.TITLE_RESOUCE, changeProperties);
            change |= UpdateUserResource(list.DescriptionResource, AveUserResourceConstants.DESCRIPTION_RESOUCE, changeProperties);
            return change;
        }
        internal override bool UpdateFieldUserResource(Field field, Dictionary<string, object> changeProperties)
        {
            bool change = false;
            change |= UpdateUserResource(field.TitleResource, AveUserResourceConstants.TITLE_RESOUCE, changeProperties);
            change |= UpdateUserResource(field.DescriptionResource, AveUserResourceConstants.DESCRIPTION_RESOUCE, changeProperties);
            return change;
        }

        internal override bool UpdateContentTypeUserResource(ContentType contentType, Dictionary<string, object> changeProperties)
        {
            bool change = false;
            change |= UpdateUserResource(contentType.NameResource, AveUserResourceConstants.TITLE_RESOUCE, changeProperties);
            change |= UpdateUserResource(contentType.DescriptionResource, AveUserResourceConstants.DESCRIPTION_RESOUCE, changeProperties);
            return change;
        }
        private bool UpdateUserResource(UserResource resource, string resourceName, Dictionary<string, object> changeProperties)
        {
            bool change = false;
            object changeResourcesObj;
            if (changeProperties.TryGetValue(resourceName, out changeResourcesObj))
            {
                changeProperties.Remove(resourceName);
                var changeResources = changeResourcesObj as Dictionary<string, string>;
                if (changeResources != null)
                {
                    foreach (var item in changeResources)
                    {
                        resource.SetValueForUICulture(item.Key, item.Value);
                        change = true;
                    }
                }
            }
            return change;
        }

        public override void UpdateSupportedUICulture(Dictionary<string, object> webProperties, Web web, ref bool changed)
        {
            if (webProperties.ContainsKey("SupportedUILanguageIds"))
            {
                List<int> languages = webProperties["SupportedUILanguageIds"] as List<int>;
                foreach (var info in languages)
                {
                    web.AddSupportedUILanguage(info);
                }
                changed = true;
            }
        }

        private string GetSPOAdminUrl(string siteUrl)
        {
            mLogger.Info("start to get admin url by site url {0}", siteUrl);

            Uri siteUri = new Uri(siteUrl);
            int firstDotIndex = siteUri.Host.IndexOf('.');
            string domainPrefix = siteUri.Host.Substring(0, firstDotIndex);
            if (domainPrefix.EndsWith("-my", StringComparison.OrdinalIgnoreCase)
                || domainPrefix.EndsWith("-public", StringComparison.OrdinalIgnoreCase)
                || domainPrefix.EndsWith("-admin", StringComparison.OrdinalIgnoreCase))
            {
                domainPrefix = domainPrefix.Remove(domainPrefix.LastIndexOf('-'));
            }
            string domainSuffix = siteUri.Host.Substring(firstDotIndex, siteUri.Host.Length - firstDotIndex);
            return string.Format("https://{0}-admin{1}", domainPrefix, domainSuffix);
        }
        private Dictionary<string, object> AssembleComplianceTagInfo(ListItem item)
        {
            Dictionary<string, object> complianceTagInfo = new Dictionary<string, object>();
            Dictionary<string, object> complianceTagInfoProperties = new Dictionary<string, object>();
            complianceTagInfoProperties.Add("ComplianceTag", item.ComplianceInfo.ComplianceTag);
            complianceTagInfoProperties.Add("TagPolicyHold", item.ComplianceInfo.TagPolicyHold);
            complianceTagInfoProperties.Add("TagPolicyRecord", item.ComplianceInfo.TagPolicyRecord);
            complianceTagInfoProperties.Add("TagPolicyEventBased", item.ComplianceInfo.TagPolicyEventBased);
            if (item.FieldValues.ContainsKey("_ComplianceFlags") && !string.IsNullOrEmpty(item["_ComplianceFlags"].ToString()))
            {
                complianceTagInfoProperties.Add("ComplianceFlags", Int32.Parse(item["_ComplianceFlags"].ToString()));
            }
            else
            {
                complianceTagInfoProperties.Add("ComplianceFlags", 0);
            }

            DateTime date = DateTime.MinValue;
            if (item.FieldValues.ContainsKey("_ComplianceTagWrittenTime"))
            {
                var dateString = item["_ComplianceTagWrittenTime"].ToString();

                if (!string.IsNullOrEmpty(dateString))
                {
                    if (!DateTime.TryParse(dateString, out date))
                    {
                        mLogger.Warn(string.Format("Failed to set the compliance apply date from list item, _ComplianceTagWrittenTime value: {0}", dateString));
                    }
                }
            }
            complianceTagInfoProperties.Add("ComplianceWrittenDate", date.ToUniversalTime());

            if (item.FieldValues.ContainsKey("_ComplianceTagUserId") && !string.IsNullOrEmpty(item["_ComplianceTagUserId"].ToString()))
            {
                complianceTagInfoProperties.Add("ComplianceTagUserId", Int32.Parse(item["_ComplianceTagUserId"].ToString()));
            }
            else
            {
                complianceTagInfoProperties.Add("ComplianceTagUserId", -1);
            }

            complianceTagInfo.Add("ComplianceTagInfo" + AveObjectModelConstant.ObjectPropertySuffix, complianceTagInfoProperties);
            return complianceTagInfo;
        }



        public override void ConvertUserIdInfo(Dictionary<string, object> userProperties)
        {
            if (userProperties.ContainsKey("UserId"))
            {
                var info = userProperties["UserId"] as UserIdInfo;
                if (info != null)
                {
                    userProperties["UserId" + AveObjectModelConstant.ObjectPropertySuffix] = new Dictionary<string, object>() {
                        { "NameId",info.NameId },
                        { "NameIdIssuer",info.NameIdIssuer}
                    };
                }
                userProperties.Remove("UserId");
            }
        }


        protected override void DeleteItemsUnderList(AveClientContext context, Web web, List list, string webServerRelativeUrl, string listName, Guid listId)
        {
            context.Load(list, l => l.ItemCount);
            context.Load(list, l => l.RootFolder);
            context.ExecuteQuery();
            string folderServerRelativeUrl = list.RootFolder.ServerRelativeUrl;
            if (list.ItemCount > 5000)
            {
                ListItemCollection listItems = null;
                List<int> itemIds = new List<int>();
                List<int> folderIds = new List<int>();
                int index = 0; //遍历使用的item id
                int itemCount = 0; //每次遍历获取过的item count
                bool delete = false; //是否执行了删除操作
                do
                {
                    CamlQuery camlQuery = new CamlQuery();
                    camlQuery.ViewXml = string.Format(
                        "<View>" +
                        "<Query><Where><And>" +
                        "<Gt><FieldRef Name=\"ID\"/>" +
                        "<Value Type=\"Integer\">{0}</Value>" +
                        "</Gt>" +
                        "<Leq><FieldRef Name=\"ID\"/>" +
                        "<Value Type=\"Integer\">{1}</Value>" +
                        "</Leq>" +
                        "</And></Where></Query>" +
                        "</View>", index, index + 2000);
                    SetCamlQueryFolderUrl(camlQuery, folderServerRelativeUrl);
                    //camlQuery.FolderServerRelativeUrl = folderServerRelativeUrl;
                    listItems = list.GetItems(camlQuery);
                    context.Load(listItems, items => items.ListItemCollectionPosition,
                                                     items => items.Include(item => item.Id, item => item["FSObjType"], item => item["FileRef"]
                                                         , item => item["ItemChildCount"], item => item["FolderChildCount"]));
                    context.ExecuteQuery();
                    if (listItems.Count > 0)
                    {
                        foreach (ListItem item in listItems)
                        {
                            index = item.Id;
                            if (item["FSObjType"].ToString().Equals("1"))
                            {
                                int count = Convert.ToInt32(item["ItemChildCount"]) + Convert.ToInt32(item["FolderChildCount"]);
                                if (count > 0)
                                {
                                    index = DeleteFolderItems(context, list, WebAppName.TrimEnd('/') + webServerRelativeUrl, item, count);
                                }
                                folderIds.Add(item.Id);
                            }
                            else
                            {
                                itemCount++;
                                if (itemCount > 4999)
                                {
                                    itemIds.Add(item.Id);
                                }
                            }
                        }
                        if (folderIds.Count > 0)
                        {
                            AveWebServiceRequest.DeleteItems(WebAppName.TrimEnd('/') + webServerRelativeUrl, listName, mObj, tokenProviders.MainTokenProvider, folderIds);
                            folderIds.Clear();
                            delete = true;
                        }
                        if (itemIds.Count > 0)
                        {
                            AveWebServiceRequest.DeleteItems(WebAppName.TrimEnd('/') + webServerRelativeUrl, listName, mObj, tokenProviders.MainTokenProvider, itemIds);
                            itemIds.Clear();
                            delete = true;
                        }
                        if (delete)
                        {
                            //list = web.Lists.GetByTitle(listName);
                            list = web.Lists.GetById(listId);
                            context.Load(list, l => l.ItemCount, l => l.Title);
                            context.ExecuteQuery();
                        }
                    }
                    else
                    {
                        index = index + 2000;
                    }
                }
                while (list.ItemCount > 5000);
            }
        }
        private int DeleteFolderItems(AveClientContext context, List list, string webUrl, ListItem folder, int childCount)
        {
            ListItemCollection listItems = null;
            List<int> itemIds = new List<int>();
            List<int> folderIds = new List<int>();
            int itemCount = 0;
            int index = folder.Id;
            do
            {
                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = string.Format(
                    "<View>" +
                    "<Query><Where><And>" +
                    "<Gt><FieldRef Name=\"ID\"/>" +
                    "<Value Type=\"Integer\">{0}</Value>" +
                    "</Gt>" +
                    "<Leq><FieldRef Name=\"ID\"/>" +
                    "<Value Type=\"Integer\">{1}</Value>" +
                    "</Leq>" +
                    "</And></Where></Query>" +
                    "</View>", index, index + 1000);
                //camlQuery.FolderServerRelativeUrl = folder["FileRef"].ToString();
                SetCamlQueryFolderUrl(camlQuery, folder["FileRef"].ToString());
                listItems = list.GetItems(camlQuery);
                context.Load(listItems, items => items.ListItemCollectionPosition,
                                                 items => items.Include(item => item.Id, item => item["FSObjType"], item => item["FileRef"]
                                                     , item => item["ItemChildCount"], item => item["FolderChildCount"]));
                context.ExecuteQuery();
                foreach (ListItem item in listItems)
                {
                    childCount--;
                    index = item.Id;
                    if (item["FSObjType"].ToString().Equals("1"))
                    {
                        int count = Convert.ToInt32(item["ItemChildCount"]) + Convert.ToInt32(item["FolderChildCount"]);
                        if (count > 0)
                        {
                            index = DeleteFolderItems(context, list, webUrl, item, count);
                        }
                        folderIds.Add(item.Id);
                    }
                    else
                    {
                        itemCount++;
                        if (itemCount > 4999)
                        {
                            itemIds.Add(item.Id);
                        }
                    }
                }
                if (folderIds.Count > 0)
                {
                    AveWebServiceRequest.DeleteItems(webUrl, list.Title, mObj, tokenProviders.MainTokenProvider, folderIds);
                    folderIds.Clear();
                }
                if (itemIds.Count > 0)
                {
                    AveWebServiceRequest.DeleteItems(webUrl, list.Title, mObj, tokenProviders.MainTokenProvider, itemIds);
                    itemIds.Clear();
                }
            }
            while (childCount > 0);
            return index;
        }

        public override void OperateOnVersion(string webServerRelativeUrl, string webAppName, object obj, string listUrl, int itemId, int versionId, string listId, string fileName, string op)
        {
            mRequestCommon.OperateOnVersion(webServerRelativeUrl, webAppName, obj, listUrl, itemId, versionId, listId, fileName, op);
            //string url = webAppName.TrimEnd('/') + "/" + webServerRelativeUrl.Trim('/') + "/_layouts/15/Versions.aspx?";
            //AveHttpWebRequestCommon.OperateOnVersion(url, webAppName, obj, listUrl, itemId, versionId, listId, fileName, op);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint URL")]
        protected override List<Dictionary<string, object>> GetInstalledApps(string webServerRelativeUrl)
        {
            string getAppsUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/15/addanapp.aspx?task=GetMyApps&sort=1&query=&myappscatalog=1&ci=1&vd=1";
            string jasonResponse = AveHttpWebRequestUtility.HttpGet(getAppsUrl, this.mObj, this.tokenProviders.GetProviderByType(TokenType.IDCLR));
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            List<Dictionary<string, object>> appsMetadata = jsSerializer.Deserialize<List<Dictionary<string, object>>>(jasonResponse);
            return appsMetadata;
        }
    }

    public class FederationToken
    {
        private ITokenProvider mainTokenProvider;
        public List<ITokenProvider> TokenProviders { get; private set; }
        /// <summary>
        /// 联合认证的时候返回app token，其他时候只有一个token直接返回。
        /// </summary>
        public ITokenProvider MainTokenProvider
        {
            get
            {
                if (mainTokenProvider == null)
                {
                    if (TokenProviders != null)
                    {
                        if (TokenProviders.Count == 1)
                        {
                            return TokenProviders[0];
                        }
                        else
                        {
                            foreach (var p in TokenProviders)
                            {
                                if (p.TokenType == TokenType.Bearer)
                                {
                                    mainTokenProvider = p;
                                }
                            }
                        }
                    }
                }
                return mainTokenProvider;
            }
        }

        public ITokenProvider GetProviderByType(TokenType type)
        {
            if (TokenProviders != null)
            {
                foreach (var provider in TokenProviders)
                {
                    if (provider.TokenType.Equals(type))
                    {
                        return provider;
                    }
                }
            }
            return null;
        }

        public FederationToken(List<ITokenProvider> tokenProviders)
        {
            this.TokenProviders = tokenProviders;
        }
    }

    public static class PropertyExtension
    {
        public static T? SafeGetAndRemoveProperty<T>(this Dictionary<string, object> source, string key) where T : struct
        {
            object obj;
            if (source.TryGetValue(key, out obj))
            {
                source.Remove(key);
                return new T?((T)obj);
            }
            return null;
        }

        public static string SafeGetAndRemoveProperty(this Dictionary<string, object> source, string key)
        {
            object obj;
            if (source.TryGetValue(key, out obj))
            {
                source.Remove(key);
                return (string)obj;
            }
            return null;
        }
    }
}

