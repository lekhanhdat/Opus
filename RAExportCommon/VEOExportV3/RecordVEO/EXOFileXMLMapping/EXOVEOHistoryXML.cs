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
    [XmlRoot("EXOVEOHistoryXML")]
    public class EXOVEOHistoryXML
    {
        [XmlElement("M1")]
        public EXORecordVEO_M1_VEOHistory M1;
    }

    [XmlRoot("M1")]
    public class EXORecordVEO_M1_VEOHistory
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
        public EXORecordVEO_M2_Version M2;

        [XmlElement("M3")]
        public List<EXORecordVEO_M3_Event> M3 = new List<EXORecordVEO_M3_Event>();

    }

    [XmlRoot("M3")]
    public class EXORecordVEO_M3_Event
    {
        [XmlAttribute("MetadataName")]
        public string MetadataName = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("DefaultValue")]
        public string DefaultValue = VEOCommonString.STRINGEMPTY;

        [XmlAttribute("ExchangeMetadataAsSource")]
        public bool ExchangeMetadataAsSource;

        [XmlAttribute("ExchangeMetadata")]
        public string ExchangeMetadata;

        [XmlElement("M4")]
        public EXORecordVEO_M4_EventDateTime M4;

        [XmlElement("M5")]
        public EXORecordVEO_M5_EventType M5;

        [XmlElement("M6")]
        public EXORecordVEO_M6_Initiator M6;

        [XmlElement("M23")]
        public List<EXORecordVEO_M23_RDFRecord_Description> M23 = new List<EXORecordVEO_M23_RDFRecord_Description>(); //optional

        [XmlElement("M7")]
        public List<EXORecordVEO_M7_Error> M7 = new List<EXORecordVEO_M7_Error>();
    }

    [XmlRoot("M4")]
    public class EXORecordVEO_M4_EventDateTime
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

    [XmlRoot("M5")]
    public class EXORecordVEO_M5_EventType
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

    [XmlRoot("M6")]
    public class EXORecordVEO_M6_Initiator
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
    public class EXORecordVEO_M7_Error
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
