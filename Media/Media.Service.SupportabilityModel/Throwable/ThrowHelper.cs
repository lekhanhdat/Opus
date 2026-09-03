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




namespace AvePoint.Media.Service.SupportabilityModel
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using AvePoint.Media.Service.DomainModel.Event;
    #endregion

    /// <summary>
    /// This file defines an  class used to throw exceptions in Media code.
    /// The main purpose is to reduce code size.
    /// <remarks>
    /// The old way to throw an exception generates quite a lot IL code and assembly code.
    /// Following is an example:
    /// <examples>
    /// C# source
    /// throw new ArgumentNullException("key", Environment.GetResourceString("ArgumentNull_Key"));
    /// IL code:
    /// IL_0003: ldstr "key"
    /// IL_0008: ldstr "ArgumentNull_Key"
    /// IL_000d: call string System.Environment::GetResourceString(string)
    /// IL_0012: newobj instance void System.ArgumentNullException::.ctor(string,string)
    /// IL_0017: throw
    /// which is 21bytes in IL.
    ///
    /// So we want to get rid of the ldstr and call to Environment.GetResource in IL.
    /// In order to do that, I created two enums: ExceptionResource, ExceptionArgument to represent the
    /// argument name and resource name in a small integer. The source code will be changed to
    /// ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key, ExceptionResource.ArgumentNull_Key);
    ///
    /// The IL code will be 7 bytes.
    /// IL_0008: ldc.i4.4
    /// IL_0009: ldc.i4.4
    /// IL_000a: call void System.ThrowHelper::ThrowArgumentNullException(valuetype System.ExceptionArgument)
    /// IL_000f: ldarg.0
    ///
    /// This will also reduce the Jitted code size a lot.
    /// </examples>
    ///
    /// It is very important we do this for generic classes because we can easily generate the same code
    /// multiple times for different instantiation.
    /// </remarks>
    /// </summary>
    public static partial class ThrowHelper
    {
        static List<Type> allowThrowableExceptions = ExceptionManager.DefaultAllowThrowableExceptions;

        public static void Throw<TEventException>(
            String eventMessage = default(String),
            String description = default(String),
            Exception innerEventExcetion = default(Exception))
            where TEventException : EventExceptionBase, new()
        {
            if (!allowThrowableExceptions.Contains(typeof(TEventException)))
                throw new ArgumentException($"The type: {typeof(TEventException)} is not a valid type which can be thrown by the throw helper.");
            else throw new TEventException() { EventMessage = eventMessage, EventDescription = description, InnerEventExcetion = innerEventExcetion };
        }
    }
}