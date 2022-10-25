using LibGit2Sharp;
using NLog;
using System.Diagnostics;
using System.Text;

namespace GitAutoCommit;

public class App {
    public readonly record struct CommandArg(
        string RepoPath,
        uint CommitTimes = DefaultCommitTimes,
        // 相对于 RepoPath 路径
        string ModifyFileName = DefaultModifyFileName,
        bool Push = DefaultPush,
        string Remote = DefaultRemote
    );

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    public const bool DefaultPush = false;
    public const string DefaultRemote = "origin";
    public const int DefaultCommitTimes = 3;
    public const string DefaultModifyFileName = "ForModify";
    public readonly CommandArg ArgInfo;
    /// <summary>
    /// 修改文件的绝对路径
    /// </summary>
    private readonly string ModifyFilePath;
    private readonly Repository Repository;

    public App(CommandArg argInfo) {
        ArgInfo = argInfo;
        Repository = new(ArgInfo.RepoPath);
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
        Commit? commit = Repository.Commits.FirstOrDefault();
        if (commit is null) {
            Console.WriteLine("请先初始化仓库");
            return;
        }
        Commands.Stage(Repository, ModifyFilePath);
        Repository.Commit(
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

    /// <summary>
    /// 推送到远程仓库，不断尝试，直到成功为止
    /// </summary>
    public void Push() {
        if (!ArgInfo.Push) {
            return;
        }

        // push 方法
        var Push = (Remote remote) => {
            try {
                Process? process = Process.Start(new ProcessStartInfo {
                    FileName = "git",
                    WorkingDirectory = ArgInfo.RepoPath,
                    Arguments = $"push {ArgInfo.Remote} {Repository.Head.FriendlyName}",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                });
                if (process is null) {
                    throw new Exception("执行 git 命令失败");
                }
                process.WaitForExit();
                // 判断是否出现 error、fatal
                string output = process.StandardError.ReadToEnd() + process.StandardOutput.ReadToEnd();
                return !(output.Contains("fatal") || output.Contains("error"));
            } catch (Exception error) {
                Logger.Error(error);
                return false;
            }
        };

        Remote remote = Repository.Network.Remotes[ArgInfo.Remote];
        if (remote == null) {
            Logger.Error($"找不到为 {ArgInfo.Remote} 的 remote！");
            return;
        }
        // 不断重试
        for (int i = 1; !Push(remote); i++) {
            Logger.Error($"git push failed {i} times");
            return;
        }
    }
}
