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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using RMSynchronize.SyncNodeFromAOS.ChangeLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RMSynchronize.SyncStorage.CosmosDB
{
    public abstract class RMContentSourceSyncCosmosDBOperator
    {

        protected static readonly HashSet<int> s_optionalStatus = new()
        {
            (int)RMRecordStatus.Active,
            (int)RMRecordStatus.ManualPreSync,
            (int)RMRecordStatus.Retention,
            (int)RMRecordStatus.TrainingManualSync,
            (int)RMRecordStatus.MoveOverwrite,
            (int)RMRecordStatus.Moved,
        };

        public abstract SourceFlag ContentSource { get; }

        public abstract Expression<Func<Record, bool>> AddPredicate(RMSyncNodeChangeInfo changeInfo);

        public abstract Expression<Func<Record, bool>> DeletePredicate(RMSyncNodeChangeInfo changeInfo);

        public abstract Expression<Func<Record, bool>> MovePredicate(RMSyncNodeChangeInfo changeInfo);

        public abstract Expression<Func<Record, bool>> ChangeSourceFlagPredicate(RMRemoteNode changeInfo);

        public abstract Record ProcessAdd(Record item, RMSyncNodeChangeInfo changeInfo);

        public abstract Record ProcessMove(Record item, RMSyncNodeChangeInfo changeInfo);

        public abstract Record ProcessChangeSourceFlag(Record item, RMRemoteNode changeInfo);
    }
}
