import { useEffect, useRef, useState } from "react";
import { BasicDataRequester } from "../../requests";
import "./index.less";
import _ from "lodash";
import CostPanel from "../CostPanel";

const SiteMap = ({ URL, onChange }) => {
    const costPanelRef = useRef(null);

    const [allTenants, setAllTenants] = useState([]);

    useEffect(() => {
        const handler = async () => {
            const tenants = await BasicDataRequester.getO365TenantInfoes();
            if (tenants && tenants.length > 0) {
                tenants[0].checked = true;
                setAllTenants(tenants);
                onChange(tenants[0].uniqueId);
            }
        };
        handler();
    }, []);

    const onCostClick = async() => {
        const cost =  await fetchUtility({
            url: "/api/RMDiscoveryOffice365ConfigurationApi/GetCostSavingInfo", 
            method: "Get"
        });
        costPanelRef.current.onShow(cost);
    };

    return (
        <section className="reco-sitemap">
            <div className="margin-top-l">
                <$g.SiteMap data={URL} />
            </div>

            <div className="reco-sitemap-right">
                <div className="margin-right-m">
                    <R.Combobox
                        id="raTenant"
                        items={allTenants}
                        searchable={false}
                        textField="name"
                        valueField="uniqueId"
                        onChange={(args) => onChange(args.newValue.uniqueId)}
                    />
                </div>
                <div>
                    <R.ButtonGroup
                        type="action"
                        tooltip={RMResx.RM_FA_Config_Tooltip}
                    >
                        <R.Button
                            id="raCostBtn"
                            text={RMResx.RM_FA_CostSaving_Configuration}
                            onClick={onCostClick}
                        />
                    </R.ButtonGroup>
                </div>
            </div>
            <CostPanel ref={costPanelRef} />
        </section>
    );
};

export default SiteMap;