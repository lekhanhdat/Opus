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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public static class TeamUtil
    {
        public const string GeneralChannelName = "General";
        private const char SplitChar = '/';
        private const string PATTERN_ENDPOINT_GRAPH = "https://graph.microsoft";

        public const string AttachmentReferenceType = "reference";
        public const string AttachmentCardHeroType = "application/vnd.microsoft.card.hero";
        public const string AttachmentCardThumbnailTye = "application/vnd.microsoft.card.thumbnail";
        public const string AttachmentAnnouncementBannerType = "application/vnd.microsoft.teams.messaging-announcementBanner";
        public const string AttachmentMeetingType = "meetingReference";
        public const string AttachmentCardCodeSnippetType = "application/vnd.microsoft.card.codesnippet";
        public const string AttachmentCardAdaptiveType = "application/vnd.microsoft.card.adaptive";
        public const string AttachmentTabType = "tabReference";
        public const string AttachmentMessageReference = "messageReference";

        public const string HostedContentSource = "../hostedContents/{0}/$value";
        public const string HostedContentImageContentType = "image/png";

        public static Dictionary<string, (string, string)> ColorThemeAnnouncementBannerColor = new Dictionary<string, (string, string)>()
        {
            {"periwinkleBlue", ("#a9d3f2", "#004377")},
            {"lavender", ("#d2ccf8", "#3f3682")},
            {"rose", ("#f7c0e3", "#80215d")},
            {"orangeSherbet", ("#f4bfab", "#7a2101")},
            {"flaxYellow", ("#ecdfa5", "#6c5700")},
            {"teal", ("#bdd99b", "#294903")},
            {"navyBlue", ("#004377", "#a9d3f2")},
            {"indigo", ("#3f3682", "#d2ccf8")},
            {"mulberryRed", ("#80215d", "#f7c0e3")},
            {"fireOrange", ("#7a2101", "#f4bfab")},
            {"rawUmber", ("#6c5700", "#ecdfa5")},
            {"oceanGreen", ("#294903", "#bdd99b")}
        };

        /// <summary>
        /// format:
        ///                                             | In "General" channel  | Has subject
        /// TeamName/TopicId                            |       Y               |     N
        /// TeamName/TopicId/Subject                    |       Y               |     Y
        /// TeamName/ChannelName/TopicId                |       N               |     N
        /// TeamName/ChannelName/TopicId/Subject        |       N               |     Y
        /// </summary>
        public static TeamConversationInfo ToConversationInfo(this string mailSubject)
        {
            if (string.IsNullOrEmpty(mailSubject)) throw new ArgumentNullException(nameof(mailSubject));

            string[] words = mailSubject.Split(SplitChar);
            switch (words.Length)
            {
                case 2://TeamName/TopicId 
                    return new TeamConversationInfo()
                    {
                        TeamName = words[0],
                        ChannelName = GeneralChannelName,
                        TopicId = words[1],
                        Subject = null,
                    };
                case 3:
                    //TeamName/TopicId/Subject 
                    if (Regex.IsMatch(words[1], @"[\d]{13}"))
                    {
                        return new TeamConversationInfo()
                        {
                            TeamName = words[0],
                            ChannelName = GeneralChannelName,
                            TopicId = words[1],
                            Subject = words[2],
                        };
                    }
                    //TeamName/ChannelName/TopicId  
                    else
                    {
                        return new TeamConversationInfo()
                        {
                            TeamName = words[0],
                            ChannelName = words[1],
                            TopicId = words[2],
                            Subject = null,
                        };
                    }
                case 4://TeamName/ChannelName/TopicId/Subject 
                    return new TeamConversationInfo()
                    {
                        TeamName = words[0],
                        ChannelName = words[1],
                        TopicId = words[2],
                        Subject = words[3],
                    };
                default:
                    throw new ArgumentException($"mail subject is not a valid conversation title format, {mailSubject}");
            }
        }

        public static string ExtractChannelName(this string mailSubject)
        {
            return TeamUtil.ToConversationInfo(mailSubject).ChannelName;
        }

        public static string ExtractSubject(this string mailSubject)
        {
            return TeamUtil.ToConversationInfo(mailSubject).Subject;
        }

        public static Dictionary<string, string> DecodeHostedContentUrl(string url)
        {
            var id = GetHostedContentId(url);
            return DecodeHostedContentId(id);
        }

        public static string GetHostedContentId(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;
            //https://graph.microsoft.com/beta/teams/ad5f439c-a2d3-4060-a49a-259aa642adbb/channels/19:a8bf590c3acd473095b950777f953924@thread.skype/messages/1587086921204/hostedContents/aWQ9eF8wLWVhLWQ4LTlmNDdkNjcxMmNkZmUzYTVjNmNjOGU3MmVkNjhmNjY2LHR5cGU9MSx1cmw9aHR0cHM6Ly9hcy1hcGkuYXNtLnNreXBlLmNvbS92MS9vYmplY3RzLzAtZWEtZDgtOWY0N2Q2NzEyY2RmZTNhNWM2Y2M4ZTcyZWQ2OGY2NjYvdmlld3MvaW1nbw==/$value
            var match = Regex.Match(url, $@"^{PATTERN_ENDPOINT_GRAPH}\S+/hostedContents/(?<id>\S+)/\$value");
            return match.Success ? match.Groups["id"].Value : null;
        }
        public static string GetHostedContentIdFromRelativeSource(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;
            var match = Regex.Match(url, $@"\.+\/hostedContents/(?<id>\S+)/\$value");
            return match.Success ? match.Groups["id"].Value : null;
        }

        public static Dictionary<string,string> DecodeHostedContentId(string hostedContentId)
        {
            if (string.IsNullOrEmpty(hostedContentId)) return new Dictionary<string, string>();
            //id=x_0-ea-d3-384eb6ed2ba5776295399c9dbc87436a,type=1,url=https://as-api.asm.skype.com/v1/objects/0-ea-d3-384eb6ed2ba5776295399c9dbc87436a/views/imgo
            var decodeStr = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(hostedContentId));
            return Regex.Matches(decodeStr, @",*(?<key>[^=]+)=(?<value>[^,]*)").Cast<Match>().
                ToDictionary(m => m.Groups["key"].Value, m => m.Groups["value"].Value, StringComparer.OrdinalIgnoreCase); 
        }
    }
   
    public class TeamConversationInfo
    {
        public string ChannelName { get; set; }
        public string Subject { get; set; }
        public string TeamName { get; set; }
        public string TopicId { get; set; }
    }
}