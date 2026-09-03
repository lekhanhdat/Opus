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

namespace AvePoint.GCommon.ComplianceDBWrapper.Model
{
    public class EDHoldItem
    {
        #region - 不论什么时候都需要的属性 -

        public string Name { get; set; }

        public ItemType ItemType { get; set; }

        #endregion - 不论什么时候都需要的属性 -

        #region - SharePoint Item操作的时候 -

        /// <summary>
        /// SharePoint的Hold Item有,其余的没有.
        /// </summary>
        public string FullPath { get; set; }

        public DateTime ModifiedTime { get; set; }

        #endregion - SharePoint Item操作的时候 -

        #region - 可选的属性 -

        public string ManagedBy { get; set; }

        public string Description { get; set; }

        #endregion - 可选的属性 -

        public Guid ID { get; set; }

        public Guid SPGuid { get; set; }

        private string _uniqueID;

        private const string ArchiveItemUniqueIDSuffix = "archiveItem";

        #region - UniqueID Build Rules -

        public string UniqueID
        {
            get
            {
                switch (ItemType)
                {
                    case ItemType.DocAveItem:
                        _uniqueID = (FarmID + Name).ToMD5();
                        break;
                    case ItemType.SharePointItem:
                        _uniqueID = (FarmID + SPGuid.ToString()).ToMD5();
                        break;
                    case ItemType.ArchiveItem:
                        _uniqueID = (FarmID + Name + ArchiveItemUniqueIDSuffix).ToMD5();
                        break;
                }
                return _uniqueID;
            }
            set
            {
                _uniqueID = value;
            }
        }

        #endregion - UniqueID Build Rules -

        private string _parentID;

        public string ParentID
        {
            get
            {
                if (ItemType != ItemType.DocAveItem)
                {
                    _parentID = GetDocAveHoldUniqueID();
                }
                return _parentID;
            }
            set { _parentID = value; }
        }

        public MarkState MarkState { get; set; }

        #region - Scope IDS -

        public string FarmID { get; set; }

        public Guid WebAppID { get; set; }

        public Guid SiteID { get; set; }

        public Guid WebID { get; set; }

        public Guid ListID { get; set; }

        #endregion - Scope IDS -

        /// <summary>
        /// 根据ED Hold Item,获得对应的DocAve Hold Item UniqueID.
        /// </summary>
        /// <returns>UniqueID</returns>
        public string GetDocAveHoldUniqueID()
        {
            return (FarmID + Name).ToMD5();
        }

        public override string ToString()
        {
            return
                string.Format(
                    @"Name:{0},ItemType:{1},FullPath:{2},Modified:{3},ManagedBy:{4},Description:{5},ID:{6},UniqueID:{7},ParentID:{8},MarkState:{9}",
                    Name, ItemType, FullPath, ModifiedTime, ManagedBy, Description, ID, UniqueID, ParentID,
                    MarkState);
        }

        public EDHoldItem GenerateDocAveHoldItem()
        {
            return new EDHoldItem
                    {
                        Name = Name,
                        FarmID = FarmID,
                        Description = Description,
                        ManagedBy = ManagedBy
                    };
        }
    }

    public enum ItemType
    {
        DocAveItem = 0,
        SharePointItem = 1,
        ArchiveItem = 2
    }
}