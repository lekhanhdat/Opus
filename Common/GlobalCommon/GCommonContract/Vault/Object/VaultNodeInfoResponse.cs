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



using System.Runtime.Serialization;

namespace AvePoint.GCommon.Contract.Vault.Object
{
    [DataContract]
    public class VaultNodeInfoResponse : OperationResult
    {
        [DataMember]
        public bool IsConfigIndexDevice { set; get; }

        /// <summary>
        /// Node{n:1}Profile
        /// </summary>
        [DataMember]
        public string ProfileID { set; get; }

        [DataMember]
        public InheritStauts InheritStauts { set; get; }

        [DataMember]
        public ApplyStatus ApplyStatus { set; get; }
    }

    /// <summary>
    /// yes:设置setting 
    /// </summary>
    [DataContract]
    public enum InheritStauts
    {
        /// <summary>
        /// current no and parent no
        /// </summary>
        [EnumMember]
        None = 0,
        /// <summary>
        /// current yes and parent no
        /// </summary>
        [EnumMember]
        Self = 1,
        /// <summary>
        /// current no and parent yes
        /// </summary>
        [EnumMember]
        Inherited = 2,
        /// <summary>
        /// current yes and parent yes
        /// </summary>
        [EnumMember]
        Individual = 3
    }

    [DataContract]
    public enum ApplyStatus
    {
        [EnumMember]
        None,
        [EnumMember]
        Apply,
        [EnumMember]
        Retract
    }

}
