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
using AvePoint.RA.Contract.PersonalSetting;

namespace AvePoint.RA.DB.Model.Extension
{
    public static class RMPersonalSettingExtension
    {
        public static RMPersonalSetting Convert2Entity(this RMPersonalSettingDto dto)
        {
            return new RMPersonalSetting
            {
                Id = dto.Id,
                Owner = dto.Owner,
                Type = dto.Type,
                Name = dto.Name,
                ContentStr = dto.ContentStr,
                //IsDefault = dto.IsDefault,
                IsBuiltIn = dto.IsBuiltIn,
            };
        }

        public static void Assemble2Entity(this RMPersonalSettingDto dto, RMPersonalSetting entity)
        {
            entity.Id = dto.Id;
            entity.Owner = dto.Owner;
            entity.Type = dto.Type;
            entity.Name = dto.Name;
            entity.ContentStr = dto.ContentStr;
            //entity.IsDefault = dto.IsDefault;
            entity.IsBuiltIn = dto.IsBuiltIn;
        }

        public static RMPersonalSettingDto Convert2Dto(this RMPersonalSetting entity, bool includeContent = false)
        {
            return new RMPersonalSettingDto
            {
                Id = entity.Id,
                Owner = entity.Owner,
                Type = entity.Type,
                Name = entity.Name,
                ContentStr = includeContent? entity.ContentStr : null,
                //IsDefault = entity.IsDefault,
                IsBuiltIn = entity.IsBuiltIn,
            };
        }
    }
}
