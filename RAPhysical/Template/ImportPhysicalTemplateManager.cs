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
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.TemplateManagement;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.Template
{
    public class ImportPhysicalTemplateManager
    {

        private static readonly IRMReportManager ReportManager = ReportMangerFactory.Instance.ReportManager;

        private static bool HasSucceedDetail { get; set; }

        private static bool HasFailedDetail { get; set; }

        private static bool IsGlobleUniqueIDSetting { get; set; }

        public static void Init(string jobId, bool isGlobleUniqueIDSetting)
        {
            ReportMangerFactory.Instance.Init(jobId, JobType.PhysicalTemplateImport);
            ReportManager.StartUpdateJobProgress();
            IsGlobleUniqueIDSetting = isGlobleUniqueIDSetting;
        }

        public static void SetJobFinished()
        {
            var jobFinishStatus = HasSucceedDetail && HasFailedDetail ?
                JobStatus.FinishWithException :
                (
                    HasFailedDetail ?
                    JobStatus.Failed :
                    JobStatus.Finished
                );
            ReportManager.SetJobFinished(jobFinishStatus);
        }

        public static void AddSuccessdDetail(SimplifySuiteDto suiteInfo ,TemplateDto templateDto)
        {
            ReportManager.SendJobDetail(new JMPhysicalTemplateImportJobDetail
            {
                TemplateSuiteName = suiteInfo.Name,
                TemplateSuiteStartFrom = suiteInfo.StartFrom == SuiteStartFromType.Box ? "RM_PRM_TM_Suite_StartFromType_Box" : "RM_PRM_TM_Suite_StartFromType_Folder",
                TemplateName = templateDto.name,
                TemplateType = GetPhysicalType(templateDto.type),
                TemplatePrefix = IsGlobleUniqueIDSetting ? string.Empty : templateDto.prefix,
                TemplateDigits = IsGlobleUniqueIDSetting ? string.Empty : templateDto.numberOfDigits.ToString(),
                Status = JobDetailsStatus.Successful,
            });
            HasSucceedDetail = true;
        }
        public static void AddSuiteFailedDetail(string suiteName, SuiteStartFromType suiteStartFrom , string comment)
        {
            ReportManager.SendJobDetail(new JMPhysicalTemplateImportJobDetail
            {
                TemplateSuiteName = suiteName,
                TemplateSuiteStartFrom = GetSuiteStartFromI18N(suiteStartFrom),
                Comment = comment,
                Status = JobDetailsStatus.Failed,
            });
            HasFailedDetail = true;
        }

        public static void AddFailedDetail(SimplifySuiteDto suiteInfo, TemplateDto templateDto, string comment)
        {
            ReportManager.SendJobDetail(new JMPhysicalTemplateImportJobDetail
            {
                TemplateSuiteName = suiteInfo.Name,
                TemplateSuiteStartFrom = GetSuiteStartFromI18N(suiteInfo.StartFrom),
                TemplateName = templateDto.name,
                TemplateType = GetPhysicalType(templateDto.type),
                TemplatePrefix = IsGlobleUniqueIDSetting ? string.Empty : templateDto.prefix,
                TemplateDigits = IsGlobleUniqueIDSetting ? string.Empty : templateDto.numberOfDigits.ToString(),
                Comment = comment,
                Status = JobDetailsStatus.Failed,
            });
            HasFailedDetail = true;
        }

        public static void AddFailedDetail(StructrueObejct templateInfo, TemplateType templateType, string comment)
        {
            ReportManager.SendJobDetail(new JMPhysicalTemplateImportJobDetail
            {
                TemplateSuiteName = templateInfo.SuiteName,
                TemplateSuiteStartFrom = GetSuiteStartFromI18N(GetSuiteStartFrom(templateInfo.StartFrom)),
                TemplateName = GetTemplateName(templateInfo, templateType),
                TemplateType = GetPhysicalType(templateType),
                TemplatePrefix = IsGlobleUniqueIDSetting ? string.Empty : templateInfo.UniqueIDPrefix,
                TemplateDigits = IsGlobleUniqueIDSetting ? string.Empty : templateInfo.UniqueIDDigits,
                Comment = comment,
                Status = JobDetailsStatus.Failed,
            });
            HasFailedDetail = true;
        }

        public static void AddFailedDetail(string templateSuiteName, string templateName, TemplateType templateType, SuiteStartFromType startFrom, string comment)
        {
            ReportManager.SendJobDetail(new JMPhysicalTemplateImportJobDetail
            {
                TemplateSuiteName = templateSuiteName,
                TemplateSuiteStartFrom = GetSuiteStartFromI18N(startFrom),
                TemplateName = templateName,
                TemplateType = GetPhysicalType(templateType),
                Comment = comment,
                Status = JobDetailsStatus.Failed,
            });
            HasFailedDetail = true;
        }

        public static void AddSkippedDetail(StructrueObejct templateInfo, TemplateType templateType, string comment)
        {
            ReportManager.SendJobDetail(new JMPhysicalTemplateImportJobDetail
            {
                TemplateSuiteName = templateInfo.SuiteName,
                TemplateSuiteStartFrom = GetSuiteStartFromI18N(GetSuiteStartFrom(templateInfo.StartFrom)),
                TemplateName = GetTemplateName(templateInfo, templateType),
                TemplateType = GetPhysicalType(templateType),
                TemplatePrefix = IsGlobleUniqueIDSetting ? string.Empty : templateInfo.UniqueIDPrefix,
                TemplateDigits = IsGlobleUniqueIDSetting ? string.Empty : templateInfo.UniqueIDDigits,
                Comment = comment,
                Status = JobDetailsStatus.Skipped,
            });
            HasSucceedDetail = true;
        }

        public static void SetJobFailed(string comment)
        {
            ReportManager.SetJobFinished(JobStatus.Failed, comment);
        }

        private static string GetSuiteStartFromI18N(SuiteStartFromType startFrom)
        {
            return startFrom switch
            {
                SuiteStartFromType.Box => "RM_PRM_TM_Suite_StartFromType_Box",
                SuiteStartFromType.Folder => "RM_PRM_TM_Suite_StartFromType_Folder",
                _ => "",
            };
        }

        private static SuiteStartFromType GetSuiteStartFrom(string startFrom)
        {
            return startFrom switch
            {
                "Box" => SuiteStartFromType.Box,
                "Folder" => SuiteStartFromType.Folder,
                _ => SuiteStartFromType.None,
            };
        }

        private static string GetTemplateName(StructrueObejct templateInfo, TemplateType templateType)
        {
            return templateType switch
            {
                TemplateType.Box => templateInfo.BoxTemplateName,
                TemplateType.Folder => templateInfo.FolderTemplateName,
                TemplateType.Records => templateInfo.RecordTemplateName,
                _ => "",
            };
        }

        private static string GetPhysicalType(TemplateType templateType)
        {
            return templateType switch
            {
                TemplateType.Box => "RM_Phy_TemplateType_Box",
                TemplateType.Folder => "RM_Phy_TemplateType_Folder",
                TemplateType.Records => "RM_Phy_TemplateType_Record",
                _ => "",
            };
        }
    }

    [Serializable]
    public class PrefixDuplicateException : Exception
    {
        public PrefixDuplicateException() { }
    }

    [Serializable]
    public class ColumnTypeException : Exception
    {
        public ColumnTypeException() { }
    }

    [Serializable]
    public class ColumnEmptyException : Exception
    {
        public ColumnEmptyException() { }
    }

    [Serializable]
    public class ColumnDuplicateException : Exception
    {
        public ColumnDuplicateException() { }
    }

    [Serializable]
    public class TemplateTypeException : Exception
    {
        public TemplateTypeException() { }
    }

    [Serializable]
    public class BuildColumnException : Exception
    {
        public BuildColumnException() { }
    }

    [Serializable]
    public class BuildColumnOptionException : Exception
    {
        public BuildColumnOptionException() { }
    }

    [Serializable]
    public class SameNameDifferentTypeException : Exception
    {
        public SameNameDifferentTypeException() { }
    }

    [Serializable]
    public class StartFromAddExsitingTemplateException : Exception
    {
        public StartFromAddExsitingTemplateException() { }
    }

    [Serializable]
    public class UniqueIdPrefixException : Exception
    {
        public UniqueIdPrefixException() { }
    }

    [Serializable]
    public class UniqueIdDigitsException : Exception
    {
        public UniqueIdDigitsException() { }
    }

    [Serializable]
    public class StartFromTypeException : Exception
    {
        public StartFromTypeException() 
        {
        }
    }

}
