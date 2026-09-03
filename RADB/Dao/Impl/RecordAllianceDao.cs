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
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Explorer;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RecordAllianceDao: BaseDao<RMRecordAlliance> , IRecordAllianceDao
    {
        /// <summary>
        /// Physical File,  Move时更新Hold信息, 没有处理Conflict， 外围逻辑可以自己实现或者在这里加上冲突处理
        /// </summary>
        /// <param name="fileId">File Id</param>
        /// <param name="srcParentId">源端的ContainerId</param>
        /// <param name="destBoxId">目的端ContainerId</param>
        public void PhysicalFileMoveWithHold(Guid fileId, Guid srcBoxId, Guid destBoxId, Guid destLocationId)
        {
            //可以参考 UpdateParentIdForAlliance
            using (var ctx = GetNewContext())
            {
                //先检查源端的Hold状态
                //&& a.AllianceType == RecordsConstants.RecordHold_PhyProfile
                List<RMRecordAlliance> srcHolds = ctx.Alliance.Where(a => (a.RecordsId == fileId || a.RecordsId == srcBoxId)).ToList();
                RMRecordAlliance destHold = destBoxId == Guid.Empty ? null : ctx.Alliance.FirstOrDefault(a => a.RecordsId == destBoxId);
                if (srcHolds.Any(a => a.RecordsId == fileId))
                {
                    //File本身是Hold的
                    RMRecordAlliance srcHold = srcHolds.First(a => a.RecordsId == fileId);
                    if (destHold == null)
                    {
                        //目的端Container没有Hold,  只更新本身的ParentId
                        srcHold.BoxId = destBoxId;
                        srcHold.LocationId = destLocationId;
                        ctx.SaveChanges();
                    }
                    else
                    {
                        ////目的端的Container 有Hold, 比较HOld Id或者ReleaseTime是否相同
                        //if (srcHold.HoldReleaseTime != destHold.HoldReleaseTime)
                        //{
                        //    //异常失败, 不允许Move
                        //    throw new GCommon.Utility.AveException("Dest container has a different hold time.");
                        //}
                        //else
                        //{
                        //    //删除file的记录
                        //    ctx.Alliance.Remove(srcHold);
                        //    ctx.SaveChanges();
                        //}
                    }
                }
                else if (srcBoxId != Guid.Empty && srcHolds.Any(a => a.RecordsId == srcBoxId))
                {
                    //File本身没有Hold,  但源端Box有Hold
                    if (destHold == null)
                    {
                        RMRecordAlliance srcContainerHOld = srcHolds.First(a => a.RecordsId == srcBoxId);
                        //目的端Container没有Hold, 按源端Container新建一个File级别的Hold
                        ctx.Alliance.Add(new RMRecordAlliance()
                        {
                            RecordsId = fileId,
                            BoxId = destBoxId,
                            AllianceType = srcContainerHOld.AllianceType,
                            HoldBy = srcContainerHOld.HoldBy,
                            HoldId = srcContainerHOld.HoldId,
                            Level = (int)Contract.RMWeb.Tree.Base.RMNodeType.PhyFile,
                            HoldReleaseTime = srcContainerHOld.HoldReleaseTime
                        });
                        ctx.SaveChanges();
                    }
                    else
                    {
                        //目的端Container 有Hold, 啥也不用做
                    }

                }
            }
        }

        public bool CanPhysicalFileMove(Guid fileId, Guid srcParentId, Guid destParentId)
        {
            using (var ctx = GetNewContext())
            {
                //先检查源端的Hold状态
                //&& a.AllianceType == RecordsConstants.RecordHold_PhyProfile
                List<RMRecordAlliance> srcHolds = ctx.Alliance.Where(a => (a.RecordsId == fileId || a.RecordsId == srcParentId)).ToList();
                RMRecordAlliance destHold = ctx.Alliance.FirstOrDefault(a => a.RecordsId == destParentId);
                if (srcHolds.Any(a => a.RecordsId == fileId))
                {
                    RMRecordAlliance srcHold = srcHolds.First(a => a.RecordsId == fileId);
                    //File本身是Hold的
                    if (destHold != null)
                    {
                        //目的端的Container 有Hold, 比较HOld Id或者ReleaseTime是否相同
                        if (srcHold.HoldId != destHold.HoldId)
                        {
                            //异常失败, 不允许Move
                            //throw new GCommon.Utility.AveException("Dest container has a different hold time.");
                            return false;
                        }
                    }
                }
                return true;
            }
        }
        /// <summary>
        /// check one record is personal hold or disposal hold
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool IsRecordsHold(List<Guid> ids, long ticks)
        {
            using (var ctx = GetNewContext())
            {
                int disposalCount = ctx.Alliance.AsQueryable().Count(a => a.HoldReleaseTime > ticks && ids.Any(temp => temp == a.RecordsId));
                if (disposalCount > 0)
                {
                    return true;
                }
                int loanCount = ctx.LoanAlliance.AsQueryable().Count(a => ids.Any(temp => temp == a.RecordsId));
                return loanCount > 0;
            }
        }
    }
}
