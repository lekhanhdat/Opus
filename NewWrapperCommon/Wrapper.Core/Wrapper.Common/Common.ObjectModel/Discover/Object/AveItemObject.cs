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

namespace AvePoint.Wrapper.Common
{
    public class AveItemObject : IDisposable
    {
        public bool VersionAdded { get; set; } //防止IB多次查询Version，提高SQL解析效率
        public bool AllListContentAdded { get; set; }//用来标记所有list content是否已经查询，提升discover效率。
        public ChangeType ChangeTypeBeforeDelete { get; set; }
        //public bool PropertyAdded { get; set; }

        private List<AveItemObject> mStubAttachmentObjs;
        public List<AveItemObject> StubAttachmentObjs
        {
            get
            {
                if (mStubAttachmentObjs == null)
                {
                    mStubAttachmentObjs = new List<AveItemObject>();
                }
                return mStubAttachmentObjs;
            }
            set
            {
                mStubAttachmentObjs = value;
            }
        }

        private List<AveItemObject> mAttachmentObjs;
        public List<AveItemObject> AttachmentObjs
        {
            get
            {
                if (mAttachmentObjs == null)
                {
                    mAttachmentObjs = new List<AveItemObject>();
                }
                return mAttachmentObjs;
            }
            set
            {
                mAttachmentObjs = value;
            }
        }
        private List<AveVersionObject> mVersionObjs;
        public List<AveVersionObject> VersionObjs
        {
            get
            {
                if (mVersionObjs == null)
                {
                    mVersionObjs = new List<AveVersionObject>();
                }
                return mVersionObjs;
            }
            set
            {
                mVersionObjs = value;
            }
        }
        private List<AveItemObject> mSubFolderObjs;
        public List<AveItemObject> SubFolderObjs
        {
            get
            {
                if (mSubFolderObjs == null)
                {
                    mSubFolderObjs = new List<AveItemObject>();
                }
                return mSubFolderObjs;
            }
            set
            {
                mSubFolderObjs = value;
            }
        }
        private List<AveItemObject> mSubItemObjs;
        public List<AveItemObject> SubItemObjs
        {
            get
            {
                if (mSubItemObjs == null)
                {
                    mSubItemObjs = new List<AveItemObject>();
                }
                return mSubItemObjs;
            }
            set
            {
                mSubItemObjs = value;
            }
        }
        private Dictionary<Guid, AveAlertObject> mAlertObjs;
        public Dictionary<Guid, AveAlertObject> AlertObjs
        {
            get
            {
                if (mAlertObjs == null)
                {
                    mAlertObjs = new Dictionary<Guid, AveAlertObject>();
                }
                return mAlertObjs;
            }
            set
            {
                mAlertObjs = value;
            }
        }
        private Dictionary<string, AveItemObject> mNoTypeDeleteItems;
        public Dictionary<string, AveItemObject> NoTypeDeleteItems
        {
            get
            {
                if (mNoTypeDeleteItems == null)
                {
                    mNoTypeDeleteItems = new Dictionary<string, AveItemObject>();
                }
                return mNoTypeDeleteItems;
            }
            set
            {
                mNoTypeDeleteItems = value;
            }
        }


        public int? ID { get; set; } //DocLibRowID
        public Guid DocID { get; set; }
        public Guid tp_GUID { get; set; }
        public ChangeType ChangeType { get; set; }
        public ItemType ObjType { get; set; }
        public string SourceName { get; set; } //上次FB时的LeafName，为了在IB处理rename的情况
        public bool isRename { get; set; }
        public string FullUrl { get; set; } //List Releated
        public string ItemName { get; set; } //当前Item上一次Rename的Name,为了在IB处理rename的情况，对应的EventCache表的ItemName
        public long Size { get; set; }
        public bool IsSystemObject { get; set; }
        public string ModifyBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime TimeLastModified { get; set; }
        public string DirName { get; set; }
        public string LeafName { get; set; } //当前的LeafName
        public byte Level { get; set; }
        public int Uiversion { get; set; }
        public string UiVersionString { get; set; }
        public bool IsCurrentVersion { get; set; }
        public int InternalVersion { get; set; }
        public Guid ParentID { get; set; }
        public byte Type { get; set; }
        public DateTime TimeCreated { get; set; }
        public int? DocFlags { get; set; }
        public byte[] RbsId { get; set; }
        public byte[] DeleteTransactionId { get; set; }//Just For Extender
        public DateTime EventTime { get; set; }
        public int? CheckoutUserId { get; set; }
        public bool HasStream { get; set; }
        public bool? Hidden { get; set; }
        public int QueryType { get; set; }//Just For Extender. 2 is from Alldocs,3 is from alldocversions
        public byte[] Content { get; set; } //Just For Extender
        public string ServerRelativeUrl { get; set; }
        public int? ItemChildCount;
        public int? FolderChildCount;
        public bool ItemPermissionChanged { get; set; }//当item的permission change时，设置为true      
        private List<AveSecurityObject> deleteRoleAssignments = null;//存放permission的删除事件
        public List<AveSecurityObject> DeleteRoleAssignments
        {
            get
            {
                if (deleteRoleAssignments == null)
                {
                    deleteRoleAssignments = new List<AveSecurityObject>(1);
                }
                return deleteRoleAssignments;
            }
            set
            {
                deleteRoleAssignments = value;
            }
        }
        /// <summary>
        /// 表示Role Assignments
        /// 
        /// 值可能有多值，不一定是单值
        /// </summary>
        public ChangeType RoleAssignmentsChangeType { get; set; }
        /// <summary>
        /// 表示Alert的改动
        /// 
        /// 值可能有多值，不一定是单值
        /// </summary>
        public ChangeType AlertChangeType 
        { 
            get 
            {
                if (mAlertObjs != null && mAlertObjs.Count > 0)
                {
                    return ChangeType.Edit;
                }
                return Wrapper.Common.ChangeType.None;
            } 
        }
        //TODO
        //public ChangeType WorkflowChangeType { get; set; }

        public void Dispose()
        {
            if (mStubAttachmentObjs != null)
            {
                this.mStubAttachmentObjs.Clear();
                this.mStubAttachmentObjs = null;
            }
            if (mAttachmentObjs != null)
            {
                this.mAttachmentObjs.Clear();
                this.mAttachmentObjs = null;
            }
            if (mVersionObjs != null)
            {
                this.mVersionObjs.Clear();
                this.mVersionObjs = null;
            }
            if (mSubFolderObjs != null)
            {
                this.mSubFolderObjs.Clear();
                this.mSubFolderObjs = null;
            }
            if (mSubItemObjs != null)
            {
                this.mSubItemObjs.Clear();
                this.mSubItemObjs = null;
            }
            if (mAlertObjs != null)
            {
                this.mAlertObjs.Clear();
                this.mAlertObjs = null;
            }
            if (mNoTypeDeleteItems != null)
            {
                this.mNoTypeDeleteItems.Clear();
                this.mNoTypeDeleteItems = null;
            }
            if (deleteRoleAssignments != null)
            {
                this.deleteRoleAssignments.Clear();
                this.deleteRoleAssignments = null;
            }
        }

        /// <summary>
        /// 外围调用清除cache
        /// </summary>
        public void ClearSubItemsCache()
        {
            if (mSubItemObjs != null)
            {
                this.mSubItemObjs.Clear();
                this.mSubItemObjs = null;
            }
        }

        /// <summary>
        /// 外围调用清除cache
        /// </summary>
        public void ClearSubFoldersCache()
        {
            if (mSubFolderObjs != null)
            {
                this.mSubFolderObjs.Clear();
                this.mSubFolderObjs = null;
            }
        }
    }

    public class ItemObjectDistinc : IEqualityComparer<AveItemObject>
    {
        public bool Equals(AveItemObject x, AveItemObject y)
        {
            return x.DocID == y.DocID;
        }
        
        public int GetHashCode(AveItemObject obj)
        {
            return obj.GetHashCode();
        }
    }

    public enum ItemType
    {
        UnKnow = 0,
        Item,
        Document, //hasStream
        Discussion,
        Folder,
        View,
        MicroFeedItem,
    }
}
