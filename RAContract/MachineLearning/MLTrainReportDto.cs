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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.MachineLearning
{
    public class MLTrainingReportDto
    {
        public Guid Id { set; get; }
        public string FileName { get; set; }
        public Guid PredictTermId { get; set; }
        public string PredictTermName { get; set; }
        public Guid TermId { get; set; }
        public string ChangeTermName { get; set; }
        public string RecordsID { get; set; }
        public int SourceFlag { get; set; }
        public string FullPath { get; set; }
        public string Type { get; set; }
        public string DateString { get; set; }
        public string Status { get; set; }
    }

    [DataContract]
    public class MLTrainingReportQueryParam
    {
        [DataMember]
        public int PageSize { get; set; }
        [DataMember]
        public string PageIndex { get; set; }
        [DataMember]
        public string SearchValue { get; set; }
        [DataMember]
        public string SortBy { get; set; }
        [DataMember]
        public bool IsAscending { get; set; }
        [DataMember]
        public List<TrainingReportFilter> Filters { set; get; }
    }

    [DataContract]
    public class TrainingReportFilter
    {
        [DataMember]
        public TrainingFilterColumn Column { get; set; }
        [DataMember]
        public List<string> ColumnValues { get; set; }
    }

    public class MLTrainingReportResult
    {
        public bool HasError { get; set; }
        public string ErrorMsg { get; set; }
        public List<MLTrainingReportDto> TrainingReports { get; set; }
        public int TotalCount { get; set; }
        public string PageIndex { get; set; }
    }

    [DataContract]
    public class MLTrainingReportExportParam
    {
        [DataMember]
        public TimeRange TimeRange { get; set; }

        [DataMember]
        public string StartTime { get; set; }

        [DataMember]
        public string EndTime { get; set; }
    }

    [DataContract]
    public enum TimeRange
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        After3Month = 1,
        [EnumMember]
        After6Month = 2,
        [EnumMember]
        After1Year = 3,
        [EnumMember]
        Custom = 4,
        [EnumMember]
        All = 5
    }
}
