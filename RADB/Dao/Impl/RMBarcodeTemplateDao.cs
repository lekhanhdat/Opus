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
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMBarcodeTemplateDao : BaseDao<RMBarcodeTemplate>, IRMBarcodeTemplateDao
    {
        public IRMBarcodeTemplateColumnMembershipDao RMBarcodeTemplateColumnMembershipDao { get; set; }
        public bool CheckBarcodeTemplateExist(int type)
        {
            using (var context = GetNewContext())
            {
                bool exist = false;
                exist = context.BarcodeTemplate.AsQueryable().Any(b => b.Type == type);
                return exist;
            }
        }

        public bool SaveBarcodeTemplate(RMBarcodeTemplate template)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    template.ModifyTime = DateTime.UtcNow.Ticks;
                    var exist = context.BarcodeTemplate.Any(d => d.Type == template.Type);
                    if (!exist)
                    {
                        context.BarcodeTemplate.Add(template);
                        bool isSaveSuccess = context.SaveChanges() > 0;
                        if (template.ColumnDList != null)
                        {
                            RMBarcodeTemplateColumnMembershipDao.CreateOrUpdateTemplateColumnMemberShips(template.Type, template.ColumnDList);
                        }
                        return isSaveSuccess;
                    }
                }
                return false;
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

        public async Task<bool> UpdateBarcodeTemplateAsync(RMBarcodeTemplate template)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    var entity = context.BarcodeTemplate.FirstOrDefault(t => t.Type == template.Type);

                    entity.ImageColumnA = template.ImageColumnA;
                    entity.ColumnB = template.ColumnB;
                    entity.ColumnC = template.ColumnC;
                    entity.ColumnE = template.ColumnE;
                    entity.ColumnF = template.ColumnF;
                    entity.Prefix = template.Prefix;
                    entity.ImageType = template.ImageType;
                    entity.ImageName = template.ImageName;
                    entity.ModifyTime = DateTime.UtcNow.Ticks;
                    bool updateTemplate= await this.UpdateAsync(entity);
                    if (template.ColumnDList != null)
                    {
                        RMBarcodeTemplateColumnMembershipDao.CreateOrUpdateTemplateColumnMemberShips(template.Type, template.ColumnDList);
                    }
                    return updateTemplate;
                }
            }
            catch (DbEntityValidationException dbex)
            {
                string message = string.Join("; ", dbex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                throw new DbEntityValidationException(message);
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public RMBarcodeTemplate GetTemplateByType(int type)
        {
            try
            {
                using (var ctx = GetNewContext())
                {
                    var template = ctx.BarcodeTemplate.FirstOrDefault(t => t.Type == type);
                    if (template != null)
                    {
                        template.ColumnDList = ctx.BarcodeTemplateColumnMembership.Where(c => c.Type == type).Select(c => c.ColumnName).ToList();
                    }
                    return template;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }


    }
}
