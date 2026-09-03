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
using AvePoint.RA.DB.Model;

namespace RAGoogle.Models.Contract;

public class GoogleLabelInfoSubscription : ISubscription
{
    private readonly RMGoogleLabelInfo _googleLabelInfo = new();

    private readonly GoogleLabelApi _googleLabelApi;

    public GoogleLabelInfoSubscription(GoogleLabelApi googleLabel)
    {
        _googleLabelApi = googleLabel;
        googleLabel.RegisterSubscription(this);
    }
    public void Update()
    {
        _googleLabelInfo.UniqueId = _googleLabelApi.UniqueId;
        _googleLabelInfo.LabelId = _googleLabelApi.LabelId;
        _googleLabelInfo.LabelName = _googleLabelApi.Name;
        _googleLabelInfo.LabelType = _googleLabelApi.LabelType;
        _googleLabelInfo.TenantId = _googleLabelApi.CustomerId;
        _googleLabelInfo.Extension = _googleLabelApi.Extension;
        _googleLabelInfo.State = (int)_googleLabelApi.State;
    }

    public RMGoogleLabelInfo GetGoogleLabelInfo()
    {
        return _googleLabelInfo;
    }
}