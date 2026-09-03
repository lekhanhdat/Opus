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
//using AvePoint.RA.DB.Explorer;
using AvePoint.RA.Common;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMRecordsUpdateTempDao : BaseDao<RMRecordsUpdateTemp>, IRMRecordsUpdateTempDao
    {
        public void DeleteFinishedTempRecords(string jobid)
        {
            using (var ctx = GetNewContext())
            {
                var finishedRecord = ctx.RecordsUpdateTemp.Where(r => r.TempJobId == jobid).FirstOrDefault();
                if (finishedRecord != null && finishedRecord.Waiting4OtherSourceChangeTerm)
                {
                    return;
                }
                ctx.RecordsUpdateTemp.Remove(finishedRecord);
                ctx.SaveChanges();
            }
        }
        public string GetFailedRecords(string jobid)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RecordsUpdateTemp.Where(r => r.TempJobId == jobid).FirstOrDefault()?.FailedRecords;
            }
        }

        public RMRecordsUpdateTemp GetRealTimeJob(string jobid)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RecordsUpdateTemp.Where(r => r.TempJobId == jobid).FirstOrDefault();
            }
        }

        public void InsertUpdateTemp(string jobid, string result, int status = -1, string startItems = "")
        {
            if (jobid.IsNullOrEmpty()) return;
            using (var ctx = GetNewContext())
            {
                RMRecordsUpdateTemp temp = null;
                temp = ctx.RecordsUpdateTemp.Where(r => r.TempJobId == jobid).FirstOrDefault();
                if (temp == null)
                {
                    temp = new RMRecordsUpdateTemp();
                    temp.TempJobId = jobid;
                    temp.FailedRecords = result;
                    if (status > 0)
                    {
                        temp.Status = status;
                    }
                    if (!string.IsNullOrEmpty(startItems))
                    {
                        temp.ProcessRecords = startItems;
                    }
                    temp.TimeStamp = DateTime.UtcNow;
                    ctx.RecordsUpdateTemp.Add(temp);
                    ctx.SaveChanges();
                }
                else
                {
                    if (!string.IsNullOrEmpty(result))
                    {
                        if (string.IsNullOrEmpty(temp.FailedRecords))
                        {
                            temp.FailedRecords = result;
                        }
                        else
                        {
                            temp.FailedRecords = temp.FailedRecords + ";" + result;
                        }
                    }
                    if (status > 0 && temp.Status != RecordsConstants.Explorer_RealTime_Failed_Partial)
                    {
                        //keep change term failed status
                        if (temp.Waiting4OtherSourceChangeTerm && temp.Status == RecordsConstants.Explorer_RealTime_Failed_Partial
                            && (status == RecordsConstants.Explorer_RealTime_Running || status == RecordsConstants.Explorer_RealTime_Finished))
                        {
                            status = RecordsConstants.Explorer_RealTime_Failed_Partial;
                        }
                        temp.Status = status;
                    }
                    if (!string.IsNullOrEmpty(startItems))
                    {
                        temp.ProcessRecords = startItems;
                    }
                    temp.TimeStamp = DateTime.UtcNow;
                    ApplyCurrentValues(ctx, temp);
                    //Update(temp);
                }
            }
        }
        public void UpdateTempWaiting4OtherSource(string jobid, bool waiting4OtherSource)
        {
            using (var ctx = GetNewContext())
            {
                RMRecordsUpdateTemp temp = null;
                temp = ctx.RecordsUpdateTemp.Where(r => r.TempJobId == jobid).FirstOrDefault();
                if (temp == null)
                {
                    return;
                }
                else
                {
                    temp.Waiting4OtherSourceChangeTerm = waiting4OtherSource;
                    temp.TimeStamp = DateTime.UtcNow;
                    ApplyCurrentValues(ctx, temp);
                    //Update(temp);
                }
            }
        }

        public void DeleteDirtData()
        {
            using (var ctx = GetNewContext())
            {
                var dirtData = ctx.RecordsUpdateTemp.Where(d => !d.Waiting4OtherSourceChangeTerm 
                && (d.Status == RecordsConstants.Explorer_RealTime_Failed_Partial || d.Status == RecordsConstants.Explorer_RealTime_Failed_All || d.Status == RecordsConstants.Explorer_RealTime_Finished)).ToList();
                foreach (var data in dirtData)
                {
                    if (DateTime.UtcNow - data.TimeStamp > new TimeSpan(0, 20, 0))
                    {
                        ctx.RecordsUpdateTemp.Remove(data);
                        ctx.SaveChanges();
                    }
                    //Delete(data);
                }
            }
        }
        //private ExplorerDbContext GetExplorerContext()
        //{
        //    return new ExplorerDbContext();
        //}
    }
}
