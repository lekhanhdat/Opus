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
using Microsoft.SharePoint.Client.WebParts;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Collections;
using AvePoint.Office365.Api;

namespace AvePoint.ObjectModel.ClientOM
{
    public partial class AveClientOMOffice365Request : AveClientOM2019Request
    {
        [ReplaceByAPI]
        public override Dictionary<string, object> GetLimitedWebPartManager(string webServerRelativeUrl, string fileServerRelativeUrl, int personalizationScope, string appWebFulUrl = null)
        {
            //return base.GetLimitedWebPartManager(webServerRelativeUrl, fileServerRelativeUrl, personalizationScope, appWebFulUrl);
            Dictionary<string, object> webpartManagerProperties = new Dictionary<string, object>();
            Dictionary<string, object> webparts = new Dictionary<string, object>();
            webpartManagerProperties["WebParts" + AveObjectModelConstant.ObjectPropertySuffix] = webparts;
            List<Dictionary<string, object>> webpartLists = new List<Dictionary<string, object>>();
            webparts[AveObjectModelConstant.ChildrenProperties] = webpartLists;

            using (AveClientContext context = CreateContext(AveUrlUtility.CombineUrl(this.WebAppName, webServerRelativeUrl)))
            {
                Web web = context.Web;
                File file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));

                var listItemExceptionScope = new ExceptionHandlingScope(context);
                using (listItemExceptionScope.StartScope())
                {
                    using (listItemExceptionScope.StartTry())
                    {
                        context.Load(file, f => f.ListItemAllFields);
                    }
                    using (listItemExceptionScope.StartCatch())
                    {
                    }
                }

                LimitedWebPartManager limitedWebPartManager = file.GetLimitedWebPartManager((PersonalizationScope)personalizationScope);
                ExceptionHandlingScope exceptionScope = new ExceptionHandlingScope(context);
                using (exceptionScope.StartScope())
                {
                    using (exceptionScope.StartTry())
                    {
                        context.Load(limitedWebPartManager, lwp => lwp.WebParts.Include(
                            wpd => wpd.WebPart.ZoneIndex,
                            wpd => wpd.ZoneId,
                            wpd => wpd.Id,
                            wpd => wpd.WebPart.ExportMode,
                            wpd => wpd.WebPart.Hidden,
                            wpd => wpd.WebPart.IsClosed,
                            wpd => wpd.WebPart.Subtitle,
                            wpd => wpd.WebPart.Title,
                            wpd => wpd.WebPart.TitleUrl,
                            wpd => wpd.WebPart.Properties));
                    }
                    using (exceptionScope.StartCatch())
                    {
                        context.Load(limitedWebPartManager, lwp => lwp.WebParts.Include(
                            wpd => wpd.WebPart.ZoneIndex,
                            wpd => wpd.ZoneId,
                            wpd => wpd.Id,
                            wpd => wpd.WebPart.ExportMode,
                            wpd => wpd.WebPart.Hidden,
                            wpd => wpd.WebPart.IsClosed,
                            wpd => wpd.WebPart.Subtitle,
                            wpd => wpd.WebPart.Title,
                            wpd => wpd.WebPart.TitleUrl));
                    }
                }
                context.ExecuteQuery();

                //if (listItemExceptionScope.HasException)
                //{
                //    mLogger.Warn("get item for proterties failed,due to {0}", listItemExceptionScope.ErrorMessage);
                //}

                if (exceptionScope.HasException)
                {
                    mLogger.Warn("get webpart proterties failed,due to {0}", exceptionScope.ErrorMessage);
                }

                Dictionary<Guid, ClientResult<string>> webPartSchemaXml = new Dictionary<Guid, ClientResult<string>>();

                Dictionary<string, WebPartDefinition> webPartDefinitionMapping = null;
                ///Storage Key --> WebPart Id
                Dictionary<Guid, string> webPartIdMapping = null;

                object webpartControlIdContent;
                if (file.IsObjectPropertyInstantiated("ListItemAllFields") && (file.ListItemAllFields.FieldValues.TryGetValue("WikiField", out webpartControlIdContent) || file.ListItemAllFields.FieldValues.TryGetValue("PublishingPageContent", out webpartControlIdContent)))
                {
                    if (webpartControlIdContent != null)
                    {
                        webPartDefinitionMapping = new Dictionary<string, WebPartDefinition>(StringComparer.OrdinalIgnoreCase);
                        const string GUIDRegex = @"([a-fA-F\d]{8}-([a-fA-F\d]{4}-){3}[a-fA-F\d]{12}|\([a-fA-F\d]{8}-([a-fA-F\d]{4}-){3}[a-fA-F\d]{12}\)|\{[a-fA-F\d]{8}-([a-fA-F\d]{4}-){3}[a-fA-F\d]{12}\})";
                        var guids = new Regex(GUIDRegex).Matches(webpartControlIdContent as string);

                        foreach (Match id in guids)
                        {
                            webPartDefinitionMapping[id.Value] = null;
                        }

                        //属于优化部分，可以这么做
                        //if (webPartDefinitionMapping.Count == 1 && limitedWebPartManager.WebParts.Count == 1)
                        //{
                        //    webPartIdMapping = new Dictionary<Guid, string>();
                        //    webPartIdMapping[limitedWebPartManager.WebParts[0].Id] = webPartDefinitionMapping.Keys.First();
                        //    webPartDefinitionMapping = null;
                        //}
                        //else
                        {
                            foreach (var id in webPartDefinitionMapping.Keys.ToArray())
                            {
                                var getByControlIdExceptionScope = new ExceptionHandlingScope(context);
                                using (getByControlIdExceptionScope.StartScope())
                                {
                                    using (getByControlIdExceptionScope.StartTry())
                                    {
                                        var webPart = limitedWebPartManager.WebParts.GetByControlId(string.Concat("g_", id.Replace('-', '_')));
                                        webPartDefinitionMapping[id] = webPart;
                                        context.Load(webPart, w => w.Id);
                                    }
                                    using (getByControlIdExceptionScope.StartCatch())
                                    {

                                    }
                                }
                            }
                        }
                    }
                }

                foreach (var webPart in limitedWebPartManager.WebParts)
                {
                    if (webPart.WebPart.ExportMode != WebPartExportMode.All)
                    {
                        //webPart.WebPart.ExportMode = WebPartExportMode.All;这种方法不好使的原因是两个获取的对象不是一个
                        //通过查看server code，只要export mode不等于None，就可以export，也不需要save，这个目的是为了在运行过程中修改。
                        limitedWebPartManager.WebParts.GetById(webPart.Id).WebPart.ExportMode = WebPartExportMode.All;
                    }

                    var definition = limitedWebPartManager.ExportWebPart(webPart.Id);

                    webPartSchemaXml[webPart.Id] = definition;
                }

                context.ExecuteQuery();


                if (webPartDefinitionMapping != null && webPartDefinitionMapping.Count > 0)
                {
                    webPartIdMapping = new Dictionary<Guid, string>();
                    foreach (var keyValue in webPartDefinitionMapping)
                    {
                        if (keyValue.Value.IsPropertyAvailable("Id"))
                        {
                            webPartIdMapping[keyValue.Value.Id] = keyValue.Key;
                        }
                    }
                }

                foreach (WebPartDefinition webPart in limitedWebPartManager.WebParts)
                {
                    Dictionary<string, object> webPartDict = new Dictionary<string, object>();
                    CopyProperty(webPartDict, webPart);
                    CopyProperty(webPartDict, webPart.WebPart);

                    webPartDict["ID"] = webPart.Id.ToString("D");
                    webPartDict.Remove("Id");
                    webPartDict.Remove("ZoneId");

                    webPartDict["ZoneID"] = webPart.ZoneId;
                    webPartDict["PartOrder"] = webPart.WebPart.ZoneIndex;

                    AnalyzeWebPart(webPart, webPartSchemaXml[webPart.Id].Value, webPartDict, webPartIdMapping);

                    webpartLists.Add(webPartDict);
                }
            }
            return webpartManagerProperties;
        }
        private void AnalyzeWebPart(WebPartDefinition webPart, string webPartDefinition, Dictionary<string, object> webPartDict, Dictionary<Guid, string> webPartIdMapping)
        {
            if (string.IsNullOrEmpty(webPartDefinition))
            {
                return;
            }

            var document = new XmlDocument();
            document.PreserveWhitespace = true;
            document.LoadXml(webPartDefinition);

            var webPartExtractor = WebPartExtractorFactory.Create(document);

            var typeFullName = webPartExtractor.TypeFullName;

            if (!string.IsNullOrEmpty(typeFullName))
            {
                webPartDict["RealWebPartType"] = typeFullName.Substring(0, typeFullName.IndexOf(','));
                //webPartDict["WebPartIdProperty"] = WebPartTypeIdUtility.GenerateId(typeFullName);
            }

            string webPartIdProperty = null;
            if (webPartIdMapping != null && webPartIdMapping.TryGetValue(webPart.Id, out webPartIdProperty))
            {
                webPartDict["WebPartIdProperty"] = webPartIdProperty;
            }

            if (!document.DocumentElement.HasAttribute("ID"))
            {
                document.DocumentElement.SetAttribute("ID", webPart.Id.ToString("D"));
            }

            if (webPartExtractor is V3WebPartPropertyExtractor && webPart.WebPart.IsObjectPropertyInstantiated("Properties"))
            {
                foreach (var keyValue in webPart.WebPart.Properties.FieldValues)
                {
                    if (keyValue.Value != null && (!webPartExtractor.ContainsProperty(keyValue.Key)))
                    {
                        webPartExtractor.AddProperty(true, keyValue.Key, keyValue.Value);
                    }
                }
            }

            var listId = webPartExtractor.GetProperty("ListName");

            if (!AveTypeHelper.IsGuid(listId))
            {
                listId = webPartExtractor.GetProperty("ListId");
                if (AveTypeHelper.IsGuid(listId))
                {
                    webPartDict["ListId"] = new Guid(listId);
                }
            }
            else
            {
                webPartDict["ListId"] = new Guid(listId);
            }

            var isIncluded = webPartExtractor.GetBoolProperty("IsIncluded");

            webPartDict["IsIncluded"] = isIncluded.GetValueOrDefault(true);

            if (webPartExtractor is V3WebPartPropertyExtractor)
            {
                webPartExtractor.AddProperty(false, "IsIncluded", webPartDict["IsIncluded"].ToString());
            }
            else
            {
                //对于V2格式的
                if (!string.IsNullOrEmpty(webPartIdProperty))
                {
                    webPartExtractor.AddProperty(false, "ID", string.Concat("g_", webPartIdProperty.Replace('-', '_')));
                }
            }


            object webPartIdPropertyObj;
            if (webPartDict.TryGetValue("WebPartIdProperty", out webPartIdPropertyObj))
            {
                webPartExtractor.AddProperty(false, "WebPartIdProperty", webPartIdPropertyObj);
            }

            if (!webPartExtractor.ContainsProperty("ZoneID"))
            {
                webPartExtractor.AddProperty(false, "ZoneID", webPartDict["ZoneID"]);
            }

            if (!webPartExtractor.ContainsProperty("PartOrder"))
            {
                webPartExtractor.AddProperty(false, "PartOrder", webPartDict["PartOrder"]);
            }

            webPartDict["DefinitionXml"] = document.OuterXml;

        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetItemWebParts(Guid siteId, Guid webId, Guid listId, Guid itemDocId)
        {
            return base.GetItemWebParts(siteId, webId, listId, itemDocId);
        }
        [KeepOriginalWithAPI]
        public override bool HaveAddAndCustomizePagesPermission
        {
            get
            {
                if (haveAddAndCustomizePagesPermission.HasValue)
                {
                    return haveAddAndCustomizePagesPermission.Value;
                }
                using (ClientContext context = CreateContext())
                {
                    haveAddAndCustomizePagesPermission = DoesUserHavePermissions(context, PermissionKind.AddAndCustomizePages);
                }
                return haveAddAndCustomizePagesPermission.Value;
            }
        }
        private bool DoesUserHavePermissions(ClientContext context, PermissionKind permissionKind)
        {
            var permissions = new BasePermissions();
            permissions.Set(permissionKind);
            var result = context.Web.DoesUserHavePermissions(permissions);
            context.ExecuteQuery();
            return result.Value;
        }
        [ReplaceByAPI]
        public override void RestoreWebParts(string webServerRelativeUrl, string listTitle, Guid listId, string fileServerRelativeUrl, int scope, IList webpartBaseInfoList, AveWebPartCache mapping, bool clearAll, IAveWeb web, IReport report)
        {
            using (ClientContext context = CreateContext(web.Url))//AveUrlUtility.GetServerUrl(mWebUrl) + webServerRelativeUrl)
            {
                using (var webpartRestore = new AveOffice365WebpartRestore(webServerRelativeUrl, listTitle, listId, fileServerRelativeUrl, scope, clearAll, context, mapping, web, report, mObj, tokenProviders.MainTokenProvider))
                {
                    //webpartRestore.RestoreWebParts(webpartBaseInfoList);
                    webpartRestore.RestoreWebParts(webpartRestore.GetNeedRestoreWebParts(webpartBaseInfoList, clearAll));
                }
            }
        }
    }
}
