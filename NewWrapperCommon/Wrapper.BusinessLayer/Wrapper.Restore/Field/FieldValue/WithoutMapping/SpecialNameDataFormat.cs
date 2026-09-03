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
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
namespace AvePoint.Wrapper.Restore
{
    class SpecialNameDataFormat : BaseDataFormat
    {
        private static AveLogger log = AveLogger.GetInstance(typeof(SpecialNameDataFormat));
        private Dictionary<string, object> userData;
        private int originalVersion;
        public SpecialNameDataFormat(AveXmlField xmlField, IAveField destField, AveSPItem mItem, Dictionary<string, object> userData, int originalVersion) :
            base(xmlField, destField, mItem)
        {
            this.userData = userData;
            this.originalVersion = originalVersion;
        }

        public override object CheckFieldValue(object value)
        {
            var option = new ReplaceOption(true) { NeedReplaceAbsoluteUrl = true };
            switch (destField.InternalName)
            {
                case "RoutingContentTypeInternal":
                    var sourceValues = value.ToString().Split('|');
                    var valueId = sourceValues[0];
                    var valueName = sourceValues[1];
                    var tempName = this.mItem.ParentWeb.ContentTypes.ContentTypeMapping.GetMappingRestoredContentTypeName(valueName);
                    var tempValueId = this.mItem.ParentWeb.ContentTypes.ContentTypeMapping.GetMappingRestoredContentTypeId(valueId);
                    if (!string.IsNullOrEmpty(tempName))
                    {
                        valueName = tempName;
                    }
                    if (!string.IsNullOrEmpty(tempValueId))
                    {
                        valueId = tempValueId;
                    }
                    value = valueId + "|" + valueName;
                    break;

                case "MasterSeriesItemID":
                    if (this.mItem.ParentList.SPList.BaseTemplate == AveListTemplateType.Events)//此处也需要特殊注意
                    {//CI-19064
                        var itemId = (int)value;
                        var tempIdMappingValue = this.mItem.ParentSite.MappingManager.SiteMappingManager.GetMappingItemId(this.mItem.ParentList.SPList.ID, itemId);
                        if (tempIdMappingValue != -1)
                        {
                            return tempIdMappingValue;
                        }
                    }
                    break;
                case "Target_x0020_Audiences":
                    return ReplaceAudienceId(this.mItem.ParentSite.MappingManager.SiteMappingManager, value.ToString());
                case "Modified_x0020_By":
                case "Created_x0020_By":
                    return this.mItem.ParentSite.SPMembers.EnsureUserWithCache(value.ToString());
                case "RoutingTargetPath":
                    if (destField is IAveFieldText)
                    {
                        //Replace the url in "RoutingTargetPath" for Content Organizer Rule list
                        //we only need to change patch when location is in the same site, otherwise keep the original data.
                        if (userData.ContainsKey("RoutingTargetLibrary") && userData["RoutingTargetLibrary"] != null)
                        {
                            return ChangeServerRelativeUrl(value.ToString());
                        }
                    }
                    break;
                case "TemplateUrl":
                    bool replace = false;
                    string url = value as string;
                    if (!string.IsNullOrEmpty(url) && url.Contains("&#39;"))
                    {
                        url = url.Replace("&#39;", "'");
                        replace = true;
                    }
                    url = AveReplaceProcessor.UrlReplace(url, this.mItem.ParentWeb.ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, option, this.mItem.ParentSite.SourceSiteInfo, this.mItem.ParentSite.ServerRelativeUrl);
                    if (replace && !string.IsNullOrEmpty(url) && url.Contains("'"))
                    {
                        url = url.Replace("'", "&#39;");
                    }
                    return url;

                case "ViewGuid":
                    //应该是同时包含ViewGuid和ViewName需要添加到KPI Mapping中            
                    //check contenttype instead of viewname for compatible reason,more details here:
                    var itemContentTypeId = GetContentTypeId((byte[])userData["#tp_ContentTypeId"]);
                    if (itemContentTypeId != null && itemContentTypeId.IsChildOf(AveSystemContentTypeId.SharePointListbasedStatusIndicator))
                    {
                        this.mItem.ParentWeb.ParentSite.MappingManager.SiteMappingManager.AddListIdToWebIdMapping(this.mItem.ParentList.SPList.ID, this.mItem.ParentWeb.SPWeb.ID);
                    }
                    break;
                case "ContentType":
                    string ctId = AveConvert.ConvertByteToContentTypeId(this.mItem.ParentSite.ObjectModelFactory, (byte[])userData["#tp_ContentTypeId"]).ToString();
                    var listCTIdMapping = this.mItem.ParentList.ParentSite.MappingManager.ListMappingManager.ListLevelCTIdMapping;
                    if (this.mItem.ParentSite.ContentTypeIdMapping.ContainsKey(ctId))
                    {
                        ctId = this.mItem.ParentSite.ContentTypeIdMapping[ctId];
                        log.Info("ContentTypeId Mapping To: {0}", ctId);
                    }
                    if (listCTIdMapping.ContainsKey(ctId))
                    {
                        return listCTIdMapping[ctId];
                    }
                    else
                    {
                        IAveContentType ct = this.mItem.ParentList.SPList.ContentTypes[this.mItem.ParentList.SPList.ContentTypes.BestMatch(this.mItem.ParentSite.ObjectModelFactory.CreateContentTypeId(ctId))];
                        if (ct != null)
                        {
                            value = ct.ID;
                        }
                        else
                        {
                            value = null;
                        }
                    }
                    break;
                //Project Policy Item List Column
                case "ProjectWebGuid":
                case "ProjectParentWebGuid":
                    string webIdString = value.ToString();
                    if (!string.IsNullOrEmpty(webIdString))
                    {
                        Guid webGuid = new Guid(webIdString);
                        if (webGuid != Guid.Empty && this.mItem.ParentWeb.ParentSite.MappingManager.SiteMappingManager.WebIDMapping.ContainsKey(webGuid))
                        {
                            value = this.mItem.ParentWeb.ParentSite.MappingManager.SiteMappingManager.WebIDMapping[webGuid];
                        }
                    }
                    break;
                case "_SourceUrl":
                    return AveReplaceProcessor.UrlReplace(value.ToString(), this.mItem.ParentWeb.ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, option, this.mItem.ParentSite.SourceSiteInfo, this.mItem.ParentSite.ServerRelativeUrl);
                case "FormData":
                case "NFFormData":
                    var contentTypeId = GetContentTypeId((byte[])userData["#tp_ContentTypeId"]);
                    if (contentTypeId != null)
                    {
                        NintexFormValueFormatBase formater;
                        if (mItem.ParentSite.SPSite.IsOnlineSite)
                        {
                            formater = new NintexFormValueFormatOnline(xmlField, destField, mItem, contentTypeId.ToString(), originalVersion);
                        }
                        else
                        {
                            formater = new NintexFormValueFormatServer(xmlField, destField, mItem, contentTypeId.ToString());
                        }
                        return formater.CheckFieldValue(value);
                    }
                    break;
            }
            return value;
        }

        private string ReplaceAudienceId(AveSiteMappingManager siteMappingMnager, string oldValue)
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.ReplaceAudienceId"))
            {
                if (string.IsNullOrEmpty(oldValue) || oldValue.IndexOf(";;", StringComparison.OrdinalIgnoreCase) <= 0)
                {
                    return oldValue;
                }
                string tempValue = oldValue.Substring(0, oldValue.IndexOf(";;", StringComparison.OrdinalIgnoreCase));
                var newResult = oldValue;
                string[] tValues = tempValue.Split(',');
                foreach (var v in tValues)
                {
                    string mappingValue;
                    if (siteMappingMnager.GetValueFromAudienceIDMapping(v, out mappingValue))
                    {
                        newResult = newResult.Replace(v, mappingValue);
                    }
                }
                return newResult;
            }
        }

        private string ChangeServerRelativeUrl(string value)
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.ChangeServerRelativeUrl"))
            {
                var index = value.IndexOf(',');
                if (index >= 0)
                {
                    string result = ChangeServerRelativeUrl(value.Substring(0, index)) + ", " +
                                    ChangeServerRelativeUrl(value.Substring(index + 1));
                    return result;
                }
                string destWebUrl = '/' + this.mItem.ParentWeb.ScopeString;
                string srcWebUrl =
                    this.mItem.ParentWeb.ParentSite.MappingManager.SiteMappingManager.WebUrlDestToSourceMapping[destWebUrl];
                if (value.TrimStart().StartsWith(srcWebUrl, StringComparison.OrdinalIgnoreCase))
                {
                    if (destWebUrl.Length == 1 && srcWebUrl.Length != 1) //目的端是top site
                    {
                        value = value.TrimStart().Substring(srcWebUrl.Length);
                    }
                    else if (srcWebUrl.Length == 1 && destWebUrl.Length != 1) //源端是top site
                    {
                        value = destWebUrl + value.TrimStart().Substring(0);
                    }
                    else
                    {
                        value = destWebUrl + value.TrimStart().Substring(srcWebUrl.Length);
                    }
                }
                return value;
            }
        }

        private IAveContentTypeId GetContentTypeId(byte[] contentTypeId)
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.GetContentTypeId"))
            {
                string ctId = AveConvert.ConvertByteToContentTypeId(this.mItem.ParentSite.ObjectModelFactory,
                    contentTypeId).ToString();
                if (this.mItem.ParentSite.MappingManager.ListMappingManager.ListLevelCTIdMapping.ContainsKey(ctId))
                {
                    return this.mItem.ParentList.ParentSite.MappingManager.ListMappingManager.ListLevelCTIdMapping[ctId];
                }
                IAveContentType ct;
                //CT不再反插 
                //源端为普通ContentType，目的端为ConnectorList时，此时ContentType赋值为connector的默认ContentType而不是通过BestMatch去获取
                if (this.mItem.ParentList.SPList.TemplateFeatureId != new Guid(AveWrapperConstants.AVEFSDLFEATRUEID)
                    && this.mItem.ParentList.SPList.TemplateFeatureId != new Guid(AveWrapperConstants.AVEVDLFEATRUEID))
                {
                    ct = this.mItem.ParentList.SPList.ContentTypes[
                            this.mItem.ParentList.SPList.ContentTypes.BestMatch(this.mItem.ParentSite.ObjectModelFactory.CreateContentTypeId(ctId))];
                    return ct != null ? ct.ID : null;
                }
                ct = this.mItem.ParentList.SPList.ContentTypes[this.mItem.ParentSite.ObjectModelFactory.CreateContentTypeId(ctId)];
                if (ct != null)
                {
                    return ct.ID;
                }
                if (this.mItem.ParentList.SPList.TemplateFeatureId == new Guid(AveWrapperConstants.AVEFSDLFEATRUEID))
                {
                    ct = this.mItem.ParentList.SPList.ContentTypes.FirstOrDefault(
                        contentType => contentType.ID != null && contentType.ID.ToString().StartsWith(
                            "0x01010003F8831469804144AE3F259EF433E9EB", StringComparison.OrdinalIgnoreCase));
                    return ct != null ? ct.ID : null;
                }
                ct = this.mItem.ParentList.SPList.ContentTypes.FirstOrDefault(
                    contentType => contentType.ID != null && contentType.ID.ToString().StartsWith(
                            "0x010100806213320A313D4DA11D1B1D6CC700CF", StringComparison.OrdinalIgnoreCase));
                return ct != null ? ct.ID : null;
            }
        }
    }
}
