import SiteMapLinks from "../../../Constants/SiteMapLinks";
import {TableRow} from "../AgentManagement/RowTemplate";
import CreateOrEditForm from "./CreateOrEditForm";
import RemindDialog from "./RemindDialog";
import DownloadConfigFileDialog from "./DownloadConfigFileDialog";
import CertificateDialog from "./CertificateDialog";
import "../../../Less/CP/agentManagement.less";
import { AgentStatus } from "../../../../src/Constants/Constants";
import { isEnableMultiGeoFeature, isShowActionByDC, LicenseHelper, showToast } from "../../../Utilities/CommonUtil";

const isJPMCFeatureEnabled = LicenseHelper.EnableJPMCFileSystemFeature();
const enableMultiGeoFeature = isEnableMultiGeoFeature();
const agentUpgradeResult = {
    0: RMResx.RM_CP_Agent_SaveUpgradeProcessing, // Means upgrading agent in progress
    1: RMResx.RM_CP_Agent_SaveUpgradeGenerateFailed,
    2: RMResx.RM_CP_Agent_SaveUpgradeNoLatestVersion,
    3: RMResx.RM_CP_Agent_SaveUpgradeNoActiveAgent,
    4: RMResx.RM_CP_Agent_SaveUpgradeHasRunningJob,
}

const isMultiGeoMainDC = isShowActionByDC();
export default class AgentManagement extends R.Component {
    idAttr = true;
    componentCreate () {
        this.menuBtnItems = [];
        this.crtOrEdtResult = {
            Succeed: 0,
            NoClientId: 1,
            NoCertificate: 2,
            NoFailed: 3,
            SameNameExist: 4,
        };

        this.certificateStatus = {
            None: 0,
            Active: 1,
            ToBeExpired: 2,
            Expired: 3
        };

        this.agentActions = {
            Create: 'create',
            Edit: 'edit',
            Download: 'download',
            Delete: 'delete',
            Disable: 'disable',
            Enable: 'enable',
        };

        this.state = {
            crtOrEdtPanelShow: false,
            crtOrEdtPanelTitle: RMResx.RM_CP_Agent_Register,
            selectedAgentInfo: {},

            columns: this.getGridColumns(),
            allAgentList: [],                     // 全部Agent数据
            agentList: [],                        // 当前页Agent数据
            appRegisterURL: null,
            agentInstallerURL: null,
            agentLatestVersion: null,
            shownCount: 0,
            totalCount: 0,
            pagerIndex: 0,
            pagerSize: 10,

            clientIdDiaShow: false,
            showClientIdMsg: false,
            clientId: '',
            showModifyClientIdMsgBox: false,

            showTip: false,
            tipType: "success",
            tipMsg: "",

            showTipForCert: false,
            tipTypeForCert: "success",
            tipMsgForCert: "",

            upgradeDiaShow: false,
            upgradeAgentBtnShow: false,
            checkedAgentList: [],

            searchValue: "",
        };
    }


    componentInit () {
        this.isOpenRemindDialog();
        this.setAgentLatestVersion();
        this.setAgentList(true);
        this.setDialogDes();
        this.setAppRegisterURL();
        this.setAgentInstallerURL();
    }
    showMsgToast(content, type) {
        let option = {
            content: content,
            classify: type,
        };
        $$.toast(option);
    }
    isOpenRemindDialog = () => {
        //第一次进入页面，默认弹出流程提示的dialog，以后再进入页面，不会自动弹出流程提示的dialog；
        let option = {
            url: "/api/CPAgentMgmtApi/IsSetupNotify",
            method: "POST",
        };
        fetchUtility(option).then((res) => {
            if (!res) {
                this.openRemindDialog();
                this.setUpNotifyFirstEnter();
            }
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    setAppRegisterURL(){
        $$.loading(true);
        let option = {
            url: "/api/CPAgentMgmtApi/GetAppRegisterURL",
            method: "POST",
        };
        fetchUtility(option).then((res) => {
            if(res){
                this.setState({appRegisterURL: res});
            }
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    setAgentInstallerURL(){
        $$.loading(true);
        let option = {
            url: "/api/CPAgentMgmtApi/GetAgentInstallerURL",
            method: "POST",
        };
        fetchUtility(option).then((res) => {
            if(res){
                this.setState({agentInstallerURL: res});
            }
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    setAgentLatestVersion(){
        $$.loading(true);
        let option = {
            url: "/api/CPAgentMgmtApi/GetAgentLatestVersion",
            method: "POST",
        };
        fetchUtility(option).then((res) => {
            if(res){
                this.setState({agentLatestVersion: res});
            }
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    setUpNotifyFirstEnter = () => {
        $$.loading(true);
        let option = {
            url: "/api/CPAgentMgmtApi/setUpNotify",
            method: "POST",
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    isLowAgentMinorVersion = (agentVersion) => {
        const agentLatestVersion = this.state.agentLatestVersion;
        const agentLatestVersionArr = agentLatestVersion.substr(agentLatestVersion.indexOf('.') + 1).split('.');
        const agentVersionArr = agentVersion.substr(agentVersion.indexOf('.') + 1).split('.');
        if(Number(agentVersionArr[0]) < Number(agentLatestVersionArr[0])) {
            return true;
        }

        if(Number(agentVersionArr[1]) < Number(agentLatestVersionArr[1])) {
            return true;
        }

        if(Number(agentVersionArr[2]) < Number(agentLatestVersionArr[2])) {
            return true;
        }

        return false;
    }

    setDialogDes () {
        $('#raCPAgentClientIdDialog .aui-dialog-header').append(`<div class="ra-dialog-des">${RMResx.RM_CP_Agent_ClientID_Intro}</div>`);
        $('#raDownloadConfigFile .aui-dialog-header').append(`<div class="ra-dialog-des">${RMResx.RM_CP_Agent_DownloadConfigFile_Introduce}</div>`);
    }

    getGridColumns () {
        const dataCenterColumn = {
            header: RMResx.RM_FS_Register_DataCenter,
            resizeable: true,
            width: [200]
        };

        const columns = [
            {
                header: RMResx.RM_CP_Agent_Column_Source,
                width: [200],
                resizeable: true,
            },
            {
                header: RMResx.RM_CP_Agent_Column_DisplayName,
                width: [250],
                resizeable: true
            },
            {
                header: RMResx.RM_CP_Agent_Column_Description,
                resizeable: true,
                width: [250]
            },
            ...(enableMultiGeoFeature ? [dataCenterColumn] : []),
            {
                header: RMResx.RM_CP_Agent_Column_Status,
                resizeable: true,
                width: [200]
            },
            {
                header: RMResx.RM_CP_Agent_Column_Version,
                resizeable: true,
                width: [250]
            },
            {
                header: RMResx.RM_CP_Agent_Column_ServerName,
                resizeable: true,
                width: [250]
            },
        ];

        if (isMultiGeoMainDC) {
        columns.push({
            header: RMResx.RM_CP_Agent_Column_Action,
            resizeable: true,
            width: [200]
        });
        }

        return columns;
    }

    handleShowMessageBar = (type, msg) => {
        let tipOption = {
            showTip: true,
            tipType: type,
            tipMsg: msg
        };
        this.setState(tipOption);
    }

    handleShowCertMessageBar = (type, msg) => {
        let tipOption = {
            showTipForCert: true,
            tipTypeForCert: type,
            tipMsgForCert: msg
        };
        this.setState(tipOption);
    }

    handleHideMessageBar = () => {
        this.setState({ showTip: false });
    }

    handleHideMessageBarForCert = () => {
        this.setState({ showTipForCert: false });
    }

    setExpiredCertificateTip(agentList) {
        let toBeExpiredOrExpiredCertificatesTip = "";
        let toBeExpiredCertificates = agentList.filter((item)=>{return item.CertificateStatus == this.certificateStatus.ToBeExpired;});
        let expiredCertificates = agentList.filter((item)=>{ return item.CertificateStatus == this.certificateStatus.Expired;});
        toBeExpiredOrExpiredCertificatesTip = <div>
            {
                toBeExpiredCertificates.map((item)=>{
                    return <div>{RMResx.RM_CP_AM_Certificate_WhichExpiredOneMouthLater.format(item.Name, item.CertificateThumbprint)}</div>
                })
            }
            { 
                expiredCertificates.map((item)=>{
                    return <div>{RMResx.RM_CP_AM_Certificate_WhichHaveExpired.format(item.Name, item.CertificateThumbprint)}</div>
                })
            }
        </div>;
        if(toBeExpiredCertificates.length > 0 || expiredCertificates.length > 0){
            this.handleShowCertMessageBar("warn", toBeExpiredOrExpiredCertificatesTip);
        }
    }

    shouldShowAgentBtn = (agentList = []) => {
        return this.onValidateAgentToUpgrade(agentList, this.state.agentLatestVersion);
    }  

    onValidateAgentToUpgrade = (agents = [], latestVersion) => {
        if (!agents.length) return false;

        return agents.every((agent) =>
            [AgentStatus.Active, AgentStatus.ActiveException].includes(agent.Status)
            && agent.Version !== latestVersion
            && agent.IsSupportUpgrade
        );
    }

    setAgentList(isFirstOrReset) {
        if (isJPMCFeatureEnabled) {
            this.loadAgents(isFirstOrReset);
            return; 
        }

        $$.loading(true);
        let option = {
            url: "/api/CPAgentMgmtApi/GetAllAgents",
            method: "POST",
        };
        fetchUtility(option)
            .then((res) => {
                //前台分页；
                this.setState({
                    allAgentList: res,
                    pagerIndex: 0,
                    totalCount: res.length,
                    agentList: RM.deepcopy(res).slice(0, this.state.pagerSize),
                    shownCount: RM.deepcopy(res).slice(0, this.state.pagerSize).length,
                });
                let [hasMismatchedAgent, hasMinorVersionMismatchedAgent] = [false, false];
                if (isFirstOrReset && (res != null) && (res.length > 0)) {
                    res.map((item) => {
                        if (item.Status == 5) {
                            hasMismatchedAgent = true;
                        }
                    });
                    if(!hasMismatchedAgent)
                    {
                        hasMinorVersionMismatchedAgent = res.some(o => this.isLowAgentMinorVersion(o.Version));
                    }
                    this.setExpiredCertificateTip(res);
                }
                if (hasMismatchedAgent) {
                    let tipType = "warn";
                    let tipMsg = (
                        <$g.I18NProvider
                            msg={RMResx.RM_CP_Agent_NeedUpdatedVersion}
                        >
                            <a className="ra-link-a" style={{ color: "#0072d0", cursor: "pointer" }} onClick={this.downloadInstaller}>
                                {RMResx.RM_CP_Agent_DownloadVersion}
                            </a>
                        </$g.I18NProvider>
                    );
                    this.handleShowMessageBar(tipType, tipMsg);
                }

                if(hasMinorVersionMismatchedAgent) {
                    let tipType = "info";
                    let tipMsg = (
                        <$g.I18NProvider
                            msg={RMResx.RM_CP_Agent_NewMinorVerisionAvailable}
                        >
                            <a className="ra-link-a" style={{ color: "#0072d0", cursor: "pointer" }} onClick={this.downloadInstaller}>
                                {RMResx.RM_CP_Agent_DownloadVersion}
                            </a>
                        </$g.I18NProvider>
                    );
                    this.handleShowMessageBar(tipType, tipMsg);
                }
                
                $$.loading(false);
            })
            .catch((e) => {
                $$.loading(false);
            });
    }

    upgradeAgentBtnShowEvent = (checkedAgentList) => {

        this.setState({
            upgradeAgentBtnShow: this.shouldShowAgentBtn(checkedAgentList),
        });
    }

    onRowEvent = (args) => {
        let rowData = args.rowData;
        switch (args.type) {
            case this.agentActions.Download:
                this.openDownloadConfigFileDialog(rowData);
                break;
            case this.agentActions.Delete:
                this.openDeleteAgentMsgBox(rowData);
                break;
            case this.agentActions.Edit:
                this.createOrEditAgent(this.agentActions.Edit, rowData);
                break;
            case this.agentActions.Disable:
                this.disableAgent(rowData);
                break;
            case this.agentActions.Enable:
                this.enableAgent(rowData);
                break;
        }
        this.setState({ selectedAgentInfo: rowData });
    };

    onCheck = (list) => {
        this.setState({ checkedAgentList: list }, () => {
            this.upgradeAgentBtnShowEvent(list);
        });
    }

    deleteAgent = (rowData) => {
        $$.loading(true);
        let option = {
            url: "/api/CPAgentMgmtApi/DeleteAgent",
            method: "POST",
            data: rowData.Id
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            let tipType = 'success';
            let tipMsg = RMResx.RM_CP_Agent_Delete_Success;
            switch(res) {
                case "1":
                    tipType = 'error';
                    tipMsg = RMResx.RM_CP_Agent_Delete_Failed;
                    break;
                case "-1":
                    tipType = 'error';
                    tipMsg = RMResx.RM_Multi_Geo_Update_Common_ErrorMessage;
                    break;
            }

            this.showMsgToast(tipMsg,tipType,true);
            this.setAgentList();
            $$.messagedialog(false);
        }).catch((e) => {
            $$.loading(false);
        });
    };

    onCancleDeleteAgent = () => {
        $$.messagedialog(false);
    };

    openDeleteAgentMsgBox = (rowData) => {
        $$.messagedialog(true, {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_CP_Agent_DeleteMsgBoxTitle,
            content: RMResx.RM_CP_Agent_DeleteMsgBoxContent,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: this.onCancleDeleteAgent
                },
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: this.deleteAgent.bind(this, rowData)
                }
            ]
        });
    };

    getCrtOrEdtPanelBtns() {
        return <>
            <R.Button
                slot="buttons"
                text={RMResx.RM_JS_Common_Cancel}
                onClick={() => {
                    this.setState({ crtOrEdtPanelShow: false });
                }}
            />
            {isMultiGeoMainDC && <R.Button
                slot="buttons"
                primary
                classify="theme"
                text={RMResx.RM_JS_Common_Save}
                onClick={this.saveAgentInfo}
            />}
        </>
    }

    createOrEditAgent = (action) => {
        let crtOrEdtPanelTitle = "";
        if (action == this.agentActions.Create) {
            crtOrEdtPanelTitle = RMResx.RM_CP_Agent_Register;
            this.actionType = this.agentActions.Create;
            this.setState({ selectedAgentInfo: {} });
        }
        if (action == this.agentActions.Edit) {
            crtOrEdtPanelTitle = RMResx.RM_CP_Agent_EditAgent;
            this.actionType = this.agentActions.Edit;
        }
        this.setState({
            crtOrEdtPanelShow: true,
            crtOrEdtPanelTitle: crtOrEdtPanelTitle
        });
    };

    onHideCrtOrEdtPanel = () => {
        this.setState({ crtOrEdtPanelShow: false });
    };

    setCreateAgentMsg (result) {
        let type = 'success';
        let msg = RMResx.RM_CP_Agent_CreateAgentSuccess;
        if (result != this.crtOrEdtResult.Succeed) {
            type = 'error';
            switch (result) {
                case this.crtOrEdtResult.NoClientId:
                    msg = RMResx.RM_CP_Agent_Create_NeedClientId;
                    break;
                case this.crtOrEdtResult.NoCertificate:
                    msg = RMResx.RM_CP_Agent_Create_NeedCertificate;
                    break;
                case this.crtOrEdtResult.NoFailed:
                    msg = RMResx.RM_CP_Agent_CreateAgentFail;
                    break;
                default:
            }
        }
        this.showMsgToast(msg,type);
    }

    saveAgentInfo = () => {
        let callback = (success, data) => {
            if (this.actionType == this.agentActions.Create) {
                this.setCreateAgentMsg(data);
            } else {
                let tipType = "success" ;
                let tipMsg = RMResx.RM_CP_Agent_EditAgentSuccess;
                switch (data) {
                    case "-1":
                       tipType = "error";
                       tipMsg = RMResx.RM_Multi_Geo_Update_Common_ErrorMessage;
                       break;
                    case "1":
                       tipType = "error";
                       tipMsg = RMResx.RM_CP_Agent_EditAgentFail;
                       break;
                }
            
                this.showMsgToast(tipMsg, tipType);
            }
            this.setAgentList();
            this.setState({ crtOrEdtPanelShow: false });
            $$.loading(false);
        };
        this.dispatch('raCreateOrEditForm', callback, this.state.selectedAgentInfo);
        return false;
    };

    openRemindDialog = () => {
        this.dispatch('raRemindDialog');
    };

    openClientIdDialog = () => {
        this.setState({
            clientId: "",
            clientIdDiaShow: true,
            showClientIdMsg: false,
        });
        this.setClientId();
    };

    onClientIdChange = (value) => {
        this.clientId = value;
    };

    setClientId = () => {
        $$.loading(true);
        let option = {
            url: "/api/CPAgentMgmtApi/GetClientId",
            method: "POST",
        };
        fetchUtility(option).then((res) => {
            this.originClientId = RM.deepcopy(res);
            this.setState({ clientId: res });
            this.clientId = res;
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    handleClientIdDiaSaveBtn = () => {
        if (!this.clientId) {
            this.setState({ showClientIdMsg: true });
            return;
        }
        if(this.originClientId && this.originClientId != this.clientId){
            this.openModifyClientIdMsgBox();
            return;
        }
        this.saveClientId();
    };

    handleModCltIdMsgBoxSaveBtn = () => {
        $$.messagedialog(false);
        this.saveClientId();
    }

    saveClientId(){
        $$.loading(true);
        let option = {
            url: "/api/CPAgentMgmtApi/SaveClientId",
            method: "POST",
            data: this.clientId
        };
        fetchUtility(option)
            .then((res) => {
                this.setState({
                    showClientIdMsg: false,
                    clientIdDiaShow: false,
                });
                let tipType = "success";
                let tipMsg = RMResx.RM_CP_Agent_SaveClientIdSuccess
                switch(res) {
                    case "1":
                        tipType = "error";
                        tipMsg = RMResx.RM_CP_Agent_SaveClientIdFail;
                        break;
                    case "-1":
                        tipType = "error";
                        tipMsg = RMResx.RM_Multi_Geo_Update_Common_ErrorMessage;
                        break;
                }
                this.showMsgToast(tipMsg,tipType);
                $$.loading(false);
            })
            .catch((e) => {
                $$.loading(false);
            });
    }

    closeClientIdDialog = () => {
        this.setState({
            showClientIdMsg: false,
            clientIdDiaShow: false,
        });
    }

    
    openModifyClientIdMsgBox = (rowData) => {
        $$.messagedialog(true, {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_CP_Agent_ModifyClientIDDiaTitle,
            content: RMResx.RM_CP_Agent_ModifyClientID_Content,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: ()=>{
                        $$.messagedialog(false);
                        this.setState({clientIdDiaShow: true});
                    }
                },
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: this.handleModCltIdMsgBoxSaveBtn
                }
            ]
        });
        this.setState({clientIdDiaShow: false});
    };

    openDownloadConfigFileDialog = (rowData) => {
        if (!isMultiGeoMainDC) {
            showToast.error(RMResx.RM_Multi_Geo_Update_Common_ErrorMessage);
            return;
        }
        this.dispatch('raCPAgentDownloadConfigFileDialog', rowData);
    }

    openCertificateDialogDialog = () => {
        this.dispatch('raCertificateDialog');
    }

    downloadInstaller = () => {
        if (!isMultiGeoMainDC) {
            showToast.error(RMResx.RM_Multi_Geo_Update_Common_ErrorMessage);
            return;
        } 
        window.location.href = this.state.agentInstallerURL;
    }

    referesh = () => {
        this.setAgentList(true);
    }

    openUpgradeDialog = () => {
        this.setState({
            upgradeDiaShow: true,
        });
    }

    closeUpgradeDialog = () => {
        this.setState({
            upgradeDiaShow: false,
        });
    }
    
    handleUpgradeDiaSaveBtn = () => {
        $$.loading(true);
        let option = {
            url: "/api/CPAgentMgmtApi/UpgradeCloudAgent",
            method: "POST",
            data: {
                mode: 0,
                agentsId: []
            }
        };
        if (this.state.checkedAgentList.length > 0) {
            option.data.mode = 1;
            option.data.agentsId = this.state.checkedAgentList.map((item) => item.Id);
        }
        fetchUtility(option).then((res) => {
            this.setState({
                upgradeDiaShow: false,
            });
            let tipType = res === 0 ? "success" : "error" 
            let tipMsg = agentUpgradeResult[res] || RMResx.RM_CP_Agent_SaveUpgradeFail;
            this.showMsgToast(tipMsg, tipType);
            this.setAgentList();
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
       
    };

    disableAgent = (rowData) => {
        $$.loading(true);
        let option = {
            url: "/api/CPAgentMgmtApi/DisableAgent",
            method: "POST",
            data: rowData.Id
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            this.setAgentList();
            let tipType = 'success';
            let tipMsg = RMResx.RM_CP_Agent_DisabledAgentSuccess;
            switch(res) {
                case "1":
                    tipType = 'error';
                    tipMsg = RMResx.RM_CP_Agent_DisabledAgentFail;
                    break;
                case "-1":
                    tipType = 'error';
                    tipMsg = RMResx.RM_Multi_Geo_Update_Common_ErrorMessage;
                    break;
            }

            this.showMsgToast(tipMsg,tipType,true);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    enableAgent = (rowData) => {
        $$.loading(true);
        let option = {
            url: "/api/CPAgentMgmtApi/EnableAgent",
            method: "POST",
            data: rowData.Id,
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            this.setAgentList();
            let tipType = 'success';
            let tipMsg = RMResx.RM_CP_Agent_EnabledAgentSuccess;
            switch(res) {
                case "1":
                    tipType = 'error';
                    tipMsg = RMResx.RM_CP_Agent_EnabledAgentFail;
                    break;
                case "-1":
                    tipType = 'error';
                    tipMsg = RMResx.RM_Multi_Geo_Update_Common_ErrorMessage;
                    break;
            }

            this.showMsgToast(tipMsg,tipType,true);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    handlePageChange = (pagerIndex, pagerSize, callback) => {
        let currentPageItems = this.state.allAgentList.slice(pagerIndex * pagerSize, (pagerIndex + 1) * pagerSize);
        this.setState({
            pagerIndex: pagerIndex,
            pagerSize: pagerSize,
            shownCount: currentPageItems.length,
            agentList: currentPageItems,
        }, () => { if (isJPMCFeatureEnabled) this.loadAgents(); });
        callback(true);
    };

    renderSiteMap () {
        return <div>
            <$g.SiteMap data={[SiteMapLinks.CP, SiteMapLinks.CP_AgentManagement]} />
        </div>;
    }

    renderMessageBar () {
        return <R.Messagebar
            message={this.state.tipMsg}
            classify={this.state.tipType}
            status={{ show: this.state.showTip }}
            onClose={this.handleHideMessageBar}
        />;
    }

    renderMessageBarForCert () {
        return <R.Messagebar
            message={this.state.tipMsgForCert}
            classify={this.state.tipTypeForCert}
            status={{ show: this.state.showTipForCert }}
            onClose={this.handleHideMessageBarForCert}
        />;
    }

    renderNavBar() {
        return (
            <div className="ra-main-navbar">
                <div className="ra-nav-bar-left">
                    <R.Button
                        id="raCpAgentRegistBtnInNavBar"
                        primary={true}
                        classify="theme"
                        text={RMResx.RM_CP_Agent_Register}
                        onClick={this.createOrEditAgent.bind(
                            this,
                            this.agentActions.Create
                        )}
                    />
                    <R.Button
                        id="raCpAgentClientIdBtnInNavBar"
                        type="button"
                        icon="fia-client-id"
                        text={RMResx.RM_CP_Agent_ClientID}
                        tooltip={RMResx.RM_CP_Agent_ClientID}
                        onClick={this.openClientIdDialog}
                    />
                    <R.Button
                        id="raCpAgentCertBtnInNavBar"
                        type="button"
                        icon="fia-certificate"
                        text={RMResx.RM_CP_Agent_Certificate}
                        tooltip={RMResx.RM_CP_Agent_Certificate}
                        onClick={this.openCertificateDialogDialog}
                    />
                    <R.Button
                        id="raCpAgentDownloadInstallerBtnInNavBar"
                        type="button"
                        icon="fia-download-package"
                        text={RMResx.RM_CP_Agent_DownloadInstaller}
                        tooltip={RMResx.RM_CP_Agent_DownloadInstaller}
                        onClick={this.downloadInstaller}
                    />
                    <R.ButtonGroup
                        type="action"
                        classify="default"
                    >
                       {RM.gData.enableJPMCFileSystemFeature && (
                        <>
                            {this.state.upgradeAgentBtnShow && (
                                <R.Button
                                    text={RMResx.RM_CP_Agent_Upgrade}
                                    onClick={this.openUpgradeDialog}
                                />
                            )}
                            {!this.state.checkedAgentList.length && !this.state.upgradeAgentBtnShow && (
                                <R.Button
                                    text={RMResx.RM_CP_Agent_UpgradeAll}
                                    onClick={this.openUpgradeDialog}
                                />
                            )}
                        </>
                       )}
                        <R.Button
                            text={RMResx.RM_CP_Agent_Refresh}
                            onClick={this.referesh} />
                    </R.ButtonGroup>
                </div>
                <div className="ra-nav-bar-right">
                    <R.Button
                        type="icon"
                        icon="fia-status-info"
                        tooltip={RMResx.RM_CP_Agent_Remind}
                        className="nav-right-btn"
                        onClick={this.openRemindDialog}
                    />
                </div>
            </div>
        );
    }

    renderAgentTable() {
        return (
            <div className="ra-main-table">
                <div>
                    <R.Table
                        id="raAgentManagementTable"
                        rootData={this.state.agentLatestVersion}
                        columns={this.state.columns}
                        rowTemplate={TableRow}
                        items={this.state.agentList}
                        checkable={!!RM.gData.enableJPMCFileSystemFeature}
                        onCheck={this.onCheck}
                        onRowEvent={this.onRowEvent}
                    />
                </div>
            </div>
        );
    }

    renderPager() {
        return (
            <div className="ra-main-footer">
                <$g.Pager
                    itemsCount={this.state.totalCount}
                    pagerIndex={this.state.pagerIndex}
                    pagerSize={this.state.pagerSize}
                    showPagerSize={true}
                    showPagerCounter={true}
                    pagerSizeOptions={[5, 10, 15]}
                    onChange={this.handlePageChange}
                />
            </div>
            // </div>
        );
    }

    renderCreateOrEditAgentPanel() {
        return <R.Panel
            id="raCrtOrEdtPanel"
            header={this.state.crtOrEdtPanelTitle}
            size={600}
            status={{ show: this.state.crtOrEdtPanelShow }}
            onHide={this.onHideCrtOrEdtPanel}
            destroy={true}
        >
            <div>
                <CreateOrEditForm
                    id='raCreateOrEditForm'
                    data={this.state.selectedAgentInfo}
                ></CreateOrEditForm>
            </div>
            {this.getCrtOrEdtPanelBtns()}
        </R.Panel>;
    }

    renderRemindDialog () {
        return <RemindDialog id="raRemindDialog" actionClick={this.openClientIdDialog} appRegisterURL={this.state.appRegisterURL}></RemindDialog>;
    }

    renderClientIdDialog () {
        return <R.Dialog
            id="raCPAgentClientIdDialog"
            header={RMResx.RM_CP_Agent_ClientID}
            width={464}
            status={{ show: this.state.clientIdDiaShow }}
            
            destroy={true}
            onClose={this.closeClientIdDialog}
            // buttons={[
            //     {
            //         text: RMResx.RM_JS_Common_Cancel,
            //         disabled: false,
            //         onClick: this.closeClientIdDialog
            //     },
            //     {
            //         text: RMResx.RM_JS_Common_Save,
            //         primary: true,
            //         classify: "theme",
            //         disabled: false,
            //         onClick: this.handleClientIdDiaSaveBtn
            //     }   
            // ]}
        >
            <div>
                <$g.FormRow label={RMResx.RM_CP_Agent_ClientID_ContentTitle}>
                    <R.Input
                        id="raCpAgentClientIdIpt"
                        type="text"
                        width={420}
                        value={this.state.clientId}
                        onChange={this.onClientIdChange}
                        aria={{ariaLabel:RMResx.RM_CP_Agent_ClientID_ContentTitle}}
                    />
                    <$g.ValidationMsg show={this.state.showClientIdMsg}>
                        {RMResx.RM_CP_Agent_Valid_ClientId}
                    </$g.ValidationMsg>
                </$g.FormRow>
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.closeClientIdDialog} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.handleClientIdDiaSaveBtn} />
            </>
        </R.Dialog>;
    }


    renderDownloadConfigFileDialog () {
        return <DownloadConfigFileDialog
            id="raCPAgentDownloadConfigFileDialog"
            callback={this.handleShowMessageBar}
            data={this.state.selectedAgentInfo}
        >
        </DownloadConfigFileDialog>;
    }

    renderCertificateDialog () {
        return <CertificateDialog id="raCertificateDialog" appRegisterURL={this.state.appRegisterURL}></CertificateDialog>;
    }

    renderUpgradeAgentDialog () {
        return <R.Dialog
            id="raCPAgentUpgradeDialog"
            header={RMResx.RM_CP_Agent_Upgrade_ContentTitle}
            width={464}
            status={{ show: this.state.upgradeDiaShow }}
            
            destroy={true}
            onClose={this.closeUpgradeDialog}
        >
            <div>
                {RMResx.RM_CP_Agent_Upgrade_Intro}
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.closeUpgradeDialog} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_OK} onClick={this.handleUpgradeDiaSaveBtn} />
            </>
        </R.Dialog>;
    }

    loadAgents(isFirstOrReset = false) {
        $$.loading(true);
        let option = {
            url: "/api/CPAgentMgmtApi/QueryAgents",
            method: "POST",
            data: {
                PageIndex: isFirstOrReset ? 1 : this.state.pagerIndex + 1,
                PageSize: this.state.pagerSize,
                SearchValue: this.state.searchValue,
            }
        };
        fetchUtility(option).then((res) => {
            this.setState({
                pagerIndex: isFirstOrReset ? 0 : this.state.pagerIndex,
                totalCount: res.TotalCount,
                agentList: res.Agents,
                shownCount: res.Agents.length,
            });
            let tipType = "";
            let tipMsg = "";
            if (res.HasMismatchedAgent) {
                tipType = "warn";
                tipMsg = (
                    <$g.I18NProvider
                        msg={RMResx.RM_CP_Agent_NeedUpdatedVersion}
                    >
                        <a className="ra-link-a" style={{ color: "#0072d0", cursor: "pointer" }} onClick={this.downloadInstaller}>
                            {RMResx.RM_CP_Agent_DownloadVersion}
                        </a>
                    </$g.I18NProvider>
                );
                this.handleShowMessageBar(tipType, tipMsg);
            } else if (res.HasMinorVersionMismatchedAgent) {
                tipType = "info";
                tipMsg = (
                    <$g.I18NProvider
                        msg={RMResx.RM_CP_Agent_NewMinorVerisionAvailable}
                    >
                        <a className="ra-link-a" style={{ color: "#0072d0", cursor: "pointer" }} onClick={this.downloadInstaller}>
                            {RMResx.RM_CP_Agent_DownloadVersion}
                        </a>
                    </$g.I18NProvider>
                );
                this.handleShowMessageBar(tipType, tipMsg);
            }
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    onSearch = (value) => {
        let searchValue = $.trim(value);

        this.setState({
            searchValue: searchValue,
            pagerIndex: 0
        }, () => {
            this.loadAgents();
        });
    }

    renderSearchBox() {
        return (
            <div className="ra-main-header">
                <R.Searchbox
                    placeholder={RMResx.RM_CP_Agent_Placeholder_SearchBox}
                    disabled={false}
                    onSearch={this.onSearch}
                    width={380}
                />
            </div>
        );
    }

    render() {
        return (
            <div id="raAgentManagement">
                {this.renderSiteMap()}
                <div className="flex flex-column gap-m">
                    {this.renderMessageBarForCert()}
                    {this.renderMessageBar()}
                </div>
                <div className="ra-page-container margin-top-m">
                    {isJPMCFeatureEnabled && this.renderSearchBox()}
                    {isMultiGeoMainDC && this.renderNavBar()}
                    {this.renderAgentTable()}
                    {this.renderPager()}
                    {this.renderCreateOrEditAgentPanel()}
                    {this.renderRemindDialog()}
                    {this.renderClientIdDialog()}
                    {this.renderDownloadConfigFileDialog()}
                    {this.renderCertificateDialog()}
                    {this.renderUpgradeAgentDialog()}
                </div>
            </div>
        );
    }
}


