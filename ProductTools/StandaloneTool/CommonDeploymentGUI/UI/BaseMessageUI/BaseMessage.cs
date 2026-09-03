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

    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.ComponentModel;
    using System.Windows.Media.Imaging;

    #endregion

    public class BaseMessage : Window, INotifyPropertyChanged
    {
        #region Property
        public ImageSource BaseMessageFormTitleImg
        {
            get { return (ImageSource) GetValue(BaseMessageFormTitleImgProperty); }
            set { SetValue(BaseMessageFormTitleImgProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TitleImg.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BaseMessageFormTitleImgProperty =
            DependencyProperty.Register("BaseMessageFormTitleImg",
                                        typeof(ImageSource),
                                        typeof(BaseMessage),
                                        new UIPropertyMetadata(null));

        public string BaseMessageFormText
        {
            get { return (string) GetValue(BaseMessageFormTextProperty); }
            set { SetValue(BaseMessageFormTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BaseMessageFormText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BaseMessageFormTextProperty =
            DependencyProperty.Register("BaseMessageFormText",
                                        typeof(string),
                                        typeof(BaseMessage),
                                        new UIPropertyMetadata(null));


        public string BaseMessageContentTitle
        {
            get { return (string) GetValue(BaseMessageContentTitleProperty); }
            set { SetValue(BaseMessageContentTitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BaseMessageContentTitle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BaseMessageContentTitleProperty =
            DependencyProperty.Register("BaseMessageContentTitle",
                                        typeof(string),
                                        typeof(BaseMessage),
                                        new UIPropertyMetadata(null));


        public string BaseMessageContentSummary
        {
            get { return (string) GetValue(BaseMessageContentSummaryProperty); }
            set { SetValue(BaseMessageContentSummaryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BaseMessageContentSummary.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BaseMessageContentSummaryProperty =
            DependencyProperty.Register("BaseMessageContentSummary",
                                        typeof(string),
                                        typeof(BaseMessage),
                                        new UIPropertyMetadata(null));


        public MessageType SubmitType
        {
            get { return (MessageType) GetValue(SubmitTypeProperty); }
            set { SetValue(SubmitTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SubmitType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SubmitTypeProperty = DependencyProperty.Register("SubmitType",
                                                                                                   typeof(MessageType),
                                                                                                   typeof(BaseMessage),
                                                                                                   new PropertyMetadata(
                                                                                                       MessageType.OK,
                                                                                                       OnSubmitTypeChanged));
        #endregion 

        private static void OnSubmitTypeChanged(object d, DependencyPropertyChangedEventArgs e)
        {
            BaseMessage self = d as BaseMessage;
            MessageType type = (MessageType) e.NewValue;
            if(self != null)
            {
                self.ResizeTextAreaWidth(type);
            }
        }

        #region converter icon

        public MessageIconType IconType
        {
            get { return (MessageIconType) GetValue(IconTypeProperty); }
            set { SetValue(IconTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IconType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconTypeProperty = DependencyProperty.Register("IconType",
                                                                                                 typeof(MessageIconType),
                                                                                                 typeof(BaseMessage),
                                                                                                 new PropertyMetadata(
                                                                                                     MessageIconType.
                                                                                                         Error,
                                                                                                     OnIconTypeChanged));

        private static void OnIconTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BaseMessage _self = d as BaseMessage;
            if(_self != null)
            {
                _self.UpdateIcon((MessageIconType) e.NewValue);
            }
        }


        private void UpdateIcon(MessageIconType type)
        {
            if(mwIcon == null)
            {
                return;
            }
            string _source = string.Empty;
            switch(type)
            {
                case MessageIconType.Done:
                    _source = "Images/msgwin_done_32x32.png";
                    break;
                case MessageIconType.Error:
                    _source = "Images/msgwin_error_32x32.png";
                    break;
                case MessageIconType.Warning:
                    _source = "Images/msgwin_warning_32x32.png";
                    break;
                case MessageIconType.Exit:
                    _source = "Images/cfmwin_exit_32x32.png";
                    break;
                case MessageIconType.About:
                    _source = "Images/install_32x32.png";
                    break;
            }
            mwIcon.Source = new BitmapImage(new Uri(_source, UriKind.RelativeOrAbsolute));
        }


        //private MessageIconType iconType;
        //public MessageIconType IconType
        //{
        //    get
        //    {
        //        return iconType;
        //    }
        //    set
        //    {
        //        iconType = value;
        //        OnPropertyChanged("IconType");
        //    }
        //}


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        private void ResizeTextAreaWidth(MessageType type)
        {
            if(this.contentGrid == null)
            {
                return;
            }
            switch(type)
            {
                case MessageType.RetryIgnoreCancel:
                    contentGrid.Width = 390;
                    break;
                case MessageType.YesNo:
                case MessageType.OK:
                case MessageType.Test:
                case MessageType.YesNoCancel:
                case MessageType.OKCancel:
                case MessageType.OKCancel1:
                case MessageType.DetailOK:
                    contentGrid.Width = 310;
                    break;
                default:
                    break;
            }
        }


        private void RedrawButton(MessageType type)
        {
            cancelButton.Visibility = Visibility.Collapsed;
            ignoreButton.Visibility = Visibility.Collapsed;
            retryButton.Visibility = Visibility.Collapsed;
            okButton.Visibility = Visibility.Collapsed;
            detailButton.Visibility = Visibility.Collapsed;
            testButton.Visibility = Visibility.Collapsed;
            noButton.Visibility = Visibility.Collapsed;
            yesButton.Visibility = Visibility.Collapsed;
            switch(type)
            {
                case MessageType.YesNoCancel:
                    yesButton.Visibility = Visibility.Visible;
                    yesButton.Style = FindResource("RecommandedButtonStyle") as Style;
                    yesButton.Focus();
                    noButton.Visibility = Visibility.Visible;
                    noButton.Style = FindResource("NotRecommandedButtonStyle") as Style;
                    cancelButton.Visibility = Visibility.Visible;
                    cancelButton.Style = FindResource("NotRecommandedButtonStyle") as Style;
                    break;
                case MessageType.Test:
                    testButton.Visibility = Visibility.Visible;
                    testButton.Style = FindResource("RecommandedButtonStyle") as Style;
                    break;
                case MessageType.OK:
                    okButton.Visibility = Visibility.Visible;
                    okButton.Style = FindResource("RecommandedButtonStyle") as Style;
                    break;
                case MessageType.OKCancel:
                    okButton.Visibility = Visibility.Visible;
                    okButton.Style = FindResource("NotRecommandedButtonStyle") as Style;
                    cancelButton.Visibility = Visibility.Visible;
                    cancelButton.Style = FindResource("NotRecommandedButtonStyle") as Style;
                    cancelButton.Focus();
                    break;
                case MessageType.OKCancel1:
                    okButton.Visibility = Visibility.Visible;
                    okButton.Style = FindResource("RecommandedButtonStyle") as Style;
                    okButton.Focus();
                    cancelButton.Visibility = Visibility.Visible;
                    cancelButton.Style = FindResource("NotRecommandedButtonStyle") as Style;
                    break;
                case MessageType.RetryIgnoreCancel:
                    cancelButton.Visibility = Visibility.Visible;
                    cancelButton.Style = FindResource("NotRecommandedButtonStyle") as Style;
                    ignoreButton.Visibility = Visibility.Visible;
                    ignoreButton.Style = FindResource("NotRecommandedButtonStyle") as Style;
                    retryButton.Visibility = Visibility.Visible;
                    retryButton.Style = FindResource("RecommandedButtonStyle") as Style;
                    retryButton.Focus();
                    break;
                case MessageType.DetailOK:
                    detailButton.Visibility = Visibility.Visible;
                    detailButton.Style = FindResource("RecommandedButtonStyle") as Style;
                    detailButton.Focus();
                    okButton.Visibility = Visibility.Visible;
                    okButton.Style = FindResource("NotRecommandedButtonStyle") as Style;
                    break;
                case MessageType.YesNo:
                    yesButton.Visibility = Visibility.Visible;
                    yesButton.Style = FindResource("NotRecommandedButtonStyle") as Style;
                    noButton.Visibility = Visibility.Visible;
                    noButton.Style = FindResource("NotRecommandedButtonStyle") as Style;
                    noButton.Focus();
                    break;
                default:
                    break;
            }
        }

        private DockPanel mBorderTitle;
        private Grid contentGrid;

        private TextBlock messageSummary;
        private TextBlock messageTitle;

        private Button retryButton;
        private Button ignoreButton;
        private Button okButton;
        private Button cancelButton;
        private Button detailButton;
        private Button testButton;
        private Button noButton;
        private Button yesButton;

        private Image mwIcon;


        public BaseMessage()
        {
            this.DefaultStyleKey = typeof(BaseMessage);
            this.Loaded += delegate { InitializeEvent(); };
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(BaseMessage), new FrameworkPropertyMetadata(typeof(BaseMessage)));
        }

        private void InitializeEvent()
        {
            mBorderTitle.MouseMove += delegate(object sender, MouseEventArgs e)
                                      {
                                          if(e.LeftButton == MouseButtonState.Pressed)
                                          {
                                              this.DragMove();
                                          }
                                      };
        }

        private void mBorderTitle_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        public override void OnApplyTemplate()
        {
            mBorderTitle = this.GetTemplateChild("borderTitle") as DockPanel;
            messageSummary = this.GetTemplateChild("messageSummary") as TextBlock;
            messageTitle = this.GetTemplateChild("messageTitle") as TextBlock;
            detailButton = this.GetTemplateChild("detailButton") as Button;
            retryButton = this.GetTemplateChild("retryButton") as Button;
            ignoreButton = this.GetTemplateChild("ignoreButton") as Button;
            okButton = this.GetTemplateChild("okButton") as Button;
            testButton = this.GetTemplateChild("testButton") as Button;
            cancelButton = this.GetTemplateChild("cancelButton") as Button;
            noButton = this.GetTemplateChild("noButton") as Button;
            yesButton = this.GetTemplateChild("yesButton") as Button;
            contentGrid = this.GetTemplateChild("contentGrid") as Grid;
            mwIcon = this.GetTemplateChild("mwIcon") as Image;

            RedrawButton(SubmitType);
            ResizeTextAreaWidth(SubmitType);

            base.OnApplyTemplate();

            UpdateIcon(IconType);

            AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(Button_Click));
        }

        private MessageResult messageResult = MessageResult.None;
        protected MessageResult MessageResult
        {
            get
            {
                return messageResult;
            }
            set
            {
                messageResult = value;
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = e.OriginalSource as Button;
            if(button == null)
            {
                return;
            }
            UpdateManageResult(button);
            Close();
        }

        private void UpdateManageResult(Button button)
        {
            switch (button.Name)
            {
                case "noButton":
                    MessageResult = MessageResult.No;
                    break;
                case "yesButton":
                    MessageResult = MessageResult.Yes;
                    break;
                case "testButton":
                    MessageResult = MessageResult.Test;
                    break;
                case "retryButton":
                    MessageResult = MessageResult.Retry;
                    break;
                case "ignoreButton":
                    MessageResult = MessageResult.Ignore;
                    break;
                case "cancelButton":
                    MessageResult = MessageResult.Cancel;
                    break;
                case "okButton":
                    MessageResult = MessageResult.OK;
                    break;
                case "detailButton":
                    MessageResult = MessageResult.Detail;
                    break;
            }
        }

        #region initialize methods

        public static MessageResult Show(string formText,
                                         string contentTitle,
                                         string contentSummary,
                                         MessageType messageType,
                                         MessageIconType messageIconType)
        {
            return ShowCore(formText, contentTitle, contentSummary, messageType, messageIconType);
        }

        public static MessageResult Show(BaseMessageConfig messageConfig)
        {
            if(messageConfig == null)
            {
                return MessageResult.None;
            }


            return ShowCore(messageConfig.FormText,
                            messageConfig.ContentTitle,
                            messageConfig.ContentSummary,
                            messageConfig.MessageType,
                            messageConfig.MessageIconType);
        }

        private static MessageResult ShowCore(string messageFormText,
                                              string messageContentTitle,
                                              object messageContentSummary,
                                              MessageType messageType,
                                              MessageIconType messageIconType)
        {
            BaseMessage baseMessage = new BaseMessage();
            baseMessage.InitializeMessageBox(messageFormText,
                                             messageContentTitle,
                                             messageContentSummary,
                                             messageType,
                                             messageIconType);
            //baseMessage
            return baseMessage.MessageResult;
        }

        protected void InitializeMessageBox(string messageFormText,
                                            string messageContentTitle,
                                            object messageContentSummary,
                                            MessageType messageType,
                                            MessageIconType messageIconType)
        {
            //BaseMessageFormTitleImg = messageFormIcon;
            BaseMessageFormText = messageFormText;
            BaseMessageContentTitle = messageContentTitle;
            IconType = messageIconType;
            SubmitType = messageType;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Title = messageFormText;
            this.AllowsTransparency = true;
            if(messageContentSummary is string)
            {
                BaseMessageContentSummary = messageContentSummary as string;
            }
            else
            {
                this.Content = messageContentSummary;
            }

            Uri iconUri = new Uri("pack://application:,,,/Images/logo.ico");
            this.Icon = BitmapFrame.Create(iconUri);
            this.ShowDialog();
        }

        #endregion
    }
}