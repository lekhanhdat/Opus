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
using AvePoint.Wrapper.Common.Office;
using Microsoft.Office.Server.Search.Administration.TopologyExport;

namespace AvePoint.ObjectModel.ServerSE.Office
{
    class AveOQueryTopologySettings : IAveOQueryTopologySettings
    {
        private QueryTopologySettings mQueryTopologySettings;

        public AveOQueryTopologySettings(QueryTopologySettings queryTopologySettings)
        {
            mQueryTopologySettings = queryTopologySettings;
        }

        #region IAveOQueryTopologySettings members

        public List<IAveOQueryPartitionSettings> Partitions
        {
            get
            {
                List<IAveOQueryPartitionSettings> partitionList = null;
                List<QueryPartitionSettings> spPartitionList = mQueryTopologySettings.Partitions;
                if (spPartitionList != null)
                {
                    partitionList = new List<IAveOQueryPartitionSettings>();
                    foreach (QueryPartitionSettings queryPartitionSettings in spPartitionList)
                    {
                        if (queryPartitionSettings != null)
                        {
                            partitionList.Add(new AveOQueryPartitionSettings(queryPartitionSettings));
                        }
                        else
                        {
                            partitionList.Add(null);
                        }
                    }
                }
                return partitionList;
            }
        }

        public List<IAveOPropertyStoreSettings> PropertyStores
        {
            get
            {
                List<IAveOPropertyStoreSettings> propertyStoresList = null;
                List<PropertyStoreSettings> spPropertyStoresList = mQueryTopologySettings.PropertyStores;
                if (spPropertyStoresList != null)
                {
                    propertyStoresList = new List<IAveOPropertyStoreSettings>();
                    foreach (PropertyStoreSettings propertyStores in spPropertyStoresList)
                    {
                        if (propertyStores != null)
                        {
                            propertyStoresList.Add(new AveOPropertyStoreSettings(propertyStores));
                        }
                        else
                        {
                            propertyStoresList.Add(null);
                        }
                    }
                }
                return propertyStoresList;
            }
        }

        #endregion
    }
}
