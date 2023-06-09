# Mini-Web-Server
(tài liệu này được viết dựa trên phiên bản [0.2.5](https://github.com/daohainam/mini-web-server/tree/112ac1111b2f955289a94ac8692d5d7b24994c2f))

Chào mừng đến với Mini-Web-Server, dự án được tạo với mục đích giúp các junior nâng cấp lên thành senior!

Mini-Web-Server, gọi tắt là Mini là một máy chủ web, với các tính năng:
- Hiệu năng cao, sử dụng bộ nhớ hiệu quả.
- Hỗ trợ multihost, cho phép cung cấp nội dung khác nhau đến các domain khác nhau.
- Hỗ trợ HTTPS, HTTP 1.1 và cung cấp khả năng nâng thêm lên WebSocket, HTTP/2, HTTP/3.
- Dễ dàng mở rộng tính năng qua cơ chế Middleware.
- Cho phép nhúng vào các ứng dụng khác một cách dễ dàng.
- Cung cấp các API cho phép phát triển các ứng dụng dựa trên các handler đơn giản hoặc MVC (gọi là các MiniApp).
- ...

# Tổng quan về dự án
- Dự án được phát triển trên .NET 7, có thể chạy trên tất cả các nền tảng mà .NET 7 hỗ trợ.
- Sử dụng tối thiểu các thư viện bên ngoài, kể cả các thư viện hỗ trợ HTTP từ .NET SDK.
- Dự án được đánh dấu qua các [tags](https://github.com/daohainam/mini-web-server/tags), giúp người đọc dễ dàng hơn khi tham khảo các tài nguyên.
- Mục đích chính của dự án là tạo bộ học liệu để học về các chủ đề nâng cao (multithreading, OOAD, networking, HTTP protocol, design patterns...), tuy nhiên vẫn phải đủ mạnh và cung cấp đầy đủ tính năng để triển khai như một web server backend phía sau các reversed proxy.

# Cấu trúc các thành phần trong solution:
Các dự án trong solution được chia thành các nhóm sau:
- Cung cấp các lớp trừu tượng cho giao thức HTTP và các thành phần liên quan (MiniWebServer.Abstractions).
- Cung cấp các lớp trừu tượng cho việc tổ chức các thành phần bên trong server (MiniWebServer.Server.Abstractions).
- Cung cấp các trình xử lý dòng dữ liệu (protocol handler) và tạo ra các request, response (MiniWebServer.HttpParser, MiniWebServer.Server/ProtocolHandlers
/Http11).
- Cung cấp các lớp trừu tượng và các mô hình dữ liệu cho các API (API cung cấp bởi Mini-Web-Server) (MiniWebServer.MiniApp).
- Server để kết nối mọi thứ lại (quản lý các connection, gọi các protocol handler, kết nối các request/response, tìm các trình xử lý request, xây dựng chuỗi middleware, gọi các middleware và trả response về cho protocol handler) (MiniWebServer.Server).
- Các lớp tiện ích (MimeMapping, MiniWebServer.Configuration, MiniWebServer.Helpers).
- Các middleware chuẩn (trong thư mục Middleware).
- Chương trình mẫu (MiniWebServer)

# Một vài lưu ý khi đọc code:
- Nhiều interface được tạo ra nhằm tạo một lớp(layer) trừu tượng trên các lớp cụ thể, nhờ vậy sẽ giúp giảm phụ thuộc giữa các lớp, dễ dàng viết các unit test hoặc nâng cấp, chỉnh sửa khi cần.
- Các đối tượng phức tạp thường được tạo bằng cách dùng Builder pattern [^builder-pattern] (MiniWebServerBuilder, HttpWebRequestBuilder, HttpWebResponseBuilder, MiniAppBuilder...)).
- Các thành phần bên trong lớp luôn là private hoặc readonly nếu có thể, dữ liệu càng ít bị thay đổi, cơ hội mắc lỗi càng giảm xuống. Các bạn sẽ thấy số lượng các biến hoặc property là readonly có thể còn nhiều hơn các biến còn lại, khi cần thay đổi tôi thường ưu tiên tạo một đối tượng mới với các giá trị mới hơn là thay đổi thuộc tính của một object cũ. Điều này về lý thuyết kém hiệu quả hơn vì việc cấp phát và giải phóng các đối tượng xảy ra thường xuyên hơn, tuy nhiên đối với các đối tượng được sử dụng nhiều, ta sẽ dùng các resource pool, do vậy sẽ không ảnh hưởng đến nhiều đến hiệu năng.
- Trong những phần đòi hỏi hiệu năng cao (các phần liên quan đến socket và phân tích chuỗi dữ liệu), việc cấp phát và giải phóng dữ liệu sẽ được làm thông qua các resource pool, và làm việc trực tiếp trên các [Span](https://learn.microsoft.com/en-us/dotnet/api/system.span-1?view=net-7.0), bạn có thể tham khảo các lớp [ByteSequenceHttpParser](https://github.com/daohainam/mini-web-server/blob/112ac1111b2f955289a94ac8692d5d7b24994c2f/MiniWebServer.HttpParser/Http11/ByteSequenceHttpParser.cs) hoặc [Http11IProtocolHandler](https://github.com/daohainam/mini-web-server/blob/112ac1111b2f955289a94ac8692d5d7b24994c2f/MiniWebServer.Server/ProtocolHandlers/Http11/Http11IProtocolHandler.cs). Bạn có thể xem cách dùng các resource pool trong [FileContent](https://github.com/daohainam/mini-web-server/blob/112ac1111b2f955289a94ac8692d5d7b24994c2f/MiniWebServer.MiniApp/Content/FileContent.cs) hoặc [SessionIdGenerator](https://github.com/daohainam/mini-web-server/blob/112ac1111b2f955289a94ac8692d5d7b24994c2f/Middleware/Session/SessionIdGenerator.cs).
- Các tài nguyên có thể được phục vụ cho client được cung cấp thông qua [ICallable](https://github.com/daohainam/mini-web-server/blob/112ac1111b2f955289a94ac8692d5d7b24994c2f/MiniWebServer.MiniApp/ICallable.cs), bao gồm cả các tài nguyên tĩnh và động. Nhờ vậy chúng ta có thể mở rộng đến bất kỳ dạng tài nguyên và phương thức nào.
- Bất kỳ lỗi nào xảy ra khi đọc và phân tích request cũng đều dẫn đến lỗi 400 Bad Request.
- Một request gửi lên nhưng không tìm được request handler tương ứng sẽ dẫn đến lỗi 404 Not Found (dù trong đa số trường hợp đây là lỗi viết code nhưng nếu trả về 500 Internal Server Error sẽ làm người dùng khó hiểu).
- Bất kỳ lỗi nào xảy ra trong quá trình xử lý ngoài hai lỗi trên đều dẫn đến lỗi 500 Internal Server Error.

# Các luồng xử lý quan trọng:
## Khi một client kết nối đến server:
- Hàm [HandleNewClientConnectionAsync](https://github.com/daohainam/mini-web-server/blob/112ac1111b2f955289a94ac8692d5d7b24994c2f/MiniWebServer.Server/MiniWebServer.cs#L51) được gọi với tham số là clientId và Socket (TcpClient). 
- Nếu được cấu hình sử dụng HTTPS, client stream sẽ được 'wrap' lại bởi một SslStream.
- Một đối tượng [MiniWebClientConnection](https://github.com/daohainam/mini-web-server/blob/112ac1111b2f955289a94ac8692d5d7b24994c2f/MiniWebServer.Server/MiniWebClientConnection.cs#L21) được tạo ra với các tham số cần thiết bao gồm client stream.
- Hàm [HandleRequestAsync](https://github.com/daohainam/mini-web-server/blob/112ac1111b2f955289a94ac8692d5d7b24994c2f/MiniWebServer.Server/MiniWebClientConnection.cs#L46) được gọi để xử lý dữ liệu từ client.
- Các pipeline [^pipe-line] cho request và response được tạo ra. Dữ liệu từ client gửi lên có thể rất lớn, có thể rời rạc và có thể không hợp lệ, việc dùng các pipeline sẽ giúp ta xử lý dữ liệu ngay trên bộ đệm, và dịch chuyển 'cửa sổ' bộ đệm để đọc dữ liệu hiệu quả hơn.
- Vòng lặp sau được thực hiện: ReadRequestAsync() -> request = requestBuilder.Build() -> MiniApp app = FindApp(request) -> ReadBodyAsync() chạy đồng thời với CallByMethod(), có nghĩa là việc thực thi request sẽ được thực hiện ngay khi đọc xong request header. ReadBodyAsync đưa dữ liệu từ socket vào vùng đệm (và dừng lại khi bộ đệm đầy), nếu trong lúc thực thi MiniApp yêu cầu đọc request body, chúng ta sẽ lấy từ bộ đệm đó, khi đó bộ đệm được làm trống và ReadBodyAsync sẽ lại tiếp tục đưa dữ liệu từ socket vào bộ đệm. Giải pháp này giúp chúng ta không tốn tài nguyên xử lý body request nếu MiniApp không yêu cầu, cũng như ta có thể chờ đến khi client gửi xong body request mới tiếp tục thực thi MiniApp. Sau khi MiniApp thực thi xong, ta sẽ tạo response = responseBuilder.Build() và gửi về client bằng [SendResponseAsync](https://github.com/daohainam/mini-web-server/blob/112ac1111b2f955289a94ac8692d5d7b24994c2f/MiniWebServer.Server/MiniWebClientConnection.cs#L139).

# Tham khảo
[^builder-pattern]: https://refactoring.guru/design-patterns/builder
[^pipe-line]: https://learn.microsoft.com/en-us/dotnet/standard/io/pipelines
