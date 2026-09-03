import _ from "lodash";
import Category from "../../../Components/Category";
import { useEffect, useState } from "react";
import { BasicDataRequester, RotDataRequester } from "../../../requests";

const CategoryMap = new Map([
    [ 2, RMResx.RM_FA_ROTRule_TreeNode_Redundant],
    [ 3, RMResx.RM_FA_ROTRule_TreeNode_Obsolete],
    [ 4, RMResx.RM_FA_ROTRule_TreeNode_Trivial]
]);

const ROTCategories = ({ queryParameter, onChange, o365TenantId, isOptimizePanel }) => {

    const [categoryItems, setCategoryItems] = useState([]);

    const [subCategoryItems, setSubCategoryItems] = useState([]);

    const [fileExtensions, setFileExtensions] = useState([]);

    const [ruleInfos, setRuleInfos] = useState({});

    useEffect(() => {
        const fetchData = async () => {
            const fileExtensions = await BasicDataRequester.getFileExtensions(o365TenantId);
            fileExtensions.map(item => {
                item.checked = true;
            });
            setFileExtensions(fileExtensions.sort(sortFun));
            const ruleInfo = await BasicDataRequester.queryRuleInfos(o365TenantId);
            setRuleInfos(ruleInfo);
            let selectedROTCategoryIds = queryParameter.rotRuleQueryParameter.ruleCategories.filter(c => c.checked).map(a => a.ruleCategory);
            let selectedRuleIds = [];
            queryParameter.rotRuleQueryParameter.ruleCategories.filter(c => c.checked).map((category) => {
                selectedRuleIds = [...selectedRuleIds, ...category.ruleIds];
                return selectedRuleIds;
            });
            if(!_.isNil(ruleInfo.children) && ruleInfo.children.length > 0){
                let categories = [];
                ruleInfo.children.forEach((category) => {
                    categories.push({
                        id: category.category,
                        name: CategoryMap.get(category.category),
                        checked: selectedROTCategoryIds.includes(category.category),
                    });
                });
                setCategoryItems(categories);
            }
            getFilterItems(ruleInfo, selectedROTCategoryIds, selectedRuleIds);
        };

        fetchData();
    }, [o365TenantId]);

    const sortFun = (a, b) => {
        const nameA = a.name.toUpperCase();
        const nameB = b.name.toUpperCase();
        if (nameA < nameB) {
            return -1;
        }
        if (nameA > nameB) {
            return 1;
        }
        return 0;
    };

    const getFilterItems = (ruleInfo, selectCategory, selectedRuleIds) => {
        let subCategorise = [];
        let categoryInfos = ruleInfo.children;
        if(!_.isNil(categoryInfos) && categoryInfos.length > 0){
            selectCategory.map((category) => {
                categoryInfos.forEach(element => {
                    if(element.category === category && element.children.length > 0){
                        
                        element.children.map((children) => {
                            subCategorise.push({
                                categoryId : category,
                                id : children.id,
                                name : children.label,
                                checked : selectedRuleIds.length > 0 ? selectedRuleIds.includes(children.id) : true
                            });
                        });
                    }       
                });
            });
        }
        setSubCategoryItems(subCategorise);
    };


    const onSelectFileExtension = (ids) => {
        const clonedValue = _.cloneDeep(queryParameter);
        clonedValue.fileExtensionQueryParameter = {
            fileExtensions : ids,
        };
        onChange(clonedValue);
    };

    const onSelectedCategoryInfo = (ids) => {
        getFilterItems(ruleInfos, ids, []);
        const clonedValue = _.cloneDeep(queryParameter);
        clonedValue.rotRuleQueryParameter.ruleCategories = ids.map((id) => {
            return {
                ruleCategory : id,
                ruleIds : [],
                checked : true
            };  
        });
        onChange(clonedValue);
    };

    const onSelectedRuleInfo = (ruleInfo) => {
        const clonedValue = _.cloneDeep(queryParameter);
        const clonedCategories = clonedValue.rotRuleQueryParameter; 
        clonedCategories.ruleCategories = clonedCategories.ruleCategories.map((category) => {
            let selectedRuleIds = [];
            ruleInfo.map((rule) => {
                if(category.ruleCategory === rule.categoryId ){
                    selectedRuleIds.push(rule.id);
                }
            });
            return { ruleCategory: category.ruleCategory, ruleIds: selectedRuleIds, checked: category.checked };
        });
        onChange(clonedValue);
    };

    return (
        <>
            <div className={isOptimizePanel ? "reco-rot-category margin-bottom-m" : "reco-rot-category"}>
                <Category
                    title={RMResx.RM_FA_ROT_OptimizationTab_ROTCategoryTitle}
                    categoryItems={categoryItems}
                    queryParameter={queryParameter}
                    onChange={onSelectedCategoryInfo}
                />
            </div>
            <div className={isOptimizePanel ? "reco-rot-category margin-bottom-m" : "reco-rot-category"}>
                <Category
                    title={RMResx.RM_FA_ROT_OptimizationTab_ROTSubCategoryTitle}
                    categoryItems={subCategoryItems}
                    queryParameter={queryParameter}
                    onChange={onSelectedRuleInfo}
                    ruleChanged = {true}
                />
            </div>
            {!isOptimizePanel && <div className="reco-rot-category-last">
                <Category
                    title={RMResx.RM_FA_ROT_OptimizationTab_FileCategoryTitle}
                    categoryItems={fileExtensions}
                    queryParameter={queryParameter}
                    onChange={onSelectFileExtension}
                />
            </div>}
        </>
    );
};

export default ROTCategories;