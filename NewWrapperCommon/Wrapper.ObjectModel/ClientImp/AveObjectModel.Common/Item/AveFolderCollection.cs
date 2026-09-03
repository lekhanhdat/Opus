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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using AvePoint.Wrapper.Restore;
using System.Diagnostics.CodeAnalysis;
namespace AvePoint.ObjectModel.Common
{
    class AveFolderCollection : AveAbstractLazyCollection<IAveFolder>, IAveFolderCollection, IDisposable
    {
        private AveWeb mWeb;
        private AveList mList;
        private AveFolder mParentFolder;
        private IAveRequest mRequest;
        private AveLogger mLogger = AveLogger.GetInstance(typeof(AveFolderCollection));
        private IReport mReport;
        public IReport Report
        {
            get
            {
                if (mReport == null)
                {
                    mReport = new AveWrapperReport();
                }
                return mReport;
            }
        }
        public void SetReport(IReport report)
        {
            mReport = report;
        }

        public AveFolderCollection(IAveRequest request, IAveWeb web, IAveList list, AveFolder parentFolder)
        {
            mWeb = web as AveWeb;
            mList = list as AveList;
            mParentFolder = parentFolder;
            mRequest = request;
        }

        protected override void InitCollection()
        {
            if (!IsCollectionInitialized)
            {
                lock (lockObject)
                {
                    //ensure flag in lock again
                    if (!IsCollectionInitialized)
                    {
                        var folderProperties = mRequest.GetFolders(mWeb.ServerRelativeUrl, mList == null ? null : mList.Title, mList != null ? mList.ID : Guid.Empty, mParentFolder.ServerRelativeUrl);
                        base.DataCache.AddPropertyies(folderProperties);
                        mListData = new List<IAveFolder>();
                        InitFolderCollection();
                        IsCollectionInitialized = true;
                    }
                }
            }
        }

        /// <summary>
        /// used in lockObject
        /// </summary>
        private void InitFolderCollection()
        {
            List<Dictionary<string, object>> folderPropertiesList = base.DataCache.GetProperty<List<Dictionary<string, object>>>(AveObjectModelConstant.ChildrenProperties);
            foreach (Dictionary<string, object> folderProperties in folderPropertiesList)
            {
                AveFolder folder = new AveFolder(mRequest, mWeb, mList, mParentFolder, folderProperties);
                mListData.Add(folder);
            }
        }

        #region IAveFolderCollection Member

        public new IAveFolder this[int index]
        {
            get
            {
                return ListData[index];
            }
        }
        public IAveFolder this[string name]
        {
            get
            {
                return GetByName(name);
            }
        }

        public IAveFolder Add(string url)
        {
            Dictionary<string, object> folderProperties = mRequest.AddFolder(mWeb.ServerRelativeUrl, this.mList == null ? Guid.Empty : this.mList.ID, mParentFolder.ServerRelativeUrl, url);
            AveFolder folder = new AveFolder(mRequest, mWeb, mList, mParentFolder, folderProperties);
            AddToCache(folder);
            return folder;
        }
        public IAveFolder GetByName(string folderName)
        {
            IAveFolder resultFolder = ListData.Find(
                    delegate (IAveFolder folder)
                    {
                        return folder.Name.Equals(folderName, StringComparison.OrdinalIgnoreCase);
                    });
            if (resultFolder == null)
            {
                throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Office365_Common_NotFoundFolder, folderName);
            }
            return resultFolder;
        }

        #endregion



        public IAveWeb Web
        {
            get { throw new NotImplementedException(); }
        }

        //public System.Collections.IEnumerator GetEnumerator()
        //{
        //    throw new NotImplementedException();
        //}
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "docset_LastRefresh")]

        public AveRestoreResult RestoreFolder(AveFolderInfo info, Dictionary<string, object> allDocData, Dictionary<string, object> allUserData)
        {
            Dictionary<string, object> docData = AveList.AssembleBaseItemInfo(info, this.mList);
            docData["AveWebObject"] = this.mWeb;
            if (mList != null)
            {
                docData["ListTemplate"] = (int)mList.BaseTemplate;
                docData["ListEnableModeration"] = mList.EnableModeration;
                docData["ListEnableVersioning"] = mList.EnableVersioning;
                mList.SetTaxonomyField(info, -1, true, info.FieldsInfo.TermIdMapping, info.FieldsInfo.MergedTermIdMapping);
            }
            if (allUserData.ContainsKey("#tp_GUID"))
            {
                docData["GUID"] = allUserData["#tp_GUID"];
            }
            Dictionary<String, object> fields = AveList.ConvertFieldValuesToString(info.FieldsInfo.Fields);
            fields.Add("NeedSetNullFields", info.NeedSetNullFields);
            if (!fields.ContainsKey("Modified"))
            {
                fields.Add("Modified", info.DTimeLastModified);
            }
            if (fields.ContainsKey("ContentType") && fields["ContentType"] != null && fields["ContentType"].ToString().StartsWith("0x0120D520", StringComparison.OrdinalIgnoreCase))
            {
                docData["docset_LastRefresh"] = DateTime.UtcNow.ToString();
            }
            if (info.FieldsInfo.Fields.ContainsKey("TaxonomyFields"))
            {
                fields.Add("TaxonomyFields", info.FieldsInfo.Fields["TaxonomyFields"]);
            }
            if (allDocData.ContainsKey("ListId") && allDocData["ListId"] != null)
            {
                var oldId = new Guid(allDocData["ListId"] as string);
                Guid value = Guid.Empty;
                if (info.MappingManager.SiteMappingManager.GetValueFromListIdMapping(oldId, out value))
                {
                    allDocData["ListId"] = value.ToString();
                }
                else
                {
                    allDocData["ListId"] = oldId.ToString();
                }
            }
            if (allDocData.ContainsKey("Properties"))
            {
                docData["Properties"] = allDocData["Properties"];
            }
            if (allDocData.ContainsKey("MetaInfo"))
            {
                docData["MetaInfo"] = allDocData["MetaInfo"];
            }
            if (allDocData.ContainsKey("vti_contenttypeorder"))
            {
                docData["vti_contenttypeorder"] = allDocData["vti_contenttypeorder"];
            }
            Dictionary<string, object> restoreResult = mRequest.RestoreFolder(docData, fields);
            info.IsNewCreated = restoreResult.ContainsKey("IsNewCreated") ? (bool)restoreResult["IsNewCreated"] : false;
            info.AveItem.Folder = new AveFolder(mRequest, mWeb, mList, null, restoreResult);
            if (info.RestoreOption == AveRestoreMode.Default && !info.IsNewCreated && (this.mList == null || this.mList.BaseTemplate != AveListTemplateType.DiscussionBoard))
            {
                info.RestoringItem.NeedSkipped = true;
                info.RestoringItem.ConflictType = ConflictType.Document;
                throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
            }
            IAveFolder folder = null;
            try
            {
                folder = GetByName(restoreResult["Name"].ToString());
            }
            catch (Exception e)
            {
                mLogger.Debug("Folder:{0} not find,add it.Error Message:{1}", restoreResult["Name"], e.ToString());
            }
            if (folder != null)
            {
                ListData.Remove(folder);
            }
            ListData.Add(info.AveItem.Folder);
            if (info.AveItem.Folder != null)
            {
                info.AveItem.ListItem = info.AveItem.Folder.Item;
            }
            if (info.AveItem.ListItem != null)
            {
                info.RowId = info.AveItem.ListItem.ID;
                info.Version = info.AveItem.ListItem.FieldValues.ContainsKey("_UIVersion") ? (int)info.AveItem.Folder.Item.FieldValues["_UIVersion"] : 0;
                PostProcessFolderMetaInfo(allDocData, info);
            }
            //For o365 oneNote
            if (mList != null && mList.IsSiteAssetsLibrary && allDocData.ContainsKey("Id"))
            {
                info.MappingManager.SiteMappingManager.AddsiteAssetsFolderUniqueIdMapping((Guid)allDocData["Id"], info.AveItem.Folder.UniqueId);
            }
            if (restoreResult.ContainsKey("ListVersionSettingChanged"))
            {
                info.SettingInfo.LIST_SETTING_CHANGED = true;
            }
            info.IsNewCreated = restoreResult.ContainsKey("IsNewCreated") ? Convert.ToBoolean(restoreResult["IsNewCreated"]) : info.IsNewCreated;
            return AveRestoreResult.Normal;
        }


        private void PostProcessFolderMetaInfo(Dictionary<string, object> allDocData, AveFolderInfo info)
        {
            //前面处理capture version 里的column value，改存到alldoc 集合中
            //if (!allDocData.ContainsKey("MetaInfo") || !(allDocData["MetaInfo"] is byte[]))
            //{
            //    return;
            //}
            //string metaInfoString = AveCompressedUtility.GetTCompressedString((byte[])allDocData["MetaInfo"]);
            //Dictionary<string, string> metaInfoDic = AveCompressedUtility.GetMetaInfoDictionary(metaInfoString);
            if (allDocData.ContainsKey("snapshots"))
            {
                info.MappingManager.ListMappingManager.DocumentSetGuidMetaInfoMapping[info.AveItem.ListItem.UniqueId] = allDocData["snapshots"].ToString();
            }
        }

        public IAveDocumentSet CreateDocumentSet(string name, IAveContentTypeId contentTypeId, Hashtable properties)
        {
            //10模拟没实现，做下判断，避免返回null
            IAveDocumentSet documentSet = null;
            var request = mRequest ;
            if (request != null)
            {
                Dictionary<string, object> folderInfo = request.AddDocumentSet(mWeb.ServerRelativeUrl, mList.Title, mList.ID, mParentFolder.ServerRelativeUrl, name, contentTypeId);
                var folder = new AveFolder(mRequest, mWeb, mList, mParentFolder, folderInfo);
                AddToCache(folder);
                documentSet = new AveDocumentSet(mRequest, folder);
            }
            return documentSet;
        }

        public IAveDocumentSet CreateDocumentSet(string name, Hashtable properties)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {

        }
    }
}
