namespace DeltaForce.Model.Dialects;

public class MsSqlDialect : ISqlDialect
{
    public string CheckSchema => @" /* DeltaForce */
select 1 from sys.schemas 
where name = 'DeltaForce'";
    
    public string CreateSchema => @" /* DeltaForce */
if not exists (select name from sys.schemas where name = 'DeltaForce')
    exec('create schema DeltaForce');

if object_id('DeltaForce.Script', 'U') is null
begin
    create table DeltaForce.Script
    (
        Id int identity primary key,
        FileName varchar(255) null,
        RepositoryPath varchar(255) null,
        Hash varchar(512) null,
        Status int not null default 0,
        ErrorMessage varchar(max) null,
        Created datetime not null,
        CreatedBy varchar(50) not null,
        Modified datetime,
        ModifiedBy varchar(50)
    );
end

if object_id('DeltaForce.State', 'U') is null
begin
    create table DeltaForce.State
    (
        Id int unique,
        LastCommit varchar(255) null,
        Modified datetime not null
    );
end";

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
    ModifiedBy = 'DeltaForce',
    Status = @Status
where Id = @Id;";
    
    public string GetState => @" /* DeltaForce */
select LastCommit from DeltaForce.State where Id = 1;";
    
    public string SaveState => @" /* DeltaForce */
if not exists (select top 1 LastCommit from DeltaForce.State) 
begin
    insert into DeltaForce.State(Id, LastCommit, Modified) 
    values (1, @Hash, current_timestamp); 
end
else 
begin
    update DeltaForce.State
    set
        LastCommit = @Hash,
        Modified = current_timestamp;
end";
}