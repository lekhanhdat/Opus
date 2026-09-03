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
    public class RMLocationAssociationDao : BaseDao<RMLocationAssociation>, IRMLocationAssociationDao
    {
        //to do how to remove folder.....
        public async Task AddOrUpdateRMLocationeAssociationAsync(RMLocationAssociation locationInfo)
        {
            using var context = GetNewContext();
            var info = GetLocationByTermId(locationInfo.SiteId, locationInfo.WebId, locationInfo.ListId, locationInfo.TermUniqueId);
            if (info != null)
            {
                info = locationInfo;
                await this.UpdateAsync(info);
            }
            else
            {
                context.LocationAssociation.Add(locationInfo);
                context.SaveChanges();
            }
        }

        public void AddRMLocationAssociation(RMLocationAssociation locationInfo)
        {
            using var context = GetNewContext();
            context.LocationAssociation.Add(locationInfo);
            context.SaveChanges();
        }

        public void DeleteLocationAssocation(RMLocationAssociation locationInfo)
        {
            using var context = GetNewContext();
            context.LocationAssociation.Remove(locationInfo);
            context.SaveChanges();
        }

        public RMLocationAssociation GetLocationByTermId(Guid siteId, Guid webId, Guid listId, Guid termUniqueId)
        {
            using var context = GetNewContext();
            var location = context.LocationAssociation.AsQueryable().Where(l => l.SiteId.Equals(siteId)
                                                                             && l.WebId.Equals(webId)
                                                                             && l.ListId.Equals(listId)
                                                                             && l.TermUniqueId.Equals(termUniqueId)).FirstOrDefault();
            return location;

        }

        public async Task UpdateRMLocationAssociationAsync(RMLocationAssociation locationInfo)
        {
            using var context = GetNewContext();
            var info = GetLocationByTermId(locationInfo.SiteId, locationInfo.WebId, locationInfo.ListId, locationInfo.TermUniqueId);
            if (info != null)
            {
                info.FolderId = locationInfo.FolderId;
                await this.UpdateAsync(info);
            }
        }
    }
}
