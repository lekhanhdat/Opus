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
using RAFileSystem.FileSystem.Discovery.Tags.Condition;
using RAFileSystem.FileSystem.Discovery.Tags.Contract;
using System;
using System.Collections.Generic;
using System.IO;

namespace RAFileSystem.FileSystem.Discovery.Tags.Analyzer
{
    public class FileShareDocumentTagAnalyzer : FileShareTagAnalyzer
    {
        public override AnalyseMethod AnalyseMethod => AnalyseMethod.FileShareDocument;

        public FileShareDocumentTagAnalyzer(List<CriteriaInfo> criteriaInfoes) : base(criteriaInfoes)
        {
        }

        public override object Analyse(FileInfo document)
        {
            for (var index = 0; index < _criteriaInfoes.Count; index++)
            {
                var currentCriteriaInfo = _criteriaInfoes[index];
                var nextLogic = CriteriaLogicType.None;
                if (index + 1 < _criteriaInfoes.Count)
                {
                    nextLogic = _criteriaInfoes[index + 1].LogicType;
                }

                var criteriaType = (DocumentCriteriaType)currentCriteriaInfo.CriteriaType;
                var value = GetValue(criteriaType, document);
                var conditionHandler = ConditionHandler.Get(currentCriteriaInfo.ConditionInfo.Category);
                var isMatch = conditionHandler.Handle(currentCriteriaInfo.ConditionInfo, value);
                if (isMatch && (nextLogic == CriteriaLogicType.Or || nextLogic == CriteriaLogicType.None))
                {
                    return document.Length;
                }

                if (isMatch && nextLogic == CriteriaLogicType.And || !isMatch && nextLogic == CriteriaLogicType.Or)
                {
                    continue;
                }

                return null;
            }
            return null;
        }

        private object GetValue(DocumentCriteriaType criteriaType, FileInfo document)
        {
            switch (criteriaType)
            {
                case DocumentCriteriaType.ModifiedTime:
                    return document.LastWriteTimeUtc;
                case DocumentCriteriaType.CreatedTime:
                    return document.CreationTimeUtc;
                case DocumentCriteriaType.DocumentType:
                    return Path.GetExtension(document.Name).Replace(".", "");
                case DocumentCriteriaType.ParentFolder:
                    return document.Directory.Name;
                case DocumentCriteriaType.Name:
                    return document.Name;
                case DocumentCriteriaType.DocumentSize:
                    return document.Length;
                default:
                    throw new NotSupportedException($"The analyse [{AnalyseMethod}] not supports [{criteriaType}].");
            }
        }
    }
}
