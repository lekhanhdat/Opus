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

namespace AvePoint.Deployment.CommonGUI
{
    #region ---namespace---

    using System.Windows;
    using System.Windows.Controls;

    #endregion
    /// <summary>
    /// This class adds binding capabilities to the standard WPF PasswordBox.
    /// </summary>
    public class PasswordBoxBindingHelper
    {
        #region PasswordBoxBindingHelper

        private static bool _updating;


        public static readonly DependencyProperty IsPasswordBindingEnabledProperty =
            DependencyProperty.RegisterAttached("IsPasswordBindingEnabled",
                                                typeof(bool),
                                                typeof(PasswordBoxBindingHelper),
                                                new UIPropertyMetadata(false, OnIsPasswordBindingEnabledChanged));

        public static bool GetIsPasswordBindingEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsPasswordBindingEnabledProperty);
        }

        public static void SetIsPasswordBindingEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsPasswordBindingEnabledProperty, value);
        }

        private static void OnIsPasswordBindingEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox passwordBox = obj as PasswordBox;

            if (passwordBox != null)
            {
                passwordBox.PasswordChanged -= PasswordBoxPasswordChanged;

                if ((bool)e.NewValue)
                {
                    passwordBox.PasswordChanged += PasswordBoxPasswordChanged;
                }
            }
        }


        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached("BoundPassword",
                                                typeof(string),
                                                typeof(PasswordBoxBindingHelper),
                                                new FrameworkPropertyMetadata(string.Empty, OnBoundPasswordChanged));

        public static string GetBoundPassword(DependencyObject dependencyObject)
        {
            return (string)dependencyObject.GetValue(BoundPasswordProperty);
        }

        public static void SetBoundPassword(DependencyObject dependencyObject, string value)
        {
            dependencyObject.SetValue(BoundPasswordProperty, value);
        }

        private static void OnBoundPasswordChanged(DependencyObject dependencyObject,
                                                   DependencyPropertyChangedEventArgs e)
        {
            PasswordBox password = dependencyObject as PasswordBox;
            if (password != null)
            {
                // Disconnect the handler while we're updating.
                password.PasswordChanged -= PasswordBoxPasswordChanged;


                if (!string.IsNullOrEmpty(e.NewValue.ToString()))
                {
                    if (!_updating)
                    {
                        password.Password = e.NewValue.ToString();
                    }
                }
                else
                {
                    password.Password = e.NewValue.ToString();
                }
                // Now, reconnect the handler.
                password.PasswordChanged += PasswordBoxPasswordChanged;
            }
        }


        /// <summary>
        /// Handles the password change event.
        /// </summary>
        private static void PasswordBoxPasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox password = sender as PasswordBox;
            if (!string.IsNullOrEmpty(password.Password))
            {
                _updating = true;
                SetBoundPassword(password, password.Password);
                _updating = false;
            }
            else
            {
                //_updating = true;
                SetBoundPassword(password, password.Password);
                //password.Password = "";
                //if (password.Password == string.Empty)
                //{
                //    SetBoundPassword(password, password.Password);
                //}
                //_updating = false;
            }
        }

        #endregion
    }
}