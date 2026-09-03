using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Common;
using RADownloadCenter;
using System.Text;

namespace RADownloadCentre.PickMoveExport
{
    public class ExportMovePickListProcessor : GenerateAndUploadFileExecutor
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ExportMovePickListProcessor));

        private static readonly RMRetryer Retryer = RMRetryerBuilder.CreateBuilder().Build();

        private static IPhysicalRecordsMoveDataTableDao _moveDataTableDao => PlatformWindsorManager.GetService<IPhysicalRecordsMoveDataTableDao>();
        private static IRMSubJobDao _subJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();

        private readonly BaseJobDto BaseJobDto;

        private readonly int PageSize = 1000;

        private readonly string FolderPath;

        private string FilePath;

        private readonly string JobId;
        private readonly string SubJobId;

        private readonly int CountOfOneSheet = 200000;
        protected override string BaseJobId => JobId;
        public ExportMovePickListProcessor(string subJobId, string jobId)
        {
            BaseJobDto = new BaseJobDto()
            {
                Id = jobId,
                JobType = (int)JobType.PhysicalMovePickExportJob
            };
            JobId = jobId;
            SubJobId = subJobId;
            GenerateAndUploadFileManager.Init(jobId, JobType.PhysicalMovePickExportJob);
            FolderPath = JobReportUtility.GetDownloadReportDetailTempleFolder(BaseJobDto);
            FilePath = JobReportUtility.GetDownloadReportDetailTempleFolder(BaseJobDto, BaseJobDto.Id.Replace("EPLM", I18NEntity.GetString("RM_JS_Phy_MovePickExport") + "_"), ".csv");
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
        }
        protected override ArchiverExportReportDto ExportReportDto => throw new NotImplementedException();

        protected override async Task GenerateDataAsync()
        {
            RMSubJob subJobWithContext = _subJobDao.GetSubJob(SubJobId, true);
            Logger.Info("Get job message:{0}", subJobWithContext.JobContext.Content);
            var jobParam = SerializerHelper.DeserializeByDataContractSerializer<PickMoveListJobMessage>(subJobWithContext.JobContext.Content);
            var pageIndex = 0;
            var currentCount = 0;
            var isCreateHeader = true;
            var sheetIndex = 0;
            var (moveDatas, totalCount) = await _moveDataTableDao.GetMoveDatasPagination(TenantLocalValue.LogonGroupId,jobParam.ActionParam, pageIndex, PageSize);
            do
            {
                try
                {
                    currentCount += moveDatas.Count();
                    var datas = new string[moveDatas.Count() + 1][];
                    pageIndex++;
                    if (isCreateHeader)
                    {
                        currentCount += 1;
                        datas = GenerateMoveData(datas, moveDatas, true);
                        ReportUtil.ExportDataToCsv(datas, FilePath);
                        isCreateHeader = false;
                        Logger.Info($"Create Excel with header success,current count is {currentCount}");
                        continue;
                    }

                    if (currentCount >= CountOfOneSheet)
                    {
                        sheetIndex++;
                        datas = GenerateMoveData(datas, moveDatas, true);
                        FilePath = JobReportUtility.GetDownloadReportDetailTempleFolder(BaseJobDto, $"_{sheetIndex}" + ".csv");
                        ReportUtil.ExportDataToCsv(datas, FilePath);
                        currentCount = moveDatas.Count();
                        Logger.Info($"Insert Excel with header success,current count is {currentCount},current sheet index is {sheetIndex}");
                        continue;
                    }

                    datas = GenerateMoveData(datas, moveDatas, false);
                    ReportUtil.ExportDataToCsv(datas, FilePath);
                    Logger.Info($"Insert data to sheet success,current count is {currentCount},current sheet index is {sheetIndex}");

                }
                catch (Exception e)
                {
                    Logger.Error($"Generate report detail to Excel error,current count is {currentCount},currrent sheet index is {sheetIndex},error : {e}");
                    GenerateAndUploadFileManager.HasFailed = true;
                    throw;
                }

            } while ((moveDatas = (await _moveDataTableDao.GetMoveDatasPagination(TenantLocalValue.LogonGroupId, jobParam.ActionParam, pageIndex, PageSize)).Item1).Any());
        }

        protected override async Task UploadBlobAsync()
        {
            AvePoint.GCommon.ZipUtil.ZipFolder(FolderPath, FolderPath + ".zip", Encoding.UTF8);
            var customId = TenantLocalValue.LogonGroupId;
            var blobName = SecurityUtils.SafeCombinePath(customId, JobId + ".zip");
            try
            {
                await Retryer.RetryAsync(() =>
                {
                    blobName = DownloadCenterUtility.UploadStorageForDownloadCenter(blobName, FolderPath + ".zip");
                    Logger.Info($"Upload move data success");
                    return Task.CompletedTask;
                });
            }
            catch (Exception e)
            {
                Logger.Error($"Upload move data failed,error is :{e}");
                throw;
            }

            Logger.Info($"finish to upload blob name:{blobName}");
            fileInfo = new FileInfo(FolderPath + ".zip");
        }

        public string[][] GenerateMoveData(string[][] datas, IEnumerable<PhysicalRecordMoveData> moveDatas, bool isCreateHeader)
        {
            try
            {
                if (isCreateHeader)
                {
                    datas = AssembleHeaderTittle(datas);
                }
                return ConvertMoveDataToArray(moveDatas, datas);
            }
            catch (Exception e)
            {
                Logger.Error($"Generate report for export job failed {e}");
                throw;
            }
        }

        private string[][] ConvertMoveDataToArray(IEnumerable<PhysicalRecordMoveData> moveDatas, string[][] datas)
        {
            int rowCount = 1;
            if (datas.Length < 1)
            {
                Logger.Error("The datas array is empty, cannot convert move data to array.");
                return datas;
            }
            if (datas[0] == null) rowCount = 0;
            foreach (var moveData in moveDatas)
            {
                try
                {
                    var colIndex = 0;
                    datas[rowCount] = new string[7];
                    datas[rowCount][colIndex++] = moveData.ItemName;
                    datas[rowCount][colIndex++] = moveData.UniqueId;
                    datas[rowCount][colIndex++] = moveData.ApproveBy;
                    datas[rowCount][colIndex++] = moveData.HomeLocation;
                    datas[rowCount][colIndex++] = moveData.DestinationPath;
                    datas[rowCount][colIndex++] = moveData.Status == (int)PickMoveStatusType.Successfull ? I18NEntity.GetString("RM_JS_Phy_PickMove_PeddingMove") : I18NEntity.GetString("RM_RC_Audit_Status_Failed");
                    datas[rowCount][colIndex++] = I18NEntity.GetString(moveData.Comment);
                    rowCount++;
                }
                catch (Exception e)
                {
                    Logger.Error($"Convert move data to array failed {e}");
                    rowCount++;
                    throw;
                }
            }
            return datas;
        }
        private static string[][] AssembleHeaderTittle(string[][] data)
        {
            var rowIndex = 0;
            var colIndex = 0;
            data[rowIndex] = new string[7];
            data[rowIndex][colIndex++] = I18NEntity.GetString("RM_PRM_MyRequest_ItemName");
            data[rowIndex][colIndex++] = I18NEntity.GetString("RM_PRM_RequestManagement_UniqueId");
            data[rowIndex][colIndex++] = I18NEntity.GetString("RM_MT_PickList_Column_ApproveBy");
            data[rowIndex][colIndex++] = I18NEntity.GetString("RM_BCM_Audit_OriginalLocationMove");
            data[rowIndex][colIndex++] = I18NEntity.GetString("RM_BCM_Audit_MoveToDestination");
            data[rowIndex][colIndex++] = I18NEntity.GetString("RM_MT_PickList_Column_Status");
            data[rowIndex][colIndex++] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return data;
        }
    }
}
