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
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RAFileSystem.SharePoint.Common
{
    [Serializable]
    public class RMRelatedItemInfo
    {
        #region
        public string AveId { get; set; }//from DocAve Register sites
        public int DocLibRowId { get; set; }
        public string name { get; set; }
        public bool NeedDelete { get; set; }
        public string url { get; set; }
        public Guid id { get; set; }//ItemUniqueId
        #endregion
        public string SiteUrl { get; set; }
        public Guid SiteId { get; set; }
        public string WebUrl { get; set; }
        public Guid WebId { get; set; }
        public string WebServerRelativeUrl { get; set; }
        public Guid ListId { get; set; }
        public string ListUrl { get; set; }
        public Guid FolderId { get; set; }
        public string FolderUrl { get; set; }

        //public Guid ItemId { get; set; }
        //public int DocLibRowId { get; set; }
        public string ItemUrl { get; set; }
        public bool ParentFolderIsRootFolder { get; set; }
        public SOEndUserArchiverNodeLevel level { get; set; }
        public int SourceFlag { get; set; }
        public string recId { get; set; } //sp page do not show phy obj
        public int NodeType { get; set; }
    }
    public enum SOEndUserArchiverNodeLevel
    {
        None = 0,
        Site = 1,
        Web = 2,
        List = 3,
        Folder = 4,
        Item = 5,
        Document = 6,
        Multifiles = 7,
        Attachment = 8
    }


    [DataContract]
    public class RelatedItemSubmit
    {
        [DataMember]
        public RelatedItemSubmitInfo CurrentInfo { get; set; }

        [DataMember]
        public List<RelatedItemSubmitInfo> RelatedInfos { get; set; }
    }


    [DataContract]
    public class RelatedItemSubmitInfo
    {
        [DataMember]
        public Guid ListId { get; set; }

        [DataMember]
        public Guid WebId { get; set; }

        [DataMember]
        public Guid UniqueId { get; set; }

        [DataMember]
        public int ListItemId { get; set; }

        [DataMember]
        public string SiteUrl { get; set; }

        [DataMember]
        public Guid SiteId { get; set; }

        [DataMember]
        public bool NeedDelete { get; set; }
    }
}
