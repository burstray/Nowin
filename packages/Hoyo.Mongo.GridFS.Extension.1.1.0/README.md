#### Hoyo.Mongo


```json
{
  // 添加文件缓存
  "MiracleStaticFile": {
    "VirtualPath": "/miraclefs",
    "PhysicalPath": "/home/username/test"
  }
}
```

##### 使用 Hoyo.Mongo.GridFS.Extension

- 新增文件缓存到物理路径,便于文件在线使用.
- 添加物理路径清理接口.(可通过调用该接口定时清理所有缓存的文件)

---

- 使用 Nuget 安装 Hoyo.Mongo.GridFS.Extension
- .Net 6 +

```csharp
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();

...

// 添加虚拟目录用于缓存文件,便于在线播放等功能.
app.UseHoyoGridFSVirtualPath(builder.Configuration);

...

app.Run();
```

- 详细内容参见 example.api 项目