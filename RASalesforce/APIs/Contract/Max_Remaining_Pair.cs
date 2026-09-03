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
namespace RASalesforce.APIs;

public class Max_Remaining_Pair
{
    public int Max { get; set; }

    public int Remaining { get; set; }

    public int Used { get { return Max - Remaining; } }

    public new string ToString()
    {
        return string.Format("Max:{0}, remaining:{1}, Used:{2}.", Max, Remaining, Used);
    }
    
    public long GetUsedDataStorage()
    {
        double result = (float)Used / (float)Max * 100;
        return (long)result ;
    }

    public int GetTotalSize()
    {
        return Used;
    }
}
