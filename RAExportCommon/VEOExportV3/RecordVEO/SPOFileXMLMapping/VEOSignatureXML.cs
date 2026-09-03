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
    [XmlRoot("VEOSignatureXML")]
    public class VEOSignatureXML
    {
        [XmlElement("M1")]
        public RecordVEO_M1_SignatureBlock M1;
    }

    [XmlRoot("M1")]
    public class RecordVEO_M1_SignatureBlock
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
        public RecordVEO_M3_SignatureAlgorithm M3;

        [XmlElement("M4")]
        public RecordVEO_M4_SignatureDateTime M4;

        [XmlElement("M5")]
        public RecordVEO_M5_Signer M5;

        [XmlElement("M6")]
        public RecordVEO_M6_Signature M6;

        [XmlElement("M7")]
        public RecordVEO_M7_CertificateChain M7;
    }

    [XmlRoot("M3")]
    public class RecordVEO_M3_SignatureAlgorithm
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
    public class RecordVEO_M4_SignatureDateTime
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

    [XmlRoot("M5")]
    public class RecordVEO_M5_Signer
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
    public class RecordVEO_M6_Signature
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
    public class RecordVEO_M7_CertificateChain
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
        public List<RecordVEO_M8_Certificate> M8 = new List<RecordVEO_M8_Certificate>();
    }

    [XmlRoot("M8")]
    public class RecordVEO_M8_Certificate
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
