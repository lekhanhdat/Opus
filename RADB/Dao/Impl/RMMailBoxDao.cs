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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMMailBoxDao : BaseDao<RMMailbox>, IRMMailboxDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMMailBoxDao));
        public IRMSecurityContainerDao SecurityContainerDao { get; set; }

        private const int NodeLevel_ExchangeOnlineMailbox = (int)NodeLevel.ExchangeOnlineMailbox;
        private const int NodeLevel_ExchangeOnlineMailboxGroup = (int)NodeLevel.ExchangeOnlineMailboxGroup;
        private const int NodeLevel_ExchangeOnlineO365Group = (int)NodeLevel.ExchangeOnlineO365Group;
        private const int NodeLevel_ExchangeOnlineO365GroupsGroup = (int)NodeLevel.ExchangeOnlineO365GroupGroup;
        private const string TABLE_NAME = "RMMailboxes";

        private string GetFullTableName(Core.RMDbContext context)
        {
            return $"[{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].[{TABLE_NAME}]";
        }
        private string GetFullTableName()
        {
            return $"[{SecurityUtils.SanitizeSQLSchemaName(GetTenantSchemaName())}].[{TABLE_NAME}]";
        }

        public void AddEmailsForAutoScan(List<EmailAccountDto> emails)
        {
            ThrowUtil.ThrowIfNull(emails, "emails");
            if (emails.Count == 0)
            {
                return;
            }
            logger.Debug("AddEmails count {0}", emails.Count);
            using (new PerformanceScope("AddEmailsForAutoScan"))
            {
                var tableName = GetFullTableName();
                using (var table = ConvertToDataTable(emails))
                {
                    if (table.Rows.Count == 0)
                    {
                        return;
                    }
                    logger.Debug("Finish convert DataTable.");
                    table.TableName = tableName;
                    BatchAdd(table, tableName);
                }
            }
        }
        private DataTable ConvertToDataTable(List<EmailAccountDto> emails)
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(String));
            table.Columns.Add("Name", typeof(String));
            table.Columns.Add("ParentId", typeof(String));
            table.Columns.Add("ObjectId", typeof(String));
            table.Columns.Add("State", typeof(Int32));
            table.Columns.Add("NodeLevel", typeof(Int32));
            table.Columns.Add("UserName", typeof(String));
            table.Columns.Add("Password", typeof(String));
            table.Columns.Add("SPVersion", typeof(String));
            table.Columns.Add("ServiceUrl", typeof(String));
            table.Columns.Add("TenantId", typeof(String));
            table.Columns.Add("AuthType", typeof(Int32));
            table.Columns.Add("ServiceAccountId", typeof(String));
            table.Columns.Add("AppType", typeof(Int32));
            table.Columns.Add("MailboxType", typeof(Int32));
            table.Columns.Add("ScanSource", typeof(Int32));
            table.Columns.Add("FromDAO", typeof(Boolean));
            table.Columns.Add("CreateTime", typeof(Int64));
            table.Columns.Add("ModifiedDate", typeof(Int64));

            var existsIDs = GetExistIDs(emails.Select(s => s.Id));
            HashSet<string> nodeIDs = new HashSet<string>();
            foreach (var email in emails)
            {
                if (!nodeIDs.Add(email.Id))
                {
                    logger.Warn($"Repeat mail id: {email.Id}, {email.Email}");
                    continue;
                }
                if (existsIDs.Contains(email.Id))
                {
                    logger.Warn($"Exists mail id: {email.Id}, {email.Email}");
                    continue;
                }
                var row = table.NewRow();
                row["Id"] = email.Id;
                row["Name"] = email.Email;
                row["ParentId"] = email.ParentId;
                row["ObjectId"] = email.ObjectId;
                row["State"] = (int)email.State;
                row["NodeLevel"] = email.NodeLevel == NodeLevel.Undefined ? NodeLevel_ExchangeOnlineMailbox : (int)email.NodeLevel;
                row["UserName"] = email.Username;
                row["Password"] = null;
                row["SPVersion"] = email.SPVersion;
                row["ServiceUrl"] = email.ServiceUrl;
                row["TenantId"] = email.TenantId;
                row["AuthType"] = (int)email.ConnectionType;
                row["ServiceAccountId"] = email.ServiceAccountId;
                row["AppType"] = (int)email.AppType;
                row["MailboxType"] = (int)email.MailboxType;
                row["ScanSource"] = (int)email.ScanSource;
                row["FromDAO"] = email.FromDAO;
                row["CreateTime"] = DateTime.UtcNow.Ticks;
                row["ModifiedDate"] = DateTime.UtcNow.Ticks;
                table.Rows.Add(row);
            }

            return table;
        }

        private List<string> GetExistIDs(IEnumerable<string> ids)
        {
            List<string> idList = new List<string>();
            DatabaseUtility.BatchOperation<string>(ids, (batchIds) =>
            {
                ExecuteWithRetry(context =>
                {
                    List<SqlParameter> paras = null;
                    string sql = $"SELECT Id From {GetFullTableName(context)} Where Id in {DatabaseUtility.BuildInClause(batchIds, out paras)}";
                    idList.AddRange(context.Database.SqlQuery<string>(sql, paras.ToArray()).ToList());
                });
            });
            return idList;
        }

        public void DeleteEmails(List<string> ids)
        {
            ThrowUtil.ThrowIfNull(ids, "ids");
            logger.Debug("DeleteEmails id count {0}", ids.Count);
            if(ids.Count == 0)
            {
                return;
            }
            DatabaseUtility.BatchOperation(ids, batchIds =>
            {
                ExecuteWithRetry(context =>
                {
                    var inSql = DatabaseUtility.BuildInClause(batchIds, out var inParams);
                    string sql = $"DELETE FROM {GetFullTableName(context)} WHERE Id IN {inSql};";
                    context.Database.ExecuteSqlCommand(sql, inParams.ToArray());
                });
            });
        }

        public List<RMMailbox> GetAllContainers()
        {
            return ExecuteWithRetry(context =>
            {
                return context.RMMailboxes.AsNoTracking().Where(item => string.IsNullOrEmpty(item.ParentId)).ToList();
            });
        }

        public void UpdateContainers(List<RMMailbox> containers)
        {
            using(var context = GetNewContext())
            {
                foreach(var contaienr in containers)
                {
                    ApplyCurrentValues(context, contaienr);
                }
            }
        }

        public void DeleteEmailGroups(List<string> ids)
        {
            DeleteEmails(ids);
        }

        public void ClearAll()
        {
            logger.Debug("Clear All Mailboxes");
            ExecuteWithRetry(context =>
            {
                context.Database.CommandTimeout = 600;
                string sql = $"DELETE FROM {GetFullTableName(context)}";
                context.Database.ExecuteSqlCommand(sql);
            });
        }

        public void CreateEmailGroups(List<EmailAccountGroupDto> emailGroups)
        {
            ThrowUtil.ThrowIfNull(emailGroups, "EmailAccountGroupDto");
            if (emailGroups.Count == 0)
            {
                return;
            }

            var existsIDs = GetExistIDs(emailGroups.Select(s => s.id));
            var nodeIDs = new HashSet<string>();
            var addingGroups = new List<RMMailbox>();
            var containerNames = new List<string>();
            foreach (var mailGroup in emailGroups)
            {
                var nodeId = mailGroup.id.ToLower();
                if (!nodeIDs.Add(nodeId))
                {
                    logger.Warn($"Repeat mail id: {mailGroup.id}, {mailGroup.Name}");
                    continue;
                }
                if (existsIDs.Contains(nodeId))
                {
                    logger.Warn($"Exists mail id: {mailGroup.id}, {mailGroup.Name}");
                    continue;
                }

                RMMailbox domain = ConvertToDomain(mailGroup);
                domain.CreateTime = DateTime.UtcNow.Ticks;
                addingGroups.Add(domain);
                containerNames.Add(mailGroup.Name);
            }

            if (addingGroups.Count == 0)
            {
                return;
            }

            List<string> groupNames = new List<string>();
            ExecuteWithRetry(context =>
            {
                context.RMMailboxes.AddRange(addingGroups);
                context.SaveChanges();
            });
            logger.Debug("Create EmailGroups: {0}", string.Join(", ", groupNames));
        }

        public void UpdateEmailGroups(List<EmailAccountGroupDto> emailGroups)
        {
            ThrowUtil.ThrowIfNull(emailGroups, "EmailAccountGroupDto");
            foreach(var emailGroup in emailGroups)
            {
                ExecuteWithRetry(context =>
                {
                    var existGroup = context.RMMailboxes.FirstOrDefault(item => item.Id == emailGroup.id);
                    existGroup.Name = emailGroup.Name;
                    ApplyCurrentValues(context, existGroup);
                });
            }
        }

        private RMMailbox ConvertToDomain(EmailAccountGroupDto emailGroup)
        {
            if (emailGroup == null)
            {
                return null;
            }
            var domain = new RMMailbox();
            domain.Id = emailGroup.id;
            domain.Name = emailGroup.Name;
            var level = (int)emailGroup.NodeLevel;
            if (level == 0)
            {
                level = NodeLevel_ExchangeOnlineMailboxGroup;
            }
            domain.NodeLevel = level;
            domain.FromDAO = emailGroup.FromDAO;
            domain.ModifiedDate = DateTime.UtcNow.Ticks;
            domain.AosId = emailGroup.AosId;
            return domain;
        }

        private EmailAccountDto ConvertToEmailAccountDto(RMMailbox domain)
        {
            if (domain == null)
            {
                return null;
            }
            var dto = new EmailAccountDto();
            dto.Id = domain.Id;
            dto.Email = domain.Name;
            dto.ParentId = domain.ParentId;
            dto.State = (EmailAccountState)domain.State;
            dto.Username = domain.UserName;
            dto.ObjectId = domain.ObjectId;
            dto.SPVersion = domain.SPVersion;
            dto.ServiceUrl = domain.ServiceUrl;
            dto.NodeLevel = (NodeLevel)domain.NodeLevel;
            dto.ConnectionType = (GCommon.Contract.CentralAdmin.Object.BposConnectionType)domain.AuthType;
            dto.TenantId = domain.TenantId;
            dto.ServiceAccountId = domain.ServiceAccountId;
            dto.AppType = (GCommon.Contract.CentralAdmin.Object.AppType)domain.AppType;
            dto.MailboxType = (MailboxType)domain.MailboxType;
            dto.ScanSource = (MailboxScanSource)domain.ScanSource;
            dto.FromDAO = domain.FromDAO;
            return dto;
        }

        public void DeleteMailboxByNames(List<string> names)
        {
            ThrowUtil.ThrowIfNull(names, nameof(names));
            DatabaseUtility.BatchOperation<string>(names, (batchNames) =>
            {
                ExecuteWithRetry(context =>
                {
                    List<SqlParameter> paras = null;
                    string sql = $"Delete From {GetFullTableName(context)} Where Name in {DatabaseUtility.BuildInClause(batchNames, out paras)}";
                    context.Database.ExecuteSqlCommand(sql, paras.ToArray());
                });
            });
        }

        public void DeleteMailboxByIDs(List<string> ids)
        {
            ThrowUtil.ThrowIfNull(ids, nameof(ids));
            DatabaseUtility.BatchOperation<string>(ids, (batchIds) =>
            {
                ExecuteWithRetry(context =>
                {
                    List<SqlParameter> paras = null;
                    string sql = $"Delete From {GetFullTableName(context)} Where Id in {DatabaseUtility.BuildInClause(batchIds, out paras)}";
                    context.Database.ExecuteSqlCommand(sql, paras.ToArray());
                });
            });
        }

        public void DeleteMailboxByParentIds(List<string> parentIds)
        {
            ThrowUtil.ThrowIfNull(parentIds, nameof(parentIds));
            var ids = GetMailboxIdsByParentIds(parentIds);
            if(ids == null || ids.Count == 0)
            {
                return;
            }
            DeleteMailboxByIDs(ids);
        }

        public Dictionary<string, string> GetMailboxNamesByParentIds(List<string> parentIds)
        {
            ThrowUtil.ThrowIfNull(parentIds, nameof(parentIds));
            Dictionary<string, string> mailboxNames = new Dictionary<string, string>();
            DatabaseUtility.BatchOperation<string>(parentIds, (batchIds) =>
            {
                ExecuteWithRetry(context =>
                {
                    context.Database.CommandTimeout = 600;
                    List<SqlParameter> paras = null;
                    string sql = $"Select name as NodeName, ParentId from {GetFullTableName(context)} where parentId in {DatabaseUtility.BuildInClause(batchIds, out paras)}";
                    foreach (var item in context.Database.SqlQuery<RemoteNodeBaseInfo>(sql, paras.ToArray()).ToList())
                    {
                        mailboxNames[item.NodeName] = item.ParentId;
                    }
                });
            });
            return mailboxNames;
        }

        public Dictionary<string, string> GetParentNamesByMailboxes(IEnumerable<string> mailboxNames, bool includeO365Group = false)
        {
            ThrowUtil.ThrowIfNull(mailboxNames, nameof(mailboxNames));
            Dictionary<string, string> parentNames = new Dictionary<string, string>();
            DatabaseUtility.BatchOperation<string>(mailboxNames, (batchNames) =>
            {
                ExecuteWithRetry(context =>
                {
                    List<SqlParameter> paras = null;
                    string sql =
$@"Select a.name as ParentId, b.name as NodeName
  from {GetFullTableName(context)} a
  join {GetFullTableName(context)} b on a.Id=b.ParentId 
  where b.Name in {DatabaseUtility.BuildInClause(batchNames, out paras)}";
                    if(!includeO365Group)
                    {
                        sql += " and a.NodeLevel != " + NodeLevel_ExchangeOnlineO365GroupsGroup;
                    }
                    foreach (var item in context.Database.SqlQuery<RemoteNodeBaseInfo>(sql, paras.ToArray()).ToList())
                    {
                        parentNames[item.NodeName] = item.ParentId;
                    }
                });
            });
            return parentNames;
        }

        public List<string> GetMailboxIdsByParentIds(List<string> parentIds)
        {
            ThrowUtil.ThrowIfNull(parentIds, nameof(parentIds));
            List<string> mailboxIds = new List<string>();
            DatabaseUtility.BatchOperation<string>(parentIds, (batchIds) =>
            {
                ExecuteWithRetry(context =>
                {
                    context.Database.CommandTimeout = 300;
                    List<SqlParameter> paras = null;
                    string sql = $"Select id from {GetFullTableName(context)} where parentId in {DatabaseUtility.BuildInClause(batchIds, out paras)}";
                    mailboxIds.AddRange(context.Database.SqlQuery<string>(sql, paras.ToArray()).ToList());
                });
            });
            return mailboxIds;
        }

        public List<RemoteNodePara> GetRemoteMailGroupNodes()
        {
            List<RemoteNodePara> result = new List<RemoteNodePara>();
            ExecuteWithRetry(context =>
            {
                var groups = context.RMMailboxes.Where(m => m.NodeLevel == NodeLevel_ExchangeOnlineMailboxGroup || m.NodeLevel == NodeLevel_ExchangeOnlineO365GroupsGroup);
                foreach (var group in groups)
                {
                    result.Add(new RemoteNodePara() {
                        NodeId = group.Id,
                        NodeName = group.Name,
                        NodeLevel = (NodeLevel)group.NodeLevel,
                        AosId = group.AosId
                    });
                }
            });
            return result;
        }

        public List<EmailAccountDto> GetMailboxesByEmailAddressName(List<string> addressNameList)
        {
            var result = new List<EmailAccountDto>();
            DatabaseUtility.BatchOperation<string>(addressNameList, (batchNames) =>
            {
                ExecuteWithRetry(context =>
                {
                    List<SqlParameter> paras = null;
                    string sql = $"Select * from {GetFullTableName(context)} where name in {DatabaseUtility.BuildInClause(batchNames, out paras)}";
                    result.AddRange(
                        context.Database.SqlQuery<RMMailbox>(sql, paras.ToArray()).ToList()
                            .Select(ConvertToEmailAccountDto));
                });
            });
            logger.Debug("Get exist mailboxes ids in DB by ids ,result account:{0}", result.Count);
            return result;
        }

        public List<SyncRemoteNodePara> GetAllMailboxNodesByPage(int pageIndex, int pageSize)
        {
            List<SyncRemoteNodePara> result = new List<SyncRemoteNodePara>();
            ExecuteWithRetry(context =>
            {
                context.Database.CommandTimeout = 600;
                var items = context.RMMailboxes
                .Where(m => m.NodeLevel == NodeLevel_ExchangeOnlineMailbox || m.NodeLevel == NodeLevel_ExchangeOnlineO365Group)
                .OrderBy(m => m.CreateTime)
                .Skip(pageIndex * pageSize)
                .Take(pageSize);
                foreach (var item in items)
                {
                    var authType = (BposConnectionType)item.AuthType;
                    result.Add(new SyncRemoteNodePara()
                    {
                        NodeName = item.Name,
                        ParentId = item.ParentId,
                        AuthType = authType,
                        AppType = (AppType)item.AppType,
                        ServiceAccountId = item.ServiceAccountId ?? string.Empty,
                        ScanSource = (RemoteNodeScanSource)item.ScanSource,
                        TenantId = item.TenantId,
                        UserName = string.IsNullOrEmpty(item.UserName) ? string.Empty : (
                            authType == BposConnectionType.ServiceAccount ? string.Empty : item.UserName),
                        NodeLevel = (NodeLevel)item.NodeLevel,
                        ObjectId = item.ObjectId,
                    });
                }
            });
            return result;
        }

        public List<SyncRemoteNodePara> GetAllMailboxNodes()
        {
            List<SyncRemoteNodePara> result = new List<SyncRemoteNodePara>();
            ExecuteWithRetry(context =>
            {
                context.Database.CommandTimeout = 600;
                var items = context.RMMailboxes.Where(m => m.NodeLevel == NodeLevel_ExchangeOnlineMailbox || m.NodeLevel == NodeLevel_ExchangeOnlineO365Group);
                foreach (var item in items)
                {
                    var authType = (BposConnectionType)item.AuthType;
                    result.Add(new SyncRemoteNodePara()
                    {
                        NodeName = item.Name,
                        ParentId = item.ParentId,
                        AuthType = authType,
                        AppType = (AppType)item.AppType,
                        ServiceAccountId = item.ServiceAccountId ?? string.Empty,
                        ScanSource = (RemoteNodeScanSource)item.ScanSource,
                        TenantId = item.TenantId,
                        UserName = string.IsNullOrEmpty(item.UserName) ? string.Empty : (
                            authType == BposConnectionType.ServiceAccount ? string.Empty : item.UserName),
                        NodeLevel = (NodeLevel)item.NodeLevel,
                        ObjectId = item.ObjectId,
                    });
                }
            });
            return result;
        }
        public List<EmailAccountDto> GetAllMailboxNodesWithId()
        {
            List<EmailAccountDto> result = new List<EmailAccountDto>();
            ExecuteWithRetry(context =>
            {
                context.Database.CommandTimeout = 600;
                var items = context.RMMailboxes.Where(m => m.NodeLevel == NodeLevel_ExchangeOnlineMailbox || m.NodeLevel == NodeLevel_ExchangeOnlineO365Group);
                foreach (var item in items)
                {
                    var authType = (BposConnectionType)item.AuthType;
                    result.Add(ConvertToEmailAccountDto(item));
                }
            });
            return result;
        }
        public int GetMailboxNodesCount()
        {
            return ExecuteWithRetry(context =>
            {
                return context.RMMailboxes.Count();
            });
        }

        public void UpdateSyncMails(List<SyncRemoteNodePara> mails)
        {
            var nameAndNodeMap = new Dictionary<string, SyncRemoteNodePara>();
            mails.ForEach(m =>
            {
                if (!string.IsNullOrEmpty(m.NodeName))
                {
                    nameAndNodeMap[m.NodeName.ToLowerInvariant()] = m;
                }
            });
            SyncRemoteNodePara tempItem = null;
            DatabaseUtility.BatchOperation<string>(nameAndNodeMap.Keys, (batchNames) =>
            {
                batchNames = DatabaseUtility.EscapeSqlParam(batchNames);
                ExecuteWithRetry(context =>
                {
                    foreach (var mail in context.RMMailboxes.Where(m => batchNames.Contains(m.Name)))
                    {
                        if (!nameAndNodeMap.TryGetValue(mail.Name.ToLowerInvariant(), out tempItem))
                        {
                            continue;
                        }
                        mail.ParentId = tempItem.ParentId;
                        mail.NodeLevel = (int)tempItem.NodeLevel;
                        mail.AppType = (int)tempItem.AppType;
                        mail.AuthType = (int)tempItem.AuthType;
                        mail.ServiceAccountId = tempItem.ServiceAccountId;
                        mail.ScanSource = (int)tempItem.ScanSource;
                        mail.TenantId = tempItem.TenantId;
                        mail.ObjectId = tempItem.ObjectId;
                        mail.ModifiedDate = DateTime.UtcNow.Ticks;
                    }
                    context.SaveChanges();
                });
            });
        }

        public RemoteNodePara GetMailGroupByNameAndNodeLevel(string name, int nodeLevel)
        {
            return ExecuteWithRetry(context =>
            {
                var result = context.RMMailboxes
                .AsNoTracking()
                .Where(m => m.NodeLevel == nodeLevel && m.Name == name)
                .Select(m => new RemoteNodePara()
                {
                    NodeId = m.Id,
                    NodeName = m.Name,
                    NodeLevel = (NodeLevel)m.NodeLevel
                }).FirstOrDefault();
                return result;
            });
        }

        public RemoteNodePara GetMailGroupByAosIdAndNodeLevel(string aosId, int nodeLevel)
        {
            return ExecuteWithRetry(context =>
            {
                var result = context.RMMailboxes
                .AsNoTracking()
                .Where(m => m.NodeLevel == nodeLevel && m.AosId == aosId)
                .Select(m => new RemoteNodePara()
                {
                    NodeId = m.Id,
                    NodeName = m.Name,
                    NodeLevel = (NodeLevel)m.NodeLevel,
                    AosId = m.AosId
                }).FirstOrDefault();
                return result;
            });
        }

        public List<EmailAccountDto> GetEmailsByEmailGroupIdForBrowse(string emailGroupId)
        {
            ThrowUtil.ThrowIfNullOrEmpty(emailGroupId, "emailGroupId");
            return ExecuteWithRetry(context =>
            {
                var dMailboxes = context.RMMailboxes.AsNoTracking().Where(m => m.ParentId == emailGroupId && m.NodeLevel == NodeLevel_ExchangeOnlineMailbox).ToList();
                var emails = dMailboxes.ConvertAll(m => ConvertToEmailAccountDto(m));
                return emails;
            });
        }        
        
        public EmailAccountDto GetEmailByEmailGroupId(string emailGroupId)
        {
            ThrowUtil.ThrowIfNullOrEmpty(emailGroupId, "emailGroupId");
            return ExecuteWithRetry(context =>
            {
                var dMailbox = context.RMMailboxes.AsNoTracking().Where(m => m.ParentId == emailGroupId && m.NodeLevel == NodeLevel_ExchangeOnlineMailbox).FirstOrDefault();
                var email = ConvertToEmailAccountDto(dMailbox);
                return email;
            });
        }

        public EmailAccountDto GetEmailById(string id)
        {
            ThrowUtil.ThrowIfNull(id, "id");
            logger.Debug("GetEmailById id {0}.", id);
            return ExecuteWithRetry(context =>
            {
                var node = context.RMMailboxes.AsNoTracking().Where(m => m.Id == id).FirstOrDefault();
                return ConvertToEmailAccountDto(node);
            });
        }

        public EmailAccountDto GetEmailByEmailAddress(string emailAddress) 
        {
            ThrowUtil.ThrowIfNull(emailAddress, "emailAddress");
            logger.Debug($"GetEmailByEmailAddress:{emailAddress}.");
            return ExecuteWithRetry(context =>
            {
                var node = context.RMMailboxes.AsNoTracking().Where(m => m.Name == emailAddress).FirstOrDefault();
                return ConvertToEmailAccountDto(node);
            });
        }

        public EmailAccountDto GetEmailGroupById(string id)
        {
            ThrowUtil.ThrowIfNull(id, "id");
            logger.Debug("GetEmailById id {0}.", id);
            return ExecuteWithRetry(context =>
            {
                var node = context.RMMailboxes.AsNoTracking().Where(m => m.Id == id && m.NodeLevel == NodeLevel_ExchangeOnlineMailboxGroup).FirstOrDefault();
                return ConvertToEmailAccountDto(node);
            });
        }

        public List<EmailAccountDto> GetEmailByIds(List<string> ids)
        {
            return ExecuteWithRetry(context =>
            {
                var nodes = context.RMMailboxes.AsNoTracking().Where(m => ids.Contains(m.ObjectId)).ToList();
                return nodes.ConvertAll(ConvertToEmailAccountDto);
            });
        }

        public EmailAccountDto GetO365GroupById(string id)
        {
            ThrowUtil.ThrowIfNull(id, "id");
            logger.Debug("GetO365GroupById, id is {0}.", id);
            return ExecuteWithRetry(context =>
            {
                var node = context.RMMailboxes.AsNoTracking().Where(m => m.Id == id && m.NodeLevel == NodeLevel_ExchangeOnlineO365Group).FirstOrDefault();
                return ConvertToEmailAccountDto(node);
            });
        }
    }
}