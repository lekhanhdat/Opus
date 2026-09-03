select filename, location_in_package, file_version, 'Binary'
from FileList
where list_name = 'Binary'

union all

select filename, location_in_package, file_version, 'Third'
from FileList
where list_name = 'Third'
-- 去掉检查EndUserSolution\PhysicalRecord.wsp中的第三方。因为PhysicalRecord.wsp是压缩包，不能检查其内部文件。
and location_in_package not like 'EndUserSolution\PhysicalRecord.wsp'