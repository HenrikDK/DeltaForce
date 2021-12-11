namespace DeltaForce.Model.Dialects;

public class PgSqlDialect : ISqlDialect
{
    public string CheckSchema => @" /* DeltaForce */
select 1 from pg_catalog.pg_namespace
where nspname = 'deltaforce'";
    
    public string CreateSchema => @" /* DeltaForce */
create schema if not exists deltaforce;

create table if not exists deltaforce.script(
    Id SERIAL primary key,
    FileName varchar(255) null,
    RepositoryPath varchar(255) null,
    Hash varchar(512) null,
    Status int not null default 0,
    ErrorMessage text null,
    Created timestamp not null,
    CreatedBy varchar(50) null,
    Modified timestamp null,
    ModifiedBy varchar(50) null
);

create table if not exists deltaforce.state(
    Id int unique,
    LastCommit varchar(255) null,
    Modified timestamp not null
);";

    public string GetScripts => @" /* DeltaForce */
select s.Id, s.FileName, s.RepositoryPath, s.Hash, s.Status, s.ErrorMessage 
from deltaforce.script s;";

    public string InsertScript => @" /* DeltaForce */
insert into deltaforce.script(FileName, RepositoryPath, Hash, Status, ErrorMessage, Created, CreatedBy)
values (@FileName, @RepositoryPath, @Hash, @Status, @ErrorMessage, current_timestamp, 'DeltaForce');";
    
    public string UpdateScript => @" /* DeltaForce */
update deltaforce.script
    set
        Hash = @Hash,
        ErrorMessage = @ErrorMessage,
        Modified = current_timestamp,
        ModifiedBy = 'DeltaForce',
        Status = @Status
where id = @Id;";
    
    public string GetState => @" /* DeltaForce */
select lastcommit from deltaforce.state where id = 1;";
    
    public string SaveState => @" /* DeltaForce */
insert into deltaforce.state (id, lastcommit, modified)
values (1, @hash, current_timestamp)
on conflict on constraint state_id_key do update set
        lastcommit = @hash,
        modified = current_timestamp;";
}