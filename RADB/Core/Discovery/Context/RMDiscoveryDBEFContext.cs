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
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Profile;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Model.Discovery.Salesforce;
using AvePoint.RA.DB.Model.Discovery.Office365;

namespace AvePoint.RA.DB.Core.Discovery.Context
{
    public partial class RMDiscoveryDBEFContext : DbContext, IDbModelCacheKeyProvider
    {
        private readonly string _schemaName;

        public string CacheKey => _schemaName;

        public RMDiscoveryDBEFContext(string schemaName, SqlConnection conn) : base(conn, true)
        {
            Database.SetInitializer<RMDiscoveryDBEFContext>(null);
            _schemaName = schemaName;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(_schemaName);
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<RMDiscoveryConfiguration> Configurations { get; set; }

        public DbSet<RMDiscoveryOffice365ExecutionInfo> ExecutionInfoList { get; set; }

        public DbSet<RMDiscoveryUpgradeInfo> UpgradeInfoes { get; set; }
    }
}
