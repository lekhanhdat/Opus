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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.AveLicense;


namespace AvePoint.GCommon.Contract.Server.ControlPanel.SystemSetting.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SystemSettingContent : ISystemSettingContent
    {
        [DataMember]
        [XmlAttribute]
        [Obsolete("不提倡使用该属性")]
        public string UserName { get; set; }

        [DataMember]
        [XmlAttribute]
        [Obsolete("不提倡使用该属性")]
        public string Password { get; set; }

        [DataMember]
        [XmlAttribute]
        [Obsolete("不提倡使用该属性")]
        public string JobReportLocation { get; set; }

        [DataMember]
        [XmlAttribute]
        public bool UseBrowserLanguage { get; set; }

        [DataMember]
        [XmlAttribute]
        [Obsolete("不提倡使用该属性")]
        public LanguageType Language { get; set; }

        [DataMember]
        [XmlAttribute]
        public LanguageDto DisplayLanguage { get; set; }

        [DataMember]
        [XmlAttribute]
        public LanguageDto AdjustLanguage { get; set; }

        [DataMember]
        [XmlAttribute]
        public string Locale { get; set; }

        [DataMember]
        [XmlAttribute]
        public string DateFormat { get; set; }

        [DataMember]
        [XmlAttribute]
        public string TimeFormat { get; set; }

        [DataMember]
        [XmlAttribute]
        public Dictionary<string, string> DisplayFarm { get; set; }

        [DataMember]
        [XmlAttribute]
        public TranslationEngine TranslationEngine { get; set; }

        [DataMember]
        [XmlAttribute]
        public Credential Credential { get; set; }

        [DataMember]
        [XmlAttribute]
        public LogoImage LogoImage { get; set; }

        [DataMember]
        [XmlAttribute]
        public SettingStatus NetAppStatus { get; set; }

        [DataMember]
        [XmlAttribute]
        public ProductType ProductType { get; set; }


    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Credential
    {
        [DataMember]
        [XmlAttribute]
        public string BingAppId { get; set; }

        [DataMember]
        [XmlAttribute]
        public string GoogleApiKey { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SystemSettingForDisplay
    {
        [DataMember]
        public Dictionary<string, LocaleValue> DataType { get; set; }
        [DataMember]
        public Dictionary<string, string> DisplayFarm { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class LanguageDto
    {
        [DataMember]
        public string LanguageName { get; set; }
        [DataMember]
        public string Culture { get; set; }
        [DataMember]
        public LanguageStatus Status { get; set; }
        [DataMember]
        public string Logs { get; set; }
        [DataMember]
        public List<I18NMessageDto> MessageDtos { get; set; }
        [DataMember]
        public I18NMode I18NMode { get; set; }

        public override string ToString()
        {
            return this.LanguageName;
        } 
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum LanguageStatus
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Default,
        [EnumMember]
        Downloading,
        [EnumMember]
        Downloaded,
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Locale
    {
        public const string ARABIC = "Arabic";                         //阿拉伯语

        public const string CHINESE_PRC = "Chinese(PRC)";             //中国

        public const string CHINESE_T = "Chinese(Taiwan)";            //中国（台湾）

        public const string CZECH = "Czech";                          //捷克语

        public const string DANISH = "Danish";                         //丹麦语

        public const string ENGLISH_US = "English(United States)";     //英语（美国）

        public const string ENGLISH_UK = "English(United Kingdom)";    //英语（英国）

        public const string ENGLISH_A = "English(Australia)";          //英语（澳大利亚）

        public const string ENGLISH_C = "English(Canada)";            //英语（加拿大）

        public const string ENGLISH_S = "English(South Africa)";       //英语（南非）

        public const string FILIPINO = "Filipino";                     //菲律宾语

        public const string FINNISH = "Finnish";                     //芬兰语

        public const string FRENCH_FRANCE = "French";                   //法语

        public const string GERMAN_GERMANY = "German";                  //德语

        public const string ITALIAN_ITALY = "Italian";                  //意大利语

        public const string JAPANESE = "Japanese";                     //日本

        public const string MALAY = "Malay";                           //马来语

        public const string POLISH = "Polish";                         //波兰语

        public const string PORTUGUESE_PORTUGAL = "Portuguese";         //葡萄牙语

        public const string ROMANIAN = "Romanian";                     //罗马尼亚语

        public const string RUSSIAN = "Russian";                       //俄语

        public const string SPANISH_SPAIN = "Spanish";                  //西班牙语

        public const string SWEDISH = "Swedish";                       //瑞典语

        public const string THAI = "Thai";                             //泰国语

        public const string TURKISH = "Turkish";                       //土耳其语

        public const string UKRAINIAN = "Ukrainian";                   //乌克兰语

        public const string VIETNAMESE = "Vietnamese";                //越南语
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class LocaleValue
    {
        [DataMember]
        public List<string> DateList { get; set; }

        [DataMember]
        public List<string> TimeList { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum LanguageType
    {
        [EnumMember]
        Default = 0,
        [EnumMember]
        English = 1,
        //[EnumMember]
        //German = 2,
        //[EnumMember]
        //French = 3,
        //[EnumMember]
        //Japanese=4,

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SystemSettingResult
    {
        [EnumMember]
        NullParam = 0,
        [EnumMember]
        ActionSuccessfull,
        [EnumMember]
        ActionSuccessfull_NeedReloadPage,
        [EnumMember]
        ActionFailed,
        [EnumMember]
        ActionFailed_DeleteUsingLanguageError,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum TranslationEngine
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Google,
        [EnumMember]
        Bing,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class LogoImage
    {
        [DataMember]
        public byte[] ImageData { get; set; }
        [DataMember]
        public double Width { get; set; }
        [DataMember]
        public double Height { get; set; }
        [DataMember]
        public double X { get; set; }
        [DataMember]
        public double Y { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SettingStatus
    {
        [EnumMember]
        Off = 0,
        [EnumMember]
        On,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SettingStatusDto
    {
        [DataMember]
        public SettingStatus SettingStatus { get; set; }
        /// <summary>
        /// 如果此值为Failed，请为ErrorMessage属性赋值，以便前台显示
        /// </summary>
        [DataMember]
        public ActionResult ActionResult { get; set; }
        /// <summary>
        /// 此值需要国际化
        /// </summary>
        [DataMember]
        public string ErrorMessage { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ActionResult
    {
        [EnumMember]
        Failed = 0,
        [EnumMember]
        Successful,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum FileExtension
    {
        [EnumMember]
        JPG = 255216,
        [EnumMember]
        GIF = 7173,
        [EnumMember]
        PNG = 13780,
        [EnumMember]
        BMP = 6677,
        [EnumMember]
        SWF = 6787,
        [EnumMember]
        RAR = 8297,
        [EnumMember]
        ZIP = 8075,
        [EnumMember]
        _7Z = 55122,
        [EnumMember]
        VALIDFILE = 9999999
    }

    #region Security Mode

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ControlSecurityModeSetting
    {
        [DataMember]
        public ControlSecurityMode ControlSecurityMode { get; set; }

        [DataMember]
        public bool IsShowProductVersion { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ControlSecurityMode
    {
        [EnumMember]
        EncryptMessage = 0, //默认加密模式
        [EnumMember]
        None = 1,
    }
    #endregion
}

