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
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Util;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMAzureFileShareConnectionGroupDao : BaseDao<RMAzureFileShareConnectionGroup>, IRMAzureFileShareConnectionGroupDao
    {
        public bool Add(RMAzureFileShareConnectionGroup connectionGroup)
        {
            if (connectionGroup == null)
            {
                throw new ArgumentNullException("connectionGroup");
            }

            using(var context = GetNewContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    connectionGroup.Id = Guid.NewGuid();

                    if(connectionGroup.Connections.Count > 0)
                    {
                        var connectionInSql = DatabaseUtility.BuildInClause(
                            connectionGroup.Connections.Select(item => item.Id),
                            out var inParams
                        );
                        SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                        var updateRelatedSql = $"UPDATE [{context.SchemaName}].[RMAzureFileShareConnections] SET ConnectionGroupId = @groupId WHERE Id IN {connectionInSql}";
                        var updateRelatedParameter = new SqlParameter("@groupId", connectionGroup.Id);
                        inParams.Add(updateRelatedParameter);
                        context.Database.ExecuteSqlCommand(updateRelatedSql, inParams.ToArray());
                    }

                    var result = Create(connectionGroup, context);
                    transaction.Commit();
                    return result != null;
                }
            }
        }

        public bool Modify(RMAzureFileShareConnectionGroup connectionGroup)
        {
            if (connectionGroup == null)
            {
                throw new ArgumentNullException("connectionGroup");
            }

            using (var context = GetNewContext())
            {
                using(var transaction = context.Database.BeginTransaction())
                {
                    var removeRelatedSql = $"UPDATE [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].[RMAzureFileShareConnections] SET ConnectionGroupId = '{Guid.Empty}' WHERE ConnectionGroupId = @groupId";
                    var removeRelatedParameter = new SqlParameter("@groupId", connectionGroup.Id);
                    context.Database.ExecuteSqlCommand(removeRelatedSql, removeRelatedParameter);

                    if(connectionGroup.Connections.Count > 0)
                    {
                        var connectionInSql = DatabaseUtility.BuildInClause(
                            connectionGroup.Connections.Select(item => item.Id),
                            out var inParams
                        );

                        var updateRelatedSql = $"UPDATE [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].[RMAzureFileShareConnections] SET ConnectionGroupId = @groupId WHERE Id IN {connectionInSql}";
                        var updateRelatedParameter = new SqlParameter("@groupId", connectionGroup.Id);
                        inParams.Add(updateRelatedParameter);
                        context.Database.ExecuteSqlCommand(updateRelatedSql, inParams.ToArray());
                    }

                    var result = ApplyCurrentValues(context, connectionGroup);
                    transaction.Commit();
                    return result;
                }
            }
        }

        public bool Remove(Guid id)
        {
            using (var context = GetNewContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    var removeRelatedSql = $"UPDATE [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].[RMAzureFileShareConnections] SET ConnectionGroupId = '{Guid.Empty}' WHERE ConnectionGroupId = @groupId";
                    var removeRelatedParameter = new SqlParameter("@groupId", id);
                    context.Database.ExecuteSqlCommand(removeRelatedSql, removeRelatedParameter);

                    var result = DeleteByKey(context, id);
                    transaction.Commit();
                    return result;
                }
            }
        }

        public async Task<bool> RemoveAsync(List<Guid> ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException("ids");
            }

            using (var context = GetNewContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    var inIdsSql = DatabaseUtility.BuildInClause(ids, out var inParams);
                    SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                    var removeRelatedSql = $"UPDATE [{context.SchemaName}].[RMAzureFileShareConnections] SET ConnectionGroupId = '{Guid.Empty}' WHERE ConnectionGroupId IN {inIdsSql}";
                    context.Database.ExecuteSqlCommand(removeRelatedSql, inParams.ToArray());

                    var effectRows = await BatchDeleteAsync(item => ids.Contains(item.Id), context);

                    transaction.Commit();
                    return effectRows > 0;
                }
                
            }
        }

        public bool Remove(RMAzureFileShareConnectionGroup connectionGroup)
        {
            if (connectionGroup == null)
            {
                throw new ArgumentNullException("connectionGroup");
            }

            return Remove(connectionGroup.Id);
        }

        public bool Has(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new Exception($"Illegal connection group id [{id}].");
            }

            using (var context = GetNewContext())
            {
                return context.RMAzureFileShareConnectionGroups.Any(item => item.Id == id);
            }
        }

        public RMAzureFileShareConnectionGroup Get(string name)
        {
            using (var context = GetNewContext())
            {
                var query = from connectionGroup in context.RMAzureFileShareConnectionGroups
                            join connection in context.RMAzureFileShareConnections
                            on connectionGroup.Id equals connection.ConnectionGroupId
                            into connections
                            from subConnection in connections.DefaultIfEmpty()
                            where connectionGroup.Name == name
                            select new
                            {
                                ConnectionGroup = connectionGroup,
                                Connection = subConnection
                            };
                var queryDictionary = query.GroupBy(item => item.ConnectionGroup.Id).ToDictionary(item => item.Key, item => item.ToList());
                if(queryDictionary.Count == 0)
                {
                    return null;
                }
                var result = queryDictionary.Values.ToList().ConvertAll(item =>
                {
                    var connectionGroup = item.First().ConnectionGroup;
                    var connections = item.ConvertAll(innerItem => innerItem.Connection);
                    if (connections.FirstOrDefault() == null)
                    {
                        connections = new List<RMAzureFileShareConnection>();
                    }
                    return new RMAzureFileShareConnectionGroup
                    {
                        Id = connectionGroup.Id,
                        Name = connectionGroup.Name,
                        Description = connectionGroup.Description,
                        Created = connectionGroup.Created,
                        Modified = connectionGroup.Modified,
                        CreatedBy = connectionGroup.CreatedBy,
                        ModifiedBy = connectionGroup.ModifiedBy,
                        Connections = connections
                    };
                });

                return result.FirstOrDefault();
            }
        }

        public RMAzureFileShareConnectionGroup Get(Guid id)
        {
            using (var context = GetNewContext())
            {
                var query = from connectionGroup in context.RMAzureFileShareConnectionGroups
                            join connection in context.RMAzureFileShareConnections
                            on connectionGroup.Id equals connection.ConnectionGroupId
                            into connections
                            from subConnection in connections.DefaultIfEmpty()
                            where connectionGroup.Id == id
                            select new
                            {
                                ConnectionGroup = connectionGroup,
                                Connection = subConnection
                            };
                var queryDictionary = query.GroupBy(item => item.ConnectionGroup.Id).ToDictionary(item => item.Key, item => item.ToList());
                var result = queryDictionary.Values.ToList().ConvertAll(item =>
                {
                    var connectionGroup = item.First().ConnectionGroup;
                    var connections = item.ConvertAll(innerItem => innerItem.Connection);
                    if (connections.FirstOrDefault() == null)
                    {
                        connections = new List<RMAzureFileShareConnection>();
                    }
                    return new RMAzureFileShareConnectionGroup
                    {
                        Id = connectionGroup.Id,
                        Name = connectionGroup.Name,
                        Description = connectionGroup.Description,
                        Created = connectionGroup.Created,
                        Modified = connectionGroup.Modified,
                        CreatedBy = connectionGroup.CreatedBy,
                        ModifiedBy = connectionGroup.ModifiedBy,
                        Connections = connections
                    };
                });

                return result.FirstOrDefault();
            }
        }

        public List<RMAzureFileShareConnectionGroup> GetAll()
        {
            using (var context = GetNewContext())
            {
                var query = from connectionGroup in context.RMAzureFileShareConnectionGroups
                            join connection in context.RMAzureFileShareConnections
                            on connectionGroup.Id equals connection.ConnectionGroupId
                            into connections
                            from subConnection in connections.DefaultIfEmpty()
                            select new
                            {
                                ConnectionGroup = connectionGroup,
                                Connection = subConnection
                            };
                var queryDictionary = query.GroupBy(item => item.ConnectionGroup.Id).ToDictionary(item => item.Key, item => item.ToList());
                var result = queryDictionary.Values.ToList().ConvertAll(item =>
                {
                    var connectionGroup = item.First().ConnectionGroup;
                    var connections = item.ConvertAll(innerItem => innerItem.Connection);
                    if(connections.FirstOrDefault() == null)
                    {
                        connections = new List<RMAzureFileShareConnection>();
                    }
                    connections.ForEach(conn => conn.AccountKey = "");
                    return new RMAzureFileShareConnectionGroup
                    {
                        Id = connectionGroup.Id,
                        Name = connectionGroup.Name,
                        Description = connectionGroup.Description,
                        Created = connectionGroup.Created,
                        Modified = connectionGroup.Modified,
                        CreatedBy = connectionGroup.CreatedBy,
                        ModifiedBy = connectionGroup.ModifiedBy,
                        Connections = connections
                    };
                });

                return result.OrderByDescending(item => item.Modified).ToList();
            }
        }

        public List<RMAzureFileShareConnectionGroup> GetAll(List<Guid> ids)
        {
            using (var context = GetNewContext())
            {
                var query = from connectionGroup in context.RMAzureFileShareConnectionGroups
                            join connection in context.RMAzureFileShareConnections
                            on connectionGroup.Id equals connection.ConnectionGroupId
                            into connections
                            from subConnection in connections.DefaultIfEmpty()
                            where ids.Contains(connectionGroup.Id)
                            select new
                            {
                                ConnectionGroup = connectionGroup,
                                Connection = subConnection
                            };
                var queryDictionary = query.GroupBy(item => item.ConnectionGroup.Id).ToDictionary(item => item.Key, item => item.ToList());
                var result = queryDictionary.Values.ToList().ConvertAll(item =>
                {
                    var connectionGroup = item.First().ConnectionGroup;
                    var connections = item.ConvertAll(innerItem => innerItem.Connection);
                    if (connections.FirstOrDefault() == null)
                    {
                        connections = new List<RMAzureFileShareConnection>();
                    }
                    return new RMAzureFileShareConnectionGroup
                    {
                        Id = connectionGroup.Id,
                        Name = connectionGroup.Name,
                        Description = connectionGroup.Description,
                        Created = connectionGroup.Created,
                        Modified = connectionGroup.Modified,
                        CreatedBy = connectionGroup.CreatedBy,
                        ModifiedBy = connectionGroup.ModifiedBy,
                        Connections = connections
                    };
                });

                return result.OrderByDescending(item => item.Modified).ToList();
            }
        }
    }
}
