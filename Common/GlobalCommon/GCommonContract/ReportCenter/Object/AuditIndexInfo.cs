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

namespace AvePoint.GCommon.Contract.ReportCenter.Object
{
    public class AuditIndexInfo 
    {
        public string FarmName { get; set; }
        public string DatabaseName { set; get; }
        public string SchemaName { set; get; }
        public string WebAppUrl { set; get; }
        public string SiteUrl { set; get; }
        /// <summary>
        /// 数据的时间，为每月1日
        /// </summary>
        public DateTime Month { set; get; }
        public long MonthTicks { set; get; }
        public string TableName { set; get; }
        /// <summary>
        /// view创建时间
        /// </summary>
        public DateTime CreateTime { set; get; }
        public long CreateTimeTicks { set; get; }

        public override string ToString()
        {
            return string.Format("AuditIndexInfo[Farm {0}, WebApp {1}, SiteUrl {2}, Month {3}, TableName {4}, CreateTime {5}, Db {6}, Schema {7}]", FarmName, WebAppUrl, SiteUrl, Month, TableName, CreateTime.ToString(), DatabaseName, SchemaName);
        }

        public string GetInsertSqlWithParams()
        {
            return "INSERT INTO AuditIndex(farmName, webAppUrl, siteUrl, auditMonth, tableName, createTime, DatabaseName,SchemaName) VALUES('', '', @siteUrl, @auditMonth, @tableName, @createTime, @DatabaseName,'')";
        }

        public List<KeyValuePair<string, object>> GetInsertParameters()
        {
            var parameters = new List<KeyValuePair<string, object>>();
            parameters.Add(new KeyValuePair<string, object>("SiteUrl", SiteUrl));
            parameters.Add(new KeyValuePair<string, object>("auditMonth", Month.Ticks));
            parameters.Add(new KeyValuePair<string, object>("TableName", TableName));
            parameters.Add(new KeyValuePair<string, object>("CreateTime", CreateTime.Ticks));
            parameters.Add(new KeyValuePair<string, object>("DatabaseName", DatabaseName));
            return parameters;
        }
    }

    /// <summary>
    /// 每个SiteCollection每个月一个view
    /// 每个view把当月全部数据表组成view
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AuditDataViewIndexInfo
    {
        [DataMember]
        public string FarmName { get; set; }
        [DataMember]
        public string WebAppUrl { set; get; }
        //这个表示的是site collection url
        [DataMember]
        public string SiteUrl { set; get; }
       //数据对应的月份
        [DataMember]
        public DateTime Month { set; get; }
        [DataMember]
        public long MonthTicks { set; get; }
        [DataMember]
        public string ViewName { set; get; }

        public override string ToString()
        {
            return string.Format("AuditDataViewIndexInfo[Farm {0}, WebApp {1}, Siteurl {2}, Month {3}, ViewName {4}]", FarmName, WebAppUrl, SiteUrl, Month, ViewName);
        }

        public string GetInsertSqlWithParams()
        {
            return "INSERT INTO AuditDataViewIndex(farmName, webAppUrl, siteUrl, auditMonth, viewName) VALUES('', '', @siteUrl, @auditMonth, @viewName)";
        }

        public List<KeyValuePair<string, object>> GetInsertParameters()
        {
            var parameters = new List<KeyValuePair<string, object>>();
            parameters.Add(new KeyValuePair<string, object>("SiteUrl", SiteUrl));
            parameters.Add(new KeyValuePair<string, object>("AuditMonth", Month.Ticks));
            parameters.Add(new KeyValuePair<string, object>("ViewName", ViewName));
            return parameters;
        }
    }
}