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
using System.Reflection;
using System.IO;
using System.Collections.Generic;

using AvePoint.GCommon;
using AvePoint.AveO365LightWeightRequest;
using AvePoint.Wrapper.Common;
using System.Linq;

namespace AvePoint.Wrapper.BackupRestore
{
    //负责和Request通信
    //包含部分业务逻辑，转换对应的Info，CheckVersion， Filter Policy等
    internal class AveOD4BRequestController
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly AveLightWeightRequest mRequest;
        private readonly GlobalCache mGlobalCache;

        public AveOD4BRequestController(RequestConfig config)
        {
            mRequest = new AveLightWeightRequest(config);
            mGlobalCache = new GlobalCache();
        }

        internal GlobalCache GlobalCache
        {
            get
            {
                return this.mGlobalCache;
            }
        }

        public void EnsureSiteAdmin(string adminSiteUrl, string siteUrl, string userName)
        {
            mRequest.EnsureSiteAdmin(adminSiteUrl, siteUrl, userName);
        }

        public AveBRSiteInfo GetOD4BSiteInfo(string siteUrl)
        {
            //return new AveBRSiteInfo() { Id = Guid.NewGuid() };
            AveLWSiteParameter par = new AveLWSiteParameter(SiteDetails.Basic);
            par.Default = false;
            par.Values = new List<string> { "Id", "Url", "ServerRelativeUrl", "RootWeb.WebTemplate", "RootWeb.Configuration", "RootWeb.Language" };
            var request = this.mRequest.LoadSiteBasicInfo(siteUrl, par);
            var props = ConvertToDictionary(request);
            props["WebAppUrl"] = props["Url"];
            props["WebTemplate"] = props["WebTemplate"] + "#" + props["Configuration"];
            return InfoConverter<AveBRSiteInfo>.ConvertInfo(props);
        }

        public Dictionary<string, object> BatchGetOD4BWebInfo(string webUrl)
        {
            AveLWWebBatchParameter batchPar = new AveLWWebBatchParameter();
            List<WebDetails> webDetails = new List<WebDetails>() { WebDetails.Basic, WebDetails.Group, WebDetails.RoleDefinition };
            webDetails.ForEach(detail =>
            {
                AveLWWebParameter par = new AveLWWebParameter(detail);
                par.Default = false;
                switch (detail)
                {
                    case WebDetails.Basic:
                        par.Values = new List<string> { "Description", "Title", "Url", "Configuration",
                            "ServerRelativeUrl", "Id", "WebTemplate", "Language", "HasUniqueRoleAssignments"};
                        break;
                    case WebDetails.RoleDefinition:
                        par.Values = new List<string> { "Id", "Description", "Name", "Order", "BasePermissions" };
                        break;
                    case WebDetails.Group:
                        par.Values = new List<string> { "Id", "Description", "Title",
                            "AllowMembersEditMembership", "AllowRequestToJoinLeave",
                            "AutoAcceptRequestToJoinLeave", "OnlyAllowMembersViewMembership" };
                        break;
                }
                if (batchPar.Parameters == null)
                {
                    batchPar.Parameters = new List<AveLWWebParameter>();
                }
                batchPar.Parameters.Add(par);
            });

            var batchRequest = this.mRequest.BatchGetWebInfo(webUrl, batchPar);

            Dictionary<string, object> result = new Dictionary<string, object>();
            webDetails.ForEach(detail =>
            {
                var request = batchRequest.GetNextResult();
                if (request != null)
                {
                    string web = "Web";
                    switch (detail)
                    {
                        case WebDetails.Basic:
                            var props = ConvertToDictionary(request);
                            props["WebTemplate"] = props["WebTemplate"] + "#" + props["Configuration"];
                            result[web + detail.ToString()] = InfoConverter<AveBRWebInfo>.ConvertInfo(props);
                            break;
                        case WebDetails.RoleDefinition:
                            List<AveBRRoleDefinitionInfo> webRoleDefinitionsInfo = new List<AveBRRoleDefinitionInfo>();
                            ConvertToList(request).ForEach(roleDefinition =>
                            {
                                roleDefinition["BasePermissions"] = ConvertBasePermToULong(roleDefinition["BasePermissions"]);
                                webRoleDefinitionsInfo.Add(InfoConverter<AveBRRoleDefinitionInfo>.ConvertInfo(roleDefinition));
                            });
                            result[web + detail.ToString()] = webRoleDefinitionsInfo;
                            break;
                        case WebDetails.Group:
                            List<AveBRGroupInfo> webGroupsInfo = new List<AveBRGroupInfo>();
                            ConvertToList(request).ForEach(group => webGroupsInfo.Add(InfoConverter<AveBRGroupInfo>.ConvertInfo(group)));
                            result[web + detail.ToString()] = webGroupsInfo;
                            break;
                    }
                }
            });
            return result;
        }

        public List<string> GetListEditableFields(string siteUrl, string listUrl)
        {
            return this.mRequest.GetListEditableFields(siteUrl, listUrl);
        }

        //[Filter]
        public List<AveBRFolderInfo> GetAllFoldersInList(string siteUrl, string listUrl, List<string> columns, bool largeList)
        {
            List<AveBRFolderInfo> folders = new List<AveBRFolderInfo>();
            var result = this.mRequest.GetAllFoldersInList(siteUrl, listUrl, columns, largeList);
            Dictionary<string, object> prop = new Dictionary<string, object>();
            while (result.TryGetNextValue(out prop))
            {
                folders.Add(InfoConverter<AveBRFolderInfo>.ConvertInfo(prop));
            }

            return folders;
        }

        //return value
        public Dictionary<string, object> BatchGetOD4BListInfo(string webUrl, string listUrl)
        {
            AveLWListBatchParameter batchPar = new AveLWListBatchParameter();
            AveLWListParameter basic = new AveLWListParameter(ListDetails.Basic);
            basic.Default = false;
            basic.Values = new List<string> { "Id", "Title", "BaseTemplate", "TemplateFeatureId",
                "BaseType", "Description", "ItemCount",
                "EnableVersioning", "EnableMinorVersions", "EnableAttachments",
                "EnableFolderCreation", "EnableModeration", "ForceCheckout",
                "OnQuickLaunch", "HasUniqueRoleAssignments" , "RootFolder.ServerRelativeUrl", "RootFolder.ItemCount",
                "IrmEnabled", "IrmReject", "IrmExpire", "InformationRightsManagementSettings"};
            if (batchPar.Parameters == null)
            {
                batchPar.Parameters = new List<AveLWListParameter>();
            }
            batchPar.Parameters.Add(basic);

            var batchRequest = this.mRequest.BatchGetListInfo(webUrl, listUrl, batchPar);

            var request = batchRequest[ListDetails.Basic.ToString()];
            Dictionary<string, object> result = new Dictionary<string, object>();
            if (request == null)
            {
                return null;
            }

            result["List" + basic.Detail.ToString()] = InfoConverter<AveBRListInfo>.ConvertInfo(ConvertToDictionary(request));
            return result;
        }

        public List<AveBRItemInfo> GetItemsInfo(string webUrl, string folderUrl, List<string> columns)
        {
            List<AveBRItemInfo> infos = new List<AveBRItemInfo>();
            return infos;
        }

        public Stream GetFileContent(string webUrl, string fileServerRelativeUrl, Guid uniqueId, int uiVersion)
        {
            AveLWFileParameter para = new AveLWFileParameter(FileDetails.Basic)
            {
                WebUrl = webUrl,
                FileServerRelativeUrl = fileServerRelativeUrl,
                UniqueId = uniqueId,
            };
            Stream fileContent = null;
            if (uiVersion > 0)
            {
                para.UIVersion = uiVersion;
                fileContent = this.mRequest.GetLargeFileVersionContent(para);
            }
            else
            {
                fileContent = this.mRequest.GetLargeFileContent(para);
            }

            if (fileContent == null)
            {
                throw new FileNotFoundException("Cannot find the specified file. It may be deleted, please check in browser.", fileServerRelativeUrl);
            }

            return fileContent;
        }

        public IEnumerable<AveBRItemInfo> QueryLazyFilesV1(string webUrl, string listUrl, string parentFolderUrl, bool includeVersions, bool includeVersionMetadata, bool includeSecurity, List<string> columns)
        {
            FileDetails details = FileDetails.Basic | FileDetails.ColumnValue;
            if (includeVersions)
            {
                details = details | FileDetails.Version;
            }
            if (includeVersionMetadata)
            {
                details = details | FileDetails.VersionMetadata;
            }
            if (includeSecurity)
            {
                details = details | FileDetails.RoleAssignment;
            }
            AveLWFilesParameter par = new AveLWFilesParameter(details);
            par.ColumnNames = columns;
            par.Default = false;
            par.WebUrl = webUrl;
            par.ListServerRelativeUrl = listUrl;
            par.ParentFolderServerRelativeUrl = parentFolderUrl;

            var lazyItems = this.mRequest.GetLazyFiles(webUrl, listUrl, parentFolderUrl, includeVersions, includeVersionMetadata, includeSecurity, columns);
            foreach (var lazyItem in lazyItems)
            {
                Dictionary<string, object> prop = null;
                while (lazyItem.TryGetNextValue(out prop))
                {
                    if (prop.ContainsKey("Item_Exception"))
                    {
                        AveBRItemInfo failedItem = new AveBRItemInfo();
                        failedItem.ServerRelativeUrl = parentFolderUrl.TrimEnd('/') + "/";
                        failedItem.Result.SetFailed(prop["ChangeItem_Exception"] as Exception);
                        yield return failedItem;
                    }
                    else
                    {
                        yield return ConvertToItemInfo(prop, parentFolderUrl);
                    }
                }
            }
        }

        public IEnumerable<AveBRItemInfo> QueryLazyFiles(string webUrl, string listUrl, string parentFolderUrl, bool includeVersions, bool includeVersionMetadata, bool includeSecurity, List<string> columns)
        {
            var lazyItems = this.mRequest.GetLazyFiles(webUrl, listUrl, parentFolderUrl, includeVersions, includeVersionMetadata, includeSecurity, columns);
            foreach (var lazyItem in lazyItems)
            {
                Dictionary<string, object> prop = null;
                while (lazyItem.TryGetNextValue(out prop))
                {
                    if (prop.ContainsKey("Item_Exception"))
                    {
                        AveBRItemInfo failedItem = new AveBRItemInfo();
                        failedItem.ServerRelativeUrl = parentFolderUrl.TrimEnd('/') + "/";
                        failedItem.Result.SetFailed(prop["ChangeItem_Exception"] as Exception);
                        yield return failedItem;
                    }
                    else
                    {
                        yield return ConvertToItemInfo(prop, parentFolderUrl);
                    }
                }
            }
        }

        protected static AveBRItemInfo ConvertToItemInfo(Dictionary<string, object> properties, string parentFolderUrl)
        {
            AveBRItemInfo info = new AveBRItemInfo();
            try
            {
                var versions = (List<Dictionary<string, object>>)properties["Versions"];

                InfoConverter<AveBRItemInfo>.ConvertInfo(info, properties);
                info.IsCurrent = true;
                info.BiggestVersionModified = info.Modified;
                object columnValuesObj;
                if (properties.TryGetValue("ColumnValues", out columnValuesObj))
                {
                    Dictionary<string, object> columnValues = (Dictionary<string, object>)columnValuesObj;

                    info.CustomColumns = columnValues;
                }

                if (versions != null && versions.Count > 0)
                {
                    info.Versions = new List<AveBRItemInfo>(versions.Count);
                    foreach (var version in versions)
                    {
                        var versionInfo = InfoConverter<AveBRItemInfo>.ConvertInfo(version);
                        versionInfo.Name = info.Name;
                        versionInfo.ServerRelativeUrl = info.ServerRelativeUrl;
                        versionInfo.Id = info.Id;
                        versionInfo.UniqueId = info.UniqueId;
                        versionInfo.IsCurrent = false;
                        info.Versions.Add(versionInfo);
                        versionInfo.BiggestVersionModified = info.Modified;
                    }
                }
                info.RoleAssignments = new List<AveBRRoleAssignmentInfo>();
                if (info.HasUniqueRoleAssignments)
                {
                    var securityObjs = (List<Dictionary<string, object>>)properties["Security"];
                    foreach (var obj in securityObjs)
                    {
                        var role = InfoConverter<AveBRRoleAssignmentInfo>.ConvertInfo(obj);
                        info.RoleAssignments.Add(role);
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Error("Convert properties to item info under folder:{0} failed:{1}", parentFolderUrl, ex);
                info.Result.SetFailed(ex);
            }

            return info;
        }

        public List<AveBRUserInfo> GetOD4WebUserInfo(string siteUrl)
        {
            AveLWWebParameter par = new AveLWWebParameter(WebDetails.User);
            par.Default = false;
            par.Values = new List<string> { "Id", "Title", "LoginName", "IsSiteAdmin", "PrincipalType" };
            var request = this.mRequest.GetWebInfo(siteUrl, par);
            if (request == null)
            {
                return null;
            }
            List<Dictionary<string, object>> result = ConvertToList(request);
            List<AveBRUserInfo> webUserInfo = new List<AveBRUserInfo>();
            result.ForEach(user =>
            {
                user["IsDomainGroup"] = (int)user["PrincipalType"] == 4;
                webUserInfo.Add(InfoConverter<AveBRUserInfo>.ConvertInfo(user));
            });
            return webUserInfo;
        }

        public List<AveBRGroupInfo> GetOD4WebGroupInfo(string siteUrl)
        {
            AveLWWebParameter par = new AveLWWebParameter(WebDetails.Group);
            par.Default = false;
            par.Values = new List<string> { "Id", "Description", "Title",
                "AllowMembersEditMembership", "AllowRequestToJoinLeave",
                "AutoAcceptRequestToJoinLeave", "OnlyAllowMembersViewMembership" };
            var request = this.mRequest.GetWebInfo(siteUrl, par);
            if (request == null)
            {
                return null;
            }
            List<Dictionary<string, object>> result = ConvertToList(request);
            List<AveBRGroupInfo> webGroupInfo = new List<AveBRGroupInfo>();
            result.ForEach(group => webGroupInfo.Add(InfoConverter<AveBRGroupInfo>.ConvertInfo(group)));
            return webGroupInfo;
        }

        public AveBRWebInfo GetOD4WebBasicInfo(string siteUrl)
        {
            AveLWWebParameter par = new AveLWWebParameter(WebDetails.Basic);
            par.Default = false;
            par.Values = new List<string> { "Description", "Title", "Url", "Id",
                "WebTemplate", "Language", "HasUniqueRoleAssignments" };
            var request = this.mRequest.GetWebInfo(siteUrl, par);
            if (request == null)
            {
                return null;
            }
            return InfoConverter<AveBRWebInfo>.ConvertInfo(ConvertToDictionary(request));
        }

        public List<AveBRRoleDefinitionInfo> GetOD4BWebRoleDefinitionInfo(string siteUrl)
        {
            AveLWWebParameter par = new AveLWWebParameter(WebDetails.RoleDefinition);
            par.Default = false;
            par.Values = new List<string> { "Id", "Description", "Name", "Order", "BasePermissions" };
            var request = this.mRequest.GetWebInfo(siteUrl, par);
            if (request == null)
            {
                return null;
            }
            List<Dictionary<string, object>> result = ConvertToList(request);
            List<AveBRRoleDefinitionInfo> webRoleDefinitionInfo = new List<AveBRRoleDefinitionInfo>();
            result.ForEach(roleDefinition =>
            {
                roleDefinition["BasePermissions"] = ConvertBasePermToULong(roleDefinition["BasePermissions"]);
                webRoleDefinitionInfo.Add(InfoConverter<AveBRRoleDefinitionInfo>.ConvertInfo(roleDefinition));
            });
            return webRoleDefinitionInfo;
        }

        public List<AveBRRoleAssignmentInfo> GetOD4BWebRoleAssignmentInfo(string siteUrl)
        {
            AveLWWebParameter par = new AveLWWebParameter(WebDetails.RoleAssignment);
            par.Default = false;
            //par.Values = new List<string>();
            var request = this.mRequest.GetWebInfo(siteUrl, par);
            if (request == null)
            {
                return null;
            }
            List<Dictionary<string, object>> result = ConvertToList(request);
            List<AveBRRoleAssignmentInfo> webRoleAssignmentInfo = new List<AveBRRoleAssignmentInfo>();
            result.ForEach(roleAssignment =>
            {
                webRoleAssignmentInfo.Add(InfoConverter<AveBRRoleAssignmentInfo>.ConvertInfo(roleAssignment));
            });
            return webRoleAssignmentInfo;
        }

        public List<AveBRRoleAssignmentInfo> GetListRoleAssignmentsInfo(string siteUrl, string listUrl)
        {
            AveLWListParameter par = new AveLWListParameter(ListDetails.RoleAssignment);
            par.Default = false;
            //par.Values = new List<string>();
            var request = this.mRequest.GetListInfo(siteUrl, listUrl, par);
            if (request == null)
            {
                return null;
            }
            List<Dictionary<string, object>> result = ConvertToList(request);
            List<AveBRRoleAssignmentInfo> listRoleAssignmentInfo = new List<AveBRRoleAssignmentInfo>();
            result.ForEach(roleAssignment =>
            {
                listRoleAssignmentInfo.Add(InfoConverter<AveBRRoleAssignmentInfo>.ConvertInfo(roleAssignment));
            });
            return listRoleAssignmentInfo;
        }

        public List<AveBRRoleAssignmentInfo> GetFolderRoleAssignmentsInfo(string siteUrl, string serverRelativeUrl)
        {
            AveLWFolderParameter par = new AveLWFolderParameter(FolderDetails.RoleAssignment);
            par.Default = false;
            var request = this.mRequest.GetFolderInfo(siteUrl, serverRelativeUrl, par);
            if (request == null)
            {
                return null;
            }
            List<Dictionary<string, object>> result = ConvertToList(request);
            List<AveBRRoleAssignmentInfo> folderRoleAssignmentInfo = new List<AveBRRoleAssignmentInfo>();
            result.ForEach(roleAssignment =>
            {
                folderRoleAssignmentInfo.Add(InfoConverter<AveBRRoleAssignmentInfo>.ConvertInfo(roleAssignment));
            });
            return folderRoleAssignmentInfo;
        }

        public List<AveBRChangeObject> GetListChangedItems(string webUrl, string listUrl, Guid listId, DateTime startTime, DateTime endTime, bool includeVersions, bool includeVersionMetadata, bool includeSecurity, bool includeSystemUpdate, List<string> columns, Dictionary<string, int> failItems)
        {
            var result = this.mRequest.GetListChangedItems(webUrl, listUrl, listId, startTime, endTime, includeVersions, includeVersionMetadata, includeSecurity, includeSystemUpdate, columns);
            List<AveBRChangeObject> changedItems = new List<AveBRChangeObject>();
            changedItems = AssembleItem(result, listUrl, true, failItems);

            if (failItems != null && failItems.Count > 0)
            {
                result = this.mRequest.GetFailedItems(webUrl, includeVersions, includeVersionMetadata, includeSecurity, columns, failItems.Keys.ToList());
                changedItems.AddRange(AssembleItem(result, listUrl, false, failItems));
            }

            return changedItems;
        }

        private List<AveBRChangeObject> AssembleItem(RequestResult result, string listUrl, bool needRemoveFailedItem, Dictionary<string, int> failItems)
        {
            List<AveBRChangeObject> changedItems = new List<AveBRChangeObject>();
            Dictionary<string, object> prop = null;
            while (result.TryGetNextValue(out prop))
            {
                var changeItem = InfoConverter<AveBRChangeObject>.ConvertInfo(prop);
                if (changeItem.ChangeType == 3) // 过滤掉delete object
                {
                    continue;
                }

                if (changeItem.Exception == null)
                {
                    var itemInfo = ConvertToItemInfo(prop, listUrl);
                    changeItem.ServerRelativeUrl = itemInfo.ServerRelativeUrl == null ? string.Empty : itemInfo.ServerRelativeUrl;
                    changeItem.ItemType = itemInfo.ItemType;

                    if (failItems != null)
                    {
                        if (needRemoveFailedItem)
                        {
                            failItems.Remove(changeItem.ServerRelativeUrl);
                        }
                        else
                        {
                            itemInfo.FailedCount = failItems[changeItem.ServerRelativeUrl];
                            if (itemInfo.Versions != null)
                            {
                                foreach (var version in itemInfo.Versions)
                                {
                                    version.FailedCount = failItems[changeItem.ServerRelativeUrl];
                                }
                            }
                        }
                    }

                    changeItem.ItemProps["Item"] = itemInfo;
                }
                else
                {
                    AveBRItemInfo failedItem = new AveBRItemInfo();
                    failedItem.Result.SetFailed(changeItem.Exception);
                    changeItem.ItemProps["Item"] = failedItem;
                }
                changedItems.Add(changeItem);
            }
            return changedItems;
        }

        public void InformationRightsManagementSettingsReset(string siteUrl, string listUrl)
        {
            this.mRequest.InformationRightsManagementSettingsReset(siteUrl, listUrl);
        }

        public void InformationRightsManagementSettingsUpdate(string siteUrl, string listUrl, Dictionary<string, object> informationRightsManagementDic)
        {
            this.mRequest.InformationRightsManagementSettingsUpdate(siteUrl, listUrl, informationRightsManagementDic);
        }

        private Dictionary<string, object> ConvertToDictionary(RequestResult result)
        {
            Dictionary<string, object> request = null;

            if (!result.TryGetNextValue(out request))
            {
                return new Dictionary<string, object>();
            }
            return request;
        }

        private List<Dictionary<string, object>> ConvertToList(RequestResult result)
        {
            List<Dictionary<string, object>> request = new List<Dictionary<string, object>>();

            Dictionary<string, object> value = null;
            while (result.TryGetNextValue(out value))
            {
                request.Add(value);
            }
            return request;
        }

        private ulong ConvertBasePermToULong(object basePerm)
        {
            if (basePerm == null)
            {
                return 0;
            }
            uint high = (uint)AveReflectionUtility.GetFieldValue("m_high", basePerm);
            uint low = (uint)AveReflectionUtility.GetFieldValue("m_low", basePerm);
            return ((ulong)high << 32) | low;
        }
    }
}
