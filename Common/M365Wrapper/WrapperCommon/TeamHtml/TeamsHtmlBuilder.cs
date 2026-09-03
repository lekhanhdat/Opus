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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft365Backup.DataBuilder.TeamHtml
{
    /// <summary>
    /// Build a html or aspx file which contains readonly content of conversations
    /// </summary>
    public class TeamsHtmlBuilder : IDisposable
    {
        private string file;
        private Stream stream;
        private StreamWriter writer;
        private bool conversationEndTagRequired = false;
        private bool needDisposeStream = false;

        /// <summary>
        /// 构造Conversation, 将生成的html文件存储在file路径下。
        /// </summary>
        /// <param name="file">输出路径</param>
        public TeamsHtmlBuilder(string file)
        {
            this.file = file;
            this.stream = new FileStream(file, FileMode.CreateNew);
            this.writer = new StreamWriter(stream);
            this.needDisposeStream = true;
            WriteStartElement();
        }

        /// <summary>
        /// 构造Conversation, 将生成的html文件存储在stream流中。
        /// </summary>
        /// <param name="file"></param>
        public TeamsHtmlBuilder(Stream stream)
        {
            if (!stream.CanWrite) throw new Exception();
            this.stream = stream;
            this.writer = new StreamWriter(stream);
            WriteStartElement();
        }

        private void WriteStartElement()
        {
            this.writer.Write(TeamHtmlConstants.HtmlStart);
            this.writer.Write(TeamHtmlResources.ConversationHeaderTemplate_html);
            this.writer.Write(TeamHtmlConstants.BodyStart);
            this.writer.Write(TeamHtmlConstants.DivStart);
        }

        public void AppendOne(ConversationItem item, Dictionary<string, string> siteUrlMap = null)
        {
            if (item is ConversationTopic)
            {
                WriteConversationEndTag();
                WriteConversationStartTag();
            }
            FormatItemBody(item);
            if (null != siteUrlMap)
            {
                foreach (var map in siteUrlMap)
                {
                    item.Body = item.Body.Replace(map.Key, map.Value);
                }
            }
            this.writer.Write(item.ToHtmlString());
        }
        /// <summary>
        /// 使用graph api 备份的数据，其附件需要提前处理。
        /// </summary>
        /// <param name="item"></param>
        public void AppendOneV2(ConversationItem item)
        {
            if (item is ConversationTopic)
            {
                WriteConversationEndTag();
                WriteConversationStartTag();
            }

            this.writer.Write(item.ToHtmlString());
        }
        private void FormatItemBody(ConversationItem item)
        {
            item.Body = new TeamHtmlFormatter(item).Process();
        }

        private void WriteConversationStartTag()
        {
            this.writer.Write(TeamHtmlConstants.ConversationStart);
            this.conversationEndTagRequired = true;
        }

        private void WriteConversationEndTag()
        {
            if (this.conversationEndTagRequired)
            {
                this.writer.Write(TeamHtmlConstants.ConversationEnd);
                this.conversationEndTagRequired = false;
            }
        }

        public void Dispose()
        {
            if (this.writer != null)
            {
                WriteConversationEndTag();
                this.writer.Write(TeamHtmlConstants.DivEnd);
                this.writer.Write(TeamHtmlConstants.BodyEnd);
                this.writer.Write(TeamHtmlConstants.HtmlEnd);
                this.writer.Flush();
                if (needDisposeStream)
                {
                    this.writer.Dispose();
                    this.writer = null;
                }
            }
            if (this.stream != null)
            {
                if (needDisposeStream)
                {
                    this.stream.Dispose();
                    this.stream = null;
                }
            }
        }
    }

}