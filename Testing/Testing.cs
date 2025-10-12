using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;


namespace LLMCodeReviewer;


public class ScriptStorageTests
{
    [Fact]
    public void UniqueId()
    {
        List<string> scriptsId = new List<string>();

        for (int i = 0; i < 10; i++)
        {
            var defaultScripts = new Script();
            scriptsId.Add(defaultScripts.Id);
        }



        bool allUnique = scriptsId.Distinct().Count() == scriptsId.Count;
        Assert.True(allUnique);
    }

    [Fact]
    public void SavingSymbolsTest()
    {
        List<Script> scripts = ScriptStorage.LoadScripts();
        List<Script> testscripts = new List<Script>();

        int SAMPLE = 10;
        int passed = 0;
        Dictionary<string, string> scriptsData = new Dictionary<string, string>();
        
        string symbols = "`~!@#$%^&*()-_=+[{]}\\\\|;:'\",<.>/? \\t\\n\\r";
        
        
        for (int i = 0; i < SAMPLE; i++)
        {
            string shuffled = ShuffleString(symbols);
            Script s = new() { Title = shuffled, Content = shuffled };
            testscripts.Add(s);
            scriptsData.Add(s.Id, shuffled);
        }
        
        
        ScriptStorage.SaveScripts(testscripts);
        List<Script> loadedScripts = ScriptStorage.LoadScripts();

        foreach (var scriptData in scriptsData)
        {
            foreach (Script script in loadedScripts)
            {
                if (script.Id == scriptData.Key)
                {
                    if (script.Title == scriptData.Value && script.Content == scriptData.Value)
                        passed++;
                }
            }
        }
        
        ScriptStorage.ClearScriptsDirectory();
        ScriptStorage.SaveScripts(scripts);
        Assert.Equal(passed, SAMPLE);
    }
    
    static string ShuffleString(string input)
    {
        Random rand = new Random();
        return new string(input.OrderBy(c => rand.Next()).ToArray());
    }
    
}

public class LLMTests
{
    public class FakeHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _func;

        public FakeHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> func)
        {
            _func = func;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _func(request);
        }
    }
    [Fact]
    public void API_KEY_ERROR()
    {
        var oldKey = Environment.GetEnvironmentVariable("OPEN_AI_API_KEY");

        try
        {
            Environment.SetEnvironmentVariable("OPEN_AI_API_KEY", null);

            LLM llm = new LLM();

            Assert.Equal(LlmInitError.NoApiKey, llm.ErrorCode);
            Assert.Equal("OpenAI API key not found.", llm.Placeholder);
        }
        finally
        {
            Environment.SetEnvironmentVariable("OPEN_AI_API_KEY", oldKey);
        }
    }
    
    [Fact]
    public void API_KEY_SUCCESS()
    {
        var oldKey = Environment.GetEnvironmentVariable("OPEN_AI_API_KEY");

        try
        {
            Environment.SetEnvironmentVariable("OPEN_AI_API_KEY", "FAKE_KEY");

            var llm = new LLM();

            Assert.NotEqual(LlmInitError.NoApiKey, llm.ErrorCode);
        }
        finally
        {
            Environment.SetEnvironmentVariable("OPEN_AI_API_KEY", oldKey);
        }
    }
    
    [Theory]
    [InlineData(HttpStatusCode.ServiceUnavailable, LlmInitError.NoConnection, "(503)")]
    [InlineData(HttpStatusCode.InternalServerError, LlmInitError.NoConnection, "(500)")]
    public void Should_Set_Correct_ErrorCode_For_Http_Status(HttpStatusCode code, LlmInitError expected, string caseName)
    {
        var http = new HttpClient(new FakeHandler(_ =>
            Task.FromResult(new HttpResponseMessage(code))))
        {
            BaseAddress = new Uri("https://api.openai.com/")
        };

        Environment.SetEnvironmentVariable("OPEN_AI_API_KEY", "FAKE_KEY");

        LLM llm = new LLM(model: "gpt-4o");
        
        Assert.Equal(expected, llm.ErrorCode);
    }
    
}