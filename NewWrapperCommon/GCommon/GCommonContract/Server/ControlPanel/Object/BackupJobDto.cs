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
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.Object
{
    [DataContract]
    public class BackupJobDto : BaseJobDto
    {
        public BackupJobDto()
        {
            this.Type = ContractConstants.BACKUP_JOB_DTO_TYPE;
        }

        //private List<BackupJobDto> subJob = new List<BackupJobDto>();
        //public List<BackupJobDto> SubJob
        //{
        //    get { return this.subJob ; }
        //    set { this.subJob = value; }
        //}

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_1)]
        public int Weight { set; get; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_2)]
        public int BackupLevel { set; get; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_3)]
        public int StopState { set; get; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string PlanID { set; get; }

    }
}
