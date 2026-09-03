import React, { Component } from 'react';
import './index.less';

export default class GlobalDC extends R.Component {
    constructor(props) {
        super(props);
        this.state = {
            globalDCItems: [],
            isMultiGeoEnabled: RM.gData.enableMultiGEOFeature || false,
            selectedDC: ""
        };
    }

    componentInit() {
        this.setAgentLatestVersion();
    }

    componentReceive(element) {
        if (element && element.type === 'MULTI_GEO_STATUS_CHANGED') {
            this.setState({
                isMultiGeoEnabled: element.payload.isEnabled
            });
        }
    }

    getSafeDCName = (dcString) => {
        if (!dcString || dcString === "undefined") {
            return "";
        }

        try {
            const parsedData = JSON.parse(dcString);
            return parsedData?.name || "";
        } catch (error) {
            return "";
        }
    }

    onGlobalDCChange = (val) => {
        const selectedObj = val.newValue || {};
        
        const displayName = selectedObj.DCDisplayName || "";
        const ssoUrl = selectedObj.SSOUrl;

        this.setState({ selectedDC: displayName });

        if (ssoUrl) {
            $$.loading(true); 
            window.location.href = ssoUrl;
        } else {
            console.error('Teleportation failed: SSOUrl is missing!');
        }
    }

    setAgentLatestVersion = () => {
        $$.loading(true);
        let option = {
            url: "/api/MultiGEODataCenterApi/GetMultiGEODCInformation",
            method: "GET",
        };
        fetchUtility(option).then((res) => { 
            $$.loading(false);

            const dcItems = res?.DCsSupported || [];
            const mainDCId = res?.MainDC;
            const targetDCId = res?.CurrentDC || "";
            
            const mainDC = dcItems.filter((x) => x.DCInternalName === mainDCId);
            const others = dcItems
                .filter((x) => x.DCInternalName !== mainDCId)
                .sort((a, b) =>
                    (a.DCDisplayName || "").localeCompare(b.DCDisplayName || ""),
                );
            const sortedDCItems = [...mainDC, ...others];
            const matchedDC = sortedDCItems.find(item => item?.DCInternalName === targetDCId) || {};
            const displayValue = matchedDC.DCDisplayName || "";

            this.setState({
                globalDCItems: sortedDCItems,
                selectedDC: displayValue
            });
            
        }).catch((e) => {
            $$.loading(false);
        });
    }

    render() {
        return (
            <div id="raGlobalDcSelector" className="ra-global-dc-selector">
                {this.state.isMultiGeoEnabled &&
                    <div className="ra-global-dc-btn-wrapper">
                        <div className="ra-global-dc-combo-container">
                            <R.Combobox
                                id="raGlobalDCCom"
                                className="ra-borderless-combo"
                                width="100%"
                                items={this.state.globalDCItems}
                                prefix={RMResx.RM_AR_CP_Multi_Geo_DC}
                                textField="DCDisplayName"
                                valueField="DCInternalName"
                                tooltipField="DCDisplayName"
                                checkedField="checked"
                                value={this.state.selectedDC}
                                onChange={this.onGlobalDCChange}
                            />
                        </div>
                    </div>
                }
            </div>
        );
    }
}