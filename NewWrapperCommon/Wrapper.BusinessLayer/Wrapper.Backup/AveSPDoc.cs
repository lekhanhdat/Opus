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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using AvePoint.Wrapper.Common;
using AvePoint.Common;
using AvePoint.Wrapper.Core.SPBackup;
using AvePoint.Wrapper.Core.SPBackupDto;
using System.Xml;
using AvePoint.GCommon;
using System.Reflection;

namespace AvePoint.Wrapper.Backup
{
    public class AveSPDoc : AvePoint.Wrapper.Backup.IAveSPDoc, ISPFileExport
    {
        private AveSPFolder mParentFolder;
        private IAveBackupStream mSender;
        private AveSPItem mAveSPItem;

        private DateTime mBiggestVersionModified;
        protected static AveLogger log = AveLogger.GetInstance(typeof(AveSPDoc));
        public AveSPDoc(AveSPFolder aveFolder, Guid id, int rowId, int version, string serverRelativeUrl = null, int level = 0)
            : this(aveFolder, id, rowId, version, serverRelativeUrl, level, DateTime.MinValue)
        {
        }

        public AveSPDoc(AveSPFolder aveFolder, Guid id, int rowId, int version, string serverRelativeUrl, int level, DateTime currentVersionModified)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPDoc.Constructor"))
            {
                mParentFolder = aveFolder;
                mSender = aveFolder.Sender;
                mAveParentSite = aveFolder.ParentSite;
                mBiggestVersionModified = currentVersionModified;
                mAveSPItem = new AveSPItem(id, rowId, version, level, serverRelativeUrl, AveItemType.Document, mParentFolder.Id,
                    aveFolder.AveList.ParentWeb.ParentSite.SPSite.ID, aveFolder.AveList, aveFolder.Sender, aveFolder.QueryService, aveFolder.AveList.Fields, aveFolder.AveList.SolutionStatus, aveFolder.SPFolder);
                //mAveSPItem.ParentId = mParentFolder.Id;
            }
        }

        public AveSPDoc(ISPListExport backupList, IAveFile file, int version)
            : this(aveFolder: new AveSPFolder((AveSPList)backupList, file.ParentFolder), id: file.UniqueId, rowId: file.Item != null ? file.Item.ID : 0, version: version, serverRelativeUrl: file.ServerRelativeUrl, level: (int)file.Level)
        {

        }

        public AveSPFolder ParentFolder
        {
            get { return mParentFolder; }
        }

        public string ExportDocInfo()
        {
            string xml = string.Empty;
            Dictionary<string, object> docInfo = mAveSPItem.GetDocInfo();
            mAveSPItem.CheckPageView(docInfo, mParentFolder.AveList.ViewCache);
            if (docInfo != null)
            {
                xml = AveConvert.ConvertAveObjToAveXml(AveMetadataType.DocProperty.ToString(), docInfo);
            }
            return xml;
        }

        public void ExportRbsId(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPDoc.ExportRbsId"))
            {
                mAveSPItem.ExportRbsId(output);
            }
        }

        public AveSPItem AveSPItem
        {
            get
            {
                return mAveSPItem;
            }
        }

        private AveSPSite mAveParentSite;

        public AveSPSite ParentSite
        {
            get { return mAveParentSite; }
        }

        public AveSPWeb AveSPWeb
        {
            get
            {
                return mParentFolder.AveList.ParentWeb;
            }
        }

        public bool HasContent
        {
            get { return mAveSPItem.HasStream; }
        }

        public string Url
        {
            get
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPDoc.Url"))
                {
                    //  s...d/a.aspx
                    if (String.IsNullOrEmpty(mAveSPItem.ScopeUrl))
                    {
                        return string.Empty;
                    }
                    string fileUrl = mAveSPItem.ScopeUrl.TrimStart('/').Substring(AveSPWeb.SPWeb.ServerRelativeUrl.TrimStart('/').Length).TrimStart('/');
                    return AveSPWeb.SPWeb.Url.TrimEnd('/') + "/" + fileUrl;
                }
            }
        }

        #region IAveSPDoc Members

        IAveSPItem IAveSPDoc.AveSPItem
        {
            get { return mAveSPItem; }
        }

        IAveSPWeb IAveSPDoc.AveSPWeb
        {
            get { return mParentFolder.AveList.ParentWeb; }
        }

        IAveSPFolder IAveSPDoc.ParentFolder
        {
            get { return mParentFolder; }
        }

        public IAveSPSite AveSPSite
        {
            get { return mAveParentSite; }
        }

        public void ExportDocInfo(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPDoc.ExportDocInfo"))
            {
                var docInfo = GetDocInfo();
                if (docInfo != null)
                {
                    output.WriteMetadata(AveMetadataType.DocProperty, docInfo);
                }
            }
        }

        /// <summary>
        /// Get Doc Info
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, object> GetDocInfo()
        {
            Dictionary<string, object> docInfo = mAveSPItem.GetDocInfo();
            mAveSPItem.CheckPageView(docInfo, mParentFolder.AveList.ViewCache);
            if (docInfo != null)
            {
                if (mBiggestVersionModified != DateTime.MinValue)
                {
                    docInfo["BiggestVersionModified"] = mBiggestVersionModified;
                }
            }
            return docInfo;
        }


        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "StorgeInfo is a part of common method name")]
        public void ExportStorgeInfo(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPDoc.ExportStorgeInfo"))
            {
                mAveSPItem.ExportStorageInfo(output);
            }
        }

        public void ExportWebParts(IAveBackupStream output, bool includeUsers = true, bool onlyUnAvaiableUser = false)
        {
            ExportWebParts(output, null, includeUsers, onlyUnAvaiableUser);
        }

        public void ExportWebParts(IAveBackupStream output, AveBackupOption backupOption, bool includeUsers = true, bool onlyUnAvaiableUser = false)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("Backup.AveSPDoc.ExportWebParts"))
            {
                var webParts = GetWebParts(includeUsers);

                if (backupOption != null && (backupOption.BackupRelatedTermSets || backupOption.BackupRelatedTermsOnly) && webParts != null)
                {
                    ExportWebPartMMSData(output, backupOption, webParts);
                }

                if (includeUsers)
                {
                    if (onlyUnAvaiableUser)
                    {
                        this.AveSPItem.ExportUnavailableUserInCache(output);
                    }
                    else
                    {
                        this.AveSPItem.ExportUserCache(output);
                    }
                }

                if (webParts != null)
                {
                    output.WriteMetadata(AveMetadataType.DocWebPart, webParts);
                }
            }
        }

        /// <summary>
        /// 备份WebPart Managed MetaData Service数据
        /// </summary>
        /// <param name="output"></param>
        /// <param name="backupOption"></param>
        /// <param name="webParts"></param>
        private void ExportWebPartMMSData(IAveBackupStream output, AveBackupOption backupOption, List<AveWebPartBaseInfo> webParts)
        {
            try
            {
                List<AveTermStoreInfo> metadataInfolist = new List<AveTermStoreInfo>();
                //处理ContentQueryWebPart 07、10
                var webPartMMSData = webParts.FindAll(info => info.ExtensionProperties != null
                   && info.ExtensionProperties.ContainsKey("WebPartMMSData"));
                if (webPartMMSData != null && webPartMMSData.Count > 0)
                {
                    List<AveTaxFieldInfo> taxFieldInfos = new List<AveTaxFieldInfo>();
                    foreach (var info in webPartMMSData)
                    {
                        CacheContentQueryWebPartTaxFieldInfo(taxFieldInfos, info);
                    }
                    if (taxFieldInfos.Count > 0)
                    {
                        var metadataInfos = mAveSPItem.mItem.GetRelatedMetadataInfo(taxFieldInfos, backupOption);
                        metadataInfolist.AddRange(metadataInfos);
                        //if (metadataInfos != null && metadataInfos.Count > 0)
                        //{
                        //    output.WriteMetadata(AveMetadataType.MetadataService, metadataInfos);
                        //}
                    }
                }
                //处理termPropertyWebPart 13、16
                var termPropertyWebPartMMSData = webParts.FindAll(info => info.ExtensionProperties != null
                   && info.ExtensionProperties.ContainsKey("TermPropertyWebPartMMSData"));
                if (termPropertyWebPartMMSData != null && termPropertyWebPartMMSData.Count > 0)
                {
                    List<string> termPropertyWebPartInfos = new List<string>();
                    foreach (var info in termPropertyWebPartMMSData)
                    {
                        string termInfo = info.ExtensionProperties["TermPropertyWebPartMMSData"];
                        termPropertyWebPartInfos.Add(termInfo);
                    }
                    if (termPropertyWebPartInfos.Count > 0)
                    {
                        var metadataInfos = mAveSPItem.mItem.GetTermPropertyWebPartMetadataInfo(termPropertyWebPartInfos, backupOption);
                        metadataInfolist.AddRange(metadataInfos);
                    }
                }
                //由于此两种webpart所在版本不同，此处处理考虑升级db的可能
                if (metadataInfolist != null && metadataInfolist.Count > 0)
                {
                    output.WriteMetadata(AveMetadataType.MetadataService, metadataInfolist);
                }
            }
            catch (Exception ex)
            {
                log.Warn("An error occurred while backing up metadata associated with web part Error:{0},server relative url: {1}", ex, this.AveSPItem.ServerRelativeUrl);
            }
        }

        /// <summary>
        /// 通过Filter Xml信息，获取FilterField，将Term数据缓存到taxFieldInfos中
        /// </summary>
        /// <param name="taxFieldInfos"></param>
        /// <param name="info"></param>
        private void CacheContentQueryWebPartTaxFieldInfo(List<AveTaxFieldInfo> taxFieldInfos, AveWebPartBaseInfo info)
        {
            string str = null;
            try
            {
                str = info.ExtensionProperties["WebPartMMSData"];

                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(str);
                var rootElmt = xDoc.DocumentElement;


                if (rootElmt.ChildNodes.Count > 0)
                {
                    List<AveTaxFieldInfo> aveTaxFieldInfos = GetQueryWebPartTaxFieldInfo(rootElmt);

                    if (aveTaxFieldInfos != null && aveTaxFieldInfos.Count > 0)
                    {
                        taxFieldInfos.AddRange(aveTaxFieldInfos);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Warn("An error occurred while backing up metadata associated with web part Error:{0},web part managed metadata filters info :{1},page url{2}  ", ex, str, this.AveSPItem.ServerRelativeUrl);
            }
            finally
            {
                info.ExtensionProperties["WebPartMMSData"] = null;
            }
        }

        /// <summary>
        ///  获取FilterValue相关的Term数据
        /// </summary>
        /// <param name="rootElmt"></param>
        /// <param name="collection"></param>
        /// <param name="isListFields"></param>
        /// <returns></returns>
        private List<AveTaxFieldInfo> GetQueryWebPartTaxFieldInfo(XmlElement xElement)
        {
            IAveWeb web = null;
            bool isRootWeb = false;
            try
            {
                string webUrl = xElement.GetAttribute("WebUrl");
                string listName = xElement.GetAttribute("ListName");
                string listGuid = xElement.GetAttribute("ListGuid");

                //SiteCollection Level
                IAveFieldCollection collection = null;
                var isListFields = false;
                if (string.IsNullOrEmpty(webUrl) || webUrl.Equals("~siteCollection", StringComparison.OrdinalIgnoreCase))
                {
                    web = this.AveSPSite.SPSite.RootWeb;
                    isRootWeb = true;
                }
                //Sub Site以这个~sitecollection开头,同时ContentByQueryWebPart支持ServerRelativeUrl
                else
                {
                    var url = webUrl;
                    if (webUrl.StartsWith("~siteCollection", StringComparison.OrdinalIgnoreCase))
                    {
                        url = webUrl.Substring("~siteCollection".Length).Trim('/');
                    }
                    web = AveSPSite.SPSite.OpenWeb(url);
                }


                if (!string.IsNullOrEmpty(listGuid))
                //List级别的Filter其值为FieldName
                {
                    var list = web.Lists[new Guid(listGuid)];
                    collection = list.Fields;
                    isListFields = true;
                }
                else if (!string.IsNullOrEmpty(listName))
                {
                    var list = web.Lists[listName];
                    collection = list.Fields;
                    isListFields = true;
                }
                else
                {
                    collection = web.AvailableFields;
                }


                return GetContentQueryWebPartTaxFieldInfo(xElement, collection, isListFields);

            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, "An error occurred while parsing ContentByQueryWebPart specified web and list. Error:{0},filter info {1},page url{2}", ex, xElement.OuterXml, this.AveSPItem.ServerRelativeUrl);
            }
            finally
            {
                if (web != null && !isRootWeb)
                {
                    web.Dispose();
                }
            }
            return null;
        }

       /// <summary>
        /// 通过FilterField以及WebPart属性，拼装TaxInfo
       /// </summary>
       /// <param name="rootElmt"></param>
       /// <param name="collection"></param>
       /// <param name="isListFields"></param>
       /// <returns></returns>
        private List<AveTaxFieldInfo> GetContentQueryWebPartTaxFieldInfo(XmlElement rootElmt, IAveFieldCollection collection, bool isListFields)
        {
            List<AveTaxFieldInfo> infos = new List<AveTaxFieldInfo>();
            try
            {
                foreach (XmlElement elmt in rootElmt.ChildElements())
                {
                    string filterField = elmt.GetAttribute("FilterField");
                    //根据DisplayValue备份。
                    string filterDisplayValue = elmt.GetAttribute("FilterDisplayValue");
                    //string FilterValue = elmt.GetAttribute("FilterValue");
                    if (!string.IsNullOrEmpty(filterDisplayValue))
                    {
                        try
                        {
                            List<Guid> guids = null;
                            //value完全合法才会返回True。
                            if (TryGetTermId(filterDisplayValue, out guids))
                            {
                                //当FilterField值为ListField时，为Field Name。当为WebField时，值为Field Guid
                                var field = isListFields ? collection.GetField(filterField) : collection[new Guid(filterField)];

                                if (field is IAveTaxonomyField)
                                {
                                    var taxField = field as IAveTaxonomyField;
                                    AveTaxFieldInfo taxInfo = new AveTaxFieldInfo();
                                    taxInfo.IsKeywordsColumn = taxField.IsKeyword;
                                    taxInfo.SspId = taxField.SspId;
                                    taxInfo.TermSetId = taxField.TermSetId;
                                    taxInfo.TermIds.AddRange(guids);
                                    infos.Add(taxInfo);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Log(AveLogLevel.WARN, "An error occurred while parsing one of ContentByQueryWebPart field info. Error:{0},is list field {1},FilterField:{2},value{3},page url{4}", ex, isListFields, filterField, filterDisplayValue,this.AveSPItem.ServerRelativeUrl);
                        }
                    }
                   
                }
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, "An error occurred while getting taxonomy filter value of contentByQueryWebPart Error:{0},filter value info : {1},page url {2}", ex, rootElmt.OuterXml, this.AveSPItem.ServerRelativeUrl);
            }
            return infos;
        }

        private bool TryGetTermId(string value, out List<Guid> termIds)
        {
            termIds = null;
            bool success = false;
            try
            {
                string[] values = value.Split(';');
                if (values != null && values.Length > 0)
                {
                    foreach (var item in values)
                    {
                        if (item.Contains("|"))
                        {
                            string[] temp = item.Split('|');
                            if (temp.Length == 2)
                            {
                                if (termIds == null)
                                {
                                    termIds = new List<Guid>();
                                }
                                termIds.Add(new Guid(temp[1]));
                                success = true;
                            }
                            else
                            {
                                log.Warn("ContentByQuery filter value is illegal.filter Value:{0},page url{1}", item, this.AveSPItem.ServerRelativeUrl);
                                success = false;
                                break;
                            }
                        }
                        else
                        {
                            log.Log(AveLogLevel.WARN, "ContentByQuery filter value is illegal.filter Value:{0},page url{1}", item, this.AveSPItem.ServerRelativeUrl);
                            success = false;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Warn("An error occurred while parsing ContentByQueryWebPart term id. Error:{0},value {1},page url{2}", ex, value,this.AveSPItem.ServerRelativeUrl);
                success = false;
            }
            return success;
        }

        internal List<AveWebPartBaseInfo> GetWebParts(bool includeUsers)
        {
            if (includeUsers)
            {
                this.AveSPItem.CacheUserFromWebParts();
            }
            var manager = new AveSPLiminitedWebPartManager(mAveSPItem);
            return manager.GetWebParts();
        }

        public void ExportAlerts(IAveBackupStream output, bool includeUsers = true, bool onlyUnAvaiableUser = false)
        {
            if (includeUsers)
            {
                this.AveSPItem.CacheUserFromAlert(this);
                if (onlyUnAvaiableUser)
                {
                    this.AveSPItem.ExportUnavailableUserInCache(output);
                }
                else
                {
                    this.AveSPItem.ExportUserCache(output);
                }
            }
            AveSPAlert alerts = AveSPAlert.CreateInstance(this);
            alerts.Export(output);
        }

        public void ExportSocialTags(IAveBackupStream output)
        {
            if (this.AveSPSite.ObjectModelFactory.ContextKind.IsServerMode10Upper())
            {
                if (AveEnv.IsMoss)
                {
                    var tag = new AveSPSocialTag(this.Url, this.ParentSite);
                    tag.Export(output);
                }
            }
        }

        public void ExportSocialComments(IAveBackupStream output)
        {
            if (this.AveSPSite.ObjectModelFactory.ContextKind.IsServerMode10Upper())
            {
                if (AveEnv.IsMoss)
                {
                    var comment = new AveSPSocialComment(this.Url, this.ParentSite);
                    comment.Export(output);
                }
            }
        }

        public void ExportContent(IAveBackupStream output, bool forceBackup = false)
        {
            ExportContent(output, null, forceBackup);
        }

        /// <summary>
        /// 备份文件content
        /// </summary>
        /// <param name="output"></param>
        /// <param name="streamConvertor">一些content需要convert 才能变成真实数据，目前只有Online IRM</param>
        /// <param name="forceBackup">如果目的端是O365的话，需要在源端备份Ghost Page的content，否则还原到目的端没有content</param>
        public void ExportContent(IAveBackupStream output, IStreamConvertor streamConvertor, bool forceBackup = false)
        {
            if (this.HasContent || forceBackup)
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPDoc.ExportContent"))
                {
                    mAveSPItem.ExportContent(output,streamConvertor);
                }
            }
            else
            {
                output.FlushMetadata(0);
            }
        }

        public void ExportToExcel()
        {
            if (mParentFolder.AveList.NeedExportExcel && mParentFolder.AveList.SPList != null && !mParentFolder.AveList.SPList.Hidden)
            {
                if (!string.IsNullOrEmpty(this.Url))
                {
                    mAveSPItem.ExportDataToExcel(this.Url.Substring(this.Url.IndexOf(this.AveSPWeb.ScopeString.ToString(), StringComparison.OrdinalIgnoreCase)));
                }
            }
        }

        public List<AveAlertInfo> GetAlerts()
        {
            AveSPAlert alerts = AveSPAlert.CreateInstance(this);
            return alerts.GetAlertInfos();
        }

        public List<AveSocialTagInfo> GetSocialTags()
        {
            if (this.AveSPSite.ObjectModelFactory.ContextKind.IsServerMode10Upper())
            {
                if (AveEnv.IsMoss)
                {
                    var tag = new AveSPSocialTag(this.Url, this.ParentSite);
                    return tag.GetSocialTags();
                }
            }
            return null;
        }

        public List<AveSocialCommentInfo> GetSocialComments()
        {
            if (this.AveSPSite.ObjectModelFactory.ContextKind.IsServerMode10Upper())
            {
                if (AveEnv.IsMoss)
                {
                    var comment = new AveSPSocialComment(this.Url, this.ParentSite);
                    return comment.GetSocialComments();
                }
            }
            return null;
        }

        #endregion

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Export Metadata for document
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="backupOption"></param>
        public void ExportMetadata(IAveBackupStream stream, SPItemMetadataBackupOption backupOption)
        {
            var metadata = new SPDocumentMetadataDto();

            #region backup ItemMetadata
            if (this.mAveSPItem.RowId > 0)
            {
                var userData = mAveSPItem.GetUserDataInfoWithDependence(backupOption);
                metadata.UserDataInfo = userData.Item1;
                metadata.MetadataInfo = userData.Item2;

                metadata.DocDataJunction = mAveSPItem.GetUserDataJunctionCache(true);
                //output.WriteMetadata(AveMetadataType.DocDataJunction, dataCache);

                if (backupOption != null && backupOption.BackupItemTPGUIDofLookupValue)
                {
                    metadata.ItemTPGUIDofLookupValue = mAveSPItem.GetLookupFieldGuidValue();
                }
            }
            #endregion

            if (backupOption.IncludeAllUIVersions)
            {
                metadata.ItemUIVersionNums = this.mAveSPItem.GetDocVersions();
            }

            var storageInfo = mAveSPItem.GetAllStorageInfo();
            metadata.StorageInfo = storageInfo.Item1;
            metadata.StorageInfo13 = storageInfo.Item2;

            metadata.WebParts = GetWebParts(true);

            metadata.DocInfo_Old = GetDocInfo();
            if (backupOption != null)
            {
                if (backupOption.IncludeUser)
                {
                    metadata.UserCache = mAveSPItem.GetUserCache(false);
                }
                if (backupOption.IncludeGroup)
                {
                    metadata.GroupCache = mAveSPItem.GetGroupCache();
                }
            }

            stream.WriteMetadata(AveMetadataType.ItemMetadataDto, metadata);
        }

        /// <summary>
        /// Export Role Assignments
        /// </summary>
        /// <param name="stream"></param>
        public void ExportRoleAssignments(IAveBackupStream stream)
        {
            ExportRoleAssignments(stream, new SPRoleAssignmentsBakupOption()
                {
                    IncludeUsers = true,
                    IncludeGroups = true,
                    IncludeInheritedRoleAssignments = false,
                });
        }

        public void ExportRoleAssignments(IAveBackupStream stream, SPRoleAssignmentsBakupOption backupOption)
        {
            if (backupOption == null)
            {
                throw new ArgumentNullException("backupOption");
            }
            mAveSPItem.ExportRoleAssignments(stream, backupOption.IncludeUsers, backupOption.IncludeGroups);
        }

        public void ExportContent(IAveBackupStream stream)
        {
            ExportContent(stream, false);
        }

        public void ExportAlerts(IAveBackupStream stream)
        {
            var alert = AveSPAlert.CreateInstance(this);
            var alertsDto = alert.GetAlertsDto();

            if (alertsDto != null)
            {
                stream.WriteMetadata(AveMetadataType.AlertsDto, alertsDto);
            }
        }

        public void ExportSocialInfos(IAveBackupStream stream)
        {
            mAveSPItem.ExportSocialInfos(stream, Url);
        }

        public void ExportSPComments(IAveBackupStream stream)
        {
            var storage = this.ParentSite.ObjectModelFactory.CreateSPCommentStorage(this.ParentSite.SPSite);
            if (storage != null)
            {
                var file = mAveSPItem.mItem.GetFile();
                if (file.Exists && file.Item != null && file.Name.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
                {
                    var comments = storage.GetComments(file.Item);
                    stream.WriteMetadata(AveMetadataType.SPComments, comments);
                }
            }
        }
    }
}