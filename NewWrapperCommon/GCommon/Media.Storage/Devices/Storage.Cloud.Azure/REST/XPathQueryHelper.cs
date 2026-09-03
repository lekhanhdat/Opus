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


namespace AvePoint.Media.Storage.Cloud.Azure
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Xml;
    using System.Globalization;
    #endregion

    class XPathQueryHelper
    {
        public static readonly string NextMarkerQuery = string.Join("/",
            new string[] 
            {
                "", "", 
                MSAzureConstants.EnumerationResults,
                MSAzureConstants.NextMarker
            });

        public static readonly string ContainerQuery = string.Join("/",
            new string[] 
            {
                "", "", 
                MSAzureConstants.EnumerationResults,
                MSAzureConstants.Containers,
                MSAzureConstants.Container
            });

        public static readonly string BlobQuery = string.Join("/",
            new string[] 
            {
                "", "", 
                MSAzureConstants.EnumerationResults,
                MSAzureConstants.Blobs,
                MSAzureConstants.Blob
            });

        public static readonly string BlockQuery = string.Join("/",
            new string[] 
            {
                "", "", 
                MSAzureConstants.BlockList,
                MSAzureConstants.Block
            });


        public static readonly string CommonPrefixQuery = string.Join("/",
            new string[] 
            {
                "", "", 
                MSAzureConstants.EnumerationResults,
                MSAzureConstants.Blobs,
                MSAzureConstants.BlobPrefix
            });

        public static readonly string ErrorCodeQuery = string.Join("/",
            new string[] 
            { 
                "", "",
                MSAzureConstants.ErrorRootElement,
                MSAzureConstants.ErrorCode
            });

        public static DateTime? LoadSingleChildDateTimeValue(XmlNode node, string childName, bool throwIfNotFound)
        {
            XmlNode childNode = node.SelectSingleNode(childName);

            if (childNode != null && childNode.FirstChild != null)
            {
                DateTime? dateTime;
                if (!TryGetDateTimeFromHttpString(childNode.FirstChild.Value, out dateTime))
                {
                    throw new ArgumentException("Date time value returned from server " + childNode.FirstChild.Value + " can't be parsed.");
                }
                return dateTime;
            }
            else if (!throwIfNotFound)
            {
                return null;
            }
            else
            {
                return null;
            }
        }

        public static string LoadSingleChildStringValue(XmlNode node, string childName, bool throwIfNotFound)
        {
            XmlNode childNode = node.SelectSingleNode(childName);

            if (childNode != null && childNode.FirstChild != null)
            {
                return childNode.FirstChild.Value;
            }
            else if (!throwIfNotFound)
            {
                return null;
            }
            else
            {
                return null;   // unnecessary since Fail will throw, but keeps the compiler happy
            }
        }

        public static string loadMetaChildStringValue(XmlNode node, string metaChildName)
        {
            XmlNode childNode = node.SelectSingleNode(MSAzureConstants.Metadata);
            if (childNode != null && childNode.FirstChild != null)
            {
                XmlNode metaChildNode = childNode.SelectSingleNode(metaChildName);
                if (metaChildNode != null && metaChildNode.FirstChild != null)
                {
                    return Uri.UnescapeDataString(metaChildNode.FirstChild.Value);
                }
            }

            return null;
        }

        public static long? LoadSingleChildLongValue(XmlNode node, string childName, bool throwIfNotFound)
        {
            XmlNode childNode = node.SelectSingleNode(childName);

            if (childNode != null && childNode.FirstChild != null)
            {
                return long.Parse(childNode.FirstChild.Value, CultureInfo.InvariantCulture);
            }
            else if (!throwIfNotFound)
            {
                return null;
            }
            else
            {
                return null;   // unnecessary since Fail will throw, but keeps the compiler happy
            }
        }


        public static bool TryGetDateTimeFromHttpString(string dateString, out DateTime? result)
        {
            DateTime dateTime;
            result = null;

            // 'R' means rfc1123 date which is the preferred format used in HTTP
            bool parsed = DateTime.TryParseExact(dateString, "R", null, DateTimeStyles.None, out dateTime);
            if (parsed)
            {
                // For some reason, format string "R" makes the DateTime.Kind as Unspecified while it's actually
                // Utc. Specifying DateTimeStyles.AssumeUniversal also doesn't make the difference. If we also
                // specify AdjustToUniversal it works as expected but we don't really want Parse to adjust 
                // things automatically.
                result = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                return true;
            }

            return false;
        }

    }
}
