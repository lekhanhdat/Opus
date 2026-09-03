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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAFarmManageFarmSolutionsOperation : CAOperation
    {
        [DataMember]
        public List<FarmSolution> FarmSolutions { get; set; }
        [DataMember]
        public OperationType Operation { get; set; }

        //For Deploy and Retract
        [DataMember]
        public bool ExecuteNow { get; set; }
        [DataMember]
        public DateTime ExecuteOtherTime { get; set; }
        [DataMember]
        public String DestWebApp { get; set; }

        [DataContract(Namespace = ContractConstants.Namespace)]
        public enum OperationType
        { 
            [EnumMember]
            Deploy,
            [EnumMember]
            Retract,
            [EnumMember]
            Remove,
            [EnumMember]
            Get,
            [EnumMember]
            CancelDeploy,
            [EnumMember]
            CancelRetract          
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FarmSolution
    {
        [DataMember]
        public String SolutionId { get; set; }
        [DataMember]
        public String Name { get; set; }
        [DataMember]
        public bool Deployed { get; set; }
        [DataMember]
        public String SolutionType { get; set; }
        [DataMember]
        public bool ContainsWebApplicationResource { get; set; }
        [DataMember]
        public bool ContainsGlobalAssembly { get; set; }
        [DataMember]
        public bool ContainsCasPolicy { get; set; }
        [DataMember]
        public SharePointServerRole DeploymentServerType { get; set; }
        [DataMember]
        public SharePointSolutionDeploymentState DeploymentState { get; set; }
        [DataMember]
        public string Status { get; set; }
        [DataMember]
        public List<String> DeployedTo { get; set; }
        [DataMember]
        public SharePointSolutionOperationResult LastOperationResult { get; set; }
        [DataMember]
        public String LastOperationDetail { get; set; }
        [DataMember]
        public DateTime LastOperationEndTime { get; set; }

        [DataMember]
        public string Lcid { get; set; }

        //For Deploy Solution
        [DataMember]
        public List<String> CanDeployTo { get; set; }
        //For Retract Solution
        [DataMember]
        public List<String> CanRetractTo { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public bool ContainsApp { get; set; }

        [DataMember]
        public List<string> Apps { get; set; }

        [DataMember]
        public bool ShowSelectBinOrGlobal { get; set; }

        [DataMember]
        public string DeployedTime { get; set; }

        [DataMember]
        public string UserSlectAppList { get; set; }

        [DataMember]
        public int UserSelectBin { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SharePointServerRole
    {
        [EnumMember]
        Invalid,
        [EnumMember]
        WebFrontEnd,
        [EnumMember]
        Application,
        [EnumMember]
        SingleServer
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SharePointSolutionDeploymentState
    {
        [EnumMember]
        NotDeployed,
        [EnumMember]
        GlobalDeployed,
        [EnumMember]
        WebApplicationDeployed,
        [EnumMember]
        GlobalAndWebApplicationDeployed
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SharePointSolutionOperationResult
    {
        [EnumMember]
        NoOperationPerformed,
        [EnumMember]
        RetractionSucceeded,
        [EnumMember]
        DeploymentSucceeded,
        [EnumMember]
        RetractionWarningsOccurred,
        [EnumMember]
        DeploymentWarningsOccurred,
        [EnumMember]
        DeploymentFailedCabExtraction,
        [EnumMember]
        DeploymentSolutionValidationFailed,
        [EnumMember]
        DeploymentFailedFileCopy,
        [EnumMember]
        DeploymentFailedFeatureInstall,
        [EnumMember]
        RetractionFailedCouldNotRemoveFile,
        [EnumMember]
        RetractionFailedCouldNotRemoveFeature,
        [EnumMember]
        DeploymentFailedCallout

    }
}
