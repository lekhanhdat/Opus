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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Extensions
{
    public static class EntityDtoExtension
    {
        public static RMAgent Convert2Entity(this RMAgentDto dto)
        {
            if (dto == null) return null;
            return new RMAgent
            {
                Id = dto.Id,
                Name = dto.Name,
                FarmId = dto.FarmId,
                ClientId = dto.ClientId,
                CertificateId = dto.CertificateId,
                InstallationCode = dto.InstallationCode,
                AuthCode = dto.AuthCode,
                SourceType = dto.SourceType,
                Status = dto.Status,
                Version = dto.Version,
                Description = dto.Description,
                ServerName = dto.ServerName,
                Errors = dto.Errors,
                CollectLog = dto.CollectLog,
                DCInternalName = dto.DCInternalName,
            };
        }

        public static RMAgentDto Convert2Dto(this RMAgent entity, bool includeAuthCode = false)
        {
            if (entity == null) return null;
            return new RMAgentDto
            {
                Id = entity.Id,
                FarmId = entity.FarmId,
                Name = entity.Name,
                ClientId = entity.ClientId,
                CertificateId = entity.CertificateId,
                InstallationCode = entity.InstallationCode,
                AuthCode = includeAuthCode ? entity.AuthCode : null,
                SourceType = entity.SourceType,
                Status = entity.Status,
                Version = entity.Version,
                Description = entity.Description,
                ServerName = entity.ServerName,
                Errors = entity.Errors,
                AvailableMemeory = entity.AvailableMemeory,
                IsSupportUpgrade = entity.IsSupportUpgrade,
                CPUUsage = entity.AvailableCPU,
                CollectLog = entity.CollectLog,
                CPUHZ = entity.CPUHZ,
                TotalMemory = entity.TotalMemory,
                DCInternalName = entity.DCInternalName,
            };
        }

        public static RMNameValueDto Conver2Dto(this RMKeyValue entity)
        {
            var key = entity.Key.Split(RMNameValueDto.Seprator);
            var type = RMNameValueType.AppManagementClientId;
            return new RMNameValueDto
            {
                Name = key[0],
                Value = entity.Value,
                Type = key.Length > 1 && Enum.TryParse(key[1], out type)? type : RMNameValueType.AppManagementClientId
            };
        }

        public static RMKeyValue Conver2Entity(this RMNameValueDto dto)
        {
            return new RMKeyValue
            {
                Key = $"{dto.Name}{RMNameValueDto.Seprator}{dto.Type}",
                Value = dto.Value,
            };
        }

        public static RMGlobalNameValueDto Conver2Dto(this RMGlobalKeyValue entity)
        {
            var key = entity.Key.Split(RMNameValueDto.Seprator);
            var type = RMGlobalNameValueType.GlobalRateLimitsPolicy;
            return new RMGlobalNameValueDto
            {
                Name = key[0],
                Value = entity.Value  ,
                Type = key.Length > 1 && Enum.TryParse(key[1], out type) ? type : RMGlobalNameValueType.GlobalRateLimitsPolicy
            };
        }

        public static RMGlobalKeyValue Conver2Entity(this RMGlobalNameValueDto dto)
        {
            return new RMGlobalKeyValue
            {
                Key = $"{dto.Name}{RMGlobalNameValueDto.Seprator}{dto.Type}",
                Value = dto.Value,
            };
        }

        public static RMSecurityContainerDto Convert2Dto(this RMSecurityContainer obj)
        {
            return new RMSecurityContainerDto
            {
                Id = obj.Id,
                Name = obj.Name,
                Parent = obj.Parent,
                SourceFlag = obj.SourceFlag,
                Status = obj.Status,
                Level = obj.Level,
            };
        }

        public static RMSecurityContainer Convert2Entity(this RMSecurityContainerDto obj)
        {
            return new RMSecurityContainer
            {
                Id = obj.Id,
                Name = obj.Name,
                Parent = obj.Parent,
                SourceFlag = obj.SourceFlag,
                Status = obj.Status,
                Level = obj.Level,
            };
        }
    }
}
