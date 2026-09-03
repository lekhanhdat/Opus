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
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Service.Services.JobMonitor.Detail;
using AvePoint.RA.Service.Services.JobMonitor.Summary.MigrationWorkerHanlder.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseJobDto = AvePoint.GCommon.Contract.Server.ControlPanel.Object.BaseJobDto;

namespace AvePoint.RA.Service.Services.JobMonitor.Summary.MigrationWorkerHanlder
{
    public class MigrationJobDetailWorkerHanlder : IMigrationJobDetailWorkerHanlder
    {
        public Dictionary<int, AbstractJobDetailWorker> jobTypeAndJobDetailWorkerDictionary { set; get; }
        public AbstractDaoMigrationJobDetailWorker GetDetailWorker(BaseJobDto jobDto)
        {
            AbstractDaoMigrationJobDetailWorker worker = null;
            if (jobTypeAndJobDetailWorkerDictionary.ContainsKey(jobDto.Type))
            {
                if (jobTypeAndJobDetailWorkerDictionary[jobDto.Type] is AbstractDaoMigrationJobDetailWorker)
                {
                    worker = jobTypeAndJobDetailWorkerDictionary[jobDto.Type] as AbstractDaoMigrationJobDetailWorker;
                }
            }
            return worker;
        }
    }
}
