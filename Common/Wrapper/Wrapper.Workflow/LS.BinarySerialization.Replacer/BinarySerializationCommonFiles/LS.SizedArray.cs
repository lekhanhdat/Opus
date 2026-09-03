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
using System.Reflection;
using System.Runtime.Serialization;

namespace LS.BinarySerialization
{
    [Serializable]
    internal sealed class SizedArray : ICloneable
    {
        internal object[] negObjects;
        internal object[] objects;

        internal SizedArray()
        {
            this.objects = new object[0x10];
            this.negObjects = new object[4];
        }

        internal SizedArray(int length)
        {
            this.objects = new object[length];
            this.negObjects = new object[length];
        }

        private SizedArray(SizedArray sizedArray)
        {
            this.objects = new object[sizedArray.objects.Length];
            sizedArray.objects.CopyTo(this.objects, 0);
            this.negObjects = new object[sizedArray.negObjects.Length];
            sizedArray.negObjects.CopyTo(this.negObjects, 0);
        }

        public object Clone()
        {
            return new SizedArray(this);
        }

        internal void IncreaseCapacity(int index)
        {
            try
            {
                if (index < 0)
                {
                    object[] destinationArray = new object[Math.Max((int)(this.negObjects.Length * 2), (int)(-index + 1))];
                    Array.Copy(this.negObjects, 0, destinationArray, 0, this.negObjects.Length);
                    this.negObjects = destinationArray;
                }
                else
                {
                    object[] objArray2 = new object[Math.Max((int)(this.objects.Length * 2), (int)(index + 1))];
                    Array.Copy(this.objects, 0, objArray2, 0, this.objects.Length);
                    this.objects = objArray2;
                }
            }
            catch (Exception e)
            {
                throw new SerializationException("Serialization_CorruptedStream",e);
            }
        }

        internal object this[int index]
        {
            get
            {
                if (index < 0)
                {
                    if (-index > (this.negObjects.Length - 1))
                    {
                        return null;
                    }
                    return this.negObjects[-index];
                }
                if (index > (this.objects.Length - 1))
                {
                    return null;
                }
                return this.objects[index];
            }
            set
            {
                if (index < 0)
                {
                    if (-index > (this.negObjects.Length - 1))
                    {
                        this.IncreaseCapacity(index);
                    }
                    this.negObjects[-index] = value;
                }
                else
                {
                    if (index > (this.objects.Length - 1))
                    {
                        this.IncreaseCapacity(index);
                    }
                    object obj1 = this.objects[index];
                    this.objects[index] = value;
                }
            }
        }
    }
}
