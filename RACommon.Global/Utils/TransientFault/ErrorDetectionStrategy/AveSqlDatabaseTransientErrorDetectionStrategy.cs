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
using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace AvePoint.RA.Common.TransientFault.ErrorDetectionStrategy
{
    public sealed class AveSqlDatabaseTransientErrorDetectionStrategy : ITransientErrorDetectionStrategy
    {
        public bool IsTransient(Exception ex)
        {
            if (ex != null)
            {
                SqlException sqlException;
                if ((sqlException = ex as SqlException) != null)
                {
                    // Enumerate through all errors found in the exception.
                    foreach (SqlError err in sqlException.Errors)
                    {
                        switch (err.Number)
                        {
                            case 0:
                                if ((err.Class == 20 || err.Class == 11) && err.State == 0 && err.Server != null && ex.InnerException == null)
                                {
                                    if (string.Equals(err.Message, "A severe error occurred on the current command.  The results, if any, should be discarded.", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        return true;
                                    }
                                }
                                return false;
                            case 40501:
                            case 4060:
                            case 10928:
                            case 10929:
                            case 10053:
                            case 10054:
                            case 10060:
                            case 40197:
                            case 40540:
                            case 40613:
                            case 40143:
                            case 233:
                            case 64:
                            case 20:
                                return true;
                        }
                    }
                }
                else if (ex is TimeoutException)
                {
                    return true;
                }
                else
                {
                    DataException entityException;
                    if ((entityException = ex as DataException) != null)
                    {
                        return this.IsTransient(entityException.InnerException);
                    }
                }
            }

            return false;
        }

    }
}