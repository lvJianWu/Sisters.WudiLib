# 无敌lib
为方便 C# 调用酷 Q [HTTP API](https://github.com/richardchien/coolq-http-api) 插件而开发的lib。

- Named by [int100](https://github.com/1004121460)

## 如何使用
克隆本项目，然后在您的项目中添加对本项目的引用。

## 帮助
### 发送消息、调用 API
``` C#
var httpApi = new HttpApiClient();
httpApi.ApiAddress = "http://127.0.0.1:5700/";
var privateResponse = await httpApi.SendPrivateMessageAsync(12345678, "hello");
```
### 监听事件
``` C#
var listener = new ApiPostListener();
listener.ApiClient = httpApi; // 上面所示客户端的实例，将作为参数传给事件处理器，便于进行各种操作。
listener.PostAddress = "http://127.0.0.1:8080/";
listener.StartListen();
listener.AnonymousMessageEvent += ApiPostListener.RepeatAsync; // 复读匿名消息。
```

### 发送图片、语音等消息
可以通过 `SendingMessage` 的静态方法构造各种类型的消息，然后通过 `+` 连接。
```C#
// 网络图片消息。
var netImage = SendingMessage.NetImage("https://your.image.url/file.jpg");
// 文本消息。
var textMessage = new SendingMessage("这是一条文本消息。");
// 混合排版消息。
var mixedMessage = netImage + textMessage;
```

### Token 和 Secret
#### Token
可以为每个客户端设置不同的 AccessToken。
```C#
httpApi.AccessToken = "this-is-your-token";
```
#### Secret
可以为每个监听实例设置不同的 Secret。
```C#
listener.SetSecret("this-is-your-secret");
```
设置后，每次收到上报都会验证上报数据的哈希。如果验证失败，将忽略此次上报。

### 与现有代码共同使用
WudiLib 支持将收到的上报数据转发到另一处，相当于有两个上报地址，使得以前的代码可以继续运行，降低迁移成本。
```C#
listener.ForwardTo = "http://[::1]:10202"; // 转发路径，监听到的事件会被转发到此处。
```
只要原来的代码监听 http://[::1]:10202，就可以不用任何修改继续运行，就像直接上报到 http://[::1]:10202 一样。

*注意：转发功能将传递头部中的 `X-Signature` 字段，也就是相当于使用相同的 Secret。不支持直接通过上报数据进行响应（请访问 API 以响应上报）。*

## Nuget 包
Nuget 包[在此](https://www.nuget.org/packages/Sisters.WudiLib/)。

注意：虽然本项目已经几近完成，但依然有少量细节需要完善。目前建议您直接添加项目引用，这样您就可以按自己的需要修改代码。此外，等到本项目功能添加得差不多的时候，可能将迎来一个不兼容的新版本，以提高使用体验，暂定版本号为 0.1.x（目前是 0.0.x）。0.0.x 在此之后应该会继续维护一段时间。*新版本计划咕咕了，请继续使用此版本。*

## 小建议
由于 `Sisters.WudiLib.Message` 和 `Sisters.WudiLib.Posts.Message` 类的类名相同（设计问题，以后会改），使用起来有诸多不便，建议您在每个**新**代码文件开头添加下列 `using`：
```C#
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;
```
这样，就可以用 `MessageContext` 表示收到的消息上报，用 `Message` 表示要发送的消息了。

## 更新日志（部分）
### 0.0.2
- 优化了多次获取 `ReceivedMessage.Sections` 属性时的性能。
- 增加 `ReceivedMessage.TryGetPlainText(out string text)` 方法
- 现在转发时将保持头部中的 `X-Signature` 不变。

### 0.0.3（尚未发布）
#### API
- 将 API 访问网络的方法移到 `HttpApiClient` 类，并提取 `CallRawAsync()` 方法，方便重载。
- 增加 `HttpApiClient.CallAsync()` 方法，方便调用本类库没提供的 API。可以使用继承或扩展方法的方式封装。
#### `Section`
- 为 `Section` 增加 `Raw` 属性，方便查看其字符串形式。
- 为 `Section` 增加几个构造方法，方便构造自己的消息段。
- 修复 `Section` 在参数顺序不同时 `GetHashCode()` 返回不同结果的 bug。现在参数顺序不分先后。*潜在可能影响现有代码行为，但目前未找到真正有影响的情况。*
#### `Message` 及其子类
- `Message` 类的构造方法改为 `protected`，`Serializing` 属性改为 `protected internal abstract`（原来是 `internal abstract`）。所以你可以继承并实现自己的 Message。
- 修改了现有的 `Message` 子类 `Serializing` 属性的修饰符，以配合上一项。
- 为 `SendingMessage` 增加 `Sections` 属性，返回类型为 `IReadOnlyList<Section>`。
#### `Message` 及其子类不兼容的更改 **(注意：不向后兼容)**
- 修改了 `SectionMesssage` 的大部分内容。现在 `SectionMessage` 的构造方法均被限制，也就是说，无法从外部继承。可能在以后重新开放。
#### 框架
- 支持 .NET Framework 4.5。