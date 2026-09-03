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



namespace AvePoint.GCommon.Contract.Service
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    #endregion

    /// <summary>
    /// The interface will be used as the Media and report service restart control
    /// </summary>
    public interface IServiceController
    {
        /// <summary>
        /// restart the service
        /// </summary>
        /// <param name="service">service dto object, you should parse a service object here</param>
        /// <returns>the result is meaningless, you should check the IsServiceRunning method for the result</returns>
        ServiceControllerResult RestartService(ServiceDto service);
        
        /// <summary>
        /// Reconfigure the service
        /// </summary>
        /// <param name="service">service dto object, you should parse a service object here</param>
        /// <returns>the result is meaningless, you should check the IsServiceRunning method for the result</returns>
        ServiceControllerResult ReConfigureService(ServiceDto service);

        /// <summary>
        /// Stop the service
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        ServiceControllerResult StopService(ServiceDto service);

        /// <summary>
        /// Check the service is running or not
        /// </summary>
        /// <param name="service">the service dto</param>
        /// <returns>if the service is running return true , else, throw a exception</returns>
        ServiceControllerResult IsServiceRunning(ServiceDto service);
    }
}
