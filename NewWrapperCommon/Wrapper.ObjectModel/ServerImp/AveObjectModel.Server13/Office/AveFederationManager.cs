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
using AvePoint.Wrapper.Common;
using Microsoft.Office.Server.Search.Administration.Query;
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.ObjectModel.Server13.Office
{
    class AveFederationManager : IAveFederationManager
    {
        private FederationManager mFederationManager;
        public IEnumerable<Wrapper.Common.Office.IAveOSource> ListSourcesWithDefault(IAveOSearchObjectFilter filter, bool includeInactive, out Wrapper.Common.Office.IAveOSource defaultSource)
        {
            Source s;
            var sources = this.mFederationManager.ListSourcesWithDefault(((AveOSearchObjectFilter)filter).Filter, includeInactive, out s);
            defaultSource = new AveOSource(s);
            List<AveOSource> aveOSourceList = new List<AveOSource>();
            foreach (var source in sources)
            {
                //yield return new AveOSource(source);
                aveOSourceList.Add(new AveOSource(source));
            }
            return aveOSourceList;
        }

        public AveFederationManager(IAveOSearchServiceApplication searchServiceApplication)
        {
            this.mFederationManager = new FederationManager(((AveOSearchServiceApplication)searchServiceApplication).SearchServiceApplication);
        }


        public IDictionary<Guid, string> GetSourceNamesByIds(IEnumerable<Guid> sourceIds)
        {
            return AveAssemblyUtility.InvokeMethod(mFederationManager, "GetSourceNamesByIds", new object[] { sourceIds }) as IDictionary<Guid, string>;
        }
    }
}
