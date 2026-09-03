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
using AvePoint.GCommon.Utility.I18N;

namespace AvePoint.GCommon.Utility.Exceptions.Configuration
{
    [Serializable]
    public class ProfileBeingUsedByNodeException : AveException
    {
        public ProfileBeingUsedByNodeException(string nodeName)
        {
            Contexts.Add(ContextKeys.Configuration.NodeName, nodeName);
        }
    }

    [Serializable]
    public class ProfileBeingUsedByRunningJobException : AveException
    {
        public ProfileBeingUsedByRunningJobException(string jobID)
        {
            Contexts.Add(ContextKeys.Job.JobID, jobID);
        }
    }

    [Serializable]
    public class ExpiredStartTimeException : AveException
    {
        public ExpiredStartTimeException(string currentTime,string startTime)
        {
            Contexts.Add(ContextKeys.Configuration.CurrentTime, currentTime);
            Contexts.Add(ContextKeys.Configuration.StartTime, startTime);
        }
    }

    [Serializable]
    public class DuplicatedProfileNameException : AveException
    {
        public DuplicatedProfileNameException()
        {
        }
    }

    [Serializable]
    public class GetResponseFromServiceFailedException : AveException
    {
        public GetResponseFromServiceFailedException(string serviceAddress, ContextValues.Service.ServiceType serviceType)
        {
            Contexts.Add(ContextKeys.Service.ServiceAddress, serviceAddress);
            Contexts.Add(ContextKeys.Service.ServiceType, ContextValues.GetContextValue(serviceType));
        }
    }

    [Serializable]
    public class ProcessingPoolNotFoundException : AveException
    {
        public ProcessingPoolNotFoundException(string processingPoolName)
        {
            Contexts.Add(ContextKeys.Configuration.ProcessingPoolName, processingPoolName);
        }
    }
    
}
