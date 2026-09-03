import { Component } from "react";
import SiteMapLinks from "../../Constants/SiteMapLinks";
import RouterUrls from "../../Constants/RouterUrls";
import {bindEvents, getUserGuildTagPage, LicenseHelper, showToast} from "../../Utilities/CommonUtil";
import Upload from "./Upload";
import "../../Less/CP/exportSettings.less";
import Enviroments from "../../Constants/Enviroments";
import { storageKeys } from "../../Utilities/Constant";

const messageType = {
    Successful: 0,
    Failed: 1,
    Exception: 2,
}

export default class ExportSettings extends Component {
    constructor(props) {
        super(props);
        this.state = {
            fileTypes: "zip",
            fileSize: 5,                       //文件大小要求
            tempUploadLists: [],               //tem信息
            nnaUploadLists: [],                //nna信息
            naraUploadLists: [],               //nara信息
            MessageTipInfo: {
                showTip: false,
                type: "success",
                content: ""
            },                                //提示信息
            fileMessage: "",
            //判断初始化时是否有数据            （true 更改    false 未更改）
            temState: false,
            nnaState: false,
            naraState: false,
            //判断文件是否有变动                （true 变更    false 未变更）
            temChooseState: false,
            naaChooseState: false,
            naraChooseState: false,
            isTurnOnEncryption: false,
            encryptionKey: "",      // Fortify Issue Type: Key Management: Empty Encryption Key; Ignore Reason: init can be empty
            isShowEncryptByPassWord: true,
            exportLocationList: [],
            exportLocationId: "",
            currentStorageId: "",
            isTurnOnNARAPublicKey: false,
            hasUpgradeVEOV3: false,
            hasVEOV3Permission: false,
            isUploadVEOV3: false,
            exportVEOPublicKey: "",
            exportNARAPublicKey: "",      // Fortify Issue Type: Key Management: Empty Encryption Key; Ignore Reason: init can be empty
            isShowVEOPublicKeyPlainText: true,
            isShowNARAPublicKeyPlainText: true,
        };
        bindEvents(this, "onYesClick", "hideMessageTip", "onUploadZipFilesToServer", "onCancelExportSetting");
    }

    componentDidMount() {
        //获取回显数据
        this.GetSavedFilesFromServer(false);
    }

    routerTo(routerUrl) {
        this.props.history.push({
            pathname: routerUrl
        });
    }
    showMsgToast(content, type) {
        let option = {
            content: content,
            classify: type,
        };
        $$.toast(option);
    }
    //获取保存数据回显
    GetSavedFilesFromServer(isSave,dataObj) {
        $$.loading(true);
        let option = {
            url: "/api/CPApi/GetSavedFileInfos"
        };
        fetchUtility(option)
            .then((res) => {
                //初始化
                this.setState({
                    //每次调用所有状态初始化
                    //默认状态没有变更
                    temChooseState: false,
                    naaChooseState: false,
                    naraChooseState: false,
                    temState: false,
                    nnaState: false,
                    naraState: false,
                });
                if (isSave) {
                    // this.setState({
                    //     MessageTipInfo: {
                    //         showTip: true,
                    //         type: "success",
                    //         content: dataObj.message,
                    //     },
                    // });
                    this.showMsgToast(dataObj.message,"success",true);
                }

                //export location
                let exportList = [];
                res.StorageInfo.forEach(item => {
                    item.checked = (item.Id == res.CurrentExportLocationId) ? true : false;
                    exportList.push(item);
                });
                this.setState({
                    exportLocationList: exportList,
                    currentStorageId: res.CurrentExportLocationId,
                });

                for (let key of res.Settings) {
                    //Temp
                    let tempUploadListsArr = [];
                    let nnaUploadListsArr = [];
                    let naraUploadListsArr = [];
                    if (key.ExportSettingType == 0) {
                        tempUploadListsArr.push(key);
                        this.setState({
                            tempUploadLists: tempUploadListsArr,
                            temState: true,
                        });
                    }
                    //NAA
                    if (key.ExportSettingType == 1) {
                        nnaUploadListsArr.push(key);
                        this.setState({
                            nnaUploadLists: nnaUploadListsArr,
                            nnaState: true,
                        });
                    }
                    //NARA
                    if (key.ExportSettingType == 2) {
                        naraUploadListsArr.push(key);
                        this.setState({
                            naraUploadLists: naraUploadListsArr,
                            naraState: true,
                        });
                    }
                }

                this.setState({
                    isTurnOnEncryption: res.EncryptionEnabled,
                    encryptionKey: res.EncryptionKey,
                    isTurnOnNARAPublicKey: res.ExportNARADataChecksumEnabled,
                    exportVEOPublicKey: res.ExportVEOPublicKey,
                    exportNARAPublicKey: res.ExportNARAPublicKey,
                    hasUpgradeVEOV3: res.HasUpgradeVEOV3,
                    hasVEOV3Permission: res.HasVEOV3Permission
                });
                $$.loading(false);
            })
            .catch((e) => {
                this.setState({generateBtnDis: true});
                showToast.error(RMResx.RM_RDM_Explorer_ChangeTerm_All_Failed);
                $$.loading(false);
            });
    }

    stringFormat() {
        if (arguments.length == 0)
            return null;
        let str = arguments[0];
        for (let i = 1; i < arguments.length; i++) {
            let re = new RegExp("\\{" + (i - 1) + "\\}", "gm");
            str = str.replace(re, arguments[i]);
        }
        return str;
    }

    //messsagebox展示
    showTimoutMsg() {
        $$.messagedialog(true, {
            // classify: "info",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_ES_OverridePrompt,
            buttons: [
                {text:RMResx.RM_JS_Common_Cancel, onClick: this.onNoClick},
                {text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: this.onYesClick}    
            ]
        });
    }

    //点确定
    onYesClick() {
        $$.messagedialog(false);
        this.APISave();
    }

    //点取消
    onNoClick() {
        $$.messagedialog(false);
    }

    //点击保存(有变动弹出提示，没有变动直接提交)
    onUploadZipFilesToServer() {
        if (this.state.temChooseState || this.state.naaChooseState || this.state.naraChooseState) {
            //当文件改变时提示
            this.showTimoutMsg();
        } else {
            //不变化是直接提交
            this.APISave();
        }
    }

    //保存接口
    APISave() {
        if (!$$.verify(this.allValidation)) {
            return false;
        }
        let self = this;
        $$.loading(true);
        let ajaxFormOption = {
            type: "POST",
            url: "/CP/ExportSettignsUploadCoinfig",
            success: function (data) {
                $$.loading(false);
                if (data) {
                    var dataObj = data;
                    if (dataObj.success) {
                        $$.loading(false);
                        //刷新列表
                        self.GetSavedFilesFromServer(true,dataObj);
                    } else {
                        $$.loading(false);
                        // self.setState({
                        //     MessageTipInfo: {
                        //         showTip: true,
                        //         type: "error",
                        //         content: dataObj.message,
                        //     },
                        // });
                        self.showMsgToast(dataObj.message,"error",true);
                    }
                }
            },
            error: function (dataObj) {
                $$.loading(false);
                // self.setState({
                //     MessageTipInfo: {
                //         showTip: true,
                //         type: "error",
                //         content: "Request Error",
                //     },
                // });
                self.showMsgToast("Request Error","error",true);
            },
        };
        $("#form-import").ajaxSubmit(ajaxFormOption);
    }

    //选择文件成功的状态
    chooseFileSuccess(file) {
        //判断状态是否有更改
        if (file.element_name == "fileUp" && this.state.temState) {
            this.setState({
                temChooseState: true
            });
        }
        if (file.element_name == "nnaFileUp" && this.state.nnaState) {
            this.setState({
                naaChooseState: true
            });
        }
        if (file.element_name == "naraFileUp" && this.state.naraState) {
            this.setState({
                naraChooseState: true
            });
        }
        if (file.fileMessage) {
            // this.setState({
            //     MessageTipInfo: {
            //         showTip: true,
            //         type: "error",
            //         content: file.fileMessage,
            //     },
            // });
            this.showMsgToast(file.fileMessage,"error",true);
        } else {
            this.setState({
                MessageTipInfo: {
                    showTip: false
                }
            });
        }
    }

    //跳回主页
    onCancelExportSetting() {
        this.props.history.push({
            pathname: RouterUrls.CP_Index
        });
    }

    //删除（不算变更）
    temDeleteFileSuccess() {
        this.setState({
            temChooseState: false
        });
    }

    nnaDeleteFileSuccess() {
        this.setState({
            naaChooseState: false
        });
    }

    naraDeleteFileSuccess() {
        this.setState({
            naraChooseState: false
        });
    }

    hideMessageTip() {
        this.setState({
            MessageTipInfo: {
                showTip: false,
            },
        });
    }

    onChangeTurnEncryptionSwicth = (checked) =>{
        this.setState({isTurnOnEncryption: checked});
        if(checked){
            let option = {
                url: "/api/CPAPI/GetCurrentAesKey",
                method: "POST",
            };
            $$.loading(true);
            fetchUtility(option).then((res) => {
                $$.loading(false);
                if(res.MessageType == messageType.Successful){
                    if(res.Extension){
                        this.setState({encryptionKey: res.Extension});
                    }else{
                        this.onClickGenerateBtn();
                    }
                }else{
                    this.setState({generateBtnDis: true});
                    showToast.error(RMResx.RM_ES_GetEncryptFail);
                }
            }).catch((e) => {
                this.setState({generateBtnDis: true});
                showToast.error(RMResx.RM_ES_GetEncryptFail);
                $$.loading(false);
            });
        }
    }

    showModifyeEncryptionMsgbox = () =>{
        $$.messagedialog(true, {
            width: "550px",
            classify: "warn",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_ES_ChangeEncryptTip, //需要确认是否要加上之前的encryption
            buttons: [
                {text:RMResx.RM_JS_Common_Cancel, onClick: ()=>{$$.messagedialog(false);}},
                {text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: this.onClickGenerateBtn}    
            ]
        });
    }

    onChangeUploadVEOV3 = (value) => {
        this.setState((prev) => ({
            isUploadVEOV3: prev.hasVEOV3Permission ? (prev.hasUpgradeVEOV3 ? true : value ) : false
        }));
    }

    onClickGenerateBtn = () =>{
        let option = {
            url: "/api/CPAPI/GenerateAesKey",
            method: "POST",
        };
        $$.loading(true);
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if(res.MessageType == messageType.Successful){
                this.setState({ encryptionKey: res.Extension });
                showToast.success(RMResx.RM_ES_ChangeEncryptSuccess);
            }else{
                showToast.error(RMResx.RM_ES_ChangeEncryptFail);
            }
        }).catch((e) => {
            showToast.error(RMResx.RM_ES_ChangeEncryptFail);
            $$.loading(false);
        });
    }

    onClickCopyBtn = () =>{
        let downloadConfigFileCodeInput = document.querySelector("#raCpEsEncryptionInput input");
        downloadConfigFileCodeInput.select();
        document.execCommand("copy");
        this.setState({ downloadConfigFileBtnDis: false });
    }

    onClickEncryptedEyeBtn = () =>{
        this.setState({ isShowEncryptByPassWord: !this.state.isShowEncryptByPassWord });
    }

    onExportLocationChanged = (args) => {
        this.setState({ exportLocationId: args.newValue.Id });
    }

    onClickCopy4PublicKeyBtn = (selector) => {
        let exportNARAPublickeyInput = document.querySelector(selector);
        exportNARAPublickeyInput.select();
        //document.execCommand("copy");
        navigator.clipboard.writeText(exportNARAPublickeyInput.value);
    }

    onChangeTurnNARAPublicKeySwicth = (checked) => {
        this.setState({ isTurnOnNARAPublicKey: checked });
    }

    onClickVEOPublicKeyEyeBtn = () => {
        this.setState({ isShowVEOPublicKeyPlainText: !this.state.isShowVEOPublicKeyPlainText });
    }
    
    onClickNARAPublicKeyEyeBtn = () => {
        this.setState({ isShowNARAPublicKeyPlainText: !this.state.isShowNARAPublicKeyPlainText });
    }

    getUserGuideLink(tag) {
        if (RM.gData.enviromentName == Enviroments.ChinaNorth) {
            return "https://cdn.avepoint.com/pdfs/cn/user_guides/AvePoint_Opus_User_Guide.pdf";
        }

        return getUserGuildTagPage(tag);
    }

    renderPublicKeyEyeBtns(isShowPublicKeyPlainText, onClick) {
        let iconClassName = isShowPublicKeyPlainText ? "fia-eye" : "fia-eye-slash";
        return <div className="es-encryption-eye-icon" onClick={onClick}>
            <span className={iconClassName} tabIndex="0" role="button" aria-label={RMResx.RM_Common_ShowPassword} aria-pressed={!isShowPublicKeyPlainText}></span>
        </div>;
    }

    renderTurnVEOPublicKey() {
        let isShowVEOPublicKeyPlainText = this.state.isShowVEOPublicKeyPlainText;
        return <div className="margin-top-m">
            <div>
                <span className="margin-left-s strong">{RMResx.RM_ES_TurnOnExportPublicKeyBtn_VEO}</span>
                <$g.Popover>{RMResx.RM_ES_TurnOnExportPublicKeyDescription_VEO}</$g.Popover>
            </div>
            {
                <div className="es-encryption-content">
                    <R.Input
                        id="raCpEsVEOPublicKeyInput"
                        className="margin-right-m"
                        type={isShowVEOPublicKeyPlainText ? "password" : "text"}
                        width={450}
                        value={this.state.exportVEOPublicKey}
                        readonly={true}
                    />
                    {this.renderPublicKeyEyeBtns(isShowVEOPublicKeyPlainText, this.onClickVEOPublicKeyEyeBtn)}
                    {
                        !isShowVEOPublicKeyPlainText && <R.Button
                            id="raCpEsPublicKeyCopy"
                            text={RMResx.RM_ES_Encrypt_Copy}
                            onClick={() => this.onClickCopy4PublicKeyBtn("#raCpEsVEOPublicKeyInput input")}
                        />
                    }
                </div>
            }
        </div>;
    }

    renderTurnNRRADataChecksum() {
        let isShowNARAPublicKeyPlainText = this.state.isShowNARAPublicKeyPlainText;
        return <div className="margin-top-m">
            <div>
                <R.Switch
                    id="raCpEsNARAPublicKeySwicth"
                    checked={this.state.isTurnOnNARAPublicKey}
                    onChange={this.onChangeTurnNARAPublicKeySwicth}
                />
                <input type="hidden" name={"exportNARADataChecksumEnabled"} value={this.state.isTurnOnNARAPublicKey} />
                <span className="margin-left-s strong">{RMResx.RM_ES_TurnOnExportPublicKeyBtn}</span>
                <$g.Popover>{RMResx.RM_ES_TurnOnExportPublicKeyDescription}</$g.Popover>
            </div>
            {
                this.state.isTurnOnNARAPublicKey && <div className="es-encryption-content">
                    <R.Input
                        id="raCpEsNRRAPublicKeyInput"
                        className="margin-right-m"
                        type={isShowNARAPublicKeyPlainText ? "password" : "text"}
                        width={450}
                        value={this.state.exportNARAPublicKey}
                        readonly={true}
                    />
                    {this.renderPublicKeyEyeBtns(isShowNARAPublicKeyPlainText, this.onClickNARAPublicKeyEyeBtn)}
                    {
                        !isShowNARAPublicKeyPlainText && <R.Button
                            id="raCpEsPublicKeyCopy"
                            text={RMResx.RM_ES_Encrypt_Copy}
                            onClick={() => this.onClickCopy4PublicKeyBtn("#raCpEsNRRAPublicKeyInput input")}
                        />
                    }
                </div>
            }
        </div>;
    }

    renderEncryptedEyeBtns(isShowEncryptByPassWord){
        let icon = isShowEncryptByPassWord ? "fia-eye" : "fia-eye-slash";
        return <div className="es-encryption-eye-icon" onClick={this.onClickEncryptedEyeBtn}>
            <span className={icon} tabIndex="0" role="button" aria-label={RMResx.RM_Common_ShowPassword} aria-pressed={!isShowEncryptByPassWord}></span>
        </div>;
    }

    renderTurnEncryption(){
        let isShowEncryptByPassWord = this.state.isShowEncryptByPassWord;
        return <div>
            <div className="ra-section">
                <div>
                    <R.Switch 
                        id="raCpEsEncryptionSwicth" 
                        checked={this.state.isTurnOnEncryption}
                        onChange={this.onChangeTurnEncryptionSwicth} 
                    />
                    <input type="hidden" name={"exportEncryptionEnabled"} value={this.state.isTurnOnEncryption}/>
                    <span className="margin-left-s strong">{RMResx.RM_ES_TurnEncryptionBtn}</span>
                    <$g.Popover>{RMResx.RM_ES_Encryption_Introduce}</$g.Popover>
                </div>
                {
                    this.state.isTurnOnEncryption && <div className="es-encryption-content">
                        <R.Input
                            id="raCpEsEncryptionInput"
                            className="margin-right-m"
                            type={isShowEncryptByPassWord ? "password" : "text"}
                            width={450}
                            value={this.state.encryptionKey}
                            readonly={true} 
                        />  
                        {this.renderEncryptedEyeBtns(isShowEncryptByPassWord)}
                        <div className="es-encryption-button">
                            <R.Button
                                id="raCpEsEncryptionGenerate"
                                disabled={this.state.generateBtnDis}
                                text={RMResx.RM_ES_Encrypt_Generate}
                                onClick={this.showModifyeEncryptionMsgbox}
                            />
                            {
                                !isShowEncryptByPassWord && <R.Button
                                    id="raCpEsEncryptionCopy"
                                    text={RMResx.RM_ES_Encrypt_Copy}
                                    onClick={this.onClickCopyBtn}
                                />
                            }
                        </div>
                    </div>
                }
            </div>
        </div>;
    }

    renderExportLocation() {
        return <div className="ra-section">
            <div className="ra-section-head">
                <span tabIndex='0'>{RMResx.RM_AR_CP_ES_ExportType_ExportLocation}</span>
            </div>
            <div className="ra-form-label">
                <span>{RMResx.RM_AR_CP_ES_ExportLocation_Content}</span>
                <$g.Popover>{RMResx.RM_AR_CP_ES_ExportLocation_Popover}</$g.Popover>
            </div>
            <R.Combobox
                id="raExportLocationCom"
                tooltipField="Name"
                width='38%'
                textField="Name"
                valueField="Id"
                checkedField="checked"
                linkMode={false}
                searchable={false}
                items={this.state.exportLocationList}
                onChange={this.onExportLocationChanged}
                aria={{ ariaLabel: RMResx.RM_AR_CP_ES_ExportLocation_Content }}
            />
            <input type="hidden" name={"exportLocationId"} value={this.state.exportLocationId || this.state.currentStorageId} />
        </div>;
    }

    // view
    render() {
        return <div id="raExportSettings">
            <div className="flex justify-between align-center">
                <$g.SiteMap data={[SiteMapLinks.CP, SiteMapLinks.CP_ExportSettings]}/>
                {LicenseHelper.EnableRecordsArchiver() && ( // LicenseHelper.EnableRecordsArchiver(): check new logical account
                    <div className="margin-bottom-l flex-item text-end">
                        <R.Button
                            id="raCpExportSettingsCompliantBtn"
                            primary={true}
                            classify="theme"
                            text={RMResx.RM_ES_CompliantExport_Title}
                            onClick={() => {
                                this.props.history.push({
                                    pathname: RouterUrls.CP_ExportSettings_CompliantExports
                                });
                            }}
                        />
                    </div>
                )}
            </div>
        
            <div className="ra-page-main">
                <form id="form-import" encType="multipart/form-data" action="" method="post">
                    <R.Validation>
                        <div ref={r => this.allValidation = r}>
                            {this.renderExportLocation()}
                        </div>
                    </R.Validation>
                    {/*VEO*/}
                    <div className="ra-section">
                        <div className="ra-section-head">
                            <div className="flex align-center">
                                <span tabIndex='0'>{RMResx.RM_ES_ExportType_VEO}</span>
                                <$g.Popover>
                                    <$g.I18NProvider msg={RMResx.RM_ES_Description}>
                                        <a className="ra-link-a" href={this.getUserGuideLink(storageKeys.veoExportSetting)}>
                                            {RMResx.RM_ES_DescriptionGuide}
                                        </a>
                                    </$g.I18NProvider>
                                </$g.Popover>
                            </div>
                        </div>
                        <div className="ra-page-form">
                            <div className="ra-form-label ra-require-after">
                                <span tabIndex='0'>{RMResx.RM_ES_UploadConfiguration}</span>
                            </div>
                            <div className="ra-form-content">
                                <Upload
                                    fileTypes={this.state.fileTypes}
                                    fileSize={this.state.fileSize}
                                    downLoadUrl='/api/CPApi/DownloadTemplateZip'
                                    downLoadTemplateV3Url='/api/CPApi/DownloadVEOV3TemplateZip'
                                    uploadLists={this.state.tempUploadLists}
                                    multiple={false}
                                    savedFileUrl='/api/CPApi/DownSavedloadFile'
                                    chooseFileInputName='fileUp'
                                    noChangeStatusHiddenInputName='veoIsNoChangeDirectSave'
                                    hasUpgradeVEOV3={this.state.hasUpgradeVEOV3}
                                    hasVEOV3Permission = {this.state.hasVEOV3Permission}
                                    chooseFileSuccess={this.chooseFileSuccess.bind(this)}
                                    deleteFileSuccess={this.temDeleteFileSuccess.bind(this)}
                                    changeUploadVEOV3={this.onChangeUploadVEOV3.bind(this)}
                                >
                                </Upload>
                            </div>
                        </div>
                        {LicenseHelper.EnableRecordsArchiver() && this.renderTurnVEOPublicKey()}
                    </div>

                    {/*NNA*/}
                    <div className="ra-section">
                        <div className="ra-section-head">
                            <span tabIndex='0'>{RMResx.RM_ES_ExportType_NAA}</span>
                            <$g.Popover>
                                <$g.I18NProvider msg={RMResx.RM_ES_NAA_Description}>
                                    <a className="ra-link-a" href={this.getUserGuideLink(storageKeys.naaExportSetting)}>
                                        {RMResx.RM_ES_DescriptionGuide}
                                    </a>
                                </$g.I18NProvider>
                            </$g.Popover>
                        </div>
                        <div className="ra-page-form">
                            <div className="ra-form-label ra-require-after">
                                <span tabIndex='0'>{RMResx.RM_ES_UploadConfiguration}</span>
                            </div>
                            <div className="ra-form-content">
                                <Upload
                                    fileTypes={this.state.fileTypes}
                                    fileSize={this.state.fileSize}
                                    downLoadUrl='/api/CPApi/DownloadNAATemplateZip'
                                    uploadLists={this.state.nnaUploadLists}
                                    savedFileUrl='/api/CPApi/DownSavedloadNaaFile'
                                    multiple={false}
                                    chooseFileInputName='nnaFileUp'
                                    noChangeStatusHiddenInputName='naaIsNoChangeDirectSave'
                                    chooseFileSuccess={this.chooseFileSuccess.bind(this)}
                                    deleteFileSuccess={this.nnaDeleteFileSuccess.bind(this)}>
                                    {RMResx.RM_CP_GSS_StoragePolicy_Tip}
                                </Upload>
                            </div>
                        </div>
                    </div>

                    {/*NARA*/}
                    <div className="ra-section">
                        <div className="ra-section-head">
                            <span tabIndex='0'>{RMResx.RM_ES_ExportType_NARA}</span>
                            <$g.Popover>
                                <$g.I18NProvider msg={RMResx.RM_ES_NARA_Description}>
                                    <a className="ra-link-a" href={this.getUserGuideLink(storageKeys.naraExportSetting)}>
                                        {RMResx.RM_ES_DescriptionGuide}
                                    </a>
                                </$g.I18NProvider>
                            </$g.Popover>
                        </div>
                        <div className="ra-page-form">
                            <div className="ra-form-label ra-require-after">
                                <span tabIndex='0'>{RMResx.RM_ES_UploadConfiguration}</span>
                            </div>
                            <div className="ra-form-content">
                                <Upload
                                    fileTypes={this.state.fileTypes}
                                    fileSize={this.state.fileSize}
                                    downLoadUrl='/api/CPApi/DownloadNARATemplateZip'
                                    uploadLists={this.state.naraUploadLists}
                                    savedFileUrl='/api/CPApi/DownSavedloadNaraFile'
                                    multiple={false}
                                    chooseFileInputName='naraFileUp'
                                    noChangeStatusHiddenInputName='naraIsNoChangeDirectSave'
                                    chooseFileSuccess={this.chooseFileSuccess.bind(this)}
                                    deleteFileSuccess={this.naraDeleteFileSuccess.bind(this)}

                                >{RMResx.RM_CP_GSS_StoragePolicy_Tip}
                                </Upload>
                            </div>
                        </div>
                        {LicenseHelper.EnableRecordsArchiver() && this.renderTurnNRRADataChecksum()}
                    </div>
                    <input type="hidden" name="needToUpgradeVEOV3" value={this.state.isUploadVEOV3} />
                    {this.renderTurnEncryption()}
                </form>

                <div className="ra-foot-btns flex justify-end align-center gap-s">
                    <R.Button
                        text={RMResx.RM_JS_Common_Cancel}
                        onClick={this.onCancelExportSetting}/>
                    <R.Button
                        id="raCpEsSaveBtn"
                        primary={true}
                        classify="theme"
                        text={RMResx.RM_JS_Common_Save}
                        onClick={this.onUploadZipFilesToServer}/>                    
                </div>
            </div>
        </div>;

    }
}
