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
using AvePoint.RA.Contract.Discovery.Model.Query.FileSystem.Parameter;
using System.Linq;

namespace AvePoint.RA.Service.Services.Discovery.FileSystem.Query.General.Inactive
{
    public abstract class RMDiscoveryFSInactiveDataQuerier<T> : RMDiscoveryFSDataQuerier<T>
    {
        public RMDiscoveryFSInactiveDataQuerier(RMDiscoveryFSQueryParameter queryParameter) : base(queryParameter)
        {
        }

        protected override string GetDataTable(bool queryNodeInfo = false)
        {
            if (queryNodeInfo)
            {
                return _queryParameter.NodeQueryParameter.ViewMode == RMDiscoveryFSNodeViewMode.Container ? "RMFSContainerInactiveData" : "RMFSConnectionInactiveData";
            }

            if (_queryParameter.NodeQueryParameter.ViewMode == RMDiscoveryFSNodeViewMode.Container &&
                _queryParameter.NodeQueryParameter.ContainerIds.Any())
            {
                return "RMFSContainerInactiveData";
            }

            if (_queryParameter.NodeQueryParameter.ViewMode == RMDiscoveryFSNodeViewMode.ConnectionInContainer ||
                _queryParameter.NodeQueryParameter.ViewMode == RMDiscoveryFSNodeViewMode.Connection && _queryParameter.NodeQueryParameter.ConnectionIds.Any())
            {
                return "RMFSConnectionInactiveData";
            }

            return "RMFSBasicInactiveData";
        }
    }
}
