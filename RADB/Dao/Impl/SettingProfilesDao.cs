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
using AvePoint.Common.Portal;
using AvePoint.GCommon.Contract.Server.ControlPanel.SuperUserConfiguration.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao.Utility;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class SettingProfilesDao : BaseDao<SettingProfiles>, ISettingProfilesDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(SettingProfilesDao));
        private readonly static object MProfileLock = new object();
        private readonly static object MArchiveSEELock = new object();

        public string Create(SettingProfileDto dto)
        {
            base.Create(StorageDeviceConvert.ConvertIndexDeviceDtoToSettingProfile(dto));
            return string.Empty;
        }

        public async Task<int> BatchCreateAsync(IEnumerable<SettingProfileDto> profiles)
        {
            return await base.BatchCreateAsync(profiles.Select(p => StorageDeviceConvert.ConvertIndexDeviceDtoToSettingProfile(p)).ToList());
        }

        public SettingProfiles Load(SettingProfileDto dto)
        {
            SettingProfiles oldDto = base.Find(o => o.Type == dto.Type && o.Name == dto.Name);
            return oldDto;
        }

        public SettingProfiles LoadById(Guid id)
        {
            SettingProfiles oldDto = base.Find(o => o.Id == id);
            return oldDto;
        }
        public SettingProfiles LoadByType(int type)
        {
            using (var context = GetNewContext())
            {
                return context.SettingProfile.FirstOrDefault(p => p.Type == type);
            }
        }

        public SettingProfiles LoadByType(SettingProfilesType type)
        {
            using (var context = GetNewContext())
            {
                return context.SettingProfile.FirstOrDefault(p => p.Type == (int)type);
            }
        }

        public List<SettingProfiles> LoadAllByType(SettingProfilesType type)
        {
            using (var context = GetNewContext())
            {
                return context.SettingProfile.Where(p => p.Type == (int)type)?.ToList();
            }
        }

        public async Task<string> UpdateAsync(SettingProfileDto dto)
        {
            SettingProfiles oldDto = base.Find(o => o.Type == dto.Type && o.Name == dto.Name);
            if (oldDto != null)
            {
                oldDto.Settings = dto.Settings;
                await base.UpdateAsync(oldDto);
            }
            else
            {
                base.Create(StorageDeviceConvert.ConvertIndexDeviceDtoToSettingProfile(dto));
            }
            return dto.Settings;
        }

        public byte[] GetEndUserStubLinkMasterKey()
        {
            lock (MProfileLock)
            {
                logger.Info("GetEndUserStubLinkMasterKey");
                byte[] mMasterKey = new byte[128];
                string mMasterKeyString = string.Empty;
                RandomNumberGenerator.Create().GetBytes(mMasterKey);
                using (var context = base.GetNewContext())
                {
                    using (var dbTransation = context.Database.BeginTransaction())
                    {
                        try
                        {
                            string queryText = @"
DECLARE @operation nvarchar(20) = N'Query';
if not exists(select 1 from {0}.SettingProfiles WITH (UPDLOCK, HOLDLOCK) where Type = @type)
BEGIN
INSERT INTO {0}.[SettingProfiles]([Id],[Name],[Type],[Settings]) VALUES(newid(),'EndUserStubLinkMasterKey',@type,@masterkey)
SET @operation = N'Insert';
END

select top(1) @operation + N'|' + Settings from {0}.SettingProfiles where Type = @type order by Name";
                            queryText = string.Format(queryText, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName));
                            DbParameter[] paras = new DbParameter[]
                            {
                            new  SqlParameter("@type",SettingProfilesType.EndUserStubLinkMasterKey),
                            new  SqlParameter("@masterkey",Convert.ToBase64String(mMasterKey)),
                            };
                            var reader = context.Database.SqlQuery(typeof(string), queryText, paras).ToListAsync().Result;
                            logger.Info($"Read masterkey count {reader.Count()}");
                            var masterKeyResult = reader[0].ToString();
                            var operationSeparatorIndex = masterKeyResult.IndexOf('|');
                            if (operationSeparatorIndex > 0)
                            {
                                logger.Info($"GetEndUserStubLinkMasterKey SQL operation: {masterKeyResult.Substring(0, operationSeparatorIndex)}.");
                                mMasterKeyString = masterKeyResult.Substring(operationSeparatorIndex + 1);
                            }
                            else
                            {
                                logger.Warn("GetEndUserStubLinkMasterKey SQL operation is unknown.");
                                mMasterKeyString = masterKeyResult;
                            }
                            dbTransation.Commit();
                        }
                        catch (Exception e)
                        {
                            logger.Error(e.ToString());
                            dbTransation.Rollback();
                        }
                    }
                }

                return Convert.FromBase64String(mMasterKeyString);
            }
        }

        public string GetDBSEEMasterKey(string tempSecureString)
        {
            lock (MArchiveSEELock)
            {
                logger.Info("GetDBSEEMasterKey group id");
                string temp = string.Empty;

                using (var context = base.GetNewContext())
                {
                    using (var dbTransation = context.Database.BeginTransaction())
                    {
                        try
                        {
                            string queryText = @"
BEGIN TRANSACTION;  
if not exists(select * from {0}.SettingProfiles where Type = @type)
INSERT INTO {0}.[SettingProfiles]([Id],[Name],[Type],[Settings]) VALUES(newid(),'DBSEEMasterKey',@type,@masterkey)

select top(1) Settings from {0}.SettingProfiles where Type = @type
COMMIT TRANSACTION; ";
                            queryText = string.Format(queryText, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName));
                            DbParameter[] paras = new DbParameter[]
                            {
                            new  SqlParameter("@type",SettingProfilesType.DBSEEMasterKey),
                            new  SqlParameter("@masterkey",tempSecureString),
                            };
                            var reader = context.Database.SqlQuery(typeof(string), queryText, paras).ToListAsync().Result;
                            logger.Info($"Read masterkey count {reader.Count()}");
                            temp = reader[0].ToString();
                            dbTransation.Commit();
                        }
                        catch (Exception e)
                        {
                            logger.Error(e.ToString());
                            dbTransation.Rollback();
                        }
                    }
                }
                    
                return temp;
            }
        }

        public string GetCommunicationEncryptionKey(string tempSecureString)
        {
            lock (MArchiveSEELock)
            {
                string temp = string.Empty;
                using (var context = base.GetNewContext())
                {
                    using (var dbTransation = context.Database.BeginTransaction())
                    {
                        try
                        {
                            string queryText = @"
BEGIN TRANSACTION;  
if not exists(select * from {0}.SettingProfiles where Type = @type)
INSERT INTO {0}.[SettingProfiles]([Id],[Name],[Type],[Settings]) VALUES(newid(),'CommunicationEncryptionKey',@type,@communicationkey)

select top(1) Settings from {0}.SettingProfiles where Type = @type
COMMIT TRANSACTION; ";
                            queryText = string.Format(queryText, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName));
                            DbParameter[] paras = new DbParameter[]
                            {
                            new  SqlParameter("@type",SettingProfilesType.CommunicationEncryptionKey),
                            new  SqlParameter("@communicationkey",tempSecureString),
                            };
                            var reader = context.Database.SqlQuery(typeof(string), queryText, paras).ToListAsync().Result;
                            logger.Info($"Read communicationkey count {reader.Count()}");
                            temp = reader[0].ToString();
                            dbTransation.Commit();
                        }
                        catch (Exception e)
                        {
                            logger.Error(e.ToString());
                            dbTransation.Rollback();
                        }
                    }
                }
                
                return temp;
            }
        }

        public async Task<int> DeleteMigratedProfilesAsync()
        {
            using (var context = GetNewContext())
            {
                string sql = $"DELETE FROM [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].SettingProfiles WHERE DAOMigrated=1;";
                return await context.Database.ExecuteSqlCommandAsync(sql);
            }
        }

        public async Task<int> DeleteOverrideProfilesAfterMigrationAsync(IEnumerable<int> types)
        {
            using (var context = GetNewContext())
            {
                string sql = $"DELETE FROM [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].SettingProfiles WHERE (DAOMigrated IS NULL OR DAOMigrated <> 1) AND [Type] IN ({string.Join(",", types)});";
                return await context.Database.ExecuteSqlCommandAsync(sql);
            }
        }

        public async Task<SettingProfiles> LoadByTypeAsync(int type)
        {
            using (var context = GetNewContext())
            {
                return await context.SettingProfile.FirstOrDefaultAsync(s => s.Type == type);
            }
        }

        public async Task<List<SettingProfiles>> LoadAllByTypeAsync(int type)
        {
            using (var context = GetNewContext())
            {
                return await context.SettingProfile.Where(s => s.Type == type).ToListAsync();
            }
        }

        public async Task<int> DeleteProfileByType(int type)
        {
            using (var context = GetNewContext())
            {
                string sql = $"DELETE FROM [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].SettingProfiles WHERE Type=@type;";
                DbParameter[] paras = [new SqlParameter("@type", type),];
                return await context.Database.ExecuteSqlCommandAsync(sql, paras);
            }
        }
    }
}
