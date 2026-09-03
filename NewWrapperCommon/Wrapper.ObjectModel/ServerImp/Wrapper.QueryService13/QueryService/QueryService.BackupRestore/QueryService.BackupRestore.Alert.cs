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
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.QueryService;

namespace AvePoint.Wrapper.QueryService
{
    internal partial class AveQueryService
    {

        [QueryReview("2012/12/18", "Austin Han")]
        public bool HasAlertsOfSpecificConditions(Guid siteId, Guid listId, int? itemId, AveEventType eventType, int userId, AveAlertFrequency frequency)
        {
            try
            {
                string queryCmd = string.Empty;
                if (frequency == AveAlertFrequency.Immediate)
                {
                    queryCmd = @"SELECT top 1 0 FROM  ImmedSubscriptions WITH(NOLOCK)"; //Union SELECT Id,Properties FROM SchedSubscriptions ";
                }
                else
                {
                    queryCmd = @"SELECT top 1 0 FROM  SchedSubscriptions WITH(NOLOCK)";
                }
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@EventType", eventType);
                mQueryWorker.AddParameter("@UserId", userId);
                queryCmd += @" WHERE SiteId=@SiteId AND ListId=@ListId AND EventType=@EventType And UserId =@UserId And Deleted=0";
                mQueryWorker.AddParameter("@ListId", listId);

                if (itemId == null)
                {
                    queryCmd += " AND ItemId is NULL AND Filter=''";
                }
                else
                {
                    queryCmd += " AND ItemId=@ItemId";
                    mQueryWorker.AddParameter("@ItemId", itemId);
                }

                if (frequency == AveAlertFrequency.Daily)
                {
                    queryCmd += " AND NotifyFreq = 1";
                }
                else if (frequency == AveAlertFrequency.Weekly)
                {
                    queryCmd += " AND NotifyFreq = 2";
                }
                return mQueryWorker.ExecuteScalar(queryCmd) != null;
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.DEBUG, WrapperQueryServiceResource.CheckIfItemHasAlertsError, ex);
                return false;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ImmedSubscriptions is the parameter of the sql statement. ")]
        [QueryReview("2012/12/18", "Austin Han")]
        public bool ItemHasAlerts(Guid siteId, Guid listId, int itemId)
        {
            try
            {
                string queryCmd = @"SELECT top 1 0 FROM  ImmedSubscriptions WITH(NOLOCK)"; //Union SELECT Id,Properties FROM SchedSubscriptions ";
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@ItemId", itemId);
                if (listId == Guid.Empty)
                {
                    queryCmd += " WHERE SiteId=@SiteId AND ListId is NULL AND ItemId=@ItemId And Deleted=0";
                }
                else
                {
                    queryCmd += " WHERE SiteId=@SiteId AND ListId=@ListId AND ItemId=@ItemId And Deleted=0";
                    mQueryWorker.AddParameter("@ListId", listId);
                }
                return mQueryWorker.ExecuteScalar(queryCmd) != null || mQueryWorker.ExecuteScalar(queryCmd.Replace("ImmedSubscriptions", "SchedSubscriptions")) != null;
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.DEBUG, WrapperQueryServiceResource.CheckIfItemHasAlertsError, ex);
                return false;
            }
        }

        /// <summary>
        /// 查询List下是否有Alerts
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ImmedSubscriptions is the parameter of sql statement. ")]
        [QueryReview("2012/12/18", "Austin Han")]
        public bool ListHasLerts(Guid siteId, Guid listId)
        {
            string queryCmd = @"SELECT top 1 0 FROM  ImmedSubscriptions WITH(NOLOCK)"; //Union SELECT Id,Properties FROM SchedSubscriptions ";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            if (listId == Guid.Empty)
            {
                queryCmd += " WHERE SiteId=@SiteId AND ListId is NULL";
            }
            else
            {
                queryCmd += " WHERE SiteId=@SiteId AND ListId=@ListId";
                mQueryWorker.AddParameter("@ListId", listId);
            }
            return mQueryWorker.ExecuteScalar(queryCmd) != null || mQueryWorker.ExecuteScalar(queryCmd.Replace("ImmedSubscriptions", "SchedSubscriptions")) != null;
        }

        /// <summary>
        /// 获取Web下的所有Alerts
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/18", "Austin Han", true, "Use UNION ALL instead of UNION")]
        public IAveQueryDataReader GetWebAlerts(Guid siteId, Guid webId)
        {
            string queryCmd = @"SELECT Id, Properties,ListId  FROM  ImmedSubscriptions WITH(NOLOCK) WHERE SiteId=@SiteId AND WebId=@WebId  AND Deleted=0 
Union All SELECT Id,Properties,ListId  FROM SchedSubscriptions WITH(NOLOCK) WHERE SiteId=@SiteId AND WebId=@WebId  AND Deleted=0";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@WebId", webId);
            return new AveQueryDataReader(mQueryWorker.ExecuteReader(queryCmd));
        }

        /// <summary>
        /// 获取特定Item上的所有Scheduled Subscriptions
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="itemRowId"></param>
        /// <param name="hostType"></param>
        /// <param name="folderUrl"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public List<Dictionary<string, object>> GetSchedSubscriptions(Guid siteId, Guid webId, Guid listId, int itemRowId, AveSPAlertHostType hostType)
        {
            string mSchedQueryCmd =
  @"SELECT Id,NotifyFreq,NotifyTime,NotifyTimeUTC,UserId,UserEmail,SiteUrl,WebUrl,WebTitle,
                 WebLanguage,WebLocale,WebTimeZone,WebTime24,WebCalendarType,WebAdjustHijriDays,
                 ListUrl,ListTitle,ListBaseType,ListServerTemplate,AlertTitle,AlertType,AlertTemplateName,
                 Filter,BinaryFilter,Properties,Status,ItemDocId,DeliveryChannel,EventType
        FROM  SchedSubscriptions WITH(NOLOCK) ";
            mQueryWorker.ClearParameters();
            string queryConditions = InitialAlert(siteId, webId, listId, itemRowId, hostType, true);
            List<Dictionary<string, object>> ImmedSubscriptions = new List<Dictionary<string, object>>();
            Dictionary<string, object> dataCache = null;
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(mSchedQueryCmd + queryConditions.ToString()))
                {
                    while (dr.Read())
                    {
                        dataCache = new Dictionary<string, object>();
                        AveQueryUtility.GetDBRow(dataCache, dr);
                        ImmedSubscriptions.Add(dataCache);
                    }
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
            return ImmedSubscriptions;
        }

        [AveQueryService.QueryReviewAttribute("2012/12/17", "Austin Han")]
        private string InitialAlert(Guid siteId, Guid webId, Guid listId, int itemRowId, AveSPAlertHostType hostType, bool isSchedSubscriptions)
        {
            StringBuilder mQueryConditions = new StringBuilder();
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            if (listId.Equals(Guid.Empty))
            {
                mQueryConditions.Append(" WHERE SiteId=@SiteId AND ListId is NULL");
            }
            else
            {
                mQueryConditions.Append(" WHERE SiteId=@SiteId AND ListId=@ListId");
                mQueryWorker.AddParameter("@ListId", listId);
            }

            if (isSchedSubscriptions)
            {
                mQueryConditions.Append(" AND NotifyFreq<>0");//as 0 is indicate for Immediate alert and save in ImmedSubscriptions table.
            }

            switch (hostType)
            {
                case AveSPAlertHostType.List:
                case AveSPAlertHostType.Folder:
                    mQueryConditions.Append(" AND ItemId is NULL AND Deleted=0");
                    break;
                case AveSPAlertHostType.Doc:
                    mQueryWorker.AddParameter("@ItemId", itemRowId);
                    mQueryConditions.Append(" AND ItemId=@ItemId AND Deleted=0");
                    break;
                case AveSPAlertHostType.Item:
                    mQueryWorker.AddParameter("@ItemId", itemRowId);
                    mQueryConditions.Append(" AND ItemId=@ItemId AND Deleted=0");
                    break;
                default:
                    break;
            }

            return mQueryConditions.ToString();
        }

        [QueryReview("2012/12/17", "Austin Han")]
        public List<Dictionary<string, object>> GetImmedSubscriptions(Guid siteId, Guid webId, Guid listId, int itemRowId, AveSPAlertHostType hostType)
        {
            string mImmedQueryCmd =
@"SELECT Id,UserId,UserEmail,SiteUrl,WebUrl,WebTitle,WebLanguage,WebLocale,WebTimeZone,
                 WebTime24,WebCalendarType,WebAdjustHijriDays,ListUrl,ListTitle,ListBaseType,
                 ListServerTemplate,AlertTitle,AlertType,AlertTemplateName,Filter,BinaryFilter,
                 Properties,Status,ItemDocId,DeliveryChannel,EventType
        FROM ImmedSubscriptions WITH(NOLOCK) ";
            mQueryWorker.ClearParameters();
            string queryConditions = InitialAlert(siteId, webId, listId, itemRowId, hostType, false);
            List<Dictionary<string, object>> ImmedSubscriptions = new List<Dictionary<string, object>>();
            Dictionary<string, object> dataCache = null;
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(mImmedQueryCmd + queryConditions.ToString()))
                {
                    while (dr.Read())
                    {
                        dataCache = new Dictionary<string, object>();
                        AveQueryUtility.GetDBRow(dataCache, dr);
                        ImmedSubscriptions.Add(dataCache);
                    }
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }

            return ImmedSubscriptions;
        }
    }
}
