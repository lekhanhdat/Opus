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



namespace AvePoint.Wrapper.QueryService
{
    using GCommon.Utility;
    using Common;
    using GCommon;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using AvePoint.Common;

    /// <summary>
    /// todo:qlluo: 业务逻辑层，最终目的是将这个类中的逻辑抽离出QueryService，目前为保证接口稳定，暂时抽离出来一个类
    /// </summary>
    class BusinessLayerForBackupRestore
    {
        protected static AveLogger logger = AveLogger.GetInstance(typeof(BusinessLayerForBackupRestore));

        /// <summary>
        /// 将string的workflow id转换成guid, workflow id是从数据库中读出来的，需要通过编码转成16为字节数组
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool TryParseWorkflowId(string workflowId, out Guid id)
        {
            try
            {
                id = new Guid(Encoding.Unicode.GetBytes(workflowId));
            }
            catch (Exception ex)
            {
                logger.Info("workflow Id is not unicode format.WorkflowId:{0}, exception:{1}", workflowId, ex);
                try
                {
                    id = new Guid(Convert.FromBase64String(workflowId));
                }
                catch (Exception ex1)
                {
                    logger.Info("workflow Id is not string format.WorkflowId:{0}, exception:{1}", workflowId, ex1);
                    id = Guid.Empty;
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 处理data集合
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Dictionary<string, object> TrimColumnsForWF(Hashtable data)
        {
            return data.Cast<DictionaryEntry>().
                            Where(kv => kv.Key.ToString().StartsWith("#", StringComparison.Ordinal)).
                            ToDictionary(kv => kv.Key.ToString().TrimStart("#".ToCharArray()), kv => kv.Value);
        }

        /// <summary>
        /// 处理metadata和conditionParam集合
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="conditionParam"></param>
        /// <param name="columns"></param>
        /// <param name="conditions"></param>
        public void TrimColumnsForWF(Hashtable metadata, Hashtable conditionParam, List<string> excludeField, out Dictionary<string, object> columns, out Dictionary<string, object> conditions, out List<string> excludeList)
        {
            columns = metadata.Cast<DictionaryEntry>().
                Where(kv => kv.Key.ToString().StartsWith("#", StringComparison.Ordinal)).
                ToDictionary(kv => kv.Key.ToString().TrimStart("#".ToCharArray()), kv => kv.Value);
            conditions = conditionParam.Cast<DictionaryEntry>().
                Where(kv => kv.Key.ToString().StartsWith("#", StringComparison.Ordinal)).
                ToDictionary(kv => kv.Key.ToString().TrimStart("#".ToCharArray()), kv => kv.Value);
            excludeList = excludeField.Select(f => f.TrimStart("#".ToCharArray())).ToList();
        }

        /// <summary>
        /// 如果冲突，获取新的LeafName
        /// </summary>
        /// <param name="listItemName"></param>
        /// <param name="lastModified"></param>
        /// <param name="isSourceWin"></param>
        /// <returns></returns>
        public string GenerateLeafName(string listItemName, DateTime lastModified, bool isSourceWin)
        {
            string newName = string.Empty;
            if (isSourceWin)
            {
                newName = AveSPUtility.GetConflictNewName(listItemName, lastModified);
            }
            else
            {
                newName = listItemName;
            }
            return newName;
        }

        /// <summary>
        /// 从NavigationWebAndPage中获取原端WebUrl, 进行Url Replace , QueryService通过原端Url查目的端WebId, 组装Mapping。
        /// 除了QueryService通过原端Url查目的端WebId之外都属于业务逻辑
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webSettingInfo"></param>
        /// <param name="siteManagedMappings"></param>
        /// <param name="sourceSiteInfo"></param>
        /// <param name="destSiteUrl"></param>
        /// <param name="webIdMapping"></param>
        /// <param name="getWebId"></param>
        /// <returns></returns>
        public Dictionary<Guid, Guid> ReloadHiddenWebProperty(Guid siteId, AveWebSettingInfo webSettingInfo, List<Dictionary<string, string>> siteManagedMappings, AveSiteInfo sourceSiteInfo, string destSiteUrl, Dictionary<Guid, Guid> webIdMapping
            ,Func<Guid,string,Guid> getWebId)
        {
            var hiddenWebMapping = new Dictionary<Guid, Guid>();
            var hiddenWeb = webSettingInfo.NavigationWebAndPage.Value["Hidden"]["web"];
            foreach (Guid Id in hiddenWeb.Keys)
            {
                if (!webIdMapping.ContainsKey(Id))
                {
                    string webUrl = GetWebUrl(siteManagedMappings, sourceSiteInfo, destSiteUrl, hiddenWeb, Id);
                    var webId = getWebId?.Invoke(siteId, webUrl) ?? Guid.Empty;
                    if (webId != Guid.Empty && !hiddenWebMapping.ContainsKey(Id))
                    {
                        hiddenWebMapping[Id] = webId;
                    }
                }
            }
            return hiddenWebMapping;
        }

        public bool HasUniquePermission(IAveSecurableObject sObj)
        {
            return sObj.HasUniqueRoleAssignments;
        }

        /// <summary>
        /// 通过API将回收站中Item删除
        /// </summary>
        /// <param name="site"></param>
        /// <param name="deleteTransactionId"></param>
        public void RemoveFromRecyclebinByAPI(IAveSite site, List<Guid> deleteTransactionId)
        {
            if (deleteTransactionId.Count > 0)
            {
                try
                {
                    site.RecycleBin.Delete(deleteTransactionId.ToArray());
                }
                catch (Exception e) //sp19 不会删DB记录，暂时用异常控制
                {
                    logger.Warn("An error occurred while deleting a item in recycle bin. Reason:{0}.", e);
                }
            }
        }

        /// <summary>
        /// 解析content type definitin xml
        /// </summary>
        /// <param name="ctInfo"></param>
        /// <param name="definition"></param>
        /// <param name="includeAdditionalInfo">true:获取全部信息, false:只获取基本信息</param>
        public void ReadDefinitionXml(AveContentTypeInfo ctInfo, string definition, bool includeAdditionalInfo)
        {
            var doc = new XmlDocument() { InnerXml = definition };
            var root = (XmlElement)doc.ChildNodes[0];
            ctInfo.Name = root.Attributes["Name"]?.Value ?? ctInfo.Name;//使用xml中的Name作为ContentType的真实名字
            ctInfo.Id = root.Attributes["ID"].Value;
            ctInfo.ReadOnly = string.Equals(root.GetAttribute("ReadOnly"), "TRUE", StringComparison.OrdinalIgnoreCase);
            ctInfo.Description = root.GetAttribute("Description");
            ctInfo.FieldsSchemaXml = $"<Fields>{root["FieldRefs"]?.InnerXml ?? string.Empty}</Fields>";
            ctInfo.ResourceFolder = root["Folder"]?.GetAttribute("TargetName") ?? string.Empty;
            ctInfo.DocumentTemplate = root["DocumentTemplate"]?.GetAttribute("TargetName") ?? string.Empty;
            ctInfo.DocumentTemplate = GetDocumentTemplateName(ctInfo.DocumentTemplate, ctInfo.ResourceFolder);
            if (!string.IsNullOrEmpty(ctInfo.DocumentTemplate) && !string.IsNullOrEmpty(ctInfo.ResourceFolder))
            {
                ctInfo.DocumentTemplateUrl = string.Format("/{0}/{1}/{2}", ctInfo.Scope, ctInfo.ResourceFolder, ctInfo.DocumentTemplate);
            }
            ctInfo.Group = root.GetAttribute("Group");
            ctInfo.Hidden = string.Equals(root.GetAttribute("Hidden"), "TRUE", StringComparison.OrdinalIgnoreCase);
            //不存在或者存在不为false都是true
            ctInfo.RequireClientRenderingOnNew = root.HasAttribute("RequireClientRenderingOnNew") ? !bool.FalseString.Equals(root.GetAttribute("RequireClientRenderingOnNew"), StringComparison.OrdinalIgnoreCase) : true;
            ctInfo.NewDocumentControl = root.GetAttribute("NewDocumentControl");

            //有一些情况只需要获取基本信息
            if (includeAdditionalInfo)
            {
                if (root["XmlDocuments"] != null)
                {
                    foreach (XmlNode node in root["XmlDocuments"].ChildNodes)
                    {
                        string temp = AveCompressedUtility.GetStringFromBase64String(node.InnerText);
                        ctInfo.XmlDocuments.Add(temp);
                    }
                }
            }
        }

        /// <summary>
        /// 将SharePoint 国际化的key转换成对应语言的value
        /// </summary>
        /// <param name="ctInfo"></param>
        public void GetLocalizedString(AveContentTypeInfo ctInfo)
        {
            ctInfo.Name = GetLocalizedString(ctInfo.Name);
            ctInfo.Description = GetLocalizedString(ctInfo.Description);
            ctInfo.Group = GetLocalizedString(ctInfo.Group);
        }

        /// <summary>
        /// 生成冲突Item的title
        /// </summary>
        /// <param name="lastModified"></param>
        /// <returns></returns>
        public string GetTimeNameForConflictListItem(DateTime lastModified)
        {
            return $"({AveDateTimeUtility.ConvertToType008(lastModified)})";
        }

        /// <summary>
        /// 通过API获取一些List信息
        /// </summary>
        /// <param name="list"></param>
        /// <param name="listInfo"></param>
        public void AddListInfoByAPI(IAveList list, AveListInfo listInfo)
        {
            string url = list.RootFolder.ServerRelativeUrl.Substring(list.ParentWeb.RootFolder.ServerRelativeUrl.Length).Trim('/');
            listInfo.Url = list.ParentWeb.Url.TrimEnd('/') + "/" + url;
            listInfo.ServerRelativeUrl = list.RootFolder.ServerRelativeUrl;
            if (list.BaseTemplate == AveListTemplateType.ExternalList && list.HasExternalDataSource)
            {
                listInfo.DataSourceXml = (string)AveAssemblyUtility.InvokeMethod(list.DataSource, list.DataSource.GetType(), "ToXml", new object[] { });
            }
        }

        /// <summary>
        /// 通过API获取一些Site Setting信息
        /// </summary>
        /// <param name="site"></param>
        /// <param name="siteSettingInfo"></param>
        public void GetSiteSettingInfoByAPI(IAveSite site, AveSiteSettingInfo siteSettingInfo)
        {
            siteSettingInfo.UseAuditFlagCache = site.Audit.UseAuditFlagCache;
        }

        /// <summary>
        /// 更新AllUserData表之前将Version相关信息重置
        /// </summary>
        /// <param name="restoringDto"></param>
        /// <param name="allUserData"></param>
        public void ResetUserDataForVersios(RestoringDto restoringDto, Dictionary<string, object> allUserData)
        {
            if (restoringDto.TargetTable == RestoreTargetTable.AllDocVersions)
            {
                if (allUserData.ContainsKey("tp_Level") && !allUserData["tp_Level"].ToString().Equals("1"))
                {
                    allUserData["tp_Level"] = 2;
                }
                if (allUserData.ContainsKey("tp_IsCurrentVersion"))
                {
                    allUserData["tp_IsCurrentVersion"] = false;
                }
                if (allUserData.ContainsKey("tp_IsCurrent"))
                {
                    allUserData["tp_IsCurrent"] = false;
                }
            }
        }
        /// <summary>
        /// 看不懂，暂时不动了。
        /// 更新AllUserData之前处理传进来的参数, 应该和RowOrdinal有关。
        /// </summary>
        /// <param name="allUserData"></param>
        /// <returns></returns>
        public Dictionary<byte, Dictionary<string, object>> GetColumns(Dictionary<string, object> allUserData)
        {
            var sharedData = new Dictionary<string, object>();
            var rowData = new Dictionary<byte, Dictionary<string, object>>();
            foreach (var kv in allUserData)
            {
                var value = kv.Value;
                var key = kv.Key;
                if (value is KeyValuePair<byte, object>)
                {
                    string colName = key.Substring(0, key.LastIndexOf('#'));
                    var tempValue = (KeyValuePair<byte, object>)value;
                    byte row = tempValue.Key;
                    if (!rowData.ContainsKey(row))
                    {
                        rowData[row] = new Dictionary<string, object>();
                    }
                    rowData[row].Add(colName, tempValue.Value);
                }
                else
                {
                    sharedData.Add(key, value);
                }
            }
            foreach (var kv in sharedData)
            {
                foreach (Dictionary<string, object> rowValue in rowData.Values)
                {
                    rowValue.Add(kv.Key, kv.Value);
                }
            }

            return rowData;
        }

        /// <summary>
        /// 获取Web Template，为后面的取Template name做准备
        /// </summary>
        /// <param name="site"></param>
        /// <param name="lcid"></param>
        /// <returns></returns>
        public IAveWebTemplateCollection GetWebTemplates(IAveSite site, uint lcid)
        {
            return site.GetWebTemplates(lcid);
        }

        /// <summary>
        /// 通过WebTemplateId,ProvisionConfig获取WebTemplateName
        /// </summary>
        /// <param name="webTemplates"></param>
        /// <param name="templates"></param>
        /// <returns></returns>
        public Dictionary<Guid, string> ConvertToWebTemplateName(IAveWebTemplateCollection webTemplates, Dictionary<Guid, System.Tuple<int, int>> templates)
        {
            return templates.ToDictionary(kv => kv.Key, kv => WebTemplateIdName(kv.Value.Item1, kv.Value.Item2.ToString(), webTemplates));
        }

        /// <summary>
        /// 将Webpart property id转换成webpart guid, 由于webpart property id API可以更新，因此不一定是期待的格式
        /// </summary>
        /// <param name="webPartId"></param>
        /// <param name="webpartGuid"></param>
        /// <returns></returns>
        public bool TryGetWebPartId(string webPartId, out Guid webpartGuid)
        {
            if (webPartId != null && webPartId.Length > 36)
            {
                webPartId = webPartId.Substring(webPartId.Length - 36);
                webPartId = webPartId.Replace("_", "-");
                if (!Guid.TryParse(webPartId, out webpartGuid))
                {
                    webpartGuid = Guid.Empty;
                }
                return true;
            }
            else
            {
                logger.Warn("The webpart id is null or length less than 36. id: {0}", webPartId);
                webpartGuid = Guid.Empty;
                return false;
            }
        }

        private static string GetWebUrl(List<Dictionary<string, string>> siteManagedMappings, AveSiteInfo sourceSiteInfo, string destSiteUrl, Dictionary<Guid, string> hiddenWeb, Guid Id)
        {
            string webUrl = hiddenWeb[Id];
            webUrl = AveReplaceProcessor.UrlReplace(hiddenWeb[Id], siteManagedMappings, new ReplaceOption(true), sourceSiteInfo, destSiteUrl);
            if (webUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                webUrl = webUrl.Substring(1);
            }

            return webUrl;
        }

        private static string GetDocumentTemplateName(string documentTemplate, string resourceFolder)
        {
            if (resourceFolder.Length > 0 && documentTemplate.StartsWith(resourceFolder, StringComparison.OrdinalIgnoreCase))
            {
                int startIndex = documentTemplate.LastIndexOf('/') + 1;
                return documentTemplate.Substring(startIndex, documentTemplate.Length - startIndex);
            }
            return documentTemplate;
        }

        private static string GetLocalizedString(string name)
        {
            if (name.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
            {
                return WrapperRuntime.CurrentContext.ModelFactory.Utility.GetLocalizedString(name, "core", (uint)CultureInfo.CurrentUICulture.LCID);
            }
            return name;
        }

        private static string WebTemplateIdName(int id, string configuration, IAveWebTemplateCollection webTemplates)
        {
            string webTemplateStr = null;
            string sConfig = "#" + configuration;
            foreach (IAveWebTemplate sWebTemplate in webTemplates)
            {
                if (sWebTemplate.ID == id && sWebTemplate.Name.EndsWith(sConfig, StringComparison.OrdinalIgnoreCase))
                {
                    webTemplateStr = sWebTemplate.Name;
                    break;
                }
            }
            return webTemplateStr;
        }

    }
}
