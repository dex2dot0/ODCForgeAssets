using System.Net;
using System.Net.Mime;
using System.Text;

namespace JsonPlaceholderKiota.Tests;

public class JsonPlaceholderPostsTests
{
    [Fact]
    public void ListPosts_MapsCollectionAndQueryParameters()
    {
        HttpRequestMessage? capturedRequest = null;
        var service = CreateService(request =>
        {
            capturedRequest = request;
            return JsonResponse("""
                [
                  { "id": 1, "userId": 9, "title": "alpha", "body": "first" }
                ]
                """);
        });

        var posts = service.ListPosts(9, "alpha");

        Assert.Single(posts);
        Assert.Equal(1, posts[0].Id);
        Assert.Equal("alpha", posts[0].Title);
        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Get, capturedRequest!.Method);
        Assert.Equal("/posts", capturedRequest.RequestUri!.AbsolutePath);
        Assert.Equal("?title=alpha&userId=9", capturedRequest.RequestUri.Query);
    }

    [Fact]
    public void GetPostById_ReturnsMappedStructure()
    {
        var service = CreateService(_ => JsonResponse("""
            { "id": 5, "userId": 2, "title": "hello", "body": "world" }
            """));

        var post = service.GetPostById(5);

        Assert.Equal(5, post.Id);
        Assert.Equal(2, post.UserId);
        Assert.Equal("hello", post.Title);
        Assert.Equal("world", post.Body);
    }

    [Fact]
    public void CreatePost_SendsJsonBodyAndReturnsMappedStructure()
    {
        string? requestBody = null;
        var service = CreateService(request =>
        {
            requestBody = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
            return JsonResponse("""
                { "id": 101, "userId": 7, "title": "created", "body": "payload" }
                """, HttpStatusCode.Created);
        });

        var created = service.CreatePost(new PostItem { UserId = 7, Title = "created", Body = "payload" });

        Assert.Equal(101, created.Id);
        Assert.Contains("\"title\":\"created\"", requestBody);
        Assert.Contains("\"userId\":7", requestBody);
    }

    [Fact]
    public void UpdatePost_UsesPatchAndReturnsMappedStructure()
    {
        HttpMethod? method = null;
        var service = CreateService(request =>
        {
            method = request.Method;
            return JsonResponse("""
                { "id": 4, "userId": 3, "title": "updated", "body": "patch" }
                """);
        });

        var updated = service.UpdatePost(4, new PostItem { Title = "updated", Body = "patch", UserId = 3 });

        Assert.Equal("updated", updated.Title);
        Assert.Equal("PATCH", method!.Method);
    }

    [Fact]
    public void DeletePost_UsesDelete()
    {
        HttpMethod? method = null;
        var service = CreateService(request =>
        {
            method = request.Method;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(string.Empty, Encoding.UTF8, MediaTypeNames.Application.Json)
            };
        });

        service.DeletePost(11);

        Assert.Equal(HttpMethod.Delete, method);
    }

    private static JsonPlaceholderPosts CreateService(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var handler = new StubHttpMessageHandler(responder);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://jsonplaceholder.typicode.com")
        };

        return new JsonPlaceholderPosts(new PostsClientFactory(httpClient));
    }

    private static HttpResponseMessage JsonResponse(string json, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json)
        };
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_responder(request));
        }
    }
}
