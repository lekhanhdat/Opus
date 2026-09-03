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




namespace AvePoint.GCommon.Contract.CentralAdmin.Object.SecurityCenter
{
    #region using directives
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.Common.AdminSearch.Object;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASearchWebPartOperation : CAOperation
    {
         [DataMember]
        public List<CASearchWebPartFilter> Filters { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASearchWebPartFilter
    {
         [DataMember]
        public string Rule { get; set; }

         [DataMember]
         public bool IsContain { get; set; }

         [DataMember]
         public string Value { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAEditWebPartOperation : CAOperation
    {
         [DataMember]
        public WebPartAction Action { get; set; }

         [DataMember]
         public List<WebPartInstanceInfo> WebPartInstances { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum WebPartAction
    {
        [EnumMember]
        Add,
        [EnumMember]
        Close,
        [EnumMember]
        Get,
        [EnumMember]
        Remove,
        [EnumMember]
        Reset
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class WebPartInstanceInfo : ResultBase
    {
        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Zone { get; set; }

        [DataMember]
        public int Order { get; set; }

        [DataMember]
        public string PageUrl { get; set; }

        [DataMember]
        public string WebURL { get; set; }

        [DataMember]
        public string WebTitle { get; set; }

        [DataMember]
        public string SiteUrl { get; set; }

        [DataMember]
        public WebPartTemplate Template { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class WebPartTemplate
    {
        [DataMember]
        public string TypeName { get; set; }

        [DataMember]
        public string Title { get; set; }

        //add Name&DisplayName(ADO-24336)
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        /// <summary>
        /// Only For GUI
        /// </summary>
        [DataMember]
        public int Usage { get; set; }

        [DataMember]
        public string CreatedBy { get; set; }

        [DataMember]
        public string ModifiedBy { get; set; }
    }
}
