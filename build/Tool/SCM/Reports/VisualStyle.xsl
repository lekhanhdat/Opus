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

    <xsl:variable name="VisualStyleReport" select="/CheckFile//VisualStyleError" />
    <xsl:variable name="VisualStyleReport.count" select="count($VisualStyleReport)" />

    <html>
      <body style="width:99%" >

        <table width="100%" height="10%" id="header" border="0" cellspacing="0" cellpadding="0">
          <tr class='toplevel'>
            <td width="5%">
            </td>
            <td class='toplevel' width="95%">
              Visual Style Check Reports
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
                    <td colspan="2" class="Summary">Visual Style Check Report Summary</td>
                  </tr>

                  <xsl:choose>
                    <xsl:when test="$VisualStyleReport.count > 0">
                      <tr >
                        <td class="SummaryError">
                          <a href="#_VisualStyleReport" target="_self">Visual Style Check Report</a>
                        </td>
                        <td class="SummaryError">
                          <xsl:value-of select="$VisualStyleReport.count"/>
                        </td>
                      </tr>
                    </xsl:when>
                    <xsl:otherwise>
                      <tr>
                        <td>Visual Style Check Report:</td>
                        <td>
                          <xsl:value-of select="$VisualStyleReport.count"/>
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
                      <td width="100%" class="Details">Visual Style Check Report Details</td>
                    </tr>

                    <xsl:if test="$VisualStyleReport.count!=0">
                      <table width="100%">
                        <tr width="100%" class="DetailsInfoTitle">
                          <td class="DetailsInfoTitleLeft" id="_VisualStyleReport">
                            <a class="a" href="https://git.avepoint.net/SCM/Documents/blob/master/CMPlan/transition-plan/scm-check-rules/visual-style.md">Visual Style Check Report</a>
                          </td>
                          <td class="DetailsInfoTitleRight">
                          </td>
                        </tr>
                        <tr width="100%">
                          <td class="DetailsInfo" colspan="2">
                            <xsl:for-each select="$VisualStyleReport">
                              <font color="red">
                                <b>
                                  <xsl:value-of select="./@filename"/>
                                </b>
                              </font>
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