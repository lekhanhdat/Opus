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
namespace AvePoint.RAI.Core.Models;

/// <summary>
/// Configuration settings for chat completion requests.
/// Provides a provider-agnostic way to configure execution parameters.
/// </summary>
public record ChatCompletionSettings
{
    /// <summary>
    /// Maximum number of tokens to generate in the response.
    /// Default is 1000.
    /// </summary>
    public int MaxTokens { get; init; } = 5000;

    /// <summary>
    /// Controls randomness in the response. Range: 0.0 to 2.0.
    /// Lower values make output more focused and deterministic.
    /// Default is 0.7.
    /// </summary>
    public double Temperature { get; init; } = 0.7;

    /// <summary>
    /// Controls diversity via nucleus sampling. Range: 0.0 to 1.0.
    /// Lower values focus on more likely tokens.
    /// Default is 1.0.
    /// </summary>
    public double TopP { get; init; } = 1.0;

    /// <summary>
    /// Number of chat completion choices to generate for each input message.
    /// Default is 1.
    /// </summary>
    public int? ResultsPerPrompt { get; init; } = 1;

    /// <summary>
    /// Up to 4 sequences where the API will stop generating further tokens.
    /// </summary>
    public IReadOnlyList<string>? StopSequences { get; init; }

    /// <summary>
    /// Number between -2.0 and 2.0. Positive values penalize new tokens based on their existing frequency.
    /// Default is 0.
    /// </summary>
    public double FrequencyPenalty { get; init; } = 0.0;

    /// <summary>
    /// Number between -2.0 and 2.0. Positive values penalize new tokens based on whether they appear in the text so far.
    /// Default is 0.
    /// </summary>
    public double PresencePenalty { get; init; } = 0.0;

    /// <summary>
    /// Creates default settings optimized for general chat completion tasks.
    /// </summary>
    public static ChatCompletionSettings Default => new();

    /// <summary>
    /// Creates settings optimized for creative writing tasks.
    /// Higher temperature and TopP for more creative responses.
    /// </summary>
    public static ChatCompletionSettings Creative => new()
    {
        Temperature = 1.2,
        TopP = 0.95,
        MaxTokens = 2000
    };

    /// <summary>
    /// Creates settings optimized for analytical or factual tasks.
    /// Lower temperature for more focused, deterministic responses.
    /// </summary>
    public static ChatCompletionSettings Analytical => new()
    {
        Temperature = 0.2,
        TopP = 0.8,
        MaxTokens = 1500
    };

    /// <summary>
    /// Creates settings optimized for code generation tasks.
    /// Very low temperature for precise, deterministic code output.
    /// </summary>
    public static ChatCompletionSettings CodeGeneration => new()
    {
        Temperature = 0.1,
        TopP = 0.5,
        MaxTokens = 3000
    };
}
