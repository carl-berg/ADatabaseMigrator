# ADatabaseMigrator - An Appeasing Database Migrator
A small and flexible package you can use to run database migration scripts.

## Getting started
1. Create a class project to contain your migrations
2. Place your migrations in a file structure like this:
   ```
   \Scripts\Migrations\001.00.00\001_MyFirstMigration.sql
   \Scripts\Migrations\001.00.00\002_MySecondMigration.sql
   \Scripts\RunAlways\001_RunLog_.sql
   \Scripts\RunIfChanged\001_MyFirstView_.sql
   ```
3. Add this `ItemGroup` to the `csproj` file to configure the files in the script folder become embedded resources in the assembly
   ```xml
   <ItemGroup>
     <EmbeddedResource Include="Scripts\**\*.sql" />
   </ItemGroup>
   ```
4. Add a `nuget` reference to `ADatabaseMigrator`
5. Create and invoke a migrator like so:
   ```c#
   var migrator = new Migrator(
       scriptLoader: new EmbeddedResourceScriptLoader(new MD5ScriptHasher(), config => config
           .UsingAssemblyFromType<ScriptLoaderTests>()
               .AddNamespaces<VersionFromPathVersionLoader>(MigrationScriptRunType.RunOnce, "Scripts.Migrations")
               .AddNamespaces<VersionFromAssemblyVersionLoader>(MigrationScriptRunType.RunIfChanged, "Scripts.RunIfChanged")
               .AddNamespaces<VersionFromAssemblyVersionLoader>(MigrationScriptRunType.RunAlways, "Scripts.RunAlways")),
       journalManager: new MigrationScriptJournalManager(connection),
       scriptRunner: new MigrationScriptRunner(connection));

   await migrator.Migrate();
   ```
   Some things to note about the configuration embedded resource script loader configuration here:
     - `MigrationScriptRunType`: `ADatabaseMigrator` comes predefined with some run types:
       - `MigrationScriptRunType.RunOnce`: Means this script is meant to run once. This can be used for changes to the database schema, like adding columns.
       - `MigrationScriptRunType.RunIfChanged`: Means this script is mean to run every time the content of the file is changed (this is what the script hasher is used for). This can be used for views or stored procedures.
       - `MigrationScriptRunType.RunAlways`: Means this script is mean to run every time. This could be used for keeping a log of every time the migrator is run for instance.
     - `UsingAssemblyFromType<ScriptLoaderTests>()` specifies an assembly from which we can configure subsequent namespaces to fetch migrations from. We can call this method multiple times if we have migrations from multiple assemblies to load.
     - `AddNamespaces<VersionFromPathVersionLoader>(MigrationScriptRunType.RunOnce, "Scripts.Migrations")`, specifies that we want to load scripts from inside the folder `Scripts\Migrations`, we want them to be run only once and we want to extract the version number from the the path.
     - `AddNamespaces<VersionFromAssemblyVersionLoader>(MigrationScriptRunType.RunIfChanged, "Scripts.RunIfChanged")` specifies we want to load scripts from inside the folder `Scripts\RunIfChanged`, we want them to be run every time they are changed (when the hash of the file changes) and we use version number from the previously specified assembly.
     - `AddNamespaces<VersionFromAssemblyVersionLoader>(MigrationScriptRunType.RunAlways, "Scripts.RunAlways")` specifies that we want to load scripts from inside the folder `Scripts\RunAlways`, we want them to be run every time we run the migrator and we use version number from the previously specified assembly.
     - Note that the order of the `AddNamespaces` invocations specifies the execution order, so in our example the _Migrations_ are executed first, then _RunIfChanged_ and last _RunAlways_. Within a namespace the migrations found are executed first in order of version, then in order of embedded resource name. This means that if you want your scripts to run in a specific order, you have full control over this by adding namespaces in the order you want.

## Packages and Contents

**ADatabaseMigrator**
Contains the `Migrator` class which you instantiate to run your migrations. The class needs 3 things in order to be instantiated:
- A script loader, that can find the script files you want to run.
- A journal manager, that can load already executed scripts from the database.
- A script runner that can execute scripts.

You can write your own script loaders, journal managers and scripts runners, but the package comes with some predefined classes to use or extend:
- `EmbeddedResourceScriptLoader` is a script loader that loads scripts from embedded resources in one or more assemblies. This class also requires a script hasher in order to create a unique hash for the loaded scripts, the `MD5ScriptHasher` is provided as a default script hasher.
- `MigrationScriptJournalManager` that loads executed scripts from a table named `SchemaVersionJournal`.
- `MigrationScriptRunner` that can execute scripts given a connection (and an optional transaction if you want the whole migration to be executed in a provided transaction).

**ADatabaseFixture.SqlServer**
Contains `SqlServerMigrationScriptRunner` which you can use as a script runner instead of `MigrationScriptRunner` if you need batch support (like if your scripts contains `GO` statements for instance).

## Logging
Simple logging of executed scripts or script failures can be implemented like this (replace `Console.WriteLine` with your own choice of logging mechanism):
```c#
var migrator = new Migrator(/*...*/)

migrator.ScriptMigrationSucceeded += (_, args) => Console.WriteLine($"Executed script '{args.Script.Name}'");
migrator.ScriptMigrationFailed += (_, args) => Console.WriteLine($"Failed to execute script '{args.Script.Name}': {args.Exception}");

await migrator.Migrate();
```

## Compatibility with GalacticWasteManagement
`ADatabaseMigrator` is inspired by, and can be somewhat made compatible with [Galactic-Waste-Management](https://github.com/mattiasnordqvist/Galactic-Waste-Management), which has been deprecated. 

GalacticWasteManagement and ADatabaseMigrator solves slightly different problems. While GalacticWasteManagement can be used as a development tool as well as a deployment tool, has an array of built in migration types and modes, ADatabaseMigrator is a simpler and more focused tool. ADatabaseMigrator can execute migration scripts, and that's it. ADatabaseMigrator however is built to simplify configuration and extension to allow you to build your own migration orchestration by configuration, extending or replacing parts and making it easy to introduce custom run types if you feel the need for it.