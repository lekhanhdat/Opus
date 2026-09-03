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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.GCommon.Utility;
using Storage;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.RAPhysical.API;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.DB.Core;
using Newtonsoft.Json;
using AvePoint.RA.Contract.RMWeb.Physical.ColumnValues;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Common.SystemSetting;
using Media.Common.ClassicStorageApi;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Contract.RMWeb.TemplateManagement.Barcode;

namespace AvePoint.RA.RAPhysical
{
    public class ExportBarcodeProcessor
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(ExportBarcodeProcessor));

        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        private IRMReportManager ReportManager => ReportMangerFactory.Instance.ReportManager;
        private IRMTemplateDao TemplateDao => PlatformWindsorManager.GetService<IRMTemplateDao>();
        private IExplorerService ExplorerService => PlatformWindsorManager.GetService<IExplorerService>();
        private IRMCustomBarcodeTemplateDao CustomBarcodeTemplateDao => PlatformWindsorManager.GetService<IRMCustomBarcodeTemplateDao>();
        private IRMCustomBarcodeTemplateSuiteDao CustomBarcodeTemplateSuiteDao => PlatformWindsorManager.GetService<IRMCustomBarcodeTemplateSuiteDao>();
        private static readonly IBarcodeTemplateService s_barcodeTemplateService = PlatformWindsorManager.GetService<IBarcodeTemplateService>();

        public ExportBarcodeProcessor(string jobId, ExportBarcodeDto exportBarcodeDto)
        {
            ReportMangerFactory.Instance.Init(jobId, AvePoint.RA.Contract.JobMonitor.JobType.PhysicalExportBarcode);
            ReportManager.Increase(1);
            ReportManager.StartUpdateJobProgress();
        }
        public async Task RunNowAsync(string jobId, ExportBarcodeDto exportBarcodeDto)
        {
            try
            {
                using (CheckJobStopScope stopScope = new CheckJobStopScope())
                {
                    logger.Info("Begin download barcode job for default template.");

                    DateTime nowTime = DateTime.UtcNow;
                    //string nowTimeStr = GeneralSettingService.ConvertTiksToDateTime(nowTime.Ticks, false).DataTime.ToString(AveDateTimeUtility.DATETYPE022);
                    string fileName = I18NEntity.GetString("RM_DAM_ExportBarcodesReport") + "_" + GetSelectNodeName(exportBarcodeDto) + "_" + jobId;
                    string folderPath = AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(JobReportUtility.GetDownloadBarcodeInfoReportTempleFolder("Temple"), fileName);
                    //Download now
                    await GenerateDownLoadReportDataInfoAsync(folderPath, fileName, exportBarcodeDto);
                    ReportManager.Increase();
                    logger.Info("Begin update to cloud export location.");
                    string zipPath = folderPath + ".zip";
                    AvePoint.GCommon.ZipUtil.ZipFolder(folderPath, folderPath + ".zip", Encoding.UTF8);
                    DAOAPIClientV1 Client1 = new DAOAPIClientV1();
                    if (string.IsNullOrEmpty(Client1.GetExportLocationbyId(exportBarcodeDto.ExportLocationId)))
                    {
                        logger.Warn("export location not found, location Id is {0}", exportBarcodeDto.ExportLocationId);
                        throw new Exception(string.Format(I18NEntity.GetString("RM_EL_NoExportLocation"), exportBarcodeDto.ExportLocationName));
                    }
                    //logger.Info("connString is {0}", connString);
                    using (FileStream fs = File.OpenRead(zipPath))
                    {
                        using (IXSystem system = XFactoryCommon.InstanceSystem(Client1.GetExportLocationbyId(exportBarcodeDto.ExportLocationId)))
                        {
                            system.Open();
                            system.CommitStream(fs, new StorageInfo() { HighName = "JobExport", LowName = fileName + ".zip", Length = fs.Length });
                        }
                    }
                }
                ReportManager.Increase();
                logger.Info("Finish export barcode job");
                ReportManager.SetJobFinished(JobStatus.Finished);
            }
            catch (JobStopException)
            {
                logger.Warn("the job has stopped.");
                ReportManager.SetJobFinished(JobStatus.Stopped);
            }
            catch (Exception ex)
            {
                logger.Error("job failed, error:{0}", ex.Message.ToString());
                ReportManager.SetJobFinished(JobStatus.Failed, ex.Message.ToString());
            }
        }

        public string GetSelectNodeName(ExportBarcodeDto exportBarcodeDto)
        {
            string selectNodeName = string.Empty;
            if (exportBarcodeDto.NodeType == RMNodeType.PhysicalNormalLocation || exportBarcodeDto.NodeType == RMNodeType.PhysicalBottomLocation)
            {
                PhysicalLocation location = new PhysicalLocation(exportBarcodeDto.NodeId);
                if (location.Name.Length > 50)
                {
                    selectNodeName = location.Name.Substring(0, 50);
                }
                else
                {
                    selectNodeName = location.Name;
                }
            }
            else if (exportBarcodeDto.NodeType == RMNodeType.PhyBox)
            {
                PhysicalBox box = new PhysicalBox(exportBarcodeDto.NodeId);
                if (box.Name.Length > 50)
                {
                    selectNodeName = box.Name.Substring(0, 50);
                }
                else
                {
                    selectNodeName = box.Name;
                }
            }
            return selectNodeName;
        }

        public async Task GenerateDownLoadReportDataInfoAsync(string folderPath, string fileName, ExportBarcodeDto exportBarcodeDto)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                var templateSuite = await CustomBarcodeTemplateSuiteDao.GetByUniqueIdAsync(exportBarcodeDto.SuiteId);
                if (templateSuite.IsDefault)
                {
                    await GetDownLoadReportDataInfoAsync(exportBarcodeDto, folderPath, fileName);
                }
                else
                {
                    await GetCustomDownLoadReportDataInfoAsync(exportBarcodeDto, folderPath, fileName, templateSuite);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in GenerateDownLoadReportDataInfo, message is :", ex.Message.ToString());
            }
        }

        public async Task GetDownLoadReportDataInfoAsync(ExportBarcodeDto exportBarcodeDto, string folderPath, string fileName)
        {
            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            int boxWidth = 0, boxHeight = 0, foldWidth = 0, foldHeight = 0;
            RMBarcodeTemplate boxBarTemplate = await CustomBarcodeTemplateDao.GetDefaultTemplateAsync(BarcodeTemplateType.Box);
            RMBarcodeTemplate foldBarTemplate = await CustomBarcodeTemplateDao.GetDefaultTemplateAsync(BarcodeTemplateType.Folder);
            try
            {
                if (boxBarTemplate != null && !boxBarTemplate.ImageColumnA.IsNullOrEmpty())
                {
                    var bi = BarcodeUtil.GetImageInfo(boxBarTemplate.ImageColumnA);
                    boxWidth = bi.Width;
                    boxHeight = bi.Height;
                }
                if (foldBarTemplate != null && !foldBarTemplate.ImageColumnA.IsNullOrEmpty())
                {
                    var bi = BarcodeUtil.GetImageInfo(foldBarTemplate.ImageColumnA);
                    foldWidth = bi.Width;
                    foldHeight = bi.Height;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in get bitmap size , message is :", ex.ToString());
            }

            logger.Info("Begin get download data.");
            // 只保留common逻辑，彻底移除重复实现
            if (exportBarcodeDto.NodeType == RMNodeType.PhysicalNormalLocation)
            {
                IPhysicalLocation location = new PhysicalLocation(exportBarcodeDto.NodeId);
                List<IPhysicalLocation> allSubBottomLocation = new List<IPhysicalLocation>();
                GetSubBottomLocation(location, allSubBottomLocation);
                var templatePath = AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(WebUtil.GetInstallPath(), "Config", "BarcodeTemplate", "BarcodeTemplate.docx");
                foreach (IPhysicalLocation bottomLocation in allSubBottomLocation)
                {
                    string reportFilePath = AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(folderPath, fileName + "_" + bottomLocation.Name + ".docx");
                    var models = BuildExportBarcodeDataModels(bottomLocation, boxBarTemplate, foldBarTemplate, gls);
                    CreateWordReport(templatePath, reportFilePath, models);
                    ReportManager.Increase();
                    ReportManager.SendJobDetail(new JMExportBarcodeJobDetail()
                    {
                        ObjectName = bottomLocation.Name,
                        FullPath = bottomLocation.DirPath,
                        ItemType = "RM_Common_ObjectLevel_PhysicalLocation",
                        Status = JobDetailsStatus.Successful,
                    });
                }
            }
            else if (exportBarcodeDto.NodeType == RMNodeType.PhysicalBottomLocation)
            {
                IPhysicalLocation bottomLocation = new PhysicalLocation(exportBarcodeDto.NodeId);
                var templatePath = AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(WebUtil.GetInstallPath(), "Config", "BarcodeTemplate", "BarcodeTemplate.docx");
                string reportFilePath = AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(folderPath, fileName + ".docx");
                try
                {
                    var models = BuildExportBarcodeDataModels(bottomLocation, boxBarTemplate, foldBarTemplate, gls);
                    CreateWordReport(templatePath, reportFilePath, models);
                    ReportManager.Increase();
                    ReportManager.SendJobDetail(new JMExportBarcodeJobDetail()
                    {
                        ObjectName = bottomLocation.Name,
                        FullPath = bottomLocation.DirPath,
                        ItemType = "RM_Common_ObjectLevel_PhysicalLocation",
                        Status = JobDetailsStatus.Successful,
                    });
                }
                catch (Exception ex)
                {
                    ReportManager.SendJobDetail(new JMExportBarcodeJobDetail()
                    {
                        ObjectName = bottomLocation.Name,
                        FullPath = bottomLocation.DirPath,
                        ItemType = "RM_Common_ObjectLevel_PhysicalLocation",
                        Status = JobDetailsStatus.Failed,
                        Comment = ex.Message,
                    });
                }
            }
            else if (exportBarcodeDto.NodeType == RMNodeType.PhyBox)
            {
                PhysicalBox box = new PhysicalBox(exportBarcodeDto.NodeId);
                var templatePath = AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(AppDomain.CurrentDomain.BaseDirectory + "Config/BarcodeTemplate/BarcodeTemplate.docx");
                string reportFilePath = AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(folderPath, fileName + ".docx");
                try
                {
                    var models = GetBoxExportValue(box, boxBarTemplate, foldBarTemplate, gls);
                    CreateWordReport(templatePath, reportFilePath, models);
                    ReportManager.SendJobDetail(new JMExportBarcodeJobDetail()
                    {
                        ObjectName = box.Name,
                        FullPath = box.DirPath,
                        ItemType = "RM_Common_ObjectLevel_PhysicalBox",
                        Status = JobDetailsStatus.Successful,
                    });
                }
                catch (Exception ex)
                {
                    ReportManager.SendJobDetail(new JMExportBarcodeJobDetail()
                    {
                        ObjectName = box.Name,
                        FullPath = box.DirPath,
                        ItemType = "RM_Common_ObjectLevel_PhysicalBox",
                        Status = JobDetailsStatus.Failed,
                        Comment = ex.Message,
                    });
                }
            }
        }
        /// <summary>
        /// 通用：根据location构建导出条码数据模型集合
        /// </summary>
        private List<ExportBarcodeDataModel> BuildExportBarcodeDataModels(IPhysicalLocation location, RMBarcodeTemplate boxBarTemplate, RMBarcodeTemplate foldBarTemplate, GeneralSettingModel gls)
        {
            var models = new List<ExportBarcodeDataModel>();
            var records = GetBoxesAndFoldOrderByDescending(location);
            if (records == null) return models;
            foreach (var r in records)
            {
                if (r.NodeType == (int)RMNodeLevel.PhysicalBox)
                {
                    var box = new PhysicalBox(r);
                    models.AddRange(GetBoxExportValue(box, boxBarTemplate, foldBarTemplate, gls));
                }
                else if (r.NodeType == (int)RMNodeLevel.PhysicalFile)
                {
                    var file = new PhysicalFile(r);
                    models.Add(GetFoldExportValue(file, foldBarTemplate, gls));
                }
            }
            return models;
        }

        /// <summary>
        /// 通用：拷贝模板并写入Word表格
        /// </summary>
        private void CreateWordReport(string templatePath, string reportFilePath, List<ExportBarcodeDataModel> models)
        {
            ReportWordUtil.CopyFile(templatePath, reportFilePath);
            using (ReportWordUtil utility = new ReportWordUtil(reportFilePath))
            {
                utility.CreateTable("Table", models);
            }
    }

        private async Task GetCustomDownLoadReportDataInfoAsync(ExportBarcodeDto exportBarcodeDto, string folderPath, string fileName, RMCustomBarcodeTemplateSuite suite)
        {
            if (exportBarcodeDto == null) { throw new ArgumentNullException(nameof(exportBarcodeDto)); }
            if (suite == null) { throw new ArgumentNullException(nameof(suite)); }

            var labelType = suite.LabelType;
            var customTemplateInfo = await s_barcodeTemplateService.GetBarcodeTemplateBySuiteIdAsync(exportBarcodeDto.SuiteId) as BarcodeCustomTemplateDto;
            var boxBarTemplate = customTemplateInfo?.Templates.FirstOrDefault(t => t.Type == BarcodeTemplateType.Box);
            var foldBarTemplate = customTemplateInfo?.Templates.FirstOrDefault(t => t.Type == BarcodeTemplateType.Folder);
            if (customTemplateInfo == null)
            {
                logger.Error("Custom template suite not found or invalid.");
                return;
            }

            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            // Local helper to build a label item from a physical object using a given content template
            LabelItem BuildLabelItem(PhysicalObjectDto node, RMTemplate contentTemplate)
            {
                var li = new LabelItem
                {
                    Barcode = string.IsNullOrEmpty(node.BarcodeId) ? node.UniqueId : node.BarcodeId,
                    Properties = new List<PropertyItem>()
                };

                var cfg = (node.NodeType == RMNodeType.PhyBox) ? boxBarTemplate : foldBarTemplate;
                if (cfg != null)
                {
                    // Logo
                    if (cfg.LogoProperties != null && !string.IsNullOrEmpty(cfg.LogoProperties.LogoImgBase64Str))
                    {
                        var isEnableLogo = !string.IsNullOrEmpty(cfg.LogoProperties.LogoImgBase64Str);
                        var imageBytes = new byte[0];
                        var width = 50;
                        var height = 50;

                        cfg.LogoProperties.LogoImgBase64Str = cfg.LogoProperties.LogoImgBase64Str[(cfg.LogoProperties.LogoImgBase64Str.IndexOf(",") + 1)..];
                        if (isEnableLogo)
                        {
                            imageBytes = Convert.FromBase64String(cfg.LogoProperties.LogoImgBase64Str);
                            var imageInfo = BarcodeUtil.GetImageInfo(imageBytes);
                            if (imageInfo != null)
                            {
                                width = imageInfo.Width > 0 ? imageInfo.Width : 50;
                                height = imageInfo.Height > 0 ? imageInfo.Height : 50;
                            }
                        }

                        li.Logo = new LogoItem
                        {
                            Enabled = isEnableLogo,
                            ImageBytes = imageBytes,
                            Mime = string.IsNullOrWhiteSpace(cfg.LogoProperties.LogoImgType) ? "image/png" : cfg.LogoProperties.LogoImgType,
                            FileName = string.IsNullOrWhiteSpace(cfg.LogoProperties.LogoImgName) ? "logo" : cfg.LogoProperties.LogoImgName,
                            Position = cfg.LogoProperties.Position,
                            Width = width,
                            Height = height
                        };
                    }

                    if (cfg.Properties != null && cfg.Properties.Count > 0)
                    {
                        foreach (var p in cfg.Properties)
                        {
                            string value = GetPropertyValueByName(node, contentTemplate, p.Name, gls);
                            if(string.IsNullOrEmpty(value))
                            {
                                continue;
                            }
                            var pi = new PropertyItem
                            {
                                Name = I18NEntity.GetString(p.Name),
                                Value = value ?? string.Empty,
                                Position = p.Position ,
                                FontSize = p.FontSize > 0 ? p.FontSize * 2 : (int?)null
                            };
                            li.Properties.Add(pi);
                        }
                    }
                }

                return li;
            }

            string vmlTemplatePath = GetTemplatePath(labelType);

            if (exportBarcodeDto.NodeType == RMNodeType.PhysicalNormalLocation)
            {
                IPhysicalLocation location = new PhysicalLocation(exportBarcodeDto.NodeId);
                List<IPhysicalLocation> allSubBottomLocation = new List<IPhysicalLocation>();
                GetSubBottomLocation(location, allSubBottomLocation);
                foreach (IPhysicalLocation bottomLocation in allSubBottomLocation)
                {
                    string reportFilePath = folderPath + Path.DirectorySeparatorChar + fileName + "_" + bottomLocation.Name + ".docx";
                    var records = GetBoxesAndFoldOrderByDescending(bottomLocation) ?? new List<Record>();
                    var labels = new List<LabelItem>();
                    foreach (var r in records)
                    {
                        if (r.NodeType == (int)RMNodeLevel.PhysicalBox)
                        {
                            PhysicalBox box = new PhysicalBox(r);
                            PhysicalObjectDto boxDto = ConvertUtil.ConvertRMBaseRecordToPhysical(box.Record);
                            boxDto.HomeLocationFullPath = ExplorerService.GetPhysicalObjectFullPath(boxDto.Id);
                            RMTemplate boxTemplate = TemplateDao.GetTemplateById(boxDto.TemplateId);
                            if (boxTemplate != null)
                                labels.Add(BuildLabelItem(boxDto, boxTemplate));
                        }
                        else if (r.NodeType == (int)RMNodeLevel.PhysicalFile)
                        {
                            PhysicalFile file = new PhysicalFile(r);
                            PhysicalObjectDto foldDto = ConvertUtil.ConvertRMBaseRecordToPhysical(file.Record);
                            foldDto.HomeLocationFullPath = ExplorerService.GetPhysicalObjectFullPath(foldDto.Id);
                            RMTemplate foldTemplate = TemplateDao.GetTemplateById(foldDto.TemplateId);
                            if (foldTemplate != null)
                                labels.Add(BuildLabelItem(foldDto, foldTemplate));
                        }
                    }
                    ReportWordUtil.CopyTemplateAndFillVml(vmlTemplatePath, reportFilePath, labels);
                }
            }
            else if (exportBarcodeDto.NodeType == RMNodeType.PhysicalBottomLocation)
            {
                string reportFilePath = folderPath + Path.DirectorySeparatorChar + fileName + ".docx";
                IPhysicalLocation bottomLocation = new PhysicalLocation(exportBarcodeDto.NodeId);
                var records = GetBoxesAndFoldOrderByDescending(bottomLocation) ?? new List<Record>();
                var labels = new List<LabelItem>();
                foreach (var r in records)
                {
                    if (r.NodeType == (int)RMNodeLevel.PhysicalBox)
                    {
                        PhysicalBox box = new PhysicalBox(r);
                        PhysicalObjectDto boxDto = ConvertUtil.ConvertRMBaseRecordToPhysical(box.Record);
                        boxDto.HomeLocationFullPath = ExplorerService.GetPhysicalObjectFullPath(boxDto.Id);
                        RMTemplate boxTemplate = TemplateDao.GetTemplateById(boxDto.TemplateId);
                        if (boxTemplate != null)
                        {
                            labels.Add(BuildLabelItem(boxDto, boxTemplate));
                        }

                        List<PhysicalFile> folds = box.GetFilesOrderByDescending(b => (b.RecordStatus == (int)RMRecordStatus.Active || b.RecordStatus == (int)RMRecordStatus.Closed || b.RecordStatus == (int)RMRecordStatus.Missing || b.RecordStatus == (int)RMRecordStatus.Destroyed));
                        var foldTemplateMap = new Dictionary<int, RMTemplate>();
                        foreach (var f in folds)
                        {
                            PhysicalObjectDto foldDto = ConvertUtil.ConvertRMBaseRecordToPhysical(f.Record);
                            foldDto.BoxTemplateId = box.TemplateId;
                            foldDto.HomeLocationFullPath = ExplorerService.GetPhysicalObjectFullPath(foldDto.Id);
                            if (!foldTemplateMap.ContainsKey(foldDto.TemplateId))
                            {
                                var ft = TemplateDao.GetTemplateById(foldDto.TemplateId);
                                if (ft != null)
                                {
                                    AddPushColumnToFoldTemplate(ft, boxTemplate);
                                    foldTemplateMap[foldDto.TemplateId] = ft;
                                }
                            }
                            if (foldTemplateMap.TryGetValue(foldDto.TemplateId, out var useTpl))
                            {
                                labels.Add(BuildLabelItem(foldDto, useTpl));
                            }
                        }
                    }
                    else if (r.NodeType == (int)RMNodeLevel.PhysicalFile)
                    {
                        PhysicalFile file = new PhysicalFile(r);
                        PhysicalObjectDto foldDto = ConvertUtil.ConvertRMBaseRecordToPhysical(file.Record);
                        foldDto.HomeLocationFullPath = ExplorerService.GetPhysicalObjectFullPath(foldDto.Id);
                        RMTemplate foldTemplate = TemplateDao.GetTemplateById(foldDto.TemplateId);
                        if (foldTemplate != null)
                        {
                            labels.Add(BuildLabelItem(foldDto, foldTemplate));
                        }
                    }
                }
                ReportWordUtil.CopyTemplateAndFillVml(vmlTemplatePath, reportFilePath, labels);
            }
            else if (exportBarcodeDto.NodeType == RMNodeType.PhyBox)
            {
                string reportFilePath = folderPath + Path.DirectorySeparatorChar + fileName + ".docx";
                PhysicalBox box = new PhysicalBox(exportBarcodeDto.NodeId);
                var labels = new List<LabelItem>();

                PhysicalObjectDto boxDto = ConvertUtil.ConvertRMBaseRecordToPhysical(box.Record);
                boxDto.HomeLocationFullPath = ExplorerService.GetPhysicalObjectFullPath(boxDto.Id);
                RMTemplate boxTemplate = TemplateDao.GetTemplateById(boxDto.TemplateId);
                if (boxTemplate != null)
                {
                    labels.Add(BuildLabelItem(boxDto, boxTemplate));
                }

                List<PhysicalFile> folds = box.GetFilesOrderByDescending(b => (b.RecordStatus == (int)RMRecordStatus.Active || b.RecordStatus == (int)RMRecordStatus.Closed || b.RecordStatus == (int)RMRecordStatus.Missing || b.RecordStatus == (int)RMRecordStatus.Destroyed));
                var foldTemplateMap = new Dictionary<int, RMTemplate>();
                foreach (var f in folds)
                {
                    PhysicalObjectDto foldDto = ConvertUtil.ConvertRMBaseRecordToPhysical(f.Record);
                    foldDto.BoxTemplateId = box.TemplateId;
                    foldDto.HomeLocationFullPath = ExplorerService.GetPhysicalObjectFullPath(foldDto.Id);
                    if (!foldTemplateMap.ContainsKey(foldDto.TemplateId))
                    {
                        var ft = TemplateDao.GetTemplateById(foldDto.TemplateId);
                        if (ft != null)
                        {
                            AddPushColumnToFoldTemplate(ft, boxTemplate);
                            foldTemplateMap[foldDto.TemplateId] = ft;
                        }
                    }
                    if (foldTemplateMap.TryGetValue(foldDto.TemplateId, out var useTpl))
                    {
                        labels.Add(BuildLabelItem(foldDto, useTpl));
                    }
                }

                ReportWordUtil.CopyTemplateAndFillVml(vmlTemplatePath, reportFilePath, labels);
            }
        }

        // Map property name value for a node based on template schema and general settings
        private string GetPropertyValueByName(PhysicalObjectDto node, RMTemplate template, string propName, GeneralSettingModel gls)
        {
            if (node == null || template == null || string.IsNullOrWhiteSpace(propName)) return string.Empty;

            // Built-in mappings
            if (propName == BuildInColumnIDs.RecordsId)
            {
                return node.UniqueId ?? string.Empty;
            }
            if (propName == BuildInColumnIDs.CreatedBy) return node.CreatedBy;
            if (propName == BuildInColumnIDs.CreatedTime) return DateTimeUtil.ConvertTimeFromUtc(node.CreateTime, gls).ToString();
            if (propName == BuildInColumnIDs.ModifiedBy) return node.ModifiedBy;
            if (propName == BuildInColumnIDs.ModifiedTime) return DateTimeUtil.ConvertTimeFromUtc(node.ModifiedTime, gls).ToString();

            // Meta columns by display name
            var schema = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(template.ColumnSchema);
            var column = schema?.Columns?.FirstOrDefault(c => string.Equals(c.Name, propName, StringComparison.OrdinalIgnoreCase));
            if (column != null)
            {
                var metaInfo = node.MetaInfo;
                if (metaInfo != null && metaInfo.ContainsKey(column.UniqueId.ToString()))
                {
                    return HandleMetaInfoColumn(column, node, gls);
                }
            }
            return string.Empty;
        }

        private static string GetTemplatePath(BarcodeTemplateLabelType labelType)
        {
            return labelType switch
            {
                BarcodeTemplateLabelType.Label_200x93 => Path.Combine(WebUtil.GetInstallPath(), "Config", "BarcodeTemplate", "Label_200x93-R_Word_Template.docx"),
                BarcodeTemplateLabelType.Label_135x95 => Path.Combine(WebUtil.GetInstallPath(), "Config", "BarcodeTemplate", "Label_135x95-R_Word_Template.docx"),
                BarcodeTemplateLabelType.Label_95x65 => Path.Combine(WebUtil.GetInstallPath(), "Config", "BarcodeTemplate", "Label_95x65-R_Word_Template.docx"),
                BarcodeTemplateLabelType.Label_99x67 => Path.Combine(WebUtil.GetInstallPath(), "Config", "BarcodeTemplate", "Label_99x67-R_Word_Template.docx"),
                BarcodeTemplateLabelType.Label_72x63 => Path.Combine(WebUtil.GetInstallPath(), "Config", "BarcodeTemplate", "Label_72x63-R_Word_Template.docx"),
                _ => throw new ArgumentException($"Unsupported label type: {labelType}"),
            };
        }

        public List<ExportBarcodeDataModel> GetBoxExportValue(PhysicalBox box, RMBarcodeTemplate boxBarTemplate, RMBarcodeTemplate foldBarTemplate, GeneralSettingModel gls)
        {
            List<ExportBarcodeDataModel> models = new List<ExportBarcodeDataModel>();
            //这里得判断 BarTemplate 如果为空会怎样

            List<int> foldTemplateIds = new List<int>();
            Dictionary<int, RMTemplate> idAndTemplate = new Dictionary<int, RMTemplate>();

            List<PhysicalObjectDto> objectList = new List<PhysicalObjectDto>();
            PhysicalObjectDto physicalBoxDto = ConvertUtil.ConvertRMBaseRecordToPhysical(box.Record);
            physicalBoxDto.HomeLocationFullPath = ExplorerService.GetPhysicalObjectFullPath(physicalBoxDto.Id);
            objectList.Add(physicalBoxDto);
            RMTemplate boxTemplate = TemplateDao.GetTemplateById(physicalBoxDto.TemplateId);
            if (boxTemplate == null)
            {
                logger.Error("Can't find box's template ,template id is {0}", box.TemplateId.ToString());
                return models;
            }

            List<PhysicalFile> folds = box.GetFilesOrderByDescending(b => (b.RecordStatus == (int)RMRecordStatus.Active || b.RecordStatus == (int)RMRecordStatus.Closed || b.RecordStatus == (int)RMRecordStatus.Missing || b.RecordStatus == (int)RMRecordStatus.Destroyed));
            foreach (PhysicalFile fold in folds)
            {
                PhysicalObjectDto foldObject = ConvertUtil.ConvertRMBaseRecordToPhysical(fold.Record);
                foldObject.BoxTemplateId = box.TemplateId;
                foldObject.HomeLocationFullPath = ExplorerService.GetPhysicalObjectFullPath(foldObject.Id);
                objectList.Add(foldObject);
                if (!foldTemplateIds.Contains(foldObject.TemplateId))
                {
                    foldTemplateIds.Add(foldObject.TemplateId);
                }
            }
            ExplorerService.AppendPushedColumns(objectList);

            foreach (int foldTemplateId in foldTemplateIds)
            {
                RMTemplate foldTemplate = TemplateDao.GetTemplateById(foldTemplateId);
                if (foldTemplate == null)
                {
                    logger.Error("Can't find fold's template ,template id is {0}", foldTemplateId.ToString());
                    continue;
                }
                AddPushColumnToFoldTemplate(foldTemplate, boxTemplate);
                idAndTemplate[foldTemplateId] = foldTemplate;
            }

            foreach (PhysicalObjectDto node in objectList)
            {
                if (node.NodeType == RMNodeType.PhyBox)
                {
                    ExportBarcodeDataModel model = GetColumnValue(node, boxTemplate, boxBarTemplate, gls);
                    model.Image = boxBarTemplate == null ? null : boxBarTemplate.ImageColumnA;
                    model.NodeType = RMNodeType.PhyBox;
                    models.Add(model);
                }
                else
                {
                    ExportBarcodeDataModel model = GetColumnValue(node, idAndTemplate[node.TemplateId], foldBarTemplate, gls);
                    model.Image = foldBarTemplate == null ? null : foldBarTemplate.ImageColumnA;
                    model.NodeType = RMNodeType.PhyFile;
                    models.Add(model);
                }
            }
            return models;
        }

        public ExportBarcodeDataModel GetFoldExportValue(PhysicalFile fold, RMBarcodeTemplate foldBarTemplate, GeneralSettingModel gls)
        {
            ExportBarcodeDataModel model = new ExportBarcodeDataModel();

            //List<PhysicalObjectDto> objectList = new List<PhysicalObjectDto>();
            PhysicalObjectDto physicalFoldDto = ConvertUtil.ConvertRMBaseRecordToPhysical(fold.Record);
            physicalFoldDto.HomeLocationFullPath = ExplorerService.GetPhysicalObjectFullPath(physicalFoldDto.Id);
            //objectList.Add(physicalFoldDto);
            RMTemplate foldTemplate = TemplateDao.GetTemplateById(physicalFoldDto.TemplateId);
            if (foldTemplate == null)
            {
                logger.Error("Can't find box's template ,template id is {0}", physicalFoldDto.TemplateId.ToString());
                return model;
            }
            model = GetColumnValue(physicalFoldDto, foldTemplate, foldBarTemplate, gls);
            model.Image = foldBarTemplate == null ? null : foldBarTemplate.ImageColumnA;
            model.NodeType = RMNodeType.PhyFile;
            return model;
        }
        public void AddPushColumnToFoldTemplate(RMTemplate foldTemplate, RMTemplate boxTemplate)
        {
            var schemaTemp = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(boxTemplate.ColumnSchema);
            foreach (ColumnXmlSchema column in schemaTemp.Columns)
            {
                if ((column.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) == (int)TemplateInheritSettingEnum.PushToChild)
                {
                    if (column.pushFoldTemplateCategoriesId == null)
                    {
                        continue;
                    }
                    foreach (TemplateIdAndCategoryId temp in column.pushFoldTemplateCategoriesId)
                    {
                        if (temp.tempalteId == foldTemplate.UniqueId.ToString())
                        {
                            var foldSchemaTemp = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(foldTemplate.ColumnSchema);
                            foldSchemaTemp.Columns.Add(column);
                            foldTemplate.ColumnSchema = SerializerHelper.SerializeByDataContractSerializer(foldSchemaTemp);
                            break;
                        }
                    }
                }
            }
        }

        public ExportBarcodeDataModel GetColumnValue(PhysicalObjectDto node, RMTemplate templte, RMBarcodeTemplate barcodeTemplate, GeneralSettingModel gls)
        {
            ExportBarcodeDataModel model = new ExportBarcodeDataModel();
            if (node == null || templte == null || barcodeTemplate == null)
            {
                return model;
            }
            string ColumnB = barcodeTemplate.ColumnB;
            string ColumnC = barcodeTemplate.ColumnC;
            string ColumnE = barcodeTemplate.ColumnE;
            string ColumnF = barcodeTemplate.ColumnF;
            Dictionary<string, string> dvalueDic = new Dictionary<string, string>();

            List<string> defaultColumns = new List<string>();
            defaultColumns.Add(BuildInColumnIDs.RecordsId);
            defaultColumns.Add(BuildInColumnIDs.CreatedBy);
            defaultColumns.Add(BuildInColumnIDs.CreatedTime);
            defaultColumns.Add(BuildInColumnIDs.ModifiedBy);
            defaultColumns.Add(BuildInColumnIDs.ModifiedTime);
            foreach (string defaultColumn in defaultColumns)
            {
                string result = "";
                if (defaultColumn == BuildInColumnIDs.RecordsId)
                {
                    result = node.UniqueId;
                }
                else if (defaultColumn == BuildInColumnIDs.CreatedBy)
                {
                    result = node.CreatedBy;
                }
                else if (defaultColumn == BuildInColumnIDs.CreatedTime)
                {
                    result = DateTimeUtil.ConvertTimeFromUtc(node.CreateTime, gls).ToString();
                }
                else if (defaultColumn == BuildInColumnIDs.ModifiedBy)
                {
                    result = node.ModifiedBy;
                }
                else if (defaultColumn == BuildInColumnIDs.ModifiedTime)
                {
                    result = DateTimeUtil.ConvertTimeFromUtc(node.ModifiedTime, gls).ToString();
                }
                if (ColumnB == defaultColumn)
                {
                    model.ColumnB = result;
                }
                if (ColumnC == defaultColumn)
                {
                    model.ColumnC = result;
                }
                if (barcodeTemplate.ColumnDList != null)
                {
                    foreach (string dcolumnName in barcodeTemplate.ColumnDList)
                    {
                        if (dcolumnName == defaultColumn)
                        {
                            if (!dvalueDic.ContainsKey(dcolumnName))
                            {
                                dvalueDic.Add(dcolumnName, result);
                            }
                        }
                    }
                }
                if (ColumnE == defaultColumn)
                {
                    model.ColumnE = result;
                }
                if (ColumnF == defaultColumn)
                {
                    model.ColumnF = result;
                }
            }

            Dictionary<string, string> metaInfo = node.MetaInfo;
            var schemaTemp = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(templte.ColumnSchema);
            foreach (ColumnXmlSchema column in schemaTemp.Columns)
            {
                if (ColumnB == column.Name)
                {
                    if (metaInfo.ContainsKey(column.UniqueId.ToString()))
                    {
                        string result = HandleMetaInfoColumn(column, node, gls);
                        model.ColumnB = result;
                    }
                }
                if (ColumnC == column.Name)
                {
                    if (metaInfo.ContainsKey(column.UniqueId.ToString()))
                    {
                        string result = HandleMetaInfoColumn(column, node, gls);
                        model.ColumnC = result;
                    }
                }
                if (barcodeTemplate.ColumnDList != null)
                {
                    foreach (string dcolumnName in barcodeTemplate.ColumnDList)
                    {
                        if (dcolumnName == column.Name)
                        {
                            if (metaInfo.ContainsKey(column.UniqueId.ToString()))
                            {
                                string result = HandleMetaInfoColumn(column, node, gls);
                                if (!dvalueDic.ContainsKey(dcolumnName))
                                {
                                    dvalueDic.Add(dcolumnName, result);
                                }
                            }
                        }
                    }
                }
                if (ColumnE == column.Name)
                {
                    if (metaInfo.ContainsKey(column.UniqueId.ToString()))
                    {
                        string result = HandleMetaInfoColumn(column, node, gls);
                        model.ColumnE = result;
                    }
                }
                if (ColumnF == column.Name)
                {
                    if (metaInfo.ContainsKey(column.UniqueId.ToString()))
                    {
                        string result = HandleMetaInfoColumn(column, node, gls);
                        model.ColumnF = result;
                    }
                }
            }
            model.ColumnDValue = dvalueDic;
            model.UniqueId = node.UniqueId;
            model.Barcode = string.IsNullOrEmpty(node.BarcodeId) ? node.UniqueId : node.BarcodeId; 
            return model;
        }

        public string HandleMetaInfoColumn(ColumnXmlSchema column, PhysicalObjectDto node, GeneralSettingModel gls)
        {
            string result = "";
            Dictionary<string, string> metaInfo = node.MetaInfo;
            if (column.UniqueId.ToString() == DefaultColumnIDs.Classification
                || column.UniqueId.ToString() == DefaultColumnIDs.Status
                || column.UniqueId.ToString() == DefaultColumnIDs.Format
                || column.UniqueId.ToString() == DefaultColumnIDs.ProtectiveMarking)
            {
                if (metaInfo[column.UniqueId.ToString()] != null)
                {
                    Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(metaInfo[column.UniqueId.ToString()]);
                    if (dic.ContainsKey("Name"))
                    {
                        result = dic["Name"];
                    }
                }
            }
            else if (column.UniqueId.ToString() == DefaultColumnIDs.HomeLocation)
            {//home location
                result = node.HomeLocationFullPath;
            }
            else if (column.ColumnType == AvePoint.RA.Contract.TemplateManagement.ColumnType.DateTime)
            {
                if (metaInfo[column.UniqueId.ToString()] != null)
                {
                    var field = JsonConvert.DeserializeObject<DateTimeColumnValue>(metaInfo[column.UniqueId.ToString()]);
                    if (field.TimeZoneId == gls.TimeZoneId && field.IsSetDayLight == gls.DayLight)
                    {
                        result = field.Date.ToString();
                    }
                    else
                    {
                        var columnUTCDate = field.GetUtcDate();
                        var glsTimeZone = GeneralSettingConfig.FindSystemTimeZoneById(gls.TimeZoneId);
                        var glsTimeZoneDateTime = DateTimeUtil.ConvertTimeFromUtc(columnUTCDate, gls);
                        if (glsTimeZoneDateTime.Kind == DateTimeKind.Utc)
                        {
                            glsTimeZoneDateTime = DateTime.SpecifyKind(glsTimeZoneDateTime, DateTimeKind.Unspecified);
                        }
                        result = glsTimeZoneDateTime.ToString();
                    }
                }
            }
            else if (column.ColumnType == AvePoint.RA.Contract.TemplateManagement.ColumnType.SingleChoice)
            {
                if (metaInfo[column.UniqueId.ToString()] != null)
                {
                    Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(metaInfo[column.UniqueId.ToString()]);
                    if (dic.ContainsKey("Name"))
                    {
                        result = dic["Name"];
                    }
                }
            }
            else if (column.ColumnType == AvePoint.RA.Contract.TemplateManagement.ColumnType.MultipleChoice)
            {
                if (metaInfo[column.UniqueId.ToString()] != null)
                {
                    List<ChoiceColumnValue> choices = JsonConvert.DeserializeObject<List<ChoiceColumnValue>>(metaInfo[column.UniqueId.ToString()]);
                    foreach (ChoiceColumnValue temp in choices)
                    {
                        result += temp.Name + ';';
                    }
                }
            }
            else if (column.ColumnType == AvePoint.RA.Contract.TemplateManagement.ColumnType.PeopleOrGroup)
            {
                if (metaInfo[column.UniqueId.ToString()] != null)
                {
                    var field = JsonConvert.DeserializeObject<List<PeopleColumnValue>>(metaInfo[column.UniqueId.ToString()]);
                    result = string.Join(";", field.Select(f => f.DisplayName.Trim()).ToList()).Trim(';');
                }
            }
            else
            {
                result = metaInfo[column.UniqueId.ToString()];
            }
            return result;
        }

        public void GetSubBottomLocation(IPhysicalLocation location, List<IPhysicalLocation> allSubBottomLocation)
        {
            List<IPhysicalLocation> subLocations = location.AllSubLocations;
            foreach (IPhysicalLocation subLocation in subLocations)
            {
                if (subLocation.IsBottomLocation)
                {
                    allSubBottomLocation.Add(subLocation);
                }
                else
                {
                    GetSubBottomLocation(subLocation, allSubBottomLocation);
                }
            }
        }

        public void ConvertDataToArrayForBottomLocation(IPhysicalLocation bottomlocation, List<string[]> sheetDatasInfo)
        {
            try
            {
                //GetBoxes(bottomlocation)?.ForEach(b => ProcessBox(b, sheetDatasInfo));
                //GetFiles(bottomlocation)?.ForEach(f => ProcessFile(f, sheetDatasInfo));
                GetBoxesAndFoldOrderByDescending(bottomlocation)?.ForEach(r =>
                {
                    if (r.NodeType == (int)RMNodeLevel.PhysicalBox)
                    {
                        PhysicalBox box = new PhysicalBox(r);
                        ProcessBox(box, sheetDatasInfo);
                    }
                    else if (r.NodeType == (int)RMNodeLevel.PhysicalFile)
                    {
                        PhysicalFile file = new PhysicalFile(r);
                        ProcessFile(file, sheetDatasInfo);
                    }
                });
                ReportManager.Increase();
                ReportManager.SendJobDetail(new JMExportBarcodeJobDetail()
                {
                    ObjectName = bottomlocation.Name,
                    FullPath = bottomlocation.DirPath,
                    ItemType = "RM_Common_ObjectLevel_PhysicalLocation",
                    Status = JobDetailsStatus.Successful,
                });
            }
            catch (Exception ex)
            {
                ReportManager.SendJobDetail(new JMExportBarcodeJobDetail()
                {
                    ObjectName = bottomlocation.Name,
                    FullPath = bottomlocation.DirPath,
                    ItemType = "RM_Common_ObjectLevel_PhysicalLocation",
                    Status = JobDetailsStatus.Failed,
                    Comment = ex.Message,
                });
            }
        }
        public void ProcessBox(IPhysicalBox box, List<string[]> sheetDatasInfo)
        {
            try
            {
                using (CheckJobStopScope stopScope = new CheckJobStopScope())
                {
                    ConvertBoxAndFoldBarcordInfoToArray(box, sheetDatasInfo);
                    ReportManager.SendJobDetail(new JMExportBarcodeJobDetail()
                    {
                        ObjectName = box.Name,
                        FullPath = box.DirPath,
                        ItemType = "RM_Common_ObjectLevel_PhysicalBox",
                        Status = JobDetailsStatus.Successful,
                    });
                }
            }
            catch (JobStopException)
            {
                throw new JobStopException("the job has stopped.");
            }
            catch (Exception ex)
            {
                ReportManager.SendJobDetail(new JMExportBarcodeJobDetail()
                {
                    ObjectName = box.Name,
                    FullPath = box.DirPath,
                    ItemType = "RM_Common_ObjectLevel_PhysicalBox",
                    Status = JobDetailsStatus.Failed,
                    Comment = ex.Message,
                });
            }
        }


        public void ProcessFile(IPhysicalFile file, List<string[]> sheetDatasInfo)
        {
            try
            {
                using (CheckJobStopScope stopScope = new CheckJobStopScope())
                {
                    ConvertFoldBarcordInfoToArray(file, sheetDatasInfo);
                    ReportManager.SendJobDetail(new JMExportBarcodeJobDetail()
                    {
                        ObjectName = file.Name,
                        FullPath = file.DirPath,
                        ItemType = "RM_Common_ObjectLevel_PhysicalFile",
                        Status = JobDetailsStatus.Successful,
                    });
                }
            }
            catch (JobStopException)
            {
                throw new JobStopException("the job has stopped.");
            }
            catch (Exception ex)
            {
                ReportManager.SendJobDetail(new JMExportBarcodeJobDetail()
                {
                    ObjectName = file.Name,
                    FullPath = file.DirPath,
                    ItemType = "RM_Common_ObjectLevel_PhysicalFile",
                    Status = JobDetailsStatus.Failed,
                    Comment = ex.Message,
                });
            }
        }

        private List<Record> GetBoxesAndFoldOrderByDescending(IPhysicalLocation location)
        {
            return location.GetBoxesAndFoldOrderByDescending(b => (b.RecordStatus == (int)RMRecordStatus.Active || b.RecordStatus == (int)RMRecordStatus.Closed
          || b.RecordStatus == (int)RMRecordStatus.Missing || b.RecordStatus == (int)RMRecordStatus.Destroyed));
        }



        public void ConvertBoxAndFoldBarcordInfoToArray(IPhysicalBox box, List<string[]> sheetDatasInfo)
        {
            List<string> boxNameInfo = new List<string>();
            List<string> boxUniqueIDInfo = new List<string>();
            List<string> boxBarcodeString = new List<string>();
            //PhysicalBox box = new PhysicalBox(NodeId);
            boxNameInfo.Add(I18NEntity.GetString("RM_EBR_Name"));
            boxNameInfo.Add(box.Name);
            boxNameInfo.Add("");
            boxUniqueIDInfo.Add(I18NEntity.GetString("RM_EBR_UniqueId"));
            boxUniqueIDInfo.Add(box.RecordId);
            boxUniqueIDInfo.Add("");
            boxBarcodeString.Add(I18NEntity.GetString("RM_EBR_Barcode"));
            boxBarcodeString.Add(box.RecordId);
            boxBarcodeString.Add("");
            sheetDatasInfo.Add(boxNameInfo.ToArray());
            sheetDatasInfo.Add(boxUniqueIDInfo.ToArray());
            sheetDatasInfo.Add(boxBarcodeString.ToArray());
            InsetEmptyRow(sheetDatasInfo);
            List<PhysicalFile> folds = box.GetFilesOrderByDescending(b => (b.RecordStatus == (int)RMRecordStatus.Active || b.RecordStatus == (int)RMRecordStatus.Closed || b.RecordStatus == (int)RMRecordStatus.Missing || b.RecordStatus == (int)RMRecordStatus.Destroyed));
            List<string> fileNameInfo = new List<string>();
            List<string> fileUniqueIDInfo = new List<string>();
            List<string> fileBarcodeString = new List<string>();

            for (int num = 1; num <= folds.Count; num++)
            {
                ReportManager.SendJobDetail(new JMExportBarcodeJobDetail()
                {
                    ObjectName = folds[num - 1].Name,
                    FullPath = folds[num - 1].DirPath,
                    ItemType = "RM_Common_ObjectLevel_PhysicalFile",
                    Status = JobDetailsStatus.Successful,
                });

                fileNameInfo.Add(I18NEntity.GetString("RM_EBR_Name"));
                fileNameInfo.Add(folds[num - 1].Name);
                fileNameInfo.Add("");
                fileUniqueIDInfo.Add(I18NEntity.GetString("RM_EBR_UniqueId"));
                fileUniqueIDInfo.Add(folds[num - 1].RecordId);
                fileUniqueIDInfo.Add("");
                fileBarcodeString.Add(I18NEntity.GetString("RM_EBR_Barcode"));
                fileBarcodeString.Add(folds[num - 1].RecordId);
                fileBarcodeString.Add("");
                if (num % 2 == 0)
                {
                    sheetDatasInfo.Add(fileNameInfo.ToArray());
                    sheetDatasInfo.Add(fileUniqueIDInfo.ToArray());
                    sheetDatasInfo.Add(fileBarcodeString.ToArray());
                    InsetEmptyRow(sheetDatasInfo);
                    fileNameInfo = new List<string>();
                    fileUniqueIDInfo = new List<string>();
                    fileBarcodeString = new List<string>();
                }
            }
            if (!fileNameInfo.IsNullOrEmpty())
            {
                sheetDatasInfo.Add(fileNameInfo.ToArray());
                sheetDatasInfo.Add(fileUniqueIDInfo.ToArray());
                sheetDatasInfo.Add(fileBarcodeString.ToArray());
                InsetEmptyRow(sheetDatasInfo);
            }
            box.Dispose();
            folds.Clear();
        }

        public void ConvertFoldBarcordInfoToArray(IPhysicalFile file, List<string[]> sheetDatasInfo)
        {
            List<string> fileNameInfo = new List<string>();
            List<string> fileUniqueIDInfo = new List<string>();
            List<string> fileBarcodeString = new List<string>();
            //PhysicalBox box = new PhysicalBox(NodeId);
            fileNameInfo.Add(I18NEntity.GetString("RM_EBR_Name"));
            fileNameInfo.Add(file.Name);
            fileNameInfo.Add("");
            fileUniqueIDInfo.Add(I18NEntity.GetString("RM_EBR_UniqueId"));
            fileUniqueIDInfo.Add(file.RecordId);
            fileUniqueIDInfo.Add("");
            fileBarcodeString.Add(I18NEntity.GetString("RM_EBR_Barcode"));
            fileBarcodeString.Add(file.RecordId);
            fileBarcodeString.Add("");
            sheetDatasInfo.Add(fileNameInfo.ToArray());
            sheetDatasInfo.Add(fileUniqueIDInfo.ToArray());
            sheetDatasInfo.Add(fileBarcodeString.ToArray());
            InsetEmptyRow(sheetDatasInfo);
            file.Dispose();
        }

        private void InsetEmptyRow(List<string[]> sheetDatasInfo)
        {
            List<string> emptyRow = new List<string>();
            for (int index = 0; index < 6; index++)
            {
                emptyRow.Add("");
            }
            sheetDatasInfo.Add(emptyRow.ToArray());
        }


        public void CreateSheets(string path, string sheetName, List<string[]> barcodeDatas)
        {
            using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Create(path, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookpart = spreadsheet.AddWorkbookPart();
                workbookpart.Workbook = new Workbook();
                //WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                //worksheetPart.Worksheet = new Worksheet(new SheetData());
                SharedStringTablePart shareStringPart;
                if (workbookpart.GetPartsOfType<SharedStringTablePart>().Count() > 0)
                {
                    shareStringPart = workbookpart.GetPartsOfType<SharedStringTablePart>().First();
                }
                else
                {
                    shareStringPart = workbookpart.AddNewPart<SharedStringTablePart>();
                }
                shareStringPart.SharedStringTable = new SharedStringTable();
                shareStringPart.SharedStringTable.AppendChild(new SharedStringItem(new DocumentFormat.OpenXml.Spreadsheet.Text("50")));
                shareStringPart.SharedStringTable.Save();

                Sheets sheets = spreadsheet.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());
                CreateTermsSheet(spreadsheet, workbookpart, sheets, sheetName, barcodeDatas);

                workbookpart.Workbook.Save();
                //worksheetPart.Worksheet.Save();
                spreadsheet.Dispose();
            }
        }

        public void CreateTermsSheet(SpreadsheetDocument spreadsheet, WorkbookPart workBookPart, Sheets sheets, string sheetName, List<string[]> datas)
        {
            UInt32 sheetId = 1;
            WorksheetPart worksheetPart = workBookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet();
            CreateSheet(spreadsheet, worksheetPart, sheets, sheetId, sheetName, datas);

            CalculatePictureCountAndPosition(datas, worksheetPart);
        }

        public void InsertWorksheet(string docPath, string sheetName, List<string[]> barcodeDatas)
        {
            using (SpreadsheetDocument spreadSheet = SpreadsheetDocument.Open(docPath, true))
            {
                WorkbookPart workbookPart = spreadSheet.WorkbookPart;
                WorksheetPart newWorksheetPart = spreadSheet.WorkbookPart.AddNewPart<WorksheetPart>();
                newWorksheetPart.Worksheet = new Worksheet(new SheetData());
                Sheets sheets = spreadSheet.WorkbookPart.Workbook.GetFirstChild<Sheets>();
                CreateTermsSheetForInsert(spreadSheet, workbookPart, sheets, sheetName, barcodeDatas);


                newWorksheetPart.Worksheet.Save();
                spreadSheet.WorkbookPart.Workbook.Save();
                spreadSheet.Dispose();
            }
        }

        public void CreateTermsSheetForInsert(SpreadsheetDocument spreadsheet, WorkbookPart workBookPart, Sheets sheets, string sheetName, List<string[]> datas)
        {
            WorksheetPart worksheetPart = workBookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet();
            CreateSheetForInsert(sheetName, spreadsheet, sheets, worksheetPart, datas);

            CalculatePictureCountAndPosition(datas, worksheetPart);
        }

        public void CreateSheetForInsert(string sheetName, SpreadsheetDocument spreadSheet, Sheets sheets, WorksheetPart worksheetPart, List<string[]> rowDatas)
        {
            string relationshipId = spreadSheet.WorkbookPart.GetIdOfPart(worksheetPart);
            uint sheetId = 1;
            if (sheets.Elements<Sheet>().Count() > 0)
            {
                sheetId = sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
            }
            Sheet sheet = new Sheet()
            {
                Id = relationshipId,
                SheetId = sheetId,
                Name = sheetName
            };
            SheetData sheetData = new SheetData();
            uint rowIndex = 1;
            foreach (var rowData in rowDatas)
            {
                try
                {
                    CreateSheetContentRow(sheetData, rowIndex, rowData.ToList());
                }
                catch (Exception ex)
                {
                    logger.Error("insert sheet error is {0}", ex.Message);
                    throw;
                }
                rowIndex++;
            }
            worksheetPart.Worksheet.AppendChild(AdjustColumnWidth());
            worksheetPart.Worksheet.AppendChild(sheetData);
            sheets.Append(sheet);
        }

        public void CalculatePictureCountAndPosition(List<string[]> datas, WorksheetPart worksheetPart)
        {
            int rowHeight = 15;//一个默认行高是15
            for (int index = 0; index < datas.Count; index++)
            {
                //有barcode的那一行
                if (index % 4 == 0 && index > 0)
                {
                    string[] barcodeRow = datas[index];
                    long y = rowHeight * 4 + ((index / 4 - 1) * 8) * 15 + 1;
                    if (!string.IsNullOrEmpty(barcodeRow[1]))
                    {
                        long x = 55;
                        logger.Info("insert row index is {0}", index);
                        ReportManager.Increase(4);
                        InsertImage(x, y, null, null, barcodeRow[1], worksheetPart);
                    }
                    if (barcodeRow.Length > 3 && !string.IsNullOrEmpty(barcodeRow[4]))
                    {
                        long x = 395;
                        logger.Info("insert row index is {0}", index);
                        InsertImage(x, y, null, null, barcodeRow[4], worksheetPart);
                    }

                }
            }
        }

        public void CreateSheet(SpreadsheetDocument spreadsheet, WorksheetPart worksheetPart, Sheets sheets, UInt32 sheetId, string name, List<string[]> rowDatas)
        {
            Sheet sheet = new Sheet()
            {
                Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart),
                SheetId = sheetId,
                Name = name
            };
            SheetData sheetData = new SheetData();
            uint rowIndex = 1;
            foreach (var rowData in rowDatas)
            {
                try
                {
                    CreateSheetContentRow(sheetData, rowIndex, rowData.ToList());
                }
                catch (Exception ex)
                {
                    logger.Error("Create Sheet error is {0}", ex.Message);
                    throw;
                }
                rowIndex++;
            }
            worksheetPart.Worksheet.AppendChild(AdjustColumnWidth());
            worksheetPart.Worksheet.AppendChild(sheetData);
            worksheetPart.Worksheet.Save();
            sheets.Append(sheet);
        }

        public void CreateSheetContentRow(SheetData sheetData, uint rowIndex, List<string> barcodeDatas)
        {
            Row row = new Row() { RowIndex = rowIndex };
            if (rowIndex > 1 && (rowIndex == 5 || (rowIndex > 4 && (rowIndex - 5) % 4 == 0)))
            {
                row.CustomHeight = true;
                row.Height = (DoubleValue)75;
            }

            foreach (string data in barcodeDatas)
            {
                var cellValueStr = ReplaceLowOrderASCIICharacters(data);
                Cell cell = new Cell()
                {
                    DataType = CellValues.String,
                    CellValue = new CellValue(cellValueStr),
                };
                row.Append(cell);
            }
            sheetData.Append(row);
        }

        public string ReplaceLowOrderASCIICharacters(string tmp)
        {
            if (string.IsNullOrEmpty(tmp))
            {
                return tmp;
            }
            StringBuilder info = new StringBuilder();
            foreach (char cc in tmp)
            {
                int ss = (int)cc;
                if (((ss >= 0) && (ss <= 8)) || ((ss >= 11) && (ss <= 12)) || ((ss >= 14) && (ss <= 32)))
                    info.AppendFormat(" ", ss);
                else info.Append(cc);
            }
            return info.ToString();
        }

        private Columns AdjustColumnWidth()
        {
            Columns columns = new Columns();
            for (int i = 1; i <= 6; i++)
            {
                double width = 10;
                if (i == 2 || i == 5)
                {
                    width = 45;
                }
                Column col = new Column() { BestFit = true, Min = (UInt32)i, Max = (UInt32)i, CustomWidth = true, Width = (DoubleValue)width };
                columns.Append(col);
            }
            return columns;
        }

        public void InsertImage(long x, long y, long? width, long? height, string barcodeValue, WorksheetPart currentWorksheetPart)
        {
            try
            {
                DrawingsPart drawingsPart;
                ImagePart imagePart;
                WorksheetDrawing worksheetDrawing;

                PartTypeInfo imagePartType = ImagePartType.Png;

                if (currentWorksheetPart.DrawingsPart == null)
                {
                    //----- no drawing part exists, add a new one
                    drawingsPart = currentWorksheetPart.AddNewPart<DrawingsPart>();
                    imagePart = drawingsPart.AddImagePart(imagePartType, currentWorksheetPart.GetIdOfPart(drawingsPart));
                    worksheetDrawing = new WorksheetDrawing();
                }
                else
                {
                    //----- use existing drawing part
                    drawingsPart = currentWorksheetPart.DrawingsPart;
                    imagePart = drawingsPart.AddImagePart(imagePartType);
                    drawingsPart.CreateRelationshipToPart(imagePart);
                    worksheetDrawing = drawingsPart.WorksheetDrawing;
                }
                var bi = new BarCodeImageInfo();
                using (var barcodeStream = new BarcodeUtil().GetBarcodeStream(barcodeValue, ref bi))
                {
                    barcodeStream.Position = 0;
                    imagePart.FeedData(barcodeStream);
                }


                int imageNumber = drawingsPart.ImageParts.Count<ImagePart>();
                if (imageNumber == 1)
                {
                    Drawing drawing = new Drawing();
                    drawing.Id = drawingsPart.GetIdOfPart(imagePart);
                    currentWorksheetPart.Worksheet.Append(drawing);
                }

                NonVisualDrawingProperties drawingProperties = new NonVisualDrawingProperties();
                drawingProperties.Id = new UInt32Value((uint)(1024 + imageNumber));
                drawingProperties.Name = "Picture " + imageNumber.ToString();
                drawingProperties.Description = "";
                DocumentFormat.OpenXml.Drawing.PictureLocks picLocks = new DocumentFormat.OpenXml.Drawing.PictureLocks();
                picLocks.NoChangeAspect = true;
                picLocks.NoChangeArrowheads = true;
                NonVisualPictureDrawingProperties pictureDrawingProperties = new NonVisualPictureDrawingProperties();
                pictureDrawingProperties.PictureLocks = picLocks;
                //pictureDrawingProperties.PreferRelativeResize = true;
                NonVisualPictureProperties nvpp = new NonVisualPictureProperties();
                nvpp.NonVisualDrawingProperties = drawingProperties;
                nvpp.NonVisualPictureDrawingProperties = pictureDrawingProperties;

                DocumentFormat.OpenXml.Drawing.Stretch stretch = new DocumentFormat.OpenXml.Drawing.Stretch();
                stretch.FillRectangle = new DocumentFormat.OpenXml.Drawing.FillRectangle();

                BlipFill blipFill = new BlipFill();
                DocumentFormat.OpenXml.Drawing.Blip blip = new DocumentFormat.OpenXml.Drawing.Blip();
                blip.Embed = drawingsPart.GetIdOfPart(imagePart);
                blip.CompressionState = DocumentFormat.OpenXml.Drawing.BlipCompressionValues.Print;
                blipFill.Blip = blip;
                blipFill.SourceRectangle = new DocumentFormat.OpenXml.Drawing.SourceRectangle();
                blipFill.Append(stretch);

                DocumentFormat.OpenXml.Drawing.Transform2D transform2D = new DocumentFormat.OpenXml.Drawing.Transform2D();
                DocumentFormat.OpenXml.Drawing.Offset offset = new DocumentFormat.OpenXml.Drawing.Offset();
                offset.X = 0;
                offset.Y = 0;
                transform2D.Offset = offset;

                DocumentFormat.OpenXml.Drawing.Extents extents = new DocumentFormat.OpenXml.Drawing.Extents();
                var w = bi.Width;
                var h = bi.Height;
                var hr = bi.HR;
                var vr = bi.VR;
                if (width == null)
                    extents.Cx = (long)w * (long)((float)914400 / hr);
                else
                    extents.Cx = width * (long)((float)914400 / hr);

                if (height == null)
                    extents.Cy = (long)h * (long)((float)914400 / vr);
                else
                    extents.Cy = height * (long)((float)914400 / vr);

                transform2D.Extents = extents;
                ShapeProperties shapeProperties = new ShapeProperties();
                shapeProperties.BlackWhiteMode = DocumentFormat.OpenXml.Drawing.BlackWhiteModeValues.Auto;
                shapeProperties.Transform2D = transform2D;
                DocumentFormat.OpenXml.Drawing.PresetGeometry presetGeometry = new DocumentFormat.OpenXml.Drawing.PresetGeometry();
                presetGeometry.Preset = DocumentFormat.OpenXml.Drawing.ShapeTypeValues.Rectangle;
                presetGeometry.AdjustValueList = new DocumentFormat.OpenXml.Drawing.AdjustValueList();
                shapeProperties.Append(presetGeometry);
                shapeProperties.Append(new DocumentFormat.OpenXml.Drawing.NoFill());

                DocumentFormat.OpenXml.Drawing.Spreadsheet.Picture picture = new DocumentFormat.OpenXml.Drawing.Spreadsheet.Picture();
                picture.NonVisualPictureProperties = nvpp;
                picture.BlipFill = blipFill;
                picture.ShapeProperties = shapeProperties;

                Position position = new Position();

                position.X = x * 914400 / 72;
                position.Y = y * 914400 / 72;
                Extent extent = new Extent();
                extent.Cx = extents.Cx;
                extent.Cy = extents.Cy;
                AbsoluteAnchor absoluteAnchor = new AbsoluteAnchor();
                absoluteAnchor.Position = position;
                absoluteAnchor.Extent = extent;
                absoluteAnchor.Append(picture);
                absoluteAnchor.Append(new ClientData());
                worksheetDrawing.Append(absoluteAnchor);
                worksheetDrawing.Save(drawingsPart);
            }
            catch (Exception ex)
            {
                logger.Error("insert image error is {0}", ex.Message);
                throw;
            }
        }

    }
}
