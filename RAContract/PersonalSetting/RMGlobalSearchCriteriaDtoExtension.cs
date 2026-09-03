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
using AvePoint.RA.I18N.Core;
using Newtonsoft.Json;
using System;

namespace AvePoint.RA.Contract.PersonalSetting
{
    public static class RMGlobalSearchCriteriaDtoExtension
    {
        public static RMPersonalSettingDto Convert2PersonalSetting(this RMExplorerSearchCriteriaDto dto)
        {
            return new RMPersonalSettingDto
            {
                Id = dto.Id,
                Name = dto.Name,
                Type = dto.Type,
                IsDefault = dto.IsDefault,
                IsBuiltIn = dto.IsBuiltIn,
                Owner = dto.Owner,
                ContentStr = dto.Setting != null ? JsonConvert.SerializeObject(dto.Setting) : null,
            };
        }

        /// <summary>
        /// check if property has value, e.g, Name. will throw ArgumentNullException if the property has no value.
        /// </summary>
        /// <param name="dto"></param>
        public static void Validate(this RMExplorerSearchCriteriaDto dto)
        {
            if (string.IsNullOrEmpty(dto.Name) || string.IsNullOrEmpty(dto.Name.Trim())) throw new ArgumentNullException("Name");
            dto.Name = dto.Name.Trim();

            //if (dto.IsBuiltIn) throw new ArgumentException("Can't create built in view manually");
            if (dto.Name == I18NEntity.GetString(RMPersonalSettingConst.Builtin_View_Name))
            {
                throw new SameNameException();
            }
            if (dto.Setting == null) throw new ArgumentNullException("Setting");

        }

    }
}
