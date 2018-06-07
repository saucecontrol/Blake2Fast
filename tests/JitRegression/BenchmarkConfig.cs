using System.Linq;
using System.Collections.Generic;

using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.DotNetCli;

public class MultipleJitConfig : ManualConfig
{
	public MultipleJitConfig()
	{
		var cli11_32 = NetCoreAppSettings.NetCoreApp11.WithCustomDotNetCliPath(Paths.DotNetCLIx86, "Default");
		var cli11_64 = NetCoreAppSettings.NetCoreApp11.WithCustomDotNetCliPath(Paths.DotNetCLIx64, "Default");
		var cli20_32 = NetCoreAppSettings.NetCoreApp20.WithCustomDotNetCliPath(Paths.DotNetCLIx86, "Default");
		var cli20_64 = NetCoreAppSettings.NetCoreApp20.WithCustomDotNetCliPath(Paths.DotNetCLIx64, "Default");
		var cli21_32 = NetCoreAppSettings.NetCoreApp21.WithCustomDotNetCliPath(Paths.DotNetCLIx86, "Default");
		var cli21_64 = NetCoreAppSettings.NetCoreApp21.WithCustomDotNetCliPath(Paths.DotNetCLIx64, "Default");

		Add(Job.RyuJitX64.With(CsProjCoreToolchain.From(cli11_64)).AsBaseline().WithId("netcoreapp1.1"));
		Add(Job.RyuJitX86.With(CsProjCoreToolchain.From(cli11_32)).AsBaseline().WithId("netcoreapp1.1"));
		Add(Job.RyuJitX64.With(CsProjCoreToolchain.From(cli20_64)).WithId("netcoreapp2.0"));
		Add(Job.RyuJitX86.With(CsProjCoreToolchain.From(cli20_32)).WithId("netcoreapp2.0"));
		Add(Job.RyuJitX64.With(CsProjCoreToolchain.From(cli21_64)).WithId("netcoreapp2.1"));
		Add(Job.RyuJitX86.With(CsProjCoreToolchain.From(cli21_32)).WithId("netcoreapp2.1"));

#if NETFRAMEWORK // Legacy JIT can only be used from a CLR host
		Add(Job.LegacyJitX64.With(Runtime.Clr).WithId("net46"));
		Add(Job.LegacyJitX86.With(Runtime.Clr).WithId("net46"));
		Add(Job.RyuJitX64.With(Runtime.Clr).WithId("net46"));
#endif

		Add(DefaultConfig.Instance.GetLoggers().ToArray());
		Add(DefaultConfig.Instance.GetColumnProviders().ToArray());
		Set(ByMethodByPlatformOrderProvider.Instance);
	}
}

class ByMethodByPlatformOrderProvider : IOrderProvider
{
	public bool SeparateLogicalGroups => true;

	public static readonly IOrderProvider Instance = new ByMethodByPlatformOrderProvider();

	public IEnumerable<Benchmark> GetExecutionOrder(Benchmark[] benchmarks) => benchmarks;

	public string GetHighlightGroupKey(Benchmark benchmark) => null;

	public string GetLogicalGroupKey(IConfig config, Benchmark[] allBenchmarks, Benchmark benchmark) =>
		string.Join("-", benchmark.Target.DisplayInfo, benchmark.Job.Env.Platform.ToString());

	public IEnumerable<IGrouping<string, Benchmark>> GetLogicalGroupOrder(IEnumerable<IGrouping<string, Benchmark>> logicalGroups) =>
		logicalGroups.OrderBy(it => it.Key);

	public virtual IEnumerable<Benchmark> GetSummaryOrder(Benchmark[] benchmarks, Summary summary)
	{
		var benchmarkLogicalGroups = benchmarks.GroupBy(b => GetLogicalGroupKey(summary.Config, benchmarks, b));
		foreach (var logicalGroup in GetLogicalGroupOrder(benchmarkLogicalGroups))
			foreach (var benchmark in logicalGroup.OrderBy(b => b.Job.Meta.IsBaseline ? 0 : 0).ThenBy(b => b.Job.Id).ThenBy(b => b.Job.Env.Jit.ToString()))
				yield return benchmark;
	}
}
