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
using AvePoint.RA.Contract.CustomizeConnector.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.CustomizeConnector
{
    public interface IRMCustomizeConnectorService
    {
        System.Threading.Tasks.Task<IEnumerable<CustomizeConnectorInfo>> GetAllAsync();

        System.Threading.Tasks.Task<CustomizeConnectorInfo> GetAsync(Guid id);

        Task<CustomizeConnectorInfo> GetSimpleInfoByNameAsync(string name);

        System.Threading.Tasks.Task DeleteAsync(List<Guid> ids);
        
        System.Threading.Tasks.Task<CustomizeConnectorActionResult> AddAsync(CustomizeConnectorInfo info);

        System.Threading.Tasks.Task<CustomizeConnectorActionResult> UpdateAsync(CustomizeConnectorInfo info);

        Task<(string, string)> GenerateJsonSchemeAsync(Guid id);

        Task<List<CustomizeConnectorNameValue<string>>> ViewItemDetailForExplorerSearchAsync(Guid id);
    }
}
