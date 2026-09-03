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
using System.Text;
using System.Xml.Serialization;

namespace RAExportCommon
{
    [XmlRoot("FileVEOXML")]
    public class FileVEOXML
    {
        [XmlElement("M1")]
        public FileVEO_M1_VERSEncapsulatedObject M1;
    }

    [XmlRoot("M1")]
    public class FileVEO_M1_VERSEncapsulatedObject
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
        [XmlElement("M2")]
        public FileVEO_M2_VEOFormatDescription M2;
        [XmlElement("M3")]
        public FileVEO_M3_Version M3;
        [XmlElement("M4")]
        public FileVEO_M4_SignedObject M4;
        [XmlElement("M134")]
        public List<FileVEO_M134_SignatureBlock> M134 = new List<FileVEO_M134_SignatureBlock>();
        [XmlElement("M152")]
        public FileVEO_M152_LockSignatureBlock M152;
    }

    [XmlRoot("M2")]
    public class FileVEO_M2_VEOFormatDescription
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M3")]
    public class FileVEO_M3_Version
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M4")]
    public class FileVEO_M4_SignedObject
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
        [XmlElement("M5")]
        public FileVEO_M5_ObjectMetadata M5;
        [XmlElement("M9")]
        public FileVEO_M9_ObjectContent M9;
    }

    [XmlRoot("M5")]
    public class FileVEO_M5_ObjectMetadata
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
        [XmlElement("M6")]
        public FileVEO_M6_ObjectType M6;
        [XmlElement("M7")]
        public FileVEO_M7_ObjectTypeDescription M7;
        [XmlElement("M8")]
        public FileVEO_M8_ObjectCreationDate M8;
    }

    [XmlRoot("M6")]
    public class FileVEO_M6_ObjectType
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M7")]
    public class FileVEO_M7_ObjectTypeDescription
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M8")]
    public class FileVEO_M8_ObjectCreationDate
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M9")]
    public class FileVEO_M9_ObjectContent
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
        [XmlElement("M142")]
        public FileVEO_M142_File M142;
    }

    [XmlRoot("M12")]
    public class FileVEO_M12_Agent
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
        [XmlElement("M13")]
        public List<FileVEO_M13_AgentType> M13 = new List<FileVEO_M13_AgentType>();
        [XmlElement("M16")]
        public List<FileVEO_M16_CorporateName> M16 = new List<FileVEO_M16_CorporateName>();

        #region 非常用column
        [XmlElement("M14")]
        public List<FileVEO_M14_Jurisdiction> M14 = new List<FileVEO_M14_Jurisdiction>();
        [XmlElement("M15")]
        public FileVEO_M15_CorporateId M15;
        [XmlElement("M17")]
        public FileVEO_M17_PersonId M17;
        [XmlElement("M18")]
        public List<FileVEO_M18_PersonalName> M18 = new List<FileVEO_M18_PersonalName>();
        [XmlElement("M19")]
        public List<FileVEO_M19_SectionName> M19 = new List<FileVEO_M19_SectionName>();
        [XmlElement("M20")]
        public List<FileVEO_M20_PositionName> M20 = new List<FileVEO_M20_PositionName>();
        [XmlElement("M21")]
        public List<FileVEO_M21_ContactDetails> M21 = new List<FileVEO_M21_ContactDetails>();
        [XmlElement("M22")]
        public List<FileVEO_M22_Email> M22 = new List<FileVEO_M22_Email>();
        [XmlElement("M23")]
        public List<FileVEO_M23_DigitalSignature> M23 = new List<FileVEO_M23_DigitalSignature>();
        #endregion
    }

    [XmlRoot("M13")]
    public class FileVEO_M13_AgentType
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M14")]
    public class FileVEO_M14_Jurisdiction
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M15")]
    public class FileVEO_M15_CorporateId
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M16")]
    public class FileVEO_M16_CorporateName
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M17")]
    public class FileVEO_M17_PersonId
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M18")]
    public class FileVEO_M18_PersonalName
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M19")]
    public class FileVEO_M19_SectionName
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M20")]
    public class FileVEO_M20_PositionName
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M21")]
    public class FileVEO_M21_ContactDetails
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M22")]
    public class FileVEO_M22_Email
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M23")]
    public class FileVEO_M23_DigitalSignature
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M24")]
    public class FileVEO_M24_RightsManagement
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
        [XmlElement("M25")]
        public FileVEO_M25_SecurityClassification M25;

        #region 非常用column
        [XmlElement("M26")]
        public List<FileVEO_M26_Caveat> M26 = new List<FileVEO_M26_Caveat>();
        [XmlElement("M27")]
        public List<FileVEO_M27_Codeword> M27 = new List<FileVEO_M27_Codeword>();
        [XmlElement("M28")]
        public List<FileVEO_M28_ReleasabilityIndicator> M28 = new List<FileVEO_M28_ReleasabilityIndicator>();
        [XmlElement("M29")]
        public FileVEO_M29_AccessStatus M29;
        [XmlElement("M30")]
        public List<FileVEO_M30_UsageCondition> M30 = new List<FileVEO_M30_UsageCondition>();
        [XmlElement("M31")]
        public FileVEO_M31_EncryptionDetails M31;
        #endregion

    }

    [XmlRoot("M25")]
    public class FileVEO_M25_SecurityClassification
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M26")]
    public class FileVEO_M26_Caveat
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M75")]
    public class FileVEO_M27_Codeword
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M28")]
    public class FileVEO_M28_ReleasabilityIndicator
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M29")]
    public class FileVEO_M29_AccessStatus
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M30")]
    public class FileVEO_M30_UsageCondition
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M31")]
    public class FileVEO_M31_EncryptionDetails
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("MExtensionContextPath")]
    public class FileVEO_MExtension_ContextPath
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
        [XmlElement("MExtensionContextPathDomain")]
        public FileVEO_MExtension_ContextPathDomain MExtensionContextPathDomain;
        [XmlElement("MExtensionContextPathValue")]
        public FileVEO_MExtension_ContextPathValue MExtensionContextPathValue;
    }

    [XmlRoot("MExtensionContextPathDomain")]
    public class FileVEO_MExtension_ContextPathDomain
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("MExtensionContextPathValue")]
    public class FileVEO_MExtension_ContextPathValue
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M32")]
    public class FileVEO_M32_Title
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
        [XmlElement("M33")]
        public List<FileVEO_M33_SchemeType> M33 = new List<FileVEO_M33_SchemeType>();
        [XmlElement("M34")]
        public FileVEO_M34_SchemeName M34;
        [XmlElement("M35")]
        public FileVEO_M35_TitleWords M35;

        #region 非常用column
        [XmlElement("M36")]
        public List<FileVEO_M36_Alternative> M36 = new List<FileVEO_M36_Alternative>();
        #endregion
    }

    [XmlRoot("M33")]
    public class FileVEO_M33_SchemeType
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M34")]
    public class FileVEO_M34_SchemeName
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M35")]
    public class FileVEO_M35_TitleWords
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M36")]
    public class FileVEO_M36_Alternative
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M37")]
    public class FileVEO_M37_Subject
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;

        #region 非常用column
        [XmlElement("M38")]
        public FileVEO_M38_KeywordLevel M38;
        [XmlElement("M39")]
        public List<FileVEO_M39_Keyword> M39 = new List<FileVEO_M39_Keyword>();
        #endregion
    }

    [XmlRoot("M38")]
    public class FileVEO_M38_KeywordLevel
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M39")]
    public class FileVEO_M39_Keyword
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M40")]
    public class FileVEO_M40_Description
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M41")]
    public class FileVEO_M41_Language
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M42")]
    public class FileVEO_M42_Relation
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;

        #region 非常用column
        [XmlElement("M43")]
        public List<FileVEO_M43_RelatedItemId> M43 = new List<FileVEO_M43_RelatedItemId>();
        [XmlElement("M44")]
        public List<FileVEO_M44_RelationType> M44 = new List<FileVEO_M44_RelationType>();
        [XmlElement("M45")]
        public List<FileVEO_M45_RelationDescription> M45 = new List<FileVEO_M45_RelationDescription>();
        #endregion
    }

    [XmlRoot("M43")]
    public class FileVEO_M43_RelatedItemId
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M44")]
    public class FileVEO_M44_RelationType
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M45")]
    public class FileVEO_M45_RelationDescription
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M46")]
    public class FileVEO_M46_Coverage
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;

        #region 非常用column
        [XmlElement("M47")]
        public List<FileVEO_M47_Jurisdiction> M47 = new List<FileVEO_M47_Jurisdiction>();
        [XmlElement("M48")]
        public List<FileVEO_M48_PlaceName> M48 = new List<FileVEO_M48_PlaceName>();
        [XmlElement("M49")]
        public List<FileVEO_M49_PeriodName> M49 = new List<FileVEO_M49_PeriodName>();
        #endregion
    }

    [XmlRoot("M47")]
    public class FileVEO_M47_Jurisdiction
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M48")]
    public class FileVEO_M48_PlaceName
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M49")]
    public class FileVEO_M49_PeriodName
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M50")]
    public class FileVEO_M50_Function
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;

        #region 非常用column
        [XmlElement("M53")]
        public List<FileVEO_M53_ThirdLevelDescriptor> M53 = new List<FileVEO_M53_ThirdLevelDescriptor>();
        [XmlElement("M51")]
        public List<FileVEO_M51_FunctionDescriptor> M51 = new List<FileVEO_M51_FunctionDescriptor>();
        [XmlElement("M52")]
        public List<FileVEO_M52_ActivityDescriptor> M52 = new List<FileVEO_M52_ActivityDescriptor>();
        #endregion
    }

    [XmlRoot("M51")]
    public class FileVEO_M51_FunctionDescriptor
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M52")]
    public class FileVEO_M52_ActivityDescriptor
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M53")]
    public class FileVEO_M53_ThirdLevelDescriptor
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M54")]
    public class FileVEO_M54_Date
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
        [XmlElement("M55")]
        public FileVEO_M55_DateTimeCreated M55;
        [XmlElement("M56")]
        public FileVEO_M56_DateTimeTransacted M56;
        [XmlElement("M57")]
        public FileVEO_M57_DateTimeRegistered M57;

        #region 非常用column
        [XmlElement("M144")]
        public FileVEO_M144_DateTimeClosed M144;
        #endregion
    }

    [XmlRoot("M55")]
    public class FileVEO_M55_DateTimeCreated
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M56")]
    public class FileVEO_M56_DateTimeTransacted
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M57")]
    public class FileVEO_M57_DateTimeRegistered
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M58")]
    public class FileVEO_M58_Type
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M59")]
    public class FileVEO_M59_AggregationLevel
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M60")]
    public class FileVEO_M60_Format
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
        #region 非常用column
        [XmlElement("M61")]
        public FileVEO_M61_MediaFormat M61;
        [XmlElement("M62")]
        public FileVEO_M62_DataFormat M62;
        [XmlElement("M63")]
        public FileVEO_M63_Medium M63;
        [XmlElement("M64")]
        public List<FileVEO_M64_Extent> M64 = new List<FileVEO_M64_Extent>();
        #endregion
    }

    [XmlRoot("M61")]
    public class FileVEO_M61_MediaFormat
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M62")]
    public class FileVEO_M62_DataFormat
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M63")]
    public class FileVEO_M63_Medium
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M64")]
    public class FileVEO_M64_Extent
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M65")]
    public class FileVEO_M65_RecordIdentifier
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M66")]
    public class FileVEO_M66_ManagementHistory
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
        [XmlElement("M67")]
        public List<FileVEO_M67_ManagementEvent> M67 = new List<FileVEO_M67_ManagementEvent>();
    }

    [XmlRoot("M67")]
    public class FileVEO_M67_ManagementEvent
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
        [XmlElement("M68")]
        public FileVEO_M68_EventDateTime M68;
        [XmlElement("M69")]
        public FileVEO_M69_EventType M69;
        [XmlElement("M70")]
        public FileVEO_M70_EventDescription M70;
    }

    [XmlRoot("M68")]
    public class FileVEO_M68_EventDateTime
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M69")]
    public class FileVEO_M69_EventType
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M70")]
    public class FileVEO_M70_EventDescription
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M71")]
    public class FileVEO_M71_UseHistory
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;

        #region 非常用column
        [XmlElement("M72")]
        public List<FileVEO_M72_Use> M72 = new List<FileVEO_M72_Use>();
        #endregion
    }

    [XmlRoot("M72")]
    public class FileVEO_M72_Use
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;

        #region 非常用column
        [XmlElement("M73")]
        public FileVEO_M73_UseDateTime M73;
        [XmlElement("M74")]
        public FileVEO_M74_UseType M74;
        [XmlElement("M75")]
        public FileVEO_M75_UseDescription M75;
        #endregion
    }

    [XmlRoot("M73")]
    public class FileVEO_M73_UseDateTime
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M74")]
    public class FileVEO_M74_UseType
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M75")]
    public class FileVEO_M75_UseDescription
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M76")]
    public class FileVEO_M76_PreservationHistory
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;

        #region 非常用column
        [XmlElement("M77")]
        public List<FileVEO_M77_Action> M77 = new List<FileVEO_M77_Action>();
        [XmlElement("M81")]
        public FileVEO_M81_NextAction M81;
        [XmlElement("M82")]
        public FileVEO_M82_NextActionDue M82;
        #endregion
    }

    [XmlRoot("M77")]
    public class FileVEO_M77_Action
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;

        #region 非常用column
        [XmlElement("M78")]
        public FileVEO_M78_ActionDateTime M78;
        [XmlElement("M79")]
        public FileVEO_M79_ActionType M79;
        [XmlElement("M80")]
        public FileVEO_M80_ActionDescription M80;
        #endregion
    }

    [XmlRoot("M78")]
    public class FileVEO_M78_ActionDateTime
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M79")]
    public class FileVEO_M79_ActionType
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M80")]
    public class FileVEO_M80_ActionDescription
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M81")]
    public class FileVEO_M81_NextAction
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M82")]
    public class FileVEO_M82_NextActionDue
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M83")]
    public class FileVEO_M83_Location
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;

        #region 非常用column
        [XmlElement("M84")]
        public FileVEO_M84_CurrentLocation M84;
        [XmlElement("M85")]
        public FileVEO_M85_HomeLocationDetails M85;
        [XmlElement("M86")]
        public FileVEO_M86_HomeStorageDetails M86;
        [XmlElement("M87")]
        public FileVEO_M87_RKSId M87;
        #endregion
    }

    [XmlRoot("M84")]
    public class FileVEO_M84_CurrentLocation
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M85")]
    public class FileVEO_M85_HomeLocationDetails
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M86")]
    public class FileVEO_M86_HomeStorageDetails
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M87")]
    public class FileVEO_M87_RKSId
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M88")]
    public class FileVEO_M88_Disposal
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
        [XmlElement("M89")]
        public List<FileVEO_M89_DisposalAuthorisation> M89 = new List<FileVEO_M89_DisposalAuthorisation>();
        [XmlElement("M90")]
        public FileVEO_M90_Sentence M90;

        #region 非常用column
        [XmlElement("M91")]
        public FileVEO_M91_DisposalActionDue M91;
        [XmlElement("M92")]
        public FileVEO_M92_DisposalStatus M92;
        #endregion
    }

    [XmlRoot("M89")]
    public class FileVEO_M89_DisposalAuthorisation
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M90")]
    public class FileVEO_M90_Sentence
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M91")]
    public class FileVEO_M91_DisposalActionDue
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M92")]
    public class FileVEO_M92_DisposalStatus
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M93")]
    public class FileVEO_M93_Mandate
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;

        #region 非常用column
        [XmlElement("M94")]
        public List<FileVEO_M94_MandateType> M94 = new List<FileVEO_M94_MandateType>();
        [XmlElement("M95")]
        public List<FileVEO_M95_RefersTo> M95 = new List<FileVEO_M95_RefersTo>();
        [XmlElement("M96")]
        public List<FileVEO_M96_MandateName> M96 = new List<FileVEO_M96_MandateName>();
        [XmlElement("M97")]
        public List<FileVEO_M97_MandateReference> M97 = new List<FileVEO_M97_MandateReference>();
        [XmlElement("M98")]
        public List<FileVEO_M98_Requirement> M98 = new List<FileVEO_M98_Requirement>();
        #endregion
    }

    [XmlRoot("M94")]
    public class FileVEO_M94_MandateType
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M95")]
    public class FileVEO_M95_RefersTo
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M96")]
    public class FileVEO_M96_MandateName
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M97")]
    public class FileVEO_M97_MandateReference
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M98")]
    public class FileVEO_M98_Requirement
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M99")]
    public class FileVEO_M99_VEOIdentifier
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
        [XmlElement("M102")]
        public FileVEO_M102_FileIdentifier M102;

        #region 非常用column
        [XmlElement("M100")]
        public FileVEO_M100_AgencyIdentifier M100;
        [XmlElement("M101")]
        public FileVEO_M101_SeriesIdentifier M101;
        #endregion
    }

    [XmlRoot("M100")]
    public class FileVEO_M100_AgencyIdentifier
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M101")]
    public class FileVEO_M101_SeriesIdentifier
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M102")]
    public class FileVEO_M102_FileIdentifier
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M134")]
    public class FileVEO_M134_SignatureBlock
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
        [XmlElement("M135")]
        public FileVEO_M135_SignatureFormatDescription M135;
        [XmlElement("M136")]
        public FileVEO_M136_SignatureDate M136;
        [XmlElement("M137")]
        public FileVEO_M137_Signer M137;
        [XmlElement("M138")]
        public FileVEO_M138_Signature M138;
        [XmlElement("M139")]
        public List<FileVEO_M139_CertificateBlock> M13 = new List<FileVEO_M139_CertificateBlock>();
        [XmlElement("M149")]
        public FileVEO_M149_SignatureAlgorithm M149;
    }



    [XmlRoot("M135")]
    public class FileVEO_M135_SignatureFormatDescription
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M136")]
    public class FileVEO_M136_SignatureDate
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M137")]
    public class FileVEO_M137_Signer
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M138")]
    public class FileVEO_M138_Signature
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M139")]
    public class FileVEO_M139_CertificateBlock
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
        [XmlElement("M140")]
        public List<FileVEO_M140_Certificate> M140 = new List<FileVEO_M140_Certificate>();
        [XmlElement("M141")]
        public FileVEO_M141_CertificateReference M141;
    }

    [XmlRoot("M140")]
    public class FileVEO_M140_Certificate
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M141")]
    public class FileVEO_M141_CertificateReference
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M142")]
    public class FileVEO_M142_File
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
        [XmlElement("M143")]
        public FileVEO_M143_FileMetadata M143;
        [XmlElement("M145")]
        public FileVEO_M145_FileDisposal M145;
    }

    [XmlRoot("M143")]
    public class FileVEO_M143_FileMetadata
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
        [XmlElement("M12")]
        public List<FileVEO_M12_Agent> M12 = new List<FileVEO_M12_Agent>();
        [XmlElement("M24")]
        public FileVEO_M24_RightsManagement M24;
        [XmlElement("MExtensionContextPath")]
        public FileVEO_MExtension_ContextPath MExtensionContextPath;
        [XmlElement("M32")]
        public FileVEO_M32_Title M32;
        [XmlElement("M54")]
        public FileVEO_M54_Date M54;
        [XmlElement("M59")]
        public FileVEO_M59_AggregationLevel M59;
        [XmlElement("M66")]
        public FileVEO_M66_ManagementHistory M66;
        [XmlElement("M88")]
        public FileVEO_M88_Disposal M88;
        [XmlElement("M99")]
        public FileVEO_M99_VEOIdentifier M99;

        #region 非常用column
        [XmlElement("M37")]
        public List<FileVEO_M37_Subject> M37 = new List<FileVEO_M37_Subject>();
        [XmlElement("M40")]
        public List<FileVEO_M40_Description> M40 = new List<FileVEO_M40_Description>();
        [XmlElement("M41")]
        public List<FileVEO_M41_Language> M41 = new List<FileVEO_M41_Language>();
        [XmlElement("M42")]
        public List<FileVEO_M42_Relation> M42 = new List<FileVEO_M42_Relation>();
        [XmlElement("M46")]
        public List<FileVEO_M46_Coverage> M46 = new List<FileVEO_M46_Coverage>();
        [XmlElement("M50")]
        public List<FileVEO_M50_Function> M50 = new List<FileVEO_M50_Function>();
        [XmlElement("M58")]
        public FileVEO_M58_Type M58;
        [XmlElement("M60")]
        public FileVEO_M60_Format M60;
        [XmlElement("M65")]
        public FileVEO_M65_RecordIdentifier M65;
        [XmlElement("M71")]
        public FileVEO_M71_UseHistory M71;
        [XmlElement("M76")]
        public FileVEO_M76_PreservationHistory M76;
        [XmlElement("M83")]
        public FileVEO_M83_Location M83;
        [XmlElement("M93")]
        public List<FileVEO_M93_Mandate> M93 = new List<FileVEO_M93_Mandate>();
        [XmlElement("M153")]
        public List<FileVEO_M153_AuxiliaryDescription> M153 = new List<FileVEO_M153_AuxiliaryDescription>();
        #endregion
    }

    [XmlRoot("M144")]
    public class FileVEO_M144_DateTimeClosed
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M145")]
    public class FileVEO_M145_FileDisposal
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;

        #region 非常用column
        [XmlElement("M146")]
        public FileVEO_M146_DisposalSchedule M146;
        [XmlElement("M147")]
        public FileVEO_M147_DisposalDate M147;
        [XmlElement("M148")]
        public FileVEO_M148_AuthorizingOfficer M148;
        #endregion
    }

    [XmlRoot("M146")]
    public class FileVEO_M146_DisposalSchedule
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M147")]
    public class FileVEO_M147_DisposalDate
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M148")]
    public class FileVEO_M148_AuthorizingOfficer
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M149")]
    public class FileVEO_M149_SignatureAlgorithm
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
        [XmlElement("M150")]
        public FileVEO_M150_SignatureAlgorithmIdentifier M150;
        [XmlElement("M151")]
        public FileVEO_M151_SignatureAlgorithmParameters M151;
    }

    [XmlRoot("M150")]
    public class FileVEO_M150_SignatureAlgorithmIdentifier
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M151")]
    public class FileVEO_M151_SignatureAlgorithmParameters
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }

    [XmlRoot("M152")]
    public class FileVEO_M152_LockSignatureBlock
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
        [XmlElement("M135")]
        public FileVEO_M135_SignatureFormatDescription M135;
        [XmlElement("M136")]
        public FileVEO_M136_SignatureDate M136;
        [XmlElement("M137")]
        public FileVEO_M137_Signer M137;
        [XmlElement("M138")]
        public FileVEO_M138_Signature M138;
        [XmlElement("M139")]
        public List<FileVEO_M139_CertificateBlock> M139 = new List<FileVEO_M139_CertificateBlock>();
        [XmlElement("M149")]
        public FileVEO_M149_SignatureAlgorithm M149;
    }

    [XmlRoot("M153")]
    public class FileVEO_M153_AuxiliaryDescription
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;
        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;
    }
}
