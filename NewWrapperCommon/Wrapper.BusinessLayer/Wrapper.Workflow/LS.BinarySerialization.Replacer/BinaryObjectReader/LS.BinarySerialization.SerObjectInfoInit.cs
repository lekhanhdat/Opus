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
using System.Collections;
using System.Reflection;

namespace LS.BinarySerialization
{
    internal sealed class SerObjectInfoInit
    {
        // Fields
        internal int objectInfoIdCount;
        internal SerStack oiPool;
        internal Hashtable seenBeforeTable;


        //internal Type parentType;
        //internal Object parentObject;

        //public Type ParentType
        //{
        //    get { return parentType; }
        //}

        //public Object Parent
        //{
        //    get
        //    {
        //        if (parentObject == null)
        //        {
        //            CreateParentInstance();
        //        }
        //        LSInvoker.SetRawProperty(parentObject,"objectInfoIdCount",objectInfoIdCount);
        //        LSInvoker.SetRawProperty(parentObject,"oiPool",oiPool.Parent);
        //        LSInvoker.SetRawProperty(parentObject, "seenBeforeTable", seenBeforeTable);
        //        return parentObject;
        //    }
        
        //}

        public SerObjectInfoInit()
        {
            this.seenBeforeTable = new Hashtable();
            this.objectInfoIdCount = 1;
            this.oiPool = new SerStack("SerObjectInfo Pool");

            //parentType = Type.GetType("System.Runtime.Serialization.Formatters.Binary.SerObjectInfoInit");

            //CreateParentInstance();
        }

        //private void CreateParentInstance()
        //{
        //    BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        //    ConstructorInfo[] cstrs = parentType.GetConstructors(flags);

        //    ConstructorInfo cstr = cstrs[0];
        //    foreach (ConstructorInfo c in cstrs)
        //    {
        //        if (c.GetParameters().Length == 0)
        //        {
        //            cstr = c;
        //            break;
        //        }
        //    }
        //    parentObject = cstr.Invoke(new object[] {});
 
        //}
        
    }
}
