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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Model.Discovery.Salesforce;
using AvePoint.RA.DB.Model.Discovery.Salesforce.Enum;
using AvePoint.RA.DB.Model.Salesforce;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Discovery.Cache;
using AvePoint.RA.Service.Services.Discovery.Salesforce.License;
using AvePoint.Records.Core.Utilities.Extensions;
using RASalesforce;
using RASalesforce.APIs;
using RASalesforce.DataObject;
using RASalesforce.Util;
using DateTime = System.DateTime;
using JobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus;

namespace AvePoint.RA.Service.Services.Discovery.Salesforce.Work.Analyzer;

public class RMSFAnalyzeObjects : RMSFBaseProcessor
{
    private SalesforceService _salesforceService;
    
    
    public override async Task RunAsync()
    {
        var groupObjectJobByOrganizations = SfObjectJobs.GroupBy(sObject => sObject.OrganizationId).Select(group => new
        {
            OrganizationId = group.Key,
            ListSfObjects = group.ToList()
        });
        var (has, mainJob) = await _jobDao.TryGetMainJobAsync(RMDiscoveryJobStatus.Running);
        foreach (var groupObjectJobByOrganization in groupObjectJobByOrganizations)
        {
            try
            {
                Cts.Config();
            
                var customerId = TenantLocalValue.LogonGroupId;
            
                _salesforceService = new SalesforceService(customerId, groupObjectJobByOrganization.OrganizationId).Build();

                CheckApiUsageUtility.Start(_salesforceService, SubJobInfo.Id);
                
                if (SalesforceAPIHelper.Instance.IsNeedPostPond)
                {
                    _logger.Warn("Salesforce API usage is over 80%, Pausing the job.");
                    await Task.Delay(24 * 60 * 60 * 1000);
                }

                var sfApiObjects =
                    await _salesforceService.GetDetailObjectsAsync(SfObjectJobs.Select(sObject => sObject.Name).ToList());
            
                await OperateSfObjects((groupObjectJobByOrganization.OrganizationId, groupObjectJobByOrganization.ListSfObjects), sfApiObjects);
                
                var cacheManager = new RMDiscoveryCacheManager(groupObjectJobByOrganization.OrganizationId, RMDiscoveryCacheDataSource.Salesforce);
                await cacheManager.ClearAsync();
            }
            catch (JobStopException)
            {
                _logger.Warn("The job has stopped.");
                ReportCenter.JobHasStopped = true;
                await Cts.CancelAsync();
                await SetDiscoveryJob(mainJob, RMDiscoveryJobStatus.Failed);
                await RMDiscoverySalesforceLicenseHelper.DecreaseConsumedFrequencyPerYearAsync();
                await ExecutionInfoDao.DeleteByMainJobIdAsync(mainJob.Id);
                throw new JobStopException("The job has stopped.");
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occurred while process job. Error: {ex}");
                ReportCenter.RecordFailedCommon(ReportCenter.GenerateCommonJobDetail(JobType.SFDiscoveryJob, new RMDiscoverySalesforceObjectInfo { DisplayName = groupObjectJobByOrganization.OrganizationId }, JobDetailsStatus.Failed, ex.Message));
            }
            finally
            {
                ReportCenter.Completed();
                Cts.Dispose();
            }
            
        }
    }

    private async Task OperateSfObjects(
        (string organizationId, List<SfObjectJobDto>) organization, List<SFObjectProxy> sfApiObjects)
    {
        int totalNodeCount = organization.Item2.Count;
        int currentNodeIndex = 0;
        await Parallel.ForEachAsync(organization.Item2, new ParallelOptions()
        {
            CancellationToken = Cts.Token,
            MaxDegreeOfParallelism = 15
        }, async (sfObjectJob, token) =>
        {
            bool isCurrentObjectFailed = false;
            try
            {
                _logger.Info($"Start to perform {sfObjectJob.Name}");

                ReportCenter.Init(sfObjectJob.Name, organization.organizationId);

                var sfApiObject = sfApiObjects.Find(sfObject => sfObject.Name == sfObjectJob.Name)!;

                var objectType = GetObjectType(sfApiObject);

                var (canQuery, newSObjectInfo) = await CreateObjectInfo(objectType, sfApiObject, token);
                
                await SalesforceDiscoveryJobDao.AddNewObjectInfoAsync(organization.organizationId, newSObjectInfo);
                
                _logger.Info($"created object{sfObjectJob.Name}");

                if (canQuery)
                {
                    switch (objectType)
                    {
                        case RMDiscoverySalesforceObjectType.StandardObject or RMDiscoverySalesforceObjectType.CustomObject:
                            await CreateInActiveRecordData(sfApiObject, newSObjectInfo, organization.organizationId, token);
                            break;
                        case RMDiscoverySalesforceObjectType.FileObject:
                            await CreateInActiveBasicData(sfApiObject, newSObjectInfo, organization.organizationId, token);
                            break;
                        case RMDiscoverySalesforceObjectType.AttachmentObject:
                            await CreateInActiveAttachmentData(sfApiObject, newSObjectInfo, organization.organizationId, token);
                            break;
                    } 
                }
                
                _logger.Info($"end to perform {sfObjectJob.Name}");
            }
            catch (JobStopException ex)
            {
                _logger.Info($"job stop exception: {ex.Message}");
                ReportCenter.JobHasStopped = true;
                throw new JobStopException("The job has stopped.");
            }
            catch (Exception ex)
            {
                isCurrentObjectFailed = true;
                _logger.Error($"Failed to create object info {sfObjectJob.Name}, exception:{ex}");
                ReportCenter.RecordFailedCommon(ReportCenter.GenerateCommonJobDetail(JobType.SFDiscoveryJob, new RMDiscoverySalesforceObjectInfo{ DisplayName = sfObjectJob.Name}, JobDetailsStatus.Failed, ex.Message));
            }
            finally
            {
                if (ReportCenter.GetMainJobState() is JobStatus.Stopping or JobStatus.Failed or JobStatus.Stopped)
                {
                    _logger.Warn("The main job is stopping, need to stop all sub job.");
                    throw new JobStopException("The job has stopped.");
                }

                if (!ReportCenter.JobHasStopped && !isCurrentObjectFailed)
                {
                    int nextProgress = 100 * ++currentNodeIndex / totalNodeCount;
                    var currentProgress = ReportCenter.GetProgress(SubJobInfo.Id);
                    if (currentProgress < nextProgress)
                    {
                        _logger.Info($"Update progress from {currentProgress} to {nextProgress} for: [{sfObjectJob.Name}].");
                        ReportCenter.SetProgress(SubJobInfo.Id, nextProgress);
                    }
                }
            }
        });
    }

    private RMDiscoverySalesforceObjectType GetObjectType(SFObjectProxy sfApiObject)
    {
        return 
            RMSFObjectUtil.IsFileStorageObject(sfApiObject.Name) switch
            {
                true when sfApiObject.Name == "Attachment" => RMDiscoverySalesforceObjectType.AttachmentObject,
                false when sfApiObject.Custom == false => RMDiscoverySalesforceObjectType
                    .StandardObject,
                false when sfApiObject.Custom => RMDiscoverySalesforceObjectType.CustomObject,
                _ => RMDiscoverySalesforceObjectType.FileObject,
            };
    }

    private async Task CreateInActiveAttachmentData(SFObjectProxy sfApiObject, RMDiscoverySalesforceObjectInfo newSObjectInfo,
        string organizationId, CancellationToken token = default)
    {
        _logger.Info($"start to create inactive record data {sfApiObject.Name}");
        List<RMDiscoverySalesforceFileInactiveData> aggregatedData = [];
        List<string> errors = [];
        var modifiedRangeIds = ModifiedRanges.Select(sizeRange => sizeRange.Id).Concat([-1]).Order().ToList();
        for (int sizeRange = 1; sizeRange <= SizeRanges.Count; sizeRange++)
        {
            int index = 0;
            foreach (var modifiedRange in modifiedRangeIds)
            {
                try
                {
                    var query = new RecordQuery
                    {
                        Filters = []
                    };
                    AddLastModifiedDate(query, index, modifiedRange);
                    AddSizeRange(query, SizeRanges[sizeRange -1], "BodyLength");
                    index += 1;
                    var files =
                        await _salesforceService.GetAttachmentsAsync(
                            sfApiObject.GetDescribeSObjectResult(), query);

                    var groupedResult = files.GroupBy(file => new
                    {
                        file.FileExtension,
                    });

                    foreach (var group in groupedResult)
                    {
                        var totalSize = group.Sum(record => record.Size);
                        var totalCount = group.Count();

                        aggregatedData.Add(new RMDiscoverySalesforceFileInactiveData
                        {
                            ObjectId = newSObjectInfo.Id,
                            ModifiedDateRange = modifiedRange,
                            FileExtension = group.Key.FileExtension.IsNullOrEmpty() ? RMConstants.UNKNOW : group.Key.FileExtension,
                            TotalFileCount = totalCount,
                            TotalFileSize = totalSize,
                            SizeRange = sizeRange
                        });
                    }
                }
                catch (JobStopException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to create inactive record data of {sfApiObject.Name}, exception:{ex}");
                    errors.Add(ex.Message);
                }
            }
        }

        var updateList = aggregatedData.Where(record => record.TotalFileCount > 0).ToList();
        await SalesforceDiscoveryJobDao.AddFileInactiveDatasAsync(organizationId,
            updateList);
        _logger.Info($"end to create inactive record data {sfApiObject.Name}");
        var objectStatus = ReportCenter.GetObjectStatus(errors, updateList);
        SendJobDetail(objectStatus, errors, newSObjectInfo);
    }

    private async Task CreateInActiveBasicData(SFObjectProxy sfApiObject, RMDiscoverySalesforceObjectInfo newSObjectInfo,
        string organizationId, CancellationToken token = default)
    {
        _logger.Info($"start to create inactive record data {sfApiObject.Name}");
        List<RMDiscoverySalesforceFileInactiveData> dataList = [];
        List<string> errors = [];
        var modifiedRangeIds = ModifiedRanges.Select(sizeRange => sizeRange.Id).Concat([-1]).Order().ToList();
        for (int sizeRange = 1; sizeRange <= SizeRanges.Count; sizeRange++)
        {
            int index = 0;
            foreach (var modifiedRange in modifiedRangeIds)
            {
                try
                {
                    var query = new RecordQuery
                    {
                        Filters = []
                    };
                    AddLastModifiedDate(query, index, modifiedRange); //0 -1, 1 1, 2 2, 3 3, 4 4,5 5
                    var sizeColumn = sfApiObject.GetDescribeSObjectResult().name.EqualsIgnoreCase("ContentVersion")
                        ? "ContentSize"
                        : "BodyLength";
                    AddSizeRange(query, SizeRanges[sizeRange -1], sizeColumn);
                    index += 1;
                    var fileData =
                        await _salesforceService.GetFileDataWithModifiedTimeAndSizeRangeAsync(
                            sfApiObject.GetDescribeSObjectResult(), query);
                    if (fileData.Any())
                    {
                        foreach (var fileExtensionResult in fileData)
                        {
                            RMDiscoverySalesforceFileInactiveData temp = new()
                            {
                                ObjectId = newSObjectInfo.Id,
                                ModifiedDateRange = modifiedRange,
                                SizeRange = sizeRange,
                                FileExtension = fileExtensionResult.Extension.IsNullOrEmpty()
                                    ? RMConstants.UNKNOW
                                    : fileExtensionResult.Extension,
                                TotalFileCount = fileExtensionResult.Count,
                                TotalFileSize = (long)fileExtensionResult.TotalSize,
                            };
                            dataList.Add(temp);
                        }
                    }
                }
                catch (JobStopException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to create inactive record data of {sfApiObject.Name}, exception:{ex}");
                    errors.Add(ex.Message);
                }
            }
        }

        await SalesforceDiscoveryJobDao.AddFileInactiveDatasAsync(organizationId, dataList);
        _logger.Info($"end to create inactive record data {sfApiObject.Name}");

        var objectStatus = ReportCenter.GetObjectStatus(errors, dataList);
        SendJobDetail(objectStatus, errors, newSObjectInfo);
    }

    private void AddSizeRange(RecordQuery query, RMDiscoverySalesforceSizeRange sizeRange, string sizeColumn)
    {
        List<string> sizeRangeValue = [(sizeRange.GenerateEqual * 1024 * 1024).ToString()];
        if (sizeRange.LessThan != 2147483647)
        {
            sizeRangeValue.Add((sizeRange.LessThan * 1024 * 1024).ToString());
        }
        query.Filters!.Add(new QueryFilter()
            {
                PropertyName = sizeColumn,
                Value = sizeRangeValue,
            }
        );
    }

    private async Task CreateInActiveRecordData(SFObjectProxy sfApiObject, RMDiscoverySalesforceObjectInfo newSObjectInfo,
        string organizationId, CancellationToken token = default)
    {
        _logger.Info($"start to create inactive record data {sfApiObject.Name}");
        List<RMDiscoverySalesforceRecordInactiveData> tempList = [];
        List<string> errors = [];
        bool breakOuterLoop = false;
        var lastYearInModifiedRange = newSObjectInfo.OldestRecordsCreatedTime!.Value.Year - 1;
        var yearRange = DateTime.UtcNow.Year - lastYearInModifiedRange;
        var modifiedRangeIds = ModifiedRanges.Select(sizeRange => sizeRange.Id).Concat([-1]).Order().ToList();
        for (int year = 0; year <= yearRange; year++)
        {
            int index = 0;
            foreach (var modifiedRange in modifiedRangeIds)
            {
                try
                {
                    var query = new RecordQuery
                    {
                        Filters = []
                    };
                    AddLastModifiedDate(query, index, modifiedRange);
                    AddCreatedDateRange(year, query);

                    index += 1;
                    var recordCounts =
                        await _salesforceService.GetRecordCountWithModifiedTimeAsync(
                            sfApiObject.GetDescribeSObjectResult(), query);

                    if (recordCounts.GetValueOrDefault() == 0) continue;
                    RMDiscoverySalesforceRecordInactiveData temp = new()
                    {
                        ObjectId = newSObjectInfo.Id,
                        TotalCount = recordCounts.GetValueOrDefault(),
                        TotalSize = recordCounts.GetValueOrDefault() * 2 * 1024,
                        CreatedDateRange = DateTime.UtcNow.AddYears(-year).Year,
                        ModifiedDateRange = modifiedRange,
                    };
                    tempList.Add(temp);
                }
                catch (JobStopException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to create inactive record data {sfApiObject.Name}, exception:{ex}");
                    errors.Add($"{sfApiObject.Name} not support query");
                    breakOuterLoop = true;
                    break;
                }
            }

            if (breakOuterLoop)
            {
                break; // Breaks if this file is not queryable
            }
        }

        await SalesforceDiscoveryJobDao.AddRecordBasicDataAsync(organizationId, tempList);
        _logger.Info($"end to create inactive record data {sfApiObject.Name}");
        var objectStatus = ReportCenter.GetObjectStatus(errors, tempList);
        SendJobDetail(objectStatus, errors, newSObjectInfo);
    }

    private void SendJobDetail(ObjectStatusEnum objectStatus, List<string> errors, RMDiscoverySalesforceObjectInfo newSObjectInfo)
    {
        if (objectStatus is ObjectStatusEnum.Success)
        {
            ReportCenter.RecordSuccessful(newSObjectInfo.ConvertToJobDetail(JobDetailsStatus.Successful,ReportCenter.GetTenantId(),string.Join(',',errors)));
        }

        if (objectStatus is ObjectStatusEnum.Failed)
        {
            ReportCenter.RecordFailed(newSObjectInfo.ConvertToJobDetail(JobDetailsStatus.Failed,ReportCenter.GetTenantId(),string.Join(',',errors)));
        }
        
        if (objectStatus is ObjectStatusEnum.FinishedWithException)
        {
            ReportCenter.RecordSuccessful(newSObjectInfo.ConvertToJobDetail(JobDetailsStatus.Exception,ReportCenter.GetTenantId(),string.Join(',',errors)));
        }
    }



    private void AddLastModifiedDate(RecordQuery query, int index, int modifiedRange)
    {
        List<string> modifiedDate = [];
        if (modifiedRange != ModifiedRanges[^1].Id)
        {
            string fromMonth = DateTime.UtcNow.AddMonths(-ModifiedRanges[index].Unit).Ticks.ToString();
            modifiedDate.Add(fromMonth);
        }
        string toMonth = modifiedRange == -1 ? DateTime.UtcNow.Ticks.ToString() : DateTime.UtcNow.AddMonths(-ModifiedRanges[index - 1].Unit).Ticks.ToString();
        modifiedDate.Add(toMonth);
        query.Filters!.Add(new QueryFilter
            {
                PropertyName = "LastModifiedDate",
                Value = modifiedDate,
                IsExclude = true
            }
        );
    }

    private void AddCreatedDateRange(int year, RecordQuery query)
    {
        DateTime createdYear = DateTime.UtcNow.AddYears(-year);
        string fromYearTicks =  new DateTime(createdYear.Year, 1, 1).Ticks.ToString();
            
        string toYearTicks = year == 0 ? createdYear.Ticks.ToString() : new DateTime(createdYear.Year, 12, 31, hour:23, minute:59, second: 59).Ticks.ToString();
        query.Filters!.Add(new QueryFilter()
        {
            PropertyName = "CreatedDate",
            Value = [fromYearTicks, toYearTicks],
        });
    }

    private async Task<(bool, RMDiscoverySalesforceObjectInfo)> CreateObjectInfo(RMDiscoverySalesforceObjectType objectType, SFObjectProxy sfApiObject, CancellationToken token = default)
    {
        var objectInfo = new RMDiscoverySalesforceObjectInfo
        {
            Id = sfApiObject.Name.ToMd5(),
            InternalName = sfApiObject.Name,
            DisplayName = sfApiObject.LabelPlural,
            ObjectType = (int)objectType,
        };
        try
        {
            var (totalItemCount, totalItemSize) = objectType switch
            {
                RMDiscoverySalesforceObjectType.StandardObject or RMDiscoverySalesforceObjectType.CustomObject => await GetTotalCountAndSizeForDataRecord(
                    sfApiObject),
                RMDiscoverySalesforceObjectType.FileObject or RMDiscoverySalesforceObjectType.AttachmentObject =>
                    await GetTotalCountAndSizeForFileRecord(sfApiObject),
                _ => throw new NotSupportedException(nameof(objectType))
            };
            objectInfo.TotalItemCount = totalItemCount;
            objectInfo.TotalSize = totalItemSize;
            var oldestRecord =
                await _salesforceService.GetOldestRecordAsync(sfApiObject.GetDescribeSObjectResult());
            objectInfo.OldestRecordsCreatedTime = oldestRecord;
            objectInfo.LatestModifiedTime = await _salesforceService.GetLastModifiedTimeAsync(sfApiObject.GetDescribeSObjectResult());
            return (true,objectInfo);
        }
        catch (JobStopException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error($"Cannot get total record data {sfApiObject.Name}, exception:{ex}");
            ReportCenter.RecordSkipCommon(ReportCenter.GenerateCommonJobDetail(JobType.SFDiscoveryJob, objectInfo, JobDetailsStatus.Skipped, I18NResource.SalesforceDiscoveryJobObjectNodata));
        }

        return (false,objectInfo);
    }
    

    private async Task<(long totalItemCount, long totalItemSize)> GetTotalCountAndSizeForFileRecord(SFObjectProxy sfApiObject)
    {
        return await _salesforceService.GetFileRecordDataAsync(sfApiObject.GetDescribeSObjectResult(), new RecordQuery());
    }

    private async Task<(long, long)> GetTotalCountAndSizeForDataRecord(SFObjectProxy sfApiObject)
    {
        var sumRecordCount =
            await _salesforceService.GetDataRecordDataAsync(sfApiObject.GetDescribeSObjectResult(),
                new RecordQuery());
        var sumRecordSize = sumRecordCount * 2 * 1024;
        return (sumRecordCount, sumRecordSize);
    }
}