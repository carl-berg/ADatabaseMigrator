# ADatabaseMigrator - An Appeasing Database Migrator
A small package run and maintain database migration scripts.

Contains the following packages:

**ADatabaseMigrator**
Contains the `Migrator` class which you instantiate to run your migrations. The class needs 3 things in order to be instantiated:
- A script loader, that can find the script files you want to run.
- A journal manager, that can load the already executed scripts from the database.
- A script runner that can execute scripts.

You can write your own script loaders, journal managers and scripts runners, but the package comes with some predefined classes to use or extend:
- `EmbeddedResourceScriptLoader` is a script loader that loads scripts from embedded resources in one or more assemblies. This class also requires a script hasher in order to create a unique hash for the loaded scripts, the `MD5ScriptHasher` is provided as a default script hasher.
- `MigrationScriptJournalManager` that loads executed scripts from a table named `SchemaVersionJournal`.
- `MigrationScriptRunner` that can execute scripts given a connection (and an optional transaction if you want the whole migration to be executed in a provided transaction).

**ADatabaseFixture.SqlServer**
Contains `SqlServerMigrationScriptRunner` which you can use as a script runner instead of `MigrationScriptRunner` if you need batch support (like if your scripts contains `GO` statements for instance).

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
     - `UsingAssemblyFromType<ScriptLoaderTests>()` specifies an assembly from which we can configure subsequent namespaces to fetch migrations from. We can call this method multiple times if we have migrations from multiple assemblies to load.
     - `AddNamespaces<VersionFromPathVersionLoader>(MigrationScriptRunType.RunOnce, "Scripts.Migrations")`, specifies that we want to load scripts from inside the folder `Scripts\Migrations`, we want them to be run only once and we want to extract the version number from the the path.
     - `AddNamespaces<VersionFromAssemblyVersionLoader>(MigrationScriptRunType.RunIfChanged, "Scripts.RunIfChanged")` specifies we want to load scripts from inside the folder `Scripts\RunIfChanged`, we want them to be run every time they are changed (when the hash of the file changes) and we use version number from the previously specified assembly.
     - `AddNamespaces<VersionFromAssemblyVersionLoader>(MigrationScriptRunType.RunAlways, "Scripts.RunAlways")` specifies that we want to load scripts from inside the folder `Scripts\RunAlways`, we want them to be run every time we run the migrator and we use version number from the previously specified assembly.
     - Note that the order of the `AddNamespaces` invocations specifies the execution order, so in our example the _Migrations_ are executed first, then _RunIfChanged_ and last _RunAlways_. Within a namespace the migrations found are executed first in order of version, then in order of embedded resource name.

## Compatibility with GalacticWasteManagement
`ADatabaseMigrator` is inspired by, and can be somewhat made compatible with [Galactic-Waste-Management](https://github.com/mattiasnordqvist/Galactic-Waste-Management), which has been deprecated. By swapping out `MigrationScriptJournalManager` in the example above for `GalacticWasteMigrationScriptJournalManager`, you should be able to run migrations compatible with what GWM calls `LiveField`.

This means that we map the `GWM` run type `Migration` to `ADatabaseMigrator` run type `RunOnce`. `GWM` has support for more run types like `Seed`, `vNext` and more, as well as creating custom run types. ADatabaseMigrator does not have support for these out of the box, but you could create custom mappings by creating your own class that inherits from `GalacticWasteMigrationScriptJournalManager` and override the `ParseGalacticWasteRunType` method. 

Note however that `ADatabaseMigrator` is a more focused tool than `GWM` and will never attempt to drop or restore the database. If you have the need for such orchestrations, I would sugest implementing this yourself, where running `ADatabaseMigrator` migrations could be part of the overall orchestration.