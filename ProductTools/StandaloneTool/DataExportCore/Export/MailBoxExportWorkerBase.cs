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
using Aspose.Email;
using Aspose.Email.Calendar;
using Aspose.Email.Calendar.Recurrences;
using AvePoint.GCommon.Contract.GranularRestore.Object;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography.DataEncryptionManagement;
using AvePoint.Media.Service.DomainModel;
using AvePoint.Metadata;
using AvePoint.RA.Common.Configurations.Bootstrap;
using AvePoint.RA.Common.GraphApi.Mail;
using AvePoint.RA.CommonUtil;
using DataExportCore.Cache;
using DataExportCore.Discover.Node;
using DataExportCore.Utils;
using Media.Service.ArchiverBackup;
using MediaDataIO;
using Microsoft.Graph.Models;
using Office365GroupRestore;
using Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataExportCore.Export
{
    public abstract class MailBoxExportWorkerBase : IDisposable
    {
        private static readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod()?.DeclaringType ?? typeof(MailBoxExportWorkerBase));
        protected Reporter Reporter;
        protected ExportQueue<ExchangeDiscoverNode> ExportQueue;
        protected IXSystem DestinationSystem;
        protected string GroupAddress = string.Empty;
        private string tempPath = "Temp" + Guid.NewGuid().ToString();
        protected bool isUpload = false;
        public MailBoxExportWorkerBase(Reporter report, ExportQueue<ExchangeDiscoverNode> exportQueue, IXSystem destinationSystem, string groupAddress, bool isUpload)
        {
            this.Reporter = report;
            this.ExportQueue = exportQueue;
            this.DestinationSystem = destinationSystem;
            this.GroupAddress = groupAddress;
            this.isUpload = isUpload;
        }

        public string Process()
        {
            try
            {
                ExchangeDiscoverNode node;
                while ((node = ExportQueue.MoveNext()) != null)
                {
                    try
                    {
                        node.ExportPath = ExportUtility.BuildExportPath(string.IsNullOrEmpty(GroupAddress) ? GlobalCache.ExportLocation : Path.Combine(GlobalCache.ExportLocation, GroupAddress, I18NEntity.GetString("SATool_ExportPath_GroupMailBoxes")), "", "", node.Level);
                        switch (node)
                        {
                            case MailBoxDiscoveryNode:
                                ExportMailBox(node);
                                break;
                            case MailDiscoveryNode mailNode:
                                ExportMail(mailNode);
                                break;
                            default:
                                break;
                        }
                    }
                    catch(Exception ex)
                    {
                        logger.Error($"An error occurs while export mailbox node. Ex:{ex}");
                        Reporter.RecordFailed(node, I18NEntity.GetString("SATool_ExportItemUnexpectedError"), GroupAddress, node.ParentName ?? string.Empty);
                    }
                }
                if (isUpload)
                {
                    return Path.Combine(GroupAddress, I18NEntity.GetString("SATool_ExportPath_GroupMailBoxes"));
                }
                return Path.Combine(GlobalCache.ExportLocation, GroupAddress, I18NEntity.GetString("SATool_ExportPath_GroupMailBoxes"));

            }
            catch (Exception e) 
            {
                logger.Error($"An error occurred while export process. Ex:{e}");
                throw;
            }
            finally
            {
                Reporter.Complete();
            }
        }

        private void ExportMail(MailDiscoveryNode node)
        {
            Reporter.CurrentFile = node.Index.Name;
            try
            {
                if (GlobalCache.IsSkipAPData && GlobalDeviceCache.IsDeviceAPStorage(node.StorageId))
                {
                    logger.Info($"Skip export item [{node.Name}] with storageId [{node.StorageId}] because it is AP storage and Skip AP data is enabled.");
                    Reporter.RecordSkipped(node, I18NEntity.GetString("SATool_SkipExportContentFileInAPStorage"), GroupAddress);
                    return;
                }

                string exportPath = string.Empty;
                if (GlobalDeviceCache.IsStorageOpenFailed(node.StorageId, out string? type))
                {
                    throw new ManagedException(ErrorType.CannotOpenDevice, new[] { node.StorageId, type ?? StorageDeviceType.None.ToString() });
                }
                if (node.DataEncryptionInfo != null) DataEncryptionInfoManager.PutEncryptionInfo(node.DataEncryptionInfo, Encoding.UTF8.GetString(node.DataEncryptionInfo.EncryptedDynamicKey));
                exportPath = node.ExportPath;
                var context = new DataContextBase
                {
                    ContentDataPosition = new DataPosition
                    {
                        StartFileNumber = node.Index.CurrentItemContentDataStartFileNumber,
                        StartOffset = node.Index.CurrentItemContentDataStartOffset,
                        PrefixNumber = node.Index.CurrentItemContentDataFilePrefixNumber,
                        ContentLength = node.Index.CurrentItemContentDataTotalLength,
                        FileType = MediaDataIO.FileType.Content,
                        ItemPageSize = node.Index.CurrentItemPageSize
                    },
                    MetaDataPosition = new DataPosition
                    {
                        StartFileNumber = node.Index.CurrentItemMetaDataStartFileNumber,
                        StartOffset = node.Index.CurrentItemMetaDataStartOffset,
                        PrefixNumber = node.Index.CurrentItemMetaDataFilePrefixNumber,
                        ContentLength = node.Index.CurrentItemMetaDataAndContentDataTotalLength - node.Index.CurrentItemContentDataTotalLength,
                        FileType = MediaDataIO.FileType.MetaData
                    },
                    DataPathGenerator = new TeamsMediaDataPathGenerator(DataModule.EXOPlatform, node.Index.BackupJobId, GroupAddress, false, tempPath),
                    ItemDataMode = (byte)node.Index.CurrentItemDataMode
                };
                var itemReader = new ItemDataReader(context, GlobalDeviceCache.GetDeviceById(node.StorageId));
                ExchangeRestoreData exchangeRestoreData = new ExchangeRestoreData();
                var restoreData = new RestoreStream(itemReader, "");
                exchangeRestoreData.RestoreStream = restoreData;
                HandleMetadata(exchangeRestoreData);
                HandleContent(exchangeRestoreData);
                RestoreToStorage(exchangeRestoreData, node);

                if(node.Index.HasAttach)
                {
                    foreach (var attachItem in node.AttachItems)
                    {
                        attachItem.ExportPath = exportPath;
                        ExportAttachItem(attachItem);
                    }
                }
            }
            catch (ManagedException me)
            {
                logger.Error($"An error occurred while export item {node.Level} with id {node.Id}. ExType: {me.ErrorType}, Ex: {me}");
                Reporter.RecordFailed(node, me.Message, GroupAddress);
            }
            catch (FileNotFoundException ex)
            {
                logger.Error($"Cannot find the archived content file [{ex.FileName}] to restore content for item {node.Level} with id {node.Id}. Ex: {ex}");
                Reporter.RecordFailed(node, string.Format(I18NEntity.GetString("SATool_ContentFileNotFoundError"), ex.FileName), GroupAddress);
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while export item {node.Level} with id {node.Id}. Ex: {ex}");
                Reporter.RecordFailed(node, I18NEntity.GetString("SATool_ExportItemUnexpectedError"), GroupAddress);
            }
        }

        private void ExportAttachItem(AttachItemDiscoveryNode node)
        {
            Reporter.CurrentFile = node.Index.Name;
            try
            {
                if (GlobalDeviceCache.IsStorageOpenFailed(node.StorageId, out string? type))
                {
                    throw new ManagedException(ErrorType.CannotOpenDevice, new[] { node.StorageId, type ?? StorageDeviceType.None.ToString() });
                }
                if (node.DataEncryptionInfo != null) DataEncryptionInfoManager.PutEncryptionInfo(node.DataEncryptionInfo, Encoding.UTF8.GetString(node.DataEncryptionInfo.EncryptedDynamicKey));
                if (node.Index.ContentLength != 0L)
                {
                    var context = new DataContextBase
                    {
                        ContentDataPosition = new DataPosition
                        {
                            StartFileNumber = node.Index.CurrentItemContentDataStartFileNumber,
                            StartOffset = node.Index.CurrentItemContentDataStartOffset,
                            PrefixNumber = node.Index.CurrentItemContentDataFilePrefixNumber,
                            ContentLength = node.Index.CurrentItemContentDataTotalLength,
                            FileType = MediaDataIO.FileType.Content,
                            ItemPageSize = node.Index.CurrentItemPageSize
                        },
                        MetaDataPosition = new DataPosition
                        {
                            StartFileNumber = node.Index.CurrentItemMetaDataStartFileNumber,
                            StartOffset = node.Index.CurrentItemMetaDataStartOffset,
                            PrefixNumber = node.Index.CurrentItemMetaDataFilePrefixNumber,
                            ContentLength = node.Index.CurrentItemMetaDataAndContentDataTotalLength - node.Index.CurrentItemContentDataTotalLength,
                            FileType = MediaDataIO.FileType.MetaData
                        },
                        DataPathGenerator = new TeamsMediaDataPathGenerator(DataModule.EXOPlatform, node.Index.BackupJobId, GroupAddress, false, tempPath),
                        ItemDataMode = (byte)node.Index.CurrentItemDataMode
                    };
                    var itemReader = new ItemDataReader(context, GlobalDeviceCache.GetDeviceById(node.StorageId));
                    ExchangeRestoreData exchangeRestoreData = new ExchangeRestoreData();
                    var restoreData = new RestoreStream(itemReader, "");
                    exchangeRestoreData.RestoreStream = restoreData;
                    HandleMetadata(exchangeRestoreData);
                    HandleContent(exchangeRestoreData);
                    RestoreToStorage(exchangeRestoreData, node);
                }
            }
            catch (ManagedException me)
            {
                logger.Error($"An error occurred while export item {node.Level} with id {node.Id}. ExType: {me.ErrorType}, Ex: {me}");
                Reporter.RecordFailed(node, me.Message, GroupAddress, node.ParentName ?? string.Empty);
            }
            catch (FileNotFoundException ex)
            {
                logger.Error($"Cannot find the archived content file [{ex.FileName}] to restore content for item {node.Level} with id {node.Id}. Ex: {ex}");
                Reporter.RecordFailed(node, string.Format(I18NEntity.GetString("SATool_ContentFileNotFoundError"), ex.FileName), GroupAddress, node.ParentName ?? string.Empty);
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while export item {node.Level} with id {node.Id}. Ex: {ex}");
                Reporter.RecordFailed(node, I18NEntity.GetString("SATool_ExportItemUnexpectedError"), GroupAddress, node.ParentName ?? string.Empty);
            }
        }

        private void RestoreToStorage(ExchangeRestoreData exchangeRestoreData, ExchangeDiscoverNode node)
        {
            switch (node.Index.Type)
            {
                case (int)ExchangeDataBlockType.Post:
                    ExportPostData(exchangeRestoreData, node);
                    break;
                case (int)ExchangeDataBlockType.CalendarEvent:
                    ExportEventData(exchangeRestoreData, node);
                    break;
                case (int)ExchangeDataBlockType.Attachment:
                    ExportAttachFile(exchangeRestoreData, node);
                    break;
            }
        }

        private void ExportAttachFile(ExchangeRestoreData exchangeRestoreData, ExchangeDiscoverNode node)
        {
            string conversationFolderPath = SecurityUtils.SafeCombinePath(BuildStorageInfoExportPath(node.ExportPath), I18NEntity.GetString("SATool_Folder_Conversation"));
            var internalConversationFolder = SecurityUtils.SafeCombinePath(conversationFolderPath, node.ParentName);
            string parentName = RemoveNotAllowedChars(node.ParentName);
            string folderName = SecurityUtils.SafeCombinePath(conversationFolderPath, parentName);
            var info = new StorageInfo { HighName = folderName, LowName = String.Empty };
            var tempFile = new StorageInfo { HighName = internalConversationFolder, LowName = node.Name + "tmp" };
            var attachment = new StorageInfo { HighName = internalConversationFolder, LowName = node.Name };
            try
            {
                using (XStream stream = DestinationSystem.OpenStream(tempFile, FileMode.Create))
                {
                    byte[] buffer = new byte[64 * 1024];
                    while (true)
                    {
                        int len = exchangeRestoreData.ContentStream.Read(buffer, 0, buffer.Length);
                        if (len <= 0) break;
                        stream.Write(buffer, 0, len);
                    }
                }
                using (var base64Stream = this.DestinationSystem.OpenStream(tempFile, FileMode.Open))
                {
                    using (var fileStream = this.DestinationSystem.OpenStream(attachment, FileMode.Create))
                    {
                        var transform = new FromBase64Transform();
                        byte[] tempBuffer = new byte[4096];
                        int bytesRead;

                        while ((bytesRead = base64Stream.Read(tempBuffer, 0, tempBuffer.Length)) > 0)
                        {
                            byte[] decoded = transform.TransformFinalBlock(tempBuffer, 0, bytesRead);
                            fileStream.Write(decoded, 0, decoded.Length);
                        }
                        node.ExportedFileSize = fileStream.Length;
                    }
                }
            }
            catch(Exception e)
            {
                logger.Error($"restore failed when restore attachment:{node.Name},error:{e}");
                throw;
            }
            finally
            {
                if(DestinationSystem.FileExists(tempFile))
                    DestinationSystem.DeleteFile(tempFile);
            }
            
            node.ExportPath = Path.Combine(node.ExportPath, I18NEntity.GetString("SATool_Folder_Conversation"), node.ParentName, node.Name);
            Reporter.RecordSuccessful(node, teamsGroupAddress: GroupAddress, mailBoxName: node.ParentName);
        }

        private void ExportEventData(ExchangeRestoreData exchangeRestoreData, ExchangeDiscoverNode node)
        {
            string parentName = RemoveNotAllowedChars(node.ParentName);
            string folderName = SecurityUtils.SafeCombinePath(I18NEntity.GetString("SATool_ObjectLevel_Calendar"), parentName);
                var metaList = exchangeRestoreData.MetadataLists;
                GroupCalendarEvent calendarEvent = null;
                foreach (var meta in metaList)
                {
                    if (meta.MetadataType == AvePoint.Metadata.AveMetadataType.ExchangeCalendarEvent)
                    {
                        string? metaJson = meta.GetMetadataObject().ToString();

                        if (!string.IsNullOrEmpty(metaJson))
                        {
                            calendarEvent = SerializerHelper.DeserializeByJsonConvert<GroupCalendarEvent>(metaJson);
                        }
                    ExportEventToIcs(calendarEvent, node.ExportPath, folderName, node);
                    }
                }
            }

        private string StripHtmlTags(string html)
        {
            string noTagNewlines = Regex.Replace(html, @"<[^>]*[\r\n\t]*[^>]*>", " ");
            noTagNewlines = Regex.Replace(noTagNewlines, @"[ ]+", " ");
            return noTagNewlines.Trim();
        }

        public void ExportEventToIcs(GroupCalendarEvent graphEventData, string icsFilePath, string folder, ExchangeDiscoverNode node)
        {
            string subject = graphEventData.Subject ?? "Unnamed event";
            string location = graphEventData.Location?.DisplayName ?? string.Empty;
            string description = StripHtmlTags(graphEventData.Body?.Content ?? string.Empty);

            DateTime startTime = DateTime.Parse(graphEventData.Start.DateTime);
            DateTime endTime = DateTime.Parse(graphEventData.End.DateTime);
            string timeZone = graphEventData.Start.TimeZone ?? "UTC";

            var appointment = new Appointment(location, subject, description, startTime, endTime, new Aspose.Email.MailAddress(graphEventData.Organizer.MailAddress.Address, graphEventData.Organizer.MailAddress.Name), new MailAddressCollection())
            {
                StartTimeZone = timeZone,
                EndTimeZone = timeZone,
            };
            //appointment.IsDescriptionHtml = true;
            if (graphEventData.Attendees != null)
            {
                foreach (var attendee in graphEventData.Attendees)
                {
                    appointment.Attendees.Add(new MailAddress(
                        attendee.MailAddress.Address,
                        attendee.MailAddress.Name
                    ));
                }
            }
            try
            {
                appointment.MicrosoftBusyStatus = (MSBusyStatus)graphEventData.LegacyFreeBusyStatus;
                if (graphEventData.Recurrence.Pattern.Type == "weekly")
                {
                    WeeklyRecurrencePattern weeklyRecurrencePattern = new WeeklyRecurrencePattern(GetDateTimeByString(graphEventData.Recurrence.Range.EndDate, graphEventData.Recurrence.Range.RecurrenceTimeZone), graphEventData.Recurrence.Pattern.Interval);
                    weeklyRecurrencePattern.StartDays = new CalendarDay[graphEventData.Recurrence.Pattern.DaysOfWeek.Length];
                    int i = 0;
                    foreach (var day in graphEventData.Recurrence.Pattern.DaysOfWeek)
                    {
                        weeklyRecurrencePattern.StartDays[i] = ((CalendarDay)System.Enum.Parse(typeof(CalendarDay), day.First().ToString().ToUpper() + string.Join("", day.Skip(1)), true));
                        i++;
                    }
                    appointment.Recurrence = weeklyRecurrencePattern;
                }
                if (graphEventData.Recurrence.Pattern.Type.Contains("Yearly"))
                {
                    YearlyRecurrencePattern yearlyRecurrencePattern = new YearlyRecurrencePattern();
                    yearlyRecurrencePattern.EndDate = GetDateTimeByString(graphEventData.Recurrence.Range.EndDate, graphEventData.Recurrence.Range.RecurrenceTimeZone);
                    yearlyRecurrencePattern.Interval = graphEventData.Recurrence.Pattern.Interval;
                    yearlyRecurrencePattern.StartPosition = (DayPosition)System.Enum.Parse(typeof(DayPosition), graphEventData.Recurrence.Pattern.Index.First().ToString().ToUpper() + string.Join("", graphEventData.Recurrence.Pattern.Index.Skip(1)));
                    yearlyRecurrencePattern.StartDay = (CalendarDay)System.Enum.Parse(typeof(DayPosition), GetDateTimeByString(graphEventData.Recurrence.Range.StartDate, graphEventData.Recurrence.Range.RecurrenceTimeZone).DayOfWeek.ToString());
                    yearlyRecurrencePattern.StartOffset = graphEventData.Recurrence.Pattern.DayOfMonth;
                    yearlyRecurrencePattern.StartMonth = (CalendarMonth)graphEventData.Recurrence.Pattern.Month;
                    appointment.Recurrence = yearlyRecurrencePattern;
                }
                if (graphEventData.Recurrence.Pattern.Type.Contains("Monthly"))
                {
                    MonthlyRecurrencePattern monthlyRecurrencePattern = new MonthlyRecurrencePattern();
                    monthlyRecurrencePattern.EndDate = GetDateTimeByString(graphEventData.Recurrence.Range.EndDate, graphEventData.Recurrence.Range.RecurrenceTimeZone);
                    monthlyRecurrencePattern.Interval = graphEventData.Recurrence.Pattern.Interval;
                    monthlyRecurrencePattern.StartPosition = (DayPosition)System.Enum.Parse(typeof(DayPosition), graphEventData.Recurrence.Pattern.Index.First().ToString().ToUpper() + string.Join("", graphEventData.Recurrence.Pattern.Index.Skip(1)));
                    monthlyRecurrencePattern.StartDay = (CalendarDay)System.Enum.Parse(typeof(DayPosition), GetDateTimeByString(graphEventData.Recurrence.Range.StartDate, graphEventData.Recurrence.Range.RecurrenceTimeZone).DayOfWeek.ToString());
                    monthlyRecurrencePattern.StartOffset = graphEventData.Recurrence.Pattern.DayOfMonth;
                    appointment.Recurrence = monthlyRecurrencePattern;
                }
                if (graphEventData.Recurrence.Pattern.Type == "daily")
                {
                    DailyRecurrencePattern dailyRecurrencePattern = new DailyRecurrencePattern(GetDateTimeByString(graphEventData.Recurrence.Range.EndDate, graphEventData.Recurrence.Range.RecurrenceTimeZone), graphEventData.Recurrence.Pattern.Interval);
                    appointment.Recurrence = dailyRecurrencePattern;
                }
            }
            catch (Exception e)
            {
                logger.Error($"Failed to set MicrosoftBusyStatus for event {subject}. Error: {e.Message}");
            }
            string fileName = subject;
            if (fileName.Length > MaxFileNameLength)
            {
                fileName = fileName.Substring(0, MaxFileNameLength);
            }
            fileName = RemoveNotAllowedChars(fileName);
            icsFilePath = SecurityUtils.SafeCombinePath(icsFilePath, folder, fileName);
            var filePath = icsFilePath + "_" + DateTime.UtcNow.Ticks + ".ics";
            var dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            appointment.Save(filePath, AppointmentSaveFormat.Ics);
            long messageSize = new FileInfo(filePath).Length;
            node.ExportedFileSize = messageSize;
            if (isUpload)
            {
                byte[] buffer = new byte[64 * 1024];
                using var fileStream = File.OpenRead(filePath);
                StorageInfo info = XConvert.FromNames(SecurityUtils.SafeCombinePath(BuildStorageInfoExportPath(node.ExportPath), "Calendar", node.ParentName), ExportUtility.ReplaceInvalidChar(fileName, true));
                using (XStream stream = DestinationSystem.OpenStream(info, FileMode.OpenOrCreate))
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    while (true)
                    {
                        int len = fileStream.Read(buffer, 0, buffer.Length);
                        if (len <= 0) break;
                        stream.Write(buffer, 0, len);
                    }
                    stopwatch.Stop();
                }
            }
            Reporter.RecordSuccessful(node, exportPath: filePath, teamsGroupAddress: GroupAddress);
        }

        private DateTime GetDateTimeByString(string endDateStr, string timeZoneStr)
        {
            DateTime endDateUtc = DateTime.ParseExact(endDateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneStr);

            DateTime endDateInTimeZone = DateTime.SpecifyKind(endDateUtc, DateTimeKind.Unspecified);
            DateTime endDateWithTimeZone = TimeZoneInfo.ConvertTimeToUtc(endDateInTimeZone, timeZone);
            return endDateWithTimeZone;
        }

        private void ExportPostData(ExchangeRestoreData exchangeRestoreData, ExchangeDiscoverNode node)
        {
            string path = string.Empty;
            try
            {
                string conversationFolderPath = SecurityUtils.SafeCombinePath(node.ExportPath, I18NEntity.GetString("SATool_Folder_Conversation"));
                var internalConversationFolder = SecurityUtils.SafeCombinePath(conversationFolderPath, node.ParentName);
                string parentName = RemoveNotAllowedChars(node.ParentName);
                string folderName = SecurityUtils.SafeCombinePath(conversationFolderPath, parentName);
                path = internalConversationFolder;
                var info = new StorageInfo { HighName = folderName, LowName = String.Empty };
                var metaList = exchangeRestoreData.MetadataLists;
                long size = 0;
                foreach (var meta in metaList)
                {
                    if (meta.MetadataType == AvePoint.Metadata.AveMetadataType.ExchangeItem)
                    {
                        string? metaJson = meta.GetMetadataObject().ToString();
                        RMGraphMailReciver from = null;
                        Dictionary<string, string> metaDataList = new Dictionary<string, string>();
                        if (!string.IsNullOrEmpty(metaJson))
                        {
                            metaDataList = SerializerHelper.DeserializeByJsonConvert<Dictionary<string, string>>(metaJson);
                            from = SerializerHelper.DeserializeByJsonConvert<RMGraphMailReciver>(metaDataList["From"]);
                        }
                        string bodyContent = ReadBodyContent(exchangeRestoreData);
                        size = bodyContent.Length;
                        ExportPostToEml(parentName, from, bodyContent, internalConversationFolder , folderName, metaDataList.ContainsKey("Importance") ? metaDataList["Importance"] : string.Empty, node);
                    }
                }

            }
            catch (Exception e)
            {
                logger.Error($"restore failed when restore post:{path},error:{e}");
            }
        }

        private int MaxFileNameLength = 199;
        public void ExportPostToEml(string topic, RMGraphMailReciver from, string body, string outputPath, string folder, string priority, ExchangeDiscoverNode node)
        {
            AsposeLicenseBootstrap.Setup();
            var message = new MailMessage
            {
                From = new MailAddress(from?.MailAddress.Address, from?.MailAddress.Name),
                Subject = topic,
                Body = body,
                HtmlBody = body,
            };
            message.BodyEncoding = Encoding.UTF8;
            message.SubjectEncoding = Encoding.UTF8;
            string mailName = GroupAddress.Split('@')[0];
            message.To.Add(new MailAddress(GroupAddress, mailName));
            if (priority == Aspose.Email.MailPriority.High.ToString())
            {
                message.Priority = Aspose.Email.MailPriority.High;
            }
            else if (priority == Aspose.Email.MailPriority.Low.ToString())
            {
                message.Priority = Aspose.Email.MailPriority.Low;
            }
            else
            {
                message.Priority = Aspose.Email.MailPriority.Normal;
            }
            string fileName = topic;
            if (fileName.Length > MaxFileNameLength)
            {
                fileName = fileName.Substring(0, MaxFileNameLength);
            }
            fileName = RemoveNotAllowedChars(fileName);
            outputPath = SecurityUtils.SafeCombinePath(outputPath, folder);
            var options = new EmlSaveOptions(MailMessageSaveType.EmlFormat);
            message.IsBodyHtml = true;
            fileName = fileName + "_" + DateTime.UtcNow.Ticks + ".eml";
            string filePath = SecurityUtils.SafeCombinePath(outputPath, fileName);
            var dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            message.Save(filePath, options);

            long messageSize = new FileInfo(filePath).Length;
            node.ExportedFileSize = messageSize;
            
            if (isUpload)
            {
                byte[] buffer = new byte[64 * 1024];
                using var fileStream = File.OpenRead(filePath);
                StorageInfo info = XConvert.FromNames(SecurityUtils.SafeCombinePath(BuildStorageInfoExportPath(node.ExportPath), I18NEntity.GetString("SATool_Folder_Conversation"), node.ParentName), ExportUtility.ReplaceInvalidChar(fileName, true));
                using (XStream stream = DestinationSystem.OpenStream(info, FileMode.OpenOrCreate))
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    while (true)
                    {
                        int len = fileStream.Read(buffer, 0, buffer.Length);
                        if (len <= 0) break;
                        stream.Write(buffer, 0, len);
                    }
                    stopwatch.Stop();
                }
            }
            Reporter.RecordSuccessful(node, exportPath: filePath, teamsGroupAddress: GroupAddress);
        }

        private string RemoveNotAllowedChars(string input)
        {
            return Regex.Replace(input, @"[\\/:*?""<>|]", "");
        }

        private string ReadBodyContent(ExchangeRestoreData data)
        {
            using (var reader = new StreamReader(data.ContentStream))
            {
                string line;
                string result = string.Empty;
                while ((line = reader.ReadLine()) != null)
                {
                    result += line;
                }
                RMGraphMailBody tempResult = SerializerHelper.DeserializeByJsonConvert<RMGraphMailBody>(result);
                return tempResult.Content;
            }
        }

        private void HandleMetadata(ExchangeRestoreData restoreData)
        {
            var metaLists = new List<AveMetadata>();
            AveMetadata meta;
            while ((meta = restoreData.RestoreStream.ReadMetadata()) != null)
            {
                metaLists.Add(meta);
            }
            restoreData.MetadataLists = metaLists;
        }

        private void HandleContent(ExchangeRestoreData restoreData)
        {
            restoreData.ContentStream = restoreData.RestoreStream.OpenContentStream();
        }

        private string ExportMailBox(ExchangeDiscoverNode node)
        {
            Reporter.ConfigForReport(Path.Combine(GroupAddress, I18NEntity.GetString("SATool_ExportPath_GroupMailBoxes")), ExportUtility.IsNeedUploadAndDeleteCache());
            node.ExportPath = Path.Combine(node.ExportPath, node.Name);
            Reporter.RecordSuccessful(node, teamsGroupAddress: GroupAddress);
            return node.ExportPath;
        }
        protected abstract string BuildStorageInfoExportPath(string exportPath);

        public void Dispose()
        {
        }
    }

    public enum ExchangeDataBlockType
    {
        Mailbox = 0,
        Folder = 1,
        Item = 2,
        Index = 3,
        Plan = 4,
        Task = 5,
        Export = 6,
        Attachment = 7,
        Calendar = 8,
        CalendarEvent = 9,
        Post = 10,
    }
}
