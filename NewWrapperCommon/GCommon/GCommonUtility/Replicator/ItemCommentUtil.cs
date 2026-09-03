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
using System.Text;
using System.Xml.Serialization;

namespace AvePoint.GCommon.Utility
{
    /// <summary>
    /// Job Detail的comment 工具类,
    /// 兼容6.3之前的普通comment
    /// </summary>
    [XmlRoot("Root")]
    public class DetailCommentUtil
    {
        static AveLogger logger = AveLogger.GetInstance(typeof(DetailCommentUtil));
        /// <summary>
        /// 构造函数
        /// </summary>
        public DetailCommentUtil()
        {
            CommentElements = new List<string>();
        }
        
        const int ForwardCompatibleType = -1;

        /// <summary>
        /// Comment 的组成元素，用于String.Format的不定参数，
        /// 请确保元素顺序
        /// </summary>
        [XmlArray("CommentElements")]
        [XmlArrayItem("Element")]
        public List<string> CommentElements { get; set; }

        /// <summary>
        /// Comment 类型，各模块可以根据自己的需要进行 Int枚举定义。
        /// 注意：-1为向前兼容预留值，不要在枚举中定义
        /// </summary>
        [XmlElement("CommentType")]
        public int CommentType { get; set; }

        /// <summary>
        /// 根据各模块自定义的国际化字典来获取comment
        /// </summary>
        /// <param name="i18NDic">根据CommentType定义的国际化字典</param>
        /// <returns></returns>
        public string GetI18NComment(Dictionary<int, string> i18NDic)
        {
            var comment = string.Empty;
            if (this.CommentType == ForwardCompatibleType)
            {
                return CommentElements[0];
            }

            switch (CommentElements.Count())
            {
                case 0: comment = i18NDic[CommentType];
                    break;
                case 1: comment = string.Format(i18NDic[CommentType], CommentElements[0]);
                    break;
                case 2: comment = string.Format(i18NDic[CommentType], CommentElements[0], CommentElements[1]);
                    break;
                case 3: comment = string.Format(i18NDic[CommentType], CommentElements[0], CommentElements[1], CommentElements[2]);
                    break;
                case 4: comment = string.Format(i18NDic[CommentType], CommentElements[0], CommentElements[1], CommentElements[2], CommentElements[3]);
                    break;
            }

            return comment;
        }

        /// <summary>
        /// Server 端的反序列化方法，兼容旧格式的普通comment
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static DetailCommentUtil Deserialize(string str)
        {
            DetailCommentUtil item = null;
            try
            {
                item = SerializerHelper.DeserializeFromXmlString<DetailCommentUtil>(str);
            }
            catch(Exception ex)
            {
                logger.Warn("DetailComment Deserialize Failed. CommentString:{0}, exception:{1}", str, ex.ToString());
                item = new DetailCommentUtil() { CommentElements = new List<string> { str }, CommentType = ForwardCompatibleType };
            }
            return item;
        }

        /// <summary>
        /// Agent 端使用, 将对象序列化成comment xml字符串
        /// </summary>
        /// <returns></returns>
        public string Serialize()
        {
            if (this.CommentElements == null) throw new ArgumentNullException("CommentElements");

            return SerializerHelper.SerializeToXmlString(this);
        }


    }
}
