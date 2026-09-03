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
        private static object lockObj = new object();
        public AveAttachmentCollection(AveListItem listItem, IAveRequest request, Dictionary<string, object> attachmentColProperties)
        {
            mListItem = listItem;
            mRequest = request;
            base.DataCache.AddPropertyies(attachmentColProperties);
            InitAttachmentCollection();
        }
        internal void InitAttachmentCollection()
        {
            List<Dictionary<string, object>> attachmentList = base.DataCache.GetProperty<List<Dictionary<string, object>>>("ChildrenProperties");
            mListData = new List<IAveAttachment>(attachmentList.Count);
            foreach (Dictionary<string, object> attachmentProperties in attachmentList)
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
            Dictionary<string, object> attachmentProperties = mRequest.AddAttachmentNow(mListItem.ParentList.ParentWeb.ServerRelativeUrl, mListItem.ParentList.Title, mListItem.ParentList.ID, mListItem.ID, leafName, data);
            AveAttachment attachment = new AveAttachment(attachmentProperties, mListItem);
            mListData.Add(attachment);
        }
        public string AddNow(string leafName, byte[] data)
        {
            Dictionary<string, object> attachmentProperties = mRequest.AddAttachmentNow(mListItem.ParentList.ParentWeb.ServerRelativeUrl, mListItem.ParentList.Title, mListItem.ParentList.ID, mListItem.ID, leafName, data);
            AveAttachment attachment = new AveAttachment(attachmentProperties, mListItem);
            mListData.Add(attachment);
            return attachment.FileName;
        }

        public void Add(string leafName, FileStream stream)
        {
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            this.AddNow(leafName, buffer);
        }

        public void Delete(string leafName)
        {
            string listServerRelativeUrl = mListItem.ParentList.RootFolder.ServerRelativeUrl;
            this.mRequest.DeleteAttachment(mListItem.ParentList.ParentWeb.ServerRelativeUrl, listServerRelativeUrl, mListItem.ParentList.Title, mListItem.ParentList.ParentWeb.ID, mListItem.ParentList.ID, mListItem.ID, leafName);
            AveAttachment attachment = mListData.Find(a => a.FileName.Equals(leafName, StringComparison.OrdinalIgnoreCase)) as AveAttachment;
            mListData.Remove(attachment);
            mListItem.DataCache.AddChangedProperty("ChangedFieldValues", new Dictionary<string, object>() { { "DeleteAttachment", null } });
        }

        public void RestoreAttachment(AveAttachmentInfo info, IAveRestoreStream receiver)
        {
            Dictionary<string, object> fieldValues = BackupListItemProperties(mListItem);

            info.ParentWebRelativeUrl = mListItem.ParentList.ParentWeb.ServerRelativeUrl;
            info.ParentListTitle = mListItem.ParentList.Title;
            info.OriginalRowId = mListItem.ID;
            Dictionary<string, object> docData = AveList.AssembleBaseItemInfo(info, this.mListItem.ParentList);
            this.AssembleAttachmentInfo(docData, info, this.mListItem.ParentList);
            this.GetAttachmentStorageInfo(receiver);
            Stream fileContent = new AveSPFileStream(receiver);
            if (fileContent.Length == 0)
            {
                throw new ArgumentNullException("realContent", @"Can not upload empty attachment");
            }

            if (docData.ContainsKey("ListId") && docData["ListId"] != null)
            {
                docData["ListId"] = this.mListItem.ParentList.ID.ToString();
            }         

            Dictionary<string, object> restoreResult = mRequest.RestoreAttachment(docData, info.FieldsInfo.Fields, fileContent);
            RefreshDataCache(restoreResult, info);
            //ADO-177075 此处加锁为了解决10模拟多线程问题
            lock (lockObj)
            {
                PreserveListItemProperties(mListItem, fieldValues); 
            }
        }

        internal Dictionary<string, object> BackupListItemProperties(AveListItem mListItem)
        {
            Dictionary<string, object> fieldValues = new Dictionary<string, object>();
            if (mListItem["Modified"] != null)
            {
                fieldValues["Modified"] = mListItem["Modified"];
            }
            if (this.mListItem.ParentList.BaseType != AveBaseType.DocumentLibrary)
            {
                if (mListItem["_ModerationStatus"] != null)
                {
                    fieldValues["_ModerationStatus"] = mListItem["_ModerationStatus"];
                    if (mListItem["_ModerationComments"] != null)
                    {
                        fieldValues["_ModerationComments"] = mListItem["_ModerationComments"];
                    }
                }
                if (mListItem["Editor"] != null)
                {
                    fieldValues["Editor"] = mListItem["Editor"];
                }
            }
            return fieldValues;
        }

        internal void PreserveListItemProperties(AveListItem mListItem, Dictionary<string, object> fieldValues)
        {
            if (fieldValues == null || fieldValues.Count <= 0)
            {
                return;
            }
            if (fieldValues.ContainsKey("Modified"))
            {
                mListItem["Modified"] = fieldValues["Modified"];
            }
            if (this.mListItem.ParentList.BaseType != AveBaseType.DocumentLibrary)
            {
                if (fieldValues.ContainsKey("_ModerationStatus"))
                {
                    mListItem["_ModerationStatus"] = fieldValues["_ModerationStatus"];
                    if (fieldValues.ContainsKey("_ModerationComments"))
                    {
                        mListItem["_ModerationComments"] = fieldValues["_ModerationComments"];
                    }
                }
                if (fieldValues.ContainsKey("Editor"))
                {
                    mListItem["Editor"] = fieldValues["Editor"];
                }
            }
            mListItem.SystemUpdate();
        }

        private void AssembleAttachmentInfo(Dictionary<string, object> docData, AveAttachmentInfo info, IAveList aveList)
        {
            docData["Name"] = info.RealName;
            docData["EnableVersioning"] = mListItem.ParentList.EnableVersioning;
        }

        private void RefreshDataCache(Dictionary<string, object> restoreResult, AveAttachmentInfo info)
        {
            AveAttachment newAttach = new AveAttachment(restoreResult, mListItem);
            if (string.IsNullOrEmpty(info.ServerRelativeUrl))
            {
                info.ServerRelativeUrl = newAttach.ServerRelativeUrl;
            }
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
