
--OPTION 1: just delete data from  tables
delete from shared.parameter_overrides;
delete from shared.parameters;
delete from iam.role_permissions;
delete from iam.user_roles;
delete from iam.roles;
delete from iam.permissions;
delete from iam.users;
delete from iam.organizations;

--OPTION 2: drop tables and schema
drop table shared.parameter_overrides;
drop table shared.parameters;
drop schema shared;

drop table iam.role_permissions;
drop table iam.user_roles;
drop table iam.roles;
drop table iam.permissions;
drop table iam.users;
drop table iam.organizations;
drop schema iam;
