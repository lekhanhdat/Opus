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
    internal class FileShareDocumentWithoutModifiedInTagAnalyzer : FileShareBuildInTagAnalyzer
    {
        internal override BuildInRuleType RuleType => BuildInRuleType.DocumentWithoutModifiedIn;

        internal FileShareDocumentWithoutModifiedInTagAnalyzer(BuildInRuleInfo ruleInfo) : base(ruleInfo)
        {
        }

        internal override object Analyse(FileInfo document)
        {
            var dateList = JsonConvert.DeserializeObject<List<DocumentWithoutModifiedInDate>>(_ruleInfo.AdditionalInformation, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }).ConvertAll(item =>
            {
                var multiple = item.UnitType == DocumentWithoutModifiedUnitType.Month ? 1 : 12;
                return (item.Id, Months: item.Unit * multiple);
            }).OrderBy(item => item.Months).ToList();
            dateList.Insert(0, (-1, 0));
            dateList.Add((999, int.MaxValue));
            var withoutModifiedMonths = (DateTime.UtcNow.Year - document.LastWriteTimeUtc.Year) * 12 + DateTime.UtcNow.Month - document.LastWriteTimeUtc.Month;
            for (var i = 1; i < dateList.Count; i++)
            {
                var beforeDate = dateList[i - 1];
                var currentDate = dateList[i];
                if (withoutModifiedMonths > beforeDate.Months && withoutModifiedMonths <= currentDate.Months)
                {
                    return currentDate.Id;
                }
            }

            return -1;
        }
    }

    public class DocumentWithoutModifiedInDate
    {
        public int Id { get; set; }

        public int Unit { get; set; }

        public DocumentWithoutModifiedUnitType UnitType { get; set; }
    }

    public enum DocumentWithoutModifiedUnitType
    {
        None = 0,
        Month = 1,
        Year = 2
    }
}
