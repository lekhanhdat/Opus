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
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.ObjectModel.O365
{
    internal class AveClientObjectsCache : IDisposable
    {
        private bool dirty = false;
        public uint SiteMaxItemsPerThrottleOperation;
        /// <summary>
        /// current discover list Id (string.ToUpper)
        /// </summary>
        public Guid ListId;
        /// <summary>
        /// current discover list Title
        /// </summary>
        public string ListTitle;

        public bool ExceedListViewThreshold = false;

        public bool FolderLoaded = false;

        public bool Loaded = false;
        ///// <summary>
        ///// current discover list
        ///// </summary>
        //public List List;
        /// <summary>
        /// saved items(listitems, folder, files) in current list
        /// </summary>
        public Dictionary<int, Dictionary<string, object>> Items;
        /// <summary>
        /// saved files in current list
        /// </summary>
        public Dictionary<string, Dictionary<string, object>> Files;
        /// <summary>
        /// saved folders in current list
        /// </summary>
        //public Dictionary<string, Dictionary<string, object>> Folders;
        public Dictionary<string, IList<int>> FoldersToSubItemIds;
        public Dictionary<string, IList<string>> FoldersToSubFiles;
        public Dictionary<string, IList<string>> FoldersToSubFolders;
        //public Dictionary<string, int> FoldersToItemIds;
        public Dictionary<string, IList<string>> FoldersToSubItemUniqueIds;
        public Dictionary<string, Dictionary<string, DateTime>> FoldersToSubItemLastAccessTime;

        public AveFolderPageInfo FolderPageInfo;

        public bool IsEmpty
        {
            get
            {
                return //this.Folders.Count == 0 &&
                     this.Files.Count == 0
                    && this.Items.Count == 0
                    //&& this.List == null
                    && this.ListId == Guid.Empty
                    && string.IsNullOrEmpty(this.ListTitle);
            }
        }
        public static AveClientObjectsCache NewCache
        {
            get
            {
                AveClientObjectsCache value = new AveClientObjectsCache();
                value.SiteMaxItemsPerThrottleOperation = 0;
                value.ListId = Guid.Empty;
                value.ListTitle = string.Empty;
                //value.List = null;
                value.dirty = false;
                value.Items = new Dictionary<int, Dictionary<string, object>>();
                value.Files = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);
                //value.Folders = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);
                //value.FoldersToItemIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                value.FoldersToSubItemIds = new Dictionary<string, IList<int>>(StringComparer.OrdinalIgnoreCase);
                value.FoldersToSubFiles = new Dictionary<string, IList<string>>(StringComparer.OrdinalIgnoreCase);
                value.FoldersToSubFolders = new Dictionary<string, IList<string>>(StringComparer.OrdinalIgnoreCase);
                value.FolderPageInfo = new AveFolderPageInfo() { QueryRange = 4999};
                value.FoldersToSubItemUniqueIds = new Dictionary<string, IList<string>>(StringComparer.OrdinalIgnoreCase);
                value.FoldersToSubItemLastAccessTime = new Dictionary<string, Dictionary<string, DateTime>>();
                return value;
            }
        }
        #region IDisposable Members
        public void Clear()
        {
            this.SiteMaxItemsPerThrottleOperation = 0;
            this.ListId = Guid.Empty;
            this.ListTitle = string.Empty;
            this.Loaded = false;
            this.FolderLoaded = false;
            this.ExceedListViewThreshold = false;
            //this.List = null;
            this.dirty = false;
            this.Items.Clear();
            this.Files.Clear();
            //this.Folders.Clear();
            //this.FoldersToItemIds.Clear();
            this.FoldersToSubItemIds.Clear();
            this.FoldersToSubItemUniqueIds.Clear();
            this.FoldersToSubItemLastAccessTime.Clear();
        }
        public void Dispose()
        {
            this.SiteMaxItemsPerThrottleOperation = 0;
            this.ListId = Guid.Empty;
            this.ListTitle = string.Empty;
            //this.List = null;
            this.dirty = false;
            if (this.Items != null)
                this.Items.Clear();
            if (this.Files != null)
                this.Files.Clear();
            //if (this.Folders != null)
            //    this.Folders.Clear();
            //if (this.FoldersToItemIds != null)
            //    this.FoldersToItemIds.Clear();
            if (this.FoldersToSubItemUniqueIds != null)
                this.FoldersToSubItemUniqueIds.Clear();
            if (this.FoldersToSubItemLastAccessTime != null)
                this.FoldersToSubItemLastAccessTime.Clear();
        }
        #endregion
    }

    internal class AveFolderPageInfo
    {
        public string ServerRelativeUrl { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public int SurplusCount { get; set; }
        public int QueryRange { get; set; }

        public Stopwatch QueryTimer = new Stopwatch();
    }
}
