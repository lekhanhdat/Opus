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

namespace AvePoint.GCommon.Contract.Media.Object
{
    #region == using directives ==
    using System;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion ==

    /// <summary>Media数据扩展设置.</summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRootAttribute(ElementName = "DataVersionContent")]
    public class DataVersionContentDto
    {
        [DataMember]
        [XmlAttribute("PlatformType")]
        public PlatformType Type { get; set; }

        [DataMember]
        [XmlAttribute("ProductVersion")]
        public ProductVersion Version { get; set; }

        /// <summary>
        /// ProductVersion并不能代表数据格式详细版本,添加此属性表示每次release后数据大版本,如6.0、6.1、6.2等.
        /// </summary>
        [DataMember]
        public string DataReleaseVersion { get; set; }

        /// <summary>
        /// Reresent last upgrading imported backup data time.
        /// </summary>
        [DataMember]
        public Int64 LastImportedTime { get; set; }


        public override String ToString()
        {
            return String.Format("Type: {0}, Version: {1}",
                this.Type,
                this.Version);
        }
    }
}
