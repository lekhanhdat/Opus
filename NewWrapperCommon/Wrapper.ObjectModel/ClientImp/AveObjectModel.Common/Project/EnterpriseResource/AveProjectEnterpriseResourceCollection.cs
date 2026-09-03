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
using System.Threading.Tasks;

using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    class AveProjectEnterpriseResourceCollection : AveAbstractCommonCollection<IAveProjectEnterpriseResource>, IAveProjectEnterpriseResourceCollection
    {
        private IAveRequest mRequest;
        private IAveSite mSite;
        private object locker = new object();

        public AveProjectEnterpriseResourceCollection(IAveRequest request, IAveSite site, List<Dictionary<string, object>> props)
        {
            this.mRequest = request;
            mSite = site;
            InitProjectEnterpriseResourceCollection(props);
        }

        private void InitProjectEnterpriseResourceCollection(List<Dictionary<string, object>> props)
        {
            mListData = new List<IAveProjectEnterpriseResource>(props.Count);
            foreach (var prop in props)
            {
                var resource = new AveProjectEnterpriseResource(mRequest, mSite, prop);
                mListData.Add(resource);
            }
        }

        public IAveProjectEnterpriseResource GetByGuid(Guid uid)
        {
            lock (locker)
            {
                return mListData.Find(
                    delegate (IAveProjectEnterpriseResource r)
                    {
                        return r.Id.Equals(uid);
                    });
            }
        }

        public IAveProjectEnterpriseResource GetById(string objectId)
        {
            throw new NotImplementedException();
        }

        public IAveProjectEnterpriseResource GetByUser(IAveUser user)
        {
            throw new NotImplementedException();
        }

        public IAveProjectEnterpriseResource GetByName(string name)
        {
            lock (locker)
            {
                return mListData.Find(
                    delegate (IAveProjectEnterpriseResource r)
                    {
                        return string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase);
                    });
            }
        }

        public IAveProjectEnterpriseResource Add(AveProjectEnterpriseResourceInfo info)
        {
            Dictionary<string, object> prop = mRequest.AddEnterpriseResource(info);
            AveProjectEnterpriseResource resource = new AveProjectEnterpriseResource(mRequest, mSite, prop);
            mListData.Add(resource);
            return resource;
        }
    }
}
