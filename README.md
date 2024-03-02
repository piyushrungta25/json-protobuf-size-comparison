Quick and dirty comparison of serialized payload size between JSON and Protobuf.

# Generate probuf classes

```
mkdir -p generated
protoc --csharp_out=generated idl/protobuf/*.proto
```

# Generate Results

```
dotnet run
```

Results will be written in `generated/results.csv` file.

# Results

```
DataSet       Json    Protobuf  Diff      JsonGzip  ProtobufGzip  DiffGzip
stock         27100   9828      -63.73 %  5575      5357          -3.91 %
Commits       366825  292467    -20.27 %  62692     63169         0.76 %
SalaryData 1  31651   12293     -61.16 %  1505      1317          -12.49 %
SalaryData 2  315794  122424    -61.23 %  10029     8099          -19.24 %

```


# DataSets

1. Stock - AAPL daily stock price (truncated to only 2016 data)
   Ref - https://www.kaggle.com/datasets/borismarjanovic/price-volume-data-for-all-us-stocks-etfs?resource=download

2. Commits - latest 100 commits in `torvalds/linux` repo fetched via Github API
   ```
   gh api "/repos/torvalds/linux/commits?per_page=100" > datasets/linux_commit.json
   ```
   The data was slightly massaged to fit Protobuf's field naming convention (it clearly knows better than it's users)

3. Salary Data 1 & 2 - Data Science salary data, trucated to 100 and 1000 rows respectively
   Ref - https://www.kaggle.com/datasets/hummaamqaasim/jobs-in-data/data?select=jobs_in_data.csv