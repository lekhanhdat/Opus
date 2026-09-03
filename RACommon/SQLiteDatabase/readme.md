# SQLiteClient使用说明
SQLiteClient集成了针对SQLite的基础操作，并提供了更加方便的实体操作。

## 注意
* 使用WAL mode，连接池管理连接，最后务必要Close

## Attributes
* TableAttribute：表名，如果不标记，以类名当作表名
* ColumnAttribute：字段名，如果不标记，以属性名当作字段名
* IgnoreAttribute：被标记的属性会过被过滤，不被当作字段
* UpdateColumnAttribute：在Update和Upsert中，只更新被标记的字段
* UniqueColumnAttribute：约束标记，在Delete和Update中被当作Where条件，在Upsert中被当作Conflict条件

## 实体用法
* Query：会自动将结果转换成实体
* Insert：传入实体
* Delete：传入实体，UniqueColumnAttribute是必须的
* Update：传入实体，UniqueColumnAttribute和UpdateColumnAttribute是必须的
* Upsert：传入实体，UniqueColumnAttribute和UpdateColumnAttribute是必须的，并且建表时要将标记了UniqueColumnAttribute的字段当作Unique字段创建