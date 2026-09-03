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
namespace AvePoint.RA.DB.ControlMigrations
{
    using CommonUtil;
    using System.Data.Entity.Migrations;

    public sealed class Configuration : DbMigrationsConfiguration<AvePoint.RA.DB.Core.RMSysDBContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            MigrationsDirectory = @"ControlMigrations";
            CommandTimeout = 30 * 60;
        }

        protected override void Seed(AvePoint.RA.DB.Core.RMSysDBContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }

    public class ExplorerDbMigConfiguration : DbMigrationsConfiguration<Explorer.ExplorerDbContext>
    {
        private RALogger logger = RALogger.GetInstance(typeof(ExplorerDbMigConfiguration));
        public ExplorerDbMigConfiguration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            ContextKey = "AvePoint.RA.DB.Explorer.ExplorerDbContext";
            CommandTimeout = int.MaxValue;
        }
        protected override void Seed(Explorer.ExplorerDbContext context)
        {
            logger.Info("Seed database info.");
            //用于Migration改动数据库表结构的时候处理默认值
        }
    }
}
