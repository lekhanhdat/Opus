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
using AvePoint.GCommon.Utility;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class BoardTotalDao : BaseDao<BoardTotal>, IBoardTotalDao
    {
        public void AddOrUpdate(BoardTotal BoardTotal)
        {
            try
            {
                using (var context = this.GetNewContext())
                {
                    var data = context.BoardTotal.Where(s => s.SourceFlag == BoardTotal.SourceFlag).FirstOrDefault();
                    if (null == data)
                    {
                        context.BoardTotal.Add(BoardTotal);
                        context.SaveChanges();
                    }
                    else
                    {
                        data.CreatedTotal = BoardTotal.CreatedTotal;
                        data.WaitingTotal = BoardTotal.WaitingTotal;
                        data.DestroyedTotal = BoardTotal.DestroyedTotal;
                        data.CollectionTime = BoardTotal.CollectionTime;

                        ApplyCurrentValues(context, data);
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

        public List<int> GetSourceFlags()
        {
            using (var context = this.GetNewContext())
            {
                string sql = "select distinct SourceFlag FROM {0}.BoardTotals";
                var result = context.Database.SqlQuery<int>(string.Format(sql, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName))).ToList();
                return result;
            }
        }

        public List<BoardTotal> GetTotalInfo()
        {
            List<BoardTotal> data = new List<BoardTotal>();
            using (var context = this.GetNewContext())
            {
                data = context.BoardTotal.Where(e=>e.SourceFlag>0).ToList();
            }
            return data;
        }
    }
}
