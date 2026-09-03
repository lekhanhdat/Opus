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
using System.IO;
namespace AvePoint.ObjectModel.Common
{
    class AveAttachmentCollection : AveAbstractCommonCollection<IAveAttachment>, IAveAttachmentCollection
    {
        private IAveRequest mRequest;
        private AveListItem mListItem;

        public AveAttachmentCollection(AveListItem listItem, IAveRequest request, Dictionary<string, object> attachmentColProperties)
        {
            mListItem = listItem;
            mRequest = request;
            base.DataCache.AddPropertyies(attachmentColProperties);
            InitAttachmentCollection();
        }
        internal void InitAttachmentCollection()
        {
            var attachmentList = base.DataCache.GetChildren();
            mListData = new List<IAveAttachment>(attachmentList.Count);
            foreach(var attachmentProperties in attachmentList )
            {
                AveAttachment attachment = new AveAttachment(attachmentProperties, mListItem);               
                mListData.Add(attachment);
            }
        }
        public IAveAttachment this[int index]
        {
            get
            {
                return mListData[index];
            }
        }
        public string UrlPrefix
        {
            get
            {
                return base.DataCache.GetProperty<string>("UrlPrefix");
            }
        }
        public void Add(string leafName, byte[] data)
        {
            Dictionary<string, object> attachmentProperties = mRequest.AddAttachmentNow(mListItem.ParentList.ParentWeb.ServerRelativeUrl, mListItem.ParentList.Title, mListItem.ID, leafName, data);
            AveAttachment attachment = new AveAttachment(attachmentProperties, mListItem);
            mListData.Add(attachment);
        }
        public string AddNow(string leafName, byte[] data)
        {
            Dictionary<string, object> attachmentProperties = mRequest.AddAttachmentNow(mListItem.ParentList.ParentWeb.ServerRelativeUrl, mListItem.ParentList.Title, mListItem.ID, leafName, data);
            AveAttachment attachment = new AveAttachment(attachmentProperties, mListItem);
            mListData.Add(attachment);
            return attachment.FileName;
        }

        public void Add(string leafName, FileStream stream)
        {
            byte[] buffer = new byte[stream.Length];
            stream.ReadExInternal(buffer, 0, buffer.Length);
            this.AddNow(leafName, buffer);
        }

        public void Delete(string leafName)
        {
            string listServerRelativeUrl = mListItem.ParentList.RootFolder.ServerRelativeUrl;
            this.mRequest.DeleteAttachmentNow(mListItem.ParentList.ParentWeb.ServerRelativeUrl, listServerRelativeUrl, mListItem.ParentList.Title, mListItem.ParentList.ID, mListItem.ID, leafName);
            AveAttachment attachment = mListData.Find(a => a.FileName.Equals(leafName,StringComparison.OrdinalIgnoreCase)) as AveAttachment;
            mListData.Remove(attachment);
        }

        public void RestoreAttachment(AveAttachmentInfo info, IAveRestoreStream receiver)
        {
            info.ParentWebRelativeUrl = mListItem.ParentList.ParentWeb.ServerRelativeUrl;
            info.ParentListTitle = mListItem.ParentList.Title;
           info.ParentListId = mListItem.ParentList.ID;
            info.OriginalRowId = mListItem.ID;
            Dictionary<string, object> docData = AveList.AssembleBaseItemInfo(info,this.mListItem.ParentList);
            if (!docData.ContainsKey("DestRowId"))
            {
                docData["DestRowId"] = mListItem.ID;
            }
            docData["Name"] = info.RealName;
            docData["EnableVersioning"] = mListItem.ParentList.EnableVersioning;
            if (mListItem.FieldValues.ContainsKey("_ModerationStatus")) //SAAS-1996
            {
                docData["ParentModerationStatus"] = mListItem.FieldValues.ContainsKey("_ModerationStatus") ? mListItem["_ModerationStatus"] : -1;
                docData["ParentModified"] = mListItem.FieldValues.ContainsKey("Modified") ? mListItem["Modified"] : DateTime.Now;
                docData["ParentFileRef"] = mListItem.FieldValues.ContainsKey("FileRef") ? mListItem["FileRef"].ToString() : string.Empty;
            }
            this.GetAttachmentStorageInfo(receiver);
            Dictionary<string, object> restoreResult = mRequest.RestoreAttachment(mListItem.ParentList.ParentWeb.Url, docData, new AveSPFileStream(receiver));
            AveAttachment newAttach = new AveAttachment(restoreResult, mListItem);
            mListData.Add(newAttach);
        }

        internal void GetAttachmentStorageInfo(IAveRestoreStream stream)
        {
            AveStorageInfo mStorageInfo = null;
            AveMetadata metadata = stream.TryReadMetadata(AveMetadataType.DocStorageInfo);
            if (null != metadata)
            {
                mStorageInfo = metadata.GetMetadata<AveStorageInfo>();
            }
        }

        public bool Exists(AveAttachmentInfo attachmentInfo)
        {
            foreach (AveAttachment aa in mListData)
            {
                if (string.Equals(aa.FileName, attachmentInfo.RealName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
