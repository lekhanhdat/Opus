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
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Common.Office
{
    public interface IAveOSource
    {
         bool Active { get; }
       // public AuthenticationInformation AuthInfo { get; set; }
         bool BuiltIn { get; }
         int ConnectionTimeout { get; set; }
         string ConnectionUrlTemplate { get; set; }
         DateTime CreatedDate { get; }
         string Description { get; set; }
         bool HasPermissionToReadAuthInfo { get; }
         Guid Id { get; }
         int IndexOffset { get; set; }
         DateTime LastModifiedDate { get; }
         int MaximumResponseLength { get; set; }
         string Name { get; set; }
        //public SearchObjectOwner Owner { get; }
         Guid ProviderId { get; set; }
         IAveQueryTransform QueryTransform { get; }

         void Activate();
         bool CanEdit();
         void Commit();
       // public QueryTransform CreateQueryTransform(string queryTemplate);
       // public QueryTransform CreateQueryTransform(QueryTransformProperties overrideProperties, string queryTemplate);
         void Deactivate();
       // public void ImportFromFederatedLocation(Stream stream);
         void ImportFromFederatedLocation(string filePath);
    }
}
