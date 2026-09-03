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
using System.ComponentModel;

namespace AvePoint.GCommon.Contract.Server.GUI
{
    public class AUITableColumnNameFilter : AUITableColumnFilterBase
    {
        private List<AUITableColumnNameFilterNameAndIsChecked> mDisplayNamesAndIsChecked = new List<AUITableColumnNameFilterNameAndIsChecked>();

        public List<AUITableColumnNameFilterNameAndIsChecked> DisplayNamesAndIsChecked
        {
            get
            {
                return mDisplayNamesAndIsChecked;
            }
            set
            {
                mDisplayNamesAndIsChecked = value;
            }
        }

        public bool IsContainsName(string name)
        {
            if (DisplayNamesAndIsChecked == null)
            {
                return false;
            }
            foreach (AUITableColumnNameFilterNameAndIsChecked item in DisplayNamesAndIsChecked)
            {
                // "(Blanks)"需要特殊处理
                if (item.Name.Equals(AUITableColumnNameFilterNameAndIsChecked.BlankText))
                {
                    if ("".Equals(name))
                    {
                        return true;
                    }
                }
                else if (item.Name.Equals(name))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsContainsAndCheckedName(string name)
        {
            if (DisplayNamesAndIsChecked == null)
            {
                return false;
            }
            foreach (AUITableColumnNameFilterNameAndIsChecked item in DisplayNamesAndIsChecked)
            {
                // modified by jptian: [ADO-7574]当内容为空或者Null的时候，显示为“(Blanks)” -- 2011.10.18
                // ====================
                if (item.Name.Equals(AUITableColumnNameFilterNameAndIsChecked.BlankText) && item.IsChecked)
                {
                    if (string.IsNullOrEmpty(name))
                    {
                        return true;
                    }
                }
                // ====================
                if (item.Name.Equals(name) && item.IsChecked)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public class AUITableColumnNameFilterNameAndIsChecked : INotifyPropertyChanged
    {
        /// <summary>
        /// 当内容为空或者Null的时候显示的内容
        /// </summary>
        public const string BlankText = "(Blanks)";

        private string name;

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    name = BlankText;
                }
                else
                {
                    name = value;
                }
                OnPropertyChanged("Name");
            }
        }

        private bool isChecked;

        public bool IsChecked
        {
            get
            {
                return isChecked;
            }
            set
            {
                isChecked = value;
                OnPropertyChanged("IsChecked");
            }
        }

        private object mValue;

        public object Value
        {
            get
            {
                return mValue;
            }
            set
            {
                mValue = value;
                OnPropertyChanged("Value");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string proName)
        { 
            if(PropertyChanged != null)
            {
                PropertyChanged(this,new PropertyChangedEventArgs(proName));
            }
        }
    }
}
