import { bindEvents } from "../../Utilities/CommonUtil";
import * as Constants from "./Constants";
import { RegexUtil } from "../../Utilities/RegexUtil";
export default class GlobalUniqueIdSettingForm extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            showTip: false,
            tipType: "success",
            tipMsg: "",
            boxTemplatePrefix: '',
            boxTemplateNumberOfDigits: 0,
            folderTemplatePrefix: '',
            folderTemplateNumberOfDigits: 0,
            recordTemplatePrefix: '',
            recordTemplateNumberOfDigits: 0,
            customTemplatePrefix: '',
            customTemplateNumberOfDigits: 0,
            invalidPrefix: {
                Box: false,
                Folder: false,
                Record: false,
                Custom: false
            },
            invalidNumberOfDigits: {
                Box: false,
                Folder: false,
                Record: false,
                Custom: false
            },
            invalidMessagePrefix: {
                Box: '',
                Folder: '',
                Record: '',
                Custom: ''
            },
            invalidMessageNumberOfDigits: {
                Box: '',
                Folder: '',
                Record: '',
                Custom: ''
            },
        };
        bindEvents(this, "onPrefixChange", "onNumberDigitsChange");
    }

    componentInit () {
        this.loadUniqueIdSettings();
    }

    componentReceive(action, ...args) {
        switch (action) {
            case "onSave":
                if (!this.validateForm()) {
                    return false;
                }
                args[0](this.getSaveDto());
                break;
        }
    }

    loadUniqueIdSettings() {
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
                    recordTemplateNumberOfDigits: uniqueIdSetting.RecordTemplateNumberOfDigits,
                    customTemplatePrefix: uniqueIdSetting.CustomTemplatePrefix,
                    customTemplateNumberOfDigits: uniqueIdSetting.CustomTemplateNumberOfDigits
                });
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

        let customValidPrefix = this.validatePrefixValue(this.state.customTemplatePrefix);
        let customValidNumberOfDigits = this.validateNumberOfDigits(this.state.customTemplateNumberOfDigits);
        this.setState({
            invalidPrefix: {
                Box: !boxValidPrefix.result,
                Folder: !folderValidPrefix.result,
                Record: !recordValidPrefix.result,
                Custom: !customValidPrefix.result
            },
            invalidNumberOfDigits: {
                Box: !boxValidNumberOfDigits.result,
                Folder: !folderValidNumberOfDigits.result,
                Record: !redordValidNumberOfDigits.result,
                Custom: !customValidNumberOfDigits.result,
            },
            invalidMessagePrefix: {
                Box: boxValidPrefix.errorMessage,
                Folder: folderValidPrefix.errorMessage,
                Record: recordValidPrefix.errorMessage,
                Custom: customValidPrefix.errorMessage
            },
            invalidMessageNumberOfDigits: {
                Box: boxValidNumberOfDigits.errorMessage,
                Folder: folderValidNumberOfDigits.errorMessage,
                Record: redordValidNumberOfDigits.errorMessage,
                Custom: customValidNumberOfDigits.errorMessage
            },
        });

        return boxValidPrefix.result && folderValidPrefix.result && recordValidPrefix.result && customValidPrefix.result
            && boxValidNumberOfDigits.result && folderValidNumberOfDigits.result && redordValidNumberOfDigits.result && customValidNumberOfDigits.result;

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
        } else if(!RegexUtil.IsMath(val))
        {
            isValid = false;
            errorMessage = RMResx.RM_PRM_UniqueId_Invalid_Message;
        }
        return {
            result: isValid,
            errorMessage: errorMessage
        };
    }

    validateIsNotEmpty(val) {
        return $.trim(val) != '';
    }

    getSaveDto() {
        return {
            BoxTemplatePrefix: this.state.boxTemplatePrefix,
            BoxTemplateNumberOfDigits: this.state.boxTemplateNumberOfDigits,
            FolderTemplatePrefix: this.state.folderTemplatePrefix,
            FolderTemplateNumberOfDigits: this.state.folderTemplateNumberOfDigits,
            RecordTemplatePrefix: this.state.recordTemplatePrefix,
            RecordTemplateNumberOfDigits: this.state.recordTemplateNumberOfDigits,
            CustomTemplatePrefix: this.state.customTemplatePrefix,
            CustomTemplateNumberOfDigits: this.state.customTemplateNumberOfDigits,
        };
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
        this.setState({
            showTip: false
        });
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
            case Constants.TemplateTypes.CustomTemplate:
                this.setState({
                    customTemplatePrefix: $.trim(value),
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
            case Constants.TemplateTypes.CustomTemplate:
                this.setState({
                    customTemplateNumberOfDigits: $.trim(value),
                });
                break;
        }
    }

    render() {
        return <div id={this.props.id}>
            {/* <R.Messagebar
                message={this.state.tipMsg} classify={this.state.tipType}
                onClose={this.hideMessageTip} status={{ show: this.state.showTip }} /> */}
            <div id={this.props.id}>
                    <div className='unique-id-settings-title' tabIndex="0">{RMResx.RM_EditTemplate_GlobalBoxUniqueIdSettingsTitle}</div>
                    <div className='unique-id-settings-block'>
                        <div className="ra-form-label" >
                            <div className='input-label  require'><span id="ariaPrefix1">{RMResx.RM_EditTemplate_Prefix}</span></div>
                        </div>
                    </div>
                    <R.Input
                        id="raPrmTplBinputPrefix"
                        name='binputPrefix'
                        type='text'
                        value={this.state.boxTemplatePrefix || ''}
                        onChange={this.onPrefixChange.bind(this, Constants.TemplateTypes.Box)}
                        aria={{ 'aria-labelledby': 'ariaPrefix1', 'aria-required': true }}/>
                    {this.state.invalidPrefix.Box && <div className='ra-validation-msg'>
                        {this.state.invalidMessagePrefix.Box}
                    </div>}
                    <div className="ra-form-label">
                        <div className='input-label  require'><span id="ariaNumberDigits1">{RMResx.RM_EditTemplate_NumberofDigits}</span></div>
                    </div>
                    <R.Input
                        id="raPrmTplBinputNumberOfDigits"
                        name='binputNumberOfDigits'
                        type="text"
                        value={this.state.boxTemplateNumberOfDigits || ''}
                        onChange={this.onNumberDigitsChange.bind(this, Constants.TemplateTypes.Box)}
                        aria={{ 'aria-labelledby': 'ariaNumberDigits1', 'aria-required': true }}/>
                    {this.state.invalidNumberOfDigits.Box && <div className='ra-validation-msg'>
                        {this.state.invalidMessageNumberOfDigits.Box}
                    </div>}
                </div>

                <div>
                    <div className='unique-id-settings-title' tabIndex="0">{RMResx.RM_EditTemplate_GlobalFileUniqueIdSettingsTitle}</div>
                    <div className='unique-id-settings-block'>
                        <div className="ra-form-label" >
                            <div className='input-label require'><span id="ariaPrefix2">{RMResx.RM_EditTemplate_Prefix}</span></div>
                        </div>
                    </div>
                    <R.Input
                        id="raPrmTplFinputPrefix"
                        name='finputPrefix'
                        type='text'
                        value={this.state.folderTemplatePrefix || ''}
                        onChange={this.onPrefixChange.bind(this, Constants.TemplateTypes.Folder)}
                        aria={{ 'aria-labelledby': 'ariaPrefix2', 'aria-required': true }}/>
                    {this.state.invalidPrefix.Folder && <div className='ra-validation-msg'>
                        {this.state.invalidMessagePrefix.Folder}
                    </div>}
                    <div className="ra-form-label">
                        <div className='input-label require'><span id="ariaNumberDigits2">{RMResx.RM_EditTemplate_NumberofDigits}</span></div>
                    </div>
                    <R.Input
                        id="raPrmTplFinputNumberOfDigits"
                        name='finputNumberOfDigits'
                        type="text"
                        value={this.state.folderTemplateNumberOfDigits || ''}
                        onChange={this.onNumberDigitsChange.bind(this, Constants.TemplateTypes.Folder)} 
                        aria={{ 'aria-labelledby': 'ariaNumberDigits2', 'aria-required': true }}/>
                    {this.state.invalidNumberOfDigits.Folder && <div className='ra-validation-msg'>
                        {this.state.invalidMessageNumberOfDigits.Folder}
                    </div>}
                </div>

                <div>
                    <div className='unique-id-settings-title' tabIndex='0'>{RMResx.RM_EditTemplate_GlobalRecordUniqueIdSettingsTitle}</div>
                    <div className='unique-id-settings-block'>
                        <div className="ra-form-label" >
                            <div className='input-label  require' ><span id="ariaPrefix3">{RMResx.RM_EditTemplate_Prefix}</span></div>
                        </div>
                    </div>
                    <R.Input
                        id="raPrmTplRinputPrefix"
                        name='rinputPrefix'
                        type='text'
                        value={this.state.recordTemplatePrefix || ''}
                        onChange={this.onPrefixChange.bind(this, Constants.TemplateTypes.Records)} 
                        aria={{ 'aria-labelledby': 'ariaPrefix3', 'aria-required': true }}/>
                    {this.state.invalidPrefix.Record && <div className='ra-validation-msg'>
                        {this.state.invalidMessagePrefix.Record}
                    </div>}
                    <div className="ra-form-label">
                        <div className='input-label  require'><span id="ariaNumberDigits3">{RMResx.RM_EditTemplate_NumberofDigits}</span></div>
                    </div>
                    <R.Input
                        id="raPrmTplRinputNumberOfDigits"
                        name='rinputNumberOfDigits'
                        type="text"
                        value={this.state.recordTemplateNumberOfDigits || ''}
                        onChange={this.onNumberDigitsChange.bind(this, Constants.TemplateTypes.Records)} 
                        aria={{ 'aria-labelledby': 'ariaNumberDigits3', 'aria-required': true }}/>
                    {this.state.invalidNumberOfDigits.Record && <div className='ra-validation-msg'>
                        {this.state.invalidMessageNumberOfDigits.Record}
                    </div>}
                </div>

                <div>
                    <div className='unique-id-settings-title' tabIndex='0'>{RMResx.RM_EditTemplate_GlobalCustomUniqueIdSettingsTitle}</div>
                    <div className='unique-id-settings-block'>
                        <div className="ra-form-label" >
                            <div className='input-label  require'><span id="ariaPrefix4">{RMResx.RM_EditTemplate_Prefix}</span></div>
                        </div>
                    </div>
                    <R.Input
                        id="raPrmTplCinputPrefix"
                        name='cinputPrefix'
                        type='text'
                        value={this.state.customTemplatePrefix || ''}
                        onChange={this.onPrefixChange.bind(this, Constants.TemplateTypes.CustomTemplate)} 
                        aria={{ 'aria-labelledby': 'ariaPrefix4', 'aria-required': true }}/>
                    {this.state.invalidPrefix.Custom && <div className='ra-validation-msg'>
                        {this.state.invalidMessagePrefix.Custom}
                    </div>}
                    <div className="ra-form-label">
                        <div className='input-label  require'><span id="ariaNumberDigits4">{RMResx.RM_EditTemplate_NumberofDigits}</span></div>
                    </div>
                    <R.Input
                        id="raPrmTplCinputNumberOfDigits"
                        name='cinputNumberOfDigits'
                        type="text"
                        value={this.state.customTemplateNumberOfDigits || ''}
                        onChange={this.onNumberDigitsChange.bind(this, Constants.TemplateTypes.CustomTemplate)} 
                        aria={{ 'aria-labelledby': 'ariaNumberDigits4', 'aria-required': true }}/>
                    {this.state.invalidNumberOfDigits.Custom && <div className='ra-validation-msg'>
                        {this.state.invalidMessageNumberOfDigits.Custom}
                    </div>}
                </div>
        </div>;
    }
}