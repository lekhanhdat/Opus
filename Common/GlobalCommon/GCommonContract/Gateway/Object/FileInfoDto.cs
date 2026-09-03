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
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Gateway.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FileInfoDto
    {
        /// <summary>
        /// Id
        /// </summary>
        [DataMember]
        public string Id { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        [DataMember]
        public string FileName { get; set; }
        /// <summary>
        /// 图片
        /// </summary>
        [DataMember]
        public byte[] FileStream { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        [DataMember]
        public string Description { get; set; }
        /// <summary>
        /// 按钮名称
        /// </summary>
        [DataMember]
        public string Button { get; set; }
        /// <summary>
        /// 超链接
        /// </summary>
        [DataMember]
        public string Link { get; set; }
        /// <summary>
        /// SharePoint语言
        /// </summary>
        /// <returns></returns>
        [DataMember]
        public SPLanguage Culture { get; set; }
        public override string ToString()
        {
            return string.Format("FileInfoDto[Id {0}, FileName {1}]", Id, FileName);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SPLanguage
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        English = 1,
        [EnumMember]
        Japanese = 2,
    }
}
