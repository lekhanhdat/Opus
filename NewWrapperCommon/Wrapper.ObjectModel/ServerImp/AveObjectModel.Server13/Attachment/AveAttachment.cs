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
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.QueryService;
using AvePoint.Wrapper.Resource.ServerAPI2010;

namespace AvePoint.ObjectModel.Server13
{
    internal class AveAttachment : AveServerObject, IAveAttachment
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private string mFileName;
        private Guid mROWID;
        private string mServerRelativeUrl;
        private AveListItem mListItem;
        private AveAttachmentInfo mAttachmentInfo;
        private AveSite mSite;

        public AveAttachment(AveWeb web, string fileName, Guid rowId)
        {
            mFileName = fileName;
            mROWID = rowId;
            mSite = web.Site as AveSite;
        }

        public AveAttachment(AveAttachmentInfo info, IAveListItem listItem)
        {
            mAttachmentInfo = info;
            mListItem = listItem as AveListItem;
            mSite = listItem.ParentList.ParentWeb.Site as AveSite;
        }

        public AveAttachment(object obj, AveListItem listItem, string serverRelativeUrl)
        {
            mListItem = listItem;
            mFileName = AveAssemblyUtility.GetPropertyValue(obj, "FileName").ToString();
            mROWID = new Guid(AveAssemblyUtility.GetPropertyValue(obj, "ROWID").ToString());
            mServerRelativeUrl = serverRelativeUrl.TrimEnd('/') + '/' + mFileName;
        }

        #region IAveAttachment Members

        public string FileName
        {
            get { return mFileName; }
        }

        public Guid ROWID
        {
            get { return mROWID; }
        }

        public string ServerRelativeUrl
        {
            get { return mServerRelativeUrl; }
        }

        public Guid GetParentId()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveAttachment.GetParentId"))
            {

                IAveFolder parentFolder;
                Guid id;
                string parentFolderServerRelativeUrl = string.Empty;
                try
                {
                    byte tempLevel = (byte)mListItem.Level;
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.DEBUG, ServerAPIResource.UpdateItemError, ex.ToString());
                    mListItem = mListItem.ParentList.GetItemById(mListItem.ListItem.ID) as AveListItem;
                }

                try
                {
                    string webFullUrl = mListItem.ParentList.ParentWeb.Site.MakeFullUrl(mListItem.ParentList.ParentWeb.ServerRelativeUrl);
                    if (mListItem.Attachments.UrlPrefix.StartsWith(webFullUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        parentFolderServerRelativeUrl = mListItem.Attachments.UrlPrefix.Substring(webFullUrl.Length + 1);
                    }
                    else
                    {
                        throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Server_AttachmentUrlIncorrect, mListItem.Attachments.UrlPrefix, webFullUrl);
                    }
                    parentFolder = mListItem.ParentList.ParentWeb.GetFolder(parentFolderServerRelativeUrl);
                    id = parentFolder.UniqueId;
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, ServerAPIResource.GetFolderIdError, e.ToString());
                    id = mListItem.ParentList.RootFolder.SubFolders["Attachments"].SubFolders[mListItem.ID.ToString()].UniqueId;
                }

                return id;

            }

        }

        public void Delete()
        {
            // Do not use DeleteNow because it will call item.Update() method and increases version
            //删除Attachment同时使用SystemUpdate不会对Item造成影响，故删除了keep editor的逻辑。提高效率。
            mListItem.Attachments.Delete(mAttachmentInfo.RealName);
            mListItem.SystemUpdate(false);
        }

        public bool Exists(AveAttachmentInfo attachmentInfo)
        {
            return mSite.QueryService.IsAttachmentExist(attachmentInfo.SiteId, attachmentInfo.ParentId, attachmentInfo.RealName);
        }

        #endregion
    }
}
