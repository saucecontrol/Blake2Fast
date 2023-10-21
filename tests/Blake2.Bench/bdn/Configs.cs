// Copyright © Clinton Ingram and Contributors.  Licensed under the MIT License.

using System.Linq;

using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.DotNetCli;

namespace Blake2Bench;

public class MultipleJitConfig : ManualConfig
{
	public MultipleJitConfig()
	{
		var cli80_32 = NetCoreAppSettings.NetCoreApp80.WithCustomDotNetCliPath(Paths.DotNetCLIx86, "Default");
		var cli80_64 = NetCoreAppSettings.NetCoreApp80.WithCustomDotNetCliPath(Paths.DotNetCLIx64, "Default");
		var cli60_32 = NetCoreAppSettings.NetCoreApp60.WithCustomDotNetCliPath(Paths.DotNetCLIx86, "Default");
		var cli60_64 = NetCoreAppSettings.NetCoreApp60.WithCustomDotNetCliPath(Paths.DotNetCLIx64, "Default");

		AddJob(Job.ShortRun.WithJit(Jit.RyuJit).WithPlatform(Platform.X64).WithToolchain(CsProjCoreToolchain.From(cli80_64)).WithId("netcoreapp8.0"));
		AddJob(Job.ShortRun.WithJit(Jit.RyuJit).WithPlatform(Platform.X86).WithToolchain(CsProjCoreToolchain.From(cli80_32)).WithId("netcoreapp8.0"));
		AddJob(Job.ShortRun.WithJit(Jit.RyuJit).WithPlatform(Platform.X64).WithToolchain(CsProjCoreToolchain.From(cli60_64)).WithId("netcoreapp6.0"));
		AddJob(Job.ShortRun.WithJit(Jit.RyuJit).WithPlatform(Platform.X86).WithToolchain(CsProjCoreToolchain.From(cli60_32)).WithId("netcoreapp6.0"));

		AddJob(Job.ShortRun.WithRuntime(ClrRuntime.Net472).WithJit(Jit.RyuJit).WithPlatform(Platform.X64).WithId("net472").AsBaseline());
		AddJob(Job.ShortRun.WithRuntime(ClrRuntime.Net472).WithJit(Jit.LegacyJit).WithPlatform(Platform.X86).WithId("net472").AsBaseline());

		AddLogger(DefaultConfig.Instance.GetLoggers().ToArray());
		AddColumnProvider(DefaultConfig.Instance.GetColumnProviders().Where(cp => cp.GetType().Name != "ParamsColumnProvider").ToArray());
		AddExporter(DefaultConfig.Instance.GetExporters().ToArray());
		AddDiagnoser(MemoryDiagnoser.Default);
		AddColumn(new DataLengthColumn());
		Options |= ConfigOptions.DisableOptimizationsValidator;
		Orderer = ByPlatformByDataLengthOrderer.Instance;
		ArtifactsPath = @"..\..\out\bdn\Blake2.Bench";
	}
}

public class AllowNonOptimizedConfig : ManualConfig
{
	public AllowNonOptimizedConfig(bool includeHash = true)
	{
		AddJob(Job.ShortRun);

		AddLogger(DefaultConfig.Instance.GetLoggers().ToArray());
		AddColumnProvider(DefaultConfig.Instance.GetColumnProviders().Where(cp => cp.GetType().Name != "ParamsColumnProvider").ToArray());
		AddExporter(DefaultConfig.Instance.GetExporters().ToArray());
		AddDiagnoser(MemoryDiagnoser.Default);
		if (includeHash) AddColumn(new HashColumn());
		AddColumn(new DataLengthColumn());
		Options |= ConfigOptions.DisableOptimizationsValidator;
		Orderer = ByPlatformByDataLengthOrderer.Instance;
		ArtifactsPath = @"..\..\out\bdn\Blake2.Bench";
	}
}
