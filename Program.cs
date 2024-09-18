using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.HttpSys;

var builder = WebApplication.CreateBuilder(args);

// 跨域存取設定
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        policy =>
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        });
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("CorsPolicy");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();




async Task<object> GetAPIDataAsync(string url)
{
    HttpClient client = new HttpClient();
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; AcmeInc/1.0)");

    var request = new HttpRequestMessage
    {
        Method = HttpMethod.Get,
        RequestUri = new Uri(url)
    };


    string responseBody = "";

    try
    {
        // 發送請求
        HttpResponseMessage response = await client.SendAsync(request);

        // 檢查響應狀態碼
        if (response.IsSuccessStatusCode)
        {
            // 讀取響應內容
            responseBody = await response.Content.ReadAsStringAsync();
            // responseBody = responseBody.Replace("\\\"", "\"");
            Console.WriteLine("已完成請求的responseBody");
            Console.WriteLine(responseBody);
            Console.WriteLine(response.Content);
        }
        else
        {
            Console.WriteLine($"請求失敗，狀態碼: {response.StatusCode}");
        }
    }
    catch (HttpRequestException e)
    {
        Console.WriteLine("HTTP 請求異常:");
        Console.WriteLine(e.Message);
    }
    catch (Exception e)
    {
        Console.WriteLine("其他異常:");
        Console.WriteLine(e.Message);
    }

    return new
    {
        code = 0,
        message = "成功",
        data = responseBody
    };
}


app.MapGet("/zoo-news", async () =>
{
    return await GetAPIDataAsync("https://www.zoo.gov.taipei/OpenData.aspx?SN=022A4E6F1C7F323A");
})
.WithName("ZooNews")
.WithOpenApi();

app.MapGet("/zoo-events", async () =>
{
    return await GetAPIDataAsync("https://www.zoo.gov.taipei/OpenData.aspx?SN=D2A75D913CDBB2CE");
}
)
.WithName("ZooEvents")
.WithOpenApi();

app.MapGet("/zoo-announcements", async () =>
{
    return await GetAPIDataAsync("https://www.zoo.gov.taipei/OpenData.aspx?SN=77C6F77920C917B2");
})
.WithName("ZooAnnouncement")
.WithOpenApi();

app.Run();
