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
using HybridServer.EF.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HybridServer.EF
{
    public class SignalRRDBContext : DbContext
    {
        public SignalRRDBContext(DbContextOptions<SignalRRDBContext> options): base(options)
        {

        }

        public DbSet<Agent> Agents { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Agent>().Property(t => t.AgentId).HasColumnName("AgentId");
            modelBuilder.Entity<Agent>().Property(t => t.TenantId).HasColumnName("TenantId").IsRequired();
            modelBuilder.Entity<Agent>().Property(t => t.ConnectionId).HasColumnName("ConnectionId");
            modelBuilder.Entity<Agent>().Property(t => t.Status).HasColumnName("Status");
            modelBuilder.Entity<Agent>().Property(t => t.LastConnected).HasColumnName("LastConnected");
            modelBuilder.Entity<Agent>().Property(t => t.RegistrationTime).HasColumnName("RegistrationTime");
    
            modelBuilder.Entity<Agent>().HasKey(t => t.AgentId);   
            modelBuilder.Entity<Agent>().HasIndex(Agents => Agents.TenantId);

        }
    }
    
}