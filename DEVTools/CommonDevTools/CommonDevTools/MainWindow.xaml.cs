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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CommonDevTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void log_convert_Click(object sender, RoutedEventArgs e)
        {
            string inputVal = this.log_input.Text.Trim();
            var btn = sender as Button;
           
            if(inputVal != string.Empty)
            {
                try
                {
                    if (btn.Name == "log_encode")
                    {
                        string outputVal = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(inputVal));
                        this.log_output.Text = outputVal;
                    }
                    else
                    {
                        string outputVal = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(inputVal));
                        this.log_output.Text = outputVal;
                    }
                }
                catch
                {
                    this.log_output.Text = "Tool Convert Error";
                }
            }
            else
            {
                this.log_output.Text = "";
            }
        }

        private void time_convert__Click(object sender, RoutedEventArgs e)
        {
            string inputVal = this.time_input.Text.Trim();
            long timeLong;
            if (long.TryParse(inputVal, out timeLong))
            {
                DateTime t = new DateTime(timeLong);
                this.time_output.Text = t.ToString();
            }
            else
            {
                DateTime dateTime;
                if(DateTime.TryParse(inputVal, out dateTime))
                {
                    this.time_output.Text = dateTime.Ticks.ToString();
                }
                else
                {
                    this.time_output.Text = "Convert Error";
                }
            }
        }

        private void guid_new_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            bool lowerCase = this.guid_case.IsChecked == true;
            Guid guid = Guid.NewGuid();
            string result = lowerCase ? guid.ToString().ToLower() : guid.ToString().ToUpper();
            this.guid_result.Text = result;
            if(btn.Name == "guid_new_copy")
            {
                Clipboard.SetText(result);
            }

        }

        private void guid_copy_Click(object sender, RoutedEventArgs e)
        {
            string result = this.guid_result.Text.Trim();
            if(result != string.Empty)
            {
                Clipboard.SetText(result);
            }
        }
    }
}
