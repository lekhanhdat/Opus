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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.GCommon.Contract.GranularBackup.Object;
using AvePoint.GCommon.Contract.Server.Common.Monitor;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Exceptions.Job;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.Wrapper.Common;
using System.Text;
using System.Xml;

namespace RAGoogle.Restore.Report
{
    public enum JobStatus
    {
        InProgress = 0,
        Finished = 2,
        Failed = 3,
        Stopped = 4,
        FinishWithException = 7
    }

    [AveCodeReview("2013/04/25", "Hongming.Zhang@avepoint.com", "Yongchao.Zhou@avepoint.com", new string[] { CodeReviewConstants.CHECK_LIST_ID_CS_2 }, "ADO-72310", true)]
    public class RestoreResultInfo
    {
        public JobStatus JobStatus { get; set; }
        public List<PropertyItem> restoreResultErrorList = new List<PropertyItem>();
        public void AddARestoreError(string defaultvalue, string key, string[] args)
        {
            if (this.restoreResultErrorList == null)
            {
                restoreResultErrorList = new List<PropertyItem>();
            }
            restoreResultErrorList.Add(new PropertyItem
            {
                Key = key,
                Args = args,
                DefaultValue = defaultvalue
            });
        }
    }

    public interface IAveRestoreReport
    {
        bool HasFailedNode { get; }
        void AddReport(AveRestoreReportDto reportDto);
        void Finish(RestoreResultInfo restoreInfo, string message);
        //string ConvertErrorMessageToXML(RestoreReportKey key, params object[] paras);
        //string ConvertErrorMessageToXML(string defaultValue, RestoreReportKey key, params object[] paras);

        void AddJobSummaryComment(string key);
    }

    public class GDriveRestoreReport : IAveRestoreReport, IDisposable
    {
        #region Properties
        private readonly AveLogger log = AveLogger.GetInstance(typeof(GDriveRestoreReport));
        private StreamWriter reportWriter;
        private const string TEMP_REPORT_FILE_NAME = "restoreTempReport.txt";
        private const string COPY_REPORT_FILE_NAME = "copyRestoreTempReport.txt";
        private const int REPORT_LIMITED_NUMBER = 1000;
        private readonly object progressLock = new object();
        private readonly object reportLock = new object();

        //private readonly XmlDocument mReportDoc = new XmlDocument();
        //private readonly XmlDocument mReportErrorDoc = new XmlDocument();
        private readonly XmlDocument mReportDocOOP = new XmlDocument();
        private readonly XmlDocument mReportErrorDocOOP = new XmlDocument();
        //private readonly XmlDocument mReportTempDoc = new XmlDocument();
        private readonly List<string> itemTypes = new List<string>() { "N", "D", "K", "I", "U", "A" };
        //key  is  countHeader "success/fail/skipped" + "site/web/list/folder/item"
        private Dictionary<string, int> kindCount = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        //private long currentSize;
        private long totalSize;

        private JobStatusInfo jobInfo;

        private bool isReportReachEnd = false;

        private List<PropertyItem> propertyItems = new List<PropertyItem>();

        public GDriveRestoreConfig Config { get; set; }

        public bool IncludeConfigurationReport { get; set; }

        public string SrcAgentName { set; get; }
        public string DestAgentName { set; get; }
        public string MediaName { set; get; }
        public bool HasFailedNode { get; set; }
        public bool HasSuccessNode { get; private set; }
        private IJobMonitorAPIService jobMoniterAPIService;//for EndUserArchive Restore ,Get JobProgress from ControlPanel
        public static long streamSent = 0;
        public static long streamReceived = 0;
        public void SetCurrentProgress(int progress)
        {
            if (progress > 99)
            {
                progress = 99;
            }
            if (progress < 0)
            {
                progress = 0;
            }
            lock (this.progressLock)
            {
                this.currentProgress = progress;
                Monitor.PulseAll(this.progressLock);
            }
        }
        public bool IsEndUserRestore { get; set; }
        private int currentProgress;
        #endregion

        #region IAveRestoreReporter Members

        public void Init(GDriveRestoreConfig config)
        {
            //Config = config;
            mReportDocOOP.LoadXml("<T t=\"\" title=\"\" p=\"\" s=\"\" ticks=\"\" finishTime=\"\" sPath=\"\"/>");
            mReportErrorDocOOP.LoadXml("<F t=\"\" title=\"\" p=\"\" m=\"\" ticks=\"\" finishTime=\"\" sPath=\"\"/>");
            string jobDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AgentData", "jobs", config.SubJobId);//use for local manifest.xml path
            if (!Directory.Exists(jobDataPath))
            {
                Directory.CreateDirectory(jobDataPath);
            }
            this.reportWriter = new StreamWriter(config.JobDir + "\\" + TEMP_REPORT_FILE_NAME, true, Encoding.UTF8);
            this.jobInfo = new JobStatusInfo { AgentHost = AveEnv.AgentAddress, Id = config.SubJobId, Type = config.JobType, Progress = 0 };
            IncludeConfigurationReport = true;
        }

        public void AddReport(IEnumerable<AveRestoreReportDto> reportDtos)
        {
            foreach (var reportDto in reportDtos)
            {
                AddReport(reportDto);
            }
        }

        public void AddReport(AveRestoreReportDto reportDto)
        {
            if (reportDto.EntityType == JobReportDetailEntityType.NormalInfo || IncludeConfigurationReport)
            {
                lock (reportLock)   //SAAS-12708 Restore的时候Item级别有多线程，在add report的时候要加锁，防止数据出错。
                {
                    if (reportDto.Status != RestoreStatus.Success)
                    {
                        this.HasFailedNode |= reportDto.IsFailedNode;
                        AddFailedOrSkippedReport(reportDto);
                    }
                    else
                    {
                        this.HasSuccessNode |= reportDto.IsSuccessNode;
                        AddSucceededReport(reportDto);
                    }
                }
            }
        }

        #region Add Report

        private void AddFailedOrSkippedReport(AveRestoreReportDto reportDto)
        {
            XmlElement xe = mReportErrorDocOOP.DocumentElement;
            SetBasicAttribute(reportDto, xe);
            xe.Attributes["m"].Value = reportDto.ErrorMessage;
            xe.SetAttribute("isSkipped", (reportDto.Status == RestoreStatus.Skipped).ToString());
            this.reportWriter.WriteLine(mReportErrorDocOOP.OuterXml);
            this.reportWriter.Flush();
        }

        private void AddSucceededReport(AveRestoreReportDto reportDto)
        {
            if (IsItem(reportDto.Type) && !Config.IncludeItemsReport)
            {
                this.reportWriter.WriteLine(reportDto.Size);
            }
            else
            {
                XmlElement xe = mReportDocOOP.DocumentElement;
                SetBasicAttribute(reportDto, xe);
                xe.Attributes["s"].Value = reportDto.Size.ToString();
                this.reportWriter.WriteLine(mReportDocOOP.OuterXml);
            }
            this.reportWriter.Flush();
        }

        private static void SetBasicAttribute(AveRestoreReportDto reportDto, XmlElement xe)
        {
            DateTime nowTime = DateTime.UtcNow;
            xe.Attributes["t"].Value = reportDto.Type;
            xe.Attributes["title"].Value = reportDto.Title;
            xe.Attributes["p"].Value = reportDto.Path;
            xe.Attributes["finishTime"].Value = AveDateTimeUtility.ConvertToType004(nowTime);
            xe.Attributes["ticks"].Value = nowTime.Ticks.ToString();
            if (reportDto.SourcePath != null)
            {
                xe.Attributes["sPath"].Value = reportDto.SourcePath.Replace("\\", "/");//modify for SAAS-10838
            }
            else
            {
                xe.Attributes["sPath"].Value = reportDto.SourcePath;
            }
            xe.SetAttribute("name", reportDto.Name);
            xe.SetAttribute("entityType", ((int)reportDto.EntityType).ToString());
            xe.SetAttribute("objectTitle", reportDto.RelatedObjectTitle);
            xe.SetAttribute("option", reportDto.Option);
            xe.SetAttribute("v", reportDto.Version);
        }

        private bool IsItem(string type)
        {
            return this.itemTypes.Contains(type);
        }

        #endregion

        public void Finish(RestoreResultInfo restoreInfo, string message)
        {
            if (this.HasFailedNode && restoreInfo.JobStatus == JobStatus.Finished)
            {
                restoreInfo.JobStatus = this.HasSuccessNode ? JobStatus.FinishWithException : JobStatus.Failed;
            }
            try
            {
                log.Info(@"Looks up a localized string similar to Restore job status:{0}..", restoreInfo.JobStatus);
                try
                {
                    foreach (var errorInfo in restoreInfo.restoreResultErrorList)
                    {
                        AddLastReport(AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(errorInfo.Key, errorInfo.DefaultValue, errorInfo.Args));
                    }
                }
                finally
                {
                    Close();
                }
                SendJobReportAndSummary(restoreInfo, message);
                SendJobStatus(restoreInfo);
            }
            catch (Exception e)
            {
                log.Error(@"Looks up a localized string similar to An error occurred while setting the job status to be failed.", e.ToString());
            }
        }

        public RestoreReportKey ConvertStringToReportKey(string key)
        {
            try
            {
                return (RestoreReportKey)Enum.Parse(typeof(RestoreReportKey), key, true);
            }
            catch (Exception e)
            {
                log.Warn("This string {0} is not an illegal report key. Error message:{1}", key, e.ToString());
                return RestoreReportKey.Item_Unknown;
            }
        }

        #region Report and Summary
        private void Close()
        {
            if (this.reportWriter != null)
            {
                this.reportWriter.Close();
                this.reportWriter = null;
            }
        }

        private void SendJobReportAndSummary(RestoreResultInfo resultInfo, string message)
        {
            try
            {
                //GenerateReport(errorMessage);                
                var subJobInfo = new SubJobDto() { Id = Config.SubJobId, ParentId = Config.JobId };
                SendJobDetails(subJobInfo);
                isReportReachEnd = true;
                var summary = GetJobSummary(resultInfo, message);
                SendJobSummary(summary, subJobInfo);
            }
            catch (Exception e)
            {
                log.Warn(@"Looks up a localized string similar to An error occurred while sending the report to server. Error message: {0}.", e.ToString());
            }
        }

        private void AddLastReport(string errorMessage)
        {
            if (!string.IsNullOrEmpty(errorMessage))
            {//Send job failed reason to server
                AddFailedOrSkippedReport(new AveRestoreReportDto
                {
                    Path = String.Empty,
                    Title = string.Empty,
                    ErrorMessage = errorMessage,
                    Type = string.Empty,
                    Status = RestoreStatus.Failed
                });
            }
        }

        #region Summary
        private List<JobSummary> GetJobSummary(RestoreResultInfo resultInfo, string errorMessage)
        {
            List<JobSummary> summaryList = new List<JobSummary>();
            ///agent summary
            summaryList.Add(new JobSummary() { Key = "Status", Value = GetJobStateName(resultInfo.JobStatus) });
            if (propertyItems.Count > 0)
            {
                propertyItems.Add(new PropertyItem() { PropertyType = ParamKey.Message, Key = "Gui_NewLine" });
                propertyItems.Add(new PropertyItem() { PropertyType = ParamKey.Message, DefaultValue = errorMessage });
                string summaryComments = SerializerHelper.SerializeToXmlString<List<PropertyItem>>(propertyItems);
                summaryList.Add(new JobSummary() { Key = "Comments", Value = summaryComments });
            }
            else
            {
                summaryList.Add(new JobSummary() { Key = "Comments", Value = errorMessage });
            }
            //TrimErrorMessage(resultInfo.restoreResultErrorList);
            //summaryList.Add(new JobSummary()
            //{
            //    Key = GConstants.JobSummaryKey.Comments,
            //    PropertyItems = resultInfo.restoreResultErrorList
            //});
            summaryList.Add(new JobSummary() { Key = "DataSize", Value = this.totalSize >> 10 });
            summaryList.Add(new JobSummary() { Key = "Stream Sent", Value = AveStreamStatistics.streamSent });
            summaryList.Add(new JobSummary() { Key = "Stream Received", Value = AveStreamStatistics.streamReceived });

            var objectsCount = new Dictionary<NodeLevel, int>();
            //objectsCount.Add(NodeLevel.WebApplication, 1);

            objectsCount.Add(NodeLevel.SiteCollection, GetValue(kindCount, ReportNodeHeader.Site));
            objectsCount.Add(NodeLevel.Site, GetValue(kindCount, ReportNodeHeader.Web));
            objectsCount.Add(NodeLevel.App, GetValue(kindCount, ReportNodeHeader.App));
            objectsCount.Add(NodeLevel.List, GetValue(kindCount, ReportNodeHeader.List));
            objectsCount.Add(NodeLevel.Folder, GetValue(kindCount, ReportNodeHeader.Folder));
            objectsCount.Add(NodeLevel.Item, GetValue(kindCount, ReportNodeHeader.Item));
            summaryList.AddRange(GetSummaryObjectsString(objectsCount, string.Empty));

            var succeededObjectsCount = new Dictionary<NodeLevel, int>();
            //succeededObjectsCount.Add(NodeLevel.WebApplication, 1);//Current is 1 webapp
            succeededObjectsCount.Add(NodeLevel.SiteCollection, GetValue(kindCount, ReportNodeHeader.Sucess + ReportNodeHeader.Site));
            succeededObjectsCount.Add(NodeLevel.Site, GetValue(kindCount, ReportNodeHeader.Sucess + ReportNodeHeader.Web));
            succeededObjectsCount.Add(NodeLevel.App, GetValue(kindCount, ReportNodeHeader.Sucess + ReportNodeHeader.App));
            succeededObjectsCount.Add(NodeLevel.List, GetValue(kindCount, ReportNodeHeader.Sucess + ReportNodeHeader.List));
            succeededObjectsCount.Add(NodeLevel.Folder, GetValue(kindCount, ReportNodeHeader.Sucess + ReportNodeHeader.Folder));
            succeededObjectsCount.Add(NodeLevel.Item, GetValue(kindCount, ReportNodeHeader.Sucess + ReportNodeHeader.Item));
            summaryList.AddRange(GetSummaryObjectsString(succeededObjectsCount, "Succeed"));

            var failedObjectsCount = new Dictionary<NodeLevel, int>();
            failedObjectsCount.Add(NodeLevel.SiteCollection, GetValue(kindCount, ReportNodeHeader.Fail + ReportNodeHeader.Site));
            failedObjectsCount.Add(NodeLevel.Site, GetValue(kindCount, ReportNodeHeader.Fail + ReportNodeHeader.Web));
            failedObjectsCount.Add(NodeLevel.App, GetValue(kindCount, ReportNodeHeader.Fail + ReportNodeHeader.App));
            failedObjectsCount.Add(NodeLevel.List, GetValue(kindCount, ReportNodeHeader.Fail + ReportNodeHeader.List));
            failedObjectsCount.Add(NodeLevel.Folder, GetValue(kindCount, ReportNodeHeader.Fail + ReportNodeHeader.Folder));
            failedObjectsCount.Add(NodeLevel.Item, GetValue(kindCount, ReportNodeHeader.Fail + ReportNodeHeader.Item));
            summaryList.AddRange(GetSummaryObjectsString(failedObjectsCount, "Failed"));


            var skippedObjectsCount = new Dictionary<NodeLevel, int>();
            skippedObjectsCount.Add(NodeLevel.SiteCollection, GetValue(kindCount, ReportNodeHeader.Skiped + ReportNodeHeader.Site));
            skippedObjectsCount.Add(NodeLevel.Site, GetValue(kindCount, ReportNodeHeader.Skiped + ReportNodeHeader.Web));
            skippedObjectsCount.Add(NodeLevel.List, GetValue(kindCount, ReportNodeHeader.Skiped + ReportNodeHeader.List));
            skippedObjectsCount.Add(NodeLevel.Folder, GetValue(kindCount, ReportNodeHeader.Skiped + ReportNodeHeader.Folder));
            skippedObjectsCount.Add(NodeLevel.Item, GetValue(kindCount, ReportNodeHeader.Skiped + ReportNodeHeader.Item));
            summaryList.AddRange(GetSummaryObjectsString(skippedObjectsCount, "Skipped"));

            summaryList.ForEach(summary => summary.SubJobId = Config.JobId);

            return summaryList;
        }

        private string GetJobStateName(JobStatus status)
        {
            switch (status)
            {
                case JobStatus.Stopped:
                    return "Stopped";// RestoreReportResource.Item_JobStateNameStoppedReport;
                case JobStatus.FinishWithException:
                    return "Finished with Exception";// RestoreReportResource.Item_JobStateNameCompletedWithExceptionReport;
                case JobStatus.Failed:
                    return "Failed";// RestoreReportResource.Item_JobStateNameFailedReport;
                default:
                case JobStatus.Finished:
                    return "Completed";//RestoreReportResource.Item_JobStateNameCompletedReport;
            }
        }

        private List<JobSummary> GetSummaryObjectsString(Dictionary<NodeLevel, int> ObjectsCount, string prefx)
        {
            int totalCount = 0;
            List<JobSummary> summaryList = new List<JobSummary>();
            foreach (var item in ObjectsCount)
            {
                if (item.Key == NodeLevel.WebApplication)
                {
                    summaryList.Add(new JobSummary() { Key = prefx + GConstants.JobSummaryKey.WebAppCount, Value = item.Value });
                }
                else if (item.Key == NodeLevel.SiteCollection)
                {
                    summaryList.Add(new JobSummary() { Key = prefx + GConstants.JobSummaryKey.SiteCollectionCount, Value = item.Value });
                }
                else if (item.Key == NodeLevel.Site)
                {
                    summaryList.Add(new JobSummary() { Key = prefx + GConstants.JobSummaryKey.SiteCount, Value = item.Value });
                }
                else if (item.Key == NodeLevel.App)
                {
                    summaryList.Add(new JobSummary() { Key = prefx + GConstants.JobSummaryKey.AppCount, Value = item.Value });
                }
                else if (item.Key == NodeLevel.List)
                {
                    summaryList.Add(new JobSummary() { Key = prefx + GConstants.JobSummaryKey.ListCount, Value = item.Value });
                }
                else if (item.Key == NodeLevel.Folder)
                {
                    summaryList.Add(new JobSummary() { Key = prefx + GConstants.JobSummaryKey.FolderCount, Value = item.Value });
                }
                else if (item.Key == NodeLevel.Item)
                {
                    summaryList.Add(new JobSummary() { Key = prefx + GConstants.JobSummaryKey.ItemCount, Value = item.Value });
                }
                totalCount += item.Value;
            }

            return summaryList;
        }
        #endregion

        #region Detail

        private void SendJobDetails(SubJobDto jobInfo)
        {
            List<JobDetail> details = new List<JobDetail>();
            using (var input = new StreamReader(Config.JobDir + "\\" + TEMP_REPORT_FILE_NAME))
            {
                while (GetJobDetails(input, details, 1000))
                {
                    if (!SendJobDetails(details, jobInfo))
                    {
                        break;
                    }
                }
            }
        }


        private bool GetJobDetails(StreamReader input, List<JobDetail> details, int count)
        {
            string line;
            var xe = new XmlDocument();
            while ((line = input.ReadLine()) != null)
            {

                //统计所有  Restore 操作 .
                if (!SetNodeCount(line, xe))
                {
                    continue;
                }
                if (line.StartsWith("<T", StringComparison.Ordinal) || line.StartsWith("<F", StringComparison.Ordinal))
                {
                    details.Add(GetJobDetail(line));
                }
                //Read回来的数据需要放到Details集合中，需要最后Break，否则会丢失Break那次循环的数据.
                if (--count < 0)
                {
                    break;
                }
            }
            return details.Count > 0;
        }


        private bool SetNodeCount(string line, XmlDocument xe)
        {
            if (!line.StartsWith("<", StringComparison.OrdinalIgnoreCase))
            {
                //Include Detail Job report is false
                if (!Config.IncludeItemsReport)
                {
                    long currentSize;
                    long.TryParse(line, out currentSize);
                    this.totalSize += currentSize;
                    AddAllKindCount(ReportNodeHeader.Item, ReportNodeHeader.Sucess);
                }
                return false;
            }
            try
            {
                xe.LoadXml(line);

                var size = xe.DocumentElement.GetAttribute(ReportNodeHeader.Size);
                var type = xe.DocumentElement.GetAttribute(ReportNodeHeader.Type);
                if (xe.GetElementsByTagName(ReportNodeHeader.Sucess).Count != 0)
                {
                    long currentSize;
                    long.TryParse(size, out currentSize);
                    this.totalSize += currentSize;
                    AddAllKindCount(type, ReportNodeHeader.Sucess);
                }
                else
                {
                    var status = string.Equals(bool.TrueString, xe.DocumentElement.GetAttribute(ReportNodeHeader.Skiped), StringComparison.OrdinalIgnoreCase) ? ReportNodeHeader.Skiped : ReportNodeHeader.Fail;
                    AddAllKindCount(type, status);
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, "An error occurred while set node count.Error Message:{0}..", e);
                return false;
            }
            return true;
        }

        private void AddAllKindCount(string type, string status)
        {
            string realType = IsItem(type) ? ReportNodeHeader.Item : type;
            var key = status + realType;
            AddAllKindCount(realType);
            AddAllKindCount(key);
        }

        private void AddAllKindCount(string key)
        {
            if (!kindCount.ContainsKey(key))
            {
                kindCount.Add(key, 0);
            }
            ++kindCount[key];
        }

        private int GetValue(Dictionary<string, int> dic, string key)
        {
            if (dic.ContainsKey(key))
            {
                return dic[key];
            }
            return 0;
        }
        private Dictionary<string, string> ReportTypeMapping
        {
            get
            {
                if (reportTypeMapping == null)
                {
                    reportTypeMapping = new Dictionary<string, string>(16)
                    {
                        {"E","Site Collection"},
                        {"W","Site"},
                        {"L","List"},
                        {"J","Project"},
                        {"Y","App"},
                        {"F","Folder"},
                        {"P","MyProfile"},
                        {"I","Item"},
                        {"U","Item Version"},
                        {"D","Document"},
                        {"K","Document Version"},
                        {"N","Folder Version"},
                        {"A","Attachment"}
                    };
                }
                return reportTypeMapping;
            }
        }

        private Dictionary<string, string> reportTypeMapping;
        private string ConverCharToString(string type)
        {
            if (ReportTypeMapping.ContainsKey(type))
            {
                return ReportTypeMapping[type];
            }
            return type;
        }

        private JobDetail GetJobDetail(string line)
        {
            JobDetail jobDetail = new JobDetail();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(line);
            jobDetail.Size = doc.DocumentElement.HasAttribute("s") ? Convert.ToInt64(doc.DocumentElement.GetAttribute("s")) : 0;
            jobDetail.Date = Convert.ToInt64(doc.DocumentElement.GetAttribute("ticks"));
            jobDetail.Title = doc.DocumentElement.GetAttribute("title");
            jobDetail.Type = ConverCharToString(doc.DocumentElement.GetAttribute("t"));
            jobDetail.Version = doc.DocumentElement.HasAttribute("v") ? doc.DocumentElement.GetAttribute("v") : "";
            //Remark3: name for configuration detail
            jobDetail.Remark3 = doc.DocumentElement.GetAttribute("name");
            //Remark4: Related Object title
            jobDetail.Remark4 = doc.DocumentElement.GetAttribute("objectTitle");
            //EntityType: 0 object, 11 configuration
            jobDetail.EntityType = doc.DocumentElement.HasAttribute("entityType") ? Convert.ToInt32(doc.DocumentElement.GetAttribute("entityType")) : (int)JobReportDetailEntityType.NormalInfo;
            if (jobDetail.EntityType == (int)JobReportDetailEntityType.NormalInfo)
            {
                jobDetail.DestURL = doc.DocumentElement.GetAttribute("p");
                jobDetail.SrcURL = doc.DocumentElement.HasAttribute("sPath") ?
                    doc.DocumentElement.GetAttribute("sPath") : jobDetail.DestURL;
            }
            jobDetail.SrcAgentHost = SrcAgentName;
            jobDetail.DestAgentHost = DestAgentName;
            jobDetail.MediaHost = MediaName;
            jobDetail.SubJobId = this.jobInfo.Id;
            jobDetail.Option = doc.DocumentElement.GetAttribute("option");

            if (line.StartsWith("<T", StringComparison.Ordinal))
            {
                jobDetail.Status = 0;
                jobDetail.Message = "";
            }
            else if (line.StartsWith("<F", StringComparison.Ordinal))
            {
                jobDetail.Status = string.Equals(bool.TrueString, doc.DocumentElement.GetAttribute("isSkipped"), StringComparison.OrdinalIgnoreCase) ? 2 : 1;
                string tempXml = doc.DocumentElement.GetAttribute("m");
                List<PropertyItem> propertyItems = null;
                string errorMessage = string.Empty;
                AnalyzeXml(tempXml, out propertyItems, out errorMessage);
                jobDetail.PropertyItems = propertyItems;
                jobDetail.Message = errorMessage;
            }
            return jobDetail;
        }
        #endregion

        public void AnalyzeXml(string tempXml, out List<PropertyItem> propertyItems, out string errorMessage)
        {
            XmlDocument xd = new XmlDocument();
            object[] args = new object[] { };
            string key = string.Empty;
            string defaultValue = string.Empty;
            try
            {
                xd.LoadXml(tempXml);
            }
            catch (Exception e)
            {
                if (tempXml.StartsWith("<", StringComparison.Ordinal))
                {
                    log.Warn("This string  is not xml. XMLString:{0}. Error:{1}", tempXml, e.Message);
                }
                errorMessage = tempXml;
                propertyItems = new List<PropertyItem>() { new PropertyItem() { PropertyType = ParamKey.Message, Key = tempXml, DefaultValue = tempXml } };
                return;
            }
            XmlElement rootElement = xd.DocumentElement;
            ConvertXmlToPara(rootElement, out key, out args, out defaultValue);
            propertyItems = new List<PropertyItem>() { new PropertyItem() { PropertyType = ParamKey.Message, Key = key, Args = args, DefaultValue = defaultValue } };
            errorMessage = key;
        }

        public void ConvertXmlToPara(XmlElement rootElement, out string key, out object[] args, out string defaultValue)
        {
            XmlNode keyNode = rootElement.ChildNodes[0];//一定含有key node.
            List<string> tempPara = new List<string>();
            key = string.Empty;
            defaultValue = string.Empty;
            args = new object[] { };
            foreach (XmlAttribute attribute in keyNode.Attributes)
            {
                if (attribute.Name.Equals("Key", StringComparison.OrdinalIgnoreCase))
                {
                    key = attribute.Value;
                    if (string.IsNullOrEmpty(defaultValue))
                    {
                        defaultValue = key;
                    }
                }
                else if (attribute.Name.Equals("DefaultValue", StringComparison.OrdinalIgnoreCase))
                {
                    defaultValue = attribute.Value;
                }
            }

            foreach (XmlElement subElement in keyNode.ChildNodes)
            {
                if (subElement.HasAttribute("Value"))
                {
                    tempPara.Add(subElement.Attributes["Value"].Value);
                }
            }
            args = tempPara.ToArray();
        }

        private void SendJobStatus(RestoreResultInfo resultInfo)
        {
            try
            {
                this.jobInfo.State = (int)resultInfo.JobStatus;
                if (resultInfo.restoreResultErrorList != null && resultInfo.restoreResultErrorList.Count > 0)
                {
                    var errors = new List<ErrorInfo>();
                    foreach (var error in resultInfo.restoreResultErrorList)
                    {
                        errors.Add(new ErrorInfo { Error = error.DefaultValue });
                    }
                    this.jobInfo.ErrorInfos = errors;
                }
                SendJobStatus(false, this.jobInfo);
            }
            catch (Exception e)
            {
                log.Warn(@"Looks up a localized string similar to An error occurred while sending the completed report to server. Error message: {0}.", e);
            }
        }
        #endregion
        #endregion

        #region Communacation with Control

        private void SendJobStatus(bool isProgress, JobStatusInfo jobInfo)
        {
            try
            {
                //var jobStatusService = JobReportServiceFactory.CreateJobStatusUpdater();

                if (isProgress)
                {
                    //JobUpdateState state = jobStatusService.UpdateJobProgress(jobInfo);
                    if (IsEndUserRestore)
                    {
                        //SetString8InJobTable();
                    }
                    //if (state == JobUpdateState.NeedNotUpdate)
                    //{
                    //    this.reportWriter.Close();//granular 的report writer是持续占用的SteamWriter，如果不close的话是无法删除的
                    //}
                    //JobProcessUtility.CheckIfJobCancelled(state, Config.JobDir);
                }
                else
                {
                    WriteJobStatusLog(jobInfo);
                    //jobStatusService.UpdateJobStatus(jobInfo);
                    if (IsEndUserRestore)
                    {
                        SetEndUserRestoreJobStatusByAgent(jobInfo);
                    }
                }
                //only for EndUserArchiveRestore 
                if (jobInfo.Id.StartsWith("ER", StringComparison.OrdinalIgnoreCase))
                {
                    SendEndUserArchiveRestoreJobProgress(jobInfo);
                }
            }
            catch (Exception ex)
            {
                log.Error(@"Looks up a localized string similar to An error occurred while updating the job status.{0}", ex.ToString());
            }
        }



        private void SetEndUserRestoreJobStatusByAgent(JobStatusInfo subJobInfo)
        {
            if (IsEndUserRestore)
            {
                //SetString8InJobTable();
                //var jobStatusService = JobReportServiceFactory.CreateJobStatusUpdater();
                JobStatusInfo mainJobStatusInfo = new JobStatusInfo();
                mainJobStatusInfo.State = subJobInfo.State;
                mainJobStatusInfo.AgentHost = subJobInfo.AgentHost;
                mainJobStatusInfo.ErrorInfos = subJobInfo.ErrorInfos;
                mainJobStatusInfo.Progress = subJobInfo.Progress;
                mainJobStatusInfo.Stamp = subJobInfo.Stamp;
                mainJobStatusInfo.Type = subJobInfo.Type;
                mainJobStatusInfo.Id = subJobInfo.Id.Split('_')[0];
                //jobStatusService.UpdateJobStatus(jobInfo);
            }
        }

        //only for endUserArchiveRestore
        private void SendEndUserArchiveRestoreJobProgress(JobStatusInfo jobInfo)
        {
            string jobId = jobInfo.Id;
            string folderPath = Path.Combine(AveEnv.AgentJobFolder, jobId);
            string processFilePath = Path.Combine(AveEnv.AgentJobFolder, jobId + "\\" + jobId + ".txt");
            string jobState = string.Empty;
            switch (jobInfo.State)
            {
                case 0:
                    jobState = "InProgress";
                    break;
                case 2:
                    jobState = "Finished";
                    break;
                case 3:
                    jobState = "Failed";
                    break;
                case 4:
                    jobState = "Stopped";
                    break;
                case 6:
                    jobState = "Skipped";
                    break;
                case 7:
                    jobState = "FinishedException";
                    break;
                default:
                    jobState = "Unknown";
                    break;
            }
            string message = string.Format("{0}:{1}", jobState, jobInfo.Progress);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            using (StreamWriter stream = new StreamWriter(processFilePath, false))
            {
                stream.WriteLine(message);
            }
        }

        private void WriteJobStatusLog(JobStatusInfo jobInfo)
        {
            switch (jobInfo.State)
            {
                case 2:
                    log.Log(EventSources.DocAveAgentService, Config.EventCategory, new EventIds.Job.CompletedEventMessage(Config.JobId));
                    break;
                case 3:
                    log.Log(EventSources.DocAveAgentService, Config.EventCategory, new EventIds.Job.FailedEventMessage(Config.JobId, new FailedException()));
                    break;
                case 4:
                    log.Log(EventSources.DocAveAgentService, Config.EventCategory, new EventIds.Job.StoppedEventMessage(Config.JobId));
                    break;
                case 7:
                    log.Log(EventSources.DocAveAgentService, Config.EventCategory, new EventIds.Job.CompletedWithExceptionEventMessage(Config.JobId, new CompletedWithExceptionException()));
                    break;
                default:
                    log.Info("Looks up a localized string similar to Unknow job status..");
                    break;
            }
        }

        private bool SendJobDetails(List<JobDetail> details, SubJobDto jobInfo)
        {
            int size = details.Count;
            while (details.Count > 0)
            {
                try
                {
                    //var jobDetailService = JobReportServiceFactory.CreateJobDetailService();
                    //jobDetailService.UpdateSubJobDetails(details.GetRange(0, size), jobInfo);
                    //details.RemoveRange(0, size);
                }
                catch (Exception ex)
                {
                    size /= 2;
                    if (size <= 10)
                    {
                        log.Error(@"Looks up a localized string similar to An error occurred while updating the job details.{0}", ex.ToString());
                        break;
                    }
                }
            }
            return details.Count == 0;
        }

        private void SendJobSummary(List<JobSummary> summary, SubJobDto jobInfo)
        {
            //try
            //{
            //    var jobReportService = JobReportServiceFactory.CreateJobDetailService();
            //    jobReportService.UpdateSubJobSummary(summary, jobInfo);
            //}
            //catch (Exception ex)
            //{
            //    log.Error(@"Looks up a localized string similar to An error occurred while sending the job summary to server.{0}", ex.ToString());
            //}
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (this.reportWriter != null)
            {
                this.reportWriter.Dispose();
                this.reportWriter = null;
            }
        }

        #endregion

        #region Progress


        public void StartKeepAliveThread()
        {
            AveThreadManager.RegisterOperation(KeepAliveThread, true);
        }

        public void StartReportSendingThread()
        {
            Thread t = new Thread(SendReportByNumber) { IsBackground = false };
            t.Start();
        }

        public void StartGetEndUserProgressThread()
        {
            var t = new Thread((GetEndUserProgress)) { IsBackground = true };
            t.Start();
        }



        private void KeepAliveThread()
        {
            try
            {
                SendJobStatus(true, this.jobInfo);
                this.jobInfo.Progress = currentProgress;
                //this.jobInfo.Progress = GetProgress();
            }
            catch (Exception ex)
            {
                log.Error(@"Looks up a localized string similar to An error occurred while sending the job status. Error message: {0}.", ex.ToString());
            }
        }

        private void SendReportByNumber()
        {
            string tempReportPath = Config.JobDir + Path.DirectorySeparatorChar + TEMP_REPORT_FILE_NAME;
            string copyReportPath = Config.JobDir + Path.DirectorySeparatorChar + COPY_REPORT_FILE_NAME;
            while (!File.Exists(tempReportPath))
            {
                Thread.Sleep(100);
            }
            try
            {
                using (FileStream readerStream = new FileStream(tempReportPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader reader = new StreamReader(readerStream))
                    {
                        int lineCount = 0;
                        StringBuilder sb = new StringBuilder();
                        while (true)
                        {
                            if (!reader.EndOfStream)
                            {
                                string eachLine = reader.ReadLine();
                                if (eachLine != string.Empty)
                                {
                                    sb.AppendLine(eachLine);
                                    lineCount++;
                                }
                                if ((Config.RestoreLevel == BackupLevel.SiteCollection || Config.RestoreLevel == BackupLevel.Site) && lineCount > 0)
                                {
                                    RealSendDetailedReport(copyReportPath, sb.ToString().Trim());
                                    ResetRecord(out sb, out lineCount);
                                }
                                if (lineCount == REPORT_LIMITED_NUMBER)
                                {
                                    RealSendDetailedReport(copyReportPath, sb.ToString().Trim());
                                    ResetRecord(out sb, out lineCount);
                                }
                            }
                            else
                            {
                                Thread.Sleep(100);
                                if (isReportReachEnd)
                                {
                                    RealSendDetailedReport(copyReportPath, sb.ToString().Trim());
                                    ResetRecord(out sb, out lineCount);
                                    isReportReachEnd = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("Looks up a localized string similar to An error orccurred while sending report by thread. Error message: {0}..", e.Message);
            }
            finally
            {
                if (File.Exists(copyReportPath))
                {
                    File.Delete(copyReportPath);
                }
            }
        }

        private void ResetRecord(out StringBuilder sb, out int lineCount)
        {
            sb = new StringBuilder();
            lineCount = 0;
        }

        private void RealSendDetailedReport(string copyReportPath, string content)
        {
            using (FileStream fileStream = new FileStream(copyReportPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                using (StreamWriter output = new StreamWriter(fileStream))
                {
                    output.Write(content);
                }
            }
            var jobInfo = new SubJobDto { Id = Config.SubJobId, ParentId = Config.JobId };
            SendJobDetails(jobInfo);
        }

        private void GetEndUserProgress()
        {

            while (true)
            {
                try
                {
                    BaseJobDto dto = JobMoniterAPIService.LoadJob(this.jobInfo.Id.Substring(0, this.jobInfo.Id.LastIndexOf('_')));
                    if (dto != null)
                    {
                        double progress = dto.Progress;
                        JobStatusInfo endUserArchiveJobInfo = new JobStatusInfo()
                        {
                            Id = this.jobInfo.Id,
                            Progress = (int)progress,
                            State = jobInfo.State
                        };

                        SendEndUserArchiveRestoreJobProgress(endUserArchiveJobInfo);
                    }
                    Thread.Sleep(1000 * 5);
                    if (this.jobInfo.State == 2 || this.jobInfo.State == 3 || this.jobInfo.State == 4 || this.jobInfo.State == 7)
                    {
                        break;
                    }
                }

                catch (Exception ex)
                {
                    log.Error(@"Looks up a localized string similar to Get progress from control panel exception.{0}", ex.ToString());
                }
            }
        }

        private void AddRehydrationAzureBlobJobSummaryComment()
        {
            propertyItems.Add(new PropertyItem() { PropertyType = ParamKey.Message, Key = "ArchiverRehydrationAzureBlobComments", DefaultValue = "The current job contains data in the Azure archive tier, so it takes time for Blob rehydration from the Archive tier." });
        }

        private void AddBlockedArchiverRehydrationAzureBlobComment()
        {
            propertyItems.Add(new PropertyItem() { PropertyType = ParamKey.Message, Key = "BlockedArchiverRehydrationAzureBlobComments", DefaultValue = "The current job contains data in the Azure archive tier, and the current setting disables endUser to restore data in the Archive tier." });
        }

        public void AddJobSummaryComment(string key)
        {
            if (key.Equals("ArchiverRehydrationAzureBlobComments", StringComparison.OrdinalIgnoreCase))
            {
                AddRehydrationAzureBlobJobSummaryComment();
            }
            else if (key.Equals("BlockedArchiverRehydrationAzureBlobComments", StringComparison.OrdinalIgnoreCase))
            {
                AddBlockedArchiverRehydrationAzureBlobComment();
            }
        }

        private IJobMonitorAPIService JobMoniterAPIService
        {
            get
            {
                if (jobMoniterAPIService == null)
                {
                    //jobMoniterAPIService = WcfUtility.GetManagerService<IJobMonitorAPIService>();
                }
                return jobMoniterAPIService;
            }
        }
        #endregion
    }

    internal static class ReportNodeHeader
    {
        internal const string Sucess = "T";
        internal const string Fail = "F";
        internal const string Site = "E";
        internal const string Web = "W";
        internal const string List = "L";
        internal const string Folder = "F";
        internal const string Item = "I";
        internal const string App = "Y";
        internal const string Project = "J";

        internal const string Type = "t";
        internal const string Size = "s";
        internal const string Skiped = "isSkipped";
    }


}
