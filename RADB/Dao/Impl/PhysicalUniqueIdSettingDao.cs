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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AvePoint.GCommon.Utility;
using AvePoint.RA.DB.Model;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class PhysicalUniqueIdSettingDao : BaseDao<RMPhysicalUniqueIdSetting>, IPhysicalUniqueIdSettingDao
    {
        public RMPhysicalUniqueIdSetting LoadingUniqueIdSetting()
        {
            using var context = GetNewContext();
            return context.PhysicalUniqueIdSetting.FirstOrDefault();
        }

        public async Task<bool> UpdateUniqueIdSettingAsync(RMPhysicalUniqueIdSetting setting)
        {
            using var content = GetNewContext();
            var settings = content.PhysicalUniqueIdSetting.AsQueryable().ToList();
            if (settings.Count == 0)
            {
                content.PhysicalUniqueIdSetting.Add(setting);
                return content.SaveChanges() > 0;
            }

            var oldSetting = settings.FirstOrDefault();
            {
                ArgumentCheck.NotNull(oldSetting, nameof(oldSetting));
                oldSetting.IsGlobalSetting = setting.IsGlobalSetting;

                oldSetting.BoxTemplatePrefix= setting.BoxTemplatePrefix;
                oldSetting.BoxTemplateNumberOfDigits = setting.BoxTemplateNumberOfDigits;

                oldSetting.FolderTemplatePrefix = setting.FolderTemplatePrefix;
                oldSetting.FolderTemplateNumberOfDigits = setting.FolderTemplateNumberOfDigits;

                oldSetting.RecordTemplatePrefix = setting.RecordTemplatePrefix;
                oldSetting.RecordTemplateNumberOfDigits = setting.RecordTemplateNumberOfDigits;

                oldSetting.CustomTemplatePrefix = setting.CustomTemplatePrefix;
                oldSetting.CustomTemplateNumberOfDigits = setting.CustomTemplateNumberOfDigits;
                return await this.UpdateAsync(oldSetting);
            }
        }
    }
}
