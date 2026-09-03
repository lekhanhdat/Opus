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
using System.Net;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Resources;
using System.Windows.Threading;

namespace AutoInstallation.Common.GUI
{
    public class GifImageExceptionRoutedEventArgs : RoutedEventArgs
    {
        public Exception ErrorException;

        public GifImageExceptionRoutedEventArgs(RoutedEvent routedEvent, object obj)
            : base(routedEvent, obj)
        {
        }
    }

    internal class WebReadState
    {
        public byte[] buffer;
        public MemoryStream memoryStream;
        public Stream readStream;
        public WebRequest webRequest;
    }


    public class GifImage : UserControl
    {
        public delegate void ExceptionRoutedEventHandler(object sender, GifImageExceptionRoutedEventArgs args);

        public static readonly DependencyProperty ForceGifAnimationProperty =
            DependencyProperty.Register("ForceGifAnimation", typeof(bool), typeof(GifImage),
                new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(string),
            typeof(GifImage),
            new FrameworkPropertyMetadata("",
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                OnSourceChanged));


        public static readonly DependencyProperty StretchProperty = DependencyProperty.Register("Stretch",
            typeof(Stretch), typeof(GifImage),
            new FrameworkPropertyMetadata(Stretch.Fill, FrameworkPropertyMetadataOptions.AffectsMeasure,
                OnStretchChanged));

        public static readonly DependencyProperty StretchDirectionProperty = DependencyProperty.Register(
            "StretchDirection", typeof(StretchDirection), typeof(GifImage),
            new FrameworkPropertyMetadata(StretchDirection.Both, FrameworkPropertyMetadataOptions.AffectsMeasure,
                OnStretchDirectionChanged));

        public static readonly RoutedEvent ImageFailedEvent = EventManager.RegisterRoutedEvent("ImageFailed",
            RoutingStrategy.Bubble, typeof(ExceptionRoutedEventHandler), typeof(GifImage));

        private GifAnimation gifAnimation;
        private Image image;

        public bool ForceGifAnimation
        {
            get { return (bool) GetValue(ForceGifAnimationProperty); }
            set { SetValue(ForceGifAnimationProperty, value); }
        }

        public string Source
        {
            get { return (string) GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public Stretch Stretch
        {
            get { return (Stretch) GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        public StretchDirection StretchDirection
        {
            get { return (StretchDirection) GetValue(StretchDirectionProperty); }
            set { SetValue(StretchDirectionProperty, value); }
        }

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (GifImage) d;
            var s = (string) e.NewValue;
            obj.CreateFromSourceString(s);
        }

        private static void OnStretchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (GifImage) d;
            var s = (Stretch) e.NewValue;
            if (obj.gifAnimation != null)
                obj.gifAnimation.Stretch = s;
            else if (obj.image != null) obj.image.Stretch = s;
        }

        private static void OnStretchDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (GifImage) d;
            var s = (StretchDirection) e.NewValue;
            if (obj.gifAnimation != null)
                obj.gifAnimation.StretchDirection = s;
            else if (obj.image != null) obj.image.StretchDirection = s;
        }

        public event ExceptionRoutedEventHandler ImageFailed
        {
            add { AddHandler(ImageFailedEvent, value); }
            remove { RemoveHandler(ImageFailedEvent, value); }
        }

        private void image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            RaiseImageFailedEvent(e.ErrorException);
        }


        private void RaiseImageFailedEvent(Exception exp)
        {
            var newArgs = new GifImageExceptionRoutedEventArgs(ImageFailedEvent, this);
            newArgs.ErrorException = exp;
            RaiseEvent(newArgs);
        }


        private void DeletePreviousImage()
        {
            if (image != null)
            {
                RemoveLogicalChild(image);
                image = null;
            }

            if (gifAnimation != null)
            {
                RemoveLogicalChild(gifAnimation);
                gifAnimation = null;
            }
        }

        private void CreateNonGifAnimationImage()
        {
            image = new Image();
            image.ImageFailed += image_ImageFailed;
            var src = (ImageSource) new ImageSourceConverter().ConvertFromString(Source);
            image.Source = src;
            image.Stretch = Stretch;
            image.StretchDirection = StretchDirection;
            AddChild(image);
        }


        private void CreateGifAnimation(MemoryStream memoryStream)
        {
            gifAnimation = new GifAnimation();
            gifAnimation.CreateGifAnimation(memoryStream);
            gifAnimation.Stretch = Stretch;
            gifAnimation.StretchDirection = StretchDirection;
            AddChild(gifAnimation);
        }


        private void CreateFromSourceString(string source)
        {
            DeletePreviousImage();
            Uri uri;

            try
            {
                uri = new Uri(source, UriKind.RelativeOrAbsolute);
            }
            catch (Exception exp)
            {
                RaiseImageFailedEvent(exp);
                return;
            }

            if (source.Trim().ToUpper().EndsWith(".GIF", StringComparison.OrdinalIgnoreCase) || ForceGifAnimation)
            {
                if (!uri.IsAbsoluteUri)
                {
                    GetGifStreamFromPack(uri);
                }
                else
                {
                    var leftPart = uri.GetLeftPart(UriPartial.Scheme);
                    if ("http://".Equals(leftPart) || "ftp://".Equals(leftPart) || "file://".Equals(leftPart))
                        GetGifStreamFromHttp(uri);
                    else if ("pack://".Equals(leftPart))
                        GetGifStreamFromPack(uri);
                    else
                        CreateNonGifAnimationImage();
                }
            }
            else
            {
                CreateNonGifAnimationImage();
            }
        }

        private void WebRequestFinished(MemoryStream memoryStream)
        {
            CreateGifAnimation(memoryStream);
        }

        private void WebRequestError(Exception exp)
        {
            RaiseImageFailedEvent(exp);
        }

        private void WebResponseCallback(IAsyncResult asyncResult)
        {
            var webReadState = (WebReadState) asyncResult.AsyncState;
            WebResponse webResponse;
            try
            {
                webResponse = webReadState.webRequest.EndGetResponse(asyncResult);
                webReadState.readStream = webResponse.GetResponseStream();
                webReadState.buffer = new byte[100000];
                webReadState.readStream.BeginRead(webReadState.buffer, 0, webReadState.buffer.Length, WebReadCallback,
                    webReadState);
            }
            catch (WebException exp)
            {
                Dispatcher.Invoke(DispatcherPriority.Render, new WebRequestErrorDelegate(WebRequestError), exp);
            }
        }

        private void WebReadCallback(IAsyncResult asyncResult)
        {
            var webReadState = (WebReadState) asyncResult.AsyncState;
            var count = webReadState.readStream.EndRead(asyncResult);
            if (count > 0)
            {
                webReadState.memoryStream.Write(webReadState.buffer, 0, count);
                try
                {
                    webReadState.readStream.BeginRead(webReadState.buffer, 0, webReadState.buffer.Length,
                        WebReadCallback, webReadState);
                }
                catch (WebException exp)
                {
                    Dispatcher.Invoke(DispatcherPriority.Render, new WebRequestErrorDelegate(WebRequestError), exp);
                }
            }
            else
            {
                Dispatcher.Invoke(DispatcherPriority.Render, new WebRequestFinishedDelegate(WebRequestFinished),
                    webReadState.memoryStream);
            }
        }

        private void GetGifStreamFromHttp(Uri uri)
        {
            try
            {
                var webReadState = new WebReadState();
                webReadState.memoryStream = new MemoryStream();
                webReadState.webRequest = WebRequest.Create(uri);
                webReadState.webRequest.Timeout = 10000;

                webReadState.webRequest.BeginGetResponse(WebResponseCallback, webReadState);
            }
            catch (SecurityException)
            {
                CreateNonGifAnimationImage();
            }
        }


        private void ReadGifStreamSynch(Stream s)
        {
            byte[] gifData;
            MemoryStream memoryStream;
            using (s)
            {
                memoryStream = new MemoryStream((int) s.Length);
                var br = new BinaryReader(s);
                gifData = br.ReadBytes((int) s.Length);
                memoryStream.Write(gifData, 0, (int) s.Length);
                memoryStream.Flush();
            }

            CreateGifAnimation(memoryStream);
        }

        private void GetGifStreamFromPack(Uri uri)
        {
            try
            {
                StreamResourceInfo streamInfo;

                if (!uri.IsAbsoluteUri)
                {
                    streamInfo = Application.GetContentStream(uri);
                    if (streamInfo == null) streamInfo = Application.GetResourceStream(uri);
                }
                else
                {
                    if (uri.GetLeftPart(UriPartial.Authority).Contains("siteoforigin"))
                    {
                        streamInfo = Application.GetRemoteStream(uri);
                    }
                    else
                    {
                        streamInfo = Application.GetContentStream(uri);
                        if (streamInfo == null) streamInfo = Application.GetResourceStream(uri);
                    }
                }

                if (streamInfo == null) throw new FileNotFoundException("Resource not found.", uri.ToString());
                ReadGifStreamSynch(streamInfo.Stream);
            }
            catch (Exception exp)
            {
                RaiseImageFailedEvent(exp);
            }
        }

        private delegate void WebRequestFinishedDelegate(MemoryStream memoryStream);

        private delegate void WebRequestErrorDelegate(Exception exp);
    }
}