Imports DreamSeat
Imports ZeroMQ
Imports ZeroMQ.Sockets
Imports System.Threading


Public Module RunAuction
    Private Const DB_NAME As String = "auctions"
    Private context As IZmqContext = ZmqContext.Create()
    Private auctionRunning As ISendSocket

    Public Sub Main()
        Dim client As New CouchClient()
        Dim db As CouchDatabase = client.GetDatabase(DB_NAME)

        Dim subscribeToAuctionStartedThread As New Thread(AddressOf SubscribeToAuctionStarted)
        subscribeToAuctionStartedThread.Start()
    End Sub

    Private Sub SubscribeToAuctionStarted()
        Dim subscriber As ISubscribeSocket = context.CreateSubscribeSocket()
        subscriber.Connect("tcp://127.0.0.1:1001")
        Dim prefix As Byte() = System.Text.Encoding.ASCII.GetBytes("AuctionStarted")
        subscriber.Subscribe(prefix)
        Console.WriteLine("Subscribed to AuctionStarted event...")

        While (True)
            Dim message As Byte() = subscriber.Receive()
            Dim receiveStr As String = System.Text.Encoding.ASCII.GetString(message)
            Console.WriteLine(receiveStr)
            Dim id As String = ParseMessage(receiveStr, "<id>", "</id>")
            Dim msg As Byte() = System.Text.Encoding.ASCII.GetBytes("AuctionRunning <id>" + id + "</id>")
        End While
    End Sub

    Private Function ParseMessage(ByVal message As String, ByVal startTag As String, ByVal endTag As String) As String
        Dim startIndex As Integer = message.IndexOf(startTag) + startTag.Length
        Dim substring As String = message.Substring(startIndex)
        Return substring.Substring(0, substring.LastIndexOf(endTag))
    End Function

End Module
