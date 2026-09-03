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

namespace AvePoint.Wrapper.Core.Common
{
    [System.Diagnostics.Conditional("DEBUG")]
    [AttributeUsage(AttributeTargets.All)]
    public class NativeAPIAttribute : Attribute
    {
        private NativeAPIType apiType;
        private NativeAPIEnvironment environment;
        private string className;
        private string name;
        /// <summary>
        /// API Type
        /// </summary>
        public NativeAPIType APIType { get { return apiType; } }
        /// <summary>
        /// API Environment
        /// </summary>
        public NativeAPIEnvironment Environment { get { return environment; } }

        /// <summary>
        /// Class Name
        /// </summary>
        public string ClassName { get { return className; } }
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get { return name; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="className"></param>
        /// <param name="name"></param>
        /// <param name="apiType"></param>
        /// <param name="environment"></param>
        public NativeAPIAttribute(string className, string name, NativeAPIType apiType, NativeAPIEnvironment environment)
        {
            this.className = className;
            this.name = name;
            this.apiType = apiType;
            this.environment = environment;
        }
    }

    /// <summary>
    /// Native API Type
    /// </summary>
    public enum NativeAPIType
    {
        /// <summary>
        /// method
        /// </summary>
        Method,
        /// <summary>
        /// Field
        /// </summary>
        Field,
    }

    /// <summary>
    /// Native API Environment
    /// </summary>
    [Flags]
    public enum NativeAPIEnvironment : int
    {
        /// <summary>
        /// SP07
        /// </summary>
        SP2007 = 1,
        /// <summary>
        /// SP10
        /// </summary>
        SP2010 = 2,
        /// <summary>
        /// SP13
        /// </summary>
        SP2013 = 4,
        /// <summary>
        /// O365 10
        /// </summary>
        O36510 = 8,
        /// <summary>
        /// O365 13
        /// </summary>
        O36513 = 16,
        /// <summary>
        /// All
        /// </summary>
        ALL = 0xFF,
    }
}
