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
using Microsoft.SharePoint.Publishing;
using System.Collections;

namespace AvePoint.ObjectModel.Server19
{
    public class AveImageRenditionCollection : IAveImageRenditionCollection
    {
        private ImageRenditionCollection imageRenditionCollection;

        public AveImageRenditionCollection()
        { }

        public AveImageRenditionCollection(ImageRenditionCollection imageRenditionCollection)
        {
            this.imageRenditionCollection = imageRenditionCollection;
        }

        public IAveImageRenditionCollection GetRenditions(Guid siteId)
        {
            object obj = AveAssemblyUtility.InvokeStaticMethod(typeof(ImageRenditionCollection), "GetRenditions", new Type[] { typeof(Guid) }, new object[] { siteId });
            if (obj != null)
            {
                this.imageRenditionCollection = obj as ImageRenditionCollection;
                return new AveImageRenditionCollection(this.imageRenditionCollection);
            }
            return null;
        }

        public void Add(IAveImageRendition item)
        {
            if (this.imageRenditionCollection != null)
            {
                this.imageRenditionCollection.Add((item as AveImageRendition).mageRendition);
            }
        }

        public void Clear()
        {
            if (this.imageRenditionCollection != null)
            {
                this.imageRenditionCollection.Clear();
            }
        }

        public bool Contains(IAveImageRendition item)
        {
            if (this.imageRenditionCollection != null)
            {
                return this.imageRenditionCollection.Contains((item as AveImageRendition).mageRendition);
            }
            return false;
        }

        public void CopyTo(IAveImageRendition[] array, int arrayIndex)
        {
            if (this.imageRenditionCollection != null)
            {
                ImageRendition[] list = new ImageRendition[array.Length];
                this.imageRenditionCollection.CopyTo(list, arrayIndex);
                foreach (ImageRendition item in list)
                {
                    array[arrayIndex++] = new AveImageRendition(item);
                }
            }
        }

        public int Count
        {
            get
            {
                if (this.imageRenditionCollection != null)
                {
                    return this.imageRenditionCollection.Count;
                }
                return 0;
            }
        }

        public bool IsReadOnly
        {
            get 
            {
                if (this.imageRenditionCollection != null)
                {
                    return this.imageRenditionCollection.IsReadOnly;
                }
                return true;
            }
        }

        public bool Remove(IAveImageRendition item)
        {
            if (this.imageRenditionCollection != null)
            {
                this.imageRenditionCollection.Remove((item as AveImageRendition).mageRendition);
            }
            return false;
        }

        public IEnumerator<IAveImageRendition> GetEnumerator()
        {
            if (this.imageRenditionCollection != null)
            {
                List<IAveImageRendition> list = new List<IAveImageRendition>();
                while (this.imageRenditionCollection.GetEnumerator().MoveNext())
                {
                    list.Add(new AveImageRendition(this.imageRenditionCollection.GetEnumerator().Current));
                }
                return list.GetEnumerator();
            }
            return null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (this.imageRenditionCollection != null)
            {
                List<IAveImageRendition> list = new List<IAveImageRendition>();
                while (this.imageRenditionCollection.GetEnumerator().MoveNext())
                {
                    list.Add(new AveImageRendition(this.imageRenditionCollection.GetEnumerator().Current));
                }
                return list.GetEnumerator();
            }
            return null;
        }
    }
}
