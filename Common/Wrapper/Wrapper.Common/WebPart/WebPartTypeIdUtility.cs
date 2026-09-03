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
namespace AvePoint.Wrapper.Common
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public static class WebPartTypeIdUtility
    {
        private static Dictionary<string, Guid> ids = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        public static Guid GenerateId(string typeFullName)
        {
            Guid id = Guid.Empty;

            if (!string.IsNullOrEmpty(typeFullName))
            {
                typeFullName = typeFullName.Trim();

                bool findValue = false;

                lock (ids)
                {
                    findValue = ids.TryGetValue(typeFullName, out id);
                }

                if (!findValue)
                {
                    string[] split = typeFullName.Split(new char[] { ',' }, 2);
                    string typeName = split[0].Trim();
                    string assemblyName = split[1].Trim();
                    id = GetTypeMD5ID(string.Concat(assemblyName, '|', typeName));

                    lock (ids)
                    {
                        ids[typeFullName] = id;
                    }
                }
            }

            return id;
        }

        private static Guid GetTypeMD5ID(string data)
        {
            using (var crptoProvider = new System.Security.Cryptography.MD5CryptoServiceProvider())
            {
                byte[] hashBytes = crptoProvider.ComputeHash(Encoding.Unicode.GetBytes(data));
                return new Guid(hashBytes);
            }
        }
    }
}
