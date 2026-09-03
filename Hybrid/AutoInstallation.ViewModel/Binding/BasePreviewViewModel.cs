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


using System.Collections.Generic;
using System.Linq;
using AutoInstallation.Contract;
using AutoInstallation.Contract.Interface;

namespace AutoInstallation.ViewModel.Binding
{
    public abstract class BasePreviewViewModel : NotifyPropertyChanged, IPreviewViewModel
    {
        private List<ReportItem> items = new List<ReportItem>();
        public string Title { get; set; }

        public List<ReportItem> Items
        {
            get { return items; }
            set
            {
                items = value;
                OnPropertyChanged("Items");
            }
        }

        public abstract List<ReportItem> AdjustmentReportOrder();

        public void AddItem(ReportItem item)
        {
            var temp = Items.FirstOrDefault(it => it.Key == item.Key && it.Index == item.Index);
            if (temp == null)
                Items.Add(item);
            else
                temp.Value = item.Value;
        }

        public void RemoveItem(string key, int index)
        {
            var temp = Items.FirstOrDefault(it => it.Key == key && it.Index == index);
            Items.Remove(temp);
        }
    }
}