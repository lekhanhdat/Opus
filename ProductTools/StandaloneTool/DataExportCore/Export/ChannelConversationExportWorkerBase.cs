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
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Utility.Cryptography.DataEncryptionManagement;
using AvePoint.Media.Common;
using AvePoint.Media.Service.DomainModel;
using AvePoint.Metadata;
using AvePoint.RA.CommonUtil;
using DataExportCore.Cache;
using DataExportCore.Discover.Node;
using DataExportCore.Utils;
using ExchangeCommonWrapper;
using ExchangeUtility;
using ExchangeUtility.Graph;
using MediaDataIO;
using Microsoft365Backup.DataBuilder.TeamHtml;
using Newtonsoft.Json;
using Office365GroupRestore;
using Storage;
using Storage.Util;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace DataExportCore.Export
{
    public abstract class ChannelConversationExportWorkerBase : IDisposable
    {
        private static readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod()?.DeclaringType ?? typeof(MailBoxExportWorkerBase));
        protected Reporter Reporter;
        protected ExportQueue<TeamsDiscoveryNode> ExportQueue;
        protected IXSystem DestinationSystem;
        protected string GroupAddress = string.Empty;
        private long fileSize;
        private List<string> restoredItemId = new List<string>();
        private string tempPath = "Temp" + Guid.NewGuid().ToString();
        private String errorMessage;
        private const string cardAppUnknown = "UNKNOWN";
        private const string cardAppPlaces = "Places";
        private const string cardAppWeather = "Weather";
        private const string cardAppNews = "News";
        public const string ConversationEmptyBody = "<html>\r\n<head>\r\n<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">\r\n</head>\r\n<body>\r\n<div></div>\r\n</body>\r\n</html>\r\n";
        public const string ConversationDeleteBody = "<html>\r\n<head>\r\n<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">\r\n</head>\r\n<body>\r\n<div>This message has been deleted.</div>\r\n</body>\r\n</html>\r\n";
        private static readonly Dictionary<string, string> cardAppIconMapping = new Dictionary<string, string>
        {
            [cardAppUnknown] = string.Empty,
            [cardAppPlaces] = "https://statics.teams.cdn.office.net/evergreen-assets/places/Places_96x96.png?v=0.1",
            [cardAppWeather] = "https://statics.teams.cdn.office.net/evergreen-assets/apps/Weather_largeimage.png?v=0.3",
            [cardAppNews] = "https://statics.teams.cdn.office.net/evergreen-assets/apps/News_largeimage.png?v=0.5",
        };
        protected bool IsUploadata = false;
        private static readonly List<string> imageExtensions = new List<string> { "jpg", "jpeg", "png", };

        public ChannelConversationExportWorkerBase(Reporter report, ExportQueue<TeamsDiscoveryNode> exportQueue, IXSystem destinationSystem, string groupAddress, bool isUploadData)
        {
            this.Reporter = report;
            this.ExportQueue = exportQueue;
            this.DestinationSystem = destinationSystem;
            this.GroupAddress = groupAddress;
            this.IsUploadata = isUploadData;
        }

        public string Process()
        {
            try
            {
                TeamsDiscoveryNode node;
                while ((node = ExportQueue.MoveNext()) != null)
                {
                    try
                    {
                        node.ExportPath = BuildExportPath(string.IsNullOrEmpty(GroupAddress) ? GlobalCache.ExportLocation : Path.Combine(GlobalCache.ExportLocation, GroupAddress, I18NEntity.GetString("SATool_ExportPath_ChannelConversations")), "", "", node.Level);
                        switch (node)
                        {
                            case ContainerDiscoveryNode:
                                ExportContainerNode(node);
                                break;
                            case ChannelDiscoveryNode channelNode:
                                ExportChannelNode(channelNode);
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"An error occurs while export mailbox node. Ex:{ex}");
                        Reporter.RecordFailed(node, I18NEntity.GetString("SATool_ExportItemUnexpectedError"), GroupAddress);
                    }
                }
                if (IsUploadata)
                {
                    return Path.Combine(GroupAddress, I18NEntity.GetString("SATool_ExportPath_ChannelConversations"));
                }
                return Path.Combine(GlobalCache.ExportLocation, GroupAddress, I18NEntity.GetString("SATool_ExportPath_ChannelConversations"));
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

        private void ExportChannelNode(ChannelDiscoveryNode node)
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

                if (GlobalDeviceCache.IsStorageOpenFailed(node.StorageId, out string? type))
                {
                    throw new ManagedException(ErrorType.CannotOpenDevice, new[] { node.StorageId, type ?? StorageDeviceType.None.ToString() });
                }
                if (node.DataEncryptionInfo != null) DataEncryptionInfoManager.PutEncryptionInfo(node.DataEncryptionInfo, Encoding.UTF8.GetString(node.DataEncryptionInfo.EncryptedDynamicKey));
                List<ExchangeRestoreDataForBatch> channelDataBlocks = new List<ExchangeRestoreDataForBatch>();
                ExchangeRestoreDataForBatch dataBlockBatch = null;
                foreach (var topic in node.GetTopics())
                {
                    foreach (var conversation in topic.GetConversationNodes())
                    {
                        var dataBlock = new ExchangeDataBlock();
                        try
                        {
                            dataBlock.FileHeader = HandleHeader(conversation.Index);
                            dataBlock.RestoreData = HandleData(conversation.Index, conversation);
                            dataBlock.FileTail = HandleTail();
                        }
                        catch (PathNotFoundException ex)
                        {
                            logger.Error("An error occurred while handle datablock info. exception:{0}", ex.ToString());
                            dataBlock.FileHeader = new ExchangeFileHeader();
                            dataBlock.RestoreData = new ExchangeRestoreData();
                            dataBlock.FileTail = new RestoreFileTail();
                        }
                        dataBlockBatch = new ExchangeRestoreDataForBatch
                        {
                            RestoreStream = dataBlock.RestoreData.RestoreStream,
                            MetadataLists = dataBlock.RestoreData.MetadataLists,
                        };
                        channelDataBlocks.Add(dataBlockBatch);
                    }
                }
                logger.Info("Start export to HTML");
                ExportToHTML(channelDataBlocks, node);
            }
            catch (Exception e)
            {
                logger.Error($"An error when export channel conversation {node.Name} :{e}");
                Reporter.RecordFailed(node, I18NEntity.GetString("SATool_ExportItemUnexpectedError"), GroupAddress);
            }
        }

        protected abstract string BuildStorageInfoExportPath(string exportPath);


        private void ExportToHTML(List<ExchangeRestoreDataForBatch> channelDataBlocks, TeamsDiscoveryNode node)
        {
            var entity = channelDataBlocks.First().Metadata;
            var dataSource = entity.Type == "IPM.SkypeTeams.Message" ? DataSource.EWS : DataSource.Graph;
            var metadata = channelDataBlocks.First().Metadata;
            var (folderName, fileName) = GenerateFileInfo(metadata, node.Name);
            folderName = Path.Combine(node.ExportPath, folderName);
            folderName = BuildStorageInfoExportPath(folderName);
            var attachment = new StorageInfo { HighName = folderName, LowName = fileName };
            using (var content = GenerateConversationHtml(channelDataBlocks, dataSource, node.Path))
            {
                using (XStream stream = DestinationSystem.OpenStream(attachment, FileMode.OpenOrCreate))
                {
                    byte[] buffer = new byte[64 * 1024];
                    while (true)
                    {
                        int len = content.Read(buffer, 0, buffer.Length);
                        if (len <= 0) break;
                        stream.Write(buffer, 0, len);
                        node.ExportedFileSize += len;
                    }
                }
            }
            node.ExportPath = Path.Combine(node.ExportPath, node.Name, fileName);
            Reporter.RecordSuccessful(node, teamsGroupAddress: GroupAddress);
        }
        private MemoryStream GenerateConversationHtml(List<ExchangeRestoreDataForBatch> dataCollection, DataSource dataSource, string sitePath)
        {
            var content = new MemoryStream();
            using (var builder = new TeamsHtmlBuilder(content))
            {
                foreach (var dataBlock in dataCollection)
                {
                    var baseEntity = dataBlock.Metadata;

                    try
                    {
                        var item = GenerateConversationItem(dataBlock, baseEntity, dataSource, sitePath);

                        if (item.Type == TeamsConst.SystemEvenMessageType || item.Type == TeamsConst.UnknownFutureValueMessageType) continue;

                        builder.AppendOne(item, null);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Append conversation error: {0}.", ex);
                    }
                }
            }
            content.Seek(0L, SeekOrigin.Begin);
            return content;
        }

        protected ConversationItem GenerateConversationItem(ExchangeRestoreDataForBatch restoreData, MetadataEntity baseEntity, DataSource dataSource, string sitePath)
        {
            if (dataSource == DataSource.EWS)
            {
                var addtionalProperties = restoreData.TryGetMetadata<MSTeamConversationItem>(AveMetadataType.ExchangeMicrosoftTeamsConversationItem);

                if (GlobalCache.TopicItemIds.Contains(baseEntity.ExchangeId))
                {
                    return (new ConversationTopic()
                    {
                        PostedBy = baseEntity.Sender,
                        PostedTime = baseEntity.SendTime,
                        Body = ReadToEnd(restoreData.RestoreStream),
                        Subject = baseEntity.DisplayPath.Split('\\').LastOrDefault(),
                        Important = addtionalProperties?.Importance == ImportanceM.High,
                    });
                }

                return (new ConversationReply()
                {
                    PostedBy = baseEntity.Sender,
                    PostedTime = baseEntity.SendTime,
                    Body = ReadToEnd(restoreData.RestoreStream),
                    Important = addtionalProperties?.Importance == ImportanceM.High,
                });
            }

            var message = restoreData.TryGetMetadata<TeamChatMessage>(AveMetadataType.ExchangeMicrosoftTeamsConversationItem);
            using (var stream = restoreData.RestoreStream.OpenContentStream())
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var content = reader.ReadToEnd();
                    try
                    {
                        message.MessageContent = JsonConvert.DeserializeObject<MessageContent>(content);
                    }
                    catch (Exception ex)
                    {
                        logger.Info("It is old version content: {0}.", ex);
                        message.Body.Content = content;
                    }
                }
            }

            if (GlobalCache.TopicItemIds.Contains(baseEntity.Id))
            {
                return new ConversationTopic()
                {
                    PostedBy = baseEntity.Sender,
                    PostedTime = message.CreatedDateTime.ToPostedTime(),
                    Body = RegenerateBody(message, sitePath),
                    Subject = message.Subject,
                    Important = message.Importance.Equals("high", StringComparison.OrdinalIgnoreCase),
                    Type = message.MessageType,
                    HostedContents = message.MessageContent?.HostedContents?.ToDictionary(key => key.TemporaryId, value => value.ContentBytes),
                    Announcement = TeamsMessageUtility.GenerateAnnouncementBanner(message),
                    Reaction = TeamsMessageUtility.GenerateConversationReaction(message)
                };
            }

            return new ConversationReply()
            {
                PostedBy = baseEntity.Sender,
                PostedTime = message.CreatedDateTime.ToPostedTime(),
                Body = RegenerateBody(message, sitePath),
                Important = message.Importance.Equals("high", StringComparison.OrdinalIgnoreCase),
                Type = message.MessageType,
                HostedContents = message.MessageContent?.HostedContents?.ToDictionary(key => key.TemporaryId, value => value.ContentBytes),
                Reaction = TeamsMessageUtility.GenerateConversationReaction(message)
            };
        }

        private string RegenerateBody(TeamChatMessage message, string sitePath)
        {
            if (message.Attachments == null || message.Attachments.Count == 0) return message.Body.Content;

            var attachments = message.Attachments.ToLookup(a => a.Id).ToDictionary(a => a.Key, a => a.FirstOrDefault());
            var doc = new AvePoint.Wrapper.Common.HtmlDocument();
            try
            {
                doc.LoadHtml(message.Body.Content);
                var root = doc.DocumentNode;
                var attachmentNodes = root.SelectNodes("//attachment");
                foreach (var node in attachmentNodes)
                {
                    if (attachments.TryGetValue(node.Id, out var attachment))
                    {
                        if (attachment.ContentType.Equals(TeamUtil.AttachmentAnnouncementBannerType))
                        {
                            node.Remove();
                            continue;
                        }
                        var content = AppendAttachment(message.MessageContent, attachment, sitePath);
                        if (!string.IsNullOrEmpty(content))
                        {
                            node.InnerHtml = content;
                        }
                    }
                }
                return root.InnerHtml;
            }
            catch (Exception ex)
            {
                logger.Warn("Regenerate Message[{0}] body failed, so use the original string. Error: {1}.", message.Id, ex);
                return message.Body.Content;
            }
        }

        private string AppendAttachment(MessageContent messageContent, Attachment attachment, string sitePath)
        {
            switch (attachment.ContentType)
            {
                case TeamUtil.AttachmentReferenceType:
                    return GenerateFileLink(attachment, sitePath);
                case TeamUtil.AttachmentCardHeroType:
                case TeamUtil.AttachmentCardThumbnailTye:
                    return GenerateHeroCard(attachment);
                case TeamUtil.AttachmentMeetingType:
                    if (messageContent == null) return null;
                    var meeting = TeamsMessageUtility.GenerateMeetingMessage(messageContent, attachment);
                    return string.Format(TeamHtmlResources.MeetingAsHtmlTemplate_html, meeting.Title, meeting.BasicInfo);
                case TeamUtil.AttachmentCardCodeSnippetType:
                    if (messageContent == null) return null;
                    return messageContent.CodeSnippets.TryGetValue(attachment.Id, out var codeSnippet) ? TeamsMessageUtility.GenerateCodeSnippetMessage(codeSnippet) : null;
                default:
                    logger.Warn("Unsupported attachment type: [{0}].", attachment.ContentType);
                    return null;
            }
        }

        private string GenerateFileLink(Attachment attachment, string sitePath)
        {
            var extension = attachment.Name.Split('.').Last().ToLower();
            var channelFilesUrl = sitePath;
            return imageExtensions.Contains(extension)
                ? $"<div align=\"center\"><img alt=\"{attachment.Name}\" src=\"{TeamsMessageUtility.ReplaceAttactmentUrl(channelFilesUrl, attachment.ContentUrl, false)}\"></img></div>"
                : $"<div class=\"file-container\"><a href=\"{TeamsMessageUtility.ReplaceAttactmentUrl(channelFilesUrl, attachment.ContentUrl, false)}\" target=\"_blank\">{attachment.Name}</a></div>";
        }


        private string GenerateHeroCard(Attachment attachment)
        {
            var template = TeamHtmlResources.PostCard_HeroCardTemplate_html;
            try
            {
                var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(attachment.Content);
                var tap = GetValue2<Dictionary<string, object>>(json, "tap");
                var title = GetValue2(json, "title");
                var text = GetValue2(json, "text");
                var images = GetValue2<object[]>(json, "images");
                var subtitle = GetValue2(json, "subtitle");
                var buttons = GetValue2<object[]>(json, "buttons");

                var cardAppType = GetCardAppType(tap);

                return string.Format(template, title, subtitle, GenrateContentImg(images), text, GenrateActionButton(buttons), cardAppType, cardAppIconMapping[cardAppType]);
            }
            catch (Exception ex)
            {
                logger.Warn("Generate HeroCard failed. Content: {0}. Error: {1}.", attachment.Content, ex);
            }
            return template;
        }
        private object GenrateContentImg(object[] images)
        {
            if (images == null || images.Length == 0) return null;

            var imgDic = images.First() as Dictionary<string, object>;
            return $"<img alt=\"{GetValue2(imgDic, "alt")}\" src=\"{GetValue2(imgDic, "url")}\">";
        }

        public static object GetValue2(Dictionary<string, object> json, string key)
        {
            if (json.TryGetValue(key, out object obj))
            {
                return obj;
            }
            return null;
        }

        private string GetCardAppType(Dictionary<string, object> tap)
        {
            if (tap == null) return cardAppPlaces;

            var value = tap["value"].ToString();
            if (value.EndsWith("?ctsrc=TEAMSWEATHER"))
            {
                return cardAppWeather;
            }
            if (value.StartsWith("https://tech.gmw.cn/"))
            {
                return cardAppNews;
            }
            return cardAppUnknown;
        }

        private object GenrateActionButton(object[] buttons)
        {
            if (buttons == null || buttons.Length == 0) return null;

            var buttonDic = buttons.First() as Dictionary<string, object>;
            var btnTitle = buttonDic["title"];
            var btnUrl = buttonDic["value"];
            return $"<button onclick=\"window.open('{btnUrl}')\">{btnTitle}</button>";
        }

        private T GetValue2<T>(Dictionary<string, object> json, string key)
        {
            if (json.TryGetValue(key, out var obj))
            {
                try
                {
                    return (T)obj;
                }
                catch
                {
                    return default(T);
                }
            }
            return default(T);
        }

        private string ReadToEnd(IRestoreStream restoreStream)
        {
            var body = string.Empty;
            using (var stream = restoreStream.OpenContentStream())
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    body = reader.ReadToEnd();
                }
            }
            return body.Contains(ConversationEmptyBody) ? ConversationDeleteBody : body;
        }

        private (string, string) GenerateFileInfo(MetadataEntity baseEntity, string channelName)
        {
            var folderName = channelName;

            var fileName = GenerateFileName(baseEntity, channelName);

            return (folderName, fileName);
        }

        private string GenerateFileName(MetadataEntity baseEntity, string channelName)
        {
            var title = $"{baseEntity.Title}{(char)0x12}{baseEntity.ExchangeId}";
            var itemTime = GlobalCache.ItemCreateTimeInfo.TryGetValue(title, out var ticks) ? new DateTime(ticks) : DateTime.Parse(baseEntity.SendTime.Split()[0]);
            var suffix = 1;
            var basicFileName = $"{channelName}-{itemTime.ToString("MMMM yyyy", CultureInfo.CreateSpecificCulture("en-GB"))}";
            return GenerateRandomName(basicFileName, suffix);
        }

        private string GenerateRandomName(string basicFileName, int suffix)
        {
            var fileName = $"{basicFileName}_{suffix}.html";
            if (GlobalCache.FileNames.Contains(fileName))
            {
                suffix += 1;
                return GenerateRandomName(basicFileName, suffix);
            }
            GlobalCache.FileNames.Add(fileName);
            return fileName;
        }

        private RestoreFileTail HandleTail()
        {
            var tail = new RestoreFileTail()
            {
                FileSize = fileSize,
                HasException = !string.IsNullOrEmpty(errorMessage),
                ErrorMessage = errorMessage
            };

            return tail;
        }

        private ExchangeRestoreData HandleData(GroupBasicIndex index, ConversationDiscoverNode conversation)
        {
            ExchangeRestoreData restoreData = new ExchangeRestoreData();
            try
            {
                restoreData.RestoreStream = new RestoreStream(GenerateReader(index, conversation), "");
                HandleMetadata(restoreData);
                this.fileSize = restoreData.RestoreStream.Size;
                restoredItemId.Add(index.Id);
            }
            catch (SkipRetryException ex) when (ex.Message.Contains("Response status code does not indicate success: 409 (This operation is not permitted on an archived blob.)"))
            {
                logger.Error($"Error occurred while HandleData index. Blob is currently in Archived status: {ex}");
                throw new BlobArchivedException(ex.Message, "");
            }
            catch (BlobArchivedException e)
            {
                logger.Error($"there is archived tier data can not restore,will retry,error:{e.ToString()}");
                throw;
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                logger.Error("An error occurred while to handle data. Reason: {0}. ", e.ToString());
            }
            return restoreData;
        }

        private IItemDataReader GenerateReader(GroupBasicIndex index, ConversationDiscoverNode conversation)
        {
            DataEncryptionInfoWrapper wrapper = DataEncryptionInfoManager.ResolveDynamicKey(conversation.DataEncryptionInfo);
            var context = new DataContextBase
            {
                ContentDataPosition = new DataPosition
                {
                    StartFileNumber = index.CurrentItemContentDataStartFileNumber,
                    StartOffset = index.CurrentItemContentDataStartOffset,
                    PrefixNumber = index.CurrentItemContentDataFilePrefixNumber,
                    ContentLength = index.CurrentItemContentDataTotalLength,
                    FileType = MediaDataIO.FileType.Content,
                    ItemPageSize = index.CurrentItemPageSize
                },
                MetaDataPosition = new DataPosition
                {
                    StartFileNumber = index.CurrentItemMetaDataStartFileNumber,
                    StartOffset = index.CurrentItemMetaDataStartOffset,
                    PrefixNumber = index.CurrentItemMetaDataFilePrefixNumber,
                    ContentLength = index.CurrentItemMetaDataAndContentDataTotalLength - index.CurrentItemContentDataTotalLength,
                    FileType = MediaDataIO.FileType.MetaData
                },
                DataPathGenerator = new TeamsMediaDataPathGenerator(DataModule.TeamsPlatform, index.BackupJobId, GroupAddress, false, tempPath),
                EncryptionKey = AvePoint.GCommon.Utility.Cryptography.CspCommunicationWrapper.UnWrapKey(wrapper.DynamicKey),
                ItemDataMode = (byte)index.CurrentItemDataMode
            };

            return new ItemDataReader(context, GlobalDeviceCache.GetDeviceById(conversation.StorageId));
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

        private ExchangeFileHeader HandleHeader(GroupBasicIndex index)
        {
            index.OpenType = StreamOpenType.Default;
            index.IsRestoreToFS = false;
            var fileHeader = new ExchangeFileHeader()
            {
                DataType = (ExchangeDataType)index.Type,
                Name = index.Name,
                NodeType = index.NodeType,
            };
            var tempPath = index.Path.Contains(ServiceConstants.Delimiter) ?
                index.Path.Substring(0, index.Path.LastIndexOf(ServiceConstants.Delimiter)) : index.Path;
            if (index.Type == (Int32)ExchangeDataType.Item)
            {
                fileHeader.ParentFullPath = tempPath.Contains(ServiceConstants.Delimiter) ?
                   tempPath.Substring(0, tempPath.LastIndexOf(ServiceConstants.Delimiter)) : tempPath;
            }
            else
                fileHeader.ParentFullPath = tempPath;
            return fileHeader;
        }

        private string ExportContainerNode(TeamsDiscoveryNode node)
        {
            Reporter.ConfigForReport(ExportUtility.BuildTargetUrl(node.ExportPath), ExportUtility.IsNeedUploadAndDeleteCache());
            Reporter.RecordSuccessful(node);
            return node.ExportPath;
        }

        public string BuildExportPath(string exportLocation, string name, string sitePath, NodeType level)
        {
            return Path.Combine(exportLocation, name);
        }

        public void Dispose()
        {

        }
    }
}
