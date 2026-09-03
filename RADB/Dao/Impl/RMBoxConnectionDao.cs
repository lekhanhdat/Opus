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
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMBoxConnectionDao : BaseDao<RMBoxConnection>, IRMBoxConnectionDao
    {
        public bool Exists(Guid id)
        {
            using (var context = GetNewContext())
            {
                return context.RMBoxConnections.Any(item => item.Id == id);
            }
        }

        public bool Add(RMBoxConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            connection.Id = Guid.NewGuid();
            return Create(connection) != null;
        }

        public bool Modify(RMBoxConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            using (var context = GetNewContext())
            {
                return ApplyCurrentValues(context, connection);
            }
        }

        public List<RMBoxConnection> GetAll()
        {
            using (var context = GetNewContext())
            {
                var query = from connection in context.RMBoxConnections
                            join connectionGroup in context.RMBoxConnectionGroups
                            on connection.ConnectionGroupId equals connectionGroup.Id
                            into connectionGroups
                            from subConnectionGroup in connectionGroups.DefaultIfEmpty()
                            select new
                            {
                                Connection = connection,
                                Group = subConnectionGroup
                            };
                var queryList = query.OrderByDescending(item => item.Connection.Modified).ToList();
                return queryList.ConvertAll(item => new RMBoxConnection
                {
                    Id = item.Connection.Id,
                    Name = item.Connection.Name,
                    Description = item.Connection.Description,
                    AuthenticationType = item.Connection.AuthenticationType,
                    ClientId = item.Connection.ClientId,
                    ClientSecret = item.Connection.ClientSecret,
                    EmailAddress = item.Connection.EmailAddress,
                    EnterpriseId = item.Connection.EnterpriseId,
                    JsonFileContent = item.Connection.JsonFileContent,
                    JsonFileName = item.Connection.JsonFileName,
                    Created = item.Connection.Created,
                    Modified = item.Connection.Modified,
                    CreatedBy = item.Connection.CreatedBy,
                    ModifiedBy = item.Connection.ModifiedBy,
                    ConnectionGroupId = item.Connection.ConnectionGroupId,
                    ConnectionGroup = item.Group
                });
            }
        }

        public async Task<bool> RemoveAsync(List<Guid> ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException("ids");
            }

            return await BatchDeleteAsync(item => ids.Contains(item.Id)) > 0;
        }

        public bool RemoveById(Guid id)
        {
            if (id == Guid.Empty || !Exists(id))
            {
                throw new Exception($"Can't find connection [{id}] in Record.");
            }

            using (var context = GetNewContext())
            {
                return DeleteByKey(context, id);
            }
        }

        public bool Remove(RMBoxConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            return RemoveById(connection.Id);
        }

        public RMBoxConnection GetById(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new Exception($"Illegal connection id [{id}].");
            }

            using (var context = GetNewContext())
            {
                var query = from connection in context.RMBoxConnections
                            join connectionGroup in context.RMBoxConnectionGroups
                            on connection.ConnectionGroupId equals connectionGroup.Id
                            into connectionGroups
                            from subConnectionGroup in connectionGroups.DefaultIfEmpty()
                            where connection.Id == id
                            select new
                            {
                                Connection = connection,
                                Group = subConnectionGroup
                            };
                var item = query?.FirstOrDefault();
                ArgumentNullException.ThrowIfNull(item);
                return new RMBoxConnection
                {
                    Id = item.Connection.Id,
                    Name = item.Connection.Name,
                    Description = item.Connection.Description,
                    AuthenticationType = item.Connection.AuthenticationType,
                    ClientId = item.Connection.ClientId,
                    ClientSecret = item.Connection.ClientSecret,
                    EmailAddress = item.Connection.EmailAddress,
                    EnterpriseId = item.Connection.EnterpriseId,
                    JsonFileContent = item.Connection.JsonFileContent,
                    JsonFileName = item.Connection.JsonFileName,
                    Created = item.Connection.Created,
                    Modified = item.Connection.Modified,
                    CreatedBy = item.Connection.CreatedBy,
                    ModifiedBy = item.Connection.ModifiedBy,
                    ConnectionGroupId = item.Connection.ConnectionGroupId,
                    ConnectionGroup = item.Group,
                    RedirectUrl = item.Connection.RedirectUrl,
                    AccessToken = item.Connection.AccessToken,
                    RefreshToken = item.Connection.RefreshToken,
                };
            }
        }

        public RMBoxConnection GetByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception($"Illegal connection name [{name}].");
            }

            using (var context = GetNewContext())
            {
                var query = from connection in context.RMBoxConnections
                            join connectionGroup in context.RMBoxConnectionGroups
                            on connection.ConnectionGroupId equals connectionGroup.Id
                            into connectionGroups
                            from subConnectionGroup in connectionGroups.DefaultIfEmpty()
                            where connection.Name == name
                            select new
                            {
                                Connection = connection,
                                Group = subConnectionGroup
                            };
                var item = query.FirstOrDefault();
                if (item == null)
                {
                    return null;
                }
                return new RMBoxConnection
                {
                    Id = item.Connection.Id,
                    Name = item.Connection.Name,
                    Description = item.Connection.Description,
                    AuthenticationType = item.Connection.AuthenticationType,
                    ClientId = item.Connection.ClientId,
                    ClientSecret = item.Connection.ClientSecret,
                    EmailAddress = item.Connection.EmailAddress,
                    EnterpriseId = item.Connection.EnterpriseId,
                    JsonFileContent = item.Connection.JsonFileContent,
                    JsonFileName = item.Connection.JsonFileName,
                    Created = item.Connection.Created,
                    Modified = item.Connection.Modified,
                    CreatedBy = item.Connection.CreatedBy,
                    ModifiedBy = item.Connection.ModifiedBy,
                    ConnectionGroupId = item.Connection.ConnectionGroupId,
                    ConnectionGroup = item.Group
                };
            }
        }

        public List<RMBoxConnection> GetAllByIds(List<Guid> ids)
        {
            using (var context = GetNewContext())
            {
                var query = from connection in context.RMBoxConnections
                            join connectionGroup in context.RMBoxConnectionGroups
                            on connection.ConnectionGroupId equals connectionGroup.Id
                            into connectionGroups
                            from subConnectionGroup in connectionGroups.DefaultIfEmpty()
                            where ids.Contains(connection.Id)
                            select new
                            {
                                Connection = connection,
                                Group = subConnectionGroup
                            };
                var queryList = query.OrderByDescending(item => item.Connection.Modified).ToList();
                return queryList.ConvertAll(item => new RMBoxConnection
                {
                    Id = item.Connection.Id,
                    Name = item.Connection.Name,
                    Description = item.Connection.Description,
                    AuthenticationType = item.Connection.AuthenticationType,
                    ClientId = item.Connection.ClientId,
                    ClientSecret = item.Connection.ClientSecret,
                    EmailAddress = item.Connection.EmailAddress,
                    EnterpriseId = item.Connection.EnterpriseId,
                    JsonFileContent = item.Connection.JsonFileContent,
                    JsonFileName = item.Connection.JsonFileName,
                    Created = item.Connection.Created,
                    Modified = item.Connection.Modified,
                    CreatedBy = item.Connection.CreatedBy,
                    ModifiedBy = item.Connection.ModifiedBy,
                    ConnectionGroupId = item.Connection.ConnectionGroupId,
                    ConnectionGroup = item.Group
                });
            }
        }

        public List<RMBoxConnection> GetAllByConnectionGroup(Guid connectionGroupId)
        {
            if (connectionGroupId == Guid.Empty)
            {
                throw new Exception($"Illegal connection group id [{connectionGroupId}].");
            }

            using (var context = GetNewContext())
            {
                var query = from connection in context.RMBoxConnections
                            join connectionGroup in context.RMBoxConnectionGroups
                            on connection.ConnectionGroupId equals connectionGroup.Id
                            into connectionGroups
                            from subConnectionGroup in connectionGroups.DefaultIfEmpty()
                            where connection.ConnectionGroupId == connectionGroupId
                            select new
                            {
                                Connection = connection,
                                Group = subConnectionGroup
                            };
                var queryList = query.OrderByDescending(item => item.Connection.Modified).ToList();
                return queryList.ConvertAll(item => new RMBoxConnection
                {
                    Id = item.Connection.Id,
                    Name = item.Connection.Name,
                    Description = item.Connection.Description,
                    AuthenticationType = item.Connection.AuthenticationType,
                    ClientId = item.Connection.ClientId,
                    ClientSecret = item.Connection.ClientSecret,
                    EmailAddress = item.Connection.EmailAddress,
                    EnterpriseId = item.Connection.EnterpriseId,
                    JsonFileContent = item.Connection.JsonFileContent,
                    JsonFileName = item.Connection.JsonFileName,
                    Created = item.Connection.Created,
                    Modified = item.Connection.Modified,
                    CreatedBy = item.Connection.CreatedBy,
                    ModifiedBy = item.Connection.ModifiedBy,
                    ConnectionGroupId = item.Connection.ConnectionGroupId,
                    ConnectionGroup = item.Group
                });
            }
        }

        public List<Guid> GetConnectionIdsByConnectionGroups(IEnumerable<Guid> connectionGroupIds)
        {
            if (connectionGroupIds == null)
            {
                throw new ArgumentNullException(nameof(connectionGroupIds));
            }

            var validGroupIds = connectionGroupIds
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            if (!validGroupIds.Any())
            {
                return new List<Guid>();
            }

            const int batchSize = 500;
            var result = new List<Guid>();

            using (var context = GetNewContext())
            {
                for (int i = 0; i < validGroupIds.Count; i += batchSize)
                {
                    var batchIds = validGroupIds.Skip(i).Take(batchSize).ToList();

                    var query = context.RMBoxConnections
                        .Where(c => batchIds.Contains(c.ConnectionGroupId))
                        .Select(c => c.Id)
                        .OrderByDescending(id => id);  

                    result.AddRange(query.ToList());
                }
            }

            return result;
        }

        public List<RMBoxConnection> GetAllWithoutRelatedConnectionGroup()
        {
            using (var context = GetNewContext())
            {
                var query = from connection in context.RMBoxConnections
                            join connectionGroup in context.RMBoxConnectionGroups
                            on connection.ConnectionGroupId equals connectionGroup.Id
                            into connectionGroups
                            from subConnectionGroup in connectionGroups.DefaultIfEmpty()
                            where connection.ConnectionGroupId == Guid.Empty
                            select new
                            {
                                Connection = connection,
                                Group = subConnectionGroup
                            };
                var queryList = query.OrderByDescending(item => item.Connection.Modified).ToList();
                return queryList.ConvertAll(item => new RMBoxConnection
                {
                    Id = item.Connection.Id,
                    Name = item.Connection.Name,
                    Description = item.Connection.Description,
                    AuthenticationType = item.Connection.AuthenticationType,
                    ClientId = item.Connection.ClientId,
                    ClientSecret = item.Connection.ClientSecret,
                    EmailAddress = item.Connection.EmailAddress,
                    EnterpriseId = item.Connection.EnterpriseId,
                    JsonFileContent = item.Connection.JsonFileContent,
                    JsonFileName = item.Connection.JsonFileName,
                    Created = item.Connection.Created,
                    Modified = item.Connection.Modified,
                    CreatedBy = item.Connection.CreatedBy,
                    ModifiedBy = item.Connection.ModifiedBy,
                    ConnectionGroupId = item.Connection.ConnectionGroupId,
                    ConnectionGroup = item.Group
                });
            }
        }

        public bool ExistsByEnterpriseId(string enterpriseId, Guid connectionId)
        {
            using (var context = GetNewContext())
            {
                return context.RMBoxConnections
                              .Any(connection => connection.EnterpriseId == enterpriseId && connection.Id != connectionId);
            }
        }
    }
}
