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
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos.Notification;
using AvePoint.RA.DB.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMAOSNotificationDao : BaseDao<RMAOSNotification>, IRMAOSNotificationDao
    {
        public const string TABLE_NAME = "RMAOSNotifications";


        public void Add(RMAosQueueMessage message)
        {
            SystemDBExecuteWithRetry(context =>
            {
                context.AOSNotification.Add(ConvertToDomain(message));
                context.SaveChanges();
            });
        }

        public void Refresh(RMAosQueueMessage message)
        {
            SystemDBExecuteWithRetry(context =>
            {
                context.AOSNotification.AddOrUpdate(new RMAOSNotification
                {
                    Id = message.TenantGroupId,
                    TenantId = message.TenantGroupId,
                    Type = (int)message.MessageType,
                    Content = "",
                    Created = DateTime.UtcNow.Ticks
                });
                context.SaveChanges();
            });
        }

        public void Delete(string id)
        {
            SystemDBExecuteWithRetry(context =>
            {
                string sql = $"DELETE FROM {TABLE_NAME} WHERE Id=@Id;";
                context.Database.ExecuteSqlCommand(sql, new SqlParameter("Id", id));
            });
        }

        public void DeleteAll(string tenantId)
        {
            var needContinue = false;
            do
            {
                SystemDBExecuteWithRetry(context =>
                {
                    string sql = $"DELETE TOP(1000) FROM {TABLE_NAME} WHERE TenantId=@TenantId;";
                    var effectCount = context.Database.ExecuteSqlCommand(sql, new SqlParameter("TenantId", tenantId));
                    needContinue = effectCount > 0 && effectCount == 1000;
                });
            } while (needContinue);
        }

        public List<RMAosQueueMessage> GetSyncNodeMessages(string tenantId, List<int> types)
        {
            var items = SystemDBExecuteWithRetry(context =>
            {
                if(types.Count == 1)
                {
                    int type = types[0];
                    return context.AOSNotification
                        .Where(m => m.TenantId == tenantId && m.Type == type)
                        .ToList();
                }
                else
                {
                    return context.AOSNotification
                        .Where(m => m.TenantId == tenantId && types.Contains(m.Type))
                        .ToList();
                }
            });

            return items.Select(item => JsonConvert.DeserializeObject<RMAosQueueMessage>(item.Content)).ToList();
        }

        public List<string> GetPendingTenants(List<int> types, long timePeriod)
        {
            HashSet<string> pendingTenants = new HashSet<string>();
            SystemDBExecuteWithRetry(context =>
            {
                SecurityUtils.SanitizeSQLSchemaName(TABLE_NAME);
                string sql = 
$@"SELECT TenantId FROM {TABLE_NAME} 
WHERE [Type] IN ({string.Join(",", types)})
GROUP BY TenantId HAVING MAX(Created)<@Period";
                foreach (var tenantId in context.Database.SqlQuery<string>(sql, new SqlParameter("Period", timePeriod)))
                {
                    pendingTenants.Add(tenantId);
                }

                sql =
$@"SELECT DISTINCT TenantId FROM {TABLE_NAME} 
WHERE [Type] = {(int)RMAosQueueMessageType.LastSyncMessage}";
                foreach (var tenantId in context.Database.SqlQuery<string>(sql))
                {
                    pendingTenants.Add(tenantId);
                }
            });
            return pendingTenants.ToList();
        }

        public RMAosQueueMessage GetSyncAOSSecurityProfileMessage(string tenantId)
        {
            var msgType = (int)RMAosQueueMessageType.SyncAOSSecurityProfile;
            var notification = SystemDBExecuteWithRetry(context =>
            {
                return context.AOSNotification
                    .Where(m => m.TenantId == tenantId && m.Type == msgType)
                    .FirstOrDefault();
            });
            
            if(notification != null)
            {
                return JsonConvert.DeserializeObject<RMAosQueueMessage>(notification.Content);
            }
            return null;   
        }

        public List<RMAosQueueMessage> GetChangeTenantOwnerMessage()
        {
            var msgType = (int)RMAosQueueMessageType.ChangeTenantOwner;
            var tenants = SystemDBExecuteWithRetry(context =>
            {
                return context.AOSNotification
                    .Where(m => m.Type == msgType)
                    .ConvertAll(ConverToQueueMessage)
                    .ToList();
            });

            return tenants;
        }

        private RMAosQueueMessage ConverToQueueMessage(RMAOSNotification message)
        {
            return new RMAosQueueMessage()
            {
                QueueMessageId = message.Id,
                TenantGroupId = message.TenantId,
                MessageType = (RMAosQueueMessageType)message.Type,
                ReceiveMessageTime = message.Created,
            };
        }

        private RMAOSNotification ConvertToDomain(RMAosQueueMessage message)
        {
            message.ReceiveMessageTime = DateTime.UtcNow.Ticks;
            return new RMAOSNotification()
            {
                Id = message.QueueMessageId,
                TenantId = message.TenantGroupId,
                Type = (int)message.MessageType,
                Content = JsonConvert.SerializeObject(message),
                Created = message.ReceiveMessageTime
            };
        }
    }
}
