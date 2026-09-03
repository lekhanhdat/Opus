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

namespace ExchangeUtility.Graph
{
    public class ExchangeUpdateItemResult
    {
        public string Id { get; set; }
        public string ErrorMessage { get;  set; }
        public string ErrorCode { get;  set; }
        public bool IsFailed
        {
            get { return !string.IsNullOrEmpty(this.ErrorMessage); }
        }
        public ExchangeUpdateItemResult() { }
        public static ExchangeUpdateItemResult CreateSuccessfulResult(string id)
        {
            return new ExchangeUpdateItemResult() { Id = id };
        }
        public static ExchangeUpdateItemResult CreateFailedResult(string id, string error, string errorCode)
        {
            return new ExchangeUpdateItemResult()
            {
                Id = id,
                ErrorMessage = string.Format("Error code: {0}.{1}{2}", errorCode, Environment.NewLine, error),
                ErrorCode = errorCode
            };
        }
        public static ExchangeUpdateItemResult CreateFailedResult(string id, string error)
        {
            return new ExchangeUpdateItemResult()
            {
                Id = id,
                ErrorMessage = error,
            };
        }
    }
}
