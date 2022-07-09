//// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");

using LibGit2Sharp;

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

    public App(string repoPath = DefaultRepositoryPath) {
        RepositoryPath = repoPath;
        TargetModifyFile = Path.Combine(RepositoryPath, @"GitAutoCommit\Demo.txt");
        CheckRepository();
    }

    static void Main(string[] args) {
        try {
            new App().Run();
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
        //ModifyFile();
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