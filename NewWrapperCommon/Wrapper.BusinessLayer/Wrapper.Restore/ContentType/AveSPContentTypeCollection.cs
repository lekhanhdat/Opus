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
namespace AvePoint.Wrapper.Restore
{

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon.Utility.I18N;
    using AvePoint.Wrapper.Common;
    using AvePoint.Wrapper.Mapping;
    using AvePoint.Wrapper.Resource.ServerAPI2010;
    using System.Xml;
    using System.Text.RegularExpressions;
    using System.Text;
    using NintexForm;
    using AvePoint.Wrapper.Resource.Restore;

    [AveCodeReview("2012/04/13", "yuzhi.jiang@avepoint.com", "jin.zhang@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CS_1, CodeReviewConstants.CHECK_LIST_ID_EH_2 }, null, true)]
    [AveCodeReview("2012/10/19", "fengfu.zhang@avepoint.com", "fengfu.zhang@avepoint.com", new string[3] { CodeReviewConstants.CHECK_LIST_ID_CO_8, CodeReviewConstants.CHECK_LIST_ID_FA_4, CodeReviewConstants.CHECK_LIST_ID_FA_10 }, null, true)]
    public abstract class AveSPContentTypeCollection : IReportable, AvePoint.Wrapper.Restore.IAveSPContentTypeCollection, IDisposable
    {
        protected static AveLogger log = AveLogger.GetInstance(typeof(AveSPContentTypeCollection));
        protected IReport report = new AveWrapperReport();
        protected AveSPSite mAveSPSite = null;
        protected AveSPWeb mAveSPWeb;
        protected AveSPList mAveSPList;

        protected List<string> OldStatusFields;

        public AveContentTypeHelper ContentTypeHelper;

        protected Dictionary<string, string> restoredContentTypeIdMapping = new Dictionary<string, string>();
        public Dictionary<string, AveContentTypeInfo> ContentTypeCache { get; private set; }

        public Dictionary<string, ContentTypeRestoreReport> ContentTypeResult { get; set; }

        private IAveContentTypeMapping mContentTypeMapping;

        protected AveSPContentTypeCollection()
        {
            ContentTypeResult = new Dictionary<string, ContentTypeRestoreReport>();
            ContentTypeCache = new Dictionary<string, AveContentTypeInfo>();
        }

        public IAveContentTypeMapping ContentTypeMapping
        {
            get
            {
                if (mContentTypeMapping == null)
                {
                    mContentTypeMapping = new AveContentTypeMapping(Title);
                }
                return mContentTypeMapping;
            }
        }

        protected abstract string ServerRelativeUrl { get; }
        protected abstract string Title { get; }
        protected abstract AveReportObjectType ObjectType { get; }
        protected abstract IAveContentTypeCollection ContentTypeCollection { get; }
        protected abstract IAveContentTypeCollection AllContentTypeCollection { get; }
        protected abstract IAveFieldCollection AllFieldCollection { get; }

        private void InitializeContentTypeHelper()
        {
            using (new AvePerformanceScope("Restore.AveSPContentTypeCollection.InitializeContentTypeHelper"))
            {
                if (null == ContentTypeHelper)
                {
                    ContentTypeHelper = new AveContentTypeHelper(mAveSPWeb.SPWeb, null == mAveSPList ? null : mAveSPList.SPList, mAveSPSite.MappingManager, mAveSPSite.ObjectModelFactory, mAveSPSite.AveLanguageProcesser);
                }
                ContentTypeHelper.Initialize(null == mAveSPList ? mAveSPWeb.Fields.FieldMapping : mAveSPList.AveFields.FieldMapping, ContentTypeMapping);
                if (mAveSPList != null)
                {
                    mAveSPList.AveFields.NeedReloadfieldsIfCreateMetadataField = false;
                }
            }
        }

        public static AveSPContentTypeCollection CreateInstance(object obj)
        {
            AveSPContentTypeCollection instance;
            if (obj is AveSPWeb)
            {
                instance = new AveSPWebContentTypeCollection((AveSPWeb)obj);
            }
            else if (obj is AveSPList)
            {
                instance = new AveSPListContentTypeCollection((AveSPList)obj);
            }
            else
            {
                throw new Exception("Cannot construct an instance for this object type: " + obj.GetType());
            }
            return instance;
        }

        #region restore content types
        public void RestoreContentTypes(AveContentTypeCollectionInfo contentTypeInfos)
        {
            var restoreOption = new AveContentTypeRestoreOption();
            RestoreContentTypes(contentTypeInfos, restoreOption);
        }

        public void RestoreContentTypes(AveContentTypeCollectionInfo contentTypeInfo, Dictionary<string, string> customerRenameTable)
        {
            var restoreOption = new AveContentTypeRestoreOption();
            RestoreContentTypes(contentTypeInfo, customerRenameTable, restoreOption);
        }

        public virtual void RestoreContentTypes(AveContentTypeCollectionInfo contentTypeInfo, Dictionary<string, string> customerRenameTable, AveContentTypeRestoreOption restoreOption)
        {
            if (customerRenameTable != null && customerRenameTable.Count > 0)
            {
                (ContentTypeMapping as AveContentTypeMapping).SetContentTypeNameMappingFromGui(customerRenameTable);
            }
            RestoreContentTypes(contentTypeInfo, restoreOption);
        }

        /// <summary>
        /// 步骤如下：
        /// 1. 先找到可以还原的Content Type，也就是唯一匹配的。
        /// 2. 然后再还原不存在的Content Type。
        /// 
        /// 里面的细节：
        /// 1. Find逻辑是一个一个找，按照外围提供的find顺序进行查找，如果还原了就不行下一轮查找方法。不限于还原失败还是还原成功。
        /// 2. Find过程中如果被还原过了，则认为不存在，会新创建一个。
        /// 3. 系统Build-in的Content Type不走conflict resolution，直接overwrite。
        /// 4. 还原结束之后，需要更新Mapping。
        /// 
        /// 存在的问题：
        /// 1. 通过Name来判断Content Type是否和哪个feature有关系，需要通过solution id来check？
        /// </summary>
        /// <param name="contentTypeInfos"></param>
        /// <param name="restoreOption"></param>
        public virtual void RestoreContentTypes(AveContentTypeCollectionInfo contentTypeInfos, AveContentTypeRestoreOption restoreOption)
        {
            using (new AvePerformanceScope("Restore.AveSPContentTypeCollection.RestoreContentTypes"))
            {
                if (restoreOption.ConflictHandleOption == ContentTypeConflictHandleOption.Replace)
                {
                    log.Info("The content type restore option is Replace");
                    DeleteExistedContentType();
                    restoreOption.ConflictHandleOption = ContentTypeConflictHandleOption.Overwrite;
                }
                ProcessPreRestore(contentTypeInfos);
                foreach (AveContentTypeInfo ctInfo in contentTypeInfos.ContentTypes)
                {
                    //Merge CI 140236,此问题是特殊语言环境导致的问题，content type id对应的content type为DisplayValue，已经在ContentTypeHelper中添加了特殊处理，故此处直接continue。
                    if (ctInfo.Id == "0x0110")
                    {
                        continue;
                    }
                    try
                    {
                        log.Debug("Start restore content type: {0}", ctInfo.Name);
                        SetContentTypeInfo(ctInfo);
                        RestoreSingleContentType(ctInfo, restoreOption);
                    }
                    catch (Exception e)
                    {
                        log.Warn(e.Message);
                    }
                }
                ProcessPostRestore();
            }
        }

        protected virtual void DeleteExistedContentType()
        {
        }

        private void ProcessPreRestore(AveContentTypeCollectionInfo contentTypeInfos)
        {
            InitializeContentTypeHelper();
            foreach (AveContentTypeInfo ctInfo in contentTypeInfos.ContentTypes)
            {
                string mappingName = ContentTypeMapping.GetContentTypeNameMappingFromGui(ctInfo.Name);
                if (!mappingName.Equals(ctInfo.Name, StringComparison.OrdinalIgnoreCase))
                {
                    ctInfo.MappingName = mappingName;
                }
            }
        }

        protected void ActiveFeature(AveContentTypeInfo ctInfo)
        {
            using (new AvePerformanceScope("Restore.AveSPContentTypeCollection.ActiveFeature"))
            {
                if (!string.IsNullOrEmpty(ctInfo.SolutionId))
                {
                    if (mAveSPList != null)
                    {
                        AveContentTypeHelper.ActivateFeature(mAveSPList, ctInfo.SolutionId);
                    }
                    else
                    {
                        AveContentTypeHelper.ActivateFeature(mAveSPWeb, ctInfo.SolutionId);
                    }
                }
                //此处逻辑仅用于处理还原web content type不能正确打开document set feature 的case。 
                else if (ctInfo.Id.StartsWith("0x0120D520", StringComparison.OrdinalIgnoreCase))
                {
                    string documentSetFeatureId = "3bae86a2-776d-499d-9db8-fa4cdc7884f8";
                    AveContentTypeHelper.ActivateFeature(mAveSPWeb, documentSetFeatureId);
                }
            }
        }

        protected virtual void ProcessPostRestore()
        {
            try
            {
                ContentTypeHelper.UpdateContentTypeIdMappingProperty(ContentTypeMapping.EnumContentTypeIdMapping().ToDictionary(pair => pair.Key, pair => pair.Value));
            }
            catch (AveSecurityTrimingException ex)
            {
                log.Warn("An error occurred while UpdateContentTypeIdMappingProperty.Error:{0}", ex);
            }
        }

        public virtual void ContentTypeRestorePostAction()
        {
            if (null != ContentTypeHelper)
            {
                ContentTypeHelper.UpdateDefaultContentTypeFieldLink(mAveSPList);
                ContentTypeHelper.UpdateWelcomePageViewId(mAveSPList);
                ContentTypeHelper.UpdateContentTypeIdMappingProperty(ContentTypeMapping.EnumContentTypeIdMapping().ToDictionary(pair => pair.Key, pair => pair.Value));
                ContentTypeHelper.UpdateDocumentTemplate(mAveSPList);
                ContentTypeHelper.UpdateStartWFRetention(mAveSPList);

                RevertRequiredFieldLink();
            }
        }

        public void RevertRequiredFieldLink()
        {
            if (ContentTypeHelper.ReqiredFieldCache.Count > 0)
            {
                foreach (var requiredField in ContentTypeHelper.ReqiredFieldCache)
                {
                    try
                    {
                        var contentType = ContentTypeCollection[requiredField.Key];

                        foreach (var fieldLinkId in requiredField.Value)
                        {
                            var fieldLink = contentType.FieldLinks[fieldLinkId];
                            fieldLink.Required = true;
                        }
                        contentType.Update();
                    }
                    catch (Exception e)
                    {
                        log.Warn("An error occurred while revert required field link {0}", e);
                    }
                }
            }
        }

        #endregion

        #region find content type

        public IAveContentType FindWebContentType(AveContentTypeInfo ctInfo, ContentTypeFindOption[] findOptions, ContentTypeFindScope[] findScopes, ref ContentTypeExistStatus status)
        {
            using (new AvePerformanceScope("Restore.AveSPContentTypeCollection.FindWebContentType"))
            {
                status = ContentTypeExistStatus.None;
                IAveContentType contentType = null;
                foreach (ContentTypeFindScope scope in findScopes)
                {
                    try
                    {
                        switch (scope)
                        {
                            case ContentTypeFindScope.Current:
                                contentType = FindContentTypeByOptions(ctInfo, mAveSPWeb.SPWeb.ContentTypes, findOptions);
                                if (contentType != null)
                                {
                                    status = ContentTypeExistStatus.Exist;
                                    break;
                                }
                                continue;
                            case ContentTypeFindScope.Parent:
                                contentType = FindContentTypeByOptions(ctInfo, mAveSPWeb.SPWeb.AvailableContentTypes, findOptions);
                                if (contentType != null)
                                {
                                    status = ContentTypeExistStatus.ExistInParent;
                                    break;
                                }
                                continue;
                        }
                        break;
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.INFO, "Can not find the content type [{0}] from the scope [{1}]. CTID=[{2}]Error message: {3}", ctInfo.Name, scope.ToString(), ctInfo.Id, e.ToString());
                    }
                }
                return contentType;
            }
        }

        public IAveContentType FindContentTypeByIdMapping(IAveContentTypeCollection contentTypes, IAveContentTypeId ctId)
        {
            using (new AvePerformanceScope("Restore.AveSPWebContentTypeCollection.FindContentTypeByIdMapping"))
            {
                if (null != ctId)
                {
                    string mappingValue = ContentTypeMapping.GetMappingRestoredContentTypeId(ctId.ToString());
                    if (!String.IsNullOrEmpty(mappingValue))
                    {
                        ctId = ContentTypeHelper.GetContentTypeId(mappingValue);
                    }
                    else
                    {
                        ctId = ContentTypeHelper.GetContentTypeIdFromMapping(ctId.ToString());
                    }
                    if (null != ctId)
                    {
                        return ContentTypeHelper.FindContentTypeById(contentTypes, ctId);
                    }
                }
                return null;
            }
        }

        public virtual IAveContentType FindContentTypeByOptions(AveContentTypeInfo ctInfo, IAveContentTypeCollection collection, ContentTypeFindOption[] findOption)
        {
            using (new AvePerformanceScope("Restore.AveSPContentTypeCollection.FindContentTypeByOptions"))
            {
                IAveContentType contentType = null;
                if (!string.IsNullOrEmpty(ctInfo.MappingName))
                {
                    findOption = new ContentTypeFindOption[] { ContentTypeFindOption.FindByName };
                }
                foreach (ContentTypeFindOption option in findOption)
                {
                    contentType = FindContentTypeByOption(ctInfo, collection, option);
                    if (contentType != null)
                    {
                        break;
                    }
                }
                return contentType;
            }
        }

        protected IAveContentType FindContentTypeByOption(AveContentTypeInfo ctInfo, IAveContentTypeCollection collection, ContentTypeFindOption findOption)
        {
            using (new AvePerformanceScope("Restore.AveSPContentTypeCollection.FindContentTypeByOption"))
            {
                IAveContentType contentType = null;
                IAveContentTypeId ctId = ContentTypeHelper.GetContentTypeId(ctInfo.Id);
                try
                {
                    switch (findOption)
                    {
                        case ContentTypeFindOption.FindBySchema:
                            contentType = FindContentTypeByIdMapping(collection, ctId);
                            break;
                        case ContentTypeFindOption.FindById:
                            contentType = ContentTypeHelper.FindContentTypeById(collection, ctId);
                            break;
                        case ContentTypeFindOption.FindByName:
                            contentType = ContentTypeHelper.FindContentTypeByName(collection, string.IsNullOrEmpty(ctInfo.MappingName) ? ctInfo.Name : ctInfo.MappingName, true, ctId, !string.IsNullOrEmpty(ctInfo.MappingName));
                            try
                            {
                                if (WrapperConfiguration.FindContentTypeByResourceFolder && contentType == null && ctInfo.ResourceFolder != null && !ctInfo.ResourceFolder.EndsWith(ctInfo.Name, StringComparison.OrdinalIgnoreCase))
                                {
                                    log.Info("[CT log]: Find content type by resource folder. ctName:{0}, ResourceFolder:{1}.", ctInfo.Name, ctInfo.ResourceFolder);
                                    string originalContentTypeName;
                                    int pot = ctInfo.ResourceFolder.LastIndexOf("/");
                                    if (pot > 0)
                                    {
                                        originalContentTypeName = ctInfo.ResourceFolder.Substring(pot + 1);
                                        contentType = ContentTypeHelper.FindContentTypeByName(collection, originalContentTypeName, true, ctId, !string.IsNullOrEmpty(ctInfo.MappingName));
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                log.Warn("[CT log]: Failed to find content type by resource folder. ctName:{0}, ResourceFolder:{1}. ex:{2}.", ctInfo.Name, ctInfo.ResourceFolder, e.ToString());
                            }
                            break;
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.INFO, "Can not find the content type [{0}] using the option [{1}]. CTID=[{2}], CTName=[{3}]. Error message: {4}", ctInfo.Name, findOption.ToString(), ctInfo.Id, ctInfo.Name, e.ToString());
                }

                return contentType;
            }
        }
        #endregion

        #region restore single content type
        protected virtual IAveContentType RestoreSingleContentType(AveContentTypeInfo ctInfo, AveContentTypeRestoreOption restoreOption)
        {
            IAveContentType contentType = null;
            ContentTypeExistStatus status = ContentTypeExistStatus.None;
            ActiveFeature(ctInfo);//对于激活Feature自动产生的ContentType，如DocumentSet，需要先去激活feature再去find，否则会出现找不到创建出双份
            contentType = FindWebContentType(ctInfo, restoreOption.FindOption, restoreOption.FindScope, ref status);
            if (status == ContentTypeExistStatus.ExistInParent)
            {
                return contentType;
            }
            return RestoreSingleContentType(ctInfo, contentType, restoreOption, false, false, false);
        }

        protected virtual void SetContentTypeInfo(AveContentTypeInfo ctInfo)
        {
            using (new AvePerformanceScope("Restore.AveSPContentTypeCollection.SetContentTypeInfo"))
            {
                if (ctInfo != null)
                {
                    if (!string.IsNullOrEmpty(ctInfo.DocumentTemplate))
                    {
                        if (!string.IsNullOrEmpty(ctInfo.ResourceFolder) && ctInfo.DocumentTemplate.StartsWith(ctInfo.ResourceFolder, StringComparison.OrdinalIgnoreCase))
                        {
                            ctInfo.DocumentTemplate = ServerRelativeUrl + "/" + ctInfo.DocumentTemplate;
                        }
                        else if (ctInfo.DocumentTemplate.IndexOf('/') >= 0 && !ctInfo.DocumentTemplate.StartsWith("/_layouts", StringComparison.OrdinalIgnoreCase))
                        {
                            ctInfo.DocumentTemplate = AveReplaceProcessor.UrlReplace(ctInfo.DocumentTemplate, mAveSPSite.MappingManager.SiteMappingManager.SiteManagedMappings,
                                                                                     new ReplaceOption(true, true), mAveSPSite.SourceSiteInfo, mAveSPSite.ServerRelativeUrl);
                        }
                    }
                    string srcName = ctInfo.Name;
                    ctInfo.Name = mAveSPSite.GetNameByLanguageMapping(ctInfo.Name, AveLanguageMappingType.ContentTypeMapping);
                    if (!string.IsNullOrEmpty(ctInfo.MappingName))
                    {
                        ctInfo.Name = ctInfo.MappingName;
                    }
                    if (!string.IsNullOrEmpty(ctInfo.Group))
                    {
                        ctInfo.Group = mAveSPSite.GetNameByLanguageMapping(ctInfo.Group, AveLanguageMappingType.ContentTypeMapping);
                    }
                    if (string.Equals(srcName, ctInfo.Name, StringComparison.Ordinal) && AveBuiltInContentTypeId.Contains(ctInfo.Id))
                    {//not mapping
                        if (mAveSPWeb.WebSrcLanguageId != 0 && mAveSPWeb.SPWeb.WorkingLanguage != mAveSPWeb.WebSrcLanguageId &&
                            !ctInfo.Name.StartsWith("$Resources:", StringComparison.OrdinalIgnoreCase))
                        {
                            ctInfo.Name = "$Resources:" + ctInfo.Name;
                        }
                    }
                    if (this.mAveSPSite.SPSite.SPVersion != null && mAveSPSite.SourceSiteInfo != null
                        && mAveSPSite.SourceSiteInfo.SPVersion != mAveSPSite.SPSite.SPVersion)
                    {
                        try
                        {
                            ctInfo.SchemaXml = ChangeContentTypeAssemblyXml(ctInfo.SchemaXml, new Version(mAveSPSite.SPSite.SPVersion).Major);
                            for (int i = 0; i < ctInfo.XmlDocuments.Count; i++)
                            {
                                ctInfo.XmlDocuments[i] = ChangeContentTypeAssemblyXml(ctInfo.XmlDocuments[i], new Version(mAveSPSite.SPSite.SPVersion).Major);
                            }
                        }
                        catch (Exception e)
                        {
                            log.Debug("Change content type XML assembly version failed. Error message: {0}", e.ToString());
                        }
                    }
                }
            }
        }
        /// <summary>
        /// For change assembly versions between different SPVersions
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="version"></param>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "spe is xml namespace")]
        protected string ChangeContentTypeAssemblyXml(string xml, int version)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(xml);
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmldoc.NameTable);
            nsmgr.AddNamespace("spe", "http://schemas.microsoft.com/sharepoint/events");
            XmlNodeList nodeList = xmldoc.SelectNodes("//spe:Receivers/Receiver/Assembly", nsmgr);
            foreach (XmlNode node in nodeList)
            {
                if (Regex.IsMatch(node.InnerText, @"Microsoft.Office.DocumentManagement, Version=([1-9][0-9].0.0.0), Culture=neutral, PublicKeyToken=71e9bce111e9429c"))
                {
                    node.InnerText = String.Format("Microsoft.Office.DocumentManagement, Version={0}.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c", version);
                }
            }
            return xmldoc.OuterXml;
        }

        protected IAveContentType RestoreSingleContentType(AveContentTypeInfo ctInfo, IAveContentType contentType, AveContentTypeRestoreOption restoreOption, bool throwWhenNotFound, bool throwWhenConflict, bool isEnsureContentType)
        {
            using (new AvePerformanceScope("Restore.AveSPContentTypeCollection.RestoreSingleContentType"))
            {
                string sourceContentTypeName = ctInfo.Name;
                try
                {
                    ActionBeforeRestore();
                    //SetContentTypeInfo(ctInfo);
                    bool needUpdateDcoumentSet = true;
                    bool isHighVersionToLowVersion = new Version(this.mAveSPSite.SourceSiteInfo.SPVersion).Major > new Version(this.mAveSPSite.SPSite.SPVersion).Major;
                    if (contentType != null)
                    {
                        bool isConfilict = false;
                        if (isEnsureContentType && (restoreOption.ConflictHandleOption == ContentTypeConflictHandleOption.Skip || restoreOption.ConflictHandleOption == ContentTypeConflictHandleOption.None))
                        {
                            isConfilict = !ContentTypeHelper.CompareEnsureContentTypes(ctInfo, contentType);
                        }
                        else
                        {
                            if (restoredContentTypeIdMapping.ContainsKey(contentType.ID.ToString()) && string.Equals(restoredContentTypeIdMapping[contentType.ID.ToString()], ctInfo.Id, StringComparison.OrdinalIgnoreCase))
                            {
                                log.Info("The CT has been restored,not need to compared. contentType:{0}", contentType.Name);
                            }
                            else
                            {
                                isConfilict = !ContentTypeHelper.CompareContentTypes(ctInfo, contentType);
                            }
                        }
                        needUpdateDcoumentSet = isConfilict;
                        if (throwWhenConflict && isConfilict)
                        {
                            throw new AveSchemaDependencyConflictException(contentType.Name, "content type");
                        }
                        if (isConfilict)
                        {
                            contentType = HandleConflict(ctInfo, contentType, restoreOption, isHighVersionToLowVersion);
                        }
                    }
                    else
                    {
                        contentType = CreateNewContentType(ctInfo, restoreOption, throwWhenNotFound, isHighVersionToLowVersion);
                    }
                    ActionAfterRestore(ctInfo, contentType, restoreOption, needUpdateDcoumentSet);
                    this.report.AddDetail(new AveWrapperReportDto(ctInfo.Name, Title, ObjectType, AveStatus.Successful, string.Empty));
                }
                catch (AveSchemaDependencyConflictException)
                {
                    throw;
                }
                catch (AveSchemaDependencyNotFoundException)
                {
                    throw;
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestoreContentTypeFailedEventMessage(sourceContentTypeName, ex));
                    this.report.AddDetail(new AveWrapperReportDto(ctInfo.Name, Title, ObjectType, AveStatus.Skipped, AveReportResource.Wrapper_Report_SkipListContentType));
                }
                catch (Exception ex)
                {
                    log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestoreContentTypeFailedEventMessage(sourceContentTypeName, ex));
                    AddContentTypeReport(ex, ctInfo.Name);
                    contentType = null;
                    if (this is AveSPCTHubContentTypeCollection)
                    {
                        if (ContentTypeResult.ContainsKey(ctInfo.Name))
                        {
                            ContentTypeResult[ctInfo.Name].FailedException = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                        }
                        else
                        {
                            ContentTypeRestoreReport report = new ContentTypeRestoreReport(restoreOption.ConflictHandleOption);
                            report.FailedException = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                            ContentTypeResult.Add(ctInfo.Name, report);
                        }
                    }
                }
                if (contentType != null)
                {
                    if (restoreOption.COMPARE_MD5)
                    {
                        //添加MD5属性到XmlDocuments中
                        ContentTypeHelper.UpdateMD5ToXmlDocuments(contentType);
                    }
                }
                AddContentTypeMappings(ctInfo, contentType, sourceContentTypeName);

                return contentType;
            }
        }

        private void AddContentTypeReport(Exception ex, string contentTypeName)
        {
            var wrapperException = ex as AveWrapperBaseException;
            if (wrapperException != null)
            {
                this.report.AddDetail(new AveWrapperReportDto(wrapperException.I18NKey, contentTypeName, Title, ObjectType, AveStatus.Failed, wrapperException.Parameters));
            }
            else
            {
                this.report.AddDetail(new AveWrapperReportDto(contentTypeName, Title, ObjectType, AveStatus.Failed, ex.Message));
            }
        }
        protected virtual IAveContentType CreateNewContentType(AveContentTypeInfo ctInfo, AveContentTypeRestoreOption restoreOption, bool throwWhenNotFound, bool isHighVersionToLowVersion = false)
        {
            using (new AvePerformanceScope("Restore.AveSPContentTypeCollection.CreateNewContentType"))
            {
                if (throwWhenNotFound)
                {
                    throw new AveSchemaDependencyNotFoundException(ctInfo.Name, "content type");
                }
                IAveContentType contentType = null;
                ctInfo.Name = ContentTypeHelper.GetAvailableContentTypeName(ctInfo, AllContentTypeCollection, ref contentType);
                if (contentType != null)
                {
                    return contentType;
                }
                contentType = CreateContentType(ctInfo, restoreOption);
                if (null != contentType)
                {
                    ContentTypeHelper.UpdateContentType(ContentTypeCollection, contentType, ctInfo, AllFieldCollection, true, restoreOption, isHighVersionToLowVersion);
                    //content type 在new add的时候已经添加到catch中，所以不需要reload contenttypes 集合
                    if (ObjectType == AveReportObjectType.WebContentType && mAveSPSite.ObjectModelFactory.ContextKind.IsServerMode())
                    {
                        AllContentTypeCollection.IsDirty = true;
                    }
                }
                return contentType;
            }
        }

        public virtual IAveContentType CreateContentType(AveContentTypeInfo ctInfo, AveContentTypeRestoreOption restoreOption)
        {
            return CreateContentType(ctInfo, restoreOption, false);
        }

        protected IAveContentType CreateContentType(AveContentTypeInfo ctInfo, AveContentTypeRestoreOption restoreOption, bool isConflictById)
        {
            using (new AvePerformanceScope("Restore.AveSPContentTypeCollection.CreateContentType"))
            {
                //ActiveFeature(ctInfo.SolutionId);//对于激活Feature自动产生的ContentType，如DocumentSet，需要先去激活feature再去find，否则会出现找不到创建出双份
                IAveContentType parentContentType = GetParentContentType(ctInfo, restoreOption, false);
                IAveContentTypeId contentTypeId = ContentTypeHelper.GetContentTypeId(ctInfo.Id);

                IAveContentType contentType = null;
                if (!ContentTypeResult.ContainsKey(ctInfo.Name))
                {
                    ContentTypeResult.Add(ctInfo.Name, new ContentTypeRestoreReport(ContentTypeConflictHandleOption.CreateNew));
                }
                int tryTimes = 0;
                foreach (ContentTypeCreateOption option in restoreOption.CreateOption)
                {
                    try
                    {
                        //清空异常信息
                        ContentTypeResult[ctInfo.Name].FailedException = string.Empty;

                        switch (option)
                        {
                            case ContentTypeCreateOption.UseId:
                                if (parentContentType != null && !isConflictById)
                                {
                                    if (contentTypeId.IsChildOf(parentContentType.ID))
                                    {
                                        contentType = ContentTypeHelper.CreateContentType(contentTypeId, ContentTypeCollection, ctInfo.Name);
                                        contentType = ContentTypeCollection.Add(contentType);
                                    }
                                }
                                break;
                            case ContentTypeCreateOption.UseParent:
                                if (parentContentType != null)
                                {
                                    contentType = ContentTypeHelper.CreateContentTypeWithParent(parentContentType, ContentTypeCollection, ctInfo.Name);
                                    //DevBranch/Replicator/20120214_Dev_Branch  此branch merge D5 ci，逻辑不适用D6，并且造成bug ADO-213338
                                    //[ADO-23430][Replicator]{Tested} Modify: Merge 57 CI to 6.x;[hanlin.zhao]
                                    //List<Guid> needRemoveFromList = null;
                                    //if (mAveSPList != null && mAveSPList.SPList != null)
                                    //{
                                    //    needRemoveFromList = new List<Guid>();
                                    //    try
                                    //    {
                                    //        foreach (IAveFieldLink fieldLink in parentContentType.FieldLinks)
                                    //        {
                                    //            try
                                    //            {
                                    //                IAveField temp = mAveSPList.SPList.Fields[fieldLink.ID];
                                    //                if (temp == null)
                                    //                {
                                    //                    needRemoveFromList.Add(fieldLink.ID);
                                    //                }
                                    //            }
                                    //            catch (Exception e)
                                    //            {
                                    //                needRemoveFromList.Add(fieldLink.ID);
                                    //                log.Debug("The field of the content type doesn't exist in the list.list title: {0},Content type:{1} ,fieldLink Id:{2},Exception:{3}", mAveSPList.SPList.Title, contentType.Name, fieldLink.ID, e.ToString());
                                    //            }
                                    //        }
                                    //    }
                                    //    catch (Exception ex)//O365的时候会抛异常
                                    //    {
                                    //        log.Warn("Get need remove fields from content type in the list.list title: {0},content type: {1},error: {2}", mAveSPList.SPList.Title, contentType.Name, ex.ToString());
                                    //    }
                                    //}
                                    try
                                    {
                                        contentType = ContentTypeCollection.Add(contentType);
                                    }
                                    catch (Exception e)
                                    {
                                        log.Debug("user parent content type add content type failed, try CreateContentTypeWithSameParent. Error:{0}", e);
                                        //创建出跟已存在content type相同parent的content type，在sharepoint界面中是不能创建出这样的content type的，只能通过调用反射的方法添加
                                        try
                                        {
                                            contentType = ContentTypeHelper.CreateContentTypeWithSameParent(ContentTypeCollection, contentType);
                                        }
                                        catch (Exception ex)
                                        {
                                            //ADO-111839，反射调用SPContentTypeCollection.UpdateContentType方法时，由于SPList.Version > SPContentTypeCollection.m_verList，因此出现异常，调用List.Reload()后解决。
                                            //由于SharePoint API也存在此问题，因此先按reload处理，具体详见JIRA comment。
                                            log.Debug("An error occurred while creating content type with same parent, need reload, error message: {0}", ex.ToString());
                                            contentType.ParentList.Reload();
                                            contentType = ContentTypeHelper.CreateContentTypeWithSameParent(ContentTypeCollection, contentType);
                                        }
                                    }
                                    //if (needRemoveFromList != null && needRemoveFromList.Count > 0)
                                    //{
                                    //    foreach (Guid id in needRemoveFromList)
                                    //    {
                                    //        try
                                    //        {
                                    //            bool needUpdate = false;
                                    //            IAveField tempField = mAveSPList.SPList.Fields[id];
                                    //            if (tempField.ReadOnlyField)
                                    //            {
                                    //                tempField.ReadOnlyField = false;
                                    //                needUpdate = true;
                                    //            }
                                    //            if (tempField.AllowDeletion == null || !(bool)tempField.AllowDeletion)
                                    //            {
                                    //                tempField.AllowDeletion = true;
                                    //                needUpdate = true;
                                    //            }
                                    //            if (needUpdate)
                                    //            {
                                    //                tempField.Update();
                                    //            }

                                    //            mAveSPList.SPList.Fields[id].Delete();
                                    //        }
                                    //        catch (Exception e)
                                    //        {
                                    //            log.Warn("Cannot delete field from list. field Id:{0},exception:{1}", id, e.ToString());
                                    //        }
                                    //    }
                                    //    mAveSPList.SPList.Reload();
                                    //    contentType = ContentTypeCollection[contentType.ID];
                                    //}
                                }
                                break;
                            case ContentTypeCreateOption.ForceCreate:
                                ForceCreateContentType(ctInfo, isConflictById, contentTypeId, out contentType);
                                break;
                            case ContentTypeCreateOption.ForceCreateWithoutKeepId:
                                ForceCreateContentType(ctInfo, true, contentTypeId, out contentType);
                                break;
                        }
                        if (contentType != null)
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        contentType = null;
                        //记录异常信息
                        ContentTypeResult[ctInfo.Name].FailedException = ex.Message;
                        tryTimes++;
                        if (tryTimes == restoreOption.CreateOption.Length)
                        {
                            log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestoreContentTypeFailedEventMessage(ctInfo.Name, ex));
                        }
                    }
                }
                if (contentType == null)
                {
                    throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Restore_ContentTypeFaild);
                }
                return contentType;
            }
        }

        private bool ForceCreateContentType(AveContentTypeInfo ctInfo, bool isConflictById, IAveContentTypeId contentTypeId, out IAveContentType contentType)
        {
            using (new AvePerformanceScope("Restore.AveSPContentTypeCollection.ForceCreateContentType"))
            {
                if (mAveSPSite.ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel)
                {
                    contentType = null;
                    log.Log(AveLogLevel.INFO, "The AveContextKind is [{0}], can not create the content type [{1}]. CTID=[{2}].", mAveSPSite.ObjectModelFactory.ContextKind.ToString(), ctInfo.Name, ctInfo.Id);
                    return false;
                }
                if (isConflictById)
                {
                    contentTypeId = ContentTypeHelper.GetAvailableContentTypeId(contentTypeId);
                }
                contentType = ContentTypeHelper.CreateContentTypeWithoutParent(contentTypeId, ContentTypeCollection, ctInfo.Name);
                contentType.Group = ctInfo.Group;
                contentType = ContentTypeCollection.AddContentType(contentType, false, false, true);
                return true;
            }
        }

        protected virtual void ActionBeforeRestore()
        {
        }

        protected virtual void ActionAfterRestore(AveContentTypeInfo ctInfo, IAveContentType contentType, AveContentTypeRestoreOption restoreOption, Boolean needUpdateDocumentSet)
        {
        }

        protected virtual IAveContentType HandleConflict(AveContentTypeInfo ctInfo, IAveContentType contentType, AveContentTypeRestoreOption restoreOption, bool isHighVersionToLowVersion = false)
        {
            using (new AvePerformanceScope("Restore.AveSPContentTypeCollection.HandleConflict"))
            {
                if (AveBuiltInContentTypeId.Contains(ctInfo.Id))
                {//对于源端是buildin的contenttype，不走冲突处理的逻辑，直接进行update
                    if (restoreOption.ConflictHandleOption != ContentTypeConflictHandleOption.Skip && null != contentType)
                    {
                        HandleConflict(ctInfo, ref contentType, restoreOption, isHighVersionToLowVersion);
                    }
                }
                else if (restoreOption.COMPARE_MD5 && !String.IsNullOrEmpty(ContentTypeHelper.GetMD5FromXmlDocuments(contentType)) &&
                    ContentTypeHelper.GetCurrentMD5Property(contentType).Equals(
                        ContentTypeHelper.GetMD5FromXmlDocuments(contentType), StringComparison.OrdinalIgnoreCase))
                {
                    //对于需要比较MD5值的，若目的端XmlDocuments中存在MD5属性，并且与当前ContentType的MD5值相同，则不认为冲突，直接进行update
                    if (restoreOption.ConflictHandleOption != ContentTypeConflictHandleOption.Skip)
                    {
                        ContentTypeHelper.UpdateContentType(ContentTypeCollection, contentType, ctInfo, AllFieldCollection, true, restoreOption, isHighVersionToLowVersion);
                    }
                }
                else
                {
                    HandleConflict(ctInfo, ref contentType, restoreOption, isHighVersionToLowVersion);
                }
                return contentType;
            }
        }

        protected virtual void AddContentTypeMappings(AveContentTypeInfo ctInfo, IAveContentType contentType, string sourceContentTypeName)
        {
            if (contentType != null)
            {
                if (!contentType.Name.Equals(sourceContentTypeName) && sourceContentTypeName != null)
                {
                    ContentTypeMapping.AddContentTypeNameMapping(sourceContentTypeName, contentType.Name);
                }
                ContentTypeMapping.AddContentTypeIdMapping(ctInfo.Id, contentType.ID.ToString());
                ContentTypeMapping.AddContentTypeNameMappingById(ctInfo.Id, sourceContentTypeName, contentType.Name);
                if (!restoredContentTypeIdMapping.ContainsKey(contentType.ID.ToString()))
                {
                    restoredContentTypeIdMapping.Add(contentType.ID.ToString(), ctInfo.Id);
                }
            }
        }
        #endregion

        #region get parent content type
        public IAveContentType GetParentContentType(AveContentTypeInfo ctInfo, AveContentTypeRestoreOption restoreOption, bool needCompare)
        {
            using (new AvePerformanceScope("Restore.AveSPContentTypeCollection.GetParentContentType"))
            {
                IAveContentType parentContentType = null;
                GetRealParentContentTypeName(ctInfo);
                GetParentContentTypeOption getParentOption = restoreOption.GetParentOption;
                if (CheckDocumentSet(ctInfo.SchemaXml))
                {
                    //在处理list级别contentType时 如果是documentSet类型的CT，需要RestoreFamily来保证web级别CT的存在，保证resourceFile对于webpart的继承。
                    getParentOption = GetParentContentTypeOption.RestoreFamily;
                }
                switch (getParentOption)
                {
                    case GetParentContentTypeOption.Default:
                        GetParentContentTypeByDefault(ctInfo, ref parentContentType, needCompare, restoreOption);
                        break;
                    case GetParentContentTypeOption.RestoreFamily:
                        RestoreContentTypeFamily(ctInfo, ref parentContentType, needCompare, restoreOption);
                        break;
                    case GetParentContentTypeOption.BuildinParent:
                        GetBuiltinParentContentType(ctInfo, ref parentContentType, needCompare, restoreOption);
                        break;
                }
                return parentContentType;
            }
        }

        private void GetRealParentContentTypeName(AveContentTypeInfo ctInfo)
        {
            using (new AvePerformanceScope("Restore.AveSPContentTypeCollection.GetRealParentContentTypeName"))
            {
                if (string.IsNullOrEmpty(ctInfo.ParentName))
                {
                    return;
                }
                string realParentName = ctInfo.ParentName;
                string mappingValue = mAveSPWeb.ContentTypes.ContentTypeMapping.GetMappingRestoredContentTypeName(ctInfo.ParentName);
                if (!String.IsNullOrEmpty(mappingValue))
                {
                    realParentName = mappingValue;
                }
                realParentName = mAveSPWeb.ParentSite.GetNameByLanguageMapping(realParentName, AveLanguageMappingType.ContentTypeMapping);
                ctInfo.ParentName = realParentName;
                if (null != ctInfo.ParentContentTypeInfo)
                {
                    ctInfo.ParentContentTypeInfo.Name = realParentName;
                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint Property")]
        private bool CheckDocumentSet(string schemaXml)
        {
            try
            {

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(schemaXml);
                if (doc.GetElementsByTagName("ContentType").Count > 0 && doc.GetElementsByTagName("ContentType")[0].Attributes["ProgId"] != null)
                {
                    string progId = doc.GetElementsByTagName("ContentType")[0].Attributes["ProgId"].Value;
                    if (!String.IsNullOrEmpty(progId) && progId.Equals("SharePoint.DocumentSet", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Warn("get the contentType schemaXml ProgId failed. Detail:{0}", ex.ToString());
                return false;
            }
            return false;
        }

        protected bool GetParentContentTypeByDefault(AveContentTypeInfo ctInfo, ref IAveContentType parentContentType, bool needCompare, AveContentTypeRestoreOption restoreOption)
        {
            using (new AvePerformanceScope("Restore.AveSPContentTypeCollection.GetParentContentTypeByDefault"))
            {
                ContentTypeExistStatus existStatus = ContentTypeExistStatus.None;
                IAveContentType contentType = null;
                if (null != ctInfo.ParentContentTypeInfo)
                {
                    contentType = FindWebContentType(ctInfo.ParentContentTypeInfo, restoreOption.FindOption, restoreOption.FindScope, ref existStatus);
                }
                else if (ctInfo.ParentContentTypeInfo == null
                    && this is AveSPListContentTypeCollection
                    && mAveSPSite.ObjectModelFactory.ContextKind.IsServerMode10Upper()
                    && !ContentTypeHelper.IsDirectChildOfBuildInContentTypeForListContentType(ctInfo.Id))
                {
                    return true;
                    //对于一些特殊的contentType，例如Connector的contentType，其在数据库中是没有记录的，导致我们的ParentInfo备份不出来，在还原过程中，若一直找Parent，有的情况下会导致contentType少还原了，
                    //对于这种情况，不再进行查找
                }
                else
                {
                    contentType = ContentTypeHelper.FindContentTypeById(mAveSPWeb.SPWeb.AvailableContentTypes, ContentTypeHelper.GetContentTypeId(ctInfo.Id).Parent);
                    if (contentType != null)
                    {
                        existStatus = ContentTypeExistStatus.ExistInParent;
                    }
                    else
                    {
                        contentType = ContentTypeHelper.GetBuildinParentContentType(ContentTypeHelper.GetContentTypeId(ctInfo.Id));
                        if (contentType != null)
                        {
                            existStatus = ContentTypeExistStatus.ExistInParent;
                        }
                    }
                }
                if (existStatus == ContentTypeExistStatus.Exist || existStatus == ContentTypeExistStatus.ExistInParent)
                {
                    if (!needCompare
                        || null == ctInfo.ParentContentTypeInfo
                        || ContentTypeHelper.CompareContentTypes(ctInfo.ParentContentTypeInfo, contentType))
                    {
                        parentContentType = contentType;
                        return true;
                    }
                }
                return false;
            }
        }

        private bool RestoreContentTypeFamily(AveContentTypeInfo ctInfo, ref IAveContentType parentContentType, bool needCompare, AveContentTypeRestoreOption restoreOption)
        {
            using (new AvePerformanceScope("Restore.AveSPContentTypeCollection.RestoreContentTypeFamily"))
            {
                //SPMigration 断层Content Type，需要创建出一个临时的temp content type, 其他模块restoreOption.WEB_CONTENTTYPE_CREATETEMP属性默认为false.             
                IAveContentTypeId tempContentTypeId = ContentTypeHelper.GetContentTypeId(ctInfo.Id).Parent;
                if (null == ctInfo.ParentContentTypeInfo && !AveBuiltInContentTypeId.Contains(tempContentTypeId) && restoreOption.WEB_CONTENTTYPE_CREATETEMP)
                {
                    //find逻辑需从包含parent web ContentTypes的集合中查找
                    IAveContentTypeCollection availableCTs = mAveSPWeb.SPWeb.AvailableContentTypes;
                    parentContentType = ContentTypeHelper.FindContentTypeByName(availableCTs, ctInfo.Name, true, tempContentTypeId, !string.IsNullOrEmpty(ctInfo.MappingName));
                    if (parentContentType == null)
                    {
                        //寻找parent的parent
                        IAveContentType parentCT = ContentTypeHelper.FindContentTypeById(availableCTs, tempContentTypeId.Parent);
                        if (parentCT == null)
                        {
                            parentCT = ContentTypeHelper.GetBuildinParentContentType(tempContentTypeId);
                        }
                        string ctName = ContentTypeHelper.GetAvailableContentTypeName(ctInfo.Name, availableCTs);
                        IAveContentTypeCollection currentCTs = mAveSPWeb.SPWeb.ContentTypes;
                        parentContentType = ContentTypeHelper.CreateContentTypeWithParent(parentCT, currentCTs, ctName);
                        parentContentType = currentCTs.Add(parentContentType);
                    }
                    return true;
                }

                Stack<AveContentTypeInfo> CTStack = new Stack<AveContentTypeInfo>();

                while (parentContentType == null)
                {

                    if (!GetParentContentTypeByDefault(ctInfo, ref parentContentType, true, restoreOption))
                    {
                        CTStack.Push(ctInfo.ParentContentTypeInfo);
                        ctInfo = ctInfo.ParentContentTypeInfo;
                    }
                    else
                    {
                        break;
                    }
                }
                if (CTStack.Count > 0)
                {
                    mAveSPWeb.ContentTypes.InitializeContentTypeHelper();
                }
                while (CTStack.Count > 0)
                {
                    ctInfo = CTStack.Pop();
                    parentContentType = mAveSPWeb.ContentTypes.RestoreSingleContentType(ctInfo, restoreOption);
                }
                if (null == parentContentType)
                {
                    return false;
                }
                return true;
            }
        }

        private bool GetBuiltinParentContentType(AveContentTypeInfo ctInfo, ref IAveContentType contentType, bool needCompare, AveContentTypeRestoreOption restoreOption)
        {
            using (new AvePerformanceScope("Restore.AveSPContentTypeCollection.GetBuiltinParentContentType"))
            {
                IAveContentTypeId ctId = ContentTypeHelper.GetContentTypeId(ctInfo.Id);
                if (!GetParentContentTypeByDefault(ctInfo, ref contentType, needCompare, restoreOption))
                {
                    contentType = ContentTypeHelper.GetBuildinParentContentType(ctId);
                    if (contentType == null)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        #endregion


        public void LoadContentTypes(AveContentTypeCollectionInfo contentTypeInfos)
        {
            using (new AvePerformanceScope("Restore.AveSPContentTypeCollection.LoadContentTypes"))
            {
                InitializeContentTypeHelper();
                ContentTypeCache.Clear();
                if (contentTypeInfos.ContentTypes != null)
                {
                    foreach (AveContentTypeInfo ctInfo in contentTypeInfos.ContentTypes)
                    {
                        try
                        {
                            ContentTypeCache[ctInfo.Id] = ctInfo;
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.WARN, "Failed to load content type [{0}]. Message: {1}", ctInfo.Name, e.ToString());
                        }
                    }
                }
            }
        }

        #region ContentTpe

        public void HandleConflict(AveContentTypeInfo ctInfo, ref IAveContentType contentType, AveContentTypeRestoreOption restoreOption, bool isHighVersionToLowVersion)
        {
            using (new AvePerformanceScope("Restore.AveSPContentTypeCollection.HandleConflict_1"))
            {
                try
                {
                    bool isNewCreated = false;
                    if (!ContentTypeResult.ContainsKey(ctInfo.Name))
                    {
                        ContentTypeResult.Add(ctInfo.Name, new ContentTypeRestoreReport(restoreOption.ConflictHandleOption));
                    }
                    switch (restoreOption.ConflictHandleOption)
                    {
                        case ContentTypeConflictHandleOption.Append:
                        case ContentTypeConflictHandleOption.AppendDestinationWin:
                            contentType = AppendContentType(ctInfo, restoreOption);
                            isNewCreated = true;
                            break;
                        case ContentTypeConflictHandleOption.AppendSourceWin:
                            if (ctInfo.Name.Equals(contentType.Name))
                            {
                                contentType.Name = ContentTypeHelper.GetAvailableContentTypeName(contentType.Name, AllContentTypeCollection);
                                if (ObjectType == AveReportObjectType.WebContentType)
                                {
                                    mAveSPWeb.SPWeb.AvailableContentTypes[contentType.ID].Name = contentType.Name;
                                }
                                contentType.Update();
                            }
                            contentType = AppendContentType(ctInfo, restoreOption);
                            isNewCreated = true;
                            break;
                        case ContentTypeConflictHandleOption.Skip:
                            return;
                    }
                    if (null != contentType)
                    {
                        string exception = ContentTypeHelper.UpdateContentType(ContentTypeCollection, contentType, ctInfo, AllFieldCollection, isNewCreated, restoreOption, isHighVersionToLowVersion);
                        ContentTypeResult[ctInfo.Name].FailedException = exception;
                    }
                }
                catch
                {
                    //throw new AveWrapperException(AveWrapperErrorCode.ContentTypeHandleConflictError, "An error occurred while handling content type confliction.", ex);
                    throw;
                }
            }
        }

        protected IAveContentType AppendContentType(AveContentTypeInfo ctInfo, AveContentTypeRestoreOption restoreOption)
        {
            using (new AvePerformanceScope("Restore.AveSPContentTypeCollection.AppendContentType"))
            {
                IAveContentType contentType = null;
                ctInfo.Name = ContentTypeHelper.GetAvailableContentTypeName(ctInfo, AllContentTypeCollection, ref contentType);
                if (contentType != null)
                {
                    return contentType;
                }
                contentType = CreateContentType(ctInfo, restoreOption);
                return contentType;
            }
        }
        #endregion

        public void DisposeReport()
        {
            if (this.report != null)
            {
                this.report.Dispose();
            }
        }

        public virtual void Dispose()
        {
        }

        public IReport GetReport()
        {
            return this.report;
        }



        public void HandleConflict(AveContentTypeInfo ctInfo, ref IAveContentType contentType, AveContentTypeRestoreOption restoreOption)
        {
            HandleConflict(ctInfo, ref contentType, restoreOption, false);
        }
    }
    [AveCodeReview("2012/04/13", "yuzhi.jiang@avepoint.com", "jin.zhang@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CS_1, CodeReviewConstants.CHECK_LIST_ID_EH_2 }, null, true)]
    [AveCodeReview("2012/10/19", "fengfu.zhang@avepoint.com", "fengfu.zhang@avepoint.com", new string[3] { CodeReviewConstants.CHECK_LIST_ID_CO_8, CodeReviewConstants.CHECK_LIST_ID_FA_4, CodeReviewConstants.CHECK_LIST_ID_FA_10 }, null, true)]
    public class AveSPWebContentTypeCollection : AveSPContentTypeCollection
    {
        protected override string ServerRelativeUrl
        {
            get { return mAveSPWeb.SPWeb.ServerRelativeUrl; }
        }
        protected override string Title
        {
            get { return mAveSPWeb.SPWeb.Title; }
        }
        protected override AveReportObjectType ObjectType
        {
            get { return AveReportObjectType.WebContentType; }
        }
        protected override IAveContentTypeCollection ContentTypeCollection
        {
            get { return mAveSPWeb.SPWeb.ContentTypes; }
        }
        protected override IAveContentTypeCollection AllContentTypeCollection
        {
            get { return mAveSPWeb.SPWeb.AvailableContentTypes; }
        }
        protected override IAveFieldCollection AllFieldCollection
        {
            get { return mAveSPWeb.SPWeb.AvailableFields; }
        }
        public AveSPWebContentTypeCollection(AveSPWeb aveSPWeb)
        {
            mAveSPSite = aveSPWeb.ParentSite;
            mAveSPWeb = aveSPWeb;
        }

        protected override void ActionAfterRestore(AveContentTypeInfo ctInfo, IAveContentType contentType, AveContentTypeRestoreOption restoreOption, Boolean needUpdateDocumentSet = true)
        {
            using (new AvePerformanceScope("Restore.AveSPWebContentTypeCollection.ActionAfterRestore"))
            {
                if (AveSPDocumentSet.IsDocumentSet(mAveSPSite.ObjectModelFactory.CreateContentTypeId(ctInfo.Id)))
                {
                    mAveSPWeb.ParentSite.MappingManager.WebMappingManager.DocumentSetCTCache.Add(ctInfo);
                }
                if (contentType != null)
                {
                    mAveSPWeb.ParentSite.MappingManager.WebMappingManager.AddWebLevelCTIdMapping(ctInfo.Id, contentType.ID, true);
                }
            }
        }

        public override IAveContentType CreateContentType(AveContentTypeInfo ctInfo, AveContentTypeRestoreOption restoreOption)
        {
            using (new AvePerformanceScope("Restore.AveSPWebContentTypeCollection.CreateContentType"))
            {
                bool isConflictById = false;
                if (mAveSPSite.ObjectModelFactory.ContextKind.IsServerMode10Upper())
                {
                    if (ContentTypeCollection.CheckIfContentTypeExistInChildren(mAveSPSite.SPSite.ID, mAveSPWeb.ServerRelativeUrl, ctInfo.Id))
                    {
                        isConflictById = true;
                    }
                }
                return base.CreateContentType(ctInfo, restoreOption, isConflictById);
            }
        }

        protected override void ProcessPostRestore()
        {
            using (new AvePerformanceScope("Restore.AveSPWebContentTypeCollection.ProcessPostRestore"))
            {
                try
                {
                    bool hasChange = false;
                    if (mAveSPWeb.AveWeb.IsRootWeb && mAveSPWeb.AveWeb.AllProperties != null)
                    {
                        string contentTypeId = string.Empty;
                        if (mAveSPWeb.AveWeb.AllProperties.ContainsKey("PolicyCTId") && !string.IsNullOrEmpty(mAveSPWeb.AveWeb.AllProperties["PolicyCTId"].ToString()))
                        {
                            string ctId = mAveSPWeb.AveWeb.AllProperties["PolicyCTId"].ToString();
                            if (restoredContentTypeIdMapping.ContainsKey(ctId))
                            {
                                contentTypeId = restoredContentTypeIdMapping[ctId];
                            }
                        }
                        if (string.IsNullOrEmpty(contentTypeId) && mAveSPWeb.AveWeb.AllProperties.ContainsKey("PolicyName") && !string.IsNullOrEmpty(mAveSPWeb.AveWeb.AllProperties["PolicyName"].ToString()))
                        {
                            string ctName = mAveSPWeb.AveWeb.AllProperties["PolicyName"].ToString();
                            IAveContentType policyContentType = mAveSPSite.AveSite.RootWeb.ContentTypes[ctName];
                            contentTypeId = policyContentType.ID.ToString();
                        }
                        if (!string.IsNullOrEmpty(contentTypeId))
                        {
                            mAveSPWeb.AveWeb.AllProperties["PolicyCTId"] = contentTypeId;
                            if (mAveSPWeb.AveWeb.Properties != null)
                            {
                                mAveSPWeb.AveWeb.Properties["ProjectPolicyId"] = contentTypeId;
                            }
                            hasChange = true;
                        }
                    }
                    if (hasChange)
                    {
                        mAveSPWeb.AveWeb.Update();
                    }
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "Failed to replace PolicyCTId in web properties. Error:{0}", ex.ToString());
                }
                base.ProcessPostRestore();
            }
        }

        public override void ContentTypeRestorePostAction()
        {
            if (null != ContentTypeHelper)
            {
                ContentTypeHelper.UpdateDocumentTemplate(mAveSPWeb);
            }
        }
    }

    [AveCodeReview("2012/06/11", "cheng.cui@avepoint.com", "qinglong.luo@avepoint.com", null, "ADO-20426", true)]
    [AveCodeReview("2012/04/13", "yuzhi.jiang@avepoint.com", "jin.zhang@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CS_1, CodeReviewConstants.CHECK_LIST_ID_EH_2 }, null, true)]
    [AveCodeReview("2012/10/19", "fengfu.zhang@avepoint.com", "fengfu.zhang@avepoint.com", new string[3] { CodeReviewConstants.CHECK_LIST_ID_CO_8, CodeReviewConstants.CHECK_LIST_ID_FA_4, CodeReviewConstants.CHECK_LIST_ID_FA_10 }, null, true)]
    public class AveSPListContentTypeCollection : AveSPContentTypeCollection
    {
        private List<IAveContentTypeId> oldOrder = new List<IAveContentTypeId>();
        private Dictionary<string, IAveContentTypeId> newOrder = new Dictionary<string, IAveContentTypeId>();
        private List<string> listOrders;
        private Dictionary<IAveContentTypeId, string> restoredCTFailedCache = new Dictionary<IAveContentTypeId, string>();
        protected override string ServerRelativeUrl
        {
            get { return mAveSPList.SPList.RootFolder.ServerRelativeUrl; }
        }
        protected override string Title
        {
            get { return mAveSPList.SPList.Title; }
        }
        protected override AveReportObjectType ObjectType
        {
            get { return AveReportObjectType.ListContentType; }
        }
        protected override IAveContentTypeCollection ContentTypeCollection
        {
            get { return mAveSPList.SPList.ContentTypes; }
        }
        protected override IAveContentTypeCollection AllContentTypeCollection
        {
            get { return mAveSPList.SPList.ContentTypes; }
        }
        protected override IAveFieldCollection AllFieldCollection
        {
            get { return mAveSPList.SPList.Fields; }
        }
        public AveSPListContentTypeCollection(AveSPList aveSPList)
        {
            mAveSPSite = aveSPList.ParentSite;
            mAveSPWeb = aveSPList.ParentWeb;
            mAveSPList = aveSPList;
        }

        public Dictionary<string, Exception> EnsuredContentTypeResult = new Dictionary<string, Exception>();

        protected override IAveContentType RestoreSingleContentType(AveContentTypeInfo ctInfo, AveContentTypeRestoreOption restoreOption)
        {
            using (new AvePerformanceScope("Restore.AveSPListContentTypeCollection.RestoreSingleContentType"))
            {
                IAveContentType contentType = null;
                //兼容D5老数据（D5在backup没有做此处理）。
                if (ctInfo.Name.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
                {
                    ctInfo.Name = mAveSPSite.ObjectModelFactory.Utility.GetLocalizedString(ctInfo.Name, "core", (uint)CultureInfo.CurrentUICulture.LCID);
                }
                try
                {
                    ActiveFeature(ctInfo);//对于激活Feature自动产生的ContentType，如DocumentSet，需要先去激活feature再去find，否则会出现找不到创建出双份
                    contentType = Find(ctInfo, restoreOption);
                    contentType = RestoreSingleContentType(ctInfo, contentType, restoreOption, false, false, false);
                    if (contentType != null)
                    {
                        IAveContentTypeId parent = contentType.ID.Parent;
                        if (!(parent.ToString().Equals(AveBuiltInContentTypeId.Folder, StringComparison.OrdinalIgnoreCase)) && !(parent.ToString().Equals(AveBuiltInContentTypeId.UntypedDocument, StringComparison.OrdinalIgnoreCase)))// 默认的CT 不能加到order里，根据reflector 查看，需要过滤这2个。
                        {
                            newOrder[ctInfo.Id] = contentType.ID;
                        }
                    }
                }
                catch (AveSecurityTrimingException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, "An error occurred while RestoreSingleContentType. list title: {0} , contentType name:{1} , Error: {2}", Title, ctInfo.Name, e);
                }
                return contentType;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint property.")]
        public override void RestoreContentTypes(AveContentTypeCollectionInfo contentTypeInfos, AveContentTypeRestoreOption restoreOption)
        {
            using (new AvePerformanceScope("Restore.AveSPListContentTypeCollection.RestoreContentTypes"))
            {
                try
                {
                    CacheContentTypeOrderBeforeRestore();
                    base.RestoreContentTypes(contentTypeInfos, restoreOption);
                    RestoreContentTypeOrder();
                    #region the property -- required of fieldRef in the default contentType
                    if (!WrapperConfiguration.RestoreDefaultContentTypeRequiredProperty)
                    {
                        log.Debug("WrapperConfiguration.RestoreDefaultContentTypeRequiredProperty is false");
                    }
                    else
                    {
                        if (mAveSPList != null)
                        {
                            try
                            {
                                //
                                Dictionary<string, Dictionary<Guid, bool>> ContentTypeLevelListRequiredCache = new Dictionary<string, Dictionary<Guid, bool>>();
                                var mContentTypeColl = mAveSPList.AveList.ContentTypes;
                                if (null != mContentTypeColl && mContentTypeColl.Count > 0)
                                {
                                    foreach (var mContentType in mContentTypeColl)
                                    {
                                        log.Debug("The ContentType Name is {0}", mContentType.Name);
                                        var fieldLinks = mContentType.FieldLinks;
                                        if (null != fieldLinks && fieldLinks.Count > 0)
                                        {
                                            Dictionary<Guid, bool> fieldLinkRequiredMapping = new Dictionary<Guid, bool>();
                                            //遍历ContentType里的每个FieldLink
                                            foreach (var fieldLink in fieldLinks)
                                            {
                                                //添加到Dictionary中
                                                fieldLinkRequiredMapping[fieldLink.ID] = fieldLink.Required;
                                                log.Debug("fieldLinkId:{0}, Name:{1}, Required:{2}", fieldLink.ID, fieldLink.Name, fieldLink.Required);
                                            }
                                            //将fieldLinkRequiredMapping加入ContentTypeLevelListRequiredCache
                                            ContentTypeLevelListRequiredCache[mContentType.Name] = fieldLinkRequiredMapping;
                                            log.Debug("the fieldLinkRequiredMapping has been add to {0}", mContentType.Name);
                                        }
                                    }
                                    //以上是构造Dictionary<string, Dictionary<Guid, bool>>对象

                                    //因为想Cache中添加的最小单位是一个List下的所有CT的FieldLink集合
                                    //因此这里只要判断Cache的某一个Web中是否包含这个List的Cache
                                    //1，先判断总Cache中是否已经有WebId
                                    //如果没有需要先new
                                    mAveSPSite.MappingManager.SiteMappingManager.AddListFieldRequiredCache(mAveSPWeb.AveWeb.ID, mAveSPList.AveList.ID, ContentTypeLevelListRequiredCache);
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Warn("An error occurred when cache the dafualt contentType fieldLinks,error:{0}", ex.ToString());
                            }
                        }
                    }

                    CacheListRequiredFieldLinks();

                    #endregion
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Warn("You don't have permissions to restore content types. ", ex.Message);
                    report.AddDetail(new AveWrapperReportDto(Title, Title, AveReportObjectType.ListContentType, AveStatus.Skipped, AveReportResource.Wrapper_Report_NoPermissionToRestoreConentTypes, ex.Message));
                }
            }
        }

        private void CacheItemRequiredFieldLink(IAveContentType contentType)
        {
            try
            {
                if (this.mAveSPSite.ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel)
                {
                    CacheRequiredFieldLink(contentType);
                }
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while cache Item required field links {0}", e);
            }
        }

        private void CacheListRequiredFieldLinks()
        {
            try
            {
                if (this.mAveSPSite.ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel)
                {
                    foreach (var contentType in mAveSPList.AveList.ContentTypes)
                    {
                        CacheRequiredFieldLink(contentType);
                    }
                }
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while cache List required field links {0}", e);
            }
        }

        private void CacheRequiredFieldLink(IAveContentType contentType)
        {
            if (!contentType.Sealed)
            {
                bool needUpdate = false;
                var fieldLinks = contentType.FieldLinks;
                if (null != fieldLinks && fieldLinks.Count > 0)
                {
                    foreach (var fieldLink in fieldLinks)
                    {
                        if (fieldLink.Required)
                        {
                            if (mAveSPList.SPList.BaseTemplate == AveListTemplateType.Survey)
                            {
                                log.Warn("The survey list doesn't allow setting of content types. List: {0}. ContentType: {1}, FieldLink: {2}",
                                    mAveSPList.SPList.RootFolder.ServerRelativeUrl, contentType.Name, fieldLink.ID);
                                continue;
                            }
                            if (!ContentTypeHelper.ReqiredFieldCache.ContainsKey(contentType.ID))
                            {
                                ContentTypeHelper.ReqiredFieldCache.Add(contentType.ID, new List<Guid>() { fieldLink.ID });
                            }
                            else
                            {
                                ContentTypeHelper.ReqiredFieldCache[contentType.ID].Add(fieldLink.ID);
                            }
                            fieldLink.Required = false;
                            needUpdate = true;
                        }
                    }
                    if (needUpdate)
                    {
                        contentType.Update();
                    }
                }
            }
        }

        protected override void DeleteExistedContentType()
        {
            for (int index = ContentTypeCollection.Count - 1; index >= 0; index--)
            {
                IAveContentType ct = ContentTypeCollection[index];
                try
                {
                    if (ct.Parent.ID.ToString().Equals("0x0120", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    ContentTypeCollection[index].Delete();
                    log.Debug("The content type: {0} has been deleted", ct.Name);
                }
                catch (Exception e)
                {
                    log.Debug("An error occurred while deleting the existed content type: {0}, error message: {1}.", ct.Name, e);
                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Vti_contenttypeorder is the dictionary key.")]
        private void RestoreContentTypeOrder()
        {
            using (new AvePerformanceScope("Restore.AveSPListContentTypeCollection.RestoreContentTypeOrder"))
            {
                try
                {
                    //这里SPList对象需要重新取，不然SPFolder.UniqueContentTypeOrder会出异常
                    mAveSPList.ReloadList();
                    var mList = mAveSPList.SPList;
                    Dictionary<string, IAveContentType> restoreOrder = new Dictionary<string, IAveContentType>();
                    if (listOrders == null)
                    {
                        List<string> destListOrder = null;
                        var properties = mAveSPList.SPList.RootFolder.Properties;
                        if (properties != null && properties.ContainsKey("vti_contenttypeorder"))
                        {
                            destListOrder = properties["vti_contenttypeorder"].ToString().Split(',').ToList<string>();
                        }
                        //当目的端的SPFolder.UniqueContentTypeOrder没有修改过,且目的端已存在的content type顺序与源端一致则不更新SPFolder.UniqueContentTypeOrder
                        if (!mList.AllowContentTypes || (destListOrder == null && CheckListEquals(oldOrder, newOrder.Values.ToList())))
                        {
                            return;
                        }
                        foreach (IAveContentTypeId id in newOrder.Values.Concat(oldOrder))
                        {
                            try
                            {
                                if (!restoreOrder.ContainsKey(id.ToString()))
                                {
                                    IAveContentType ct = mList.ContentTypes[id];
                                    restoreOrder[ct.ID.ToString()] = ct;
                                }
                            }
                            catch (AveSecurityTrimingException)
                            {
                                throw;
                            }
                            catch (Exception e)
                            {
                                log.Warn("An error occurred while add new content type order." + e.ToString());
                            }
                        }
                    }
                    else
                    {
                        foreach (string id in listOrders)
                        {
                            try
                            {
                                if (newOrder.ContainsKey(id) && newOrder[id] != null)
                                {
                                    IAveContentType ct = mList.ContentTypes[newOrder[id]];
                                    restoreOrder.Add(ct.ID.ToString(), ct);
                                }
                            }
                            catch (AveSecurityTrimingException)
                            {
                                throw;
                            }
                            catch (Exception e)
                            {
                                log.Warn("An error occurred while add new content type order." + e.ToString());
                            }
                        }
                        //将原来的Order中的ContentType中的值添到最后
                        if (mAveSPList.SPList.RootFolder.UniqueContentTypeOrder != null)
                        {
                            foreach (IAveContentType ct in mAveSPList.SPList.RootFolder.UniqueContentTypeOrder)
                            {
                                if (!restoreOrder.ContainsKey(ct.ID.ToString()))
                                {
                                    restoreOrder.Add(ct.ID.ToString(), ct);
                                }
                            }
                        }
                    }
                    //修改ContentOrder的赋值方法，原来为Add()
                    mList.RootFolder.UniqueContentTypeOrder = restoreOrder.Values.ToList();
                    mList.RootFolder.Update();
                }
                catch (AveSecurityTrimingException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    log.Warn("An error occurred while restore content type order." + e.ToString());
                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Vti_contenttypeorder is the metainfo key.")]
        private void CacheContentTypeOrderBeforeRestore()
        {
            using (new AvePerformanceScope("Restore.AveSPListContentTypeCollection.CacheContentTypeOrderBeforeRestore"))
            {
                oldOrder.Clear();
                newOrder.Clear();
                IAveList mList = mAveSPList.SPList;
                listOrders = null;
                try
                {//DOC-72178添加处理ContentTypeOrder
                    if (mAveSPList.ListSettingInfo != null && mAveSPList.ListSettingInfo.RootFolderInfo != null)
                    {
                        var metaInfoDic = mAveSPList.ListSettingInfo.RootFolderInfo.Value.MetaInfoDic;
                        if (metaInfoDic != null && metaInfoDic.Contains("vti_contenttypeorder"))
                        {
                            listOrders = metaInfoDic["vti_contenttypeorder"].ToString().Split(',').ToList<string>();
                        }
                    }
                }
                catch (AveSecurityTrimingException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, "Error when get content type order, Reason:{0}.", e.ToString());
                }
                if (listOrders == null)
                {
                    List<string> destListOrder = null;
                    var properties = mAveSPList.SPList.RootFolder.Properties;
                    if (properties != null && properties.ContainsKey("vti_contenttypeorder"))
                    {
                        destListOrder = properties["vti_contenttypeorder"].ToString().Split(',').ToList<string>();
                    }
                    foreach (IAveContentType ct in mList.ContentTypes)
                    {
                        try
                        {
                            IAveContentTypeId parent = ct.ID.Parent;
                            if (!(parent.ToString().Equals(AveBuiltInContentTypeId.Folder, StringComparison.OrdinalIgnoreCase)) &&
                                !(parent.ToString().Equals(AveBuiltInContentTypeId.UntypedDocument, StringComparison.OrdinalIgnoreCase)))
                            {// 默认的CT 不能加到order里，根据reflector 查看，需要过滤这2个。
                                if (destListOrder != null && !destListOrder.Contains(ct.ID.ToString()))
                                {
                                    continue;
                                }
                                oldOrder.Add(ct.ID);
                            }
                        }
                        catch (AveSecurityTrimingException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            log.Warn("An error occurred while add old content type order." + e.ToString());
                        }
                    }
                }
            }
        }

        protected override void ActionBeforeRestore()
        {
            GetWorkflowStatusFields();
        }

        private void GetWorkflowStatusFields()
        {
            OldStatusFields = new List<string> { };
            try
            {
                if (mAveSPSite != null &&
                    mAveSPSite.ObjectModelFactory != null &&
                    mAveSPSite.ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel &&
                    mAveSPList != null &&
                    mAveSPList.SPList != null)
                {
                    foreach (var field in mAveSPList.SPList.Fields)
                    {
                        if (field.Type == AveFieldType.WorkflowStatus &&
                            !OldStatusFields.Contains(field.InternalName))
                        {
                            OldStatusFields.Add(field.InternalName);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while cache workflow status fields.Error:{0}", e);
            }
        }

        protected override void ActionAfterRestore(AveContentTypeInfo ctInfo, IAveContentType contentType, AveContentTypeRestoreOption restoreOption, Boolean needUpdateDocumentSet)
        {
            using (new AvePerformanceScope("Restore.AveSPListContentTypeCollection.ActionAfterRestore"))
            {
                if (needUpdateDocumentSet && AveSPDocumentSet.IsDocumentSet(mAveSPSite.ObjectModelFactory.CreateContentTypeId(ctInfo.Id)))
                {
                    AveSPDocumentSet ctDocumentSet = new AveSPDocumentSet(ctInfo, contentType, mAveSPList.SPList, mAveSPSite.MappingManager, restoreOption.WEB_CONTENTTYPE_UPDATECHILD);
                    ctDocumentSet.Update();
                }
                if (contentType != null)
                {
                    mAveSPList.ParentSite.MappingManager.ListMappingManager.AddToListLevelCTMapping(ctInfo.Id, contentType);
                }

                #region restore nintex form
                if (ctInfo.NintexFormXmls != null && ctInfo.NintexFormXmls.Count > 0)
                {
                    bool needReload = false;
                    int i = 0;
                    INintexFormService service = NintexFormServiceBase.CreateNintexForm(mAveSPList.SPList, mAveSPWeb, false);
                    try
                    {
                        service.DeleteForm(mAveSPList.SPList.ID.ToString("B"), ctInfo.Id);
                    }
                    catch(Exception ex)
                    {
                        log.Error("Failed to delete nintex form. list id: {0}, content type id: {1}, exception: {2}", mAveSPList.SPList.ID, ctInfo.Id, ex);
                    }
                    try
                    {
                        for (; i < ctInfo.NintexFormXmls.Count; ++i)
                        {                            
                            service.RestoreForm(ctInfo.NintexFormXmls[i], contentType.ID.ToString());
                            needReload = true;
                        }
                        log.Info("Success to restore nintex form in content type:{0} of list:{1}", contentType.ID.ToString(), mAveSPList.SPList.Title);
                    }
                    catch(AveNintexFormPostException e)
                    {
                        log.Debug("A known issue happend during restore nintex form. add it to site post action. contentTypeId: {0}, list title:{1}, error: {2}", contentType.ID, mAveSPList.SPList.Title, e);
                        mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.CacheNintexFormsDataFormSitePostAction(mAveSPWeb.SPWeb.ServerRelativeUrl, mAveSPList.SPList.ID, contentType.ID.ToString(), ctInfo.NintexFormXmls.GetRange(i, ctInfo.NintexFormXmls.Count - i));
                    }
                    catch (Exception e)
                    {
                        log.Warn(WrapperRestoreResource.RestoreNintexFormFailed, contentType.ID.ToString(), mAveSPList.SPList.ID, e.ToString());
                    }
                    finally
                    {
                        if (needReload)
                        {
                            //Publish Form之后需要Reload所有，单ReloadList不好使。
                            mAveSPList.ReloadAll();
                        }
                    }
                }
                #endregion
            }
        }

        protected override void AddContentTypeMappings(AveContentTypeInfo ctInfo, IAveContentType contentType, string sourceContentTypeName)
        {
            base.AddContentTypeMappings(ctInfo, contentType, sourceContentTypeName);
            if (contentType == null && !restoredCTFailedCache.ContainsKey(ContentTypeHelper.GetContentTypeId(ctInfo.Id)))
            {
                restoredCTFailedCache.Add(ContentTypeHelper.GetContentTypeId(ctInfo.Id), ctInfo.Id);
            }
        }

        public IAveContentType Find(AveContentTypeInfo ctInfo, AveContentTypeRestoreOption restoreOption)
        {
            using (new AvePerformanceScope("Restore.AveSPListContentTypeCollection.Find"))
            {
                IAveContentType contentType = null;
                ContentTypeFindOption[] findOption = restoreOption.FindOption;
                if (!string.IsNullOrEmpty(ctInfo.MappingName))
                {
                    findOption = new ContentTypeFindOption[] { ContentTypeFindOption.FindByName };
                    if (restoreOption.CreateContentTypeWithParentFindByMappingName)
                    {
                        CreateParentContentTypeInfoForMapping(ctInfo);
                    }
                }

                foreach (ContentTypeFindOption option in findOption)
                {
                    contentType = FindContentTypeByOption(ctInfo, ContentTypeCollection, option);
                    if (contentType == null && option == ContentTypeFindOption.FindByParent)
                    {
                        IAveContentType parent = null;
                        if (GetParentContentTypeByDefault(ctInfo, ref parent, false, restoreOption))
                        {
                            if (parent != null)
                            {
                                contentType = ContentTypeHelper.FindChildContentTypeInCollection(ContentTypeCollection, parent.ID);
                                if (contentType != null && restoredContentTypeIdMapping.ContainsKey(contentType.ID.ToString()))
                                {
                                    contentType = null;
                                }
                            }
                        }
                    }
                    if (contentType != null)
                    {
                        break;
                    }
                }
                return contentType;
            }
        }

        private void CreateParentContentTypeInfoForMapping(AveContentTypeInfo sourceCTInfo)
        {
            using (new AvePerformanceScope("Restore.AveSPContentTypeCollection.CreateParentContentTypeInfoForMapping"))
            {
                try
                {
                    if (!string.IsNullOrEmpty(sourceCTInfo.MappingName))
                    {
                        IAveContentType webContentType = mAveSPWeb.SPWeb.AvailableContentTypes[sourceCTInfo.MappingName];
                        if (webContentType != null)
                        {
                            if (ContentTypeHelper.IsBaseBuiltinContentTypeMatch(ContentTypeHelper.GetContentTypeId(sourceCTInfo.Id), webContentType.ID, true))
                            {
                                AveContentTypeInfo ctInfo = new AveContentTypeInfo();
                                ctInfo.Name = webContentType.Name;
                                ctInfo.Id = webContentType.ID.ToString();
                                ctInfo.MappingName = sourceCTInfo.MappingName;
                                ctInfo.ReadOnly = webContentType.ReadOnly;
                                ctInfo.Description = webContentType.Description;
                                ctInfo.FieldsSchemaXml = webContentType.Fields.SchemaXml;
                                ctInfo.DocumentTemplate = webContentType.DocumentTemplate;
                                ctInfo.Group = webContentType.Group;
                                ctInfo.DisplayFormTemplateName = webContentType.DisplayFormTemplateName;
                                ctInfo.DisplayFormUrl = webContentType.DisplayFormUrl;
                                ctInfo.DocumentTemplateUrl = webContentType.DocumentTemplateUrl;
                                ctInfo.EditFormTemplateName = webContentType.EditFormTemplateName;
                                ctInfo.EditFormUrl = webContentType.EditFormUrl;
                                ctInfo.Hidden = webContentType.Hidden;
                                ctInfo.NewFormTemplateName = webContentType.NewFormTemplateName;
                                ctInfo.NewFormUrl = webContentType.NewFormUrl;
                                sourceCTInfo.ParentContentTypeInfo = ctInfo;
                                sourceCTInfo.ParentName = sourceCTInfo.MappingName;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Debug("An error occurred while creating parent ContentType. Message:" + e.ToString());
                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100013:CheckExistingExceptionHandlingBlocks")]
        public IAveContentType EnsureContentType(string contentTypeId, AveContentTypeRestoreOption restoreOption, bool throwWhenNotFound, bool throwWhenConflict, bool needUpdateIfExist)
        {
            using (new AvePerformanceScope("Restore.AveSPListContentTypeCollection.EnsureContentType"))
            {
                report.Dispose();
                IAveContentType ct = null;
                if (mAveSPList.AveFields.NeedReloadfieldsIfCreateMetadataField)
                {
                    mAveSPList.AveFields.NeedReloadfieldsIfCreateMetadataField = false;
                    ContentTypeHelper.InitMetadataFieldAndTextFieldMapping(true);
                }
                if (!ContentTypeCache.ContainsKey(contentTypeId))
                {
                    return null;
                }
                bool isHighVersionToLowVersion = new Version(this.mAveSPSite.SourceSiteInfo.SPVersion).Major > new Version(this.mAveSPSite.SPSite.SPVersion).Major;
                AveContentTypeInfo ctInfo = ContentTypeCache[contentTypeId];
                string mappingName = ContentTypeMapping.GetContentTypeNameMappingFromGui(ctInfo.Name);
                if (!mappingName.Equals(ctInfo.Name, StringComparison.OrdinalIgnoreCase))
                {
                    ctInfo.MappingName = mappingName;
                }
                string id = ctInfo.Id;

                if (EnsuredContentTypeResult.ContainsKey(id))
                {
                    Exception exception = EnsuredContentTypeResult[id];
                    if (exception != null)
                    {
                        throw exception;
                    }
                    string mappingId = ContentTypeMapping.GetMappingRestoredContentTypeId(id);
                    if (!string.IsNullOrEmpty(mappingId))
                    {
                        id = mappingId;
                    }
                    ct = ContentTypeHelper.FindContentTypeById(ContentTypeCollection, ContentTypeHelper.GetContentTypeId(id));
                    if (needUpdateIfExist)
                    {
                        ContentTypeHelper.UpdateContentType(ContentTypeCollection, ct, ctInfo, AllFieldCollection, false, restoreOption, isHighVersionToLowVersion);
                    }
                    return ct;
                }

                Exception schemaDependencyError = null;
                try
                {
                    string mappingId = ContentTypeMapping.GetMappingRestoredContentTypeId(id);
                    if (String.IsNullOrEmpty(mappingId) || !restoredContentTypeIdMapping.Keys.Contains(mappingId))
                    {
                        if (restoredCTFailedCache.ContainsValue(id) && throwWhenConflict)
                        {
                            throw new AveSchemaDependencyConflictException(ctInfo.Name, "content type");
                        }
                        if (ContentTypeResult.ContainsKey(ctInfo.Name) && ContentTypeResult[ctInfo.Name].RestoreOption.Equals(ContentTypeConflictHandleOption.Skip))
                        {
                            throw new AveSchemaDependencyNotFoundException(ctInfo.Name, "content type");
                        }
                        ct = Find(ctInfo, restoreOption);
                        ct = RestoreSingleContentType(ctInfo, ct, restoreOption, throwWhenNotFound, throwWhenConflict, true);
                    }
                    else
                    {
                        if (ContentTypeResult.ContainsKey(ctInfo.Name) && ContentTypeResult[ctInfo.Name].RestoreOption.Equals(ContentTypeConflictHandleOption.Skip))
                        {
                            throw new AveSchemaDependencyNotFoundException(ctInfo.Name, "content type");
                        }
                        ct = ContentTypeHelper.FindContentTypeById(ContentTypeCollection, ContentTypeHelper.GetContentTypeId(mappingId));
                        if (ct == null)
                        {
                            ct = Find(ctInfo, restoreOption);
                            ct = RestoreSingleContentType(ctInfo, ct, restoreOption, throwWhenNotFound, throwWhenConflict, true);
                        }
                        if (needUpdateIfExist)
                        {
                            ContentTypeHelper.UpdateContentType(ContentTypeCollection, ct, ctInfo, AllFieldCollection, false, restoreOption, isHighVersionToLowVersion);
                        }
                    }
                }
                catch (Exception e)
                {
                    schemaDependencyError = e;
                    throw;
                }
                finally
                {
                    if (!this.EnsuredContentTypeResult.ContainsKey(ctInfo.Id))
                    {
                        this.EnsuredContentTypeResult.Add(ctInfo.Id, schemaDependencyError);
                    }
                }
                CacheItemRequiredFieldLink(ct);
                return ct;
            }
        }

        public override IAveContentType CreateContentType(AveContentTypeInfo ctInfo, AveContentTypeRestoreOption restoreOption)
        {
            using (new AvePerformanceScope("Restore.AveSPListContentTypeCollection.CreateContentType"))
            {
                bool isConflictById = ContentTypeHelper.IsListContentTypeIdExist(ctInfo.Id);
                var ct = base.CreateContentType(ctInfo, restoreOption, isConflictById);
                //如果目的端存在同Id的CT，不会Keep Id，不存在则Keep Id，因此删除WF不会破坏目的端
                RemoveAllWorkflowAssociation(ct);
                return ct;
            }
        }

        private void RemoveAllWorkflowAssociation(IAveContentType ct)
        {
            if (ct != null)
            {
                //status field internal name 集合，用于之后删除status field column
                List<string> statusFieldNames = new List<string> { };
                var associationCollection = ct.WorkflowAssociations;
                int count = associationCollection.Count;
                StringBuilder debugLog = new StringBuilder();
                try
                {
                    debugLog.AppendFormat("[ContentTypeName:{0}],[ContentTypeId:{1}]", ct.Name, ct.ID);
                    if (count > 0)
                    {
                        for (int k = count - 1; k >= 0; k--)
                        {
                            IAveWorkflowAssociation asso = associationCollection[k];
                            string fieldName = asso.InternalNameStatusField;
                            debugLog.AppendFormat("[WorkflowName:{0},StatusFieldName:{1}]", asso.Name, fieldName);
                            if (!string.IsNullOrEmpty(fieldName) && !statusFieldNames.Contains(fieldName))
                            {
                                statusFieldNames.Add(asso.InternalNameStatusField);
                            }
                            //associationCollection.Remove(asso);
                        }
                        associationCollection.RemoveAll();
                        if (mAveSPSite != null &&
                            mAveSPSite.ObjectModelFactory != null &&
                            mAveSPSite.ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel)
                        {
                            //365无法获取Association的Status Field,所以在还原CT前缓存status fields，然后还原后check是否有新创建的status field，有的话在remove association后将field记录下来等待删除
                            foreach (var field in mAveSPList.SPList.Fields)
                            {
                                if (field.Type == AveFieldType.WorkflowStatus &&
                                    OldStatusFields != null &&
                                    !OldStatusFields.Contains(field.InternalName) &&
                                    !statusFieldNames.Contains(field.InternalName))
                                {
                                    debugLog.AppendFormat("[statusFieldName:{0}]", field.InternalName);
                                    statusFieldNames.Add(field.InternalName);
                                }
                            }
                        }
                        if (statusFieldNames.Count > 0)
                        {
                            bool needReload = false;
                            foreach (string name in statusFieldNames)
                            {
                                try
                                {
                                    IAveField field = mAveSPList.SPList.Fields.GetFieldByInternalName(name, false);
                                    if (field != null)
                                    {
                                        bool needUpdate = false;
                                        if (field.ReadOnlyField)
                                        {
                                            field.ReadOnlyField = false;
                                            needUpdate = true;
                                        }
                                        if (field.AllowDeletion == null || !(bool)field.AllowDeletion)
                                        {
                                            field.AllowDeletion = true;
                                            needUpdate = true;
                                        }
                                        if (needUpdate)
                                        {
                                            field.Update();
                                        }
                                        debugLog.AppendFormat("[Delete workflow status field:{0}]", name);
                                        field.Delete();
                                        needReload = true;
                                    }
                                }
                                catch (Exception e)
                                {
                                    log.Warn("An error occurred while delete workflow status field {0} from contentType: {1}. Error: {2}", name, ct.Name, e);
                                }
                            }
                            if (needReload)
                            {
                                //还原contentType过程中删除workflow status column后，SPList的version会高于ContentTypeCollection的version，导致update contentType会出错，所以需要在此处reload
                                //如果有更好的方法避免这个问题，此处的reload就可以去掉
                                mAveSPList.SPList.Reload();
                                ct = ContentTypeCollection[ct.ID];
                            }
                        }
                    }
                }
                finally
                {
                    log.Debug(debugLog.ToString());
                }
            }
        }

        private bool CheckListEquals(List<IAveContentTypeId> listA, List<IAveContentTypeId> listB)
        {
            try
            {
                for (int i = 0; i < listA.Count; i++)
                {
                    if (!listA[i].Equals(listB[i]))
                    {
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.CheckListEqualError, e.ToString());
            }
            return true;
        }
    }
    [AveCodeReview("2012/04/13", "yuzhi.jiang@avepoint.com", "jin.zhang@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CS_1, CodeReviewConstants.CHECK_LIST_ID_EH_2 }, null, true)]
    [AveCodeReview("2012/10/19", "fengfu.zhang@avepoint.com", "fengfu.zhang@avepoint.com", new string[3] { CodeReviewConstants.CHECK_LIST_ID_CO_8, CodeReviewConstants.CHECK_LIST_ID_FA_4, CodeReviewConstants.CHECK_LIST_ID_FA_10 }, null, true)]
    public class AveSPCTHubContentTypeCollection : AveSPContentTypeCollection, AvePoint.Wrapper.Restore.IAveSPCTHubContentTypeCollection
    {
        protected override string ServerRelativeUrl
        {
            get { return mAveSPWeb.SPWeb.ServerRelativeUrl; }
        }
        protected override string Title
        {
            get { return mAveSPWeb.SPWeb.Title; }
        }
        protected override AveReportObjectType ObjectType
        {
            get { return AveReportObjectType.WebContentType; }
        }
        protected override IAveContentTypeCollection ContentTypeCollection
        {
            get { return mAveSPWeb.SPWeb.ContentTypes; }
        }
        protected override IAveContentTypeCollection AllContentTypeCollection
        {
            get { return mAveSPWeb.SPWeb.AvailableContentTypes; }
        }
        protected override IAveFieldCollection AllFieldCollection
        {
            get { return mAveSPWeb.SPWeb.AvailableFields; }
        }
        public AveSPCTHubContentTypeCollection(AveSPSite aveSPSite)
        {
            mAveSPSite = aveSPSite;
            mAveSPWeb = new AveSPWeb(aveSPSite, ".", true);
        }

        #region << Run ContentType Hub Timer Job Now >>
        /// <summary>
        /// 提供给外围允许立即运行关联的TimerJob
        /// 使得新增的ContentType可以立刻Push下去
        /// </summary>
        public void ContentTypeHubTimerJobRunNow(string serviceName)
        {
            using (new AvePerformanceScope("Restore.AveSPCTHubContentTypeCollection.ContentTypeHubTimerJobRunNow"))
            {
                //Property
                bool findedJob = false;
                IAveJobDefinition currentJob = null;
                DateTime preLastRunTime = DateTime.MaxValue;
                DateTime timeFlag = DateTime.Now;

                //①：Content Type Hub Timer Job
                try
                {
                    //1：Find
                    foreach (IAveService service in mAveSPSite.ObjectModelFactory.CreateFarm().Local.Services)
                    {
                        foreach (IAveJobDefinition job in service.JobDefinitions)
                        {
                            //暂时根据Name来判断，多语言应该会在DisplayName上有所变化
                            if (job.Name.Equals("MetadataHubTimerJob", StringComparison.OrdinalIgnoreCase))
                            {
                                findedJob = true;
                                currentJob = job;
                                break;
                            }
                        }

                        //找到后直接跳出
                        if (findedJob)
                        {
                            break;
                        }
                    }
                    //2：RunNow
                    if (findedJob)
                    {
                        preLastRunTime = currentJob.LastRunTime;
                        timeFlag = DateTime.Now;

                        currentJob.RunNow();
                    }

                    //3: 循环等待Run真正结束
                    if (preLastRunTime != DateTime.MaxValue)
                    {
                        while (true)
                        {
                            //After Complete Break
                            if (currentJob.LastRunTime > preLastRunTime)
                            {
                                break;
                            }
                            //超时20分钟
                            if (timeFlag.AddMinutes(20) < DateTime.Now)
                            {
                                log.Warn("Refresh metadata service hub timer job time out. metadata service name:{0}.", serviceName);
                                break;
                            }
                            Thread.Sleep(500);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Update content type hub timer job failed. " + ex.ToString());
                }

                //②：Content Type Subscriber Timer Job
                try
                {
                    List<IAveWebApplication> webAppGroup = new List<IAveWebApplication>();
                    foreach (IAveWebApplication webApp in mAveSPSite.ObjectModelFactory.CreateWebService().ContentService.WebApplications)
                    {
                        foreach (IAveServiceApplicationProxy proxy in webApp.ServiceApplicationProxyGroup.Proxies)
                        {
                            if (proxy.Name.Equals(serviceName, StringComparison.OrdinalIgnoreCase) && !webAppGroup.Contains(webApp))
                            {
                                webAppGroup.Add(webApp);
                                break;
                            }
                        }
                    }
                    //1: Find
                    foreach (IAveWebApplication webApp in webAppGroup)
                    {
                        //Clear Property
                        currentJob = null;
                        preLastRunTime = DateTime.MaxValue;
                        foreach (IAveJobDefinition job in webApp.JobDefinitions)
                        {
                            //暂时根据Name来判断，多语言应该会在DisplayName上有所变化
                            if (job.Name.Equals("MetadataSubscriberTimerJob", StringComparison.OrdinalIgnoreCase))
                            {
                                currentJob = job;
                                break;
                            }
                        }
                        //2: RunNow
                        if (currentJob != null)
                        {
                            preLastRunTime = currentJob.LastRunTime;
                            timeFlag = DateTime.Now;
                            currentJob.RunNow();
                        }

                        //3: 循环等待Run真正结束
                        if (preLastRunTime != DateTime.MaxValue)
                        {
                            while (true)
                            {
                                //After Complete Break
                                if (currentJob.LastRunTime > preLastRunTime)
                                {
                                    break;
                                }
                                //超时20分钟
                                if (timeFlag.AddMinutes(20) < DateTime.Now)
                                {
                                    log.Warn("Refresh current timer job time out. web application name:{0}.", webApp.DisplayName);
                                    break;
                                }
                                Thread.Sleep(500);
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    log.Error("Update content type sub scriber timer job failed. " + ex.ToString());
                }
            }
        }
        #endregion << Run ContentType Hub Timer Job Now >>

        public override void RestoreContentTypes(AveContentTypeCollectionInfo contentTypeInfo, Dictionary<string, string> customerRenameTable, AveContentTypeRestoreOption restoreOption)
        {
            mAveSPWeb.ParentSite.SourceSiteInfo = contentTypeInfo.SourceSiteInfo;
            base.RestoreContentTypes(contentTypeInfo, customerRenameTable, restoreOption);
        }

        protected override void SetContentTypeInfo(AveContentTypeInfo ctInfo)
        {
            base.SetContentTypeInfo(ctInfo);
            mAveSPWeb.Fields.RestoreFields(ctInfo.SchemaXml, new AveFieldRestoreOption());
        }

        protected override IAveContentType HandleConflict(AveContentTypeInfo ctInfo, IAveContentType contentType, AveContentTypeRestoreOption restoreOption, bool isHighVersionToLowVersion = false)
        {
            HandleConflict(ctInfo, ref contentType, restoreOption, isHighVersionToLowVersion);
            mAveSPWeb.SPWeb.AvailableContentTypes.IsDirty = true;
            return contentType;
        }

        protected override IAveContentType CreateNewContentType(AveContentTypeInfo ctInfo, AveContentTypeRestoreOption restoreOption, bool throwWhenNotFound, bool isHighVersionToLowVersion = false)
        {
            using (new AvePerformanceScope("Restore.AveSPCTHubContentTypeCollection.CreateNewContentType"))
            {
                IAveContentType contentType = null;
                ctInfo.Name = ContentTypeHelper.GetAvailableContentTypeName(ctInfo, AllContentTypeCollection, ref contentType);
                contentType = CreateContentType(ctInfo, restoreOption);
                if (contentType != null)
                {
                    string exception = ContentTypeHelper.UpdateContentType(ContentTypeCollection, contentType, ctInfo, AllFieldCollection, true, restoreOption, isHighVersionToLowVersion);
                    ContentTypeResult[ctInfo.Name].FailedException = exception;
                }
                return contentType;
            }
        }

        protected override void ActionAfterRestore(AveContentTypeInfo ctInfo, IAveContentType contentType, AveContentTypeRestoreOption restoreOption, Boolean needUpdateDocumentSet = true)
        {
            using (new AvePerformanceScope("Restore.AveSPCTHubContentTypeCollection.ActionAfterRestore"))
            {
                if (null != contentType)
                {//记录非冲突Report
                    if (!ContentTypeResult.ContainsKey(ctInfo.Name))
                    {
                        //ContentTypeResult.Add(ctInfo.Name, new ContentTypeRestoreReport(ContentTypeConflictHandleOption.Skip));
                        ContentTypeResult.Add(ctInfo.Name, new ContentTypeRestoreReport(ContentTypeConflictHandleOption.None));
                        ContentTypeResult[ctInfo.Name].RestoreName = contentType.Name;
                    }
                    else
                    {
                        ContentTypeResult[ctInfo.Name].RestoreName = contentType.Name;
                    }
                    //Push hub content type
                    // IAveContentTypePublisher aveCTPublisher = mAveSPSite.ObjectModelFactory.CreateContentTypePublisher(mAveSPSite.SPSite);
                    // PublishContentType(aveCTPublisher, ctInfo, contentType);
                }
                if (AveSPDocumentSet.IsDocumentSet(mAveSPSite.ObjectModelFactory.CreateContentTypeId(ctInfo.Id)))
                {
                    mAveSPWeb.ParentSite.MappingManager.WebMappingManager.DocumentSetCTCache.Add(ctInfo);
                }
            }
        }

        public override IAveContentType CreateContentType(AveContentTypeInfo ctInfo, AveContentTypeRestoreOption restoreOption)
        {
            using (new AvePerformanceScope("Restore.AveSPCTHubContentTypeCollection.CreateContentType"))
            {
                bool isConflictById = false;
                if (mAveSPSite.ObjectModelFactory.ContextKind.IsServerMode10Upper())
                {
                    if (ContentTypeCollection.CheckIfContentTypeExistInChildren(mAveSPSite.SPSite.ID, mAveSPWeb.ServerRelativeUrl, ctInfo.Id))
                    {
                        isConflictById = true;
                    }
                }
                return base.CreateContentType(ctInfo, restoreOption, isConflictById);
            }
        }

        public void PublishContentType(IAveContentTypePublisher aveCTPublisher, AveContentTypeInfo ctInfo, IAveContentType contentType)
        {
            using (new AvePerformanceScope("Restore.AveSPCTHubContentTypeCollection.PublishContentType"))
            {
                if (ctInfo.IsUnPublished)
                {
                    aveCTPublisher.Unpublish(contentType);
                }
                if (ctInfo.IsPublished)
                {
                    aveCTPublisher.Publish(contentType);
                }
            }
        }
    }

}