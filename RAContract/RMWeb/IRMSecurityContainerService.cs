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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using System.Collections.Generic;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface IRMSecurityContainerService
    {
        /// <summary>
        /// load root containers
        /// </summary>
        /// <param name="sourceFlag"></param>
        /// <returns></returns>
        IList<NameAndIdDto> GetRootContainers(SourceFlag sourceFlag);

        /// <summary>
        /// get sub containers, e.g., sitecollections or mailboxes, by parent container id
        /// </summary>
        /// <param name="rootContainerId"></param>
        /// <returns></returns>
        IList<NameAndIdDto> GetSubContainers(string rootContainerId);

        /// <summary>
        /// update or create a new container
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        int UpSert(RMSecurityContainerDto dto);

        /// <summary>
        /// add to job queue to be invoked by timer schedule
        /// </summary>
        /// <param name="jobRunBy"></param>
        /// <param name="syncNodeJobId"></param>
        /// <returns></returns>
        string RunScheduleJob(JobRunBy jobRunBy, string syncNodeJobId = "");

        /// <summary>
        /// will send job message to queue to be invoked by job
        /// </summary>
        /// <param name="jobRunBy"></param>
        /// <param name="jobRunByUser"></param>
        /// <returns></returns>
        string RealScheduleJob(JobRunBy jobRunBy, string parameter = "", string jobRunByUser = "");
    }
}
