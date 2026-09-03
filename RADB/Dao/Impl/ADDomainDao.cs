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
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.DB.Model;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace AvePoint.RA.DB.Dao.Impl
{
    [RACodeReview("Allen Yin")]
    public class ADDomainDao : BaseDao<RMADDomain>, IADDomainDao
    {
        public IAccountDao AccountDao { get; set; }

        public List<RMADDomain> GetADDomains(bool onlyEnableDomain)
        {
            var context = SharedDbContext;
            List<RMADDomain> results = null;
            if (onlyEnableDomain)
            {
                results = context.ADDomain.AsQueryable().Where(add => add.Enable).ToList();
            }
            else
            {
                results = context.ADDomain.AsQueryable().ToList();
            }
            return results;
        }

        public bool DeleteDomainById(int id)
        {
            var context = SharedDbContext;

            List<RMAccount> entities = AccountDao.FindList(a => a.DomainId == id);
            foreach (RMAccount account in entities)
            {
                context.Set<RMAccount>().Attach(account);
                context.Entry(account).State = EntityState.Deleted;
            }

            RMADDomain entity = context.Set<RMADDomain>().Find(id);
            context.Set<RMADDomain>().Attach(entity);
            context.Entry(entity).State = EntityState.Deleted;

            return context.SaveChanges() > 0;
        }

        public bool DeleteDomainByIds(List<int> ids)
        {
            var context = SharedDbContext;

            List<RMAccount> accounts = AccountDao.FindList(a => ids.Contains(a.DomainId));
            foreach (RMAccount account in accounts)
            {
                context.Set<RMAccount>().Attach(account);
                context.Entry(account).State = EntityState.Deleted;
            }

            List<RMADDomain> domains = FindList(a => ids.Contains(a.Id));
            foreach (RMADDomain domain in domains)
            {
                context.Set<RMADDomain>().Attach(domain);
                context.Entry(domain).State = EntityState.Deleted;
            }

            return context.SaveChanges() > 0;
        }

    }
}
