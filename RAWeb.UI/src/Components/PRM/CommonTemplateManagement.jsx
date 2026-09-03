import { Component } from "react";
import { withRouter } from 'react-router-dom';
import SiteMapLinks from "../../Constants/SiteMapLinks";
import { TemplateCardView } from "../PRM/TemplateCommon.jsx";
import { bindEvents } from "../../Utilities/CommonUtil";
import RouterUrls from "../../Constants/RouterUrls";
import * as Constants from "./Constants";
import "../../Less/PRM/TemplateManagement.less";

const GuidEmpty = "00000000-0000-0000-0000-000000000000";
export default withRouter(class CommonTemplateManagement extends Component {
    constructor(props) {
        super(props);
        this.state = {
            suiteItems: [],
            pageIndex: 1,
            pageSize: 12,
            totalCount: 0,
            pageItemCount: 0,
            searchValue: "",
            templateInfoOfBreadCrumbs: null,
            noData: false,
            showTip: false,
            tipType: "success",
            tipMsg: "",
            showAddFolderPanel: { show: false },
            showAddRecordPanel: { show: false },
            showUniqueIDSettingsPanel: { show: false },
            exitsFolderTemplateItems: [],
            exitsRecordTemplateItems: [],
            checkedFolderIds: [],
            checkedRecordIds: [],
            folderPanelBtn: <>
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_BCM_Explorer_MRR_Add_Button_Add} disabled={true} onClick={this.onAddExitsFolderTemplate.bind(this)} />
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={() => { this.setState({ showAddFolderPanel: { show: false } }) }} />
            </>,
            recordPanelBtn: <>
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_BCM_Explorer_MRR_Add_Button_Add} disabled={true} onClick={this.onAddExitsRecordTemplate.bind(this)} />
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={() => { this.setState({ showAddRecordPanel: { show: false } }) }} />
            </>,
            showUniqueIdSettingsBtn: false,
            boxTemplatePrefix: '',
            boxTemplateNumberOfDigits: 0,
            folderTemplatePrefix: '',
            folderTemplateNumberOfDigits: 0,
            recordTemplatePrefix: '',
            recordTemplateNumberOfDigits: 0,
            invalidPrefix: {
                Box: false,
                Folder: false,
                Record: false
            },
            invalidNumberOfDigits: {
                Box: false,
                Folder: false,
                Record: false
            },
            invalidMessagePrefix: {
                Box: '',
                Folder: '',
                Record: ''
            },
            invalidMessageNumberOfDigits: {
                Box: '',
                Folder: '',
                Record: ''
            },
        };
        // this.pUniqueId = RM.Url.getParam(window.location.href, "pId");
        this.suiteUniqueId = RM.Url.getParam(window.location.href, "suiteId") || GuidEmpty;
        this.boxTemplateId = RM.Url.getParam(window.location.href, "bId") || GuidEmpty;
        this.folderTemplateId = RM.Url.getParam(window.location.href, "fId") || GuidEmpty;
        this.curTempType = RM.Url.getParam(window.location.href, "cType");
        this.timer = null;
        this.templateCardContainerRef = React.createRef();
        bindEvents(this, "handelNewTemplate", "onClickSuiteMenu", "onClickBoxOrFolderMenu", "onClickFolderOrRecordMenu", "linkClickNewBoxTemplate",
            "linkClickNewFolderTemplate", "browseTemplate", "onScroll", "onSearch", "handleExitsFolderCheckChanged", "handleExitsRecordCheckChanged", "onAddExitsFolderTemplate", "onAddExitsRecordTemplate",
            "renderAddFolderPanelBtns", "renderAddRecordPanelBtns");
    }

    componentDidMount() {
        this.initData();
        this.initIsGlobalUniqueIdSetting();
        window.addEventListener('scroll', this.bindScroll);
        this.checkStatus();
    }

    componentWillUnmount() {
        window.removeEventListener('scroll', this.bindScroll);
        // 卸载异步操作设置状态
        this.setState = (state, callback) => {
            return;
        };
    }

    initData() {
        let reqOption = this.getRequestOption();
        $$.loading(true);
        fetchUtility(reqOption).then((result) => {
            console.log('init first page data.');
            if(result.ResultList && result.ResultList.length > 0)
            {
                this.setState({
                    suiteItems: result.ResultList,
                    totalCount: result.TotalCount,
                    pageItemCount: result.ResultList.length,
                    templateInfoOfBreadCrumbs: result.TemplateInfoOfBreadCrumbs,
                    noData: false
                });
            } else {
                this.setState({
                    suiteItems: [],
                    pageIndex: 1,
                    totalCount: 0,
                    pageItemCount: 0,
                    templateInfoOfBreadCrumbs: result.TemplateInfoOfBreadCrumbs,
                    noData: true
                });
            }
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    checkStatus = () => {
        var status = RM.CommStatus.get();
        if (status) {
            let contentMessage = '';
            if (status.includes('template')) {
                contentMessage = status == Constants.NewOrEditTemplateCookieNames.CreateSuccess ? RMResx.RM_PRM_TM_CreateTemplate_Success : RMResx.RM_PRM_TM_EditTemplate_Success;
            } else {
                contentMessage = status == RM.CommStatus.CreateSuccess ? RMResx.RM_PRM_TM_CreateSuite_Success : RMResx.RM_PRM_TM_EditSuite_Success;
            }
            this.showMessageTip("success", contentMessage);
            RM.CommStatus.remove();
        }
    };

    initIsGlobalUniqueIdSetting() {
        let option = {
            url: "/api/TemplateManagementApi/LoadingUniqueIdSetting",
            method: "get",
        };
        fetchUtility(option).then((result) => {
            let uniqueIdSetting = JSON.parse(result);
            if (uniqueIdSetting) {
                this.setState({ showUniqueIdSettingsBtn: uniqueIdSetting.IsGlobalSetting });
            }
        }).catch((e) => {

        });
    }

    loadNewData() {
        let reqOption = this.getRequestOption();
        let pageItemCount = this.state.pageItemCount;
        this.timer = setTimeout(() => {
            fetchUtility(reqOption).then((result) => {
                let newPageItems = result.ResultList;
                if (newPageItems && newPageItems.length > 0) {
                    let beforePageItems = RM.deepcopy(this.state.suiteItems);
                    pageItemCount += newPageItems.length;
                    this.setState({
                        suiteItems: beforePageItems.concat(newPageItems),
                        pageItemCount: pageItemCount
                    });
                }
            }).catch((e) => {

            });

        }, 500);
    }

    loadExitsFolderTemplateData() {
        let option = {
            url: `/api/TemplateManagementApi/GetExistingTemplatesInfo`,
            method: "post",
            data: {
                Type: Constants.TemplateTypes.Folder,
                SuiteId: this.suiteUniqueId,
                BoxTemplateId: this.boxTemplateId,
                FolderTemplateId: this.folderTemplateId
            }
        };
        fetchUtility(option).then((result) => {
            if (result && result.FolderTemplates) {
                // this.exitsFolderTemplateItems = this.initExitsTemplateItems(Constants.TemplateTypes.Folder, result.FolderTemplates);
                this.exitsFolderTemplateItems = this.initExitsTemplateItems(result.FolderTemplates);
                this.setState({
                    exitsFolderTemplateItems: result.FolderTemplates,
                    checkedFolderIds: [],
                    showAddFolderPanel: { show: true }
                });
            } else {
                this.showMessageBoxForNoExistTemplate();
            }
        }).catch((e) => {

        });
    }

    loadExitsRecordTemplateData() {
        let option = {
            url: `/api/TemplateManagementApi/GetExistingTemplatesInfo`,
            method: "post",
            data: {
                Type: Constants.TemplateTypes.Records,
                SuiteId: this.suiteUniqueId,
                BoxTemplateId: this.boxTemplateId,
                FolderTemplateId: this.folderTemplateId
            }
        };
        fetchUtility(option).then((result) => {
            if (result && result.RecordTemplates) {
                // this.exitsRecordsTemplateItems = this.initExitsTemplateItems(Constants.TemplateTypes.Records, result.RecordTemplates);
                this.exitsRecordsTemplateItems = this.initExitsTemplateItems(result.RecordTemplates);
                this.setState({
                    exitsRecordTemplateItems: result.RecordTemplates,
                    checkedRecordIds: [],
                    showAddRecordPanel: { show: true }
                });
            } else {
                this.showMessageBoxForNoExistTemplate();
            }
        }).catch((e) => {

        });
    }

    refreshPage() {
        this.setState({
            suiteItems: [],
            pageIndex: 1,
            totalCount: 0,
            pageItemCount: 0,
            folderPanelBtn: <>
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_BCM_Explorer_MRR_Add_Button_Add} disabled={true} onClick={this.onAddExitsFolderTemplate.bind(this)} />
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={() => { this.setState({ showAddFolderPanel: { show: false } }) }} />
            </>,
            recordPanelBtn: <>
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_BCM_Explorer_MRR_Add_Button_Add} disabled={true} onClick={this.onAddExitsRecordTemplate.bind(this)} />
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={() => { this.setState({ showAddRecordPanel: { show: false } }) }} />
            </>,
        }, () => {
            this.initData();
        });
    }

    getRequestOption() {
        let reqUrl = this.props.getDataUrl;
        let option = {
            url: reqUrl,
            method: "post",
            data: {
                PagingInfo: { PageIndex: this.state.pageIndex, PageSize: this.state.pageSize },
                SearchValue: this.state.searchValue,
                // ParentUniqueId: GuidEmpty,
            }
        };
        if (this.props.commonType != Constants.CommonTemplateManagementType.Suite) {
            // option.data.ParentUniqueId = this.pUniqueId;
            option.data.SuiteUniqueId = this.suiteUniqueId;
            option.data.BoxTemplateUniqueId = this.boxTemplateId;
            option.data.FolderTemplateUniqueId = this.folderTemplateId;
        }
        if (this.props.commonType == Constants.CommonTemplateManagementType.Record) {
            option.data.PagingInfo.pageSize = 20;
        }
        return option;
    }

    onScroll() {
        // log("scrollHeight:" + this.templateCardContainerRef.current.scrollHeight)
        // log("clientHeight:" + this.templateCardContainerRef.current.clientHeight)
        // log("scrollTop:" + this.templateCardContainerRef.current.scrollTop)
        // log((this.templateCardContainerRef.current.scrollHeight - this.templateCardContainerRef.current.clientHeight - this.templateCardContainerRef.current.scrollTop))
        if ((this.templateCardContainerRef.current.scrollHeight - this.templateCardContainerRef.current.clientHeight) > this.templateCardContainerRef.current.scrollTop) {
            //未到底
        } else {
            //已到底部
            if (this.timer) {
                clearTimeout(this.timer);
            }
            let itemTotalCount = this.state.totalCount;
            let pageItemCount = this.state.pageItemCount;
            if (itemTotalCount == pageItemCount) {
                console.log('no data need to load.');
            } else {
                let pageIndex = this.state.pageIndex;
                ++pageIndex;
                this.setState({
                    pageIndex: pageIndex
                }, () => {
                    this.loadNewData();
                });
            }

        }
    }

    handleDelSuite(id) {
        let option = {
            url: this.props.delSuiteUrl,
            method: "post",
            data: id
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if (res.MessageType === 0) {
                this.showMessageTip("success", RMResx.RM_PRM_TM_SuccessToDeleteSuite);
                this.refreshPage();
            } else {

                this.showMessageTip("error", RMResx.RM_PRM_TM_FailedToDeleteSuite);
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    handleDelTemplate(id, parentFolderId, parentBoxId) {
        let option = {
            url: this.props.delTemplateUrl,
            method: "post",
            data: {
                TemplateId: id,
                ParentFolderId: parentFolderId || GuidEmpty,
                ParentBoxId: parentBoxId || GuidEmpty
            }
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if (res.MessageType === 0) {
                this.showMessageTip("success", RMResx.RM_PRM_TM_SuccessToDeleteTemplate);
                this.refreshPage();
            } else {

                this.showMessageTip("error", RMResx.RM_PRM_TM_FailedToDeleteTemplate);
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    handelNewTemplate(url) {
        this.routerTo(url);
    }

    handelAddExitsTemplate(type) {
        switch (type) {
            case Constants.CommonTemplateManagementType.Folder:
                this.loadExitsFolderTemplateData();
                break;
            case Constants.CommonTemplateManagementType.Record:
                this.loadExitsRecordTemplateData();
                break;
        }
    }

    handelUniqueIdSettings() {
        let option = {
            url: "/api/TemplateManagementApi/LoadingUniqueIdSetting",
            method: "get",
        };
        fetchUtility(option).then((result) => {
            let uniqueIdSetting = JSON.parse(result);
            if (uniqueIdSetting) {
                this.setState({
                    showUniqueIDSettingsPanel: { show: true },
                    boxTemplatePrefix: uniqueIdSetting.BoxTemplatePrefix,
                    boxTemplateNumberOfDigits: uniqueIdSetting.BoxTemplateNumberOfDigits,
                    folderTemplatePrefix: uniqueIdSetting.FolderTemplatePrefix,
                    folderTemplateNumberOfDigits: uniqueIdSetting.FolderTemplateNumberOfDigits,
                    recordTemplatePrefix: uniqueIdSetting.RecordTemplatePrefix,
                    recordTemplateNumberOfDigits: uniqueIdSetting.RecordTemplateNumberOfDigits
                });
            }
        }).catch((e) => {

        });
    }

    routerTo(url) {
        this.props.history.push({
            pathname: url
        });
    }

    onClickSuiteMenu(item, id) {
        switch (item.index) {
            case 1:
                this.props.history.push({
                    pathname: `${this.props.editSuiteUrl}/?id=${id}`
                });
                break;
            case 2:
                this.handleDelSuite(id);
                break;
        }
    }

    onClickBoxOrFolderMenu(item, suiteItem) {
        let type = suiteItem.StartFromType == Constants.StartFromType.Box ? Constants.TemplateTypes.Box : Constants.TemplateTypes.Folder;
        switch (item.index) {
            case 1:
                this.props.history.push({
                    pathname: `${this.props.editTemplateUrl}/?id=${suiteItem.RootTemplateGuid}&type=${type}`
                });
                break;
            case 2:
                this.handleDelTemplate(suiteItem.RootTemplateGuid);
                break;
        }
    }

    onClickFolderOrRecordMenu(item, id) {
        let info = this.state.templateInfoOfBreadCrumbs;
        switch (item.index) {
            case 1:
                var redirectUrl = "";
                if (info.FolderTemplateId == this.folderTemplateId) {
                    redirectUrl = RouterUrls.PRM_EditTemplate + `/?id=${id}&suiteId=${this.suiteUniqueId}&bId=${this.boxTemplateId}&fId=${this.folderTemplateId}`;
                } else {
                    redirectUrl = RouterUrls.PRM_EditTemplate + `/?id=${id}&suiteId=${this.suiteUniqueId}&bId=${this.boxTemplateId}`;
                }
                this.props.history.push({
                    pathname: redirectUrl
                });
                break;
            case 2:
                this.handleDelTemplate(id, this.folderTemplateId, this.boxTemplateId);
                break;
        }
    }

    onSearch(args) {
        let sv = !args ? "" : args.trim();

        this.setState({
            searchValue: sv
        }, () => {
            this.initData();
        });
    }

    linkClickNewBoxTemplate(suiteId) {
        this.props.history.push({
            pathname: `${this.props.newTemplateUrl}/?type=${Constants.TemplateTypes.Box}&suiteId=${suiteId}`
        });
    }

    linkClickNewFolderTemplate(suiteId) {
        this.props.history.push({
            pathname: `${this.props.newTemplateUrl}/?type=${Constants.TemplateTypes.Folder}&suiteId=${suiteId}`
        });
    }

    browseTemplate(item) {
        let childTempType, redirectUrl;

        if (item.ViewDataLevel == Constants.ViewDataLevel.Suite && item.StartFromType == Constants.StartFromType.Box) {
            // pUniqueId = item.RootTemplateGuid;
            this.boxTemplateId = item.RootTemplateGuid;
            childTempType = Constants.TemplateTypes.Folder;
            redirectUrl = this.props.redirectFolderTemplateUrl + `/?cType=${childTempType}&suiteId=${item.SuiteUniqueId}&bId=${this.boxTemplateId}`;

        } else {
            let folderTemplateId = item.ViewDataLevel == Constants.ViewDataLevel.Suite ? item.RootTemplateGuid : item.TemplateUniqueId;
            let suiteId = item.ViewDataLevel == Constants.ViewDataLevel.Suite ? item.SuiteUniqueId : this.suiteUniqueId;
            childTempType = Constants.TemplateTypes.Records;
            redirectUrl = this.props.redirectRecordTemplateUrl + `/?cType=${childTempType}&suiteId=${suiteId}&bId=${this.boxTemplateId}&fId=${folderTemplateId}`;
        }

        this.props.history.push({
            pathname: redirectUrl
        });
    }

    getCardTemplateType(dto) {
        if (this.props.commonType == Constants.CommonTemplateManagementType.Suite) {
            let startFromType = dto.StartFromType;
            if (dto.RootTemplateGuid == GuidEmpty) {
                return startFromType == 1 ? Constants.CardType.EmptyBoxSuite : Constants.CardType.EmptyFolderSuite;
            } else {
                return startFromType == 1 ? Constants.CardType.BoxSuite : Constants.CardType.FolderSuite;
            }
        } else {
            switch (parseInt(this.curTempType, 10)) {
                case Constants.TemplateTypes.Folder:
                    return Constants.CardType.Folder;
                case Constants.TemplateTypes.Records:
                    return Constants.CardType.Record;
            }
        }
    }

    newGuid() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    showMessageTip = (type, msg) => {
        let tipOption = {
            showTip: true,
            tipType: type,
            tipMsg: msg
        };
        this.setState(tipOption);
    }

    hideMessageTip = () => {
        this.setState({ showTip: false });
    }

    renderNewTemplateBtn() {
        let type = this.props.commonType,
            btnText = "",
            newUrl = "",
            btnExitsText = "";
        switch (type) {
            case Constants.CommonTemplateManagementType.Suite:
                btnText = RMResx.RM_PRM_TM_Btn_NewSuite;
                newUrl = this.props.newSuiteUrl;
                break;
            case Constants.CommonTemplateManagementType.Folder:
                btnText = RMResx.RM_PRM_TM_Btn_NewFolderTemplate;
                newUrl = RouterUrls.PRM_CreateTemplate + `/?type=${Constants.TemplateTypes.Folder}&suiteId=${this.suiteUniqueId}&bId=${this.boxTemplateId}`;
                btnExitsText = RMResx.RM_PRM_TM_Btn_AddExistingFolderTemplate;
                break;
            case Constants.CommonTemplateManagementType.Record:
                btnText = RMResx.RM_PRM_TM_Btn_NewRecordTemplate;
                newUrl = RouterUrls.PRM_CreateTemplate + `/?type=${Constants.TemplateTypes.Records}&suiteId=${this.suiteUniqueId}&bId=${this.boxTemplateId}&fId=${this.folderTemplateId}`;
                btnExitsText = RMResx.RM_PRM_TM_Btn_AddExistingRecordTemplate;
                break;
        }
        return <React.Fragment>
            <R.Button primary={true} classify="theme" text={btnText} onClick={this.handelNewTemplate.bind(this, newUrl)} />
            {btnExitsText != "" && <R.Button primary={true} classify="theme" text={btnExitsText} onClick={this.handelAddExitsTemplate.bind(this, type)} />}
        </React.Fragment>;
    }

    renderUniqueIdSettingsBtn() {
        let type = this.props.commonType;
        if (type == Constants.CommonTemplateManagementType.Suite) {
            return <React.Fragment>
                {this.state.showUniqueIdSettingsBtn && <R.Button type="bald" icon="fia-uniqueid" id="raPhyTplUniqueIdSettingBtn" primary={true} classify="theme" text={RMResx.RM_EditTemplate_PhysicalUniqueIdSettingsTitle} onClick={this.handelUniqueIdSettings.bind(this)} />}
            </React.Fragment>;
        }
    }

    renderBarcodeTemplateBtn() {
        let type = this.props.commonType;
        if (type == Constants.CommonTemplateManagementType.Suite) {
            return <R.Button type="bald" icon="" primary={true} classify="theme" text={RMResx.RM_PRM_BarcodeTemplate} onClick={() => { this.routerTo(RouterUrls.PRM_BarcodeTemplate); }} />;
        }
    }

    renderNavBar() {
        return <div className='navbar margin-bottom-24'>
            <div className='navbar-left'>
                {this.renderNewTemplateBtn()}
                {this.renderUniqueIdSettingsBtn()}
                {this.renderBarcodeTemplateBtn()}
            </div>
            <div className='navbar-right'>
                <div className='navbar-search'>
                    <R.Searchbox
                        placeholder={RMResx.RM_JS_TM_SearchTxt}
                        disabled={false}
                        onSearch={this.onSearch}
                    />
                </div>
            </div>
        </div>;
    }

    renderSuiteCards() {
        let suiteItems = RM.deepcopy(this.state.suiteItems);
        let cardComponents = [];
        suiteItems.map((item, index) => {
            cardComponents.push(
                <TemplateCardView
                    key={this.newGuid()}
                    type={this.getCardTemplateType(item)}
                    pId={this.pUniqueId}
                    suiteItem={item}
                    onClickSuiteMenu={this.onClickSuiteMenu}
                    onClickBoxOrFolderMenu={this.onClickBoxOrFolderMenu}
                    onClickFolderOrRecordMenu={this.onClickFolderOrRecordMenu}
                    browseTemplate={this.browseTemplate}
                    handleCreateBoxTemplate={this.linkClickNewBoxTemplate}
                    handleCreateFolderTemplate={this.linkClickNewFolderTemplate}
                />
            );
        });
        return cardComponents;
    }


    handleExitsFolderCheckChanged(ids) {
        this.setState({
            checkedFolderIds: ids,
            folderPanelBtn: ids.length == 0 ? <>
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_BCM_Explorer_MRR_Add_Button_Add} disabled={true} onClick={this.onAddExitsFolderTemplate.bind(this)} />
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={() => { this.setState({ showAddFolderPanel: { show: false } }) }} />
            </> : <>
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_BCM_Explorer_MRR_Add_Button_Add} disabled={false} onClick={this.onAddExitsFolderTemplate.bind(this)} />
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={() => { this.setState({ showAddFolderPanel: { show: false } }) }} />
            </>,
        });
    }

    handleExitsRecordCheckChanged(ids) {
        this.setState({
            checkedRecordIds: ids,
            recordPanelBtn: ids.length == 0 ? <>
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_BCM_Explorer_MRR_Add_Button_Add} disabled={true} onClick={this.onAddExitsRecordTemplate.bind(this)} />
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={() => { this.setState({ showAddRecordPanel: { show: false } }) }} />
            </> : <>
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_BCM_Explorer_MRR_Add_Button_Add} disabled={false} onClick={this.onAddExitsRecordTemplate.bind(this)} />
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={() => { this.setState({ showAddRecordPanel: { show: false } }) }} />
            </>,
        });
    }

    initExitsTemplateItems(templates) {
        // let templates =  type == Constants.TemplateTypes.Folder ? this.state.exitsFolderTemplateItems: this.state.exitsRecordTemplateItems;
        let items = [];
        templates.map(item => {
            items.push({
                text: item.Name,
                value: item.UniqueId,
                checked: false,
            });
        });
        return items;
    }

    onPrefixChange = (type, value) => {
        switch (type) {
            case Constants.TemplateTypes.Box:
                this.setState({
                    boxTemplatePrefix: $.trim(value),
                });
                break;
            case Constants.TemplateTypes.Folder:
                this.setState({
                    folderTemplatePrefix: $.trim(value),
                });
                break;
            case Constants.TemplateTypes.Records:
                this.setState({
                    recordTemplatePrefix: $.trim(value),
                });
                break;
        }
    }

    onNumberDigitsChange = (type, value) => {
        switch (type) {
            case Constants.TemplateTypes.Box:
                this.setState({
                    boxTemplateNumberOfDigits: $.trim(value),
                });
                break;
            case Constants.TemplateTypes.Folder:
                this.setState({
                    folderTemplateNumberOfDigits: $.trim(value),
                });
                break;
            case Constants.TemplateTypes.Records:
                this.setState({
                    recordTemplateNumberOfDigits: $.trim(value),
                });
                break;
        }
    }

    renderAddFolderTemplatePanel() {
        return <R.Panel
            id="addFolderPanel"
            header={RMResx.RM_PRM_TM_Btn_AddExistingFolderTemplate}
            size={600}
            // actionType='back'
            status={this.state.showAddFolderPanel}
            destroy={true}
        >
            <div className="ra-panel-content reclassify-panel">
                <div className="ra-form-label margin-top-20 margin-bottom-10">{RMResx.RM_PRM_TM_SelectExistingFolderTemplatesDesc}</div>
                <R.Checkbox.Group
                    block={true}
                    name="ck-group-folder"
                    items={this.exitsFolderTemplateItems}
                    onChange={this.handleExitsFolderCheckChanged}
                />
            </div>
            {this.state.folderPanelBtn}
        </R.Panel>;

    }

    renderAddRecordTemplatePanel() {
        return <R.Panel
            id="addRecordPanel"
            header={RMResx.RM_PRM_TM_Btn_AddExistingRecordTemplate}
            size={600}
            // actionType='back'
            status={this.state.showAddRecordPanel}
            destroy={true}
        >
            <div className="ra-panel-content reclassify-panel">
                <div className="ra-form-label margin-top-20 margin-bottom-10">{RMResx.RM_PRM_TM_SelectExistingRecordTemplatesDesc}</div>
                <R.Checkbox.Group
                    block={true}
                    name="ck-group-record"
                    items={this.exitsRecordsTemplateItems}
                    onChange={this.handleExitsRecordCheckChanged}
                />
            </div>
            {this.state.recordPanelBtn}
        </R.Panel>;

    }

    renderGlobalUniqueIdSettingPanel() {
        return <R.Panel
            id="uniqueIdPanel"
            header={RMResx.RM_EditTemplate_PhysicalUniqueIdSettingsTitle}
            size={600}
            status={this.state.showUniqueIDSettingsPanel}
            destroy={true}
        >
            <div id="uniqueId-panel-container" className="ra-panel-content reclassify-panel">
                <div>
                    <div className='unique-id-settings-title' tabIndex='0'>{RMResx.RM_EditTemplate_GlobalBoxUniqueIdSettingsTitle}</div>
                    <div className='unique-id-settings-block'>
                        <div className="ra-form-label ra-require" >
                            <div className='input-label' tabIndex='0'>{RMResx.RM_EditTemplate_Prefix + ':'}</div>
                        </div>
                    </div>
                    <R.Input
                        name='binputPrefix'
                        type='text'
                        width={280}
                        value={this.state.boxTemplatePrefix || ''}
                        onChange={this.onPrefixChange.bind(this, Constants.TemplateTypes.Box)} aria={{ariaLabel:RMResx.RM_EditTemplate_Prefix}} />
                    {this.state.invalidPrefix.Box && <div className='ra-validation-msg'>
                        {this.state.invalidMessagePrefix.Box}
                    </div>}
                    <div className="ra-form-label ra-require"><div className='input-label' tabIndex='0'>{RMResx.RM_EditTemplate_NumberofDigits + ':'}</div></div>
                    <R.Input
                        name='binputNumberOfDigits'
                        type="text"
                        width={280}
                        value={this.state.boxTemplateNumberOfDigits || ''}
                        onChange={this.onNumberDigitsChange.bind(this, Constants.TemplateTypes.Box)} aria={{ariaLabel:RMResx.RM_EditTemplate_NumberofDigits}} />
                    {this.state.invalidNumberOfDigits.Box && <div className='ra-validation-msg'>
                        {this.state.invalidMessageNumberOfDigits.Box}
                    </div>}
                </div>

                <div>
                    <div className='unique-id-settings-title' tabIndex='0'>{RMResx.RM_EditTemplate_GlobalFileUniqueIdSettingsTitle}</div>
                    <div className='unique-id-settings-block'>
                        <div className="ra-form-label ra-require" >
                            <div className='input-label' tabIndex='0'>{RMResx.RM_EditTemplate_Prefix + ':'}</div>
                        </div>
                    </div>
                    <R.Input
                        name='finputPrefix'
                        type='text'
                        width={280}
                        value={this.state.folderTemplatePrefix || ''}
                        onChange={this.onPrefixChange.bind(this, Constants.TemplateTypes.Folder)} aria={{ariaLabel:RMResx.RM_EditTemplate_Prefix}} />
                    {this.state.invalidPrefix.Folder && <div className='ra-validation-msg'>
                        {this.state.invalidMessagePrefix.Folder}
                    </div>}
                    <div className="ra-form-label ra-require"><div className='input-label' tabIndex='0'>{RMResx.RM_EditTemplate_NumberofDigits + ':'}</div></div>
                    <R.Input
                        name='finputNumberOfDigits'
                        type="text"
                        width={280}
                        value={this.state.folderTemplateNumberOfDigits || ''}
                        onChange={this.onNumberDigitsChange.bind(this, Constants.TemplateTypes.Folder)} aria={{ariaLabel:RMResx.RM_EditTemplate_NumberofDigits}}  />
                    {this.state.invalidNumberOfDigits.Folder && <div className='ra-validation-msg'>
                        {this.state.invalidMessageNumberOfDigits.Folder}
                    </div>}
                </div>

                <div>
                    <div className='unique-id-settings-title' tabIndex='0'>{RMResx.RM_EditTemplate_GlobalRecordUniqueIdSettingsTitle}</div>
                    <div className='unique-id-settings-block'>
                        <div className="ra-form-label ra-require" >
                            <div className='input-label' tabIndex='0'>{RMResx.RM_EditTemplate_Prefix + ':'}</div>
                        </div>
                    </div>
                    <R.Input
                        name='rinputPrefix'
                        type='text'
                        width={280}
                        value={this.state.recordTemplatePrefix || ''}
                        onChange={this.onPrefixChange.bind(this, Constants.TemplateTypes.Records)} aria={{ariaLabel:RMResx.RM_EditTemplate_Prefix}} />
                    {this.state.invalidPrefix.Record && <div className='ra-validation-msg'>
                        {this.state.invalidMessagePrefix.Record}
                    </div>}
                    <div className="ra-form-label ra-require"><div className='input-label' tabIndex='0'>{RMResx.RM_EditTemplate_NumberofDigits + ':'}</div></div>
                    <R.Input
                        name='rinputNumberOfDigits'
                        type="text"
                        width={280}
                        value={this.state.recordTemplateNumberOfDigits || ''}
                        onChange={this.onNumberDigitsChange.bind(this, Constants.TemplateTypes.Records)} aria={{ariaLabel:RMResx.RM_EditTemplate_NumberofDigits}} />
                    {this.state.invalidNumberOfDigits.Record && <div className='ra-validation-msg'>
                        {this.state.invalidMessageNumberOfDigits.Record}
                    </div>}
                </div>
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={() => {
                    this.setState({ showUniqueIDSettingsPanel: { show: false } });
                }} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onSaveUniqueIdSetting.bind(this)} />
            </>
        </R.Panel>;

    }

    onAddExitsFolderTemplate()
    {
        let option = {
            url: `/api/TemplateManagementApi/AddExistingTemplates`,
            method: "post",
            data: {
                Ids: this.state.checkedFolderIds,
                SuiteId: this.suiteUniqueId,
                BoxTemplateId: this.boxTemplateId,
                FolderTemplateId: this.folderTemplateId
            }
        };
        fetchUtility(option).then((result) => {
            if(result)
            {
                this.refreshPage();
                this.setState({ showAddFolderPanel: { show: false } });
                this.showMessageTip("success", RMResx.RM_PRM_TM_AddExistingTemplate_Success);
            } else {
                this.showMessageTip("error", RMResx.RM_PRM_TM_AddExistingTemplate_Fail);
            }
        }).catch((e) => {

        });
    }

    onAddExitsRecordTemplate() {
        let option = {
            url: `/api/TemplateManagementApi/AddExistingTemplates`,
            method: "post",
            data: {
                Ids: this.state.checkedRecordIds,
                SuiteId: this.suiteUniqueId,
                BoxTemplateId: this.boxTemplateId,
                FolderTemplateId: this.folderTemplateId
            }
        };
        fetchUtility(option).then((result) => {
            if(result)
            {
                this.refreshPage();
                this.setState({ showAddRecordPanel: { show: false } });
                this.showMessageTip("success", RMResx.RM_PRM_TM_AddExistingTemplate_Success);
            } else {
                this.showMessageTip("error", RMResx.RM_PRM_TM_AddExistingTemplate_Fail);
            }
        }).catch((e) => {

        });
    }

    validateForm = () => {
        let boxValidPrefix = this.validatePrefixValue(this.state.boxTemplatePrefix);
        let boxValidNumberOfDigits = this.validateNumberOfDigits(this.state.boxTemplateNumberOfDigits);

        let folderValidPrefix = this.validatePrefixValue(this.state.folderTemplatePrefix);
        let folderValidNumberOfDigits = this.validateNumberOfDigits(this.state.folderTemplateNumberOfDigits);

        let recordValidPrefix = this.validatePrefixValue(this.state.recordTemplatePrefix);
        let redordValidNumberOfDigits = this.validateNumberOfDigits(this.state.recordTemplateNumberOfDigits);

        this.setState({
            invalidPrefix: {
                Box: !boxValidPrefix.result,
                Folder: !folderValidPrefix.result,
                Record: !recordValidPrefix.result
            },
            invalidNumberOfDigits: {
                Box: !boxValidNumberOfDigits.result,
                Folder: !folderValidNumberOfDigits.result,
                Record: !redordValidNumberOfDigits.result
            },
            invalidMessagePrefix: {
                Box: boxValidPrefix.errorMessage,
                Folder: folderValidPrefix.errorMessage,
                Record: recordValidPrefix.errorMessage
            },
            invalidMessageNumberOfDigits: {
                Box: boxValidNumberOfDigits.errorMessage,
                Folder: folderValidNumberOfDigits.errorMessage,
                Record: redordValidNumberOfDigits.errorMessage
            },
        });

        return boxValidPrefix.result && folderValidPrefix.result && recordValidPrefix.result
            && boxValidNumberOfDigits.result && folderValidNumberOfDigits.result && redordValidNumberOfDigits.result;

    }

    validateNumberOfDigits(val) {
        let [isValid, errorMessage, minValue, maxValue] = [true, '', 2, 15];
        var regExp = /(^[2-9]$)|(^1[0-5]$)/g;//2-15 number
        if (!regExp.test(val)) {
            isValid = false;
            errorMessage = RMResx.RM_EditTemplate_ValidateNumberOfDigitsErrorMessage.format(minValue, maxValue);
        }
        return {
            result: isValid,
            errorMessage: errorMessage
        };
        // if (!isValid) {
        //     // document.getElementsByName('inputNumberOfDigits')[0].focus();
        //     this.setState({
        //         invalidMessageNumberOfDigits: errorMessage,
        //         invalidNumberOfDigits: !isValid
        //     });
        // }
        // return isValid;
    }

    validatePrefixValue(val) {
        let [isValid, errorMessage] = [true, ''];
        let maxLength = 10;
        if (!this.validateIsNotEmpty(val)) {
            isValid = false;
            errorMessage = RMResx.RM_JS_RDM_CreateRule_Validation_ConditionNoValue;
        } else if (val.length > maxLength) {
            isValid = false;
            errorMessage = RMResx.RM_EditTemplate_ValidatePrefixErrorMessage.format(maxLength);
        }
        return {
            result: isValid,
            errorMessage: errorMessage
        };
        // if (!isValid) {
        //     // document.getElementsByName('inputPrefix')[0].focus();
        //     this.setState({
        //         invalidMessagePrefix: errorMessage,
        //         invalidPrefix: !isValid
        //     });
        // }
        // return isValid;
    }

    validateIsNotEmpty(val) {
        return $.trim(val) != '';
    }

    onSaveUniqueIdSetting() {
        if (!this.validateForm()) {
            return false;
        }
        let option = {
            url: `/api/TemplateManagementApi/SaveGlobalUniqueIdSettings`,
            method: "post",
            data: {
                BoxTemplatePrefix: this.state.boxTemplatePrefix,
                BoxTemplateNumberOfDigits: this.state.boxTemplateNumberOfDigits,
                FolderTemplatePrefix: this.state.folderTemplatePrefix,
                FolderTemplateNumberOfDigits: this.state.folderTemplateNumberOfDigits,
                RecordTemplatePrefix: this.state.recordTemplatePrefix,
                RecordTemplateNumberOfDigits: this.state.recordTemplateNumberOfDigits,
            }
        };
        fetchUtility(option).then((result) => {
            if (result) {
                this.setState({ showUniqueIDSettingsPanel: { show: false } });
            } else {
                //TODO messagebox 
            }
        }).catch((e) => {

        });
        // return false;
    }

    showMessageBoxForNoExistTemplate = (id) => {
        this.args = {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_PRM_TM_NoExistingTemplates,
            buttons: [{ text: RMResx.RM_JS_Common_OK, onClick: this.hideMessageBox }]
        };
        $$.messagedialog(true, this.args);
    }

    hideMessageBox = () => {
        $$.messagedialog(false);
    }

    initSiteMapData() {
        let items = [SiteMapLinks.PRM_TemplateManagement];
        let info = this.state.templateInfoOfBreadCrumbs;
        if (info) {
            if (info.BoxTemplateName) {
                let boxMapLink = SiteMapLinks.PRM_BoxTemplate;
                boxMapLink.text = info.BoxTemplateName;
                boxMapLink.href = RouterUrls.PRM_FolderTemplateManagement + `/?cType=${Constants.TemplateTypes.Folder}&suiteId=${this.suiteUniqueId}&bId=${info.BoxTemplateId}`;
                items.push(boxMapLink);
            }
            if (info.FolderTemplateName) {
                let folderMapLink = SiteMapLinks.PRM_FolderTemplate;
                folderMapLink.text = info.FolderTemplateName;
                items.push(folderMapLink);
            }
        }
        return items;
    }

    render() {
        return <React.Fragment>
            <$g.SiteMap data={this.initSiteMapData()} />
            <div>
                <R.Messagebar
                    message={this.state.tipMsg}
                    classify={this.state.tipType}
                    status={{ show: this.state.showTip }}
                    onClose={this.hideMessageTip}
                />
            </div>
            <div id='templateSuiteMain'>
                {this.renderNavBar()}
                <div id="templateCardContainer" className="row row-xlg"
                    ref={this.templateCardContainerRef}
                    onScroll={this.onScroll}
                >
                    {this.renderSuiteCards()}
                    {this.state.noData && <div className="temp-nodata-info">
                        {RMResx.RM_PRM_TM_NoTemplates}
                    </div>}
                </div>
            </div>
            {this.renderAddFolderTemplatePanel()}
            {this.renderAddRecordTemplatePanel()}
            {this.renderGlobalUniqueIdSettingPanel()}
        </React.Fragment>;
    }
});