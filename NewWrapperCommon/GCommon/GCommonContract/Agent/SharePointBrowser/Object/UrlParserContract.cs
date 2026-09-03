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
using System.Linq;
using System.Text;
using AvePoint.GCommon.Contract.SharePointBrowser;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using System.Runtime.Serialization;

namespace AvePoint.GCommon.Contract.Agent.SharePointBrowser.Object
{
    [DataContract]
    public class UrlParserContract : BrowserContractBase
    {
        [DataMember]
        public List<UrlPareserUnit> Units { get; set; }
    }

    [DataContract]
    public class UrlPareserUnit
    {
        [DataMember]
        public string Url { get; set; }

        [DataMember]
        public NodeLevel Level { get; set; }

        [DataMember]
        public SPTreeNodeDto Tree { get; set; }

        [DataMember]
        public BposInfo BposInfo { get; set; }

        [DataMember]
        public bool HasError { get; set; }

        [DataMember]
        public UrlPareserErrorType Error { get; set; }
    }

    [DataContract]
    public enum UrlPareserErrorType : int
    {
        [EnumMember]
        Unknown = 0,

        [EnumMember]
        BposInfoIncorrect = 1,

        [EnumMember]
        TreeNodeNotFound = 2,

        [EnumMember]
        TreeLevelMismatched = 3,
    }
}
