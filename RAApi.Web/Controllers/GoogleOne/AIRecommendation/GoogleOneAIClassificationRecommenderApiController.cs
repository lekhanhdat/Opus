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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.TaxonomyModel.GoogleOne;
using AvePoint.RA.I18N.Core;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Common.Util;
using System.IO;
using System.Linq;
using AvePoint.RA.Contract.TaxonomyModel;
using Newtonsoft.Json;
using AvePoint.RA.DB.SecurityTrimming;
using Microsoft.AspNetCore.Http;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.SharePoint.Discover;
using Microsoft.AspNetCore.StaticFiles;

namespace AvePoint.RA.Api.Web.Controllers.GoogleOne.AIRecommendation
{
    [Route("api/googleone/airecommendation/classifications")]
    public class GoogleOneAIClassificationRecommenderApiController : GoogleOneApiBaseController
    {
        private readonly IRALogger _logger = RALogger.GetInstance(typeof(GoogleOneAIClassificationRecommenderApiController));

        private readonly ITaxonomyService _taxonomyService = PlatformWindsorManager.GetService<ITaxonomyService>();
        private readonly IRMSecurityTrimmingHelper _securityTrimmingHelper = PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();
        private readonly IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();


        [HttpPost("generate")]
        public async Task<RAReturnMessage> GenerateClassificationRecommender(GoogleOneAIRecommendation recommendation)
        {
            if (string.IsNullOrEmpty(recommendation.OpusAIRecommendationInfo.Industry))
            {
                return new RAReturnMessage()
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Illegal Parameter"
                };
            }
            if (!string.IsNullOrEmpty(recommendation.OpusAIRecommendationInfo.Requirement) && recommendation.OpusAIRecommendationInfo.Requirement.Length > 20000)
            {
                return new RAReturnMessage()
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_TM_AI_Recommendations_RequirementValidate")
                };
            }
            var content = new List<string[]>();
            if (recommendation.FileTemplate != null)
            {
                if (recommendation.FileTemplate.FileBytes is { Length: > 0 })
                {
                    using var stream = new MemoryStream(recommendation.FileTemplate.FileBytes);
                    Dictionary<string, int> sheetNameCountDic = new ()
                    {
                        { "Terms", 6 }
                    };

                    var fileContent = ExcelUtil.ReadExcel(stream, sheetNameCountDic);
                    if (fileContent.TryGetValue("Terms", out var termContent))
                    {
                        if (termContent.Count > 0)
                        {
                            content = termContent;
                        }
                        else
                        {
                            return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_TM_AI_Recommendations_Template_Content") };
                        }
                    }
                    else
                    {
                        if (fileContent.Count > 0)
                        {
                            content = fileContent.First().Value;
                        }
                    }
                }

            }
            recommendation.OpusAIRecommendationInfo.FileContent = content;
            var result =  await _taxonomyService.AIRecomendationAsync(recommendation.OpusAIRecommendationInfo);
            result.Extsion1 = JsonConvert.SerializeObject(result.Extsion1);
            return result;
        }

        [HttpPost("export")]
        public async Task<GoogleOneFileAIClassification> ExportClassificationAIRecommendation(GoogleOneExportClassificationAIRecommendation classificationAIRecommendation)
        {
            try
            {
                using var memoryStream = await _taxonomyService.GetStreamAIRecommendation(classificationAIRecommendation.Industry , JsonConvert.DeserializeObject<List<RecordCategory>>(classificationAIRecommendation.RecordCategories), true);
                memoryStream.Position = 0;
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                var exportFile = new GoogleOneFileAIClassification
                {
                    FileName = "AI Recommendation.xlsx",
                    ContentType = contentType,
                    FileBytes = memoryStream.ToArray()
                };
                return exportFile;
            }
            catch(Exception ex)
            {
                throw new Exception("Error while exporting recommendation", ex);
            }
        }

        [HttpPost("import")]
        public async Task<string> ImportData(GoogleOneFileAIClassification fileClassification)
        {
            string jobId = "";
            try
            {
                _logger.Info("tm import file,file name :{0}", fileClassification.FileName);
                var formFile = ConvertBytesToIFormFile(fileClassification);
                CheckFile(formFile);

                string extension = fileClassification.FileName.Substring(fileClassification.FileName.LastIndexOf(".") + 1);
                DateTime dt = DateTime.Now;
                string fileName = "Terms_" + dt.Ticks + ".csv";
                if (extension.Equals("xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    fileName = "TermsAndRules_" + dt.Ticks + ".xlsx";
                }
                else if (extension.Equals("xml", StringComparison.OrdinalIgnoreCase))
                {
                    fileName = "Terms_" + dt.Ticks + ".xml";
                }
                var blobName = Path.Combine(JobReportUtility.GetTenantIdentity(), JobReportUtility.ImportCSVFile, fileName);
                RAStorageUtil.UploadReportBlob(blobName, formFile.OpenReadStream());
                _logger.Info("save file success.");
                jobId = _taxonomyService.RunImportTermStructure(JobRunBy.Control, extension, blobName, isControlPlus: true);
            }
            catch (Exception ex)
            {
                _logger.Error("error occurred import data:{0}", ex);
            }
            return jobId;
        }

        [HttpGet("import/downloadtemplate")]
        public async Task<GoogleOneFileAIClassification> DownloadTemplateImport()
        {
            try
            {
                string filepath = _taxonomyService.GetTemplateFilePath();
                var name = System.IO.Path.GetFileName(filepath);
                var memoryStream = new MemoryStream();
                await using var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
                await stream.CopyToAsync(memoryStream);

                memoryStream.Position = 0;
                
                var fileInfo = new GoogleOneFileAIClassification
                {
                    FileName = name,
                    ContentType = GetContentType(filepath),
                    FileBytes = memoryStream.ToArray()
                };
                return fileInfo;
            }
            catch (Exception e)
            {
                _logger.Error($"Fail download term and rule template,ex:{e}");
                return null;
            }
        }

        [HttpGet("feature/enabled")]
        public Task<bool> IsEnableAIRecommendationFeature()
        {
            return Task.Run(() => _keyValueDao.EnableAIRecommendationFeature());
        }

        private MemoryStream EditTemplateForJpmc(MemoryStream memoryStream)
        {
            var isEnableJPMC = _keyValueDao.GetValueByKey("JPMC_Customization") != null;
            ExportAddition exportAddition;
            exportAddition = new ExportAddition();
            exportAddition.TermColumArray = GetAdditionTermColumnArray(exportAddition, isEnableJPMC);
            exportAddition.RuleColumArray = new string[] { JPMCTemplateColumn.ADDITION_RULE_COL };
            exportAddition.ConditionArray = !isEnableJPMC ? new string[] { } : new string[] { JPMCTemplateColumn.ADDITION_CONTITION };
            var content = ExcelUtil.ReadExcelWithHeader(memoryStream);
            var termContent = content["Terms"];
            var ruleContent = content["Rules"];
            #region 更改Term
            try
            {
                if (termContent[0][TermPropertyIndex.TimeZone] != null && termContent[0][TermPropertyIndex.TimeZone + 1] == "Notes")
                {
                    for (int i = 1; i < termContent.Count; i++)
                    {
                        List<string> termItem = new List<string>(termContent[i]);
                        for (int j = 0; j < exportAddition.TermColumArray.Length - 1; j++)
                        {
                            termItem.Insert(TermPropertyIndex.TimeZone + j + 1, "");
                        }
                        termContent[i] = termItem.ToArray();
                    }
                    termContent.RemoveAt(0);
                }
                else
                {
                    _logger.Error("JPMC-Column in template is occupied");
                    throw new Exception("JPMC- Column in template is occupied");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("JPMC - Edit Term Failed", ex);
                throw;
            }
            #endregion
            #region 更改Rule
            ruleContent.RemoveAt(0);
            #endregion
            #region 创建新模板
            var newStream = new MemoryStream();
            string tempFilePath = null;
            try
            {
                var tempFolderPath = Path.Combine(WebUtil.GetInstallPath(), "Temp", "Config");
                if (!Directory.Exists(tempFolderPath))
                {
                    _logger.Info("Temp path not find Create Path");
                    Directory.CreateDirectory(tempFolderPath);
                }
                var fileName = $"Temp excel for download{Guid.NewGuid().ToString("N")}.xlsx";
                tempFilePath = Path.Combine(tempFolderPath, fileName);
                ReportUtil.CreateTermsAndRulesSheets(tempFilePath, ruleContent, termContent, exportAddition);


                using (var stream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
                {
                    stream.CopyTo(newStream);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("JPMC - Create new Template Failed", ex);
                throw;
            }
            finally
            {
                try
                {
                    memoryStream.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.Warn("Dispose memory stream Failed", ex);
                }
                try
                {
                    if (!string.IsNullOrEmpty(tempFilePath))
                    {
                        if (System.IO.File.Exists(tempFilePath))
                        {
                            System.IO.File.Delete(tempFilePath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn("Dispose temp file path Failed");
                }

            }
            #endregion
            return newStream;
        }

        private string[] GetAdditionTermColumnArray(ExportAddition exportAddition, bool isEnableJPMC)
        {
            List<string> result = new();
            if (isEnableJPMC) result.Add("RM_TM_AdvanceSetting");
            result.Add("Notes");
            return result.ToArray();
        }
        private void CheckFile(IFormFile file)
        {
            string extension = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
            var allowFileExts = new List<FileExtension> { FileExtension.CSV, FileExtension.XLSX, FileExtension.XML };
            WebUtil.CheckFileExtension(extension, allowFileExts);
        }
        private string GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;

            if (!provider.TryGetContentType(path, out contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }
        public IFormFile ConvertBytesToIFormFile(GoogleOneFileAIClassification fileAIClassification)
        {
            var stream = new MemoryStream(fileAIClassification.FileBytes);
            var formFile = new FormFile(stream, 0, fileAIClassification.FileBytes.Length, "file", fileAIClassification.FileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = fileAIClassification.ContentType
            };
            return formFile;
        }
    }
}
