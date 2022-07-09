//// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");

using LibGit2Sharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;

namespace GitAutoCommit;

class App {
    /// <summary>
    /// 默认仓库路径
    /// </summary>
    private const string DefaultRepositoryPath = @"E:\Projects\CSharp\GitAutoCommit";
    /// <summary>
    /// 工作仓库路径
    /// </summary>
    private string RepositoryPath = DefaultRepositoryPath;
    /// <summary>
    /// 修改文件
    /// </summary>
    private string TargetModifyFile;

    private record CommandArg {
        public uint CommitTimes { get; set; } = 1;
        public string RepoPath { get; set; } = string.Empty;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="repoPath">如果是 null 或者空串，则为 DefaultRepositoryPath</param>
    /// <see cref="DefaultRepositoryPath"/>
    public App(string repoPath = DefaultRepositoryPath) {
        if (!string.IsNullOrWhiteSpace(repoPath)) {
            RepositoryPath = repoPath;
        }
        TargetModifyFile = Path.Combine(RepositoryPath, @"GitAutoCommit\Demo.txt");
        CheckRepository();
    }

    /// <summary>
    /// 解析命令行参数
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    private static CommandArg ParseCommandArgs(string[] args) {
        var configRoot = new ConfigurationBuilder().AddCommandLine(args).Build();
        var repoPath = configRoot.GetSection("repo").Value;
        var times = configRoot.GetSection("times").Value;
        var result = new CommandArg() {
            RepoPath = repoPath ?? string.Empty,
        };
        if (!string.IsNullOrWhiteSpace(times)) {
            if (uint.TryParse(times, out var tempTimes)) {
                result.CommitTimes = tempTimes;
            }
        }
        return result;
    }

    /// <summary>
    /// --repo 指定仓库路径
    /// --times 指定提交次数
    /// </summary>
    /// <param name="args"></param>
    static void Main(string[] args) {
        CommandArg commandArg = ParseCommandArgs(args);
        var app = new App(commandArg.RepoPath);
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

    /// <summary>
    /// 检查仓库是否合法
    /// </summary>
    /// <exception cref="Exception"></exception>
    private void CheckRepository() {
        if (!Repository.IsValid(RepositoryPath)) {
            throw new Exception("给定路径不是合法仓库");
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
        var repository = new Repository(RepositoryPath);
        Commit? commit = repository.Commits.FirstOrDefault();
        if (commit is null) {
            Console.WriteLine("请先初始化仓库");
            return;
        }
        Commands.Stage(repository, TargetModifyFile);
        repository.Commit("modify", commit.Author, commit.Committer);
    }

    /// <summary>
    /// 修改文件
    /// </summary>
    private void ModifyFile() {
        using var stream = File.Open(TargetModifyFile, FileMode.Append, FileAccess.Write);
        stream.Write(new byte[] { 32 });
    }
}