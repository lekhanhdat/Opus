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
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;
using System.IO;
using AvePoint.Wrapper.Common;
using AvePoint.Office365.Api;
using AvePoint.GCommon;
using AveClientRequest.Common;

namespace AvePoint.ObjectModel.ClientOM
{
    internal class AveO365AttachmentRestore : IDisposable
    {
        private static IAveLogger logger = AveLogger.GetInstance(typeof(AveAttachmentRestore));
        private string mWebRelativeUrl;
        private string mListTitle;
        private Guid mListId;
        private int mRowId;
        private string mAttachmentLeafName;
        private int mAttachmentSize;
        private Web mWeb;
        private List mList;
        private ListItem mListItem;
        private AveRestoreOption mRestoreOption;
        protected AveClientContext mContext;
        protected ITokenProvider mTokenProvider;
        private const int LARGE_FILE_BLOCK_SIZE = 50 * 1024 * 1024;//50M
        private AveClientOM2013Request mRequest;
        private bool listVersionEnable;
        private bool listModeration;

        public AveO365AttachmentRestore(AveClientOM2013Request request, AveClientContext context, ITokenProvider mainTokenProvider)
        {
            mContext = context;
            mTokenProvider = mainTokenProvider;
            mRequest = request;
        }


        protected void PrepareRestoreContext(Dictionary<string, object> data)
        {
            mWebRelativeUrl = data["WebUrl"] as string;
            mListTitle = data["ListTitle"] as string;
            mListId = new Guid(data["ListId"].ToString());
            mRowId = Convert.ToInt32(data["DoclibRowId"]);
            mAttachmentLeafName = data["Name"] as string;
            mAttachmentSize = Convert.ToInt32(data["Size"]);
            mRestoreOption = (AveRestoreOption)data["RestoreOption"];
            mWeb = mContext.Site.OpenWeb(mWebRelativeUrl);
            mContext.Load(mWeb, w => w.Url);
            mList = mWeb.Lists.GetById(mListId);
            mContext.Load(mList, l => l.EnableVersioning, l => l.EnableModeration);
            mListItem = mList.GetItemById(mRowId);
        }

        public Dictionary<string, object> RestoreAttachment(Dictionary<string, object> docData, Stream fileStream)
        {
            PrepareRestoreContext(docData);

            Attachment attachment = null;
            if (IsAttachmentExists(ref attachment))
            {
                if (mRestoreOption == AveRestoreOption.OverWrite)
                {
                    attachment.DeleteObject();
                    mContext.ExecuteQuery();
                }
                else
                {
                    Dictionary<string, object> attachmentProperties = new Dictionary<string, object>();
                    mRequest.AssembleAttachmentProperties(attachment, attachmentProperties);
                    return attachmentProperties;
                }
            }
            lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("ListSettingLocker"))
            {
                DisableListVersion();
                try
                {
                    if (fileStream.Length < WrapperConfiguration.BPOS_S.UploadLimit)
                    {
                        return AddSmallAttachment(fileStream);
                    }
                    else if (fileStream.Length < LARGE_FILE_BLOCK_SIZE)
                    {
                        return AddLargeFile(fileStream, true);
                    }
                    else
                    {
                        return AddLargeFile(fileStream, false);
                    }
                }
                finally
                {
                    RevertListVersion();
                }
            }
        }

        private void RevertListVersion()
        {
            bool ifChange = false;
            if (listVersionEnable)
            {
                mList.EnableVersioning = true;
                ifChange |= true;
            }
            if (!listModeration)
            {
                mList.EnableModeration = false;
                ifChange |= true;
            }

            if (ifChange)
            {
                mList.Update();
                mContext.ExecuteQuery();
            }
        }

        private void DisableListVersion()
        {
            listVersionEnable = mList.EnableVersioning;
            if (listVersionEnable)
            {
                mList.EnableVersioning = false;
            }
            listModeration = mList.EnableModeration;
            mList.EnableModeration = true;
            mList.Update();
        }

        private bool IsAttachmentExists(ref Attachment attachment)
        {
            ExceptionHandlingScope ehScope = new ExceptionHandlingScope(mContext);
            using (ehScope.StartScope())
            {
                using (ehScope.StartTry())
                {
                    attachment = mListItem.AttachmentFiles.GetByFileName(mAttachmentLeafName);
                    //modify for SAAS-23463 增加comment的load
                    mContext.Load(mListItem, i => i["Editor"], i => i["Modified"], i => i["_ModerationStatus"], i => i["_ModerationComments"]);
                    mContext.Load(attachment);
                }
                using (ehScope.StartCatch())
                {
                    //modify for SAAS-23463
                    mContext.Load(mListItem, i => i["Editor"], i => i["Modified"], i => i["_ModerationStatus"], i => i["_ModerationComments"]);
                }
            }
            mContext.ExecuteQuery();
            return !ehScope.HasException;
        }

        private Dictionary<string, object> AddSmallAttachment(Stream fileStream)
        {
            Dictionary<string, object> attachmentProperties = new Dictionary<string, object>();
            Attachment attachment = null;

            attachment = AddAttachment(fileStream);
            UpdateModifiedAndModeration();
            mContext.ExecuteQuery();
            mRequest.AssembleAttachmentProperties(attachment, attachmentProperties);
            return attachmentProperties;
        }

        private Dictionary<string, object> AddLargeFile(Stream fileStream, bool useRestApi)
        {
            Dictionary<string, object> attachmentProperties = new Dictionary<string, object>();
            Attachment attachment = null;
            attachment = AddAttachment(new MemoryStream(new byte[] { 0 }));
            mContext.ExecuteQuery();
            if (useRestApi)
            {
                FileRestProcessor.AddFileByRestApi(mContext, mTokenProvider, mWeb.Url, Guid.Empty, attachment.ServerRelativeUrl, fileStream, true);
            }
            else
            {
                FileCsomProcessor.UploadLargeFile(mContext, attachment.ServerRelativeUrl, fileStream, () =>
                {
                    return mWeb.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(attachment.ServerRelativeUrl));
                });
            }
            UpdateModifiedAndModeration();
            mContext.ExecuteQuery();
            mRequest.AssembleAttachmentProperties(attachment, attachmentProperties);
            return attachmentProperties;
        }

        private void UpdateModifiedAndModeration() //SAAS-1996
        {
            ListItem tempListItem = new ListItem(this.mList.Context, new ObjectPathMethod(this.mList.Context, this.mList.Path, "GetItemById", new object[] { mRowId }));
            tempListItem["Modified"] = mListItem["Modified"];
            tempListItem["Editor"] = mListItem["Editor"];
            tempListItem["_ModerationStatus"] = mListItem["_ModerationStatus"];
            //add for SAAS-23463
            tempListItem["_ModerationComments"] = mListItem["_ModerationComments"];
            tempListItem.Update();
        }

        private Attachment AddAttachment(Stream stream)
        {
            AttachmentCreationInformation attachmentCreationInfo = new AttachmentCreationInformation();
            attachmentCreationInfo.FileName = mAttachmentLeafName;
            attachmentCreationInfo.ContentStream = stream;
            Attachment attachment = mListItem.AttachmentFiles.Add(attachmentCreationInfo);
            mContext.Load(attachment);
            return attachment;
        }

        public void Dispose()
        {
        }
    }
}
