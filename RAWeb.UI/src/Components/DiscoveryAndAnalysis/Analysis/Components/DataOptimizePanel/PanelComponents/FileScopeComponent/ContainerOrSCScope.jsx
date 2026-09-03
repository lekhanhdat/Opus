import { useState, useEffect } from "react";
import _ from "lodash";
import { useStableCallback } from "../../../../../../Common/Hooks";
import { InactiveDataRequester } from "../../../../requests";
import { DiscoveryNodeViewMode } from "../../../../Constants";

const ContainerOrSCScope = ({ dataOptimizeParameter, onChange }) => {

    const [pageCount, setPageCount] = useState(0);

    const [isInit, setIsInit] = useState(true);

    const [selectedScopes, setSelectedScopes] = useState([]);

    useEffect(() => {
        const handler = async () => {
            if (isInit) {
                dataOptimizeParameter.nodeQueryParameter.checkedItems.forEach((i) => {
                    i.nameOrUrl = i.name || i.url;
                });
                setSelectedScopes(dataOptimizeParameter.nodeQueryParameter.checkedItems);
            }
        };
        handler();
    }, [dataOptimizeParameter]);

    const doLoad = useStableCallback(async (args) => {
        let pageSize = 15;
        const pageIndex = (args.start / args.count) >>> 0;
        if (pageIndex > 0 && pageIndex >= pageCount) {
            return [];
        }

        const clonedParameter = _.cloneDeep(dataOptimizeParameter);
        clonedParameter.nodeQueryParameter.searchKey = args.key;
        clonedParameter.nodeQueryParameter.pageSize = pageSize;
        clonedParameter.nodeQueryParameter.pageIndex = pageIndex;
        const res = await InactiveDataRequester.querySummaryNodesData(clonedParameter);
        if (pageIndex === 0) {
            setPageCount((res.count + args.count - 1) / args.count >>> 0);
        }
        res.items = res.items.filter(item => !selectedScopes.some(selectedItem => selectedItem.nameOrUrl === item.name || selectedItem.nameOrUrl === item.url));
        res.items.forEach(item => {
            item.nameOrUrl = item.name || item.url;
        });
        return res.items;
    });

    const onScopeChange = (args) => {
        const scopes = _.cloneDeep(args.newValue);
        const clonedParameter = _.cloneDeep(dataOptimizeParameter);
        setSelectedScopes(scopes);
        setIsInit(false);
        switch (clonedParameter.nodeQueryParameter.viewMode) {
            case DiscoveryNodeViewMode.Container:
                clonedParameter.nodeQueryParameter.containerIds = scopes.map(item => item.id);
                clonedParameter.nodeQueryParameter.siteIds = [];
                break;
            case DiscoveryNodeViewMode.Site:
            case DiscoveryNodeViewMode.SiteInContainer:
                clonedParameter.nodeQueryParameter.containerIds = [];
                clonedParameter.nodeQueryParameter.siteIds = scopes.map(item => item.id);
                break;
            default:
                break;
        }
        onChange(clonedParameter);
    };

    return (
        <div>
            <div className="reco-optimize-title require">{RMResx.RM_FA_DataOptimize_ScopeTitle}</div>
            <div>
                <R.Multicombobox
                    id="raScope"
                    width={"100%"}
                    checkedField="checked"
                    textField="nameOrUrl"
                    valueField="id"
                    hasFilter={true}
                    required={true}
                    value={selectedScopes}
                    onChange={onScopeChange}
                    doLoad={doLoad}
                    lazyStep={15}
                    aria={{ ariaLabel: RMResx.RM_FA_DataOptimize_ScopeTitle }}
                />
            </div>
        </div>
    );
};

export default ContainerOrSCScope;