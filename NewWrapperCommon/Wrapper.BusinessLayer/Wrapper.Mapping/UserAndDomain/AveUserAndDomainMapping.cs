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
    using AvePoint.Common;

    public class AveUserAndDomainMapping : AveRestoredUserMapping, IAveUserAndDomainMapping
    {
        public IAveCustomUserAndDomainMapping customUserAndDomainMapping;

        public AveUserAndDomainMapping()
        {
            AveCustomUserAndDomainMappingFactory customUserAndDomainMappingFactory = new AveCustomUserAndDomainMappingFactory();
            customUserAndDomainMapping = customUserAndDomainMappingFactory.GetCustomUserAndDomainMapping();
        }

        public string GetMappingLoginNameBeforeAdd(string srcLoginName)
        {
            return customUserAndDomainMapping.GetMappingLoginNameBeforeAdd(srcLoginName);
        }

        public string GetMappingDomainNameBeforeAdd(string srcDomainName)
        {
            return customUserAndDomainMapping.GetMappingDomainNameBeforeAdd(srcDomainName);
        }


        public IEnumerable<KeyValuePair<string, string>> EnumCustomUserMapping()
        {
            return customUserAndDomainMapping.EnumCustomUserMapping();
        }

        public IEnumerable<KeyValuePair<string, string>> EnumCustomDomainMapping()
        {
            return customUserAndDomainMapping.EnumCustomDomainMapping();
        }


        public void SetUserAndDomainMappings(Dictionary<string, string> userMappings, Dictionary<string, string> domainMappings)
        {
            customUserAndDomainMapping.SetUserAndDomainMappings(userMappings, domainMappings);
        }

        public override void Dispose()
        {
            if (customUserAndDomainMapping != null)
            {
                customUserAndDomainMapping.Dispose();
                customUserAndDomainMapping = null;
            }
            base.Dispose();
        }
    }
}
