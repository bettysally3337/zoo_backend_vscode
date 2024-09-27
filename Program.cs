using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Server;

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



string connectionString = app.Configuration.GetConnectionString("SQL_CONNECTIONSTRING")!;


//Foodstand api拿到的資料類別

app.MapGet("/Facility/foodstand", () =>
{
    // var rows = new List<string>();

    var foodstands = new List<Foodstand>();

    var conn = new SqlConnection(connectionString);
    conn.Open();

    var command = new SqlCommand(
        @"SELECT S_Title,S_Meal,S_Location,S_Memo,S_Pic01_URL 
        FROM serviceSpot 
        WHERE S_Item = '餐飲'", conn);
    using SqlDataReader reader = command.ExecuteReader();

    if (reader.HasRows)
    {
        while (reader.Read())
        {
            var foodstand = new Foodstand
            {
                Title = reader.GetString(0),
                Meal = reader.GetString(1),
                Location = reader.GetString(2),
                Memo = reader.GetString(3),
                PicUrl = reader.GetString(4)
            };
            foodstands.Add(foodstand);
        }
    }

    return foodstands;
})
.WithName("GetFoodstands")
.WithOpenApi();

app.MapGet("/Facility/giftshop", () =>
{
    var giftshops = new List<Giftshop>();
    var conn = new SqlConnection(connectionString);
    conn.Open();
    var command = new SqlCommand(
        @"SELECT S_Title,S_Brief,S_Location,S_Memo,S_Pic01_URL 
        FROM serviceSpot 
        WHERE S_Item = '商店'", conn);
    using SqlDataReader reader = command.ExecuteReader();

    if (reader.HasRows)
    {
        while (reader.Read())
        {
            var giftshop = new Giftshop
            {
                Title = reader.GetString(0),
                Brief = reader.GetString(1),
                Location = reader.GetString(2),
                Memo = reader.GetString(3),
                PicUrl = reader.GetString(4)
            };
            giftshops.Add(giftshop);
        }
    }
    return giftshops;
})
.WithName("GetGiftShops")
.WithOpenApi();

app.MapGet("/Facility/guestservices", () =>
{



    var guestserviceslist = new List<GuestServices>();
    var conn = new SqlConnection(connectionString);
    conn.Open();

    var command = new SqlCommand(
    @"SELECT S_Title,S_Brief,S_Location,S_Memo,S_Pic01_URL 
    FROM serviceSpot 
    WHERE S_Item IN ('娃娃車／輪椅租用','列車站','車站') 
    AND S_Pic01_URL IS NOT NULL 
    AND TRIM(S_Pic01_URL) <> ''", conn);
    using SqlDataReader reader = command.ExecuteReader();

    if (reader.HasRows)
    {
        while (reader.Read())
        {
            var guestservice = new GuestServices
            {
                Title = reader.GetString(0),
                Brief = reader.GetString(1),
                Location = reader.GetString(2),
                Memo = reader.GetString(3),
                PicUrl = reader.GetString(4)
            };
            guestserviceslist.Add(guestservice);
        }
    }
    else
    {
        Console.WriteLine("沒有返回任何資料");
    }
    return guestserviceslist;
})
.WithName("GetGuestServices")
.WithOpenApi();

// 建立 table
// try
// {
//     // Table would be created ahead of time in production
//     using var conn = new SqlConnection(connectionString);
//     conn.Open();

//     var command = new SqlCommand(
//         "CREATE TABLE Persons (ID int NOT NULL PRIMARY KEY IDENTITY, FirstName varchar(255), LastName varchar(255));",
//         conn);
//     using SqlDataReader reader = command.ExecuteReader();
// }
// catch (Exception e)
// {
//     // Table may already exist
//     Console.WriteLine(e.Message);
// }

//LatestNews三個API的function
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


//LatestNews的三個API
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


public class Foodstand
{
    [Required]
    public string Title { get; set; } = string.Empty;
    public string? Meal { get; set; }

    public string? Location { get; set; }

    public string? Memo { get; set; }
    [Required]
    public string PicUrl { get; set; } = string.Empty;
}
public class Giftshop
{
    [Required]
    public string Title { get; set; } = string.Empty;
    public string? Brief { get; set; }

    public string? Location { get; set; }

    public string? Memo { get; set; }
    [Required]
    public string PicUrl { get; set; } = string.Empty;
}

public class GuestServices
{
    [Required]
    public string Title { get; set; } = string.Empty;
    public string? Brief { get; set; }

    public string? Location { get; set; }

    public string? Memo { get; set; }

    [Required]
    public string PicUrl { get; set; } = string.Empty;
}