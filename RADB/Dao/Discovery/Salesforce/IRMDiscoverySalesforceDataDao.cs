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
using System.Collections.Generic;
using System.Threading.Tasks;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Salesforce;
using AvePoint.RA.DB.Model.Salesforce;

namespace AvePoint.RA.DB.Dao.Discovery.Salesforce;

public interface IRMDiscoverySalesforceDataDao
{
    public Task AddNewObjectInfoAsync(string organizationId, RMDiscoverySalesforceObjectInfo sfObjectInfos);

    public Task<SfAggrerateDataDto> GetAggregateTotalDataAsync(string organizationId);

    public Task AddAggregateTotalDataAsync(string organizationId, RMDiscoverySalesforceAggregateTotalData data);

    public Task AddRecordBasicDataAsync(string organizationId, List<RMDiscoverySalesforceRecordInactiveData> data);

    public Task AddFileInactiveDatasAsync(string organizationId, List<RMDiscoverySalesforceFileInactiveData> data);
}