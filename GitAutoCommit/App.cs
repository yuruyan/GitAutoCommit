using LibGit2Sharp;
using System.Text;

namespace GitAutoCommit;

public class App {
    public readonly record struct CommandArg(
        string RepoPath,
        uint CommitTimes = DefaultCommitTimes,
        // 相对于 RepoPath 路径
        string ModifyFileName = DefaultModifyFileName
    );

    public const int DefaultCommitTimes = 3;
    public const string DefaultModifyFileName = "ForModify";
    public readonly CommandArg ArgInfo;
    /// <summary>
    /// 修改文件的绝对路径
    /// </summary>
    private readonly string ModifyFilePath;

    public App(CommandArg argInfo) {
        ArgInfo = argInfo;
        ModifyFilePath = Path.Combine(argInfo.RepoPath, argInfo.ModifyFileName);
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
