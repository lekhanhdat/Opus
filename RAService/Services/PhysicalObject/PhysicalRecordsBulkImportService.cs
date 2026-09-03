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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.PhysicalObject
{
    public class PhysicalRecordsBulkImportService : RMServiceBase, IPhysicalRecordsBulkImportService
    {

        private RALogger logger = RALogger.GetInstance(typeof(PhysicalRecordsBulkImportService));

        public IRMSuiteDao RMSuiteDao => PlatformWindsorManager.GetService<IRMSuiteDao>();

        public ITemplateManagementService TemplateManagementService => PlatformWindsorManager.GetService<ITemplateManagementService>();

        public IRMTemplateRelationshipDao RMTemplateRelationshipDao => PlatformWindsorManager.GetService<IRMTemplateRelationshipDao>();

        public async Task<string> DownloadTemplateForImportAsync(Guid suiteId)
        {
            logger.Info("start to download template of suite {0}", suiteId);
            RMSuite suite = RMSuiteDao.Find(a => a.UniqueId == suiteId);
            List<int> templateIds = this.AssembleOrderedTemplateInSuite(suiteId);
            Dictionary<string, List<string[]>> dic = await AssembleSheetDataAsync(templateIds);
            return CreateExcel(dic);
        }

        private async Task<Dictionary<string, List<string[]>>> AssembleSheetDataAsync(List<int> templateIds)
        {
            Dictionary<string, List<string[]>> dic = new Dictionary<string, List<string[]>>();
            foreach (int id in templateIds)
            {
                TemplateDto template = await TemplateManagementService.LoadTemplateDtoAsync(id); 

                if (template != null && template.categories != null && template.categories.Count > 0)
                {
                    List<string> headers = new List<string>();
                    //新增Sheet， 按Teplate生成Excel Header
                    foreach(TemplateCategoryDto categoryDto in template.categories)
                    {
                        foreach(TemplateColumnDto column in categoryDto.columns)
                        {
                            if(column.uniqueId.ToString().ToLower() == Contract.TemplateManagement.DefaultColumnIDs.LoanedBy)
                            {
                                logger.Info("Not supported loan by yet");
                                continue;
                            }

                            string columnName = column.columnName;

                            if (!string.IsNullOrWhiteSpace(columnName))
                            {
                                string trimmed = columnName.TrimStart();

                                bool isSafeFirstChar = char.IsLetterOrDigit(trimmed[0]);
                                if (!isSafeFirstChar)
                                {
                                    columnName = "'" + columnName;
                                }
                            }

                            headers.Add(I18N.Core.I18NEntity.GetString(columnName));
                        }
                    }
                    headers.Insert(0, "Unique ID");
                    if(template.type == Contract.TemplateManagement.TemplateType.Custom)
                    {
                        headers.Add(I18N.Core.I18NEntity.GetString("RM_Template_Column_Name_HomeLocation"));
                    }
                    headers.Add("Parent ID");
                    if (template.type != Contract.TemplateManagement.TemplateType.Custom)
                    {
                        headers.Add("Created time");
                    }
                    headers.Add("Modified time");
                    string[] header1 = new string[headers.Count];
                    header1[0] = I18N.Core.I18NEntity.GetString(template.name);
                    if (!dic.ContainsKey(template.name))
                    {
                        dic.Add(I18N.Core.I18NEntity.GetString(template.name), new List<string[]>() { header1, headers.ToArray() }); 
                    }
                }
            }
            return dic;
        }
        public List<int> AssembleOrderedTemplateInSuite(Guid suiteId)
        {
            List<int> result = new List<int>();
            List<string> allPath = RMTemplateRelationshipDao.GetAllPathBySuite(suiteId);
            //一个排序逻辑， 保证整合后的ID 列表对于 所有Suite下的Templat分支都是顺序的
            var orderList = allPath.OrderByDescending(a => a.Count(c=>c =='/'));
            foreach(string path in orderList)
            {
                string[] temp = path.Split('/'); 
                for(int i = 0; i<temp.Length; i++)
                {
                    if(i == 0)
                    {
                        continue;
                    }
                    int tempId; 
                    if (int.TryParse(temp[i], out tempId) && !result.Contains(tempId))
                    {
                        result.Add(tempId);
                    }
                }
            }
            return result;
        }
        public string CreateExcel(Dictionary<string, List<string[]>> dic)
        {
            string tenantGroupId = TenantLocalValue.LogonGroupId;
            string tempPath = JobReportUtility.GetDownloadPhysicalBulkImportTempleFolder(tenantGroupId);
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }
            string filePath = tempPath + "/" + "PhysicalRecordsImportTemplate" + DateTime.Now.ToString("yyyyMMddhhssmm") + GenerateRandomNumber(4) + ".xlsx";
            ExcelUtil.CreateExcel(filePath, dic);
            return filePath;
        }
        private string GenerateRandomNumber(int count)
        {
            /* Fortify Issue Type: Insecure Randomness 
            * Sink Details:  this class DownloadTemplateForImportAsync
            * Ignore Reason: random用于拼接excel名称,不涉及安全问题
            */
            Random ran = new Random((int)DateTime.Now.Ticks);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                sb.Append(ran.Next(0, 9)).ToString();
            }
            return sb.ToString();
        }
    }
}
