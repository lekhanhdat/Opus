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

using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.FileTransfer;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.Wrapper.Common;
using RAGoogle.Helper;
using RAGoogle.Restore.Content;
using RAGoogle.Restore.Report;
using RAGoogle.Services;
using RAGoogle.Models;
using RAGoogle.Report;
using RAGoogle.Util;
using System.Reflection;
using RAGoogle.Archive.Wrapper;
using Microsoft.SharePoint.Client;
using RAGoogle.Restore.Common;
using RAArchiverCommon;
using AvePoint.RA.Common;

namespace RAGoogle.Restore
{
    public class GDriveRestoreBase : IDisposable
    {
        protected static readonly AveLogger _logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private Exception mError;

        public IAveRestoreStream RestoreStream { get; set; }
        public GDriveRestoreConfig Config { get; set; }
        public GDriveRestoreContentReader ContentReader { get; set; }
        public IFileReceiver FileReceiver { get; set; }
        public char ReplaceType { get; set; }
        public RAGoogle.Restore.Content.RestoreTreeNode RestoreTree { get; set; }

        public JobReportImps Report { get; set; }
        public ReportCenter ReportCenter { get; set; }
        //public GoogleDriveService GoogleDriveService { get; set; }
        //public GoogleDriveHelper GoogleDriveHelper { get; set; }
        public bool IsEnduserRestore { get; set; }
        public bool IsForceDeleteStub { get; set; }
        public bool SetNowAsRestoreFileModifyTime { get; set; }
        public string OopStubUrl { get; set; }
        public string PossiblyStubType { get; set; }
        public GoogleDriveData GoogleDriveData { get; set; }
        public AveGDrive AveGDrive { get; set; }
        public AveGDFolder CurrentFolder { get; set; }
        public Exception Error { get { return mError; } set { mError = value; } }
        private const string TempFileName = "restorefile";


        protected virtual void AddReport(AveRestoreReportDto reportDto)
        {
            try
            {
                ReportCenter.ReportManager.Increase();
                this.ReportCenter.AddGoogleDriveRestoreReport(reportDto.DriveId, reportDto.SourcePath, reportDto.Size, reportDto.Path, (int)reportDto.Status, reportDto.Type, reportDto.ErrorMessage);
            }
            catch (Exception e)
            {
                _logger.Warn(@"Looks up a localized string similar to An error occurred while adding restore report. Path: {0}, type: {1} {2}", reportDto.Title, reportDto.Type, e);
            }
        }
        protected void AddReport(IEnumerable<AveRestoreReportDto> reportDtos)
        {
            foreach (var reportDto in reportDtos)
            {
                AddReport(reportDto);
            }
        }

        protected virtual void WaitForItems(bool isEndOfJob)
        {
        }

        protected virtual void IsItemHasDepedenciesList()
        {
        }

        public virtual void PostProcess()
        {
            WaitForItems(true);
        }



        public async Task ProcessForOpus()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("GranularRestore.Process"))
            {
                try
                {
                    _logger.Info("Looks up a localized string similar to Begin restoring....");
                    Init();
                    RestoreContentDto dto;
                    while ((dto = ContentReader.MoveNext()) != null)
                    {
                        _logger.Info(@"Looks up a localized string similar to The restore content is received from media. Name: {0}. Type: {1}.", dto.UniqueId, dto?.Type);
                        RestoreStream.Reset();
                        if (!dto.ReplaceType.Equals('\0'))
                        {
                            ReplaceType = dto.ReplaceType;
                        }
                        try
                        {
                            using (new CheckJobStopScope()) { }

                            switch (dto.Type)
                            {
                                case GDriveDataType.MyDrive:
                                case GDriveDataType.SharedDrive:
                                    await RestoreDrive(dto);
                                    break;
                                case GDriveDataType.Folder:
                                    await RestoreFolder(dto);
                                    break;

                                case GDriveDataType.File:
                                case GDriveDataType.FileVersion:
                                    await RestoreFile(dto);
                                    break;

                                default:
                                    _logger.Warn(@"Looks up a localized string similar to Unknown object type: {0}.", dto.Type);
                                    break;
                            }
                        }
                        catch (JobStopException ex)
                        {
                            _logger.Warn("job is stopped by manual");
                            throw;
                        }
                        catch (Exception e)
                        {
                            _logger.Error(@"Looks up a localized string similar to An error occurred while doing the restore job. Type: {0},exception:{1}", dto.Type, e);
                            mError = e;
                        }
                    }
                }
                catch (JobStopException ex)
                {
                    _logger.Warn("job is stopped by manual");
                    throw;
                }
                catch (Exception e)
                {
                    _logger.Error(@"Looks up a localized string similar to An error occurred while receiving backup data from media.{0}", e);
                    ReportCenter.HasErrorNode = true;
                    if (e.Message.Contains("Cannot find the index with the path"))
                    {
                        ReportCenter.SummaryComments = "RM_JM_RestoreFaild_IndexNotExsit_ErrorMessage";
                    }
                    else if (e.Message.Contains("This site has the maximum number of lists and libraries"))
                    {
                        ReportCenter.SummaryComments = "RM_JM_RestoreFaild_OutOfListCountLimit_ErrorMessage";
                    }
                    mError = e;
                }
                finally
                {
                    PostProcess();
                }
            }
        }
        public async System.Threading.Tasks.Task Process()
        {
            if (IsEnduserRestore)
            {
                ProcessForEndUser();
            }
            else
            {
                await ProcessForOpus();
            }
        }
        public void ProcessForEndUser()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("GranularRestore.Process"))
            {
                try
                {
                    _logger.Info("Looks up a localized string similar to Begin restoring....");
                    Init();
                    RestoreContentDto dto;
                    while ((dto = ContentReader.MoveNext()) != null)
                    {
                        _logger.Info(@"Looks up a localized string similar to The restore content is received from media. Name: {0}. SrcName: {1}. Type: {2}.", dto.UniqueId, dto?.SrcUrl, dto?.Type);
                        RestoreStream.Reset();
                        if (!dto.ReplaceType.Equals('\0'))
                        {
                            ReplaceType = dto.ReplaceType;
                        }
                        try
                        {
                            using (new CheckJobStopScope()) { }
                        }
                        catch (JobStopException ex)
                        {
                            _logger.Warn("job is stopped by manual");
                            throw;
                        }
                        catch (Exception e)
                        {
                            _logger.Error(@"Looks up a localized string similar to An error occurred while doing the restore job. Type: {0},exception:{1}", dto.Type, e);
                            mError = e;
                        }
                    }
                }
                catch (JobStopException ex)
                {
                    _logger.Warn("job is stopped by manual");
                    throw;
                }
                catch (Exception e)
                {
                    _logger.Error(@"Looks up a localized string similar to An error occurred while receiving backup data from media.{0}", e);
                    mError = e;
                }
                finally
                {
                    PostProcess();
                }
            }
        }
        #region Need OverRide in sub Class

        public virtual void Init()
        {
            RestoreStream = new WrapperRestoreStreamV2(new FileReceiverWrapper(FileReceiver));
            ContentReader = new GDriveRestoreContentReader(RestoreStream, Config, RestoreTree);
        }

        public virtual async Task RestoreDrive(RestoreContentDto aveSiteDto)
        {
            using var _ = new PerformanceScope("GDriveRestoreBase.RestoreDrive");
            AveRestoreReportDto reportDto = new AveRestoreReportDto { DriveId = aveSiteDto.DriveId, Type = I18NResource.ObjectLevelGoogleDrive, Status = RestoreStatus.Success, ErrorMessage = string.Empty };          
            try
            {
                var driveNode = Config.ArchiverConfigForMedia.TreeRoot;
                GoogleDriveData = ConvertHelper.ConvertDtoNodeTreeToData(driveNode, Config.appProfile.TenantId);
                AveGDrive = new AveGDrive(Config.appProfile, GoogleDriveData, GoogleActionType.Restore);
                AveGDrive.ReportCenter = ReportCenter;
                AveGDrive.AveRestoreReportDto = reportDto;
                AveGDrive.ConflictResolution = Config.ContainerConflictResolution;
                _logger.Info($"Begin restore drive, source id:{aveSiteDto.Id}, name:{aveSiteDto.DriveName}.");
                await AveGDrive.RestoreSelf(aveSiteDto);
                _logger.Info($"Begin restore metadata, target id:{AveGDrive.DriveProxy.Id}, name:{AveGDrive.DriveProxy.Name}");
                AveMetadata metadata;
                while ((metadata = RestoreStream.ReadMetadata()) != null)
                {
                    _logger.Info($"Processing metadata of type: {metadata.MetadataType} for drive: {AveGDrive.DriveProxy.Id}");
                    switch (metadata.MetadataType)
                    {
                        case AveMetadataType.DriveBasicInfo:
                            var driveBasicInfo = metadata.GetMetadata<GoogleDriveBasic>();
                            await AveGDrive.HandleRestoreDriveBasic(driveBasicInfo, aveSiteDto);
                            break;
                        case AveMetadataType.DriveMembers:
                            var driveMemberInfo = metadata.GetMetadata<List<GoogleDriveMember>>();
                            await AveGDrive.HandleRestoreDriveMember(driveMemberInfo, aveSiteDto);
                            break;
                        case AveMetadataType.DriveSetting:
                            var driveSettingInfo = metadata.GetMetadata<GoogleDriveSetting>();
                            await AveGDrive.HandleRestoreDriveSetting(driveSettingInfo, aveSiteDto);
                            break;
                    }

                }
                //read tail
                if (RestoreStream == null)
                {
                    var tail = ContentReader.GetFileTail();
                }
                else
                {
                    var tail = RestoreStream.ReadTail();
                }
            }
            catch(Exception ex)
            {
                AveGDrive.AveRestoreReportDto.Status = RestoreStatus.Failed;
                AveGDrive.AveRestoreReportDto.ErrorMessage = ex.Message;
                _logger.Error($"Restore Google Drive Error ! {ex}");
                throw;
            }
            finally
            {
                _logger.Info($"Restore drive finish.");
                ReportCenter.SummaryComments = AveGDrive.AveRestoreReportDto.ErrorMessage;
                AddReport(AveGDrive.AveRestoreReportDto);
            }
            
        }

        public virtual async Task RestoreFile(RestoreContentDto aveItemDto)
        {
            using var _ = new PerformanceScope("GDriveRestoreBase.RestoreFile");
            AveRestoreReportDto reportDto = new AveRestoreReportDto {DriveId = aveItemDto.DriveId, Type = I18NResource.ObjectLevelFile, SourcePath = aveItemDto.SrcName, Path = aveItemDto.SrcUrl, Status = RestoreStatus.Success, ErrorMessage = string.Empty };
            try
            {
                if (aveItemDto.Type == GDriveDataType.File)
                {
                    SOArchiverJobInfoStatistics.Instance.FileCurrentVersionCount++;
                }
                else if (aveItemDto.Type == GDriveDataType.FileVersion)
                {
                    SOArchiverJobInfoStatistics.Instance.FileHisVersionCount++;
                }
                AveGDFile aveGDFile = null;
                AveMetadata metadata;
                _logger.Info($"Begin restore file, id:{aveItemDto.Id}.");

                while ((metadata = RestoreStream.ReadMetadata()) != null)
                {
                    _logger.Info($"Processing metadata of type: {metadata.MetadataType} for file: {aveItemDto.Id}");
                    switch (metadata.MetadataType)
                    {
                        case AveMetadataType.DriveFileMetadata:
                            var fileMetaData = metadata.GetMetadata<GDFileBasic>();
                            if (aveItemDto.Type == GDriveDataType.FileVersion)
                            {
                                reportDto.Type = I18NResource.ObjectLevelGoogleDriveFileVersion;
                                reportDto.SourcePath = $"{aveItemDto.SrcName}:{aveItemDto.Version}";
                                reportDto.Path = $"{aveItemDto.SrcUrl}:{aveItemDto.Version}";
                            }
                            GlobalCache.Instance.ObjectIdMappings.TryGetValue(aveItemDto.ParentId, out var realParentId);
                            if (aveItemDto.DriveId == aveItemDto.ParentId || realParentId == AveGDrive.DriveProxy.Id)
                            {
                                aveGDFile = new AveGDFile(AveGDrive);
                            }
                            else
                            {
                                aveGDFile = new AveGDFile(CurrentFolder);
                            }
                            aveGDFile.ConflictResolution = Config.ContentConflictResolution;
                            aveGDFile.AveRestoreReportDto = reportDto;
                            aveGDFile.ReportCenter = ReportCenter;
                            await aveGDFile.HandleRestoreFileMetaData(aveItemDto, fileMetaData, RestoreStream);
                            break;
                        case AveMetadataType.DriveFilePermission:
                            if (aveGDFile != null)
                            {
                                var filePermissions = metadata.GetMetadata<List<PermissionInfo>>();
                                await aveGDFile.HandleRestoreObjectPermission(new GDPermissionList() { Permissions = filePermissions });
                            }
                            break;
                    }
                }
            
                //read tail
                if (RestoreStream == null)
                {
                    var tail = ContentReader.GetFileTail();
                }
                else
                {
                    var tail = RestoreStream.ReadTail();
                }
            }
            catch (Exception ex)
            {
                reportDto.Status = RestoreStatus.Failed;
                reportDto.ErrorMessage = ex.Message;
                _logger.Error($"Failed to restore drive file,exception:{ex}");
            }
            finally
            {
                ReportCenter.SummaryComments = reportDto.ErrorMessage;
                AddReport(reportDto);
            }            
        }
        public virtual async Task RestoreFolder(RestoreContentDto aveFolderDto)
        {
            using var _ = new PerformanceScope("GDriveRestoreBase.RestoreFolder");
            AveRestoreReportDto reportDto = new AveRestoreReportDto {DriveId = aveFolderDto.DriveId, Type = I18NResource.ObjectLevelFolder, SourcePath = aveFolderDto.SrcName, Path = aveFolderDto.SrcUrl, Status = RestoreStatus.Success, ErrorMessage = string.Empty };

            try
            {
                ReportCenter.SummaryComments = string.Empty;
                AveGDFolder aveGDFolder = null;
                _logger.Info($"Begin restore folder, source id:{aveFolderDto.Id}.");
                AveMetadata metadata;
                while ((metadata = RestoreStream.ReadMetadata()) != null)
                {
                    _logger.Info($"Processing metadata of type: {metadata.MetadataType} for folder: {aveFolderDto.Id}");
                    switch (metadata.MetadataType)
                    {
                        case AveMetadataType.DriveFolderMetadata:
                            var folderBasicInfo = metadata.GetMetadata<GDFileBasic>();
                            GlobalCache.Instance.ObjectIdMappings.TryGetValue(aveFolderDto.ParentId, out var realParentId);
                            if (aveFolderDto.DriveId == aveFolderDto.ParentId || realParentId == AveGDrive.DriveProxy.Id)
                            {
                                aveGDFolder = new AveGDFolder(AveGDrive);
                            }
                            else
                            {
                                aveGDFolder = new AveGDFolder(CurrentFolder);
                            }
                            CurrentFolder = aveGDFolder;
                            aveGDFolder.ConflictResolution = Config.ContainerConflictResolution;
                            aveGDFolder.AveRestoreReportDto = reportDto;
                            await aveGDFolder.HandleRestoreGoogleFolderBasicInfo(folderBasicInfo);
                            break;
                        case AveMetadataType.DriveFolderPermission:
                            if(aveGDFolder != null)
                            {
                                var permissionInfo = metadata.GetMetadata<GDPermissionList>();

                                await aveGDFolder.HandleRestoreObjectPermission(permissionInfo);
                            }
                            break;
                    }
                }
                if (RestoreStream == null)
                {
                    var tail = ContentReader.GetFileTail();
                }
                else
                {
                    var tail = RestoreStream.ReadTail();
                }
            }
            catch (Exception ex)
            {
                reportDto.Status = RestoreStatus.Failed;
                reportDto.ErrorMessage = ex.Message;
                _logger.Error($"Restore Google Drive Folder Error ! {ex}");
            }
            finally
            {
                ReportCenter.SummaryComments = reportDto.ErrorMessage;
                AddReport(reportDto);
            }          
        }
     
        #endregion

        public virtual void Dispose()
        {
            _logger.Info("Looks up a localized string similar to The Restore Base has disposed..");
            try
            {
                //AveItemRestorePauseResume.SendCloseResponse();
                FileReceiver.Close(this.mError == null ? string.Empty : this.mError.Message);
            }
            catch (Exception e)
            {
                _logger.Error(@"Looks up a localized string similar to An error occurred while closing file receiver.{0}", e.ToString());
                //AveRestoreReportDto reportDto = new AveRestoreReportDto();
                //reportDto.Status = RestoreStatus.Failed;
                //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(e,RestoreReportKey.Item_DisposeError.ToString(), RestoreReportResource.Item_DisposeError, e.Message);
                //AddReport(reportDto);
            }
            //RestoreResultInfo resultInfo = null;
            if (this.mError != null)
            {
                //这里如果给一个默认的key，如果出现的异常没有key就能显示默认key的国际化内容
                string errorMessage = AveWrapperHandleErrorMessage.GetFormateErrorMessage(this.mError, string.Empty, this.mError.Message);
                //if (this.mError is PauseProcessException)
                //{
                //    resultInfo = new RestoreResultInfo() { JobStatus = JobStatus.Stopped };
                //    Report.Finish(resultInfo, errorMessage);
                //    return;
                //}
                if (!this.mError.Message.StartsWith("Error because of insufficient", StringComparison.OrdinalIgnoreCase))
                {
                    if (this.mError is AvePoint.GCommon.Network.BlockQueueSyncException ||
                        this.mError is AvePoint.GCommon.Network.ClosedWithErrorException ||
                        this.mError is AvePoint.GCommon.Network.NetworkBrokenException)
                    {
                        //resultInfo = new RestoreResultInfo() { JobStatus = JobStatus.Failed };
                        //resultInfo.AddARestoreError(RestoreReportResource.Item_MediaError, RestoreReportKey.Item_MediaError.ToString(), new string[] { });
                    }
                }
                //if (resultInfo == null)
                {
                    // resultInfo = new RestoreResultInfo(JobStatus.Failed, errorMessage);
                    //resultInfo = new RestoreResultInfo() { JobStatus = JobStatus.Failed };
                    //resultInfo.AddARestoreError(this.mError.Message, null, new string[] { });
                }
                //Report.Finish(resultInfo, errorMessage);
            }
            else
            {
                // Report.Finish(JobStatus.Finished, null);
                //resultInfo = new RestoreResultInfo() { JobStatus = JobStatus.Finished };
                //Report.Finish(resultInfo,string.Empty);
            }
        }
    }
}