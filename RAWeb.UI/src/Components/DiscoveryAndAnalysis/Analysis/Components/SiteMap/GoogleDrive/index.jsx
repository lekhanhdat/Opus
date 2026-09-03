import { useEffect, useState } from "react";

import { GoogleDriveBasicDataRequester } from "../../../requests/GoogleDrive";

function GoogleDriveSiteMap(props) {
    const { URL, onChange } = props;

    const [allOrganizations, setAllOrganizations] = useState([]);

    useEffect(() => {
        handleGetAllOrganizations();
    }, []);

    const handleGetAllOrganizations = async () => {
        const organizations = await GoogleDriveBasicDataRequester.getOrganizationInfoes();
        if (organizations && organizations.length) {
            organizations[0].checked = true;
            setAllOrganizations(organizations);
            onChange(organizations[0].organizationId);
        }
    };

    return (
        <section className="reco-sitemap">
            <div className="margin-top-l">
                <$g.SiteMap data={URL} />
            </div>

            <div className="reco-sitemap-right">
                <R.Combobox
                    id="raTenant"
                    items={allOrganizations}
                    searchable={false}
                    textField="name"
                    valueField="organizationId"
                    onChange={(args) => onChange(args.newValue.organizationId)}
                />
            </div>
        </section>
    );
}

export default GoogleDriveSiteMap;
