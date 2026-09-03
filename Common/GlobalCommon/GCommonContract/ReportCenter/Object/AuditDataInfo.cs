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
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AuditDataInfo
    {
        public int Id { get; set; }

        [DataMember]
        public string SiteUrl { set; get; }
        [DataMember]
        public string SiteId { set; get; }
        [DataMember]
        public string ItemId { set; get; }
        [DataMember]
        public int ItemType { set; get; }
        /// <summary>
        /// ItemType的字符串名字 例如 "view" "delete"
        /// </summary>
        [DataMember]
        public string ItemTypeName { set; get; }
        [DataMember]
        public string UserName { set; get; }
        [DataMember]
        public string UserDisplayName { get; set; }
        [DataMember]
        public string MachineName { set; get; }
        [DataMember]
        public string MachineIp { set; get; }
        [DataMember]
        public string DocLocation { set; get; }
       
        [DataMember]
        public long Occurred { set; get; }
        [DataMember]
        public string CustomEventName { set; get; }

        private string timeString;
        /// <summary>
        /// Occurred转换成当前时区的字符串时间
        /// </summary>
        public string TimeString
        {
            get
            {
                if (timeString == null)
                {
                    return new DateTime(Occurred).ToString("yyyy-MM-dd HH:mm:ss");
                }
                return timeString;
            }
            set
            {
                timeString = value;
            }
        }
        [DataMember]
        public string OccurredDisplayValue { set; get; }
        [DataMember]
        public long AuditTime { set; get; }
        [DataMember]
        public int Event { set; get; }
        [DataMember]
        public string ListId { set; get; }
        [DataMember]
        public string ListUrl { set; get; }

        /// <summary>
        /// event代表的字符串显示
        /// </summary>
        [DataMember]
        public string EventAction { set; get; }
        [DataMember]
        public string Title { set; get; }
        [DataMember]
        public string EventName { set; get; }
        [DataMember]
        public int EventSource { set; get; }
        [DataMember]
        public string SourceName { set; get; }
        [DataMember]
        public string EventData { set; get; }

        [Obsolete]
        [DataMember]
        public string EventDataKey { set; get; }
        [DataMember]
        public string FriendlyEventData { set; get; }
      
        [DataMember]
        public string WebUrl { set; get; }

        private string termSet;

        /// <summary>
        /// 当前会把TermSet保存到Titile，所以TermSet为空时返回Title
        /// </summary>
        [DataMember]
        public string TermSet
        {
            set
            {
                termSet = value;
            }
            get
            {
                return termSet ?? Title;
            }
        }

        /// <summary>
        /// 当前会把Outcome保存到Titile，所以Outcome为空时返回Title
        /// </summary>
        [DataMember]
        public string Outcome
        {
            get;
            set;
        }

        public string ItemLifeGroupName
        {
            get
            {
                return FullUrl + ", " + ItemTypeName;
            }
        }

        /// <summary>
        /// 该列仅用于查询完整url，不需要传输
        /// </summary>
        public string CaculatedFullUrl
        {
            get
            {
                string fullUrl = null;
                if (SiteUrl != null)
                {
                    var index = SiteUrl.Substring(9).IndexOf("/", StringComparison.OrdinalIgnoreCase);
                    //处理形如http://hostheader/abc/bcd这样的url
                    if (index > 0)
                    {
                        fullUrl = SiteUrl.Substring(0, index) + DocLocation;
                    }
                    else
                    {
                        fullUrl = SiteUrl + "/" + DocLocation;
                    }
                }
                return fullUrl;
            }
        }

        /// <summary>
        /// 该列仅用于查询List完整url，不需要传输
        /// </summary>
        public string ListFullUrl
        {
            get
            {
                string fullUrl = null;
                if (SiteUrl != null)
                {
                    fullUrl = ListUrl;
                }
                return fullUrl;
            }
        }

        [DataMember]
        public string FullUrl { set; get; }
        [DataMember]
        public Byte[] EventDataByte { set; get; }
        //SAAS-32285 增加 Browser 列.
        [DataMember]
        public string Browser { set; get; }

        public string AveSiteId { get; set; }

        //为了将数据存到 storage 中，方便写入txt文件,
        //AveSiteId,ItemId,ListId,ItemType,UserName,UserDisplayName,ClientHost,ClientIp,DocLocation,Occurred,
        //Event,EventName,EventSource,SourceName,EventData,FriendlyEventData,siteUrl,WebUrl,ListUrl,Title
        public string[] AuditDataArray
        {
            get
            {
                var auditDataList = new List<string>()
                {
                    AveSiteId, ItemId, ListId, ItemType.ToString(), UserName, UserDisplayName, MachineName, MachineIp, DocLocation, Occurred.ToString(),
                    Event.ToString(), EventName, EventSource.ToString(), SourceName, FriendlyEventData, WebUrl, ListUrl, Title
                };
                return auditDataList.ToArray();
            }
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ContentTypeInfo
    {
        [DataMember]
        public string ContentTypeId { get; set; }

        [DataMember]
        public string SiteUrl { get; set; }

        [DataMember]
        public string ContentTypeName { get; set; }

        /// <summary>
        /// Changed By(Login Name)
        /// </summary>
        [DataMember]
        public string ChangedBy { get; set; }

        [DataMember]
        public long Time { get; set; }

        [DataMember]
        public string Location { get; set; }

        [DataMember]
        public string NodeLevel { get; set; }

        [DataMember]
        public string ContentTypeDescription { get; set; }

        [DataMember]
        public string Group { get; set; }

        [DataMember]
        public bool ReadOnlyStatus { get; set; }

        [DataMember]
        public string LatestName { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public List<ContentTypeColumn> Columns { get; set; }

        /// <summary>
        /// Changed By(Display Name)
        /// </summary>
        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string WebUrl { get; set; }

        private string timeString;
        /// <summary>
        /// Occurred转换成当前时区的字符串时间
        /// </summary>
        public string TimeString
        {
            get
            {
                if (timeString == null)
                {
                    return new DateTime(Time).ToString("yyyy-MM-dd HH:mm:ss");
                }
                return timeString;
            }
            set
            {
                timeString = value;
            }
        }

        /// <summary>
        /// 该ContentType上一条记录
        /// </summary>
        [DataMember]
        public ContentTypeInfo LastRecord { get; set; }

        /// <summary>
        /// 删除ContentType产生的记录该值为True，否则为False
        /// </summary>
        [DataMember]
        public bool DeletionRecord { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ContentTypeColumn
    {
        [DataMember]
        public ContentTypeInfo Owner { get; set; }

        [DataMember]
        public string ColumnId { get; set; }

        [DataMember]
        public int ColumnOrder { get; set; }

        [DataMember]
        public string ColumnName { get; set; }

        [DataMember]
        public string ColumnType { get; set; }

        [DataMember]
        public string ColumnStatus { get; set; }

    }

}
