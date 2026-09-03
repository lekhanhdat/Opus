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



namespace AvePoint.GCommon.MicroKernel.MicroKernelIntentionImpl
{
    #region using directives
    using System;
    using System.Diagnostics;
    #endregion

    #region Attribute

    /// <summary>
    /// 
    /// </summary>
    [DebuggerNonUserCode]
    #endregion
    public class DefaultAuthInterseption : IOperationInterseption
    {
        /// <summary>
        /// 
        /// </summary>
        public static String AuthorizationToken { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void PreInvoke(InterseptionContext context)
        {
            if (context.CoreMessage.InvocationContext.TypeKey.Equals("AvePoint.GCommon.Contract.AgentService.IAAgentService", StringComparison.Ordinal)
                && context.CoreMessage.InvocationContext.MethodName.Equals("StartSingletonProcess", StringComparison.Ordinal))
            {
                return;
            }

            if (String.IsNullOrEmpty(AuthorizationToken)
                && context.CoreMessage.InvocationContext.TypeKey.Equals("AvePoint.GCommon.Contract.Server.ControlPanel.UpdateManager.IADeployPatchService"))
            {
                return;
            }
            if (String.IsNullOrEmpty(context.CoreMessage.AuthorizationKey)
                || !context.CoreMessage.AuthorizationKey.Equals(AuthorizationToken))
            {
                throw new UnauthorizedAccessException(
                  @"Unauthorized access to microkernel server because the communication authorization key is null or invalid, it is often caused by the installed Media service or Agent have not been successfully registered to the Control service.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void PostInvoke(InterseptionContext context)
        { }
    }
}