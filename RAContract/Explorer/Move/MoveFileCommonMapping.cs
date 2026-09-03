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
using AvePoint.GCommon.Contract.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Explorer
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class MoveFileCommonMapping
    {
        [DataMember]
        public LengthItem LengthItem { get; set; }

        [DataMember]
        public List<IllegalCharReplaceMappingItem> IllegalCharReplaceMappings { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class LengthItem
    {
        [DataMember]
        public bool IsCheckedMaxForlderName { get; set; }

        [DataMember]
        public int MaxForlderNameLength { get; set; }

        [DataMember]
        public bool IsCheckedMaxFileName { get; set; }

        [DataMember]
        public int MaxFileNameLength { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class IllegalCharReplaceMappingItem
    {
        [DataMember]
        public string IllegalChar { get; set; }

        [DataMember]
        public string ReplaceChar { get; set; }

        [DataMember]
        public int Type { get; set; }

        public override string ToString()
        {
            if (IllegalChar == "{")
            {
                return "left brace";
            }
            else if (IllegalChar == "}")
            {
                return "right brace";
            }
            else if (IllegalChar == ".")
            {
                return "dot";
            }
            return IllegalChar;
        }
    }
}
