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
using System.Threading.Tasks;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Contract.TemplateManagement;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMCustomBarcodeTemplateSuiteDao
    {
        Task<RMCustomBarcodeTemplateSuite> GetByIdAsync(int id);
        Task<RMCustomBarcodeTemplateSuite> GetByUniqueIdAsync(Guid uniqueId);
        Task<List<RMCustomBarcodeTemplateSuite>> GetByUniqueIdsAsync(List<Guid> uniqueIds);
        Task<List<RMCustomBarcodeTemplateSuite>> GetAllAsync();
        Task<List<RMCustomBarcodeTemplateSuite>> GetByLabelTypeAsync(BarcodeTemplateLabelType labelType);
        Task<RMCustomBarcodeTemplateSuite> GetDefaultAsync();
        Task<RMCustomBarcodeTemplateSuite> GetDefaultByLabelTypeAsync(BarcodeTemplateLabelType labelType);
        Task<RMCustomBarcodeTemplateSuite> GetByNameAsync(string name);
        Task<int> CreateAsync(RMCustomBarcodeTemplateSuite suite);
        Task<bool> UpdateAsync(RMCustomBarcodeTemplateSuite suite);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteByUniqueIdAsync(Guid uniqueId);
        Task<bool> IsNameExistsAsync(string name, int? excludeId = null);
        Task<(List<RMCustomBarcodeTemplateSuite> Suites, int TotalCount)> GetPagedAsync(int pageIndex, int pageSize, string searchName = null, BarcodeTemplateLabelType? labelType = null);
        Task<List<RMCustomBarcodeTemplateSuite>> SearchByNameAsync(string searchName, BarcodeTemplateLabelType? labelType = null);
        Task<int> GetCountAsync(string searchName = null, BarcodeTemplateLabelType? labelType = null);
    }
}
