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




namespace AvePoint.GCommon.Contract.ReportCenter.Object
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
using AvePoint.GCommon.Contract.SharePointBrowser.Object;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SPUserScope : BaseScope
    {
        [DataMember]
        public bool IncludeAllUsers { get; set; }
        [DataMember]
        public List<UserDetail> IncludeUsers { get; set; }
        [DataMember]
        public List<UserDetail> ExcludeUsers { get; set; }

        public override string ToString()
        {
            StringBuilder buider = new StringBuilder();
            buider.Append(string.Format("IncludeAllUsers:{0}", IncludeAllUsers));
            if (IncludeUsers != null)
            {
                List<string> names = new List<string>();
                foreach (var item in IncludeUsers)
                {
                    names.Add(item.LoginName.ToLower());
                }
                names.Sort();
                buider.Append(string.Format("IncludeUsers:{0}", string.Join("|", names.ToArray())));
            }
            if (ExcludeUsers != null)
            {
                List<string> names = new List<string>();
                foreach (var item in ExcludeUsers)
                {
                    names.Add(item.LoginName.ToLower());
                }
                names.Sort();
                buider.Append(string.Format("ExcludeUsers:{0}", string.Join("|", names.ToArray())));
            }
            return buider.ToString();
        }
    }
}