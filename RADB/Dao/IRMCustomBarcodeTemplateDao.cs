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
    public interface IRMCustomBarcodeTemplateDao
    {
        Task<RMCustomBarcodeTemplate> GetByIdAsync(int id);
        Task<List<RMCustomBarcodeTemplate>> GetBySuiteIdAsync(Guid suiteId);
        Task<List<RMCustomBarcodeTemplate>> GetBySuiteIdAndTypeAsync(Guid suiteId, BarcodeTemplateType type);
        Task<RMCustomBarcodeTemplate> GetDefaultTemplateAsync(Guid suiteId, BarcodeTemplateType type);
        Task<int> CreateAsync(RMCustomBarcodeTemplate template);
        Task<bool> UpdateAsync(RMCustomBarcodeTemplate template);
        Task<bool> DeleteAsync(int id);
        Task<int> DeleteBySuiteIdAsync(Guid suiteId);
        Task<bool> IsNameExistsAsync(Guid suiteId, string name, int? excludeId = null);
        Task<bool> SetAsDefaultAsync(int id);
        Task<int> BatchUpdateAsync(List<RMCustomBarcodeTemplate> templates);
        Task<List<int>> BatchCreateAsync(List<RMCustomBarcodeTemplate> templates);
        Task<int> BatchDeleteAsync(List<int> ids);
        Task<RMBarcodeTemplate> GetDefaultTemplateAsync(BarcodeTemplateType type);
        Task<bool> CheckDefaultBarcodeTemplateExistByTypeAsync(BarcodeTemplateType type);
    }
}
