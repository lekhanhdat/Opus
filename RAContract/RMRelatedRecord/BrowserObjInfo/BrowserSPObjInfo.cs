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

namespace AvePoint.RA.Contract.RMRelatedRecord.BrowserObjInfo
{
    [DataContract]
    public class BrowserSPObjInfo
    {
        [DataMember]
        public Guid FolderId { get; set; }
        [DataMember]
        public Guid ListId { get; set; }
        [DataMember]
        public Guid WebId { get; set; }
        [DataMember]
        public int NodeLevel { get; set; }
        [DataMember]
        public string WebUrl { get; set; }//location url
        [DataMember] 
        public Guid id; 
        [DataMember]
        public string name;
        [DataMember]
        public string url;
        [DataMember]
        public List<BrowserSPObjInfo> children;
    }

    [DataContract]
    public class RecordsSiteBrowserInfo : BrowserSPObjInfo
    {
        [DataMember]
        public string Title;
        [DataMember]
        public string TemplateTitle;
        [DataMember]
        public string TemplateName;
        [DataMember]
        public uint Language;
    }

    [DataContract]
    public class RecordsWebBrowserInfo : BrowserSPObjInfo
    {
        [DataMember]
        public bool IsRootWeb;
        [DataMember]
        public string TemplateName;
        [DataMember]
        public string TemplateTitle;
        [DataMember]
        public uint Language;
        [DataMember]
        public string ServerRelativeUrl;
    }

    [DataContract]
    public class RecordsListBrowserInfo : BrowserSPObjInfo
    {
        [DataMember]
        public string Title;
        [DataMember]
        public string ServerRelativeUrl;
        [DataMember]
        public string WebServerRelativeUrl;
        [DataMember]
        public int BaseTemplate;
        [DataMember]
        public int BaseType;
        [DataMember]
        public bool Hidden;
        [DataMember]
        public string rootFolderName;
        [DataMember]
        public Guid parentWebId;
        [DataMember]
        public List<PageInfo> pageInfo;
    }

    [DataContract]
    public class RecordsFolderBrowserInfo : BrowserSPObjInfo
    {
        [DataMember]
        public string ServerRelativeUrl;
        [DataMember]
        public Guid ParentListId;
        [DataMember]
        public Guid RootFolderListId;
        [DataMember]
        public Guid ParentId;
        [DataMember]
        public bool Hidden;
        [DataMember]
        public int ParentListBaseType;
        [DataMember]
        public Guid parentWebId;
        [DataMember]
        public List<PageInfo> pageInfo;
    }
    [DataContract]
    public class RecordsItemBrowserInfo : BrowserSPObjInfo
    {
        [DataMember]
        public string DisplayName;
        [DataMember]
        public Guid UniqueId;
        [DataMember]
        public int ListBaseType;
        [DataMember]
        public string CurrentUIVersionString;
        [DataMember]
        public int LastModifier;
        [DataMember]
        public string LastModifierName;
        [DataMember]
        public DateTime LastModifyTime;// utc time
        [DataMember]
        public byte Level;
        [DataMember]
        public int DocLibRowId;
        [DataMember]
        public string Extension;
        [DataMember]
        public bool NeedDelete;
        [DataMember]
        public string SiteUrl { get; set; }
        [DataMember]
        public Guid SiteId { get; set; }
        [DataMember]
        public string WebServerRelativeUrl { get; set; }
        [DataMember]
        public string ListUrl { get; set; }
        [DataMember]
        public string FolderUrl { get; set; }

        //public Guid ItemId { get; set; }
        //public int DocLibRowId { get; set; }
        [DataMember]
        public string ItemUrl { get; set; }
        [DataMember]
        public bool ParentFolderIsRootFolder;
    }
    [DataContract]
    public class SPTreePage
    {
        [DataMember]
        public int? PageIndex { get; set; }
        [DataMember]
        public int? PageSize { get; set; }
        //待展开的NodeId
        [DataMember]
        public int NodeLevel { get; set; }
        [DataMember]
        public string WebUrl { get; set; }
        [DataMember]
        public Guid FolderId { get; set; }
        [DataMember]
        public Guid ListId { get; set; }
        [DataMember]
        public Guid WebId { get; set; }
        [DataMember]
        public List<BrowserSPObjInfo> infos { get; set; }
        [DataMember]
        public int ChildrenCount { get; set; }
        [DataMember]
        public string ServerRelativeUrl { get; set; }
        [DataMember]
        public List<PageInfo> pageInfo { get; set; }
    }
    [DataContract]
    public class PageInfo
    {
        [DataMember]
        public int pageIndex { get; set; }
        [DataMember]
        public string pageInfo { get; set; }
    }


}
