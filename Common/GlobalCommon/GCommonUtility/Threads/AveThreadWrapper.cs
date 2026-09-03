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




namespace AvePoint.GCommon.Utility
{
    #region using directives
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;
    #endregion

    /// <summary>
    /// provide start and stop thread method,and there is an inner thread to dump the stack trace
    ///  of each thread when the debug file exists.
    /// </summary>
    /// <example>
    /// public class AveThreadWrapperTestCase
    /// {
    ///     public static void Test()
    ///     {
    ///         AveThreadWrapper wrapper1 = AveThreadUtility.StartThread(Thread1, "Test1", "small job1");
    ///         AveThreadWrapper wrapper2 = AveThreadUtility.StartThread(Thread2, "Test2", "small job1");
    ///
    ///         //wrapper1.KeepRunning = false;
    ///
    ///         Thread.Sleep(20000);
    ///
    ///         //please create an empty file (DumpMonitoredThreads.avepoint) under driver C
    ///         //if you want to output the stack trace
    ///
    ///         AveThreadUtility.SafeStopThread(string.Empty, 20000, "exception");
    ///     }
    ///
    ///     public static void Thread1()
    ///     {
    ///         int i = 0;
    ///         AveThreadWrapper currentThreadWrapper = AveThreadUtility.CurrentThreadWrapper;
    ///         while (currentThreadWrapper.KeepRunning)
    ///         {
    ///             Console.WriteLine("Thread_" + i++);
    ///             Thread.Sleep(1000);
    ///         }
    ///         Console.WriteLine("Thread1 end");
    ///     }
    ///
    ///     public static void Thread2()
    ///     {
    ///         int i = 0;
    ///         AveThreadWrapper currentThreadWrapper = AveThreadUtility.CurrentThreadWrapper;
    ///         while (currentThreadWrapper.KeepRunning)
    ///         {
    ///             Console.WriteLine("Thread2_" + i++);
    ///             Thread.Sleep(1000);
    ///         }
    ///         Console.WriteLine("Thread2 end");
    ///     }
    /// }
    ///
    /// public class AveThreadPoolItemBaseTestCase : AveThreadPoolItemBase
    /// {
    ///     public AveThreadPoolItemBaseTestCase(string name)
    ///         : base(name)
    ///     {
    ///     }
    ///
    ///     public override void Run()
    ///     {
    ///         Console.WriteLine(Name + " sleep for 5s");
    ///         Thread.Sleep(5000);
    ///     }
    /// }
    ///
    /// public class AveThreadPoolTestCase
    /// {
    ///     public static void Test()
    ///     {
    ///         int workerThreads;
    ///         int completionPortThreads;
    ///
    ///         ThreadPool.GetMaxThreads(out workerThreads, out completionPortThreads);
    ///         Console.WriteLine(workerThreads + "\t" + completionPortThreads);
    ///
    ///         workerThreads = 5;
    ///         completionPortThreads = 10;
    ///         ThreadPool.SetMaxThreads(workerThreads, completionPortThreads);
    ///
    ///         ThreadPool.GetMaxThreads(out workerThreads, out completionPortThreads);
    ///         Console.WriteLine(workerThreads + "\t" + completionPortThreads);
    ///
    ///         for (int s = 1; s < 30; s++)
    ///         {
    ///             AveThreadPoolItemBaseTestCase testRunItem = new AveThreadPoolItemBaseTestCase(s.ToString());
    ///             AveThreadPoolRunner.RunThread(testRunItem);
    ///         }
    ///
    ///         Console.ReadLine();
    ///     }
    /// }
    /// </example>
    public class AveThreadWrapper
    {
        static AveLogger logger = AveLogger.GetInstance(typeof(AveThreadWrapper));

        ThreadStart threadStart;
        ParameterizedThreadStart parameterizedThreadStart;
        Thread innerThread;
        Object obj;
        String threadName;
        String groupId;
        Boolean isBackupgroup;
        Boolean keepRunning;
        int status;

        #region Constructer Methods

        public AveThreadWrapper(ThreadStart start, string threadName) : this(start, threadName, string.Empty, true) { }
        public AveThreadWrapper(ThreadStart start, string threadName, string groupId) : this(start, threadName, groupId, true) { }
        public AveThreadWrapper(ParameterizedThreadStart start, object obj, string threadName) : this(start, obj, threadName, string.Empty, true) { }
        public AveThreadWrapper(ParameterizedThreadStart start, object obj, string threadName, string groupId) : this(start, obj, threadName, groupId, true) { }
        public AveThreadWrapper(ThreadStart start, string threadName, string groupId, bool isBackground)
        {
            this.threadStart = start;
            this.parameterizedThreadStart = null;
            this.obj = null;
            this.threadName = threadName;
            this.groupId = groupId;
            this.isBackupgroup = isBackground;
            this.status = 0;
            this.Init();
        }

        public AveThreadWrapper(ParameterizedThreadStart start, object obj, string threadName, string groupId, bool isBackground)
        {
            this.threadStart = null;
            this.parameterizedThreadStart = start;
            this.obj = obj;
            this.threadName = threadName;
            this.groupId = groupId;
            this.isBackupgroup = isBackground;
            this.status = 0;
            this.Init();
        }

        #endregion

        #region Public Fields

        public string Name
        {
            get { return threadName; }
        }
        public string GroupId
        {
            get { return groupId; }
        }
        public int ManagedThreadId
        {
            get { return innerThread.ManagedThreadId; }
        }
        public bool KeepRunning
        {
            get { return keepRunning; }
            set { keepRunning = value; }
        }
        public bool IsAlive
        {
            get { return innerThread.IsAlive; }
        }
        /// <summary>
        /// 0-->初始化，还没有start
        /// 1-->正在运行。
        /// 2-->结束了。
        /// </summary>
        public int Status
        {
            get { return status; }
        }
        public string MethodName
        {
            get
            {
                if (threadStart != null)
                {
                    return string.Format("{0}.{1}", threadStart.Method.DeclaringType
                        , threadStart.Method.Name);
                }
                return string.Format("{0}.{1}", parameterizedThreadStart.Method.DeclaringType
                    , parameterizedThreadStart.Method.Name);
            }
        }

        #endregion

        #region Public Method

        public void Start()
        {
            keepRunning = true;
            if (threadStart != null)
            {
                innerThread.Start();
            }
            else
            {
                innerThread.Start(obj);
            }
        }

        public void Join()
        {
            this.innerThread.Join();
        }

        public bool Join(int millisecondsTimeout)
        {
            return this.innerThread.Join(millisecondsTimeout);
        }

        public bool Join(TimeSpan timeout)
        {
            return this.innerThread.Join(timeout);
        }

        public void Abort()
        {
            this.innerThread.Abort();
        }

        public void SafeStop(int millisecondsTimeout, string message)
        {
            //AveThreadUtility.SafeStopThread(this, millisecondsTimeout, message);
            this.KeepRunning = false;
            if (this.IsAlive)
            {
                try
                {
                    logger.Info(string.Format("wait for thread [{0}] exit. {1}", this.Name, message));
                    if (!this.Join(millisecondsTimeout))
                    {
                        logger.Info(string.Format("wait for thread [{0}] exit timeout, so abort it.{1}", this.Name, message));
                        this.Abort();
                    }
                }
                catch (Exception e)
                {
                    logger.Warn(e.ToString());
                }
            }
        }

        public void Stop(int millisecondsTimeout, string message, bool force)
        {
            if (force)
            {
                SafeStop(millisecondsTimeout, message);
            }
            else
            {
                Join();
            }
        }

        public void DumpThreadInfo()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("ThreadName:{0}, GroupId:{1}, ManagedThreadId:{2}, MethodName:{3}, IsAlive:{4}\r\n"
                    , threadName, groupId, ManagedThreadId, MethodName, IsAlive);

                if (IsAlive)
                {
                    //will come back later, just comment out it for upgrading .net purpose. Byron


                    //try
                    //{
                    //    innerThread.Suspend();
                    //    StackTrace trace = new StackTrace(innerThread, false);

                    //    sb.AppendFormat("StackTrace:\r\n{0}\r\n", trace.ToString());
                    //}
                    //catch (Exception ex)
                    //{
                    //    sb.AppendFormat("Get StackTrace Failed:\r\n{0}\r\n", ex.ToString());
                    //}
                    //finally
                    //{
                    //    try
                    //    {
                    //        innerThread.Resume();
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        sb.AppendFormat("Resume Thread Failed:\r\n{0}\r\n", ex.ToString());
                    //    }
                    //}
                }

                logger.Debug(sb.ToString());

#if DEBUG
                Console.WriteLine(sb.ToString());
#endif
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred in dump thread:{0} info. Exception:{1}", threadName, ex.ToString());
            }
        }

        #endregion

        #region private methods
        private void Init()
        {
            if (threadStart != null)
            {
                innerThread = new Thread(ThreadStartWrapper);
            }
            else
            {
                innerThread = new Thread(ParameterizedThreadStartWrapper);
            }

            innerThread.IsBackground = isBackupgroup;
            if (!string.IsNullOrEmpty(threadName))
            {
                innerThread.Name = threadName;
            }
        }

        private void ThreadStartWrapper()
        {
            try
            {
                this.status = 1;
                threadStart();
            }
            catch (Exception ex)
            {
                logger.Error("An exception occurred in thread:{0}, exception:{1}", threadName, ex.ToString());
            }
            finally
            {
                this.status = 2;
            }
        }

        private void ParameterizedThreadStartWrapper(object obj)
        {
            try
            {
                this.status = 1;
                parameterizedThreadStart(obj);
            }
            catch (Exception ex)
            {
                logger.Error("An exception occurred in thread:{0}, exception:{1}", threadName, ex.ToString());
            }
            finally
            {
                this.status = 2;
            }
        }
        #endregion
    }
}
