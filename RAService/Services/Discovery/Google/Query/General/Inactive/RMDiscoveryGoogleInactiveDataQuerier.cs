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
using System.Linq;
using AvePoint.RA.Contract.Discovery.Model.Query.Google.Parameter;

namespace AvePoint.RA.Service.Services.Discovery.Google.Query.General.Inactive
{
    public abstract class RMDiscoveryGoogleInactiveDataQuerier<T> : RMDiscoveryGoogleDataQuerier<T>
    {
        public RMDiscoveryGoogleInactiveDataQuerier(RMDiscoveryGoogleQueryParameter queryParameter) : base(queryParameter)
        {
        }

        protected override string GetDataTable(bool queryNodeInfo = false)
        {
            if (queryNodeInfo)
            {
                return _queryParameter.NodeQueryParameter.ViewMode == RMDiscoveryGoogleNodeViewMode.Container ? "RMGoogleContainerInactiveData" : "RMGoogleDriveInactiveData";
            }

            if (_queryParameter.NodeQueryParameter.ViewMode == RMDiscoveryGoogleNodeViewMode.Container &&
                _queryParameter.NodeQueryParameter.ContainerIds.Any())
            {
                return "RMGoogleContainerInactiveData";
            }

            if (_queryParameter.NodeQueryParameter.ViewMode == RMDiscoveryGoogleNodeViewMode.DriveInContainer ||
                _queryParameter.NodeQueryParameter.ViewMode == RMDiscoveryGoogleNodeViewMode.Drive && _queryParameter.NodeQueryParameter.DriveIds.Any())
            {
                return "RMGoogleDriveInactiveData";
            }

            return "RMGoogleBasicInactiveData";
        }
    }
}
