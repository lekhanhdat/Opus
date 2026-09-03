import '../../../Less/PRM/TemplateRightPanel/TemplateEditPanel.less';
import StringUtil from '../../../Utilities/StringUtil';

class CategoryForm extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            showTip: false,
            showMessageTip: this.showMessageTip,
            categoryName:this.props.item.categoryName
        };
    }

    componentCreate() {

    }

    componentInit() {

    }

    componentReceive(action, args) {
        switch (action) {
            case "onSave":
                if($$.verify(this.allValidation))
                {
                    if(!args({ categoryName: $.trim(this.state.categoryName), id: this.props.item.id })){
                        this.showMessageTip("error", RMResx.RM_EditTemplate_SameCategoryNameErrorMessage);
                    }
                }
                break;
        }

    }

    handleCategoryNameChanged = (value) => {
        this.setState({ categoryName: value });
        this.props.notifySettingsChanged();
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

    render() {
        return <div id={this.props.id}>
            <R.Messagebar
                message={this.state.tipMsg} classify={this.state.tipType}
                onClose={this.hideMessageTip} status={{ show: this.state.showTip }} />
            <R.Validation>
            <div ref={r => this.allValidation = r}>
                <div className="ra-form-label">
                    <div className="ra-option-title  require">
                        <label>{StringUtil.trimEndColon(RMResx.RM_EditTemplate_CategoryName)}</label>
                    </div>
                </div>
                <R.Validation element="Input" require={RMResx.RM_Template_Column_ValueValidate}>
                    <R.Input
                        type='text'
                        value={this.state.categoryName}
                        onChange={this.handleCategoryNameChanged}
                        aria={{ ariaLabel: StringUtil.trimEndColon(RMResx.RM_EditTemplate_CategoryName) }}
                    />
                </R.Validation>
            </div>
            </R.Validation>
        </div>;
    }
}
export { CategoryForm };