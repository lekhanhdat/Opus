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
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMTermUsageDao : BaseDao<RMTermUsage>, IRMTermUsageDao
    {
        public List<RMTermUsage> GeteTermUsage(int sourceFlag)
        {
            using (var context = GetNewContext())
            {
                if ((SourceFlag)sourceFlag != Contract.Explorer.SourceFlag.All)
                {
                    return context.TermUsage.Where(t => t.Size != 0 && t.SourceFlag == sourceFlag).OrderByDescending(t => t.Size).ThenBy(t => t.TermName).Take(10).ToList();

                }
                else
                {
                    return context.TermUsage.Where(t => t.Size != 0).OrderByDescending(t => t.Size).ThenBy(t => t.TermName).Take(10).ToList();
                }
            }
        }

        public void SaveTermUsage(List<RMTermUsage> datas)
        {
            using (var context = GetNewContext())
            {
                context.TermUsage.AddRange(datas);
                context.SaveChanges();
            }
        }

        public void UpdateTermUsage(List<RMTermUsage> datas)
        {

            try
            {
                using (var context = GetNewContext())
                {
                    foreach (var data in datas)
                    {
                        if (context.TermUsage.Any(s => s.TermId == data.TermId && s.SourceFlag == data.SourceFlag))
                        {
                            var usage = context.TermUsage.Where(s => s.TermId == data.TermId && s.SourceFlag == data.SourceFlag).FirstOrDefault();

                            usage.TermName = data.TermName;
                            usage.Size = data.Size;
                            ApplyCurrentValues(context, usage);
                            //BatchUpdate(entities);//TODO xwwang run job test
                        }
                        else
                        {
                            context.TermUsage.Add(data);
                            context.SaveChanges();
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
            

        }

        [Obsolete]
        public void RemoveAll(SourceFlag sourceFlag)
        {
            
        }
    }
}
