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



using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace AvePoint.GCommon.Contract.DeploymentManager.Object
{
    [DataContract]
    [XmlRootAttribute("DashBoardLocationDto")]
    public class DashBoardLocationDto
    {
        [DataMember]
        [XmlAttribute("name")]
        public string Name { get; set; }

        [DataMember]
        [XmlAttribute("id")]
        public string Id { set; get; }
        /// <summary>
        /// 每个模块所用的空间
        /// </summary>
        [DataMember]
        [XmlAttribute("categoryusespace")]
        public float CategoryUseSpace { get; set; }

        /// <summary>
        /// 每个模块所用的空间
        /// </summary>
        [DataMember]
        [XmlAttribute("gbcategoryusespace")]
        public string GBCategoryUseSpace { get; set; }

        /// <summary>
        /// 其他应用空间
        /// </summary>
        [DataMember]
        [XmlAttribute("otherusespace")]
        public float OtherUseSpace { get; set; }

        /// <summary>
        /// 其他应用空间
        /// </summary>
        [DataMember]
        [XmlAttribute("gbotherusespace")]
        public string GBOtherUseSpace { get; set; }

        /// <summary>
        /// 剩余空间
        /// </summary>
        [DataMember]
        [XmlAttribute("remainingspace")]
        public float RemainingSpace { get; set; }

        /// <summary>
        /// 剩余空间
        /// </summary>
        [DataMember]
        [XmlAttribute("gbremainingspace")]
        public string GBRemainingSpace { get; set; }

        /// <summary>
        /// 总空间
        /// </summary>
        [DataMember]
        [XmlAttribute("totalspace")]
        public float TotalSpace { get; set; }

        /// <summary>
        /// 总空间
        /// </summary>
        [DataMember]
        [XmlAttribute("gbtotalspace")]
        public string GBTotalSpace { get; set; }

        /// <summary>
        /// 存储Location的类型
        /// </summary>
        [DataMember]
        [XmlAttribute("type")]
        public LocationType Type { get; set; }
    }
    [DataContract]
    [XmlRootAttribute("LocationType")]
    public enum LocationType : int
    {
        [EnumMember]
        ExportLocation = 0,
        [EnumMember]
        StoragePolicy = 1
    }
}

