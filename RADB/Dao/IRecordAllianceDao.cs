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
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IRecordAllianceDao : IBaseDao<RMRecordAlliance>
    {
        /// <summary>
        /// Physical File,  Move时更新Hold信息
        /// </summary>
        /// <param name="fileId">File Id</param>
        /// <param name="srcParentId">源端的ContainerId</param>
        /// <param name="destParentId">目的端ContainerId</param>
        void PhysicalFileMoveWithHold(Guid fileId, Guid srcBoxId, Guid destBoxId, Guid destLocationId);
        bool CanPhysicalFileMove(Guid fileId, Guid srcParentId, Guid destParentId);
        /// <summary>
        /// check one record is personal hold or disposal hold
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        bool IsRecordsHold(List<Guid> ids, long ticks);
    }
}
