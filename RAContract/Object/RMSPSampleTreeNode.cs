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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.RA.Contract.Object.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.Object
{
    [DataContract(IsReference = true)]
    [JsonObject]
    public class RMSPSampleTreeNode : RMBaseTreeNode<RMSPSampleTreeNode>, IDisposable
    {
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsArchiverTree { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string SPObjectId { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string FarmId { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string FarmName { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int SPType { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int SPVersion { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int TemplateId { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string TeamName { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public BposInfo BposInfo { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public RMSPSampleTreeNode Parent { set { base.Parent = value; } get { return base.Parent; } }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public List<RMSPSampleTreeNode> Children { set { base.Children = value; } get { return base.Children; } }

        [DataMember(EmitDefaultValue = true)]
        [JsonProperty]
        public int ChannelType { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string SearchKey { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsSearch { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int SourceType { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string TeamsId { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string O365TenantId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsTeams { set; get; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsEnableTeams { set; get; }

        /// <summary>
        /// 浅拷贝
        /// </summary>
        /// <returns></returns>
        public RMSPTreeNode Clone()
        {
            return this.MemberwiseClone() as RMSPTreeNode;
        }

        public void Dispose()
        {
            try
            {
                foreach (var child in this.Children)
                {
                    using (child as IDisposable)
                    { }
                }
                this.Children = null;
            }
            catch
            { //Noncompliant
            }
        }
    }

    //public class RMSPTreeNodeSchedule {
    //    public RMSPTreeNode TreeNode { get; set; }
    //    public ScheduleInfo ScheduleInfo { get; set; }
    //}

    public enum SettingScheduleType
    {
        Dispose = 0,
        OneDriveDisposal = 1,
        //Collection = 1
        TeamsDisposal = 2
    }

    public enum IconStatus
    {
        NoSet = 0,
        Inhert = 1,
        Break = 2
    }
}
