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



namespace AvePoint.GCommon.MicroKernel
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Runtime.Remoting.Messaging;
    #endregion

    /// <summary>
    /// InvocationContext represents the context of a method invocation.
    /// </summary>
    public class InvocationContext
    {
        /// <summary>
        /// A <see cref="String"/> a string value.
        /// </summary>
        public String StackTrace { get; set; }

        /// <summary>
        /// A <see cref="IMethodCallMessage"/> object represents the method call.
        /// </summary>
        public IMethodCallMessage Request { get; set; }

        /// <summary>
        /// A <see cref="ReturnMessage"/> object represents the return of the method call.
        /// </summary>
        public ReturnMessage Reply { get; set; }

        /// <summary>
        /// A <see>
        ///     <cref>IDictionary{object, object}</cref>
        ///   </see>  object used to set extra contextual information.
        /// </summary>
        public IDictionary<Object, Object> Properties { get; set; }
    }
}