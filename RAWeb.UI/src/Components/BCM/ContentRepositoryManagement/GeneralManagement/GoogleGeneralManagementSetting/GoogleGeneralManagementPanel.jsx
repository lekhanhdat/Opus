import "../../../../../Less/BCM/ContentRepositoryManagement/generalManagementSetting.less";
import { checkPermission } from "../../../../../Utilities/permissionManager";

export const EnableRecordManagementSetting = {
    Enable: 1,
    Disable: 2,
    ParentDisable: 3
};

export default class GoogleGeneralManagementPanel extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            radioClassification: [
                {
                    text: RMResx.RM_JS_Common_Yes,
                    value: EnableRecordManagementSetting.Enable,
                    checked: this.props.data.EnableRecordManagement == EnableRecordManagementSetting.Enable ? true : false
                },
                {
                    text: RMResx.RM_JS_Common_No,
                    value: EnableRecordManagementSetting.Disable,
                    checked: this.props.data.EnableRecordManagement == EnableRecordManagementSetting.Enable ? false : true
                }
            ],
            radioDataSync: [
                { text: RMResx.RM_JS_Common_Yes, value: true, checked: this.props.data.IsSyncData },
                { text: RMResx.RM_JS_Common_No, value: false, checked: !this.props.data.IsSyncData }
            ],
            radioDisplayUniqueId: [
                { text: RMResx.RM_JS_Common_Yes, value: true, checked: this.props.data.IsShowUniqueId },
                { text: RMResx.RM_JS_Common_No, value: false, checked: !this.props.data.IsShowUniqueId },
            ],
            enableClassification: this.props.data.EnableRecordManagement,
            enableDataSync: this.props.data.IsSyncData,
            enableClassificationChanged: false,
            enableDataSyncChanged: false,
            displayUniqueId: this.props.data.IsShowUniqueId,
            displayUniqueIdChanged: false,
            uniqueIdData: {}
        };
    }

    componentInit() {
        this.loadUniqueIdSetting();
    }

    componentReceive(type, args) {
        switch (type) {
            case "reloadUniqueId":
                this.loadUniqueIdSetting(args);
                break;
        }
    }

    onKeyDown(e) {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    loadUniqueIdSetting() {
        let option = {
            url: this.props.context.loadingUniqueIdSettingUrl,
            method: "GET",
        };
        fetchUtility(option).then((res) => {
            this.setState({
                uniqueIdData: res
            });
        }).catch((e) => { console.log(e) });
    }

    onSave(callback) {
        let generalSettingData = this.props.data;
        //TODO Doris
        //generalSettingData set value
        generalSettingData.EnableRecordManagement = this.state.enableClassification;
        generalSettingData.IsSyncData = this.state.enableDataSync;
        generalSettingData.IsShowUniqueId = this.state.displayUniqueId;
        generalSettingData.ObjectId = this.props.data.ObjectId;

        let option = {
            url: this.props.context.saveDataUrl,
            method: "Post",
            data: generalSettingData
        };
        return fetchUtility(option).then(function (res) {
            return { data: JSON.parse(res) };
        }).then(result => {
            callback(result, this.state.enableClassification == EnableRecordManagementSetting.Disable && this.state.enableClassificationChanged);
        });
    }

    onClassificationChanged = (args) => {
        this.setState({ enableClassification: args, enableClassificationChanged: true });

    }

    onDataSyncChanged = (args) => {
        this.setState({ enableDataSync: args, enableDataSyncChanged: true });
    }

    onDisplayUniqueIdChanged = (args) => {
        this.setState({ displayUniqueId: args, displayUniqueIdChanged: true });
    }

    showUniqueIdPanel = () => {
        if (checkPermission(this.props.context.resource, RM.UserResources)) {
            this.dispatch("uniqueId", 'showUniqueIdPanel');
        } else {
            let args = {
                width: "550px",
                hideActions: false,
                title: RMResx.RM_JS_Common_Confirmation,
                content: RMResx.RM_JS_SPS_UniqueIdDisplay_DelegateWarning,
                buttons: [
                    {
                        text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: () => {
                            $$.messagedialog(false);
                        }
                    }
                ]
            };
            $$.messagedialog(true, args);
        }
    }

    renderDateSyncChange = () => {
        if (!this.state.enableDataSync) {
            return (
                <div className="ra-general-panel">
                    <div className="ra-general-panel-content" role="alert" aria-live="assertive" tabIndex={0}>
                        <span className="ra-general-panel-warn">{RMResx.RM_JS_SPS_Warning}</span>
                        <span className="ra-general-panel-font"> {RMResx.RM_JS_SPS_Warning_DisableDataSync}</span>
                    </div>
                </div>
            );
        }

        if (this.props.context.showUniqueIdWarn && !this.state.uniqueIdData.IsActived && this.state.uniqueIdData.Id == 0) {
            return (
                <div className="ra-general-panel">
                    <div className="ra-general-panel-content" role="alert" aria-live="assertive" tabIndex={0}>
                        <span className="ra-general-panel-font">
                            <$g.I18NProvider msg={RMResx.RM_JS_SPS_UniqueIdDisplay_Warning}>
                                <span className="ra-general-panel-uniqueid" onClick={this.showUniqueIdPanel} tabIndex="0" onKeyDown={this.onKeyDown}>
                                    {RMResx.RM_JS_SP_UniqueIdSetting_Btn}
                                </span>
                            </$g.I18NProvider>
                        </span>
                    </div>
                </div>
            );
        }

        return (
            <div className="ra-general-panel">
                <div className="ra-general-panel-content" role="alert" aria-live="assertive" tabIndex={0}>
                    <span className="ra-general-panel-warn">{RMResx.RM_JS_SPS_Warning}</span>
                    <span className="ra-general-panel-font"> {RMResx.RM_JS_SPS_Warning_EnableDataSync}</span>
                </div>
            </div>
        )
    }

    renderUniqueIdChange = () => {
        if (!this.state.displayUniqueId) {
            return (
                <div className="ra-general-panel">
                    <div className="ra-general-panel-content" role="alert" aria-live="assertive" tabIndex={0}>
                        <span className="ra-general-panel-warn">{RMResx.RM_JS_SPS_Warning}</span>
                        <span className="ra-general-panel-font"> {RMResx.RM_JS_SPS_Warning_DisableDisplayUniqueId}</span>
                    </div>
                </div>
            )
        }

        if (this.props.context.showUniqueIdWarn && !this.state.uniqueIdData.IsActived && this.state.uniqueIdData.Id == 0) {
            return (
                <div className="ra-general-panel">
                    <div className="ra-general-panel-content" role="alert" aria-live="assertive" tabIndex={0}>
                        <span className="ra-general-panel-font">
                            <$g.I18NProvider msg={RMResx.RM_JS_SPS_UniqueIdDisplay_Warning}>
                                <span className="ra-general-panel-uniqueid" onClick={this.showUniqueIdPanel} tabIndex="0" onKeyDown={this.onKeyDown}>
                                    {RMResx.RM_JS_SP_UniqueIdSetting_Btn}
                                </span>
                            </$g.I18NProvider>
                        </span>
                    </div>
                </div>
            )
        }

        return (
            <div className="ra-general-panel" role="alert" aria-live="assertive" tabIndex={0}>
                <div className="ra-general-panel-content">
                    <span className="ra-general-panel-warn">{RMResx.RM_JS_SPS_Warning}</span>
                    <span className="ra-general-panel-font"> {RMResx.RM_JS_SPS_Warning_EnableDisplayUniqueId}</span>
                </div>
            </div>
        )
    }

    render() {
        const { data: generalSetting, context } = this.props;
        const { radioClassification, radioDataSync, radioDisplayUniqueId, enableClassification, enableClassificationChanged, enableDataSyncChanged, displayUniqueIdChanged } = this.state;

        return <div id={this.props.id}>
            <div className="ra-crm-form-content">
                <div id="ariaEnableClassification" className="ra-setting-panel-title" tabIndex={0}>
                    {RMResx.RM_JS_SPS_EnableClassicationTitle}
                </div>
                <R.Radio.Group
                    aria="#ariaEnableClassification"
                    name="enableClassification"
                    items={radioClassification}
                    isSeparate={false}
                    onChange={this.onClassificationChanged}
                />

                {(enableClassification == EnableRecordManagementSetting.Enable) && enableClassificationChanged &&
                    <div className="ra-general-panel">
                        <div className="ra-general-panel-content" role="alert" aria-live="assertive" tabIndex={0}>
                            <span className="ra-general-panel-warn">{RMResx.RM_JS_SPS_Warning}</span>
                            <span className="ra-general-panel-font"> {RMResx.RM_JS_SPS_Warning_EnableRecordsManagement}</span>
                        </div>
                    </div>
                }

                {(enableClassification == EnableRecordManagementSetting.Disable) && enableClassificationChanged &&
                    <div className="ra-general-panel">
                        <div className="ra-general-panel-content" role="alert" aria-live="assertive" tabIndex={0}>
                            <span className="ra-general-panel-warn">{RMResx.RM_JS_SPS_Warning}</span>
                            <span className="ra-general-panel-font"> {RMResx.RM_JS_SPS_Warning_DisableRecordsManagement}</span>
                        </div>
                    </div>
                }
            </div>

            {generalSetting && (enableClassification == EnableRecordManagementSetting.Enable) && context.supportSync(generalSetting) &&
                <div className="ra-crm-form-content">
                    <div id="ariaEnableDataSync" className="ra-setting-panel-title" tabIndex={0}>{RMResx.RM_SPS_IsSync}</div>
                    <R.Radio.Group
                        aria="#ariaEnableDataSync"
                        name="enableDataSync"
                        items={radioDataSync}
                        isSeparate={false}
                        onChange={this.onDataSyncChanged}
                    />
                    {enableDataSyncChanged && this.renderDateSyncChange()}
                </div>
            }

            {generalSetting && (enableClassification == EnableRecordManagementSetting.Enable) && context.supperDisplayUniqueId(generalSetting) &&
                <div className="ra-crm-form-content">
                    <div id="ariaUniqueId" className="ra-setting-panel-title">{RMResx.RM_JS_SPS_OneDriveUniqueId}</div>
                    <R.Radio.Group
                        aria="#ariaUniqueId"
                        name="displayUniqueId"
                        items={radioDisplayUniqueId}
                        isSeparate={false}
                        onChange={this.onDisplayUniqueIdChanged}
                    />
                    {displayUniqueIdChanged && this.renderUniqueIdChange()}
                </div>
            }
        </div>;
    }
}