Imports DreamSeat
Imports ZeroMQ
Imports ZeroMQ.Sockets
Imports System.Threading


Public Module RunAuction
    Private Const DB_NAME As String = "auctions"
    Private context As IZmqContext = ZmqContext.Create()
    Private auctionRunning, bidChanged, auctionOver As ISendSocket
    Private Const DECREMENT_AMOUNT As Integer = 50
    Private auctionIsRunning As Boolean = False

    Public Sub Main()
        Dim subscribeToAuctionStartedThread As New Thread(AddressOf SubscribeToAuctionStarted)
        subscribeToAuctionStartedThread.Start()
        Dim subscribeToBidPlacedThread As New Thread(AddressOf SubscribeToBidPlaced)
        subscribeToBidPlacedThread.Start()
        auctionRunning = context.CreatePublishSocket()
        auctionRunning.Bind("tcp://127.0.0.1:1010")
        bidChanged = context.CreatePublishSocket()
        bidChanged.Bind("tcp://127.0.0.1:1011")
        auctionOver = context.CreatePublishSocket()
        auctionOver.Bind("tcp://127.0.0.1:1100")
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
            auctionRunning.Send(msg)
            Thread.Sleep(10000)
            Dim runAuctionThread As New Thread(AddressOf RunAuction)
            runAuctionThread.Start()
        End While
    End Sub

    Private Sub SubscribeToBidPlaced()
        Dim subscriber As ISubscribeSocket = context.CreateSubscribeSocket()
        subscriber.Connect("tcp://127.0.0.1:1101")
        Dim prefix As Byte() = System.Text.Encoding.ASCII.GetBytes("BidPlaced")
        subscriber.Subscribe(prefix)
        Console.WriteLine("Subscribed to BidPlaced event...")

        While (True)
            Dim message As Byte() = subscriber.Receive()
            Dim receiveStr As String = System.Text.Encoding.ASCII.GetString(message)
            Console.WriteLine("Received command " + receiveStr)
            auctionIsRunning = False
            Dim id As String = ParseMessage(receiveStr, "<id>", "</id>")
            Dim bidderEmail As String = ParseMessage(receiveStr, "<params>", "<params>")
            Dim msg As Byte() = System.Text.Encoding.ASCII.GetBytes("AuctionOver <id>" + id + "</id> " + "<params>" + bidderEmail + "</params>")
            auctionOver.Send(msg)
        End While
    End Sub

    Private Sub RunAuction(ByVal id As String)
        Dim client As New CouchClient()
        Dim db As CouchDatabase = client.GetDatabase(DB_NAME)
        Dim document As AuctionItem = db.GetDocument(Of AuctionItem)(id)

        Dim currentBid As Double = document.Starting_Bid
        auctionIsRunning = True

        While (auctionIsRunning)
            Thread.Sleep(10000)

            If (currentBid >= document.Estimated_Value) Then
                currentBid -= DECREMENT_AMOUNT
                bidChanged.Send(System.Text.Encoding.ASCII.GetBytes("CurrentBidChanged <id>" + id + "</id> " + "<params>" + currentBid.ToString + "</params>"))
            Else
                auctionOver.Send(System.Text.Encoding.ASCII.GetBytes("AuctionOver <id>" + id + "</id>" + "<params>No Winner<params>"))
                auctionIsRunning = False
                Exit Sub
            End If
        End While
    End Sub

    Private Function ParseMessage(ByVal message As String, ByVal startTag As String, ByVal endTag As String) As String
        Dim startIndex As Integer = message.IndexOf(startTag) + startTag.Length
        Dim substring As String = message.Substring(startIndex)
        Return substring.Substring(0, substring.LastIndexOf(endTag))
    End Function

End Module
