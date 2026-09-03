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
using System.Diagnostics;

namespace AvePoint.RA.Contract.CodeView
{
    [Conditional("DEBUG")]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Interface
        , AllowMultiple = true, Inherited = false)]
    public class RACodeReviewAttribute : Attribute
    {
        public string Date { get; set; }
        public string Reviewer { get; set; }
        public string Comment { get; set; }
        public RACodeReviewAttribute(string reviewer, string comment = "", string date = "")
        {
            this.Date = Date;
            this.Reviewer = reviewer;
            this.Comment = comment;
        }
    }
}
