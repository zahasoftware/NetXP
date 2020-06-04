# NetXP
Cross-Platform framework for .Net, Starting From NetStandard To NetCore, And Xamarin.

For more information about Net Standard visit: [Net Standard](https://docs.microsoft.com/en-us/dotnet/articles/standard/library).

### Source Or Nugets
Download source code or download from nuget.

| Package          | Comment
|------------------|----------------------------------------------------------------------------------------|
|NetXP.NetStandard | Main Reference (Contains all interfaces and some implementations)
|NetXP.NetStandard.CompositionRoot | Initialize implementations of Auditory, Cryptogrqaphy, Network.Services, Serialization).
|NetXP.NetStandard.DependencyInjection				|					Interface to work with dependency injection.
|NetXP.NetStandard.DependencyInjection.Implementations.StructureMap	| Implement depency injection interface to work with StructureMap. 
|NetXP.NetStandard.Network							|					Network utils (Email, LJP Protocol, SLP Protocol, TCP, SOAP [Services Client]
|NetXP.NetStandard.Network.Services.Implementations|					Implementations of NetXP.NetStandard.Network.Services [SOAP].


### Initialization in ASP.Net Core (Just for Cryptography Example) 
Download Nuget Packages:
- NetXP.NetStandard.Cryptography.Implementations 
- NetXP.NetStandard.DependencyInjection.Implementations.StructureMaps

```csharp

public IServiceProvider ConfigureServices(IServiceCollection services)
{
	var container = new Container(); //Structure Map Container
	var customContainer = new SMContainer(container); //NetXP Container
	
	//Registration of classes
	customContainer.Configuration.Configure((IRegister r) =>
	{
		r.RegisterCryptography();//For All Registration r.RegisterAllNetXP(customContainer); //Nuget NetXP.NetStandarad.CompositionRoot
	}
	
	container.Populate(services);//<-- !!!Populate must be execute on the final of this block
	return container.GetInstance<IServiceProvider>();
}

```

or manually

```csharp

public void ConfigureServices(IServiceCollection services)
{
	uc.AddTrasient<INameResolverFactory<IAsymetricCrypt>, AsymetricFactory>();
    uc.AddTrasient<IHash, HashSHA256>();
	uc.AddTrasient<ISymetricCrypt, SymetricAes>();
    uc.AddTrasient<IAsymetricCrypt, AsymetricCryptWithMSRSA>();
}

```

### Examples
See unit tests projects for examples.

**NOTES:** 
- UnitTest projects are optional.
- Current NetStandard Version Is Set To 2.1
- Not compatible with net framework.

### License
[MIT](https://choosealicense.com/licenses/mit/)
