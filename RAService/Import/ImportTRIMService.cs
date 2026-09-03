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
using AvePoint.Common;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Import;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Physical.ColumnValues;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.JobMonitor.Detail;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AvePoint.RA.Service.Import
{
    public class ImportTRIMService : IImportTRIMService
    {
        private RALogger logger = RALogger.GetInstance(typeof(ImportTRIMService)); 
        private IRMLocationDao LocationDao => PlatformWindsorManager.GetService<IRMLocationDao>();
        private ITemplateManagementService TemplateManagementService => PlatformWindsorManager.GetService<ITemplateManagementService>();
        private RA.DB.Explorer.Dao.IExplorerDao _explorerDao;
        public RA.DB.Explorer.Dao.IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new RA.DB.Explorer.Dao.CosmosImp.ExplorerDao(true);
                }
                return _explorerDao;
            }
        }
        private IPhysicalRecordSettingDao PhysicalRecordSettingDao => PlatformWindsorManager.GetService<IPhysicalRecordSettingDao>();
        private IRecordLoanAllianceDao RecordLoanAllianceDao => PlatformWindsorManager.GetService<IRecordLoanAllianceDao>();
        private IRecordImportSettingDao ImportSettingDao => PlatformWindsorManager.GetService<IRecordImportSettingDao>();
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IGeneralSettingService mGeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        private IJobQueueService mJobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();


        #region Global Param
        public List<RecordTypeMapping> RecordTypeMappings;
        public List<ColumnValueMapping> ColumnValueMappings;
        public List<UserMapping> UserMappings;
        public ColumnMapping BoxColumnMapping;
        public ColumnMapping FolderColumnMapping;
        public ColumnMapping RecordColumnMapping;
        public string DateFormate = "d/MM/yyyy 'at' h:mm tt";
        public string TimeZoneId = "(GMT+10:00) Canberra, Melbourne, Sydney";
        private TimeZoneInfo _timeZone;
        public TimeZoneInfo GTimeZoneInfo
        {
            get
            {
                if(_timeZone == null)
                {
                    try
                    {
                        _timeZone = GeneralSettingConfig.FindSystemTimeZoneById(this.TimeZoneId);
                    }
                    catch 
                    {
                        _timeZone = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(a=>a.DisplayName == TimeZoneId);
                    }
                }
                return _timeZone;
            }
        }
        public double DefaultLocationSize = 1000.0;
        public double DefaultBoxSize = 1.0;
        public string MigDBConnString = ""; 
        #endregion

        #region Import Meta Info
        public async System.Threading.Tasks.Task ImportMetaFileAsync(Dictionary<string, List<string[]>> datas)
        {
            foreach (KeyValuePair<string, List<string[]>> pair in datas)
            {
                if (pair.Key.StartsWith("record type", StringComparison.OrdinalIgnoreCase))
                {
                    await this.ImportRecordTypeMappingAsync(pair.Value);
                }
                else if (pair.Key.StartsWith("physical box column", StringComparison.OrdinalIgnoreCase))
                {
                    await this.ImportColunMappingAsync(RMNodeType.PhyBox, pair.Value);
                }
                else if (pair.Key.StartsWith("physical folder column", StringComparison.OrdinalIgnoreCase))
                {
                    await this.ImportColunMappingAsync(RMNodeType.PhyFile, pair.Value);
                }
                else if (pair.Key.StartsWith("physical record column", StringComparison.OrdinalIgnoreCase))
                {
                    await this.ImportColunMappingAsync(RMNodeType.PhyRecord, pair.Value);
                }
                else if (pair.Key.StartsWith("column value", StringComparison.OrdinalIgnoreCase))
                {
                    await this.ImportColumnValueMappingAsync(pair.Value);
                }
                else if (pair.Key.StartsWith("user mapping", StringComparison.OrdinalIgnoreCase))
                {
                    await this.ImportUserMappingAsync(pair.Value);
                }else if (pair.Key.Contains("Setting"))
                {
                    //Dealwith general setting
                    await this.ImportSystemSettingAsync(pair.Value);
                }
            }
        }

        private async Task<bool> ImportRecordTypeMappingAsync(List<string[]> datas)
        {
            int index = 0;
            List<RecordTypeMapping> mappings = new List<RecordTypeMapping>();
            foreach(string[] data in datas)
            {
                if(index != 0 && data.Length > 1)
                {
                    if(data[0] != null && data[0] != string.Empty && data[1] != null)
                    {
                        RecordTypeMapping map = new RecordTypeMapping();
                        map.SrcRecordType = data[0].Trim();
                        map.DestTemplateType = data[1].Trim();
                        mappings.Add(map);
                        logger.Info("Import datatype mapping {0}", string.Join(":", data));
                    }
                }
                index++;
            }

            RMMiscProfile profile = new RMMiscProfile();
            profile.Extension = SerializerHelper.SerializeByDataContractSerializer(mappings);
            await this.SaveOrUpdateProfileAsync((int)ImportProfileType.RecordTypeMapping, "RecordTypeMapping", profile);
            return true;
            //string xml = SerializerHelper.SerializeByDataContractSerializer(mappings);
            //return SaveXmlFile("RecordTypeMapping", xml);
        }
        private async Task<bool> ImportColumnValueMappingAsync(List<string[]> datas)
        {
            int index = 0;
            List<ColumnValueMapping> mappings = new List<ColumnValueMapping>();
            foreach (string[] data in datas)
            {
                if (index != 0 && data.Length > 4)
                {
                    if (data[0] != null && data[1] != null && data[2] != null && data[3] != null&& data[4] != null)
                    {
                        ColumnValueMapping map = new ColumnValueMapping();
                        map.RecordType = data[0].Trim();
                        map.SrcColumn = data[1].Trim();
                        map.DescColumn = data[2].Trim();
                        map.SrcValue = data[3].Trim();
                        map.DestValue = data[4].Trim();
                        mappings.Add(map);
                        logger.Info("Import column value mapping  {0}", string.Join(" | ", data));
                    }
                }
                index++;
            }

            RMMiscProfile profile = new RMMiscProfile();
            profile.Extension = SerializerHelper.SerializeByDataContractSerializer(mappings);
            await this.SaveOrUpdateProfileAsync((int)ImportProfileType.ColumnValueMapping, "ColumnValueMapping", profile);
             
            return true;
            //this.ColumnValueMappings = mappings;
            //string xml = SerializerHelper.SerializeByDataContractSerializer(mappings);
            //return SaveXmlFile("ColumnValueMapping", xml);
        }
        private async Task<bool> ImportUserMappingAsync(List<string[]> datas)
        {
            int index = 0;
            List<UserMapping> mappings = new List<UserMapping>();
            foreach (string[] data in datas)
            {
                if (index != 0 && data.Length > 1)
                {
                    if (data[0] != null && data[1] != null)
                    {
                        UserMapping map = new UserMapping();
                        map.SrcUserName = data[0].Trim();
                        map.DestEmailAddress = data[1].Trim();
                        mappings.Add(map);
                        logger.Info("Import user mapping  {0}", string.Join(":", data));
                    }
                }
                index++;
            }

            RMMiscProfile profile = new RMMiscProfile();
            profile.Extension = SerializerHelper.SerializeByDataContractSerializer(mappings);
            await this.SaveOrUpdateProfileAsync((int)ImportProfileType.UserMapping, "UserMapping", profile);
             
            return true;
            //this.UserMappings = mappings;
            //string xml = SerializerHelper.SerializeByDataContractSerializer(mappings);
            //return SaveXmlFile("UserMapping", xml);
        }
        private async Task<bool> ImportSystemSettingAsync(List<string[]> datas)
        {
            int index = 0;
            ImportGeneralSetting setting = new ImportGeneralSetting();
            foreach (string[] data in datas)
            {
                if (index != 0 && data.Length > 1)
                {
                    if (data[0] != null && data[1] != null)
                    {
                        if(data[0].Equals("Default Box Size", StringComparison.OrdinalIgnoreCase))
                        {
                            double temp = 0.0;
                            if (!double.TryParse(data[1].Trim(), out temp))
                            {
                                temp = 1;
                            }
                            setting.DefaultBoxSize = temp;
                        }
                        else if (data[0].Equals("Default Location Size", StringComparison.OrdinalIgnoreCase))
                        {

                            double temp = 0.0;
                            if (!double.TryParse(data[1].Trim(), out temp))
                            {
                                temp = 1;
                            }
                            setting.DefaultLocaionSize = temp;
                        }
                        else if (data[0].Equals("Date Time Format", StringComparison.OrdinalIgnoreCase))
                        {
                            setting.DateTimeFormate = data[1].Trim();
                        }
                        else if (data[0].Equals("Date Format", StringComparison.OrdinalIgnoreCase))
                        {
                            setting.DateFormate = data[1].Trim();
                        }
                        else if (data[0].Equals("Time Zone Id", StringComparison.OrdinalIgnoreCase))
                        {
                            setting.TimeZone = data[1].Trim();
                        }
                    }
                }
                index++;
            }
            RMMiscProfile profile = new RMMiscProfile();
            profile.Extension = SerializerHelper.SerializeByDataContractSerializer(setting);
            await this.SaveOrUpdateProfileAsync((int)ImportProfileType.GeneralSetting, "GeneralSetting", profile);
             
            return true;
            //this.UserMappings = mappings;
            //string xml = SerializerHelper.SerializeByDataContractSerializer(mappings);
            //return SaveXmlFile("UserMapping", xml);
        }
        private async System.Threading.Tasks.Task SaveOrUpdateProfileAsync(int profileType, string name, RMMiscProfile profile)
        {
            RMMiscProfile exist = ImportSettingDao.GetProfileByType(profileType);
            if (exist == null)
            { 
                profile.Id = Guid.NewGuid().ToString();
                profile.Type = profileType;
                profile.Name = name; 
                profile.ModifiedTime = DateTime.UtcNow.Ticks;
                ImportSettingDao.Create(profile);
            }
            else
            {
                exist.Extension = profile.Extension ;
                exist.ModifiedTime = DateTime.UtcNow.Ticks;
                await ImportSettingDao.UpdateAsync(exist);
            }
        }
        private async Task<bool> ImportColunMappingAsync(RMNodeType nodeType, List<string[]> datas)
        {
            int index = 0;
            logger.Info("Import column mapping at level {0}", nodeType);
            ColumnMapping mappings = new ColumnMapping() { RecordType = (int)nodeType, Details = new List<ColumnMappingDetail>()};
            foreach (string[] data in datas)
            {
                if (index != 0 && data.Length > 1)
                {
                    if (data[0] != null && data[1] != null)
                    {
                        ColumnMappingDetail map = new ColumnMappingDetail(); 
                        map.SrcName = data[0].Trim();
                        map.DestName = data[1].Trim();
                        map.ColumnType = data[2].Trim();
                        map.MustHave = data[3].Trim();
                        mappings.Details.Add(map);
                        logger.Info("Import Column mapping detail  {0}", string.Join("--", data));
                    }
                }
                index++;
            }
            RMMiscProfile profile = new RMMiscProfile();
            profile.Id = Guid.NewGuid().ToString(); 
            switch (nodeType)
            {
                case RMNodeType.PhyBox:
                    profile.Type = (int)ImportProfileType.BoxColumnMapping;
                    profile.Name = "BoxColumnMapping";
                    break;
                case RMNodeType.PhyFile:
                    profile.Type = (int)ImportProfileType.FolderColumnMapping;
                    profile.Name = "FolderColumnMapping";
                    break;
                case RMNodeType.PhyRecord:
                    profile.Type = (int)ImportProfileType.RecordColumnMapping;
                    profile.Name = "RecordColumnMapping";
                    break;
                default:break;
            } 
            profile.Extension = SerializerHelper.SerializeByDataContractSerializer(mappings);
            profile.ModifiedTime = DateTime.UtcNow.Ticks;

            await this.SaveOrUpdateProfileAsync(profile.Type, profile.Name, profile); 
            return true;
            //string xml = SerializerHelper.SerializeByDataContractSerializer(mappings);
            //return SaveXmlFile(fileName, xml);
        }

        #endregion

        #region Init Mapping Meta before import record
        private void InitMapping()
        {
            try
            {
                List<RMMiscProfile> profiles = ImportSettingDao.FindAll();
                foreach(RMMiscProfile profile in profiles)
                {
                    if (profile.Type == (int)ImportProfileType.RecordTypeMapping)
                    {
                        this.RecordTypeMappings = SerializerHelper.DeserializeByDataContractSerializer<List<RecordTypeMapping>>(profile.Extension);
                    }
                    else if(profile.Type == (int)ImportProfileType.ColumnValueMapping)
                    { 
                        this.ColumnValueMappings = SerializerHelper.DeserializeByDataContractSerializer<List<ColumnValueMapping>>(profile.Extension);
                    }
                    else if (profile.Type == (int)ImportProfileType.UserMapping)
                    { 
                            this.UserMappings = SerializerHelper.DeserializeByDataContractSerializer<List<UserMapping>>(profile.Extension);
                    }
                    else if (profile.Type == (int)ImportProfileType.BoxColumnMapping)
                    {
                        this.BoxColumnMapping = SerializerHelper.DeserializeByDataContractSerializer<ColumnMapping>(profile.Extension);
                    }
                    else if (profile.Type == (int)ImportProfileType.FolderColumnMapping)
                    {
                        this.FolderColumnMapping = SerializerHelper.DeserializeByDataContractSerializer<ColumnMapping>(profile.Extension);
                    }
                    else if (profile.Type == (int)ImportProfileType.RecordColumnMapping)
                    {
                        this.RecordColumnMapping = SerializerHelper.DeserializeByDataContractSerializer<ColumnMapping>(profile.Extension);
                    }
                    else if (profile.Type == (int)ImportProfileType.GeneralSetting)
                    {
                        ImportGeneralSetting setting = SerializerHelper.DeserializeByDataContractSerializer<ImportGeneralSetting>(profile.Extension);
                    }
                } 
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }
        }
         

        
        public async Task<bool> InitMetaAsync()
        {
            InitMapping();
            //ReadImportSettingFromConfigFile();
            InitLocationDic();
            await InitTemplateDicAsync();
            if (this.RecordTypeMappings == null)
            {
                return false;
            }
            return true;
        }
        #endregion

        #region Import Records

        #region Location And Templated Dictionary
        Dictionary<string, RMLocation> locationDic;
        private void InitLocationDic()
        {
            if (locationDic == null || locationDic.Count == 0)
            {
                locationDic = new Dictionary<string, RMLocation>();
                List<RMLocation> allLocation = LocationDao.GetAllLocations();
                foreach (RMLocation lo in allLocation)
                {
                    if (!locationDic.ContainsKey(lo.Name))
                    {
                        locationDic.Add(lo.Name, lo);
                    }
                }
            }
        }

        Dictionary<RMNodeType, TemplateDto> templateDic = new Dictionary<RMNodeType, TemplateDto>();
        private async System.Threading.Tasks.Task InitTemplateDicAsync()
        {
            if (templateDic == null || templateDic.Count == 0)
            {
                List<TemplateDto> templates = await TemplateManagementService.GetAllTemplateDtosAsync();
                foreach (TemplateDto temp in templates)
                {
                    RMNodeType nodeType = convertTemplateType2NodeType(temp.type);
                    if (!templateDic.ContainsKey(nodeType))
                    {
                        templateDic.Add(nodeType, temp);
                    }
                }
            }
        } 

        private RMNodeType convertTemplateType2NodeType(TemplateType templateType)
        {
            switch (templateType)
            {
                case TemplateType.Box:
                    return RMNodeType.PhyBox;
                case TemplateType.Folder:
                    return RMNodeType.PhyFile;
                case TemplateType.Records:
                    return RMNodeType.PhyRecord;
                default:
                    return RMNodeType.PhyRecord;
            }
        } 
        #endregion

        private int GetNodeTypeIndex(string[] header)
        {
            for(int i = 0; i<header.Length; i++)
            {
                if("Record Type".Equals(header[i], StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            return 0;
        }

        private Dictionary<string, int> AssembleColumnIndexNumber(string[] header, string destTemplateType)
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            ColumnMapping columnMapping = BoxColumnMapping;
            if("Physical Box".Equals(destTemplateType, StringComparison.OrdinalIgnoreCase))
            {
                columnMapping = this.BoxColumnMapping;
            }else if ("Physical Folder".Equals(destTemplateType, StringComparison.OrdinalIgnoreCase))
            {
                columnMapping = this.FolderColumnMapping;
            }else if ("Physical Record".Equals(destTemplateType, StringComparison.OrdinalIgnoreCase))
            {
                columnMapping = this.RecordColumnMapping;
            }
            for(int i = 0; i<header.Length; i++)
            {
                ColumnMappingDetail columnMappingDetail = columnMapping.Details.FirstOrDefault(a => a.SrcName.Equals(header[i]));
                if(columnMappingDetail != null)
                {
                    dictionary.Add(columnMappingDetail.DestName, i);
                }
            }
            return dictionary;
        }

        public void ImportPhysicalRecord(string sheetName, List<string[]> sheetData)
        {
            //InitMapping();
            if (this.RecordTypeMappings == null)
            {
                throw new AveException("Please import mapping infomation first.");
            }
            logger.Info("Import physical record sheet {0}, row count {1}", sheetName, sheetData.Count);
            if(sheetData.Count < 2)
            {
                logger.Warn("There is no data in this sheet {0}", sheetName);
                return;
            }
            string[] header = sheetData[0];
            int recordTypeIndex = this.GetNodeTypeIndex(header);
            RecordTypeMapping current = this.RecordTypeMappings.First(a => a.SrcRecordType.Equals(sheetData[1][recordTypeIndex], StringComparison.OrdinalIgnoreCase));
            this.columnIndexDic = AssembleColumnIndexNumber(header, current.DestTemplateType);
            int rowIndex = 0;
            foreach(string[] rowData in sheetData)
            {
                if(rowIndex == 0)
                {
                    rowIndex++;
                    continue;
                }
                rowIndex++;
                try
                {
                    if ("Physical Box".Equals(current.DestTemplateType, StringComparison.OrdinalIgnoreCase))
                    {
                        ProcessBox(rowData, RMNodeType.PhyBox, rowIndex);
                    }
                    else if ("Physical Folder".Equals(current.DestTemplateType, StringComparison.OrdinalIgnoreCase))
                    {
                        ProcessFolder(rowData, RMNodeType.PhyFile, rowIndex);
                    }
                    else if ("Physical Record".Equals(current.DestTemplateType, StringComparison.OrdinalIgnoreCase))
                    {
                        ProcessRecord(rowData, RMNodeType.PhyFile, rowIndex);
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e.Message, e);
                    //add failed report
                }
            }
        }

        private bool ProcessRecord(string[] rowData, RMNodeType nodeType, int rowNumber)
        {
            //Size, Classification, CreatedBy, ModifyBy Excel里没有需要自己处理值
            logger.Info("Start to process record, type {0}, rowNumber {1}", nodeType, rowNumber);


            TemplateDto template = templateDic[nodeType];
            string uniqueId = rowData[columnIndexDic["Unique ID"]];
            Record record = ExplorerDao.GetPhysicalRecordByRecordsId(uniqueId);
            if (record != null)
            {
                logger.Warn("Record Folder with Record Number {0}, UniqueId {1},  already exist, skip.");
                //add skip report
                return false;
            }
            Record rec = new Record();
            rec.Id = Guid.NewGuid();
            rec.NodeId = rec.Id;
            rec.LeafName = rowData[columnIndexDic["Title"]];
            rec.NodeType = (int)nodeType;
            rec.TemplateId = template.id;
            rec.SourceFlag = (int)SourceFlag.Physical;
            rec.ModifiedBy = TenantLocalValue.DisplayName;
            rec.CreatedBy = TenantLocalValue.DisplayName;
            rec.RecordsId = uniqueId;

            string homeLocation = null;
            Record folder = null;
            if (columnIndexDic.ContainsKey("Contained Within (HPRM Container)"))
            {
                homeLocation = rowData[columnIndexDic["Contained Within (HPRM Container)"]];
                folder = ExplorerDao.GetPhysicalRecordByRecordsId(homeLocation);
                if(folder == null)
                {
                    throw new AveException("No folder found with unique id {0}", homeLocation);
                }
            }
            else
            {
                throw new AveException("No 'Contained Within (HPRM Container)' column found in import file");
            }
             
            rec.LocationId = folder.LocationId;
            rec.BoxId = folder.BoxId;
            rec.FolderId = folder.Id;
            rec.TermId = folder.TermId;
            rec.TermName = folder.TermName;
            Dictionary<string, string> metaInfo = this.AssembleColumnInTemplate(template, rec, rowData, folder.Id, folder.LeafName);
            rec.MetaInfo = JsonConvert.SerializeObject(metaInfo);

            if (columnIndexDic.ContainsKey("Created Time"))
            {
                string createdTime = rowData[columnIndexDic["Created Time"]];
                rec.TimeCreated = this.GetTimeLong(createdTime);
            }
            else
            {
                rec.TimeCreated = DateTime.UtcNow.Ticks;
            }
            if (columnIndexDic.ContainsKey("Modified Time"))
            {
                string modifiedTime = rowData[columnIndexDic["Modified Time"]];
                rec.TimeModified = this.GetTimeLong(modifiedTime);
            }
            else
            {
                rec.TimeCreated = DateTime.UtcNow.Ticks;
            }
            ExplorerDao.Add(rec);  
            return true;
        }

        private bool ProcessFolder(string[] rowData, RMNodeType nodeType, int rowNumber)
        {
            //Size, Classification, CreatedBy, ModifyBy Excel里没有需要自己处理值
            logger.Info("Start to process record, type {0}, rowNumber {1}", nodeType, rowNumber);


            TemplateDto template = templateDic[nodeType];
            string uniqueId = rowData[columnIndexDic["Unique ID"]];
            Record exist = ExplorerDao.GetPhysicalRecordByRecordsId(uniqueId);
            if (exist != null)
            {
                logger.Warn("Record Folder with Record Number {0}, UniqueId {1},  already exist, skip.");
                //add skip report
                return false;
            }
            Record rec = new Record();
            rec.Id = Guid.NewGuid();
            rec.NodeId = rec.Id;
            rec.LeafName = rowData[columnIndexDic["Title"]];
            rec.NodeType = (int)nodeType;
            rec.TemplateId = template.id;
            rec.SourceFlag = (int)SourceFlag.Physical;
            rec.ModifiedBy = TenantLocalValue.DisplayName;
            rec.CreatedBy = TenantLocalValue.DisplayName;
            rec.RecordsId = uniqueId;
            string homeLocation = null;
            Record box = null;
            if (columnIndexDic.ContainsKey("File (Container)"))
            {
                homeLocation = rowData[columnIndexDic["File (Container)"]].Trim();
                box = ExplorerDao.GetPhysicalRecordByRecordsId(homeLocation);
                //处理多层Folder
                //TODO
                if(box != null && box.NodeType == (int)RMNodeType.PhyFile)
                {
                    logger.Info("folder {0} located in another folder {1}", rec?.Id, box?.Id);
                }
                if(box == null)
                {
                    throw new AveException("No Box found with unique id {0}", homeLocation);
                }
            }
            else
            {
                throw new AveException("No File(Container) information found.");
            }
            

            rec.LocationId = box.LocationId;
            rec.BoxId = box.Id;
            rec.TermId = box.TermId;
            rec.TermName = box.TermName;  
            Dictionary<string, string> metaInfo = this.AssembleColumnInTemplate(template, rec, rowData, box.Id, box.LeafName);
            rec.MetaInfo = JsonConvert.SerializeObject(metaInfo);

            bool hasAssignee = false;
            string assignee = null;
            if (columnIndexDic.ContainsKey("Where is it? (Assignee)"))
            {
                assignee = rowData[columnIndexDic["Where is it? (Assignee)"]];
                if (!assignee.StartsWith("In file") && !assignee.StartsWith("At home") && !this.locationDic.Values.Any(a => a.Name == assignee))
                {
                    //可能是在个人手上.
                    logger.Info("Assign is {0}, try to add loan info", assignee);
                    ArgumentCheck.NotNull(exist, nameof(exist));
                    exist.HoldType = (int)HoldType.PersonalHold;
                    hasAssignee = true;
                    //add loan alliance
                }
            }
            if (columnIndexDic.ContainsKey("Created Time"))
            {
                string createdTime = rowData[columnIndexDic["Created Time"]];
                rec.TimeCreated = this.GetTimeLong(createdTime);
            }
            else
            {
                rec.TimeCreated = DateTime.UtcNow.Ticks;
            }
            if (columnIndexDic.ContainsKey("Modified Time"))
            {
                string modifiedTime = rowData[columnIndexDic["Modified Time"]];
                rec.TimeModified = this.GetTimeLong(modifiedTime);
            }
            else
            {
                rec.TimeCreated = DateTime.UtcNow.Ticks;
            }
            ExplorerDao.Add(rec);
            if (hasAssignee)
            {
                RecordLoanAllianceDao.CreateOrUpdateLoanAlliance(new RMRecordLoanAlliance() { RecordsId = rec.Id, HoldBy = assignee, HoldReleaseTime = DateTime.MaxValue.Ticks, ParentId = box.Id });
            }
            
            return true;
        }

        Dictionary<string, int> columnIndexDic = new Dictionary<string, int>();
        private bool ProcessBox(string[] rowData, RMNodeType nodeType, int rowNumber)
        {
            //Size, Classification, CreatedBy, ModifyBy Excel里没有需要自己处理值
            logger.Info("Start to process record, type {0}, rowNumber {1}", nodeType, rowNumber);


            TemplateDto template = templateDic[nodeType];
            string uniqueId = rowData[columnIndexDic["Unique ID"]];
            Record exist = ExplorerDao.GetPhysicalRecordByRecordsId(uniqueId);
            if(exist != null)
            {
                logger.Warn("Record Box with Record Number {0}, UniqueId {1},  already exist, skip.");
                //add skip report
                return false;
            }
            Record rec = new Record();
            rec.Id = Guid.NewGuid();
            rec.NodeId = rec.Id;
            rec.LeafName = rowData[columnIndexDic["Title"]];
            rec.NodeType = (int)nodeType;
            rec.TemplateId = template.id;
            rec.SourceFlag = (int)SourceFlag.Physical;
            rec.ModifiedBy = TenantLocalValue.DisplayName;
            rec.CreatedBy = TenantLocalValue.DisplayName;
            rec.RecordsId = uniqueId;
            string homeLocation = validateHomeLocation(rowData[columnIndexDic["Home Location"]], RMNodeType.PhyBox);
            if (this.locationDic.ContainsKey(homeLocation))
            {
                RMLocation location = locationDic[homeLocation];
                if(location.NodeType != (int)RMNodeType.PhysicalBottomLocation)
                {
                    throw new Exception(string.Format("Location {0} is not bottom level location", location.Name));
                }
                rec.LocationId = location.UniqueId;
                TaxonomyColumnValue termInfo = this.GetDefaultTermId(location);
                rec.TermId = new Guid(termInfo.Id);
                rec.TermName = termInfo.Name;
                Dictionary<string, string> metaInfo = this.AssembleColumnInTemplate(template, rec, rowData, location.UniqueId, location.Name);
                rec.MetaInfo = JsonConvert.SerializeObject(metaInfo);

                bool hasAssignee = false;
                string assignee = null;
                if(columnIndexDic.ContainsKey("Where is it? (Assignee)"))
                {
                    assignee = rowData[columnIndexDic["Where is it? (Assignee)"]]; // metaInfo["Where is it? (Assignee)"];
                    if(!assignee.Contains("At home") && !this.locationDic.Values.Any(a=>a.Name == assignee))
                    {
                        //可能是在个人手上.
                        logger.Info("Assign is {0}, try to add loan info", assignee);
                        rec.HoldType = (int)HoldType.PersonalHold;
                        hasAssignee = true;
                        //add loan alliance
                    }
                }else
                {
                    logger.Warn("No assignee infomation in import file");
                }
            
                string createdTime = rowData[columnIndexDic["Created Time"]];
                rec.TimeCreated = this.GetTimeLong(createdTime);
                if(columnIndexDic.ContainsKey("Modified Time"))
                {
                    string modifiedTime = rowData[columnIndexDic["Modified Time"]];
                    rec.TimeModified = this.GetTimeLong(modifiedTime);
                }
                ExplorerDao.Add(rec);
                logger.Info("Add physical record successfully, id {0}, unique id {1}", rec?.Id, rec.RecordsId);
                if (hasAssignee)
                {
                    RecordLoanAllianceDao.CreateOrUpdateLoanAlliance(new RMRecordLoanAlliance() { RecordsId = rec.Id, HoldBy = assignee, HoldReleaseTime = 0, ParentId = location.UniqueId });
                }
            }
            else
            {
                logger.Warn("No location found with name {0}", homeLocation);
                throw new AveException(string.Format("No location found with name {0}", homeLocation));
            }
            return true;
        }

        private Dictionary<string, string> AssembleColumnInTemplate(TemplateDto template, Record rec, string[] rowData, Guid locationId, string locationName)
        {
            Dictionary<string, string> metaInfo = new Dictionary<string, string>();
            foreach(TemplateCategoryDto cat in template.categories)
            {
                foreach(TemplateColumnDto col in cat.columns)
                {
                    if("RM_Template_Column_Name_Title" == col.columnName)
                    {
                        metaInfo.Add(col.uniqueId.ToString(), rec.LeafName);
                    }
                    else if("RM_Template_Column_Name_Capability" == col.columnName)
                    {
                        metaInfo.Add(col.uniqueId.ToString(), this.DefaultBoxSize.ToString());
                    }
                    else if ("RM_Template_Column_Name_HomeLocation" == col.columnName)
                    {
                        metaInfo.Add(col.uniqueId.ToString(), JsonConvert.SerializeObject(new TaxonomyColumnValue() {Id = locationId.ToString(), Name = locationName })); //RM_Template_Column_Name_Classification
                    }
                    else if ("RM_Template_Column_Name_Classification" == col.columnName)
                    {
                        metaInfo.Add(col.uniqueId.ToString(), JsonConvert.SerializeObject(new TaxonomyColumnValue() { Id = rec.TermId.ToString(), Name = rec.TermName}));  
                    }
                    else if ("RM_Template_Column_Name_Status" == col.columnName)
                    {
                        string status = rowData[columnIndexDic["Status"]];
                        ColumnValueMapping map = this.ColumnValueMappings.FirstOrDefault(a => a.DescColumn == "Status" && a.SrcValue == status);
                        if(map == null)
                        {
                            throw new AveException("No value mapping for Status:{0}", status);
                        }
                        string recordsStatus = map.DestValue;
                        int statusInt = GetStauts(recordsStatus);
                        rec.RecordStatus = statusInt;
                        ChoiceColumnValue statusFiled = new ChoiceColumnValue()
                        {
                            Value = statusInt.ToString(),
                            Name = GetStautsName(statusInt)
                        };
                        metaInfo.Add(col.uniqueId.ToString(), JsonConvert.SerializeObject(statusFiled)); 
                    }
                    else if (col.allowEdit && this.columnIndexDic.ContainsKey(col.columnName))  //allow edit说明不是默认Column
                    {
                        string colValue = rowData[columnIndexDic[col.columnName]];
                        if(col.typeId == (int)Contract.Explorer.ColumnType.Text)
                        {
                            metaInfo.Add(col.uniqueId.ToString(), colValue);
                        }
                        else if (col.typeId == (int)Contract.Explorer.ColumnType.DateTime)
                        {
                            DateTime localTime = this.GetTimeLocal(colValue);
                            DateTimeColumnValue timeColumn = new DateTimeColumnValue() { Date = localTime, TimeZoneId = this.TimeZoneId, IsSetDayLight=true };
                            metaInfo.Add(col.uniqueId.ToString(), JsonConvert.SerializeObject(timeColumn));
                        }
                        else
                        {
                            logger.Warn("Not mapping column type {0}", (Contract.Explorer.ColumnType)col.typeId);
                        }
                    }
                    else
                    {
                        logger.Warn("Record File or column mapping file does not contains column {0}", col.columnName);
                    }
                }
            }
            return metaInfo;
        }

        private string validateHomeLocation(string homeLocation, RMNodeType nodeType)
        {
            if(homeLocation == null || homeLocation == string.Empty)
            {
                return homeLocation;
            }
            if(nodeType == RMNodeType.PhyBox)
            {
                if (homeLocation.StartsWith("At home:"))
                { 
                    return homeLocation.Substring(8, homeLocation.Length - 8);
                }
            }
            else if (nodeType == RMNodeType.PhyFile)
            { 
                if(homeLocation.Contains('(') && homeLocation.Contains(')'))
                {
                    int startIndex = homeLocation.IndexOf('(');
                    return homeLocation.Substring(0, startIndex);
                }
            }
            return homeLocation;
        }

        private int GetStauts(string statusStr)
        {
            if("Open".Equals(statusStr, StringComparison.OrdinalIgnoreCase))
            {
                return (int)RMRecordStatus.Active;
            }
            else if("Closed".Equals(statusStr, StringComparison.OrdinalIgnoreCase))
            {
                return (int)RMRecordStatus.Closed;
            }
            else if("Destroyed".Equals(statusStr, StringComparison.OrdinalIgnoreCase))
            {
                return (int)RMRecordStatus.Destroyed;
            }
            else if("Missing".Equals(statusStr, StringComparison.OrdinalIgnoreCase))
            {
                return (int)RMRecordStatus.Missing;
            }
            return (int)RMRecordStatus.None; 
        }
        private string GetStautsName(int statusInt)
        {
            RMRecordStatus status = (RMRecordStatus)statusInt;
            if (status == RMRecordStatus.Active)
            {
                return I18NEntity.GetString("RM_PRM_PRE_Column_Status_Open");
            }
            else if (status == RMRecordStatus.Closed)
            {
                return I18NEntity.GetString("RM_PRM_PRE_Column_Status_Closed");
            }
            else if (status == RMRecordStatus.Destroyed)
            {
                return I18NEntity.GetString("RM_PRM_PRE_Column_Status_Destroyed");
            }
            else if (status == RMRecordStatus.Missing)
            {
                return I18NEntity.GetString("RM_PRM_PRE_Column_Status_Missing");
            } 
            return "None";
        }

        private long GetTimeLong(string time)
        {
            //TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(this.TimeZoneId);
            DateTime temp = new DateTime();
            if(!DateTime.TryParseExact(time,  this.DateFormate, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out temp))
            {
                if(!DateTime.TryParse(time, out temp))
                {
                    logger.Error("Parse time failed, {0}", time);
                    return 0;
                }
            }
            DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(temp, GTimeZoneInfo??TimeZoneInfo.Local);
            return utcTime.Ticks;
        }

        private DateTime GetTimeLocal(string time)
        {
            DateTime temp = new DateTime();
            if(DateTime.TryParseExact(time,  this.DateFormate, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out temp))
            {
                return temp;
            }
            if(DateTime.TryParse(time, out temp))
            {
                return temp;
            }
            try
            {
                return Convert.ToDateTime(time);
            }
            catch (Exception e)
            {
                logger.Error("Parse time failed, {0}, {1}", time, e);
            }
            return temp;
        }

        Dictionary<int, RMPhysicalRecordSetting> GlocalSettingDic = new Dictionary<int, RMPhysicalRecordSetting>();
        private TaxonomyColumnValue GetDefaultTermId(RMLocation location)
        {
            RMLocation temp = location;
            RMLocation parent = this.locationDic.Values.FirstOrDefault(a => a.Id == temp.ParentId);
            while(parent?.NodeType != (int)RMNodeType.PhysicalRootLocation)
            { 
                temp = parent;
                parent = this.locationDic.Values.FirstOrDefault(a => a.Id == temp.ParentId);
            }
            logger.Info("Home Location is {0}", temp.Name);
            if (!GlocalSettingDic.ContainsKey(temp.Id))
            {
                RMPhysicalRecordSetting topLevelSetting = PhysicalRecordSettingDao.GetPhysicalRecordSetting(temp.UniqueId);
                GlocalSettingDic.Add(temp.Id, topLevelSetting);
            }
            if (GlocalSettingDic.ContainsKey(temp.Id))
            {
                return new TaxonomyColumnValue() { Id = GlocalSettingDic[temp.Id].DefaultTermId.ToString(), Name = GlocalSettingDic[temp.Id].DefaultTermName };
            }
            else
            {
                logger.Error("No Global physcial setting on location {0}", temp.Name);
                throw new Exception(string.Format("No Global physcial setting on location {0}", temp.Name));
            } 
        }

        #endregion

        #region Deal with Related

        public string RunImportRecordsRelated(JobRunBy jobRunBy, string upFilePath, int baseOn)
        {

            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ImportRecordsRelated,
                    Parameters = string.Format("{0} {1} ", upFilePath, baseOn),
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };
                id = mJobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while RunImportPhysicalFilesAndRecords,ERROR:{0}", ex.ToString());
            }

            return id;

        }
        //[Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.PhysicalItemImport, Action = AuditAction.PhysicalItemImportReport, AfterHandler = typeof(TermManagementAfterAuditHandler))]
        public async Task<string> RealRunImportRecordsRelatedAsync(JobRunBy jobRunBy, string jobRunByUser, string upFilePath, int settingId)
        {
            string id = string.Empty;
            if (jobRunBy == JobRunBy.Control)
            {
                id = JobMonitorService.CreateJob(JobType.ImportRecordsRelated, jobRunByUser);
                logger.Info("Begin control Import physical records Job {0}", id);
            }

            //List<string> runningImportPhyscailRecrodsJobs = JobMonitorService.GetRunningJobs(JobType.ImportRecordsRelated);

            //Import Term Job一次只能同时运行一个，所以判断当前起的Job是否要Skip掉
            bool isSkip = false;
            //if (runningImportPhyscailRecrodsJobs.Any(j => j != id))
            //{
            //    //isSkip = true;
            //}
            //if (!isSkip)
            {
                //新起线程起Job
                await StartImportRecordsRelatedAsync(id, upFilePath, settingId);
            }
            //else
            //{
            //    logger.Info(I18NEntity.GetString("Skipped this job. A physical files and records import job is already running."));
            //    JobMonitorService.UpdateJobStatus(id, Contract.RMWeb.JobMonitor.JobStatus.Skipped, "Skipped this job. A physical files and records import job is already running.");
            //}

            return id;
        }
        private async System.Threading.Tasks.Task StartImportRecordsRelatedAsync(string jobId, string upFilePath, int baseOn)
        {
            mJobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = jobId,
                JobType = JobType.ImportPhysicalRecords,
                CommandLine = string.Format("{0} {1} {2} {3}", JobType.ImportRecordsRelated, jobId, baseOn, (await mGeneralSettingService.GetGeneralSettingAsync()).TimeZoneId.Replace(" ", "_")),
            });

        } 
        #endregion

        public string StartDeletionJob(string upFilePath)
        {
            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.TrimRecordsDeletion,
                    Parameters = string.Format("{0}", upFilePath),
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };
                id = mJobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while StartImportedTRIMDeletionJob,ERROR:{0}", ex.ToString());
            }

            return id;

        }

        public async Task<string> RealRunImportRecordsDeletionAsync(JobRunBy jobRunBy, string jobRunByUser, string upFilePath)
        {
            string jobId = string.Empty;
            if (jobRunBy == JobRunBy.Control)
            {
                jobId = JobMonitorService.CreateJob(JobType.TrimRecordsDeletion, jobRunByUser);
                logger.Info("Begin control Import records deletion Job {0}", jobId);
            }
            
            List<string> runningImportPhyscailRecrodsJobs = JobMonitorService.GetRunningJobs(JobType.TrimRecordsDeletion);

            //Import Term Job一次只能同时运行一个，所以判断当前起的Job是否要Skip掉
            bool isSkip = false;
            if (runningImportPhyscailRecrodsJobs.Any(j => j != jobId))
            {
                //isSkip = true;
            }
            //if (!isSkip)
            //{
                upFilePath = "\"" + upFilePath + "\"";
                mJobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = jobId,
                    JobType = JobType.TrimRecordsDeletion,
                    CommandLine = string.Format("{0} {1} {2} {3}", JobType.TrimRecordsDeletion, jobId, upFilePath, (await mGeneralSettingService.GetGeneralSettingAsync()).TimeZoneId.Replace(" ", "_")),
                });
            //}
            //else
            //{
            //    logger.Info(I18NEntity.GetString("Skipped this job. An imported records deletion job is already running."));
            //    JobMonitorService.UpdateJobStatus(jobId, Contract.RMWeb.JobMonitor.JobStatus.Skipped, "Skipped this job. An imported records deletion job is already running");
            //}

            return jobId;
        }
        public string ClearSubFolders()
        {
            try
            {
                List<Record> allSubFolders = ExplorerDao.QueryAll(a => a.ScopeId == Guid.Empty && a.SendTo == "sub folder").ToList();
                logger.Info("Start to delete sub folders, count {0}", allSubFolders.Count);
                foreach (Record rec in allSubFolders)
                {
                    logger.Debug("Delete sub folder, Unique Id {0}, id {1}", rec.RecordsId, rec?.Id);
                    ExplorerDao.Delete(rec.CreateDate, rec.Id);
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                return e.Message;
            }
            return "ok";
        }

        public string DownloadSubFolderList(string downloadCSVFile)
        {
            List<Record> allSubFolders = ExplorerDao.QueryAll(a => a.ScopeId == Guid.Empty && a.SendTo == "sub folder").ToList(); 
            logger.Info("Start to download sub folder list, count {0}", allSubFolders.Count);

            string downloadToFilename = Path.Combine(RMGlobalConfiguration.StorageConfig[Contract.Configurations.RMStorageSettingKey.REPORT_TEMP_FOLDER], JobReportUtility.GetTenantIdentity(), downloadCSVFile); 
            var downloadFolder = Path.GetDirectoryName(downloadToFilename);
            if (!Directory.Exists(downloadFolder)) { Directory.CreateDirectory(downloadFolder); }
            logger.Info("Sub folder list download file:{0}", downloadToFilename);
            using(FileStream fs = new FileStream(downloadToFilename, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(string.Join(",", new string[] {"Record Id", "Unique Id", "Title", "Created By", "Record Status" }));
                    foreach(Record rec in allSubFolders)
                    {
                        sw.WriteLine(string.Join(",", new string[] { rec.Id.ToString(), rec.RecordsId, rec.LeafName, rec.CreatedBy, ((RMRecordStatus)rec.RecordStatus).ToString() }));
                    }
                }
            }
            return downloadToFilename;
        }
    }
}
