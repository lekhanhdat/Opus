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
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.StubSetting;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMMiscProfileDao: IBaseDao<RMMiscProfile>
    {
        int Create(RMMiscProfile dto);
        Task<int> CreateAsync(RMMiscProfile dto);
        Task<int> UpdateAsync(RMMiscProfile dto);
        RMMiscProfile Load(string id);
        List<RMMiscProfile> LoadAll(CommonSettingResultForPage res);
        List<RMMiscProfile> LoadAllByTypeNotPage(ProfileType type);

        List<RMMiscProfile> LoadAllRecordsRules();
        RMMiscProfile Load(RMMiscProfile profile);
        Task<RMMiscProfile> LoadAsync(RMMiscProfile profile);
        int Delete(string id);
        Task<int> BatchDeleteAsync(List<string> ids);
        Task<int> SoftDeleteAsync(RMMiscProfile profile);
        bool IsNameExist(int type, string name);
        List<RMMiscProfile> LoadByTypes(List<int> types);

        /// <summary>
        /// 只有在 Cloud Archiver Migration Job里可以调用此方法来清理历史数据
        /// </summary>
        Task<int> DeleteMigratedMiscProfilesAsync();
        Task<IEnumerable<RMMiscProfile>> LoadByPager(int pageIndex, int pageSize);
        Task<long> MultiGeoInsertMiscProfileTableAsync(IEnumerable<RMMiscProfile> miscProfiles);
        Task<long> MultiGeoDeleteAllMiscProfileAsync();
    }
}
