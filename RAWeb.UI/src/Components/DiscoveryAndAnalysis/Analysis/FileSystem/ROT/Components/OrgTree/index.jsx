import { useEffect, useState } from "react";
import _ from "lodash";

import OrgTree from "../../../../../../Common/AveOrgTree";
import { CalculateUtil } from "../../../../Utils";

import "./index.less";

const DefaultRuleInfo = {
    ruleCategories: [
        {
            ruleCategory: 2,
            ruleIds: [],
        },
        {
            ruleCategory: 3,
            ruleIds: [],
        },
        {
            ruleCategory: 4,
            ruleIds: [],
        },
    ],
};

const TreeChart = ({ queryParameter, onQuery, onChange }) => {
    const [dataInfo, setDataInfo] = useState({});

    useEffect(() => {
        const handler = async () => {
            const filteredRuleCategories =
                queryParameter.rotRuleQueryParameter.ruleCategories;

            if (
                !_.isNil(filteredRuleCategories) &&
                filteredRuleCategories.length === 1
            ) {
                return;
            }

            const res = await onQuery(queryParameter);
            setDataInfo(await CalculateUtil.CalculateRotTreeRuleData(res));
        };
        handler();
    }, [queryParameter]);

    const queryRule = (e, data) => {
        if (e.target.style.cssText === "background: rgba(0, 114, 208, 0.12);") {
            e.target.style = "";
            const clonedQueryParameter = _.cloneDeep(queryParameter);
            clonedQueryParameter.rotRuleQueryParameter = DefaultRuleInfo;
            onChange(clonedQueryParameter);
        } else {
            const allElement = $(".org-tree-node-label-inner");
            for (var i = 0; i < allElement.length; i++) {
                allElement[i].style = "";
            }
            const clonedQueryParameter = _.cloneDeep(queryParameter);
            clonedQueryParameter.rotRuleQueryParameter.ruleCategories =
                DefaultRuleInfo;
            if (data.id != 0 && data.category != 0) {
                e.target.style = "background: rgba(0, 114, 208, 0.12);";
                clonedQueryParameter.rotRuleQueryParameter.ruleCategories = [
                    {
                        ruleCategory: data.category,
                        ruleIds: [data.id],
                    },
                ];
                onChange(clonedQueryParameter);
            }
        }
    };

    return (
        <div className="reco-rot-tree">
            <OrgTree
                data={dataInfo}
                horizontal={true}
                collapsable={true}
                expandAll={true}
                onClick={(e, data) => {
                    queryRule(e, data);
                }}
            />
        </div>
    );
};

export default TreeChart;
