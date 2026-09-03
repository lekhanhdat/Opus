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
using AvePoint.RA.Common.Util;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMBoxConnectionGroupDao : BaseDao<RMBoxConnectionGroup>, IRMBoxConnectionGroupDao
    {
        // Create new group and add connections to group
        public bool Add(RMBoxConnectionGroup connectionGroup)
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
                        var updateRelatedSql = $@"UPDATE [{context.SchemaName}].[RMBoxConnections] SET ConnectionGroupId = @groupId WHERE Id IN {connectionInSql} AND ConnectionGroupId = '{Guid.Empty}'";
                        var updateRelatedParemeter = new SqlParameter("@groupId", connectionGroup.Id);
                        inParams.Add(updateRelatedParemeter);

                        var affectedRows = context.Database.ExecuteSqlCommand(updateRelatedSql, inParams.ToArray());

                        // If affectedRows < connectionGroup.Connections.Count, it means at least one connection already belongs to another group
                        if (affectedRows < connectionGroup.Connections.Count)
                        {
                            throw new InvalidOperationException("At least one connection already belongs to another group. Please check again.");
                        }
                    }

                    var result = Create(connectionGroup, context);
                    transaction.Commit();
                    return result != null;
                }
            }
        }

        public RMBoxConnectionGroup GetById(Guid id)
        {
            using (var context = GetNewContext())
            {
                var query = from connectionGroup in context.RMBoxConnectionGroups
                            join connection in context.RMBoxConnections
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
                    var connectionGroup = item.FirstOrDefault()?.ConnectionGroup;
                    if(connectionGroup == null)
                    {
                        return new();
                    }
                    var connections = item.ConvertAll(innerItem => innerItem.Connection);
                    if (connections.FirstOrDefault() == null)
                    {
                        connections = new List<RMBoxConnection>();
                    }
                    return new RMBoxConnectionGroup
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

        public RMBoxConnectionGroup GetByName(string name)
        {
            using (var context = GetNewContext())
            {
                var query = from connectionGroup in context.RMBoxConnectionGroups
                            join connection in context.RMBoxConnections
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
                if (queryDictionary.Count == 0)
                {
                    return null;
                }
                var result = queryDictionary.Values.ToList().ConvertAll(item =>
                {
                    var connectionGroup = item.FirstOrDefault()?.ConnectionGroup;
                    if (connectionGroup == null)
                    {
                        return new();
                    }
                    var connections = item.ConvertAll(innerItem => innerItem.Connection);
                    if (connections.FirstOrDefault() == null)
                    {
                        connections = new List<RMBoxConnection>();
                    }
                    return new RMBoxConnectionGroup
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

        public List<RMBoxConnectionGroup> GetAll()
        {
            using (var context = GetNewContext())
            {
                var query = from connectionGroup in context.RMBoxConnectionGroups
                            join connection in context.RMBoxConnections
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
                    var connectionGroup = item.FirstOrDefault()?.ConnectionGroup;
                    if (connectionGroup == null)
                    {
                        return new();
                    }
                    var connections = item.ConvertAll(innerItem => innerItem.Connection);
                    if (connections.FirstOrDefault() == null)
                    {
                        connections = new List<RMBoxConnection>();
                    }
                    return new RMBoxConnectionGroup
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

        public List<RMBoxConnectionGroup> GetByIds(List<Guid> ids)
        {
            using (var context = GetNewContext())
            {
                var query = from connectionGroup in context.RMBoxConnectionGroups
                            join connection in context.RMBoxConnections
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
                    var connectionGroup = item.FirstOrDefault()?.ConnectionGroup;
                    if (connectionGroup == null)
                    {
                        return new();
                    }
                    var connections = item.ConvertAll(innerItem => innerItem.Connection);
                    if (connections.FirstOrDefault() == null)
                    {
                        connections = new List<RMBoxConnection>();
                    }
                    return new RMBoxConnectionGroup
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

        public bool Exists(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new Exception($"Illegal connection group id [{id}].");
            }

            using (var context = GetNewContext())
            {
                return context.RMBoxConnectionGroups.Any(item => item.Id == id);
            }

        }

        // Edit connection group (change name, description or modify the connections)
        public bool Modify(RMBoxConnectionGroup connectionGroup)
        {
            if (connectionGroup == null)
            {
                throw new ArgumentNullException("connectionGroup");
            }

            using (var context = GetNewContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                    // Check if any connections already belong to another group
                    if (connectionGroup.Connections.Count > 0)
                    {
                        var connectionInSql = DatabaseUtility.BuildInClause(
                            connectionGroup.Connections.Select(item => item.Id),
                            out var inParams
                        );

                        var checkSql = $@"SELECT COUNT(*) FROM [{context.SchemaName}].[RMBoxConnections] WHERE Id IN {connectionInSql} AND ConnectionGroupId != '{Guid.Empty}' AND ConnectionGroupId != @groupId";
                        var checkParameter = new SqlParameter("@groupId", connectionGroup.Id);
                        inParams.Add(checkParameter);

                        var existingGroupConnectionsCount = context.Database.SqlQuery<int>(checkSql, inParams.ToArray()).FirstOrDefault();

                        // If any connections are already in another group, throw an exception
                        if (existingGroupConnectionsCount > 0)
                        {
                            throw new InvalidOperationException("At least one connection already belongs to another group. Please check again.");
                        }
                    }

                    var removeRelatedSql = $"UPDATE [{context.SchemaName}].[RMBoxConnections] SET ConnectionGroupId = '{Guid.Empty}' WHERE ConnectionGroupId = @groupId";
                    var removeRelatedParameter = new SqlParameter("@groupId", connectionGroup.Id);
                    context.Database.ExecuteSqlCommand(removeRelatedSql, removeRelatedParameter);

                    if (connectionGroup.Connections.Count > 0)
                    {
                        var connectionInSql = DatabaseUtility.BuildInClause(
                            connectionGroup.Connections.Select(item => item.Id),
                            out var inParams
                        );

                        var updateRelatedSql = $@"UPDATE [{context.SchemaName}].[RMBoxConnections] SET ConnectionGroupId = @groupId WHERE Id IN {connectionInSql} AND ConnectionGroupId = '{Guid.Empty}'";
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

        public bool RemoveById(Guid id)
        {
            using (var context = GetNewContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    var removeRelatedSql = $"UPDATE [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].[RMBoxConnections] SET ConnectionGroupId = '{Guid.Empty}' WHERE ConnectionGroupId = @groupId";
                    var removeRelatedParameter = new SqlParameter("@groupId", id);
                    context.Database.ExecuteSqlCommand(removeRelatedSql, removeRelatedParameter);

                    var result = DeleteByKey(context, id);
                    transaction.Commit();
                    return result;
                }
            }

        }

        public bool Remove(RMBoxConnectionGroup connectionGroup)
        {
            if (connectionGroup == null)
            {
                throw new ArgumentNullException("connectionGroup");
            }

            return RemoveById(connectionGroup.Id);
        }

        public async Task<bool> RemoveByIdsAsync(List<Guid> ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException("ids");
            }

            using (var context = GetNewContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                    var inIdsSql = DatabaseUtility.BuildInClause(ids, out var inParams);
                    var removeRelatedSql = $"UPDATE [{context.SchemaName}].[RMBoxConnections] SET ConnectionGroupId = '{Guid.Empty}' WHERE ConnectionGroupId IN {inIdsSql}";
                    context.Database.ExecuteSqlCommand(removeRelatedSql, inParams.ToArray());

                    var effectRows = await BatchDeleteAsync(item => ids.Contains(item.Id), context);

                    transaction.Commit();
                    return effectRows > 0;
                }

            }

        }
    }
}
