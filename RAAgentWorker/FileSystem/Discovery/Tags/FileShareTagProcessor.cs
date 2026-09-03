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
using RAFileSystem.FileSystem.Discovery.Tags.Analyzer;
using RAFileSystem.FileSystem.Discovery.Tags.Analyzer.BuildIn;
using RAFileSystem.FileSystem.Discovery.Tags.Contract;
using System;
using System.IO;

namespace RAFileSystem.FileSystem.Discovery.Tags
{
    public class FileShareTagProcessor
    {
        public Object GetDocumentTagValue(string tagDefinition, FileInfo document)
        {
            var tagInfo = JsonConvert.DeserializeObject<TagInfo>(tagDefinition);
            if (!tagInfo.IsBuildIn)
            {
                var ruleInfo = JsonConvert.DeserializeObject<RuleInfo>(tagInfo.TagDefinition, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
                return new FileShareDocumentTagAnalyzer(ruleInfo.CriteriaInfoes).Analyse(document);
            }
            else
            {
                var buildInRuleInfo = JsonConvert.DeserializeObject<BuildInRuleInfo>(tagInfo.TagDefinition, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
                switch (buildInRuleInfo.RuleType)
                {
                    case BuildInRuleType.DocumentSizeRange:
                        return new FileShareDocumentSizeRangeTagAnalyzer(buildInRuleInfo).Analyse(document);
                    case BuildInRuleType.DocumentWithoutModifiedIn:
                        return new FileShareDocumentWithoutModifiedInTagAnalyzer(buildInRuleInfo).Analyse(document);
                    case BuildInRuleType.ROTRuleContainer:
                        return new FileShareBuildInRotRuleContainerTagAnalyzer(buildInRuleInfo).Analyse(document);
                    default:
                        throw new NotSupportedException($"The [{buildInRuleInfo.RuleType}] does not support analyse.");
                }
            }
        }
    }
}
