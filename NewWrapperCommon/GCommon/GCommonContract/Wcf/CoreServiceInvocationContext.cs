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



namespace AvePoint.GCommon.Contract
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CoreServiceInvocationContext
    {
        [DataMember]
        public String TypeKey { get; set; }
        [DataMember]
        public String TypeName { get; set; }
        [DataMember]
        public String MethodName { get; set; }
        [DataMember]
        public Int32 ArgsCount { get; set; }
        [DataMember]
        public List<String> Args { get; set; }
        [DataMember]
        public List<String> ArgsTypeNames { get; set; }
        [DataMember]
        public List<String> GenericParameterTypeNames { get; set; }
        [DataMember]
        public String ReturnValue { get; set; }
        [DataMember]
        public String ReturnValueTrueType { get; set; }
        [DataMember]
        public String Uri { get; set; }
    }
}
