import "./index.less";

const Category = ({ title, categoryItems, onChange, ruleChanged }) => {

    const onSelectContainer = (args) => {
        let ids = args.newValue.map((item) => { return item.id; });
        let selectedIds = ids.length > 0 ? ids : [];
        onChange(selectedIds);
    };

    const onSelectRule = (args) => {
        let ruleInfo = args.newValue.map((item) => { return { id: item.id, categoryId: item.categoryId, checked: item.checked }; });
        let selectedRuleInfo = ruleInfo.length > 0 ? ruleInfo : [];
        onChange(selectedRuleInfo);
    };

    return (
        <div className="reco-category">
            <div className="reco-category-title">{title}</div>
            <div>
                <R.Validation
                    element="Multicombobox"
                    require={RMResx.RM_FA_DataOptimize_Validation_ErrorMsg}>
                    <R.Multicombobox
                        id="raCategory"
                        width="100%"
                        popupMaxHeight={400}
                        searchable={false}
                        items={categoryItems}
                        textField="name"
                        valueField="id"
                        checkedField="checked"
                        onChange={ruleChanged ? onSelectRule : onSelectContainer}
                        aria={{ ariaLabel: title }}
                    />
                </R.Validation>
            </div>
        </div>
    );
};

export default Category;