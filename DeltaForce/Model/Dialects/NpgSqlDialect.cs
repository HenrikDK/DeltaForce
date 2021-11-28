namespace DeltaForce.Model.Dialects;

public class NpgSqlDialect : ISqlDialect
{
    public string CreateDb => @" /* DeltaForce */
drop schema deltaforce cascade;

create schema if not exists deltaforce;

create table if not exists deltaforce.script(
    Id SERIAL primary key,
    ScriptName varchar(255) null,
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

    public string GetScript => @" /* DeltaForce */
select s.id, s.scriptname, s.repositorypath, s.hash, s.status, s.errormessage 
from deltaforce.script s 
where s.repositorypath = @path
limit 1;";

    public string InsertScript => @" /* DeltaForce */
insert into deltaforce.script(scriptname, repositorypath, hash, status, errormessage, created, createdby)
values (@scriptname, @repositorypath, @hash, @status, @errormessage, current_timestamp, 'DeltaForce');";
    
    public string UpdateScript => @" /* DeltaForce */
update deltaforce.script
    set
        Hash = @Hash,
        ErrorMessage = @errormessage,
        Modified = current_timestamp,
        modifiedby = 'DeltaForce',
        Status = @Status
where id = @id;";
    
    public string GetState => @" /* DeltaForce */
select lastcommit from deltaforce.state where id = 1;";
    
    public string SaveState => @" /* DeltaForce */
insert into deltaforce.state (id, lastcommit, modified)
values (1, @commitHash, current_timestamp)
on conflict on constraint state_id_key do update set 
        lastcommit = @commitHash,
        modified = current_timestamp;";
}