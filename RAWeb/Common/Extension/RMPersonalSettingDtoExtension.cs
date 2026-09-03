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
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.I18N.Core;
using Newtonsoft.Json;

namespace AvePoint.RA.Contract.PersonalSetting
{
    public static class RMPersonalSettingDtoExtension
    {
        public static RMExplorerSearchCriteriaDto Convert2GlobalSearchCriteria(this RMPersonalSettingDto dto, bool isSharedBy = false)
        {
            RMExplorerSearchCriteriaDto result = new RMExplorerSearchCriteriaDto
            {
                Id = dto.Id,
                Name = dto.IsBuiltIn ? I18NEntity.GetString(dto.Name) : dto.Name,
                Type = dto.Type,
                IsDefault = dto.IsDefault,
                IsBuiltIn = dto.IsBuiltIn,
                IsSharedBy = isSharedBy,
                Owner = dto.Owner,
                Setting = (dto.ContentStr != null && dto.Type == PersonalSettingType.GlobalSearchCriteria) ? JsonConvert.DeserializeObject<RMExplorerSearchCriteriaSetting>(dto.ContentStr) : null
            };
            if (result.Setting != null && result.Setting.AdvancedSearchs != null)
            {
                try
                {
                    foreach (var setting in result.Setting.AdvancedSearchs)
                    {
                        if (!string.IsNullOrEmpty(setting.ContentStr))
                        {

                            ExplorerSearchOptionV3 searchOption = SerializerHelper.DeserializeByJsonConvert<ExplorerSearchOptionV3>(setting.ContentStr);
                            if (searchOption != null && (searchOption.ColumnOperationLogic == ExplorerSearchColumnOperationLogic.Contains))
                            {
                                var originString = searchOption.Value.Replace("\"", "");
                                if (searchOption.Value.Contains("*") && (originString.Split("*").Length - 1 != originString.Replace("\"","").Length))
                                {
                                    result.IsOffline = true;
                                }
                            }
                        }
                    }
                }
                catch
                {
                    result.IsOffline = false;
                }
            }
            return result;
        }
    }
}
