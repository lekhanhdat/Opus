export default class HSPhyTemplates extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            templateList: RM.deepcopy(this.props.templateList)
        };
    }

    componentReceive(data) {
        for(let item of this.state.templateList){
            item.Checked = data.Value.includes(item.Id);
        }
        this.setState({templateList: RM.deepcopy(this.state.templateList)});
    }

    onTemplatesChanged = (args)=>{
        let selectedTemplateIds = args.newValue.map((item)=>{
            return item.Id;
        });
        this.props.onChange(selectedTemplateIds);
    }

    render() {
        return <div className="flex">
            <div className="flex-1">
                <R.Input
                    type="text"
                    value={RMResx.RM_HS_Contains}
                    width={"100%"}
                    height={40}
                    readonly={true}
                />
            </div>
            <div className="flex-1 margin-left-m width-0">
                <R.Validation element="Multicombobox" require={RMResx.RM_HS_NoSearchColValValidMsg}>
                    <R.Multicombobox
                        items={this.state.templateList}
                        width={"100%"}
                        height={40}
                        textField="Name"
                        tooltipField="Name"
                        valueField="UniqueId"
                        checkedField="Checked"
                        groupField="group"
                        searchable={true}
                        linkMode={false}
                        clearable={true}
                        onChange={this.onTemplatesChanged}
                    />
                </R.Validation>
            </div>
        </div>;
    }
}
