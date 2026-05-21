using System.Net;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace KiotaOutSystems.Generator.Tests;

public class OpenApiToOutSystemsGeneratorTests
{
    [Fact]
    public async Task Generate_FromLocalSpec_CreatesActionsStructuresAndManifest()
    {
        var repoRoot = GetRepoRoot();
        var outputDirectory = CreateTempDirectory();
        var configPath = Path.Combine(repoRoot, "JsonPlaceholderKiota", "outsystems-generator.json");
        var options = CreateOptions(
            specSource: Path.Combine(repoRoot, "JsonPlaceholderKiota", "Specs", "posts-api.yml"),
            outputDirectory: outputDirectory,
            kiotaLockPath: Path.Combine(repoRoot, "JsonPlaceholderKiota", "Client", "kiota-lock.json"),
            configPath: configPath);

        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);
        var result = OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        Assert.Equal(3, result.Files.Count);
        var actionsFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedActions.g.cs"));
        var structuresFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedStructures.g.cs"));
        var manifestFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "generation-manifest.json"));

        Assert.Contains("ListPosts", actionsFile);
        Assert.Contains("GetPostById", actionsFile);
        Assert.Contains("CreatePost", actionsFile);
        Assert.Contains("includeUserId = false", actionsFile);
        Assert.Contains("includeTitle = false", actionsFile);
        Assert.Contains("public struct Post", structuresFile);
        Assert.Contains("public struct CreatePostRequest", structuresFile);
        Assert.Contains("public struct UpdatePostRequest", structuresFile);
        Assert.Contains("Creates a post in the downstream API.", actionsFile);
        var configDocument = JsonDocument.Parse(await File.ReadAllTextAsync(configPath));
        var expectedIconResourceName = configDocument.RootElement.GetProperty("icon").GetProperty("resourceName").GetString();
        Assert.NotNull(expectedIconResourceName);
        Assert.Contains($"IconResourceName = \"{expectedIconResourceName}\"", actionsFile);
        Assert.Contains("\"operations\"", manifestFile);
    }

    [Fact]
    public async Task Generate_WithIconConfig_EmitsIconResourceName()
    {
        var repoRoot = GetRepoRoot();
        var outputDirectory = CreateTempDirectory();
        var configPath = Path.Combine(CreateTempDirectory(), "generator-config.json");
        await File.WriteAllTextAsync(configPath, """
            {
              "icon": {
                "fileName": "ProjectIcon.png",
                "resourceName": "JsonPlaceholderKiota.resources.ProjectIcon.png"
              }
            }
            """);

        var options = CreateOptions(
            specSource: Path.Combine(repoRoot, "JsonPlaceholderKiota", "Specs", "posts-api.yml"),
            outputDirectory: outputDirectory,
            kiotaLockPath: Path.Combine(repoRoot, "JsonPlaceholderKiota", "Client", "kiota-lock.json"),
            configPath: configPath);

        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);
        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        var actionsFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedActions.g.cs"));
        var manifestFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "generation-manifest.json"));

        Assert.Contains("IconResourceName = \"JsonPlaceholderKiota.resources.ProjectIcon.png\"", actionsFile);
        Assert.Contains("\"iconResourceName\": \"JsonPlaceholderKiota.resources.ProjectIcon.png\"", manifestFile);
    }

    [Fact]
    public async Task Generate_WithoutIconConfig_OmitsIconResourceName()
    {
        var repoRoot = GetRepoRoot();
        var outputDirectory = CreateTempDirectory();
        var options = CreateOptions(
            specSource: Path.Combine(repoRoot, "JsonPlaceholderKiota", "Specs", "posts-api.yml"),
            outputDirectory: outputDirectory,
            kiotaLockPath: Path.Combine(repoRoot, "JsonPlaceholderKiota", "Client", "kiota-lock.json"));

        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);
        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        var actionsFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedActions.g.cs"));
        Assert.DoesNotContain("IconResourceName =", actionsFile);
    }

    [Fact]
    public async Task Generate_FromRemoteSpecUrl_CreatesFiles()
    {
        var repoRoot = GetRepoRoot();
        var outputDirectory = CreateTempDirectory();
        var spec = await File.ReadAllTextAsync(Path.Combine(repoRoot, "JsonPlaceholderKiota", "Specs", "posts-api.yml"));
        using var server = new TestHttpServer(spec);
        var options = CreateOptions(
            specSource: server.Url,
            outputDirectory: outputDirectory,
            kiotaLockPath: Path.Combine(repoRoot, "JsonPlaceholderKiota", "Client", "kiota-lock.json"),
            configPath: Path.Combine(repoRoot, "JsonPlaceholderKiota", "outsystems-generator.json"));

        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);
        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        Assert.True(File.Exists(Path.Combine(outputDirectory, "GeneratedActions.g.cs")));
        Assert.True(File.Exists(Path.Combine(outputDirectory, "GeneratedStructures.g.cs")));
    }

    [Fact]
    public async Task Generate_WithEmitInterfaceFalse_DoesNotEmitOsInterface()
    {
        var repoRoot = GetRepoRoot();
        var outputDirectory = CreateTempDirectory();
        var baseOptions = CreateOptions(
            specSource: Path.Combine(repoRoot, "JsonPlaceholderKiota", "Specs", "posts-api.yml"),
            outputDirectory: outputDirectory,
            kiotaLockPath: Path.Combine(repoRoot, "JsonPlaceholderKiota", "Client", "kiota-lock.json"),
            configPath: Path.Combine(repoRoot, "JsonPlaceholderKiota", "outsystems-generator.json"));
        var options = new GeneratorOptions
        {
            SpecSource = baseOptions.SpecSource,
            OutputDirectory = baseOptions.OutputDirectory,
            Namespace = baseOptions.Namespace,
            ClientNamespace = baseOptions.ClientNamespace,
            ClientClassName = baseOptions.ClientClassName,
            InterfaceName = baseOptions.InterfaceName,
            ClassName = baseOptions.ClassName,
            KiotaLockPath = baseOptions.KiotaLockPath,
            InputName = baseOptions.InputName,
            EmitInterface = false,
            ConfigPath = baseOptions.ConfigPath
        };

        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);
        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        var actionsFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedActions.g.cs"));
        var manifestFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "generation-manifest.json"));

        Assert.DoesNotContain("[OSInterface", actionsFile);
        Assert.DoesNotContain("public interface", actionsFile);
        Assert.Contains("\"emitInterface\": false", manifestFile);
    }

    [Fact]
    public async Task Generate_WithExistingOsInterfaceAndEmitInterfaceTrue_ThrowsValidationError()
    {
        var projectRoot = CreateTempDirectory();
        var outputDirectory = Path.Combine(projectRoot, "Generated");
        Directory.CreateDirectory(outputDirectory);
        var existingFacadePath = Path.Combine(projectRoot, "HandWrittenFacade.cs");
        var projectFilePath = Path.Combine(projectRoot, "TempProject.csproj");
        await File.WriteAllTextAsync(existingFacadePath, """
            using OutSystems.ExternalLibraries.SDK;
            [OSInterface(Description = "Existing facade", Name = "ExistingFacade")]
            public interface IExistingFacade {}
            """);
        await File.WriteAllTextAsync(projectFilePath, "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>");

        try
        {
            var options = new GeneratorOptions
            {
                SpecSource = Path.Combine(GetRepoRoot(), "JsonPlaceholderKiota", "Specs", "posts-api.yml"),
                OutputDirectory = outputDirectory,
                ClientNamespace = "JsonPlaceholderKiota.Client",
                ClientClassName = "PostsClient",
                ClassName = "JsonPlaceholderPostsGenerated",
                InterfaceName = "IJsonPlaceholderPostsGenerated",
                InputName = "JSONPlaceholder",
                EmitInterface = true
            };

            var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);

            var error = Assert.Throws<InvalidOperationException>(() => OpenApiToOutSystemsGenerator.Generate(options, loadedSpec));
            Assert.Contains("only one OSInterface", error.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            File.Delete(existingFacadePath);
            File.Delete(projectFilePath);
        }
    }

    [Fact]
    public async Task Generate_WithConfigNameOverrides_EmitsOriginalNameMetadata()
    {
        var repoRoot = GetRepoRoot();
        var outputDirectory = CreateTempDirectory();
        var configPath = Path.Combine(CreateTempDirectory(), "generator-config.json");
        await File.WriteAllTextAsync(configPath, """
            {
              "actionOverrides": {
                "GET /posts": { "name": "BrowsePosts" }
              },
              "structureOverrides": {
                "post": { "name": "BlogPost" }
              },
              "structureFieldOverrides": {
                "post": {
                  "title": { "name": "Headline" }
                }
              }
            }
            """);

        var options = CreateOptions(
            specSource: Path.Combine(repoRoot, "JsonPlaceholderKiota", "Specs", "posts-api.yml"),
            outputDirectory: outputDirectory,
            kiotaLockPath: Path.Combine(repoRoot, "JsonPlaceholderKiota", "Client", "kiota-lock.json"),
            configPath: configPath);

        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);
        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        var actionsFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedActions.g.cs"));
        var structuresFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedStructures.g.cs"));

        Assert.Contains("OriginalName = \"ListPosts\"", actionsFile);
        Assert.Contains("OriginalName = \"Post\"", structuresFile);
        Assert.Contains("OriginalName = \"Title\"", structuresFile);
    }

    [Fact]
    public void SetupProjectIcon_UpdatesHandwrittenProjectToEmbeddedResourcePattern()
    {
        var tempRoot = CreateTempDirectory();
        var projectName = "TempIconProject";
        var projectPath = Path.Combine(tempRoot, projectName);
        Directory.CreateDirectory(projectPath);
        Directory.CreateDirectory(Path.Combine(projectPath, "resources"));

        File.WriteAllText(Path.Combine(projectPath, $"{projectName}.csproj"), """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
              </PropertyGroup>
              <ItemGroup>
                <Content Include="resources\legacy.png">
                  <CopyToPublishDirectory>PreserveNewest</CopyToPublishDirectory>
                </Content>
              </ItemGroup>
            </Project>
            """);
        File.WriteAllText(Path.Combine(projectPath, "Facade.cs"), """
            using OutSystems.ExternalLibraries.SDK;
            [OSInterface(Description = "Example facade", Name = "ExampleFacade")]
            public interface IExampleFacade {}
            """);

        RunIconScript(tempRoot, projectName, "-ScaffoldPlaceholder");

        var csproj = File.ReadAllText(Path.Combine(projectPath, $"{projectName}.csproj"));
        var facade = File.ReadAllText(Path.Combine(projectPath, "Facade.cs"));

        Assert.Contains("EmbeddedResource Include=\"resources\\ProjectIcon.png\"", csproj);
        Assert.DoesNotContain("<Content Include=\"resources\\legacy.png\"", csproj);
        Assert.Contains("IconResourceName = \"TempIconProject.resources.ProjectIcon.png\"", facade);
        Assert.True(File.Exists(Path.Combine(projectPath, "resources", "ProjectIcon.png")));
    }

    [Fact]
    public void SetupProjectIcon_UpdatesGeneratorConfigInsteadOfGeneratedSource()
    {
        var tempRoot = CreateTempDirectory();
        var projectName = "GeneratedIconProject";
        var projectPath = Path.Combine(tempRoot, projectName);
        Directory.CreateDirectory(projectPath);
        Directory.CreateDirectory(Path.Combine(projectPath, "Generated"));

        File.WriteAllText(Path.Combine(projectPath, $"{projectName}.csproj"), """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """);
        File.WriteAllText(Path.Combine(projectPath, "outsystems-generator.json"), """
            {
              "namespace": "GeneratedIconProject.Generated"
            }
            """);
        File.WriteAllText(Path.Combine(projectPath, "Generated", "GeneratedActions.g.cs"), """
            [OSInterface(Description = "Generated", Name = "Generated")]
            public interface IGenerated {}
            """);

        var output = RunIconScript(tempRoot, projectName, "-ScaffoldPlaceholder");

        var csproj = File.ReadAllText(Path.Combine(projectPath, $"{projectName}.csproj"));
        var config = File.ReadAllText(Path.Combine(projectPath, "outsystems-generator.json"));
        var generated = File.ReadAllText(Path.Combine(projectPath, "Generated", "GeneratedActions.g.cs"));

        Assert.Contains("EmbeddedResource Include=\"resources\\ProjectIcon.png\"", csproj);
        Assert.Contains("\"icon\"", config);
        Assert.Contains("\"resourceName\"", config);
        Assert.Contains("GeneratedIconProject.resources.ProjectIcon.png", config);
        Assert.DoesNotContain("IconResourceName =", generated);
        Assert.True(File.Exists(Path.Combine(projectPath, "resources", "ProjectIcon.png")));
        Assert.Contains("Regenerate the OutSystems wrapper files before you publish", output);
    }

    private static GeneratorOptions CreateOptions(string specSource, string outputDirectory, string kiotaLockPath, string? configPath = null)
    {
        return new GeneratorOptions
        {
            SpecSource = specSource,
            OutputDirectory = outputDirectory,
            Namespace = "JsonPlaceholderKiota.Generated",
            KiotaLockPath = kiotaLockPath,
            ClassName = "JsonPlaceholderPostsGenerated",
            InterfaceName = "IJsonPlaceholderPostsGenerated",
            InputName = "JSONPlaceholder",
            ConfigPath = configPath
        };
    }

    private static string GetRepoRoot()
    {
        var currentDirectory = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(currentDirectory))
        {
            if (File.Exists(Path.Combine(currentDirectory, "ForgeAssets.sln")))
            {
                return currentDirectory;
            }

            currentDirectory = Directory.GetParent(currentDirectory)?.FullName ?? string.Empty;
        }

        throw new InvalidOperationException("Could not locate the repository root.");
    }

    private static string CreateTempDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static string RunIconScript(string baseDirectory, string projectName, string additionalArguments)
    {
        var scriptPath = Path.Combine(GetRepoRoot(), "setup-project-icon.ps1");
        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\" -ProjectName \"{projectName}\" -BaseDirectory \"{baseDirectory}\" {additionalArguments}",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo)!;
        process.WaitForExit();
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();

        Assert.True(process.ExitCode == 0, $"Icon setup script failed.{Environment.NewLine}STDOUT:{Environment.NewLine}{stdout}{Environment.NewLine}STDERR:{Environment.NewLine}{stderr}");
        return stdout;
    }

    private sealed class TestHttpServer : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly Task _backgroundTask;

        public TestHttpServer(string content)
        {
            var port = GetAvailablePort();
            Url = $"http://localhost:{port}/posts-api.yml";
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://localhost:{port}/");
            _listener.Start();
            _backgroundTask = Task.Run(async () =>
            {
                while (_listener.IsListening)
                {
                    try
                    {
                        var context = await _listener.GetContextAsync();
                        var buffer = Encoding.UTF8.GetBytes(content);
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        context.Response.ContentType = "application/yaml";
                        context.Response.ContentLength64 = buffer.Length;
                        await context.Response.OutputStream.WriteAsync(buffer);
                        context.Response.Close();
                    }
                    catch (HttpListenerException)
                    {
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }
                }
            });
        }

        public string Url { get; }

        public void Dispose()
        {
            _listener.Stop();
            _listener.Close();
            _backgroundTask.GetAwaiter().GetResult();
        }

        private static int GetAvailablePort()
        {
            var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}
