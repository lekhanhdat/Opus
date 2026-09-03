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

namespace AvePoint.GCommon.Contract.Server.Common
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot(ElementName = "I18NMessageDto")]
    public class I18NMessageDto
    {
        [DataMember]
        [XmlAttribute]
        public string Key { get; set; }
        [DataMember]
        [XmlAttribute]
        public string Value { get; set; }
        [DataMember]
        [XmlAttribute]
        public string EnValue { get; set; }
        [DataMember]
        [XmlAttribute]
        public string JaValue { get; set; }
        [DataMember]
        [XmlAttribute]
        public string FrValue { get; set; }
        [DataMember]
        [XmlAttribute]
        public string DeValue { get; set; }
        [DataMember]
        [XmlAttribute]
        public string ZhValue { get; set; }
        [DataMember]
        [XmlAttribute]
        public string ModifyColunm { get; set; }
        [DataMember]
        [XmlAttribute]
        public Dictionary<string, string> CultureValue { get; set; }

        public string GetI18NMessage(string culture = "en")
        {
            if (!string.IsNullOrEmpty(this.Value))
            {
                return this.Value;
            }
            string value = null;
            if (string.IsNullOrEmpty(culture))
            {
                culture = "en";
            }
            string tempCulture = culture;
            int index = tempCulture.IndexOf("-", StringComparison.OrdinalIgnoreCase);
            if (index != -1)
            {
                tempCulture = culture.Substring(0, index);
            }
            switch (tempCulture.ToLower())
            {
                case "en":
                    value = this.EnValue;
                    break;
                case "ja":
                    value = this.JaValue;
                    break;
                default:
                    if (this.CultureValue != null && this.CultureValue.ContainsKey(culture))
                    {
                        value = this.CultureValue[culture];
                    }
                    else
                    {
                        value = this.EnValue;
                    }
                    break;
            }
            return value;
        }

        public void SetI18NMessage(string culture, string value)
        {
            if (string.IsNullOrEmpty(culture))
            {
                culture = "en";
            }
            string tempCulture = culture;
            int index = tempCulture.IndexOf("-", StringComparison.OrdinalIgnoreCase);
            if (index != -1)
            {
                tempCulture = culture.Substring(0, index);
            }
            switch (tempCulture.ToLower())
            {
                case "en":
                    this.EnValue = value;
                    break;
                case "de":
                    this.DeValue = value;
                    break;
                case "fr":
                    this.FrValue = value;
                    break;
                case "ja":
                    this.JaValue = value;
                    break;
                case "zh":
                    this.ZhValue = value;
                    break;
                default:
                    if (this.CultureValue == null)
                    {
                        this.CultureValue = new Dictionary<string, string>();
                    }
                    this.CultureValue[culture] = value;
                    break;
            }
        }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum I18NMode
    {
        /// <summary>
        /// 默认从数据库中存储国际化文件
        /// </summary>
        [EnumMember]
        Default = 0,
        /// <summary>
        /// 从Xml中读取国际化资源文件
        /// </summary>
        [EnumMember]
        Xml,
        /// <summary>
        /// 从Resource中读取国际化资源文件
        /// </summary>
        [EnumMember]
        Resource,
    }
}