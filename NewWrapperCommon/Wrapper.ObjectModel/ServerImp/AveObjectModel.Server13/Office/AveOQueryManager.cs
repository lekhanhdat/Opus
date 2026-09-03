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
using AvePoint.Wrapper.Common.Office;
using System.Xml;
using Microsoft.Office.Server.Search.Query;

namespace AvePoint.ObjectModel.Server13.Office
{
    class AveOQueryManager : IAveOQueryManager
    {
        private QueryManager mQueryManager;

        public AveOQueryManager(QueryManager queryManager)
        {
            mQueryManager = queryManager;
        }

        public AveOQueryManager()
            : this(new QueryManager())
        { }

        public XmlDocument GetResults()
        {
            foreach (LocationList list in mQueryManager)
            {
                if (list != null)
                {
                    return mQueryManager.GetResults(list);
                }
            }
            return null;
        }

        public XmlDocument GetResults(IAveOLocationList locationList)
        {
            return mQueryManager.GetResults((locationList as AveOLocationList).LocationList);
        }

        public XmlDocument GetResults(IAveOLocationList locationList, int count)
        {
            return this.GetResults(locationList, count, locationList.StartItem);
        }

        public XmlDocument GetResults(IAveOLocationList locationList, int count, int start)
        {
            return (XmlDocument)AveAssemblyUtility.InvokeMethod(mQueryManager, "GetResults", new Type[] { typeof(LocationList), typeof(int), typeof(int) }, new object[] { (locationList as AveOLocationList).LocationList, count, start });
        }

        public bool IsTriggered(IAveOLocationList locationList)
        {
            return mQueryManager.IsTriggered((locationList as AveOLocationList).LocationList);
        }

        public void Add(IAveOLocationList item)
        {
            mQueryManager.Add((item as AveOLocationList).LocationList);
        }
    }
}
