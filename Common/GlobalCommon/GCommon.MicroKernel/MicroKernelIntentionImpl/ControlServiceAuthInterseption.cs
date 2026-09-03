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

namespace AvePoint.GCommon.MicroKernel.MicroKernelIntentionImpl
{
    public class ControlServiceAuthInterseption : IOperationInterseption
    {
        public static string AuthorizationToken { get; set; }
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

            if (context.CoreMessage.InvocationContext.TypeKey == "AvePoint.GCommon.Contract.Server.Login.IMLoginService" && context.CoreMessage.InvocationContext.MethodName == "Login")
            {

                return;
            }
            if (context.CoreMessage.InvocationContext.TypeKey == "AvePoint.GCommon.Contract.Server.Login.IMLoginService" && context.CoreMessage.InvocationContext.MethodName == "OnlineLogin")
            {

                return;
            }
            if (context.CoreMessage.InvocationContext.TypeKey == "AvePoint.GCommon.Contract.Server.Login.IMLoginService" && context.CoreMessage.InvocationContext.MethodName == "PortalLogin")
            {

                return;
            }
            if (context.CoreMessage.InvocationContext.TypeKey == "AvePoint.GCommon.Contract.Server.Login.IMLoginService" && context.CoreMessage.InvocationContext.MethodName == "APILogin")
            {

                return;
            }

            if (context.CoreMessage.InvocationContext.TypeKey == "AvePoint.GCommon.Contract.Server.Service.IMServManageService" && context.CoreMessage.InvocationContext.MethodName == "ValidatePassphrase")
            {
                return;
            }

            if (context.CoreMessage.InvocationContext.TypeKey == "AvePoint.GCommon.Contract.AccountManager.IMAccountManagerService" && context.CoreMessage.InvocationContext.MethodName == "GetAllGroupsWhitItsOwner")
            {
                return;
            }

            if (context.CoreMessage.InvocationContext.TypeKey == "AvePoint.GCommon.Contract.Server.Login.IMLoginService" && context.CoreMessage.InvocationContext.MethodName == "RegisterForSimple")
            {
                return;
            }

            if (context.CoreMessage.InvocationContext.TypeKey == "AvePoint.ControlPanel.Web.Contract.IOffice365WebService" && context.CoreMessage.InvocationContext.MethodName == "CreateRemoteWebApplication")
            {
                return;
            }

            if (context.CoreMessage.InvocationContext.TypeKey == "AvePoint.GCommon.Contract.Server.Login.IMLoginService" && context.CoreMessage.InvocationContext.MethodName == "LoginForSimple")
            {
                return;
            }

            if (context.CoreMessage.AuthorizationKey == null || !context.CoreMessage.AuthorizationKey.Equals(AuthorizationToken))
            {

                throw new Exception("Unauthorized Access Denied");
            } 
        }

        public void PostInvoke(InterseptionContext context)
        {
            
        }
    }
}
