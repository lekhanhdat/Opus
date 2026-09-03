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
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AutoInstallation.Contract.Message
{
    public class BaseMessage : Window, INotifyPropertyChanged
    {
        public static readonly DependencyProperty TestContentProperty =
            DependencyProperty.Register("TestContent", typeof(string), typeof(BaseMessage),
                new UIPropertyMetadata(null));

        public static readonly DependencyProperty YesContentProperty =
            DependencyProperty.Register("YesContent", typeof(string), typeof(BaseMessage),
                new UIPropertyMetadata(null));

        public static readonly DependencyProperty NoContentProperty =
            DependencyProperty.Register("NoContent", typeof(string), typeof(BaseMessage), new UIPropertyMetadata(null));

        public static readonly DependencyProperty UpdateContentProperty =
            DependencyProperty.Register("UpdateContent", typeof(string), typeof(BaseMessage),
                new UIPropertyMetadata(null));

        public static readonly DependencyProperty CancelContentProperty =
            DependencyProperty.Register("CancelContent", typeof(string), typeof(BaseMessage),
                new UIPropertyMetadata(null));

        public static readonly DependencyProperty OkContentProperty =
            DependencyProperty.Register("OkContent", typeof(string), typeof(BaseMessage), new UIPropertyMetadata(null));

        public static readonly DependencyProperty IgnoreContentProperty =
            DependencyProperty.Register("IgnoreContent", typeof(string), typeof(BaseMessage),
                new UIPropertyMetadata(null));

        public static readonly DependencyProperty RetryContentProperty =
            DependencyProperty.Register("RetryContent", typeof(string), typeof(BaseMessage),
                new UIPropertyMetadata(null));

        // Using a DependencyProperty as the backing store for TitleImg.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DetailContentProperty =
            DependencyProperty.Register("DetailContent", typeof(string), typeof(BaseMessage),
                new UIPropertyMetadata(null));

        // Using a DependencyProperty as the backing store for TitleImg.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BaseMessageFormTitleImgProperty =
            DependencyProperty.Register("BaseMessageFormTitleImg", typeof(ImageSource), typeof(BaseMessage),
                new UIPropertyMetadata(null));

        // Using a DependencyProperty as the backing store for BaseMessageFormText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BaseMessageFormTextProperty =
            DependencyProperty.Register("BaseMessageFormText", typeof(string), typeof(BaseMessage),
                new UIPropertyMetadata(null));

        // Using a DependencyProperty as the backing store for BaseMessageContentTitle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BaseMessageContentTitleProperty =
            DependencyProperty.Register("BaseMessageContentTitle", typeof(string), typeof(BaseMessage),
                new UIPropertyMetadata(null));

        // Using a DependencyProperty as the backing store for BaseMessageContentSummary.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BaseMessageContentSummaryProperty =
            DependencyProperty.Register("BaseMessageContentSummary", typeof(string), typeof(BaseMessage),
                new UIPropertyMetadata(null));

        // Using a DependencyProperty as the backing store for SubmitType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SubmitTypeProperty =
            DependencyProperty.Register("SubmitType", typeof(MessageType), typeof(BaseMessage),
                new PropertyMetadata(MessageType.OK, OnSubmitTypeChanged));

        private Button cancelButton;
        private Grid contentGrid;
        private Button detailButton;
        private Button ignoreButton;

        private DockPanel mBorderTitle;

        protected MessageResult messageResult = MessageResult.None;

        private TextBlock messageSummary;
        private TextBlock messageTitle;
        private Image mwIcon;
        private Button noButton;
        private Button okButton;

        private Button retryButton;
        private Button testButton;
        private Button updateButton;
        private Button yesButton;


        public BaseMessage()
        {
            DefaultStyleKey = typeof(BaseMessage);
            Loaded += delegate { InitializeEvent(); };
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(BaseMessage), new FrameworkPropertyMetadata(typeof(BaseMessage)));
        }

        public string TestContent
        {
            get { return (string) GetValue(TestContentProperty); }
            set { SetValue(TestContentProperty, value); }
        }

        public string YesContent
        {
            get { return (string) GetValue(YesContentProperty); }
            set { SetValue(YesContentProperty, value); }
        }

        public string NoContent
        {
            get { return (string) GetValue(NoContentProperty); }
            set { SetValue(NoContentProperty, value); }
        }

        public string UpdateContent
        {
            get { return (string) GetValue(UpdateContentProperty); }
            set { SetValue(UpdateContentProperty, value); }
        }


        public string CancelContent
        {
            get { return (string) GetValue(CancelContentProperty); }
            set { SetValue(CancelContentProperty, value); }
        }

        public string OkContent
        {
            get { return (string) GetValue(OkContentProperty); }
            set { SetValue(OkContentProperty, value); }
        }

        public string IgnoreContent
        {
            get { return (string) GetValue(IgnoreContentProperty); }
            set { SetValue(IgnoreContentProperty, value); }
        }

        public string RetryContent
        {
            get { return (string) GetValue(RetryContentProperty); }
            set { SetValue(RetryContentProperty, value); }
        }

        public string DetailContent
        {
            get { return (string) GetValue(DetailContentProperty); }
            set { SetValue(DetailContentProperty, value); }
        }


        public ImageSource BaseMessageFormTitleImg
        {
            get { return (ImageSource) GetValue(BaseMessageFormTitleImgProperty); }
            set { SetValue(BaseMessageFormTitleImgProperty, value); }
        }

        public string BaseMessageFormText
        {
            get { return (string) GetValue(BaseMessageFormTextProperty); }
            set { SetValue(BaseMessageFormTextProperty, value); }
        }


        public string BaseMessageContentTitle
        {
            get { return (string) GetValue(BaseMessageContentTitleProperty); }
            set { SetValue(BaseMessageContentTitleProperty, value); }
        }


        public string BaseMessageContentSummary
        {
            get { return (string) GetValue(BaseMessageContentSummaryProperty); }
            set { SetValue(BaseMessageContentSummaryProperty, value); }
        }


        public MessageType SubmitType
        {
            get { return (MessageType) GetValue(SubmitTypeProperty); }
            set { SetValue(SubmitTypeProperty, value); }
        }

        private static void OnSubmitTypeChanged(object d, DependencyPropertyChangedEventArgs e)
        {
            var _self = d as BaseMessage;
            var _type = (MessageType) e.NewValue;
            if (_self != null
                && _type != null)
                _self.ResizeTextAreaWidth(_type);
        }


        private void ResizeTextAreaWidth(MessageType type)
        {
            if (contentGrid == null) return;
            switch (type)
            {
                case MessageType.RetryIgnoreCancel:
                    contentGrid.Width = 390;
                    break;
                case MessageType.OK:
                case MessageType.OKCancel1:
                case MessageType.Test:
                case MessageType.RetryCancel:
                case MessageType.YesNoCancel:
                case MessageType.DetailUpdateOk:
                case MessageType.OKCancel:
                case MessageType.DetailOK:
                    contentGrid.Width = 370;
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
            updateButton.Visibility = Visibility.Collapsed;
            switch (type)
            {
                case MessageType.YesNoCancel:
                    yesButton.Visibility = Visibility.Visible;
                    yesButton.Style = FindResource("RecommandedButtonStyle") as Style;
                    noButton.Visibility = Visibility.Visible;
                    noButton.Style = FindResource("NotRecommandedButtonStyle") as Style;
                    cancelButton.Visibility = Visibility.Visible;
                    cancelButton.Style = FindResource("NotRecommandedButtonStyle") as Style;
                    yesButton.Focus();
                    break;
                case MessageType.DetailUpdateOk:
                    detailButton.Visibility = Visibility.Visible;
                    detailButton.Style = FindResource("NotRecommandedButtonStyle") as Style;
                    updateButton.Visibility = Visibility.Visible;
                    updateButton.Style = FindResource("RecommandedButtonStyle") as Style;
                    okButton.Visibility = Visibility.Visible;
                    okButton.Style = FindResource("NotRecommandedButtonStyle") as Style;
                    detailButton.Focus();
                    break;
                case MessageType.Test:
                    testButton.Visibility = Visibility.Visible;
                    testButton.Style = FindResource("RecommandedButtonStyle") as Style;
                    testButton.Focus();
                    break;
                case MessageType.OK:
                    okButton.Visibility = Visibility.Visible;
                    okButton.Style = FindResource("RecommandedButtonStyle") as Style;
                    okButton.Focus();
                    break;
                case MessageType.OKCancel:
                    okButton.Visibility = Visibility.Visible;
                    okButton.Style = FindResource("NotRecommandedButtonStyle") as Style;
                    cancelButton.Visibility = Visibility.Visible;
                    cancelButton.Style = FindResource("RecommandedButtonStyle") as Style;
                    cancelButton.Focus();
                    break;
                case MessageType.OKCancel1:
                    okButton.Visibility = Visibility.Visible;
                    okButton.Style = FindResource("RecommandedButtonStyle") as Style;
                    okButton.Focus();
                    cancelButton.Visibility = Visibility.Visible;
                    cancelButton.Style = FindResource("NotRecommandedButtonStyle") as Style;
                    break;
                case MessageType.RetryCancel:
                    cancelButton.Visibility = Visibility.Visible;
                    cancelButton.Style = FindResource("NotRecommandedButtonStyle") as Style;
                    cancelButton.Focus();
                    retryButton.Visibility = Visibility.Visible;
                    retryButton.Style = FindResource("RecommandedButtonStyle") as Style;
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
                    okButton.Visibility = Visibility.Visible;
                    okButton.Style = FindResource("NotRecommandedButtonStyle") as Style;
                    detailButton.Focus();
                    break;
                default:
                    break;
            }
        }

        private void InitializeEvent()
        {
            MouseMove += delegate(object sender, MouseEventArgs e)
            {
                if (e.LeftButton == MouseButtonState.Pressed) DragMove();
            };
            mBorderTitle.MouseMove += delegate(object sender, MouseEventArgs e)
            {
                if (e.LeftButton == MouseButtonState.Pressed) DragMove();
            };
        }

        private void mBorderTitle_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        public override void OnApplyTemplate()
        {
            mBorderTitle = GetTemplateChild("borderTitle") as DockPanel;
            messageSummary = GetTemplateChild("messageSummary") as TextBlock;
            messageTitle = GetTemplateChild("messageTitle") as TextBlock;
            detailButton = GetTemplateChild("detailButton") as Button;
            updateButton = GetTemplateChild("updateButton") as Button;
            retryButton = GetTemplateChild("retryButton") as Button;
            ignoreButton = GetTemplateChild("ignoreButton") as Button;
            okButton = GetTemplateChild("okButton") as Button;
            testButton = GetTemplateChild("testButton") as Button;
            cancelButton = GetTemplateChild("cancelButton") as Button;
            noButton = GetTemplateChild("noButton") as Button;
            yesButton = GetTemplateChild("yesButton") as Button;
            contentGrid = GetTemplateChild("contentGrid") as Grid;
            mwIcon = GetTemplateChild("mwIcon") as Image;

            RedrawButton(SubmitType);
            ResizeTextAreaWidth(SubmitType);

            base.OnApplyTemplate();

            UpdateIcon(IconType);

            AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(Button_Click));
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = e.OriginalSource as Button;
            if (button == null) return;
            switch (button.Name)
            {
                case "noButton":
                    messageResult = MessageResult.No;
                    break;
                case "yesButton":
                    messageResult = MessageResult.Yes;
                    break;
                case "updateButton":
                    messageResult = MessageResult.Update;
                    break;
                case "testButton":
                    messageResult = MessageResult.Test;
                    break;
                case "retryButton":
                    messageResult = MessageResult.Retry;
                    break;
                case "ignoreButton":
                    messageResult = MessageResult.Ignore;
                    break;
                case "cancelButton":
                    messageResult = MessageResult.Cancel;
                    break;
                case "okButton":
                    messageResult = MessageResult.OK;
                    break;
                case "detailButton":
                    messageResult = MessageResult.Detail;
                    break;
            }

            Close();
        }

        #region converter icon

        public MessageIconType IconType
        {
            get { return (MessageIconType) GetValue(IconTypeProperty); }
            set { SetValue(IconTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IconType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconTypeProperty =
            DependencyProperty.Register("IconType", typeof(MessageIconType), typeof(BaseMessage),
                new PropertyMetadata(MessageIconType.Error, OnIconTypeChanged));

        private static void OnIconTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var _self = d as BaseMessage;
            if (_self != null) _self.UpdateIcon((MessageIconType) e.NewValue);
        }


        private void UpdateIcon(MessageIconType type)
        {
            if (mwIcon == null) return;
            var _source = string.Empty;
            switch (type)
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
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region initialize methods

        public static MessageResult Show(Window owner, string formText, string contentTitle, string contentSummary,
            MessageType messageType, MessageIconType messageIconType, ImageSource iconPath, string okContent,
            string cancelContent, string retryContent, string detailContent, string ignoreContent, string updateContent)
        {
            return ShowCore(owner, formText, contentTitle, contentSummary, messageType, messageIconType, iconPath,
                okContent, cancelContent, detailContent, ignoreContent, retryContent, updateContent);
        }

        public static MessageResult Show(BaseMessageConfig messageConfig)
        {
            if (messageConfig == null) return MessageResult.None;
            return ShowCore(
                messageConfig.OwnerWindow,
                messageConfig.FormText,
                messageConfig.ContentTitle,
                messageConfig.ContentSummary,
                messageConfig.MessageType,
                messageConfig.MessageIconType,
                messageConfig.Icon,
                messageConfig.OkContent,
                messageConfig.CancelContent,
                messageConfig.DetailContent,
                messageConfig.IgnoreContent,
                messageConfig.RetryContent,
                messageConfig.UpdateContent);
        }

        private static MessageResult ShowCore(
            Window messageOwner,
            string messageFormText,
            string messageContentTitle,
            object messageContentSummary,
            MessageType messageType,
            MessageIconType messageIconType,
            ImageSource icon,
            string okContent,
            string cancelContent,
            string detailContent,
            string ignoreContent,
            string retryContent,
            string updateContent)
        {
            var baseMessage = new BaseMessage();
            baseMessage.InitializeMessageBox(
                messageOwner,
                messageFormText,
                messageContentTitle,
                messageContentSummary,
                messageType,
                messageIconType,
                icon,
                okContent,
                cancelContent,
                detailContent,
                ignoreContent,
                retryContent,
                updateContent);
            //baseMessage
            return baseMessage.messageResult;
        }

        protected void InitializeMessageBox(
            Window messageOwner,
            string messageFormText,
            string messageContentTitle,
            object messageContentSummary,
            MessageType messageType,
            MessageIconType messageIconType,
            ImageSource icon,
            string okContent,
            string cancelContent,
            string detailContent,
            string ignoreContent,
            string retryContent,
            string updateContent)
        {
            Owner = messageOwner;
            MaxHeight = 160;
            MaxWidth = 500;
            ResizeMode = ResizeMode.NoResize;
            //BaseMessageFormTitleImg = messageFormIcon;
            BaseMessageFormText = messageFormText;
            BaseMessageContentTitle = messageContentTitle;
            IconType = messageIconType;
            SubmitType = messageType;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Title = messageFormText;
            OkContent = okContent;
            CancelContent = cancelContent;
            RetryContent = retryContent;
            IgnoreContent = ignoreContent;
            DetailContent = detailContent;
            UpdateContent = updateContent;
            AllowsTransparency = true;
            WindowStyle = WindowStyle.None;
            if (messageContentSummary is string)
                BaseMessageContentSummary = messageContentSummary as string;
            else
                Content = messageContentSummary;
            Icon = icon;
            ShowDialog();
            //this.Dispatcher.Invoke(new Action(() => { this.Icon = icon; }));
        }

        #endregion
    }
}