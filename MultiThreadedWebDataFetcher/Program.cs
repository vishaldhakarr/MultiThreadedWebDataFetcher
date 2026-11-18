
using System;
using System.Net.Http;
using System.Text.Json;
using System.Diagnostics;

Console.WriteLine("====================================");
Console.WriteLine(" MULTI-THREADED WEB DATA FETCHER");
Console.WriteLine("====================================");

using HttpClient client = new HttpClient();

// Configure 5 API URLs
string[] apiUrls =
{
    "https://jsonplaceholder.typicode.com/posts/1",
    "https://jsonplaceholder.typicode.com/posts/2",
    "https://jsonplaceholder.typicode.com/posts/3",
    "https://jsonplaceholder.typicode.com/posts/4",
    "https://jsonplaceholder.typicode.com/posts/5"
};

// --------------------------------
// 1. SEQUENTIAL EXECUTION
// --------------------------------

Console.WriteLine("\nStarting Sequential Execution...");

Stopwatch sequentialWatch = Stopwatch.StartNew();

string[] sequentialResults =
    await FetchSequentialAsync(client, apiUrls);

sequentialWatch.Stop();

Console.WriteLine("\nSequential Results:");

foreach (string result in sequentialResults)
{
    Console.WriteLine(result);
}

Console.WriteLine(
    $"Sequential Time: {sequentialWatch.Elapsed.TotalSeconds:F3} seconds");


// --------------------------------
// 2. CONCURRENT EXECUTION
// --------------------------------

Console.WriteLine("\nStarting Concurrent Execution...");

Stopwatch concurrentWatch = Stopwatch.StartNew();

string[] concurrentResults =
    await FetchConcurrentAsync(client, apiUrls);

concurrentWatch.Stop();

Console.WriteLine("\nConcurrent Results:");

foreach (string result in concurrentResults)
{
    Console.WriteLine(result);
}

Console.WriteLine(
    $"Concurrent Time: {concurrentWatch.Elapsed.TotalSeconds:F3} seconds");



int successfulApis = 0;
int failedApis = 0;

foreach (string result in concurrentResults)
{
    if (result.Contains("STATUS: SUCCESS"))
    {
        successfulApis++;
    }
    else if (result.Contains("STATUS: FAILED"))
    {
        failedApis++;
    }
}

Console.WriteLine();
Console.WriteLine("====================================");
Console.WriteLine("          EXECUTION SUMMARY");
Console.WriteLine("====================================");

Console.WriteLine("Total APIs       : " + apiUrls.Length);
Console.WriteLine("Successful APIs  : " + successfulApis);
Console.WriteLine("Failed APIs      : " + failedApis);
Console.WriteLine("Sequential Time  : " + sequentialWatch.Elapsed.TotalSeconds.ToString("F3") + " seconds");
Console.WriteLine("Concurrent Time  : " + concurrentWatch.Elapsed.TotalSeconds.ToString("F3") + " seconds");

double timeSaved =
    (sequentialWatch.Elapsed.TotalSeconds -
     concurrentWatch.Elapsed.TotalSeconds)
    / sequentialWatch.Elapsed.TotalSeconds * 100;

Console.WriteLine("Time Saved       : " + timeSaved.ToString("F2") + "%");
Console.WriteLine("====================================");


// --------------------------------
// METHOD 1: SEQUENTIAL API CALLS
// --------------------------------

static async Task<string[]> FetchSequentialAsync(
    HttpClient client,
    string[] apiUrls)
{
    List<string> results = new List<string>();

    foreach (string url in apiUrls)
    {
        Console.WriteLine("Starting API: " + url);

        string result = await FetchApiAsync(client, url);

        results.Add(result);

        Console.WriteLine("Completed API: " + url);
    }

    return results.ToArray();
}


// --------------------------------
// METHOD 2: CONCURRENT API CALLS
// --------------------------------

static async Task<string[]> FetchConcurrentAsync(
    HttpClient client,
    string[] apiUrls)
{
    Task<string>[] tasks = new Task<string>[apiUrls.Length];

    for (int i = 0; i < apiUrls.Length; i++)
    {
        Console.WriteLine("Starting API: " + apiUrls[i]);

        tasks[i] = FetchApiAsync(client, apiUrls[i]);
    }

    return await Task.WhenAll(tasks);
}


// --------------------------------
// COMMON API REQUEST METHOD
// --------------------------------

static async Task<string> FetchApiAsync(
    HttpClient client,
    string url)
{

   
        try
        {
            Log("INFO", "Starting API: " + url);

            // Send GET request
            HttpResponseMessage response = await client.GetAsync(url);

            // Check whether API returned success
            response.EnsureSuccessStatusCode();

            // Read response
            string result = await response.Content.ReadAsStringAsync();

            // Parse JSON
            using JsonDocument document = JsonDocument.Parse(result);

            // Extract title
            string title = document.RootElement
                .GetProperty("title")
                .GetString() ?? "";

             Log("SUCCESS", "Completed API: " + url);

            // Return successful result
            return
                "STATUS: SUCCESS" +
                "\nAPI URL: " + url +
                "\nResponse Length: " + result.Length +
                "\nTitle: " + title +
                "\n------------------------------------";
        }
        catch (HttpRequestException ex)
        {
            Log("ERROR", "HTTP ERROR: " + url);

            return
                "STATUS: FAILED" +
                "\nAPI URL: " + url +
                "\nError: " + ex.Message +
                "\n------------------------------------";
        }
        catch (TaskCanceledException ex)
        {
            Log("ERROR", "TIMEOUT: " + url);

            return
                "STATUS: FAILED" +
                "\nAPI URL: " + url +
                "\nError: " + ex.Message +
                "\n------------------------------------";
        }
        catch (Exception ex)
        {
            Log("ERROR", "UNEXPECTED ERROR: " + url);

            return
                "STATUS: FAILED" +
                "\nAPI URL: " + url +
                "\nError: " + ex.Message +
                "\n------------------------------------";
        }





    static void Log(string level, string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");

        Console.WriteLine(
            $"[{timestamp}] [{level}] {message}");
    }


}
