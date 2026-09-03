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
    class AveProjectCustomFieldCollection : AveAbstractCommonCollection<IAveProjectCustomField>, IAveProjectCustomFieldCollection
    {
        private IAveRequest mRequest;
        private object locker = new object();

        public AveProjectCustomFieldCollection(IAveRequest request, List<Dictionary<string, object>> props)
        {
            this.mRequest = request;
            InitProjectCustomFieldCollection(props);
        }

        private void InitProjectCustomFieldCollection(List<Dictionary<string, object>> props)
        {
            mListData = new List<IAveProjectCustomField>(props.Count);
            foreach (var prop in props)
            {
                var pcf = new AveProjectCustomField(mRequest, prop);
                mListData.Add(pcf);
            }
        }

        public IAveProjectCustomField GetByAppAlternateId(string objectId)
        {
            lock (locker)
            {
                return mListData.Find(
                    delegate (IAveProjectCustomField t)
                    {
                        return t.AppAlternateId.Equals(objectId);
                    });
            }
        }

        public IAveProjectCustomField GetByGuid(Guid uid)
        {
            lock (locker)
            {
                return mListData.Find(
                    delegate (IAveProjectCustomField t)
                    {
                        return t.Id.Equals(uid);
                    });
            }
        }

        public IAveProjectCustomField GetById(string objectId)
        {
            throw new NotImplementedException();
        }

        public IAveProjectCustomField Add(AveProjectCustomFieldInfo info)
        {
            Dictionary<string,object> prop = mRequest.AddCustomField(info);
            AveProjectCustomField field = new AveProjectCustomField(mRequest, prop);
            mListData.Add(field);
            return field;
        }

        public IAveProjectCustomField GetByName(string name)
        {
            lock (locker)
            {
                return mListData.Find(
                    delegate (IAveProjectCustomField t)
                    {
                        return string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase);
                    });
            }
        }
    }
}
