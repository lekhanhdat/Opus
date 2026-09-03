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
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.CustomizeConnector.I18ns;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Core.Upgrade;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.TenantMigrations.Upgrade.Impl
{
    public class RMCustomizeConnectorContentSourceUpgradeDao : IDbUpgradeDao
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(RMCustomizeConnectorContentSourceUpgradeDao));
        public async Task UpgradeAsync(RMDbContext context)
        {
            try
            {
                var buildinSources = context.RMCustomizeConnectorContentSources.Where(item => item.Origin == Contract.CustomizeConnector.Enums.CustomizeConnectorOrigin.BuildIn).ToList();
                var sources = buildinSources.ConvertAll(item => (SourceFlag)item.Flag);
                var defineSources = Enum.GetValues(typeof(SourceFlag)).Cast<SourceFlag>().ToList();
                var needAddedSources = defineSources.Except(sources).ToList();
                Logger.Info($"Need added build-in sources: [{string.Join(", ", needAddedSources)}].");

                var now = DateTime.UtcNow.Ticks;
                var dbContentSources = new List<RMCustomizeConnectorContentSource>();

                foreach(var needAddedSource in needAddedSources)
                {
                    if(BuildInContentSourceI18Ns.SourceFlagI18ns.TryGetValue(needAddedSource, out var name))
                    {
                        dbContentSources.Add(new RMCustomizeConnectorContentSource
                        {
                            Id = Guid.NewGuid(),
                            Name = name,
                            Flag = (int)needAddedSource,
                            Origin = Contract.CustomizeConnector.Enums.CustomizeConnectorOrigin.BuildIn,
                            Created = now,
                            Modified = now,
                            CreatedBy = SystemAccountInfo.UserId.ToString(),
                            ModifiedBy = SystemAccountInfo.UserId.ToString(),
                            IsRemoved = false,
                        });
                    }
                }

                context.RMCustomizeConnectorContentSources.AddRange(dbContentSources);
                context.SaveChanges();
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while upgrade content source. Error: {e}");
            }
        }
    }
}
