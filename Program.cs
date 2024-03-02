using System.Globalization;
using System.IO.Compression;
using System.Text.Json;
using CsvHelper;
using Google.Protobuf;

public class Serializer
{
    public static long ToJson(object o)
    {
        return JsonSerializer.SerializeToUtf8Bytes(o).Count();
    }

    public static long ToJsonGZip(object o)
    {
        using (var memStream = new MemoryStream())
        using (var compressedStream = new GZipStream(memStream, CompressionLevel.SmallestSize))
        {
            JsonSerializer.Serialize(compressedStream, o);
            memStream.Flush();
            compressedStream.Flush();
            return memStream.Length;
        }
    }

    public static long ToProtoBuf(IMessage o)
    {
        using (var memStream = new MemoryStream())
        {
            o.WriteTo(memStream);
            memStream.Flush();
            return memStream.Length;
        }
    }

    public static long ToProtoBufGZip(IMessage o)
    {
        using (var memStream = new MemoryStream())
        using (var compressedStream = new GZipStream(memStream, CompressionLevel.SmallestSize))
        {
            o.WriteTo(compressedStream);
            compressedStream.Flush();
            memStream.Flush();
            return memStream.Length;
        }
    }
}

class Result
{
    public string DataSet { get; set; }
    public long Json { get; set; }
    public long Protobuf { get; set; }
    public string Diff { get; set; }
    public long JsonGzip { get; set; }
    public long ProtobufGzip { get; set; }
    public string DiffGzip { get; set; }
}

internal class Program
{
    private static string DiffPerc(long baseline, long data)
    {
        var diff = (data - baseline) * 100.0 / baseline;
        return $"{Math.Round(diff, 2)} %";
    }

    private static Result GenerateResult(IMessage obj, string datasetName)
    {
        var json = Serializer.ToJson(obj);
        var jsonGzip = Serializer.ToJsonGZip(obj);
        var proto = Serializer.ToProtoBuf(obj);
        var protogzip = Serializer.ToProtoBufGZip(obj);

        return new Result
        {
            DataSet = datasetName,
            Json = json,
            Protobuf = proto,
            JsonGzip = jsonGzip,
            ProtobufGzip = protogzip,
            Diff = DiffPerc(json, proto),
            DiffGzip = DiffPerc(jsonGzip, protogzip)
        };
    }

    private static Result Stock()
    {

        var stockList = new StockList();
        using (var reader = new StreamReader($"datasets/aapl.csv"))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<Stock>();

            foreach (var r in records)
                stockList.Stocks.Add(r);
        }

        return GenerateResult(stockList, $"Stock");
    }



    private static Result Commits()
    {
        var commits = JsonSerializer.Deserialize<List<CommitTL>>(
            File.ReadAllText("datasets/linux_commit_pascal.json")
        );

        CommitList commitList = new CommitList();
        foreach (var c in commits)
        {
            commitList.Commits.Add(c);
        }

        return GenerateResult(commitList, "Commits");
    }

    private static Result JobData(int i)
    {
        var jobDataList = new JobList();
        using (var reader = new StreamReader($"datasets/jobs_in_data_{i}.csv"))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<JobData>();

            foreach (var r in records)
                jobDataList.JobsData.Add(r);
        }

        return GenerateResult(jobDataList, $"SalaryData {i}");
    }


    private static void Main(string[] args)
    {
        using (var writer = new StreamWriter("generated/results.csv"))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteHeader<Result>();
            csv.NextRecord();
            csv.WriteRecord<Result>(Stock());
            csv.NextRecord();
            csv.WriteRecord<Result>(Commits());
            csv.NextRecord();
            csv.WriteRecord<Result>(JobData(1));
            csv.NextRecord();
            csv.WriteRecord<Result>(JobData(2));
        }
    }
}
