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
using AvePoint.ObjectModel.ClientOM;
using AvePoint.ObjectModel.WebService;
using System.IO;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using System.Diagnostics.CodeAnalysis;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Application;

namespace AvePoint.ObjectModel.ClientOM
{
    class Ave2013AttachmentRestore : IDisposable
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(Ave2013AttachmentRestore));
        private string mWebRelativeUrl;
        private string mListTitle;
        private int mRowId;
        private string mAttachmentLeafName;
        private int mAttachmentSize;
        private bool mEnalbeVersioning;
        private AveClientOM2013Request mRequest;
        protected ClientContext mContext;
        protected object mObj;
        private object lockObj = new object();

        public Ave2013AttachmentRestore(AveClientOM2013Request request, ClientContext context, object obj)
        {
            mRequest = request;
            mContext = context;
            mObj = obj;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Doclib is a part of Keys")]
        protected void PrepareRestoreContext(Dictionary<string, object> data)
        {
            mWebRelativeUrl = data["WebUrl"] as string;
            mListTitle = data["ListTitle"] as string;
            mRowId = Convert.ToInt32(data["DoclibRowId"]);
            mAttachmentLeafName = data["Name"] as string;
            mAttachmentSize = Convert.ToInt32(data["Size"]);
            mEnalbeVersioning = Convert.ToBoolean(data["EnableVersioning"]);
        }

        public Dictionary<string, object> RestoreAttachment(Dictionary<string, object> docData, Stream fileStream)
        {
            lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("ListSettingLocker"))
            {
                PrepareRestoreContext(docData);
                if (fileStream.Length < WrapperConfiguration.BPOS_S.UploadLimit || mAttachmentLeafName.EndsWith(".mht"))
                {
                    return AddSmallAttachment(fileStream);
                }
                else
                {
                    return AddLargeAttachment(fileStream);
                }
            }
        }

        private Dictionary<string, object> AddSmallAttachment(Stream fileStream)
        {
            var attachmentProperties = new Dictionary<string, object>();
            Attachment attachment = null;
            //make sure list version will be reverted to origin
            ExceptionHandlingScope revertListVersionScope = new ExceptionHandlingScope(mContext);
            Web web = mContext.Site.OpenWeb(mWebRelativeUrl);
            List list = web.Lists.GetByTitle(mListTitle);
            using (revertListVersionScope.StartScope())
            {
                lock (lockObj)
                {
                    using (revertListVersionScope.StartTry())
                    {
                        DisableListVersion(list);
                        attachment = AddAttachment(list, fileStream);
                    }
                    using (revertListVersionScope.StartFinally())
                    {
                        RevertListVersion(list, false);
                    }
                }
            }
            mContext.ExecuteQuery();
            mRequest.AssembleAttachmentProperties(attachment, attachmentProperties);
            return attachmentProperties;
        }

        private Dictionary<string, object> AddLargeAttachment(Stream fileStream)
        {
            Dictionary<string, object> attachmentProperties = new Dictionary<string, object>();
            Attachment attachment = null;
            Web web = mContext.Site.OpenWeb(mWebRelativeUrl);
            List list = web.Lists.GetByTitle(mListTitle);
            lock (lockObj)
            {
                try
                {
                    DisableListVersion(list);
                    attachment = AddAttachment(list, new MemoryStream(new byte[] { 0 }));
                    mContext.ExecuteQuery();
                    mRequest.SaveBinary(attachment.ServerRelativeUrl, fileStream, null, true,
                        AveClientOMRequest.SaveBinaryCheckMode.Overwrite, mContext, mObj);
                }
                finally
                {
                    RevertListVersion(list, true);
                }
            }
            mRequest.AssembleAttachmentProperties(attachment, attachmentProperties);
            return attachmentProperties;
        }

        private Attachment AddAttachment(List list, Stream stream)
        {
            ListItem item = list.GetItemById(mRowId);
            AttachmentCreationInformation attachmentCreationInfo = new AttachmentCreationInformation();
            attachmentCreationInfo.FileName = mAttachmentLeafName;
            attachmentCreationInfo.ContentStream = stream;
            Attachment attachment = item.AttachmentFiles.Add(attachmentCreationInfo);
            mContext.Load(attachment);
            return attachment;
        }

        private void DisableListVersion(List list)
        {
            if (mEnalbeVersioning)
            {
                list.EnableVersioning = false;
                list.Update();
            }
        }

        private void RevertListVersion(List list, bool now)
        {
            if (mEnalbeVersioning)
            {
                list.EnableVersioning = true;
                list.Update();

                if (now)
                {
                    mContext.ExecuteQuery();
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
