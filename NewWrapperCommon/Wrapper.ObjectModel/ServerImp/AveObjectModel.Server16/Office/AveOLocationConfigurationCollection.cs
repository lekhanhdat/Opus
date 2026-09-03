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
using Microsoft.Office.Server.Search.Administration;
using AvePoint.Wrapper.Common.Office;
using System.Collections;

namespace AvePoint.ObjectModel.Server16.Office
{
    class AveOLocationConfigurationCollection : AveAbstractCommonCollection<IAveOLocationConfiguration>, IAveOLocationConfigurationCollection
    {
        private LocationConfigurationCollection mLocationConfigurationCollection;

        public AveOLocationConfigurationCollection(LocationConfigurationCollection locationConfigurationCollection)
            : base(locationConfigurationCollection)
        {
            mLocationConfigurationCollection = locationConfigurationCollection;
        }

        internal LocationConfigurationCollection LocationConfigurationCollection
        {
            get
            {
                return mLocationConfigurationCollection;
            }
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveOLocationConfiguration(t as LocationConfiguration);
        }

        public override int Count
        {
            get
            {
                return mLocationConfigurationCollection.Count;
            }
        }

        #region IAveOLocationConfiguration

        public void Add(IAveOLocationConfiguration item)
        {
            mLocationConfigurationCollection.Add((item as AveOLocationConfiguration).LocationConfiguration);
        }

        public void Clear()
        {
            mLocationConfigurationCollection.Clear();
        }

        public bool Contains(IAveOLocationConfiguration item)
        {
            return mLocationConfigurationCollection.Contains((item as AveOLocationConfiguration).LocationConfiguration);
        }

        public void CopyTo(IAveOLocationConfiguration[] array, int arrayIndex)
        {
            CopyTo(array as Array, arrayIndex);
        }

        public bool IsReadOnly
        {
            get
            {
                return mLocationConfigurationCollection.IsReadOnly;
            }
        }

        public bool Remove(IAveOLocationConfiguration item)
        {
            return mLocationConfigurationCollection.Remove((item as AveOLocationConfiguration).LocationConfiguration);
        }

        #endregion
    }
}
