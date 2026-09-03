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
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class ContainerDao : BaseDao<RMContainer>, IContainerDao
    {
        public int SaveContainer(RMContainer container)
        {
            try
            {
                using var context = GetNewContext();
                if (HasSameNameContainer(container.TypeName))
                {
                    throw new Exception("container has same name");
                }
                context.Container.Add(container);
                context.SaveChanges();
                return container.Id;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<RMContainer> GetAllContainers()
        {
            using var context = GetNewContext();
            List<RMContainer> containers = context.Container.AsQueryable().Where(c => c.IsRemoved == false).ToList();
            foreach (RMContainer container in containers)
            {
                container.Size = Math.Round(container.Size, 2);
            }
            return containers;
        }

        public RMContainer CreateContainer(string typeName, float size, string description, bool isDefault = false)
        {
            using var context = GetNewContext();
            if (isDefault == true)
            {
                List<RMContainer> defaultContainers = context.Container.AsQueryable().Where(c => c.IsDefault == true && c.IsRemoved == false).ToList();
                if (defaultContainers.Count > 0)
                {
                    foreach (RMContainer c in defaultContainers)
                    {
                        c.IsDefault = false;
                    }
                    this.BatchUpdate(defaultContainers, c => c.IsDefault);
                }
            }
            if (HasSameNameContainer(typeName))
            {
                throw new Exception("container has same name");
            }
            RMContainer contrainer = new RMContainer() { TypeName = typeName, Size = Math.Round(size, 2), Description = description, IsDefault = isDefault };
            context.Container.Add(contrainer);
            context.SaveChanges();
            return contrainer;
        }

        public async Task<RMContainer> UpdateContainerTypeAsync(int containerId, string typeName, float size, string description, bool isDefault = false)
        {
            using var context = GetNewContext();
            if (isDefault == true)
            {
                List<RMContainer> defaultContainers = context.Container.AsQueryable().Where(c => c.IsDefault == true && c.IsRemoved == false).ToList();
                if (defaultContainers.Count > 0)
                {
                    foreach (RMContainer c in defaultContainers)
                    {
                        c.IsDefault = false;
                    }
                    this.BatchUpdate(defaultContainers, c => c.IsDefault);
                }
            }
            if (EditHasSameNameContainer(containerId,typeName))
            {
                throw new Exception("container has same name");
            }
            RMContainer container = context.Container.AsQueryable().Where(t => t.Id == containerId).FirstOrDefault();
            container.TypeName = typeName;
            container.Size = Math.Round(size, 2);
            container.Description = description;
            container.IsDefault = isDefault;
            await this.UpdateAsync(container);
            return container;
        }

        public async Task<bool> UpdateContainerIsDefaultAsync(int containerId, bool isDefault = false)
        {
            using var context = GetNewContext();
            if (isDefault == true)
            {
                List<RMContainer> defaultContainers = context.Container.AsQueryable().Where(c => c.IsDefault == true && c.IsRemoved == false).ToList();
                if (defaultContainers.Count > 0)
                {
                    foreach (RMContainer c in defaultContainers)
                    {
                        c.IsDefault = false;
                    }
                    this.BatchUpdate(defaultContainers, c => c.IsDefault);
                }
            }
            
            RMContainer container = context.Container.AsQueryable().Where(t => t.Id == containerId).FirstOrDefault();
            container.IsDefault = isDefault;
            await this.UpdateAsync(container);
            return true;
        }

        public async Task<bool> DeleteContainerTypeAsync(int containerId)
        {
            using var context = GetNewContext();
            RMContainer container = context.Container.AsQueryable().Where(t => t.Id == containerId).FirstOrDefault();
            container.IsRemoved = true;
            await this.UpdateAsync(container);
            return true;
        }

        public bool HasSameNameContainer(string containerName)
        {
            bool hasSame = false;
            try
            {
                using var context = GetNewContext();
                if (context.Container.AsQueryable().Where(c => c.TypeName.Equals(containerName) && !c.IsRemoved).FirstOrDefault() != null)
                {
                    hasSame = true;
                }
            }
            catch
            {
                hasSame = false;
            }
            return hasSame;
        }

        public bool EditHasSameNameContainer(int Id,string containerName)
        {
            bool hasSame = false;
            try
            {
                using var context = GetNewContext();
                if (context.Container.AsQueryable().Where(c => c.Id != Id && c.TypeName.Equals(containerName) && !c.IsRemoved).FirstOrDefault() != null)
                {
                    hasSame = true;
                }
            }
            catch
            {
                hasSame = false;
            }
            return hasSame;
        }

        public List<RMContainer> GetDefaultContainers()
        {
            using var context = GetNewContext();
            List<RMContainer> containers = context.Container.AsQueryable().Where(c => c.IsRemoved == false && c.IsDefault == true).ToList();
            return containers;
        }

        public RMContainer GetContainerById(int containerId)
        {
            using var context = GetNewContext();
            RMContainer container = context.Container.AsQueryable().Where(t => t.Id == containerId).FirstOrDefault();
            return container;
        }
    }
}
