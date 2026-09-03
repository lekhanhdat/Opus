import { ColumnNumberCondition } from './../../Constants';
export default class HSFilterNumber extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            compareList: [
                { text: RMResx.RM_HS_NumTypeColumnEqual, value: ColumnNumberCondition.Equal, checked: true },
                { text: RMResx.RM_HS_NumTypeColumnMoreThanOrEqual, value: ColumnNumberCondition.GreaterThenOrEqual, checked: false },
                { text: RMResx.RM_HS_NumTypeColumnLessThanOrEqual, value: ColumnNumberCondition.LessThenOrEqual, checked: false }
            ],
            selectedNum: "",
        };
        this.selectedNumRangeInfo = {
            Condition: ColumnNumberCondition.Equal,
            Value: null
        };
    }

    componentReceive(data) {
        this.selectedNumRangeInfo = data.Value || {};
        this.echoNumRange();
    }

    onNumSelectionChanged = (value) => {
        this.selectedNumRangeInfo.Value = value;
        this.setState({ selectedNum: value });
        if (this.selectedNumRangeInfo.Value || this.selectedNumRangeInfo.Value === 0 ) {
            this.props.onChange(this.selectedNumRangeInfo);
        } else {
            this.props.onChange(null);
        }
    }

    onCompareSelectionChanged = (args) => {
        this.selectedNumRangeInfo.Condition = args.newValue.value;
        if (this.selectedNumRangeInfo.Value || this.selectedNumRangeInfo.Value === 0) {
            this.props.onChange(this.selectedNumRangeInfo);
        } else {
            this.props.onChange(null);
        }
    }

    echoNumRange = () => {
        this.state.compareList.forEach((item) => {
            if (item.value == this.selectedNumRangeInfo.Condition) {
                item.checked = true;
            } else {
                item.checked = false;
            }
        });
        this.setState({
            selectedNum: this.selectedNumRangeInfo.Value,
            compareList: RM.deepcopy(this.state.compareList)
        });
    }

    render() {
        return <div className="flex">
            <div className="flex-1">
                <R.Combobox
                    items={this.state.compareList}
                    width={"100%"}
                    height={40}
                    textField="text"
                    valueField="value"
                    checkedField="checked"
                    linkMode={false}
                    searchable={false}
                    onChange={this.onCompareSelectionChanged}
                />
            </div>
            <div className="flex-1 margin-left-m">
                <R.Validation element="Input" require={RMResx.RM_HS_NoSearchColValValidMsg}>
                    <R.Input
                        type="number"
                        width={"100%"}
                        height={40}
                        value={this.state.selectedNum}
                        float={2}
                        fixFloat={false}
                        hasControl
                        placeholder={RMResx.RM_HS_Filter_NumberWatermark}
                        onChange={this.onNumSelectionChanged}
                    />
                </R.Validation>
            </div>
        </div>;
    }
}
