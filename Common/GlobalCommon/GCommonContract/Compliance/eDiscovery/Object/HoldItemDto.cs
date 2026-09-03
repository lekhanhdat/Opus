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



using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Compliance.eDiscovery.Object.HoldManager;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.SharePointBrowser.Object;

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class HoldItemDto
    {
        public HoldItemDto()
        {
            Children = new List<HoldItemDto>();
        }

        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public FarmDto Farm { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }

        #region 废弃
        [DataMember]
        public string ManagedBy { get; set; }
        #endregion

        [DataMember]
        public long HeldCount { get; set; }
        [DataMember]
        public string Location { get; set; }
        [DataMember]
        public long LastModifiedTime { get; set; }
        [DataMember]
        public HoldItemType HoldItemType { get; set; }
        [DataMember]
        public List<HoldItemDto> Children { get; set; }
        [DataMember]
        public int ChildrenCount { get; set; }
        [DataMember]
        public List<HeldFileDto> HoldFiles { get; set; }
        [DataMember]
        public string IdInSharePoint { get; set; }
        [DataMember]
        public string WebAppId { get; set; }
        [DataMember]
        public string SiteCollectionId { get; set; }
        [DataMember]
        public string WebId { get; set; }
        [DataMember]
        public string ListId { get; set; }
        [DataMember]
        public int MarkState { get; set; }
        [DataMember]
        public string ParentID { get; set; }

        [DataMember]
        public UserDetail UserDetail { get; set; }

        /// <summary>
        /// 给Release操作用的属性
        /// </summary>
        [DataMember]
        public List<ReleaseHoldFile> ReleaseFiles { get; set; }

    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ManagedByXmlObj
    {

        public ManagedByXmlObj()
        {

        }

        [DataMember]
        [XmlAttribute]
        public string DisplayName { get; set; }

        [DataMember]
        [XmlAttribute]
        public string DomainId { get; set; }

        [DataMember]
        [XmlAttribute]
        public string LoginName { get; set; }

        [DataMember]
        [XmlAttribute]
        public int AccountType { get; set; }

    }



    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum HoldItemType
    {
        [EnumMember]
        DocAve = 0,
        [EnumMember]
        SharePoint = 1,
        [EnumMember]
        Archive = 2
    }
}
