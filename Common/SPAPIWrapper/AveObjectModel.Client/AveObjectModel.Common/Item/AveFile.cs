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
using AvePoint.Wrapper.Common;
using System.Collections;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource;

namespace AvePoint.ObjectModel.Common
{
    class AveFile : AveClientObject, IAveFile
    {
        private static AveLogger Logger = AveLogger.GetInstance(typeof(AveFile));
        private IAveRequest mRequest;
        private AveList mParentList;

        public AveFile(IAveRequest request, AveWeb web, AveList parentList, AveFolder parentFolder, IDictionary<string, object> prop)
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
        public IAveList ParentList => mParentList;

        public IAveUser Author
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Author") && base.DataCache.IsPropertyAvailable("Author" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    string loginName = base.DataCache.GetProperty<string>("Author" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveUser author = this.Web.SiteUsers.GetByLoginName(loginName) as AveUser;
                    base.DataCache.AddProperty("Author",author);
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
                    base.DataCache.AddProperty("CheckedOutByUser",checkedOutByUser);
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
                    base.DataCache.AddProperty("Item",item);
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
                    base.DataCache.AddProperty("LockedByUser",lockedByUser);
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
                    Hashtable properties = null;
                    if (this.Item != null)
                    {
                        string metainfo = this.Item[AveBuiltInFieldId.MetaInfo] as string;
                        MetaInfoHandler infoHandler = new MetaInfoHandler(metainfo);
                        properties = infoHandler.ToHashtable();
                    }
                    else
                    {
                        properties = new Hashtable();
                    }
                    base.DataCache.AddProperty("Properties",new AveCustomHashtable(properties, SetChangeProperty));
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
                    base.DataCache.AddProperty("Versions" + AveObjectModelConstant.ObjectPropertySuffix, versions);
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
                        parentFolderProp = mRequest.GetFolder(this.Web.ServerRelativeUrl, this.mParentList.Title, parentFolderServerRelativeUrl);
                    }
                    else
                    {
                        parentFolderProp = mRequest.GetFolder(this.Web.ServerRelativeUrl, null, parentFolderServerRelativeUrl);
                    }
                    AveFolder parentFolder = new AveFolder(mRequest, this.Web, mParentList, null, parentFolderProp);
                    base.DataCache.AddProperty("ParentFolder",parentFolder);
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

        public void CheckIn(string comment)
        {
            this.CheckIn(comment, AveCheckinType.MinorCheckIn);
        }

        public void CheckIn(string comment, AveCheckinType checkinType)
        {
            this.mRequest.CheckIn(this.Web.ServerRelativeUrl, this.ServerRelativeUrl, comment, (int)checkinType);
        }

        public void CheckOut()
        {
            this.mRequest.CheckOut(this.Web.ServerRelativeUrl, this.ServerRelativeUrl);
        }

        public void CopyTo(string strNewUrl, bool bOverWrite)
        {
            this.mRequest.CopyTo(this.Web.ServerRelativeUrl, this.ServerRelativeUrl, strNewUrl, bOverWrite);
        }

        public IAveLimitedWebPartManager GetLimitedWebPartManager(AvePersonalizationScope scope)
        {
            if (AveUrlUtility.IsAspx(this.ServerRelativeUrl, false))
            {
                Dictionary<string, object> webpartManagerProperties = this.mRequest.GetLimitedWebPartManager(this.Web.ServerRelativeUrl, this.ServerRelativeUrl, (int)scope);
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

        public void MoveToKeepEditor(string newUrl, string editor, DateTime modified, AveMoveOperations flags)
        {
            this.mRequest.MoveToKeepEditor(this.Web.ServerRelativeUrl, this.ServerRelativeUrl, newUrl, editor, modified, (int)flags);
        }

        public byte[] OpenBinary(AveOpenBinaryOptions openOptions)
        {
            var content = mRequest.GetFileBinary(this.Web.ServerRelativeUrl, this.ServerRelativeUrl, (int)openOptions, this.UniqueId);
            ThrowIfFileContentDismatch(content.Length);
            return content;
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
        public void DeleteAllVersion()
        {
            this.mRequest.DeleteFileVersions(this.Web.ServerRelativeUrl, this.ServerRelativeUrl);
        }
        public void Delete()
        {
            this.mRequest.DeleteFile(this.Web.ServerRelativeUrl, this.ServerRelativeUrl);
        }

        public byte[] OpenBinary()
        {
            var content = mRequest.GetFileBinary(this.Web.ServerRelativeUrl, this.ServerRelativeUrl, (int)AveOpenBinaryOptions.None, this.UniqueId);
            ThrowIfFileContentDismatch(content.Length);
            return content;
        }

        public Stream OpenBinaryStream()
        {
            var stream = mRequest.GetFileStream(this.Web.ServerRelativeUrl, this.ServerRelativeUrl, "File", this.UniqueId);
            if (WrapperConfiguration.CheckFileContentDismatch)
            {
                try
                {
                    ThrowIfFileContentDismatch(stream.Length);
                }
                catch (Exception)
                {
                    stream?.Dispose();
                    throw;
                }
            }
            else 
            {
                long contentLength = stream.Length;
                if (contentLength < this.Length)
                {
                    mLogger.Warn($"FileContentDismatch, discover length:{this.Length}, rest api result length:{contentLength}, url:{this.ServerRelativeUrl}");
                }
            }
            return stream;
        }

        public Stream OpenVersionBinaryStream(int versionId)
        {
            string versionUrl = "_vti_history/" + versionId + "/" + this.ServerRelativeUrl.Substring(this.Web.ServerRelativeUrl.Length).TrimStart('/');
            var stream = mRequest.GetFileVersionStream(this.Web.ServerRelativeUrl, this.ServerRelativeUrl, versionUrl, versionId, this.UniqueId);
            try
            {
                ThrowIfFileContentDismatch(stream.Length);
            }
            catch (Exception)
            {
                stream?.Dispose();
                throw;
            }
            return stream;
        }

        public void RevertContentStream()
        {
            mRequest.RevertContentStream(this.Web.ServerRelativeUrl, this.Url);
        }

        #endregion

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Used as a key")]
        internal void GetDocInfo(AveBaseItemInfo baseItemInfo, Dictionary<string, object> docInfo)
        {
            //system file always has one version unless it is checkout
            if (baseItemInfo.RowId > 0 || this.Level == AveFileLevel.Checkout)
            {
                IAveFileVersion itemVersion = this.Versions.GetVersionFromID(baseItemInfo.Version);
            }
            else
            {
                baseItemInfo.IsCurrentVersion = true;
            }            
            docInfo["IsCurrentVersion"] = baseItemInfo.IsCurrentVersion;
            docInfo["Id"] = baseItemInfo.GUID;
            docInfo["DoclibRowId"] = baseItemInfo.RowId;
            docInfo["UIVersion"] = baseItemInfo.Version;
            docInfo["Level"] = (byte)this.Level;
            if (this.Properties != null && this.Properties.ContainsKey("vti_setuppath"))
            {
                docInfo["SetupPath"] = this.Properties["vti_setuppath"].ToString();                
            }
            docInfo["HasStream"] = 1;// this.CustomizedPageStatus == AveCustomizedPageStatus.Uncustomized ? 0 : 1;
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
            docInfo["CustomizedPageStatus"] = (int)this.CustomizedPageStatus;
            baseItemInfo.ScopeUrl = base.DataCache.GetProperty<string>("ServerRelativeUrl").Trim('/');// (base.DataCache.GetProperty<string>("FileDirRef") + "/" + base.DataCache.GetProperty<string>("FileLeafRef")).TrimStart('/');
            baseItemInfo.HasStream = Convert.ToInt32(docInfo["HasStream"]) == 1 ? true : false;
        }


        public Stream OpenBinaryStream(AveOpenBinaryOptions option)
        {
            //throw new NotImplementedException();
            bool isNeedGetFileStreamWithAPI = mParentList == null ? false : mParentList.IsSpecialList;
            var stream = mRequest.GetFileStream(this.Web.ServerRelativeUrl, this.ServerRelativeUrl, "File", this.UniqueId, isNeedGetFileStreamWithAPI);
            ThrowIfFileContentDismatch(stream.Length);
            return stream;
        }


        public IAveUser ModifiedBy
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("ModifiedBy") &&
                    base.DataCache.IsPropertyAvailable("ModifiedBy" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    string loginName = base.DataCache.GetProperty<string>("ModifiedBy" + AveObjectModelConstant.ObjectPropertySuffix);
                    base.DataCache.AddProperty("ModifiedBy",this.Web.SiteUsers.GetByLoginName(loginName) as AveUser);
                }
                return base.DataCache.GetProperty<IAveUser>("ModifiedBy");
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
        public void RecycleVersionsByIds(List<int> ids)
        {
            if (ids != null && ids.Count > 0)
            {
                this.mRequest.RecycleFileVersionByIdList(this.Web.ServerRelativeUrl, this.ServerRelativeUrl, ids);
            }
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

        public string LinkingUri
        {
            get {
                return base.DataCache.GetProperty<string>("LinkingUri");
            }
        }

        public bool ChangeContent(IAveSite site, IAveFile file, AveDocumentInfo info)
        {
            bool changed = false;
            bool result = false;
            InfoPathLinkReplace replacer = new InfoPathLinkReplace();
            string contnetType = null;
            bool isListForm = false;
            byte[] buffer = replacer.FixXSNBinary(file.OpenBinary(), info.Url, info.MappingManager, ref changed, ref contnetType, ref isListForm);
            if (changed && file.ParentFolder.ParentList != null)
            {
                IAveList list = file.ParentFolder.ParentList;
                bool updateModeration = false;
                if (file.Level == AveFileLevel.Published)
                {
                    if (list.EnableMinorVersions || list.EnableModeration)
                    {
                        updateModeration = list.EnableModeration;
                        list.EnableMinorVersions = false;
                        list.EnableModeration = false;
                        info.SettingInfo.LIST_SETTING_CHANGED = true;
                        list.Update();
                    }
                }
                //file.CheckOut();
                try
                {
                    file.SaveBinary(buffer);
                    result = true;
                }
                catch (Exception e)
                {
                    Logger.Warn("Save InfoPath content error {0}", e.Message);
                }
                //file.CheckIn("", AveCheckinType.OverwriteCheckIn);
                if (updateModeration)
                {
                    list.EnableModeration = true;
                    list.Update();
                }
            }
            else if (changed)
            {
                try
                {
                    file.SaveBinary(buffer);
                    result = true;
                }
                catch (Exception e)
                {
                    Logger.Warn("Save InfoPath content error when parent list was null {0}", e.Message);
                }
            }
            return result;
        }

        public DateTime GetLastAccessTime(Guid id, string folderServerRelativeUrl, DateTime modified, bool isCompatibleByModifiedTime = false)
        {
            return mRequest.QueryLastAccessTime(id, folderServerRelativeUrl, modified, isCompatibleByModifiedTime);
        }

        public void UnlockSensitivityLabelEncryptedFile(string justificationText)
        {
            mRequest.UnlockSensitivityLabelEncryptedFile(Url, "");
        }

        /// <summary>
        /// SAAS-37060
        /// Compare discover length with rest api stream length
        /// </summary>
        /// <param name="contentLength"></param>
        private void ThrowIfFileContentDismatch(long contentLength)
        {
            if (contentLength < this.Length)
            {
                mLogger.Error($"ThrowIfFileContentDismatch, discover length:{this.Length}, rest api result length:{contentLength}, url:{this.ServerRelativeUrl}");
                throw new AveWrapperException(AveWrapperErrorCode.FileContentLengthDismatch, string.Format(WrapperRestoreReportResource.FileContentLengthDismatch, this.ServerRelativeUrl));
            }
        }
    }
}
