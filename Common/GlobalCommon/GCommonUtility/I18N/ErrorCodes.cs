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
using AvePoint.I18N;

namespace AvePoint.GCommon.Utility.I18N
{
    [Serializable]
    public abstract class AveErrorCodeException : AveException
    {
        private Dictionary<Enum, string> contexts = new Dictionary<Enum, string>();

        internal Dictionary<Enum, string> Contexts { get { return contexts; } }

        internal AveErrorCodeException() : this(null) { }

        internal AveErrorCodeException(Exception e)
            : base(e)
        {
            this.Contexts.Add(ContextKeys.Common.ErrorCode, GetValue("ErrorCode"));
            this.Contexts.Add(ContextKeys.Common.ErrorDescription, GetValue("ErrorDescription"));
        }

        public string GetAllContexts()
        {
            this.Contexts.Add(ContextKeys.Common.MoreInformation, "<http://www.DocAve.com>");
            return ContextKeys.GetAllContexts(Contexts);
        }

        private string GetValue(string prefix)
        {
            string temp = this.GetType().FullName;
            temp = temp.Substring(temp.IndexOf("ErrorCodeExceptions") + "ErrorCodeExceptions".Length);
            temp = temp.Replace("+", "_");
            temp = prefix + temp;
            return EventViewerResources.ResourceManager.GetString(temp);
        }

        public override string Message
        {
            get
            {
                if (this.Contexts.ContainsKey(ContextKeys.Common.ErrorDescription))
                {
                    return Contexts[ContextKeys.Common.ErrorDescription];
                }
                return string.Empty;
            }
        }
    }

    /// <summary>
    /// common code should throw ErrorCodeExpceptions to logic layer, so the logic layer can catch and log error code according to the ErrorCodeExceptions.
    /// </summary>
    public class ErrorCodeExceptions
    {
        public class SharePoint
        {
            [Serializable]
            public class SharePointErrorException : AveErrorCodeException
            {

            }

            #region DocumentException

            [Serializable]
            public class DocumentTypeBlockedException : SharePointErrorException
            {
                public DocumentTypeBlockedException(string fileName, string fileExtension, string listTitle, string webUrl)
                {
                    base.Contexts.Add(ContextKeys.File.FileName, fileName);
                    base.Contexts.Add(ContextKeys.File.FileType, fileExtension);
                    base.Contexts.Add(ContextKeys.SharePoint.ListTitle, listTitle);
                    base.Contexts.Add(ContextKeys.SharePoint.ParentWebUrl, webUrl);
                }
            }

            [Serializable]
            public class DocumentSizeExceedException : SharePointErrorException
            {
                public DocumentSizeExceedException(string fileName, string size, string listTitle, string webUrl, string maxSize, string solutionUrl)
                {
                    base.Contexts.Add(ContextKeys.File.FileName, fileName);
                    base.Contexts.Add(ContextKeys.File.FileSize, size);
                    base.Contexts.Add(ContextKeys.SharePoint.ListTitle, listTitle);
                    base.Contexts.Add(ContextKeys.SharePoint.MaxSize, maxSize);
                    base.Contexts.Add(ContextKeys.SharePoint.ParentWebUrl, webUrl);
                    base.Contexts.Add(ContextKeys.SharePoint.SolutionUrl, solutionUrl);
                }
            }

            #endregion

            #region SiteException

            [Serializable]
            public class WebAppUsageNotEnoughException : SharePointErrorException
            {
                public WebAppUsageNotEnoughException(string siteUrl, long maxSize, string solutionUrl)
                {
                    base.Contexts.Add(ContextKeys.SharePoint.MaxSize, maxSize.ToString());
                    base.Contexts.Add(ContextKeys.SharePoint.SiteUrl, siteUrl);
                    base.Contexts.Add(ContextKeys.SharePoint.SolutionUrl, solutionUrl);
                }
            }

            [Serializable]
            public class SiteUsageNotEnoughException : SharePointErrorException
            {
                public SiteUsageNotEnoughException(string siteUrl, long maxSize, string solutionUrl)
                {
                    base.Contexts.Add(ContextKeys.SharePoint.MaxSize, maxSize.ToString());
                    base.Contexts.Add(ContextKeys.SharePoint.SiteUrl, siteUrl);
                    base.Contexts.Add(ContextKeys.SharePoint.SolutionUrl, solutionUrl);
                }
            }

            [Serializable]
            public class SiteLockedException : SharePointErrorException
            {
                public SiteLockedException(string url, string solutionUrl)
                {
                    base.Contexts.Add(ContextKeys.SharePoint.SiteUrl, url);
                    base.Contexts.Add(ContextKeys.SharePoint.SolutionUrl, solutionUrl);
                }
            }

            [Serializable]
            public class SiteAlreadyExistException : SharePointErrorException
            {
                public SiteAlreadyExistException(string url)
                {
                    base.Contexts.Add(ContextKeys.SharePoint.SolutionUrl, url);
                }

            }

            #endregion

            #region FeatureException

            [Serializable]
            public class FeatureNotFoundException : SharePointErrorException
            {
                public FeatureNotFoundException(Guid featureId, string featureScope, string scopeUrl)
                {
                    base.Contexts.Add(ContextKeys.SharePoint.FeatureId, featureId.ToString());
                    base.Contexts.Add(ContextKeys.SharePoint.FeatureScope, featureScope);
                    base.Contexts.Add(ContextKeys.SharePoint.FeatureScopeUrl, scopeUrl);
                }
            }

            #endregion

            #region UserException

            [Serializable]
            public class UserProfileInaccessibleException : SharePointErrorException
            {
                public UserProfileInaccessibleException()
                { }
            }

            [Serializable]
            public class UserNotFoundExcpetion : SharePointErrorException
            {
                public UserNotFoundExcpetion(string loginName, string webUrl)
                {
                    base.Contexts.Add(ContextKeys.Authentication.UserName, loginName);
                    base.Contexts.Add(ContextKeys.SharePoint.WebUrl, webUrl);
                }
            }

            [Serializable]
            public class UserProfileServiceNotFoundException : SharePointErrorException
            {
                public UserProfileServiceNotFoundException()
                { }
            }

            #endregion

            #region TemplateExcepiton

            [Serializable]
            public class ListTemplateNotFoundException : SharePointErrorException
            {
                public ListTemplateNotFoundException(string strTitle, string webUrl, string templateType)
                {
                    base.Contexts.Add(ContextKeys.SharePoint.ListTemplate, templateType);
                    base.Contexts.Add(ContextKeys.SharePoint.ListTitle, strTitle);
                    base.Contexts.Add(ContextKeys.SharePoint.WebUrl, webUrl);
                }

            }

            [Serializable]
            public class WebTemplateNotFoundException : SharePointErrorException
            {
                public WebTemplateNotFoundException(string strWebUrl, uint nLCID, string webTemplate)
                {
                    base.Contexts.Add(ContextKeys.SharePoint.LanguageId, nLCID.ToString());
                    base.Contexts.Add(ContextKeys.SharePoint.WebTemplate, webTemplate);
                    base.Contexts.Add(ContextKeys.SharePoint.WebUrl, strWebUrl);
                }

            }

            #endregion

            #region DataBaseException

            [Serializable]
            public class DataBaseExcepiton : SharePointErrorException
            {
                public DataBaseExcepiton(string contentDBName, string webApplicationName, string solutionUrl)
                {
                    base.Contexts.Add(ContextKeys.SharePoint.ContentDBName, contentDBName);
                    base.Contexts.Add(ContextKeys.SharePoint.SolutionUrl, solutionUrl);
                    base.Contexts.Add(ContextKeys.SharePoint.WebAppName, webApplicationName);
                }
            }

            [Serializable]
            public class ContentDatabaseSitesFullException : DataBaseExcepiton
            {
                public ContentDatabaseSitesFullException(string contentDBName, string webApplicationName, string solutionUrl)
                    : base(contentDBName, webApplicationName, solutionUrl)
                {
                }

                public ContentDatabaseSitesFullException(string webApplicationName, string solutionUrl)
                    : this(string.Empty, webApplicationName, solutionUrl)
                { }
            }

            [Serializable]
            public class ContentDatabaseOfflineException : DataBaseExcepiton
            {
                public ContentDatabaseOfflineException(string contentDBName, string webApplicationName, string solutionUrl)
                    : base(contentDBName, webApplicationName, solutionUrl)
                { }

                public ContentDatabaseOfflineException(string webApplicationName, string solutionUrl)
                    : this(string.Empty, webApplicationName, solutionUrl)
                { }
            }

            #endregion

            [Serializable]
            public class ManagedPathNotFoundException : SharePointErrorException
            {
                public ManagedPathNotFoundException(string mSiteUrl)
                {
                    base.Contexts.Add(ContextKeys.SharePoint.SiteUrl, mSiteUrl);
                }
            }

            [Serializable]
            public class LanguagePackageNotFoundException : SharePointErrorException
            {

                public LanguagePackageNotFoundException(uint nLCID, string siteUrl)
                {
                    base.Contexts.Add(ContextKeys.SharePoint.LanguageId, nLCID.ToString());
                    base.Contexts.Add(ContextKeys.SharePoint.SiteUrl, siteUrl);
                }
            }


            [Serializable]
            public class ContentTypeAlreadyExistException : SharePointErrorException
            {
                public ContentTypeAlreadyExistException(string contentName, string id, string displayUrl, string listTitle, string webUrl)
                {
                    base.Contexts.Add(ContextKeys.SharePoint.ContentTypeId, id);
                    base.Contexts.Add(ContextKeys.SharePoint.ContentTypeName, contentName);
                    base.Contexts.Add(ContextKeys.SharePoint.ContentTypeUrl, displayUrl);
                    base.Contexts.Add(ContextKeys.SharePoint.ListTitle, listTitle);
                    base.Contexts.Add(ContextKeys.SharePoint.WebUrl, webUrl);
                }

            }

            [Serializable]
            public class WorkflowDefinitionNotFoundException : SharePointErrorException
            {
                public WorkflowDefinitionNotFoundException(string parentWorkflowDefinationName, string itemName, string listTitle, string webUrl)
                {
                    base.Contexts.Add(ContextKeys.SharePoint.ListTitle, listTitle);
                    base.Contexts.Add(ContextKeys.SharePoint.ParentWorkflowDefinationName, parentWorkflowDefinationName);
                    base.Contexts.Add(ContextKeys.SharePoint.WebUrl, webUrl);
                }
            }


        }

        public class Service
        {
            [Serializable]
            public class ServicePortAlreadyInUsedException : AveErrorCodeException
            {
                public ServicePortAlreadyInUsedException(int port)
                {
                    base.Contexts.Add(ContextKeys.Socket.Port, port.ToString());
                }
            }

            [Serializable]
            public class PortSharingNotStartedException : AveErrorCodeException
            {
                public PortSharingNotStartedException(int port)
                {
                    base.Contexts.Add(ContextKeys.Socket.Port, port.ToString());
                }
            }

            [Serializable]
            public class ControlServiceNotAvailableException : AveErrorCodeException
            {
                public ControlServiceNotAvailableException(string controlAddress, int controlPort)
                {
                    this.Contexts.Add(ContextKeys.Socket.IP, controlAddress);
                    this.Contexts.Add(ContextKeys.Socket.Port, controlPort.ToString());
                }
            }

            [Serializable]
            public class WebServiceNotAvailableException : AveErrorCodeException
            {
                public WebServiceNotAvailableException(string controlAddress, int controlPort)
                {
                    this.Contexts.Add(ContextKeys.Socket.IP, controlAddress);
                    this.Contexts.Add(ContextKeys.Socket.Port, controlPort.ToString());
                }
            }

        }

        public class Storage
        {
            [Serializable]
            public class UsernameOrPasswordIncorrectException : AveErrorCodeException
            {
                public UsernameOrPasswordIncorrectException()
                { }

                public UsernameOrPasswordIncorrectException(string userName)
                    : this()
                {
                    base.Contexts.Add(ContextKeys.Authentication.UserName, userName);
                }
            }

            [Serializable]
            public class AccessDeniedException : AveErrorCodeException
            {
                public AccessDeniedException()
                { }

                public AccessDeniedException(string targetPath)
                    : this()
                {
                    base.Contexts.Add(ContextKeys.Storage.TargetPath, targetPath);
                }

                public AccessDeniedException(string targetPath, string userName)
                    : this()
                {
                    base.Contexts.Add(ContextKeys.Storage.TargetPath, targetPath);
                    base.Contexts.Add(ContextKeys.Authentication.UserName, userName);
                }
            }

            [Serializable]
            public class NetworkBrokenException : AveErrorCodeException
            {
                public NetworkBrokenException()
                { }

                public NetworkBrokenException(string targetPath)
                    : this()
                {
                    base.Contexts.Add(ContextKeys.Storage.TargetPath, targetPath);
                }

                public NetworkBrokenException(string targetPath, string fileName)
                    : this()
                {
                    base.Contexts.Add(ContextKeys.File.FileName, fileName);
                    base.Contexts.Add(ContextKeys.Storage.TargetPath, targetPath);
                }
            }

            [Serializable]
            public class NotEnoughSpaceException : AveErrorCodeException
            {
                public NotEnoughSpaceException()
                { }

                public NotEnoughSpaceException(string targetPath)
                    : this()
                {
                    base.Contexts.Add(ContextKeys.Storage.TargetPath, targetPath);
                }
            }

        }

      

        public class Job
        {
            [Serializable]
            public class FailedException : AveErrorCodeException
            {
                public FailedException(string jobId)
                {
                    base.Contexts.Add(ContextKeys.Job.JobId, jobId);
                }
            }
            [Serializable]
            public class CompletedWithException : AveErrorCodeException
            {
                public CompletedWithException(string jobId)
                {
                    base.Contexts.Add(ContextKeys.Job.JobId, jobId);
                }
            }
        }

        public class Socket
        {
            [Serializable]
            public class ConnectToMediaException : AveErrorCodeException
            {
                public ConnectToMediaException()
                { }
            }
        }

        public class Common
        {
            [Serializable]
            public class UnexpectedException : AveErrorCodeException
            {
                public UnexpectedException()
                {
                }

                public UnexpectedException(Exception e)
                    : base(e)
                {
                    this.Contexts.Add(ContextKeys.Common.UnexpectedException, e.ToString());
                }
            }
        }
    }
}
