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
    public async Task Generate_WithPetstorePathTargets_OnlyEmitsPetOperationsAndRequiredStructures()
    {
        var repoRoot = GetRepoRoot();
        var outputDirectory = CreateTempDirectory();
        var configPath = Path.Combine(CreateTempDirectory(), "generator-config.json");
        await File.WriteAllTextAsync(configPath, """
            {
              "targets": [
                {
                  "path": "/pet*"
                }
              ]
            }
            """);

        var options = new GeneratorOptions
        {
            SpecSource = Path.Combine(repoRoot, "PetstoreKiota", "Specs", "petstore-31.json"),
            OutputDirectory = outputDirectory,
            Namespace = "PetstoreKiota.Generated",
            KiotaLockPath = Path.Combine(repoRoot, "PetstoreKiota", "Client", "kiota-lock.json"),
            ClassName = "PetstoreActionsGenerated",
            InterfaceName = "IPetstoreActionsGenerated",
            InputName = "Swagger Petstore",
            ConfigPath = configPath
        };

        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);
        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        var actionsFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedActions.g.cs"));
        var structuresFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedStructures.g.cs"));
        var manifestFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "generation-manifest.json"));

        Assert.Contains("AddPet(Pet requestBody)", actionsFile);
        Assert.Contains("UpdatePet(Pet requestBody)", actionsFile);
        Assert.Contains("FindPetsByStatus", actionsFile);
        Assert.Contains("FindPetsByTags", actionsFile);
        Assert.Contains("GetPetById", actionsFile);
        Assert.Contains("UpdatePetWithForm", actionsFile);
        Assert.Contains("DeletePet", actionsFile);
        Assert.Contains("UploadFile", actionsFile);
        Assert.DoesNotContain("PlaceOrder", actionsFile);
        Assert.DoesNotContain("LoginUser", actionsFile);

        Assert.Contains("public struct Pet", structuresFile);
        Assert.Contains("public struct Category", structuresFile);
        Assert.Contains("public struct Tag", structuresFile);
        Assert.Contains("public struct ApiResponse", structuresFile);
        Assert.Contains("public struct UpdatePetWithFormRequest", structuresFile);
        Assert.DoesNotContain("public struct Order", structuresFile);
        Assert.DoesNotContain("public struct User", structuresFile);

        Assert.Contains("\"AddPet\"", manifestFile);
        Assert.Contains("\"UpdatePetWithForm\"", manifestFile);
        Assert.Contains("\"UploadFile\"", manifestFile);
        Assert.Contains("\"DeletePet\"", manifestFile);
        Assert.DoesNotContain("\"PlaceOrder\"", manifestFile);
        Assert.DoesNotContain("\"LoginUser\"", manifestFile);
    }

    [Fact]
    public async Task Generate_WithPetstorePathAndMethodTargets_OnlyEmitsMatchingOperations()
    {
        var repoRoot = GetRepoRoot();
        var outputDirectory = CreateTempDirectory();
        var configPath = Path.Combine(CreateTempDirectory(), "generator-config.json");
        await File.WriteAllTextAsync(configPath, """
            {
              "targets": [
                {
                  "path": "/pet*",
                  "methods": ["GET"]
                }
              ]
            }
            """);

        var options = new GeneratorOptions
        {
            SpecSource = Path.Combine(repoRoot, "PetstoreKiota", "Specs", "petstore-31.json"),
            OutputDirectory = outputDirectory,
            Namespace = "PetstoreKiota.Generated",
            KiotaLockPath = Path.Combine(repoRoot, "PetstoreKiota", "Client", "kiota-lock.json"),
            ClassName = "PetstoreActionsGenerated",
            InterfaceName = "IPetstoreActionsGenerated",
            InputName = "Swagger Petstore",
            ConfigPath = configPath
        };

        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);
        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        var actionsFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedActions.g.cs"));
        var structuresFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedStructures.g.cs"));
        var manifestFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "generation-manifest.json"));

        Assert.Contains("FindPetsByStatus", actionsFile);
        Assert.Contains("FindPetsByTags", actionsFile);
        Assert.Contains("GetPetById", actionsFile);
        Assert.DoesNotContain("AddPet", actionsFile);
        Assert.DoesNotContain("UpdatePet", actionsFile);
        Assert.DoesNotContain("DeletePet", actionsFile);
        Assert.DoesNotContain("UpdatePetWithForm", actionsFile);
        Assert.DoesNotContain("UploadFile", actionsFile);
        Assert.DoesNotContain("PlaceOrder", actionsFile);

        Assert.Contains("public struct Pet", structuresFile);
        Assert.DoesNotContain("public struct Category", structuresFile);
        Assert.DoesNotContain("public struct Tag", structuresFile);
        Assert.DoesNotContain("public struct ApiResponse", structuresFile);
        Assert.DoesNotContain("public struct UpdatePetWithFormRequest", structuresFile);
        Assert.DoesNotContain("public struct Order", structuresFile);
        Assert.DoesNotContain("public struct User", structuresFile);

        Assert.Contains("\"FindPetsByStatus\"", manifestFile);
        Assert.Contains("\"GetPetById\"", manifestFile);
        Assert.DoesNotContain("\"AddPet\"", manifestFile);
        Assert.DoesNotContain("\"UpdatePetWithForm\"", manifestFile);
        Assert.DoesNotContain("\"PlaceOrder\"", manifestFile);
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
        Assert.Contains("OriginalName = \"post\"", structuresFile);
        Assert.Contains("OriginalName = \"title\"", structuresFile);
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

    [Fact]
    public async Task Generate_WithNullableAnyOfProperty_EmitsNullableNestedStructure()
    {
        var specPath = CreateTempSpecFile("""
            {
              "openapi": "3.0.1",
              "info": { "title": "Nullable Probe", "version": "1.0.0" },
              "paths": {},
              "components": {
                "schemas": {
                  "account_business_profile": {
                    "type": "object",
                    "properties": {
                      "name": { "type": "string" }
                    }
                  },
                  "account": {
                    "type": "object",
                    "properties": {
                      "business_profile": {
                        "anyOf": [
                          { "$ref": "#/components/schemas/account_business_profile" },
                          { "type": "null" }
                        ]
                      }
                    }
                  }
                }
              }
            }
            """);

        var outputDirectory = CreateTempDirectory();
        var options = CreateMinimalOptions(specPath, outputDirectory);
        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);

        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        var structuresFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedStructures.g.cs"));
        Assert.Contains("public struct AccountBusinessProfile", structuresFile);
        Assert.Contains("public AccountBusinessProfile? BusinessProfile", structuresFile);
    }

    [Fact]
    public async Task Generate_WithDottedNames_UsesOriginalNameMetadata()
    {
        var specPath = CreateTempSpecFile("""
            {
              "openapi": "3.0.1",
              "info": { "title": "Dotted Name Probe", "version": "1.0.0" },
              "paths": {},
              "components": {
                "schemas": {
                  "apps.secret": {
                    "type": "object",
                    "properties": {
                      "filter.type": { "type": "string" }
                    }
                  }
                }
              }
            }
            """);

        var outputDirectory = CreateTempDirectory();
        var options = CreateMinimalOptions(specPath, outputDirectory);
        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);

        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        var structuresFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedStructures.g.cs"));
        Assert.Contains("[OSStructure(Description = \"Represents the apps.secret schema from the source OpenAPI document.\", OriginalName = \"apps.secret\")]", structuresFile);
        Assert.Contains("[OSStructureField(Description = \"Maps the 'filter.type' field.\", OriginalName = \"filter.type\")]", structuresFile);
        Assert.Contains("public string? FilterType", structuresFile);
    }

    [Fact]
    public async Task Generate_WithReservedFieldName_AvoidsMemberNameCollision()
    {
        var specPath = CreateTempSpecFile("""
            {
              "openapi": "3.0.1",
              "info": { "title": "Reserved Name Probe", "version": "1.0.0" },
              "paths": {},
              "components": {
                "schemas": {
                  "error": {
                    "type": "object",
                    "properties": {
                      "error": { "type": "string" },
                      "request_id": { "type": "string" }
                    }
                  }
                }
              }
            }
            """);

        var outputDirectory = CreateTempDirectory();
        var options = CreateMinimalOptions(specPath, outputDirectory);
        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);

        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        var structuresFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedStructures.g.cs"));
        Assert.Contains("public struct Error", structuresFile);
        Assert.Contains("OriginalName = \"error\")]", structuresFile);
        Assert.Contains("public string? ErrorValue", structuresFile);
    }

    [Fact]
    public async Task Generate_WithPolymorphicUnionComponent_EmitsProxyStructure()
    {
        var specPath = CreateTempSpecFile("""
            {
              "openapi": "3.0.1",
              "info": { "title": "Polymorphic Probe", "version": "1.0.0" },
              "paths": {},
              "components": {
                "schemas": {
                  "deleted_bank_account": {
                    "type": "object",
                    "properties": {
                      "id": { "type": "string" }
                    }
                  },
                  "deleted_card": {
                    "type": "object",
                    "properties": {
                      "id": { "type": "string" }
                    }
                  },
                  "deleted_external_account": {
                    "anyOf": [
                      { "$ref": "#/components/schemas/deleted_bank_account" },
                      { "$ref": "#/components/schemas/deleted_card" }
                    ]
                  }
                }
              }
            }
            """);

        var outputDirectory = CreateTempDirectory();
        var options = CreateMinimalOptions(specPath, outputDirectory);
        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);

        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        var structuresFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedStructures.g.cs"));
        Assert.Contains("public struct DeletedExternalAccount", structuresFile);
        Assert.Contains("public string? ObjectType", structuresFile);
        Assert.Contains("public DeletedBankAccount? DeletedBankAccount", structuresFile);
        Assert.Contains("public DeletedCard? DeletedCard", structuresFile);
    }

    [Fact]
    public async Task Generate_WithFormUrlEncodedRequestBody_EmitsOperation()
    {
        var specPath = CreateTempSpecFile("""
            {
              "openapi": "3.0.1",
              "info": { "title": "Form Probe", "version": "1.0.0" },
              "paths": {
                "/customers": {
                  "post": {
                    "operationId": "createCustomer",
                    "requestBody": {
                      "required": true,
                      "content": {
                        "application/x-www-form-urlencoded": {
                          "schema": {
                            "$ref": "#/components/schemas/create_customer_request"
                          }
                        }
                      }
                    },
                    "responses": {
                      "200": {
                        "description": "ok",
                        "content": {
                          "application/json": {
                            "schema": {
                              "$ref": "#/components/schemas/customer"
                            }
                          }
                        }
                      }
                    }
                  }
                }
              },
              "components": {
                "schemas": {
                  "create_customer_request": {
                    "type": "object",
                    "properties": {
                      "email": { "type": "string" }
                    }
                  },
                  "customer": {
                    "type": "object",
                    "properties": {
                      "id": { "type": "string" }
                    }
                  }
                }
              }
            }
            """);

        var outputDirectory = CreateTempDirectory();
        var options = CreateMinimalOptions(specPath, outputDirectory);
        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);

        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        var actionsFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedActions.g.cs"));
        Assert.Contains("CreateCustomer", actionsFile);
    }

    [Fact]
    public async Task Generate_WithAnyOfQueryParameter_PrefersPrimitiveBinding()
    {
        var specPath = CreateTempSpecFile("""
            {
              "openapi": "3.0.1",
              "info": { "title": "Query AnyOf Probe", "version": "1.0.0" },
              "paths": {
                "/charges": {
                  "get": {
                    "operationId": "listCharges",
                    "parameters": [
                      {
                        "name": "created",
                        "in": "query",
                        "schema": {
                          "anyOf": [
                            { "type": "integer" },
                            {
                              "type": "object",
                              "properties": {
                                "gte": { "type": "integer" }
                              }
                            }
                          ]
                        }
                      }
                    ],
                    "responses": {
                      "200": {
                        "description": "ok",
                        "content": {
                          "application/json": {
                            "schema": {
                              "$ref": "#/components/schemas/charge_list"
                            }
                          }
                        }
                      }
                    }
                  }
                }
              },
              "components": {
                "schemas": {
                  "charge_list": {
                    "type": "object",
                    "properties": {
                      "id": { "type": "string" }
                    }
                  }
                }
              }
            }
            """);

        var outputDirectory = CreateTempDirectory();
        var options = CreateMinimalOptions(specPath, outputDirectory);
        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);

        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        var actionsFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedActions.g.cs"));
        Assert.Contains("ListCharges", actionsFile);
        Assert.Contains("int created = 0", actionsFile);
        Assert.Contains("bool includeCreated = false", actionsFile);
    }

    [Fact]
    public async Task Generate_WithObjectQueryParameter_FallsBackToStringBinding()
    {
        var specPath = CreateTempSpecFile("""
            {
              "openapi": "3.0.1",
              "info": { "title": "Query Object Probe", "version": "1.0.0" },
              "paths": {
                "/people": {
                  "get": {
                    "operationId": "listPeople",
                    "parameters": [
                      {
                        "name": "relationship",
                        "in": "query",
                        "schema": {
                          "type": "object",
                          "properties": {
                            "owner": { "type": "boolean" }
                          }
                        }
                      }
                    ],
                    "responses": {
                      "200": {
                        "description": "ok",
                        "content": {
                          "application/json": {
                            "schema": {
                              "$ref": "#/components/schemas/person_list"
                            }
                          }
                        }
                      }
                    }
                  }
                }
              },
              "components": {
                "schemas": {
                  "person_list": {
                    "type": "object",
                    "properties": {
                      "id": { "type": "string" }
                    }
                  }
                }
              }
            }
            """);

        var outputDirectory = CreateTempDirectory();
        var options = CreateMinimalOptions(specPath, outputDirectory);
        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);

        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        var actionsFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedActions.g.cs"));
        Assert.Contains("ListPeople", actionsFile);
        Assert.Contains("string relationship = \"\"", actionsFile);
        Assert.Contains("bool includeRelationship = false", actionsFile);
    }

    [Fact]
    public async Task Generate_WithArrayOfObjectQueryParameter_FallsBackToStringListBinding()
    {
        var specPath = CreateTempSpecFile("""
            {
              "openapi": "3.0.1",
              "info": { "title": "Query Object Array Probe", "version": "1.0.0" },
              "paths": {
                "/credit_notes/preview": {
                  "get": {
                    "operationId": "previewCreditNote",
                    "parameters": [
                      {
                        "name": "invoice",
                        "in": "query",
                        "required": true,
                        "schema": { "type": "string" }
                      },
                      {
                        "name": "lines",
                        "in": "query",
                        "schema": {
                          "type": "array",
                          "items": {
                            "title": "credit_note_line_item_params",
                            "type": "object",
                            "properties": {
                              "amount": { "type": "integer" }
                            }
                          }
                        }
                      }
                    ],
                    "responses": {
                      "200": {
                        "description": "ok",
                        "content": {
                          "application/json": {
                            "schema": {
                              "$ref": "#/components/schemas/credit_note"
                            }
                          }
                        }
                      }
                    }
                  }
                }
              },
              "components": {
                "schemas": {
                  "credit_note": {
                    "type": "object",
                    "properties": {
                      "id": { "type": "string" }
                    }
                  }
                }
              }
            }
            """);

        var outputDirectory = CreateTempDirectory();
        var options = CreateMinimalOptions(specPath, outputDirectory);
        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);

        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        var actionsFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedActions.g.cs"));
        Assert.Contains("PreviewCreditNote", actionsFile);
        Assert.Contains("List<string>? lines = null", actionsFile);
        Assert.Contains("bool includeLines = false", actionsFile);
    }

    [Fact]
    public async Task Generate_WithProxyComponentResponse_EmitsMapperForComposedKiotaModel()
    {
        var specPath = CreateTempSpecFile("""
            {
              "openapi": "3.0.1",
              "info": { "title": "Proxy Component Response Probe", "version": "1.0.0" },
              "paths": {
                "/accounts/{account}/bank_accounts": {
                  "post": {
                    "operationId": "PostAccountsAccountBankAccounts",
                    "parameters": [
                      { "name": "account", "in": "path", "required": true, "schema": { "type": "string" } }
                    ],
                    "requestBody": {
                      "content": {
                        "application/x-www-form-urlencoded": {
                          "schema": {
                            "type": "object",
                            "properties": {
                              "external_account": { "type": "string" }
                            }
                          }
                        }
                      }
                    },
                    "responses": {
                      "200": {
                        "description": "ok",
                        "content": {
                          "application/json": {
                            "schema": { "$ref": "#/components/schemas/external_account" }
                          }
                        }
                      }
                    }
                  }
                }
              },
              "components": {
                "schemas": {
                  "bank_account": {
                    "type": "object",
                    "properties": {
                      "id": { "type": "string" }
                    }
                  },
                  "card": {
                    "type": "object",
                    "properties": {
                      "id": { "type": "string" }
                    }
                  },
                  "external_account": {
                    "anyOf": [
                      { "$ref": "#/components/schemas/bank_account" },
                      { "$ref": "#/components/schemas/card" }
                    ]
                  }
                }
              }
            }
            """);

        var outputDirectory = CreateTempDirectory();
        var options = CreateMinimalOptions(specPath, outputDirectory);
        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);

        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        var actionsFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedActions.g.cs"));
        Assert.Contains("PostAccountsAccountBankAccounts(", actionsFile);
        Assert.Contains("GeneratedModelMapper.Convert<", actionsFile);
    }

    [Fact]
    public async Task Generate_WithPrimitiveAndObjectUnionProperty_EmitsInlinePrimitiveProxyBranch()
    {
        var specPath = CreateTempSpecFile("""
            {
              "openapi": "3.0.1",
              "info": { "title": "Primitive Object Proxy Probe", "version": "1.0.0" },
              "paths": {},
              "components": {
                "schemas": {
                  "file": {
                    "type": "object",
                    "properties": {
                      "id": { "type": "string" }
                    }
                  },
                  "document": {
                    "type": "object",
                    "properties": {
                      "front": {
                        "anyOf": [
                          { "type": "string" },
                          { "$ref": "#/components/schemas/file" }
                        ]
                      }
                    }
                  }
                }
              }
            }
            """);

        var outputDirectory = CreateTempDirectory();
        var options = CreateMinimalOptions(specPath, outputDirectory);
        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);

        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        var structuresFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedStructures.g.cs"));
        Assert.Contains("public struct DocumentFront", structuresFile);
        Assert.Contains("public string? Option1", structuresFile);
        Assert.Contains("public File? File", structuresFile);
    }

    [Fact]
    public async Task Generate_WithSyntheticProxyItemResponse_UsesNestedKiotaResponseType()
    {
        var specPath = CreateTempSpecFile("""
            {
              "openapi": "3.0.1",
              "info": { "title": "Synthetic Proxy Item Response Probe", "version": "1.0.0" },
              "paths": {
                "/customers/{customer}": {
                  "get": {
                    "operationId": "GetCustomersCustomer",
                    "parameters": [
                      { "name": "customer", "in": "path", "required": true, "schema": { "type": "string" } }
                    ],
                    "requestBody": {
                      "content": {
                        "application/x-www-form-urlencoded": {
                          "schema": { "type": "object", "properties": {} }
                        }
                      }
                    },
                    "responses": {
                      "200": {
                        "description": "ok",
                        "content": {
                          "application/json": {
                            "schema": {
                              "anyOf": [
                                { "$ref": "#/components/schemas/customer" },
                                { "$ref": "#/components/schemas/deleted_customer" }
                              ]
                            }
                          }
                        }
                      }
                    }
                  }
                }
              },
              "components": {
                "schemas": {
                  "customer": {
                    "type": "object",
                    "properties": {
                      "id": { "type": "string" }
                    }
                  },
                  "deleted_customer": {
                    "type": "object",
                    "properties": {
                      "deleted": { "type": "boolean" }
                    }
                  }
                }
              }
            }
            """);

        var outputDirectory = CreateTempDirectory();
        var options = CreateMinimalOptions(specPath, outputDirectory);
        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);

        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        var actionsFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedActions.g.cs"));
        Assert.Contains("GetCustomersCustomer(", actionsFile);
        Assert.Contains("return GeneratedModelMapper.Convert<GetCustomersCustomer200Response>(response)!;", actionsFile);
    }

    [Fact]
    public async Task Generate_WithNormalizedNameCollision_AssignsDeterministicSuffix()
    {
        var specPath = CreateTempSpecFile("""
            {
              "openapi": "3.0.1",
              "info": { "title": "Collision Probe", "version": "1.0.0" },
              "paths": {},
              "components": {
                "schemas": {
                  "foo.bar": {
                    "type": "object",
                    "properties": {
                      "id": { "type": "string" }
                    }
                  },
                  "foo_bar": {
                    "type": "object",
                    "properties": {
                      "id": { "type": "string" }
                    }
                  }
                }
              }
            }
            """);

        var outputDirectory = CreateTempDirectory();
        var options = CreateMinimalOptions(specPath, outputDirectory);
        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);

        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        var structuresFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedStructures.g.cs"));
        Assert.Contains("public struct FooBar", structuresFile);
        Assert.Contains("public struct FooBar2", structuresFile);
        Assert.Contains("OriginalName = \"foo.bar\"", structuresFile);
        Assert.Contains("OriginalName = \"foo_bar\"", structuresFile);
    }

    [Fact]
    public async Task Generate_WithUnsupportedPropertyOnReferencedSchema_KeepsStructureAndSkipsOnlyBadField()
    {
        var specPath = CreateTempSpecFile("""
            {
              "openapi": "3.0.1",
              "info": { "title": "Partial Schema", "version": "1.0.0" },
              "paths": {},
              "components": {
                "schemas": {
                  "problem": {
                    "type": "object",
                    "properties": {
                      "ok": { "type": "string" },
                      "bad": { "type": "null" }
                    }
                  },
                  "wrapper": {
                    "type": "object",
                    "properties": {
                      "problem": { "$ref": "#/components/schemas/problem" }
                    }
                  }
                }
              }
            }
            """);
        var outputDirectory = CreateTempDirectory();
        var options = CreateMinimalOptions(specPath, outputDirectory);
        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);

        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        var structuresFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedStructures.g.cs"));
        Assert.Contains("public struct Problem", structuresFile);
        Assert.Contains("public string? Ok", structuresFile);
        Assert.Contains("public string? Bad", structuresFile);
        Assert.Contains("public Problem? Problem", structuresFile);
    }

    [Fact]
    public async Task Generate_WithSnakeCasePathSegments_PreservesKiotaMemberNames()
    {
        var specPath = CreateTempSpecFile("""
            {
              "openapi": "3.0.1",
              "info": { "title": "Snake Case Paths", "version": "1.0.0" },
              "paths": {
                "/app/installations/{installation_id}/access_tokens": {
                  "post": {
                    "operationId": "apps/create-installation-access-token",
                    "parameters": [
                      {
                        "name": "installation_id",
                        "in": "path",
                        "required": true,
                        "schema": { "type": "integer" }
                      }
                    ],
                    "requestBody": {
                      "required": true,
                      "content": {
                        "application/json": {
                          "schema": {
                            "type": "object",
                            "properties": {
                              "repositories": {
                                "type": "array",
                                "items": { "type": "string" }
                              }
                            }
                          }
                        }
                      }
                    },
                    "responses": {
                      "200": {
                        "description": "ok",
                        "content": {
                          "application/json": {
                            "schema": {
                              "type": "object",
                              "properties": {
                                "token": { "type": "string" }
                              }
                            }
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
            """);
        var outputDirectory = CreateTempDirectory();
        var options = CreateMinimalOptions(specPath, outputDirectory);
        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);

        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        var actionsFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedActions.g.cs"));
        Assert.Contains("_client.App.Installations[installationId.ToString()].Access_tokens", actionsFile);
        Assert.Contains("Probe.Client.App.Installations.Item.Access_tokens.Access_tokensPostRequestBody", actionsFile);
    }

    [Fact]
    public async Task Generate_WithVoidGetResponse_EmitsNoResponseVariable()
    {
        var specPath = CreateTempSpecFile("""
            {
              "openapi": "3.0.1",
              "info": { "title": "Void Get", "version": "1.0.0" },
              "paths": {
                "/gists/{gist_id}/star": {
                  "get": {
                    "operationId": "gists/check-is-starred",
                    "parameters": [
                      {
                        "name": "gist_id",
                        "in": "path",
                        "required": true,
                        "schema": { "type": "string" }
                      }
                    ],
                    "responses": {
                      "204": { "description": "starred" }
                    }
                  }
                }
              }
            }
            """);
        var outputDirectory = CreateTempDirectory();
        var options = CreateMinimalOptions(specPath, outputDirectory);
        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);

        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        var actionsFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedActions.g.cs"));
        Assert.Contains("requestBuilder.GetAsync().GetAwaiter().GetResult();", actionsFile);
        Assert.DoesNotContain("var response = requestBuilder.GetAsync()", actionsFile);
    }

    [Fact]
    public async Task Generate_WithDateQueryParameter_UsesDateTimePublicSignature()
    {
        var specPath = CreateTempSpecFile("""
            {
              "openapi": "3.0.1",
              "info": { "title": "Date Query", "version": "1.0.0" },
              "paths": {
                "/reports/enterprise_1_day": {
                  "get": {
                    "operationId": "reports/get-enterprise-1-day",
                    "parameters": [
                      {
                        "name": "day",
                        "in": "query",
                        "required": true,
                        "schema": { "type": "string", "format": "date" }
                      }
                    ],
                    "responses": {
                      "200": {
                        "description": "ok",
                        "content": {
                          "application/json": {
                            "schema": {
                              "type": "object",
                              "properties": {
                                "count": { "type": "integer" }
                              }
                            }
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
            """);
        var outputDirectory = CreateTempDirectory();
        var options = CreateMinimalOptions(specPath, outputDirectory);
        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);

        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        var actionsFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedActions.g.cs"));
        Assert.Contains("GetEnterprise1Day(DateTime day)", actionsFile);
        Assert.Contains("config.QueryParameters.Day = day;", actionsFile);
    }

    [Fact]
    public async Task Generate_WithKiotaDateQueryProperty_ConvertsDateTimeToKiotaDate()
    {
        var specPath = CreateTempSpecFile("""
            {
              "openapi": "3.0.1",
              "info": { "title": "Kiota Date Query", "version": "1.0.0" },
              "paths": {
                "/reports/enterprise_1_day": {
                  "get": {
                    "operationId": "reports/get-enterprise-1-day",
                    "parameters": [
                      {
                        "name": "day",
                        "in": "query",
                        "required": true,
                        "schema": { "type": "string", "format": "date" }
                      }
                    ],
                    "responses": {
                      "200": {
                        "description": "ok",
                        "content": {
                          "application/json": {
                            "schema": {
                              "type": "object",
                              "properties": {
                                "count": { "type": "integer" }
                              }
                            }
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
            """);
        var outputDirectory = CreateTempDirectory();
        var kiotaLockPath = CreateTempKiotaClient(
            ("Reports\\Enterprise_1_day\\Enterprise_1_dayRequestBuilder.cs", """
                namespace Probe.Client.Reports.Enterprise_1_day;

                public partial class Enterprise_1_dayRequestBuilder
                {
                    public async Task<Probe.Client.Models.Report?> GetAsync(Action<RequestConfiguration<Enterprise_1_dayRequestBuilderGetQueryParameters>>? requestConfiguration = default, CancellationToken cancellationToken = default)
                    {
                        throw new NotImplementedException();
                    }

                    public partial class Enterprise_1_dayRequestBuilderGetQueryParameters
                    {
                        public Date? Day { get; set; }
                    }
                }
                """));

        var options = CreateMinimalOptionsWithKiotaLock(specPath, outputDirectory, kiotaLockPath);
        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);

        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        var actionsFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedActions.g.cs"));
        Assert.Contains("GetEnterprise1Day(DateTime day)", actionsFile);
        Assert.Contains("config.QueryParameters.Day = GeneratedModelMapper.ToKiotaDate(day);", actionsFile);
    }

    [Fact]
    public async Task Generate_WithEmptyObjectSchema_UsesStringFallbackInsteadOfEmptyStruct()
    {
        var specPath = CreateTempSpecFile("""
            {
              "openapi": "3.0.1",
              "info": { "title": "Empty Object Schema", "version": "1.0.0" },
              "paths": {},
              "components": {
                "schemas": {
                  "metadata": {
                    "type": "object"
                  },
                  "wrapper": {
                    "type": "object",
                    "properties": {
                      "metadata": { "$ref": "#/components/schemas/metadata" }
                    }
                  }
                }
              }
            }
            """);
        var outputDirectory = CreateTempDirectory();
        var options = CreateMinimalOptions(specPath, outputDirectory);
        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);

        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        var structuresFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedStructures.g.cs"));
        Assert.DoesNotContain("public struct Metadata", structuresFile);
        Assert.Contains("public string? Metadata", structuresFile);
    }

    [Fact]
    public async Task Generate_WithLongAndNumericNames_UsesOutSystemsSafeNames()
    {
        var specPath = CreateTempSpecFile("""
            {
              "openapi": "3.0.1",
              "info": { "title": "Long Names", "version": "1.0.0" },
              "paths": {},
              "components": {
                "schemas": {
                  "WebhookSecurityAdvisoryWithdrawnSecurityAdvisoryVulnerabilitiesVulnerabilitiesItem": {
                    "type": "object",
                    "properties": {
                      "1": { "type": "integer" },
                      "__1": { "type": "integer" }
                    }
                  }
                }
              }
            }
            """);
        var outputDirectory = CreateTempDirectory();
        var options = CreateMinimalOptions(specPath, outputDirectory);
        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);

        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        var structuresFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedStructures.g.cs"));
        Assert.DoesNotContain("public struct WebhookSecurityAdvisoryWithdrawnSecurityAdvisoryVulnerabilitiesVulnerabilitiesItem", structuresFile);
        Assert.DoesNotContain("public int _1", structuresFile);
        Assert.DoesNotContain("public int __1", structuresFile);
    }

    [Fact]
    public async Task Generate_WithNestedArraySurfaces_UsesStringListsForOutSystemsCompatibility()
    {
        var specPath = CreateTempSpecFile("""
            {
              "openapi": "3.0.1",
              "info": { "title": "Nested Arrays", "version": "1.0.0" },
              "paths": {
                "/repos/{owner}/{repo}/stats/code_frequency": {
                  "get": {
                    "operationId": "repos/get-code-frequency-stats",
                    "parameters": [
                      { "name": "owner", "in": "path", "required": true, "schema": { "type": "string" } },
                      { "name": "repo", "in": "path", "required": true, "schema": { "type": "string" } }
                    ],
                    "responses": {
                      "200": {
                        "description": "ok",
                        "content": {
                          "application/json": {
                            "schema": {
                              "type": "array",
                              "items": {
                                "type": "array",
                                "items": { "type": "integer" }
                              }
                            }
                          }
                        }
                      }
                    }
                  }
                }
              },
              "components": {
                "schemas": {
                  "projects-v2-view": {
                    "type": "object",
                    "properties": {
                      "sort_by": {
                        "type": "array",
                        "items": {
                          "type": "array",
                          "items": { "type": "integer" }
                        }
                      }
                    }
                  }
                }
              }
            }
            """);
        var outputDirectory = CreateTempDirectory();
        var options = CreateMinimalOptions(specPath, outputDirectory);
        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);

        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        var structuresFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedStructures.g.cs"));
        var actionsFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedActions.g.cs"));

        Assert.Contains("public List<string> SortBy", structuresFile);
        Assert.Contains("public List<string> ReposGetCodeFrequencyStats(string owner, string repo)", actionsFile);
    }

    [Fact]
    public async Task Generate_WithRecursiveExpandableProxyBranch_PreservesNestedOptionType()
    {
        var specPath = CreateTempSpecFile("""
            {
              "openapi": "3.0.1",
              "info": { "title": "Recursive Proxy Branch", "version": "1.0.0" },
              "paths": {},
              "components": {
                "schemas": {
                  "checkout_session": {
                    "type": "object",
                    "properties": {
                      "subscription": {
                        "anyOf": [
                          { "type": "string" },
                          { "$ref": "#/components/schemas/subscription" }
                        ]
                      }
                    }
                  },
                  "subscription": {
                    "type": "object",
                    "properties": {
                      "latest_invoice": {
                        "anyOf": [
                          { "type": "string" },
                          { "$ref": "#/components/schemas/invoice" }
                        ]
                      }
                    }
                  },
                  "invoice": {
                    "type": "object",
                    "properties": {
                      "lines": {
                        "anyOf": [
                          { "type": "string" },
                          { "$ref": "#/components/schemas/line_list" }
                        ]
                      }
                    }
                  },
                  "line_list": {
                    "type": "object",
                    "properties": {
                      "data": {
                        "type": "array",
                        "items": {
                          "$ref": "#/components/schemas/line_item"
                        }
                      }
                    }
                  },
                  "line_item": {
                    "type": "object",
                    "properties": {
                      "subscription": {
                        "anyOf": [
                          { "type": "string" },
                          { "$ref": "#/components/schemas/subscription" }
                        ]
                      }
                    }
                  }
                }
              }
            }
            """);
        var outputDirectory = CreateTempDirectory();
        var options = CreateMinimalOptions(specPath, outputDirectory);
        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);

        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        var structuresFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedStructures.g.cs"));
        var checkoutSubscriptionBlock = System.Text.RegularExpressions.Regex.Match(
            structuresFile,
            @"public struct CheckoutSessionSubscription\s*\{(?<body>[\s\S]*?)\n\}",
            System.Text.RegularExpressions.RegexOptions.CultureInvariant);

        Assert.True(checkoutSubscriptionBlock.Success);
        Assert.Contains("OriginalName = \"option_2\"", checkoutSubscriptionBlock.Groups["body"].Value);
    }

    [Fact]
    public async Task Generate_WithMissingProxyBranchReference_FallsBackToString()
    {
        var specPath = CreateTempSpecFile("""
            {
              "openapi": "3.0.1",
              "info": { "title": "Missing Proxy Branch", "version": "1.0.0" },
              "paths": {},
              "components": {
                "schemas": {
                  "broken_proxy": {
                    "type": "object",
                    "properties": {
                      "option_1": { "type": "string" },
                      "option_2": { "$ref": "#/components/schemas/missing_branch" }
                    }
                  }
                }
              }
            }
            """);
        var outputDirectory = CreateTempDirectory();
        var options = CreateMinimalOptions(specPath, outputDirectory);
        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);

        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        var structuresFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedStructures.g.cs"));
        Assert.Contains("public string? Option2", structuresFile);
    }

    [Fact]
    public async Task Generate_WithPrimitiveArrayResponse_UsesMapperInsteadOfToList()
    {
        var specPath = CreateTempSpecFile("""
            {
              "openapi": "3.0.1",
              "info": { "title": "Primitive Array", "version": "1.0.0" },
              "paths": {
                "/versions": {
                  "get": {
                    "operationId": "meta/get-all-versions",
                    "responses": {
                      "200": {
                        "description": "ok",
                        "content": {
                          "application/json": {
                            "schema": {
                              "type": "array",
                              "items": { "type": "string" }
                            }
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
            """);
        var outputDirectory = CreateTempDirectory();
        var options = CreateMinimalOptions(specPath, outputDirectory);
        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);

        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        var actionsFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedActions.g.cs"));
        Assert.Contains("return GeneratedModelMapper.Convert<List<string>>(response) ?? new List<string>();", actionsFile);
        Assert.DoesNotContain("return response?.ToList() ?? new List<string>();", actionsFile);
    }

    [Fact]
    public async Task Generate_WithBinaryRequestBody_UsesByteArrayInputAndStreamConversion()
    {
        var specPath = CreateTempSpecFile("""
            {
              "openapi": "3.0.1",
              "info": { "title": "Binary Request Probe", "version": "1.0.0" },
              "paths": {
                "/logos/{logoId}": {
                  "put": {
                    "operationId": "putLogo",
                    "parameters": [
                      { "name": "logoId", "in": "path", "required": true, "schema": { "type": "string" } }
                    ],
                    "requestBody": {
                      "required": true,
                      "content": {
                        "image/png": {
                          "schema": { "type": "string", "format": "binary" }
                        }
                      }
                    },
                    "responses": {
                      "204": { "description": "Updated" }
                    }
                  }
                }
              }
            }
            """);

        var outputDirectory = CreateTempDirectory();
        var options = CreateMinimalOptions(specPath, outputDirectory);
        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);

        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        var actionsFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedActions.g.cs"));
        Assert.Contains("byte[] requestBody", actionsFile);
        Assert.Contains("requestBuilder.PutAsync(new MemoryStream(requestBody ?? Array.Empty<byte>()))", actionsFile);
    }

    [Fact]
    public async Task Generate_WithBinaryResponse_UsesByteArrayReturnAndReadAllBytes()
    {
        var specPath = CreateTempSpecFile("""
            {
              "openapi": "3.0.1",
              "info": { "title": "Binary Response Probe", "version": "1.0.0" },
              "paths": {
                "/logos/{logoId}": {
                  "get": {
                    "operationId": "getLogo",
                    "parameters": [
                      { "name": "logoId", "in": "path", "required": true, "schema": { "type": "string" } }
                    ],
                    "responses": {
                      "200": {
                        "description": "Binary content",
                        "content": {
                          "image/png": {
                            "schema": { "type": "string", "format": "binary" }
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
            """);

        var outputDirectory = CreateTempDirectory();
        var options = CreateMinimalOptions(specPath, outputDirectory);
        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);

        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        var actionsFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedActions.g.cs"));
        Assert.Contains("public byte[] ", actionsFile);
        Assert.Contains("return GeneratedModelMapper.ReadAllBytes(response);", actionsFile);
    }

    [Fact]
    public async Task Generate_WithObsoleteKiotaQueryPropertyAndMethod_UsesPreferredMembers()
    {
        var specPath = CreateTempSpecFile("""
            {
              "openapi": "3.0.1",
              "info": { "title": "Preferred Members Probe", "version": "1.0.0" },
              "paths": {
                "/advisories": {
                  "get": {
                    "operationId": "listAdvisories",
                    "parameters": [
                      {
                        "name": "direction",
                        "in": "query",
                        "required": false,
                        "schema": { "type": "string" }
                      }
                    ],
                    "responses": {
                      "200": {
                        "description": "ok",
                        "content": {
                          "application/json": {
                            "schema": {
                              "type": "object",
                              "properties": {
                                "count": { "type": "integer" }
                              }
                            }
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
            """);
        var outputDirectory = CreateTempDirectory();
        var kiotaLockPath = CreateTempKiotaClient(
            ("Advisories\\AdvisoriesRequestBuilder.cs", """
                namespace Probe.Client.Advisories;

                public partial class AdvisoriesRequestBuilder
                {
                    public async Task<Probe.Client.Advisories.AdvisoriesGetResponse?> GetAsAdvisoriesGetResponseAsync(Action<RequestConfiguration<AdvisoriesRequestBuilderGetQueryParameters>>? requestConfiguration = default, CancellationToken cancellationToken = default)
                    {
                        throw new NotImplementedException();
                    }

                    [Obsolete("This method is obsolete. Use GetAsAdvisoriesGetResponseAsync instead.")]
                    public async Task<List<Probe.Client.Models.GlobalAdvisory>?> GetAsync(Action<RequestConfiguration<AdvisoriesRequestBuilderGetQueryParameters>>? requestConfiguration = default, CancellationToken cancellationToken = default)
                    {
                        throw new NotImplementedException();
                    }

                    public partial class AdvisoriesRequestBuilderGetQueryParameters
                    {
                        [Obsolete("This property is deprecated, use DirectionAsGetDirectionQueryParameterType instead")]
                        public string? Direction { get; set; }

                        public global::Probe.Client.Advisories.GetDirectionQueryParameterType? DirectionAsGetDirectionQueryParameterType { get; set; }
                    }
                }
                """),
            ("Advisories\\GetDirectionQueryParameterType.cs", """
                namespace Probe.Client.Advisories;

                public enum GetDirectionQueryParameterType
                {
                    Asc,
                    Desc
                }
                """));

        var options = CreateMinimalOptionsWithKiotaLock(specPath, outputDirectory, kiotaLockPath);
        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);

        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        var actionsFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedActions.g.cs"));
        Assert.Contains("config.QueryParameters.DirectionAsGetDirectionQueryParameterType = GeneratedModelMapper.ParseEnum<Probe.Client.Advisories.GetDirectionQueryParameterType>(direction);", actionsFile);
    }

    [Fact]
    public async Task Generate_WithTypedKiotaIndexer_UsesTypedPathAccess()
    {
        var specPath = CreateTempSpecFile("""
            {
              "openapi": "3.0.1",
              "info": { "title": "Typed Indexer Probe", "version": "1.0.0" },
              "paths": {
                "/app/hook/deliveries/{delivery_id}": {
                  "get": {
                    "operationId": "getDelivery",
                    "parameters": [
                      {
                        "name": "delivery_id",
                        "in": "path",
                        "required": true,
                        "schema": { "type": "integer" }
                      }
                    ],
                    "responses": {
                      "200": {
                        "description": "ok",
                        "content": {
                          "application/json": {
                            "schema": {
                              "type": "object",
                              "properties": {
                                "id": { "type": "integer" }
                              }
                            }
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
            """);
        var outputDirectory = CreateTempDirectory();
        var kiotaLockPath = CreateTempKiotaClient(
            ("App\\Hook\\Deliveries\\DeliveriesRequestBuilder.cs", """
                namespace Probe.Client.App.Hook.Deliveries;

                public partial class DeliveriesRequestBuilder
                {
                    public Probe.Client.App.Hook.Deliveries.Item.WithDelivery_ItemRequestBuilder this[int position] => throw new NotImplementedException();

                    [Obsolete("This indexer is deprecated and will be removed in the next major version. Use the one with the typed parameter instead.")]
                    public Probe.Client.App.Hook.Deliveries.Item.WithDelivery_ItemRequestBuilder this[string position] => throw new NotImplementedException();
                }
                """));

        var options = CreateMinimalOptionsWithKiotaLock(specPath, outputDirectory, kiotaLockPath);
        var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None);

        OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

        var actionsFile = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "GeneratedActions.g.cs"));
        Assert.Contains("_client.App.Hook.Deliveries[deliveryId]", actionsFile);
        Assert.DoesNotContain("_client.App.Hook.Deliveries[deliveryId.ToString()]", actionsFile);
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

    private static GeneratorOptions CreateMinimalOptions(string specSource, string outputDirectory)
    {
        return new GeneratorOptions
        {
            SpecSource = specSource,
            OutputDirectory = outputDirectory,
            Namespace = "Probe.Generated",
            ClientNamespace = "Probe.Client",
            ClientClassName = "ProbeClient",
            ClassName = "ProbeGenerated",
            InterfaceName = "IProbeGenerated",
            InputName = "Probe API"
        };
    }

    private static GeneratorOptions CreateMinimalOptionsWithKiotaLock(string specSource, string outputDirectory, string kiotaLockPath)
    {
        return new GeneratorOptions
        {
            SpecSource = specSource,
            OutputDirectory = outputDirectory,
            Namespace = "Probe.Generated",
            KiotaLockPath = kiotaLockPath,
            ClassName = "ProbeGenerated",
            InterfaceName = "IProbeGenerated",
            InputName = "Probe API"
        };
    }

    private static string CreateTempKiotaClient(params (string RelativePath, string Content)[] files)
    {
        var rootDirectory = CreateTempDirectory();
        var clientDirectory = Path.Combine(rootDirectory, "Client");
        Directory.CreateDirectory(clientDirectory);

        var lockPath = Path.Combine(clientDirectory, "kiota-lock.json");
        File.WriteAllText(lockPath, """
            {
              "clientNamespaceName": "Probe.Client",
              "clientClassName": "ProbeClient"
            }
            """);

        foreach (var (relativePath, content) in files)
        {
            var filePath = Path.Combine(clientDirectory, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            File.WriteAllText(filePath, content);
        }

        return lockPath;
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

    private static string CreateTempSpecFile(string content)
    {
        var directory = CreateTempDirectory();
        var filePath = Path.Combine(directory, "openapi.json");
        File.WriteAllText(filePath, content);
        return filePath;
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
