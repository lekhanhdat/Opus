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
using System.Windows.Media;
using AutoInstallation.Contract;

namespace AutoInstallation.ViewModel.Binding
{
    public class BaseTreeViewModel : NotifyPropertyChanged
    {
        /// <summary>
        ///     构造
        /// </summary>
        public BaseTreeViewModel()
        {
            Children = new List<BaseTreeViewModel>();
            _isChecked = false;
            IsExpanded = false;
            //_icon = "/Images/16_16/folder_go.png";
        }

        /// <summary>
        ///     键值
        /// </summary>
        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }

        /// <summary>
        ///     显示的字符
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        ///     图标
        /// </summary>
        public ImageSource Icon
        {
            get { return _icon; }
            set { _icon = value; }
        }

        /// <summary>
        ///     指针悬停时的显示说明
        /// </summary>
        public string ToolTip => string.Format("{0}-{1}", Id, Name);

        /// <summary>
        ///     是否选中
        /// </summary>
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (value != _isChecked)
                {
                    _isChecked = value;
                    OnPropertyChanged("IsChecked");

                    if (_isChecked)
                    {
                        //如果选中则父项也应该选中
                        if (Parent != null) Parent.IsChecked = true;
                    }
                    else
                    {
                        //如果取消选中子项也应该取消选中
                        foreach (var child in Children) child.IsChecked = false;
                    }
                }
            }
        }

        /// <summary>
        ///     是否展开
        /// </summary>
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value != _isExpanded)
                {
                    //折叠状态改变
                    _isExpanded = value;
                    OnPropertyChanged("IsExpanded");
                }
            }
        }

        /// <summary>
        ///     父项
        /// </summary>
        public BaseTreeViewModel Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        /// <summary>
        ///     子项
        /// </summary>
        public IList<BaseTreeViewModel> Children
        {
            get { return _children; }
            set { _children = value; }
        }

        /// <summary>
        ///     设置所有子项的选中状态
        /// </summary>
        /// <param name="isChecked"></param>
        public void SetChildrenChecked(bool isChecked)
        {
            foreach (var child in Children)
            {
                child.IsChecked = IsChecked;
                child.SetChildrenChecked(IsChecked);
            }
        }

        /// <summary>
        ///     设置所有子项展开状态
        /// </summary>
        /// <param name="isExpanded"></param>
        public void SetChildrenExpanded(bool isExpanded)
        {
            foreach (var child in Children)
            {
                child.IsExpanded = isExpanded;
                child.SetChildrenExpanded(isExpanded);
            }
        }

        #region 私有变量

        /// <summary>
        ///     Id值
        /// </summary>
        private string _id;

        /// <summary>
        ///     显示的名称
        /// </summary>
        private string _name;

        /// <summary>
        ///     图标路径
        /// </summary>
        private ImageSource _icon;

        /// <summary>
        ///     选中状态
        /// </summary>
        private bool _isChecked;

        /// <summary>
        ///     折叠状态
        /// </summary>
        private bool _isExpanded;

        /// <summary>
        ///     子项
        /// </summary>
        private IList<BaseTreeViewModel> _children;

        /// <summary>
        ///     父项
        /// </summary>
        private BaseTreeViewModel _parent;

        #endregion
    }
}