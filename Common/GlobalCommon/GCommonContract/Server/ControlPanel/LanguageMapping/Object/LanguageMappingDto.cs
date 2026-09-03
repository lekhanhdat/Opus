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
using System.Runtime.Serialization;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.LanguageMapping.Object.LanguageXmlDto;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.LanguageMapping.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot(ElementName = "LanguageMapping")]
    public class LanguageMappingDto : IProfileContent
    {
        public LanguageMappingDto()
        {
            languages = new List<LanguageMappingItemDto>();
        }

        [XmlAttribute("mappingId")]
        [DataMember]
        public string mappingId { set; get; }
        /// <summary>
        /// 界面上的所有mapping
        /// </summary>
        [XmlArrayItem("language")]
        [DataMember]
        public List<LanguageMappingItemDto> languages { get; set; }
        /// <summary>
        /// Mapping 名字
        /// </summary>
        [XmlAttribute("mappingName")]
        [DataMember]
        public String mappingName { set; get; }
        /// <summary>
        /// mapping的描述
        /// </summary>
        [XmlAttribute("description")]
        [DataMember]
        public String description { set; get; }
        /// <summary>
        /// 标识了是否使用了languageMapping给client发消息的时候使用,不需要保存到数据库中。
        /// </summary>
        [XmlIgnore]
        [DataMember]
        public Boolean useLanguageMapping { set; get; }
        /// <summary>
        /// 源端语言
        /// </summary>
        [DataMember]
        public String sourceLanguage { get; set; }
        /// <summary>
        /// 目的端语言
        /// </summary>
        [DataMember]
        public String targetLangugae { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        [DataMember]
        public long modifiedTime { get; set; }
    }
}
