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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using CloudRecordDownloadManager.Checkers;
using CloudRecordDownloadManager.Properties;

namespace CloudRecordDownloadManager.Modules {

    public enum ExaminationType {

        Network,
        PhysicalMemory,
        DotNetFramework,
        DiskSpace

    }

    public enum ExaminationStatus {

        Error,
        Warn,
        Wait,
        Pass

    }

    public sealed class ExaminationItem : INotifyPropertyChanged {

        private Brush _iconColor;
        private string _iconPath;
        private string _message;
        private ExaminationStatus _status;
        private string _title;

        private ExaminationType _type;
        private readonly TaskScheduler _taskScheduler;
        private Checker _checker;
        private Visibility _tipVisibility = Visibility.Hidden;
        private string _tipMessage;

        public ExaminationItem(ExaminationType type) {
            Status = ExaminationStatus.Wait;
            Type = type;
        }

        public ExaminationItem(ExaminationType type, TaskScheduler taskScheduler) {
            _taskScheduler = taskScheduler;
            Status = ExaminationStatus.Wait;
            Type = type;
        }

        public ExaminationItem(ExaminationType type, ExaminationStatus status) {
            Type = type;
            Status = status;
        }

        public ExaminationType Type {
            get => _type;
            set {
                _type = value;
                switch (_type) {
                    case ExaminationType.Network:
                        Title = I18N.key_07462cc5_2685_4595_923a_9c4a71fd5364;
                        Checker = new NetworkChecker(this, TaskScheduler);
                        break;
                    case ExaminationType.PhysicalMemory:
                        Title = I18N.key_42e3c7e5_ec8d_46f8_a367_292db9c1035d;
                        Checker = new PhysicalMemoryChecker(this, TaskScheduler);
                        break;
                    case ExaminationType.DotNetFramework:
                        Title = I18N.key_55740329_a988_47c6_9c64_bab0aa27c7f8;
                        Checker = new DotNetFrameworkChecker(this, TaskScheduler);
                        break;
                    case ExaminationType.DiskSpace:
                        Title = I18N.key_721008dd_96c3_4819_a95b_aa3ac6041131;
                        Checker = new DiskSpaceChecker(this, TaskScheduler);
                        break;
                }
            }
        }

        private TaskScheduler TaskScheduler => _taskScheduler;

        public ExaminationStatus Status {
            get => _status;
            set {
                _status = value;
                switch (_status) {
                    case ExaminationStatus.Error:
                        Message = I18N.key_ac1166cf_e270_43dc_b0ff_a43b79ff1af2;
                        IconPath = Icons.Error;
                        IconColor = new SolidColorBrush(Color.FromRgb(216, 0, 0));
                        TipVisibility = Visibility.Visible;
                        break;
                    case ExaminationStatus.Warn:
                        Message = I18N.key_b864387b_ddff_483b_8c32_7c8c6fd03c22;
                        IconPath = Icons.Warn;
                        IconColor = new SolidColorBrush(Color.FromRgb(246, 182, 6));
                        TipVisibility = Visibility.Visible;
                        break;
                    case ExaminationStatus.Wait:
                        Message = I18N.key_89b3350b_2410_4533_aa29_2700d5dfd2b2;
                        IconPath = Icons.Wait;
                        IconColor = new SolidColorBrush(Color.FromRgb(39, 125, 216));
                        break;
                    case ExaminationStatus.Pass:
                        Message = I18N.key_ccd03b9d_93d4_4b38_bb45_4390a76558db;
                        IconPath = Icons.Pass;
                        IconColor = new SolidColorBrush(Color.FromRgb(11, 188, 70));
                        break;
                }
            }
        }

        public Checker Checker {
            get => _checker;
            private set => _checker = value;
        }

        public string Title {
            get => _title;
            set {
                _title = value;
                OnPropertyChanged();
            }
        }

        public string Message {
            get => _message;
            set {
                _message = value;
                OnPropertyChanged();
            }
        }

        public string IconPath {
            get => _iconPath;
            set {
                _iconPath = value;
                OnPropertyChanged();
            }
        }

        public Brush IconColor {
            get => _iconColor;
            set {
                _iconColor = value;
                OnPropertyChanged();
            }
        }

        public Visibility TipVisibility {
            get => _tipVisibility;
            set {
                _tipVisibility = value;
                OnPropertyChanged();
            }
        }

        public string TipMessage {
            get => _tipMessage;
            set {
                _tipMessage = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

}