# DeltaForce
A simple containerized DB migration service that will run sql scripts from a git repository folder.

DeltaForce currently supports pgsql, mysql, mariadb, and mssql.

The service will monitor the folder (non-recursively) and apply new scripts sorted alphabetically. 

When using the service it is recommended to prefix scripts using a common key and 4 digit counter to ensure scripts are applied in the correct sequence.

You can combine this tool with a scripts repository and a sturdy review process to ensure that any change making it's way into production will be of high quality and have a clear and visible audit trail.

## Usage
To use the service deploy it using a container orchestrator of your choice (container image available on docker hub [here](https://hub.docker.com/repository/docker/henrikdk/delta-force)). 

An example of the settings required by the service are provided below (more setting samples can be found in the test project [here](https://github.com/HenrikDK/DeltaForce/tree/main/DeltaForce.Test)):
```
{
  "database-type": "pgsql, mariadb, mysql, mssql",
  "connection-string": "ado.net connection string",
  
  "git-branch": "branch name eg. main / master / dev / tst / uat",
  "git-repository": ".git url for use with ssh key",
  "git-ssh-key-path": "/path/to/key.file",
  "git-path": "/"
}
```

As apparent the settings file shipped in the container doesn't actually contain a valid configuration.

The service uses .Net's built in IConfiguration facility to fetch it's settings, allowing any configuration value to be overridden by environment variables (most container orchestrators provide facilities to do this). 

The ssh key needs to be mounted in the container and the configuration value git-ssh-key-path should point the service to use this file.

The service uses serilog to logs it's work in a json structure to std out and std error so scraping these will allow the service to be integrated into existing error collection workflows.

## Startup
On first startup the service will attempt to check if the database instance already has a schema called 'DeltaForce', and if not create it and the required tables. These tables are then used to maintain application state and track which scripts have been processed and if they need re-processing (eg. file contents have changed).

## Metrics
The service also exposes a prometheus metrics endpoint on port 1402 with the path '/metrics', for now it only tracks two values:

1.  "delta_force_pending_scripts_count"
2.  "delta_force_apply_script_time_seconds"

## Inspiration: 
This work was inspired by other projects check them out:

[grate - the SQL scripts migration runner](https://github.com/erikbra/grate)

[RoundhousE - a Database Migration Utility for .NET](https://github.com/chucknorris/roundhouse)

## Todo

- Azure key vault support
- Teams / Slack Notifications support