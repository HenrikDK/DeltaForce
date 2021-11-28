namespace DeltaForce.Model.Dialects;

public class MySqlDialect : ISqlDialect
{
    public string CreateDb => @" /* DeltaForce */
create schema if not exists DeltaForce;

create table if not exists DeltaForce.Script(
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

create table if not exists DeltaForce.State(
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
insert into deltaforce.script (scriptname, repositorypath, hash, status, errormessage, createdby)
values (@scriptname, @repositorypath, @hash, @status, @errormessage, 'DeltaForce');";
    
    public string UpdateScript => @" /* DeltaForce */
update deltaforce.script
    set
        hash = @Hash,
        errormessage = @errormessage,
        Modified = current_timestamp,
        Status = @Status
where id = @id;";
    
    public string GetState => @" /* DeltaForce */
select lastcommit from deltaforce.state where id = 1;";
    
    public string SaveState => @" /* DeltaForce */
insert into deltaforce.state (id, lastcommit, modified)
values (1, @hash, current_timestamp)
on conflict on constraint state_id_key do update set 
        lastcommit = @hash,
        modified = current_timestamp;";

}