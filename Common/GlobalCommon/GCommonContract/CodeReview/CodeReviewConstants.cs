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
namespace AvePoint.GCommon.Contract.CodeReview
{

    public class CodeReviewConstants
    {
        //Business Logic
        ///<summary>
        ///​代码中是否存在明显的业务逻辑方面的错误, 会导致程序无法按照期望的行为运行.
        /// </summary>
        public const string CHECK_LIST_ID_BL_1 = "BL-1";

        ///<summary>
        ///​在实现具体的业务逻辑中使用的一些​外部API, 是否会导致出现程序Hang或操作没有响应等影响功能效率的情况.
        /// </summary>
        public const string CHECK_LIST_ID_BL_2 = "BL-2";

        ///<summary>
        ///​​是否存在在业务逻辑中Log输出不合理的地方
        /// </summary>
        [Obsolete("Using Log Logic Instead")]
        public const string CHECK_LIST_ID_BL_3 = "BL-3";


        //Code Optimization
        ///<summary>
        ///​字符串比较语句不应该使用"==", 而应该使用"equals"
        /// </summary>
        public const string CHECK_LIST_ID_CO_1 = "CO-1";

        ///<summary>
        ///在for, foreach, while等循环语句中, 如果有频繁拼装字符串时, 是否使用了StringBuffer/StringBuilder
        /// </summary>
        public const string CHECK_LIST_ID_CO_2 = "CO-2";

        ///<summary>
        ///是否存在可以合并的循环或条件判断
        /// </summary>
        public const string CHECK_LIST_ID_CO_3 = "CO-3";

        ///<summary>
        ///代码中不应该使用GOTO语句
        /// </summary>
        public const string CHECK_LIST_ID_CO_4 = "CO-4";

        ///<summary>
        ///是否存在if嵌套可以转换成switch嵌套
        /// </summary>
        public const string CHECK_LIST_ID_CO_5 = "CO-5";

        ///<summary>
        ///代码是否避免了Runtime Exception (数组越界, 被0除, 值越界, 堆栈溢出, 等等…)
        /// </summary>
        public const string CHECK_LIST_ID_CO_6 = "CO-6";

        ///<summary>
        ///代码是否避免了陷入死循环, 无穷递归
        /// </summary>
        public const string CHECK_LIST_ID_CO_7 = "CO-7";

        ///<summary>
        ///类或方法是否过于巨大, 是否使用了不必要的过于复杂的实现
        /// </summary>
        public const string CHECK_LIST_ID_CO_8 = "CO-8";

        ///<summary>
        ///是否存在过深的层次调用
        /// </summary>
        public const string CHECK_LIST_ID_CO_9 = "CO-9";

        ///<summary>
        ///是否代码中存在不必要的代码, 如强类型转换, 等等...
        /// </summary>
        public const string CHECK_LIST_ID_CO_10 = "CO-10";

        ///<summary>
        ///方法中是否存在过多的Return分支, 建议最多不能超过3个
        /// </summary>
        public const string CHECK_LIST_ID_CO_11 = "CO-11";

        ///<summary>
        ///对于一些全局变量是否一定有必要使用, 是否可以通过参数或者局部变量代替
        /// </summary>
        public const string CHECK_LIST_ID_CO_12 = "CO-12";


        //Socket
        ///<summary>
        ///任何设计到Socket的操作, 都必须在使用之后, 进行相关的关闭操作
        /// </summary>
        public const string CHECK_LIST_ID_SOCKET_1 = "SOCKET-1";

        ///<summary>
        ///Socket是否具有合适的超时(Timeout)处理逻辑
        /// </summary>
        public const string CHECK_LIST_ID_SOCKET_2 = "SOCKET-2";


        //Security
        ///<summary>
        ///是否客户的敏感信息(密码), 经过了加密/解密的处理
        /// </summary>
        public const string CHECK_LIST_ID_SECURITY_1 = "SECURITY-1";

        ///<summary>
        ///无论密码是明文还是密文, 都不可能Road到GUI前台
        /// </summary>
        public const string CHECK_LIST_ID_SECURITY_2 = "SECURITY-2";


        //Exception Handling
        ///<summary>
        ///是否使用了"异常处理逻辑"而不是用"返回错误状态"或"错误代码"去处理程序中的错误.
        /// </summary>
        public const string CHECK_LIST_ID_EH_1 = "EH-1";

        ///<summary>
        ///是否在异常处理逻辑中, 添加了将错误消息输出到Log的逻辑.
        /// </summary>
        public const string CHECK_LIST_ID_EH_2 = "EH-2";

        ///<summary>
        ///在业务逻辑中, 是否对代码中可能存在异常的逻辑中添加了"异常处理逻辑". 同时必须明确这个"异常处理逻辑"对业务逻辑执行的影响:
        ///1. 如果业务逻辑需要继续执行, 首先处理好异常, 以便让业务逻辑可以继续执行, 然后考虑如何把异常体现在Job Report中, 以便让客户了解到程序运行过程中出现了异常.
        ///2. 如果业务逻辑需要终止执行, 需要做好程序的收尾工作, 比如: 释放占用的资源, 更新Job的状态, 等等...
        /// </summary>
        public const string CHECK_LIST_ID_EH_3 = "EH-3";

        ///<summary>
        ///是否存在业务逻辑中捕获了异常, 但是异常逻辑中没有做任何操作处理的地方
        /// </summary>
        public const string CHECK_LIST_ID_EH_4 = "EH-4";

        ///<summary>
        ///对于只捕获一个Common的”System.Exception”且没有对特定Exception进行处理的逻辑, 需要加入FxCop自定义检查中, 确保异常捕获逻辑中至少要捕获2个Exception.
        /// </summary>
        public const string CHECK_LIST_ID_EH_5 = "EH-5";

        //Database
        ///<summary>
        ///SQL语句中不应该存在SQL注入, 即: SQL语句中如果带有参数, 不应该通过拼装字符串的方式组合SQL语句, 而应该使用参数化查询
        /// </summary>
        public const string CHECK_LIST_ID_DB_1 = "DB-1";

        ///<summary>
        ///using语句块内的业务逻辑要尽量的简单, 执行时间不要太长, 以免出现占有DB事务锁时间过长的情况.
        /// </summary>
        public const string CHECK_LIST_ID_DB_2 = "DB-2";

        ///<summary>
        ///执行SQL语句的时候, 从SQL Server 返回的结果条目要尽量的少, 尽量不要把结果拿到control端在进行过滤和处理, 当然这种情况不是绝对的, 如果可以的话, 最好这么做, 但如果这么做会导致业务逻辑变的更加复杂的话, 也可以根据实际情况决定使用哪种实现方式更加合理.
        /// </summary>
        public const string CHECK_LIST_ID_DB_3 = "DB-3";

        ///<summary>
        ///如果没有特殊的需求, 务必确保查询数据库的SQL语句中都要带有"with nolock"关键字.
        /// </summary>
        public const string CHECK_LIST_ID_DB_4 = "DB-4";

        ///<summary>
        ///SQL语句中存在IN、OR等关键字时，务必确保在大数据环境下代码能正确执行，避免出现SQL语句中Filter的逻辑过长，语句在执行过程中出现堆栈溢出的情况.
        /// </summary>
        public const string CHECK_LIST_ID_DB_5 = "DB-5";

        //File
        ///<summary>
        ///临时文件使用完后是否及时释放或删除了
        /// </summary>
        public const string CHECK_LIST_ID_FILE_1 = "FILE-1";


        //Framework/Architecture
        ///<summary>
        ///是否存在废弃的源代码, 配置等文件没有在SVN中清理或删除
        /// </summary>
        public const string CHECK_LIST_ID_FA_1 = "FA-1";

        ///<summary>
        ///模块(类, 方法)之间是否具有低耦合性
        /// </summary>
        public const string CHECK_LIST_ID_FA_2 = "FA-2";

        ///<summary>
        ///模块(类, 方法)自身是否具有高内聚性
        /// </summary>
        public const string CHECK_LIST_ID_FA_3 = "FA-3";

        ///<summary>
        ///是否存在过多(大段的)重复代码
        /// </summary>
        public const string CHECK_LIST_ID_FA_4 = "FA-4";

        ///<summary>
        ///类的设计和抽象是否合适, 是否符合面向接口编程的思想.
        /// </summary>
        public const string CHECK_LIST_ID_FA_5 = "FA-5";

        ///<summary>
        ///是否采用了合适的设计模式
        /// </summary>
        public const string CHECK_LIST_ID_FA_6 = "FA-6";

        ///<summary>
        ///包结构/命名空间设计是否合理
        /// </summary>
        public const string CHECK_LIST_ID_FA_7 = "FA-7";

        ///<summary>
        ///代码中的实现技术是否便于Unit Test
        /// </summary>
        public const string CHECK_LIST_ID_FA_8 = "FA-8";

        ///<summary>
        ///​是否存在不再引用的变量/命名空间没有在源代码中注释或删除
        /// </summary>
        public const string CHECK_LIST_ID_FA_9 = "FA-9";

        ///<summary>
        ///是否存在废弃的代码段没有在源代码中注释或删除
        /// </summary>
        public const string CHECK_LIST_ID_FA_10 = "FA-10";


        //Stream
        ///<summary>
        ///在代码当中, 使用的任何Stream的派生类 (包括但不限于): 
        ///System.IO.BufferedStream   
        ///System.IO.Compression.DeflateStream   
        ///System.IO.Compression.GZipStream    
        ///System.IO.FileStream    
        ///System.IO.MemoryStream    
        ///System.IO.UnmanagedMemoryStream    
        ///System.Net.Security.AuthenticatedStream   
        ///System.Net.Sockets.NetworkStream    
        ///System.Security.Cryptography.CryptoStream   
        ///都必须在使用之后, 进行相关的关闭(Close)/释放(Dispose)操作
        /// </summary>
        public const string CHECK_LIST_ID_STREAM_1 = "STREAM-1"; 


        //Hard Code
        ///<summary>
        ///是否存在应该定义为常量的数字, 字符, 字符串
        /// </summary>
        public const string CHECK_LIST_ID_HC_1 = "HC-1";

        ///<summary>
        ///代码中涉及到的常量的定义是否易于读取/修改/配置/维护 (如, 是否需要使用专门的常量类, 枚举类来定义可读性较差的数值型"标志位", 等等...)
        /// </summary>
        public const string CHECK_LIST_ID_HC_2 = "HC-2";


        //Thread
        ///<summary>
        ///(Thread) ​是否有必要使用多线程
        ///建议使用多线程的逻辑:
        ///1. 有同时进行的, 有I/O Block的操作
        ///2. 涉及到用户体验, 界面不能卡死(Hang)的操作
        ///3. 有大量的任务, 如果顺序执行需要较长时间的等待, 且可以同时进行的操作
        ///不建议或者没必要使用多线程的地方逻辑:
        ///1. 对于多线程操作中, 如果每个线程的逻辑都会消耗大量CPU资源的话, 这种情况是不建议使用多线程的, 因为: 
        ///在单核的情况下, 这种情况多线程并不能带来任何好处, 反而会使速度降低. 即使在多核的情况下, 线程数量不能超过核的数量, 否则性能也会开始下降
        /// </summary>
        public const string CHECK_LIST_ID_THREAD_1 = "THREAD-1";

        ///<summary>
        ///需要检查所有和线程相关的全局对象, 确认线程操作该对象时是否是线程安全的
        /// </summary>
        public const string CHECK_LIST_ID_THREAD_2 = "THREAD-2";

        ///<summary>
        ///当不使用多线程操作的时候, 没有必要使用过大的缓存
        /// </summary>
        public const string CHECK_LIST_ID_THREAD_3 = "THREAD-3";

        ///<summary>
        ///对于多线程操作相关的业务功能逻辑, 需要在事务的角度上看多线程会不会影响到整个任务的状态, 而不能仅仅看全局对象是否有同步问题
        /// </summary>
        public const string CHECK_LIST_ID_THREAD_4 = "THREAD-4";

        ///<summary>
        ///当多线程出现异常后, 要有异常处理逻辑, 保证线程不会异常退出.
        /// </summary>
        public const string CHECK_LIST_ID_THREAD_5 = "THREAD-5";

        ///<summary>
        ///线程对系统资源的分配要合理. 要控制对CPU内核的使用限制. 对线程个数应该是可配置的.
        /// </summary>
        public const string CHECK_LIST_ID_THREAD_6 = "THREAD-6";


        
        //Coding Standard
        ///<summary>
        ///模块(类, 方法)中是否具有清晰的注释
        /// </summary>
        public const string CHECK_LIST_ID_CS_1 = "CS-1";

        ///<summary>
        ///模块(类, 方法, 属性)名是否按照命名规范起了具有特殊意义的名字
        /// </summary>
        public const string CHECK_LIST_ID_CS_2 = "CS-2"; 

        ///<summary>
        ///模块(类, 方法, 属性)名不应该使用Java的命名方式. 应该使用正规的C#命名方式
        /// </summary>
        public const string CHECK_LIST_ID_CS_3 = "CS-3"; 

        ///<summary>
        ///方法名中是否存在:
        ///void getXXX()->get方法没有返回值
        ///Object setXXX() ->set方法有返回值
        ///等类似的命名方式
        /// </summary>
        public const string CHECK_LIST_ID_CS_4 = "CS-4";

        ///<summary>
        ///代码中会显示到GUI前台或者会被客户看到的任何Report中的字符串, 相应字符串必须按照国际化的流程书写成正规的国际化词条的格式.
        /// </summary>
        public const string CHECK_LIST_ID_CS_5 = "CS-5"; 


        //Validating Logic
        ///<summary>
        ///是否使用了参数验证逻辑去处理方法(Method)或函数(Function)​中不合法的(Invalid)参数.
        /// </summary>
        public const string CHECK_LIST_ID_VL_1 = "VL-1";


        //GUI Logic
        ///<summary>
        ///注册的事件, 要有相应的注销逻辑
        /// </summary>
        public const string CHECK_LIST_ID_GUI_1 = "GUI-1";

        ///<summary>
        ///​任何前台验证都必须有对应的后台验证
        /// </summary>
        public const string CHECK_LIST_ID_GUI_2 = "GUI-2";

        ///<summary>
        ///​所有的控件和元素都必须有ID, 且是唯一的
        /// </summary>
        public const string CHECK_LIST_ID_GUI_3 = "GUI-3";

        ///<summary>
        ///自定义控件的部件, 状态, 状态组的元数据必须进行添加
        /// </summary>
        public const string CHECK_LIST_ID_GUI_4 = "GUI-4";

        ///<summary>
        ///​部件的每次使用, 必须判定是否为null
        /// </summary>
        public const string CHECK_LIST_ID_GUI_5 = "GUI-5";

        ///<summary>
        ///如果视觉呈现, 与CommonStates 和 FocusStates状态组中的状态相同, 不要重新定义状态组和状态
        /// </summary>
        public const string CHECK_LIST_ID_GUI_6 = "GUI-6";

        ///<summary>
        ///子类的状态应该添加在新的状态组中
        /// </summary>
        public const string CHECK_LIST_ID_GUI_7 = "GUI-7";

        ///<summary>
        ///​暴露给控件使用者的属性, 应该是依赖属性, 否则不会支持数据绑定
        /// </summary>
        public const string CHECK_LIST_ID_GUI_8 = "GUI-8";

        ///<summary>
        ///​自定义控件是否严格分离了控件逻辑和控件的视觉呈现
        /// </summary>
        public const string CHECK_LIST_ID_GUI_9 = "GUI-9";

        ///<summary>
        ///应该尽量使用数据绑定的方式进行页面数据的收集和设置
        /// </summary>
        public const string CHECK_LIST_ID_GUI_10 = "GUI-10";


        //SharePoint API
        ///<summary>
        ///在使用SharePoint API时, 是否使用了不当的方式/方法, 会导致出现影响/降低程序运行效率的情况.
        /// </summary>
        public const string CHECK_LIST_ID_SHAREPOINT_1 = "SHAREPOINT-1";

        ///<summary>
        ///​​在使用SharePoint API时, 是否使用了不当的方式/方法, 会导致程序资源或内存出现泄漏/溢出的情况或隐患.
        /// </summary>
        public const string CHECK_LIST_ID_SHAREPOINT_2 = "SHAREPOINT-2";


        //Wrapper API
        ///<summary>
        ///在使用Wrapper API时, 是否使用了不当的方式/方法, 会导致出现影响/降低程序运行效率的情况.
        /// </summary>
        public const string CHECK_LIST_ID_WRAPPER_1 = "WRAPPER-1";

        ///<summary>
        ///在使用Wrapper API时, 是否使用了不当的方式/方法, 会导致程序资源或内存出现泄漏/溢出的情况或隐患.
        /// </summary>
        public const string CHECK_LIST_ID_WRAPPER_2 = "WRAPPER-2";


        //Log Logic
        ///<summary>
        ///是否存在Log级别与输出消息级别不匹配的情况.
        /// </summary>
        public const string CHECK_LIST_ID_LOG_1 = "LOG-1";

        ///<summary>
        ///不要在循环操作逻辑中输出过多对诊断问题没有帮助的Log.
        /// </summary>
        public const string CHECK_LIST_ID_LOG_2 = "LOG-2";

        ///<summary>
        ///不允许出现输出的Log内容与实际的业务操作结果不符的情况.
        /// </summary>
        public const string CHECK_LIST_ID_LOG_3 = "LOG-3";

        ///<summary>
        ///​Log输出的内容要尽量详细, 要达到输出该Log的目的. 不要频繁输出对诊断问题没有帮助的Log.
        /// </summary>
        public const string CHECK_LIST_ID_LOG_4 = "LOG-4";

        ///<summary>
        ///需要输出到Event Viewer中的log,是否正确的使用了Event ID的log输出格式. 
        /// </summary>
        public const string CHECK_LIST_ID_LOG_5 = "LOG-5";

        ///<summary>
        ///​是否在应用层捕获并输出了底层抛出的Customized Exception.
        /// </summary>
        public const string CHECK_LIST_ID_LOG_6 = "LOG-6";
    }
}
