import AutoCriteria from "./AutoCriteria";
import StringUtil from "../../../../Utilities/StringUtil";
export default class AutoFilterGroup extends R.Component{
    idAttr = true;
    constructor(props) {
        super(props);
        this.accordionContent = [];
        this.props.data.FilterGroups.forEach(filterGroup => {
            filterGroup.UniqueId = StringUtil.newGuid();
        });
        this.state = {
            filterGroup: this.props.data
        };
    }
    
    componentReceive(action, data) {
        this.setData(data);
    }
    
    newGuid(){
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }
    
    getFilterGroupData(indexData) {
        let filterGroup = {};
        filterGroup.CombineMode = this.refFilter.getCombineMode();
        filterGroup.Filters = this.refFilter.getFiltersData(indexData);
        filterGroup.FilterGroups = [];
        for (const key in this.accordionContent) {
            if (Object.hasOwnProperty.call(this.accordionContent, key)) {
                const groupContent = this.accordionContent[key];
                if (groupContent) {
                    filterGroup.FilterGroups.push(groupContent.getFilterGroupData(indexData));
                }
            }
        }
        return filterGroup;
    }

    filterGroupValidate() {
        let filterValidateResult = this.refFilter.archiveContentCustomValidate();
        if (!filterValidateResult) {
            return false;
        }
        for (const key in this.accordionContent) {
            if (Object.hasOwnProperty.call(this.accordionContent, key)) {
                const groupContent = this.accordionContent[key];
                if (groupContent != null) {
                    if (!groupContent.filterGroupValidate()) {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    updateGroupCount(groupCount) {
        this.refFilter.updateGroupCount(groupCount);
    }

    addGroup = () => {
        this.state.filterGroup.FilterGroups.push({ UniqueId: StringUtil.newGuid(), FilterGroups: [], Filters: [] });
        this.setState({
            filterGroup: RM.deepcopy(this.state.filterGroup)
        });
        // }, () => { this.props.focusFirstRule(); });
    }

    // delGroup = () => {
    //     this.props.delateFilterGroup();
    // }
    
    delateFilterGroup = (filterGroup) => {
        let deleteIndex = 0;
        this.state.filterGroup.FilterGroups.forEach((f, index) => {
            if (filterGroup.UniqueId == f.UniqueId) {
                deleteIndex = index;
            }
        });
        this.state.filterGroup.FilterGroups.splice(deleteIndex, 1);
        this.setState({
            filterGroup: Object.assign({}, this.state.filterGroup)
        });
        // }, () => { this.props.focusFirstRule(); });
    }

    render() {
        this.accordionContent = [];
        let isEven = this.props.deepCount % 2 == 0;
        return <div className={"auto-filterGroup-body " + (isEven ? "auto-rule-bg-color-even" : "auto-rule-bg-color-odd")} id={this.props.id}>
            <AutoCriteria
                ref={c => this.refFilter = c}
                itemId={this.props.itemId}
                levelId={64}
                data={this.state.filterGroup}
                groupCount={this.props.groupCount}
                addGroup={this.addGroup}
                delGroup={this.props.delGroup}
                deepCount={this.props.deepCount}
                focusFirstRule={this.props.focusFirstRule}
                lastAccessTimeCollection={this.props.lastAccessTimeCollection}
            ></AutoCriteria>
            <div>
                {this.state.filterGroup.FilterGroups && this.state.filterGroup.FilterGroups.map((filterGroup, index) => {
                    return <AutoFilterGroup
                        ref={accordionContent => this.accordionContent[filterGroup.UniqueId] = accordionContent}
                        key={filterGroup.UniqueId}
                        itemId={this.props.itemId}
                        data={filterGroup}
                        groupCount={this.state.filterGroup.FilterGroups.length}
                        delGroup={this.delateFilterGroup.bind(this, filterGroup)}
                        deepCount={this.props.deepCount + 1}
                        focusFirstRule={this.props.focusFirstRule}
                        lastAccessTimeCollection={this.props.lastAccessTimeCollection}
                    ></AutoFilterGroup>;
                })}
            </div>
        </div>;
    }
}