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
using System.Net;
using System.Xml;
using System.Web;
using System.Collections;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using AvePoint.GCommon;
using System.Security.Cryptography;

namespace AvePoint.ObjectModel.Common
{
    class AveFile : AveClientObject, IAveFile
    {
        private IAveRequest mRequest;
        private AveList mParentList;
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveFile));
        public AveFile(IAveRequest request, AveWeb web, AveList parentList, AveFolder parentFolder, Dictionary<string, object> prop)
        {
            mRequest = request;
            mParentList = parentList;
            if (prop != null)
            {
                prop["Web"] = web;
                if (parentFolder != null)
                {
                    prop["ParentFolder"] = parentFolder;
                }
                base.DataCache.AddPropertyies(prop);
            }
        }

        #region IAveFile Members
        public IAveUser Author
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Author") &&
                    base.DataCache.IsPropertyAvailable("Author" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    string loginName = base.DataCache.GetProperty<string>("Author" + AveObjectModelConstant.ObjectPropertySuffix);
                    base.DataCache.PropertiesCache["Author"] = this.Web.SiteUsers.GetByLoginName(loginName) as AveUser;
                }
                return base.DataCache.GetProperty<IAveUser>("Author");
            }
        }

        public IAveUser CheckedOutByUser
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("CheckedOutByUser") && base.DataCache.IsPropertyAvailable("CheckedOutByUser" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    string loginName = base.DataCache.GetProperty<string>("CheckedOutByUser" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveUser checkedOutByUser = this.Web.SiteUsers.GetByLoginName(loginName) as AveUser;
                    base.DataCache.PropertiesCache["CheckedOutByUser"] = checkedOutByUser;
                }
                return base.DataCache.GetProperty<IAveUser>("CheckedOutByUser");
            }
        }

        public string CheckInComment
        {
            get
            {
                return base.DataCache.GetProperty<string>("CheckInComment");
            }
        }

        public AveCheckOutType CheckOutType
        {
            get
            {
                return base.DataCache.GetProperty<AveCheckOutType>("CheckOutType");
            }
        }

        public AveCustomizedPageStatus CustomizedPageStatus
        {
            get
            {
                return base.DataCache.GetProperty<AveCustomizedPageStatus>("CustomizedPageStatus");
            }
        }

        public string ETag
        {
            get
            {
                return base.DataCache.GetProperty<string>("ETag");
            }
        }

        public bool Exists
        {
            get
            {
                return base.DataCache.GetProperty<bool>("Exists");
            }
        }

        public bool InDocumentLibrary
        {
            get
            {
                if (mParentList != null)
                {
                    if (mParentList.BaseType == AveBaseType.DocumentLibrary)
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
        }

        public IAveListItem Item
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Item") && base.DataCache.IsPropertyAvailable("Item" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    Dictionary<string, object> itemProperties = base.DataCache.GetProperty<Dictionary<string, object>>("Item" + AveObjectModelConstant.ObjectPropertySuffix);
                    itemProperties["File"] = this;
                    AveListItem item = new AveListItem(this.mRequest, this.Web, this.mParentList, itemProperties, false);
                    base.DataCache.PropertiesCache["Item"] = item;
                }
                return base.DataCache.GetProperty<IAveListItem>("Item");
            }
        }

        public long Length
        {
            get
            {
                return base.DataCache.GetProperty<long>("Length");
            }
        }

        public AveFileLevel Level
        {
            get
            {
                return base.DataCache.GetProperty<AveFileLevel>("Level");
            }
        }

        public IAveUser LockedByUser
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("LockedByUser") && base.DataCache.IsPropertyAvailable("LockedByUser" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    string loginName = base.DataCache.GetProperty<string>("LockedByUser" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveUser lockedByUser = this.Web.SiteUsers.GetByLoginName(loginName) as AveUser;
                    base.DataCache.PropertiesCache["LockedByUser"] = lockedByUser;
                }
                return base.DataCache.GetProperty<IAveUser>("LockedByUser");
            }
        }

        public int MajorVersion
        {
            get
            {
                return base.DataCache.GetProperty<int>("MajorVersion");
            }
        }

        public int MinorVersion
        {
            get
            {
                return base.DataCache.GetProperty<int>("MinorVersion");
            }
        }

        public string Name
        {
            get
            {
                return base.DataCache.GetProperty<string>("Name");
            }
        }

        public Hashtable Properties
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Properties"))
                {
                    IDictionary properties = null;
                    if (base.DataCache.IsPropertyAvailable("Properties" + AveObjectModelConstant.ObjectPropertySuffix))
                    {
                        properties = base.DataCache.GetProperty<IDictionary>("Properties" + AveObjectModelConstant.ObjectPropertySuffix);
                    }
                    else if (this.Item != null)
                    {
                        string metainfo = this.Item[AveBuiltInFieldId.MetaInfo] as string;
                        MetaInfoHandler infoHandler = new MetaInfoHandler(metainfo);
                        properties = infoHandler.ToHashtable();
                    }
                    else
                    {
                        properties = new Hashtable(mRequest.GetMetaInfo(this.Web.ServerRelativeUrl, this.ServerRelativeUrl));
                    }
                    base.DataCache.PropertiesCache["Properties"] = new AveCustomHashtable(properties, SetChangeProperty);
                }
                return base.DataCache.GetProperty<AveCustomHashtable>("Properties");
            }
        }

        public void SetChangeProperty(object key, object value)
        {
            if (key == null)
            {
                return;
            }
            if (!this.DataCache.ChangedProperties.ContainsKey("ChangedMetaInfo"))
            {
                this.DataCache.ChangedProperties["ChangedMetaInfo"] = new Dictionary<string, object>();
            }
            Dictionary<string, object> fileChangedProperties = this.DataCache.ChangedProperties["ChangedMetaInfo"] as Dictionary<string, object>;
            fileChangedProperties[key.ToString()] = value;
        }

        public string ServerRelativeUrl
        {
            get
            {
                return base.DataCache.GetProperty<string>("ServerRelativeUrl");
            }
        }

        public DateTime TimeCreated
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("TimeCreated");
            }
        }

        public DateTime TimeLastModified
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("TimeLastModified");
            }
        }

        public string Title
        {
            get
            {
                return base.DataCache.GetProperty<string>("Title");
            }
        }

        public int UIVersion
        {
            get
            {
                return base.DataCache.GetProperty<int>("UIVersion");
            }
        }

        public string UIVersionLabel
        {
            get
            {
                return base.DataCache.GetProperty<string>("UIVersionLabel");
            }
        }

        public Guid UniqueId
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("UniqueId");
            }
        }

        public IAveFileVersionCollection Versions
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Versions" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    Dictionary<string, object> versionsProp = mRequest.GetFileVersions(this.Web.ServerRelativeUrl, this.ServerRelativeUrl);
                    AveFileVersionCollection versions = new AveFileVersionCollection(this.mRequest, this.Web, this, versionsProp);
                    base.DataCache.PropertiesCache.Add("Versions" + AveObjectModelConstant.ObjectPropertySuffix, versions);
                }
                return base.DataCache.GetProperty<IAveFileVersionCollection>("Versions" + AveObjectModelConstant.ObjectPropertySuffix);
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
                    if (mParentList != null)
                    {
                        parentFolderProp = mRequest.GetFolder(this.Web.ServerRelativeUrl, this.mParentList.Title, this.mParentList.ID, parentFolderServerRelativeUrl);
                    }
                    else
                    {
                        parentFolderProp = mRequest.GetFolder(this.Web.ServerRelativeUrl, null, Guid.Empty, parentFolderServerRelativeUrl);
                    }
                    AveFolder parentFolder = new AveFolder(mRequest, this.Web, mParentList, null, parentFolderProp);
                    base.DataCache.PropertiesCache["ParentFolder"] = parentFolder;
                }
                return base.DataCache.GetProperty<IAveFolder>("ParentFolder");
            }
        }

        public IAveWeb Web
        {
            get
            {
                return base.DataCache.GetProperty<IAveWeb>("Web");
            }
        }

        public string Url
        {
            get
            {
                return base.DataCache.GetProperty<string>("Url");
            }
        }

        public AveCheckOutStatus CheckOutStatus
        {
            get
            {
                return base.DataCache.GetProperty<AveCheckOutStatus>("CheckOutStatus");
            }
        }

        public void Approve(string comment)
        {
            this.mRequest.Approve(this.Web.ServerRelativeUrl, this.ServerRelativeUrl, comment);
        }

        public void Deny(string comment)
        {
            this.mRequest.Deny(this.Web.ServerRelativeUrl, this.ServerRelativeUrl, comment);
        }

        public void CheckIn(string comment)
        {
            this.CheckIn(comment, AveCheckinType.MinorCheckIn);
        }

        public void CheckIn(string comment, AveCheckinType checkinType)
        {
            base.DataCache.UpdateProperties(this.mRequest.CheckIn(this.Web.ServerRelativeUrl, this.ServerRelativeUrl, comment, (int)checkinType));
        }

        public void CheckOut()
        {
            base.DataCache.UpdateProperties(this.mRequest.CheckOut(this.Web.ServerRelativeUrl, this.ServerRelativeUrl));
        }

        public void CopyTo(string strNewUrl, bool bOverWrite)
        {
            this.mRequest.CopyTo(this.Web.ServerRelativeUrl, this.ServerRelativeUrl, strNewUrl, bOverWrite);
        }

        public IAveLimitedWebPartManager GetLimitedWebPartManager(System.Web.UI.WebControls.WebParts.PersonalizationScope scope)
        {
            if (AveUrlUtility.IsAspx(this.ServerRelativeUrl, false))
            {
                Dictionary<string, object> webpartManagerProperties = this.mRequest.GetLimitedWebPartManager(this.Web.ServerRelativeUrl, this.ServerRelativeUrl, (int)scope, this.Web.IsAppWeb ? this.Web.Url : null);
                AveLimitedWebPartManager limitedWebPartManager = new AveLimitedWebPartManager(this.Web as AveWeb, this, mRequest, webpartManagerProperties);
                return limitedWebPartManager;
            }
            else
            {
                return null;
            }
        }

        public void MoveTo(string newUrl, AveMoveOperations flags)
        {
            this.mRequest.MoveTo(this.Web.ServerRelativeUrl, this.ServerRelativeUrl, newUrl, (int)flags);
        }

        public byte[] OpenBinary(AveOpenBinaryOptions openOptions)
        {
            var result = mRequest.GetFileBinary(this.Web.ServerRelativeUrl, this.ServerRelativeUrl, (int)openOptions);
            if (result.Length < this.Length)
            {
                throw new AveWrapperFileContentBrwokenException(AveInternalResourceKey.Wrapper_Exception_Office365_Common_FileConentBroken, this.ServerRelativeUrl);
            }
            return result;
        }

        public void SaveBinary(Stream file)
        {
            this.mRequest.SaveBinary(this.Web.ServerRelativeUrl, this.ServerRelativeUrl, file);
        }

        public void SaveBinary(byte[] file)
        {
            this.mRequest.SaveBinary(this.Web.ServerRelativeUrl, this.ServerRelativeUrl, file);
        }

        public void SaveBinary(System.IO.Stream file, bool checkRequiredFields, bool createVersion, string etagMatch, string lockIdMatch, System.IO.Stream fileFormatMetaInfo, out string etagNew)
        {
            throw new NotImplementedException();
        }

        public void UndoCheckOut()
        {
            this.mRequest.UndoCheckOut(this.Web.ServerRelativeUrl, this.ServerRelativeUrl);
        }

        public void UnPublish(string comment)
        {
            this.mRequest.UnPublish(this.Web.ServerRelativeUrl, this.ServerRelativeUrl, comment);
        }

        public void Update()
        {
            Dictionary<string, object> newProp = null;
            if (mParentList != null)
            {
                newProp = this.mRequest.UpdateFile(this.Web.ServerRelativeUrl, mParentList.Title, this.ServerRelativeUrl, base.DataCache.ChangedProperties);
            }
            else
            {
                newProp = this.mRequest.UpdateFile(this.Web.ServerRelativeUrl, null, this.ServerRelativeUrl, base.DataCache.ChangedProperties);
            }
            base.DataCache.UpdateProperties(newProp);
        }

        public void Publish(string comment)
        {
            this.mRequest.Publish(this.Web.ServerRelativeUrl, this.ServerRelativeUrl, comment);
        }

        public void Delete()
        {
            this.mRequest.DeleteFile(this.Web.ServerRelativeUrl, this.ServerRelativeUrl);
        }

        public byte[] OpenBinary()
        {
            var result = mRequest.GetFileBinary(this.Web.ServerRelativeUrl, this.ServerRelativeUrl, (int)AveOpenBinaryOptions.None);
            if (result.Length < this.Length)
            {
                throw new AveWrapperFileContentBrwokenException(AveInternalResourceKey.Wrapper_Exception_Office365_Common_FileConentBroken, this.ServerRelativeUrl);
            }
            return result;
        }

        public Stream OpenBinaryStream()
        {
            var result = mRequest.GetFileStream(this.Web.ServerRelativeUrl, this.ServerRelativeUrl, "File");
            if (result.Length < this.Length)
            {
                throw new AveWrapperFileContentBrwokenException(AveInternalResourceKey.Wrapper_Exception_Office365_Common_FileConentBroken, this.ServerRelativeUrl);
            }
            return result;
        }

        public Stream OpenVersionBinaryStream(int versionId)
        {
            string fileServerRelativeUrl = this.ServerRelativeUrl;
            if (!this.Web.ServerRelativeUrl.Equals("/"))
            {
                fileServerRelativeUrl = this.ServerRelativeUrl.Substring(this.Web.ServerRelativeUrl.Length);
            }
            string versionUrl = "_vti_history/" + versionId + fileServerRelativeUrl;
            var result = mRequest.GetFileVersionStream(this.Web.ServerRelativeUrl, this.ServerRelativeUrl, versionUrl, versionId);
            //if (result.Length < this.Length)
            //{
            //    throw new AveWrapperFileContentBrwokenException(AveInternalResourceKey.Wrapper_Exception_Office365_Common_FileConentBroken, this.ServerRelativeUrl);
            //}
            return result;
        }

        public void RevertContentStream()
        {
            mRequest.RevertContentStream(this.Web.ServerRelativeUrl, this.Url);
        }

        #endregion

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Used as a key")]
        internal void GetDocInfo(AveBaseItemInfo baseItemInfo, Dictionary<string, object> docInfo)
        {
            docInfo["Id"] = baseItemInfo.GUID;
            docInfo["DoclibRowId"] = baseItemInfo.RowId;
            docInfo["UIVersion"] = baseItemInfo.Version;
            docInfo["Level"] = (byte)this.Level;
            docInfo["CustomizedPageStatus"] = (int)this.CustomizedPageStatus;
            if (this.Properties != null && this.Properties.ContainsKey("vti_setuppath"))
            {
                docInfo["SetupPath"] = this.Properties["vti_setuppath"].ToString();
                docInfo["HasStream"] = 0;
            }
            if (this.DataCache.GetProperty<bool>("IsSystemFile"))
            {
                docInfo["HasStream"] = 0;
                int slashIndex = baseItemInfo.ServerRelativeUrl.LastIndexOf('/');
                if (slashIndex != -1 &&
                    (!baseItemInfo.ServerRelativeUrl.Substring(slashIndex + 1).EndsWith(".aspx", StringComparison.OrdinalIgnoreCase) ||
                     baseItemInfo.ServerRelativeUrl.Substring(slashIndex + 1).Equals("MonthlyArchive.aspx", StringComparison.OrdinalIgnoreCase) ||
                     baseItemInfo.ServerRelativeUrl.Substring(slashIndex + 1).Equals("docsethomepage.aspx", StringComparison.OrdinalIgnoreCase)))
                {
                    docInfo["HasStream"] = 1;
                }
                docInfo["IsSystemFile"] = true;
                docInfo["IsCurrentVersion"] = true;
                baseItemInfo.IsCurrentVersion = true;
            }
            else
            {
                docInfo["HasStream"] = 1;//set file hasStream to 1 as default
            }
            if (this.mParentList == null)
            {//Set  HasUniqueRoleAssignment = false for files in System folder  
                docInfo["HasUniqueRoleAssignments"] = false;
            }
            if (this.Properties.ContainsKey("ContentTypeId"))
            {
                var contentTypeIdString = "ContentTypeId:LW|" + this.Properties["ContentTypeId"];
                docInfo["MetaInfo"] = AveCompressedUtility.GetTCompressedBytes(contentTypeIdString);
            }
            docInfo["LeafName"] = this.Name;
            baseItemInfo.ScopeUrl = base.DataCache.GetProperty<string>("ServerRelativeUrl").Trim('/');// (base.DataCache.GetProperty<string>("FileDirRef") + "/" + base.DataCache.GetProperty<string>("FileLeafRef")).TrimStart('/');
            baseItemInfo.HasStream = Convert.ToInt32(docInfo["HasStream"]) == 1 ? true : false;
        }


        public Stream OpenBinaryStream(AveOpenBinaryOptions option)
        {
            var result = mRequest.GetFileStream(this.Web.ServerRelativeUrl, this.ServerRelativeUrl, "File");
            if (result.Length < this.Length)
            {
                throw new AveWrapperFileContentBrwokenException(AveInternalResourceKey.Wrapper_Exception_Office365_Common_FileConentBroken, this.ServerRelativeUrl);
            }
            return result;
        }


        public IAveUser ModifiedBy
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("ModifiedBy") &&
                    base.DataCache.IsPropertyAvailable("ModifiedBy" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    string loginName = base.DataCache.GetProperty<string>("ModifiedBy" + AveObjectModelConstant.ObjectPropertySuffix);
                    base.DataCache.PropertiesCache["ModifiedBy"] = this.Web.SiteUsers.GetByLoginName(loginName) as AveUser;
                }
                return base.DataCache.GetProperty<IAveUser>("ModifiedBy");
            }
        }

        public string LinkingUrl
        {
            get
            {
                return base.DataCache.GetProperty<string>("LinkingUrl");
            }
        }

        public Guid Recycle()
        {
            if (this.Item != null)
            {
                return this.Item.Recycle();
            }
            return Guid.Empty;
        }


        public void CheckOut(bool checkOutToLocal, string lastModifiedDate)
        {
            this.CheckOut();
        }

        public string CharSetName
        {
            get
            {
                return base.DataCache.GetProperty<string>("CharSetName");
            }
        }


        public DateTime CheckedOutDate
        {
            get { throw new NotImplementedException(); }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ipfs streamhash is a part of Keys")]
        public bool ChangeXSNContent(AveDocumentInfo info, Guid listId, out string publishContentTypeId)
        {
            publishContentTypeId = String.Empty;
            bool changed = false;
            string streamHashValue;
            InfoPathLinkReplace replacer = new InfoPathLinkReplace();
            replacer.site = Web.Site;
            byte[] buffer = replacer.FixXSNBinary(OpenBinary(), info.Url, info.MappingManager, listId, out publishContentTypeId, ref changed);
            using (SHA256 sha = new SHA256Managed())
            {
                byte[] hashBuffer = null;
                hashBuffer = sha.ComputeHash(buffer);
                string value = Convert.ToBase64String(hashBuffer);
                streamHashValue = value;
            }
            if (changed && ParentFolder.ParentList != null)
            {
                IAveList tempList = this.ParentFolder.ParentList;
                bool needUpdate = false;
                bool enableVersioning = tempList.EnableVersioning;
                bool enableMinorVersions = tempList.EnableMinorVersions;
                bool enableModeration = tempList.EnableModeration;
                try
                {
                    if (UIVersion % 512 == 0)
                    {
                        if (tempList.EnableMinorVersions)
                        {
                            tempList.EnableMinorVersions = false;
                            needUpdate = true;
                        }
                    }
                    else
                    {
                        if (!tempList.EnableMinorVersions)
                        {
                            tempList.EnableMinorVersions = true;
                            needUpdate = true;
                        }
                    }
                    if (Level == AveFileLevel.Published)
                    {
                        if (tempList.EnableModeration)
                        {
                            tempList.EnableModeration = false;
                            needUpdate = true;
                        }
                    }
                    else
                    {
                        if (tempList.EnableModeration == false)
                        {
                            tempList.EnableModeration = true;
                            needUpdate = true;
                        }
                    }
                    if (needUpdate)
                    {
                        tempList.Update();
                    }

                    #region SaveBinary

                    if (this.Item != null)
                    {
                        CheckOut();
                        SaveBinary(buffer);
                        Properties["ipfs_streamhash"] = streamHashValue;
                        Update();
                        CheckIn("", AveCheckinType.OverwriteCheckIn);
                    }
                    else
                    {
                        //System file do not need to chceck out.
                        SaveBinary(buffer);
                        Properties["ipfs_streamhash"] = streamHashValue;
                        Update();
                    }

                    #endregion

                }
                finally
                {
                    if (needUpdate)
                    {
                        tempList.EnableVersioning = enableVersioning;
                        tempList.EnableMinorVersions = enableMinorVersions;
                        tempList.EnableModeration = enableModeration;
                        tempList.Update();
                    }
                }
            }
            else if (changed)
            {
                try
                {
                    SaveBinary(buffer);
                    Properties["ipfs_streamhash"] = streamHashValue;
                    Update();
                }
                catch (Exception e)
                {
                    logger.Warn("Save InfoPath content error when parent list was null, error message: {0}", e);
                    return false;
                }
            }
            return true;
        }

        public IAveLinkCollection BackwardLinks
        {
            get { throw new NotImplementedException(); }
        }

        public IAveLinkCollection ForwardLinks
        {
            get { throw new NotImplementedException(); }
        }

        public void ReplaceLink(string oldUrl, string newUrl)
        {
            throw new NotImplementedException();
        }
    }
}
