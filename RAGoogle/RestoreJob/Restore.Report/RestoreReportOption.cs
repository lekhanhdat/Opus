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

namespace RAGoogle.Restore.Report
{
    internal enum RestoreReportOption
    {
        None,
        Skipped,
        Overwritten,
        Appended,
        Replaced,
        NewCreated,
    }

    internal static class RestoreReportOptionExtension
    {
        private static Dictionary<RestoreReportOption, string> mapping
        {
            get
            {
                return new Dictionary<RestoreReportOption, string>()
                {
                    //{RestoreReportOption.None,string.Empty},
                    //{RestoreReportOption.NewCreated,RestoreReportResource.Item_OptionNewCreate},
                    //{RestoreReportOption.Replaced,RestoreReportResource.Item_OptionReplaced},
                    //{RestoreReportOption.Appended,RestoreReportResource.Item_OptionAppended},
                    //{RestoreReportOption.Overwritten,RestoreReportResource.Item_OptionOverwritten},
                    //{RestoreReportOption.Skipped,RestoreReportResource.Item_OptionSkipped},
                };
            }
        }

        public static string GetResourceString(this RestoreReportOption option)
        {
            if (mapping.ContainsKey(option))
            {
                return mapping[option];
            }
            return option.ToString();
        }
    }
}
