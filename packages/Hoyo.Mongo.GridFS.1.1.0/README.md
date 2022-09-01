#### Hoyo.Mongo

##### 如何使用?

- 在系统环境变量或者 Docker 容器中设置环境变量名称为: CONNECTIONSTRINGS_MONGO = mongodb 链接字符串 或者在 appsetting.json 中添加,
- 现在你也可以参考example.api项目查看直接传入相关数据.

```json
{
  "ConnectionStrings": {
    "Mongo": "mongodb链接字符串"
  },
  // 或者使用
  "CONNECTIONSTRINGS_MONGO": "mongodb链接字符串"
}
```

##### 使用 Hoyo.Mongo.GridFS

- 使用 Nuget 安装 Hoyo.Mongo.GridFS
- .Net 6 +

```csharp
using example.api;
using Hoyo.Mongo;
using Miracle.WebApi.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 添加Mongodb数据库服务
var dboptions = new HoyoMongoOptions();
dboptions.AppendConventionRegistry("IdentityServer Mongo Conventions", new()
{
    Conventions = new()
    {
        new IgnoreExtraElementsConvention(true)
    },
    Filter = _ => true
});

var db = await builder.Services.AddMongoDbContext<DbContext>(clientSettings: new()
{
    ServerAddresses = new()
    {
        new("192.168.2.10", 27017),
    },
    AuthDatabase = "admin",
    DatabaseName = "miracle",
    UserName = "oneblogs",
    Password = "&oneblogs.cn",
}, dboptions: dboptions);
// 添加GridFS服务
builder.Services.AddMiracleGridFS(db._database!, new()
{
    BusinessApp = "HoyoFS",
    Options = new()
    {
        BucketName = "hoyofs",
        ChunkSizeBytes = 1024,
        DisableMD5 = true,
        ReadConcern = new() { },
        ReadPreference = ReadPreference.Primary,
        WriteConcern = WriteConcern.Unacknowledged
    },
    DefalutDB = true,
    ItemInfo = "item.info"

});

...

var app = builder.Build();
```
- 详细内容参见 example.api 项目