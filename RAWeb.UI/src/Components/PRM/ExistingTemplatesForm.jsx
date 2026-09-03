import { bindEvents } from "../../Utilities/CommonUtil";
export default class ExistingTemplatesForm extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            selectedTemplateIds: [],
            showTip: false,
            tipType: "success",
            tipMsg: "",
        };
        bindEvents(this, "onTemplatesChanged");
    }
    componentReceive(action, ...args) {
        switch (action) {
            case "onSave":
                if($$.verify(this.allValidation))
                {
                    args[0](this.getSaveDto());
                }
                break;
            case "showErrorMessage":
                this.showMessageTip("error", args[0]);
                break;
        }
    }

    getSaveDto() {
        return {
            Ids: this.state.selectedTemplateIds,
            UniqueId: this.props.parentUniqueId,
            TemplateIdList: this.props.templateIdList
        };
    }
    
    onTemplatesChanged(args) {
        if(args && args.newValue) {
            let ids = args.newValue.map((item)=> { return item.value;});
            let selectedIds = ids.length > 0? ids : [];
            this.setState({ selectedTemplateIds: selectedIds})
        }
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
        return  <div id={this.props.id}> 
            <R.Validation>
                <div ref={r => this.allValidation = r}>
                    <R.Messagebar
                        message={this.state.tipMsg} classify={this.state.tipType}
                        onClose={this.hideMessageTip} status={{ show: this.state.showTip }} />
                    <div className="ra-form-label">
                        <div className='input-label require'>
                            <span>{RMResx.RM_PRM_TM_TemplateName_Title}</span>
                        </div>
                    </div>
                    <R.Validation element="Multicombobox" require={RMResx.RM_PRM_TM_NoSelectTemplatesTitle}>
                    <R.Multicombobox
                        items={this.props.items}
                        width={"100%"}
                        disabled={false}
                        textField="name"
                        valueField="value"
                        checkedField="checked"
                        tooltipField="tooltip"
                        disabledField="disabled"
                        groupField="group"
                        searchable={true}
                        clearable={false}
                        linkMode={false}
                        allNames={true}
                        onChange={this.onTemplatesChanged}
                        aria={{ ariaLabel: RMResx.RM_PRM_TM_TemplateName_Title }}
                    />
                </R.Validation>
                </div>
            </R.Validation>
        </div>;
    }
}