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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.ReportCenter.AuditReport;
using AvePoint.GCommon.Contract.Server.Common.ExportReport.Object;

namespace AvePoint.GCommon.Contract.ReportCenter.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AuditReportDefinition : BaseCollectorDefinition
    {
        /// <summary>
        /// RunReport or DownloadReportNew or RunReportAndExport
        /// </summary>
        [DataMember]
        public AuditReportChartType DefinitionType { get; set; }

        [DataMember]
        public AuditReportChartType ReportDataType { get; set; }

        [DataMember]
        public ComplianceReportTitleType ReportDataTitlesType { get; set; }

        [DataMember]
        public ExportReportDto ExportReportDto { get; set; }

        [DataMember]
        public DateTime StartTime { get; set; }

        [DataMember]
        public DateTime EndTime { get; set; }

        [DataMember]
        public AuditReportType ReportType { get; set; }
        [DataMember]
        public string CustomReportFileName { get; set; }

        [DataMember]
        public int AuditEvents { get; set; }

        [DataMember]
        public UrlFilterCondition UrlFilter { get; set; }
    
        [DataMember]
        public List<FilterCondition> Filters { get; set; }

        [DataMember]
        public TimeSpan TimeOffset { get; set; }

        [DataMember]
        public double Offset { get; set; }

        [DataMember]
        public string PlanName { get; set; }

        [DataMember]
        public bool IsSkipAll { get; set; }
        [DataMember]
        public bool IsDBOverSize { get; set; }

        [DataMember]
        public SPUserScope UserScope { get; set; }

        [DataMember]
        public bool ZipFileToSP { get; set; }       

        [DataMember]
        public string PageViewConnString { get; set; }

        [DataMember]
        public bool CustomExportReportColumn { get; set; }

        [DataMember]
        public bool ExportEachSiteNewSite { get; set; }

        /// <summary>
        /// audit log 存储在 default storage.
        /// </summary>
        [DataMember]
        public string ConnectString { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FilterCondition
    {
        [DataMember]
        public string FilterKey { set; get; }

        /// <summary>
        /// 这个值永远不会返回null，没有数据会返回 new List<string>()
        /// </summary>
        [DataMember]
        public List<string> FilterValues { set; get; }

        [DataMember]
        public FilterAction FilterAction { get; set; }

        [DataMember]
        public Dictionary<string,string> FilterValueMaping { get; set; }

        public override string ToString()
        {
            return string.Format("FilterCondition[Action {0}, Key {1}, Values {2}]",
                FilterAction, FilterKey, string.Join(",", FilterValues.ToArray()));
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FilterCondition2
    {
        [DataMember]
        public string FilterKey { set; get; }

        private List<string> filterValues;

        /// <summary>
        /// 这个值永远不会返回null，没有数据会返回 new List<string>()
        /// </summary>
        [DataMember]
        public List<string> FilterValues
        {
            get
            {
                return filterValues ?? new List<string>();
            }
            set
            {
                filterValues = value;
            }
        }

        public List<string> TrimedFilterValues
        {
            get
            {
                List<string> filterValuesTrimed = new List<string>();
                if (filterValues != null)
                {
                    //把内容trim
                    for (int i = 0; i < filterValues.Count; i++)
                    {
                        if (filterValues[i] != null)
                        {
                            filterValuesTrimed.Add(filterValues[i].Trim());
                        }
                        else
                        {
                            filterValuesTrimed.Add(filterValues[i]);
                        }
                    }
                    return filterValuesTrimed;
                }
                return filterValuesTrimed;
            }
        }

        [DataMember]
        public FilterAction FilterAction { get; set; }

        public override string ToString()
        {
            return string.Format("FilterCondition[Action {0}, Key {1}, Values {2}]",
                FilterAction, FilterKey, string.Join(",", FilterValues.ToArray()));
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UrlFilterCondition : FilterCondition
    {
        [DataMember]
        public List<string[]> UrlPairs { get; set; }

        [DataMember]
        public List<string> UrlFilterValues
        {
            get
            {
                var urlFilterValues = new List<string>();
                var urlPairs = UrlPairs;
                if (urlPairs != null)
                {
                    foreach (var urlPair in UrlPairs)
                    {
                        if (urlPair[1].Trim() == "")
                        {
                            urlFilterValues.Add(urlPair[0].Trim());
                        }
                        else
                        {
                            var url = string.Format("{0}/{1}", urlPair[0].Trim().TrimEnd('/'), urlPair[1].Trim().TrimStart('/'));
                            urlFilterValues.Add(url);
                        }
                    }
                }
                return urlFilterValues;
            }
        }

        /// <summary>
        /// 每条记录的唯一值，用于区分记录，
        /// </summary>
        public int HashKey
        {
            get
            {
                var hashKey = 0;
                if (this.UrlPairs != null)
                {
                    hashKey += UrlPairs.GetHashCode();
                }
                if (this.FilterAction != null)
                {
                    hashKey += this.FilterAction.GetHashCode();
                }
                return hashKey;
            }
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SortCondition
    {
        [DataMember]
        public string SortKey { set; get; }
        [DataMember]
        public SortType SortType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SortType
    {
        [EnumMember]
        ASC,
        [EnumMember]
        DESC
    }

    /// <summary>
    /// filter的条件
    /// Like NotLike,  Equal, NotEqual GreaterThan仅使用FilterValues[0]
    /// BetweenAnd 会执行 between FilterValues[0] and FilterValues[1]
    /// GreaterThanInt 需要FilterValues[0]为数字 
    /// GreaterThanChar 需要FilterValues[0]为string
    /// 
    /// Likes (like FilterValues[0] or like FilterValues[1] or ...)
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum FilterAction
    {
        //对应sql的 in
        [EnumMember]
        Include,
        //not in
        [EnumMember]
        Exclude,
        //like
        [EnumMember]
        Like,
        //(like a or like b or ...)
        [EnumMember]
        Likes,
        //not like
        [EnumMember]
        NotLike,
        //=
        [EnumMember]
        Equal,
        [EnumMember]
        EqualInt,
        //!=
        [EnumMember]
        NotEqual,
        [EnumMember]
        NotEqualInt,
        [EnumMember]
        BetweenAnd,
        //>
        [EnumMember]
        GreaterThanInt,
        [EnumMember]
        GreaterThanChar,
        //对应sql的 in，把内容当作int
        [EnumMember]
        IncludeInt,
    }
}
