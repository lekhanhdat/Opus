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
using System.Collections.Generic;
using AvePoint.GCommon.Utility.I18N;

namespace AvePoint.GCommon.Utility.Exceptions.Job
{
    [Serializable]
    public class CompletedWithExceptionException : AveException
    {
        public CompletedWithExceptionException()
        {}
    }

    [Serializable]
    public class ConnectToMediaServiceFailedException : AveException
    {
        public ConnectToMediaServiceFailedException(string serviceAddress, int servicePort)
        {
            Contexts.Add(ContextKeys.Service.ServiceAddress, serviceAddress);
            Contexts.Add(ContextKeys.Service.ServicePort, servicePort.ToString());
        }
    }

    [Serializable]
    public class ContentDatabaseOfflineException : AveException
    {
        public ContentDatabaseOfflineException(List<string> contentDatabaseNames, string webApplicationURL)
        {
            Contexts.Add(ContextKeys.SharePoint.ContentDatabaseName, Util.ParseList(contentDatabaseNames));
            Contexts.Add(ContextKeys.SharePoint.WebApplicationURL, webApplicationURL);
        }
    }

    [Serializable]
    public class NoArchiverDatabaseException : AveException
    {
        public NoArchiverDatabaseException()
        {
        }
    }

    [Serializable]
    public class ConnectToAgentServiceFailedException : AveException
    {
        public ConnectToAgentServiceFailedException(string serviceAddress, int servicePort)
        {
            Contexts.Add(ContextKeys.Service.ServiceAddress, serviceAddress);
            Contexts.Add(ContextKeys.Service.ServicePort, servicePort.ToString());
        }
    }

    [Serializable]
    public class ExistedRunningJobException : AveException
    {
        public ExistedRunningJobException(string planID)
        {
            Contexts.Add(ContextKeys.Configuration.PlanID, planID);
        }
    }

    [Serializable]
    public class ExistedRunningJobInScopeException : AveException
    {
        public ExistedRunningJobInScopeException(string scopeName)
        {
            Contexts.Add(ContextKeys.Configuration.ScopeName, scopeName);
        }
    }

    [Serializable]
    public class ExistedPausedJobException : AveException
    {
        public ExistedPausedJobException(string planID)
        {
            Contexts.Add(ContextKeys.Configuration.PlanID, planID);
        }
    }

    [Serializable]
    public class FailedException : AveException
    {
        public FailedException()
        {}
    }

    [Serializable]
    public class GetResponseTimeoutException : AveException
    {
        public GetResponseTimeoutException(string serviceAddress,ContextValues.Service.ServiceType serviceType,int timeout)
        {
            Contexts.Add(ContextKeys.Service.ServiceAddress, serviceAddress);
            Contexts.Add(ContextKeys.Service.ServiceType, ContextValues.GetContextValue(serviceType));
            Contexts.Add(ContextKeys.Communication.Timeout, Util.MillisecondToSecond(timeout));
        }
    }

    [Serializable]
    public class JobLimitationException : AveException
    {
        public JobLimitationException(string limit)
        {
            Contexts.Add(ContextKeys.Job.JobLimit, limit);
        }
    }

    [Serializable]
    public class LoadSecurityProfileFailedException​ : AveException
    {
        public LoadSecurityProfileFailedException​(string protectionGUID, string securityProfileGUID)
        {
            Contexts.Add(ContextKeys.Job.ProtectionGUID, protectionGUID);
            Contexts.Add(ContextKeys.Job.SecurityProfileGUID, securityProfileGUID);
        }
    }

    [Serializable]
    public class NoAvailableMediaException : AveException
    {
        public NoAvailableMediaException(string storagePolicy)
        {
            Contexts.Add(ContextKeys.Job.StoragePolicy, storagePolicy);
        }

        public NoAvailableMediaException()
        {
        }
    }
    
    [Serializable]
    public class NoAvailableMappingException : AveException
    {
        public NoAvailableMappingException(string planName)
        {
            Contexts.Add(ContextKeys.Configuration.PlanName, planName);
        }
    }

    [Serializable]
    public class NoAvailableAgentException : AveException
    {
        public NoAvailableAgentException(string agentGroup)
        {
            Contexts.Add(ContextKeys.Job.AgentGroup, agentGroup);
        }
    }

    [Serializable]
    public class NoAvailableAgentInFarmException : AveException
    {
        public NoAvailableAgentInFarmException(string farmName)
        {
            Contexts.Add(ContextKeys.SharePoint.FarmName, farmName);
        }
    }

    [Serializable]
    public class NoAvailableAgentInNoneSPEnvironmentException  : AveException
    {
        public NoAvailableAgentInNoneSPEnvironmentException()
        {
        }
    }

    [Serializable]
    public class NoAvailableStoragePolicyException : AveException
    {
        public NoAvailableStoragePolicyException(string storagePolicy)
        {
            Contexts.Add(ContextKeys.Job.StoragePolicy, storagePolicy);
        }
    }

    [Serializable]
    public class NoAvailableDeviceException : AveException
    {
        public NoAvailableDeviceException(string logicalDevice)
        {
            Contexts.Add(ContextKeys.Job.LogicalDevice, logicalDevice);
        }
    }

    [Serializable]
    public class NoAvailableSiteCollectionException : AveException
    {
        public NoAvailableSiteCollectionException(string webApplicationURL)
        {
            Contexts.Add(ContextKeys.SharePoint.WebApplicationURL, webApplicationURL);
        }
    }

    [Serializable]
    public class NoUpdateException : AveException
    {
        public NoUpdateException(string lastUpdateTime)
        {
            Contexts.Add(ContextKeys.Job.LastUpdateTime, lastUpdateTime);
        }
    }


    [Serializable]
    public class NoAvailablePlanException : AveException
    {
        public NoAvailablePlanException(string filePath)
        {
            Contexts.Add(ContextKeys.File.FilePath, filePath);
        }
    }

    [Serializable]
    public class NoEnoughContentDatabasePermissionException : AveException
    {
        public NoEnoughContentDatabasePermissionException(string contentDatabaseName, string webApplicationURL)
        {
            Contexts.Add(ContextKeys.SharePoint.ContentDatabaseName, contentDatabaseName);
            Contexts.Add(ContextKeys.SharePoint.WebApplicationURL, webApplicationURL);
        }
    }

    [Serializable]
    public class NoEnoughFreeSpaceException : AveException
    {
        public NoEnoughFreeSpaceException(string storagePolicy)
        {
            Contexts.Add(ContextKeys.Job.StoragePolicy, storagePolicy);
        }
    }

    [Serializable]
    public class NoEnoughWebApplicationPermissionException : AveException
    {
        public NoEnoughWebApplicationPermissionException(string webApplicationURL)
        {
            Contexts.Add(ContextKeys.SharePoint.WebApplicationURL, webApplicationURL);
        }
    }

    [Serializable]
    public class NoFarmAdministratorPermissionException : AveException
    {
        public NoFarmAdministratorPermissionException()
        {
        }
    }

    [Serializable]
    public class ScanJobFailedException : AveException
    {
        public ScanJobFailedException(string scanJobID)
        {
            Contexts.Add(ContextKeys.Job.ScanJobID, scanJobID);
        }
    }

    [Serializable]
    public class SendRequestFailedException : AveException
    {
        public SendRequestFailedException(string requestMessage)
        {
            Contexts.Add(ContextKeys.Communication.RequestMessage, requestMessage);
        }
    }

    [Serializable]
    public class SomeRemoteSiteCollectionLicenseExpiredException : AveException
    {
        public SomeRemoteSiteCollectionLicenseExpiredException(List<string> siteCollectionURLs)
        {
            Contexts.Add(ContextKeys.SharePoint.SiteCollectionURL, Util.ParseList(siteCollectionURLs));
        }
    }

    [Serializable]
    public class TimerServiceDownException : AveException
    {
        public TimerServiceDownException() { }
    }

    [Serializable]
    public class TSMLicenseExpiredException : AveException
    {
        public TSMLicenseExpiredException()
        {}

        public TSMLicenseExpiredException(string physicalDevice)
        {
            Contexts.Add(ContextKeys.Job.PhysicalDevice, physicalDevice);
        }
    }

}