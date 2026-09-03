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
using AvePoint.GCommon.Contract.Common;
using AvePoint.RA.Contract.Object.JobMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Explorer
{
    public class RMExplorerMoveJobMessage : RMJobMessage
    {
        public RecordFlag SourceFlag { set; get; }

        public RecordFlag DestFlag { set; get; }

        public List<SourceRecord> SourceRecords { set; get; }
        //public List<BaseRecordDto> SourceRecords { set; get; }

        public MoveRecordSetting MoveSetting { set; get; }

        public MoveDestination MoveDestination { set; get; }

        public string Operator { get; set; }
    }

    public enum RecordFlag
    {
        None = -1,
        SP = 1,
        FS = 2,
        OneDrive = 6,
        Teams = 11,
        Groups = 12
    }
}
