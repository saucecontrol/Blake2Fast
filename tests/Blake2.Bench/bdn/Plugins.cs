using System.Linq;
using System.Collections.Generic;

using BenchmarkDotNet.Order;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

class HashColumn : IColumn
{
	public string ColumnName { get; } = "Hash";
	public string Legend => "Hex display of hash result";
	public string Id => ColumnName;

	public ColumnCategory Category => ColumnCategory.Job;
	public UnitType UnitType => UnitType.Dimensionless;

	public int PriorityInCategory => 2;
	public bool AlwaysShow => true;
	public bool IsNumeric => false;

	public bool IsDefault(Summary summary, Benchmark benchmark) => false;
	public bool IsAvailable(Summary summary) => true;

	public string GetValue(Summary summary, Benchmark benchmark) => GetValue(summary, benchmark, null);
	public string GetValue(Summary summary, Benchmark benchmark, ISummaryStyle style)
	{
		var hash = ((byte[])benchmark.Target.Method.Invoke(null, null)).ToHexString();
		if (hash.Length > 16)
			hash = hash.Substring(0, 12) + "...";
		return hash;
	}

	public override string ToString() => ColumnName;
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
