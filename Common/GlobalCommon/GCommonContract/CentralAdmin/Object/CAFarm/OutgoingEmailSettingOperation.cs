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
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion


    [DataContract(Namespace = ContractConstants.Namespace)]
    public class OutgoingEmailSettingOperation : CAOperation
    {
        [DataMember]
        public String OutboudSMTPServer { get; set; }

        [DataMember]
        public String FromAddress { get; set; }
        
        [DataMember]
        public String ReplyToAddress { get; set; }
        
        [DataMember]
        public List<CharacterSet> CharacterSets { get; set; }
        
        [DataMember]
        public CharacterSet SelectedCharSet { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CharacterSet
    {
        [DataMember]
        public Int32 Id { get; set; }
        
        [DataMember]
        public String Name { get; set; }

        public override String ToString()
        {
            return this.Name;
        }

        public override bool Equals(object obj)
        {
            CharacterSet CurrCharSet = obj as CharacterSet;

            if (CurrCharSet != null && this.Id == CurrCharSet.Id)
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
            return this.Id.GetHashCode();
        }
    }
}
