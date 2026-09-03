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
    class AveOQueryPartitionSettings : IAveOQueryPartitionSettings
    {
        private QueryPartitionSettings mQueryPartitionSettings;
        private AveOPropertyStoreSettings mPropertyStore;

        public AveOQueryPartitionSettings(QueryPartitionSettings queryPartitionSettings)
        {
            mQueryPartitionSettings = queryPartitionSettings;
        }

        #region IAveOQueryPartitionSettings members

        public IAveOPropertyStoreSettings PropertyStore
        {
            get
            {
                if (mPropertyStore == null)
                {
                    PropertyStoreSettings propertyStore = mQueryPartitionSettings.PropertyStore;
                    if (propertyStore != null)
                    {
                        mPropertyStore = new AveOPropertyStoreSettings(propertyStore);
                    }
                }
                return mPropertyStore;
            }
            set
            {
                mPropertyStore = value as AveOPropertyStoreSettings;
                mQueryPartitionSettings.PropertyStore = (mPropertyStore == null) ? null : mPropertyStore.PropertyStoreSettings;
            }
        }

        public List<IAveOQueryComponentSettings> QueryComponents
        {
            get
            {
                List<IAveOQueryComponentSettings> queryComponents = null;
                List<QueryComponentSettings> spQueryComponents = mQueryPartitionSettings.QueryComponents;
                if (spQueryComponents != null)
                {
                    queryComponents = new List<IAveOQueryComponentSettings>();
                    foreach (QueryComponentSettings queryComponentSettings in spQueryComponents)
                    {
                        if (queryComponentSettings != null)
                        {
                            queryComponents.Add(new AveOQueryComponentSettings(queryComponentSettings));
                        }
                        else
                        {
                            queryComponents.Add(null);
                        }
                    }
                }
                return queryComponents;
            }
        }

        public int UniqueID
        {
            get { return mQueryPartitionSettings.UniqueID; }
            set { mQueryPartitionSettings.UniqueID = value; }
        }

        #endregion
    }
}
