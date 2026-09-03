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
using System.Text;

namespace AvePoint.Wrapper.Common
{
    [Serializable]
    public class AveWebPartPostActionInfo
    {
        public Guid WebPartId;
        public int UserId = -1;
    }

    [Serializable]
    public class AveWebPartBaseInfo
    {
        public Guid ID;
        public Guid OriginalListId;
        public Guid ListId;
        public string ListTitle;
        public Nullable<byte> Type;
        public int Flags;
        public Nullable<Int16> BaseViewID;
        public string DisplayName;
        public int Version;
        public int PartOrder;
        public string ZoneID;
        public bool IsIncluded;
        public byte FrameState;
        public byte[] View;
        public Guid WebPartTypeId;
        public byte[] AllUsersProperties;
        public byte[] PerUserProperties;
        public byte[] Cache;
        public int UserID;
        public string Source;
        public DateTime CreateTime;
        public long Size;
        public byte Level;
        public bool Deleted;
        public bool HasFGP;
        public byte[] ContentTypeId;

        // add for XslListViewWebPart
        public string TitleUrl;
        public string DetailLink;

        #region new in SP 14
        public bool IsCurrentVersion;
        public int PageVersion;
        public Guid SolutionId;
        public string WebPartIdProperty;
        public string Assembly;
        public string Class;
        #endregion

        public string DefinitionXml;

        //ADO-160937 add for Access Requests XslListViewWebPart
        //对应WebPart上的 XmlDefinition Property
        public string XmlDefinition;

        public bool IsViewBuildInWebPart;

        public List<AvePersonalizationInfo> Personalization;
        public List<AveWebPartListInfo> WebPartList;
        //Office 365 Properties 存在 DicAllUserPerUserPros 中 
        //对应Local AllUsersProperties and PerUserProperties中的数据
        //Replicator  的一个offline 特殊功能也将local的属性存在这里
        public Dictionary<string, object> DicAllUserPerUserPros;
        //For SP10，重构之后需要去掉
        //used for restore view id existed in alluserproperties, for instance slideshowwebpart
        public Guid ViewGuid;
        //For SP10，重构之后需要去掉
        //we have different way to handle restoring webpart in postaction whether the webpart has been created before.
        public bool IsCreated = false;
        public Dictionary<string, string> ExtensionProperties { get; set; }
    }

    [Serializable]
    public class AvePersonalizationInfo
    {
        public int UserID;//[tp_UserID] [int] NOT NULL,
        public Nullable<int> PartOrder = new Nullable<int>();//[tp_PartOrder] [int] NULL,
        public string ZoneID;//[tp_ZoneID] [nvarchar](64) NULL,
        public bool IsIncluded;//[tp_IsIncluded] [bit] NOT NULL,
        public byte FrameState;//[tp_FrameState] [tinyint] NOT NULL,
        public byte[] PerUserProperties;//[tp_PerUserProperties] [varbinary](max) NULL,
        public byte[] Cache;//[tp_Cache] [varbinary](max) NULL,
        public long Size;//[tp_Size] [bigint] NOT NULL,
        public bool Deleted;//[tp_Deleted] [bit] NOT NULL,
    }

    [Serializable]
    public class AveWebPartListInfo
    {
        public Guid WebId;
        public string FullUrl;
        public Nullable<int> UserID = new Nullable<int>();//[tp_UserID] [int] NULL,
        public byte Level;//[tp_Level] [tinyint] NOT NULL
    }

}