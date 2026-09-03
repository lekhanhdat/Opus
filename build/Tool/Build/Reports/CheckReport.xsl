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

      TR.toplevel { background-color:#800786;}
      TD.toplevel { font-size:250%; color:white; text-align: left; padding-right: 1em; vertical-align: middle; }

      TD.side-panel { text-align: left; padding-right: 1em; vertical-align: top; }
      TD.Summary {font-size:150%; color:#800786; }
      TD.SummaryError {color:red;}
      TABLE.Summary {
      background-repeat: no-repeat;
      background-position-x: left;
      background-position-y: bottom;
      background-attachment:fixed;}

      TD.Details {font-size:150%; color:#800786; }
      TABLE.Details {font-size:120%; }

      TABLE.DetailsInfo {}
      TD.DetailsInfo {border-width:3px;border-color:#800786;border-style:solid;padding:5px;}
      TD.DetailsInfoTitleLeft {width:20%; font-size:120%; background-color:#800786; color:white;}
      TD.DetailsInfoTitleRight  {width:80%; font-size:120%; }
	  
	  .a{color:#FFFFFF}
	  .a:visited{color:#BBFFFF}
      .a:hover{color:#FFFF00}


    </STYLE>

    <xsl:variable name="PI.NotInPackageFiles" select="/PackageFilesCheckerReport/IntegrityReport/NotInPackage//IntegrityFile" />
    <xsl:variable name="PI.NotInPackageFiles.count" select="count($PI.NotInPackageFiles)" />
    <xsl:variable name="PI.NotInBinaryListFiles" select="/PackageFilesCheckerReport/IntegrityReport/NotInBinaryList//IntegrityFile" />
    <xsl:variable name="PI.NotInBinaryListFiles.count" select="count($PI.NotInBinaryListFiles)" />
    <xsl:variable name="FP.CopyrightReport" select="/PackageFilesCheckerReport/FilePropertiesReport/CopyrightReport//CopyrightErrorFile" />
    <xsl:variable name="FP.CopyrightReport.count" select="count($FP.CopyrightReport)" />
    <xsl:variable name="FP.CompanyReport" select="/PackageFilesCheckerReport/FilePropertiesReport/CompanyReport//CompanyErrorFile" />
    <xsl:variable name="FP.CompanyReport.count" select="count($FP.CompanyReport)" />
    <xsl:variable name="FP.ProductNameReport" select="/PackageFilesCheckerReport/FilePropertiesReport/ProductNameReport//ProductNameErrorFile" />
    <xsl:variable name="FP.ProductNameReport.count" select="count($FP.ProductNameReport)" />
    <xsl:variable name="FP.VersionReport" select="/PackageFilesCheckerReport/FilePropertiesReport/VersionReport//VersionErrorFile" />
    <xsl:variable name="FP.VersionReport.count" select="count($FP.VersionReport)" />
    <xsl:variable name="FP.DescriptionReport" select="/PackageFilesCheckerReport/FilePropertiesReport/DescriptionReport//DescriptionErrorFile" />
    <xsl:variable name="FP.DescriptionReport.count" select="count($FP.DescriptionReport)" />
    <xsl:variable name="FE.DotfuscatorReport" select="/PackageFilesCheckerReport/FileProtectedReport/DotfuscatorReport//DotfuscatorErrorFile" />
    <xsl:variable name="FE.DotfuscatorReport.count" select="count($FE.DotfuscatorReport)" />
    <xsl:variable name="FE.EncryptedReport" select="/PackageFilesCheckerReport/FileProtectedReport/EncryptedReport//EncryptedErrorFile" />
    <xsl:variable name="FE.EncryptedReport.count" select="count($FE.EncryptedReport)" />
    <xsl:variable name="MemoryLeakReport" select="/PackageFilesCheckerReport/MemoryLeakReport/ExistMemoryLeakFile//MemoryLeakFile" />
    <xsl:variable name="MemoryLeakReport.count" select="count($MemoryLeakReport)" />
    <xsl:variable name="DigitalSignErrorReport" select="/PackageFilesCheckerReport/DigitalSignReport//DigitalSignErrorFile/@Path[normalize-space(.)!= 0]" />
    <xsl:variable name="DigitalSignErrorReport.count" select="count($DigitalSignErrorReport)" />
    <xsl:variable name="ThirdPartyFileVersionErrorReport" select="/PackageFilesCheckerReport/IntegrityReport/VersionReport//VersionErrorFile" />
    <xsl:variable name="ThirdPartyFileVersionErrorReport.count" select="count($ThirdPartyFileVersionErrorReport)" />
    <xsl:variable name="StrongSignErrorReport" select="/PackageFilesCheckerReport/StrongSignReport//StrongSignErrorFile/@Path[normalize-space(.)!= 0]" />
    <xsl:variable name="StrongSignErrorReport.count" select="count($StrongSignErrorReport)" />
    <xsl:variable name="ConfigurationModeErrorReport" select="/PackageFilesCheckerReport/ConfigrationModeReport//ConfigrationModeErrorFile/@Path[normalize-space(.)!= 0]" />
    <xsl:variable name="ConfigurationModeErrorReport.count" select="count($ConfigurationModeErrorReport)" />

    <html>
      <body style="width:100%" >

        <xsl:if test="count(/PackageFilesCheckerReport)!=0">


          <table width="100%" height="10%" id="header" border="0" cellspacing="0" cellpadding="0">
            <tr class='toplevel'>
              <td width="5%">
              </td>
              <td class='toplevel' width="95%">
                大包检查结果
              </td>
            </tr>
          </table>

          <table width="100%" height="6px" id="header" border="0" cellspacing="0" cellpadding="0">
            <tr>
              <td width="100%" height="2px">
              </td>
            </tr>
            <tr style="background-color:#800786;" >
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
                      <td colspan="2" class="Summary">检查结果概要</td>
                    </tr>

                    <xsl:choose>
                      <xsl:when test="$PI.NotInPackageFiles.count > 0">
                        <tr >
                          <td class="SummaryError">
                            <a href="#_NotInPackage" target="_self">已经申请但没有Build到包里的文件数量:</a>
                          </td>
                          <td class="SummaryError">
                            <xsl:value-of select="$PI.NotInPackageFiles.count"/>
                          </td>
                        </tr>
                      </xsl:when>
                      <xsl:otherwise>
                        <tr>
                          <td>已经申请但没有Build到包里的文件数量:</td>
                          <td>
                            <xsl:value-of select="$PI.NotInPackageFiles.count"/>
                          </td>
                        </tr>
                      </xsl:otherwise>
                    </xsl:choose>

                    <xsl:choose>
                      <xsl:when test="$PI.NotInBinaryListFiles.count > 0">
                        <tr>
                          <td class="SummaryError">
                            <a href="#_NotInBinaryList" target="_self">没有申请的文件数量:</a>
                          </td>
                          <td class="SummaryError">
                            <xsl:value-of select="$PI.NotInBinaryListFiles.count"/>
                          </td>
                        </tr>
                      </xsl:when>
                      <xsl:otherwise>
                        <tr>
                          <td>没有申请的文件数量:</td>
                          <td>
                            <xsl:value-of select="$PI.NotInBinaryListFiles.count"/>
                          </td>
                        </tr>
                      </xsl:otherwise>
                    </xsl:choose>

                    <xsl:choose>
                      <xsl:when test="$MemoryLeakReport.count > 0">
                        <tr>
                          <td class="SummaryError">
                            <a href="#_MemoryLeak" target="_self">SPDispose工具检查出存在内存泄漏问题的文件数量:</a>
                          </td>
                          <td class="SummaryError">
                            <xsl:value-of select="$MemoryLeakReport.count"/>
                          </td>
                        </tr>
                      </xsl:when>
                      <xsl:otherwise>
                        <tr>
                          <td>SPDispose工具检查出存在内存泄漏问题的文件数量:</td>
                          <td>
                            <xsl:value-of select="$MemoryLeakReport.count"/>
                          </td>
                        </tr>
                      </xsl:otherwise>
                    </xsl:choose>

                    <xsl:choose>
                      <xsl:when test="$FP.CopyrightReport.count > 0">
                        <tr>
                          <td class="SummaryError">
                            <a href="#_CopyrightError" target="_self">Copyright文件属性不正确的文件数量:</a>
                          </td>
                          <td class="SummaryError">
                            <xsl:value-of select="$FP.CopyrightReport.count"/>
                          </td>
                        </tr>
                      </xsl:when>
                      <xsl:otherwise>
                        <tr>
                          <td>Copyright文件属性不正确的文件数量:</td>
                          <td>
                            <xsl:value-of select="$FP.CopyrightReport.count"/>
                          </td>
                        </tr>
                      </xsl:otherwise>
                    </xsl:choose>

                    <xsl:choose>
                      <xsl:when test="$FP.CompanyReport.count > 0">
                        <tr>
                          <td class="SummaryError">
                            <a href="#_CompanyError" target="_self">Company文件属性不正确的文件数量:</a>
                          </td>
                          <td class="SummaryError">
                            <xsl:value-of select="$FP.CompanyReport.count"/>
                          </td>
                        </tr>
                      </xsl:when>
                      <xsl:otherwise>
                        <tr>
                          <td>Company文件属性不正确的文件数量:</td>
                          <td>
                            <xsl:value-of select="$FP.CompanyReport.count"/>
                          </td>
                        </tr>
                      </xsl:otherwise>
                    </xsl:choose>

                    <xsl:choose>
                      <xsl:when test="$FP.ProductNameReport.count > 0">
                        <tr>
                          <td class="SummaryError">
                            <a href="#_ProductNameError" target="_self">Product Name文件属性不正确的文件数量:</a>
                          </td>
                          <td class="SummaryError">
                            <xsl:value-of select="$FP.ProductNameReport.count"/>
                          </td>
                        </tr>
                      </xsl:when>
                      <xsl:otherwise>
                        <tr>
                          <td>Product Name文件属性不正确的文件数量:</td>
                          <td>
                            <xsl:value-of select="$FP.ProductNameReport.count"/>
                          </td>
                        </tr>
                      </xsl:otherwise>
                    </xsl:choose>

                    <xsl:choose>
                      <xsl:when test="$FP.VersionReport.count > 0">
                        <tr>
                          <td class="SummaryError">
                            <a href="#_VersionError" target="_self">Version文件属性不正确的文件数量:</a>
                          </td>
                          <td class="SummaryError">
                            <xsl:value-of select="$FP.VersionReport.count"/>
                          </td>
                        </tr>
                      </xsl:when>
                      <xsl:otherwise>
                        <tr>
                          <td>Version文件属性不正确的文件数量:</td>
                          <td>
                            <xsl:value-of select="$FP.VersionReport.count"/>
                          </td>
                        </tr>
                      </xsl:otherwise>
                    </xsl:choose>

                    <xsl:choose>
                      <xsl:when test="$FP.DescriptionReport.count > 0">
                        <tr>
                          <td class="SummaryError">
                            <a href="#_DescriptionError" target="_self">Description文件属性不正确的文件数量:</a>
                          </td>
                          <td class="SummaryError">
                            <xsl:value-of select="$FP.DescriptionReport.count"/>
                          </td>
                        </tr>
                      </xsl:when>
                      <xsl:otherwise>
                        <tr>
                          <td>Description文件属性不正确的文件数量:</td>
                          <td>
                            <xsl:value-of select="$FP.DescriptionReport.count"/>
                          </td>
                        </tr>
                      </xsl:otherwise>
                    </xsl:choose>

                    <xsl:choose>
                      <xsl:when test="$FE.DotfuscatorReport.count > 0">
                        <tr>
                          <td class="SummaryError">
                            <a href="#_ObfuscationError" target="_self">混淆失败的文件数量:</a>
                          </td>
                          <td class="SummaryError">
                            <xsl:value-of select="$FE.DotfuscatorReport.count"/>
                          </td>
                        </tr>
                      </xsl:when>
                      <xsl:otherwise>
                        <tr>
                          <td>混淆失败的文件数量:</td>
                          <td>
                            <xsl:value-of select="$FE.DotfuscatorReport.count"/>
                          </td>
                        </tr>
                      </xsl:otherwise>
                    </xsl:choose>

                    <xsl:choose>
                      <xsl:when test="$FE.EncryptedReport.count > 0">
                        <tr>
                          <td class="SummaryError">
                            <a href="#_EncryptionError" target="_self">加密失败的文件数量:</a>
                          </td>
                          <td class="SummaryError">
                            <xsl:value-of select="$FE.EncryptedReport.count"/>
                          </td>
                        </tr>
                      </xsl:when>
                      <xsl:otherwise>
                        <tr>
                          <td>加密失败的文件数量:</td>
                          <td>
                            <xsl:value-of select="$FE.EncryptedReport.count"/>
                          </td>
                        </tr>
                      </xsl:otherwise>
                    </xsl:choose>

                    <xsl:choose>
                      <xsl:when test="$DigitalSignErrorReport.count > 0">
                        <tr>
                          <td class="SummaryError">
                            <a href="#_DigitalSignError" target="_self">数字签名失败的文件数量:</a>
                          </td>
                          <td class="SummaryError">
                            <xsl:value-of select="$DigitalSignErrorReport.count"/>
                          </td>
                        </tr>
                      </xsl:when>
                      <xsl:otherwise>
                        <tr>
                          <td>数字签名失败的文件数量:</td>
                          <td>
                            <xsl:value-of select="$DigitalSignErrorReport.count"/>
                          </td>
                        </tr>
                      </xsl:otherwise>
                    </xsl:choose>

                    <xsl:choose>
                      <xsl:when test="$ThirdPartyFileVersionErrorReport.count > 0">
                        <tr>
                          <td class="SummaryError">
                            <a href="#_ThirdPartyFileVersionError" target="_self">已经升级但没有申请的第三方文件数量:</a>
                          </td>
                          <td class="SummaryError">
                            <xsl:value-of select="$ThirdPartyFileVersionErrorReport.count"/>
                          </td>
                        </tr>
                      </xsl:when>
                      <xsl:otherwise>
                        <tr>
                          <td>已经升级但没有申请的第三方文件数量:</td>
                          <td>
                            <xsl:value-of select="$ThirdPartyFileVersionErrorReport.count"/>
                          </td>
                        </tr>
                      </xsl:otherwise>
                    </xsl:choose>

                    <xsl:choose>
                      <xsl:when test="$StrongSignErrorReport.count > 0">
                        <tr>
                          <td class="SummaryError">
                            <a href="#_StrongSignError" target="_self">强签名失败的文件数量:</a>
                          </td>
                          <td class="SummaryError">
                            <xsl:value-of select="$StrongSignErrorReport.count"/>
                          </td>
                        </tr>
                      </xsl:when>
                      <xsl:otherwise>
                        <tr>
                          <td>强签名失败的文件数量:</td>
                          <td>
                            <xsl:value-of select="$StrongSignErrorReport.count"/>
                          </td>
                        </tr>
                      </xsl:otherwise>
                    </xsl:choose>

                    <xsl:choose>
                      <xsl:when test="$ConfigurationModeErrorReport.count > 0">
                        <tr>
                          <td class="SummaryError">
                            <a href="#_ConfigurationModeError" target="_self">Debug文件:</a>
                          </td>
                          <td class="SummaryError">
                            <xsl:value-of select="$ConfigurationModeErrorReport.count"/>
                          </td>
                        </tr>
                      </xsl:when>
                      <xsl:otherwise>
                        <tr>
                          <td>Debug文件:</td>
                          <td>
                            <xsl:value-of select="$ConfigurationModeErrorReport.count"/>
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
                        <td width="100%" class="Details">大包检查结果详细信息</td>
                      </tr>
                      <xsl:if test="$PI.NotInPackageFiles.count!=0">
                        <table width="100%">
                          <tr width="100%" class="DetailsInfoTitle">
                            <td class="DetailsInfoTitleLeft" id="_NotInPackage">
                              <a class="a" href="https://git.avepoint.net/SCM/Documents/blob/master/CMPlan/transition-plan/scm-check-rules/pkg-check-report/not-in-package.md">已经申请但没有Build到包中的文件</a>
                            </td>
                            <td class="DetailsInfoTitleRight">
                            </td>
                          </tr>
                          <tr width="100%">
                            <td class="DetailsInfo" colspan="2">
                              <xsl:for-each select="$PI.NotInPackageFiles">
                                <xsl:value-of select="./@Path"/>
                                <br/>
                              </xsl:for-each>
                            </td>
                          </tr>

                        </table>
                        <br/>
                        <br/>
                      </xsl:if>
                      <xsl:if test="$PI.NotInBinaryListFiles.count!=0">

                        <table width="100%">
                          <tr width="100%" class="DetailsInfoTitle">
                            <td class="DetailsInfoTitleLeft" id="_NotInBinaryList">
                              <a class="a" href="https://git.avepoint.net/SCM/Documents/blob/master/CMPlan/transition-plan/scm-check-rules/pkg-check-report/not-in-list.md">没有申请的文件</a>
                            </td>
                            <td class="DetailsInfoTitleRight">
                            </td>
                          </tr>
                          <tr width="100%">
                            <td class="DetailsInfo" colspan="2">
                              <xsl:for-each select="$PI.NotInBinaryListFiles">
                                <xsl:value-of select="./@Path"/>
                                <br/>
                              </xsl:for-each>
                            </td>
                          </tr>

                        </table>
                        <br/>
                        <br/>
                      </xsl:if>
                      <xsl:if test="$MemoryLeakReport.count!=0">
                        <table width="100%">
                          <tr width="100%" class="DetailsInfoTitle">
                            <td class="DetailsInfoTitleLeft" id="_MemoryLeak">
                              <a class="a" href="https://git.avepoint.net/SCM/Documents/blob/master/CMPlan/transition-plan/scm-check-rules/pkg-check-report/memory-leak.md">SPDispose工具检查出存在内存泄漏问题的文件</a>
                            </td>
                            <td class="DetailsInfoTitleRight">
                            </td>
                          </tr>
                          <tr width="100%">
                            <td class="DetailsInfo" colspan="2">
                              <xsl:for-each select="$MemoryLeakReport">
                                <xsl:value-of select="./@Path"/>
                                <br/>
                              </xsl:for-each>
                            </td>
                          </tr>

                        </table>
                        <br/>
                        <br/>
                      </xsl:if>
                      <xsl:if test="$FP.CopyrightReport.count!=0">
                        <table width="100%">
                          <tr width="100%" class="DetailsInfoTitle">
                            <td class="DetailsInfoTitleLeft" id="_CopyrightError">
                              <a class="a" href="https://git.avepoint.net/SCM/Documents/blob/master/CMPlan/transition-plan/scm-check-rules/pkg-check-report/copyright.md">Copyright文件属性不正确的文件</a>
                            </td>
                            <td class="DetailsInfoTitleRight">
                            </td>
                          </tr>
                          <tr width="100%">
                            <td class="DetailsInfo" colspan="2">
                              <xsl:for-each select="$FP.CopyrightReport">
                                <xsl:value-of select="./@FileName"/>
                                <font color="green">
                                  Correct: "<xsl:value-of select="./@Correct"/>"
                                </font>
                                <font color="red">
                                  Current: "<xsl:value-of select="./@Current"/>"
                                </font>
                                <br/>
                              </xsl:for-each>
                            </td>
                          </tr>

                        </table>
                        <br/>
                        <br/>
                      </xsl:if>
                      <xsl:if test="$FP.CompanyReport.count!=0">
                        <table width="100%">
                          <tr width="100%" class="DetailsInfoTitle">
                            <td class="DetailsInfoTitleLeft" id="_CompanyError">
                              <a class="a" href="https://git.avepoint.net/SCM/Documents/blob/master/CMPlan/transition-plan/scm-check-rules/pkg-check-report/company.md">Company文件属性不正确的文件</a>
                            </td>
                            <td class="DetailsInfoTitleRight">
                            </td>
                          </tr>
                          <tr width="100%">
                            <td class="DetailsInfo" colspan="2">
                              <xsl:for-each select="$FP.CompanyReport">
                                <xsl:value-of select="./@FileName"/>
                                <font color="green">
                                  Correct: "<xsl:value-of select="./@Correct"/>"
                                </font>
                                <font color="red">
                                  Current: "<xsl:value-of select="./@Current"/>"
                                </font>
                                <br/>
                              </xsl:for-each>
                            </td>
                          </tr>

                        </table>
                        <br/>
                        <br/>
                      </xsl:if>
                      <xsl:if test="$FP.ProductNameReport.count!=0">
                        <table width="100%">
                          <tr width="100%" class="DetailsInfoTitle">
                            <td class="DetailsInfoTitleLeft" id="_ProductNameError">
                              <a class="a" href="https://git.avepoint.net/SCM/Documents/blob/master/CMPlan/transition-plan/scm-check-rules/pkg-check-report/productname.md">Product Name文件属性不正确的文件</a>
                            </td>
                            <td class="DetailsInfoTitleRight">
                            </td>
                          </tr>
                          <tr width="100%">
                            <td class="DetailsInfo" colspan="2">
                              <xsl:for-each select="$FP.ProductNameReport">
                                <xsl:value-of select="./@FileName"/>
                                <font color="green">
                                  Correct: "<xsl:value-of select="./@Correct"/>"
                                </font>
                                <font color="red">
                                  Current: "<xsl:value-of select="./@Current"/>"
                                </font>
                                <br/>
                              </xsl:for-each>
                            </td>
                          </tr>

                        </table>
                        <br/>
                        <br/>
                      </xsl:if>
                      <xsl:if test="$FP.VersionReport.count!=0">
                        <table width="100%">
                          <tr width="100%" class="DetailsInfoTitle">
                            <td class="DetailsInfoTitleLeft" id="_VersionError">
                              <a class="a" href="https://git.avepoint.net/SCM/Documents/blob/master/CMPlan/transition-plan/scm-check-rules/pkg-check-report/version.md">Version文件属性不正确的文件</a>
                            </td>
                            <td class="DetailsInfoTitleRight">
                            </td>
                          </tr>
                          <tr width="100%">
                            <td class="DetailsInfo" colspan="2">
                              <xsl:for-each select="$FP.VersionReport">
                                <xsl:value-of select="./@FileName"/>
                                <font color="green">
                                  Correct: "<xsl:value-of select="./@Correct"/>"
                                </font>
                                <font color="red">
                                  Current: "<xsl:value-of select="./@Current"/>"
                                </font>
                                <br/>
                              </xsl:for-each>
                            </td>
                          </tr>

                        </table>
                        <br/>
                        <br/>
                      </xsl:if>
                      <xsl:if test="$FP.DescriptionReport.count!=0">
                        <table width="100%">
                          <tr width="100%" class="DetailsInfoTitle">
                            <td class="DetailsInfoTitleLeft" id="_DescriptionError">
                              <a class="a" href="https://git.avepoint.net/SCM/Documents/blob/master/CMPlan/transition-plan/scm-check-rules/pkg-check-report/description.md">Description文件属性不正确的文件</a>
                            </td>
                            <td class="DetailsInfoTitleRight">
                            </td>
                          </tr>
                          <tr width="100%">
                            <td class="DetailsInfo" colspan="2">
                              <xsl:for-each select="$FP.DescriptionReport">
                                <xsl:value-of select="./@FileName"/>
                                <font color="green">
                                  Correct: "<xsl:value-of select="./@Correct"/>"
                                </font>
                                <font color="red">
                                  Current: "<xsl:value-of select="./@Current"/>"
                                </font>
                                <br/>
                              </xsl:for-each>
                            </td>
                          </tr>

                        </table>
                        <br/>
                        <br/>
                      </xsl:if>
                      <xsl:if test="$FE.DotfuscatorReport.count!=0">
                        <table width="100%">
                          <tr width="100%" class="DetailsInfoTitle">
                            <td class="DetailsInfoTitleLeft" id="_ObfuscationError">
                              <a class="a" href="https://git.avepoint.net/SCM/Documents/blob/master/CMPlan/transition-plan/scm-check-rules/pkg-check-report/protecterror.md">混淆失败的文件</a>
                            </td>
                            <td class="DetailsInfoTitleRight">
                            </td>
                          </tr>
                          <tr width="100%">
                            <td class="DetailsInfo" colspan="2">
                              <xsl:for-each select="$FE.DotfuscatorReport">
                                <xsl:value-of select="./@Path"/>
                                <br/>
                              </xsl:for-each>
                            </td>
                          </tr>

                        </table>
                        <br/>
                        <br/>
                      </xsl:if>
                      <xsl:if test="$FE.EncryptedReport.count!=0">
                        <table width="100%">
                          <tr width="100%" class="DetailsInfoTitle">
                            <td class="DetailsInfoTitleLeft" id="_EncryptionError">
                              <a class="a" href="https://git.avepoint.net/SCM/Documents/blob/master/CMPlan/transition-plan/scm-check-rules/pkg-check-report/protecterror.md">加密失败的文件</a>
                            </td>
                            <td class="DetailsInfoTitleRight">
                            </td>
                          </tr>
                          <tr width="100%">
                            <td class="DetailsInfo" colspan="2">
                              <xsl:for-each select="$FE.EncryptedReport">
                                <xsl:value-of select="./@Path"/>
                                <br/>
                              </xsl:for-each>
                            </td>
                          </tr>

                        </table>
                        <br/>
                        <br/>
                      </xsl:if>

                      <xsl:if test="$DigitalSignErrorReport.count!=0">
                        <table width="100%">
                          <tr width="100%" class="DetailsInfoTitle">
                            <td class="DetailsInfoTitleLeft" id="_DigitalSignError">
                              <a class="a" href="https://git.avepoint.net/SCM/Documents/blob/master/CMPlan/transition-plan/scm-check-rules/pkg-check-report/digitalsign.md">数字签名失败的文件</a>
                            </td>
                            <td class="DetailsInfoTitleRight">
                            </td>
                          </tr>
                          <tr width="100%">
                            <td class="DetailsInfo" colspan="2">
                              <xsl:for-each select="$DigitalSignErrorReport">
                                <xsl:value-of select="."/>
                                <br/>
                              </xsl:for-each>
                            </td>
                          </tr>

                        </table>
                        <br/>
                        <br/>
                      </xsl:if>

                      <xsl:if test="$ThirdPartyFileVersionErrorReport.count!=0">
                        <table width="100%">
                          <tr width="100%" class="DetailsInfoTitle">
                            <td class="DetailsInfoTitleLeft" id="_ThirdPartyFileVersionError">
                              <a class="a" href="https://git.avepoint.net/SCM/Documents/blob/master/CMPlan/transition-plan/scm-check-rules/pkg-check-report/thirdversion.md">已经升级但没有申请的第三方文件</a>
                            </td>
                            <td class="DetailsInfoTitleRight">
                            </td>
                          </tr>
                          <tr width="100%">
                            <td class="DetailsInfo" colspan="2">
                              <xsl:for-each select="$ThirdPartyFileVersionErrorReport">
                                <xsl:value-of select="./@Path"/>
                                <font color="green">
                                  Correct: "<xsl:value-of select="./@Correct"/>"
                                </font>
                                <font color="red">
                                  Current: "<xsl:value-of select="./@Current"/>"
                                </font>
                                <br/>
                              </xsl:for-each>
                            </td>
                          </tr>

                        </table>
                        <br/>
                        <br/>
                      </xsl:if>

                      <xsl:if test="$StrongSignErrorReport.count!=0">
                        <table width="100%">
                          <tr width="100%" class="DetailsInfoTitle">
                            <td class="DetailsInfoTitleLeft" id="_StrongSignError">
                              <a class="a" href="https://git.avepoint.net/SCM/Documents/blob/master/CMPlan/transition-plan/scm-check-rules/pkg-check-report/strongsign.md">强签名失败的文件</a>
                            </td>
                            <td class="DetailsInfoTitleRight">
                            </td>
                          </tr>
                          <tr width="100%">
                            <td class="DetailsInfo" colspan="2">
                              <xsl:for-each select="$StrongSignErrorReport">
                                <xsl:value-of select="."/>
                                <br/>
                              </xsl:for-each>
                            </td>
                          </tr>

                        </table>
                        <br/>
                        <br/>
                      </xsl:if>

                      <xsl:if test="$ConfigurationModeErrorReport.count!=0">
                        <table width="100%">
                          <tr width="100%" class="DetailsInfoTitle">
                            <td class="DetailsInfoTitleLeft" id="_ConfigurationModeError">
                              <a class="a" href="https://git.avepoint.net/SCM/Documents/blob/master/CMPlan/transition-plan/scm-check-rules/pkg-check-report/debug.md">Debug文件</a>
                            </td>
                            <td class="DetailsInfoTitleRight">
                            </td>
                          </tr>
                          <tr width="100%">
                            <td class="DetailsInfo" colspan="2">
                              <xsl:for-each select="$ConfigurationModeErrorReport">
                                <xsl:value-of select="."/>
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
        </xsl:if>
      </body>
    </html>


  </xsl:template>
</xsl:stylesheet>