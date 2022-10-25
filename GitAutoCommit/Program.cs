using LibGit2Sharp;
using Microsoft.Extensions.Configuration;
using NLog;
using System.Text;

namespace GitAutoCommit;

public class App {
    public readonly record struct CommandArg(
        string RepoPath,
        uint CommitTimes = DefaultCommitTimes,
        // 相对于 RepoPath 路径
        string ModifyFileName = DefaultModifyFileName
    );

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private const int DefaultCommitTimes = 3;
    private const string DefaultModifyFileName = "ForModify";
    public readonly CommandArg ArgInfo;
    /// <summary>
    /// 修改文件的绝对路径
    /// </summary>
    private readonly string ModifyFilePath;

    public App(CommandArg argInfo) {
        ArgInfo = argInfo;
        ModifyFilePath = Path.Combine(argInfo.RepoPath, argInfo.ModifyFileName);
    }

    /// <summary>
    /// 解析命令行参数
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    private static CommandArg ParseCommandArgs(string[] args) {
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

        return new(repoArg, commitTimes, modifyFileName);
    }

    /// <summary>
    /// --repo 指定仓库路径
    /// --times 指定提交次数
    /// </summary>
    /// <param name="args"></param>
    static void Main(string[] args) {
        // 提示信息
        if (args.Length == 0) {
            Console.WriteLine(Resource.Resource.Help);
            return;
        }
        CommandArg commandArg = ParseCommandArgs(args);
        var app = new App(commandArg);
        try {
            for (int i = 0; i < commandArg.CommitTimes; i++) {
                app.Run();
                Console.WriteLine($"commit {i + 1} times");
            }
            Console.WriteLine("success");
        } catch (Exception e) {
            Console.WriteLine(e.Message);
        }
    }

    public void Run() {
        ModifyFile();
        Commit();
    }

    /// <summary>
    /// 提交
    /// </summary>
    private void Commit() {
        var repository = new Repository(ArgInfo.RepoPath);
        Commit? commit = repository.Commits.FirstOrDefault();
        if (commit is null) {
            Console.WriteLine("请先初始化仓库");
            return;
        }
        Commands.Stage(repository, ModifyFilePath);
        repository.Commit(
            "modify",
            new(commit.Author.Name, commit.Author.Email, DateTimeOffset.Now),
            new(commit.Committer.Name, commit.Committer.Email, DateTimeOffset.Now)
        );
    }

    /// <summary>
    /// 修改文件
    /// </summary>
    private void ModifyFile() {
        using var stream = File.Open(ModifyFilePath, FileMode.Create, FileAccess.Write);
        stream.Write(Encoding.ASCII.GetBytes($"{Random.Shared.NextDouble()}{DateTimeOffset.UtcNow.Millisecond}"));
    }
}