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
using System.Collections;
using Microsoft.Office.Server.Search.Administration;

namespace AvePoint.ObjectModel.Server13.Office
{
    class AveOMappingCollection : AveAbstractCommonCollection, IAveOMappingCollection
    {
        private MappingCollection mMappingCollction;

        public AveOMappingCollection(MappingCollection mappingCollcetion)
            : base(mappingCollcetion)
        {
            mMappingCollction = mappingCollcetion;
        }

        public AveOMappingCollection()
            : this(new MappingCollection())
        {
        }

        public int Count
        {
            get
            {
                return mMappingCollction.Count;
            }
        }

        internal MappingCollection MappingCollction
        {
            get
            {
                return mMappingCollction;
            }
        }

        internal override object CreatElementInstance(object obj)
        {
            return new AveOMapping(obj as Mapping);
        }

        public IAveOMapping this[int i]
        {
            get
            {
                return new AveOMapping(mMappingCollction[i]);
            }

            set
            {
                mMappingCollction[i] = (value as AveOMapping).Mapping;
            }
        }

        public void Add(IAveOMapping mapping)
        {
            mMappingCollction.Add((mapping as AveOMapping).Mapping);
        }

        public void Clear()
        {
            mMappingCollction.Clear();
        }

        public bool Contains(IAveOMapping mapping)
        {
            return mMappingCollction.Contains((mapping as AveOMapping).Mapping);
        }

        public new IEnumerator<IAveOMapping> GetEnumerator()
        {
            foreach (Mapping mapping in mMappingCollction)
            {
                yield return new AveOMapping(mapping);
            }
        }

        public int IndexOf(IAveOMapping mapping)
        {
            return mMappingCollction.IndexOf((mapping as AveOMapping).Mapping);
        }

        public void Insert(int i, IAveOMapping mapping)
        {
            mMappingCollction.Insert(i, (mapping as AveOMapping).Mapping);
        }

        public bool Remove(IAveOMapping mapping)
        {
            return mMappingCollction.Remove((mapping as AveOMapping).Mapping);
        }

        public void RemoveAt(int i)
        {
            mMappingCollction.RemoveAt(i);
        }
    }
}
