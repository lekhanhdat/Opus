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
using System.Reflection;
using System.Runtime.Serialization;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.Workflow;

namespace LS.BinarySerialization
{
    internal sealed class BinaryAssemblyInfo
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        // Fields
        internal Assembly assembly;
        internal string assemblyString;

        internal BinaryAssemblyInfo(string assemblyString)
        {
            this.assemblyString = assemblyString;
        }

        internal BinaryAssemblyInfo(string assemblyString, Assembly assembly)
        {
            this.assemblyString = assemblyString;
            this.assembly = assembly;
        }

        internal Assembly GetAssembly()
        {
            if (this.assembly == null)
            {
                try
                {
                    //PerformanceIssue
                    Console.WriteLine("performance issue:" + this.assemblyString);
                    this.assembly = Assembly.Load(this.assemblyString);
                }
                catch(Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.LoadAssemblyError, e.ToString());
                    if (this.assembly == null)
                    {
                        throw new SerializationException("Serialization_AssemblyNotFound");
                    }
                }
            }
            return this.assembly;
        }
    }
}
