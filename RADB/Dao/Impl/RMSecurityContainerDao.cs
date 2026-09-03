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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMSecurityContainerDao : BaseDao<RMSecurityContainer>, IRMSecurityContainerDao
    {
        public int CreateOrUpdate(IList<RMSecurityContainerDto> dtos)
        {
            if (dtos.Count == 0) return 0;

            using (var context = GetNewContext())
            {
                foreach(var dto in dtos)
                {
                    var entity = context.RMSecurityContainer.Where(o => o.Id == dto.Id).FirstOrDefault();
                    if (entity == null)
                    {
                        entity = new RMSecurityContainer() { Id = dto.Id};
                        context.RMSecurityContainer.Add(entity);
                    }
                    entity.ObjectId = dto.ObjectId;
                    entity.Name = dto.Name;
                    entity.Parent = dto.Parent;
                    entity.SourceFlag = dto.SourceFlag;
                    entity.Status = dto.Status;
                    entity.Level = dto.Level;
                }
                return context.SaveChanges();
            }
        }

        public IList<NameAndIdDto> GetContainers(Contract.Explorer.SourceFlag sourceFlag = Contract.Explorer.SourceFlag.SharePoint, RMSecurityContainerLevel level = RMSecurityContainerLevel.RootContainer, 
            RMSecurityContainerStaus status = RMSecurityContainerStaus.Active)
        {
            using (var context = GetNewContext())
            {
                var dtos = context.RMSecurityContainer.Where(o => o.Level == level && o.Status == status && o.SourceFlag == sourceFlag)
                    .Select(o => new NameAndIdDto { Id = o.Id, Name = o.Name })
                    .OrderBy(o => o.Name)
                    .ToList();
                return dtos;
            }
        }

        public IList<NameAndIdDto> GetSubContainersByParent(string parent, RMSecurityContainerStaus status = RMSecurityContainerStaus.Active)
        {
            using (var context = GetNewContext())
            {
                var dtos = context.RMSecurityContainer.Where(o => o.Parent == parent && o.Status == status)
                    .Select(o => new NameAndIdDto { Id = o.Id, Name = o.Name })
                    .OrderBy(o => o.Name)
                    .ToList();
                return dtos;
            }
        }

        public IList<RMSecurityContainerDto> GetContainers(IEnumerable<string> ids,
            RMSecurityContainerStaus status = RMSecurityContainerStaus.Active)
        {
            using (var context = GetNewContext())
            {
                var dtos = context.RMSecurityContainer.Where(o => ids.Contains(o.Id) && o.Status == status)
                    .Select(o =>  new RMSecurityContainerDto { Id = o.Id, ObjectId = o.ObjectId, Name = o.Name, Parent = o.Parent, SourceFlag = o.SourceFlag })
                    .ToList();
                return dtos;
            }
        }

        public int UpdateStatus(IEnumerable<string> ids, RMSecurityContainerStaus targetStatus)
        {
            using (var context = GetNewContext())
            {
                var entities = context.RMSecurityContainer.Where(o => ids.Contains(o.Id));
                foreach(var entity in entities)
                {
                    entity.Status = targetStatus;
                }

                return context.SaveChanges();
            }
        }

        public IList<RMSecurityContainerDto> GetByLambda(Func<RMSecurityContainer, bool> whereLambda)
        {
            using (var context = GetNewContext())
            {
                var dtos = context.RMSecurityContainer.Where(whereLambda)
                    .Select(o => new RMSecurityContainerDto { Id = o.Id, ObjectId = o.ObjectId, Name = o.Name, Parent = o.Parent, SourceFlag = o.SourceFlag })
                    .ToList();
                return dtos;
            }
        }
    }
}
