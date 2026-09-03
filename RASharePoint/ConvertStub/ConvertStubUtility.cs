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
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.Media.Service.DomainModel;

namespace AvePoint.RA.SharePoint.ConvertStub
{
    public static class ConvertStubUtility
    {
        private static readonly AveLogger s_logger = AveLogger.GetInstance(typeof(ConvertStubUtility));

        public static (bool hasStub, string stubId, string stubType) TryGetStubInfo(string stubInfo)
        {
            if (string.IsNullOrWhiteSpace(stubInfo) || stubInfo.Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                return (false, null, null);
            }

            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(stubInfo);
                var element = doc.GetElementsByTagName("StubInfo").Cast<XmlElement>().FirstOrDefault();
                if (element == null)
                {
                    return (false, null, null);
                }

                var id = element.HasAttribute("StubId") ? element.GetAttribute("StubId") : null;
                var type = element.HasAttribute("StubType") ? element.GetAttribute("StubType") : null;
                return (!string.IsNullOrWhiteSpace(id) || !string.IsNullOrWhiteSpace(type), id, type);
            }
            catch (Exception ex)
            {
                s_logger.Warn($"Invalid StubInfo: {stubInfo}, ex: {ex}");
                return (false, null, null);
            }
        }

        public static string GenerateStubInfo(string stubType, string stubId = "")
        {
            var doc = new XmlDocument();
            XmlElement headerExtraAttribute = doc.CreateElement("StubInfo");
            headerExtraAttribute.SetAttribute("StubType", stubType);
            headerExtraAttribute.SetAttribute("StubId", stubId);
            return headerExtraAttribute.OuterXml;
        }

        public static Dictionary<string, List<ArchiverBasicIndex>> GroupIndexesByWebUrl(Dictionary<string, ArchiverBasicIndex> pathLookup, List<ArchiverBasicIndex> fileIndexes)
        {
            Dictionary<string, List<ArchiverBasicIndex>> webGroups = [];
            foreach (var file in fileIndexes)
            {
                var current = file;

                while (current != null && current.Type != "W")
                {
                    if(!pathLookup.TryGetValue(current.ParentPathMD5, out current))
                    {
                        s_logger.Warn($@"Fail get parent, current id:{current?.Id}, current path:{current?.Url} , file id:{file?.Id}, file url:{file?.Url}");
                        break;
                    }
                }

                if (current != null)
                {
                    if (!webGroups.TryGetValue(current.Url, out var fileList))
                    {
                        fileList = [];
                        webGroups[current.Url] = fileList;
                    }
                    fileList.Add(file);
                }
            }

            return webGroups;
        }

        public static bool TryAddStubTypeToList(List<LeaveStubType> stubTypes, LeaveStubType stubType)
        {
            if (!stubTypes.Any(i => i == stubType))
            {
                stubTypes.Add(stubType);
                return true;
            }
            return false;
        }
    }
}
