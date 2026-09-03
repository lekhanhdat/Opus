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
namespace Microsoft365Backup.DataBuilder.TeamHtml
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    public static class TeamHtmlResources
    {
        /// <summary>
        /// 如何添加一个资源模板文件
        /// 1. 在本工程中添加一个html文件，作为模板，build action选择Embedded Resource
        /// 2. 在本类中添加一个public static field, field名字为html文件名(将.替换成_), 大小写敏感
        /// </summary>
        public static string ConversationTopicTemplate_html = null;
        public static string ConversationReplyTemplate_html = null;
        //public static string CssStyle_css = null;
        public static string ConversationHeaderTemplate_html = null;
        public static string WebPartRedBangTemplate_html = null;
        public static string WebPartSweetHeartTemplate_html = null;
        #region post Card
        public static string PostCard_HeroCardTemplate_html = null;
        public static string PostCard_AdaptiveCardTemplate_html = null;
        #endregion
        public static string MessageHeadingTemplate_html = null;
        public static string FileWithoutIconTemplate_html = null;
        public static string AttachmentUrlsTemplate_html = null;
        public static string CodeSnippetTemplate_html = null;
        public static string CodeSnippetBodyTemplate_html = null;
        public static string MeetingAsHtmlTemplate_html = null;
        public static string MeetingAsPostTemplate_html = null;
        public static string MessageRefrence_html = null;
        public static string MergeTopicTemplate_html = null;
        public static string MergeReplyTemplate_html = null;
        public static string ColorThemeAnnouncementBannerTemplate_html = null;
        public static string UploadedImageAnnouncementBannerTemplate_html = null;
        public static Dictionary<string, Dictionary<string, string>> ChatEmotions = null;
        public static string ConversationReactionTemplate_html = null;

        private const string Important = "Important!";

        static TeamHtmlResources()
        {
            ConversationTopicTemplate_html = ReadFromResourceFile(nameof(ConversationTopicTemplate_html));
            ConversationReplyTemplate_html = ReadFromResourceFile(nameof(ConversationReplyTemplate_html));
            ConversationHeaderTemplate_html = ReadFromResourceFile(nameof(ConversationHeaderTemplate_html));
            WebPartRedBangTemplate_html = ReadFromResourceFile(nameof(WebPartRedBangTemplate_html));
            WebPartSweetHeartTemplate_html = ReadFromResourceFile(nameof(WebPartSweetHeartTemplate_html));
            PostCard_HeroCardTemplate_html = ReadFromResourceFile(nameof(PostCard_HeroCardTemplate_html));
            PostCard_AdaptiveCardTemplate_html = ReadFromResourceFile(nameof(PostCard_AdaptiveCardTemplate_html));
            MessageHeadingTemplate_html = ReadFromResourceFile(nameof(MessageHeadingTemplate_html));
            FileWithoutIconTemplate_html = ReadFromResourceFile(nameof(FileWithoutIconTemplate_html));
            AttachmentUrlsTemplate_html = ReadFromResourceFile(nameof(AttachmentUrlsTemplate_html));
            CodeSnippetTemplate_html = ReadFromResourceFile(nameof(CodeSnippetTemplate_html));
            CodeSnippetBodyTemplate_html = ReadFromResourceFile(nameof(CodeSnippetBodyTemplate_html));
            MeetingAsHtmlTemplate_html = ReadFromResourceFile(nameof(MeetingAsHtmlTemplate_html));
            MeetingAsPostTemplate_html = ReadFromResourceFile(nameof(MeetingAsPostTemplate_html));
            MessageRefrence_html = ReadFromResourceFile(nameof(MessageRefrence_html));
            MergeTopicTemplate_html = ReadFromResourceFile(nameof(MergeTopicTemplate_html));
            MergeReplyTemplate_html = ReadFromResourceFile(nameof(MergeReplyTemplate_html));
            ColorThemeAnnouncementBannerTemplate_html = ReadFromResourceFile(nameof(ColorThemeAnnouncementBannerTemplate_html));
            UploadedImageAnnouncementBannerTemplate_html = ReadFromResourceFile(nameof(UploadedImageAnnouncementBannerTemplate_html));
            ChatEmotions = ConvertToEmojiDictionary(ReadFromResourceFile("emotions_json"));
            ConversationReactionTemplate_html = ReadFromResourceFile(nameof(ConversationReactionTemplate_html));
        }

        public static string ReadFromResourceFile(string name)
        {
            var fileExtension = name.Split('_')[1];
            name = $"M365.Wrapper.Common.TeamHtml.Template.{name.Replace('_', '.')}";
            var result = default(string);
            using (var sourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
            {
                switch (fileExtension)
                {
                    case "png":
                        using (var binaryReader = new BinaryReader(sourceStream))
                        {
                            var imageBytes = binaryReader.ReadBytes((int)sourceStream.Length);
                            result = Convert.ToBase64String(imageBytes);
                        }
                        break;
                    case "json":
                    case "html":
                    default:
                        using (var streamReader = new StreamReader(sourceStream))
                        {
                            result = streamReader.ReadToEnd();
                        }
                        break;
                }
            }
            return result;
        }

        public static string AssemblyTopicHtml(ConversationTopic topic)
        {
            //performance issue if body is large.
            return string.Format(ConversationTopicTemplate_html,
                topic.PostedBy,                                                     //{0}
                topic.PostedTime,                                                   //{1}
                topic.PostedTime,                                                   //{2}
                topic.Important ? Important : string.Empty,                         //{3}
                topic.Subject ?? string.Empty,                                      //{4}
                topic.Body,                                                         //{5}
                topic.Important ? WebPartRedBangTemplate_html : string.Empty,       //{6}
                topic.Announcement ?? string.Empty,                                 //{7}
                topic.Reaction ?? string.Empty);                                    //{8}
        }

        public static string AssemblyReplyHtml(ConversationReply reply)
        {
            //performance issue if body is large.
            return string.Format(ConversationReplyTemplate_html,
                reply.PostedBy,                                                     //{0}
                reply.PostedTime,                                                   //{1}
                reply.PostedTime,                                                   //{2}
                reply.Important ? Important : string.Empty,                         //{3}
                reply.Body,                                                         //{4}
                reply.Important ? WebPartRedBangTemplate_html : string.Empty,       //{5}
                string.Empty,                                                       //{6} 头像img src
                reply.Reaction ?? string.Empty);                                    //{7}                                      
        }

        private static Dictionary<string, Dictionary<string, string>> ConvertToEmojiDictionary(string content)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(content);
        }
        
        private static bool IsDefaultEmoji(string emoji)
        {
            string[] defaultEmojis = {"like", "heart", "laugh", "surprised"};
            return defaultEmojis.Contains(emoji);
        }
        private static string GetDefaultEmojiId(string emoji)
        {
            return emoji.Equals("like", StringComparison.OrdinalIgnoreCase) ? "yes" : emoji;
        }
        public static string GetEmojiHtml(string emoji, string displayName)
        {
            string id = string.Empty;
            if (IsDefaultEmoji(emoji))
            {
                id = GetDefaultEmojiId(emoji);
            } 
            else
            {
                TeamHtmlResources.ChatEmotions.TryGetValue(emoji, out Dictionary<string, string> keyValuePairs);
                if(keyValuePairs != null)
                {
                    if(displayName != null)
                    {
                        keyValuePairs.TryGetValue(string.Join("_", displayName.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries)), out id);
                    }
                    id = string.IsNullOrEmpty(id) ? keyValuePairs.First().Value : id;
                }
            }
            return string.IsNullOrEmpty(id) ? emoji : GetEmojiSource(id, emoji);
        }

        public static string GetEmojiSource(string emojiId, string emoji)
        {
            return $"<img alt=\"{emoji}\" src=\"https://statics.teams.cdn.office.net/evergreen-assets/personal-expressions/v2/assets/emoticons/{emojiId}/default/20_f.png\" width=\"20\"/>";
        }
    }

    static class TeamHtmlConstants
    {
        public const string HtmlStart = "<html>";
        public const string HtmlEnd = "</html>";
        public const string BodyStart = "<body onload=\"init()\">";
        public const string BodyEnd = "</body>";
        public const string DivStart = "<div>";
        public const string DivEnd = "</div>";
        public const string ConversationStart = "<div class=\"conversation\">";
        public const string ConversationEnd = "</div>";
    }
}