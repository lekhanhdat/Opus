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
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMCustomizeConnectorTemplateDao
    {
        Task<RMCustomizeConnectorTemplate> Add(RMCustomizeConnectorTemplate templateInfo);

        Task<RMCustomizeConnectorTemplate> Add(RMDbContext context, RMCustomizeConnectorTemplate templateInfo);

        Task<IEnumerable<RMCustomizeConnectorTemplate>> Add(IEnumerable<RMCustomizeConnectorTemplate> templateInfoes);

        Task<IEnumerable<RMCustomizeConnectorTemplate>> Add(RMDbContext context, IEnumerable<RMCustomizeConnectorTemplate> templateInfoes);

        Task Update(RMCustomizeConnectorTemplate templateInfo);

        Task Update(RMDbContext context, RMCustomizeConnectorTemplate templateInfo);

        Task Update(IEnumerable<RMCustomizeConnectorTemplate> templateInfoes);

        Task Update(RMDbContext context, IEnumerable<RMCustomizeConnectorTemplate> templateInfoes);

        Task Delete(Guid id);

        Task Delete(RMDbContext context, Guid id);

        Task Delete(IEnumerable<Guid> ids);

        Task Delete(RMDbContext context, IEnumerable<Guid> ids);

        Task<IEnumerable<RMCustomizeConnectorTemplate>> GetAll();
    }
}
