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
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace AvePoint.RA.DB.Explorer
{
    public class ExplorerDbContext : DbContext
    {
        //private AveImpersonator impersonator = null;
        public ExplorerDbContext()
            : base(ExplorerDBSetting.ConnectionDatabaseString)
        {
            //Windows 认证模拟登陆  --已经去掉, 目前使用web和Timer进程的进程User windows认证.
            //if (ExplorerDBSetting.DatabaseIsIntegrated)
            //{
            //    #region//debug by ylgu
            //    //ExplorerDBSetting.Domain = "ccdev12server.com";
            //    //ExplorerDBSetting.DatabaseUsername = "ylgu";
            //    #endregion
            //}
            //Database.SetInitializer<RMDbContext>(new CreateDatabaseIfNotExists<RMDbContext>());
            Configuration.LazyLoadingEnabled = false;
            //下边这个超时是SQLCommand执行的超时时间, 默认是30(秒), 大数据表有可能会超时. 改成30分钟.
            ((IObjectContextAdapter)this).ObjectContext.CommandTimeout = 1800;

            
        }

        //protected override void OnModelCreating(DbModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<RMRecordAlliance>().Property(a => a.SrcKey)
        //    .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
        //}

        public DbSet<RMScope> Scope { get; set; }
        public DbSet<RMRecordsUpdateTemp> RecordsUpdateTemp { get; set; }

        public DbSet<RMHold> Hold { get; set; }
        
        public DbSet<RMRecordAlliance> Alliance { get; set; }

        public DbSet<RMManagedRecordRelated> ManagedRecordRelated { get; set; }
    }
}
