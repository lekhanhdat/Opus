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


namespace AvePoint.Media.Storage.Centera
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon;
    #endregion

    #region CodeReview
    [AveCodeReview(
       "2012/8/9",
       "rongbiao.sun@avepoint.com",
       "dapeng.zhang@avepoint.com",
        new String[] { CodeReviewConstants.CHECK_LIST_ID_LOG_1 },
        null,
        true)]
    #endregion
    class CenteraClient : IDisposable
    {
        private FPClip currentClip;
        private FPPool currentPool;
        private AveLogger logger = new AveLogger(typeof(CenteraClient));
        private Dictionary<String, FPClip> openedClips = new Dictionary<String, FPClip>();
        public UInt64 RetentionDays { get; set; }

        public CenteraClient(FPPool pool, UInt64 customRetentionDays = 0)
        {
            this.currentPool = pool;
            this.RetentionDays = customRetentionDays;
        }

        public FPClip OpenClip(String clipId)
        {
            return currentPool.OpenClip(clipId);
        }

        /// <summary>
        /// Readonly operation, Get Tag.
        /// </summary>
        /// <param name="clipId">ClipId</param>
        /// <param name="tagName">StorageInfo.LowName</param>
        /// <returns></returns>
        public FPTag FetchTag(String clipId, String tagName)
        {
            var tag = default(FPTag);
            String preClipId = clipId;
            while (!String.IsNullOrEmpty(preClipId))
            {
                FPClip clip = null;
                if (openedClips.ContainsKey(preClipId))
                {
                    clip = openedClips[preClipId];
                }
                else
                {
                    clip = currentPool.OpenClip(preClipId);
                    openedClips[preClipId] = clip;
                }
                tag = clip.OpenTag(CheckName(tagName));
                if (tag != null)
                {
                    break;
                }
                try
                {
                    preClipId = clip.GetClipMeta(CenteraConst.PREVIOUS_CLIP_ID);
                }
                catch (Exception e)
                {
                    preClipId = String.Empty;
                    logger.Warn(e.ToString());
                }
            }
            return tag;
        }

        public String CheckName(String name)
        {
            String tagName = Regex.Replace(name, "[^[\\w0-9-.]]*", "");
            Char b = tagName.ToCharArray()[0];
            if (b > 47 && b < 58)
            {
                tagName = "INVALID" + tagName;
            }
            return tagName;
        }

        public FPClip CreateClip(String clipName, CASLevel casLevel = CASLevel.MulitiBlobs)
        {
            switch (casLevel)
            {
                case CASLevel.MulitiBlobs:
                    String lastClipId = null;
                    if (currentClip != null)
                    {
                        if (currentClip.Writed)
                        {
                            lastClipId = currentClip.ClipId;
                            currentClip.Close();
                            currentClip = null;
                        }
                        else
                            return currentClip;
                    }

                    currentClip = currentPool.CreateClip(clipName);

                    if (!string.IsNullOrEmpty(lastClipId))
                    {
                        if (!this.currentClip.UpdateClipMeta(FPClip.PREVIOUS_CLIP_ID, lastClipId))
                        {
                            throw new Exception("update meta info failed");
                        }
                    }
                    return currentClip;
                case CASLevel.SingleBlob:
                    return currentPool.CreateClip(clipName);
                default:
                    break;
            }
            return null;
        }

        public void Dispose()
        {
            foreach (FPClip clip in openedClips.Values)
            {
                clip.Dispose();
            }
            openedClips.Clear();
            if (currentClip != null)
            {
                currentClip.Close();
                currentClip = null;
            }
        }
    }
}
