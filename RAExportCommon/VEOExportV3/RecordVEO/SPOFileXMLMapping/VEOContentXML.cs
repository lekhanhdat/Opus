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
using System.Xml.Serialization;

namespace RAExportCommon
{
    /// <summary>
    /// For SharePoint Online.
    /// </summary>
    [XmlRoot("VEOContentXML")]
    public class VEOContentXML
    {
        [XmlElement("M1")]
        public RecordVEO_M1_VEOContent M1;
    }

    [XmlRoot("M1")]
    public class RecordVEO_M1_VEOContent
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
        public RecordVEO_M2_Version M2;

        [XmlElement("M3")]
        public RecordVEO_M3_HashFunctionAlgorithm M3;

        [XmlElement("M4")]
        public List<RecordVEO_M4_InfomationObject> M4 = new List<RecordVEO_M4_InfomationObject>();
    }

    [XmlRoot("M2")]
    public class RecordVEO_M2_Version
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
    public class RecordVEO_M3_HashFunctionAlgorithm
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
    public class RecordVEO_M4_InfomationObject
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
        public RecordVEO_M5_InformationObjectType M5;

        [XmlElement("M6")]
        public RecordVEO_M6_InformationObjectDepth M6;

        [XmlElement("M7")]
        public List<RecordVEO_M7_MetadataPackage> M7 = new List<RecordVEO_M7_MetadataPackage>();

        [XmlElement("M68")]
        public List<RecordVEO_M68_InformationPiece> M68 = new List<RecordVEO_M68_InformationPiece>();
    }

    [XmlRoot("M5")]
    public class RecordVEO_M5_InformationObjectType
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

    [XmlRoot("M6")]
    public class RecordVEO_M6_InformationObjectDepth
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
    public class RecordVEO_M7_MetadataPackage
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;

        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;

        [XmlElement("M8")]
        public RecordVEO_M8_MetadataSchemaIdentifier M8;

        [XmlElement("M9")]
        public RecordVEO_M9_MetadataSyntaxIdentifier M9;

        [XmlElement("M10")]
        public List<RecordVEO_M10_RDF> M10 = new List<RecordVEO_M10_RDF>();
    }

    [XmlRoot("M8")]
    public class RecordVEO_M8_MetadataSchemaIdentifier
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
    public class RecordVEO_M9_MetadataSyntaxIdentifier
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

    [XmlRoot("M10")]
    public class RecordVEO_M10_RDF
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;

        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;

        [XmlElement("M90")]
        public List<RecordVEO_M90_RDFDescription> M90;
    }

    [XmlRoot("M11")]
    public class RecordVEO_M11RDFRecord
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
        public RecordVEO_M12_RDFRecord_EntityType M12;

        [XmlElement("M13")]
        public RecordVEO_M13_RDFRecord_Category M13;

        [XmlElement("M14")]
        public List<RecordVEO_M14_RDFRecord_Identifier> M14 = new List<RecordVEO_M14_RDFRecord_Identifier>();

        [XmlElement("M17")]
        public List<RecordVEO_M17_RDFRecord_Name> M17 = new List<RecordVEO_M17_RDFRecord_Name>();

        [XmlElement("M20")]
        public RecordVEO_M20_RDFRecord_DateRange M20;

        [XmlElement("M23")]
        public List<RecordVEO_M23_RDFRecord_Description> M23 = new List<RecordVEO_M23_RDFRecord_Description>(); //Optional

        [XmlElement("M24")]
        public List<RecordVEO_M24_RDFRecord_Jurisdiction> M24 = new List<RecordVEO_M24_RDFRecord_Jurisdiction>(); //Optional

        [XmlElement("M25")]
        public List<RecordVEO_M25_RDFRecord_SecurityClassification> M25 = new List<RecordVEO_M25_RDFRecord_SecurityClassification>();

        [XmlElement("M26")]
        public List<RecordVEO_M26_RDFRecord_SecurityCaveat> M26 = new List<RecordVEO_M26_RDFRecord_SecurityCaveat>();

        [XmlElement("M29")]
        public List<RecordVEO_M29_RDFRecord_Rights> M29 = new List<RecordVEO_M29_RDFRecord_Rights>();

        [XmlElement("M33")]
        public List<RecordVEO_M33_RDFRecord_Language> M33 = new List<RecordVEO_M33_RDFRecord_Language>();

        [XmlElement("M34")]
        public List<RecordVEO_M34_RDFRecord_Coverage> M34 = new List<RecordVEO_M34_RDFRecord_Coverage>(); //Optional

        [XmlElement("M38")]
        public List<RecordVEO_M38_RDFRecord_Keyword> M38 = new List<RecordVEO_M38_RDFRecord_Keyword>();

        [XmlElement("M43")]
        public List<RecordVEO_M43_RDFRecord_Disposal> M43 = new List<RecordVEO_M43_RDFRecord_Disposal>();

        [XmlElement("M49")]
        public RecordVEO_M49_RDFRecord_Format M49;

        [XmlElement("M56")]
        public List<RecordVEO_M56_RDFRecord_Extent> M56 = new List<RecordVEO_M56_RDFRecord_Extent>();

        [XmlElement("M61")]
        public RecordVEO_M61_RDFRecord_Medium M61;

        [XmlElement("M62")]
        public RecordVEO_M62_RDFRecord_IntegrityCheck M62;

        [XmlElement("M65")]
        public List<RecordVEO_M65_RDFRecord_Location> M65 = new List<RecordVEO_M65_RDFRecord_Location>(); //Optional

        [XmlElement("M66")]
        public RecordVEO_M66_RDFRecord_DocumentForm M66; //Optional

        [XmlElement("M67")]
        public RecordVEO_M67_RDFRecord_Precedence M67; //Optional

        [XmlElement("M73")]
        public RecordVEO_M73_RDFRecord_Relationship M73;

    }

    [XmlRoot("M12")]
    public class RecordVEO_M12_RDFRecord_EntityType
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

    [XmlRoot("M13")]
    public class RecordVEO_M13_RDFRecord_Category
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
    public class RecordVEO_M14_RDFRecord_Identifier
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;

        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;

        [XmlElement("M15")]
        public RecordVEO_M15_RDFRecord_IdentifierString M15;

        [XmlElement("M16")]
        public RecordVEO_M16_RDFRecord_IdentifierScheme M16;
    }

    [XmlRoot("M15")]
    public class RecordVEO_M15_RDFRecord_IdentifierString
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
    public class RecordVEO_M16_RDFRecord_IdentifierScheme
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
    public class RecordVEO_M17_RDFRecord_Name
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;

        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;

        [XmlElement("M18")]
        public RecordVEO_M18_RDFRecord_NameWords M18;

        [XmlElement("M19")]
        public RecordVEO_M19_RDFRecord_NameScheme M19;
    }

    [XmlRoot("M18")]
    public class RecordVEO_M18_RDFRecord_NameWords
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
    public class RecordVEO_M19_RDFRecord_NameScheme
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
    public class RecordVEO_M20_RDFRecord_DateRange
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;

        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;

        [XmlElement("M21")]
        public RecordVEO_M21_RDFRecord_StartDate M21;

        [XmlElement("M22")]
        public RecordVEO_M22_RDFRecord_EndDate M22;

    }

    [XmlRoot("M21")]
    public class RecordVEO_M21_RDFRecord_StartDate
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
    public class RecordVEO_M22_RDFRecord_EndDate
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
    public class RecordVEO_M23_RDFRecord_Description
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
    public class RecordVEO_M24_RDFRecord_Jurisdiction
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

    [XmlRoot("M25")]
    public class RecordVEO_M25_RDFRecord_SecurityClassification
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
    public class RecordVEO_M26_RDFRecord_SecurityCaveat
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;

        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;

        [XmlElement("M27")]
        public RecordVEO_M27_RDFRecord_CaveatText M27;

        [XmlElement("M28")]
        public RecordVEO_M28_RDFRecord_CaveatCategory M28;
    }

    [XmlRoot("M27")]
    public class RecordVEO_M27_RDFRecord_CaveatText
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
    public class RecordVEO_M28_RDFRecord_CaveatCategory
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
    public class RecordVEO_M29_RDFRecord_Rights
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;

        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;

        [XmlElement("M30")]
        public List<RecordVEO_M30_RDFRecord_RightsStatement> M30 = [];

        [XmlElement("M31")]
        public RecordVEO_M31_RDFRecord_RightsType M31;

        [XmlElement("M32")]
        public RecordVEO_M32_RDFRecord_RightsStatus M32;
    }

    [XmlRoot("M30")]
    public class RecordVEO_M30_RDFRecord_RightsStatement
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
    public class RecordVEO_M31_RDFRecord_RightsType
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
    public class RecordVEO_M32_RDFRecord_RightsStatus
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

    [XmlRoot("M33")]
    public class RecordVEO_M33_RDFRecord_Language
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
    public class RecordVEO_M34_RDFRecord_Coverage
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;

        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;

        [XmlElement("M35")]
        public List<RecordVEO_M35_RDFRecord_JurisdictionalCoverage> M35 = new List<RecordVEO_M35_RDFRecord_JurisdictionalCoverage>(); //optional

        [XmlElement("M36")]
        public List<RecordVEO_M36_RDFRecord_TemporalCoverage> M36 = new List<RecordVEO_M36_RDFRecord_TemporalCoverage>(); //optional

        [XmlElement("M37")]
        public List<RecordVEO_M37_RDFRecord_SpatialCoverage> M37 = new List<RecordVEO_M37_RDFRecord_SpatialCoverage>(); //optional
    }

    [XmlRoot("M35")]
    public class RecordVEO_M35_RDFRecord_JurisdictionalCoverage
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
    public class RecordVEO_M36_RDFRecord_TemporalCoverage
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
    public class RecordVEO_M37_RDFRecord_SpatialCoverage
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

    [XmlRoot("M38")]
    public class RecordVEO_M38_RDFRecord_Keyword
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;

        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;

        [XmlElement("M39")]
        public RecordVEO_M39_RDFRecord_KeywordTerm M39;

        [XmlElement("M40")]
        public RecordVEO_M40_RDFRecord_KeywordID M40;

        [XmlElement("M41")]
        public RecordVEO_M41_RDFRecord_KeywordScheme M41;

        [XmlElement("M42")]
        public RecordVEO_M42_RDFRecord_KeywordSchemeType M42;
    }

    [XmlRoot("M39")]
    public class RecordVEO_M39_RDFRecord_KeywordTerm
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
    public class RecordVEO_M40_RDFRecord_KeywordID
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
    public class RecordVEO_M41_RDFRecord_KeywordScheme
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
    public class RecordVEO_M42_RDFRecord_KeywordSchemeType
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

    [XmlRoot("M43")]
    public class RecordVEO_M43_RDFRecord_Disposal
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;

        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;

        [XmlElement("M44")]
        public RecordVEO_M44_RDFRecord_RetentionAndDisposalAuthority M44;

        [XmlElement("M45")]
        public RecordVEO_M45_RDFRecord_DisposalClassID M45;

        [XmlElement("M46")]
        public RecordVEO_M46_RDFRecord_DisposalAction M46;

        [XmlElement("M47")]
        public List<RecordVEO_M47_RDFRecord_DisposalTriggerDate> M47 = [];

        [XmlElement("M48")]
        public List<RecordVEO_M48_RDFRecord_DisposalActionDue> M48 = [];
    }

    [XmlRoot("M44")]
    public class RecordVEO_M44_RDFRecord_RetentionAndDisposalAuthority
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
    public class RecordVEO_M45_RDFRecord_DisposalClassID
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
    public class RecordVEO_M46_RDFRecord_DisposalAction
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

    [XmlRoot("M47")]
    public class RecordVEO_M47_RDFRecord_DisposalTriggerDate
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
    public class RecordVEO_M48_RDFRecord_DisposalActionDue
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
    public class RecordVEO_M49_RDFRecord_Format
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;

        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;

        [XmlElement("M50")]
        public RecordVEO_M50_RDFRecord_FormatName M50;

        [XmlElement("M51")]
        public RecordVEO_M51_RDFRecord_FormatVersion M51;

        [XmlElement("M52")]
        public RecordVEO_M52_RDFRecord_CreatingApplicationName M52;

        [XmlElement("M53")]
        public RecordVEO_M53_RDFRecord_CreatingApplicationVersion M53;

        [XmlElement("M54")]
        public RecordVEO_M54_RDFRecord_FormatRegistry M54;

        [XmlElement("M55")]
        public RecordVEO_M55_RDFRecord_FormatRegistryID M55;
    }

    [XmlRoot("M50")]
    public class RecordVEO_M50_RDFRecord_FormatName
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

    [XmlRoot("M51")]
    public class RecordVEO_M51_RDFRecord_FormatVersion
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
    public class RecordVEO_M52_RDFRecord_CreatingApplicationName
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
    public class RecordVEO_M53_RDFRecord_CreatingApplicationVersion
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
    public class RecordVEO_M54_RDFRecord_FormatRegistry
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

    [XmlRoot("M55")]
    public class RecordVEO_M55_RDFRecord_FormatRegistryID
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
    public class RecordVEO_M56_RDFRecord_Extent
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;

        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;

        [XmlElement("M57")]
        public RecordVEO_M57_RDFRecord_PhysicalDimensions M57;

        [XmlElement("M58")]
        public RecordVEO_M58_RDFRecord_LogicalSize M58;

        [XmlElement("M59")]
        public RecordVEO_M59_RDFRecord_Quantity M59;

        [XmlElement("M60")]
        public RecordVEO_M60_RDFRecord_Units M60;
    }

    [XmlRoot("M57")]
    public class RecordVEO_M57_RDFRecord_PhysicalDimensions
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
    public class RecordVEO_M58_RDFRecord_LogicalSize
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
    public class RecordVEO_M59_RDFRecord_Quantity
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
    public class RecordVEO_M60_RDFRecord_Units
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

    [XmlRoot("M61")]
    public class RecordVEO_M61_RDFRecord_Medium
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
    public class RecordVEO_M62_RDFRecord_IntegrityCheck
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;

        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;

        [XmlElement("M63")]
        public RecordVEO_M63_RDFRecord_HashFunctionName M63;

        [XmlElement("M64")]
        public RecordVEO_M64_RDFRecord_MessageDigest M64;
    }

    public class RecordVEO_M63_RDFRecord_HashFunctionName
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

    public class RecordVEO_M64_RDFRecord_MessageDigest
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
    public class RecordVEO_M65_RDFRecord_Location
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
    public class RecordVEO_M66_RDFRecord_DocumentForm
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

    [XmlRoot("M67")]
    public class RecordVEO_M67_RDFRecord_Precedence
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

    [XmlRoot("M68")]
    public class RecordVEO_M68_InformationPiece
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;

        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;

        [XmlElement("M69")]
        public RecordVEO_M69_Label M69;

        [XmlElement("M70")]
        public List<RecordVEO_M70_ContentFile> M70 = new List<RecordVEO_M70_ContentFile>();
    }

    [XmlRoot("M69")]
    public class RecordVEO_M69_Label
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
    public class RecordVEO_M70_ContentFile
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;

        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;

        [XmlElement("M71")]
        public RecordVEO_M71_PathName M71;

        [XmlElement("M72")]
        public RecordVEO_M72_HashValue M72;
    }

    [XmlRoot("M71")]
    public class RecordVEO_M71_PathName
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

    [XmlRoot("M72")]
    public class RecordVEO_M72_HashValue
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

    [XmlRoot("M73")]
    public class RecordVEO_M73_RDFRecord_Relationship
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
        public RecordVEO_M12_RDFRecord_EntityType M12;

        [XmlElement("M13")]
        public RecordVEO_M13_RDFRecord_Category M13;

        [XmlElement("M14")]
        public List<RecordVEO_M14_RDFRecord_Identifier> M14 = new List<RecordVEO_M14_RDFRecord_Identifier>();

        [XmlElement("M17")]
        public List<RecordVEO_M17_RDFRecord_Name> M17 = new List<RecordVEO_M17_RDFRecord_Name>();

        [XmlElement("M20")]
        public RecordVEO_M20_RDFRecord_DateRange M20;

        [XmlElement("M23")]
        public List<RecordVEO_M23_RDFRecord_Description> M23 = new List<RecordVEO_M23_RDFRecord_Description>();

        [XmlElement("M74")]
        public List<RecordVEO_M74_RDFRecord_RelationshipRelatedEntity> M74 = [];

        [XmlElement("M86")]
        public List<RecordVEO_M86_RDFRecord_RelationshipChangeHistory> M86 = [];
    }

    [XmlRoot("M74")]
    public class RecordVEO_M74_RDFRecord_RelationshipRelatedEntity
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;

        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;

        [XmlElement("M75")]
        public RecordVEO_M75_RDFRecord_AssignedEntityID M75;

        [XmlElement("M76")]
        public RecordVEO_M76_RDFRecord_AssignedEntityIDScheme M76;

        [XmlElement("M77")]
        public RecordVEO_M77_RDFRecord_RelationshipRole M77;

        [XmlElement("M78")]
        public RecordVEO_M78_RDFAgent M78;
    }

    [XmlRoot("M75")]
    public class RecordVEO_M75_RDFRecord_AssignedEntityID
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
    public class RecordVEO_M76_RDFRecord_AssignedEntityIDScheme
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

    [XmlRoot("M77")]
    public class RecordVEO_M77_RDFRecord_RelationshipRole
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

    [XmlRoot("M78")]
    public class RecordVEO_M78_RDFAgent
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
        public RecordVEO_M12_RDFRecord_EntityType M12;

        [XmlElement("M13")]
        public RecordVEO_M13_RDFRecord_Category M13;

        [XmlElement("M14")]
        public List<RecordVEO_M14_RDFRecord_Identifier> M14 = new List<RecordVEO_M14_RDFRecord_Identifier>();

        [XmlElement("M17")]
        public List<RecordVEO_M17_RDFRecord_Name> M17 = new List<RecordVEO_M17_RDFRecord_Name>();

        [XmlElement("M20")]
        public RecordVEO_M20_RDFRecord_DateRange M20;

        [XmlElement("M23")]
        public List<RecordVEO_M23_RDFRecord_Description> M23 = new List<RecordVEO_M23_RDFRecord_Description>();

        [XmlElement("M24")]
        public List<RecordVEO_M24_RDFRecord_Jurisdiction> M24 = new List<RecordVEO_M24_RDFRecord_Jurisdiction>();

        [XmlElement("M80")]
        public List<RecordVEO_M80_RDFAgent_Permissions> M80 = new List<RecordVEO_M80_RDFAgent_Permissions>();

        [XmlElement("M83")]
        public List<RecordVEO_M83_RDFAgent_Contact> M83 = new List<RecordVEO_M83_RDFAgent_Contact>();

        [XmlElement("M79")]
        public RecordVEO_M79_RDFAgent_Position M79;

        [XmlElement("M33")]
        public List<RecordVEO_M33_RDFRecord_Language> M33 = new List<RecordVEO_M33_RDFRecord_Language>();
    }

    [XmlRoot("M79")]
    public class RecordVEO_M79_RDFAgent_Position
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
    public class RecordVEO_M80_RDFAgent_Permissions
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;

        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;

        [XmlElement("M81")]
        public RecordVEO_M81_RDFAgent_PermissionText M81;

        [XmlElement("M82")]
        public RecordVEO_M82_RDFAgent_PermissionType M82;

    }

    [XmlRoot("M81")]
    public class RecordVEO_M81_RDFAgent_PermissionText
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
    public class RecordVEO_M82_RDFAgent_PermissionType
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
    public class RecordVEO_M83_RDFAgent_Contact
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;

        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;

        [XmlElement("M84")]
        public RecordVEO_M84_RDFAgent_ContactDetails M84;

        [XmlElement("M85")]
        public RecordVEO_M85_RDFAgent_ContactType M85;

    }

    [XmlRoot("M84")]
    public class RecordVEO_M84_RDFAgent_ContactDetails
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
    public class RecordVEO_M85_RDFAgent_ContactType
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
    public class RecordVEO_M86_RDFRecord_RelationshipChangeHistory
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;

        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;

        [XmlElement("M87")]
        public RecordVEO_M87_RDFRecord_PropertyName M87;

        [XmlElement("M88")]
        public RecordVEO_M88_RDFRecord_PriorValue M88;

        [XmlElement("M89")]
        public RecordVEO_M89_RDFRecord_RelationshipID M89;
    }

    [XmlRoot("M87")]
    public class RecordVEO_M87_RDFRecord_PropertyName
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
    public class RecordVEO_M88_RDFRecord_PriorValue
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

    [XmlRoot("M89")]
    public class RecordVEO_M89_RDFRecord_RelationshipID
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
    public class RecordVEO_M90_RDFDescription
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("SharePointMetadataAsSource")]
        public bool SharePointMetadataAsSource;

        [XmlAttribute("SharePointMetadata")]
        public string SharePointMetadata;

        [XmlElement("M11")]
        public RecordVEO_M11RDFRecord M11;
    }
}
