function ExportLocationComponent(props) {
    const { exportLocationList, onChangeExportLocation } = props;

    return (
        <div id="raCPCompliantExportsExportLocation">
            <section className="ce-component-title-main">
                <span tabIndex="0">{RMResx.RM_ES_CompliantExport_Wizard_Step03}</span>
            </section>
            <section>
                <div style={{ font: 'normal normal 600 14px/20px Open Sans' }}>
                    <span>{RMResx.RM_AR_CP_ES_ExportLocation_Content}</span>
                    <$g.Popover>
                        {RMResx.RM_AR_CP_ES_ExportLocation_Popover}
                    </$g.Popover>
                </div>
                <R.Combobox
                    id="raExportLocationCom"
                    tooltipField="Name"
                    width="100%"
                    textField="Name"
                    valueField="Id"
                    checkedField="checked"
                    linkMode={false}
                    searchable={false}
                    items={exportLocationList}
                    onChange={onChangeExportLocation}
                    aria={{
                        ariaLabel: RMResx.RM_AR_CP_ES_ExportLocation_Content,
                    }}
                />
            </section>
        </div>
    );
}

export default ExportLocationComponent;
