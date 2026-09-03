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
using System.Runtime.Serialization;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.LanguageMapping.Object.LanguageXmlDto
{
    
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class LanguageMappingItemDto
    {
        /// <summary>
        /// List 或者 Column
        /// </summary>
        [DataMember]
        [XmlAttribute("spType")]
        public String spType { set; get; }
        /// <summary>
        /// 标识是column的第几行或者是List的第几行
        /// Column0 或者 List0
        /// </summary>
        [DataMember]
        [XmlAttribute("sourceName")]
        public String sourceName { set; get; }
        /// <summary>
        /// 前台输入的字符换比如aaa
        /// </summary>
        [DataMember]
        [XmlAttribute("destName")]
        public String destName { set; get; }
        /// <summary>
        /// 是语言名称，例如English
        /// </summary>
        [DataMember]
        [XmlAttribute("destLanguage")]
        public String destLanguage { set; get; }
        /// <summary>
        /// 语言对应的数字,例如英语对应1033
        /// </summary>
        [DataMember]
        [XmlAttribute("destLanguageId")]
        public String destLanguageId { set; get; }
    }
}
