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




namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    #region using directives
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;

    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRSNCommandOperationDto
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string ScriptProfileName { get; set; }
        [DataMember]
        public ScriptOperationType OperationType { get; set; }
        [DataMember]
        public List<PRSNScriptProfileDto> ScriptProfileList { get; set; }
        [DataMember]
        public OldErrorCode ErrorCodeForCommand { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRSNScriptProfileDto
    {
        [DataMember]
        public string AgentId { get; set; }
        [DataMember]
        public string AgentName { get; set; }
        
        
        #region PRE operation command
        [DataMember]
        public bool IsRunPreCommand { get; set; }
        [DataMember]
        public bool IsFatalPreError { get; set; }
        [DataMember]
        public string PreCommandHost { get; set; }
        [DataMember]
        public string PreScriptLocation { get; set; }
        [DataMember]
        public string ParametersForPreScriptStr { get; set; }
        #endregion

        #region POST operation command
        [DataMember]
        public bool IsRunPostCommand { get; set; }
        [DataMember]
        public bool IsFatalPostError { get; set; }
        [DataMember]
        public string PostCommandHost { get; set; }
        [DataMember]
        public string PostScriptLocation { get; set; }
        [DataMember]
        public string ParametersForPostScriptStr { get; set; }
        #endregion
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ScriptOperationType
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        Backup = 210,

        [EnumMember]
        Restore = 211,

        [EnumMember]
        Verify = 212,
    }
}
