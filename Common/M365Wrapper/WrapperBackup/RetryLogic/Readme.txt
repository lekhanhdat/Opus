/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   Harborside Financial Center
 *                   9th Fl.   Plaza Ten
 *                   Jersey City, NJ 07311
 *                   United States of America
 *                   Telephone: +1-800-661-6588
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
 *  Copyright © 2017 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */
背景:
对于Exchange的备份还原过程, 使用EWS Managed API以及Exchange Web Service进行操作时, 有几率出现暂时性异常。
因此需要对Exchange的请求添加重连逻辑。

需求:
1. 集中处理异常信息并根据错误信息自动重连
2. 对于无法集中处理的异常信息, 在调用处添加重连逻辑
3. 重连逻辑分为三种:
	a. 等待-重连: 出错后等待一段时间再进行重连操作, 主要针对"Rate Limiting"系统过多请求, 或已知服务器端临暂时不可用的情况, 例如ErrorServerBusy, ErrorMailboxMoveInProgress
	b. 立即重连 : 出错后立即重连, 主要针对服务器端偶发性低概率错误, 通过立即重连增加系统健壮性, 例如ErrorInternalServerError
	c. 永不重连 : 出错后不再重连, 将错误返回给调用者处理, 主要针对已知原因的对Exchange不合理的请求, 例如ErrorAccessDenied, ErrorAccountDisabled

设计:
1. 集中处理, 这部分思路是通过截获Http层异常来处理, 目前只处理Exchange Server返回的错误, 即HttpStatusCode.InternalServerError(ErrorCode=500), 以后可以添加其他类型的错误处理
   由于具体的错误代码是存放在SOAP协议的Fault节点中, 因此只通过Http协议返回的错误代码无法准确判断错误类型, 需要解析SOAP协议ResponseStream来分析错误类型
   a. 通过扩展ExchangeService的RetryController接口来实现自动重连机制, 目前正在使用的方式。
   b. 所有操作ExchangeService的类必须直接或间接继承类ExchangeObjectBase, 并通过ExchangeObjectBase中提供的方法构建ExchangeService对象。
      构建ExchangeService的逻辑已经在ExchangeObjectBase中封装好, 主要包括认证和重连两部分。
   c. #a中方式的不足:
      (1). 无法处理批处理请求中部分数据失败的重连, 例如ExportItems中少数Item发生ErrorInternalServerError错误。
      (2). 重连的callback无法区分不同的请求类型。(考虑到会加大retry callback的复杂度, 暂时不支持)
2. 提供公共方法处理重连逻辑, 调用处调用公共方法不需要关心重连逻辑
3. 通过BackOffMilliseconds控制重连等待时间, 将ServiceError错误码与BackOffMilliseconds做映射, 重连逻辑通过解析到的ServiceError自动生成BackOffMilliseconds
   a. 等待-重连: BackOffMilliseconds>0
   b. 立即重连 : BackOffMilliseconds=0
   c. 永不重连 : BackOffMilliseconds<0
   目前在ServiceErrorExtension类中(Hard Code)集中控制ServiceError和BackOffMilliseconds的映射关系, 以后可以考虑迁移到配置文件中。