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





namespace AvePoint.Wrapper.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.Wrapper.Common;

    public class AveCustomUserAndDomainMapping : IAveCustomUserAndDomainMapping
    {
        object _lock = new object();
        Dictionary<string, string> customUserMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, string> customDomainMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public AveCustomUserAndDomainMapping()
        { 
        }
        public AveCustomUserAndDomainMapping(Dictionary<string, string> usermappings, Dictionary<string, string> domainmappings)
        {
            customUserMappings = usermappings;
            customDomainMappings = domainmappings;
        }

        public void SetUserAndDomainMappings(Dictionary<string, string> usermMppings, Dictionary<string, string> domainMappings)
        {
            if (usermMppings != null)
            {
                foreach (KeyValuePair<string, string> user in usermMppings)
                {
                    customUserMappings.AddWithLock(user.Key, user.Value);
                }
            }
            if (domainMappings != null)
            {
                foreach (KeyValuePair<string, string> domain in domainMappings)
                {
                    customDomainMappings.AddWithLock(domain.Key, domain.Value);
                }
            }
        }

        public string GetMappingLoginNameBeforeAdd(string srcLoginName)
        {
            return customUserMappings.GetValueWithLock(srcLoginName);
        }
        public IEnumerable<KeyValuePair<string, string>> EnumCustomUserMapping()
        {
            lock (_lock)
            {
                foreach (var value in customUserMappings)
                {
                    yield return value;
                }
            }
        }

        public string GetMappingDomainNameBeforeAdd(string srcDomainName)
        {
            return customDomainMappings.GetValueWithLock(srcDomainName);
        }

        public IEnumerable<KeyValuePair<string, string>> EnumCustomDomainMapping()
        {
            lock (_lock)
            {
                foreach (var value in customDomainMappings)
                {
                    yield return value;
                }
            }
        }

        public void Dispose()
        {
            if (customUserMappings != null)
            {
                customUserMappings = null;
            }
            if (customDomainMappings != null)
            {
                customDomainMappings = null;
            }
        }

        //For UnitTest
        public void ClearMapping()
        {
            customUserMappings.Clear();
            customDomainMappings.Clear();
        }
    }
}
