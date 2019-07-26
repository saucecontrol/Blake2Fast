using System.Linq;

using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Validators;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.DotNetCli;

public class MultipleJitConfig : ManualConfig
{
	public MultipleJitConfig()
	{
		var cli30_32 = NetCoreAppSettings.NetCoreApp30.WithCustomDotNetCliPath(Paths.DotNetCLIx86, "Default");
		var cli30_64 = NetCoreAppSettings.NetCoreApp30.WithCustomDotNetCliPath(Paths.DotNetCLIx64, "Default");
		var cli21_32 = NetCoreAppSettings.NetCoreApp21.WithCustomDotNetCliPath(Paths.DotNetCLIx86, "Default");
		var cli21_64 = NetCoreAppSettings.NetCoreApp21.WithCustomDotNetCliPath(Paths.DotNetCLIx64, "Default");

		Add(Job.ShortRun.With(Jit.RyuJit).With(Platform.X64).With(CsProjCoreToolchain.From(cli30_64)).WithId("netcoreapp3.0"));
		Add(Job.ShortRun.With(Jit.RyuJit).With(Platform.X86).With(CsProjCoreToolchain.From(cli30_32)).WithId("netcoreapp3.0"));
		Add(Job.ShortRun.With(Jit.RyuJit).With(Platform.X64).With(CsProjCoreToolchain.From(cli21_64)).WithId("netcoreapp2.1"));
		Add(Job.ShortRun.With(Jit.RyuJit).With(Platform.X86).With(CsProjCoreToolchain.From(cli21_32)).WithId("netcoreapp2.1"));

		Add(Job.ShortRun.With(Runtime.Clr).With(Jit.RyuJit).With(Platform.X64).WithId("net472").AsBaseline());
		Add(Job.ShortRun.With(Runtime.Clr).With(Jit.LegacyJit).With(Platform.X86).WithId("net472").AsBaseline());

		Add(DefaultConfig.Instance.GetLoggers().ToArray());
		Add(DefaultConfig.Instance.GetColumnProviders().Where(cp => cp.GetType().Name != "ParamsColumnProvider").ToArray());
		Add(DefaultConfig.Instance.GetExporters().ToArray());
		Add(MemoryDiagnoser.Default);
		Add(new DataLengthColumn());
		Orderer = ByPlatformByDataLengthOrderer.Instance;
		ArtifactsPath = @"..\..\out\bdn\Blake2.Bench";
	}
}

public class AllowNonOptimizedConfig : ManualConfig
{
	public AllowNonOptimizedConfig(bool includeHash = true)
	{
		var cli30_32 = NetCoreAppSettings.NetCoreApp30.WithCustomDotNetCliPath(Paths.DotNetCLIx86, "Default");
		var cli30_64 = NetCoreAppSettings.NetCoreApp30.WithCustomDotNetCliPath(Paths.DotNetCLIx64, "Default");

		//Add(Job.Job.ShortRun.With(Jit.RyuJit).With(Platform.X64).With(CsProjCoreToolchain.From(cli30_64)).WithId("netcoreapp3.0"));
		//Add(Job.Job.ShortRun.With(Jit.RyuJit).With(Platform.X86).With(CsProjCoreToolchain.From(cli30_32)).WithId("netcoreapp3.0"));
		Add(Job.ShortRun);

		Add(JitOptimizationsValidator.DontFailOnError); // needed for Blake2Core, which is not optimized in nuget package

		Add(DefaultConfig.Instance.GetLoggers().ToArray());
		Add(DefaultConfig.Instance.GetColumnProviders().Where(cp => cp.GetType().Name != "ParamsColumnProvider").ToArray());
		Add(DefaultConfig.Instance.GetExporters().ToArray());
		Add(MemoryDiagnoser.Default);
		if (includeHash) Add(new HashColumn());
		Add(new DataLengthColumn());
		Orderer = ByPlatformByDataLengthOrderer.Instance;
		ArtifactsPath = @"..\..\out\bdn\Blake2.Bench";
	}
}
