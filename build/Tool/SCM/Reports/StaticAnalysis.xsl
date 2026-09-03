<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="html"/>
  <xsl:template match="/">
    <STYLE>
      BODY, TABLE, TD, TH, P {
      font-family:Segoe UI;
      font-size:12px;
      color:black;
      }

      TR.toplevel { background-color:#008B8B;}
      TD.toplevel { font-size:250%; color:white; text-align: left; padding-right: 1em; vertical-align: middle; }

      TD.side-panel { text-align: left; padding-right: 1em; vertical-align: top; }
      TD.Summary {font-size:150%; color:#008B8B; }
      TD.SummaryError {color:red;}
      TABLE.Summary {
      background-repeat: no-repeat;
      background-position-x: left;
      background-position-y: bottom;
      background-attachment:fixed;}

      TD.Details {font-size:150%; color:#008B8B; }
      TABLE.Details {font-size:120%; }

      TABLE.DetailsInfo {}
      TD.DetailsInfo {border-width:3px;border-color:#008B8B;border-style:solid;padding:5px;}
      TD.DetailsInfoTitleLeft {width:20%; font-size:120%; background-color:#008B8B; color:white;}
      TD.DetailsInfoTitleRight  {width:80%; font-size:120%; }
	  
	  .a{color:#FFFFFF}
	  .a:visited{color:#BBFFFF}
      .a:hover{color:#FFFF00}


    </STYLE>

    <xsl:variable name="ProjectInSolutionReport" select="/StaticCodeAnalyzerReport/ProjectInSolutionReport/ErrorSolutions//ErrorSolution" />
    <xsl:variable name="ProjectInSolutionReport.count" select="count($ProjectInSolutionReport)" />

    <xsl:variable name="ReferenceFileReport" select="/StaticCodeAnalyzerReport/ReferenceFileReport/ReferenceProjectErrors//ReferenceProjectError" />
    <xsl:variable name="ReferenceFileReport.count" select="count($ReferenceFileReport)" />

    <xsl:variable name="BuildModeReport" select="/StaticCodeAnalyzerReport/BuildModeReport/Errors//BuildModeErrorItem" />
    <xsl:variable name="BuildModeReport.count" select="count($BuildModeReport)" />

    <xsl:variable name="ProjectSettingsReport" select="/StaticCodeAnalyzerReport/ProjectSettingsReport/Errors//ProjectSettingsErrorItem" />
    <xsl:variable name="ProjectSettingsReport.count" select="count($ProjectSettingsReport)" />

    <xsl:variable name="ThirdDllReferencesReport" select="/StaticCodeAnalyzerReport/ThirdDllReferencesReport/Errors//ThirdDllReferencesErrorItem" />
    <xsl:variable name="ThirdDllReferencesReport.count" select="count($ThirdDllReferencesReport)" />

    <xsl:variable name="UnitTestReport" select="/StaticCodeAnalyzerReport/UnitTestReport/Errors//UnitTestErrorItem" />
    <xsl:variable name="UnitTestReport.count" select="count($UnitTestReport)" />

    <xsl:variable name="BuildConfigrationReport" select="/StaticCodeAnalyzerReport/BuildConfigrationReport/Errors//BuildConfigrationErrorItem" />
    <xsl:variable name="BuildConfigrationReport.count" select="count($BuildConfigrationReport)" />

    <xsl:variable name="MessyCodeReport" select="/StaticCodeAnalyzerReport/MessyCodeReport/Errors//MessyCodeErrorItem" />
    <xsl:variable name="MessyCodeReport.count" select="count($MessyCodeReport)" />

    <html>
      <body style="width:99%" >

        <table width="100%" height="10%" id="header" border="0" cellspacing="0" cellpadding="0">
          <tr class='toplevel'>
            <td width="5%">
            </td>
            <td class='toplevel' width="95%">
              Static Analysis Reports
            </td>
          </tr>
        </table>

        <table width="100%" height="6px" id="header" border="0" cellspacing="0" cellpadding="0">
          <tr>
            <td width="100%" height="2px">
            </td>
          </tr>
          <tr style="background-color:#008B8B;" >
            <td width="100%" height="2px">
            </td>
          </tr>
          <tr>
            <td width="100%" height="2px">
            </td>
          </tr>
        </table>


        <table width="100%" height="70%" id="main-table" class="Summary" border="0">
          <tbody width="100%">
            <tr width="100%">
              <td width="20%" class="side-panel">
                <table >
                  <tr>
                    <td colspan="2" class="Summary">Static Analysis Report Summary</td>
                  </tr>

                  <xsl:choose>
                    <xsl:when test="$ProjectInSolutionReport.count > 0">
                      <tr >
                        <td class="SummaryError">
                          <a href="#_ProjectInSolutionReport" target="_self">引用的工程没有加载到Solution中:</a>
                        </td>
                        <td class="SummaryError">
                          <xsl:value-of select="$ProjectInSolutionReport.count"/>
                        </td>
                      </tr>
                    </xsl:when>
                    <xsl:otherwise>
                      <tr>
                        <td>引用的工程没有加载到Solution中:</td>
                        <td>
                          <xsl:value-of select="$ProjectInSolutionReport.count"/>
                        </td>
                      </tr>
                    </xsl:otherwise>
                  </xsl:choose>
                  <xsl:choose>
                    <xsl:when test="$ReferenceFileReport.count > 0">
                      <tr >
                        <td class="SummaryError">
                          <a href="#_ReferenceFileReport" target="_self">工程引用文件的方式不正确:</a>
                        </td>
                        <td class="SummaryError">
                          <xsl:value-of select="$ReferenceFileReport.count"/>
                        </td>
                      </tr>
                    </xsl:when>
                    <xsl:otherwise>
                      <tr>
                        <td>工程引用文件的方式不正确:</td>
                        <td>
                          <xsl:value-of select="$ReferenceFileReport.count"/>
                        </td>
                      </tr>
                    </xsl:otherwise>
                  </xsl:choose>

                  <xsl:choose>
                    <xsl:when test="$BuildModeReport.count > 0">
                      <tr>
                        <td class="SummaryError">
                          <a href="#_BuildModeReport" target="_self">Target Platform设置错误:</a>
                        </td>
                        <td class="SummaryError">
                          <xsl:value-of select="$BuildModeReport.count"/>
                        </td>
                      </tr>
                    </xsl:when>
                    <xsl:otherwise>
                      <tr>
                        <td>Target Platform设置错误:</td>
                        <td>
                          <xsl:value-of select="$BuildModeReport.count"/>
                        </td>
                      </tr>
                    </xsl:otherwise>
                  </xsl:choose>

                  <xsl:choose>
                    <xsl:when test="$ProjectSettingsReport.count > 0">
                      <tr>
                        <td class="SummaryError">
                          <a href="#_ProjectSettingsReport" target="_self">Project设置错误:</a>
                        </td>
                        <td class="SummaryError">
                          <xsl:value-of select="$ProjectSettingsReport.count"/>
                        </td>
                      </tr>
                    </xsl:when>
                    <xsl:otherwise>
                      <tr>
                        <td>Project设置错误:</td>
                        <td>
                          <xsl:value-of select="$ProjectSettingsReport.count"/>
                        </td>
                      </tr>
                    </xsl:otherwise>
                  </xsl:choose>

                  <xsl:choose>
                    <xsl:when test="$ThirdDllReferencesReport.count > 0">
                      <tr>
                        <td class="SummaryError">
                          <a href="#_ThirdDllReferencesReport" target="_self">第三方文件引用位置错误:</a>
                        </td>
                        <td class="SummaryError">
                          <xsl:value-of select="$ThirdDllReferencesReport.count"/>
                        </td>
                      </tr>
                    </xsl:when>
                    <xsl:otherwise>
                      <tr>
                        <td>第三方文件引用位置错误:</td>
                        <td>
                          <xsl:value-of select="$ThirdDllReferencesReport.count"/>
                        </td>
                      </tr>
                    </xsl:otherwise>
                  </xsl:choose>

                  <xsl:choose>
                    <xsl:when test="$UnitTestReport.count > 0">
                      <tr>
                        <td class="SummaryError">
                          <a href="#_UnitTestReport" target="_self">编译Unit Test工程:</a>
                        </td>
                        <td class="SummaryError">
                          <xsl:value-of select="$UnitTestReport.count"/>
                        </td>
                      </tr>
                    </xsl:when>
                    <xsl:otherwise>
                      <tr>
                        <td>编译Unit Test工程:</td>
                        <td>
                          <xsl:value-of select="$UnitTestReport.count"/>
                        </td>
                      </tr>
                    </xsl:otherwise>
                  </xsl:choose>

                  <xsl:choose>
                    <xsl:when test="$BuildConfigrationReport.count > 0">
                      <tr>
                        <td class="SummaryError">
                          <a href="#_BuildConfigrationReport" target="_self">Configration设置错误:</a>
                        </td>
                        <td class="SummaryError">
                          <xsl:value-of select="$BuildConfigrationReport.count"/>
                        </td>
                      </tr>
                    </xsl:when>
                    <xsl:otherwise>
                      <tr>
                        <td>Configration设置错误:</td>
                        <td>
                          <xsl:value-of select="$BuildConfigrationReport.count"/>
                        </td>
                      </tr>
                    </xsl:otherwise>
                  </xsl:choose>
				  
				  <xsl:choose>
                    <xsl:when test="$MessyCodeReport.count > 0">
                      <tr>
                        <td class="SummaryError">
                          <a href="#_MessyCodeReport" target="_self">非英文字符检查:</a>
                        </td>
                        <td class="SummaryError">
                          <xsl:value-of select="$MessyCodeReport.count"/>
                        </td>
                      </tr>
                    </xsl:when>
                    <xsl:otherwise>
                      <tr>
                        <td>非英文字符检查:</td>
                        <td>
                          <xsl:value-of select="$MessyCodeReport.count"/>
                        </td>
                      </tr>
                    </xsl:otherwise>
                  </xsl:choose>

                </table>
              </td>
              <td width="80%" class="main-panel">
                <table class="Details" width="100%">
                  <tbody width="100%">
                    <tr width="100%">
                      <td width="100%" class="Details">Static Analysis Report Details</td>
                    </tr>

                    <xsl:if test="$ProjectInSolutionReport.count!=0">
                      <table width="100%">
                        <tr width="100%" class="DetailsInfoTitle">
                          <td class="DetailsInfoTitleLeft" id="_ProjectInSolutionReport">
                            <a class="a" href="https://git.avepoint.net/SCM/Documents/blob/master/CMPlan/transition-plan/scm-check-rules/referenced-project-not-in-solution.md">引用工程没有加载到Solution中</a>
                          </td>
                          <td class="DetailsInfoTitleRight">
                          </td>
                        </tr>
                        <tr width="100%">
                          <td class="DetailsInfo" colspan="2">
                            <xsl:for-each select="$ProjectInSolutionReport">
                              <font color="red">
                                <b>
                                  <xsl:value-of select="./@Value"/>
                                </b>
                              </font>
                              <br/>
                              <xsl:for-each select="./ErrorProjects//ErrorProject">
                                <font color="orange">
                                  <b>
                                    <xsl:value-of select="./@Value"/>
                                  </b>
                                </font>
                                <br/>
                                <xsl:for-each select="./ErrorReferentProjects//ErrorReferentProject">
                                  <xsl:value-of select="./@Value"/>
                                  <br/>
                                </xsl:for-each>
                              </xsl:for-each>
                            </xsl:for-each>
                          </td>
                        </tr>

                      </table>
                      <br/>
                      <br/>
                    </xsl:if>
                    <xsl:if test="$ReferenceFileReport.count!=0">

                      <table width="100%">
                        <tr width="100%" class="DetailsInfoTitle">
                          <td class="DetailsInfoTitleLeft" id="_ReferenceFileReport">
                            <a class="a" href="https://git.avepoint.net/SCM/Documents/blob/master/CMPlan/transition-plan/scm-check-rules/project-internal-file-reference.md">工程引用文件的方式不正确</a>
                          </td>
                          <td class="DetailsInfoTitleRight">
                          </td>
                        </tr>
                        <tr width="100%">
                          <td class="DetailsInfo" colspan="2">
                            <xsl:for-each select="$ReferenceFileReport">
                              <font color="red">
                                <xsl:value-of select="./@Value"/>
                              </font>
                              <br/>
                              <xsl:for-each select="./ReferenceFileErrors//ReferenceFileError">
                                <font>
                                  <xsl:value-of select="./@Value"/>
                                </font>
                                <br/>
                              </xsl:for-each>
                            </xsl:for-each>
                          </td>
                        </tr>

                      </table>
                      <br/>
                      <br/>
                    </xsl:if>

                    <xsl:if test="$BuildModeReport.count!=0">

                      <table width="100%">
                        <tr width="100%" class="DetailsInfoTitle">
                          <td class="DetailsInfoTitleLeft" id="_BuildModeReport">
                            <a class="a" href="https://git.avepoint.net/SCM/Documents/blob/master/CMPlan/transition-plan/scm-check-rules/target-platform.md">Target Platform设置错误</a>
                          </td>
                          <td class="DetailsInfoTitleRight">
                          </td>
                        </tr>
                        <tr width="100%">
                          <td class="DetailsInfo" colspan="2">
                            <xsl:if test="$BuildModeReport/@Type='PlatformMismatch'">
                              <b>Project的Platform与申请表信息不一致 ：</b>
                            </xsl:if>
                            <xsl:for-each select="$BuildModeReport">
                              <xsl:if test="./@Type='PlatformMismatch'">
                                <xsl:value-of select="./@Value"/>
                                <font color="green">
                                  Correct: "<xsl:value-of select="./@Correct"/>"
                                </font>
                                <font color="red">
                                  Current: "<xsl:value-of select="./@Current"/>"
                                </font>
                                <br/>
                              </xsl:if>
                            </xsl:for-each>
                            <xsl:if test="$BuildModeReport/@Type='PlatformError'">
                              <b>Project在申请表中有多个Platform值 ：</b>
                            </xsl:if>
                            <br/>
                            <xsl:for-each select="$BuildModeReport">
                              <xsl:if test="./@Type='PlatformError'">
                                <xsl:value-of select="./@Value"/>
                                <br/>
                              </xsl:if>
                            </xsl:for-each>

                            <xsl:if test="$BuildModeReport/@Type='NotInBinaryList'">
                              <b>编译的Project没有申请 ：</b>
                            </xsl:if>
                            <br/>
                            <xsl:for-each select="$BuildModeReport">
                              <xsl:if test="./@Type='NotInBinaryList'">
                                <xsl:value-of select="./@Value"/>
                                <br/>
                              </xsl:if>
                            </xsl:for-each>

                          </td>
                        </tr>
                      </table>
                      <br/>
                      <br/>
                    </xsl:if>

                    <xsl:if test="$ProjectSettingsReport.count!=0">
                      <table width="100%">
                        <tr width="100%" class="DetailsInfoTitle">
                          <td class="DetailsInfoTitleLeft" id="_ProjectSettingsReport">
                            <a class="a" href="https://git.avepoint.net/SCM/Documents/blob/master/CMPlan/transition-plan/scm-check-rules/build-configuration.md">Project设置错误</a>
                          </td>
                          <td class="DetailsInfoTitleRight">
                          </td>
                        </tr>
                        <tr width="100%">
                          <td class="DetailsInfo" colspan="2">
                            <xsl:if test="$ProjectSettingsReport/@Type='OutputPathMismatch'">
                              <b>Project的输出路径不正确 ：</b>
                              <br/>
                            </xsl:if>
                            <xsl:for-each select="$ProjectSettingsReport">
                              <xsl:if test="./@Type='OutputPathMismatch'">
                                <xsl:value-of select="./@Value"/>
                                <br/>
                              </xsl:if>
                            </xsl:for-each>
                            <xsl:if test="$ProjectSettingsReport/@Type='DebugTypeMismatch'">
                              <b>Project的DebugType不正确 ：</b>
                            </xsl:if>
                            <br/>
                            <xsl:for-each select="$ProjectSettingsReport">
                              <xsl:if test="./@Type='DebugTypeMismatch'">
                                <xsl:value-of select="./@Value"/>
                                <br/>
                              </xsl:if>
                            </xsl:for-each>

                            <xsl:if test="$ProjectSettingsReport/@Type='OptimizeMismatch'">
                              <b>Project的Optimize不正确 ：</b>
                            </xsl:if>
                            <br/>
                            <xsl:for-each select="$ProjectSettingsReport">
                              <xsl:if test="./@Type='OptimizeMismatch'">
                                <xsl:value-of select="./@Value"/>
                                <br/>
                              </xsl:if>
                            </xsl:for-each>

                          </td>
                        </tr>

                      </table>
                      <br/>
                      <br/>
                    </xsl:if>

                    <xsl:if test="$ThirdDllReferencesReport.count!=0">
                      <table width="100%">
                        <tr width="100%" class="DetailsInfoTitle">
                          <td class="DetailsInfoTitleLeft" id="_ThirdDllReferencesReport">
                            <a class="a" href="https://git.avepoint.net/SCM/Documents/blob/master/CMPlan/transition-plan/scm-check-rules/third-file-reference-location.md">第三方文件引用位置错误</a>
                          </td>
                          <td class="DetailsInfoTitleRight">
                          </td>
                        </tr>
                        <tr width="100%">
                          <td class="DetailsInfo" colspan="2">
                            <xsl:for-each select="$ThirdDllReferencesReport">
                              <font color="red">
                                <xsl:value-of select="./@Value"/>
                              </font>
                              <br/>
                              <xsl:for-each select="./CurrentThirdDllPathErrors//string">
                                <font>
                                  <xsl:value-of select="."/>
                                </font>
                                <br/>
                              </xsl:for-each>
                            </xsl:for-each>
                          </td>
                        </tr>

                      </table>
                      <br/>
                      <br/>
                    </xsl:if>

                    <xsl:if test="$UnitTestReport.count!=0">
                      <table width="100%">
                        <tr width="100%" class="DetailsInfoTitle">
                          <td class="DetailsInfoTitleLeft" id="_UnitTestReport">
                            <a class="a" href="https://git.avepoint.net/SCM/Documents/blob/master/CMPlan/transition-plan/scm-check-rules/unit-test.md">编译 Unit Test工程</a>
                          </td>
                          <td class="DetailsInfoTitleRight">
                          </td>
                        </tr>
                        <tr width="100%">
                          <td class="DetailsInfo" colspan="2">
                            <xsl:for-each select="$UnitTestReport">
                              <xsl:value-of select="./@Value"/>
                              <br/>
                            </xsl:for-each>
                          </td>
                        </tr>

                      </table>
                      <br/>
                      <br/>
                    </xsl:if>

                    <xsl:if test="$BuildConfigrationReport.count!=0">
                      <table width="100%">
                        <tr width="100%" class="DetailsInfoTitle">
                          <td class="DetailsInfoTitleLeft" id="_BuildConfigrationReport">
                            Configration设置错误
                          </td>
                          <td class="DetailsInfoTitleRight">
                          </td>
                        </tr>
                        <tr width="100%">
                          <td class="DetailsInfo" colspan="2">
							根据当前的Build Configuration，工具无法获取Build配置，请查看工程配置文件是否包含Configuration和Platform设置(<a href="https://git.avepoint.net/SCM/Documents/blob/master/CMPlan/transition-plan/basic-knowledge/sln-proj-structure.md">查看方法</a>)。如果有问题，请联系原来负责该产品的SCM负责人。
                            <xsl:for-each select="$BuildConfigrationReport">
                              <xsl:value-of select="./@ProjectPath"/>
                              <br/>
                            </xsl:for-each>
                          </td>
                        </tr>

                      </table>
                      <br/>
                      <br/>
                    </xsl:if>
					
					<xsl:if test="$MessyCodeReport.count!=0">
                      <table width="100%">
                        <tr width="100%" class="DetailsInfoTitle">
                          <td class="DetailsInfoTitleLeft" id="_MessyCodeReport">
                            <a class="a" href="https://git.avepoint.net/SCM/Documents/blob/master/CMPlan/transition-plan/scm-check-rules/messy-code.md">非英文字符检查</a>
                          </td>
                          <td class="DetailsInfoTitleRight">
                          </td>
                        </tr>
                        <tr width="100%">
                          <td class="DetailsInfo" colspan="2">
                            <xsl:for-each select="$MessyCodeReport">
                              <font color="red">
								  <xsl:value-of select="./@FilePath"/>
								  <br/>
							  </font>
							  <xsl:for-each select="./DetailInfos//MessyCodeDetailInfo">
								  <xsl:value-of select="./@LineCount"/>:<xsl:value-of select="./@MessyCode"/>
								  <br/>
							  </xsl:for-each>
							  <br/>
                            </xsl:for-each>
                          </td>
                        </tr>
                      </table>
                      <br/>
                      <br/>
                    </xsl:if>
                  </tbody>
                </table>
              </td>
            </tr>
          </tbody>
        </table>
      </body>
    </html>


  </xsl:template>
</xsl:stylesheet>