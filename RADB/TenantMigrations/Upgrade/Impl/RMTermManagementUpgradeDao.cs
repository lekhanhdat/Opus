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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Core.Upgrade;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.TenantMigrations.Upgrade.Impl
{
    public class RMTermManagementUpgradeDao : BaseDao<RMTerm>, IDbUpgradeDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMTermManagementUpgradeDao));
        public async Task UpgradeAsync(Core.RMDbContext context)
        {
            try
            {
            //这里往一起和的时候需要去掉
            logger.Info("init data in TermSets table.");
            if (!context.TermSets.Any(a => a.Id == 1))
            {
                RMTermSet t = new RMTermSet();
                t.UniqueId = Guid.NewGuid();
                t.Name = "TermSet";
                context.TermSets.Add(t);
                context.SaveChanges();
            }
            logger.Info("init data in TermGruops table.");
            if (!context.TermGruops.Any(a => a.Id == 1))
            {
                RMTermGroup t = new RMTermGroup();
                t.UniqueId = Guid.NewGuid();
                t.Name = "TermGroup";
                context.TermGruops.Add(t);
                context.SaveChanges();
            }

            //update termset termGroupId
            var termSets = context.TermSets.Where(t => t.TermGroupId.Equals(Guid.Empty)).ToList();
            var termGroup = context.TermGruops.First();
            if (termSets != null && termSets.Count > 0)
            {
                foreach (var termSet in termSets)
                {
                    termSet.TermGroupId = termGroup.UniqueId;
                    context.SaveChanges();
                }
            }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while upgrade term:{0}", ex.ToString());
            }
            
        }
    }
}
