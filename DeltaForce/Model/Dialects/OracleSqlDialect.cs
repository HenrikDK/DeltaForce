namespace DeltaForce.Model.Dialects;

public class OracleSqlDialect : ISqlDialect
{
    public string CreateDb => @" /* DeltaForce */
";

    public string GetScript => @" /* DeltaForce */
select s.id, s.scriptname, s.repositorypath, s.scripttext, s.hash, s.status, s.errormessage 
from deltaforce.script s 
where s.repositorypath = @path
limit 1;";

    public string InsertScript => @" /* DeltaForce */
insert into deltaforce.script (scriptname, repositorypath, scripttext, hash, status, errormessage, createdby)
values (@scriptname, @repositorypath, @scripttext, @hash, @status, @errormessage, 'DeltaForce');";
    
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
    
    public string UpdateState => @" /* DeltaForce */
insert into deltaforce.state (id, lastcommit, modified)
values (1, @hash, current_timestamp)
on conflict on constraint state_id_key do update set 
        lastcommit = @hash,
        modified = current_timestamp;";

}