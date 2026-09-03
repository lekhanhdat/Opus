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



using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;
using System.Collections;
using System;
using System.IO;
using AvePoint.GCommon;
using System.Collections.Generic;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.ObjectModel.Server13
{
    class AveAttachmentCollection : AveAbstractCommonCollection<IAveAttachment>, IAveAttachmentCollection
    {
        private static AveLogger mLog = AveLogger.GetInstance(typeof(AveAttachmentCollection));
        private SPAttachmentCollection mAttachments;
        private IList m_InnerAttachments;
        private AveListItem mListItem;
        private AveSite mSite;
        private bool mDirty = false;
        private string mServerRelativeUrlPrefix;

        public AveAttachmentCollection(SPAttachmentCollection attachments, AveListItem listItem)
            : base(AveAssemblyUtility.GetFieldValue(attachments, "m_Attachments") as IList)
        {
            m_InnerAttachments = mEnumerable as IList;
            mAttachments = attachments;
            mListItem = listItem;
            mSite = mListItem.ParentList.ParentWeb.Site as AveSite;
            mDirty = false;
            mServerRelativeUrlPrefix = AveAssemblyUtility.GetFieldValue(attachments, "m_strServerRelativeUrlPrefix") as string;
        }

        #region IAveAttachmentCollection Members

        public string AddNow(string leafName, byte[] data)
        {
            string name = mAttachments.AddNow(leafName, data);
            mDirty = true;
            return name;
        }

        public void Add(string leafName, FileStream stream)
        {
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            mAttachments.AddNow(leafName, buffer);
        }

        public string UrlPrefix
        {
            get { return mAttachments.UrlPrefix; }
        }

        public override IAveAttachment this[int index]
        {
            get
            {
                return new AveAttachment(m_InnerAttachments[index], mListItem, mServerRelativeUrlPrefix);
            }
        }

        public void Add(string leafName, byte[] data)
        {
            mAttachments.Add(leafName, data);
            mDirty = true;
        }

        public void Delete(string leafName)
        {
            mAttachments.Delete(leafName);
            mDirty = true;
        }

        #endregion

        /// <summary>
        /// Use the SharePoint API instead of Native restore.
        /// </summary>
        /// <exception>
        /// ArgumentNullException, content size of attachment is 0.
        /// </exception>
        /// <param name="info"></param>
        /// <param name="receiver"></param>
        [SuppressMessage("FxCopCustomRules", "C100004:DoNotUseGCCollectMethod", Justification = "The reason about add this supress message")]
        public void RestoreAttachment(AveAttachmentInfo info, IAveRestoreStream receiver)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveAttachmentCollection.RestoreAttachment"))
            {

                AveItem aveItem = info.AveItem as AveItem;
                aveItem.GetSOIntegrationUtilForRestore(receiver);
                if (aveItem.mList.SOIntegrationUtil.StorageInfo.Size > mSite.WebApplication.MaximumFileSize * 1024 * 1024)
                {
                    throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Server_FileSizeTooLarge);
                }

                if (aveItem.mList.SOIntegrationUtil.StorageInfo.IsBackupLinkForArchivedData)
                {
                    info.IsStubData = true;
                    RestoreSOAttachment(info, receiver);
                    return;
                }

                byte[] realContent = new byte[receiver.ContentLength];
                //不能上传Content为空的attachment，对于只备份Stub的case，前面已经return了，这里不需要考虑。
                if (realContent.Length == 0)
                {
                    throw new ArgumentNullException("realContent", @"Can not upload empty attachment");
                }
                if(receiver.ContentLength>=int.MaxValue)
                {
                    throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Server_FileSizeTooLarge);
                }
                //we should use the SP API directly instead of uploading a fake data for SP2013.
                receiver.ReadContent(realContent, 0, (int)receiver.ContentLength);
                mAttachments.Add(info.RealName, realContent);
                mDirty = true;
                //ADO-132066
                //当List有Calculate Column使用了Modified或者Created Column的时候，需要在这个地方给Column重新复制才会让Calculate column value正确
                //正常情况下不需要对column 进行重新赋值
                mListItem[SPBuiltInFieldId.Created] = mListItem[SPBuiltInFieldId.Created];
                mListItem[SPBuiltInFieldId.Modified] = mListItem[SPBuiltInFieldId.Modified];
                mListItem.SystemUpdate();


                info.ParentId = info.Attachment.GetParentId();
                info.GUID = mSite.QueryService.GetAttachmentUniqueId(info, info.RealName);
                //We don't need to use internal version for SP2013.
                //int internalVersion = mSite.QueryService.GetAttachmentVersion(info, info.RealName);

                //It will increase item version if we use stream to restore the content of attachment
                //Let's wait for next SP version and test it.
                //SPFile attachment = mRootItem.ParentList.ParentWeb.GetFile(mId);
                //AveSPFileStream stream = new AveSPFileStream(mReceiver);
                //attachment.SaveBinary(stream);

                aveItem.SetAttachmentInfo(info.GUID, 0);
                //Have used the SP API to upload the file directly, so no need to update the content again.
                //aveItem.RestoreContentByNative(receiver, storageInfo);    

                aveItem.File = aveItem.Web.GetFile(info.GUID, info.ServerRelativeUrl); //keep the same behavior as sp2010 attachment restore
                //We don't need to restore stub info for SP2013
                //aveItem.RestoreStubDBInfo(receiver);

            }

        }

        private void RestoreSOAttachment(AveAttachmentInfo info, IAveRestoreStream receiver)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveAttachmentCollection.RestoreAttachment"))
            {

                AveItem aveItem = info.AveItem as AveItem;

                byte[] tempContent = new Guid("1c834929-4f7d-4ac5-a3a7-a13fd8578ec7").ToByteArray();

                mAttachments.Add(info.RealName, tempContent);
                mDirty = true;

                mListItem.SystemUpdate();
                
                info.ParentId = info.Attachment.GetParentId();
                info.GUID = mSite.QueryService.GetAttachmentUniqueId(info, info.RealName);

                aveItem.SetAttachmentInfo(info.GUID, 0);
                aveItem.File = aveItem.Web.GetFile(info.GUID, info.ServerRelativeUrl); //keep the same behavior as sp2010 attachment restore
                //Have used the SP API to upload the file directly, so no need to update the content again.
                aveItem.RestoreContentByNative(receiver);

                //We don't need to restore stub info for SP2013
                aveItem.RestoreStubDBInfo();

            }

        }

        public bool Exists(AveAttachmentInfo attachmentInfo)
        {
            foreach (string leafName in mAttachments)
            {
                if (string.Equals(leafName, attachmentInfo.RealName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
            //return mSite.DBService.IsAttachmentExsits(attachmentInfo.SiteId, attachmentInfo.ParentId, attachmentInfo.RealName);
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveAttachment(t, mListItem, mServerRelativeUrlPrefix);
        }

        public override int Count
        {
            get { return mAttachments.Count; }
        }

        public bool IsDirty
        {
            get
            {
                return mDirty;
            }
        }
    }
}
