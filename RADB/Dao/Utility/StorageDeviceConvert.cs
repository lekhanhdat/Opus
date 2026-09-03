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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Model;
using LiteDB;
using Storage;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScheduleType = AvePoint.RA.Contract.Schedule.ScheduleType;

namespace AvePoint.RA.DB.Dao.Utility
{
    public class StorageDeviceConvert
    {
        private static IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        private static IAveLogger logger = AveLogger.GetInstance(typeof(StorageDeviceConvert));
        public static RMStorageDeviceInfo ConvertStorageDeviceDto(StorageDeviceDto dto)
        {
            var gls = GeneralSettingService.GetGeneralSettingAsync().GetAwaiter().GetResult();
            RMStorageDeviceInfo rmStorageDeviceInfo = new RMStorageDeviceInfo();
            if (dto.Id == null)
            {
                rmStorageDeviceInfo.Id = Guid.NewGuid();
            }
            else
            {
                rmStorageDeviceInfo.Id = new Guid(dto.Id);
            }

            rmStorageDeviceInfo.IsSystemStorage = dto.IsAveStorage;
            rmStorageDeviceInfo.DAOMigrated = dto.DAOMigrated;
            rmStorageDeviceInfo.DAOStoragePolicyId = dto.DAOStoragePolicyId;
            rmStorageDeviceInfo.DAOLogicalDeviceId = dto.DAOLogicalDeviceId;
            rmStorageDeviceInfo.DAOPhysicalDeviceId = dto.DAOPhysicalDeviceId;

            StorageDeviceParaDto paraDto = new StorageDeviceParaDto();
            paraDto.Description = dto.Description;
            paraDto.UseCompression=dto.UseCompression;
            paraDto.UseEncryption = dto.UseEncryption;
            paraDto.CompressionSpeed=dto.CompressionSpeed;
            paraDto.EncryptionProfileId = dto.EncryptionProfileId;
            //paraDto.Schedule = dto.Schedule;
            rmStorageDeviceInfo.DriveInfo = SerializerHelper.SerializeToXmlString<StorageDeviceParaDto>(paraDto);
            rmStorageDeviceInfo.Name = dto.Name;
            rmStorageDeviceInfo.Type = dto.Type;
            //rmStorageDeviceInfo.DriveInfo = dto.
            //rmStorageDeviceInfo.InfoExtension = SerializerHelper.SerializeToXmlString<StorageDeviceExtension>(dto.Extension);
            rmStorageDeviceInfo.ModifiedTime = dto.ModifyTime;
            rmStorageDeviceInfo.Status = dto.Status;
            rmStorageDeviceInfo.ConnectionString = dto.ConnectionString;
            //rmStorageDeviceInfo.IsSystemStorage = dto.IsSystemStorage;
            if (dto.ArchiveRetentionRules != null)
            {
                rmStorageDeviceInfo.Retention = SerializerHelper.SerializeByDataContractSerializer(dto.ArchiveRetentionRules);
            }
            else
            {
                rmStorageDeviceInfo.Retention = null;
            }
            //rmStorageDeviceInfo.RetentionNextTime =
            rmStorageDeviceInfo.Notification = dto.NotificationId;
            DateTimeFormatInfo dtFormat = new DateTimeFormatInfo();
            dtFormat.ShortDatePattern = "yyyy-MM-dd HH:mm:ss";
            //dto.LastArchivedTime = dto.LastArchivedTime.Substring(0, dtFormat.ShortDatePattern.Length);
            //dto.LastModifiedTime = dto.LastModifiedTime.Substring(0, dtFormat.ShortDatePattern.Length);
            //rmStorageDeviceInfo.LastArchivedTime = Convert.ToDateTime(dto.LastArchivedTime, dtFormat).Ticks;//.ToInt64();
            //rmStorageDeviceInfo.LastModifiedTime = Convert.ToDateTime(dto.LastArchivedTime, dtFormat).Ticks;
            return rmStorageDeviceInfo;
        }

        public static StorageDeviceDto ConvertStorageDeviceInfoDto(RMStorageDeviceInfo domain,bool includedDate=false)
        {
            var gls = GeneralSettingService.GetGeneralSettingAsync().GetAwaiter().GetResult();
            StorageDeviceParaDto paraDto = new StorageDeviceParaDto();
            try
            {
                paraDto = SerializerHelper.DeserializeFromXmlString<StorageDeviceParaDto>(domain.DriveInfo);
            }
            catch(Exception e)
            {
                logger.Error($"error occured when AveRetryProjectContext,error:{e}");
            }
            StorageDeviceDto dto = new StorageDeviceDto();
            dto.Id = domain.Id.ToString();
            dto.Name = domain.Name;
            dto.Type = domain.Type;
            dto.Description = paraDto.Description;
            dto.UseCompression= paraDto.UseCompression;
            dto.UseEncryption = paraDto.UseEncryption;
            dto.CompressionSpeed= paraDto.CompressionSpeed;
            dto.EncryptionProfileId = paraDto.EncryptionProfileId;
            //dto.Schedule = paraDto.Schedule;
            //dto.DeviceMode = RMConstants.PHYSICAL_DEVICE_DATA_ONLINE; //由于Online和Offline的状态已经取消，因此这里默认将DeviceMode状态设置为Online
            dto.Status = domain.Status;
            dto.ModifyTime = domain.ModifiedTime;
            //dto.Description = paraDto.Description;
            dto.ConnectionString = domain.ConnectionString;
            dto.IsSystemStorage = dto.Id.Equals(RecordsConstants.AVEPOINT_DEFAULT_STORAGEID, StringComparison.OrdinalIgnoreCase) || domain.IsSystemStorage;
            dto.IsAveStorage = dto.IsSystemStorage;
            dto.DAOMigrated = domain.DAOMigrated ?? false;
            if (!string.IsNullOrEmpty(domain.ConnectionString))
            {
                dto.mCurrentXRI = new AvePoint.GCommon.Contract.Storage.Entity.UIXRI() { Params = new Dictionary<string, string>() };
                var mXRI = ConnectionBuilder.ValueOf(domain.ConnectionString);
                foreach (var dic in mXRI.Params)
                {
                    if (dic.Key == "secret" && includedDate)
                    {
                        dto.mCurrentXRI.Params.Add(dic.Key, new Guid().ToString());
                    }
                    else
                    {
                        dto.mCurrentXRI.Params.Add(dic.Key, dic.Value);
                    }
                }
                dto.mCurrentXRI.VIM = mXRI.StorageName;
            }
            
            if (domain.Retention != null)
            {
                try
                {
                    dto.ArchiveRetentionRules = SerializerHelper.DeserializeByDataContractSerializer<List<RetentionRule>>(domain.Retention);
                    if (dto.ArchiveRetentionRules != null)
                    {
                        foreach (var r in dto.ArchiveRetentionRules)
                        {
                            if (r.SetupDataRetention)
                            {
                                dto.SetupDataRetention = true;
                                if (r.RetentionDataTimeType == KeepDateType.None)
                                {
                                    r.RetentionDataTimeType = KeepDateType.ArchiveTime;
                                }
                                break;
                            }
                            else
                            {
                                r.RetentionDataTimeType = KeepDateType.ArchiveTime;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    dto.ArchiveRetentionRules = null;
                }

            }
            else
            {
                dto.ArchiveRetentionRules = null;
            }
            if (domain.LastArchivedTime <= 0)
            {
                dto.LastArchivedTime = string.Empty;
            }
            else
            {
                dto.LastArchivedTime = GeneralSettingService.ConvertTiksToDateTime(gls, domain.LastArchivedTime, true).SimplifyFormatTime;
            }
            if (domain.ModifiedTime <= 0)
            {
                dto.LastModifiedTime = string.Empty;
            }
            else
            {
                dto.LastModifiedTime = GeneralSettingService.ConvertTiksToDateTime(gls, domain.ModifiedTime,true).SimplifyFormatTime;
            }
            dto.DAOStoragePolicyId = domain.DAOStoragePolicyId;
            dto.DAOLogicalDeviceId = domain.DAOLogicalDeviceId;
            dto.DAOPhysicalDeviceId = domain.DAOPhysicalDeviceId;
            return dto;
        }

        public static StorageDeviceUIDto ConvertStorageDeviceDtoToUIDto(StorageDeviceDto dto)
        {
            StorageDeviceUIDto uIDto = new StorageDeviceUIDto();
            uIDto.Name = dto.Name;
            uIDto.Type = dto.Type;
            uIDto.Id = dto.Id;
            uIDto.Description = dto.Description;
            uIDto.FreeSpace = dto.FreeSpace;
            uIDto.StorageDeviceSpace = dto.StorageDeviceSpace;
            uIDto.Extension = new StorageDeviceUIExtension();
            if (dto.Extension != null)
            {
                uIDto.Extension.TotalSpace = dto.Extension.TotalSpace;
                uIDto.Extension.UsedSpace = dto.Extension.UsedSpace;
            }
            uIDto.UseSpace = dto.UseSpace;
            uIDto.SpaceType = dto.SpaceType;
            uIDto.mCurrentXRI = dto.mCurrentXRI;
            uIDto.SetupDataRetention = dto.SetupDataRetention;
            uIDto.Schedule = dto.Schedule;
            uIDto.NotificationId = dto.NotificationId;
            uIDto.ArchiveRetentionRules = dto.ArchiveRetentionRules;
            uIDto.CompressionSpeed = dto.CompressionSpeed;
            uIDto.UseCompression = dto.UseCompression;
            uIDto.UseEncryption = dto.UseEncryption;
            uIDto.EncryptionProfileId = dto.EncryptionProfileId;
            uIDto.LastModifiedTime = dto.LastModifiedTime;
            uIDto.LastArchivedTime = dto.LastArchivedTime;
            uIDto.IsUsingDevice = dto.IsUsingDevice;
            uIDto.IsSystemStorage = dto.IsSystemStorage;
            uIDto.DAOMigrated = dto.DAOMigrated;
            uIDto.DAOStoragePolicyId = dto.DAOStoragePolicyId;
            uIDto.DAOLogicalDeviceId = dto.DAOLogicalDeviceId;
            uIDto.DAOPhysicalDeviceId = dto.DAOPhysicalDeviceId;
            return uIDto;

        }

        public static SettingProfiles ConvertIndexDeviceDtoToSettingProfile(SettingProfileDto dto)
        {
            SettingProfiles pro= new SettingProfiles();
            pro.Id = new Guid(dto.Id);
            pro.Name = dto.Name;
            pro.Type = dto.Type;
            pro.Settings = dto.Settings;
            pro.DAOMigrated = dto.DAOMigrated;
            return pro;
        }
        public static SettingProfileDto ConvertSettingProfileToIndexDeviceDto(SettingProfiles pro)
        {
            SettingProfileDto dto = new SettingProfileDto();
            dto.Id=pro.Id.ToString();
            dto.Name=pro.Name;
            dto.Type=pro.Type;
            dto.Settings=pro.Settings;
            return dto;
        }
        public static RetentionRuleOption ConvertToRetentionRuleOption(List<RetentionRule> reRetentionRules)
        {
            RetentionRuleOption daoOption = new RetentionRuleOption();
            daoOption.ArchiveRetentionRules = new List<ArchiveRetentionRule>();
            foreach (var rule in reRetentionRules)
            {
                daoOption.ArchiveRetentionRules.Add(ConvertToRetentionRule(rule));
            }
            //var scheduleRetention = ScheduleService.GetScheduleByTypeServiceAsync(ScheduleType.ArchiveDataRetentionSchedule).GetAwaiter().GetResult().FirstOrDefault();
            //if (scheduleRetention == null)
            //{
            //    daoOption.Schedule = null;
            //}
            //else
            //{
            //    daoOption.Schedule = ConvertToScheduleDto(scheduleRetention);
            //}
            return daoOption;
        }
        public static ArchiveRetentionRule ConvertToRetentionRule(RetentionRule reRetentionRule)
        {
            ArchiveRetentionRule daoRetention = new ArchiveRetentionRule();
            daoRetention.KeepValue = reRetentionRule.KeepValue;
            daoRetention.ArchiveDateUnit = reRetentionRule.ArchiveDateUnit;
            daoRetention.KeepValueErrorMessage= reRetentionRule.KeepValueErrorMessage;
            daoRetention.TakeEffectToExistingData = reRetentionRule.TakeEffectToExistingData;
            daoRetention.RemoveOrphanedStub = reRetentionRule.RemoveOrphanedStub;
            daoRetention.DeleteTheData = reRetentionRule.DeleteTheData;
            daoRetention.RemoveTheJob = reRetentionRule.RemoveTheJob;
            daoRetention.IsMove = reRetentionRule.IsMove;
            //daoRetention.MoveLogicalDeviceId = reRetentionRule.MoveDeviceId;
            daoRetention.LogicalName = reRetentionRule.LogicalName;
            daoRetention.SetupDataRetention = reRetentionRule.SetupDataRetention;
            return daoRetention;
        }
        public static ScheduleDto ConvertToScheduleDto(ScheduleInfo reRetentionRule)
        {
            ScheduleDto dto = new ScheduleDto();
            dto.Interval = reRetentionRule.Interval;
            switch (reRetentionRule.IntervalType)
            {
                case Contract.Schedule.IntervalType.None:
                    dto.IntervalType = GCommon.Contract.Server.Common.Schedule.Object.IntervalType.None;
                    break;
                case Contract.Schedule.IntervalType.Daily:
                    dto.IntervalType = GCommon.Contract.Server.Common.Schedule.Object.IntervalType.Daily;
                    break;
                case Contract.Schedule.IntervalType.Hourly:
                    dto.IntervalType = GCommon.Contract.Server.Common.Schedule.Object.IntervalType.Hourly;
                    break;
                case Contract.Schedule.IntervalType.Weekly:
                    dto.IntervalType = GCommon.Contract.Server.Common.Schedule.Object.IntervalType.Weekly;
                    break;
                default:
                    dto.IntervalType = GCommon.Contract.Server.Common.Schedule.Object.IntervalType.None;
                    break;
            }
            dto.NextTime = reRetentionRule.NextTime.Ticks;
            try
            {
                dto.StartTime = Convert.ToInt64(reRetentionRule.StartTime);
                //dto.StartTimeUTC= TimeZoneInfo.ConvertTimeToUtc(Convert.ToDateTime(Convert.ToInt64(reRetentionRule.StartTime))).Ticks;
                dto.EndTime = Convert.ToInt64(reRetentionRule.EndTime);
                dto.NextTime = Convert.ToInt64(reRetentionRule.NextTime);
            }
            catch (Exception e)
            {
                logger.Error($"error occured when ConvertToScheduleDto,error:{e}");
            }
            dto.TimeZoneId = reRetentionRule.TimeZoneId;

            return dto;
        }
    }
}
