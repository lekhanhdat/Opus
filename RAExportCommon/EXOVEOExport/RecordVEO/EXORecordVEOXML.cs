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
    [XmlRoot("EXORecordVEOXML")]
    public class EXORecordVEOXML
    {
        [XmlElement("M1")]
        public EXORecordVEO_M1_VERSEncapsulatedObject M1;
    }

    [XmlRoot("M1")]
    public class EXORecordVEO_M1_VERSEncapsulatedObject
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
        [XmlElement("M2")]
        public EXORecordVEO_M2_VEOFormatDescription M2;
        [XmlElement("M3")]
        public EXORecordVEO_M3_Version M3;
        [XmlElement("M4")]
        public EXORecordVEO_M4_SignedObject M4;
        [XmlElement("M134")]
        public List<EXORecordVEO_M134_SignatureBlock> M134 = new List<EXORecordVEO_M134_SignatureBlock>();
        [XmlElement("M152")]
        public EXORecordVEO_M152_LockSignatureBlock M152;
    }

    [XmlRoot("M2")]
    public class EXORecordVEO_M2_VEOFormatDescription
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M3")]
    public class EXORecordVEO_M3_Version
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M4")]
    public class EXORecordVEO_M4_SignedObject
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
        [XmlElement("M5")]
        public EXORecordVEO_M5_ObjectMetadata M5;
        [XmlElement("M9")]
        public EXORecordVEO_M9_ObjectContent M9;
    }

    [XmlRoot("M5")]
    public class EXORecordVEO_M5_ObjectMetadata
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
        [XmlElement("M6")]
        public EXORecordVEO_M6_ObjectType M6;
        [XmlElement("M7")]
        public EXORecordVEO_M7_ObjectTypeDescription M7;
        [XmlElement("M8")]
        public EXORecordVEO_M8_ObjectCreationDate M8;
    }

    [XmlRoot("M6")]
    public class EXORecordVEO_M6_ObjectType
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }
    [XmlRoot("M7")]
    public class EXORecordVEO_M7_ObjectTypeDescription
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }
    [XmlRoot("M8")]
    public class EXORecordVEO_M8_ObjectCreationDate
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M9")]
    public class EXORecordVEO_M9_ObjectContent
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
        [XmlElement("M10")]
        public EXORecordVEO_M10_Record M10;
        [XmlElement("M114")]
        public List<EXORecordVEO_M114_Document> M114 = new List<EXORecordVEO_M114_Document>();
    }

    [XmlRoot("M10")]
    public class EXORecordVEO_M10_Record
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
        [XmlElement("M11")]
        public EXORecordVEO_M11_RecordMetadata M11;
    }

    [XmlRoot("M11")]
    public class EXORecordVEO_M11_RecordMetadata
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
        [XmlElement("M12")]
        public List<EXORecordVEO_M12_Agent> M12 = new List<EXORecordVEO_M12_Agent>();
        [XmlElement("M24")]
        public EXORecordVEO_M24_RightsManagement M24;
        [XmlElement("M32")]
        public EXORecordVEO_M32_Title M32;
        [XmlElement("M54")]
        public EXORecordVEO_M54_Date M54;
        [XmlElement("M59")]
        public EXORecordVEO_M59_AggregationLevel M59;
        [XmlElement("M66")]
        public EXORecordVEO_M66_ManagementHistory M66;
        [XmlElement("M88")]
        public EXORecordVEO_M88_Disposal M88;
        [XmlElement("M99")]
        public EXORecordVEO_M99_VEOIdentifier M99;

        #region 非必要column
        [XmlElement("M37")]
        public List<EXORecordVEO_M37_Subject> M37 = new List<EXORecordVEO_M37_Subject>();
        [XmlElement("M40")]
        public List<EXORecordVEO_M40_Description> M40 = new List<EXORecordVEO_M40_Description>();
        [XmlElement("M153")]
        public List<EXORecordVEO_M153_AuxiliaryDescription> M153 = new List<EXORecordVEO_M153_AuxiliaryDescription>();
        [XmlElement("M41")]
        public List<EXORecordVEO_M41_Language> M41 = new List<EXORecordVEO_M41_Language>();
        [XmlElement("M42")]
        public List<EXORecordVEO_M42_Relation> M42 = new List<EXORecordVEO_M42_Relation>();
        [XmlElement("M46")]
        public List<EXORecordVEO_M46_Coverage> M46 = new List<EXORecordVEO_M46_Coverage>();
        [XmlElement("M50")]
        public List<EXORecordVEO_M50_Function> M50 = new List<EXORecordVEO_M50_Function>();
        [XmlElement("M58")]
        public EXORecordVEO_M58_Type M58;
        [XmlElement("M60")]
        public EXORecordVEO_M60_Format M60;
        [XmlElement("M65")]
        public EXORecordVEO_M65_RecordIdentifier M65;
        [XmlElement("M71")]
        public EXORecordVEO_M71_UseHistory M71;
        [XmlElement("M76")]
        public EXORecordVEO_M76_PreservationHistory M76;
        [XmlElement("M83")]
        public EXORecordVEO_M83_Location M83;
        [XmlElement("M93")]
        public List<EXORecordVEO_M93_Mandate> M93 = new List<EXORecordVEO_M93_Mandate>();
        [XmlElement("M104")]
        public List<EXORecordVEO_M104_Transaction> M104 = new List<EXORecordVEO_M104_Transaction>();
        #endregion
    }

    [XmlRoot("M12")]
    public class EXORecordVEO_M12_Agent
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
        [XmlElement("M13")]
        public List<EXORecordVEO_M13_AgentType> M13 = new List<EXORecordVEO_M13_AgentType>();
        [XmlElement("M16")]
        public List<EXORecordVEO_M16_CorporateName> M16 = new List<EXORecordVEO_M16_CorporateName>();

        #region 非必要的column
        [XmlElement("M14")]
        public List<EXORecordVEO_M14_Jurisdiction> M14 = new List<EXORecordVEO_M14_Jurisdiction>();
        [XmlElement("M15")]
        public EXORecordVEO_M15_CorporateId M15;
        [XmlElement("M17")]
        public EXORecordVEO_M17_PersonId M17;
        [XmlElement("M18")]
        public List<EXORecordVEO_M18_PersonalName> M18 = new List<EXORecordVEO_M18_PersonalName>();
        [XmlElement("M19")]
        public List<EXORecordVEO_M19_SectionName> M19 = new List<EXORecordVEO_M19_SectionName>();
        [XmlElement("M20")]
        public List<EXORecordVEO_M20_PositionName> M20 = new List<EXORecordVEO_M20_PositionName>();
        [XmlElement("M21")]
        public List<EXORecordVEO_M21_ContactDetails> M21 = new List<EXORecordVEO_M21_ContactDetails>();
        [XmlElement("M22")]
        public List<EXORecordVEO_M22_Email> M22 = new List<EXORecordVEO_M22_Email>();
        [XmlElement("M23")]
        public List<EXORecordVEO_M23_DigitalSignature> M23 = new List<EXORecordVEO_M23_DigitalSignature>();
        #endregion
    }

    [XmlRoot("M13")]
    public class EXORecordVEO_M13_AgentType
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M14")]
    public class EXORecordVEO_M14_Jurisdiction
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M15")]
    public class EXORecordVEO_M15_CorporateId
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M16")]
    public class EXORecordVEO_M16_CorporateName
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M17")]
    public class EXORecordVEO_M17_PersonId
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M18")]
    public class EXORecordVEO_M18_PersonalName
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M19")]
    public class EXORecordVEO_M19_SectionName
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M20")]
    public class EXORecordVEO_M20_PositionName
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M21")]
    public class EXORecordVEO_M21_ContactDetails
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M22")]
    public class EXORecordVEO_M22_Email
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M23")]
    public class EXORecordVEO_M23_DigitalSignature
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M24")]
    public class EXORecordVEO_M24_RightsManagement
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
        [XmlElement("M25")]
        public EXORecordVEO_M25_SecurityClassification M25;
        #region 非必要column
        [XmlElement("M26")]
        public List<EXORecordVEO_M26_Caveat> M26 = new List<EXORecordVEO_M26_Caveat>();
        [XmlElement("M27")]
        public List<EXORecordVEO_M27_Codeword> M27 = new List<EXORecordVEO_M27_Codeword>();
        [XmlElement("M28")]
        public List<EXORecordVEO_M28_ReleasabilityIndicator> M28 = new List<EXORecordVEO_M28_ReleasabilityIndicator>();
        [XmlElement("M29")]
        public EXORecordVEO_M29_AccessStatus M29;
        [XmlElement("M30")]
        public List<EXORecordVEO_M30_UsageCondition> M30 = new List<EXORecordVEO_M30_UsageCondition>();
        [XmlElement("M31")]
        public EXORecordVEO_M31_EncryptionDetails M31;
        #endregion
    }

    [XmlRoot("M25")]
    public class EXORecordVEO_M25_SecurityClassification
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M26")]
    public class EXORecordVEO_M26_Caveat
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M27")]
    public class EXORecordVEO_M27_Codeword
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M28")]
    public class EXORecordVEO_M28_ReleasabilityIndicator
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M29")]
    public class EXORecordVEO_M29_AccessStatus
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M30")]
    public class EXORecordVEO_M30_UsageCondition
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M31")]
    public class EXORecordVEO_M31_EncryptionDetails
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M32")]
    public class EXORecordVEO_M32_Title
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
        [XmlElement("M33")]
        public List<EXORecordVEO_M33_SchemeType> M33 = new List<EXORecordVEO_M33_SchemeType>();
        [XmlElement("M34")]
        public EXORecordVEO_M34_SchemeName M34;
        [XmlElement("M35")]
        public EXORecordVEO_M35_TitleWords M35;

        #region 非必要column
        [XmlElement("M36")]
        public List<EXORecordVEO_M36_Alternative> M36 = new List<EXORecordVEO_M36_Alternative>();
        #endregion
    }

    [XmlRoot("M33")]
    public class EXORecordVEO_M33_SchemeType
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M34")]
    public class EXORecordVEO_M34_SchemeName
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M35")]
    public class EXORecordVEO_M35_TitleWords
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M36")]
    public class EXORecordVEO_M36_Alternative
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M37")]
    public class EXORecordVEO_M37_Subject
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;

        #region 非必要column
        [XmlElement("M38")]
        public EXORecordVEO_M38_KeywordLevel M38;
        [XmlElement("M39")]
        public List<EXORecordVEO_M39_Keyword> M39 = new List<EXORecordVEO_M39_Keyword>();
        #endregion
    }

    [XmlRoot("M38")]
    public class EXORecordVEO_M38_KeywordLevel
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M39")]
    public class EXORecordVEO_M39_Keyword
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M40")]
    public class EXORecordVEO_M40_Description
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M41")]
    public class EXORecordVEO_M41_Language
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M42")]
    public class EXORecordVEO_M42_Relation
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;

        #region 非必要column
        [XmlElement("M43")]
        public List<EXORecordVEO_M43_RelatedItemId> M43 = new List<EXORecordVEO_M43_RelatedItemId>();
        [XmlElement("M44")]
        public List<EXORecordVEO_M44_RelationType> M44 = new List<EXORecordVEO_M44_RelationType>();
        [XmlElement("M45")]
        public List<EXORecordVEO_M45_RelationDescription> M45 = new List<EXORecordVEO_M45_RelationDescription>();
        #endregion
    }

    [XmlRoot("M43")]
    public class EXORecordVEO_M43_RelatedItemId
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M44")]
    public class EXORecordVEO_M44_RelationType
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M45")]
    public class EXORecordVEO_M45_RelationDescription
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M46")]
    public class EXORecordVEO_M46_Coverage
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;

        #region 非必要column
        [XmlElement("M47")]
        public List<EXORecordVEO_M47_Jurisdiction> M47 = new List<EXORecordVEO_M47_Jurisdiction>();
        [XmlElement("M48")]
        public List<EXORecordVEO_M48_PlaceName> M48 = new List<EXORecordVEO_M48_PlaceName>();
        [XmlElement("M49")]
        public List<EXORecordVEO_M49_PeriodName> M49 = new List<EXORecordVEO_M49_PeriodName>();
        #endregion
    }

    [XmlRoot("M47")]
    public class EXORecordVEO_M47_Jurisdiction
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M48")]
    public class EXORecordVEO_M48_PlaceName
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M49")]
    public class EXORecordVEO_M49_PeriodName
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M50")]
    public class EXORecordVEO_M50_Function
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;

        #region 非必要column
        [XmlElement("M51")]
        public List<EXORecordVEO_M51_FunctionDescriptor> M51 = new List<EXORecordVEO_M51_FunctionDescriptor>();
        [XmlElement("M52")]
        public List<EXORecordVEO_M52_ActivityDescriptor> M52 = new List<EXORecordVEO_M52_ActivityDescriptor>();
        [XmlElement("M53")]
        public List<EXORecordVEO_M53_ThirdLevelDescriptor> M53 = new List<EXORecordVEO_M53_ThirdLevelDescriptor>();
        #endregion
    }

    [XmlRoot("M51")]
    public class EXORecordVEO_M51_FunctionDescriptor
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M52")]
    public class EXORecordVEO_M52_ActivityDescriptor
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M53")]
    public class EXORecordVEO_M53_ThirdLevelDescriptor
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M54")]
    public class EXORecordVEO_M54_Date
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
        [XmlElement("M55")]
        public EXORecordVEO_M55_DateTimeCreated M55;
        [XmlElement("M56")]
        public EXORecordVEO_M56_DateTimeTransacted M56;
        [XmlElement("M57")]
        public EXORecordVEO_M57_DateTimeRegistered M57;
    }

    [XmlRoot("M55")]
    public class EXORecordVEO_M55_DateTimeCreated
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M56")]
    public class EXORecordVEO_M56_DateTimeTransacted
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M57")]
    public class EXORecordVEO_M57_DateTimeRegistered
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M58")]
    public class EXORecordVEO_M58_Type
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M59")]
    public class EXORecordVEO_M59_AggregationLevel
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M60")]
    public class EXORecordVEO_M60_Format
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;

        #region 非必要column
        [XmlElement("M61")]
        public EXORecordVEO_M61_MediaFormat M61;
        [XmlElement("M62")]
        public EXORecordVEO_M62_DataFormat M62;
        [XmlElement("M63")]
        public EXORecordVEO_M63_Medium M63;
        [XmlElement("M64")]
        public List<EXORecordVEO_M64_Extent> M64 = new List<EXORecordVEO_M64_Extent>();
        #endregion
    }

    [XmlRoot("M61")]
    public class EXORecordVEO_M61_MediaFormat
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M62")]
    public class EXORecordVEO_M62_DataFormat
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M63")]
    public class EXORecordVEO_M63_Medium
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M64")]
    public class EXORecordVEO_M64_Extent
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M65")]
    public class EXORecordVEO_M65_RecordIdentifier
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M66")]
    public class EXORecordVEO_M66_ManagementHistory
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
        [XmlElement("M67")]
        public List<EXORecordVEO_M67_ManagementEvent> M67 = new List<EXORecordVEO_M67_ManagementEvent>();
    }

    [XmlRoot("M67")]
    public class EXORecordVEO_M67_ManagementEvent
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
        [XmlElement("M68")]
        public EXORecordVEO_M68_EventDateTime M68;
        [XmlElement("M69")]
        public EXORecordVEO_M69_EventType M69;
        [XmlElement("M70")]
        public EXORecordVEO_M70_EventDescription M70;
    }

    [XmlRoot("M68")]
    public class EXORecordVEO_M68_EventDateTime
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M69")]
    public class EXORecordVEO_M69_EventType
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M70")]
    public class EXORecordVEO_M70_EventDescription
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M71")]
    public class EXORecordVEO_M71_UseHistory
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;

        #region 非必要column
        [XmlElement("M72")]
        public List<EXORecordVEO_M72_Use> M72 = new List<EXORecordVEO_M72_Use>();
        #endregion
    }

    [XmlRoot("M72")]
    public class EXORecordVEO_M72_Use
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;

        #region 非必要column
        [XmlElement("M73")]
        public EXORecordVEO_M73_UseDateTime M73;
        [XmlElement("M74")]
        public EXORecordVEO_M74_UseType M74;
        [XmlElement("M75")]
        public EXORecordVEO_M75_UseDescription M75;
        #endregion
    }

    [XmlRoot("M73")]
    public class EXORecordVEO_M73_UseDateTime
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M74")]
    public class EXORecordVEO_M74_UseType
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M75")]
    public class EXORecordVEO_M75_UseDescription
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M76")]
    public class EXORecordVEO_M76_PreservationHistory
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;

        #region 非必要column
        [XmlElement("M77")]
        public List<EXORecordVEO_M77_Action> M77 = new List<EXORecordVEO_M77_Action>();
        [XmlElement("M81")]
        public EXORecordVEO_M81_NextAction M81;
        [XmlElement("M82")]
        public EXORecordVEO_M82_NextActionDue M82;
        #endregion
    }

    [XmlRoot("M77")]
    public class EXORecordVEO_M77_Action
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;

        #region 非必要column
        [XmlElement("M78")]
        public EXORecordVEO_M78_ActionDateTime M78;
        [XmlElement("M79")]
        public EXORecordVEO_M79_ActionType M79;
        [XmlElement("M80")]
        public EXORecordVEO_M80_ActionDescription M80;
        #endregion
    }

    [XmlRoot("M78")]
    public class EXORecordVEO_M78_ActionDateTime
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M79")]
    public class EXORecordVEO_M79_ActionType
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M80")]
    public class EXORecordVEO_M80_ActionDescription
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M81")]
    public class EXORecordVEO_M81_NextAction
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M82")]
    public class EXORecordVEO_M82_NextActionDue
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M83")]
    public class EXORecordVEO_M83_Location
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;

        #region 非必要column
        [XmlElement("M84")]
        public EXORecordVEO_M84_CurrentLocation M84;
        [XmlElement("M85")]
        public EXORecordVEO_M85_HomeLocationDetails M85;
        [XmlElement("M86")]
        public EXORecordVEO_M86_HomeStorageDetails M86;
        [XmlElement("M87")]
        public EXORecordVEO_M87_RKSId M87;
        #endregion
    }

    [XmlRoot("M84")]
    public class EXORecordVEO_M84_CurrentLocation
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M85")]
    public class EXORecordVEO_M85_HomeLocationDetails
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M86")]
    public class EXORecordVEO_M86_HomeStorageDetails
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M87")]
    public class EXORecordVEO_M87_RKSId
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M88")]
    public class EXORecordVEO_M88_Disposal
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
        [XmlElement("M89")]
        public List<EXORecordVEO_M89_DisposalAuthorisation> M89 = new List<EXORecordVEO_M89_DisposalAuthorisation>();
        [XmlElement("M90")]
        public EXORecordVEO_M90_Sentence M90;

        #region 非必要column
        [XmlElement("M91")]
        public EXORecordVEO_M91_DisposalActionDue M91;
        [XmlElement("M92")]
        public EXORecordVEO_M92_DisposalStatus M92;
        #endregion
    }

    [XmlRoot("M89")]
    public class EXORecordVEO_M89_DisposalAuthorisation
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M90")]
    public class EXORecordVEO_M90_Sentence
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M91")]
    public class EXORecordVEO_M91_DisposalActionDue
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M92")]
    public class EXORecordVEO_M92_DisposalStatus
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M93")]
    public class EXORecordVEO_M93_Mandate
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;

        #region 非必要column
        [XmlElement("M94")]
        public List<EXORecordVEO_M94_MandateType> M94 = new List<EXORecordVEO_M94_MandateType>();
        [XmlElement("M95")]
        public List<EXORecordVEO_M95_RefersTo> M95 = new List<EXORecordVEO_M95_RefersTo>();
        [XmlElement("M96")]
        public List<EXORecordVEO_M96_MandateName> M96 = new List<EXORecordVEO_M96_MandateName>();
        [XmlElement("M97")]
        public List<EXORecordVEO_M97_MandateReference> M97 = new List<EXORecordVEO_M97_MandateReference>();
        [XmlElement("M98")]
        public List<EXORecordVEO_M98_Requirement> M98 = new List<EXORecordVEO_M98_Requirement>();
        #endregion
    }

    [XmlRoot("M94")]
    public class EXORecordVEO_M94_MandateType
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M95")]
    public class EXORecordVEO_M95_RefersTo
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M96")]
    public class EXORecordVEO_M96_MandateName
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M97")]
    public class EXORecordVEO_M97_MandateReference
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M98")]
    public class EXORecordVEO_M98_Requirement
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M99")]
    public class EXORecordVEO_M99_VEOIdentifier
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
        [XmlElement("M102")]
        public List<EXORecordVEO_M102_FileIdentifier> M102 = new List<EXORecordVEO_M102_FileIdentifier>();
        [XmlElement("M103")]
        public EXORecordVEO_M103_VERSRecordIdentifier M103;

        #region 非必要column
        [XmlElement("M100")]
        public EXORecordVEO_M100_AgencyIdentifier M100;
        [XmlElement("M101")]
        public EXORecordVEO_M101_SeriesIdentifier M101;
        #endregion
    }

    [XmlRoot("M100")]
    public class EXORecordVEO_M100_AgencyIdentifier
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M101")]
    public class EXORecordVEO_M101_SeriesIdentifier
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M102")]
    public class EXORecordVEO_M102_FileIdentifier
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M103")]
    public class EXORecordVEO_M103_VERSRecordIdentifier
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M104")]
    public class EXORecordVEO_M104_Transaction
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;

        #region 非必要column
        [XmlElement("M105")]
        public EXORecordVEO_M105_TransactionIdentifier M105;
        [XmlElement("M106")]
        public EXORecordVEO_M106_Originator M106;
        [XmlElement("M107")]
        public List<EXORecordVEO_M107_Recipient> M107 = new List<EXORecordVEO_M107_Recipient>();
        [XmlElement("M108")]
        public List<EXORecordVEO_M108_ActionRequired> M108 = new List<EXORecordVEO_M108_ActionRequired>();
        [XmlElement("M109")]
        public EXORecordVEO_M109_OriginatorsCopy M109;
        [XmlElement("M110")]
        public List<EXORecordVEO_M110_TransactionType> M110 = new List<EXORecordVEO_M110_TransactionType>();
        [XmlElement("M111")]
        public List<EXORecordVEO_M111_BusinessProcedureReference> M111 = new List<EXORecordVEO_M111_BusinessProcedureReference>();
        [XmlElement("M112")]
        public List<EXORecordVEO_M112_TransactionReference> M112 = new List<EXORecordVEO_M112_TransactionReference>();
        [XmlElement("M113")]
        public List<EXORecordVEO_M113_TransactionLinkage> M113 = new List<EXORecordVEO_M113_TransactionLinkage>();
        #endregion
    }

    [XmlRoot("M105")]
    public class EXORecordVEO_M105_TransactionIdentifier
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M106")]
    public class EXORecordVEO_M106_Originator
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M107")]
    public class EXORecordVEO_M107_Recipient
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M108")]
    public class EXORecordVEO_M108_ActionRequired
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M109")]
    public class EXORecordVEO_M109_OriginatorsCopy
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M110")]
    public class EXORecordVEO_M110_TransactionType
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M111")]
    public class EXORecordVEO_M111_BusinessProcedureReference
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M112")]
    public class EXORecordVEO_M112_TransactionReference
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M113")]
    public class EXORecordVEO_M113_TransactionLinkage
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M114")]
    public class EXORecordVEO_M114_Document
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
        [XmlElement("M115")]
        public EXORecordVEO_M115_DocumentMetadata M115;
        [XmlElement("M126")]
        public List<EXORecordVEO_M126_Encoding> M126 = new List<EXORecordVEO_M126_Encoding>();
    }

    [XmlRoot("M115")]
    public class EXORecordVEO_M115_DocumentMetadata
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
        [XmlElement("M116")]
        public List<EXORecordVEO_M116_DocumentAgent> M116 = new List<EXORecordVEO_M116_DocumentAgent>();
        [XmlElement("M117")]
        public List<EXORecordVEO_M117_DocumentTitle> M117 = new List<EXORecordVEO_M117_DocumentTitle>();
        [XmlElement("M123")]
        public EXORecordVEO_M123_DocumentDate M123;
        [XmlElement("M125")]
        public List<EXORecordVEO_M125_DocumentSource> M125 = new List<EXORecordVEO_M125_DocumentSource>();

        #region 非必要column

        [XmlElement("M118")]
        public List<EXORecordVEO_M118_DocumentSubject> M118 = new List<EXORecordVEO_M118_DocumentSubject>();
        [XmlElement("M119")]
        public List<EXORecordVEO_M19_SectionName> M119 = new List<EXORecordVEO_M19_SectionName>();
        [XmlElement("M120")]
        public List<EXORecordVEO_M120_DocumentLanguage> M120 = new List<EXORecordVEO_M120_DocumentLanguage>();
        [XmlElement("M121")]
        public List<EXORecordVEO_M121_DocumentRelation> M121 = new List<EXORecordVEO_M121_DocumentRelation>();
        [XmlElement("M122")]
        public List<EXORecordVEO_M122_DocumentCoverage> M122 = new List<EXORecordVEO_M122_DocumentCoverage>();
        [XmlElement("M124")]
        public List<EXORecordVEO_M124_DocumentType> M124 = new List<EXORecordVEO_M124_DocumentType>();
        [XmlElement("M154")]
        public List<EXORecordVEO_M154_DocumentRightsManagement> M154 = new List<EXORecordVEO_M154_DocumentRightsManagement>();
        [XmlElement("M155")]
        public List<EXORecordVEO_M155_DocumentFunction> M155 = new List<EXORecordVEO_M155_DocumentFunction>();
        #endregion
    }

    [XmlRoot("M116")]
    public class EXORecordVEO_M116_DocumentAgent
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M117")]
    public class EXORecordVEO_M117_DocumentTitle
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M118")]
    public class EXORecordVEO_M118_DocumentSubject
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M119")]
    public class EXORecordVEO_M119_DocumentDescription
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M120")]
    public class EXORecordVEO_M120_DocumentLanguage
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M121")]
    public class EXORecordVEO_M121_DocumentRelation
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M122")]
    public class EXORecordVEO_M122_DocumentCoverage
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M123")]
    public class EXORecordVEO_M123_DocumentDate
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M124")]
    public class EXORecordVEO_M124_DocumentType
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M125")]
    public class EXORecordVEO_M125_DocumentSource
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M126")]
    public class EXORecordVEO_M126_Encoding
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
        [XmlElement("M127")]
        public EXORecordVEO_M127_EncodingMetadata M127;
        [XmlElement("M133")]
        public EXORecordVEO_M133_DocumentData M133;
    }

    [XmlRoot("M127")]
    public class EXORecordVEO_M127_EncodingMetadata
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
        [XmlElement("M128")]
        public EXORecordVEO_M128_FileEncoding M128;
        [XmlElement("M130")]
        public EXORecordVEO_M130_FileRendering M130;

        #region 非常用column
        [XmlElement("M129")]
        public EXORecordVEO_M129_SourceFileIdentifier M129;
        #endregion
    }

    [XmlRoot("M128")]
    public class EXORecordVEO_M128_FileEncoding
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M129")]
    public class EXORecordVEO_M129_SourceFileIdentifier
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M130")]
    public class EXORecordVEO_M130_FileRendering
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
        [XmlElement("M131")]
        public List<EXORecordVEO_M131_RenderingText> M131 = new List<EXORecordVEO_M131_RenderingText>();
        [XmlElement("M132")]
        public EXORecordVEO_M132_RenderingKeywords M132;
    }

    [XmlRoot("M131")]
    public class EXORecordVEO_M131_RenderingText
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M132")]
    public class EXORecordVEO_M132_RenderingKeywords
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M133")]
    public class EXORecordVEO_M133_DocumentData
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M134")]
    public class EXORecordVEO_M134_SignatureBlock
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
        [XmlElement("M135")]
        public EXORecordVEO_M135_SignatureFormatDescription M135;
        [XmlElement("M138")]
        public EXORecordVEO_M138_Signature M138;
        [XmlElement("M139")]
        public List<EXORecordVEO_M139_CertificateBlock> M139 = new List<EXORecordVEO_M139_CertificateBlock>();
        [XmlElement("M149")]
        public EXORecordVEO_M149_SignatureAlgorithm M149;

        #region 非常用coumn
        [XmlElement("M136")]
        public EXORecordVEO_M136_SignatureDate M136;
        [XmlElement("M137")]
        public EXORecordVEO_M137_Signer M137;
        #endregion
    }

    [XmlRoot("M135")]
    public class EXORecordVEO_M135_SignatureFormatDescription
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M136")]
    public class EXORecordVEO_M136_SignatureDate
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M137")]
    public class EXORecordVEO_M137_Signer
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M138")]
    public class EXORecordVEO_M138_Signature
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M139")]
    public class EXORecordVEO_M139_CertificateBlock
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
        [XmlElement("M140")]
        public List<EXORecordVEO_M140_Certificate> M140 = new List<EXORecordVEO_M140_Certificate>();
        [XmlElement("M141")]
        public EXORecordVEO_M141_CertificateReference M141;
        #region 非常用column

        #endregion
    }

    [XmlRoot("M140")]
    public class EXORecordVEO_M140_Certificate
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M141")]
    public class EXORecordVEO_M141_CertificateReference
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M149")]
    public class EXORecordVEO_M149_SignatureAlgorithm
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
        [XmlElement("M150")]
        public EXORecordVEO_M150_SignatureAlgorithmIdentifier M150;

        #region 非必要column
        [XmlElement("M151")]
        public EXORecordVEO_M151_SignatureAlgorithmParameters M151;
        #endregion
    }

    [XmlRoot("M150")]
    public class EXORecordVEO_M150_SignatureAlgorithmIdentifier
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M151")]
    public class EXORecordVEO_M151_SignatureAlgorithmParameters
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M152")]
    public class EXORecordVEO_M152_LockSignatureBlock
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
        [XmlElement("M135")]
        public EXORecordVEO_M135_SignatureFormatDescription M135;
        [XmlElement("M138")]
        public EXORecordVEO_M138_Signature M138;
        [XmlElement("M139")]
        public List<EXORecordVEO_M139_CertificateBlock> M139 = new List<EXORecordVEO_M139_CertificateBlock>();
        [XmlElement("M149")]
        public EXORecordVEO_M149_SignatureAlgorithm M149;
    }

    [XmlRoot("M153")]
    public class EXORecordVEO_M153_AuxiliaryDescription
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M154")]
    public class EXORecordVEO_M154_DocumentRightsManagement
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }

    [XmlRoot("M155")]
    public class EXORecordVEO_M155_DocumentFunction
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;
        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;
        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;
    }
}
