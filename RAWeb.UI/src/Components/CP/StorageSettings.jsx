import { Component } from "react";
import RouterUrls from "../../Constants/RouterUrls";
import SiteMapLinks from "../../Constants/SiteMapLinks";
import { bindEvents, setCheckedStatus, getCheckedItem } from "../../Utilities/CommonUtil";
import "../../Less/CP/storageSettings.less";

export default class StorageSettings extends Component {
    constructor(props) {
        super(props);
        this.state = {
            encryptionFlag: true,
            CompressionFlag: false,
            selectedFruit: "",
            AllStoragePolicy: [],//全部的数据存where
            CurrentStoragePolicy: {}, //当前数据存where
            AllExportLocation: [],//Where would you like to store the exported data（all）
            CurrentExportLocation: {},//Where would you like to store the exported data（当前）
            AllSecurityProfile: [], //Security Profile (all)
            CurrentSecurityProfile: {},//Security Profile (当前)
            CompressionSpeed: "",
            //注释的显示隐藏表示
            storagePolicyExplain: false,
            ExportLocationExplain: false,
            CompressionExplain: false,
            encryptionExplain: false,
            storagePolicyValidationShow: false,
            exportLocationvVlidationShow: false,
            securityProfileValidationShow: false,
            content: "",
            tipStatus: {show: false},
            step: 1,
            value: 5,
            min: 1,
            max: 9,
        };
        bindEvents(this, "hideMessageTip", "onSaveSettings", "onCancel");
    }

    //组件销毁时停止
    componentWillUnmount() {
    }
    componentDidMount() {
        //获取slider
        this.getStorageSettingsData();
    }

    showMsgToast(content,type){
        let option = {
            content : content,
            classify : type
        };
        $$.toast(option);
    }
    //点击取消按钮回到主页面
    onCancel() {
        this.props.history.push({
            pathname: RouterUrls.CP_Index
        });
    }

    //切换CurrentStoragePolicy
    onCurrentStoragePolicyChange(args) {
        this.selectedStoragePolicy = args.newValue;
        this.setState({
            storagePolicyValidationShow: false
        });
    }

    //切换CurrentExportLocation
    onCurrentExportLocationChange(args) {
        this.selectedCurrentExportLocation = args.newValue;
        this.setState({
            exportLocationvVlidationShow: false
        });
    }

    //切换CurrentSecurityProfile
    onCurrentSecurityProfileChange(args) {
        this.selectedSecurityProfile = args.newValue;
        this.setState({
            securityProfileValidationShow: false
        });
    }

    onSliderChange(args) {
        this.setState({ value: args });
    }

    //slider初始化
    getSliderInit(compressionSpeed) {
        this.setState({ value: compressionSpeed ? compressionSpeed : 5 });
    }

    getNoneItem() {
        return {
            ID: "00000000-0000-0000-0000-000000000000",
            Name: RMResx.RM_JS_RDM_CreateRule_ExportType_None
        };
    }

    //获取页面数据
    getStorageSettingsData() {
        let loadingtimer = setTimeout(function () {
            $$.loading(true);
        }, 100);
        let urlData = "/api/CPApi/GetStorageSettings";
        let option = {
            url: urlData,
            method: "GET"
        };
        fetchUtility(option)
            .then((res) => {
                let resObj = JSON.parse(res);
                //页面提示信息判断
                if (resObj.GSSExceptionType == 1) {
                    this.showMsgToast( resObj.ExceptionMsg, "error",true );
                }
                if (
                    resObj.CurExportLocationRemoved ||
                    resObj.CurSecurityProfileRemoved ||
                    resObj.CurStoragePolicyRemoved
                ) {
                    this.setState({
                        tipStatus: { show: true },
                        type: "warn",
                        content: RMResx.RM_JS_CP_GSS_UsedItemRemoved,
                    });
                }
                let allStoragePolicy = [
                    this.getNoneItem(),
                    ...resObj.AllStoragePolicy,
                ];
                if (resObj.CurrentStoragePolicy.ID == null) {
                    allStoragePolicy[0].Checked = true;
                } else {
                    this.selectedStoragePolicy = resObj.CurrentStoragePolicy;
                    setCheckedStatus(
                        "ID",
                        "Checked",
                        allStoragePolicy,
                        resObj.CurrentStoragePolicy
                    );
                }

                if (resObj.CurrentExportLocation.ID == null) {
                    resObj.AllExportLocation[0].Checked = true;
                } else {
                    this.selectedCurrentExportLocation = resObj.CurrentExportLocation;
                    setCheckedStatus("ID", "Checked", resObj.AllExportLocation, resObj.CurrentExportLocation);
                }

                if (resObj.CurrentSecurityProfile.ID == null) {
                    resObj.AllSecurityProfile[0].Checked = true;
                    this.selectedSecurityProfile = resObj.AllSecurityProfile[0];
                } else {
                    this.selectedSecurityProfile = resObj.CurrentSecurityProfile;
                    setCheckedStatus("ID", "Checked", resObj.AllSecurityProfile, resObj.CurrentSecurityProfile);
                }

                this.setState({
                    AllStoragePolicy: allStoragePolicy,
                    AllExportLocation: resObj.AllExportLocation,
                    AllSecurityProfile: resObj.AllSecurityProfile,
                    //checkbox 选中和不选中
                    encryptionFlag: resObj.UseEncryption,
                    CompressionFlag: resObj.UseCompression,
                    CompressionSpeed: resObj.CompressionSpeed,
                }, () => {
                    this.getSliderInit();
                });
                //slider初始化
                this.getSliderInit(this.state.CompressionSpeed);
                clearTimeout(loadingtimer);
                $$.loading(false);
            })
            .catch((e) => {
                clearTimeout(loadingtimer);
                $$.loading(false);
            });
    }

    //Encryptionde 的切换
    encryptionCheck() {
        this.setState({
            encryptionFlag: !this.state.encryptionFlag
        });
        if (!this.state.encryptionFlag) {
            this.setState({
                securityProfileValidationShow: false
            });
        }
    }

    // Compression切换
    CompressionCheck() {
        this.setState({
            CompressionFlag: !this.state.CompressionFlag
        }, () => {
            this.getSliderInit();
        });
    }

    //保存
    onSaveSettings() {
        let curSecurityProf = null;
        if (this.state.encryptionFlag == true) {
            curSecurityProf = this.selectedSecurityProfile;
            if (!curSecurityProf || !curSecurityProf.ID) {
                this.setState({
                    securityProfileValidationShow: true
                });
                return false;
            }
        }
        let param = {
            UseCompression: this.state.CompressionFlag,
            UseEncryption: this.state.encryptionFlag,
            CompressionSpeed: this.state.CompressionFlag ? this.state.value : "0",
            CompressionMethod: "4",
            EncryptionMethod: "8",
            CurrentStoragePolicy: this.selectedStoragePolicy,
            CurrentExportLocation: this.selectedCurrentExportLocation,
            CurrentSecurityProfile: curSecurityProf
        };
        let urlData = "/api/CPApi/StorageSettings";
        $$.loading(true);
        let option = {
            url: urlData,
            data: param
        };
        fetchUtility(option).then((res) => {
            var resultData = JSON.parse(res);
            if (resultData.MessageType === 0) { 
                this.showMsgToast(RMResx.RM_JS_Common_SaveSucess,"success",true);           
            } else {
                this.showMsgToast(resultData.ErrorMessage,"error",true);              
            }
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    hideMessageTip() {
        this.setState({
            tipStatus: {show: false}
        });
    }

    //view
    render() {
        return (
            <div id="raStorageSettings">
                <$g.SiteMap
                    data={[SiteMapLinks.CP, SiteMapLinks.CP_StorageSettings]}
                />
                <R.Messagebar
                    message={this.state.content}
                    classify={this.state.type}
                    onClose={this.hideMessageTip}
                    status={{ show: this.state.tipStatus.show }}
                />
                <div className="ra-page-main">
                    <div className="ra-form-label">
                        <span id="ariaArchivedData">
                            {RMResx.RM_CP_GSS_StoragePolicy_Question}
                        </span>
                        <$g.Popover>{RMResx.RM_CP_GSS_StoragePolicy_Tip}</$g.Popover>
                    </div>
                    <div className="ra-form-content">
                        <R.Combobox
                            id="raCpSsPolicy"
                            searchable={false}
                            textField="Name"
                            valueField="ID"
                            checkedField="Checked"
                            items={this.state.AllStoragePolicy}
                            onChange={this.onCurrentStoragePolicyChange.bind(
                                this
                            )}
                            placeholder=""
                            width="559"
                            aria="#ariaArchivedData"
                        />
                    </div>
                    <div className="ra-form-label">
                        <span id="ariaExportData">
                            {RMResx.RM_CP_GSS_ExportLocation_Question}
                        </span>
                        <$g.Popover>{RMResx.RM_CP_GSS_ExportLocation_Tip}</$g.Popover>
                    </div>
                    <div className="ra-form-content">
                        <R.Combobox
                            id="raCpSsExportLocation"
                            searchable={false}
                            textField="Name"
                            valueField="ID"
                            checkedField="Checked"
                            items={this.state.AllExportLocation}
                            onChange={this.onCurrentExportLocationChange.bind(
                                this
                            )}
                            placeholder=""
                            width="559"
                            aria="#ariaExportData"
                        />
                    </div>
                    <div className="ra-form-label">
                        <span>
                            {RMResx.RM_CP_GSS_DataHandle_Compression}
                        </span>
                        <$g.Popover>{RMResx.RM_CP_GSS_Compression_Tip}</$g.Popover>
                    </div>
                    <div className="ra-form-content">
                        <div>
                            <R.Checkbox
                                id="raCpSsCompressionChk"
                                text={RMResx.RM_CP_GSS_Compression}
                                title={RMResx.RM_CP_GSS_Compression}
                                checked={this.state.CompressionFlag}
                                onChange={this.CompressionCheck.bind(this)}
                            />
                        </div>
                        {this.state.CompressionFlag && <div id="compressionContainer">
                            <R.Slider
                                value={this.state.value}
                                step={this.state.step}
                                min={this.state.min}
                                max={this.state.max}
                                format="{0}"
                                markStep={1}
                                size={500}
                                priorities={[RMResx.RM_CP_GSS_Fastest, RMResx.RM_CP_GSS_Best]}
                                onChange={this.onSliderChange.bind(this)}
                            />
                        </div>}
                    </div>

                    <div className="ra-form-label">
                        <span>
                            {RMResx.RM_CP_GSS_DataHandle_Encryption}
                        </span>
                        <$g.Popover>{RMResx.RM_CP_GSS_Encryption_Tip}</$g.Popover>
                    </div>
                    <div className="ra-form-content">
                        <div className="margin-bottom-8">
                            <R.Checkbox
                                id="raCpSsEncryptionChk"
                                text={RMResx.RM_CP_GSS_Encryption}
                                title={RMResx.RM_CP_GSS_Encryption}
                                checked={this.state.encryptionFlag}
                                onChange={this.encryptionCheck.bind(this)}
                            />
                        </div>
                        {this.state.encryptionFlag && <div id="encryptionContainer">
                            <div className="ra-inline-middle">
                                <span id="ariaSecurityProfile" className="margin-right-8">
                                    {RMResx.RM_CP_GSS_SecurityProfile}
                                </span>
                                <R.Combobox
                                    id="raCpSsSecurityProfile"
                                    width="450"
                                    searchable={false}
                                    textField="Name"
                                    valueField="ID"
                                    checkedField="Checked"
                                    items={this.state.AllSecurityProfile}
                                    onChange={this.onCurrentSecurityProfileChange.bind(
                                        this
                                    )}
                                    searchPlaceholder={
                                        RMResx.RM_JS_CP_GSS_SecurityProfile_Default
                                    }
                                    aria="#ariaSecurityProfile"
                                />
                            </div>
                            <$g.ValidationMsg
                                show={this.state.securityProfileValidationShow}
                            >
                                {RMResx.RM_Common_FillOut}
                            </$g.ValidationMsg>
                        </div>}
                    </div>

                    <div className="ra-foot-btns">
                        <R.Button
                            text={RMResx.RM_JS_Common_Cancel}
                            onClick={this.onCancel}
                        />
                        <R.Button
                            id="raCpSsSaveBtn"
                            primary={true}
                            classify="theme"
                            text={RMResx.RM_JS_Common_Save}
                            onClick={this.onSaveSettings}
                        />
                    </div>
                </div>
            </div>
        );
    }
}
