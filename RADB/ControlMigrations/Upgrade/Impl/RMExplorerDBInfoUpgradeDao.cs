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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.ControlMigrations.Upgrade.Impl
{
    public class RMExplorerDBInfoUpgradeDao
    {
        RALogger logger = new RALogger(MethodBase.GetCurrentMethod().DeclaringType);
        public void Upgrade(Core.RMSysDBContext context)
        {
            try
            {
                if (!context.ExplorerDBMapping.Any(e => e.DBName == RecordsConstants.ExplorerDBDefaultName)) 
                {

                    RecordRepositoryV2 recordRepository = new RecordRepositoryV2();
                    var containers = recordRepository.GetContainersInDBAsync(RecordsConstants.ExplorerDBDefaultName).Result;
                    foreach (var name in containers)
                    {
                        context.ExplorerDBMapping.Add(new Model.RMExplorerDBInfoMapping() { DBName = RecordsConstants.ExplorerDBDefaultName, ContainerName = name });
                    }
                    if (containers.Count > 0) 
                    {
                        context.SaveChanges();
                        logger.Info($"upgrade explorer db mapping:{containers.Count}");
                    }
                    
                }

            }
            catch (Exception ex)
            {
                logger.Error($"error occurred while upgrade explorer db mapping:{ex.ToString()}");
            }

        }
    }
}
