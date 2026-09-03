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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Common;
using AvePoint.Wrapper.Resource;


namespace AvePoint.Wrapper.Restore
{
    public class AveSPAttachment : RestoreableObject,IDisposable
    {
        private static AveLogger log = AveLogger.GetInstance(typeof(AveSPAttachment));
        private AveAttachmentInfo mAttachmentInfo = new AveAttachmentInfo();
        private AveSPFolder mAveRootItemParentFolder;
        private IAveBackupRestoreQueryService mQueryService;
        private IAveRestoreStream mReceiver;
        private AveSPItem mAveSPItem;
        private IAveAttachment mAttachment;
        private AveSPSite mParentSite;
        private IAveListItem mRootItem;
        private AveSPList mParentList;
        private AveSPWeb mAveWeb;
        private AveSPListItem mAveSPListItem;

        public AveSPFolder ParentFolder
        {
            get { return mAveRootItemParentFolder; }
        }

        public AveSPList ParentList
        {
            get { return mParentList; }
        }

        public IAveListItem ListItem
        {
            get { return mRootItem; }
        }

        public string Name
        {
            get { return mAttachmentInfo.RealName; }
        }
        private Guid ParentId
        {
            get
            {
                if (mAttachmentInfo.ParentId == Guid.Empty)
                {
                    mAttachmentInfo.ParentId = mAttachment.GetParentId();
                }
                return mAttachmentInfo.ParentId;
            }
        }

        public string SrcUrl
        {
            get
            {
                return mAttachmentInfo.SrcUrl;
            }
        }

        public string Url
        {
            get
            {
                return mAttachmentInfo.Url;
            }
        }

        public long Size
        {
            get
            {
                return mAttachmentInfo.Size;
            }
        }

        public IAveAttachment SPAttachment
        {
            get;
            internal set;
        }

        public AveAttachmentInfo AttachmentInfo
        {
            get
            {
                return mAttachmentInfo;
            }
        }

        public AveSPAttachment(AveSPWeb aveWeb, AveSPListItem aveSPItem, IAveRestoreStream aveRestoreStream)
        {
            mAveWeb = aveWeb;
            mAveSPListItem = aveSPItem;
            mReceiver = aveRestoreStream;
            mParentSite = mAveWeb.ParentSite;
        }

        public AveSPAttachment(AveSPFolder parent, string name)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPAttachment.Constructor"))
            {
#endif
                mAveRootItemParentFolder = parent;
                mQueryService = parent.QueryService;
                mAttachmentInfo.FullName = name;
                mAttachmentInfo.RealName = mAttachmentInfo.FullName.Substring(mAttachmentInfo.FullName.IndexOf(':') + 1);
                int rowId = Convert.ToInt32(mAttachmentInfo.FullName.Substring(0, mAttachmentInfo.FullName.IndexOf("_.", StringComparison.OrdinalIgnoreCase)));
                int tempId = parent.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.GetMappingItemId(parent.ParentList.SPList.ID, rowId);
                if (tempId != -1)
                {
                    rowId = tempId;
                }
                mRootItem = mAveRootItemParentFolder.ParentList.SPList.GetItemById(rowId);
                InitializeAttachmentInfo(parent);
                mParentSite = parent.ParentSite;
                mParentList = parent.ParentList;
                mAttachmentInfo.OriginalRowId = Convert.ToInt32(name.Substring(0, name.IndexOf('_')));
                //mAttachmentInfo.RowId = mParentList.ParentSite.MappingManager.SiteMappingManager.GetMappingItemId(mParentList.SPList.ID, mAttachmentInfo.OriginalRowId);
                mAttachmentInfo.MappingManager = mParentList.ParentSite.MappingManager;
#if PerformanceLog
            }
#endif
        }
        //replicator 使用
        public AveSPAttachment(AveSPFolder parent, AveSPListItem listItem, string name)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPAttachment.Constructor"))
            {
#endif
                parent.ParentList.ParentWeb.ReloadWebAndParentInternalForSPRequestTimeout(false);
                mAveRootItemParentFolder = parent;
                mQueryService = parent.QueryService;
                mAttachmentInfo.RealName = name.Substring(name.IndexOf(':') + 1);
                mRootItem = listItem.SPListItem;
                InitializeAttachmentInfo(parent);
                mParentSite = parent.ParentSite;
                mParentList = parent.ParentList;
#if PerformanceLog
            }
#endif
        }
        public void InitializeAttachmentInfo(AveSPFolder parent)
        {
            mAttachmentInfo.ListId = parent.ParentList.SPList.ID;
            mAttachmentInfo.SiteId = parent.ParentSite.SPSite.ID;
            mAttachment = parent.ParentSite.ObjectModelFactory.CreateAttachment(mAttachmentInfo, mRootItem);
            mAttachmentInfo.Attachment = mAttachment;
        }
        //used for replicator discussionBoard
        public AveSPAttachment(AveSPFolder parent, AveSPFolder folder, string name)
        {
            mAveRootItemParentFolder = parent;
            mQueryService = parent.QueryService;
            mAttachmentInfo.RealName = name.Substring(name.IndexOf(':') + 1);
            mRootItem = folder.SPFolder.Item;
            InitializeAttachmentInfo(parent);
            mParentSite = parent.ParentSite;
            mParentList = parent.ParentList;
            mAttachmentInfo.MappingManager = mParentList.ParentSite.MappingManager;
        }

        public AveSPAttachment(AveSPList parentList, string name, int parentRowId = 0)
        {                        
            mAttachmentInfo.RealName = name.Substring(name.IndexOf(':') + 1);            
            mAttachmentInfo.OriginalRowId = parentRowId == 0 ? Convert.ToInt32(name.Substring(0, name.IndexOf('_'))) : parentRowId;
            mAttachmentInfo.RowId = parentList.ParentSite.MappingManager.SiteMappingManager.GetMappingItemId(parentList.SPList.ID, mAttachmentInfo.OriginalRowId);
            mAttachmentInfo.ListId = parentList.SPList.ID;
            mAttachmentInfo.SiteId = parentList.SPList.ParentWeb.Site.ID;
            mAttachmentInfo.MappingManager = parentList.ParentSite.MappingManager;
            mAttachment = parentList.ParentSite.ObjectModelFactory.CreateAttachment(mAttachmentInfo, mRootItem);
            mAttachmentInfo.Attachment = mAttachment;
            mParentSite = parentList.ParentSite;
            mParentList = parentList;
            
        }

        public void SetStream(IAveRestoreStream stream)
        {
            mReceiver = stream;
        }

        public void AddAttachment()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPAttachment.AddAttachment"))
            {
                mAttachmentInfo.DocumentSize = mReceiver.ContentLength;
                mAttachmentInfo.WebappBlockTypes = ParentFolder.ParentList.ParentWeb.ParentSite.webappBlockTypes;
                IAveWebApplication Webapp = ParentFolder.ParentList.ParentWeb.ParentSite.SPSite.WebApplication;
                if (mAttachmentInfo.WebappBlockTypes == null && Webapp != null)
                {
                    mAttachmentInfo.WebappBlockTypes = Webapp.BlockedFileExtensions;
                }
                //Need set value of EnableAttachments property to true if parent list template is Posts.
                //Although it value still show false but after reset value to true the Posts list can add attachment. 
                if (mRootItem.ParentList.BaseTemplate == AveListTemplateType.Posts)
                {
                    mRootItem.ParentList.EnableAttachments = true;
                    try
                    {
                        mRootItem.ParentList.Update();
                    }
                    catch (Exception e)
                    {
                        log.Warn("Update List after set EnableAttachments to true with exception :" + e.ToString());
                    }
                }
                else
                {
                    if (!mRootItem.ParentList.EnableAttachments)
                    {
                        ParentFolder.RestoringItem.NeedSkipped = true;
                        throw new AveWrapperSkipException(string.Format("Attachments are not enabled in this list: {0}.", mRootItem.ParentList.Title));
                    }
                }
                mAveSPItem = new AveSPItem(mAveRootItemParentFolder);
                mAttachmentInfo.AveItem = mParentSite.ObjectModelFactory.CreateAveItem(mAttachmentInfo, mAveRootItemParentFolder.SPFolder, mAveRootItemParentFolder.ParentList.ParentWeb.SPWeb, mAveRootItemParentFolder.ParentList.SPList);
                mRootItem.Attachments.RestoreAttachment(mAttachmentInfo, mReceiver);

            }
        }

        public void RestoreAttachment(bool inplaceRestore, Dictionary<string, object> data)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPAttachment.AddAttachment"))
            {
#endif
            mAttachmentInfo.DocumentSize = mReceiver.ContentLength;
            mAttachmentInfo.WebappBlockTypes = this.mParentList.ParentWeb.ParentSite.webappBlockTypes;
            IAveWebApplication Webapp = this.mParentList.ParentWeb.ParentSite.SPSite.WebApplication;
            if (mAttachmentInfo.WebappBlockTypes == null && Webapp != null)
            {
                mAttachmentInfo.WebappBlockTypes = Webapp.BlockedFileExtensions;
            }
            if (data?.ContainsKey("ParentItemGuid") == true)
            {
                mAttachmentInfo.ParentId = (Guid)data["ParentItemGuid"];
            }
            int restoreOption = this.CheckRestoreOption(AveRestoreMode.OverWrite) ? 2 : 0;
            IAveAttachmentSerializer attachmentSerializer = mParentList.ParentSite.ObjectModelFactory.CreateAttachmentSerializer(mParentList.SPList, restoreOption);

            this.SPAttachment = attachmentSerializer.RestoreAttachment(mAttachmentInfo, mReceiver, inplaceRestore);
#if PerformanceLog
            }
#endif
        }
        //修改Alldocs中对应的TimeCreated和TimeLastModified字段。
        public void UpdateAllDocsPropertyByNative(DateTime timeCreated, DateTime timeLastModified)
        {
            try
            {
                mQueryService.UpdateAllDocsPropertyByNative(timeCreated, timeLastModified, ParentId, mAveRootItemParentFolder.ParentList.ParentWeb.ParentSite.SPSite.ID, mAttachmentInfo.RealName);
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, WrapperRestoreResource.UpdateAttachmentFailed, e);
            }
        }



        


        public void Dispose()
        {
            mAveSPItem?.Dispose();
        }
    }
}
