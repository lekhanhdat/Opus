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
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Native13NinTexWorkflowEntity;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace LS.SPWorkflowProcessor
{
    class NWWebRequestActionProcessor : NWActionProcessorBase
    {

        public NWWebRequestActionProcessor(NintexWFActionProcessor workflowActionProcessor)
            : base(workflowActionProcessor)
        {
            CLASSNAME = "#NintexLive";
        }

        protected override Image CreateImage()
        {
            return new Image
            {
                Src = "https://ec.nintex.com/EXT/V1/Icons?type=primary&serviceId=9dc6f65e-e1b0-4e41-ab90-bb5de39e44bd",
                x49x49 = 0,
                y49x49 = 0,
                x30x30 = 0,
                y30x30 = 0,
                x16x16 = 0,
                y16x16 = 0
            };
        }

        public override WorkflowAction UpgradeWorkflowAction(NWActionConfig nwActionConfig)
        {
            sourceConfig = nwActionConfig;
            actionId = Guid.NewGuid().ToString();
            var action = new WorkflowAction
            {
                Id = actionId,
                ClassName = CLASSNAME,
                Configuration = CreateConfiguration()
            };

            return action;
        }

        protected override List<Property> CreateProperties()
        {
            ActivityParameter mode = null;
            ActivityParameter userName = null;
            ActivityParameter password = null;
            ActivityParameter soapAction = null;
            ActivityParameter url = null;
            ActivityParameter content = null;
            ActivityParameter contentType = null;
            ActivityParameter outPut = null;
            ActivityParameter outPutStatus = null;
            ActivityParameter outputCookies = null;
            ActivityParameter outputHeaders = null;

            foreach (var para in sourceConfig.Parameters)
            {
                if (string.Equals(para.Name, "Mode", StringComparison.OrdinalIgnoreCase))
                {
                    mode = para;
                }
                else if (string.Equals(para.Name, "Username", StringComparison.OrdinalIgnoreCase))
                {
                    userName = para;
                }
                else if (string.Equals(para.Name, "Password", StringComparison.OrdinalIgnoreCase))
                {
                    password = para;
                }
                else if (string.Equals(para.Name, "SoapAction", StringComparison.OrdinalIgnoreCase))
                {
                    soapAction = para;
                }
                else if (string.Equals(para.Name, "Url", StringComparison.OrdinalIgnoreCase))
                {
                    url = para;
                }
                else if (string.Equals(para.Name, "Content", StringComparison.OrdinalIgnoreCase))
                {
                    content = para;
                }
                else if (string.Equals(para.Name, "ContentType", StringComparison.OrdinalIgnoreCase))
                {
                    contentType = para;
                }
                else if (string.Equals(para.Name, "Output", StringComparison.OrdinalIgnoreCase))
                {
                    outPut = para;
                }
                else if (string.Equals(para.Name, "OutputStatus", StringComparison.OrdinalIgnoreCase))
                {
                    outPutStatus = para;
                }
                else if (string.Equals(para.Name, "OutputCookies", StringComparison.OrdinalIgnoreCase))
                {
                    outputCookies = para;
                }
                else if (string.Equals(para.Name, "OutputHeaders", StringComparison.OrdinalIgnoreCase))
                {
                    outputHeaders = para;
                }

            }

            var p0 = new Property
            {
                DesignerType = "Text",
                DisplayName = "URL",
                ID = "p0",
                Parameters = new[]
                {
                    CreateWeburl(url)
                }
            };

            var p1 = new Property
            {
                DesignerType = "DisplayExpression",
                DisplayName = "Method",
                ID = "p1",
                Parameters = new[] { CreateRquestMetadataXml(mode, contentType, soapAction, sourceConfig.FieldReferences) }

            };

            var p2 = new Property
            {
                ID = "p2",
                DesignerType = "FileUpload",
                DisplayName = "Body",
                Parameters = new[]
                {
                    new Parameters
                    {
                        Name="InputFileContent",
                        Value = new ParametersValue
                        {
                             PrimitiveValue =new PrimitiveValue { Type = "String", Value = new Value { StringValue = "" } }
                        },
                        Description="Data enclosed in the body of the request. Not required for GET method. File should be encoded in UTF-8.",
                        Required=false,
                        DataType="Blob",
                        DesignerType= "FileUpload",
                        Direction="Input",
                        DependentOn="",
                        OriginalSelectedValue=""
                    }
                }
            };

            var p3 = GenerateP3Property();

            var p4 = GenerateP4Property();

            var p5 = new Property
            {
                ID = "p5",
                DesignerType = "Variable",
                DisplayName = "Store response content in",
                Parameters = new[]
                {
                    new Parameters
                    {
                        Name="OutputResult",
                        Value = new ParametersValue
                        {
                            Variable = string.IsNullOrEmpty(outPut.Variable.Name)?null:base.workflowActionProcessor.VariablesCacheManager.GetSimpleVariable(outPut.Variable),
                        },
                        Description="Select a variable to store the contents of the response to this request.",
                        DataType="String",
                        DesignerType="Variable",
                        Direction="Output",
                        DependentOn="",
                        OriginalSelectedValue=""
                    }
                }
            };

            var p6 = new Property
            {
                ID = "p6",
                DesignerType = "Variable",
                DisplayName = "Store http status code in",
                Parameters = new[]
                {
                    new Parameters
                    {
                        Name="OutputHttpStatus",
                        Value = new ParametersValue
                        {
                            Variable = string.IsNullOrEmpty(outPutStatus.Variable.Name)?null:base.workflowActionProcessor.VariablesCacheManager.GetSimpleVariable(outPutStatus.Variable),
                        },
                        Description="Select a variable to store the numeric http response code of this request.",
                        Required=false,
                        DataType="Int32",
                        DesignerType="Variable",
                        Direction="Output",
                        DependentOn="",
                        OriginalSelectedValue=""
                    }
                }
            };

            var p7 = new Property
            {
                ID = "p7",
                DesignerType = "Variable",
                DisplayName = "Store response headers in",
                Parameters = new[]
                {
                    new Parameters
                    {
                        Name="OutputResponseHeaders",
                        Value = new ParametersValue
                        {
                            Variable = string.IsNullOrEmpty(outputHeaders.Variable.Name)?null: base.workflowActionProcessor.VariablesCacheManager.GetSimpleVariable(outputHeaders.Variable),
                        },
                        Description="Select a variable to store each response header for this request.",
                        Required=false,
                        DataType="DynamicValue",
                        DefaultType="Array",
                        DesignerType="Variable",
                        Direction="Output",
                        DependentOn="",
                        OriginalSelectedValue=""

                    }
                }
            };

            var p8 = new Property
            {
                ID = "p8",
                DesignerType = "Variable",
                DisplayName = "Store response cookies in",
                Parameters = new[]
                {
                    new Parameters
                    {
                        Name="OutputResponseCookies",
                        Value = new ParametersValue
                        {
                            Variable = string.IsNullOrEmpty(outputCookies.Variable.Name)?null:base.workflowActionProcessor.VariablesCacheManager.GetSimpleVariable(outputCookies.Variable),
                        },
                        Description="Select a variable to store each response cookie for this request.",
                        Required=false,
                        DataType="DynamicValue",
                        DefaultType="Array",
                        DesignerType="Variable",
                        Direction="Output",
                        DependentOn="",
                        OriginalSelectedValue=""
                    }
                }
            };


            return new List<Property> { p0, p1, p2, p3, p4, p5, p6, p7, p8 };
        }


        protected override Configuration CreateConfiguration()
        {
            var configuration = base.CreateConfiguration();
            configuration.HelpKey = "NL9DC6F65EE1B04E41AB90BB5DE39E44BD";//不知道是什么含义
            configuration.Live = CreateLive();
            configuration.SubscriptionInfo = CreateSubscriptionInfo();
            return configuration;
        }

        private SubscriptionInfo CreateSubscriptionInfo()
        {
            //当前365站点的subscription信息
            return new SubscriptionInfo()
            {
                Type = "Free",
                ProductId = "WebRequest",
                EndDate = DateTime.Now.AddYears(1)
            };
        }

        private Live CreateLive()
        {
            return new Live
            {
                ServiceId = "9dc6f65e-e1b0-4e41-ab90-bb5de39e44bd",//不知道什么含义
                VersionId = "20141218061913",
                ProductId = "WebRequest"
            };
        }

        private Parameters CreateRquestMetadataXml(ActivityParameter mode, ActivityParameter contentType, ActivityParameter soapAction, NWFieldReference[] fieldReferences)
        {
            var stream = Assembly.GetCallingAssembly().GetManifestResourceStream("Wrapper.Workflow.Nintex.NinTexWorkflowUpgradeProcessor.NintexWorkflowActionProcessor.Integration.DisplayExpression.txt");
            if (stream == null)
            {
                throw new FileNotFoundException("DisplayExpression.txt");
            }
            using (var sr = new StreamReader(stream))
            {
                List<FormatValues> formatValuesList;
                return new Parameters
                {
                    Name = "InputWebRequestMetaDataXml",
                    Value = new ParametersValue
                    {
                        PrimitiveValue = new PrimitiveValue
                        {
                            Type = "String",
                            Value = new Value("0" + CreateMetadataCollection(mode, contentType, soapAction, fieldReferences, out formatValuesList)),
                            FormatValues = formatValuesList
                        }
                    },
                    Description = "The following types of authentication are supported: Anonymous and Basic.",
                    Required = true,
                    DataType = "String",
                    DesignerType = "DisplayExpression",
                    Direction = "Input",
                    DependentOn = "",
                    Properties = new ParametersProperties
                    {
                        DisplayExpression = sr.ReadToEnd()
                    }
                };
            }
        }


        private string CreateMetadataCollection(ActivityParameter mode, ActivityParameter contentType, ActivityParameter soapAction, NWFieldReference[] fieldReferences, out List<FormatValues> formatValuesList)
        {
            string customMethodValue = string.Empty;
            formatValuesList = new List<FormatValues>();

            if (!mode.PrimitiveValue.Value.Equals("GET", StringComparison.OrdinalIgnoreCase)
                && !mode.PrimitiveValue.Value.Equals("POST", StringComparison.OrdinalIgnoreCase)
                && !mode.PrimitiveValue.Value.Equals("SOAP11", StringComparison.OrdinalIgnoreCase)
                && !mode.PrimitiveValue.Value.Equals("SOAP12", StringComparison.OrdinalIgnoreCase))
            {
                customMethodValue = mode.PrimitiveValue.Value;
            }
            var index = 0;
            var metadata = new MetaDataCollection
            {
                MetaDatas = new[]
                {
                    new MetaData { Key="RequestMethod",Value = GenerateMetaDataKeyOrValue(mode.PrimitiveValue.Value, ref index) },
                    new MetaData { Key="ContentType",Value = (mode.PrimitiveValue.Value.Equals("POST", StringComparison.OrdinalIgnoreCase) || mode.PrimitiveValue.Value.Equals("PUT", StringComparison.OrdinalIgnoreCase)) ? GenerateMetaDataKeyOrValue(contentType.PrimitiveValue.Value,ref index) : "" },
                    new MetaData { Key="SoapAction",Value=GenerateMetaDataKeyOrValue(soapAction.PrimitiveValue.Value,ref index) },
                    new MetaData { Key="CustomMethod",Value=GenerateMetaDataKeyOrValue(customMethodValue,ref index) }
                }
            };



            #region create metadata related with headers
            foreach (var fieldReference in fieldReferences)
            {
                CheckUnsupportedNWFieldReference(fieldReference);
                var tmpMetadata = new MetaData { Key = GenerateMetaDataKeyOrValue(fieldReference.Name, ref index), Value = GenerateMetaDataKeyOrValue(fieldReference.Value, ref index) };
                var tmpMetadataArray = new MetaData[metadata.MetaDatas.Length + 1];
                metadata.MetaDatas.CopyTo(tmpMetadataArray, 0);
                tmpMetadataArray.SetValue(tmpMetadata, tmpMetadataArray.Length - 1);
                metadata.MetaDatas = tmpMetadataArray;
            }
            #endregion
            #region GenerateFormatValues
            GenerateFormatValues(mode.PrimitiveValue.Value, formatValuesList);
            if ((mode.PrimitiveValue.Value.Equals("POST", StringComparison.OrdinalIgnoreCase) || mode.PrimitiveValue.Value.Equals("PUT", StringComparison.OrdinalIgnoreCase)))
            {
                GenerateFormatValues(contentType.PrimitiveValue.Value, formatValuesList);
            }
            GenerateFormatValues(soapAction.PrimitiveValue.Value, formatValuesList);
            GenerateFormatValues(customMethodValue, formatValuesList);
            CreateFormatValuesList(fieldReferences, formatValuesList);
            #endregion

            return SerializerHelper.SerializeObjectToString(metadata);
        }

        private string GenerateMetaDataKeyOrValue(string fieldReferenceNameOrValue, ref int index)
        {
            string result = fieldReferenceNameOrValue;
            if (fieldReferenceNameOrValue.StartsWith("{Common:", StringComparison.OrdinalIgnoreCase)
                    || fieldReferenceNameOrValue.StartsWith("{ItemProperty:", StringComparison.OrdinalIgnoreCase))
            {
                result = string.Format("{{{0}}}", index);
                index++;
            }
            return result;
        }

        private List<FormatValues> CreateFormatValuesList(NWFieldReference[] fieldReferences, List<FormatValues> formatValuesList)
        {
            foreach (var fieldReference in fieldReferences)
            {
                CheckUnsupportedNWFieldReference(fieldReference);
                GenerateFormatValues(fieldReference.Name, formatValuesList);
                GenerateFormatValues(fieldReference.Value, formatValuesList);
            }
            return formatValuesList;
        }

        private Property GenerateP3Property()
        {
            var parameter = NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "Username", true);
            return new Property
            {
                ID = "p3",
                DesignerType = "Text",
                DisplayName = "Username",
                Parameters = new[]
                {
                    new Parameters
                    {
                        Name = "InputUsername",
                        Description="Specify a username if using Basic authentication. When username and password are specified, a value is generated for the required Authentication header.",
                        Value = new ParametersValue { PrimitiveValue = NWPrimitiveValueConverter.ConvertPrimitiveValue(parameter.PrimitiveValue, base.workflowActionProcessor, true) },
                        Required = false,
                        DataType = "String",
                        DesignerType = "Text",
                        Direction = "Input"
                    }
    }
            };
        }
        private Property GenerateP4Property()
        {
            var parameter = NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "Password", true);

            return new Property
            {
                ID = "p4",
                DesignerType = "Secure",
                DisplayName = "Password",

                Parameters = new Parameters[]
                    {
                        new Parameters
                        {
                            Name="InputPassword",
                            Description="Specify the password for the above username.",
                            Required=false,
                            DataType ="String",
                            DesignerType="Secure",
                            Direction = "Input",
                            Value = new ParametersValue
                            {
                                PrimitiveValue = NWPrimitiveValueConverter.ConvertPrimitiveValue(parameter.PrimitiveValue,base.workflowActionProcessor, false),
                            }

                        }
                 }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldReferenceNameOrValue">like {ItemProperty:FieldName} or {Common:WorkflowContext}</param>
        /// <returns></returns>
        private void GenerateFormatValues(string fieldReferenceNameOrValue, List<FormatValues> formatValuesList)
        {
            Regex regex = new Regex("(({ItemProperty:+).*?([}]))|(({Common:+).*?([}]))"); //查找包含{ItemProperty:XXXX}或者{Common:XXXX}格式的字符串
            var matchedStrList = regex.Matches(fieldReferenceNameOrValue);
            if (matchedStrList.Count > 0)
            {
                var tempFormatValues = new FormatValues
                {
                    SelectedValue = new SelectedValue
                    {

                    }
                };
                #region generate value string
                string valueString = string.Empty;
                string[] tempArrary = regex.Replace(fieldReferenceNameOrValue, "|").Split('|');
                int index = 0;
                for (int i = 0; i < tempArrary.Length; i++)
                {
                    if (i == tempArrary.Length - 1)
                    {
                        valueString = valueString + tempArrary[i];
                    }
                    else
                    {
                        valueString = valueString + tempArrary[i] + string.Format("{{{0}}}", index);
                        index++;
                    }
                }
                valueString = valueString.Replace("&amp;nbsp;", " "); //因为使用System.Web.HttpUtility.HtmlDecode无法正确转换这些转义符，所以采用replace的方式逐个替换
                valueString = valueString.Replace("&amp;amp;", "&");
                valueString = valueString.Replace("&amp;lt;", "<");
                valueString = valueString.Replace("&amp;gt;", ">");
                valueString = valueString.Replace("&quot;", "\"");
                #endregion

                foreach (Match matchedStr in matchedStrList)
                {
                    tempFormatValues.SelectedValue.PrimitiveValue = NWPrimitiveValueConverter.ConvertPrimitiveValue(matchedStr.Value, "String", base.workflowActionProcessor, true);
                }
                formatValuesList.Add(tempFormatValues);
            }
        }

        private void CheckUnsupportedNWFieldReference(NWFieldReference fieldReference)
        {
            if (fieldReference.Name.IndexOf("{WFConstant:", StringComparison.OrdinalIgnoreCase) >= 0
                || fieldReference.Value.IndexOf("{WFConstant:", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                throw new UnSupportedActionTypeException("Unsupported value type WorkflowConstant");
            }
            if ((fieldReference.Name.IndexOf("fn-", StringComparison.OrdinalIgnoreCase) >= 0 && fieldReference.Name.IndexOf("()", StringComparison.OrdinalIgnoreCase) >= 0)
                || (fieldReference.Value.IndexOf("fn-", StringComparison.OrdinalIgnoreCase) >= 0 && fieldReference.Value.IndexOf("()", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                throw new UnSupportedActionTypeException("Unsupported value type Inline Functions");
            }
        }

        private Parameters CreateWeburl(ActivityParameter url)
        {
            return new Parameters
            {
                Name = "InputWebRequestUrl",
                Value = new ParametersValue { PrimitiveValue = NWPrimitiveValueConverter.ConvertPrimitiveValue(url.PrimitiveValue, base.workflowActionProcessor, true) },
                Description = "URL of the remote server.",
                Required = true,
                DataType = "String",
                DesignerType = "Text",
                Direction = "Input",
                DependentOn = "",
                OriginalSelectedValue = ""
            };
        }
    }
}
