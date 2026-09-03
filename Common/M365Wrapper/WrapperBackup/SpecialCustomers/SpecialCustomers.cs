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


namespace ExchangeBackupUtility.Graph
{
    using System;
    using System.Collections.Generic;

    public class SpecialCustomers
    {
        /// <summary>
        /// Customers whose old domain has been deleted
        /// </summary>
        List<string> Customers_DomainNotFound = new List<string>
        {          
            "3be5972b-1b45-4824-92ee-2b82827dc838",
            "df23b802-46e7-40c8-864a-c6175b7de596",//test
            "5a85836a-a4ef-461c-b5d7-b2c2509a21af",//test
            "bf1ff696-d056-460b-88c4-afb0057663ae",//test
        };
        public bool IsDomainNotFound(string tenantId)
        {
            return Customers_DomainNotFound.Contains(tenantId);
        }

        public Dictionary<string, string> GetDomainMapping(string tenantGroupId)
        {
            if (!IsDomainNotFound(tenantGroupId)) return null;
            switch (tenantGroupId)
            {
                case "3be5972b-1b45-4824-92ee-2b82827dc838":
                    return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        {"P3-Group.com","Umlaut.com" },
                    };
                case "df23b802-46e7-40c8-864a-c6175b7de596":
                case "5a85836a-a4ef-461c-b5d7-b2c2509a21af":
                case "bf1ff696-d056-460b-88c4-afb0057663ae":
                    return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        {"MeetDux.onmicrosoft.com","octo.avepointps.com"},
                    };
                default:
                    return null;
            }
        }
    }
}