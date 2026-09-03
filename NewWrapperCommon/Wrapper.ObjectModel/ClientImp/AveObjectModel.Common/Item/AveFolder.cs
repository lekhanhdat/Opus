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
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.ObjectModel.Common
{
    class AveFolder : AveClientObject, IAveFolder
    {
        private IAveRequest mRequest;
        private IAveList mParentList;
        private List<AveHiddenFileInfo> mHiddenFileInfoList;

        public AveFolder(IAveRequest request, IAveWeb parentWeb, IAveList parentList, IAveFolder parentFolder, Dictionary<string, object> prop)
        {
            mRequest = request;
            mParentList = parentList;
            prop["ParentWeb"] = parentWeb;
            if (parentFolder != null)
            {
                prop["ParentFolder"] = parentFolder;
            }
            if (parentList != null)
            {
                prop["ParentList"] = parentList;
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
                    //Dictionary<string, object> filesProp = null;
                    //if (mParentList == null)
                    //{
                    //    filesProp = this.mRequest.GetFiles(this.ParentWeb.ServerRelativeUrl, null, this.ServerRelativeUrl);
                    //}
                    //else
                    //{
                    //    filesProp = this.mRequest.GetFiles(this.ParentWeb.ServerRelativeUrl, mParentList.Title, this.ServerRelativeUrl);
                    //}
                    //AveFileCollection files = new AveFileCollection(mRequest, ParentWeb, mParentList, this, filesProp);
                    //base.DataCache.PropertiesCache["Files"] = files;
                    AveFileCollection files = new AveFileCollection(mRequest, ParentWeb, mParentList, this);
                    base.DataCache.PropertiesCache["Files"] = files;
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
                    if (mParentList != null && parentFolderServerRelativeUrl.StartsWith(this.mParentList.RootFolder.ServerRelativeUrl.TrimEnd('/') + "/", StringComparison.OrdinalIgnoreCase))
                    {
                        parentFolderProp = mRequest.GetFolder(this.ParentWeb.ServerRelativeUrl, this.mParentList.Title, this.mParentList.ID, parentFolderServerRelativeUrl);
                    }
                    else
                    {
                        parentFolderProp = mRequest.GetFolder(this.ParentWeb.ServerRelativeUrl, null, Guid.Empty, parentFolderServerRelativeUrl);
                    }
                    AveFolder parentFolder = new AveFolder(mRequest, this.ParentWeb, mParentList, null, parentFolderProp);
                    base.DataCache.PropertiesCache["ParentFolder"] = parentFolder;
                }
                return base.DataCache.GetProperty<IAveFolder>("ParentFolder");
            }
        }

        public Hashtable Properties
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Properties"))
                {
                    //13 API include Folder.Properties, we can init it with API.
                    if (base.DataCache.IsPropertyAvailable("Properties" + AveObjectModelConstant.ObjectPropertySuffix))
                    {
                        IDictionary properties = base.DataCache.PropertiesCache["Properties" + AveObjectModelConstant.ObjectPropertySuffix] as IDictionary;
                        base.DataCache.PropertiesCache["Properties"] = new AveCustomHashtable(properties, SetChangeProperty);
                    }
                    //For SP 2010 , we can use RPC to get properties.
                    else if (this.ParentWeb.Site.SPVersion.StartsWith("14", StringComparison.OrdinalIgnoreCase))
                    {
                        base.DataCache.PropertiesCache["Properties"] = new AveCustomHashtable(mRequest.GetMetaInfo(this.ParentWeb.ServerRelativeUrl, this.ServerRelativeUrl), SetChangeProperty);
                    }
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
                    base.DataCache.PropertiesCache["Item"] = item;
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
                    AveFolderCollection folders = new AveFolderCollection(mRequest, this.ParentWeb, this.mParentList, this);
                    base.DataCache.PropertiesCache["SubFolders"] = folders;
                    //Dictionary<string, object> subFoldersProp = null;
                    //if (mParentList == null)
                    //{
                    //    subFoldersProp = mRequest.GetFolders(this.ParentWeb.ServerRelativeUrl, null, Guid.Empty, this.ServerRelativeUrl);
                    //}
                    //else
                    //{
                    //    subFoldersProp = mRequest.GetFolders(this.ParentWeb.ServerRelativeUrl, mParentList.Title, mParentList.ID, this.ServerRelativeUrl);
                    //}
                    //AveFolderCollection folders = new AveFolderCollection(mRequest, this.ParentWeb, this.mParentList, this, subFoldersProp);
                    //base.DataCache.PropertiesCache["SubFolders"] = folders;
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
            if ((this.ParentFolder as AveFolder).DataCache.IsPropertyAvailable("SubFolders"))
            {
                //Remove cache data.
                ((this.ParentFolder as AveFolder).SubFolders as AveFolderCollection).ListData.Remove(this);
            }
        }

        public void Update()
        {
            Dictionary<string, object> newProp = null;
            if (mParentList != null)
            {
                if (base.DataCache.ChangedProperties.ContainsKey("UniqueContentTypeOrder"))
                {
                    IList<IAveContentType> uniqueContentTypeOrder = base.DataCache.ChangedProperties["UniqueContentTypeOrder"] as IList<IAveContentType>;
                    List<string> contentTypeOrder = new List<string>();
                    foreach (IAveContentType contentType in uniqueContentTypeOrder)
                    {
                        contentTypeOrder.Add(contentType.ID.ToString());
                    }
                    base.DataCache.ChangedProperties["UniqueContentTypeOrder"] = contentTypeOrder;
                }
                newProp = this.mRequest.UpdateFolder(this.ParentWeb.ServerRelativeUrl, mParentList.Title, mParentList.ID, this.ServerRelativeUrl, base.DataCache.ChangedProperties);
            }
            else
            {
                newProp = this.mRequest.UpdateFolder(this.ParentWeb.ServerRelativeUrl, null, Guid.Empty, this.ServerRelativeUrl, base.DataCache.ChangedProperties);
            }
            base.DataCache.UpdateProperties(newProp);
        }

        public IList<IAveContentType> ContentTypeOrder
        {
            get
            {
                throw new NotImplementedException();
            }
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

        public void MoveTo(string newUrl)
        {
            this.mRequest.MoveTo(this.ParentWeb.Url,this.ParentWeb.ServerRelativeUrl, this.ServerRelativeUrl, newUrl);
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
                base.DataCache.PropertiesCache["ParentList"] = value;
            }
        }

        #endregion
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Doclib is a part of keys")]
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
            if (this.Properties != null)
            {
                docInfo["Properties"] = new Hashtable(this.Properties);
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
                        fileInfo.TimeLastModified = file.TimeLastModified;
                        mHiddenFileInfoList.Add(fileInfo);
                    }
                }
                return mHiddenFileInfoList;
            }
        }

        public Guid Recycle()
        {
            throw new NotImplementedException();
        }

        public IAveAudit Audit
        {
            get { throw new NotImplementedException(); }
        }

        public void Reload(bool force = true)
        {
            //throw new NotImplementedException();
        }


        public List<int> GetItemsByColumnValue(string columnDisplayName, string value)
        {
            List<int> listIds = new List<int>();
            if (this.mParentList.Fields.ContainsField(columnDisplayName))
            {
                IAveField field = this.mParentList.Fields[columnDisplayName];
                string internalName = field.InternalName;
                AveCamlQuery camlQuery = new AveCamlQuery();
                camlQuery.FolderServerRelativeUrl = this.ServerRelativeUrl;
                string query = string.Empty;
                if (field.Type == AveFieldType.Text)
                {
                    query = string.Format("<View><Query><Where><Eq><FieldRef Name='{0}'/><Value Type='Text'>{1}</Value></Eq></Where></Query></View>", internalName, value);
                }
                else if (field.Type == AveFieldType.User)
                {
                    query = string.Format("<View><Query><Where><Eq><FieldRef Name='{0}' LookupId=\"TRUE\"/><Value Type='Integer'>{1}</Value></Eq></Where></Query></View>", internalName, value);
                }
                if (!string.IsNullOrEmpty(query))
                {
                    camlQuery.ViewXml = query;
                    IAveListItemCollection items = this.mParentList.GetItems(camlQuery);
                    foreach (IAveListItem item in items)
                    {
                        if (!listIds.Contains(item.ID))
                        {
                            listIds.Add(item.ID);
                        }
                    }
                    return listIds;
                }
            }
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

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "vti_docstoreversion is folder's property")]
        private void SetChangeProperty(object key, object value)
        {
            //folder的"vti_docstoreversion"属性只有真实365才存在，无法将该属性更新到模拟365站点。目的端如果为模拟站点，在此不Set
            if ((key == null) || (this.mRequest.Type != AveClientRequestType.AveClientOMOffice365Request && key.ToString().Equals("vti_docstoreversion", StringComparison.Ordinal)))
            {
                return;
            }
            if (!this.DataCache.ChangedProperties.ContainsKey("FolderChangeProperties"))
            {
                this.DataCache.ChangedProperties["FolderChangeProperties"] = new Dictionary<string, object>();
            }          
            Dictionary<string, object> folderChangedProperties = this.DataCache.ChangedProperties["FolderChangeProperties"] as Dictionary<string, object>;
            folderChangedProperties[key.ToString()] = value;
        }
    }
}
