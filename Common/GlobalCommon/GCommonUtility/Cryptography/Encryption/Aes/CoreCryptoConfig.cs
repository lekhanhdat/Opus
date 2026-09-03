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





namespace AvePoint.GCommon.Utility.Cryptography.Encryption.Aes
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    #endregion

    internal static class CoreCryptoConfig
    {
        // Fields
        private static Dictionary<string, Type> s_nameMap;

        // Methods
        [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
        public static T CreateFromName<T>(string name) where T : class
        {
            Type type;
            if (AlgorithmNameMap.TryGetValue(name, out type))
            {
                return (T)Activator.CreateInstance(type);
            }
            return (T)CryptoConfig.CreateFromName(name);
        }

        // Properties
        private static Dictionary<string, Type> AlgorithmNameMap
        {
            get
            {
                if (s_nameMap == null)
                {
                    Dictionary<string, Type> dictionary = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
                    dictionary.Add("AES", typeof(AesCryptoServiceProvider));
                    dictionary.Add(typeof(AesCryptoServiceProvider).Name, typeof(AesCryptoServiceProvider));
                    dictionary.Add(typeof(AesCryptoServiceProvider).FullName, typeof(AesCryptoServiceProvider));

                    s_nameMap = dictionary;
                }
                return s_nameMap;
            }
        }

        //internal static bool EnforceFipsAlgorithms
        //{
        //    get
        //    {
        //        if (!s_enforceFipsAlgorithms.HasValue)
        //        {
        //            try
        //            {
        //                using (new SHA1Managed())
        //                {
        //                    s_enforceFipsAlgorithms = false;
        //                }
        //            }
        //            catch (InvalidOperationException )
        //            {
        //                s_enforceFipsAlgorithms = true;
        //            }
        //        }
        //        return s_enforceFipsAlgorithms.Value;
        //    }
        //}
    }
}
