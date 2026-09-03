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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.Tree;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RAPhysical.API;
using AvePoint.RA.RAPhysical.Cache;
using AvePoint.RA.RAPhysical.Report.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.Report
{
    public class PRAvailableSpaceReportService : IPRAvailableSpaceReportService
    {
        public ILocationManagementService LocationManagementService { get; set; }
        public IPRReportProcessor PRReportProcessor { get; set; }

        private static readonly RALogger mLog = RALogger.GetInstance(typeof(PRContentDueReportService));

        public async Task RunReportJobAsync(string jobId, string profileId)
        {

        }
        public Task RunAvailableSpaceReportJobAsync(string jobId, string profileId)
        {
            var browseOption = new BrowseOptions()
            {
                NeedProcessBox = false,
                NeedProcessFile = false,
                NeedProcessRecord = false
            };
            var option = new ReportOptions()
            {
                BrowseOptions = browseOption,
                JobId = jobId,
                JobType = JobType.AvailableSpaceReport,
                ProfileId = profileId,
                IsUseBuildInGetTreeNodesFunc = false,
                IsUseBuiltInRootLocationAction = false,
                IsUseBuiltInNormalLocationAction = false,
                IsUseBuiltInBottomLocationAction = false,
                IsUseBuiltInBoxAction = false,
                IsUseBuiltInFileAction = false,
                IsUseBuiltInRecordsGroupAction = false
            };
            PRReportProcessor.ConfigGetTreeFun(GetTreeFunAsync)
                .PRTreeService
                .ConfigNormalLocationAction(ProcessNormalLocationAsync)
                .ConfigBottomLocationAction(ProcessBottomLocationAsync);
            return PRReportProcessor.ProcessAsync(option);
        }

        public async Task ProcessNormalLocationAsync(IPhysicalLocation normalLocation)
        {
            string fullPath = normalLocation.DirPath;
            double size = normalLocation.TotalCapacity;

            double availableSpace = normalLocation.TotalCapacity;
            double remainedSpace = 0.0;
            double locationUsedSpace = 0.0;
            PRReportProcessor.ReportManager.IncreaseBase(2 * 100);
            try
            {
                normalLocation.AllSubLocations.ForEach(el => locationUsedSpace += el.TotalCapacity);
                remainedSpace = availableSpace - locationUsedSpace;
                GenerateJobDetailItem(normalLocation, JobDetailsStatus.Successful);
                GenerateReport(normalLocation, remainedSpace);
            }
            catch (Exception ex)
            {
                mLog.Error($"process normal location exception,msg{ex.Message},stackTrace:{ex.StackTrace}");
                GenerateJobDetailItem(normalLocation, JobDetailsStatus.Failed, ".normal location occured exception");
            }

        }
        public async Task ProcessBottomLocationAsync(IPhysicalLocation bottomLocation)
        {
            double roomCapacity = bottomLocation.TotalCapacity;
            double remainedSpace = 0.0;
            double usedSpace = 0.0;
            try
            {
                List<IPhysicalBox> boxesOfLocation = bottomLocation.GetBoxes(r => (r.RecordStatus != (int)RMRecordStatus.RMDeleted && r.RecordStatus != (int)RMRecordStatus.Destroyed && r.RecordStatus != (int)RMRecordStatus.MoveOverwrite) && r.LocationId == bottomLocation.UniqueId);
                PRReportProcessor.ReportManager.IncreaseBase(boxesOfLocation.Count() * 100);
                boxesOfLocation.ForEach(b =>
                {
                    PhysicalBaseObject box = (PhysicalBaseObject)b;
                    string capa = box.Fields.ContainsKey(DefaultColumnIDs.Capability) ? box.Fields[DefaultColumnIDs.Capability] : string.Empty;
                    double boxSize = 0.0;
                    if (double.TryParse(capa, out boxSize))
                    {
                        usedSpace += boxSize;
                    }
                });

                remainedSpace = roomCapacity - usedSpace;
                GenerateJobDetailItem(bottomLocation, JobDetailsStatus.Successful);
                GenerateReport(bottomLocation, remainedSpace);
            }
            catch (Exception ex)
            {
                mLog.Error($"available process bottom location exception,msg{ex.Message},stackTrace:{ex.StackTrace}");
                GenerateJobDetailItem(bottomLocation, JobDetailsStatus.Failed, "bottom location occured exception");
            }
        }

        private JMAvailableSpaceReportJobDetail GenerateJobDetailItem(IPhysicalLocation location, JobDetailsStatus detailsStatus, string comment = "")
        {
            JMAvailableSpaceReportJobDetail jobDetails = new JMAvailableSpaceReportJobDetail();
            jobDetails.Comment = comment;
            jobDetails.Status = detailsStatus;
            jobDetails.Location = location.DirPath;
            jobDetails.LocationSize = location.TotalCapacity;
            PRReportProcessor.AddJobDetail(jobDetails);
            PRReportProcessor.ReportManager.Increase(1);
            return jobDetails;
        }
        private void GenerateReport(IPhysicalLocation location, double availableSpace, string boxTypeAndNumber = "")
        {
            AvailableSpaceReport report = new AvailableSpaceReport()
            {
                AvailableSpace = availableSpace,
                Location = location.DirPath,
                LocationSize = location.TotalCapacity,
                InculdingContainerInfo = boxTypeAndNumber,
                Url = location.DirPath
            };
            PRReportProcessor.AddJobReport(report);
            PRReportProcessor.ReportManager.Increase(1);
        }

        /*
         Available Space Report Profile目前是单选,构造将要遍历的Tree结构是以当前选中的Location结点为
         根结点的Tree,之后进入Common方法循环流转下去.
         */
        public async Task<List<RMLocationProfileNode>> GetTreeFunAsync(string profileId)
        {
            List<RMLocationProfileNode> treeNodes = new List<RMLocationProfileNode>();
            try
            {
                var profileDto = await PRReportProcessor.mRMReportService.GetProfileByIdAsync(profileId);
                var locationId = Convert.ToInt32(profileDto.Extension2);
                RMLocationProfileNode profileNode = LocationManagementService.Convert2ProfileNode(locationId, true, true);
                treeNodes.Add(profileNode);
            }
            catch (Exception ex)
            {
                mLog.Error($"GetTreeFun exception,msg:{ex.Message},stackTrace:{ex.StackTrace}");
            }
            return treeNodes;
        }
    }
}
