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
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Common;
using System.Xml;
using AvePoint.Wrapper.Core.SPBackup;
using AvePoint.Wrapper.Core.SPBackupDto;
using AvePoint.Wrapper.Common.Office;
using System.Linq;
using LS.SPWorkflowProcessor;

namespace AvePoint.Wrapper.Backup
{
    public class AveSPWeb : IDisposable, AvePoint.Wrapper.Backup.IAveSPWeb, ISPWebExport
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private AveSPSite mAveSPSite = null;
        private IAveWeb mSPWeb = null;
        private IAveBackupStream mSender = null;
        private IAveBackupRestoreQueryService mQueryService = null;
        private Guid mId;
        private string mName;
        private string mScope = string.Empty;
        private long mDataSize = 0;
        private Guid mScopeId;
        private string mTaxonomyList;
        private string timeZoneInfoId;
        private Dictionary<Guid, Dictionary<string, string>> mContentTypesInfoIncludeNintexForm;
        private IList<IAveAppInstance> nintexFormInstance;

        public string TaxonomyList
        {
            get { return mTaxonomyList; }
            set { mTaxonomyList = value; }
        }

        protected DateTime mModifyTime;

        public DateTime ModifyTime
        {
            get { return mModifyTime; }
        }

        public IAveBackupRestoreQueryService QueryService
        {
            get { return mQueryService; }
        }

        public IAveBackupStream Sender
        {
            get { return mSender; }
        }

        public AveSPSite ParentSite
        {
            get { return mAveSPSite; }
        }

        public string ScopeString
        {
            get { return mScope; }
        }

        public Guid ScopeId
        {
            get { return mScopeId; }
        }

        public Dictionary<int, object> MicroFeedCache
        {
            get { return GetMicroFeedCache(); }
        }

        public AveSPWeb(AveSPSite _AveSite, Guid _WebId, string _name, bool enableReloadForTimeout = true)
        {
            using (new AvePerformanceScope("Backup.AveSPWeb.Constructor"))
            {
                mAveSPSite = _AveSite;
                mSender = _AveSite.Sender;
                mQueryService = _AveSite.QueryService;
                mId = _WebId;
                mName = _name;
                if (enableReloadForTimeout)
                {
                    mReloadWebAndParentForSPRequestTimeout = ReloadWebAndParentInternalForSPRequestTimeout;
                    if (mReloadWebAndParentForSPRequestTimeout != null)
                    {
                        mReloadWebAndParentForSPRequestTimeout(false);
                    }
                }
                mSPWeb = mAveSPSite.SPSite.OpenWeb(mId);//.AllWebs[mId];
                mScope = mSPWeb.ServerRelativeUrl.Substring(1);
                mScopeId = mSPWeb.RoleAssignments.ID;
                //GetTaxonomyList();
                //mReloadWebAndParentForSPRequestTimeout = ReloadWebAndParentInternalForSPRequestTimeout;
                //mLog.Debug("Current user{0}\\{1}", System.Environment.UserDomainName, System.Environment.UserName);
            }
        }

        public AveSPWeb(ISPSiteExport backupSite, IAveWeb web)
            : this((AveSPSite)backupSite, web.ID, web.Title)
        {
        }

        private void GetTaxonomyList()
        {
            if (mSPWeb.IsRootWeb && mSPWeb.Properties.ContainsKey("TaxonomyHiddenList"))
            {
                mTaxonomyList = mSPWeb.Properties["TaxonomyHiddenList"];
            }
        }

        public bool HasUniqueRoleAssignments
        {
            get { return mSPWeb.HasUniqueRoleAssignments; }
        }

        public bool HasUniqueRoleDefinitions
        {
            get { return mSPWeb.HasUniqueRoleDefinitions; }
        }

        internal Action<bool> mReloadWebAndParentForSPRequestTimeout;

        public void SetReloadWebAndParentForSPRequestTimeout(Action<bool> reloadMethod)
        {
            mReloadWebAndParentForSPRequestTimeout = reloadMethod;
        }

        internal Dictionary<Guid, Dictionary<string, string>> ContentTypesInfoIncludeNintexForm
        {
            get
            {
                if (mContentTypesInfoIncludeNintexForm == null)
                {
                    mContentTypesInfoIncludeNintexForm = new Dictionary<Guid, Dictionary<string, string>>();

                    var nintexFormLibrayId = this.SPWeb.Properties.ContainsKey("nintexformslibraryid")
                        && AveTypeHelper.IsGuid(this.SPWeb.Properties["nintexformslibraryid"]) ?
                        new Guid(this.SPWeb.Properties["nintexformslibraryid"]) : Guid.Empty;
                    if (nintexFormLibrayId == Guid.Empty)
                    {
                        mLog.Info("No nintex forms in this web. Url: {0}", this.SPWeb.Url);
                        return mContentTypesInfoIncludeNintexForm;
                    }
                    IAveList nintexFormsList = this.SPWeb.Lists[nintexFormLibrayId];
                    if (nintexFormsList != null)
                    {
                        foreach (var file in nintexFormsList.RootFolder.Files)
                        {
                            if (file.Item["FormListId"] == null || !Validator.IsGuid(file.Item["FormListId"].ToString()))
                            {
                                mLog.Warn("Can not find form list id, form file url is: {0}",file.ServerRelativeUrl);
                                continue;
                            }
                            Guid listId = new Guid(file.Item["FormListId"].ToString());
                            
                            Dictionary<string, string> contentTypesId;
                            string contentTypeId = file.Item["FormContentTypeId"] == null ? string.Empty : file.Item["FormContentTypeId"].ToString();
                            if (!string.IsNullOrEmpty(contentTypeId))
                            {
                                if (mContentTypesInfoIncludeNintexForm.TryGetValue(listId, out contentTypesId))
                                {
                                    if (contentTypesId.ContainsKey(contentTypeId))
                                    {
                                        mLog.Warn("Duplicate nintex files in the same content types, content type id: {0}, file url: {1}",contentTypeId,file.ServerRelativeUrl);
                                    }
                                    contentTypesId[contentTypeId] = file.ServerRelativeUrl;
                                }
                                else
                                {
                                    contentTypesId = new Dictionary<string, string>();
                                    contentTypesId.Add(contentTypeId, file.ServerRelativeUrl);
                                }
                                mContentTypesInfoIncludeNintexForm[listId] = contentTypesId;
                            }
                        }
                    }
                }
                return mContentTypesInfoIncludeNintexForm;
            }
        }

        internal bool IfNintexFormInstanceInstalled
        {
            get
            {
                if (ParentSite.SPSite.IsOnlineSite)
                {
                    if (nintexFormInstance == null)
                    {
                        nintexFormInstance = this.SPWeb.GetAppInstancesByProductId(new Guid("353e0dc9-57f5-40da-ae3f-380cd5385ab9"));
                    }
                    return nintexFormInstance.Count > 0;
                }
                else
                {
                    return false;
                }
            }
        }
        /// <summary>
        /// 如果程序运行一天以上，访问Web的一些属性，例如WebPartManager或者CreatList对象，都会出现如下错误：
        /// System.Runtime.InteropServices.COMException (0x80090317): The context has expired and can no longer be used.
        /// </summary>
        /// <param name="ingoreTimeout"></param>
        internal void ReloadWebAndParentInternalForSPRequestTimeout(bool ingoreTimeout)
        {
            if (ingoreTimeout || ParentSite.mSPRequestTimeout.AddHours(ParentSite.mHoursReloadSite) < DateTime.UtcNow)
            {
                this.ParentSite.ReloadSite();
                this.ReloadWeb();
            }
        }

        public void ReloadWeb()
        {
            try
            {
                if (mSPWeb != null)
                {
                    mSPWeb.ReloadWeb();
                }
                //InitializeMembers();
            }
            catch (Exception e)
            {
                mLog.Log(AveLogLevel.WARN, string.Format("Reload web failed. web name:{0}\n error message:{1}", mName, e));
            }
        }

        private void InitializeMembers()
        {
            mId = mSPWeb.ID;
            mScope = mSPWeb.ServerRelativeUrl.Substring(1);
        }

        public string TimeZoneInfoId
        {
            get
            {
                if (string.IsNullOrEmpty(this.timeZoneInfoId))
                {
                    this.timeZoneInfoId = AveTimeZoneUtility.ToTimeZoneInfoId(SPWeb.RegionalSettings.TimeZone.ID);
                }
                return this.timeZoneInfoId;
            }
        }

        public void Dispose()
        {
            mSPWeb.Dispose();
        }

        #region IAveSPWeb Members

        public IAveWeb SPWeb
        {
            get { return mSPWeb; }
        }

        IAveSPSite IAveSPWeb.ParentSite
        {
            get { return mAveSPSite; }
        }

        public string Name
        {
            get { return mName; }
        }

        public void ExportBaseInfo(IAveBackupStream output)
        {
            var webInfo = new AveSPWebInfo(this);
            webInfo.Export(output);
        }

        /// <summary>
        /// PR Item is virtual site
        /// </summary>
        public void ExportBaseInfo(IAveBackupStream output, string url)
        {
            var webInfo = new AveSPWebInfo(this);
            var result = webInfo.GetWebInfo();
            result.Url = url;
            output.WriteMetadata(AveMetadataType.WebBasicInfo, result);
        }

        public void ExportFeatures(IAveBackupStream output)
        {
            var featureManager = AveSPFeature.CreateInstance(this);
            featureManager.Export(output);
        }

        public void ExportSettings(IAveBackupStream output)
        {
            AveBackupOption option = new AveBackupOption();
            ExportSettings(output, option);
        }

        public void ExportSettings(IAveBackupStream output, AveBackupOption option)
        {
            var webSettinginfo = new AveSPWebSettingInfo(this, option);
            webSettinginfo.Export(output);
        }

        public void ExportLanguageInfo(IAveBackupStream output)
        {
            if (this.mAveSPSite.SPContextKind.IsServerMode())
            {
                var languageResFile = AveLanguage.CreateInstance(this);
                languageResFile.Export(output);
            }
            else
            {
                //ADO-61291 
                output.WriteMetadata(AveMetadataType.LanguageFile, new AveLanguageInfo() { LanguageLCD = this.SPWeb.Language });//client虽不用加载资源文件，但是在后面需用到LanguageFile，进行LoadXML
            }
        }

        public void ExportFields(IAveBackupStream output, AveBackupOption backupColumnOption = null)
        {
            var fields = AveSPFieldCollection.CreateInstance(this);
            if (backupColumnOption == null)
            {
                backupColumnOption = new AveBackupOption();
            }
            fields.Export(output, backupColumnOption);
        }

        public void ExportFields(IAveBackupStream output, List<string> filterFields)
        {
            if (filterFields == null)
            {
                ExportFields(output);
            }
            else
            {
                var backupColumnOption = new AveBackupOption();
                backupColumnOption.BeforeExportFieldsAction = new Action<AveFieldCollectionInfo>(info => FilterFields(filterFields, info));
                ExportFields(output, backupColumnOption);
            }
        }

        private void FilterFields(List<string> filterFields, AveFieldCollectionInfo info)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(info.AveSchemaXml);
            for (int i = doc.DocumentElement.ChildNodes.Count - 1; i >= 0; --i)
            {
                var fieldXe = doc.DocumentElement.ChildNodes[i] as XmlElement;
                if (fieldXe != null)
                {
                    if (!filterFields.Contains(fieldXe.GetAttribute("Name")))
                    {
                        doc.DocumentElement.RemoveChild(fieldXe);
                    }
                }
            }
            info.AveSchemaXml = doc.OuterXml;
        }

        public void ExportContentTypes(IAveBackupStream output, List<string> filterContentTypes = null)
        {
            var option = new AveBackupOption();

            if (filterContentTypes != null && filterContentTypes.Count > 0)
            {
                option.BeforeExportContentTypesAction = new Action<AveContentTypeCollectionInfo>(info => FilterContentType(filterContentTypes, info));
            }
            this.ExportContentTypes(output, option);
        }

        private static void FilterContentType(List<string> filterContentTypes, AveContentTypeCollectionInfo result)
        {
            for (int i = result.ContentTypes.Count - 1; i >= 0; --i)
            {
                if (!filterContentTypes.Contains(result.ContentTypes[i].Name))
                {
                    result.ContentTypes.RemoveAt(i);
                }
            }
        }

        public void ExportContentTypes(IAveBackupStream output, AveBackupOption backupContentTypeOption)
        {
            var contentTypes = AveSPContentTypeCollection.CreateInstance(this);
            var result = contentTypes.GetContentTypeCollectionInfoObj();
            if (backupContentTypeOption != null && backupContentTypeOption.BeforeExportContentTypesAction != null)
            {
                backupContentTypeOption.BeforeExportContentTypesAction(result);
            }
            output.WriteMetadata(AveMetadataType.WebContentType.ToString(), result);
        }

        public void ExportContentTypes(IAveBackupStream stream, SPContentTypeBackupOption backupContentTypeOption)
        {
            var backupOption = new AveBackupOption()
            {
                BeforeExportContentTypesAction = backupContentTypeOption.BeforeExportConentTypesAction
            };
            this.ExportContentTypes(stream, backupOption);
        }

        public void ExportEventReceivers(IAveBackupStream output)
        {
            var events = AveSPEventReceiver.CreateInstance(this);
            events.Export(output); ;
        }

        public void ExportSearchInfo(IAveBackupStream output)
        {
            if (this.mAveSPSite.SPContextKind.IsServerMode10Upper())
            {
                if (AveEnv.IsMoss)
                {
                    var aveSPWebSearch = new AveSPSearch(this);
                    aveSPWebSearch.Export(output);
                }
            }
        }

        public void ExportSocialTags(IAveBackupStream output)
        {
            if (this.mAveSPSite.SPContextKind.IsServerMode10Upper())
            {
                if (AveEnv.IsMoss)
                {
                    var tag = new AveSPSocialTag(this.SPWeb.Url + "/", this.mAveSPSite);
                    tag.Export(output);
                }
            }
        }

        public void ExportSocialComments(IAveBackupStream output)
        {
            if (this.mAveSPSite.SPContextKind.IsServerMode10Upper())
            {
                if (AveEnv.IsMoss)
                {
                    var comment = new AveSPSocialComment(this.SPWeb.Url + "/", this.mAveSPSite);
                    comment.Export(output);
                }
            }
        }

        /// <summary>
        /// Export the social feeds
        /// </summary>
        /// <param name="output"></param>
        public void ExportSocialFeeds(IAveBackupStream output)
        {
            if (this.mAveSPSite.SPContextKind.IsServerMode13Upper() || this.mAveSPSite.SPContextKind == AveContextKind.ClientObjectModel)
            {
                if (AveEnv.IsMoss)
                {
                    var feed = new AveSPSocialFeed(this.SPWeb.Url, this.mAveSPSite);
                    feed.Export(output);
                }
            }
        }

        //add for micro feed archive
        public Dictionary<int, object> GetMicroFeedCache()
        {
            Dictionary<int, object> result = new Dictionary<int, object>();
            if (this.mAveSPSite.SPContextKind.IsServerMode13Upper() || this.mAveSPSite.SPContextKind == AveContextKind.ClientObjectModel)
            {
                if (AveEnv.IsMoss)
                {
                    var feed = new AveSPSocialFeed(this.SPWeb.Url, this.mAveSPSite);
                    List<AveSocialFeedReplyInfo> feedInfoCacheForArchive = new List<AveSocialFeedReplyInfo>();
                    foreach (AveSocialFeedInfo feedInfo in feed.GetSocialFeeds(ref feedInfoCacheForArchive))
                    {
                        result.Add(Convert.ToInt32(feedInfo.Id.Split('.')[7]), (object)feedInfo);
                    }
                    foreach (AveSocialFeedReplyInfo arFeedInfo in feedInfoCacheForArchive)
                    {
                        result.Add(Convert.ToInt32(arFeedInfo.Id.Split('.')[7]), (object)arFeedInfo);
                    }
                }
            }
            return result;
        }

        //add for micro feed granular
        public Dictionary<int, object> GetSocialThreadCache()
        {
            Dictionary<int, object> result = new Dictionary<int, object>();
            if (this.mAveSPSite.SPContextKind.IsServerMode13Upper() || this.mAveSPSite.SPContextKind == AveContextKind.ClientObjectModel)
            {
                if (AveEnv.IsMoss)
                {
                    var feed = new AveSPSocialFeed(this.SPWeb.Url, this.mAveSPSite);

                    foreach (AveSocialFeedInfo feedInfo in feed.GetSocialFeeds())
                    {
                        result.Add(Convert.ToInt32(feedInfo.Id.Split('.')[7]), (object)feedInfo);
                    }

                }
            }
            return result;
        }

        public void ExportNavigation(IAveBackupStream output, bool backupInheritedNavNodes = true, bool needFullUrl = false, string srcWebAppUrl = null)
        {
            var navigation = new AveSPNavigation(this);
            if (srcWebAppUrl == null)
            {
                srcWebAppUrl = string.Empty;
            }
            navigation.Export(output, backupInheritedNavNodes, srcWebAppUrl, needFullUrl);
        }

        public void ExportUsers(IAveBackupStream output, bool includeUsersWithoutSecurity = false)
        {
            ExportUsers(output, new AveUserBackupOption() { IncludeUsersWithoutSecurity = includeUsersWithoutSecurity });
        }

        private void ExportUsers(IAveBackupStream output, AveUserBackupOption option)
        {
            var users = AveUser.CreateInstance(this);
            users.Export(output, option);
        }

        public void ExportGroups(IAveBackupStream output, bool includeGroupsWithoutSecurity = false)
        {
            var groups = AveGroup.CreateInstatnce(this);
            groups.Export(output, includeGroupsWithoutSecurity);
        }

        public void ExportRoles(IAveBackupStream output)
        {
            var roles = new AveRoles(this);
            roles.Export(output); ;
        }

        public void ExportRoleAssignments(IAveBackupStream output)
        {
            var roleAssignments = AveRoleAssignments.CreateInstance(this);
            roleAssignments.Export(output); ;
        }

        public void ExportFullTextIndex(IAveBackupStream output, Dictionary<string, object> customFieldValues)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPWeb.ExportFullTextIndex"))
            {
                var index = new FullTextIndex()
                {
                    TimeZoneInfoID = TimeZoneInfoId,
                };
                if (customFieldValues != null)
                {
                    index.SetCustomColumnValues(customFieldValues);
                }
                output.WriteMetadata(AveMetadataType.FullTextIndex, index);
            }
        }

        public void ExportPolicy(IAveBackupStream output)
        {
            var policy = new AveSPPolicy(this.ParentSite, this);
            policy.Export(output);
        }

        public List<AveEventReceiverInfo> GetEventReceivers()
        {
            var events = AveSPEventReceiver.CreateInstance(this);
            return events.GetReceivers();
        }

        public AveFeatureInfoBox GetFeatures()
        {
            var featureManager = AveSPFeature.CreateInstance(this);
            return featureManager.GetFeatures();
        }

        public List<AveRoleInfo> GetRoles()
        {
            var roles = new AveRoles(this);
            return roles.GetRoles();
        }

        public List<AveUserInfo> GetUsers(bool includeUsersWithoutSecurity = true)
        {
            return GetUsers(new AveUserBackupOption() { IncludeUsersWithoutSecurity = includeUsersWithoutSecurity });
        }

        private List<AveUserInfo> GetUsers(AveUserBackupOption option)
        {
            var users = AveUser.CreateInstance(this);
            return users.GetUsers(option);
        }

        public List<AveGroupInfo> GetGroupsWithAllMembers(bool includeUsersWithoutSecurity = true)
        {
            var groups = AveGroup.CreateInstatnce(this);
            return groups.GetGroupsWithAllMembers(includeUsersWithoutSecurity);
        }
        #endregion


        public void ExportBaseInfo(IAveBackupStream stream, SetWebInfoAction setWebInfo)
        {
            var webInfo = new AveSPWebInfo(this);
            var result = webInfo.GetWebInfo();
            if (setWebInfo != null)
            {
                setWebInfo(result);
            }
            stream.WriteMetadata(AveMetadataType.WebBasicInfo, result);
        }

        public void ExportNavigation(IAveBackupStream stream, SPNavigationOption backupNavigationOption)
        {
            var navigation = new AveSPNavigation(this);
            if (backupNavigationOption.SrcWebAppUrl == null)
            {
                backupNavigationOption.SrcWebAppUrl = string.Empty;
            }
            navigation.Export(stream, backupNavigationOption.BackupInheritedNavNodes, backupNavigationOption.SrcWebAppUrl, backupNavigationOption.NeedFullUrl);
        }

        public void ExportFields(IAveBackupStream stream, SPWebFieldBackupOption backupColumnOption)
        {
            var fields = AveSPFieldCollection.CreateInstance(this);
            var backupOption = new AveBackupOption()
            {
                BackupRelatedTermSets = backupColumnOption.BackupRelatedTermSets,
                BackupRelatedTermsOnly = backupColumnOption.BackupRelatedTermsOnly,
                BeforeExportFieldsAction = backupColumnOption.BeforeExportFieldsAction
            };
            fields.Export(stream, backupOption);
        }
        public void ExportWorkflows(IAveBackupStream stream, SPWebWorkflowAssociationBackupOption option)
        {
            var workflow = new AveWorkflow()
            {
                ForceBackupAssoiciation = true,
                ForceBackupInstance = true,
                BackupWorkflowAssocationToExportedFile = option.BackupWorkflowAssocationToExportedFile
            };
            if (!string.IsNullOrEmpty(option.NWContentDBConnectionString))
            {
                workflow.SetNWDBConnectionString(option.NWContentDBConnectionString);
            }
            if (!string.IsNullOrEmpty(option.NWConfigDBConnectionString))
            {
                workflow.SetNWConfigDBConnectionString(option.NWConfigDBConnectionString);
            }
            if (option.ExportWebAssociation)
            {
                workflow.ExportReusableWorkflowTemplates(stream, this, option.TemplateFilterFunc);
                workflow.ExportWebWFAssociation(stream, this, option.FilterFunc);
            }
            if (option.ExportContentTypeAssociation)
            {
                workflow.ExportWebContentTypeWFAssociation(stream, this, option.FilterFunc);
            }
            if (option.ExportInstance)
            {
                workflow.ExportWebWorkflowInstance(stream, this);
            }
            if (option.ExportWebAssociation)
            {
                workflow.ExportWebWorkflowSchedule(stream, this);
            }
            if (option.ExportWebAssociation || option.ExportInstance)
            {
                workflow.ExportNintexWorkflowTemplates(stream, this);
            }
        }

        private bool FilterWorkflowByCTName(AveWorkflowAssociationInfo info, List<string> contentTypeFilter)
        {
            if (contentTypeFilter != null && info.CTName != null)
            {
                return contentTypeFilter.Contains(info.CTName);
            }
            return true;
        }

        public void ExportSocialInfos(IAveBackupStream stream)
        {
            if (this.mAveSPSite.SPContextKind.IsServerMode10Upper())
            {
                if (AveEnv.IsMoss)
                {
                    var socialDto = new SPSocialDto();

                    socialDto.Comments = new AveSPSocialComment(this.SPWeb.Url + "/", this.mAveSPSite).GetSocialComments();
                    socialDto.Tags = new AveSPSocialTag(this.SPWeb.Url + "/", this.mAveSPSite).GetSocialTags();

                    if ((socialDto.Comments != null && socialDto.Comments.Count > 0) ||
                    (socialDto.Tags != null && socialDto.Tags.Count > 0))
                    {
                        stream.WriteMetadata(AveMetadataType.SocialDto, socialDto);
                    }
                }
            }
        }

        public void ExportRoleAssignments(IAveBackupStream stream, SPRoleAssignmentsBakupOption backupOption)
        {//todo:oliver 重复代码
            SPRoleAssignmentsDto roleAssignmentsDto = new SPRoleAssignmentsDto();

            if (backupOption.IncludeInheritedRoleAssignments || HasUniqueRoleAssignments)
            {
                using (var roleAssignments = AveRoleAssignments.CreateInstance(this))
                {
                    roleAssignmentsDto = roleAssignments.GetRoleAssignmentsDto(backupOption.IncludeUsers, backupOption.IncludeGroups);
                }
            }
            roleAssignmentsDto.IsInherit = !HasUniqueRoleAssignments;

            stream.WriteMetadata(AveMetadataType.RoleAssignmentsDto, roleAssignmentsDto);
        }

        public void ExportUserCustomActions(IAveBackupStream output)
        {
            AveSPUserCustomActionCollection spUserCustomActionCollection = new AveSPWebUserCustomActionCollection(this);
            output.WriteMetadata(AveMetadataType.WebUserCustomAction, spUserCustomActionCollection.GetUserCustomActionInfos());
        }
    }
}