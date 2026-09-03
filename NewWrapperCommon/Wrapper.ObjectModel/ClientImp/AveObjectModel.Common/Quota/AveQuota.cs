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

namespace AvePoint.ObjectModel.Common
{
    class AveQuota : AveClientObject, IAveQuota
    {
        private IAveRequest mRequest;
        public AveQuota(IAveRequest request)
        {
            this.mRequest = request ;
            Dictionary<string, object> properties = mRequest.GetSiteStorageInfo();
            base.DataCache.AddPropertyies(properties);
        }
        public ushort QuotaID
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int InvitedUserMaximumLevel
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public long StorageMaximumLevel
        {
            get
            {
                return base.DataCache.GetProperty<long>("StorageMaximumLevel");
            }
            set
            {
                base.DataCache.AddChangedProperty("StorageMaximumLevel", value);
            }
        }

        public long StorageWarningLevel
        {
            get
            {
                return base.DataCache.GetProperty<long>("StorageWarningLevel");
            }
            set
            {
                base.DataCache.AddChangedProperty("StorageWarningLevel", value);
            }
        }

        public double UserCodeMaximumLevel
        {
            get
            {
                return base.DataCache.GetProperty<double>("UserCodeMaximumLevel");
            }
            set
            {
                base.DataCache.AddChangedProperty("UserCodeMaximumLevel", value);
            }
        }

        public double UserCodeWarningLevel
        {
            get
            {
                return base.DataCache.GetProperty<double>("UserCodeWarningLevel");
            }
            set
            {
                base.DataCache.AddChangedProperty("UserCodeWarningLevel", value);
            }
        }

        public System.Xml.XmlDocument GetStateXml()
        {
            throw new NotImplementedException();
        }

    }
}
