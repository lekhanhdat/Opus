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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;

namespace AvePoint.Deployment.CommonGUI
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:OCT.DocumentManagement.Client.GUI.CustomControls.UIControls.SearchTextBox"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:OCT.DocumentManagement.Client.GUI.CustomControls.UIControls.SearchTextBox;assembly=OCT.DocumentManagement.Client.GUI.CustomControls.UIControls.SearchTextBox"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:SearchToggleButton/>
    ///
    /// </summary>
    public class SearchToggleButton : ToggleButton
    {
        static SearchToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchToggleButton), new FrameworkPropertyMetadata(typeof(SearchToggleButton)));
        }

        Image searchImage;
        Image stopImage;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            searchImage = this.GetTemplateChild("X_OnSeachBar") as Image;
            stopImage = this.GetTemplateChild("X_OnDeleteBar") as Image;
            if (searchImage != null && SearchUri != null)
            {
                searchImage.Source = GetImageSourceByString(SearchUri);
            }
            if (stopImage != null && StopUri != null)
            {
                stopImage.Source = GetImageSourceByString(StopUri);
            }
        }

        #region == SearchUri ==
        /// <summary>
        /// 代表正常状态下Button的背景图片的Uri
        /// </summary>
        public string SearchUri
        {
            get { return (string)GetValue(SearchUriProperty); }
            set { SetValue(SearchUriProperty, value); }
        }

        public static readonly DependencyProperty SearchUriProperty = DependencyProperty.Register("SearchUri", typeof(string), typeof(SearchToggleButton), new PropertyMetadata(null, SearchUriPropertyChanged));

        private static void SearchUriPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            SearchToggleButton button = obj as SearchToggleButton;
            string newValue = args.NewValue as string;
            if (button != null && button.searchImage != null)
            {
                button.searchImage.Source = button.GetImageSourceByString(newValue);
            }
        }
        #endregion == SearchUri ==

        #region == SearchUri ==
        /// <summary>
        /// 代表正常状态下Button的背景图片的Uri
        /// </summary>
        public string StopUri
        {
            get { return (string)GetValue(StopUriProperty); }
            set { SetValue(StopUriProperty, value); }
        }

        public static readonly DependencyProperty StopUriProperty = DependencyProperty.Register("StopUri", typeof(string), typeof(SearchToggleButton), new PropertyMetadata(null, StopUriPropertyChanged));

        private static void StopUriPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            SearchToggleButton button = obj as SearchToggleButton;
            string newValue = args.NewValue as string;
            if (button != null && button.searchImage != null)
            {
                button.searchImage.Source = button.GetImageSourceByString(newValue);
            }
        }
        #endregion == SearchUri ==

        /// <summary>
        /// 通过图片的Uri加载图片。
        /// </summary>
        /// <param name="sourseString"></param>
        /// <returns></returns>
        private BitmapImage GetImageSourceByString(string sourseString)
        {
            BitmapImage image = null;
            if (!string.IsNullOrEmpty(sourseString))
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri("pack://application:,,,/AvePoint.CallAssist.CommonDeploymentGUI;component" + sourseString, UriKind.RelativeOrAbsolute);
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0} is a invalid uri to be a image source.\n", sourseString) + e.ToString());
                }
            }
            return image;
        }
    }
}