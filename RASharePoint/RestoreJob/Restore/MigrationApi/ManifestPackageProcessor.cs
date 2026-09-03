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
using AvePoint.GCommon;
using AvePoint.GCommon.Utility;
using AvePoint.Item.Restore;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Restore;
using Cloud.Sdk.Data.Cop.Insights;
using DocumentFormat.OpenXml.Bibliography;
using HSMAzureCommon;
using HSMCommon;
using HSMCommon.DeploymentXML;
using LiteDB;
using RAArchiverCommon;
using RAGoogle.Archive.Wrapper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using static Org.BouncyCastle.Math.EC.ECCurve;


namespace AvePoint.RA.SharePoint.RestoreJob.Restore
{
    /// <summary>
    /// Result status of package finalization operation
    /// </summary>
    public enum PackageStatus
    {
        /// <summary>
        /// Package is ready for import
        /// </summary>
        Ready,

        /// <summary>
        /// Package is not ready yet (more items needed)
        /// </summary>
        NotReady,

        /// <summary>
        /// Package is empty (cleanup performed)
        /// </summary>
        Empty,

        /// <summary>
        /// An error occurred during package preparation
        /// </summary>
        Error
    }

    public class ManifestPackageProcessor
    {
        static readonly AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ItemRestoreConfig _config;
        private static readonly object fieldslock = new object();
        public WinAzure AzureInfo { get; set; }
        private static int DefaultLCID = -1;
        private List<int> mUserGroupMappingForCurrentPackage = new List<int>();
        private Dictionary<Guid, SPLookupList> mSPLookupListCollection = new Dictionary<Guid, SPLookupList>();
        DeploymentUserGroupMap mUserGroupMap = new DeploymentUserGroupMap();
        private Dictionary<string, MetadataCacheInfo> mMetadataInfoList = new Dictionary<string, MetadataCacheInfo>();
        
        /// <summary>
        /// Last error message from package finalization
        /// </summary>
        public string LastError { get; private set; }
        
        public SPGenericObject SPObject { get; set; }
        SPGenericObject SPFileObject { get; set; }
        public SPGenericObjectCollection SPObjectCollection { get; set; }

        SPLookupLists SPLookupLists { get; set; }
        Dictionary<Guid, SPLookupList> SPLookupListCollection { get; set; }

        SPGenericObject mRoleDefinitionObject { get; set; }
        SPGenericObject RoleAssignmentsObject { get; set; }


        public List<int> UserIdCache { get; set; }
        List<string> UsedRoleID { get; set; }

        bool IsNewObject { get; set; } = false;

        string ParentSiteId { get;  set; }
        string ParentSiteUrl { get; set; }
        string ParentSiteFullUrl { get; set; }
        string RootWebId { get; set; }
        string RootWebUrl { get; set; }
        string ParentWebUrl { get; set; }
        string ParentWebId { get; set; }
        string ParentListId { get; set; }
        string ParentListUrl { get; set; }
        string ParentFolderId { get; set; }
        string ParentFolderUrl { get; set; }

        string SiteUserInfoListId { get; set; }
        string SiteUserInfoListUrl { get; set; }

        /// <summary>
        /// FreeContainer parameters for uploading files
        /// </summary>
        private FreeContainerParameters _fcParameters;

        /// <summary>
        /// Parent site reference for provisioning new FreeContainers
        /// </summary>
        private AveSPSite _parentSite;

        public int CurrentPackageCount { get; set; }
        public long CurrentPackageSize { get; set; }
        int FileValue { get; set; } = 0;
        public Dictionary<string, FileHash> UploadFileHashDic { get; set; }
        List<string> CurrentPackageIdList { get; set; }

        public string TempManifestPath { get; private set; }
        public string TempContentPath { get; private set; }
        string JobDir { get; set; }
        AveSPList mAveList { get; set; }

        public Dictionary<string, Guid> ItemUniqueIdMapping { get; set; }

        public string GenerateWebRelativeUrl(string fileName)        // e.g. test.xlsx
        {
            // 1. Pick folder path
            string serverRelativeFolder = string.IsNullOrEmpty(ParentFolderUrl)
                ? ParentWebUrl.TrimEnd('/') + "/" + ParentListUrl.Trim('/')
                : ParentFolderUrl; // already server-relative and correct per your guarantee

            // 2. File path (server-relative)
            string serverRelativeFile = serverRelativeFolder.TrimEnd('/') + "/" + fileName;

            // 3. Web-relative (strip parent web prefix + leading slash)
            string webRelative = serverRelativeFile.Substring(ParentWebUrl.Length).TrimStart('/');

            return webRelative; // e.g. Shared Documents/F1/test.xlsx
        }

        public ManifestPackageProcessor(AveSPSite site, AveSPWeb web, AveSPList list, string workingPath, ItemRestoreConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            //IsNewObject = true;
            SPObjectCollection = new SPGenericObjectCollection();
            SPLookupLists = new SPLookupLists();
            SPLookupListCollection = new Dictionary<Guid, SPLookupList>();
            CurrentPackageIdList = new List<string>();
            ItemUniqueIdMapping = new Dictionary<string, Guid>();
            UserIdCache = new List<int>();
            UsedRoleID = new List<string>();
            UploadFileHashDic = [];

            JobDir = workingPath;

            ResetContainerInfo();

            if (site != null)
            {
                // Store parent site for later FreeContainer provisioning
                _parentSite = site;

                ParentSiteId = site.SPSite.ID.ToString();
                ParentSiteUrl = site.SPSite.ServerRelativeUrl;
                ParentSiteFullUrl = site.SPSite.Url;
                RootWebId = site.SPSite.RootWeb.ID.ToString();
                RootWebUrl = site.SPSite.RootWeb.ServerRelativeUrl;
                SiteUserInfoListId = site.SPSite.RootWeb.SiteUserInfoList.ID.ToString();
                SiteUserInfoListUrl = site.SPSite.RootWeb.SiteUserInfoList.RootFolder.ServerRelativeUrl;
                _config.StubUserInfos = site.SPSite.SiteUsersSerializer.GetObjectData(true);
                _config.StubGroupInfos = site.SPSite.RootWeb.GroupsSerializer.GetObjectData(false);
            }
            if (web != null)
            {
                ParentWebId = web.SPWeb.ID.ToString();
                ParentWebUrl = web.SPWeb.ServerRelativeUrl;
            }
            if (list != null)
            {
                mAveList = list;
                ParentListId = list.SPList.ID.ToString();
                ParentListUrl = list.SPList.RootFolder.ServerRelativeUrl;
            }

            mLog.Info($"Init processor for list. listUrl:{ParentListUrl}, listId:{ParentListId}, workDir:{workingPath}");

            // FreeContainer will be provisioned lazily on first ProcessContentData call
            // This avoids unnecessary provisioning for lists with no content
        }

        /// <summary>
        /// Initializes FreeContainer parameters by provisioning containers from SharePoint.
        /// Called lazily on first file upload to avoid unnecessary provisioning for empty lists.
        /// </summary>
        private void InitializeFreeContainerParameters(AveSPSite site)
        {
            try
            {
                mLog.Info("Provisioning SharePoint FreeContainers");
                var fcManager = new FreeContainerManager();
                _fcParameters = fcManager.CreateFreeContainers(site.SPSite);

                if (_fcParameters == null)
                {
                    throw new Exception("Failed to provision FreeContainers - returned null");
                }

                mLog.Info("FreeContainer provisioned. DataContainerUri: {0}", _fcParameters.DataContainerUri);
            }
            catch (Exception ex)
            {
                mLog.Error("Failed to initialize FreeContainer parameters: {0}", ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Gets the FreeContainer parameters for uploading files.
        /// Provisions containers on first call if not already initialized.
        /// </summary>
        public FreeContainerParameters GetFreeContainerParameters()
        {
            // Lazy initialization - provision on first call
            if (_fcParameters == null)
            {
                if (_parentSite == null)
                {
                    throw new InvalidOperationException("Cannot provision FreeContainer - parent site not available");
                }
                InitializeFreeContainerParameters(_parentSite);
            }
            return _fcParameters;
        }

        Dictionary<string, string> mFileValueDic = new Dictionary<string, string>();

        private readonly object fileValueLock = new object();

        public void ResetContainerInfo()
        {
            if (AzureInfo == null) { AzureInfo = new WinAzure(); }
            string containerId = Guid.NewGuid().ToString().ToLower().Replace("-", "");
            AzureInfo.AzureManifestContainerName = "m-" + containerId;
            AzureInfo.AzureSourceContainerName = "s-" + containerId;
            AzureInfo.AzureQueueReportContainerName = "q-" + containerId;

            TempContentPath = SecurityUtils.SafeCombinePath(JobDir, AzureInfo.AzureSourceContainerName);
            TempManifestPath = SecurityUtils.SafeCombinePath(JobDir, AzureInfo.AzureManifestContainerName);

            CreateDirectory(TempContentPath);
            CreateDirectory(TempManifestPath);
            _fcParameters = null;
        }

        public void ClearTempFolder()
        {
            try
            {
                if (Directory.Exists(TempManifestPath))
                {
                    Directory.Delete(TempManifestPath, true);
                }
                if (Directory.Exists(TempContentPath))
                {
                    Directory.Delete(TempContentPath, true);
                }
            }
            catch (Exception e)
            {
                mLog.Error($"An error occurred while deleting temp folder. Exception: {e.ToString()}.");
            }
        }

        public void CopySPListItem(SPListItem item, SPFile file)
        {
            if (item.Author != file.Author)
            {
                file.Author = item.Author;
            }
            DateTime modifiedTime = DateTime.UtcNow;
            if (item.TimeLastModified != DateTime.MinValue)
            {
                modifiedTime = item.TimeLastModified;
            }
            else if (file.TimeLastModified != DateTime.MinValue)
            {
                modifiedTime = file.TimeLastModified;
            }
            file.TimeLastModified = item.TimeLastModified = modifiedTime;

            DateTime createTime = DateTime.UtcNow;
            if (item.TimeCreated != DateTime.MinValue)
            {
                createTime = item.TimeCreated;
            }
            else if (file.TimeCreated != DateTime.MinValue)
            {
                createTime = file.TimeCreated;
            }
            file.TimeCreated = item.TimeCreated = createTime;
            if (item.ModifiedBy != file.ModifiedBy)
            {
                file.ModifiedBy = item.ModifiedBy;
            }
        }

        public void CopySPListItem(SPListItem item, SPFolder spFolder)
        {
            if (item.TimeLastModified == DateTime.MinValue)
            {
                if (spFolder.TimeLastModified == DateTime.MinValue)
                {
                    item.TimeLastModified = DateTime.Now;
                    spFolder.TimeLastModified = item.TimeLastModified;
                }
                else
                {
                    item.TimeLastModified = spFolder.TimeLastModified;
                }
            }
            if (item.TimeCreated == DateTime.MinValue)
            {
                if (spFolder.TimeCreated == DateTime.MinValue)
                {
                    item.TimeCreated = DateTime.Now;
                    spFolder.TimeCreated = item.TimeCreated;
                }
                else
                {
                    item.TimeCreated = spFolder.TimeCreated;
                }
            }
            if (item.TimeLastModified != spFolder.TimeLastModified)
            {
                spFolder.TimeLastModified = item.TimeLastModified;
            }
            if (item.TimeCreated != spFolder.TimeCreated)
            {
                spFolder.TimeCreated = item.TimeCreated;
            }
            if (item.Author != spFolder.Author)
            {
                spFolder.Author = item.Author;
            }
            if (item.ModifiedBy != spFolder.ModifiedBy)
            {
                spFolder.ModifiedBy = item.ModifiedBy;
            }
        }

        public void InitParentFolder(AveSPFolder parentFolder)
        {
            ParentFolderId = parentFolder.SPFolder.UniqueId.ToString();
            ParentFolderUrl = parentFolder.SPFolder.ServerRelativeUrl;
        }

        public SPFile GenerateSPFile(string leafName, string fileKey, string parentFolderId, Dictionary<string, object> docData, Dictionary<string, object> userData, bool isVersion = false)
        {
            var file = new SPFile();
            {
                file.Name = leafName;
                file.ParentId = parentFolderId;
                file.ParentWebId = ParentWebId;
                file.ParentWebUrl = ParentWebUrl;
                file.ListId = ParentListId;
                file.InDocumentLibrary = true;
                file.FileValue = mFileValueDic[fileKey];
                file.Url = GenerateWebRelativeUrl(leafName);
            }
            
            if (docData != null && docData.ContainsKey("Id"))
            {
                file.Id = docData["Id"].ToString();
            }
            if (userData.ContainsKey("Created"))
            {
                file.TimeCreated = Convert.ToDateTime(userData["Created"]);
            }
            else
            {
                file.TimeCreated = Convert.ToDateTime(userData["Modified"]);
            }
            file.TimeLastModified = Convert.ToDateTime(userData["Modified"]);
            file.Version = userData["#tp_UIVersionString"].ToString();
            userData["Author"] = mAveList.ParentWeb.ParentSite.SPMembers.FindMemberId(Convert.ToInt32(userData["Author"]));
            userData["Editor"] = mAveList.ParentWeb.ParentSite.SPMembers.FindMemberId(Convert.ToInt32(userData["Editor"]));
            file.Author = userData["Author"].ToString();
            file.ModifiedBy = userData["Editor"].ToString();
            file.ListItemIntId = Convert.ToInt32(userData["#tp_ID"]);
            if (userData.ContainsKey("SETUPPATH"))
            {
                file.SetupPath = userData["SETUPPATH"].ToString();
                file.IsGhosted = true;
            }
            if (userData.ContainsKey("#tp_CheckinComment"))
            {
                file.CheckinComment = userData["#tp_CheckinComment"].ToString();
            }
            if (!isVersion)
            {
                //AddExpirationDate(userData, file);

                List<DictionaryEntry> properties = new List<DictionaryEntry>();

                ProcessMetaDataInfo(docData, userData, properties);

                if (properties != null)
                {
                    file.Properties = properties.Distinct(new DictionaryEntryComparer()).ToList();
                }
            }
            return file;
        }
        private KeyValuePair<string, MetaInfoProperty> ProcessImageTag(Dictionary<string, object> docData)
        {
            byte[] bts = (byte[])docData["MetaInfo"];
            try
            {
                MetaInfoHandler metaInfoHandler = new MetaInfoHandler(bts);
                Dictionary<string, MetaInfoProperty> meta = metaInfoHandler.m_MetaCollection;
                return meta.Where(a=>a.Key == "MediaServiceImageTags").FirstOrDefault();
            }
            catch (Exception e)
            {
                mLog.Warn("An error occurred while getting compressed metadata string. Exception: {0}.", e.ToString());
                return new KeyValuePair<string, MetaInfoProperty>();
            }
        }
        private static List<string> _IgnoredMetadataOfOneDrive = new List<string>() { "vti_shardwithdetails", "vti_sharinghinthash", "display_urn\\", "display_urn\\:schemas-microsoft-com\\:office\\:office#sharedwithusers", "sharedwithdetails", "sharinghinthash", "sharedwithusers" };
        private static List<string> _IgnoredMetadataInfo = new List<string>() { "vti_modifiedby", "vti_author", "vti_writevalidationtoken", "vti_dbschemaversion", "vti_sprocsschemaversion", "vti_parserversion", "SharedWithDetails", "SharedWithUsers", "vti_shardwithdetails", "display_urn:schemas-microsoft-com:office:office#SharedWithUsers", "display_urn:schemas-microsoft-com:office:office#Author", "display_urn:schemas-microsoft-com:office:office#Editor" };
        private static List<string> _SetEmptyMetadataOfRecord = new List<string>() { "_vti_ItemHoldRecordStatus", "ecm_RecordRestrictions", "ecm_ItemLockHolders", "ecm_ItemDeleteBlockHolders" };

        private void ProcessMetaDataInfo(Dictionary<string, object> docData, Dictionary<string, object> userData, List<DictionaryEntry> properties)
        {
            if (docData.ContainsKey("MetaInfo"))
            {
                byte[] bts = (byte[])docData["MetaInfo"];
                try
                {
                    MetaInfoHandler metaInfoHandler = new MetaInfoHandler(bts);
                    Dictionary<string, MetaInfoProperty> meta = metaInfoHandler.m_MetaCollection;
                    foreach (KeyValuePair<string, MetaInfoProperty> property in meta)
                    {
                        try
                        {
                            DictionaryEntry fileProperty = new DictionaryEntry();
                            fileProperty.Name = property.Key;
                            if (userData.ContainsKey(property.Key.Replace(" ", "_x0020_")))
                            {
                                continue;
                            }
                            if (IsIgnoreMetadataForMIP(fileProperty.Name))
                            {
                                continue;
                            }
                            if (IsIgnoreMetadataForOneDrive(fileProperty.Name))
                            {
                                continue;
                            }
                            fileProperty.Type = ConvertCharToType(property.Value.Type.ToString());
                            fileProperty.Access = ConvertCharToAccess(property.Value.Access.ToString());

                            if (fileProperty.Type == SPDictionaryEntryValueType.FileSystemTime)
                            {
                                fileProperty.Value = GetFileTime(property.Value.Value.ToString());
                            }
                            else if (fileProperty.Type == SPDictionaryEntryValueType.Time)
                            {
                                try
                                {//date format is current thread(local), ms can't analysis and convert to normal format.
                                    fileProperty.Value = Convert.ToDateTime(property.Value.Value.ToString()).ToString("MM/dd/yyyy hh:mm:ss", DateTimeFormatInfo.InvariantInfo);
                                }
                                catch (Exception ex)
                                {
                                    mLog.Warn("An error occurred while getting datetime,exception:{0},property name: {1},value {2}", ex.ToString(), property.Key, property.Value.Value);
                                    continue;
                                }
                            }
                            else if (IsSetEmptyMetadataForRecord(fileProperty.Name))
                            {
                                fileProperty.Value = string.Empty;
                            }
                            else if (fileProperty.Type == SPDictionaryEntryValueType.Integer)
                            {
                                try
                                {
                                    fileProperty.Value = int.Parse(property.Value.Value.ToString()).ToString();
                                }
                                catch (Exception ex)
                                {
                                    mLog.Warn("An error occurred while getting integer,exception:{0}", ex.ToString());
                                    continue;
                                }
                            }
                            else if (fileProperty.Type == SPDictionaryEntryValueType.Boolean)
                            {
                                try
                                {
                                    fileProperty.Value = Convert.ToBoolean(property.Value.Value).ToString();
                                }
                                catch (Exception ex)
                                {
                                    mLog.Warn("An error occurred while getting bool value,exception:{0},property name: {1},value {2}", ex.ToString(), property.Key, property.Value.Value);
                                    continue;
                                }
                            }
                            else if (fileProperty.Type == SPDictionaryEntryValueType.Double)
                            {
                                try
                                {
                                    fileProperty.Value = Convert.ToDouble(property.Value.Value.ToString()).ToString();
                                }
                                catch (Exception ex)
                                {
                                    mLog.Warn("An error occurred while getting double value,exception:{0}, property name: {1},value {2}", ex.ToString(), property.Key, property.Value.Value);
                                    continue;
                                }
                            }
                            else
                            {
                                fileProperty.Value = property.Value.Value.ToString();
                            }
                            properties.Add(fileProperty);
                        }

                        catch (Exception e)
                        {
                            mLog.Warn("An error occurred while process meta info. Exception: {0}.", e.ToString());
                        }
                    }
                }
                catch (Exception e)
                {
                    mLog.Warn("An error occurred while getting compressed metadata string. Exception: {0}.", e.ToString());
                }
            }

            SPDictionaryEntryValueType ConvertCharToType(string c)
            {
                switch (c)
                {
                    case "Boolean":
                        return SPDictionaryEntryValueType.Boolean;

                    case "Double":
                        return SPDictionaryEntryValueType.Double;

                    case "Empty":
                        return SPDictionaryEntryValueType.Empty;

                    case "FileSystemTime":
                        return SPDictionaryEntryValueType.FileSystemTime;

                    case "Integer":
                        return SPDictionaryEntryValueType.Integer;

                    case "LongText":
                        return SPDictionaryEntryValueType.LongText;

                    case "Time":
                        return SPDictionaryEntryValueType.Time;

                    case "StringVector":
                        return SPDictionaryEntryValueType.StringVector;
                }
                return SPDictionaryEntryValueType.String;
            }

            SPDictionaryEntryAccess ConvertCharToAccess(string c)
            {
                switch (c)
                {
                    case "ReadWrite":
                        return SPDictionaryEntryAccess.ReadWrite;

                    case "ReadOnly":
                        return SPDictionaryEntryAccess.ReadOnly;
                }
                return SPDictionaryEntryAccess.ReadWrite;
            }

            bool IsIgnoreMetadataForMIP(string metadataName)
            {
                if (metadataName.IndexOf("msip_label", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
                if (metadataName.Equals("vti_stickycachedpluggableparserprops", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                if (metadataName.Equals("vti_mediaservicemetadata", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                if (metadataName.Equals("vti_mediaservicemetadataasync", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                if (metadataName.Equals("vti_mediaservicefastmetadata", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                return false;
            }
            bool IsSetEmptyMetadataForRecord(string metadataName)
            {
                if (_SetEmptyMetadataOfRecord.Contains(metadataName, StringComparer.OrdinalIgnoreCase))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            bool IsIgnoreMetadataForOneDrive(string metadataName)
            {
                if (_IgnoredMetadataInfo.Contains(metadataName, StringComparer.OrdinalIgnoreCase))
                {
                    return true;
                }
                else if (mAveList.SPList.BaseTemplate == AveListTemplateType.MySiteDocumentLibrary)
                {
                    if (_IgnoredMetadataOfOneDrive.Contains(metadataName, StringComparer.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            string GetFileTime(string value)
            {
                int index = value.IndexOf('|');
                string subString1 = value.Substring(0, index);
                string subString2 = value.Substring(index + 1);
                long timeTick1 = Convert.ToInt64(subString1, 16);
                timeTick1 = timeTick1 << 32;
                long timeTick2 = Convert.ToInt64(subString2, 16);
                long timeTick = timeTick1 | timeTick2;
                DateTime time = DateTime.FromFileTime(timeTick);
                return time.ToString("MM/dd/yyyy hh:mm:ss", DateTimeFormatInfo.InvariantInfo);
            }
        }

        public SPListItem GenerateSPListItem(Dictionary<string, object> docData, Dictionary<string, object> userData, List<Dictionary<string, object>> dataJunction, ListItemDocType type, bool isVersion = false, bool isFile = false)
        {
            var item = new SPListItem();
            {
                item.DocType = type;
                item.Id = docData["Id"].ToString();
                item.DocId = docData["Id"].ToString();
                item.IntId = Convert.ToInt32(userData["#tp_ID"]);
                item.ParentWebId = ParentWebId;
                item.ParentFolderId = ParentFolderId;
                item.ParentListId = ParentListId;
                item.DirName = ParentFolderUrl;
                item.Author = userData["Author"].ToString();
                item.ModifiedBy = userData["Editor"].ToString();
            }

            int version = Convert.ToInt32(userData["#tp_UIVersion"]);

            if (docData.ContainsKey("LeafName"))
            {
                item.Name = docData["LeafName"].ToString();
            }
            item.FileUrl = GenerateWebRelativeUrl(item.Name);
            if (docData.ContainsKey("ComplianceTag"))
            {
                item.ComplianceTag = docData["ComplianceTag"].ToString();
                item.ComplianceFlags = "3";
            }
            if (userData.ContainsKey("#tp_UIVersionString") && userData["#tp_UIVersionString"] != null)
            {
                item.Version = userData["#tp_UIVersionString"].ToString();
            }
            else
            {
                mLog.Debug("There is no #tp_UIVersionString");
            }

            if (userData.ContainsKey("#tp_ModerationStatus"))
            {
                item.ModerationStatus = (SPModerationStatusType)userData["#tp_ModerationStatus"];
                if (_config.ResetMajorVersionApprovalStatus && item.DocType == ListItemDocType.File && isFile)
                {
                    if (version % 512 == 0 && item.ModerationStatus != SPModerationStatusType.Approved)
                    {
                        item.ModerationStatus = SPModerationStatusType.Approved;
                    }
                }
            }
            if (userData.ContainsKey("_ModerationComments"))
            {
                item.ModerationComment = userData["_ModerationComments"].ToString();
            }
           
           
            return item;
        }

        public SPFieldCollection GenerateSPFieldCollection(AveSPList list, SPListItem listItem, Dictionary<string, object> docData, Dictionary<string, object> userData, List<Dictionary<string, object>> dataJunction, ListItemDocType type, List<DictionaryEntry> mmsProperties, bool isVersion = false, bool isFile = false)
        {
            SPFieldCollection fieldCollection = new SPFieldCollection();
            try
            {
                string taxonomyListId = Guid.Empty.ToString();
                //Wait Wrapper Team provide method
                var version = Convert.ToInt32(userData["#tp_UIVersion"]);
                int docRowId = Convert.ToInt32(userData["#tp_ID"]);
                ItemMetadataForHSMConnector itemData = new ItemMetadataForHSMConnector(_config.ObjectModelFactory, list, version, docRowId, userData, dataJunction);
                Dictionary<string, AveFieldValueInfo> fieldValues = itemData.ProcessItemMetadata();
                if (fieldValues == null)
                {
                    mLog.Warn($"ProcessFieldCollection.The fieldValues is null1.");
                    return fieldCollection;
                }
                List<string> NeedSetNullFields = SetNeedSetNullFieldsEx(fieldValues?.Keys.ToList());
                var termIdCache = new List<string>();
                foreach (var fieldValue in fieldValues)
                {
                    string columnName = string.Empty;
                    try
                    {
                        columnName = fieldValue.Key;
                        AveFieldValueInfo valueInfo = fieldValue.Value;
                        if (valueInfo == null)
                        {
                            mLog.Warn($"The valueInfo is null,need skip.Column:{columnName}.");
                            continue;
                        }
                        if (valueInfo.ColValue == null)
                        {
                            mLog.Warn($"The column value is null,need skip.Column:{fieldValue.Key}.");
                            continue;
                        }

                        SPField field = new SPField();

                        switch (columnName)
                        {
                            case "Author":
                                listItem.Author = valueInfo.ColValue.ToString();
                                try
                                {
                                    if (!mUserGroupMappingForCurrentPackage.Contains(Convert.ToInt32(valueInfo.ColValue)))
                                    {
                                        mUserGroupMappingForCurrentPackage.Add(Convert.ToInt32(valueInfo.ColValue));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    mLog.Info($"An error occur when ProcessFieldCollection Author.Message:{ex}");
                                }
                                continue;
                            case "Editor":
                                listItem.ModifiedBy = valueInfo.ColValue.ToString();
                                try
                                {
                                    if (!mUserGroupMappingForCurrentPackage.Contains(Convert.ToInt32(valueInfo.ColValue)))
                                    {
                                        mUserGroupMappingForCurrentPackage.Add(Convert.ToInt32(valueInfo.ColValue));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    mLog.Info($"An error occur when ProcessFieldCollection Editor.Message:{ex}");
                                }
                                continue;
                            case "Modified":
                                listItem.TimeLastModified = new DateTime(((DateTime)valueInfo.ColValue).Ticks);
                                continue;
                            case "Created":
                                listItem.TimeCreated = new DateTime(((DateTime)valueInfo.ColValue).Ticks);
                                continue;
                            case "ContentType":
                                listItem.ContentTypeId = valueInfo.ColValue.ToString();
                                continue;
                            case "Order":
                                listItem.Order = Convert.ToSingle(valueInfo.ColValue);
                                continue;
                            case "Modified_x0020_By":
                            case "Created_x0020_By":
                            case LinkFileCommon.LinkFileFieldName:
                                continue;
                        }
                        #region process field
                        lock (fieldslock)
                        {
                            switch (valueInfo.FieldType)
                            {
                                case AveFieldType.Lookup:
                                    field = ProcessLookupColumnValue(field, valueInfo);
                                    break;
                                case AveFieldType.URL:
                                    field.Value = valueInfo.ColValue.ToString();
                                    if (!columnName.EndsWith("#2"))
                                    {
                                        if (fieldValues.ContainsKey(columnName + "#2"))
                                        {
                                            field.Value2 = fieldValues[columnName + "#2"].ColValue.ToString();
                                        }
                                        else
                                        {
                                            mLog.Debug("No description of hyperlink column, column name:{0}", columnName);
                                        }
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    break;
                                case AveFieldType.User:
                                    if (valueInfo.ColValue is Int32)
                                    {
                                        field.Value = mAveList.ParentWeb.ParentSite.SPMembers.FindMemberId(Convert.ToInt32(valueInfo.ColValue.ToString())) + ";UserInfo";
                                    }
                                    else
                                    {
                                        field.Value = valueInfo.ColValue.ToString();
                                    }
                                        try
                                        {
                                            string[] userIDs = valueInfo.ColValue.ToString().Split(new string[] { ";#" }, StringSplitOptions.RemoveEmptyEntries);
                                            for (int i = 0; i < userIDs.Length; i++)
                                            {
                                                try
                                                {
                                                    int userPricinpleId = Convert.ToInt32(userIDs[i]);
                                                    if ((!userIDs[i].Contains("\\")) && !mUserGroupMappingForCurrentPackage.Contains(userPricinpleId))
                                                    {
                                                        mUserGroupMappingForCurrentPackage.Add(userPricinpleId);
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    mLog.Info($"An error occurred while add user to user cache {ex}.UserID:{userIDs[i]}.");
                                                }
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            mLog.Warn("An error occurred while add user to user cache {0}", e.ToString());
                                        }
                                    break;
                                case AveFieldType.DateTime:
                                    try
                                    {
                                        DateTime currentDate = Convert.ToDateTime(valueInfo.ColValue);
                                        //field.Value
                                        field.Value = currentDate.ToString("MM/dd/yyyy hh:mm:ss tt", DateTimeFormatInfo.InvariantInfo);
                                    }
                                    catch (Exception e)
                                    {
                                        mLog.Warn("An error occurred while convert datetime {0}", e.ToString());
                                        field.Value = valueInfo.ColValue.ToString();
                                    }
                                    break;
                                case AveFieldType.Invalid:
                                    if (columnName.Equals("Facilities", StringComparison.OrdinalIgnoreCase))
                                    {
                                        field = ProcessLookupColumnValue(field, valueInfo);
                                    }
                                    else
                                    {
                                        try
                                        {
                                            var termInfo = GetTermInfo(columnName, valueInfo, DefaultLCID, false);

                                            SPField taxonomyTextField = new SPField()
                                            {
                                                ID = termInfo.TextFieldId.ToString(),
                                                Name = termInfo.TextFieldName,
                                                Value = valueInfo.ColValue.ToString(),
                                            };
                                            fieldCollection.Field.Add(taxonomyTextField);

                                            if (valueInfo.ColValue.ToString().Contains(";"))
                                            {
                                                field.Value = valueInfo.ColValue.ToString().Replace(";", ";#-1;#");
                                                field.Value = "-1;#" + field.Value;
                                            }
                                            else
                                            {
                                                field.Value = "-1;#" + valueInfo.ColValue.ToString();
                                            }
                                            if (NeedSetNullFields.Contains(termInfo.TextFieldName))
                                            {
                                                NeedSetNullFields.Remove(termInfo.TextFieldName);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            mLog.Warn("An error occurred while getting terms info, exception:{0}", e.ToString());
                                            //BUG ADO-180692 Need to Clear the column value while the value could not be found in the destination.
                                            //resolve list issue
                                            //field.Value = valueInfo.ColValue.ToString();
                                            if (columnName.Equals("LikesCount"))
                                            {
                                                field.Value = valueInfo.ColValue.ToString();
                                            }
                                            if (columnName.Equals("AverageRating"))
                                            {
                                                field.Value = valueInfo.ColValue.ToString();
                                            }
                                        }
                                    }
                                    break;
                                case AveFieldType.Currency:
                                case AveFieldType.Number:
                                    field.Value = valueInfo.ColValue.ToString();
                                    if (Thread.CurrentThread.CurrentUICulture.LCID == 1031)
                                    {
                                        field.Value = field.Value.Replace(',', '.');
                                    }
                                    mLog.Info("Handle currency and number field type, field value:{0}", field.Value);
                                    break;
                                default:
                                    field.Value = valueInfo.ColValue.ToString();
                                    break;
                            }
                        }
                        #endregion

                        field.Name = columnName;
                        if (field.Name.Equals("FormData", StringComparison.OrdinalIgnoreCase))
                        {
                            IAveField formField = mAveList.SPList.Fields.GetFieldByInternalName("NFFormData");
                            if (formField != null)
                            {
                                field.Name = "NFFormData";
                                field.ID = formField.ID.ToString();
                                NeedSetNullFields.Remove("NFFormData");
                            }
                        }
                        field.ID = valueInfo.Id.ToString();
                        fieldCollection.Field.Add(field);
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn($"ProcessFieldCollection.column process failed.ColumnName:{columnName}.Message:{ex}.");
                    }
                }
                #region add ArchiverLinkFileType column
                //SPField linkfield = new SPField();
                //linkfield.Name = LinkFileCommon.LinkFileFieldName;
                //linkfield.Value = LinkFileCommon.GenerateLinkFieldValue(mConfig.JobId);
                //linkfield.ID = "b4b338db-fc52-4bf4-a363-0ae0b59ec1cd";
                //fieldCollection.Field.Add(linkfield);
                foreach (var name in NeedSetNullFields)
                {
                    try
                    {
                        if (name.Equals(LinkFileCommon.LinkFileFieldName, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                        if (name == "MediaServiceImageTags")
                        {
                            var tag = ProcessImageTag(docData);
                            SPField field = new SPField();
                            field.Name = name;
                            field.Value = tag.Value.Value.ToString();
                            fieldCollection.Field.Add(field);
                        }
                        else
                        {
                            SPField field = new SPField();
                            field.Name = name;
                            field.Value = null;
                            fieldCollection.Field.Add(field);
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn($"ProcessFieldCollection.SetNullFields failed.ColumnName:{name}.Message:{ex}.");
                    }
                }
                #endregion
            }
            catch (Exception e)
            {
                mLog.Warn("An error occurred while processing fields, Exception:{0}", e.ToString());
                throw;
            }
            return fieldCollection;
        }
        private IAveTermStore GetTermStore(IAveField field, IAveTaxonomySession session, ref int LCID)
        {
            IAveTermStore termStore = null;
            IAveTaxonomyField tField = field as IAveTaxonomyField;
            Guid sspId = Guid.Empty;
            if (tField.SspId == Guid.Empty && !tField.ID.Equals(new Guid("23F27201-BEE3-471e-B2E7-B64FD8B7CA38")))
            {
                object customProperty = field.GetCustomProperty("SspId");
                if (customProperty != null)
                {
                    sspId = new Guid(customProperty.ToString());
                }
            }
            else
            {
                sspId = tField.SspId;
            }
            if (sspId != Guid.Empty)
            {
                try
                {
                    termStore = session.TermStores[sspId];
                }
                catch (Exception ex)
                {
                    //如果原端的field使用的service不在被原端引用，也就是说mms没有被还原，该field的原端属性无法替换，这个sspid也是原端的Id，这时在目的端无法找到
                    //为了保障其他的mms field属性的正确还原，添加try catch，跳过该field的还原
                    mLog.Log(AveLogLevel.WARN, "Can not Get TermStore by sspId:{0},Skip to restore this field value.Exception:{1}.", sspId, ex.ToString());
                    return null;
                }
            }
            else
            {
                termStore = session.DefaultKeywordsTermStore;
                if (termStore == null)
                {
                    termStore = session.DefaultSiteCollectionTermStore;
                }
                if (termStore == null)
                {
                    termStore = session.TermStores[0];
                }
            }
            if (LCID < 0)
            {
                LCID = termStore.WorkingLanguage;
            }
            if (termStore != null && !termStore.Languages.Contains(DefaultLCID))
            {
                DefaultLCID = termStore.WorkingLanguage;
                LCID = DefaultLCID;
            }
            return termStore;
        }
        private IAveTaxonomyField GetTaxonomyField(string columnName)
        {
            IAveTaxonomyField taxonomyField;
            var aveField = mAveList.SPList.Fields.GetFieldByInternalName(columnName, false);
            if (aveField != null && aveField is IAveTaxonomyField)
            {
                taxonomyField = aveField as IAveTaxonomyField;
                //taxonomySchemaField.Name = textField.InternalName;
            }
            else
            {
                throw new ArgumentException("Can't find Taxonomy Field or field type is not taxonomy, column name:{0}", columnName);
                //taxonomySchemaField.Name = mTaxonomyDic[columnName];
            }
            return taxonomyField;
        }
        private MetadataCacheInfo GetTermInfo(string columnName, AveFieldValueInfo valueInfo, int LCID = -1, bool forceAddTerm = true)
        {
            IAveTaxonomyField tField = null;
            Dictionary<Guid, IAveTerm> termCache = new Dictionary<Guid, IAveTerm>();
            Dictionary<Guid, Guid> termIdMapping = new Dictionary<Guid, Guid>();
            Dictionary<Guid, List<Guid>> mergedTermIdMapping = new Dictionary<Guid, List<Guid>>();
            tField = GetTaxonomyField(columnName);
            var textField = mAveList.SPList.Fields.GetFieldById(tField.TextField, false);

            if (!mMetadataInfoList.ContainsKey(columnName))
            {
                mMetadataInfoList[columnName] = new MetadataCacheInfo { TextFieldName = textField.InternalName, TextFieldId = textField.ID };
            }

            IAveTaxonomySession session = mAveList.SPList.ParentWeb.Site.AveSPTaxonomySession;
            IAveTermStore termStore = GetTermStore(tField, session, ref LCID);
            if (termStore == null)
            {
                return null;
            }

            IAveTermSet termSet = null;
            if (tField.TermSetId != Guid.Empty && termStore != null)
            {
                termSet = termStore.GetTermSet(tField.TermSetId);
            }

            IAveTerm endTerm = null;
            if (tField.AnchorId != Guid.Empty && termSet != null)
            {
                endTerm = termSet.GetTerm(tField.AnchorId);
            }

            var columnValue = valueInfo.ColValue.ToString();
            var newColumnValue = string.Empty;

            bool submit = false;
            HashSet<String> termNames = new HashSet<string>(columnValue.Split(';'), StringComparer.OrdinalIgnoreCase);

            foreach (string termName in termNames)
            {
                if (!mMetadataInfoList[columnName].TermValueMapping.ContainsKey(termName))
                {
                    string tName = termName.StartsWith("#", StringComparison.Ordinal) ? termName.Substring(1) : termName;
                    if (string.IsNullOrEmpty(tName) || string.IsNullOrEmpty(tName.Trim()))
                    {
                        continue;
                    }
                    var term = AveTaxonomyFieldUtility.FindTerm(tName, LCID, forceAddTerm, endTerm, termSet, tField, session, termCache, termIdMapping, mergedTermIdMapping, termStore, ref submit);
                    if (term != null)
                    {
                        var mappedValue = term.Name + "|" + term.ID;
                        if (!mMetadataInfoList[columnName].TermValueMapping.ContainsKey(termName))
                        {
                            mMetadataInfoList[columnName].TermValueMapping[termName] = mappedValue;
                        }
                        newColumnValue += mappedValue + ";";

                        //如果field不允许多值，没有必要找多个term了。
                        if (!tField.AllowMultipleValues)
                        {
                            break;
                        }
                    }
                    else
                    {
                        mLog.Warn("Can't find term info, term label {0}", termName);
                    }
                }
                else
                {
                    newColumnValue += mMetadataInfoList[columnName].TermValueMapping[termName] + ";";
                }
            }

            valueInfo.ColValue = newColumnValue.TrimEnd(';');

            return mMetadataInfoList[columnName];
        }
        private SPField ProcessLookupColumnValue(SPField field, AveFieldValueInfo valueInfo)
        {
            //ADO-190596
            SPLookupItem mSPLookupItem = null;
            try
            {
                SPLookupList spLookupListInfo = null;
                IAveFieldLookup lookupField = mAveList.SPList.Fields.GetById(valueInfo.Id) as IAveFieldLookup;
                Guid lookupListId = new Guid(lookupField.LookupList);
                IAveList lookupList = mAveList.SPList.ParentWeb.Site.OpenWeb(lookupField.LookupWebId).GetList(lookupListId);
                //lookup list already exists in the destination
                if (lookupList != null)
                {
                    if (!mSPLookupListCollection.ContainsKey(lookupListId))
                    {
                        spLookupListInfo = new SPLookupList()
                        {
                            Included = false,
                            Url = lookupList.RootFolder.ServerRelativeUrl,
                            Id = lookupList.ID.ToString(),
                        };
                        mSPLookupListCollection.Add(lookupList.ID, spLookupListInfo);
                    }
                    else
                    {
                        spLookupListInfo = mSPLookupListCollection[lookupListId];
                    }
                    if (!SPLookupLists.LookupList.Contains(spLookupListInfo))
                    {
                        SPLookupLists.LookupList.Add(spLookupListInfo);
                    }
                    //lookup item already exists in the destination
                    if (lookupField.AllowMultipleValues)
                    {
                        //mulit lookup values
                        if (valueInfo.ColValue != null && valueInfo.ColValue is List<LookupItemValue>)
                        {
                            List<LookupItemValue> lookupItemValues = valueInfo.ColValue as List<LookupItemValue>;
                            string lookupItemIdStrs = null;
                            foreach (LookupItemValue value in lookupItemValues)
                            {
                                try
                                {
                                    IAveListItem lookupItem = lookupList.GetItemById(value.ItemRowId);
                                    mSPLookupItem = new SPLookupItem()
                                    {
                                        Included = false,
                                        Url = lookupList.ParentWeb.ServerRelativeUrl + "/" + lookupItem.Url,
                                        Id = lookupItem.ID.ToString(),
                                        DocId = lookupItem.UniqueId.ToString(),
                                    };
                                    //防止LookupListMap.xml加入重复的lookup item信息
                                    if (mSPLookupListCollection.ContainsKey(lookupListId) && mSPLookupListCollection[lookupListId].LookupItems.Where(j => j.DocId == mSPLookupItem.DocId).ToList().Count == 0)
                                    {
                                        mSPLookupListCollection[lookupListId].LookupItems.Add(mSPLookupItem);
                                    }

                                    lookupItemIdStrs += lookupItem.ID.ToString() + ";# ;#";
                                }
                                catch (Exception e)
                                {
                                    mLog.Warn("An error occurred while getting multi lookup value, Message:{0}", e.ToString());
                                }
                            }
                            //for manifest file xml --> value = itemID;# ;#itemID;# ;itemIDLookupListID
                            if (!string.IsNullOrEmpty(lookupItemIdStrs))
                            {
                                field.Value = lookupItemIdStrs.TrimEnd('#') + lookupListId.ToString();
                            }
                            else
                            {
                                field.Value = lookupListId.ToString();
                            }
                        }
                    }
                    else
                    {
                        //single value
                        if (valueInfo.ColValue != null && valueInfo.ColValue is LookupItemValue)
                        {
                            LookupItemValue lookupItemValue = valueInfo.ColValue as LookupItemValue;
                            IAveListItem lookupItem = lookupList.GetItemById(lookupItemValue.ItemRowId);
                            mSPLookupItem = new SPLookupItem()
                            {
                                Included = false,
                                Url = lookupList.ParentWeb.ServerRelativeUrl + "/" + lookupItem.Url,
                                Id = lookupItem.ID.ToString(),
                                DocId = lookupItem.UniqueId.ToString(),
                            };
                            //防止LookupListMap.xml 加入重复的lookup item 信息
                            if (mSPLookupListCollection.ContainsKey(lookupListId) &&
                                mSPLookupListCollection[lookupListId].LookupItems.Where(j => j.DocId == mSPLookupItem.DocId).ToList().Count == 0)
                            {
                                mSPLookupListCollection[lookupListId].LookupItems.Add(mSPLookupItem);
                            }
                            //for manifest file xml --> value = itemID;LookupListID
                            field.Value = lookupItem.ID.ToString() + ";" + lookupListId.ToString();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Warn("Error occurred while getting the lookup value for HSM, Info:{0}", e.ToString());
            }
            return field;
        }
        private List<string> SetNeedSetNullFieldsEx(List<string> fieldValues)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AveSPList.SetNeedSetNullFields"))
            {

                List<string> needSetNullFields = new List<string>();
                string[] AllCols = new string[] {"nvarchar1" ,"nvarchar2" ,"nvarchar3" ,"nvarchar4" ,"nvarchar5" ,"nvarchar6" ,"nvarchar7" ,"nvarchar8" ,
                "ntext1" ,"ntext2" ,"ntext3" ,"ntext4" ,"sql_variant1","nvarchar9" ,"nvarchar10" ,"nvarchar11" ,"nvarchar12" ,"nvarchar13" ,
                "nvarchar14" ,"nvarchar15" ,"nvarchar16" ,"ntext5" ,"ntext6" ,"ntext7" ,"ntext8" ,"sql_variant2","nvarchar17" ,"nvarchar18" ,
                "nvarchar19" ,"nvarchar20" ,"nvarchar21" ,"nvarchar22" ,"nvarchar23" ,"nvarchar24" ,"ntext9" ,"ntext10" ,"ntext11" ,"ntext12" ,
                "sql_variant3","nvarchar25" ,"nvarchar26" ,"nvarchar27" ,"nvarchar28" ,"nvarchar29" ,"nvarchar30" ,"nvarchar31" ,"nvarchar32" ,
                "ntext13" ,"ntext14" ,"ntext15" ,"ntext16" ,"sql_variant4","nvarchar33" ,"nvarchar34" ,"nvarchar35" ,"nvarchar36" ,"nvarchar37" ,
                "nvarchar38" ,"nvarchar39" ,"nvarchar40" ,"ntext17" ,"ntext18" ,"ntext19" ,"ntext20" ,"sql_variant5","nvarchar41" ,"nvarchar42" ,
                "nvarchar43" ,"nvarchar44" ,"nvarchar45" ,"nvarchar46" ,"nvarchar47" ,"nvarchar48" ,"ntext21" ,"ntext22" ,"ntext23" ,"ntext24" ,
                "sql_variant6","nvarchar49" ,"nvarchar50" ,"nvarchar51" , "nvarchar52" ,"nvarchar53" ,"nvarchar54" ,"nvarchar55" ,"nvarchar56" ,
                "ntext25" ,"ntext26" ,"ntext27" ,"ntext28" ,"sql_variant7","nvarchar57" ,"nvarchar58" ,"nvarchar59" ,"nvarchar60" ,"nvarchar61" ,
                "nvarchar62" ,"nvarchar63" ,"nvarchar64" ,"ntext29" ,"ntext30" ,"ntext31" ,"ntext32" ,"sql_variant8","int1","int2","int3","int4",
                "int5","int6","int7","int8","int9","int10","int11","int12","int13","int14","int15","int16","float1","float2","float3","float4",
                "float5","float6","float7","float8","float9","float10","float11","float12", "datetime1","datetime2","datetime3","datetime4",
                "datetime5","datetime6","datetime7","datetime8","bit1","bit2","bit3","bit4","bit5","bit6","bit7","bit8","bit9","bit10","bit11",
                "bit12","bit13","bit14","bit15","bit16","uniqueidentifier1"};

                //ExternalList 没有ColName，会抛异常
                if (mAveList != null && mAveList.SPList.BaseTemplate != AveListTemplateType.ExternalList && (int)mAveList.SPList.BaseTemplate != 160)
                {
                    IAveFieldCollection fieldCollection = mAveList.SPList.Fields;
                    bool isCollecterList = mAveList.SPList.IsConnectorList.HasValue ? mAveList.SPList.IsConnectorList.Value : false;
                    foreach (IAveField field in fieldCollection)
                    {
                        try
                        {
                            object obj = field.ColName;
                            if (obj != null
                                //ADO-129426 item的SetNeedSetNullFields逻辑中，过滤BaseType是Facilities类型的column，在还column的过程中，
                                //如果将这个column设为null，在update的时候会报System.Exception: Field or property "Facilities" does not exist.的错。
                                && !string.Equals(field.TypeAsString, "Facilities", StringComparison.OrdinalIgnoreCase)
                                //ADO-89825 App Store Site中，特殊field AppMetadataLocale不能设置为null。
                                && !field.ID.Equals(new Guid("{14c6cd06-7417-42c1-a051-89e455fd1090}")))
                            {
                                string colName = obj.ToString();
                                if (IsColColumn(colName) && IsSupportToSetNull(field.InternalName))
                                {
                                    if (field.Type == AveFieldType.WorkflowStatus || fieldValues.Exists(name => name.Equals(field.InternalName, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        continue;
                                    }
                                    needSetNullFields.Add(field.InternalName);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            mLog.Log(AveLogLevel.WARN, "An error occurred while SetNeedSetNullFields. error:{0}", e.ToString());
                        }
                    }
                }
                return needSetNullFields;

            }

        }
        private bool IsSupportToSetNull(string internalName)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.IsSupportToSetNull"))
            {
#endif
            bool isSupportToSetNull = true;
            try
            {
                if ((string.Equals(internalName, "_dlc_Reporting_TemplateId", StringComparison.Ordinal)
                    || string.Equals(internalName, "_dlc_Reporting_QueryAssembly", StringComparison.Ordinal)
                    || string.Equals(internalName, "_dlc_Reporting_InjectionAssembly", StringComparison.Ordinal)
                    || string.Equals(internalName, "_dlc_Reporting_InjectionClass", StringComparison.Ordinal)
                    || string.Equals(internalName, "_dlc_Reporting_IconUrl", StringComparison.Ordinal)
                    || string.Equals(internalName, "_dlc_Reporting_HttpContentType", StringComparison.Ordinal)
                    && IsReportingMetadataList()))
                {
                    isSupportToSetNull = false;
                }
            }
            catch (Exception e)
            {
                mLog.Warn("charge whether the list field is '_dlc_Reporting_TemplateId', Exception:{0}", e.ToString());
            }
            return isSupportToSetNull;
#if PerformanceLog
            }
#endif
        }
        private bool IsReportingMetadataList()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.IsReportingMetadataList"))
            {
#endif
            bool isReportMetadataList = false;
            try
            {
                AveSPList list = mAveList;
                AveSPWeb web = mAveList.ParentWeb;
                if (web.SPWeb.Properties != null)
                {
                    if (web.SPWeb.Properties.ContainsKey("_reportinggallerymetadataid"))
                    {
                        if (string.Equals(web.SPWeb.Properties["_reportinggallerymetadataid"].ToString(), list.Id.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            isReportMetadataList = true;
                        }
                    }
                }
                else
                {
                    if (web.SPWeb.AllProperties.ContainsKey("_reportinggallerymetadataid"))
                    {
                        if (string.Equals(web.SPWeb.AllProperties["_reportinggallerymetadataid"].ToString(), list.Id.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            isReportMetadataList = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Warn("charge whether the list is reporting metadata error,Exception:{0}", e.ToString());
            }
            return isReportMetadataList;
#if PerformanceLog
            }
#endif
        }
        private bool IsColColumn(string colName)
        {
            //添加对column 类型的判断，SP对类型的数量是有限制的，可以通过SP的数据库查询，当前没发现超过数据的情况，因此没有添加对于超过限制的判断，如果有问题，需要添加检查类型数量的逻辑
            List<string> allcols = new List<string> { "nvarchar", "ntext", "sql_variant", "int", "float", "datetime", "bit", "uniqueidentifier" };
            Regex reg = new Regex("^(nvarchar|ntext|sql_variant|int|float|datetime|bit|uniqueidentifier)[0-9]*$");
            return reg.IsMatch(colName);
        }
        public SPGenericObject ProcessListItemNode(Dictionary<string, object> docData, Dictionary<string, object> userData, List<Dictionary<string, object>> dataJunction, bool isVersion, SPListItem item)
        {
            string id = docData["Id"].ToString();
            if (userData.ContainsKey(AveFieldNameCollection.HTMLFileType))
            {
                mLog.Info("this item has html file type field. value is {0}", userData[AveFieldNameCollection.HTMLFileType] == null ? "null" : userData[AveFieldNameCollection.HTMLFileType].ToString());
            }
            if (SPObject == null)
            {
                SPObject = new SPGenericObject();
                SPObject.Id = id;
                SPListItem itemObject = new SPListItem();
                SPObject.Item = itemObject;
            }
            else if (IsNewObject || !SPObject.Id.Equals(id, StringComparison.OrdinalIgnoreCase))
            {
                if (SPObjectCollection == null)
                {
                    SPObjectCollection = new SPGenericObjectCollection();
                }
                SPObject = new SPGenericObject();
                SPObject.Id = id;
                SPListItem itemObject = new SPListItem();
                SPObject.Item = itemObject;
            }

            if (isVersion)
            {
                SPListItem itemObject = (SPListItem)SPObject.Item;
                if (itemObject.Items.Count == 0)
                {
                    SPListItemVersionCollection versions = new SPListItemVersionCollection();
                    itemObject.Items.Add(versions);
                }
                SPListItemVersionCollection versionCollection = (SPListItemVersionCollection)itemObject.Items[0];
                versionCollection.ListItem.Add(item);
            }
            else
            {
                SPObject.Id = item.Id;
                SPObject.Name = item.Name;
                SPObject.ObjectType = SPObjectType.SPListItem;
                SPObject.ParentWebUrl = ParentWebUrl;
                SPObject.ParentWebId = ParentWebId;
                SPObject.ParentId = ParentListId;
                SPObject.Url = ParentWebUrl.TrimEnd('/') + "/" + item.FileUrl;

                SPListItem itemObject = (SPListItem)SPObject.Item;
                if (itemObject.Items != null && itemObject.Items.Count > 0)
                {
                    SPListItemVersionCollection versionCollection = (SPListItemVersionCollection)itemObject.Items[0];
                    versionCollection.ListItem.Add(item);

                    itemObject.Author = item.Author;
                    itemObject.ContentTypeId = item.ContentTypeId;
                    itemObject.DirName = item.DirName;
                    itemObject.DocId = item.DocId;
                    itemObject.DocType = item.DocType;
                    itemObject.FileUrl = item.FileUrl;
                    itemObject.Id = item.Id;
                    itemObject.IntId = item.IntId;
                    itemObject.ModerationComment = item.ModerationComment;
                    itemObject.ModerationStatus = item.ModerationStatus;
                    itemObject.ModifiedBy = item.ModifiedBy;
                    itemObject.Name = item.Name;
                    itemObject.Order = item.Order;
                    itemObject.ParentFolderId = item.ParentFolderId;
                    itemObject.ParentListId = item.ParentListId;
                    itemObject.ParentWebId = item.ParentWebId;
                    if (item.TimeCreated != DateTime.MinValue)
                    {
                        itemObject.TimeCreated = item.TimeCreated;
                    }
                    else
                    {
                        itemObject.TimeCreated = DateTime.UtcNow;
                    }
                    if (item.TimeLastModified != DateTime.MinValue)
                    {
                        itemObject.TimeLastModified = item.TimeLastModified;
                    }
                    else
                    {
                        itemObject.TimeLastModified = DateTime.UtcNow;
                    }
                    itemObject.Version = item.Version;
                    //listitemlabel setting
                    if (!string.IsNullOrEmpty(item.ComplianceFlags) &&
                       !string.IsNullOrEmpty(item.ComplianceTagWrittenTime) &&
                       !string.IsNullOrEmpty(item.ComplianceTagUserId) &&
                       !string.IsNullOrEmpty(item.ComplianceTag))
                    {
                        itemObject.ComplianceFlags = item.ComplianceFlags;
                        itemObject.ComplianceTagWrittenTime = item.ComplianceTagWrittenTime;
                        itemObject.ComplianceTagUserId = item.ComplianceTagUserId;
                        itemObject.ComplianceTag = item.ComplianceTag;
                    }


                    //itemfields
                    itemObject.Items.Add(item.Items[0]);

                    for (int i = versionCollection.ListItem.Count - 1; i >= 0; i--)
                    {
                        SPListItem versionItem = versionCollection.ListItem[i];
                        versionItem.FileUrl = item.FileUrl;
                        versionItem.DirName = item.DirName;
                        versionItem.ParentFolderId = item.ParentFolderId;
                    }

                }
                else if (itemObject.Items != null && itemObject.Items.Count == 0 && item.DocType != ListItemDocType.Folder)
                {
                    if (itemObject.Items.Count == 0)
                    {
                        SPListItemVersionCollection versions = new SPListItemVersionCollection();
                        itemObject.Items.Add(versions);
                    }
                    SPListItemVersionCollection versionCollection = (SPListItemVersionCollection)itemObject.Items[0];
                    versionCollection.ListItem.Add(item);
                    itemObject.Author = item.Author;
                    itemObject.ContentTypeId = item.ContentTypeId;
                    itemObject.DirName = item.DirName;
                    itemObject.DocId = item.DocId;
                    itemObject.DocType = item.DocType;
                    itemObject.FileUrl = item.FileUrl;
                    itemObject.Id = item.Id;
                    itemObject.IntId = item.IntId;
                    itemObject.ModerationComment = item.ModerationComment;
                    itemObject.ModerationStatus = item.ModerationStatus;
                    itemObject.ModifiedBy = item.ModifiedBy;
                    itemObject.Name = item.Name;
                    itemObject.Order = item.Order;
                    itemObject.ParentFolderId = item.ParentFolderId;
                    itemObject.ParentListId = item.ParentListId;
                    itemObject.ParentWebId = item.ParentWebId;
                    if (item.TimeCreated != DateTime.MinValue)
                    {
                        itemObject.TimeCreated = item.TimeCreated;
                    }
                    else
                    {
                        itemObject.TimeCreated = DateTime.UtcNow;
                    }
                    if (item.TimeLastModified != DateTime.MinValue)
                    {
                        itemObject.TimeLastModified = item.TimeLastModified;
                    }
                    else
                    {
                        itemObject.TimeLastModified = DateTime.UtcNow;
                    }
                    itemObject.Version = item.Version;
                    //listitemlabel setting
                    //if (!string.IsNullOrEmpty(item.ComplianceFlags) &&
                    //   !string.IsNullOrEmpty(item.ComplianceTagWrittenTime) &&
                    //   !string.IsNullOrEmpty(item.ComplianceTagUserId) &&
                    //   !string.IsNullOrEmpty(item.ComplianceTag))
                    if (!string.IsNullOrEmpty(item.ComplianceFlags) &&
                       !string.IsNullOrEmpty(item.ComplianceTag))
                    {
                        itemObject.ComplianceFlags = item.ComplianceFlags;
                        //itemObject.ComplianceTagWrittenTime = item.ComplianceTagWrittenTime;
                        //itemObject.ComplianceTagUserId = item.ComplianceTagUserId;
                        itemObject.ComplianceTag = item.ComplianceTag;
                    }



                    //itemfields
                    if (item.Items.Count() > 0)
                    {
                        itemObject.Items.Add(item.Items[0]);
                    }

                    for (int i = versionCollection.ListItem.Count - 1; i >= 0; i--)
                    {
                        SPListItem versionItem = versionCollection.ListItem[i];
                        versionItem.FileUrl = item.FileUrl;
                        versionItem.DirName = item.DirName;
                        versionItem.ParentFolderId = item.ParentFolderId;
                    }

                }
                else
                {
                    SPObject.Item = item;
                }

                //using (new AvePerformanceScope("HSWorker.ProcessRoleAssignmentsXML"))
                //{
                //    var roleAssignments = AvailableResource.MetadataGenerator.GetData<List<AveRoleAssignmentInfo>>(AveMetadataType.RoleAssignment.ToString());
                //    var sharedLinks = ProcessRoleAssignmentsXML(roleAssignments, item.Id, item.FileUrl, aveItem, objectIdentity);
                //    if (sharedLinks != null && sharedLinks.Count > 0)
                //    {
                //        var reportKey = AssembleObjectReportKey(docData);
                //        if (!CurrentPackageSharedLinks.TryGetValue(reportKey, out var links))
                //        {
                //            links = new List<PostCacheObjectShareLink>();
                //        }
                //        links.AddRange(sharedLinks);
                //        CurrentPackageSharedLinks.TryAdd(reportKey, links);
                //    }
                //}

                
            }
            return SPObject;
        }

        public string GenerateFileServerRelativeUrl(string fileName)
        {
            return $"{ParentWebUrl}/{GenerateWebRelativeUrl(fileName)}";
        }

        public SPGenericObject ProcessFileObjectNode(Dictionary<string, object> docData, Dictionary<string, object> userData, List<Dictionary<string, object>> dataJunction, bool isVersion, SPFile file)
        {
            string id = docData["Id"].ToString();
            if (SPFileObject == null)
            {
                SPFileObject = new SPGenericObject();
                SPFileObject.Id = id;
                SPFile fileObject = new SPFile();
                SPFileObject.Item = fileObject;
            }
            else if (!SPFileObject.Id.Equals(id, StringComparison.OrdinalIgnoreCase))
            {
                if (SPObjectCollection == null)
                {
                    SPObjectCollection = new SPGenericObjectCollection();
                }
                SPFileObject = new SPGenericObject();
                SPFileObject.Id = id;
                SPFile fileObject = new SPFile();
                SPFileObject.Item = fileObject;
            }

            if (isVersion)
            {
                SPFile fileObject = (SPFile)SPFileObject.Item;
                if (fileObject.Versions == null)
                {
                    fileObject.Versions = new List<SPFile>();
                }
                fileObject.Versions.Add(file);
            }
            else
            {
                SPFileObject.Id = file.Id;
                SPFileObject.ObjectType = SPObjectType.SPFile;
                SPFileObject.ParentId = file.ParentId;
                SPFileObject.ParentWebId = file.ParentWebId;
                SPFileObject.ParentWebUrl = file.ParentWebUrl;
                SPFileObject.Url = $"{ParentWebUrl}/{file.Url}";// file.ParentWebUrl.TrimEnd('/') + "/" + file.Url;
                SPFile fileObject = (SPFile)SPFileObject.Item;
                if (fileObject.Versions != null && fileObject.Versions.Count > 0)
                {
                    if (fileObject.Versions == null)
                    {
                        fileObject.Versions = new List<SPFile>();
                    }
                    fileObject.Versions.Add(file);
                    fileObject.Url = file.Url;
                    fileObject.Id = file.Id;
                    fileObject.ParentWebId = file.ParentWebId;
                    fileObject.ParentWebUrl = file.ParentWebUrl;
                    fileObject.Name = file.Name;
                    fileObject.ListId = file.ListId;
                    fileObject.ParentId = file.ParentId;
                    fileObject.TimeCreated = file.TimeCreated;
                    fileObject.TimeLastModified = file.TimeLastModified;
                    fileObject.Version = file.Version;
                    fileObject.FileValue = file.FileValue;
                    fileObject.Author = file.Author;
                    fileObject.ModifiedBy = file.ModifiedBy;
                    fileObject.InDocumentLibrary = file.InDocumentLibrary;
                    fileObject.ListItemIntId = file.ListItemIntId;
                    for (int i = fileObject.Versions.Count - 1; i >= 0; i--)
                    {
                        SPFile versionFile = fileObject.Versions[i];
                        versionFile.Url = file.Url;
                    }
                }
                else
                {
                    SPFileObject.Item = file;
                }
            }

            return SPFileObject;
        }

        public DeploymentRoleAssignment GenerateDeploymentRoleAssignments()
        {
            if (RoleAssignmentsObject == null)
            {
                RoleAssignmentsObject = new SPGenericObject()
                {
                    Id = Guid.NewGuid().ToString(),
                    ParentId = ParentWebId,
                    ParentWebId = ParentWebId,
                    ParentWebUrl = ParentWebUrl,
                    ObjectType = SPObjectType.DeploymentRoleAssignments,
                    Item = new DeploymentRoleAssignments(),
                };
            }

            DeploymentRoleAssignment roleAssignment = new DeploymentRoleAssignment()
            {
                ScopeId = SPObject.Id,
                ObjectId = SPObject.Id,
                ObjectType = "2",
                Assignment = new List<DeploymentAssignment>(),
                RoleDefWebId = ParentWebId,
                RoleDefWebUrl = ParentWebUrl,
                ObjectUrl = SPObject.Url,
                AnonymousPermMask = "0",
            };
            ((DeploymentRoleAssignments)RoleAssignmentsObject.Item).RoleAssignment.Add(roleAssignment);
            return roleAssignment;
        }

        public DeploymentAssignment GenerateDeploymentAssignment(DeploymentRoleAssignment roleAssignment, IAvePrincipal principal, IAveRoleDefinition spRoleDefinition)
        {
            DeploymentAssignment assignment = new DeploymentAssignment();
            assignment.PrincipalId = principal.ID.ToString();
            assignment.RoleId = spRoleDefinition.ID.ToString();
            roleAssignment.Assignment.Add(assignment);
            if (!UsedRoleID.Contains(spRoleDefinition.ID.ToString()))
            {
                UsedRoleID.Add(spRoleDefinition.ID.ToString());
            }
            if (!UserIdCache.Contains(principal.ID))
            {
                UserIdCache.Add(principal.ID);
            }
            if (!mUserGroupMappingForCurrentPackage.Contains(principal.ID))
            {
                mUserGroupMappingForCurrentPackage.Add(principal.ID);
            }
            return assignment;
        }

        public string GenerateContentPath(string fileKey)
        {
            FileValue++;
            string azureFileValue = $"{FileValue}.dat";
            string datFilePath = SecurityUtils.SafeCombinePath(TempContentPath, azureFileValue);
            lock (fileValueLock)
            {
                if (!mFileValueDic.ContainsKey(fileKey))
                {
                    mFileValueDic.Add(fileKey, azureFileValue);
                }
                else
                {
                    throw new Exception($"Same key exists in File value dictionary. key:{fileKey}");
                }
            }
            return datFilePath;
        }

        #region ---Deploynt XML---

        public void StorageManifest()
        {
            if (SPObjectCollection.SPObject.Count > 0)
            {
                try
                {
                    if (RoleAssignmentsObject != null && RoleAssignmentsObject.Item != null)
                    {
                        if (mRoleDefinitionObject != null && mRoleDefinitionObject.Item != null && mRoleDefinitionObject.Item is DeploymentRoles)
                        {
                            SPGenericObject usedRoleDefinitionObject = (SPGenericObject)mRoleDefinitionObject.Clone();
                            DeploymentRoles originalRoles = mRoleDefinitionObject.Item as DeploymentRoles;
                            DeploymentRoles usedRoles = new DeploymentRoles();
                            foreach (DeploymentRole role in originalRoles.Role)
                            {
                                if (UsedRoleID.Contains(role.RoleId))
                                {
                                    usedRoles.Role.Add(role);
                                }
                            }
                            usedRoleDefinitionObject.Item = usedRoles;
                            SPObjectCollection.SPObject.Add(usedRoleDefinitionObject);
                        }
                        SPObjectCollection.SPObject.Add(RoleAssignmentsObject);
                    }
                    XmlSerializer(Path.Combine(TempManifestPath, SPMHSConstant.MANIFEST_XML_NAME), SPObjectCollection);
                }
                catch (Exception ex)
                {
                    mLog.Error("An error occurred while processing manifest xml. Exception: {0}.", ex.ToString());
                    SPObjectCollection.SPObject.Clear();
                    RoleAssignmentsObject = null;
                    throw new Exception("Manifest");
                }
                StorageSystemDataXML();
                SPObjectCollection.SPObject.Clear();
                RoleAssignmentsObject = null;
            }
            void StorageSystemDataXML()
            {
                try
                {
                    SPSystemData systemData = new SPSystemData();
                    SPSchemaVersion schemaVersion = new SPSchemaVersion();
                    schemaVersion.Version = "15.0.0.0";
                    schemaVersion.SiteVersion = "15";
                    schemaVersion.DatabaseVersion = "11552";
                    schemaVersion.Build = "16.0.3111.1200";
                    schemaVersion.ObjectsProcessed = SPObjectCollection.SPObject.Count;

                    systemData.SchemaVersion = schemaVersion;

                    SPManifestFile manifestFile = new SPManifestFile();
                    manifestFile.Name = "Manifest.xml";

                    systemData.ManifestFiles.Add(manifestFile);

                    try
                    {
                        SPSystemObject systemObject2 = new SPSystemObject();
                        systemObject2.Id = SiteUserInfoListId;
                        systemObject2.Url = SiteUserInfoListUrl;
                        systemObject2.Type = SPDeploymentObjectType.List;
                        systemData.SystemObjects.Add(systemObject2);
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn($"Assemble System data user info list failed. exception:{ex.ToString()}");
                    }
                    SPSystemObject systemObject3 = new SPSystemObject();
                    systemObject3.Id = RootWebId;
                    systemObject3.Url = RootWebUrl;
                    systemObject3.Type = SPDeploymentObjectType.Web;

                    systemData.SystemObjects.Add(systemObject3);

                    XmlSerializer(Path.Combine(TempManifestPath, SPMHSConstant.SYSTEMDATA_XML_NAME), systemData);
                }
                catch (Exception e)
                {
                    mLog.Error("An error occurred while processing SystemDataXML. Exception: {0}.", e.ToString());
                    throw new Exception("SystemData");
                }
            }
        }

        public void StorageRequirementXML()
        {
            using (new AvePerformanceScope("HSWorker.StorageRequirementXML"))
            {
                StorageExportSettingsXml();
                StorageRequirementsXml();
                StorageRootObjectMapXML();
                StorageViewFormsListXML();
            }

            void StorageExportSettingsXml()
            {
                try
                {
                    var spExportSettings = new SPExportSettings()
                    {
                        SiteUrl = ParentSiteFullUrl,
                        FileLocation = string.Empty,//location
                        BaseFileName = "Doclib11.cmp",//need to change
                        IncludeSecurity = SPIncludeSecurity.All,
                        ExportPublicSchema = true,
                        ExportFrontEndFileStreams = true,
                        ExportMethod = SPExportMethodType.ExportAll,
                        ExcludeDependencies = false,

                    };

                    //if (GlobalPreferenceSettings.EnableMigrationAPISourceTypeV2)
                    //{
                    //    if (WrapperConfiguration.SourceIsOneDriveSite)
                    //    {
                    //        spExportSettings.SourceType = SourceType.OneDrive.ToString();
                    //    }
                    //    else
                    //    {
                    //        spExportSettings.SourceType = WrapperConfiguration.SourceIsOnlineSite ? SourceType.SharePointOnline.ToString() : SourceType.SharePointOnPremServer.ToString();
                    //    }
                    //}
                    //else if (GlobalPreferenceSettings.EnableMigrationAPISourceType)
                    //{
                    //    spExportSettings.SourceType = SourceType.Other.ToString();
                    //    if (WrapperConfiguration.SourceIsOnlineSite)
                    //    {
                    //        spExportSettings.DetailedSource = WrapperConfiguration.SourceIsOneDriveSite ? SourceType.OneDrive.ToString() : SourceType.SharePointOnline.ToString();
                    //    }
                    //    else
                    //    {
                    //        spExportSettings.DetailedSource = SourceType.SharePointOnPremServer.ToString();
                    //    }
                    //}

                    spExportSettings.ExportObjects.Add(new SPExportObject()
                    {
                        Id = ParentListId,
                        Type = SPDeploymentObjectType.List,
                        ParentId =ParentWebId,
                        Url = ParentListUrl,
                        ExcludeChildren = false,
                        IncludeDescendants = SPIncludeDescendants.All,
                    });
                    XmlSerializer(Path.Combine(TempManifestPath, SPMHSConstant.EXPORTSETTINGS_XML_NAME), spExportSettings);
                }
                catch (Exception e)
                {
                    mLog.Error("An error occurred while processing ExportSettingsXml. Exception: {0}.", e.ToString());
                    throw new Exception("ExportSettings");
                }
            }

            void StorageRequirementsXml()
            {
                try
                {
                    var spImportRequirements = new SPImportRequirements();
                    XmlSerializer(Path.Combine(TempManifestPath, SPMHSConstant.REQUIREMENTS_XML_NAME), spImportRequirements);
                }
                catch (Exception e)
                {
                    mLog.Error("An error occurred while processing RequirementsXml. Exception: {0}.", e.ToString());
                    throw new Exception("Requirements");
                }
            }
            void StorageRootObjectMapXML()
            {
                try
                {
                    SPRootObjects rootObjects = new SPRootObjects();
                    SPRootObject rootObject = new SPRootObject();
                    rootObject.IsDependency = false;
                    rootObject.Url = ParentListUrl;
                    rootObject.WebUrl = ParentWebUrl;
                    rootObject.ParentId = ParentWebId;
                    rootObject.Type = SPDeploymentObjectType.List;
                    rootObject.Id = ParentListId;
                    rootObjects.RootObject.Add(rootObject);
                    XmlSerializer(Path.Combine(TempManifestPath, SPMHSConstant.ROOTOBJECTMAP_XML_NAME), rootObjects);
                }
                catch (Exception e)
                {
                    mLog.Error("An error occurred while processing RootObjectMap xml. Exception: {0}.", e.ToString());
                    throw new Exception("RootObjectMap");
                }
            }
            void StorageViewFormsListXML()
            {
                try
                {
                    SPViewFormsList viewFormsList = new SPViewFormsList();
                    SPViewForm viewForm = new SPViewForm();
                    viewForm.Id = "";
                    viewForm.Type = "";
                    viewFormsList.ViewForm.Add(viewForm);
                    XmlSerializer(Path.Combine(TempManifestPath, SPMHSConstant.VIEWFORMSLIST_XML_NAME), viewFormsList);
                }
                catch (Exception e)
                {
                    mLog.Error("An error occurred while processing ViewFormsListXML. Exception: {0}.", e.ToString());
                    throw new Exception("ViewFormsList");
                }
            }
        }
        public void StorageUserGroupXML()
        {
            try
            {
                try
                {
                    mUserGroupMap.Users.Clear();
                    mUserGroupMap.Groups.Clear();
                    if (_config.StubUserInfos != null)
                    {
                        List<string> cache = new List<string>();
                        foreach (AveUserInfo userInfo in _config.StubUserInfos)
                        {
                            if (mUserGroupMappingForCurrentPackage.Contains(userInfo.ID))
                            {
                                if (!cache.Contains(userInfo.Login))
                                {
                                    DeploymentUser dUser = new DeploymentUser();
                                    dUser.Id = userInfo.ID.ToString();
                                    dUser.Login = userInfo.Login;
                                    dUser.Name = userInfo.Title;
                                    dUser.Email = userInfo.Email;
                                    dUser.IsDomainGroup = userInfo.DomainGroup;
                                    dUser.IsSiteAdmin = userInfo.SiteAdmin;
                                    dUser.SystemId = Convert.ToBase64String(userInfo.SystemID ?? Guid.NewGuid().ToByteArray());
                                    if (userInfo.Deleted == 0)
                                    {
                                        dUser.IsDeleted = false;
                                    }
                                    if (userInfo.Deleted == 1)
                                    {
                                        dUser.IsDeleted = true;
                                    }
                                    if (!string.IsNullOrEmpty(dUser.Login) && !string.IsNullOrEmpty(dUser.Name) && dUser.Login.Equals(dUser.Name, StringComparison.OrdinalIgnoreCase))
                                    {
                                        dUser.Login = dUser.Name + "_PlaceHolder";
                                    }

                                    mUserGroupMap.Users.Add(dUser);
                                    cache.Add(userInfo.Login);
                                }
                                else
                                {
                                    mLog.Info("DeploymentUsers already contains {0}, id:{1},Title{2}", userInfo.Login, userInfo.ID, userInfo.Title);
                                }
                            }
                        }
                    }
                    if (_config.StubGroupInfos != null)
                    {
                        foreach (AveGroupInfo group in _config.StubGroupInfos)
                        {
                            if (mUserGroupMappingForCurrentPackage.Contains(group.ID))
                            {
                                DeploymentGroup dGroup = new DeploymentGroup();
                                dGroup.Id = group.ID.ToString();
                                dGroup.Name = group.Title;
                                dGroup.Description = group.Description;
                                dGroup.Owner = group.Owner.ToString();
                                dGroup.OwnerIsUser = group.OwnerIsUser;
                                dGroup.RequestToJoinLeaveEmailSetting = "";
                                dGroup.OnlyAllowMembersViewMembership = group.OnlyAllowMembersViewMembership;
                                mUserGroupMap.Groups.Add(dGroup);
                            }
                        }
                    }
                    XmlSerializer(Path.Combine(TempManifestPath, SPMHSConstant.USETGROUP_XML_NAME), mUserGroupMap);
                    try
                    {
                        mLog.Info($"StorageUserGroupXMLXml.Groups Count:{mUserGroupMap.Groups.Count}." +
                        $"Users Count:{mUserGroupMap.Users.Count}." +
                        $"mUserGroupMappingForCurrentPackage Count:{mUserGroupMappingForCurrentPackage.Count}." +
                        $"mConfig.StubGroupInfos Count:{_config.StubGroupInfos.Count}." +
                        $"mConfig.StubUserInfos Count:{_config.StubUserInfos.Count}.");
                    }
                    catch (Exception e)
                    {
                        mLog.Error("An error occurred while StorageUserGroupXMLXml. Exception: {0}.", e.ToString());
                    }
                    mUserGroupMappingForCurrentPackage.Clear();
                }
                catch (Exception e)
                {
                    mUserGroupMappingForCurrentPackage.Clear();
                    mLog.Error("An error occurred while StorageUserGroupXMLXml. Exception: {0}.", e.ToString());
                }
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while processing usergroup xml. Exception: {0}.", e.ToString());
                throw new Exception("UserGroup");
            }
        }
        public void ResetObjects()
        {
            CurrentPackageCount = 0;
            CurrentPackageSize = 0;
            SPObjectCollection.SPObject.Clear();
            UserIdCache.Clear();
            SPLookupLists = new SPLookupLists();
            // clear role assignment state between packages to avoid leaking principals/roles
            RoleAssignmentsObject = null;
            UsedRoleID.Clear();
            mUserGroupMappingForCurrentPackage.Clear();
        }

        public void Increase(long size, bool increasePackageCount)
        {
            CurrentPackageSize += size;
            if (increasePackageCount)
            {
                CurrentPackageCount++;
            }
        }

        public bool ShouldSplitPackage()
        {
            var shouldSplit = CurrentPackageCount >= _config.MigrationImportJobPackageCountCapacity || CurrentPackageSize >= _config.MigrationImportJobPackageSizeCapacity;
            if (shouldSplit)
            {
                mLog.Info($"Current package reached capacity. CurrentPackageCount: {CurrentPackageCount}, CurrentPackageSize: {CurrentPackageSize} bytes. Thresholds - PackageCountCapacity: {_config.MigrationImportJobPackageCountCapacity}, PackageSizeCapacity: {_config.MigrationImportJobPackageSizeCapacity} bytes.");
            }
            return shouldSplit;
        }

        /// <summary>
        /// Finalizes the current package and prepares it for import.
        /// This method encapsulates the package preparation workflow previously in SplitPackage.
        /// </summary>
        /// <param name="web">The SharePoint web context for processing role definitions</param>
        /// <param name="isLastPackage">Indicates if this is the last package</param>
        /// <returns>PackageStatus indicating the result (Ready, NotReady, Empty, or Error)</returns>
        public PackageStatus FinalizePackage(IAveWeb web, bool isLastPackage = false)
        {
            LastError = null;
            PackageStatus status = PackageStatus.NotReady;
            if (isLastPackage || ShouldSplitPackage())
            {
                if (CurrentPackageCount > 0)
                {
                    mLog.Debug("Start FinalizePackage. isLastPackage: {0}, CurrentPackageCount: {1}", isLastPackage, CurrentPackageCount);

                    try
                    {
                        // Process role definitions (SharePoint permissions)
                        using (new AvePerformanceScope("HSWorker.ProcessRoleDefinitionsXML"))
                        {
                            ProcessRoleDefinitionsXML(web);
                        }

                        if (SPObjectCollection.SPObject.Count > 0)
                        {
                            // Storage phase: Generate all required XML files
                            using (new AvePerformanceScope("HSWorker.ProcessUserGroupXML"))
                            {
                                StorageUserGroupXML();
                            }
                            StorageRequirementXML();
                            StorageManifest();
                            StorageLookupListMapXml();
                            ResetObjects();

                            mLog.Debug("Package finalized successfully. TempManifestPath: {0}", TempManifestPath);

                            status = PackageStatus.Ready;
                        }
                        else
                        {
                            mLog.Debug("No objects to migrate in this package.");
                            status = PackageStatus.NotReady;
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Error($"Error occurred while finalizing package: {ex.Message}. Exception: {ex}");
                        ResetObjects();

                        LastError = $"Deployment schema file '{ex.Message}' not found";
                        status = PackageStatus.Error;
                    }
                }
                else if (isLastPackage)
                {
                    // Empty package - just cleanup
                    mLog.Debug("Last package is empty. Clearing temp folder.");
                    ClearTempFolder();
                    status = PackageStatus.Empty;
                }
            }

            return status;
        }

        public void StorageLookupListMapXml()
        {
            try
            {
                if (SPLookupLists.LookupList.Count > 0)
                {
                    XmlSerializer(Path.Combine(TempManifestPath, SPMHSConstant.LOOKUPLISTSMAP_XML_NAME), SPLookupLists);
                    SPLookupLists.LookupList.Clear();
                    if (SPLookupListCollection.Count > 0)
                    {
                        SPLookupListCollection.Clear();
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while storing LookupListMapXml.xml. Exception: {0}.", e.ToString());
                throw new Exception("LookupListMap");
            }
        }

        private void InitContainerInfo(WinAzure AzureInfo)
        {
            try
            {
                string containerId = Guid.NewGuid().ToString().ToLower().Replace("-", "");
                AzureInfo.AzureManifestContainerName = "m-" + containerId;
                AzureInfo.AzureSourceContainerName = "s-" + containerId;
                AzureInfo.AzureQueueReportContainerName = "q-" + containerId;

                var jobDir = Path.Combine(BackgroundSettings.GetInstance().ArchiveTemp, "tenantGroupId", "jobId");

                TempContentPath = SecurityUtils.SafeCombinePath(jobDir, AzureInfo.AzureSourceContainerName);
                TempManifestPath = SecurityUtils.SafeCombinePath(jobDir, AzureInfo.AzureManifestContainerName);

                CreateDirectory(TempContentPath);
                CreateDirectory(TempManifestPath);
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while initiating container information. Exception: {0}.", e.ToString());
            }
        }

        private void UpdateContainerInfo()
        {
            AzureInfo.AzureManifestContainerName = UpdateContainerName(AzureInfo.AzureManifestContainerName);
            AzureInfo.AzureSourceContainerName = UpdateContainerName(AzureInfo.AzureSourceContainerName);
            AzureInfo.AzureQueueReportContainerName = UpdateContainerName(AzureInfo.AzureQueueReportContainerName);

            var jobDir = Path.Combine(BackgroundSettings.GetInstance().ArchiveTemp, "tenantGroupId", "jobId");

            TempContentPath = SecurityUtils.SafeCombinePath(jobDir, AzureInfo.AzureSourceContainerName);
            TempManifestPath = SecurityUtils.SafeCombinePath(jobDir, AzureInfo.AzureManifestContainerName);

            CreateDirectory(TempContentPath);
            CreateDirectory(TempManifestPath);

            string UpdateContainerName(string containerName)
            {
                string containerId = Guid.NewGuid().ToString().ToLower().Replace("-", "");
                int index = containerName.LastIndexOf('-');
                return containerName.Substring(0, index) + "-" + containerId;
            }
        }
        protected void StorageUserGroupXML(string path)
        {
            //try
            //{
            //    UserGroupMap = new DeploymentUserGroupMap();
            //    if (mConfig.UserAndDomainMappingInfo != null && !String.IsNullOrEmpty(mConfig.UserAndDomainMappingInfo.DefaultUser))
            //    {
            //        try
            //        {
            //            IAveUser defaultUser = mAveList.ParentSite.SPSite.RootWeb.EnsureAvailableUser(mConfig.UserAndDomainMappingInfo.DefaultUser);
            //            if (defaultUser != null)
            //            {
            //                if (UserIdCache.Contains(defaultUser.ID))
            //                {
            //                    UserIdCache.Remove(defaultUser.ID);
            //                }
            //                DeploymentUser user = new DeploymentUser();
            //                user.Id = defaultUser.ID.ToString();
            //                if (!string.IsNullOrEmpty(defaultUser.LoginName))
            //                {
            //                    user.Login = defaultUser.LoginName;
            //                }
            //                user.Name = defaultUser.Name;
            //                user.Email = defaultUser.Email;
            //                user.IsDomainGroup = defaultUser.IsDomainGroup;
            //                user.IsSiteAdmin = defaultUser.IsSiteAdmin;
            //                byte[] byteArray = System.Text.Encoding.Default.GetBytes(Guid.NewGuid().ToString());
            //                user.SystemId = Convert.ToBase64String(byteArray);
            //                user.IsDeleted = false;
            //                UserGroupMap.Users.Add(user);
            //            }
            //        }
            //        catch (Exception e)
            //        {
            //            mLog.Warn("An error occurred while add default user to xml {0}", e.ToString());
            //        }
            //    }
            //    foreach (int id in UserIdCache)
            //    {
            //        try
            //        {
            //            List<AveSPMemberInfo> userANDGroupInfo = mAveSite.SPMembers.UserAndDomainMapping.EnumUserMapping().Select(info => info.Value as AveSPMemberInfo).Where(info => (info != null && info.NewId == id)).ToList<AveSPMemberInfo>();
            //            if (userANDGroupInfo.Count > 0)
            //            {
            //                AveSPMemberInfo memberInfo = userANDGroupInfo[0];
            //                if (memberInfo.IsUser)
            //                {
            //                    DeploymentUser user = new DeploymentUser();
            //                    user.Id = memberInfo.NewId.ToString();
            //                    if (!string.IsNullOrEmpty(memberInfo.AccountName))
            //                    {
            //                        user.Login = memberInfo.AccountName;
            //                    }
            //                    AveUserInfo userInfo = memberInfo.SourceInfo as AveUserInfo;
            //                    if (userInfo != null)
            //                    {
            //                        user.Name = userInfo.Title;
            //                        user.Email = userInfo.Email;
            //                        user.IsDomainGroup = userInfo.DomainGroup;
            //                        user.IsSiteAdmin = memberInfo.IsSiteAdmin;
            //                        byte[] byteArray = Encoding.Default.GetBytes(Guid.NewGuid().ToString());
            //                        user.SystemId = Convert.ToBase64String(byteArray);
            //                        if (userInfo.Deleted == 0)
            //                        {
            //                            user.IsDeleted = false;
            //                        }
            //                        else if (userInfo.Deleted == 1)
            //                        {
            //                            user.IsDeleted = true;
            //                        }
            //                    }
            //                    else
            //                    {
            //                        user.Name = user.Login;
            //                        byte[] byteArray = System.Text.Encoding.Default.GetBytes(Guid.NewGuid().ToString());
            //                        user.SystemId = Convert.ToBase64String(byteArray);
            //                    }
            //                    if (string.IsNullOrEmpty(user.Login) || string.IsNullOrEmpty(user.Name))
            //                    {
            //                        try
            //                        {
            //                            var defaultUser = ParentAveWeb.SPWeb.EnsureAvailableUser(ParentAveSite.BPOSUserAccountInfo.DefaultUser);
            //                            user.Login = defaultUser.LoginName;
            //                            user.Name = defaultUser.Name;
            //                            user.IsSiteAdmin = defaultUser.IsSiteAdmin;
            //                        }
            //                        catch (Exception ex)
            //                        {
            //                            mLog.Info($"Ensure default user failed :{ex.Message}");
            //                        }
            //                    }
            //                    if (!string.IsNullOrEmpty(user.Login) && !string.IsNullOrEmpty(user.Name) && user.Login.Equals(user.Name, StringComparison.OrdinalIgnoreCase))
            //                    {
            //                        user.Login = user.Name + "_PlaceHolder";
            //                    }
            //                    UserGroupMap.Users.Add(user);
            //                }
            //                else
            //                {
            //                    DeploymentGroup group = new DeploymentGroup();
            //                    group.Id = memberInfo.NewId.ToString();
            //                    AveGroupInfo groupInfo = memberInfo.SourceInfo as AveGroupInfo;
            //                    if (groupInfo != null)
            //                    {
            //                        group.Name = string.IsNullOrEmpty(memberInfo.AccountName) ? groupInfo.Title : memberInfo.AccountName;
            //                        group.Description = groupInfo.Description;
            //                        group.Owner = groupInfo.Owner.ToString();
            //                        group.OwnerIsUser = groupInfo.OwnerIsUser;
            //                        group.RequestToJoinLeaveEmailSetting = "";
            //                        group.OnlyAllowMembersViewMembership = groupInfo.OnlyAllowMembersViewMembership;
            //                    }
            //                    UserGroupMap.Groups.Add(group);
            //                }
            //            }
            //            else if (id == ParentAveWeb.AveWeb.CurrentUser.ID)
            //            {
            //                try
            //                {
            //                    IAveUser currentUser = ParentAveWeb.AveWeb.CurrentUser;
            //                    if (currentUser != null)
            //                    {
            //                        DeploymentUser user = new DeploymentUser();
            //                        user.Id = currentUser.ID.ToString();
            //                        if (!string.IsNullOrEmpty(currentUser.LoginName))
            //                        {
            //                            user.Login = currentUser.LoginName;
            //                        }
            //                        user.Name = currentUser.Name;
            //                        user.Email = currentUser.Email;
            //                        user.IsDomainGroup = currentUser.IsDomainGroup;
            //                        user.IsSiteAdmin = currentUser.IsSiteAdmin;
            //                        byte[] byteArray = System.Text.Encoding.Default.GetBytes(Guid.NewGuid().ToString());
            //                        user.SystemId = Convert.ToBase64String(byteArray);
            //                        user.IsDeleted = false;
            //                        UserGroupMap.Users.Add(user);
            //                    }
            //                }
            //                catch (Exception e)
            //                {
            //                    mLog.Warn($"An error occurred while add current user to xml {e}");
            //                }
            //            }
            //            else
            //            {
            //                mLog.Debug("Can not find user id is :{0}", id);
            //            }
            //        }
            //        catch (Exception e)
            //        {
            //            mLog.Warn("An error occurred while processing group xml.{0}", e.ToString());
            //        }

            //    }

            //    XmlSerializer(path + "\\" + SPMHSConstant.USETGROUP_XML_NAME, UserGroupMap);
            //}
            //catch (Exception e)
            //{
            //    mLog.Error("An error occurred while processing usergroup xml. Exception: {0}.", e.ToString());
            //    throw new Exception("UserGroup");
            //}
        }

        List<string> mBuildinRoleDefinitions = new List<string>() { "1073741825", "1073741826", "1073741827", "1073741828", "1073741829", "1073741830", "1073741832" };
        public SPGenericObject ProcessRoleDefinitionsXML(IAveWeb web)
        {
            if (mRoleDefinitionObject == null || !mRoleDefinitionObject.ParentWebId.Equals(web.ID.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                using (new AvePerformanceScope("HSWorker.ProcessRoleDefinitionsXML"))
                {
                    mRoleDefinitionObject = new SPGenericObject();
                    mRoleDefinitionObject.Id = Guid.NewGuid().ToString();
                    mRoleDefinitionObject.ParentId = web.ID.ToString();
                    mRoleDefinitionObject.ParentWebId = web.ID.ToString();
                    mRoleDefinitionObject.ParentWebUrl = web.ServerRelativeUrl.ToString();
                    mRoleDefinitionObject.ObjectType = SPObjectType.DeploymentRoles;

                    DeploymentRoles roles = new DeploymentRoles();
                    roles.Role = new List<DeploymentRole>();
                    foreach (var role in web.RoleDefinitions)
                    {
                        if (!mBuildinRoleDefinitions.Contains(role.ID.ToString()))
                        {
                            DeploymentRole roleInfo = new DeploymentRole();
                            roleInfo.Title = role.Name;
                            roleInfo.RoleId = role.ID.ToString();
                            roleInfo.PermMask = ((long)role.BasePermissions).ToString();
                            roleInfo.RoleOrder = role.Order.ToString();
                            roleInfo.Type = ((byte)role.Type).ToString();
                            roleInfo.Description = role.Description == null ? string.Empty : role.Description;
                            roleInfo.Hidden = role.Hidden;
                            roles.Role.Add(roleInfo);
                        }
                        else if (role.ID.ToString().Equals("1073741825"))
                        {
                            DeploymentRole roleInfo = new DeploymentRole();
                            roleInfo.Title = "$Resources:fpext,0x001C0040u";
                            roleInfo.RoleId = role.ID.ToString();
                            //roleInfo.PermMask = "206292717568";
                            roleInfo.PermMask = ((long)role.BasePermissions).ToString();
                            roleInfo.RoleOrder = "160";
                            roleInfo.Type = "1";
                            roleInfo.Description = "$Resources:fpext,0x001C0046u";
                            roleInfo.Hidden = true;
                            roles.Role.Add(roleInfo);
                        }
                        else if (role.ID.ToString().Equals("1073741826"))
                        {
                            DeploymentRole roleInfo = new DeploymentRole();
                            roleInfo.Title = "$Resources:fpext,0x001C003Fu";
                            roleInfo.RoleId = role.ID.ToString();
                            //roleInfo.PermMask = "756052856929";
                            roleInfo.PermMask = ((long)role.BasePermissions).ToString();
                            roleInfo.RoleOrder = "128";
                            roleInfo.Type = "2";
                            roleInfo.Description = "$Resources:fpext,0x001C0045u";
                            roleInfo.Hidden = false;
                            roles.Role.Add(roleInfo);
                        }
                        else if (role.ID.ToString().Equals("1073741827"))
                        {
                            DeploymentRole roleInfo = new DeploymentRole();
                            roleInfo.Title = "$Resources:fpext,0x001C003Du";
                            roleInfo.RoleId = role.ID.ToString();
                            //roleInfo.PermMask = "1856436900591";
                            roleInfo.PermMask = ((long)role.BasePermissions).ToString();
                            roleInfo.RoleOrder = "64";
                            roleInfo.Type = "3";
                            roleInfo.Description = "$Resources:fpext,0x001C0043u";
                            roleInfo.Hidden = false;
                            roles.Role.Add(roleInfo);
                        }
                        else if (role.ID.ToString().Equals("1073741828"))
                        {
                            DeploymentRole roleInfo = new DeploymentRole();
                            roleInfo.Title = "$Resources:fpext,0x001C003Cu";
                            roleInfo.RoleId = role.ID.ToString();
                            //roleInfo.PermMask = "1856438737919";
                            roleInfo.PermMask = ((long)role.BasePermissions).ToString();
                            roleInfo.RoleOrder = "32";
                            roleInfo.Type = "4";
                            roleInfo.Description = "$Resources:fpext,0x001C0042u";
                            roleInfo.Hidden = false;
                            roles.Role.Add(roleInfo);
                        }
                        else if (role.ID.ToString().Equals("1073741829"))
                        {
                            DeploymentRole roleInfo = new DeploymentRole();
                            roleInfo.Title = "$Resources:fpext,0x001C003Bu";
                            roleInfo.RoleId = role.ID.ToString();
                            //roleInfo.PermMask = "9223372036854775807";
                            roleInfo.PermMask = ((long)role.BasePermissions).ToString();
                            roleInfo.RoleOrder = "1";
                            roleInfo.Type = "5";
                            roleInfo.Description = "$Resources:fpext,0x001C0041u";
                            roleInfo.Hidden = false;
                            roles.Role.Add(roleInfo);
                        }
                        else if (role.ID.ToString().Equals("1073741830"))
                        {
                            DeploymentRole roleInfo = new DeploymentRole();
                            roleInfo.Title = "$Resources:fpext,0x001C003Eu";
                            roleInfo.RoleId = role.ID.ToString();
                            //roleInfo.PermMask = "1856436902639";
                            roleInfo.PermMask = ((long)role.BasePermissions).ToString();
                            roleInfo.RoleOrder = "48";
                            roleInfo.Type = "6";
                            roleInfo.Description = "$Resources:fpext,0x001C0044u";
                            roleInfo.Hidden = false;
                            roles.Role.Add(roleInfo);
                        }
                        else if (role.ID.ToString().Equals("1073741924"))
                        {
                            DeploymentRole roleInfo = new DeploymentRole();
                            roleInfo.Title = "$Resources:xlsrv,RoleNameViewer;";
                            roleInfo.RoleId = role.ID.ToString();
                            roleInfo.PermMask = ((long)role.BasePermissions).ToString();
                            roleInfo.RoleOrder = role.Order.ToString();
                            roleInfo.Type = ((byte)role.Type).ToString();
                            roleInfo.Description = role.Description == null ? string.Empty : role.Description;
                            roleInfo.Hidden = role.Hidden;
                            roles.Role.Add(roleInfo);
                        }
                        else if (role.ID.ToString().Equals("1073741832"))
                        {
                            DeploymentRole roleInfo = new DeploymentRole();
                            roleInfo.Title = "$Resources:core,RestrictedReaderRole;";
                            roleInfo.RoleId = role.ID.ToString();
                            roleInfo.PermMask = ((long)role.BasePermissions).ToString();
                            roleInfo.RoleOrder = "144";
                            roleInfo.Type = ((byte)role.Type).ToString();
                            roleInfo.Description = "$Resources:core,RestrictedReaderRoleDesc;";
                            roleInfo.Hidden = true;
                            roles.Role.Add(roleInfo);
                        }
                    }

                    mRoleDefinitionObject.Item = roles;
                    //web.RoleDefinitionChanged = false;
                }
            }
            return mRoleDefinitionObject;
        }
        private void XmlSerializer(string xmlPath, Type type, System.Object obj)
        {
            XmlSerializer serializer = new XmlSerializer(type);
            using (XmlWriter sw = XmlWriter.Create(xmlPath, new XmlWriterSettings { Encoding = Encoding.UTF8, CheckCharacters = false }))
            {
                serializer.Serialize(sw, obj);
            }
        }
        private void XmlSerializer(string xmlPath, System.Object obj)
        {
            XmlSerializer(xmlPath, obj.GetType(), obj);
        }
        private void CreateDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }
        private void DeleteDirectory(string directoryPath)
        {
            try
            {
                if (Directory.Exists(directoryPath))
                {
                    Directory.Delete(directoryPath, true);
                }
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while deleting folder {0}. Exception: {1}.", directoryPath, e.ToString());
            }
        }
        #endregion
    }
}
