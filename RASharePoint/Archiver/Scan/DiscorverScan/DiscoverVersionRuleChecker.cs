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
using DataOrchestration.Tag.Sdk;
using DataOrchestration.Tag.Sdk.Service.CloudRecords.Analyzer;
using DataOrchestration.Tag.Sdk.Service.CloudRecords.Condition;
using DataOrchestration.Tag.Sdk.Service.CloudRecords.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataOrchestration.Tag.Sdk.Service.CloudRecords.Analyzer.Office365;

namespace AvePoint.RA.SharePoint.Archiver.Scan.DiscorverScan
{
    public class DiscoverVersionRuleChecker : Office365TagAnalyzer
    {
        public override AnalyseMethod AnalyseMethod => AnalyseMethod.Version;

        public DiscoverVersionRuleChecker(List<CriteriaInfo> criteriaInfoes) : base(criteriaInfoes)
        {
        }

        public override object Analyse(SPDocument document)
        {
            var matchedVersions = new List<string>();
            if (document.Versions == null || !document.Versions.Any())
            {
                return null;
            }
            foreach (var documentVersion in document.Versions)
            {
                for (var index = 0; index < _criteriaInfoes.Count; index++)
                {
                    var currentCriteriaInfo = _criteriaInfoes[index];
                    var nextLogic = CriteriaLogicType.None;
                    if (index + 1 < _criteriaInfoes.Count)
                    {
                        nextLogic = _criteriaInfoes[index + 1].LogicType;
                    }

                    var criteriaType = (VersionCriteriaType)currentCriteriaInfo.CriteriaType;
                    var value = GetValue(criteriaType, document, documentVersion);
                    var conditionHandler = ConditionHandler.Get(currentCriteriaInfo.ConditionInfo.Category);
                    var isMatch = conditionHandler.Handle(currentCriteriaInfo.ConditionInfo, value);
                    if (isMatch && (nextLogic == CriteriaLogicType.Or || nextLogic == CriteriaLogicType.None))
                    {
                        matchedVersions.Add(documentVersion.Version);
                        break;
                    }

                    if ((isMatch && nextLogic == CriteriaLogicType.And) || (!isMatch && nextLogic == CriteriaLogicType.Or))
                    {
                        continue;
                    }

                    break;
                }
            }

            return matchedVersions;
        }

        private object GetValue(VersionCriteriaType criteriaType, SPDocument document, SPDocumentVersion documentVersion) =>
            criteriaType switch
            {
                VersionCriteriaType.ModifiedTime => documentVersion.ModifiedTime,
                VersionCriteriaType.DocumentType => document.FileExtension,
                VersionCriteriaType.DocumentSize => documentVersion.VersionSize,
                VersionCriteriaType.KeepLastVersions => new VersionConditionDataObject
                {
                    Versions = document.Versions.Where(item => item.Version != document.CurrentVersion)
                .OrderByDescending(item => int.Parse(item.Version.Replace(".", ""))).ToList(),
                    NeedCheckVersion = documentVersion,
                },
                _ => throw new NotSupportedException($"The analyse [{AnalyseMethod}] not supports [{criteriaType}].")
            };
    }
}
