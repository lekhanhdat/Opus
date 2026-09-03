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
using AvePoint.GCommon.Utility;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Contract.Tenant;
using System.Data.SqlClient;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace AvePoint.RA.DB.Core
{
    public class RMDbContext : DbContext,  IDbModelCacheKeyProvider
    {
        private DateTime _expireTime = DateTime.Now;
        private AveImpersonator impersonator = null;
        
        public string SchemaName { get; private set; }
        public RMDbContext()
            : base(CommonRoleConfiguration.ConfigDatabaseConnection)
        {}

        public RMDbContext(string conn, string schema) : base(conn)
        {
            SchemaName = schema;
        }



        #region properties
        public bool IsDispose { set; get; }

        /// <summary>
        /// 从创建DbContext实例开始，7天后超时
        /// </summary>
        public bool IsExpire
        {
            get {
                if ((_expireTime - DateTime.Now).Days < 7)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        #endregion

        #region common functions
        public void DetachLocalObject<T>(T obj) where T : class
        {
            var localObj = FindLocalObject(obj);
            if (localObj != null)
            {
                Detach(localObj);
            }
        }

        public void Detach<T>(T obj) where T : class
        {
            ObjectContext oc = ((IObjectContextAdapter)this).ObjectContext;
            oc.Detach(obj);
        }

        public T FindLocalObject<T>(T obj) where T : class
        {
            var keys = GetEntityKeys<T>();
            var func = GetFindExp<T>(obj, keys).Compile();
            return Set<T>().Local.FirstOrDefault(func);
        }

        public IEnumerable<string> GetEntityKeys<T>() where T : class
        {
            ObjectContext oc = ((IObjectContextAdapter)this).ObjectContext; 
            var keys = oc.CreateObjectSet<T>().EntitySet.ElementType.KeyProperties.Select(x => x.Name);
            return keys;
        }

        private Expression<Func<T, bool>> GetFindExp<T>(T obj, IEnumerable<string> keys) where T : class
        {
            var pe = Expression.Parameter(typeof(T), "p");

            var keyExps = keys.Select(k =>
            {
                var member = Expression.PropertyOrField(pe, k);
                var val = typeof(T).GetProperty(k).GetValue(obj);
                var eq = Expression.Equal(member, Expression.Constant(val));
                return eq;
            }).ToList();

            if (keys.Count() == 1)
            {
                return Expression.Lambda<Func<T, bool>>(keyExps[0], new[] { pe });
            }

            var combinExp = Expression.AndAlso(keyExps[0], keyExps[1]);
            for (var i = 2; i < keyExps.Count; i++)
            {
                combinExp = Expression.AndAlso(combinExp, keyExps[i]);
            }
            return Expression.Lambda<Func<T, bool>>(combinExp, new[] { pe });
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            IsDispose = true;
            if (IsDispose && impersonator != null)
            {
                impersonator.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Database Sets

        public DbSet<RMTermSet> TermSets { set; get; }
        public DbSet<RMTerm> Terms { set; get; }
        public DbSet<RMTermSetMembership> TermSetMemberships { set; get; }

        public DbSet<RMTermRuleAssociation> RMTermRuleAssociations { set; get; }

        public DbSet<RMTermGroup> TermGruops { set; get; }

        public DbSet<RMTermGroupMembership> TermGroupMembership { get; set; }
        public DbSet<RMCPGlobalStorageSetting> GlobalStorageSettingInfos { get; set; }

        public DbSet<RMJobMonitor> JobMonitors { set; get; }

        public DbSet<RMAuthenticationMode> AuthenticationMode { set; get; }

        public DbSet<RMSharePointSetting> RMSharePointSettings { get; set; }

        public DbSet<RMAudit> Audit { get; set; }

        public DbSet<RMSchedule> Schedule { get; set; }

        public DbSet<RMCPGeneralSetting> RMCPGeneralSetting { get; set; }

        public DbSet<RMProfile> Profile { get; set; }
        public DbSet<RMSettingJobInfo> SettingJobInfo { get; set; }
        public DbSet<RMLocationAssociation> LocationAssociation { get; set; }
        public DbSet<RMContainer> Container { get; set; }

        public DbSet<RMExcuteResult> ExcuteResult { get; set; }

        public DbSet<RMRecordOwner> RecordOwner { get; set; }
        #endregion
        public string CacheKey
        {
            get
            {
                return SchemaName;
            }
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            if (!string.IsNullOrEmpty(CacheKey))
            {
                modelBuilder.HasDefaultSchema(SchemaName);
            }
            base.OnModelCreating(modelBuilder);
        }

        private string EscapeSqlObjectName(string accountName)
        {
            if (accountName == null)
            {
                return null;
            }
            var schemaName = new StringBuilder();
            var accountNameChars = accountName.ToCharArray();
            foreach (var c in accountNameChars)
            {
                if (Char.IsLetter(c) || Char.IsNumber(c))
                {
                    schemaName.Append(c);
                }
                else
                {
                    schemaName.Append('#');
                }
            }
            return schemaName.ToString();
        }
    }
}

