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




namespace AvePoint.Media.Service
{
    #region using directives
    using System.Reflection;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Network;
    using AvePoint.GCommon.Utility;
    using Merged18NResources.MediaServiceApplicationModel;
    #endregion

    /// <summary>
    /// Data service is used to listen a tcp socket to accept or send data to agent
    /// </summary>
    public class DataService
        : Startable
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public IAveNetworkServer NetworkServer { get; set; }

        #region IStartable Member

        /// <summary>
        /// start the Media data service
        /// </summary>
        public override void InternalStart()
        {
            this.logger.Info(MediaServiceApplicationModelResource.DataServiceStartStarting, this.NetworkServer.ListeningPort);
            this.NetworkServer.Start();
            this.logger.Info(MediaServiceApplicationModelResource.DataServiceStartSucceed);
        }

        /// <summary>
        /// stop the Media data service
        /// </summary>
        public override void InternalStop()
        {
            logger.Info(MediaServiceApplicationModelResource.DataServiceStopBegin, this.NetworkServer.ListeningPort);
            this.NetworkServer.Stop();
            logger.Info(MediaServiceApplicationModelResource.DataServiceStopSucceed);
        }
        #endregion
    }
}