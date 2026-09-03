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

namespace AvePoint.Wrapper.Restore.NintexForm
{
    class NintexFormServiceOnline:NintexFormServiceBase
    {
        internal NintexFormServiceOnline(IAveList aveList, IAveSPWeb aveSPWeb, bool isPost)
            :base(aveList, aveSPWeb,isPost)
        {
            contentProcessor=new NintexFormContentProcessorOnline(aveSPWeb,aveList);
        }

        public override void RestoreForm(AveNintexFormInfo nintexFormInfo, string contentTypeId)
        {
            var finalFormXml = contentProcessor.ReplaceFormContent(nintexFormInfo.FormXml, contentTypeId, isPost);
            PublishNintexForm(finalFormXml, contentTypeId);
        }

        protected override void PublishNintexForm(string newNintexFormXml, string contentTypeId)
        {
            EnsureNintexFormAppExist();
            mAveList.SaveNintexForm(newNintexFormXml, contentTypeId);
            mAveList.PublishNintexForm(contentTypeId);
        }
        private void EnsureNintexFormAppExist()
        {
            Guid NintexFormsAppProductId = new Guid("353e0dc9-57f5-40da-ae3f-380cd5385ab9");
            var nintexWorkflowAppInstance = mAveList.ParentWeb.GetAppInstancesByProductId(NintexFormsAppProductId);
            if (nintexWorkflowAppInstance.Count == 0)
            {
                mAveList.ParentWeb.AppSerializer.SetObjectData(new AveAppPackageInfo { ProductId = NintexFormsAppProductId });
            }
        }
        public override void DeleteForm(string listId, string contentTypeId)
        {
            // 由于365目前没有实现nintex form的version转移，所以不涉及到目的端删除历史version，所以这个方法目前没有意义，暂时不做任何事情。
        }
    }
}
