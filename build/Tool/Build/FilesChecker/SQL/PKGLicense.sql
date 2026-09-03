select '', Replace(cast(license_agreement_name AS varchar(8000)),';#',';'), null, 'Binary'
from FileList
where list_name = 'Third'