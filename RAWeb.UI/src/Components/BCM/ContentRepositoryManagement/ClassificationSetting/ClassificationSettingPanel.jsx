import "../../../../Less/BCM/ContentRepositoryManagement/classificationSetting.less";
import { LicenseHelper, showToast } from "../../../../Utilities/CommonUtil";

export const ClassificationSettingType = {
    None: 0,
    FolderLevel: 2100,
    FileLevel: 2200
};

const isEnableJPMCFeature = LicenseHelper.EnableJPMCFileSystemFeature();

export default class ClassificationSettingPanel extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            classificationData: ClassificationSettingType.FileLevel,
            loadClassificationData: false,
        };
    }

    componentInit() {
        this.loadClassificationSetting();
    }

    componentReceive(type, args) {
        switch (type) {
            case "onSave":
                this.onSave(args);
                break;
        }
    }

    buildRadioOptions(classificationData) {
        const options = [
            {
                text: RMResx.RM_JS_FS_ClassificationSetting_FolderLevel,
                value: ClassificationSettingType.FolderLevel,
                checked: classificationData === ClassificationSettingType.FolderLevel,
                isVisible: !isEnableJPMCFeature,
            },
            {
                text: RMResx.RM_JS_FS_ClassificationSetting_FileLevel,
                value: ClassificationSettingType.FileLevel,
                checked: classificationData === ClassificationSettingType.FileLevel,
                isVisible: true,
            },
        ];
        return options.filter((option) => option.isVisible);
    }

    loadClassificationSetting() {
        let option = {
            url: "/API/FSSettingApi/GetClassificationLevel",
            method: "Post",
        };
        fetchUtility(option).then((res) => {
            let tempClassificationData = this.state.classificationData;
            if (res) {
                tempClassificationData = isEnableJPMCFeature ? ClassificationSettingType.FileLevel : res;
            }
            this.radioClassification = this.buildRadioOptions(tempClassificationData);
            this.setState({
                classificationData: tempClassificationData,
                loadClassificationData: true
            });
        }).catch((e) => {
        });
    }

    onSave(callback) {
        if (!$$.verify(this.allValidation)) {
            return false;
        }
        $$.loading(true);
        let getClassificationData = this.state.classificationData;
        let option = {
            url: "/API/FSSettingApi/SetClassificationLevel",
            method: "Post",
            data: getClassificationData
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            callback(true, getClassificationData);
            showToast.success(RMResx.RM_JS_FS_ClassificationSetting_Save_Success);
            this.props.initClassificationData();
        }).catch((e) => {
            $$.loading(false);
        });
    }

    handleClassificationChanged = (args) => {
        this.setState({ classificationData: args });
    }

    render() {
        return <div id={this.props.id}>
            <R.Validation>
                <div ref={r => this.allValidation = r}>
                    {this.state.loadClassificationData && <div className="ra-crm-form-content">
                        <div className="require ra-setting-panel-title">
                            <span id="ariaRadioClassification">{RMResx.RM_JS_FS_ClassificationSetting_Title}</span>
                        </div>
                        <R.Validation
                            element="Radio.Group"
                            require>
                            <R.Radio.Group
                                block={true}
                                name="radioClassification"
                                items={this.radioClassification}
                                onChange={this.handleClassificationChanged}
                                aria="#ariaRadioClassification"
                            />
                        </R.Validation>
                    </div>}
                </div>
            </R.Validation>
        </div>;
    }
}