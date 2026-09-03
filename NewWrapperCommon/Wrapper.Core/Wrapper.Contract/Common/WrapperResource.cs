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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Security.Permissions;
using System.Text;

namespace AvePoint.Wrapper.Core.Common
{
    class WrapperResource
    {
        private static readonly WrapperResourceManager manager = new WrapperResourceManager(Constants.WrapperCoreName,
                                                                                            WrapperEnv.ResourceFolder);

        /// <summary>
        /// Get String
        /// </summary>
        /// <param name="resourceKey"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static string GetString(string resourceKey, params object[] args)
        {
            var text = string.Empty;
            try
            {
                text = WrapperResource.GetResourceString(resourceKey);

                if (text == null)
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendFormat("Key:{0};", resourceKey);
                    if (args != null)
                    {
                        foreach (var arg in args)
                        {
                            builder.AppendFormat("Argument:{0};", arg);
                        }
                    }
                    return builder.ToString();
                }
                else
                {
                    var flag = args != null && args.Length > 0;
                    if (flag || text.Contains("{"))
                    {
                        text = string.Format(CultureInfo.CurrentUICulture, text, args);
                    }
                }
            }
            catch (Exception ex)
            {
                var message = string.Format(CultureInfo.InvariantCulture, "Failed to load or format string Id {0} for culture {1}: {2}", new object[]
		        {
			        resourceKey,
			        CultureInfo.CurrentUICulture.Name,
			        ex
		        });
                throw new InvalidOperationException(message, ex);
            }
            return text;
        }


        private static string GetResourceString(string key)
        {
            return manager.GetString(key, CultureInfo.CurrentUICulture);
        }

        /// <summary>
        /// Release the resources
        /// </summary>
        public static void ReleaseAllResources()
        {
            manager.Dispose();
        }
    }

    /// <summary>
    /// Resource Manager for Wrapper
    /// </summary>
    class WrapperResourceManager : IDisposable
    {
        private readonly string baseName;
        private readonly string resourceDir;
        private readonly Dictionary<int, ResourceSet> resourceSets; 

        public WrapperResourceManager(string baseName, string resourceDir)
        {
            this.baseName = baseName;
            this.resourceDir = resourceDir;
            this.resourceSets = new Dictionary<int, ResourceSet>();
        }

        public string GetString(string name)
        {
            return GetString(name, null);
        }

        public string GetString(string name, CultureInfo culture)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (culture == null)
            {
                culture = CultureInfo.CurrentUICulture;
            }

            ResourceSet resourceSet = GetResourceSet(culture, true);

            if (resourceSet == null)
            {
                throw new InvalidOperationException(string.Format("Cannot get string by name:{0} and culture:{1}", name,
                                                                  culture));
            }

            return resourceSet.GetString(name);
        }

        private ResourceSet GetResourceSet(CultureInfo culture, bool tryParents)
        {
            ResourceSet resourceSet = null;

            lock (resourceSets)
            {
                if (!resourceSets.TryGetValue(culture.LCID, out resourceSet))
                {
                    string resourceFileName = FindResourceFile(culture);

                    if (resourceFileName == null)
                    {
                        if (!culture.Equals(CultureInfo.InvariantCulture))
                        {
                            resourceSet = GetResourceSet(culture.Parent, true);
                        }
                    }
                    else
                    {
                        resourceSet = new ResXResourceSet(resourceFileName);
                        resourceSets[culture.LCID] = resourceSet;
                    }
                }
            }

            return resourceSet;
        }

        private string FindResourceFile(CultureInfo culture)
        {
            var resourceFileName = this.GetResourceFileName(culture);
            if (this.resourceDir != null)
            {
                var path = Path.Combine(this.resourceDir, resourceFileName);
                if (File.Exists(path)) return path;
            }
            if (File.Exists(resourceFileName))
            {
                return resourceFileName;
            }
            return null;
        }


        private string GetResourceFileName(CultureInfo culture)
        {
            var builder = new StringBuilder(0xff);
            builder.Append(this.baseName);
            if (!culture.Equals(CultureInfo.InvariantCulture))
            {
                //CultureInfo.VerifyCultureName(culture, true);
                builder.Append('.');
                builder.Append(culture.Name);
            }
            builder.Append(".resx");
            return builder.ToString();
        }

        public void Dispose()
        {
            lock (resourceSets)
            {
                foreach (var keyValue in resourceSets)
                {
                    keyValue.Value.Dispose();
                }
                resourceSets.Clear();
            }
        }
    }
}
