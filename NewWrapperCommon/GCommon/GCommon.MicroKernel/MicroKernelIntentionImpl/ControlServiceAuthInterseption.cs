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
    using System.Diagnostics.CodeAnalysis;

    #endregion
    /// <summary>
    /// 
    /// </summary>
    public class ControlServiceAuthInterseption : IOperationInterseption
    {
        /// <summary>
        /// 
        /// </summary>
        public static string AuthorizationToken { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// 
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "IMServManageService is unmodifiable as the cause of being referenced.")]
        public void PreInvoke(InterseptionContext context)
        {
            if (context.CoreMessage.InvocationContext.TypeKey == "AvePoint.Patch.PatchCommon.Contract.Wcf.IMWcfPatchControlService")
            {
                return;
            }

            if (context.CoreMessage.InvocationContext.TypeKey == "AvePoint.GCommon.Contract.Server.ControlPanel.IMAgentService" && context.CoreMessage.InvocationContext.MethodName == "Register")
            {

                return;
            }

            if (context.CoreMessage.InvocationContext.TypeKey == "AvePoint.GCommon.Contract.Server.Service.IMServManageService" && context.CoreMessage.InvocationContext.MethodName == "Register")
            {

                return;
            }

            if (context.CoreMessage.InvocationContext.TypeKey == "AvePoint.GCommon.Contract.Server.Login.IMLoginService" && context.CoreMessage.InvocationContext.MethodName == "PrepareCredential")
            {

                return;
            }

            // Add this exception because DocAve SDK needs to call login method.
            if (context.CoreMessage.InvocationContext.TypeKey == "AvePoint.GCommon.Contract.Server.Login.IMLoginService"
                && (context.CoreMessage.InvocationContext.MethodName == "Login" || context.CoreMessage.InvocationContext.MethodName == "Login63"))
            {

                return;
            }

            if (context.CoreMessage.InvocationContext.TypeKey == "AvePoint.GCommon.Contract.Server.Service.IMServManageService" && context.CoreMessage.InvocationContext.MethodName == "ValidatePassphrase")
            {
                return;
            }

            // Add this exception because DocAve SDK needs to gets control service's version before login.
            if (context.CoreMessage.InvocationContext.TypeKey == "AvePoint.GCommon.Contract.ServiceVersion.IServiceVersionInfoService" && context.CoreMessage.InvocationContext.MethodName == "GetCurrentManagerVersion")
            {
                return;
            }

            if (context.CoreMessage.AuthorizationKey == null || !context.CoreMessage.AuthorizationKey.Equals(AuthorizationToken))
            {
                if (string.IsNullOrEmpty(context.CoreMessage.AuthorizationKey))
                {
                    if (context.CoreMessage.AuthorizationKey == null)
                    {
                        throw new Exception("Unauthorized Access Denied, No Key");
                    }
                    throw new Exception("Unauthorized Access Denied, Empty Key");
                }
                throw new Exception("Unauthorized Access Denied, Key Mismatch");
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
