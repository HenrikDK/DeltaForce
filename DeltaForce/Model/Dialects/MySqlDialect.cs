namespace DeltaForce.Model.Dialects;

public class MySqlDialect : ISqlDialect
{
    public string CheckSchema => @" /* DeltaForce */
select 1 from information_schema.SCHEMATA
where SCHEMA_NAME = 'DeltaForce'";
    
    public string CreateSchema => @" /* DeltaForce */
create schema if not exists DeltaForce;

create table if not exists DeltaForce.Script(
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

create table if not exists DeltaForce.State(
    Id int unique,
    LastCommit varchar(255) null,
    Modified timestamp not null
);";

    public string GetScripts => @" /* DeltaForce */
select s.Id, s.FileName, s.RepositoryPath, s.Hash, s.Status, s.ErrorMessage
from DeltaForce.Script s;";

    public string InsertScript => @" /* DeltaForce */
insert into DeltaForce.Script (FileName, RepositoryPath, Hash, Status, ErrorMessage, Created, CreatedBy)
values (@FileName, @RepositoryPath, @Hash, @Status, @ErrorMessage, current_timestamp, 'DeltaForce');";
    
    public string UpdateScript => @" /* DeltaForce */
update DeltaForce.Script
    set
        Hash = @Hash,
        ErrorMessage = @ErrorMessage,
        Modified = current_timestamp,
        Status = @Status
where Id = @Id;";
    
    public string GetState => @" /* DeltaForce */
select LastCommit from DeltaForce.State where Id = 1;";
    
    public string SaveState => @" /* DeltaForce */
insert into DeltaForce.State (Id, LastCommit, Modified)
values (1, @Hash, current_timestamp)
on duplicate key update
        LastCommit = @hash,
        Modified = current_timestamp;";
}