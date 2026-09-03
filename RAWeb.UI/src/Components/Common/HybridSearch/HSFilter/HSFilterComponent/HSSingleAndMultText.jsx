import { SearchKeyOperationLogic } from '../../Constants';
export default class HSSingleAndMultText extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            conditions: [
                { text: RMResx.RM_HS_Contains, value: SearchKeyOperationLogic.Contains, checked: true },
                { text: RMResx.RM_HS_Equals, value: SearchKeyOperationLogic.Equals, checked: false },
            ],
            searchValue: ""
        };
        this.singleAndMultTextSearchData = {
            ColumnOperationLogic: SearchKeyOperationLogic.Contains
        };
    }

    componentReceive(data) {
        this.singleAndMultTextSearchData = data || { ColumnOperationLogic: SearchKeyOperationLogic.Contains };
        for (let item of this.state.conditions) {
            item.checked = data.ColumnOperationLogic == item.value;
        }
        this.setState({
            searchValue: data.Value,
            conditions: RM.deepcopy(this.state.conditions)
        });
    }

    onConditionChanged = (args) => {
        this.singleAndMultTextSearchData.ColumnOperationLogic = args.newValue.value;
        this.props.onChange(this.singleAndMultTextSearchData);
    }

    onSearchValueChanged = (value) => {
        this.singleAndMultTextSearchData.Value = value;
        this.props.onChange(this.singleAndMultTextSearchData);
    }

    render() {
        return <div className="flex">
            <div className="flex-1">
                <R.Combobox
                    items={this.state.conditions}
                    height={40}
                    width={"100%"}
                    textField="text"
                    valueField="value"
                    checkedField="checked"
                    searchable={false}
                    onChange={this.onConditionChanged}
                />
            </div>
            <div className="flex-1 margin-left-m">
                <R.Validation element="Input" require={RMResx.RM_HS_NoSearchColValValidMsg}>
                    <R.Input
                        type="text"
                        height={40}
                        width={"100%"}
                        value={this.state.searchValue}
                        placeholder={RMResx.RM_HS_TextTypeColumnPlaceholder}
                        onChange={this.onSearchValueChanged}
                    />
                </R.Validation>
            </div>
        </div>;
    }
}
