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
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RAFileSystem.FileSystem.Discovery.Tags.Contract;
using System.Collections.Generic;
using System.IO;

namespace RAFileSystem.FileSystem.Discovery.Tags.Analyzer.BuildIn
{
    internal class FileShareDocumentSizeRangeTagAnalyzer : FileShareBuildInTagAnalyzer
    {
        internal override BuildInRuleType RuleType => BuildInRuleType.DocumentSizeRange;

        internal FileShareDocumentSizeRangeTagAnalyzer(BuildInRuleInfo ruleInfo) : base(ruleInfo)
        {
        }

        internal override object Analyse(FileInfo document)
        {
            var sizeRangeInfoes = JsonConvert.DeserializeObject<List<DocumentSizeRangeInfo>>(_ruleInfo.AdditionalInformation, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            var size = document.Length;
            foreach (var sizeRangeInfo in sizeRangeInfoes)
            {
                if (size >= sizeRangeInfo.GenerateEqual * 1024 * 1024 && size < sizeRangeInfo.LessThan * 1024 * 1024)
                {
                    return sizeRangeInfo.Id;
                }
            }

            return null;
        }
    }

    public class DocumentSizeRangeInfo
    {
        public int Id { get; set; }

        public long GenerateEqual { get; set; }

        public long LessThan { get; set; }
    }
}
