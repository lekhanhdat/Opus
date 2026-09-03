import { NodeLevel } from "../../../../Constants/DAEnums";
import RouterUrls from "../../../../Constants/RouterUrls";
import { checkPermission } from "../../../../Utilities/permissionManager";
import { EnableRecordManagementSetting } from "../CRMForSPO/ArchiveCRMForSPO";
import { CleanupAndDelRestoredType } from "../Common/CRMCommonUtil";

export default class GeneralManagementPanel extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            radioClassification: [
                { text: RMResx.RM_JS_Common_Yes, value: EnableRecordManagementSetting.Enable, checked: this.props.data.EnableArchiverManagement == EnableRecordManagementSetting.Enable ? true : false },
                { text: RMResx.RM_JS_Common_No, value: EnableRecordManagementSetting.Disable, checked: this.props.data.EnableArchiverManagement == EnableRecordManagementSetting.Enable ? false : true },
            ],
            delArchivedDataItems: [
                { text: RMResx.RM_AR_SPS_General_DelFileAndVersion, value: CleanupAndDelRestoredType.OnlyFileAndVersion, checked: this.props.data.CleanupAndDelRestoredType === CleanupAndDelRestoredType.OnlyFileAndVersion },
                { text: RMResx.RM_AR_SPS_General_DelRelatedFileOrVersion, value: CleanupAndDelRestoredType.RelatedFileOrVersion, checked: this.props.data.CleanupAndDelRestoredType === CleanupAndDelRestoredType.RelatedFileOrVersion },
            ],
            enableClassification: this.props.data.EnableArchiverManagement,
            enableClassificationChanged: false,
            enableDelArchivedData: this.props.data.EnableDelArchivedData,
            enableCleanStubs: this.props.data.EnableCleanStubs ?? false,
            cleanupAndDelRestoredType: this.props.data.CleanupAndDelRestoredType,
            dayNum:  this.props.data.DayNum ?? 30,
        };
    }

    onClassificationChanged = (args) => {
        this.setState({ enableClassification: args, enableClassificationChanged: true });
    }

    onSave(callback) {
        if (!$$.verify(this.allValidation)) {
            return false;
        }
        let generalSettingData = this.props.data;
        generalSettingData.EnableArchiverManagement = this.state.enableClassification;
        generalSettingData.EnableDelArchivedData = this.state.enableDelArchivedData;
        generalSettingData.CleanupAndDelRestoredType = this.state.cleanupAndDelRestoredType;
        generalSettingData.EnableCleanStubs = this.state.enableCleanStubs;
        generalSettingData.DayNum = this.state.dayNum;
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

    onDelArchivedDataChanged = (checked) => {
        if (checked) {
            this.state.delArchivedDataItems.map(op => {
                op.checked = op.value === CleanupAndDelRestoredType.OnlyFileAndVersion;
                return op;
            })
            this.setState({
                delArchivedDataItems: RM.deepcopy(this.state.delArchivedDataItems),
                cleanupAndDelRestoredType: CleanupAndDelRestoredType.OnlyFileAndVersion,
                dayNum: 30,
            });
        } else {
            this.setState({
                cleanupAndDelRestoredType: 0,
                dayNum: 0,
            });
        }
        this.setState({ enableDelArchivedData: checked });
    }

    onDelArchivedDataRadioChanged = (args) => {
        this.setState({ cleanupAndDelRestoredType: args });
    }

    onEnableCleanAllStubsChanged = (checked) => {
        this.setState({ enableCleanStubs: checked });
    }

    onDayNumChanged = (value) => {
        this.setState({ dayNum: value });
    }

    renderDelArchivedData() {
        return <div>
            <div className="ra-crm-form-content">
                <div className="ra-setting-panel-title" tabIndex="0">{RMResx.RM_AR_SPS_General_EnableDelDataTitle}</div>
                <div>
                    <div className="margin-bottom-s">
                        <R.Checkbox
                            id="raDelArchivedDataChk"
                            text={RMResx.RM_AR_SPS_General_EnableDelDataCheckbox}
                            checked={this.state.enableDelArchivedData}
                            onChange={this.onDelArchivedDataChanged}
                        />
                    </div>
                    {this.state.enableDelArchivedData && this.renderDelArchivedDataRadio()}
                    {
                        this.state.enableDelArchivedData && <div className="margin-top-s">
                        <R.Checkbox
                            id="raCleanupAllStubs"
                            text={RMResx.RM_AR_SPS_General_EnableCleanupAllStubsCheckbox}
                            checked={this.state.enableCleanStubs}
                            onChange={this.onEnableCleanAllStubsChanged}
                        />
                        </div>
                    }
                    
                </div>
            </div>
            {this.state.enableDelArchivedData && this.renderDelAllData()}
        </div>;
    }

    renderDelArchivedDataRadio() {
        return <div className="margin-left-l">
            <R.Radio.Group
                block
                name="delDataRadio"
                items={this.state.delArchivedDataItems}
                onChange={this.onDelArchivedDataRadioChanged}
            />
        </div>;
    }

    renderDelAllData() {
        return <div className="ra-crm-form-content">
            <div className="ra-setting-panel-title require">{RMResx.RM_AR_SPS_General_CleanupRestoreDataDays}</div>
            <div>
                <R.Validation
                    element="Input"
                    require={RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]}
                >
                    <R.Input
                        id="raAfterDaysNumIpt"
                        type="number"
                        width="120px"
                        min={0}
                        max={99999}
                        value={this.state.dayNum}
                        onChange={this.onDayNumChanged}
                        aria={{ ariaLabel: RMResx.RM_AR_SPS_General_CleanupRestoreDataDays }}
                    />
                </R.Validation>
            </div>
        </div>;
    }

    render() {
        let generalSetting = this.props.data;
        let supportDelArchivedData = RM.gData.enableDeleteRestoredDataFeature && (generalSetting.Level === NodeLevel.WebApplication || generalSetting.Level === NodeLevel.SiteCollection || generalSetting.Level === NodeLevel.Office365GroupEntire);
        return <div id={this.props.id}>
            <R.Validation>
                <div ref={r => this.allValidation = r}>
                    <div className="ra-crm-form-content">
                        <div id="ariaEnableClassification" className="ra-setting-panel-title">{RMResx.RM_AR_SPS_General_EnableArchiveTitle}</div>
                        <R.Radio.Group
                            aria="#ariaEnableClassification"
                            name="enableClassification"
                            items={this.state.radioClassification}
                            isSeparate={false}
                            onChange={this.onClassificationChanged}
                        />
                        {(this.state.enableClassification == EnableRecordManagementSetting.Enable) && this.state.enableClassificationChanged && <div className="ra-general-panel">
                            <div className="ra-general-panel-content" role="alert" aria-live="assertive" tabIndex={0}>
                                <span className="ra-general-panel-warn">{RMResx.RM_JS_SPS_Warning}</span>
                                <span className="ra-general-panel-font"> {RMResx.RM_AR_SPS_General_Warning_Enable}</span>
                            </div>
                        </div>}
                        {(this.state.enableClassification == EnableRecordManagementSetting.Disable) && this.state.enableClassificationChanged && <div className="ra-general-panel">
                            <div className="ra-general-panel-content" role="alert" aria-live="assertive" tabIndex={0}>
                                <span className="ra-general-panel-warn">{RMResx.RM_JS_SPS_Warning}</span>
                                <span className="ra-general-panel-font"> {RMResx.RM_AR_SPS_General_Warning_Disable}</span>
                            </div>
                        </div>}
                    </div>
                    {supportDelArchivedData && this.renderDelArchivedData()}
                </div>
            </R.Validation>
        </div>;
    }
}