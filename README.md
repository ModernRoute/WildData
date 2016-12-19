# WildData
.NET Lightweight Data Client Framework based on native queries and utilizes optional Linq support. 

## Nuget
https://www.nuget.org/packages/WildData

https://www.nuget.org/packages/WildData.Npgsql

## Example of use
https://github.com/foxanthony/MeteringDevices/tree/master/Application/Data

## Quick start
1. Install nuget packages.
2. Create your models (examples: [one](https://github.com/foxanthony/MeteringDevices/blob/master/Application/Data/MeteringValue.cs), [two](https://github.com/foxanthony/MeteringDevices/blob/master/Application/Data/CurrentMeteringValue.cs)). The models must implement one of the following interfaces: IReadOnlyModel, IReadOnlyModel\<TKey\>, IReadWriteModel\<TKey\>.
3. Describe your [session](https://github.com/foxanthony/MeteringDevices/blob/master/Application/Data/ISession.cs) interface. Put your repositories into the session. If you need just base CRUD functions (Store, Update, Delete) you even **don't** have to extend your repositories!
4. Introduce [Session](https://github.com/foxanthony/MeteringDevices/blob/master/Application/Data/Session.cs) class that extends the BaseSession and implements your ISession interface defined above.
5. Are you using dependency injection? No problem! See Ninject [example](https://github.com/foxanthony/MeteringDevices/blob/master/Application/Data/Register.cs).
6. Use your ISession and repositories like this:
```csharp
using (ISession session = NinjectKernelObject.Get<ISession>())
{
   IList<MeteringValue> query = session.MeteringValueRepository.Fetch().ToList(); // Linq!
   
   MeteringValue meteringValue = new MeteringValue 
   {
      When = DateTime.Now,
      Night = 1,
      Day = 2,
      Hot = 3,
      Cold = 4,
      Kitchen = 5,
      Room = 6
   };
   
   session.MeteringValueRepository.StoreOrUpdate(meteringValue); // <-- It also reads an Id!
   session.MeteringValueRepository.Delete(meteringValue); 
}
```

Enjoy!

Tip: if you using the project inside the MVC/WebApi controller it's a good idea to create a base one and implement lazy session initialization. Thus, you always have a session inside your controller and it would be created on demand. Thus, if one of your methods doesn't use a session the attempt to establish database connection would not be done!

## Attributes
Use the [attributes](https://github.com/ModernRoute/WildData/tree/master/WildData/Attributes) to customize behaviour (documentation will be added soon.)

## Custom queries
Custom queries should be constructed using a bunch of embedded helpers!

## Roadmap (future plans)
- Stable API.
- More databases support (MySQL, SqlServer are the first candidates).
- Bulk insert/update/delete.
- Microsoft.AspNet.Identity support.
- Asynchronous
- Tests
- Better implementation for Linq support.

## Linq
- Linq support are limited for ordering/filtering/pagination purposes. The native queries must be used instead. The purpose of Linq support is to be compatible with .NET OData libraries. 

Project is in early stage (alpha) and it's not recommended for production use. However, pull requests are welcome!



