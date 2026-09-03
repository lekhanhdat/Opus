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
    public class RMSiteCollectionSizeDao : BaseDao<RMSiteCollectionSize>, IRMSiteCollectionSizeDao
    {
        public void SaveSiteCollectionSizes(List<RMSiteCollectionSize> datas)
        {
            using (var context = this.GetNewContext())
            {
                context.SiteCollectionSize.AddRange(datas);
                context.SaveChanges();
            }
        }

        public void UpdateSiteCollectionSizes(RMSiteCollectionSize data)
        {
            try
            {
                using (var context = this.GetNewContext())
                {
                    if (context.SiteCollectionSize.Any(s => s.Id == data.Id))
                    {
                        var ss = context.SiteCollectionSize.Where(s => s.Id == data.Id).FirstOrDefault();
                        ss.Title = data.Title;
                        ss.Size = data.Size;
                        ApplyCurrentValues(context, ss);

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

        public List<RMSiteCollectionSize> GetTop10SiteCollectionSizes(int sourceFlag)
        {
            using (var context = GetNewContext())
            {
                var result = context.SiteCollectionSize.Where(a => a.SourceFlag == sourceFlag && a.Size > 0).OrderByDescending(a => a.Size).Take(10).ToList();
                return result;
            }
        }


        public void RemoveAll(int sourceFlag)
        {
            using (var context = this.GetNewContext())
            {
                string sql = "delete from {0}.RMSiteCollectionSizes where SourceFlag = {1}";
                context.Database.ExecuteSqlCommand(string.Format(sql, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName), sourceFlag));
            }
        }
    }
}
