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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    [KnownType(typeof(PRTreeNodeDto))]
    [KnownType(typeof(PRTreeDataNodeDto))]
    [DataContract]    
    public class PRBackupCatalogDto
    {
        [DataMember]        
        public PRTreeNodeDto TreeNode { get; set; }
        [DataMember]
        public PRBackupLevel BackupLevel { get; set; }
        [DataMember]
        public PRStagingPolicyDto StagingPolicy { get; set; }
        [DataMember]
        public Version DataFormatVersion 
        {
            get 
            {
                if (this.TreeNode != null && !string.IsNullOrEmpty(this.TreeNode.DataFormatVersion))
                {
                    return new Version(this.TreeNode.DataFormatVersion);
                }
                return PRDataFormatVersion.DefaultVersion;                
            }
            set
            {
                if (this.TreeNode != null)
                {
                    this.TreeNode.DataFormatVersion = value.ToString();
                }
            }
        }
        [DataMember]
        public bool HasClientSnapshot { get; set; }
        [DataMember]
        public string AgentVersion { get; set; }
        [DataMember]
        public long TimeStamp { get; set; }
    }


    public class PRDataFormatVersion
    {
        public static Version DefaultVersion 
        {
            get { return new Version("6.0.0.0"); }
        }
    }
}
