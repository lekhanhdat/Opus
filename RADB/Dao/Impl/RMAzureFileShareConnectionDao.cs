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
using AvePoint.GCommon.GraphAPI;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMAzureFileShareConnectionDao : BaseDao<RMAzureFileShareConnection>, IRMAzureFileShareConnectionDao
    {
        public bool Add(RMAzureFileShareConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            connection.Id = Guid.NewGuid();
            return Create(connection) != null;
        }

        public bool Modify(RMAzureFileShareConnection connection)
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

        public bool Remove(Guid id)
        {
            if (id == Guid.Empty || !Has(id))
            {
                throw new Exception($"Can't find connection [{id}] in Record.");
            }

            using (var context = GetNewContext())
            {
                return DeleteByKey(context, id);
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

        public bool Remove(RMAzureFileShareConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            return Remove(connection.Id);
        }

        public bool Has(Guid id)
        {
            using (var context = GetNewContext())
            {
                return context.RMAzureFileShareConnections.Any(item => item.Id == id);
            }
        }

        public RMAzureFileShareConnection Get(string name)
        {
            if(string.IsNullOrEmpty(name))
            {
                throw new Exception($"Illegal connection name [{name}].");
            }

            using (var context = GetNewContext())
            {
                var query = from connection in context.RMAzureFileShareConnections
                            join connectionGroup in context.RMAzureFileShareConnectionGroups
                            on connection.ConnectionGroupId equals connectionGroup.Id
                            into connectionGroups
                            from subConnectionGrup in connectionGroups.DefaultIfEmpty()
                            where connection.Name == name
                            select new
                            {
                                Connection = connection,
                                Group = subConnectionGrup
                            };
                var item = query.FirstOrDefault();
                if(item == null)
                {
                    return null;
                }
                return new RMAzureFileShareConnection
                {
                    Id = item.Connection.Id,
                    Name = item.Connection.Name,
                    Description = item.Connection.Description,
                    AccessEndPoint = item.Connection.AccessEndPoint,
                    FileShareName = item.Connection.FileShareName,
                    AccountName = item.Connection.AccountName,
                    AccountKey = item.Connection.AccountKey,
                    Created = item.Connection.Created,
                    Modified = item.Connection.Modified,
                    CreatedBy = item.Connection.CreatedBy,
                    ModifiedBy = item.Connection.ModifiedBy,
                    ConnectionGroupId = item.Connection.ConnectionGroupId,
                    ConnectionGroup = item.Group
                };
            }
        }

        public RMAzureFileShareConnection Get(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new Exception($"Illegal connection id [{id}].");
            }

            using (var context = GetNewContext())
            {
                var query = from connection in context.RMAzureFileShareConnections
                            join connectionGroup in context.RMAzureFileShareConnectionGroups
                            on connection.ConnectionGroupId equals connectionGroup.Id
                            into connectionGroups
                            from subConnectionGrup in connectionGroups.DefaultIfEmpty()
                            where connection.Id == id
                            select new
                            {
                                Connection = connection,
                                Group = subConnectionGrup
                            };
                if(query == null)
                {
                    throw new ArgumentNullException(nameof(query));
                }
                var item = query.First();
                return new RMAzureFileShareConnection
                {
                    Id = item.Connection.Id,
                    Name = item.Connection.Name,
                    Description = item.Connection.Description,
                    AccessEndPoint = item.Connection.AccessEndPoint,
                    FileShareName = item.Connection.FileShareName,
                    AccountName = item.Connection.AccountName,
                    AccountKey = item.Connection.AccountKey,
                    Created = item.Connection.Created,
                    Modified = item.Connection.Modified,
                    CreatedBy = item.Connection.CreatedBy,
                    ModifiedBy = item.Connection.ModifiedBy,
                    ConnectionGroupId = item.Connection.ConnectionGroupId,
                    ConnectionGroup = item.Group
                };
            }
        }

        public List<RMAzureFileShareConnection> GetAll()
        {
            using (var context = GetNewContext())
            {
                var query = from connection in context.RMAzureFileShareConnections
                            join connectionGroup in context.RMAzureFileShareConnectionGroups
                            on connection.ConnectionGroupId equals connectionGroup.Id
                            into connectionGroups
                            from subConnectionGrup in connectionGroups.DefaultIfEmpty()
                            select new
                            {
                                Connection = connection,
                                Group = subConnectionGrup
                            };
                var queryList = query.OrderByDescending(item => item.Connection.Modified).ToList();
                return queryList.ConvertAll(item => new RMAzureFileShareConnection
                {
                    Id = item.Connection.Id,
                    Name = item.Connection.Name,
                    Description = item.Connection.Description,
                    AccessEndPoint = item.Connection.AccessEndPoint,
                    FileShareName = item.Connection.FileShareName,
                    AccountName = item.Connection.AccountName,
                    AccountKey = item.Connection.AccountKey,
                    Created = item.Connection.Created,
                    Modified = item.Connection.Modified,
                    CreatedBy = item.Connection.CreatedBy,
                    ModifiedBy = item.Connection.ModifiedBy,
                    ConnectionGroupId = item.Connection.ConnectionGroupId,
                    ConnectionGroup = item.Group
                });                            
            }
        }

        public List<RMAzureFileShareConnection> GetAllWithoutSecret()
        {
            using (var context = GetNewContext())
            {
                var query = from connection in context.RMAzureFileShareConnections
                            join connectionGroup in context.RMAzureFileShareConnectionGroups
                            on connection.ConnectionGroupId equals connectionGroup.Id
                            into connectionGroups
                            from subConnectionGrup in connectionGroups.DefaultIfEmpty()
                            select new
                            {
                                Connection = connection,
                                Group = subConnectionGrup
                            };
                var queryList = query.OrderByDescending(item => item.Connection.Modified).ToList();
                return queryList.ConvertAll(item => new RMAzureFileShareConnection
                {
                    Id = item.Connection.Id,
                    Name = item.Connection.Name,
                    Description = item.Connection.Description,
                    AccessEndPoint = item.Connection.AccessEndPoint,
                    FileShareName = item.Connection.FileShareName,
                    AccountName = item.Connection.AccountName,
                    AccountKey = "",
                    Created = item.Connection.Created,
                    Modified = item.Connection.Modified,
                    CreatedBy = item.Connection.CreatedBy,
                    ModifiedBy = item.Connection.ModifiedBy,
                    ConnectionGroupId = item.Connection.ConnectionGroupId,
                    ConnectionGroup = item.Group
                }); ;
            }
        }

        public List<RMAzureFileShareConnection> GetAll(List<Guid> ids)
        {
            using (var context = GetNewContext())
            {
                var query = from connection in context.RMAzureFileShareConnections
                            join connectionGroup in context.RMAzureFileShareConnectionGroups
                            on connection.ConnectionGroupId equals connectionGroup.Id
                            into connectionGroups
                            from subConnectionGrup in connectionGroups.DefaultIfEmpty()
                            where ids.Contains(connection.Id)
                            select new
                            {
                                Connection = connection,
                                Group = subConnectionGrup
                            };
                var queryList = query.OrderByDescending(item => item.Connection.Modified).ToList();
                return queryList.ConvertAll(item => new RMAzureFileShareConnection
                {
                    Id = item.Connection.Id,
                    Name = item.Connection.Name,
                    Description = item.Connection.Description,
                    AccessEndPoint = item.Connection.AccessEndPoint,
                    FileShareName = item.Connection.FileShareName,
                    AccountName = item.Connection.AccountName,
                    AccountKey = item.Connection.AccountKey,
                    Created = item.Connection.Created,
                    Modified = item.Connection.Modified,
                    CreatedBy = item.Connection.CreatedBy,
                    ModifiedBy = item.Connection.ModifiedBy,
                    ConnectionGroupId = item.Connection.ConnectionGroupId,
                    ConnectionGroup = item.Group
                });
            }
        }

        public List<RMAzureFileShareConnection> GetAllByConnectionGroup(Guid connectionGroupId)
        {
            if (connectionGroupId == Guid.Empty)
            {
                throw new Exception($"Illegal connection group id [{connectionGroupId}].");
            }

            using (var context = GetNewContext())
            {
                var query = from connection in context.RMAzureFileShareConnections
                            join connectionGroup in context.RMAzureFileShareConnectionGroups
                            on connection.ConnectionGroupId equals connectionGroup.Id
                            into connectionGroups
                            from subConnectionGrup in connectionGroups.DefaultIfEmpty()
                            where connection.ConnectionGroupId == connectionGroupId
                            select new
                            {
                                Connection = connection,
                                Group = subConnectionGrup
                            };
                var queryList = query.OrderByDescending(item => item.Connection.Modified).ToList();
                return queryList.ConvertAll(item => new RMAzureFileShareConnection
                {
                    Id = item.Connection.Id,
                    Name = item.Connection.Name,
                    Description = item.Connection.Description,
                    AccessEndPoint = item.Connection.AccessEndPoint,
                    FileShareName = item.Connection.FileShareName,
                    AccountName = item.Connection.AccountName,
                    AccountKey = item.Connection.AccountKey,
                    Created = item.Connection.Created,
                    Modified = item.Connection.Modified,
                    CreatedBy = item.Connection.CreatedBy,
                    ModifiedBy = item.Connection.ModifiedBy,
                    ConnectionGroupId = item.Connection.ConnectionGroupId,
                    ConnectionGroup = item.Group
                });
            }
        }

        public List<RMAzureFileShareConnection> GetAllWithoutRelatedConnectionGroup()
        {
            using (var context = GetNewContext())
            {
                var query = from connection in context.RMAzureFileShareConnections
                            join connectionGroup in context.RMAzureFileShareConnectionGroups
                            on connection.ConnectionGroupId equals connectionGroup.Id
                            into connectionGroups
                            from subConnectionGrup in connectionGroups.DefaultIfEmpty()
                            where connection.ConnectionGroupId == Guid.Empty
                            select new
                            {
                                Connection = connection,
                                Group = subConnectionGrup
                            };
                var queryList = query.OrderByDescending(item => item.Connection.Modified).ToList();
                return queryList.ConvertAll(item => new RMAzureFileShareConnection
                {
                    Id = item.Connection.Id,
                    Name = item.Connection.Name,
                    Description = item.Connection.Description,
                    AccessEndPoint = item.Connection.AccessEndPoint,
                    FileShareName = item.Connection.FileShareName,
                    AccountName = item.Connection.AccountName,
                    AccountKey = item.Connection.AccountKey,
                    Created = item.Connection.Created,
                    Modified = item.Connection.Modified,
                    CreatedBy = item.Connection.CreatedBy,
                    ModifiedBy = item.Connection.ModifiedBy,
                    ConnectionGroupId = item.Connection.ConnectionGroupId,
                    ConnectionGroup = item.Group
                });
            }
        }
    }
}
