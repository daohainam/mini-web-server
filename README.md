# Mini-Web-Server
**(English below)**

Trang web demo chạy trên Mini-Web-Server: https://www.mini-web-server.com

Chào mừng đến với Mini-Web-Server, dự án được tạo với mục đích giúp các junior nâng cấp lên thành senior!

Mini-Web-Server, gọi tắt là Mini là một máy chủ web, với các tính năng:
- Hiệu năng cao, sử dụng bộ nhớ hiệu quả.
- Hỗ trợ multihost, cho phép cung cấp nội dung khác nhau đến các domain khác nhau.
- Hỗ trợ HTTPS, HTTP/1.1, HTTP/2 và WebSocket.
- Dễ dàng mở rộng tính năng qua cơ chế Middleware.
- Cho phép nhúng vào các ứng dụng khác một cách dễ dàng.
- Hỗ trợ Authorization, Session, Hsts, Https redirection, Mvc, caching...
- Cung cấp các API cho phép phát triển các ứng dụng dựa trên các handler đơn giản hoặc MVC (gọi là các MiniApp).

# Tổng quan về dự án
- Dự án nhắm đến .NET 10 và cần .NET 10 SDK để build.
- Sử dụng tối thiểu các thư viện bên ngoài, kể cả các thư viện hỗ trợ HTTP từ .NET SDK.
- Dự án được đánh dấu qua các [tags](https://github.com/daohainam/mini-web-server/tags), giúp người đọc dễ dàng hơn khi tham khảo các tài nguyên.
- Mục đích chính của dự án là tạo bộ học liệu để học về các chủ đề nâng cao (multithreading, OOAD, networking, HTTP protocol, design patterns...), tuy nhiên vẫn phải đủ mạnh và cung cấp đầy đủ tính năng để triển khai như một web server backend phía sau các reversed proxy.

## Bắt đầu
Từ thư mục gốc của repository, dùng .NET 10 SDK để restore, build và chạy các bài kiểm thử:

```sh
dotnet restore
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build
```

Chương trình mẫu nằm trong `MiniWebServer/`. Cấu hình mặc định lắng nghe tại `http://127.0.0.1:8080`; có thể chạy bằng:

```sh
dotnet run --project MiniWebServer/MiniWebServer.csproj
```

Sau khi khởi động, thử endpoint `http://127.0.0.1:8080/string-api/toupper?text=hello`.

## Cấu trúc repository
- `MiniWebServer.Abstractions/`, `MiniWebServer.Server.Abstractions/`: các abstraction cho HTTP và server.
- `MiniWebServer.HttpParser/`, `MiniWebServer.Server/`: phân tích giao thức, protocol handler và server.
- `MiniWebServer.MiniApp/`, `MiniWebServer.Mvc.Abstraction/`: API cho MiniApp và MVC.
- `Middleware/`: các middleware như Authentication, Authorization, MVC, Session, StaticFiles và WebSocket.
- `MiniWebServer/`: chương trình server mẫu và nội dung demo.
- `Tests/`: các project kiểm thử.
- [`mini-web-server-course/`](mini-web-server-course/index.html): khóa học tương tác giới thiệu cách một request đi qua server.

# Cấu trúc các thành phần trong solution:
Các dự án trong solution được chia thành các nhóm sau:
- Cung cấp các lớp trừu tượng cho giao thức HTTP và các thành phần liên quan (MiniWebServer.Abstractions).
- Cung cấp các lớp trừu tượng cho việc tổ chức các thành phần bên trong server (MiniWebServer.Server.Abstractions).
- Cung cấp các trình xử lý dòng dữ liệu (protocol handler) và tạo ra các request, response (MiniWebServer.HttpParser, MiniWebServer.Server/ProtocolHandlers).
- Cung cấp các lớp trừu tượng và các mô hình dữ liệu cho các API (API cung cấp bởi Mini-Web-Server) (MiniWebServer.MiniApp).
- Server để kết nối mọi thứ lại (quản lý các connection, gọi các protocol handler, kết nối các request/response, tìm các trình xử lý request, xây dựng chuỗi middleware, gọi các middleware và trả response về cho protocol handler) (MiniWebServer.Server).
- Các lớp tiện ích (MimeMapping, MiniWebServer.Configuration, MiniWebServer.Helpers).
- Các middleware chuẩn (trong thư mục Middleware).
- Chương trình mẫu (MiniWebServer)

# Một vài lưu ý khi đọc code:
- Nhiều interface được tạo ra nhằm tạo một lớp(layer) trừu tượng trên các lớp cụ thể, nhờ vậy sẽ giúp giảm phụ thuộc giữa các lớp, dễ dàng viết các unit test hoặc nâng cấp, chỉnh sửa khi cần.
- Các đối tượng phức tạp thường được tạo bằng cách dùng Builder pattern [^builder-pattern] (MiniWebServerBuilder, HttpWebRequestBuilder, HttpWebResponseBuilder, MiniAppBuilder...)).
- Các thành phần bên trong lớp luôn là private hoặc readonly nếu có thể, dữ liệu càng ít bị thay đổi, cơ hội mắc lỗi càng giảm xuống. Các bạn sẽ thấy số lượng các biến hoặc property là readonly có thể còn nhiều hơn các biến còn lại, khi cần thay đổi tôi thường ưu tiên tạo một đối tượng mới với các giá trị mới hơn là thay đổi thuộc tính của một object cũ. Điều này về lý thuyết kém hiệu quả hơn vì việc cấp phát và giải phóng các đối tượng xảy ra thường xuyên hơn, tuy nhiên đối với các đối tượng được sử dụng nhiều, ta sẽ dùng các resource pool, do vậy sẽ không ảnh hưởng đến nhiều đến hiệu năng.
- Trong những phần đòi hỏi hiệu năng cao (các phần liên quan đến socket và phân tích chuỗi dữ liệu), việc cấp phát và giải phóng dữ liệu sẽ được làm thông qua các resource pool, và làm việc trực tiếp trên các [Span](https://learn.microsoft.com/en-us/dotnet/api/system.span-1?view=net-10.0), bạn có thể tham khảo các lớp [ByteSequenceHttpParser](MiniWebServer.HttpParser/Http11/ByteSequenceHttpParser.cs) hoặc [Http11ProtocolHandler](MiniWebServer.Server/ProtocolHandlers/Http11/Http11ProtocolHandler.cs). Bạn có thể xem cách dùng các resource pool trong [FileContent](MiniWebServer.MiniApp/Content/FileContent.cs) hoặc [SessionIdGenerator](Middleware/Session/SessionIdGenerator.cs).
- Các tài nguyên có thể được phục vụ cho client được cung cấp thông qua [ICallable](MiniWebServer.MiniApp/ICallable.cs), bao gồm cả các tài nguyên tĩnh và động. Nhờ vậy chúng ta có thể mở rộng đến bất kỳ dạng tài nguyên và phương thức nào.
- Bất kỳ lỗi nào xảy ra khi đọc và phân tích request cũng đều dẫn đến lỗi 400 Bad Request.
- Một request gửi lên nhưng không tìm được request handler tương ứng sẽ dẫn đến lỗi 404 Not Found (dù trong đa số trường hợp đây là lỗi viết code nhưng nếu trả về 500 Internal Server Error sẽ làm người dùng khó hiểu).
- Bất kỳ lỗi nào xảy ra trong quá trình xử lý ngoài hai lỗi trên đều dẫn đến lỗi 500 Internal Server Error.

# Các luồng xử lý quan trọng:
## Khi một client kết nối đến server:
- Hàm [HandleNewClientConnectionAsync](MiniWebServer.Server/MiniWebServer.cs) được gọi với tham số là clientId và Socket (TcpClient).
- Nếu được cấu hình sử dụng HTTPS, client stream sẽ được 'wrap' lại bởi một SslStream.
- Một đối tượng [MiniWebClientConnection](MiniWebServer.Server/MiniWebClientConnection.cs) được tạo ra với các tham số cần thiết bao gồm client stream.
- Hàm [HandleRequestAsync](MiniWebServer.Server/MiniWebClientConnection.cs) được gọi để xử lý dữ liệu từ client.
- Các pipeline [^pipe-line] cho request và response được tạo ra. Dữ liệu từ client gửi lên có thể rất lớn, có thể rời rạc và có thể không hợp lệ, việc dùng các pipeline sẽ giúp ta xử lý dữ liệu ngay trên bộ đệm, và dịch chuyển 'cửa sổ' bộ đệm để đọc dữ liệu hiệu quả hơn.
- Vòng lặp sau được thực hiện: ReadRequestAsync() -> request = requestBuilder.Build() -> MiniApp app = FindApp(request) -> ReadBodyAsync() chạy đồng thời với CallByMethod(), có nghĩa là việc thực thi request sẽ được thực hiện ngay khi đọc xong request header. ReadBodyAsync đưa dữ liệu từ socket vào vùng đệm (và dừng lại khi bộ đệm đầy), nếu trong lúc thực thi MiniApp yêu cầu đọc request body, chúng ta sẽ lấy từ bộ đệm đó, khi đó bộ đệm được làm trống và ReadBodyAsync sẽ lại tiếp tục đưa dữ liệu từ socket vào bộ đệm. Giải pháp này giúp chúng ta không tốn tài nguyên xử lý body request nếu MiniApp không yêu cầu, cũng như ta có thể chờ đến khi client gửi xong body request mới tiếp tục thực thi MiniApp. Sau khi MiniApp thực thi xong, ta sẽ tạo response = responseBuilder.Build() và gửi về client bằng [SendResponseAsync](MiniWebServer.Server/MiniWebClientConnection.cs).


[**English**]
# Mini-Web-Server
A demo web site running on Mini-Web-Server can be found at: https://www.mini-web-server.com

Welcome to Mini-Web-Server, a project created with the purpose of helping junior developers upgrade to seniors!

Mini-Web-Server - aka Mini, is a web server, with many features:
- High performance, memory use optimized.
- Multi host supported, allowing to serve different contents to different domains.
- Supports HTTPS, HTTP/1.1, HTTP/2, and WebSockets.
- Easy to add more features, thanks to Middleware support.
- Easy to embed to other apps.
- Support Authorization, Session, Hsts, Https redirection, Mvc, caching...
- Support writing MiniApp or Mvc app to serve dynamic content. (similar to ASP.NET apps)

# Project Overview
- Targets .NET 10; the .NET 10 SDK is required to build the project.
- Minimize using 3rd party libraries, including standard HTTP libraries from .NET SDK.
- Marked with [tags](https://github.com/daohainam/mini-web-server/tags) to make it easier for readers to reference the resources. 
- The main purpose of the project is to create a learning resource to study advanced topics, such as multithreading, OOAD, networking, HTTP protocol, design patterns... However, it must be robust and feature-complete enough to be deployed as a web server backend behind reverse proxies.

## Getting started
From the repository root, restore dependencies, build, and run the test suite with the .NET 10 SDK:

```sh
dotnet restore
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build
```

The sample server is in `MiniWebServer/`. Its default configuration listens at `http://127.0.0.1:8080`; start it with:

```sh
dotnet run --project MiniWebServer/MiniWebServer.csproj
```

Then try `http://127.0.0.1:8080/string-api/toupper?text=hello`.

## Repository layout
- `MiniWebServer.Abstractions/`, `MiniWebServer.Server.Abstractions/`: HTTP and server abstractions.
- `MiniWebServer.HttpParser/`, `MiniWebServer.Server/`: protocol parsing, protocol handlers, and the server.
- `MiniWebServer.MiniApp/`, `MiniWebServer.Mvc.Abstraction/`: MiniApp and MVC APIs.
- `Middleware/`: middleware such as Authentication, Authorization, MVC, Session, StaticFiles, and WebSocket.
- `MiniWebServer/`: sample server and demo content.
- `Tests/`: test projects.
- [`mini-web-server-course/`](mini-web-server-course/index.html): an interactive course following a request through the server.

# Tham khảo/References
[^builder-pattern]: https://refactoring.guru/design-patterns/builder
[^pipe-line]: https://learn.microsoft.com/en-us/dotnet/standard/io/pipelines
