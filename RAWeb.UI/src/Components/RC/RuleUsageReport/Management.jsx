import { Component, Fragment } from 'react';
import SiteMapLinks from '../../../Constants/SiteMapLinks';
import RuleDetail from '../../Common/RuleDetail/Index';
import { bindEvents, showToast } from '../../../Utilities/CommonUtil';
import TermTable from "./TermTable";
import '../../../Less/RC/ruleUsageReport.less';
import "../../../Less/RDM/ruleDetails.less";
import { Messagebox } from '../../Common/Messagebox';

export default class RuleUsageReportManagement extends Component {
    constructor(props) {
        super(props);
        this.initBinding();
        this.state = {
            isDisabled: true,
            searchKey: "",
            rules: [],
            selectedRule: null,
            termAllData: [],
            terms: [],
            termPageSize: 10,
            termPageIndex: 0,
            termsCount: 0,
            exportBtnDisabled: true,
            isOpenDialog: false,
            selRuleId: "",
            MessageTipInfo: {
                showTip: false,
                type: "success",
                content: ""
            },
            noneMessage: RMResx.RM_JS_RC_RUR_SearchNoData,
            columns: this.getColumns(),
        };
    }

    componentDidMount() {
        this.getRuleList();
        this.validateDAConSetting();
    }

    routerTo(routerUrl) {
        this.props.history.push({
            pathname: routerUrl
        });
    }

    initBinding() {
        bindEvents(this, "ruleSelectChange", "onSearch", "onStopSearch", "queryReport", "getRuleUsageInfo", "exportReport", "termPageChange");
    }

    validateDAConSetting() {
        $$.loading(true);
        let urlData = "/API/TermUsageReportApi/ValidateDAConnectionSetting";
        let option = {
            url: urlData,
            method: "POST"
        };
        fetchUtility(option).then((res) => {
            if (!res.Success) {
                showToast.error(res.Message);
            }
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });

    }

    getColumns() {
        return [
            {
                header: RMResx.RM_JS_RC_RUR_TermName,
                width: 200,
                resizeable: true
            },
            {
                header: RMResx.RM_JS_RC_RUR_TermPath,
                width: 300,
                resizeable: true
            },
            {
                header: RMResx.RM_JS_RC_ReportColumn_TermStatus,
                width: 120,
                resizeable: true
            }
        ];
    }

    getRuleList() {
        $$.loading(true);
        let urlData = "/api/RuleApi/GetRuleDatas";
        let option = {
            url: urlData,
            method: "POST"
        };
        fetchUtility(option).then((res) => {
            //刷新列表
            let ruleData = JSON.parse(res);

            this.setState({
                rules: ruleData,
            });

            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    getRuleUsageInfo() {
        let curRule = this.selectedRule;
        if (!curRule) {
            return;
        }
        $$.loading(true);
        this.forceUpdateRuleDetail = true;
        this.setState({
            selRuleId: curRule.RuleId,
            isOpenDialog: true
        });
        let urlData = "/api/RuleUsageReportApi/GetRuleUsageInfo?RuleId=" + curRule.RuleId + "&RuleName=" + curRule.RuleName;
        if (curRule) {
            let option = {
                url: urlData,
                method: "GET",
            };

            fetchUtility(option).then((res) => {
                //刷新列表
                let terms = JSON.parse(res);
                this.setState({
                    termAllData: terms,
                    termsCount: terms.length,
                    exportBtnDisabled: false
                });
                this.getTermsPaging(0, this.state.termPageSize);
                $$.loading(false);
            }).catch((e) => {
                $$.loading(false);
            });
        }

    }

    ruleSelectChange(args) {
        this.selectedRule = args.newValue;
        this.setState({
            isDisabled: false,
        });
    }

    queryReport(e) {
        this.getRuleUsageInfo();
        this.ruleDetail.load({ ruleId: this.selectedRule.RuleId });
    }

    onSearch(args) {
        this.setState({ searchKey: args });
        this.searchData(args, 0, this.state.termPageSize);
    }

    onStopSearch(args) {
        if (this.state.termAllData.length > 0) {
            this.getTermsPaging(0, this.state.termPageSize);
        }
        this.setState({
            termsCount: this.state.termAllData.length,
            searchKey: ""
        });
    }

    searchData(key, pageIndex, pageSize) {
        let reg = new RegExp(key, "i"),
            searchMatchArr = new Array();
        if (key.length == 0) {
            return false;
        }
        for (const term in this.state.termAllData) {
            if (this.state.termAllData.hasOwnProperty(term)) {
                var regBool = reg.test(this.state.termAllData[term].TermName);
                if (regBool) { searchMatchArr.push(this.state.termAllData[term]); }
            }
        }

        var startIndex = pageIndex * pageSize;
        var endIndex = (pageIndex + 1) * pageSize;
        var pageArr = searchMatchArr.slice(startIndex, endIndex);
        let currentSelectedItems = JSON.parse(JSON.stringify(pageArr));

        this.setState({
            termPageIndex: pageIndex,
            terms: currentSelectedItems,
            termsCount: searchMatchArr.length
        });
    }

    //echo terms by cache
    echoTerms() {
    }

    termPageChange(pagerIndex, pagerSize, callback) {
        this.getTermsPaging(pagerIndex, pagerSize);
        callback(true);
        var key = this.state.searchKey;
        this.searchData(key, pagerIndex, pagerSize);
    }

    getTermsPaging(pagerIndex, pagerSize) {
        this.setState({
            termPageIndex: pagerIndex,
            termPageSize: pagerSize
        });

        let termStartIndex = pagerIndex * pagerSize;
        let termEndIndex = (pagerIndex + 1) * pagerSize;
        let currentSelectedItems = JSON.parse(JSON.stringify(this.state.termAllData.slice(termStartIndex, termEndIndex)));
        this.setState({
            terms: currentSelectedItems
        });
    }

    onExportReportBtn = () => {
        Messagebox({ content: RMResx.RM_JS_Common_ExportMsg, actionFun: this.exportReport });
    }

    exportReport() {
        this.downloadForm.submit();
    }

    renderBaseInfo() {
        let selectRule = RMResx.RM_RC_RUR_SelectRule;
        selectRule = selectRule.substring(0, selectRule.length - 1);
        return <div className="ra-section">
            <div className="ra-section-head ra-inline-middle">
                <span tabIndex='0'>{selectRule}</span>
            </div>
            <div className="ra-ruleusage-rule-select">
                <span className="ra-ruleusage-selrule" tabIndex="0">{RMResx.RM_RC_RuleUsage_SelectRule}</span>
                <div tabIndex="0">
                    <R.Combobox
                        width={330}
                        height={36}
                        readonly={true}
                        disabled={this.state.rules.length == 0}
                        textField='RuleName'
                        valueField='RuleId'
                        checkedField='checked'
                        waterMark={RMResx.RM_JS_ImportPhsicalRecord_Water}
                        items={this.state.rules}
                        onChange={this.ruleSelectChange}
                    />
                    <div className="margin-left-10 inline-block" >
                        <R.Button
                            primary={true}
                            classify="theme"
                            disabled={this.state.isDisabled}
                            text={RMResx.RM_JS_TM_SearchTxt}
                            onClick={this.queryReport} />

                        <R.Button
                            text={RMResx.RM_JS_Common_ExportReport}
                            disabled={this.state.exportBtnDisabled}
                            onClick={this.exportReport} />
                    </div>

                </div>

            </div>

        </div>;

    }

    renderReportDesc() {
        return <div className="introduction">
            <div className="introduction-title">
                <span tabIndex='0'>{RMResx.RM_RC_Common_ReportInfo}</span>
            </div>
            <div className="introduction-headline"></div>
            <div className="introduction-content">
                <span tabIndex='0'>{RMResx.RM_RC_RUR_PageDescription}</span>
            </div>
        </div>;
    }

    renderAssoicatedTerm() {

        return <div className="ra-section">
            <div className="ra-section-head ra-inline-middle">
                <span tabIndex='0'>{RMResx.RM_RDM_Rule_AssociatedTerms}</span>
            </div>
            <div className="ra-ruleusage-term-search-box">
                <R.Searchbox
                    placeholder={RMResx.RM_RC_RUR_TermSearchPlaceholder}
                    disabled={this.state.termsCount <= 0 && this.state.searchKey == ""}
                    onSearch={(args) => (args || "").trim() === "" ? this.onStopSearch(args) : this.onSearch(args)}
                />
            </div>
            <div className="margin-top-10">
                <TermTable
                    id='termTable'
                    columnInfo={this.state.columns}
                    items={this.state.terms}
                />

                <div className="text-end margin-top-m">
                    <$g.Pager
                        itemsCount={this.state.termsCount}
                        pagerIndex={this.state.termPageIndex}
                        pagerSize={this.state.termPageSize}
                        showPagerSize={true}
                        pagerSizeOptions={[5, 10, 15]}
                        onChange={this.termPageChange} />
                </div>
            </div>
        </div>;
    }

    render() {

        return (
            <Fragment>
                <div className="reco-rule-usage-report-wrapper">
                    <section className="reco-rule-usage-report-header">
                        <$g.SiteMap data={[SiteMapLinks.RC_RuleUsageReportManagement]} />
                        <R.Button
                            id="raRcRurExportBtn"
                            primary={true}
                            classify="theme"
                            text={RMResx.RM_JS_Common_ExportReport}
                            disabled={this.state.exportBtnDisabled}
                            onClick={this.onExportReportBtn} />
                    </section>
                    <section className="reco-rule-usage-report-card">
                        <div className="reco-rule-usage-report-form">
                            <div className="reco-rule-usage-selector-title" tabIndex="0">
                                {RMResx.RM_RC_RuleUsage_SelectRule}
                            </div>
                            <div className="reco-rule-usage-selector">
                                <R.Combobox
                                    id="raRcRurRulesCmb"
                                    height={36}
                                    width={"100%"}
                                    disabled={this.state.rules.length == 0}
                                    textField='RuleName'
                                    valueField='RuleId'
                                    checkedField='checked'
                                    waterMark={RMResx.RM_JS_ImportPhsicalRecord_Water}
                                    items={this.state.rules}
                                    onChange={this.ruleSelectChange}
                                />
                            </div>
                            <div className="reco-rule-usage-report-search-btn">
                                <R.Button
                                    id="raRcRurSearchBtn"
                                    primary={true}
                                    classify="theme"
                                    disabled={this.state.isDisabled}
                                    text={RMResx.RM_JS_TM_SearchTxt}
                                    onClick={this.queryReport} />
                            </div>
                        </div>
                        <div className="reco-rule-usage-report-tips">
                            <div className="reco-rule-usage-report-tips-header">
                                <span className="reco-rule-usage-report-tips-icon fia-light">
                                </span>
                                <span className="reco-rule-usage-report-tips-header-title" tabIndex="0">
                                    {RMResx.RM_RC_Common_ReportInfo}
                                </span>
                            </div>
                            <div className="reco-rule-usage-report-tips-content" tabIndex="0">
                                {RMResx.RM_RC_RUR_PageDescription}
                            </div>
                        </div>
                    </section>
                    <section className="reco-rule-usage-report-content-card">
                        <div className="reco-rule-usage-report-detail">
                            <div className="reco-rule-usage-report-content-title" tabIndex='0'>
                                {RMResx.RM_RC_RUR_RuleDetailTitle}
                            </div>
                            <RuleDetail 
                                isExistPanel={false}
                                ref={r => this.ruleDetail = r}
                            />
                        </div>
                        <div className="reco-rule-usage-report-associated">
                            <div className="reco-rule-usage-report-content-title" tabIndex='0'>
                                {RMResx.RM_RDM_Rule_AssociatedTerms}
                            </div>
                            <div className="reco-rule-usage-report-search">
                                <R.Searchbox
                                    width={380}
                                    placeholder={RMResx.RM_RC_RUR_TermSearchPlaceholder}
                                    disabled={this.state.termsCount <= 0 && this.state.searchKey == ""}
                                    onSearch={(args) => (args || "").trim() === "" ? this.onStopSearch(args) : this.onSearch(args)}
                                />
                            </div>
                            <TermTable
                                id='reco-rule-usage-report-table'
                                columnInfo={this.state.columns}
                                items={this.state.terms}
                            />
                            <div className="reco-rule-usage-report-table-footer">
                                <$g.Pager
                                    itemsCount={this.state.termsCount}
                                    pagerIndex={this.state.termPageIndex}
                                    pagerSize={this.state.termPageSize}
                                    // showPagerSize={true}
                                    pagerSizeOptions={[5, 10, 15]}
                                    showPagerCounter={true}
                                    onChange={this.termPageChange} />
                            </div>
                        </div>
                    </section>
                </div>
                <form id="downloadForm" method="get" ref={r => this.downloadForm = r}
                    action="/api/RuleUsageReportApi/DownLoadReport">
                    <input id="RuleUsageDownloadRuleId" type="hidden" name="ruleId" value={this.state.selRuleId}></input>
                </form>
            </Fragment>
        );
    }
}
