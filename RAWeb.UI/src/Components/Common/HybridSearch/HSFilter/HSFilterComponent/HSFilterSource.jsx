export default class HSFilterSource extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            sourceItems: this.props.options
        };
    }

    componentReceive(data) {
        let sourceItems = RM.deepcopy(this.state.sourceItems);
        for (let item of sourceItems) {
            item.isChecked = data.Value.includes(item.Value);
        }
        this.setState({ sourceItems: sourceItems });
    }

    onSourceChange = (args) => {
        let selectSourceIds = args.newValue.map((item) => { return item.Value; });
        this.props.onChange(selectSourceIds);
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
                        id={"raHsFilterSource" + this.props.index}
                        items={this.state.sourceItems}
                        disabled={false}
                        searchable={false}
                        width={"100%"}
                        checkedField="isChecked"
                        textField="Name"
                        valueField="Value"
                        height={40}
                        clearable={true}
                        linkMode={false}
                        onChange={this.onSourceChange}
                    />
                </R.Validation>
            </div>
        </div>;
    }
}