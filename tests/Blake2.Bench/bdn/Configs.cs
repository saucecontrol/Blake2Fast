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

		AddJob(Job.ShortRun.WithJit(Jit.RyuJit).WithPlatform(Platform.X64).WithToolchain(CsProjCoreToolchain.From(cli30_64)).WithId("netcoreapp3.0"));
		AddJob(Job.ShortRun.WithJit(Jit.RyuJit).WithPlatform(Platform.X86).WithToolchain(CsProjCoreToolchain.From(cli30_32)).WithId("netcoreapp3.0"));
		AddJob(Job.ShortRun.WithJit(Jit.RyuJit).WithPlatform(Platform.X64).WithToolchain(CsProjCoreToolchain.From(cli21_64)).WithId("netcoreapp2.1"));
		AddJob(Job.ShortRun.WithJit(Jit.RyuJit).WithPlatform(Platform.X86).WithToolchain(CsProjCoreToolchain.From(cli21_32)).WithId("netcoreapp2.1"));

		AddJob(Job.ShortRun.WithRuntime(ClrRuntime.Net472).WithJit(Jit.RyuJit).WithPlatform(Platform.X64).WithId("net472").AsBaseline());
		AddJob(Job.ShortRun.WithRuntime(ClrRuntime.Net472).WithJit(Jit.LegacyJit).WithPlatform(Platform.X86).WithId("net472").AsBaseline());

		AddLogger(DefaultConfig.Instance.GetLoggers().ToArray());
		AddColumnProvider(DefaultConfig.Instance.GetColumnProviders().Where(cp => cp.GetType().Name != "ParamsColumnProvider").ToArray());
		AddExporter(DefaultConfig.Instance.GetExporters().ToArray());
		AddDiagnoser(MemoryDiagnoser.Default);
		AddColumn(new DataLengthColumn());
		Orderer = ByPlatformByDataLengthOrderer.Instance;
		ArtifactsPath = @"..\..\out\bdn\Blake2.Bench";
	}
}

public class AllowNonOptimizedConfig : ManualConfig
{
	public AllowNonOptimizedConfig(bool includeHash = true)
	{
		AddJob(Job.ShortRun);

		AddValidator(JitOptimizationsValidator.DontFailOnError); // needed for Blake2Core, which is not optimized in nuget package

		AddLogger(DefaultConfig.Instance.GetLoggers().ToArray());
		AddColumnProvider(DefaultConfig.Instance.GetColumnProviders().Where(cp => cp.GetType().Name != "ParamsColumnProvider").ToArray());
		AddExporter(DefaultConfig.Instance.GetExporters().ToArray());
		AddDiagnoser(MemoryDiagnoser.Default);
		if (includeHash) AddColumn(new HashColumn());
		AddColumn(new DataLengthColumn());
		Orderer = ByPlatformByDataLengthOrderer.Instance;
		ArtifactsPath = @"..\..\out\bdn\Blake2.Bench";
	}
}
