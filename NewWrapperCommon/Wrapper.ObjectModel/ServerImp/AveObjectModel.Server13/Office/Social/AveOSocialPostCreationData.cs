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
using AvePoint.Wrapper.Common;
using Microsoft.Office.Server.SocialData;
using AvePoint.Wrapper.Common.Office;
using Microsoft.Office.Server.Social;

namespace AvePoint.ObjectModel.Server13.Office
{
    class AveOSocialPostCreationData : IAveOSocialPostCreationData
    {
        private SPSocialPostCreationData mSocialPostCreationData;
        private AveOSocialAttachment mSocialAttachment;

        public AveOSocialPostCreationData()
        {
            mSocialPostCreationData = new SPSocialPostCreationData();
        }

        public AveOSocialPostCreationData(SPSocialPostCreationData socialPostCreationData)
        {
            mSocialPostCreationData = socialPostCreationData;
        }

        internal SPSocialPostCreationData SocialPostCreationData
        {
            get
            {
                return mSocialPostCreationData;
            }
        }

        public IAveOSocialAttachment Attachment
        {
            get
            {
                if (mSocialAttachment == null)
                {
                    SPSocialAttachment socialAttachment = mSocialPostCreationData.Attachment;
                    if (socialAttachment != null)
                    {
                        mSocialAttachment = new AveOSocialAttachment(socialAttachment);
                    }
                }
                return mSocialAttachment;
            }
            set
            {
                mSocialAttachment = value as AveOSocialAttachment;
                if (mSocialPostCreationData != null)
                {
                    mSocialPostCreationData.Attachment = mSocialAttachment.SocialAttachment;
                }
                else
                {
                    mSocialPostCreationData.Attachment = null;
                }
            }
        }

        public bool UpdateStatusText
        {
            get
            {
                return mSocialPostCreationData.UpdateStatusText;
            }
            set
            {
                mSocialPostCreationData.UpdateStatusText = value;
            }
        }

        public string ContentText
        {
            get
            {
                return mSocialPostCreationData.ContentText;
            }
            set
            {
                mSocialPostCreationData.ContentText = value;
            }
        }

        public IAveOSocialDataItem[] ContentItems
        {
            get
            {
                SPSocialDataItem[] dataItem = mSocialPostCreationData.ContentItems;
                if (dataItem != null)
                {
                    int length = dataItem.Length;
                    AveOSocialDataItem[] mSocicalDataItem = new AveOSocialDataItem[length];
                    for(int i=0;i<length;i++)
                    {
                        if (dataItem[i] != null)
                        {
                            mSocicalDataItem[i] = AveServerAssemblyInit.CreateElement(typeof(IAveOSocialDataItem), new object[] { dataItem[i] }) as AveOSocialDataItem;
                        }
                        else
                        {
                            mSocicalDataItem[i] = null;
                        }
                    }
                    return mSocicalDataItem;
                }

                return null;
            }
            set
            {
                int length = value.Length;
                mSocialPostCreationData.ContentItems = new SPSocialDataItem[length];
                for (int i = 0; i < length; i++)
                {
                    if (value[i] != null)
                    {
                        mSocialPostCreationData.ContentItems[i] = ((AveOSocialDataItem)value[i]).DataItem;
                    }
                    else
                    {
                        mSocialPostCreationData.ContentItems[i] = null;
                    }
                }
            }
        }

        //public IAveOSocialPostDefinitionData DefinitionData
        //{
        //    get
        //    {
        //        if (mSocialPostDefinitionData == null)
        //        {
        //            SPSocialPostDefinitionData data = mSocialPostCreationData.DefinitionData;
        //            if (data != null)
        //            {
        //                mSocialPostDefinitionData = new AveOSocialPostDefinitionData(data);
        //            }
        //        }
        //        return mSocialPostDefinitionData;
        //    }
        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public Uri[] SecurityUris
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }
        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public IAveOSocialLink Source
        //{
        //    get
        //    {
        //        if (mSocialLink == null)
        //        {
        //            SPSocialLink link = mSocialPostCreationData.Source;
        //            if (link != null)
        //            {
        //                mSocialLink = new AveOSocialLink(link);
        //            }
        //        }
        //        return mSocialLink;
        //    }
        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}
    }
}
