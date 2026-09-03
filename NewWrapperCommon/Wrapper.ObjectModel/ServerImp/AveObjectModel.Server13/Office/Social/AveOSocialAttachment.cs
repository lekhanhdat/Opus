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
    class AveOSocialAttachment : IAveOSocialAttachment
    {
        private SPSocialAttachment mSocialAttachment;

        public AveOSocialAttachment()
        {
            mSocialAttachment = new SPSocialAttachment();
        }

        public AveOSocialAttachment(SPSocialAttachment socialAttachemnt)
        {
            mSocialAttachment = socialAttachemnt;
        }

        internal SPSocialAttachment SocialAttachment
        {
            get
            {
                return mSocialAttachment;
            }
        }

        public AveOSocialAttachmentKind AttachmentKind
        {
            get
            {
                return (AveOSocialAttachmentKind)mSocialAttachment.AttachmentKind;
            }
            set
            {
                mSocialAttachment.AttachmentKind = (SPSocialAttachmentKind)value;
            }
        }

        public string Name
        {
            get
            {
                return mSocialAttachment.Name;
            }
            set
            {
                mSocialAttachment.Name = value;
            }
        }

        public Uri Uri
        {
            get
            {
                return mSocialAttachment.Uri;
            }
            set
            {
                mSocialAttachment.Uri = value;
            }
        }

        public string Description
        {
            get
            {
                return mSocialAttachment.Description;
            }
            set
            {
                mSocialAttachment.Description = value;
            }
        }
    }
}
