using System.Linq;

using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.DotNetCli;
using BenchmarkDotNet.Validators;

public class MultipleJitConfig : ManualConfig
{
	public MultipleJitConfig()
	{
		var cli21_32 = NetCoreAppSettings.NetCoreApp21.WithCustomDotNetCliPath(Paths.DotNetCLIx86, "Default");
		var cli21_64 = NetCoreAppSettings.NetCoreApp21.WithCustomDotNetCliPath(Paths.DotNetCLIx64, "Default");
		var cli20_32 = NetCoreAppSettings.NetCoreApp20.WithCustomDotNetCliPath(Paths.DotNetCLIx86, "Default");
		var cli20_64 = NetCoreAppSettings.NetCoreApp20.WithCustomDotNetCliPath(Paths.DotNetCLIx64, "Default");
		var cli11_32 = NetCoreAppSettings.NetCoreApp11.WithCustomDotNetCliPath(Paths.DotNetCLIx86, "Default");
		var cli11_64 = NetCoreAppSettings.NetCoreApp11.WithCustomDotNetCliPath(Paths.DotNetCLIx64, "Default");

		Add(Job.RyuJitX64.With(CsProjCoreToolchain.From(cli21_64)).WithId("netcoreapp2.1"));
		Add(Job.RyuJitX86.With(CsProjCoreToolchain.From(cli21_32)).WithId("netcoreapp2.1"));
		Add(Job.RyuJitX64.With(CsProjCoreToolchain.From(cli20_64)).WithId("netcoreapp2.0"));
		Add(Job.RyuJitX86.With(CsProjCoreToolchain.From(cli20_32)).WithId("netcoreapp2.0"));
		Add(Job.RyuJitX64.With(CsProjCoreToolchain.From(cli11_64)).AsBaseline().WithId("netcoreapp1.1"));
		Add(Job.RyuJitX86.With(CsProjCoreToolchain.From(cli11_32)).AsBaseline().WithId("netcoreapp1.1"));

#if NETFRAMEWORK // Legacy JIT can only be used from a CLR host
		Add(Job.LegacyJitX64.With(Runtime.Clr).WithId("net46"));
		Add(Job.LegacyJitX86.With(Runtime.Clr).WithId("net46"));
		//Add(Job.RyuJitX64.With(Runtime.Clr).WithId("net46"));
#else
		Add(Job.RyuJitX64.With(Runtime.Clr).WithId("net46"));
		Add(Job.RyuJitX86.With(Runtime.Clr).WithId("net46")); //.NET Framework doesn't yet include RyuJIT-32, so this runs LegacyJIT
#endif

		Add(DefaultConfig.Instance.GetLoggers().ToArray());
		Add(DefaultConfig.Instance.GetColumnProviders().ToArray());
		Add(MemoryDiagnoser.Default);
		Set(ByMethodByPlatformOrderProvider.Instance);
	}
}

public class AllowNonOptimizedConfig : ManualConfig
{
	public AllowNonOptimizedConfig()
	{
		Add(JitOptimizationsValidator.DontFailOnError);

		Add(DefaultConfig.Instance.GetLoggers().ToArray());
		//Add(DefaultConfig.Instance.GetExporters().ToArray());
		Add(DefaultConfig.Instance.GetColumnProviders().ToArray());
		Add(MemoryDiagnoser.Default);
		Add(new HashColumn());
	}
}
