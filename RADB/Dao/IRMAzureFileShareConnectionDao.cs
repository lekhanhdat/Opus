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
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMAzureFileShareConnectionDao : IBaseDao<RMAzureFileShareConnection>
    {

        bool Add(RMAzureFileShareConnection connection);

        bool Modify(RMAzureFileShareConnection connection);

        bool Remove(Guid id);

        Task<bool> RemoveAsync(List<Guid> ids);

        bool Remove(RMAzureFileShareConnection connection);

        bool Has(Guid id);

        RMAzureFileShareConnection Get(Guid id);

        RMAzureFileShareConnection Get(string name);

        List<RMAzureFileShareConnection> GetAll();

        List<RMAzureFileShareConnection> GetAllWithoutSecret();

        List<RMAzureFileShareConnection> GetAll(List<Guid> ids);

        List<RMAzureFileShareConnection> GetAllByConnectionGroup(Guid connectionGroupId);

        List<RMAzureFileShareConnection> GetAllWithoutRelatedConnectionGroup();

    }
}
