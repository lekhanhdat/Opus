import React, { useState } from "react";

import { WizardDemo } from "./WizardDemo/index";
import { OrderTableDemo } from "./OrderTableDemo/index";
import { ExportSettingsTreeDemo } from "./ExportSettingsTreeDemo/index";
import { NewScheduledSettings } from "./ScheduledSettingsDemo/index";

import "./index.less";

const ComponentDemo = () => {
    const [tabIndex, setTabIndex] = useState(2);

    const handleChanged = (index) => {
        setTabIndex(index);
    };

    return (
        <div className="opus-common-component-demo">
            <R.Tabcontrol active={tabIndex} onChange={handleChanged}>
                <R.TabPanel tab={"Wizard"}>
                    <WizardDemo />
                </R.TabPanel>
                <R.TabPanel tab={"Order Table"}>
                    <OrderTableDemo />
                </R.TabPanel>
                <R.TabPanel tab={"Export Settings Tree"}>
                    <ExportSettingsTreeDemo/>
                </R.TabPanel>
                <R.TabPanel tab={"Scheduled Settings"}>
                    <NewScheduledSettings/>
                </R.TabPanel>
            </R.Tabcontrol>
        </div>
    );
};

export default ComponentDemo;
