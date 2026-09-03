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

namespace Office365GroupRestore
{
    using AvePoint.GCommon.Contract.StorageOptimization.Object;
    using AvePoint.Wrapper.Common;
    using ExchangeCommonWrapper;
    using ExchangeUtility.Graph;
    using Job.ModernManagement.Report;
    using Microsoft365.Authentication;
    using Microsoft365Backup.DataBuilder.TeamHtml;
    using Office365GroupBackup;
    using RAArchiverCommon;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    internal abstract class RestoreConversationAsHtml : RestoreConversation
    {
        public RestoreConversationAsHtml(BaseRestoreHelperBatch baseHelper) : base(baseHelper)
        {
        }

        internal string GenerateFileName(MetadataEntity baseEntity, string channelName)
        {
            var title = GetEntityTitle(baseEntity);
            var itemTime = RestoreConfig.ItemCreateTimeInfo.TryGetValue(title, out var ticks) ? new DateTime(ticks) : DateTime.Parse(baseEntity.SendTime.Split()[0]);
            var suffix = 1;
            var basicFileName = $"{channelName}-{itemTime.ToString("MMMM yyyy", CultureInfo.CreateSpecificCulture("en-GB"))}";
            return GenerateRandomName(basicFileName, suffix);
        }

        internal MemoryStream GenerateConversationHtml(IEnumerable<ExchangeDataBlockForBatch> dataCollection)
        {
            var content = new MemoryStream();
            using (var builder = new TeamsHtmlBuilder(content))
            {
                foreach (var dataBlock in dataCollection)
                {
                    var baseEntity = dataBlock.RestoreData.Metadata;
                    InitReport(baseEntity, baseEntity.DisplayPath);
                    ReportDto.Title = TeamsConst.ConversationMessageReportTitle;
                    ReportDto.Type = ReportNodeHeader.Conversation;

                    try
                    {
                        var (item, siteUrlMap) = GenerateConversationItem(dataBlock.RestoreData, baseEntity);

                        if (item.Type.IsSystemMessage()) continue;

                        if (Config.IsSkipRestoreConversation)
                        {
                            ReportDto.Status = ReportStatus.Skipped;
                            Report.AddRestoreReport(ReportDto);
                            continue;
                        }

                        builder.AppendOne(item, siteUrlMap);
                    }
                    catch (Exception ex)
                    {
                        ReportDto.Status = ReportStatus.Failed;
                        ReportDto.ErrorMessage = ex.Message;
                        logger.Error("Append conversation error: {0}.", ex);
                    }
                    Report.AddRestoreReport(ReportDto);
                    SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(ReportDto.Size, ReportDto.SourcePath);
                }
            }
            content.Seek(0L, SeekOrigin.Begin);
            return content;
        }

        protected override void RealRestore(IEnumerable<ExchangeDataBlockForBatch> dataCollection)
        {
            if (_SiteNotFound) throw new Exception("Agent.Teams.SiteNotFound_152A5656-8624-4179-86C7-8684C2B1B5F0");

            var metadata = dataCollection.First().RestoreData.Metadata;

            var (channelName, folderName, fileName) = GenerateFileInfo(metadata);

            if (!CanUpdateFile())
            {
                ReportFileFolderError(channelName, fileName, metadata.InternalPath);
                return;
            }

            using (var content = GenerateConversationHtml(dataCollection))
            {

                if (Config.IsSkipRestoreConversation)
                {
                    logger.Info("Skip upload html file to restore conversation for channel {0}.", channelName);
                    return;
                }

                var uploader = GenerateTeamsFileUploader(_CurrentChannel?.FilesFolderUrl);
                try
                {
                    uploader.UploadFileToDocumentLibrary(folderName, fileName, content, true);
                }
                catch (Exception ex) when (ex.Message.Contains("The length of the URL for this request exceeds the configured maxUrlLength value"))
                {
                    ReportDto.Status = ReportStatus.Failed;
                    ReportDto.ErrorMessage = ex.Message;
                    Report.AddRestoreReport(ReportDto);
                }
                catch (Exception ex) when (ex.IsUnauthorizedException() || ex.IsForbiddenException())
                {
                    ReportDto.Status = ReportStatus.Failed;
                    ReportDto.ErrorMessage = ExchangeReportMessage.CreateReportMessage("Agent.Teams.NotChannelMember_C65279D3-C359-61DA-3350-2FE673A979C5", I18NDataCollector.GetData(DynamicDataKey.UserName));
                    Report.AddRestoreReport(ReportDto);
                }
            }
        }

        protected abstract string GenerateChannelName(MetadataEntity baseEntity);

        protected abstract string GenerateparentFolderName(string channelName);

        protected abstract string GetEntityTitle(MetadataEntity baseEntity);

        protected abstract (ConversationItem Item, Dictionary<string, string> SiteUrlMap) GenerateConversationItem(ExchangeRestoreDataForBatch restoreData, MetadataEntity baseEntity);

        public (string, string, string) GenerateFileInfo(MetadataEntity baseEntity)
        {
            var channelName = GenerateChannelName(baseEntity);

            var folderName = _CurrentChannel.IsSharedChannel()
                ? Config.JobId.Substring(0, Config.JobId.LastIndexOf('_'))
                : $"{GenerateparentFolderName(channelName).TrimEnd(' ')}/{Config.JobId.Substring(0, Config.JobId.LastIndexOf('_'))}";

            var fileName = GenerateFileName(baseEntity, channelName);

            return (channelName, folderName, fileName);
        }

        private string GenerateRandomName(string basicFileName, int suffix)
        {
            var fileName = $"{basicFileName}_{suffix}.html";
            if (RestoreConfig.FileNames.Contains(fileName))
            {
                suffix += 1;
                return GenerateRandomName(basicFileName, suffix);
            }
            RestoreConfig.FileNames.Add(fileName);
            return fileName;
        }

        private bool CanUpdateFile() => !(_CurrentChannel?.CurrenIsPrivateChannelSite ?? false) || !string.IsNullOrEmpty(_CurrentChannel?.FilesFolderUrl);

        private void ReportFileFolderError(string channelName, string fileName, string intenalpath)
        {
            if (string.IsNullOrEmpty(_CurrentChannel?.DisplayName))
            {
                logger.Warn("Create private channel failed, so skiped.");
                return;
            }

            Report.AddRestoreReport(new ReportDto
            {
                EntityType = AvePoint.GCommon.Contract.Server.Job.Object.JobReportDetailEntityType.Objects,
                Title = fileName,
                Type = 'R',
                Status = ReportStatus.Failed,
                ErrorMessage = "Agent.Office365Group.NotfoundChannelFilesFolder_6FC1553B-9F92-41A4-80B2-4E8633D35136",
                SourcePath = string.Join("\\", intenalpath.Split(ExchangeConstants.PathParser).Take(2)),
                Path = $"{RestoreConfig.CurrentRestoreMailbox}\\{channelName}",
                //Option = RestoreOption.NewCreated.GetEnumDescription()
            });
            logger.Error("Cannot find the channel's file folder.");
        }

        private TeamsFileUploader GenerateTeamsFileUploader(string filesFolderUrl = null)
        {
            //var token = BposInfos[RestoreConfig.CurrentRestoreMailbox].ConvertToAveBPOSAccountInfo().Convert2TokenProvider(new List<ProviderType> { ProviderType.AppProfile, ProviderType.ServiceAccount });//studo
            var token = M365APIService.BposInfo.ConvertToAveBPOSAccountInfo().Convert2TokenProvider();
            if (string.IsNullOrEmpty(filesFolderUrl))
            {
                var filesUrl = string.IsNullOrEmpty(_GroupSiteFilesUrl) ? string.Empty : _GroupSiteFilesUrl.Substring(_GroupSiteFilesUrl.LastIndexOf('/') + 1);
                return new TeamsFileUploader(_GroupSiteUrl, filesUrl, token);
            }
            //https://m365x634191.sharepoint.com/sites/nat_team_sct2-private_c2/Shared%20Documents/private_c2
            var urlInfo = _CurrentChannel.FilesFolderUrl.Split('/');
            var offset = urlInfo.Length == 6 ? 5 : urlInfo.Length - 2;
            var siteUrl = string.Join("/", urlInfo.Take(offset));
            var docFolderName = urlInfo[offset];
            return new TeamsFileUploader(siteUrl, docFolderName, token);
        }
    }
}