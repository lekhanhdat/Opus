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
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.QueryService
{
    internal partial class AveQueryService
    {
        /// <summary>
        /// 备份task item关联的Event Receivers
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="hostId"></param>
        /// <param name="contextCollectionId"></param>
        /// <returns></returns>
        [QueryReview("2013/10/09", "Qianwen Hu", false, "未使用索引，可能存在效率问题，暂无改进方法，可考虑使用其他方式实现")]
        public IAveQueryDataReader BackupTaskItemEvents(Guid siteId, Guid webId, Guid hostId, byte[] contextCollectionId)
        {
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@WebId", webId);
                mQueryWorker.AddParameter("@HostId", hostId);
                mQueryWorker.AddParameter("@ContextCollectionId", contextCollectionId);
                string commandText = @"SELECT * FROM EventReceivers WITH(NOLOCK) WHERE SiteId=@SiteId AND WebId=@WebId AND HostId=@HostId AND ContextCollectionId=@ContextCollectionId";

                return new AveQueryDataReader(mQueryWorker.ExecuteReader(commandText));
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        /// <summary>
        /// 备份workfow instance parent item关联的Event Receivers
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="hostId"></param>
        /// <param name="contextCollectionId"></param>
        /// <returns></returns>
        [QueryReview("2013/10/09", "Qianwen Hu", false, "未使用索引，可能存在效率问题，暂无改进方法，可考虑使用其他方式实现")]
        public IAveQueryDataReader BackupInstanceParentItemEvents(Guid siteId, Guid webId, Guid hostId, byte[] contextCollectionId)
        {
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@WebId", webId);
                mQueryWorker.AddParameter("@HostId", hostId);
                string commandText = string.Empty;
                if (contextCollectionId != null)
                {
                    mQueryWorker.AddParameter("@ContextCollectionId", contextCollectionId);
                    commandText = @"SELECT * FROM EventReceivers WITH(NOLOCK) WHERE SiteId=@SiteId AND WebId=@WebId AND HostId=@HostId AND HostType=2 AND 
                    Type=32767 AND ContextCollectionId=@ContextCollectionId AND ContextObjectId IS NULL AND ContextId IS NULL AND 
                    ContextType IS NULL AND ContextEventType IS NULL AND SequenceNumber=10000 AND Assembly='' AND Class=''";
                }
                else
                {
                    commandText = @"SELECT * FROM EventReceivers WITH(NOLOCK) WHERE SiteId=@SiteId AND WebId=@WebId AND HostId=@HostId AND HostType=2 AND  
                    Type=32767 AND ContextCollectionId IS NULL AND ContextObjectId IS NULL AND ContextId IS NULL AND 
                    ContextType IS NULL AND ContextEventType IS NULL AND SequenceNumber=10000 AND Assembly='' AND Class=''";
                }

                return new AveQueryDataReader(mQueryWorker.ExecuteReader(commandText));
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }
        
        /// <summary>
        /// 备份workflow instance关联的Event Receivers
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="hostId"></param>
        /// <param name="contextCollectionId"></param>
        /// <returns></returns>
        [QueryReview("2013/10/09", "Qianwen Hu", false, "未使用索引，可能存在效率问题，暂无改进方法，可考虑使用其他方式实现")]
        public IAveQueryDataReader BackupInstanceEvents(Guid siteId, Guid webId, byte[] contextCollectionId)
        {
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@WebId", webId);
                mQueryWorker.AddParameter("@ContextCollectionId", contextCollectionId);
                string commandText = @"SELECT * FROM EventReceivers WITH(NOLOCK) WHERE SiteId=@SiteId AND WebId=@WebId AND HostType=5 AND ContextCollectionId=@ContextCollectionId ORDER BY ItemId";

                return new AveQueryDataReader(mQueryWorker.ExecuteReader(commandText));
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }
    }
}
