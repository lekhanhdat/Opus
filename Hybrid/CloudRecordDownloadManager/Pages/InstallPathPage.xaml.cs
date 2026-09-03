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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using CloudRecordDownloadManager.Cache;
using CloudRecordDownloadManager.Properties;
using CloudRecordDownloadManager.Windows;
using MessageBox = System.Windows.MessageBox;

namespace CloudRecordDownloadManager.Pages {

    public partial class InstallPathPage : BasePage {

        public InstallPathPage() {
            InitializeComponent();
            AutoSetBackgroundImage(BackgroundImage);
            PathInput.Text = RuntimeCache.InstallPath;
        }

        private void FolderAction(object sender, RoutedEventArgs e) {
            using (var dialog = new FolderBrowserDialog {
                Description = I18N.key_a5ce2d83_e6a5_4aa6_9893_b4eab31ab9dd,
                SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                ShowNewFolderButton = true
            }) {
                if (dialog.ShowDialog() != DialogResult.OK) return;

                // var items = dialog.SelectedPath.Split(Path.PathSeparator).ToList();
                // if (string.IsNullOrEmpty(items.Last())) items.RemoveAt(items.Count - 1);
                //
                // if (items.Last() != RuntimeCache.InstallFolder) items.Add(RuntimeCache.InstallFolder);
                //
                // PathInput.Text = Path.Combine(items.ToArray());

                PathInput.Text = dialog.SelectedPath;
            }
        }

        private void BackAction(object sender, RoutedEventArgs e) {
            ToPage<LicensePage>();
        }

        private void NextAction(object sender, RoutedEventArgs e) {
            if (!IsValidPath(PathInput.Text)) {
                MessageBox.Show(string.Format(I18N.key_ac4d7b37_c3a5_4c16_913b_9e90d3656c7d, PathInput.Text),
                    I18N.key_257e3086_b755_4a8d_a694_451b6ab02aa7, MessageBoxButton.OK, MessageBoxImage.Warning);
                PathInput.Text = RuntimeCache.InstallPath;
            } else {
                RuntimeCache.InstallPath = PathInput.Text;
                Log.Info($"[{ClassName}] install path: {RuntimeCache.InstallPath}");
                ToPage<ExaminationPage>();
            }
        }

        private bool IsValidPath(string path) {
            // if ./ /.
            if (path.Contains(@".\") || path.Contains(@"\.")) return false;
            // Check if the path is rooted in a driver
            if (path.Length < 3) return false;
            var driveCheck = new Regex(@"^[a-zA-Z]:\\$");
            if (!driveCheck.IsMatch(path.Substring(0, 3))) return false;

            // Check if such driver exists
            var allMachineDrivers = DriveInfo.GetDrives().Select(drive => drive.Name);
            if (!allMachineDrivers.Contains(path.Substring(0, 3))) return false;

            // Check if the rest of the path is valid
            var InvalidFileNameChars = new string(Path.GetInvalidPathChars());
            InvalidFileNameChars += @":/?*" + "\"";
            var containsABadCharacter = new Regex("[" + Regex.Escape(InvalidFileNameChars) + "]");
            if (containsABadCharacter.IsMatch(path.Substring(3, path.Length - 3)))
                return false;
            return path[path.Length - 1] != '.';
        }

    }

}