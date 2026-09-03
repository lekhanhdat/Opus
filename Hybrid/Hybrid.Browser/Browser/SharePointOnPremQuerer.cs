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
using AvePoint.Hybrid.Contract.SignalR;
using AvePoint.RA.Common.Global;
using AvePoint.RA.Common.Global.Util;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Hybrid.Browser.Contract;
using AvePoint.Wrapper.Common;
using log4net;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;

namespace AvePoint.RA.Hybrid.Browser.Browser
{
    public class SharePointOnPremQuerer : IBrowser
    {
        private static readonly GCommon.AveLogger Logger = GCommon.AveLogger.GetInstance(typeof(SharePointOnPremQuerer));

        public HybridBrowserType BrowserType => HybridBrowserType.SharePointOnPremQuerer;

        private const string relatedColumnInternalName = "RecordsRelated";

        private static string key = "3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E";

        public string Browse(string message)
        {
            var args = SerializerHelper.DeserializeByJsonSerializer<SharePointOnPremQuererArgs>(message);
            var factory = AveObjectModelFactory.CreateObjectModelFactory(null, null, AveContextKind.ServerObjectModel);
            var result = new SharePointOnPremQuererResult();
            try
            {
                using (var site = factory.CreateSite(args.SiteUrl))
                {
                    var web = site.AllWebs[args.WebId];
                    using (web)
                    {
                        var webServerRelativeUrl = web.ServerRelativeUrl;
                        var webUrl = web.Url;
                        var list = web.GetList(args.ListId);
                        var listItem = list.GetItemByUniqueId(args.ItemId);

                        result.Id = listItem.ID;
                        result.UniqueId = listItem.UniqueId;
                        result.Name = listItem.Name;
                        result.WebUrl = webUrl;
                        result.ListId = list.ID;
                        result.ListId = listItem.ParentList.ID;
                        result.WebId = listItem.ParentList.ParentWeb.ID;
                        result.SiteId = listItem.ParentList.ParentWeb.Site.ID;
                        result.SiteUrl = listItem.ParentList.ParentWeb.Site.Url;
                        result.WebServerRelativeUrl = listItem.ParentList.ParentWeb.ServerRelativeUrl;
                        result.ListUrl = listItem.ParentList.RootFolder.ServerRelativeUrl;
                        result.Level = listItem.ParentList.BaseType == AveBaseType.GenericList ? 5 : 6;

                        if (listItem.FieldValues.TryGetValue("FileRef", out object value4FileRef))
                        {
                            string fileRef = value4FileRef.ToString();
                            var fullPath = WebUtil.MakeFullUrl(result.SiteUrl, fileRef);
                            if (fullPath.EndsWith("_.000", StringComparison.OrdinalIgnoreCase))
                            {
                                result.FullPath = GetListItemRealPath(fullPath);
                            }
                        }

                        if ((listItem.FieldValues["FSObjType"] as string).Equals("0"))
                        {
                            if ((listItem.FieldValues["FileLeafRef"] as string).EndsWith("_.000", StringComparison.OrdinalIgnoreCase))
                            {
                                result.Name = listItem.FieldValues["Title"] as string;
                                //if (string.IsNullOrEmpty(result.Name))
                                //{
                                //    result.Name = GetSpecialListItemName(listItem);
                                //}
                            }
                            else
                            {
                                result.Name = listItem.FieldValues["FileLeafRef"].ToString();
                            }
                        }
                        else
                        {
                            result.Name = listItem.FieldValues["FileLeafRef"].ToString();
                        }

                        if (listItem.FieldValues.TryGetValue(SPColumnConstants.DocumentId, out object value4DocumentId))
                        {
                            result.RecordId = value4DocumentId?.ToString();
                        }
                        else if (listItem.FieldValues.ContainsKey(RcordsBuiltInColumn.UNIQUEID_NAME))
                        {
                            result.RecordId = listItem.FieldValues[RcordsBuiltInColumn.UNIQUEID_NAME]?.ToString();
                        }

                        if (listItem.FieldValues.TryGetValue("FileDirRef", out object value))
                        {
                            string fileDirRef = value.ToString();
                            var parentFolder = web.GetFolder(fileDirRef);
                            string relatedUrl = fileDirRef.Substring(webServerRelativeUrl.TrimEnd('/').Length + 1);
                            result.Url = webUrl + "/" + relatedUrl;
                            result.ItemUrl = fileDirRef;
                            result.FolderId = parentFolder.UniqueId;
                            result.FolderUrl = parentFolder.ServerRelativeUrl;
                            result.ParentFolderIsRootFolder = listItem.ParentList.RootFolder.UniqueId.Equals(parentFolder.UniqueId);
                        }
                        var sourceUrlValue = listItem[relatedColumnInternalName] != null ? listItem[relatedColumnInternalName].ToString() : string.Empty;
                        result.RelatedRecordsInfo = sourceUrlValue;
                        result.DeclareAsRecord = IsBlockEditAndDeleteRecord(listItem);
                        result.IsRecord = CheckIsRecord(listItem);

                        try
                        {
                            var columnInternalName = RcordsBuiltInColumn.ITEM_BCS_NAME;
                            if (args.IsUsingExistColumnName)
                            {
                                var collection = listItem.Fields;
                                var tempField = collection.Where(f => f.Title == args.ExistColumnName).FirstOrDefault();
                                tempField = collection.Where(f => f.InternalName == args.ExistColumnName).FirstOrDefault();
                                if (tempField == null)
                                {
                                    string staticName = GetSiteLevelExistColumnStaticName(site, args.ExistColumnName);
                                    tempField = collection.Where(f => f.StaticName == staticName).FirstOrDefault();
                                }
                                if (tempField == null)
                                {
                                    Logger.Warn($"[RelatedApp] Can not get column by name. site: {args.SiteUrl}, exist colum name: {args.ExistColumnName}");
                                }
                                else
                                {
                                    columnInternalName = tempField.InternalName;
                                }
                            }
                            

                            if (listItem.FieldValues.TryGetValue(columnInternalName, out object termTempValue))
                            {
                                var termString = termTempValue?.ToString();
                                var termId = termString?.Split('|')?.LastOrDefault();
                                result.TermId = termId;
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Warn($"Error occurred while gettting related item term, error: {e}");
                        }

                        return SerializerHelper.SerializeByJsonConvert(result);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"An error occur while query sharepoint item info. Error: {e}");
                return string.Empty;
            }
        }

        public string GetSiteLevelExistColumnStaticName(IAveSite site, string columnName)
        {
            var collection = site.RootWeb.Fields;
            var tempField = collection.Where(f => f.Title == columnName).FirstOrDefault();
            tempField = collection.Where(f => f.InternalName == columnName).FirstOrDefault();
            if (tempField == null)
            {
                Logger.Warn($"[GetSiteLevelExistColumnStaticName] Can not get column by name.");
                return columnName;
            }
            else
            {
                Logger.Info($"[GetSiteLevelExistColumnStaticName] Configuration ColumnName:{columnName}, Title:{tempField.Title}, InternalName: {tempField.InternalName}, StaticName: {tempField.StaticName}");
                return tempField.StaticName;
            }
        }

        public string GetListItemRealPath(string itemUrl)
        {
            if (string.IsNullOrEmpty(itemUrl))
            {
                throw new ArgumentNullException("itemUrl");
            }
            if (itemUrl.StartsWith("http:") || itemUrl.StartsWith("https:"))
            {
                var splitArrary = itemUrl.Split(new string[] { "/Lists/" }, StringSplitOptions.None);
                if (splitArrary.Length > 1)
                {
                    var webUrl = splitArrary[0];
                    var listName = splitArrary[1].Split('/')[0];
                    var itemName = itemUrl.Substring(itemUrl.LastIndexOf("/") + 1).Split('_')[0];
                    //eg:https://m365b310744.sharepoint.com/sites/yySite01/Lists/list1/f3/f31/13_.000
                    return $"{webUrl}/Lists/{listName}/DispForm.aspx?ID={itemName}";
                }

            }
            return itemUrl;
        }

        private bool CheckIsRecord(IAveListItem item)
        {
            bool isRecord = false;
            int result = 0;
            try
            {
                object obj = item.FieldValues["_vti_ItemHoldRecordStatus"];
                if (obj != null && !int.TryParse(obj.ToString(), out result)) result = 0;
            }
            catch (ArgumentException ex)
            {
                result = 0;
            }
            catch (Exception e)
            {
                isRecord = false;
            }
            isRecord = IsBlockEditAndDeleteRecord(result);
            return isRecord;
        }

        private bool IsBlockEditAndDeleteRecord(IAveListItem item)
        {
            return IsBlockEditAndDeleteRecord(GetHoldAndRecordStatus(item));
        }

        private bool IsBlockEditAndDeleteRecord(int holdAndRecordStatus)
        {
            return ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.RecordMask) != 0L) && ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.EditBlockedMask) != 0L) && ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.DeleteBlockedMask) != 0L);
        }

        private int GetHoldAndRecordStatus(IAveListItem item)
        {
            int result = 0;
            try
            {
                if ((GetBoolIprPropertyCore(item.ParentList, "ecm_ListFieldsReadyForIPR")) || IsHoldOrRecordsEnabled(item.ParentList))
                {
                    try
                    {
                        if (item.Fields.Contains(new Guid(key)))
                        {
                            object obj2 = item[new Guid(key)];
                            if ((obj2 != null) && !int.TryParse(obj2.ToString(), out result))
                            {
                                result = 0;
                            }
                        }
                    }
                    catch (ArgumentException)
                    {
                        result = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(string.Format("An error occur in get hold and declare status, reason : {0}.", ex.ToString()));
            }
            return result;
        }

        private bool IsHoldOrRecordsEnabled(IAveList list)
        {
            if (list == null || list.Fields == null)
            {
                throw new ArgumentNullException("list");
            }
            if (list.Fields.Contains(new Guid(key)))
            {
                return (list.Fields[new Guid(key)] != null);
            }
            else
            {
                return false;
            }
        }

        private bool GetBoolIprPropertyCore(IAveList list, string propName)
        {
            bool? nullable = null;
            if (list != null && list.RootFolder != null && list.RootFolder.Properties != null)
            {
                object obj = list.RootFolder.Properties[propName];
                if (obj != null) nullable = new bool?(obj.ToString().Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase));
            }
            return (nullable == true);
        }
    }

    public enum HoldAndRecordStatusMask
    {
        EditBlockedMask = 1,
        RecordMask = 0x10,
        DeleteBlockedMask = 0x100,
        HoldMask = 0x1000,
    }
}
