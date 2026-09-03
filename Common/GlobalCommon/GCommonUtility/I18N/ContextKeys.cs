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
using System.Text;
using AvePoint.I18N;

namespace AvePoint.GCommon.Utility.I18N
{
    public class ContextKeys
    {
        private static string GetContextKey(Enum key)
        {
            string temp = key.GetType().FullName;
            temp = temp.Substring(temp.IndexOf("ContextKeys") + "ContextKeys".Length);
            temp = temp.Replace("+", "_");
            temp = "ContextKey" + temp + "_" + key;
            return EventViewerResources.ResourceManager.GetString(temp);
        }

        public static string GetAllContexts(Dictionary<Enum, string> Contexts)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder errorDetails = new StringBuilder();
            StringBuilder moreInformation = new StringBuilder();
            foreach (Enum key in Contexts.Keys)
            {
                string contextKey = GetContextKey(key);
                string contextValue = Contexts[key];
//                if (string.IsNullOrEmpty(contextKey) || string.IsNullOrEmpty(contextValue)) continue;
                if (key.GetType() == typeof(ContextKeys.Common) && string.Compare(key.ToString(), ContextKeys.Common.Cause.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
                {
                    errorDetails.Append(" " + contextKey + " " + contextValue + "\n");
                }
                else if (key.GetType() == typeof(ContextKeys.Common) && string.Compare(key.ToString(), ContextKeys.Common.MoreInformation.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
                {
                    moreInformation.Append(" " + contextKey + " " + contextValue + "\n");
                }
                else
                {
                    sb.Append(" " + contextKey + " " + contextValue + "\n");
                }
            }
            sb.Append(errorDetails);
            sb.Append(moreInformation);
            return sb.ToString();
        }

        public enum Authentication
        {
            DomainServer,
            MaximumUserSessionCount,
            LoginAddress,
            LoginTime,
            LoginType,

            LogoffTime,
            
            OperatingUserName,
        
            SessionStartTime,

            UserName
        }

        public enum Common
        {
            Cause,
            MoreInformation,
            OperationType
        }

        public enum Communication
        {
            RequestMessage,
            DestinationAddress,
            SourceAddress,
            Timeout
        }

        public enum Configuration
        {
            NodeName,
            PlanID,
            PlanName,
            PlanType,


            ProfileName,
            ProfileType,

            SettingType,

            ScopeName,
            CurrentTime,
            StartTime,
            ProcessingPoolName
        }

        public enum Database
        {
            DatabaseInstance,
            DatabaseName,
            DatabaseType
        }

        public enum Driver
        {
            DriverName
        }

        public enum File
        {
            FileName,
            FilePath,
            FileSize,
            FileType
        }

        public enum Job
        {
            JobID,
            RunningJobID,
            ScanJobID,

            OriginalJobType,
            FinalJobType,

            JobReport,
            JobLimit,
            StoragePolicy,
            LogicalDevice,
            PhysicalDevice,
            AgentGroup,
            LastUpdateTime,
            MediaService,
            ProtectionGUID,
            SecurityProfileGUID
        }

        public enum License
        {
            HostAddress,
            LicenseAddress
        }

        public enum Process
        {
            CommandName,

            ProcessName,
            Parameter,
            ErrorMessage
        }

        public enum Packaging
        {
            PackageType
        }

        public enum Service
        {
            ManagerAddress,
            ManagerPort,
            ServiceAddress,
            ServicePort,
            ServiceType
        }

        public enum SharePoint
        {
            FarmName,

            ContentTypeName,
            ContentDatabaseName,
            ColumnTitle,
            DatabaseName,
            DatabaseType,
            DependencyTitle,
            DependencyType,
            CreateTime,
            BlobName,
            IndexName,

            DocumentName,
            DestinationPath,
            FeatureID,
            FeatureScope,
            ListID,
            ObjectName,
            ObjectType,
            SourcePath,
            ScopeURL,
            ItemName,
            ItemURL,
            ItemID,
            FolderURL,
            ListTitle,

            PropertyName,

            SiteCollectionURL,
            SiteURL,
            
            SolutionName,

            StsadmCommand,
            StsadmMessage,

            TermGroupName,
            TermSetName,

            UserID,
            ExpectedLoginName,
            ConflictedLoginName,

            
            WorkflowDefinationName,
            WebApplicationURL,
            WebPartDisplayName,
            WebPartType
        }

        public enum Snapshot
        {
            VolumeName,
            AgentName,
            CurrentSnapshotCount,
            MaxSnapshotCount
        }

        public enum Socket
        {
            Address,
            IP,
            Port
        }

        public enum Storage
        {
            Path,
            StorageType,

            CurrentFreeSpace,
            RequiredFreeSpace
        }

        public enum Update
        {
            HostfixName
        }
    }
}
