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
using System.Runtime.Serialization;

namespace LS.BinarySerialization
{
    internal sealed class IntSizedArray : ICloneable
    {
        // Fields
        internal int[] negObjects;
        internal int[] objects;

        public IntSizedArray()
        {
            this.objects = new int[0x10];
            this.negObjects = new int[4];
        }


        private IntSizedArray(IntSizedArray sizedArray)
        {
            this.objects = new int[0x10];
            this.negObjects = new int[4];
            this.objects = new int[sizedArray.objects.Length];
            sizedArray.objects.CopyTo(this.objects, 0);
            this.negObjects = new int[sizedArray.negObjects.Length];
            sizedArray.negObjects.CopyTo(this.negObjects, 0);
        }


        public object Clone()
        {
            return new IntSizedArray(this);
        }


        internal void IncreaseCapacity(int index)
        {
            try
            {
                if (index < 0)
                {
                    int[] destinationArray = new int[Math.Max((int)(this.negObjects.Length * 2), (int)(-index + 1))];
                    Array.Copy(this.negObjects, 0, destinationArray, 0, this.negObjects.Length);
                    this.negObjects = destinationArray;
                }
                else
                {
                    int[] numArray2 = new int[Math.Max((int)(this.objects.Length * 2), (int)(index + 1))];
                    Array.Copy(this.objects, 0, numArray2, 0, this.objects.Length);
                    this.objects = numArray2;
                }
            }
            catch(Exception e)
            {
                throw new SerializationException(LSEnvironment.GetResourceString("Serialization_CorruptedStream"),e);
            }
        }

 

        internal int this[int index]
        {
            get
            {
                if (index < 0)
                {
                    if (-index > (this.negObjects.Length - 1))
                    {
                        return 0;
                    }
                    return this.negObjects[-index];
                }
                if (index > (this.objects.Length - 1))
                {
                    return 0;
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
                    this.objects[index] = value;
                }
            }
        }

    }
}
