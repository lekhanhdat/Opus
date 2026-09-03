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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RAFileSystem.FileSystem.Discovery.Tags.Analyzer.BuildIn
{
    internal class FileShareBuildInRotRuleContainerTagAnalyzer : FileShareBuildInTagAnalyzer
    {
        internal FileShareBuildInRotRuleContainerTagAnalyzer(BuildInRuleInfo ruleInfo) : base(ruleInfo)
        {
        }

        internal override BuildInRuleType RuleType => BuildInRuleType.ROTRuleContainer;

        internal override object Analyse(FileInfo document)
        {
            var rules = JsonConvert.DeserializeObject<List<RuleInfo>>(_ruleInfo.AdditionalInformation, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            var documentRules = rules.Where(item => item.Method == AnalyseMethod.FileShareDocument).ToList();
            foreach (var documentRule in documentRules)
            {
                var analyzer = new FileShareDocumentTagAnalyzer(documentRule.CriteriaInfoes);
                var obj = analyzer.Analyse(document);
                if (obj != null)
                {
                    var size = Convert.ToInt64(obj);
                    return size;
                }
            }

            return null;
        }
    }
}
