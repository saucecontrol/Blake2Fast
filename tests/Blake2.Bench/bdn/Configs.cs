// Copyright © Clinton Ingram and Contributors.  Licensed under the MIT License.

using System.IO;
using System.Linq;

using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.DotNetCli;

namespace Blake2Bench;

public class CustomConfig : ManualConfig
{
	public CustomConfig(bool includeHash = false)
	{
		var outDir = Directory.GetParent(typeof(CustomConfig).Assembly.Location);
		while (outDir.Name != "out")
			outDir = outDir.Parent;

		AddLogger(DefaultConfig.Instance.GetLoggers().ToArray());
		AddColumnProvider(DefaultConfig.Instance.GetColumnProviders().Where(cp => cp.GetType().Name != "ParamsColumnProvider").ToArray());
		AddExporter(DefaultConfig.Instance.GetExporters().ToArray());
		AddDiagnoser(MemoryDiagnoser.Default);
		if (includeHash) AddColumn(new HashColumn());
		AddColumn(new DataLengthColumn());
		Options |= ConfigOptions.DisableOptimizationsValidator;
		Orderer = ByPlatformByDataLengthOrderer.Instance;
		ArtifactsPath = Path.Combine(outDir.FullName, "bdn", "Blake2.Bench");
	}
}

public class MultipleJitConfig : CustomConfig
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
	}
}

public class DefaultCustomConfig : CustomConfig
{
	public DefaultCustomConfig(bool includeHash = true) : base(includeHash)
	{
		AddJob(Job.ShortRun);
	}
}
