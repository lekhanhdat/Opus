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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AvePoint.RA.Contract.JPMC
{
    public class JPMCAppConfig
    {
        [JsonProperty(PropertyName = "recordRetentionLabel")]
        public string RecordRetentionLabel { get; set; }

        [JsonProperty(PropertyName = "siteTypePropertyName")]
        public string SiteTypePropertyName { get; set; }

        [JsonProperty(PropertyName = "appVersion")]
        public string AppVersion { get; set; }

        [JsonProperty(PropertyName = "customColumns")]
        public CustomColumns CustomColumns { get; set; }
        
        [JsonProperty(PropertyName = "classCodeConfigs")]
        public List<ClassCodeConfig> ClassCodeConfigs { get; set; }
    }


    public class CustomColumns
    {
        [JsonProperty(PropertyName = "classCode")]
        public string ClassCode { get; set; }

        [JsonProperty(PropertyName = "recordStatus")]
        public string RecordStatus { get; set; }

        [JsonProperty(PropertyName = "countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty(PropertyName = "retentionType")]
        public string RetentionType { get; set; }

        [JsonProperty(PropertyName = "startDate")]
        public string StartDate { get; set; }

        [JsonProperty(PropertyName = "endDate")]
        public string EndDate { get; set; }
    }

    public class ClassCodeConfig
    {
        [JsonProperty(PropertyName = "classCode")]
        public ClassCode ClassCode { get; set; }

        [JsonProperty(PropertyName = "siteType")]
        public string SiteType { get; set; }

        [JsonProperty(PropertyName = "retentionSchedules")]
        public List<RetentionSchedule> RetentionSchedules { get; set; }
    }
    //classCodeConfigs -- classCode
    public class ClassCode
    {
        [JsonProperty(PropertyName = "termLabel")]
        public string TermLabel { get; set; }

        [JsonProperty(PropertyName = "termId")]
        public Guid TermId { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
    }

    //classCodeConfigs -- retentionSchedules
    public class RetentionSchedule
    {
        [JsonProperty(PropertyName = "recordStatus")]
        public string RecordStatus { get; set; }

        [JsonProperty(PropertyName = "countryCodes")]
        public List<string> CountryCodes { get; set; }

        [JsonProperty(PropertyName = "retentionType")]
        public string RetentionType { get; set; }

        [JsonProperty(PropertyName = "retentionPeriod")]
        public RetentionPeriod RetentionPeriod { get; set; }
    }

    //classCodeConfigs -- retentionSchedules -- retentionPeriod
    public class RetentionPeriod
    {
        [JsonProperty(PropertyName = "value")]
        public int Value { get; set; }

        [JsonProperty(PropertyName = "unit")]
        public string Unit { get; set; }
    }


    public class ClassCodeConfig4CheckDuplicate
    {
        public string RecordStatus { get; set; }
        public string CountryCode { get; set; }
        public string RetentionScheduleType { get; set; }
        public string RuleName { get; set; }
        public override bool Equals(object obj)
        {
            ClassCodeConfig4CheckDuplicate config = obj as ClassCodeConfig4CheckDuplicate;
            return config.RecordStatus.Equals(this.RecordStatus, StringComparison.OrdinalIgnoreCase)
                && config.CountryCode.Equals(this.CountryCode, StringComparison.OrdinalIgnoreCase)
                && config.RetentionScheduleType.Equals(this.RetentionScheduleType, StringComparison.OrdinalIgnoreCase);
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            return $"{this.RecordStatus}{this.CountryCode}{this.RetentionScheduleType}";
        }
    }

}
