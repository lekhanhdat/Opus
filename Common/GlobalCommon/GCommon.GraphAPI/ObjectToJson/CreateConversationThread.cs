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

namespace AvePoint.GCommon.GraphAPI
{
    using Newtonsoft.Json;

    public class CreateConversationThreadObj
    {
        public CreateConversationThreadObj(string topic, string contentType, string content)
        {
            Topic = topic;
            Posts = new CTPost[]
            {
                 new CTPost()
                 {
                     Body =  new CTPostBody() {  ContentType = contentType, Content = content }
                 }
            };
        }
        [JsonProperty("topic")]
        public string Topic { get; set; }

        [JsonProperty("posts")]
        public CTPost[] Posts { get; set; }
    }
    public class AddPlannerTaskCommentObj
    {
        public AddPlannerTaskCommentObj(string BodyType, string BodyContent)
        {
            Post = new CTPost()
            {
                Body = new CTPostBody()
                {
                    ContentType = BodyType,
                    Content = BodyContent
                }
            };
        }
        [JsonProperty("post")]
        public CTPost Post { get; set; }
    }

    #region CTPost
    public class CTPost
    {
        [JsonProperty("body")]
        public CTPostBody Body { get; set; }
    }
    public class CTPostBody
    {
        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }
    #endregion
}