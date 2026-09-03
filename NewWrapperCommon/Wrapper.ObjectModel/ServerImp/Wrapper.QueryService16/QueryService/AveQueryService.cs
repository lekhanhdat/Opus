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

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "type", Target = "AvePoint.Wrapper.QueryService.AveQueryService", MessageId = "SQL Command.")]
namespace AvePoint.Wrapper.QueryService
{
    using System;
    using AvePoint.Wrapper.Common;
    using System.Runtime.CompilerServices;


    public class AveQueryServiceProvider
    {
        public static T Instance<T>(object arg) where T : IAveQueryService
        {
            return (T)CreateQueryService(arg);
        }
        internal static IAveQueryService CreateQueryService(object arg)
        {
            var queryService = new AveQueryService();
            queryService.InitQuerySession(arg);
            return queryService;
        }
    }

    internal partial class AveQueryService : AveQueryServiceBase
    {
        private const string PerformanceScopePerfix = "AvePoint.Wrapper.QueryService";
        internal AveQueryService()
        {
            mQueryWorker = new AveQueryWorker();
        }

        protected void ExceptionHandlingScope(Action run,
            [CallerMemberName] string memberName = "",//.Net 4.5新特性, 在编译时获取调用方法名, 避免在运行时反射
            [CallerLineNumber] int lineNumber = 0)
        {
            var scopeName = $"{PerformanceScopePerfix}.{memberName}.{lineNumber}";
                //string.Format("{0}.{1}.{2}", PerformanceScopePerfix, memberName, lineNumber);
            base.ExceptionHandlingScope(scopeName, run);
        }
    }
}
