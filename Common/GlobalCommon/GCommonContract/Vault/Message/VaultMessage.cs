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



namespace AvePoint.GCommon.Contract.Vault.Message
{
    #region ==== using ====
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.StorageOptimization.Object;
    using AvePoint.GCommon.Contract.Vault.Object;
    #endregion
    /// <summary>
    /// Communications contract with the terminal(SharePointAgent)
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class VaultMessage
    {
        /// <summary>
        /// Used to identify the Vault Operator
        /// </summary>
        [DataMember]
        public VaultAction Action { set; get; } 

        /// <summary>
        /// Vault job information
        /// </summary>
        [DataMember]
        public JobInfo JobInfo { set; get; }

        /// <summary>
        /// It includes the scope node and the node to breaking the the inheritance 
        /// </summary>
        [DataMember]
        public List<RuleNodeContract> Nodes { get; set; }

        /// <summary>
        /// The export file format of the export job 
        /// </summary>
        [DataMember]
        public ExportType ExportType { set; get; }

        [DataMember]
        public string ProcessingPoolID { set; get; }

        /// <summary>
        /// The physical path of the export file
        /// </summary>
        [DataMember]
        public string MediaStorageXri { set; get; }

        [DataMember]
        public PhysicalDeviceDto PhysicalDeviceDto { set; get; }

        [DataMember]
        public ServiceDto AgentInfo { get; set; }

        /// <summary>
        /// Identifies the message success
        /// </summary>
        [DataMember]
        public MessageType MessageType { set; get; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class JobInfo
    {
        [DataMember]
        public string JobID { set; get; }

        [DataMember]
        public long JobStartTime { set; get; }

        [DataMember]
        public int JobType { set; get; }

        [DataMember]
        public int Category { get; set; }

        [DataMember]
        public string PlanID { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum MessageType  
    {
        [EnumMember]
        Successful,
        [EnumMember]
        Failed
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum VaultAction  
    {
        [EnumMember]
        Run_Export_Job,
        [EnumMember]
        Stop_Export_Job
    }
}
