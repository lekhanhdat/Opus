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
    using System.Configuration;
    using System.Diagnostics;
    #endregion

    #region Attribute
    [DebuggerNonUserCode]
    #endregion
    internal class IocContainerAnalyzerFactory
    {
        public static IIocContainerAnalyzer GetContainerAnalyzer(Object container)
        {
            var result = default(IIocContainerAnalyzer);
            var microKernelSectionHandler = ConfigurationManager.GetSection(MicroKernelConstant.MicroKernelSectionName) as MicroKernelSectionHandler;
            if (microKernelSectionHandler != null)
            {
                var microKernelIocContainerCollection = microKernelSectionHandler.MicroKernelIocContainers;
                if (microKernelIocContainerCollection != null
                    && microKernelIocContainerCollection.Count > 0)
                {
                    var iocContainerElement = microKernelIocContainerCollection[0];
                    var iocContainerType = Type.GetType(iocContainerElement.Type);
                    if (typeof(IIocContainerAnalyzer).IsAssignableFrom(iocContainerType))
                        result = Activator.CreateInstance(Type.GetType(iocContainerElement.Type), container) as IIocContainerAnalyzer;
                }
            }
            else result = new DefaultIocContainerAnalyzer(container);
            return result;
        }
    }
}
