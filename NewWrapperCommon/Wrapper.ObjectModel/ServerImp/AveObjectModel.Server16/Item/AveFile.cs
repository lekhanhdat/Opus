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
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebPartPages;
using AvePoint.Wrapper.Common;
using System.IO;
using System.Collections;
using SPDisposeCheck;
using System.Web.UI.WebControls.WebParts;
using AvePoint.GCommon;
using System.Collections.Generic;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using AvePoint.ObjectModel.Server16.NonPublicAPI;
using AvePoint.Wrapper.Resource.ServerAPI2010;

namespace AvePoint.ObjectModel.Server16
{
    public class AveFile : AveServerObject, IAveFile, IDisposable
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveFile));

        private SPFile file;
        private AveUser author;
        private AveUser checkedOutByUser;
        private AveListItem item;
        private AveUser lockedByUser;
        private AveFileVersionCollection fileVersions;
        private AveFolder parentFolder;
        private AveWeb web;
        private AveFileCollection mFiles;
        private AveUser modifiedBy;
        private IAveBackupRestoreQueryService queryService;
        private AveLinkCollection backwardLinks;
        private AveLinkCollection forwardLinks;
        public AveFile(AveWeb aveWeb, SPFile spFile)
        {
            web = aveWeb;
            file = spFile;
            queryService = (web.Site as AveSite).QueryService;
        }

        internal SPFile File
        {
            get
            {
                return file;
            }
        }

        #region IAveFile Members

        internal AveFileCollection Files
        {
            get
            {
                this.EnsureParentFileCollection();
                return this.mFiles;
            }
        }

        private void EnsureParentFileCollection()
        {
            if (this.mFiles == null)
            {
                this.mFiles = this.ParentFolder.Files as AveFileCollection;
            }
        }

        public void CheckIn(string comment, AveCheckinType checkinType)
        {
            file.CheckIn(comment, (SPCheckinType)checkinType);
        }

        public void CheckOut()
        {
            file.CheckOut();
        }

        public void CheckOut(bool checkOutToLocal, string lastModifiedDate)
        {
            file.CheckOut(checkOutToLocal, lastModifiedDate);
        }

        public AveCheckOutStatus CheckOutStatus
        {
            get
            {
                return (AveCheckOutStatus)file.CheckOutStatus;
            }
        }

        public void CopyTo(string strNewUrl, bool bOverWrite)
        {
            file.CopyTo(strNewUrl, bOverWrite);
        }

        public void MoveTo(string newUrl, AveMoveOperations flags)
        {
            file.MoveTo(newUrl, (SPMoveOperations)flags);
        }

        public void SaveBinary(Stream file)
        {
          this.file.SaveBinary(file);
        }

        public void UndoCheckOut()
        {
            file.UndoCheckOut();
        }

        public void UnPublish(string comment)
        {
            file.UnPublish(comment);
        }

        public void Update()
        {
            file.Update();
        }

        public IAveUser Author
        {
            get
            {
                if (author == null)
                {
                    SPUser spAuthor = file.Author;
                    if (spAuthor != null)
                    {
                        author = new AveUser(web, spAuthor);
                    }
                }
                return author;
            }
        }

        public IAveUser CheckedOutByUser
        {
            get
            {
                if (checkedOutByUser == null)
                {
                    SPUser user = file.CheckedOutByUser;
                    if (user != null)
                    {
                        checkedOutByUser = new AveUser(web, user);
                    }
                }
                return checkedOutByUser;
            }
        }

        public string CheckInComment
        {
            get { return file.CheckInComment; }
        }

        public AveCheckOutType CheckOutType
        {
            get { return (AveCheckOutType)file.CheckOutType; }
        }

        public DateTime CheckedOutDate
        {
            get { return file.CheckedOutDate; }
        }

        public AveCustomizedPageStatus CustomizedPageStatus
        {
            get { return (AveCustomizedPageStatus)file.CustomizedPageStatus; }
        }

        public string ETag
        {
            get { return file.ETag; }
        }

        public bool Exists
        {
            get { return file.Exists; }
        }

        public AveFileLevel Level
        {
            get
            {
                return (AveFileLevel)file.Level;
            }
        }

        public IAveListItem Item
        {
            get
            {
                if (item == null)
                {
                    try
                    {
                        if (file.ParentFolder.ParentListId != null && (!Guid.Empty.Equals(file.ParentFolder.ParentListId)))
                        {
                            SPListItem spItem = file.Item;
                            if (spItem != null)
                            {
                                item = new AveListItem(this.Files.Folder.ParentList.Items as AveListItemCollection, spItem);
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        logger.Warn("Get the list item of file: {0} failed: {1}.", file.Url, ex.ToString());
                    }
                }
                return item;
            }
        }

        public IAveUser LockedByUser
        {
            get
            {
                if (lockedByUser == null)
                {
                    SPUser user = file.LockedByUser;
                    if (user != null)
                    {
                        lockedByUser = new AveUser(web, user);
                    }
                }
                return lockedByUser;
            }
        }

        public int MajorVersion
        {
            get { return file.MajorVersion; }
        }

        public int MinorVersion
        {
            get { return file.MinorVersion; }
        }

        public string Name
        {
            get { return file.Name; }
        }

        public string ServerRelativeUrl
        {
            get { return file.ServerRelativeUrl; }
        }

        public DateTime TimeCreated
        {
            get { return file.TimeCreated; }
        }

        public DateTime TimeLastModified
        {
            get { return file.TimeLastModified; }
        }

        public string Title
        {
            get { return file.Title; }
        }

        public int UIVersion
        {
            get { return file.UIVersion; }
        }

        public string UIVersionLabel
        {
            get { return file.UIVersionLabel; }
        }

        public IAveFileVersionCollection Versions
        {
            get
            {
                if (fileVersions == null)
                {
                    fileVersions = new AveFileVersionCollection(web, file.Versions);
                }
                return fileVersions;
            }
        }

        public void SaveBinary(byte[] file)
        {
           this. file.SaveBinary(file);
        }

        public void Publish(string comment)
        {
            file.Publish(comment);
        }

        public void Delete()
        {
            file.Delete();
        }

        public byte[] OpenBinary()
        {
            return file.OpenBinary();
        }

        public Stream OpenBinaryStream()
        {
            return file.OpenBinaryStream();
        }

        public Stream OpenBinaryStream(AveOpenBinaryOptions option)
        {
            return file.OpenBinaryStream((SPOpenBinaryOptions)option);
        }

        public Stream OpenVersionBinaryStream(int versionId)
        {
            return this.Versions.GetVersionFromID(versionId).OpenBinaryStream();
        }

        public IAveFolder ParentFolder
        {
            get
            {
                if (parentFolder == null)
                {
                    parentFolder = new AveFolder(web, file.ParentFolder);
                }
                return parentFolder;
            }
        }

        public IAveWeb Web
        {
            get
            {
                return web;
            }
        }

        public string Url
        {
            get { return file.Url; }
        }

        public void Approve(string comment)
        {
            file.Approve(comment);
        }

        public void Deny(string comment)
        {
            file.Deny(comment);
        }

        public void CheckIn(string comment)
        {
            file.CheckIn(comment);
        }

        [SPDisposeCheckIgnore(SPDisposeCheckID._160, "This Web will be Disposed by AveWeb")]
        public IAveLimitedWebPartManager GetLimitedWebPartManager(PersonalizationScope scope)
        {
            SPLimitedWebPartManager limitedWebPartManager = file.GetLimitedWebPartManager(scope);
            if (limitedWebPartManager == null)
            {
                return null;
            }
            AveLimitedWebPartManager manager = new AveLimitedWebPartManager(web, limitedWebPartManager, this);
            manager.File = this;
            return manager;
        }

        public byte[] OpenBinary(AveOpenBinaryOptions openOptions)
        {
            return file.OpenBinary((SPOpenBinaryOptions)openOptions);
        }

        public void SaveBinary(Stream file, bool checkRequiredFields, bool createVersion, string etagMatch, string lockIdMatch, Stream fileFormatMetaInfo, out string etagNew)
        {
           this.file.SaveBinary(file, checkRequiredFields, createVersion, etagMatch, lockIdMatch, fileFormatMetaInfo, out etagNew);
        }

        public bool InDocumentLibrary
        {
            get { return file.InDocumentLibrary; }
        }

        public long Length
        {
            get { return file.Length; }
        }

        public Hashtable Properties
        {
            get { return file.Properties; }
        }

        public Guid UniqueId
        {
            get { return file.UniqueId; }
        }

        public void RevertContentStream()
        {
            file.RevertContentStream();
        }

        public IAveUser ModifiedBy
        {
            get
            {
                if (modifiedBy == null)
                {
                    SPUser user = file.ModifiedBy;
                    if (user != null)
                    {
                        modifiedBy = new AveUser(web, user);
                    }
                }
                return modifiedBy;
            }
        }

        public string LinkingUrl
        {
            get { return file.LinkingUrl; }
        }

        private SPFile LoadCheckoutFile(bool fakeDeletedUser)
        {
            AveSite aveSite = web.Site as AveSite;
            SPWeb spWeb = (web as AveWeb).Web;
            int userId = -1;
            if (aveSite.QueryService.IsCheckOutFile(web.Site.ID, file.UniqueId, ref userId) && userId != web.CurrentUser.ID)
            {
                SPUser user = null;
                SPFile sfile = null;
                try
                {
                    user = spWeb.SiteUsers.GetByID(userId);
                    SPList spList = null;
                    try
                    {
                        spList = spWeb.Lists[file.ParentFolder.ParentListId];
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("Failed to get parent list of file {0}. Exception: {1}.", file.Title, ex);
                    }
                    sfile = aveSite.GetCheckoutWeb(web.Site.ID, spWeb, spList, user, file.UniqueId, false).GetFile(file.UniqueId);
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetFileFaild, e.ToString());
                    if (fakeDeletedUser)
                    { //file checkout user doesn't exist in the site users list.
                        aveSite.QueryService.ChangeCheckoutUserID(web.Site.ID, file.UniqueId, web.CurrentUser.ID);
                        file = spWeb.GetFile(file.UniqueId);
                        web.Site.CheckOutFileId = file.UniqueId;
                        web.Site.CheckOutUser = userId;
                    }
                }
                return sfile;
            }
            return null;
        }

        internal void Reload()
        {
            var checkoutFile = LoadCheckoutFile(false);
            if (checkoutFile != null)
            {
                file = checkoutFile;
            }
            else
            {
                file = web.Web.GetFile(file.UniqueId);
            }
        }

        public Guid Recycle()
        {
            return file.Recycle();
        }

        public string CharSetName
        {
            get
            {
                return file.CharSetName;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ipfs_streamhash is file property")]
        public bool ChangeXSNContent(AveDocumentInfo info, Guid listId, out string publishContentTypeId)
        {
            publishContentTypeId = String.Empty;
            bool changed = false;
            byte[] buffer = null;
            try
            {
                InfoPathLinkReplace replacer = new InfoPathLinkReplace();
                replacer.site = web.Site;
                buffer = replacer.FixXSNBinary(file.OpenBinary(), info.Url, info.MappingManager, listId,out publishContentTypeId, ref changed);
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred in FixXSNBinary. XSN File Url: {0}. Error: {1}", info.Url, ex);
                return false;
            }
            if (!changed)
            {
                return false;
            }

            try
            {
                SPUser author = null;
                SPUser editor = null;
                if (file.ParentFolder.ParentListId != Guid.Empty && file.Item != null)
                {
                    //SPFile.Author和SPFile.ModifiedBy是从SPFile.Property中获取value，而界面上显示的是对应的Column Value，出现过两个Value不一致的情况
                    //因此从相关Column中find对应的user信息
                    var columnValue = new SPFieldUserValue(file.ParentFolder.ParentWeb, file.Item[SPBuiltInFieldId.Author].ToString());
                    author = columnValue.User;
                    columnValue = new SPFieldUserValue(file.ParentFolder.ParentWeb, file.Item[SPBuiltInFieldId.Editor].ToString());
                    editor = columnValue.User;
                }
                else
                {
                    author = file.Author;
                    editor = file.ModifiedBy;
                }

                byte[] hashBuffer = null;
                using (SHA256 sha = new SHA256Managed())
                {
                    hashBuffer = sha.ComputeHash(buffer);
                }
                string value = Convert.ToBase64String(hashBuffer);
                //由于infopathDoc被修改，所以ipfs_streamhash也已经被改变，需要更新该property
                file.Properties["ipfs_streamhash"] = value;
                if (file.ParentFolder.ParentListId != Guid.Empty && file.Item != null)
                {
                    file.Item.SystemUpdate(false);
                }
                else
                {
                    file.Update();
                }

                using (MemoryStream stream = new MemoryStream(buffer))
                {
                    SaveBinaryWithoutIncreasingVersion(stream);
                }

                if ((Web.Site as AveSite).NativeApiPermission == WrapperNativeApiPermission.FullControl)
                {
                    queryService.UpdateAllDocsPropertyByNative(info, info.DTimeCreated, info.DTimeLastModified, info.Version);
                    queryService.UpdateSpecialPropertyByNative(editor.ID.ToString(), author.ID.ToString(), info.DTimeLastModified, info.DTimeCreated, info);
                }
                else
                {
                    if (file.Item != null)
                    {
                        //SPFile.TimeLastModified is UTC Time
                        //Info中的DateTime都是UTC时间，使用API更新的时候，需要转换成对应的Local Time
                        SPTimeZone zone = file.ParentFolder.ParentWeb.RegionalSettings.TimeZone;
                        file.Item[SPBuiltInFieldId.Author] = author;
                        file.Item[SPBuiltInFieldId.Editor] = editor;
                        file.Item[SPBuiltInFieldId.Modified] = zone.UTCToLocalTime(info.DTimeLastModified);
                        file.Item[SPBuiltInFieldId.Created] = zone.UTCToLocalTime(info.DTimeCreated);
                        if (!AveItem.AveItemSystemUpdate(file.Item, false, true, info.Level == 1, true))
                        {
                            logger.Log(AveLogLevel.WARN, "Failed to internal update file basic info while replacing file content. File url:{0}", file.ServerRelativeUrl);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred while replacing the infoPath file in content, source version: {0}, current file version: {1}, error: {2}", file.UIVersion, info.Version, ex);
                return false;
            }
            return true;
        }

        internal void SaveBinaryWithoutIncreasingVersion(Stream stream)
        {
            if (file.Level != SPFileLevel.Checkout && this.ParentFolder.ParentList != null)
            {
                IAveList tempList = this.ParentFolder.ParentList;
                bool needUpdate = false;
                bool enableVersioning = tempList.EnableVersioning;
                bool enableMinorVersions = tempList.EnableMinorVersions;
                bool enableModeration = tempList.EnableModeration;
                try
                {
                    if (file.UIVersion % 512 == 0)
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
                    if (file.Level == SPFileLevel.Published)
                    {
                        if (tempList.EnableModeration)
                        {
                            tempList.EnableModeration = false;
                            needUpdate = true;
                        }
                    }
                    if (needUpdate)
                    {
                        tempList.Update();
                    }

                    file.CheckOut();
                    file.SaveBinaryExtension(stream);
                    file.CheckIn("", SPCheckinType.OverwriteCheckIn);
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
            else
            {
                file.SaveBinaryExtension(stream);
            }

        }

        public IAveLinkCollection BackwardLinks
        {
            get
            {
                if (backwardLinks == null)
                {
                    backwardLinks = new AveLinkCollection(file.BackwardLinks);
                }
                return backwardLinks;
            }
        }

        public IAveLinkCollection ForwardLinks
        {
            get
            {
                if (forwardLinks == null)
                {
                    forwardLinks = new AveLinkCollection(file.ForwardLinks);
                }
                return forwardLinks;
            }
        }

        public void ReplaceLink(string oldUrl, string newUrl)
        {
            file.ReplaceLink(oldUrl, newUrl);
        }

        
        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (item != null)
            {
                item.Dispose();
                item = null;
            }
        }

        #endregion
    }
}
