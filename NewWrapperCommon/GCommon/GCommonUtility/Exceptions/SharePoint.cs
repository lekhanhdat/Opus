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
using AvePoint.GCommon.Utility.Exceptions;

namespace AvePoint.GCommon.Utility.Exceptions.SharePoint
{
    [Serializable]
    public class ConfigurationConflictException : AveException
    {
        public ConfigurationConflictException(string objectName, ContextValues.SharePoint.ObjectType objectType)
        {
            Contexts.Add(ContextKeys.SharePoint.ObjectName, objectName);
            Contexts.Add(ContextKeys.SharePoint.ObjectType, ContextValues.GetContextValue(objectType));
        }
    }
    
    [Serializable]
    public class DuplicatedObjectInRecycleBinException : AveException
    {
        public DuplicatedObjectInRecycleBinException(ContextValues.SharePoint.ObjectType objectType)
        {
            Contexts.Add(ContextKeys.SharePoint.ObjectType, ContextValues.GetContextValue(objectType));
        }
    }
    
    [Serializable]
    public class ExistedErrorInMediaServiceException​ : AveException
    {
        public ExistedErrorInMediaServiceException​(string errorMessage, string serviceAddress)
        {
            this.Contexts.Add(ContextKeys.Process.ErrorMessage, errorMessage);
            this.Contexts.Add(ContextKeys.Service.ServiceAddress, serviceAddress);
        }
    }

    [Serializable]
    public class WorkflowDefinitionNotFoundException : AveException
    {
        public WorkflowDefinitionNotFoundException(string parentWorkflowDefinationName, string itemName, string listTitle, string webURL)
        {
            this.Contexts.Add(ContextKeys.SharePoint.WorkflowDefinationName, parentWorkflowDefinationName);
            this.Contexts.Add(ContextKeys.SharePoint.ItemName, itemName);
            this.Contexts.Add(ContextKeys.SharePoint.ListTitle, listTitle);
            this.Contexts.Add(ContextKeys.SharePoint.SiteURL, webURL);
        }
    }

    [Serializable]
    public class FeatureNotFoundException : AveException
    {
        public FeatureNotFoundException(Guid featureID, string featureScope, string scopeURL)
        {
            this.Contexts.Add(ContextKeys.SharePoint.FeatureID, featureID.ToString());
            this.Contexts.Add(ContextKeys.SharePoint.FeatureScope, featureScope);
            this.Contexts.Add(ContextKeys.SharePoint.ScopeURL, scopeURL);
        }
    }

    [Serializable]
    public class ItemTypeConflictException : AveException
    {
        public ItemTypeConflictException(int itemID, string folderURL)
        {
            this.Contexts.Add(ContextKeys.SharePoint.ItemID, itemID.ToString());
            this.Contexts.Add(ContextKeys.SharePoint.FolderURL, folderURL);
        }
    }

    [Serializable]
    public class NoAvailableUserCodeServiceException  : AveException
    {
        public NoAvailableUserCodeServiceException(string createTime, string listTitle)
        {
            this.Contexts.Add(ContextKeys.SharePoint.CreateTime, createTime);
            this.Contexts.Add(ContextKeys.SharePoint.ListTitle, listTitle);
        }
    }

    [Serializable]
    public class NoAvailableWebApplicationServiceException  : AveException
    {
        public NoAvailableWebApplicationServiceException()
        {
        }
    }

    [Serializable]
    public class NotSupportedColumnMappingException : AveException
    {
        public NotSupportedColumnMappingException(string sourceColumnName, string destinationColumnName)
        {
            this.Contexts.Add(ContextKeys.SharePoint.SourceColumnName, sourceColumnName);
            this.Contexts.Add(ContextKeys.SharePoint.DestinationColumnName,destinationColumnName);
        }
    }

    [Serializable]
    public class UserNotFoundException : AveException
    {
        public UserNotFoundException(int userID)
        {
            this.Contexts.Add(ContextKeys.SharePoint.UserID, userID.ToString());
        }
    }

    [Serializable]
    public class UserAlreadyMappedException : AveException
    {
        public UserAlreadyMappedException(string expectedLoginName, string conflictedLoginName)
        {
            this.Contexts.Add(ContextKeys.SharePoint.ExpectedLoginName, expectedLoginName);
            this.Contexts.Add(ContextKeys.SharePoint.ConflictedLoginName, conflictedLoginName);
        }
    }

    [Serializable]
    public class UserProfileInaccessibleException : AveException
    {
        public UserProfileInaccessibleException()
        { }
    }

    [Serializable]
    public class ManagedPathNotFoundException : AveException
    {
        public ManagedPathNotFoundException() { }
    }

    [Serializable]
    public class ExecuteStsadmFailedException : AveException
    {
        public ExecuteStsadmFailedException(string parameter, string errorMessage)
        {
            this.Contexts.Add(ContextKeys.Process.Parameter, parameter);
            this.Contexts.Add(ContextKeys.Process.ErrorMessage, errorMessage);
        }
    }

    [Serializable]
    public class RuleFileNotFoundException : AveException
    {
        public RuleFileNotFoundException(string fileName)
        {
            Contexts.Add(ContextKeys.File.Name, fileName);
        }
    }

    [Serializable]
    public class TermNotFoundException : AveException
    {
        public TermNotFoundException(string termGroupName, string termSetName, string termName)
        {
            Contexts.Add(ContextKeys.SharePoint.TermGroupName, termGroupName);
            Contexts.Add(ContextKeys.SharePoint.TermSetName, termSetName);
            Contexts.Add(ContextKeys.SharePoint.TermName, termName);
        }
        public TermNotFoundException( string termSetName, string termName)
        {
            Contexts.Add(ContextKeys.SharePoint.TermSetName, termSetName);
            Contexts.Add(ContextKeys.SharePoint.TermName, termName);
        }
    }

    [Serializable]
    public class TermSetNotFoundException : AveException
    {
        public TermSetNotFoundException(string termGroupName, string termSetName)
        {
            Contexts.Add(ContextKeys.SharePoint.TermGroupName, termGroupName);
            Contexts.Add(ContextKeys.SharePoint.TermSetName, termSetName);
        }

        public TermSetNotFoundException(string termSetName)
        {
            Contexts.Add(ContextKeys.SharePoint.TermSetName, termSetName);
        }

        public TermSetNotFoundException()
        { }
    }

}