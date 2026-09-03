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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Backup
{
    public class AveSPAttachment
    {
        private Guid mId;
        private string mName;
        private AveSPFolder mAveParentFolder;
        private IAveBackupStream mSender;
        private AveSPItem mAveSPItem;
        //add for RevIM export
        //Vault use，means item or folder's AveSPItem depended by attachment. Default is null.
        public AveSPItem HostListItem { get; private set; }
        public AveSPFolder ParentFolder
        {
            get { return mAveParentFolder; }
        }

        public AveSPItem AveSPItem
        {
            get { return mAveSPItem; }
        }

        public string Name
        {
            get { return mName; }
        }

        public AveSPAttachment(AveSPFolder aveFolder, Guid id, string name, string serverRelativeUrl = null)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPAttachment.Constructor"))
            {
                mAveParentFolder = aveFolder;
                mSender = aveFolder.Sender;
                mId = id;
                mName = name;
                mAveSPItem = new AveSPItem(id, 0, 512, serverRelativeUrl, AveItemType.Attachement, Guid.Empty, aveFolder.AveList.ParentWeb.ParentSite.SPSite.ID, aveFolder.AveList,
                    mSender, aveFolder.QueryService, null, null, aveFolder.SPFolder);

                //mAveSPItem.InternalVersion = mAveSPItem.InternalVersion;
                mAveSPItem.SetAttachmentInfo();
            }
        }

        public AveSPAttachment(AveSPFolder aveFolder, Guid id, string name, string serverRelativeUrl, AveSPItem dependItem)
            : this(aveFolder, id, name, serverRelativeUrl)
        {
            this.HostListItem = dependItem;
        }

        public AveSPAttachment(AveSPList aveList, Guid id, string name, string serverRelativeUrl = null)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPAttachment.Constructor"))
            {
                mSender = aveList.Sender;
                mId = id;
                mName = name;
                mAveSPItem = new AveSPItem(id, 0, 512, serverRelativeUrl, AveItemType.Attachement, Guid.Empty, aveList.ParentWeb.ParentSite.SPSite.ID, aveList,
                    mSender, aveList.QueryService, null, null, null);

                //mAveSPItem.InternalVersion = mAveSPItem.InternalVersion;
                mAveSPItem.SetAttachmentInfo();
            }
        }

        public void ExportRbsId(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPAttachment.ExportRbsId"))
            {
                mAveSPItem.ExportRbsId(output);
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ExportStorgeInfo is a common method")]
        public void ExportStorgeInfo(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPAttachment.ExportStorgeInfo"))
            {
                mAveSPItem.ExportStorageInfo(output);
            }
        }

        public Dictionary<string, object> GetAttachmentInfo()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPAttachment.GetAttachmentInfo"))
            {
                return mAveSPItem.GetAttachmentInfo();
            }
        }

        public void ExportDocInfo(IAveBackupStream output ,AveSPItem parentNode = null)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPAttachment.ExportDocInfo"))
            {
                Dictionary<string, object> attachmentInfo = GetAttachmentInfo();
                if (parentNode?.Item?.ListItem?.FieldValues?.ContainsKey("GUID") == true)
                {
                    attachmentInfo["ParentItemGuid"] = parentNode.Item.ListItem.FieldValues["GUID"];
                }
                output.WriteMetadata(AveMetadataType.DocProperty.ToString(), attachmentInfo);
            }
        }

        //public void ExportContent(IAveBackupStream output, IStreamConvertor streamConvertor)
        //{
        //    using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPAttachment.ExportContent"))
        //    {
        //        mAveSPItem.ExportContent(output, streamConvertor);
        //    }
        //}

        public void ExportContent(IAveBackupStream output)
        {
            using(AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPAttachment.ExportContent"))
            {
                mAveSPItem.ExportContent(output);
            }
        }

        public string ExportContentAndCalculateCRC(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPAttachment.ExportContentAndCalculateCRC"))
            {
                return mAveSPItem.ExportContentByAPIAndCalculateCRC(output);
            }
        }

        public Stream GetContent()
        {
            return mAveSPItem.GetContent();
        }

        public void ExportFullTextIndex(IAveBackupStream output, Dictionary<string, object> customFieldValues)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPAttachment.ExportFullTextIndex"))
            {
                var index = ParentFolder.AveList.AveIndexCache.GetIndexForAttachment(this.AveSPItem);
                if (customFieldValues != null)
                {
                    index.SetCustomColumnValues(customFieldValues);
                }
                output.WriteMetadata(AveMetadataType.FullTextIndex, index);
            }
        }
    }
}