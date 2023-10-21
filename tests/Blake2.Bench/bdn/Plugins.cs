// Copyright © Clinton Ingram and Contributors.  Licensed under the MIT License.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;

using BenchmarkDotNet.Order;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Blake2Bench;

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

	public bool IsDefault(Summary summary, BenchmarkCase benchmark) => false;
	public bool IsAvailable(Summary summary) => true;

	public string GetValue(Summary summary, BenchmarkCase benchmark) => GetValue(summary, benchmark, null);
	public string GetValue(Summary summary, BenchmarkCase benchmark, SummaryStyle style)
	{
		var method = benchmark.Descriptor.WorkloadMethod;
		var instance = Activator.CreateInstance(method.DeclaringType);
		var hash = ((byte[])method.Invoke(instance, new[] { benchmark.Parameters[0].Value })).ToHexString();
		if (hash.Length > 16)
			hash = hash.Substring(0, 12) + "...";
		return hash;
	}

	public override string ToString() => ColumnName;
}

class DataLengthColumn : IColumn
{
	public string ColumnName { get; } = "Data Length";
	public string Legend => "Size of data hashed";
	public string Id => ColumnName;

	public ColumnCategory Category => ColumnCategory.Job;
	public UnitType UnitType => UnitType.Size;

	public int PriorityInCategory => 3;
	public bool AlwaysShow => true;
	public bool IsNumeric => true;

	public bool IsDefault(Summary summary, BenchmarkCase benchmark) => false;
	public bool IsAvailable(Summary summary) => true;

	public string GetValue(Summary summary, BenchmarkCase benchmark) => GetValue(summary, benchmark, null);
	public string GetValue(Summary summary, BenchmarkCase benchmark, SummaryStyle style)
	{
		return ((byte[])benchmark.Parameters[0].Value).Length.ToString();
	}

	public override string ToString() => ColumnName;
}

class ByPlatformByDataLengthOrderer : IOrderer
{
	public bool SeparateLogicalGroups => true;

	public static readonly IOrderer Instance = new ByPlatformByDataLengthOrderer();

	public IEnumerable<BenchmarkCase> GetExecutionOrder(ImmutableArray<BenchmarkCase> benchmarks, IEnumerable<BenchmarkLogicalGroupRule>? order = null) => benchmarks;

	public string GetHighlightGroupKey(BenchmarkCase benchmark) => null;

	public string GetLogicalGroupKey(ImmutableArray<BenchmarkCase> allBenchmarks, BenchmarkCase benchmark) =>
		string.Join("-", benchmark.Job.Environment.Platform.ToString(), benchmark.Job.ToString(), ((byte[])benchmark.Parameters.Items[0].Value).Length.ToString("X8"));

	public IEnumerable<IGrouping<string, BenchmarkCase>> GetLogicalGroupOrder(IEnumerable<IGrouping<string, BenchmarkCase>> logicalGroups, IEnumerable<BenchmarkLogicalGroupRule>? order = null) =>
		logicalGroups.OrderBy(lg => lg.Key);

	public IEnumerable<BenchmarkCase> GetSummaryOrder(ImmutableArray<BenchmarkCase> benchmarks, Summary summary)
	{
		var benchmarkLogicalGroups = benchmarks.GroupBy(b => GetLogicalGroupKey(benchmarks, b));
		foreach (var logicalGroup in GetLogicalGroupOrder(benchmarkLogicalGroups))
			foreach (var benchmark in logicalGroup.OrderBy(b => b.Job.Meta.Baseline ? 0 : 0).ThenBy(b => b.Job.Id).ThenBy(b => b.Job.Environment.Jit.ToString()))
				yield return benchmark;
	}
}
