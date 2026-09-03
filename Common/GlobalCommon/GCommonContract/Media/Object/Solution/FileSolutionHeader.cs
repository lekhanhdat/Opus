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






namespace AvePoint.GCommon.Contract.Media.Object
{
    #region using directives
    using System;
    using System.Runtime.Serialization;
    using System.Text;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Tree.Object;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FileSolutionHeader : FileHeader
    {
        [DataMember]
        public String Description { get; set; }

        [DataMember]
        public SolutionType SolutionType { get; set; }

        [DataMember]
        public String DisplayName { get; set; }

        [DataMember]
        public String SolutionId { get; set; }

        [DataMember]
        public UInt32 Lcid { get; set; }

        [DataMember]
        public String DeploymentServerType { get; set; }

        [DataMember]
        public String SolutionName { get; set; }

        [DataMember]
        public Boolean ContainsWebApplicationResource { get; set; }

        [DataMember]
        public Boolean ContainsGlobalAssembly { get; set; }

        [DataMember]
        public Boolean ContainsCodeAccessSecurityPolicy { get; set; }

        public override String ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("File Solution Header: ");
            stringBuilder.AppendFormat("Description: {0}, ", this.Description);
            stringBuilder.AppendFormat("Solution Type: {0}, ", this.SolutionType);
            stringBuilder.AppendFormat("Display Name: {0}, ", this.DisplayName);
            stringBuilder.AppendFormat("Solution Name: {0}, ", this.SolutionName);
            stringBuilder.AppendFormat("Solution Id: {0}", this.SolutionId);
            return stringBuilder.ToString();
        }
    }
}
