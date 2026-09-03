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






namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    #region using directives
    using System;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class QuotaTemplate
    {
        [DataMember]
        public String Name { get; set; }

        [DataMember]
        public Int64 StorageMaximumLevel { get; set; }

        [DataMember]
        public Int32 InvitedUserMaximumLevel { get; set; }

        [DataMember]
        public Double UserCodeMaximumLevel { get; set; }

        [DataMember]
        public Double UserCodeWarningLevel { get; set; }

        [DataMember]
        public Int64 StorageWarningLevel { get; set; }

        [DataMember]
        public String OldName { get; set; }

        public override string ToString()
        {
            return this.Name;
        }

        public override bool Equals(object obj)
        {
            QuotaTemplate Template = obj as QuotaTemplate;

            if (Template != null && this.Name.Equals(Template.Name))
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
            return this.Name.GetHashCode();
        }
    }
}
