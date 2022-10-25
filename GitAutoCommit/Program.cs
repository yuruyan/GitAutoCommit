using GitAutoCommit;
using GitAutoCommit.Resource;
using LibGit2Sharp;
using Microsoft.Extensions.Configuration;
using NLog;
using static GitAutoCommit.App;

// 提示信息
if (args.Length == 0) {
    Console.WriteLine(Resource.Help);
    return;
}

Logger Logger = LogManager.GetCurrentClassLogger();

var ParseCommandArgs = () => {
    var config = new ConfigurationBuilder().AddCommandLine(args).Build();
    var repoArg = config["repo"] ?? string.Empty;
    var modifyFileName = config["modifyFile"] ?? DefaultModifyFileName;
    var timesArg = config["times"] ?? DefaultCommitTimes.ToString();

    if (!uint.TryParse(timesArg, out var commitTimes)) {
        commitTimes = DefaultCommitTimes;
    }
    // 验证
    if (!Directory.Exists(repoArg)) {
        Logger.Error($"仓库 '{repoArg}' 不存在！");
        Environment.Exit(-1);
    }
    // 仓库是否合法
    if (!Repository.IsValid(repoArg)) {
        Logger.Error("给定路径不是合法仓库！");
        Environment.Exit(-1);
    }
    return new CommandArg(repoArg, commitTimes, modifyFileName);
};

var argInfo = ParseCommandArgs();
var app = new App(argInfo);
try {
    for (int i = 0; i < argInfo.CommitTimes; i++) {
        app.Run();
        Logger.Debug($"commit {i + 1} times");
    }
    Logger.Debug("success");
} catch (Exception e) {
    Logger.Debug(e.Message);
}