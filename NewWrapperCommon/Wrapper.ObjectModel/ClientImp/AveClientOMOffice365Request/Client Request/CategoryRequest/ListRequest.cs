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
using AveClientRequest.Common;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.ObjectModel.ClientOM
{
    public partial class AveClientOMOffice365Request
    {
        [KeepOriginalWithAPIAttribute]
        public override Dictionary<string, object> AddList(string webServerRelativeUrl, string title, string description, Guid featureId, int webTemplateType)
        {
            return base.AddList(webServerRelativeUrl, title, description, featureId, webTemplateType);
        }

        [NoAPIAttribute]
        public override Dictionary<string, object> AddList(string webServerRelativeUrl, string title, string description, IAveListTemplate listTemplate)
        {
            this.mRequestCommon.AddList(webServerRelativeUrl, title, description, listTemplate);
            return this.GetList(webServerRelativeUrl, title);
        }

        [ReplaceByAPIAttribute]
        public override Dictionary<string, object> AddList(string webServerRelativeUrl, string title, string description, string url, string featureId, int templateType, string docTemplateType, int quickLaunchOptions)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ListCreationInformation newList = new ListCreationInformation();
                newList.Description = description;
                newList.Title = title;
                newList.Url = url;
                newList.TemplateFeatureId = new Guid(featureId);
                newList.TemplateType = templateType;
                newList.DocumentTemplateType = string.IsNullOrEmpty(docTemplateType) ? 0 : Convert.ToInt32(docTemplateType);
                newList.QuickLaunchOption = (QuickLaunchOptions)quickLaunchOptions;
                List list = web.Lists.Add(newList);
                TryLoadList(context, web, ref list, title);
                Dictionary<string, object> prop = new Dictionary<string, object>();
                AveObjectCopy.GetObjectBasicProperties(prop, list);
                Dictionary<string, object> rootFolderProp = new Dictionary<string, object>();
                AssemblRootFolderProperties(webServerRelativeUrl, rootFolderProp, list.RootFolder);
                prop["RootFolder" + AveObjectModelConstant.ObjectPropertySuffix] = rootFolderProp;
                return prop;
            }
        }
        
        [KeepOriginalWithAPIAttribute]
        public override Dictionary<string, object> AddList(string webServerRelativeUrl, string title, string description, string url, Dictionary<string, object> dataSource)
        {
            return base.AddList(webServerRelativeUrl, title, description, url, dataSource);
        }

        [KeepOriginalWithAPIAttribute]
        public override Dictionary<string, object> AddEventReceiverDefinition(string webServerRelativeUrl, string listServerRealtiveUrl, string listTitle, Guid listId, string eventReceiverDefSource, int receiverType, string assembly, string className, string name)
        {
            return base.AddEventReceiverDefinition(webServerRelativeUrl, listServerRealtiveUrl, listTitle, listId, eventReceiverDefSource, receiverType, assembly, className, name);
        }

        [KeepOriginalWithAPIAttribute]
        public override void AddViewField(string webServerRelativeUrl, string listTitle, Guid listId, Guid viewId, string field)
        {
            base.AddViewField(webServerRelativeUrl, listTitle, listId, viewId, field);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetListTemplates(string webServerRelativeUrl)
        {
            return base.GetListTemplates(webServerRelativeUrl);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetCatalog(string webServerRelativeUrl, int typeCatalog)
        {
            return base.GetCatalog(webServerRelativeUrl, typeCatalog);
        }
        [KeepOriginalWithAPI]
        public override Guid GetListId(Guid webId, string listTitle)
        {
            return base.GetListId(webId, listTitle);
        }
        [NoAPI]
        public override Dictionary<string, object> GetListAccessRequestsSettingProperties(string webServerRelativeUrl, Guid listId)
        {
            return base.GetListAccessRequestsSettingProperties(webServerRelativeUrl, listId);
        }
        [NoAPI]
        public override bool GetListRated(string webServerRelativeUrl, Guid listId)
        {
            return base.GetListRated(webServerRelativeUrl, listId);
        }
        [ReplaceByAPI]
        public override Dictionary<string, object> GetListVersionLimited(string webServerRelativeUrl, Guid listId)
        {
            using (var context = CreateContext())
            {
                Dictionary<string, object> versionLimitedProp = new Dictionary<string, object>();
                var web = context.Site.OpenWeb(webServerRelativeUrl);
                var list = web.Lists.GetById(listId);
                context.Load(list, l => l.MajorVersionLimit, l => l.MajorWithMinorVersionsLimit, l => l.DraftVersionVisibility);
                context.ExecuteQuery();
                versionLimitedProp["MajorVersionLimit"] = list.MajorVersionLimit;
                versionLimitedProp["MajorWithMinorVersionsLimit"] = list.MajorWithMinorVersionsLimit;
                versionLimitedProp["DraftVersionVisibility"] = (int)list.DraftVersionVisibility;
                return versionLimitedProp;
            }
        }
        [NoAPI]
        public override Dictionary<string, object> GetPerLocationViewSettings(string webServerRelativeUrl, Guid listId)
        {
            return base.GetPerLocationViewSettings(webServerRelativeUrl, listId);
        }
        [TODO("可以解析RootFolder来获取RSS属性")]
        public override Dictionary<string, object> GetListRssProperties(string webServerRelativeUrl, Guid listId)
        {
            return base.GetListRssProperties(webServerRelativeUrl, listId);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetViews(string webServerRelativeUrl, string listName, Guid listId)
        {
            return base.GetViews(webServerRelativeUrl, listName, listId);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetForms(string webServerRelativeUrl, string listName, Guid listId)
        {
            return base.GetForms(webServerRelativeUrl, listName, listId);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetLists(Guid webId)
        {
            return base.GetLists(webId);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetList(string webServerRelativeUrl, Guid listId)
        {
            return base.GetList(webServerRelativeUrl, listId);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetLists(string webServerRelativeUrl)
        {
            return base.GetLists(webServerRelativeUrl);
        }

        [ReplaceByAPI]
        protected override void LoadListCollection(AveClientContext context, ExceptionHandlingScope scope, ListCollection listCollection)
        {
            using (scope.StartScope())
            {
                using (scope.StartTry())
                {
                    context.Load(listCollection, tempListCollection => tempListCollection.IncludeWithDefaultProperties(l => l.ValidationFormula,
                                                                                                      l => l.ValidationMessage,
                                                                                                      l => l.OnQuickLaunch,
                                                                                                      l => l.IsSiteAssetsLibrary,
                                                                                                      l => l.HasUniqueRoleAssignments,
                                                                                                      l => l.DataSource,
                                                                                                      l => l.Id,
                                                                                                      l => l.ItemCount,
                                                                                                      l => l.EnableAttachments,
                                                                                                      l => l.EnableVersioning,
                                                                                                      l => l.DefaultDisplayFormUrl,
                                                                                                      l => l.EnableAssignToEmail,
                                                                                                      l => l.ExcludeFromOfflineClient,
                                                                                                      l => l.ListExperienceOptions,
                                                                                                      l => l.ReadSecurity,
                                                                                                      l => l.WriteSecurity,
                                                                                                      l => l.RootFolder,
                                                                                                      l => l.RootFolder.Properties
                                                                                                      ));
                }
                using (scope.StartCatch())
                {
                    context.Load(listCollection, tempListCollection => tempListCollection.IncludeWithDefaultProperties(
                                                                                                      l => l.HasUniqueRoleAssignments,
                                                                                                      l => l.DataSource,
                                                                                                      l => l.Id,
                                                                                                      l => l.ItemCount,
                                                                                                      l => l.EnableAttachments,
                                                                                                      l => l.EnableVersioning,
                                                                                                      l => l.RootFolder,
                                                                                                      l => l.RootFolder.Properties
                                                                                                      ));
                }
            }
        }

        [ReplaceByAPI]
        public override void DeleteList(string webServerRelativeUrl, string listName, Guid listId)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                //List list = web.Lists.GetByTitle(listName);
                List list = null;
                if (listId != Guid.Empty)
                {
                    list = web.Lists.GetById(listId);
                }
                else
                {
                    list = web.Lists.GetByTitle(listName);
                }
                try
                {
                    list.DeleteObject();
                    context.ExecuteQuery();
                }
                catch (Exception e)
                {
                    mLogger.Debug("Can not delete system list. List name: {0}, Exception Message: {1}", listName, e);
                    throw;
                }

            }
        }

        [ReplaceByAPI]
        public override Dictionary<string, object> UpdateList(string webServerRelativeUrl, string listName, Guid listId, Dictionary<string, object> listProperties)
        {
            int type = listProperties.ContainsKey("ListType") ? (int)listProperties["ListType"] : -1;
            listProperties.Remove("ListType");
            if (type == (int)AveListTemplateType.Survey
                && mWebServiceRequest.TokenProvider != null
                && mWebServiceRequest.TokenProvider.TokenType == Office365.Api.TokenType.IDCLR)
            {
                Dictionary<string, object> dicPro = new Dictionary<string, object>();
                foreach (var property in UpdateListNormalProperties)
                {
                    if (listProperties.ContainsKey(property))
                    {
                        dicPro[property] = listProperties[property];
                        listProperties.Remove(property);
                    }
                }
                if (dicPro.Count > 0)
                {
                    base.UpdateList(webServerRelativeUrl, listName, listId, dicPro);
                }
                return mWebServiceRequest.UpdateList(webServerRelativeUrl, listName, listId, listProperties);
            }
            else
            {
                Dictionary<string, object> advancedSettingProp = new Dictionary<string, object>();
                SetAdvancedSetting(advancedSettingProp, listProperties);
                Dictionary<string, object> generalSettings = new Dictionary<string, object>();
                SetGeneralSetting(generalSettings, listProperties);
                using (ClientContext context = CreateContext())
                {
                    //code: "list.DocumentTemplateUrl = string.Empty;" works fine in server mode, we should make it work in client mode
                    context.ValidateOnClient = false;
                    Web web = context.Site.OpenWeb(webServerRelativeUrl);
                    List list = null;
                    if (listId != Guid.Empty)
                    {
                        list = web.Lists.GetById(listId);
                    }
                    else
                    {
                        list = web.Lists.GetByTitle(listName);
                    }
                    //API都支持的Setting，可以不需要走反射
                    //某些list setting是需要在开启version的情况下才能设置的，例如：MajorVersionLimit，提前设置list version setting [ADO-159059]
                    object obj = null;
                    if (listProperties.TryGetValue("EnableModeration", out obj))
                    {
                        list.EnableModeration = (bool)obj;
                        listProperties.Remove("EnableModeration");
                    }
                    if (listProperties.TryGetValue("EnableVersioning", out obj))
                    {
                        list.EnableVersioning = (bool)obj;
                        listProperties.Remove("EnableVersioning");
                    }
                    if (listProperties.TryGetValue("EnableMinorVersions", out obj))
                    {
                        list.EnableMinorVersions = (bool)obj;
                        listProperties.Remove("EnableMinorVersions");
                    }
                    AveObjectCopy.UpdateObjectBasicProperties(listProperties, list);
                    Dictionary<string, object> newProp = new Dictionary<string, object>();
                    UpdateListUserResource(list, listProperties);
                    list.Update();
                    this.LoadList(context, list);
                    AveObjectCopy.GetObjectBasicProperties(newProp, list);
                    if (advancedSettingProp.Count > 0)
                    {
                        mRequestCommon.UpdateListAdvancedSetting(webServerRelativeUrl, listId, advancedSettingProp);
                    }
                    if (generalSettings.Count > 0)
                    {
                        mRequestCommon.UpdateListGeneralSetting(webServerRelativeUrl, listId, generalSettings);
                    }
                    return newProp;
                }
            }
        }

        [NoAPI]
        [TODO("没有API直接获取到。需要看下是否可以通过RootFolder的property来获取。")]
        public override Dictionary<string, object> GetListGeneralProperties(string webServerRelativeUrl, Guid listId)
        {
            return base.GetListGeneralProperties(webServerRelativeUrl, listId);
        }
        [NoAPI]
        public override Dictionary<string, object> GetListEditViewSettingProperties(string webServerRelativeUrl, string listTitle, Guid listId, Guid viewId)
        {
            return base.GetListEditViewSettingProperties(webServerRelativeUrl, listTitle, listId, viewId);
        }
        [NoAPI("目前API不支持List的DefaultItemOpen，SendToLocationName，SendToLocationUrl，NavigateForFormsPages，EnableManagedIndexes属性")]
        public override Dictionary<string, object> GetListAdvancedSettingProperties(string webServerRelativeUrl, Guid listId)
        {
            var resultFromWebRequest = base.GetListAdvancedSettingProperties(webServerRelativeUrl, listId);
            //using (var clientContext = CreateContext())
            //{
            //    var list = clientContext.Site.OpenWeb(webServerRelativeUrl).GetListById(listId);
            //    clientContext.Load(list, 
            //        l => l.DefaultItemOpenUseListSetting,
            //        l => l.EnableAssignToEmail,
            //        l => l.ExcludeFromOfflineClient,
            //        l => l.DisableGridEditing,
            //        l => l.ReadSecurity,
            //        l => l.WriteSecurity);
            //    clientContext.ExecuteQuery();
            //    resultFromWebRequest["DefaultItemOpenUseListSetting"] = list.DefaultItemOpenUseListSetting;
            //    resultFromWebRequest["EnableAssignToEmail"] = list.EnableAssignToEmail;
            //    resultFromWebRequest["ExcludeFromOfflineClient"] = list.ExcludeFromOfflineClient;
            //    resultFromWebRequest["DisableGridEditing"] = list.DisableGridEditing;
            //    resultFromWebRequest["ReadSecurity"] = list.ReadSecurity;
            //    resultFromWebRequest["WriteSecurity"] = list.WriteSecurity;
            //}
            return resultFromWebRequest;
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetListByTitle(Guid webId, string listTitle)
        {
            return base.GetListByTitle(webId, listTitle);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetListChangesByQuery(string webServerRelativeUrl, Guid listId, Dictionary<string, object> queryProps)
        {
            return base.GetListChangesByQuery(webServerRelativeUrl, listId, queryProps);
        }

        [NoAPI]
        public override bool SetListRating(string webServerRelativeUrl, string listUrl, Guid listId, bool enableRating)
        {
            return base.SetListRating(webServerRelativeUrl, listUrl, listId, enableRating);
        }
        [NoAPI]
        public override void SetPerLocalViewSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> viewSettingProp)
        {
            base.SetPerLocalViewSetting(webServerRelativeUrl, listId, viewSettingProp);
        }
        [KeepOriginalWithAPI]
        public override string GetListSchemalXml(string ParentWebUrl, Guid Id, string listTitle)
        {
            return base.GetListSchemalXml(ParentWebUrl, Id, listTitle);
        }
        [KeepOriginalWithAPI]
        public override Guid RecycleList(string webRelativeUrl, string listTitle, Guid listId)
        {
            return base.RecycleList(webRelativeUrl, listTitle, listId);
        }

        [TODO("可以通过更新rootfolder的properties来更新，但是当前没有找到list.EnableSyndication 对应的属性")]
        public override void UpdateListRssSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProp)
        {
            base.UpdateListRssSetting(webServerRelativeUrl, listId, updateProp);
        }
        [ReplaceByAPI]
        public override Dictionary<string, object> GetListItemComplianceTag(Guid webID, Guid listID, int rowID)
        {
            try
            {
                using (var context = CreateContext())
                {
                    var web = context.Site.OpenWebById(webID);
                    var list = web.Lists.GetById(listID);
                    var item = list.GetItemById(rowID);
                    context.Load(item);
                    context.Load(item, i => i.ComplianceInfo);
                    context.LoadQuery(web.SiteUsers.IncludeWithDefaultProperties(u => u.LoginName, u => u.Id));
                    context.ExecuteQuery();
                    // 如果ComplianceTag表示item没有设置label
                    if (string.IsNullOrEmpty(item.ComplianceInfo.ComplianceTag))
                    {
                        return new Dictionary<string, object>();
                    }
                    return AssembleComplianceTagInfo(item);
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn(string.Format("Failed to get the compliance info from list item, webID: {0}, listID: {1}, rowID:{2}. Exception: {3}", webID, listID, rowID, ex));
            }
            return new Dictionary<string, object>();
        }


        [ReplaceByAPI]
        public override AveComplianceTagInfo UpdateListComplianceTagProperties(string webServerRelativeUrl, string listServerRelativeUrl, AveComplianceTagInfo properties)
        {
            AveComplianceTagInfo complianceTagInfo = new AveComplianceTagInfo();
            try
            {
                using (AveClientContext context = CreateContext(this.WebAppName + webServerRelativeUrl))
                {
                    Microsoft.SharePoint.Client.CompliancePolicy.SPPolicyStoreProxy.SetListComplianceTag(
                    context,
                    listServerRelativeUrl,
                    properties.ComplianceTagValue,
                    properties.BlockEdit,
                    properties.BlockDelete,
                    false);
                    context.ExecuteQuery();
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn(string.Format("Failed to set compliance tag setting for the list. Url: {0}, exception: {1}", listServerRelativeUrl, ex));
                return null;
            }
            return complianceTagInfo;
        }


        [ReplaceByAPI]
        public override AveComplianceTagInfo GetListComplianceTagProperties(string webServerRelativeUrl, string listServerRelativeUrl)
        {
            var complianceTag = new AveComplianceTagInfo();
            try
            {
                using (AveClientContext context = CreateContext(this.WebAppName + webServerRelativeUrl))
                {
                    var listComplianceTagProperties = Microsoft.SharePoint.Client.CompliancePolicy.SPPolicyStoreProxy.GetListComplianceTag(context, listServerRelativeUrl);
                    context.ExecuteQuery();
                    if (listComplianceTagProperties != null)
                    {
                        if (listComplianceTagProperties.Value == null)
                        {
                            complianceTag.ComplianceTagValue = string.Empty;
                            complianceTag.BlockEdit = false;
                            complianceTag.BlockDelete = false;
                        }
                        else
                        {
                            complianceTag.ComplianceTagValue = listComplianceTagProperties.Value.TagName;
                            complianceTag.BlockEdit = listComplianceTagProperties.Value.BlockEdit;
                            complianceTag.BlockDelete = listComplianceTagProperties.Value.BlockDelete;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn("Failed to get the compliance tag properties. Info error, Url : {0}  Error : {1}", listServerRelativeUrl, ex.ToString());
                return null;
            }
            return complianceTag;
        }



        [ReplaceByAPI]
        public override Dictionary<string, string> GetListUserResource(string webServerRelativeUrl, Guid id, string resourceName, List<string> cultureNames)
        {
            using (AveClientContext context = CreateContext())
            {
                UserResource resource;
                Dictionary<string, ClientResult<string>> values = new Dictionary<string, ClientResult<string>>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                var list = web.Lists.GetById(id);
                switch (resourceName)
                {
                    case AveUserResourceConstants.TITLE_RESOUCE:
                        resource = list.TitleResource;
                        break;
                    case AveUserResourceConstants.DESCRIPTION_RESOUCE:
                        resource = list.DescriptionResource;
                        break;
                    default:
                        throw new NotSupportedException(string.Format("The resource {0} is not supported.", resourceName));
                }
                foreach (string cultureName in cultureNames)
                {
                    values.Add(cultureName, resource.GetValueForUICulture(cultureName));
                }
                context.ExecuteQuery();
                return values.ToDictionary(k => k.Key, v => v.Value.Value);
            }
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> UpdateListInformationRightsManagementSettings(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProperties)
        {
            return base.UpdateListInformationRightsManagementSettings(webServerRelativeUrl, listId, updateProperties);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> ResetListInformationRightsManagementSettings(string webServerRelativeUrl, Guid listId)
        {
            return base.ResetListInformationRightsManagementSettings(webServerRelativeUrl, listId);

        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetListInformationRightsManagementSettings(string webServerRelativeUrl, Guid listId)
        {
            return base.GetListInformationRightsManagementSettings(webServerRelativeUrl, listId);
        }

        [NoAPI]
        public override bool SetListRateSetting(string webServerRelativeUrl, string listUrl, Guid listId, bool enableRating, string experience)
        {
            return base.SetListRateSetting(webServerRelativeUrl, listUrl, listId, enableRating, experience);
        }

        [NoAPI]
        public override string GetListExperience(string webServerRelativeUrl, Guid guid)
        {
            return base.GetListExperience(webServerRelativeUrl, guid);
        }
    }
}
