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
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;

namespace AvePoint.Wrapper.Restore.NintexForm
{
    class NintexFormService
    {
       // private static AveLogger log = AveLogger.GetInstance(typeof(NintexFormService));
        private IAveList mAveList;
        private bool isPost;
        internal NintexFormContentProcessor contentProcessor;
        internal NintexFormService(IAveList aveList, AveSPWeb aveSPWeb, bool isPost)
        {
            mAveList = aveList;
            this.isPost = isPost;
            contentProcessor = new NintexFormContentProcessor(aveSPWeb, aveList);
        }

        public void RestoreForm(string nintexFormXml, string contentTypeId)
        {
            var finalFormXml = contentProcessor.ReplaceFormContent(nintexFormXml, contentTypeId, isPost);
            PublishNintexForm(finalFormXml, contentTypeId);
        }

        protected void PublishNintexForm(string newNintexFormXml, string contentTypeId)
        {
            mAveList.SaveNintexForm(newNintexFormXml, contentTypeId);
            mAveList.PublishNintexForm(contentTypeId);
        }
    }
}
