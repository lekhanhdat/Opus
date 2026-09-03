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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;

namespace ExchangeBackupUtility.Graph;

public abstract class BaseExchangeItem
{
    protected IRALogger Logger;

    public BaseExchangeItem()
    {
        Logger = new RALogger(GetType());
    }
    protected string GetSensitivityLabelFromString(string msipStr)
    {
        string res = "";
        if (!string.IsNullOrWhiteSpace(msipStr))
        {
            string[] strArr = msipStr.Split(';');
            foreach (string str in strArr)
            {
                int index = str.IndexOf('=');
                if (index > -1)
                {
                    string key = str.Substring(0, index);
                    string value = str.Substring(index + 1);
                    if (key.Contains("Name"))
                    {
                        res = value;
                    }
                }
                string[] keyValue = str.Split('=');

            }
        }
        return res;
    }


}