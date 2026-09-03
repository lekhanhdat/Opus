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
    [XmlRoot("ManifestXML")]
    public class ManifestVEOXML
    {
        [XmlElement("created_timestamp")]
        public CreatedTimeStamp CreatedTimeStamp;

        [XmlElement("agency_identifier")]
        public AgencyIdentifier AgencyIdentifier;

        [XmlElement("series_type")]
        public SeriesType SeriesType;

        [XmlElement("series_number")]
        public SeriesNumber SeriesNumber;

        [XmlElement("consignment_type")]
        public ConsignmentType ConsignmentType;

        [XmlElement("consignment_number")]
        public ConsignmentNumber ConsignmentNumber;

        [XmlElement("job_id")]
        public JobID JobID;

        [XmlElement("computer_filename")]
        public ComputerFileName ComputerFileName;

        [XmlElement("file_identifier")]
        public FileIdentifier FileIdentifier;

        [XmlElement("vers_record_identifier")]
        public VersRecordIdentifier VersRecordIdentifier;

        [XmlElement("veo_title")]
        public VEOTitle VEOTitle;

        [XmlElement("veo_classification")]
        public VEOClassification VEOClassification;

        [XmlElement("veo_access_category")]
        public VEOAccessCategory VEOAccessCategory;

        [XmlElement("veo_disposal_authority")]
        public VEODisposalAutority VEODisposalAutority;

        [XmlElement("veo_date_range")]
        public VEODateRange VEODateRange;

        [XmlElement("size_kb")]
        public SizeKB SizeKB;

    }

    [XmlRoot("created_timestamp")]
    public class CreatedTimeStamp
    {
        [XmlAttribute("ElementName")]
        public string ElementName;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue;
    }

    [XmlRoot("agency_identifier")]
    public class AgencyIdentifier
    {
        [XmlAttribute("ElementName")]
        public string ElementName;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue;
    }

    [XmlRoot("series_type")]
    public class SeriesType
    {
        [XmlAttribute("ElementName")]
        public string ElementName;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue;
    }

    [XmlRoot("series_number")]
    public class SeriesNumber
    {
        [XmlAttribute("ElementName")]
        public string ElementName;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue;
    }

    [XmlRoot("consignment_type")]
    public class ConsignmentType
    {
        [XmlAttribute("ElementName")]
        public string ElementName;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue;
    }

    [XmlRoot("consignment_number")]
    public class ConsignmentNumber
    {
        [XmlAttribute("ElementName")]
        public string ElementName;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue;
    }

    [XmlRoot("job_id")]
    public class JobID
    {
        [XmlAttribute("ElementName")]
        public string ElementName;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue;
    }

    [XmlRoot("computer_filename")]
    public class ComputerFileName
    {
        [XmlAttribute("ElementName")]
        public string ElementName;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue;
    }

    [XmlRoot("file_identifier")]
    public class FileIdentifier
    {
        [XmlAttribute("ElementName")]
        public string ElementName;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue;
    }

    [XmlRoot("vers_record_identifier")]
    public class VersRecordIdentifier
    {
        [XmlAttribute("ElementName")]
        public string ElementName;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue;
    }

    [XmlRoot("veo_title")]
    public class VEOTitle
    {
        [XmlAttribute("ElementName")]
        public string ElementName;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue;
    }

    [XmlRoot("veo_classification")]
    public class VEOClassification
    {
        [XmlAttribute("ElementName")]
        public string ElementName;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue;
    }

    [XmlRoot("veo_access_category")]
    public class VEOAccessCategory
    {
        [XmlAttribute("ElementName")]
        public string ElementName;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue;
    }

    [XmlRoot("veo_disposal_authority")]
    public class VEODisposalAutority
    {
        [XmlAttribute("ElementName")]
        public string ElementName;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue;
    }

    [XmlRoot("veo_date_range")]
    public class VEODateRange
    {
        [XmlAttribute("ElementName")]
        public string ElementName;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue;

        [XmlElement("veo_start_date")]
        public VEOStartRange StartDate;

        [XmlElement("veo_end_date")]
        public VEOEndRange EndDate;
    }

    [XmlRoot("size_kb")]
    public class SizeKB
    {
        [XmlAttribute("ElementName")]
        public string ElementName;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue;
    }

    [XmlRoot("veo_start_date")]
    public class VEOStartRange
    {
        [XmlAttribute("ElementName")]
        public string ElementName;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue;
    }

    [XmlRoot("veo_end_date")]
    public class VEOEndRange
    {
        [XmlAttribute("ElementName")]
        public string ElementName;
        [XmlAttribute("DefaultValue")]
        public string DefaultValue;
    }
}
