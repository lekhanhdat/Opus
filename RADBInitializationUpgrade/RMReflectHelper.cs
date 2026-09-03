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
using AvePoint.RA.RACommonUtility.Email.Client.Compilers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RADBInitializationUpgrade
{
    public class RMReflectHelper
    {
        public static IEnumerable<T> GetInterfaceImplements<T>()
        {
            var compilerType = typeof(T);
            var assembly = Assembly.GetAssembly(compilerType);

            foreach (var type in assembly.GetTypes())
            {
                if (type.IsInterface) continue;
                if (type.GetInterfaces().Contains(compilerType))
                {
                    var instance = (T)Activator.CreateInstance(type);
                    yield return instance;
                }
            }
        }
    }
}
