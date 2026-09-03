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

namespace AvePoint.ObjectModel.Common.Office
{
    class AveOPropertyCollection:AveAbstractCommonCollection<IAveOProperty>,IAveOPropertyCollection
    {
        private IAveRequest mRequest;

        public AveOPropertyCollection(IAveRequest request,Dictionary<string,object>proprttyColProp)
        {
            mRequest = request;
            base.DataCache.AddPropertyies(proprttyColProp);
            InitPropertyCollection();
        }
        internal void InitPropertyCollection()
        {
            List<Dictionary<string, object>> propertyList = base.DataCache.GetProperty<List<Dictionary<string, object>>>(AveObjectModelConstant.ChildrenProperties);
            mListData = new List<IAveOProperty>(propertyList.Count);
            foreach(Dictionary<string,object>propertyProp in propertyList )
            {
                AveOProperty property = new AveOProperty(propertyProp);
                mListData.Add(property);
            }
        }

        public IAveOProperty GetPropertyByName(string strPropName)
        {
            throw new NotImplementedException();
        }
        public IAveOProperty Create(bool fIsSection)
        {
            throw new NotImplementedException();
        }
        public void Add(IAveOProperty property)
        {
            throw new NotImplementedException();
        }


        public IAveOProperty GetSectionByName(string p)
        {
            throw new NotImplementedException();
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void RemovePropertyByName(string strPropName, bool IsSection)
        {
            throw new NotImplementedException();
        }
    }
}
