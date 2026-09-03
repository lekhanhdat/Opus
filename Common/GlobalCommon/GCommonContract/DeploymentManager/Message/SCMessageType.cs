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
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.DeploymentManager.Message
{
    /// <summary>
    /// SCMessageType用来定义SolutionCenter做job的方式
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SCMessageType
    {
        [EnumMember]
        Retract = 1,
        [EnumMember]
        Remove = 2,
        [EnumMember]
        Update = 3,
        [EnumMember]
        Deploy = 4,
        [EnumMember]
        Active = 5,
        [EnumMember]
        DeActive = 6,
        [EnumMember]
        DeployFromMedia = 7,
        [EnumMember]
        DeployFromDisk = 8,
        [EnumMember]
        DeployToMedia = 9,
        [EnumMember]
        RemoveVersion = 10,
        [EnumMember]
        Upgrade = 11,
        [EnumMember]
        DeployToMediaForRollback = 12,
        [EnumMember]
        None = 0
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum BrowseLevel
    {
        [EnumMember]
        Farm,
        [EnumMember]
        SiteCollection
    }
}
