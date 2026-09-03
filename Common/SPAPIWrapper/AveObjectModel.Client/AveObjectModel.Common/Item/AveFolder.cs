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
using System.Text;
using AvePoint.Wrapper.Common;
using System.Collections;
using System.Xml;

namespace AvePoint.ObjectModel.Common
{
    class AveFolder : AveClientObject, IAveFolder
    {
        private IAveRequest mRequest;
        private AveList mParentList;
        private List<AveHiddenFileInfo> mHiddenFileInfoList;
        private AveDocumentSerializer mDocumentSerializer;

        public AveFolder(IAveRequest request, IAveWeb parentWeb, IAveList parentList, IAveFolder parentFolder, IDictionary<string, object> prop)
        {
            mRequest = request;
            mParentList = parentList as AveList;
            //prop["ParentWeb"] = parentWeb;
            base.DataCache.AddProperty("ParentWeb", parentWeb);
            if (parentFolder != null)
            {
                //prop["ParentFolder"] = parentFolder;
                base.DataCache.AddProperty("ParentFolder", parentFolder);
            }
            if (parentList != null)
            {
                //prop["ParentList"] = parentList;
                base.DataCache.AddProperty("ParentList", parentList);
            }
            base.DataCache.AddPropertyies(prop);
        }

        #region IAveFolder Members

        public IAveFileCollection Files
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Files"))
                {
                    IDictionary<string, object> filesProp = new Dictionary<string, object>();
                    if (mParentList == null)
                    {
                        if (base.DataCache.IsPropertyAvailable("FilesCount") && base.DataCache.GetPropertyWithoutChange<int>("FilesCount") > 0) //在获取folder的时候直接获取到filecount和foldercount，如果下面没有fiels的话就不需要再发请求
                        {
                            filesProp = this.mRequest.GetFiles(this.ParentWeb.ServerRelativeUrl, null, this.ServerRelativeUrl);
                        }
                        else
                        {
                            filesProp.AddChildren(new List<IDictionary<string, object>>());
                        }
                    }
                    else
                    {
                        filesProp = this.mRequest.GetFiles(this.ParentWeb.ServerRelativeUrl, mParentList.Title, this.ServerRelativeUrl);
                    }
                    AveFileCollection files = new AveFileCollection(mRequest, ParentWeb, mParentList, this, filesProp);
                    base.DataCache.AddProperty("Files",files);
                }
                return base.DataCache.GetProperty<IAveFileCollection>("Files");
            }
        }

        public int ItemCount
        {
            get
            {
                return base.DataCache.GetProperty<int>("ItemCount");
            }
        }

        public string Name
        {
            get
            {
                return base.DataCache.GetProperty<string>("Name");
            }
        }

        public IAveFolder ParentFolder
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("ParentFolder"))
                {
                    string parentFolderServerRelativeUrl = base.DataCache.GetProperty<string>("ParentFolder" + AveObjectModelConstant.ObjectPropertySuffix);
                    Dictionary<string, object> parentFolderProp = null;
                    if (string.IsNullOrEmpty(parentFolderServerRelativeUrl))
                    {
                        parentFolderProp = new Dictionary<string, object>();
                        parentFolderProp["Exists"] = false;
                    }
                    else
                    {
                        if (mParentList != null && parentFolderServerRelativeUrl.StartsWith(this.mParentList.RootFolder.ServerRelativeUrl.TrimEnd('/') + "/", StringComparison.OrdinalIgnoreCase))
                        {
                            parentFolderProp = mRequest.GetFolder(this.ParentWeb.ServerRelativeUrl, this.mParentList.Title, parentFolderServerRelativeUrl);
                        }
                        else
                        {
                            parentFolderProp = mRequest.GetFolder(this.ParentWeb.ServerRelativeUrl, null, parentFolderServerRelativeUrl);
                        }
                    }
                    AveFolder parentFolder = new AveFolder(mRequest, this.ParentWeb, mParentList, null, parentFolderProp);
                    base.DataCache.AddProperty("ParentFolder",parentFolder);
                }
                return base.DataCache.GetProperty<IAveFolder>("ParentFolder");
            }
        }

        public Hashtable Properties
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Properties") && base.DataCache.IsPropertyAvailable("Properties" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    Hashtable properties = base.DataCache.GetPropertyWithoutChange<Hashtable>("Properties" + AveObjectModelConstant.ObjectPropertySuffix);
                    base.DataCache.AddProperty("Properties",new AveCustomHashtable(properties, SetChangeProperty));
                }
                return base.DataCache.GetProperty<AveCustomHashtable>("Properties");
            }
        }

        public string ServerRelativeUrl
        {
            get
            {
                return base.DataCache.GetProperty<string>("ServerRelativeUrl");
            }
        }

        public string WelcomePage
        {
            get
            {
                return base.DataCache.GetProperty<string>("WelcomePage");
            }
            set
            {
                base.DataCache.AddChangedProperty("WelcomePage", value);
            }
        }

        public string Url
        {
            get
            {
                return base.DataCache.GetProperty<string>("Url");
            }
        }

        public IAveWeb ParentWeb
        {
            get
            {
                return base.DataCache.GetProperty<IAveWeb>("ParentWeb");
            }
        }

        public bool Exists
        {
            get
            {
                return base.DataCache.GetProperty<bool>("Exists");
            }
        }

        public IAveListItem Item
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Item") && base.DataCache.IsPropertyAvailable("Item" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    Dictionary<string, object> itemProperties = base.DataCache.GetProperty<Dictionary<string, object>>("Item" + AveObjectModelConstant.ObjectPropertySuffix);
                    itemProperties["Folder"] = this;
                    AveListItem item = new AveListItem(this.mRequest, this.ParentWeb, this.mParentList, itemProperties, false);
                    base.DataCache.AddProperty("Item",item);
                }
                return base.DataCache.GetProperty<IAveListItem>("Item");
            }
        }

        public Guid ParentListId
        {
            get
            {
                return this.ParentList != null ? this.ParentList.ID : Guid.Empty;
            }
        }

        public IAveFolderCollection SubFolders
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("SubFolders"))
                {
                    Dictionary<string, object> subFoldersProp = new Dictionary<string, object>();
                    if (mParentList == null)
                    {
                        int folderCount;
                        if (base.DataCache.TryGetProperty<int>("FoldersCount",out folderCount) && folderCount > 0) //在获取folder的时候直接获取到filecount和foldercount，如果下面没有folder的话就不需要再发请求
                        {
                            subFoldersProp = mRequest.GetFolders(this.ParentWeb.ServerRelativeUrl, null, Guid.Empty, this.ServerRelativeUrl);
                        }
                        else
                        {
                            subFoldersProp.AddChildren(new List<IDictionary<string, object>>());
                        }
                    }
                    else
                    {
                        subFoldersProp = mRequest.GetFolders(this.ParentWeb.ServerRelativeUrl, mParentList.Title, mParentList.ID, this.ServerRelativeUrl);
                    }
                    AveFolderCollection folders = new AveFolderCollection(mRequest, this.ParentWeb, this.mParentList, this, subFoldersProp);
                    base.DataCache.AddProperty("SubFolders",folders);
                }
                return base.DataCache.GetProperty<IAveFolderCollection>("SubFolders");
            }
        }

        public Guid UniqueId
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("UniqueId");
            }
        }
        public object SPFolder
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public void Delete()
        {
            this.mRequest.DeleteFolder(this.ParentWeb.ServerRelativeUrl, this.ServerRelativeUrl);
        }

        public void Update()
        {
            Dictionary<string, object> newProp = null;
            if (mParentList != null)
            {
                IList<IAveContentType> uniqueContentTypeOrder;
                if (base.DataCache.TryGetChangedProperty("UniqueContentTypeOrder",out uniqueContentTypeOrder))
                {
                    List<string> contentTypeOrder = new List<string>();
                    if (uniqueContentTypeOrder != null)
                    {
                        foreach (IAveContentType contentType in uniqueContentTypeOrder)
                        {
                            contentTypeOrder.Add(contentType.ID.ToString());
                        }
                        base.DataCache.AddChangedProperty("UniqueContentTypeOrder",contentTypeOrder);
                    }
                }
                newProp = this.mRequest.UpdateFolder(this.ParentWeb.ServerRelativeUrl, mParentList.Title, this.ServerRelativeUrl, base.DataCache.ChangedProperties);
            }
            else
            {
                newProp = this.mRequest.UpdateFolder(this.ParentWeb.ServerRelativeUrl, null, this.ServerRelativeUrl, base.DataCache.ChangedProperties);
            }
            base.DataCache.UpdateProperties(newProp);
        }

        public IList<IAveContentType> UniqueContentTypeOrder
        {
            get
            {
                return base.DataCache.GetProperty<IList<IAveContentType>>("UniqueContentTypeOrder");
            }
            set
            {
                base.DataCache.AddChangedProperty("UniqueContentTypeOrder", value);
            }
        }

        public void MoveTo(string desServerRelativeUrl)
        {
            this.mRequest.FolderMoveTo(this.ParentWeb.ServerRelativeUrl, this.ServerRelativeUrl, desServerRelativeUrl);
        }

        public IAveFolderCollection Folders
        {
            get
            {
                return this.SubFolders;
            }
        }

        public IAveList ParentList
        {
            get
            {
                return base.DataCache.GetProperty<IAveList>("ParentList");
            }
            set
            {
                base.DataCache.AddProperty("ParentList",value);
            }
        }

        #endregion

        internal Dictionary<string, object> GetDocInfo(AveBaseItemInfo baseItemInfo, Dictionary<string, object> docInfo)
        {
            docInfo["Id"] = baseItemInfo.GUID;
            docInfo["DoclibRowId"] = baseItemInfo.RowId;
            docInfo["UIVersion"] = baseItemInfo.Version;
            if (this.Properties != null)
            {
                docInfo["Level"] = this.Properties.ContainsKey("vti_level") ? Byte.Parse(this.Properties["vti_level"].ToString()) : 1;
            }
            else
            {
                docInfo["Level"] = 1;
            }
            //this.Properties
            if (this.Properties != null)
            {
                docInfo["Properties"] = this.Properties;
            }
            return docInfo;
        }


        public List<AveHiddenFileInfo> HiddenFiles
        {
            get
            {
                if (mHiddenFileInfoList == null)
                {
                    mHiddenFileInfoList = new List<AveHiddenFileInfo>(this.Files.Count);
                    foreach (AveFile file in this.Files)
                    {
                        if (file.Item != null && file.Item.ID > 0)
                        {
                            continue;
                        }
                        AveHiddenFileInfo fileInfo = new AveHiddenFileInfo();
                        fileInfo.Name = file.Name;
                        fileInfo.Level = (byte)file.Level;
                        fileInfo.Version = file.UIVersion;
                        fileInfo.ID = file.UniqueId.ToString();
                        mHiddenFileInfoList.Add(fileInfo);
                    }
                }
                return mHiddenFileInfoList;
            }
        }

        public Guid Recycle()
        {
            return this.mRequest.RecycleFolder(this.ParentWeb.ServerRelativeUrl, this.ServerRelativeUrl);
        }

        public IAveAudit Audit
        {
            get { throw new NotImplementedException(); }
        }
        /// <summary>
        /// folder.StorageMetrics
        /// </summary>
        public AveStorageMetrics StorageMetrics
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("StorageMetrics"))
                {
                    AveStorageMetrics aveStorageMetrics = this.mRequest.GetFolderStorageMetrics(this.ParentWeb.ServerRelativeUrl, this.ServerRelativeUrl);
                    base.DataCache.AddProperty("StorageMetrics",aveStorageMetrics);
                }
                return base.DataCache.GetProperty<AveStorageMetrics>("StorageMetrics");
            }
        }

        public DateTime? TimeCreated
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("TimeCreated"))
                {
                    return null;
                }
                return base.DataCache.GetProperty<DateTime>("TimeCreated");
            }
        }

        public DateTime? TimeLastModified
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("TimeLastModified"))
                {
                    return null;
                }
                return base.DataCache.GetProperty<DateTime>("TimeLastModified");
            }
        }

        public void Reload()
        {
            //throw new NotImplementedException();
            this.DataCache.RemoveProperty("SubFolders");
        }


        public List<int> GetItemsByColumnValue(string columnDisplayName, string value)
        {
            return null;
        }

        public IAveDocumentSet DocumentSet
        {
            get
            {
                if (this.Item.FieldValues.ContainsKey("HTML_x0020_File_x0020_Type")
                    && this.Item.FieldValues["HTML_x0020_File_x0020_Type"].ToString().Equals("Sharepoint.DocumentSet", StringComparison.OrdinalIgnoreCase))
                {
                    return new AveDocumentSet(mRequest, this);
                }
                else
                {
                    return null;
                }
            }
        }

        public IAveDocumentSerializer DocumentSerializer
        {
            get
            {
                if (mDocumentSerializer == null)
                {
                    mDocumentSerializer = new AveDocumentSerializer(this, this.mParentList as AveList, this.ParentWeb as AveWeb, mRequest);
                }
                return mDocumentSerializer;
            }
        }

        public int ID
        {
            get
            {
                var itemObject = base.DataCache.GetProperty<Dictionary<string, object>>("ItemObject");
                object id;
                if (itemObject != null && itemObject.TryGetValue("ID", out id))
                {
                    return (id != null && id is int) ? (int)id : default(int);
                }
                return default(int);
            }
        }

        public void SetChangeProperty(object key, object value)
        {
            if (key == null)
            {
                return;
            }
            if (!this.DataCache.ChangedProperties.ContainsKey("FolderChangeProperties"))
            {
                this.DataCache.AddChangedProperty("FolderChangeProperties",new Dictionary<string, object>());
            }
            Dictionary<string, object> folderChangedProperties = this.DataCache.ChangedProperties["FolderChangeProperties"] as Dictionary<string, object>;
            folderChangedProperties[key.ToString()] = value;
        }

        public AveRestoreResult RestoreFolder(AveFolderInfo info, Dictionary<string, object> allDocData, Dictionary<string, object> allUserData)
        {
            Dictionary<string, object> docData = AveList.AssembleBaseItemInfo(info, this.mParentList);

            if (mParentList != null)
            {
                Dictionary<string, object> listProperties = new Dictionary<string, object>();

                listProperties.Add("ListId", mParentList.ID);
                listProperties.Add("ListTitle", mParentList.Title);
                listProperties.Add("BaseType", (int)mParentList.BaseType);
                listProperties.Add("ListRootFolderUrl", mParentList.RootFolder.ServerRelativeUrl);
                listProperties.Add("ListDefaultViewUrl", mParentList.DefaultViewUrl);

                listProperties.Add("ListTemplate", (int)mParentList.BaseTemplate);
                listProperties.Add("ListEnableModeration", mParentList.EnableModeration);
                listProperties.Add("ListEnableVersioning", mParentList.EnableVersioning);
                listProperties.Add("ListEnableMinorVersions", mParentList.EnableMinorVersions);
                docData["ParentListProperties"] = listProperties;
            }
            if (this.ParentWeb != null)
            {
                Dictionary<string, object> webProperties = new Dictionary<string, object>();
                webProperties.Add("ParentWebTemplate", ParentWeb.WebTemplate);
                webProperties.Add("ServerRelativeUrl", ParentWeb.ServerRelativeUrl);
                docData["ParentWebProperties"] = webProperties;
            }
            if (allDocData.ContainsKey("ComplianceTag"))
            {
                docData["ComplianceTag"] = allDocData["ComplianceTag"];
            }
            if (allUserData.ContainsKey("#tp_GUID"))
            {
                docData["GUID"] = allUserData["#tp_GUID"];
            }
            if (allDocData.ContainsKey("Properties"))
            {
                docData["Properties"] = allDocData["Properties"];
            }
            if (allDocData.ContainsKey("docset_LastRefresh"))
            {
                docData.Add("docset_LastRefresh", allDocData["docset_LastRefresh"]);
            }
            if (allDocData.ContainsKey("snapshots"))
            {
                docData.Add("snapshots", allDocData["snapshots"]);
            }
            if (allDocData.ContainsKey("vti_contenttypeorder") && allDocData["vti_contenttypeorder"] != null)
            {
                docData["vti_contenttypeorder"] = allDocData["vti_contenttypeorder"];
            }
            Dictionary<string, object> fields = AveList.ConvertFieldValuesToString(info.FieldsInfo.Fields, info.FieldsInfo.MultilookupFields, mParentList != null ? (int)mParentList.BaseTemplate : -1);
            if (this.mParentList != null)
            {
                mParentList.SetTaxonomyField(info, -1, true, info.FieldsInfo.TermIdMapping);
                if (mParentList.NeedSetNullFields == null)
                {
                    mParentList.NeedSetNullFields = mParentList.SetNeedSetNullFields(info.KeepDefaultValue, info.FieldsInfo.Fields);
                }
                fields.Add("NeedSetNullFields", mParentList.NeedSetNullFields);
                if (info.FieldsInfo.Fields.ContainsKey("TaxonomyFields"))
                {
                    fields.Add("TaxonomyFields", info.FieldsInfo.Fields["TaxonomyFields"]);
                }
            }
            if (!fields.ContainsKey("Modified"))
            {
                fields.Add("Modified", info.DTimeLastModified);
            }
            Dictionary<string, object> restoreResult = mRequest.RestoreFolder(docData, fields);

            info.AveItem.Folder = new AveFolder(mRequest, this.ParentWeb, mParentList, null, restoreResult);
            if (info.AveItem.Folder != null)
            {
                info.AveItem.ListItem = info.AveItem.Folder.Item;
                if (mParentList != null && (int)mParentList.BaseTemplate == 108 && info.IsInCommunityDiscussion)
                {
                    List<string> internalNames = new List<string>() { "Popularity" };
                    info.AveItem.SkipRestoreSpecialListColumnValues(info, internalNames);
                }
            }
            if (info.AveItem.ListItem != null)
            {
                info.RowId = info.AveItem.ListItem.ID;
            }
            info.IsNewCreated = restoreResult.ContainsKey("IsNewCreated") && Convert.ToBoolean(restoreResult["IsNewCreated"]);
            if (mParentList != null && (int)this.mParentList.BaseTemplate == 108 && info.IsInCommunityDiscussion && info.AveItem.Folder.Item != null && info.IsNewCreated)
            {
                this.ParentWeb.DiscussionTopicCache[info.AveItem.Folder.Item.ID] = info;
            }
            return AveRestoreResult.Normal;
        }
    }
}
