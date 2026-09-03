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
using System.Reflection;

namespace AvePoint.Wrapper.Mapping
{
    public class AveCustomFieldMappingForDynamic : IAveCustomFieldMappingFactory
    {
        private IAveCustomFieldMapping customMapping = null;

        public bool IsNull
        {
            get { return (customMapping == null); }
        }

        /// <summary>
        /// Get the custom field mapping class in the specified assembly.
        /// </summary>
        /// <param name="assembly">The byte[] of the assembly content</param>
        /// <exception cref="ArgumentExcpetion">Will throw ArgumentException if something wrong when we try to get the expected class</exception>
        public AveCustomFieldMappingForDynamic(byte[] assembly)
            : this(assembly, string.Empty)
        {
        }

        /// <summary>
        /// Get the specified custom field mapping class in the specified assembly
        /// </summary>
        /// <param name="assembly">The byte[] of the assembly content</param>
        /// <param name="fullTypeName">The full type name which you want to get from the assembly</param>
        /// <exception cref="ArgumentException">Will throw ArgumentException if something wrong when we try to get the expected class</exception>
        public AveCustomFieldMappingForDynamic(byte[] assembly, string fullTypeName)
        {
            Assembly ass = Assembly.Load(assembly);
            if (ass == null)
            {
                throw new ArgumentException("This is not a valid assembly.");
            }

            ConstructorInfo ci = null;
            if (!string.IsNullOrEmpty(fullTypeName))
            {
                Type t = ass.GetType(fullTypeName);
                if (t == null)
                {
                    throw new ArgumentException(string.Format("There is no expected type {0} in the assembly.", fullTypeName));
                }

                Type baseIntr = t.GetInterface(typeof(IAveCustomFieldMapping).ToString());
                if (baseIntr == null)
                {
                    throw new ArgumentException("The base type of the class is not expected.");
                }

                ci = t.GetConstructor(new Type[0]);
            }
            else
            {
                foreach (Type t in ass.GetTypes())
                {
                    Type baseIntr = t.GetInterface(typeof(IAveCustomFieldMapping).ToString());
                    if (baseIntr != null)
                    {
                        ci = t.GetConstructor(new Type[0]);
                        break;
                    }
                }
            }
            
            customMapping = ci == null ? null : ci.Invoke(null) as IAveCustomFieldMapping;
        }

        public IAveCustomFieldMapping GetMappingForListOrWeb(object listOrWeb)
        {
            return customMapping;
        }

        public IAveCustomFieldMapping GetMappingForList(IAveFieldMappingConditionInfo condition)
        {
            return customMapping;
        }

        public string GetValueFromGuiMapping(AveSourceFieldValueInfo source)
        {
            return null;
        }
    }
}
