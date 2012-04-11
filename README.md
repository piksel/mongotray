## MongoDB server monitor for windows

A tool for controlling a mongodb-server instance on windows.

### Requirements
 * .NET 4 (could probably be loosened to 3.5 or even 2.0)
 * MongoDB C# Driver [github:mongodb/mongo-csharp-driver](https://github.com/mongodb/mongo-csharp-driver)

### Known bugs / limitations
 * No support for shards or mongod as a service.
 * Tries to kill all the systems mongod-processes when failing to quit nicely.

### Things to fix
 * Some bad naming in the source (button1 etc.)
 * Change the namespace to mongotray