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




namespace AvePoint.Wrapper.Restore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using LS.SPWorkflowProcessor.Services;
    using AvePoint.Wrapper.Common;
    using System.Text.RegularExpressions;

    internal sealed class AveUserMappingService : UserMappingService
    {
        private const string regsting = @"\\|([/*?""<>|:])";
        private Dictionary<string, string> loginNameExclude = null;
        internal static AveSPSite AveSite
        {
            get;
            set;
        }

        private AveUserMappingService()
            : base()
        { }

        private AveUserMappingService(Dictionary<string, string> param)
        {
            if (param != null)
            {
                loginNameExclude = param;
            }
        }
        public override IAveUser GetOrCreateUser(string loginName)
        {
            if (AveSite == null || AveSite.SPMembers == null)
                return null;
            if (CheckLoginName(loginName))
            {
                return null;
            }
            return AveSite.SPMembers.GetOrAddUser(loginName);
        }
        private bool CheckLoginName(string loginName)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveUserMappingService.CheckLoginName"))
            {
#endif
                if (CheckExclude(loginName))
                {
                    return true;
                }
                int i;
                if ((i = loginName.IndexOf('\\')) > 0)
                {
                    string later = loginName.Substring(i + 1, loginName.Length - i - 1);
                    Regex regex = new Regex(regsting);
                    if (regex.IsMatch(later))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if ((i = loginName.LastIndexOf('|')) > 0)
                {
                    string later = loginName.Substring(i + 1, loginName.Length - i - 1);
                    Regex regex = new Regex(regsting);
                    if (regex.IsMatch(later))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;

#if PerformanceLog
            }
#endif
        }
        private bool CheckExclude(string loginName)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveUserMappingService.CheckExclude"))
            {
#endif
                if (loginNameExclude != null)
                {
                    if (loginNameExclude.ContainsKey("FilterString"))
                    {
                        string filterstring = loginNameExclude["FilterString"];
                        string[] filterstrings = filterstring.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string s in filterstrings)
                        {
                            if (loginName.Contains(s))
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                }
                return false;

#if PerformanceLog
            }
#endif
        }
    }
}
