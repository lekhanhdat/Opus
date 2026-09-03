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
using AvePoint.RA.Contract.Label;
using Google.Apis.DriveLabels.v2.Data;
using Newtonsoft.Json;
using RAGoogle.Extension;

namespace RAGoogle.Models.Contract;

public class GoogleLabelApi : ISubject
{
    public Guid UniqueId { get; set; }
    public string LabelId { get; set; }
    public string Name { get; set; }
    public LabelType LabelType { get; set; }
    public string CustomerId { get; set; }
    public string Extension { get; set; }
    public string Description { get; set; }
    public State State { get; set; }
    private readonly List<ISubscription> _subscriptions = [];
    public void RegisterSubscription(ISubscription subscription)
    {
        _subscriptions.Add(subscription);
    }

    public void RemoveSubscription(ISubscription subscription)
    {
        _subscriptions.Remove(subscription);
    }

    public void NotifyUpdate()
    {
        _subscriptions.ForEach(subscription => subscription.Update());
    }

    private void GetNewLabel()
    {
        NotifyUpdate();
    }

    public void SetGoogleLabel(GoogleAppsDriveLabelsV2Label labelsV2Label)
    {
        UniqueId = new Guid();
        LabelId = labelsV2Label.Id;
        Name = labelsV2Label.Properties.Title;
        LabelType = GoogleLabelExtension.ConvertLabelType(labelsV2Label.LabelType);
        CustomerId = GoogleLabelExtension.GetCustomerId(labelsV2Label.Customer);
        Extension = JsonConvert.SerializeObject(labelsV2Label);
        Description = labelsV2Label.Properties.Description;
        State = GoogleLabelExtension.ConvertState(labelsV2Label.Lifecycle.State);
        GetNewLabel();
    }
}