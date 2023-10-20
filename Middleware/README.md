# Middleware

Một middleware là một class implement interface [MiniWebServer.MiniApp.IMiddleware](https://github.com/daohainam/mini-web-server/blob/master/MiniWebServer.MiniApp/IMiddleware.cs), và sẽ được đưa vào một chuỗi middleware (middleware chain), khi một middleware được thêm vào, nó sẽ trở thành đầu vào (entry point) của chuỗi.
Khi Mini-Web-Server tạo xong một đối tượng context (IMiniAppContext, chứa request, response và các thông tin khác), nó sẽ gọi đến hàm InvokeAsync của entry point, khi gọi nó sẽ truyền vào 3 tham số:
- [IMiniAppContext](https://github.com/daohainam/mini-web-server/blob/master/MiniWebServer.MiniApp/IMiniAppContext.cs) context: đối tượng chứa thông tin về lời gọi.
- [ICallable](https://github.com/daohainam/mini-web-server/blob/master/MiniWebServer.MiniApp/ICallable.cs) next: đối tượng kế tiếp trong chuỗi middleware. 
- [CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken?view=net-7.0) cancellationToken: một cancellationToken cho phép server dừng xử lý request, server sẽ yêu cầu hủy khi nó không có nhu cầu tiếp tục xử lý nữa (request timeout, connection bị đóng, xảy ra lỗi ở một phần nào đó...).

![image](https://github.com/daohainam/mini-web-server/assets/11513104/6e6ba02f-c0c8-42dd-a3b1-ec23e7b933cb)
_Minh họa middleware chain._
- Các đường mũi tên liền biểu thị lời gọi đến các middle kế tiếp.
- Các đường mũi tên đứt khúc biểu thị việc middle trở về mà không gọi đến middleware kế tiếp trong chuỗi (khi gặp một điều kiện nào đó). 
- Trong hình minh họa này, Authentication middleware sẽ luôn gọi đến Authorization, bất kể nó nó thể xác minh được người dùng hay không, nó sẽ đơn giản là đặt người dùng ở trạng thái không xác minh thay vì ngừng middleware chain.
Việc kết thúc chuỗi middleware chỉ đơn giản là ngưng gọi đến cái kế tiếp, middleware hiện tại vẫn phải trở lại middleware trước đó gọi nó (hoặc về server nếu nó là middleware đầu tiên trong chuỗi - trong hình minh họa là Rate Limit).

## Tham số next
Ta thấy ICallable được thiết kế như một phiên bản rút gọn của IMiddleware, hàm InvokeAsync của nó tương tự nhưng rút bớt tham số ICallable để gọi đến middleware kế tiếp. Từ góc độ thiết kế ta có thể coi một callable cũng là một middleware, chỉ khác là nó chỉ thực hiện một hành động nào đó, chứ không tiếp tục gọi đến các callable khác như với middleware. 
Thực chất các ICallable được truyền đến trong nhiều trường hợp chỉ là một wrapper của một middleware, ngoài trường hợp này ICallable có thể rơi vào một trong hai trường hợp:
- Nó là action delegate của một phương thức mà ra đã dùng hàm Map (hoặc một trong các biến thể của nó: MapGet, MapPost...). Trong tương lai tôi cũng muốn thiết kế lại để loại bỏ khái niệm action delegate, các action delegate cũng sẽ được phục vụ bởi một middleware.
- Nó chính là hàm InvokeAsync của server, với phương thức mặc nhiên cho phép trả về 404 Not Found. Hàm này luôn là hàm cuối cùng trong middleware chain (vì nó là hàm đầu tiên được thêm vào chuỗi), do vậy khi không có middleware nào ngừng gọi đến middleware kế tiếp, 404 Not Found sẽ là kết quả trả về cho client.

## Vì sao server không tự gọi lần lượt các middleware, thay vì để middleware tự gọi đến cái kế tiếp?
Ở đây ta muốn giao toàn quyền cho một middleware khi tới lượt nó xử lý, nó sẽ được quyết định kết quả của nó có phải là kết quả cuối cùng hay không, hoặc tiếp tục gọi.
Ví dụ nếu Mvc tìm thấy một route có khớp với một trong các action, nó sẽ gọi đến action đó, ngược lại, thay vì trả về 404 nó sẽ gọi đến middleware tiếp theo, đó có thể là StaticFiles middleware, và nó tìm thấy một file trùng với yêu cầu trong request, trong trường hợp này nó sẽ trả về nội dung file và kết thúc chuỗi xử lý bằng cách không gọi đến callable.
Từ đây ta thấy thứ tự thêm vào các middleware rất quan trọng, lấy ví dụ nếu bạn có một request đến "/css/styles.css", và đường dẫn này được tìm thấy bởi cả một file trên đĩa, lẫn một Mvc action (vì bạn có một class CssController, và một hàm action có RouteName attribute là "styles.css"), vậy tùy vào thứ tự bạn gọi UseMvc() và UseStaticFiles() mà kết quả nào sẽ được trả về:
- UseMvc(); UseStaticFiles(); => kết quả trả về sẽ là file trên đĩa.
- UseStaticFiles(); UseMvc(); => kết quả trả về sẽ là giá trị của hàm action.

_Note: middleware nào được thêm vào sau sẽ được gọi trước._

Một ví dụ khác là Authentication middleware và Authorization middleware: Authentication có nhiệm vụ xác định xem người dùng là ai, và tự động tạo một đối tượng User phù hợp trong IMiniAppContext, còn Authorization có nhiệm vụ xác định quyền truy cập vào một tài nguyên của một User. Nếu Authorization được gọi trước Authentication, khi đối tượng User còn chưa được khởi tạo, đồng nghĩa với việc người dùng ở trạng thái chưa đăng nhập, bạn sẽ luôn bị lỗi không đủ quyền truy xuất tài nguyên.

_Note: Trong ASP.NET, các hàm Build sẽ tự động sắp xếp lại thứ tự các middleware nên bạn sẽ không bị những lỗi như trên, trong Mini-Web-Server, tôi vẫn muốn để như vậy để giúp các bạn dễ hiểu hơn về cách mọi thứ bên dưới hoạt động (trong tương lai có thể thêm các lớp hỗ trợ tự động sắp xếp)._

Tất nhiên, ta vẫn hoàn toàn có thể thiết kế để server tự động gọi, và dựa trên kết quả trả về của callable.InvokeAsync() mà quyết định có gọi đến cái kế tiếp hay không, tuy nhiên khi đó ta sẽ phải xử lý thêm một chút ở phía server, và tôi muốn server càng đơn giản càng tốt, cái gì có thể đẩy về được cho các module mở rộng (middleware là một ví dụ) thì chúng ta sẽ ưu tiên giao cho chúng. Nếu các bạn đọc hàm CallByMethod trong MiniWebServer.Server.MiniWebClientConnection, các bạn sẽ thấy nó đơn giản như thế nào.

