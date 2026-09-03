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

namespace AvePoint.Wrapper.QueryService
{
    using Common;
    using Common.QueryService;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// 这个类直接从QueryService 13中复制过来，由于资源问题，没有进行优化。
    /// todo:qlluo: 抽取SQL语句
    /// </summary>
    internal partial class AveQueryService : IAveAuditorQueryService
    {
        [QueryReview("2012/05/08", "Kexin Guo")]
        public IAveQueryDataReader RetrieveSiteAuditData(string siteId, DateTime startTime, DateTime endTime)
        {
            using (var sqlCommand = this.mQueryWorker.CreateCommand())
            {
                sqlCommand.Parameters.AddWithValue("@SiteId", siteId);
                sqlCommand.Parameters.AddWithValue("@StartTime", startTime);
                sqlCommand.Parameters.AddWithValue("@EndTime", endTime);
                sqlCommand.CommandText = @"SELECT Siteid,Itemid,Itemtype,UserId,Machinename,Machineip,Doclocation,Locationtype,Occurred,Event,Eventname,Eventsource,Sourcename,Eventdata FROM Auditdata WITH(NOLOCK) WHERE Siteid=@SiteId AND Occurred>@StartTime AND Occurred<=@EndTime;";
                return new AveQueryDataReader(this.mQueryWorker.ExecuteReader(sqlCommand));
            }
        }

        [QueryReview("2012/05/15", "Kexin Guo")]
        public IAveQueryDataReader GetIdsInSite(string siteId)
        {
            using (var cmd = mQueryWorker.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@SiteId", siteId);
                cmd.CommandText = @"select Id,ListId,WebId from alldocs with(nolock) where SiteId=@SiteId and Id in (select distinct(itemid) from auditdata with(nolock) where SIteId=@SiteId);";
                return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
            }
        }

        public void UpdateAuditFlag(IAveWeb web, AveAuditMaskType auditFlags)
        {
            using (var cmd = mQueryWorker.CreateCommand())
            {
                cmd.CommandText = @"update Webs set AuditFlags=@AuditFlags where Id=@Id and siteId = @siteId";
                cmd.Parameters.AddWithValue("@Id", web.ID);
                cmd.Parameters.AddWithValue("@siteId", web.Site.ID);
                cmd.Parameters.AddWithValue("@AuditFlags", (int)auditFlags);
                mQueryWorker.ExecuteNonQuery(cmd);
            }
        }

        public void UpdateAuditFlag(IAveList list, AveAuditMaskType auditFlags)
        {
            using (var cmd = mQueryWorker.CreateCommand())
            {
                cmd.CommandText = @"update AllLists set tp_AuditFlags=@tp_AuditFlags where tp_SiteId=@tp_SiteId AND tp_WebId=@tp_WebId and tp_Id=@tp_Id and tp_DeleteTransactionId=0x";
                cmd.Parameters.AddWithValue("@tp_AuditFlags", (int)auditFlags);
                cmd.Parameters.AddWithValue("@tp_SiteId", list.ParentWeb.Site.ID);
                cmd.Parameters.AddWithValue("@tp_WebId", list.ParentWeb.ID);
                cmd.Parameters.AddWithValue("@tp_Id", list.ID);
                mQueryWorker.ExecuteNonQuery(cmd);
            }
        }

        public void UpdateAuditFlag(IAveFolder folder, AveAuditMaskType auditFlags)
        {
            using (var cmd = mQueryWorker.CreateCommand())
            {
                cmd.CommandText = @"update AllDocs set AuditFlags=@AuditFlags where Id=@Id and SiteId=@SiteId";
                cmd.Parameters.AddWithValue("@AuditFlags", (int)auditFlags);
                cmd.Parameters.AddWithValue("@Id", folder.UniqueId);
                cmd.Parameters.AddWithValue("@SiteId", folder.ParentWeb.Site.ID);
                mQueryWorker.ExecuteNonQuery(cmd);
            }
        }

        public void UpdateAllDocsAuditFlag(Guid siteId, Guid parentId, AveAuditMaskType auditFlags)
        {
            using (var cmd = mQueryWorker.CreateCommand())
            {
                cmd.CommandText = @"update AllDocs set AuditFlags=@AuditFlags where ParentId=@ParentId and SiteId=@SiteId and DeleteTransactionId = 0x";
                cmd.Parameters.AddWithValue("@AuditFlags", (int)auditFlags);
                cmd.Parameters.AddWithValue("@SiteId", siteId);
                cmd.Parameters.AddWithValue("@ParentId", parentId);
                mQueryWorker.ExecuteNonQuery(cmd);
            }
        }


        public IAveQueryDataReader GetItemIdsInSite(string siteId, DateTime startTime, DateTime endTime)
        {
            IAveQueryDataReader result;
            using (var sqlCommand = this.mQueryWorker.CreateCommand())
            {
                sqlCommand.Parameters.AddWithValue("@SiteId", siteId);
                sqlCommand.Parameters.AddWithValue("@StartTime", startTime);
                sqlCommand.Parameters.AddWithValue("@EndTime", endTime);
                sqlCommand.CommandText = @"SELECT Id,ListId,WebId, LeafName FROM Alldocs WITH(NOLOCK) WHERE SiteId=@SiteId AND Id IN (SELECT DISTINCT(Itemid) FROM Auditdata WHIT(NOLOCK) WHERE SiteId=@SiteId AND Occurred BETWEEN @StartTime AND @EndTime);";
                result = new AveQueryDataReader(this.mQueryWorker.ExecuteReader(sqlCommand));
            }
            return result;
        }

        public IAveQueryDataReader GetDeletedSiteAuditData(DateTime startTime, DateTime endTime)
        {
            IAveQueryDataReader result;
            using (var sqlCommand = this.mQueryWorker.CreateCommand())
            {
                sqlCommand.Parameters.AddWithValue("@StartTime", startTime);
                sqlCommand.Parameters.AddWithValue("@EndTime", endTime);
                sqlCommand.CommandText = @"SELECT DISTINCT(SiteId) FROM AuditData WITH(NOLOCK) JOIN Sites WITH(NOLOCK) ON SiteId != Id WHERE Occurred BETWEEN @StartTime AND @EndTime";
                result = new AveQueryDataReader(this.mQueryWorker.ExecuteReader(sqlCommand));
            }
            return result;
        }

        public List<Guid> GetAuditDeletedSiteId(DateTime startTime, DateTime endTime)
        {
            HashSet<Guid> auditSiteIds = new HashSet<Guid>();
            HashSet<Guid> siteIds = new HashSet<Guid>();
            List<Guid> result = new List<Guid>();
            using (var sqlCommand = this.mQueryWorker.CreateCommand())
            {
                sqlCommand.Parameters.AddWithValue("@StartTime", startTime);
                sqlCommand.Parameters.AddWithValue("@EndTime", endTime);
                sqlCommand.CommandText = @"SELECT DISTINCT(SiteId) FROM AuditData WITH(NOLOCK) WHERE Occurred BETWEEN @StartTime AND @EndTime";
                using (var reader = this.mQueryWorker.ExecuteReader(sqlCommand))
                {
                    while (reader.Read())
                    {
                        auditSiteIds.Add(reader.GetGuid(0));
                    }
                }
            }
            using (var sqlCommand = this.mQueryWorker.CreateCommand())
            {
                sqlCommand.Parameters.AddWithValue("@StartTime", startTime);
                sqlCommand.Parameters.AddWithValue("@EndTime", endTime);
                sqlCommand.CommandText = @"SELECT Id FROM Sites WITH(NOLOCK)";
                using (var reader = this.mQueryWorker.ExecuteReader(sqlCommand))
                {
                    while (reader.Read())
                    {
                        siteIds.Add(reader.GetGuid(0));
                    }
                }
            }

            foreach (Guid auditSiteId in auditSiteIds)
            {
                if (!siteIds.Contains(auditSiteId))
                {
                    result.Add(auditSiteId);
                }
            }

            return result;
        }

        public DateTime GetMinSiteAuditDataTime(Guid siteId)
        {
            var minDateTime = DateTime.MinValue;
            using (var sqlCommand = this.mQueryWorker.CreateCommand())
            {
                sqlCommand.Parameters.AddWithValue("@SiteId", siteId);
                sqlCommand.CommandText = @"SELECT MIN(Occurred) FROM AuditData WITH(NOLOCK) WHERE SiteId=@SiteId";
                using (var reader = this.mQueryWorker.ExecuteReader(sqlCommand))
                {
                    while (reader.Read())
                    {
                        minDateTime = reader.IsDBNull(0) ? DateTime.MinValue : reader.GetDateTime(0);
                    }
                }
            }
            return minDateTime;
        }
    }
}
