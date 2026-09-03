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
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMAppProfileDao: IBaseDao<RMAppProfileInfo>
    {
        //获取当前tenant下性能最好的app profile（使用次数最少）
        RMAppProfileInfo GetBestAppProfile(Guid o365tenantId, List<int> appTypes = null);

        //性能最好的app profile在aos中已经不存在了，则需要重新从aos获取tenant下所有app profile，并且更新到db中
        Task UpdateAppProfilesForTenantAsync(Guid o365tenantId, List<RMAppProfileInfo> appProfileInfos);

        //当前tenant没有app profile，移除DB中当前tenant的app profile记录
        void RemoveAppProfilesForTenant(List<Guid> o365tenantId);

        //如果新添加了app profile，该app profile的使用次数为0，所有job可能都会选择该app profile。因此需要定期获取可用app profile，并且重置使用次数
        void ResetAppProfilesForTenant(Guid o365tenantId, List<RMAppProfileInfo> appProfileInfos);
        RMAppProfileInfo GetBestDedicatedAppProfile(Guid o365tenantId, List<int> appTypes = null);
    }
}
