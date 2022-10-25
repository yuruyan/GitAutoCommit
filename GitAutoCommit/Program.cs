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
    var modifyFileNameArg = config["modifyFile"] ?? DefaultModifyFileName;
    var timesArg = config["times"] ?? DefaultCommitTimes.ToString();
    var remoteArg = config["remote"] ?? DefaultRemote;

    // 解析 times
    if (!uint.TryParse(timesArg, out var commitTimes)) {
        commitTimes = DefaultCommitTimes;
    }
    // 解析 push
    if (!bool.TryParse(config["push"], out var push)) {
        push = DefaultPush;
    }

    // 验证 repo
    if (!Directory.Exists(repoArg)) {
        Logger.Error($"仓库 '{repoArg}' 不存在！");
        Environment.Exit(-1);
    }
    // 仓库是否合法
    if (!Repository.IsValid(repoArg)) {
        Logger.Error("给定路径不是合法仓库！");
        Environment.Exit(-1);
    }
    // 验证 remote
    if (string.IsNullOrWhiteSpace(remoteArg)) {
        remoteArg = DefaultRemote;
    }

    return new CommandArg(
        repoArg,
        commitTimes,
        modifyFileNameArg,
        push,
        remoteArg
    );
};

var argInfo = ParseCommandArgs();
var app = new App(argInfo);
for (int i = 0; i < argInfo.CommitTimes; i++) {
    app.Run();
    Logger.Debug($"commit {i + 1} times");
}
Logger.Debug("start pushing");
app.Push();
Logger.Debug("success");
