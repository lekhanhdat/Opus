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
    using System;
    using System.Reflection;
    using System.Threading;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.Media.TCPRequest;
    using AvePoint.GCommon.Network;
    using Merged18NResources.MediaServiceApplicationModel;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.Media.Service.SupportabilityModel;
    #endregion

    /// <summary>
    /// Provide the main logic of the socket channel restore service
    /// </summary>
    /// <typeparam name="TRestoreJob">the specific restore job</typeparam>
    /// <typeparam name="TRequest">The Media Tcp request which sent by agent</typeparam>
    public abstract class RestoreServiceBase<TRestoreJob, TRequest>
        : RequestHandlerBase
        , IRestoreRequestHandler
        where TRestoreJob : RestoreJobBase
        where TRequest : MediaTCPRequest
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        String errorMessage = String.Empty;

        public TRestoreJob RestoreJob { get; private set; }

        public override void HandleRequest(MediaTCPRequest request, IAveNetwork network)
        {
            base.HandleRequest(request, network);
            try
            {
                this.RestoreJob = Activator.CreateInstance(typeof(TRestoreJob), request) as TRestoreJob;
                Thread.CurrentThread.Name = this.RestoreJob.JobId;
                this.Open();
                this.Restore();
            }
            catch (Exception e)
            {
                errorMessage = CatchHelper.ProcessException(e);
                this.logger.Error(MediaServiceApplicationModelResource.RestoreServiceBaseHandleRequestRestoreError, e.ToString());
                throw;
            }
            finally
            {
                this.Close(errorMessage);
            }
        }

        public abstract void Open();

        public abstract void Restore();

        public abstract void Close(String errorMessage);
    }
}