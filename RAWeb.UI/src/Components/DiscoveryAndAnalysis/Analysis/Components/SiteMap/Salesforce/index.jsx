import _ from "lodash";

import "../index.less";

const SFSiteMap = ({ URL }) => {
    return (
        <section className="reco-sitemap">
            <div className="margin-top-l">
                <$g.SiteMap data={URL} />
            </div>
        </section>
    );
};

export default SFSiteMap;
