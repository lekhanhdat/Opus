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
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMDataOfDayDao : BaseDao<RMDataOfDay>, IRMDataOfDayDao
    {
        public void AddDatas(List<DataOfDayDto> dataDtos)
        {

            var datas = ConvertToRMDataOfDay(dataDtos);
            using (var context = this.GetNewContext())
            {
                context.DataOfDay.AddRange(datas);
                context.SaveChanges();
            }

        }

        public List<RMDataOfDay> GetDatas(BoardQueryOption options)
        {
            List<RMDataOfDay> result = new List<RMDataOfDay>();
            using (var context = this.GetNewContext())
            {
                if (options == null)
                {
                    result = context.DataOfDay.ToList();
                }
                else
                {
                    //result = context.DataOfDay.Where(d => options.DateRange.StartTime > DateTime.Parse(d.Date) && options.DateRange.EndTime < DateTime.Parse(d.Date)).ToList();
                }
            }
            return result;
        }

        public void RemoveAll(SourceFlag SourceFlag)
        {
            using (var context = this.GetNewContext())
            {
                var datas = context.DataOfDay.Where(s => s.SourceFlag == (int)SourceFlag).ToList();
                if (datas.Count() > 0)
                {
                    context.DataOfDay.RemoveRange(datas);
                    context.SaveChanges();
                }
            }
        }


        public List<RMDataOfDay> FindLineChartInfoByTimeRange(DateTime start, DateTime end, Contract.Explorer.SourceFlag sourceFlag)
        {
            List<RMDataOfDay> result = new List<RMDataOfDay>();
            int flag = (int)sourceFlag;
            using (var context = this.GetNewContext())
            {
                if (sourceFlag != Contract.Explorer.SourceFlag.All)
                {
                    result = context.DataOfDay.AsNoTracking().Where(d => d.Dater > start.Ticks && d.Dater < end.Ticks && d.SourceFlag == flag).ToList();
                }
                else
                {
                    result = context.DataOfDay.AsNoTracking().Where(d => d.Dater > start.Ticks && d.Dater < end.Ticks).ToList();
                }
            }
            return result;
        }


        private List<RMDataOfDay> ConvertToRMDataOfDay(List<DataOfDayDto> dtos)
        {
            return dtos.ConvertAll(a => new RMDataOfDay()
            { Created = a.Created, Destroyed = a.Destroyed, WaitingApproval = a.WaitingApproval, Dater = a.Date });
        }
        private void SetDataOfDay(RMDataOfDay dbdata, RMDataOfDay colData)
        {
            dbdata.Created = colData.Created > 0 ? colData.Created : dbdata.Created;
            dbdata.Destroyed = colData.Destroyed > 0 ? colData.Destroyed : dbdata.Destroyed;
            dbdata.WaitingApproval = colData.WaitingApproval > 0 ? colData.WaitingApproval : dbdata.WaitingApproval;
        }
        public void AddOrUpdateDatas(List<RMDataOfDay> datas)
        {

            try
            {
                using (var context = this.GetNewContext())
                {
                    foreach (var data in datas)
                    {
                        var queryData = context.DataOfDay.AsQueryable().Where(t => t.Dater.Equals(data.Dater) && t.SourceFlag == data.SourceFlag).FirstOrDefault();
                        if (queryData == null)
                        {
                            context.DataOfDay.Add(data);
                            context.SaveChanges();
                        }
                        else
                        {
                            SetDataOfDay(queryData, data);
                            DoUpdate(queryData);
                        }
                    }


                }
            }
            catch (DbEntityValidationException dbex)
            {
                string message = string.Join("; ", dbex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                throw new DbEntityValidationException(message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
            //Batch Update or add.

        }

        private bool DoUpdate(RMDataOfDay entity)
        {
            using (var ctx = this.GetNewContext())
            {
                var entry = ctx.Entry(entity);
                if (entry.State == EntityState.Modified)
                {
                    return ctx.SaveChanges() > 0;
                }
                else if (entry.State == EntityState.Detached)
                {
                    ctx.DetachLocalObject<RMDataOfDay>(entity);
                    ctx.Set<RMDataOfDay>().Attach(entity);
                    entry.State = EntityState.Modified;
                    return ctx.SaveChanges() > 0;
                }
                return false;
            }

        }

    }
}
