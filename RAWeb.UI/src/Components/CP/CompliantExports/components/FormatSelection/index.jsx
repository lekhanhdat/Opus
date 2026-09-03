function FormatSelectionComponent(props) {
    const { formatSelection, onChangeFormatSelection } = props;

    return (
        <div id="raCPCompliantExportsFormatSelection">
            <section className="ce-component-title-main">
                <span tabIndex="0">
                    {RMResx.RM_ES_CompliantExport_Wizard_Step01}
                </span>
            </section>
            <section>
                <div tabIndex="0" className="ce-component-title-secondary">
                    {RMResx.RM_ES_CompliantExport_FormatSelection}
                    <span className="ce-required-input">*</span>
                </div>
                <div>
                    <R.Radio.Group
                        id="compliant-export-format"
                        block
                        name="export-format"
                        items={formatSelection.list}
                        onChange={onChangeFormatSelection}
                    />
                </div>
            </section>
        </div>
    );
}

export default FormatSelectionComponent;
