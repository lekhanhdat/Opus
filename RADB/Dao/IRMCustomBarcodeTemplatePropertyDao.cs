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
    public interface IRMCustomBarcodeTemplatePropertyDao
    {
        Task<RMCustomBarcodeTemplateProperty> GetByIdAsync(int id);
        Task<List<RMCustomBarcodeTemplateProperty>> GetByTemplateIdAsync(int templateId);
        Task<List<RMCustomBarcodeTemplateProperty>> GetByTemplateIdAndPositionAsync(int templateId, BarcodeTemplatePosition position);
        Task<List<RMCustomBarcodeTemplateProperty>> GetByTemplateIdsAsync(List<int> templateIds);
        Task<int> CreateAsync(RMCustomBarcodeTemplateProperty property);
        Task<bool> UpdateAsync(RMCustomBarcodeTemplateProperty property);
        Task<bool> DeleteAsync(int id);
        Task<int> DeleteByTemplateIdAsync(int templateId);
        Task<bool> IsNameExistsAsync(int templateId, string name, int? excludeId = null);
        Task<int> BatchUpdateAsync(List<RMCustomBarcodeTemplateProperty> properties);
        Task<List<int>> BatchCreateAsync(List<RMCustomBarcodeTemplateProperty> properties);
        Task<int> BatchDeleteAsync(List<int> ids);
    }
}
