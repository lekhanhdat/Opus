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
using System.Runtime.Serialization;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.CentralAdmin.Object;


namespace AvePoint.GCommon.Contract.Tree.Object
{
    [DataContract]
    [XmlRootAttribute("SolutionDetailDTO")]
    public class SolutionDetailDTO : IExtensibleDataObject
    {
        /// <summary>
        /// solution ID
        /// </summary>
        [DataMember]
        [XmlAttribute("solutionId")]
        public Guid solutionId { get; set; }
        [DataMember]
        [XmlAttribute("ParentSolutionName")]
        public string ParentSolutionName { get; set; }
        [DataMember]
        [XmlAttribute("Lcid")]
        public uint Lcid { get; set; }
        [DataMember]
        [XmlAttribute("DisplayName")]
        public string DisplayName { get; set; }
        /// <summary>
        /// 当前solution deploy到那个webapp里面的url
        /// </summary>
        [DataMember]
        [XmlAttribute("DeployTo")]
        public List<string> DeployTo { get; set; }
        /// <summary>
        /// "Core Solution" or  "Web Part Package" or "Language Pack(2052)"
        /// </summary>
        [DataMember]
        [XmlAttribute("Type")]
        public string Type { get; set; }

        [DataMember]
        [XmlAttribute("LastOperationResult")]
        public string LastOperationResult { get; set; }
        [DataMember]
        [XmlAttribute("LastOperationDetails")]
        public string LastOperationDetails { get; set; }
        [DataMember]
        [XmlAttribute("LastOperationTime")]
        public string LastOperationTime { get; set; }
        /// <summary>
        /// special for "Core Solution"
        /// </summary>
        [DataMember]
        [XmlAttribute("ContainsWebApplicationResource")]
        public bool ContainsWebApplicationResource { get;set; }
        [DataMember]
        [XmlAttribute("ContainsGlobalAssembly")]
        public bool ContainsGlobalAssembly { get; set; }
        [DataMember]
        [XmlAttribute("ContainsCodeAccessSecurityPolicy")]
        public bool ContainsCodeAccessSecurityPolicy { get; set; }
        /// <summary>
        /// "Front-end Web server" or "Application Server"
        /// </summary>
        [DataMember]
        [XmlAttribute("DeploymentServerType")]
        public string DeploymentServerType { get; set; }

        /// <summary>
        /// special for  "Web Part Package"
        /// </summary>
        [DataMember]
        [XmlAttribute("GACDeployment")]
        public bool GACDeployment { get; set; }
        /// <summary>
        /// 给GUI使用的，用来判断可以对当前solution正在采用的动作，给GUItree 选择判断使用
        /// </summary>
        [DataMember]
        [XmlAttribute("InternalDeploymentStatus")]
        public SolutionStaus InternalDeploymentStatus { get; set; }
        /// <summary>
        /// for retract
        /// </summary>
        [DataMember]
        [XmlAttribute("urls")]
        public List<string> urls{get;set;}
        [DataMember]
        [XmlAttribute("retractTime")]
        public string retractTime{get;set;}
        /// <summary>
        /// for Language Pack
        /// </summary>
        [DataMember]
        [XmlAttribute("LanguagePacks")]
        public string LanguagePacks { get; set; }
        /// <summary>
        /// SharePoint API返回的，用来判断可以对当前solution当前状态的，给用户查看使用
        /// </summary>
        [DataMember]
        [XmlAttribute("DeploymentStatus")]
        public string DeploymentStatus { get; set; }
        /// <summary>
        /// solution的hash code，userSolution使用
        /// </summary>
        [DataMember]
        [XmlAttribute("SolutionHash")]
        public string SolutionHash { get; set; }
        /// <summary>
        /// 当solution是globalDeployed时，需要给GUI返回一个状态，GUI判断使用
        /// </summary>
        [DataMember]
        [XmlAttribute("IsGlobalDeployed")]
        public bool IsGlobalDeployed { get; set; }

        private ExtensionDataObject extensionData;
        public ExtensionDataObject ExtensionData
        {
            get
            {
                return extensionData;
            }
            set
            {
                extensionData = value;
            }
        }
    }
    [DataContract]
    public enum SolutionStaus
    {
        [EnumMember]
        Installed= 0,

        [EnumMember]
        Deployed = 1,

        [EnumMember]
        Deploying = 2,

        [EnumMember]
        Retracting = 3,

        [EnumMember]
        Removing = 4,

        [EnumMember]
        Actived = 5,

        [EnumMember]
        DeActived = 6,

        [EnumMember]
        Activing = 7,

        [EnumMember]
        Upgraded = 8,

        [EnumMember]
        Upgrading = 9

    }
}
