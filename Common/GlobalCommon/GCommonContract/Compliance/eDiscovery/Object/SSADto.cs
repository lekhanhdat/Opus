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
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SSADto : ProfileDto
    {
        [DataMember]
        public const string ID_PREFIX = "SSA_";
       // [DataMember]
       // public string SSAId { set; get; }

        [DataMember]
        public string SSAName { set; get; }

        [DataMember]
        public SSAState SSAState { set; get; }

        [DataMember]
        public bool IsAvailable { set; get; }

        [DataMember]
        public bool IsDeleted { get; set; }

        [DataMember]
        public bool AgentIsAvailable { get; set; }
        
        [DataMember]
        public List<ContentSourceDto> ContentSourceList { get; set; }

        public override bool Equals(object obj)
        {
            SSADto another = obj as SSADto;
            if (another == null)
            {
                return false;
            }

            if (this.Id.Equals(another.Id)
                && this.SSAName.Equals(another.SSAName)
                && this.SSAState == another.SSAState)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static void AddIdPrefix(SSADto ssa)
        {
            if (ssa == null) {
                return;
            }
            if (!String.IsNullOrEmpty(ssa.Id) && !ssa.Id.StartsWith(ID_PREFIX, StringComparison.Ordinal))
            {
                ssa.Id = ID_PREFIX + ssa.Id;
            }
        }

        public static void AddIdPrefix(List<SSADto> ssaList)
        {
            if (ssaList == null || ssaList.Count == 0)
            {
                return;
            }
            foreach(SSADto ssa in ssaList)
            {
                if (!String.IsNullOrEmpty(ssa.Id) && !ssa.Id.StartsWith(ID_PREFIX, StringComparison.Ordinal))
                {
                    ssa.Id = ID_PREFIX + ssa.Id;
                }
            }
        }

        public static void RemoveIdPrefix(SSADto ssa)
        {
            if (ssa == null)
            {
                return;
            }
            if (!String.IsNullOrEmpty(ssa.Id) && ssa.Id.StartsWith(ID_PREFIX, StringComparison.Ordinal))
            {
                ssa.Id = ssa.Id.Remove(0, ID_PREFIX.Length);
            }
        }

        public static void RemoveIdPrefix(List<SSADto> ssaList)
        {
            if (ssaList == null || ssaList.Count == 0)
            {
                return;
            }
            foreach (SSADto ssa in ssaList)
            {
                if (!String.IsNullOrEmpty(ssa.Id) && ssa.Id.StartsWith(ID_PREFIX, StringComparison.Ordinal))
                {
                    ssa.Id = ssa.Id.Remove(0, ID_PREFIX.Length);
                }
            }
        }
             
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SSAState
    {
        [EnumMember]
        Installed = 1,
        [EnumMember]
        Uninstalled = 0
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SSAInstallOrUnInstallResult
    {
        [EnumMember]
        Successful = 1,
        [EnumMember]
        Failed = 0
    }
   
}
