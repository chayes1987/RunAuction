Imports DreamSeat
Imports ZeroMQ
Imports ZeroMQ.Sockets
Imports System.Threading


Public Module RunAuction
    Private context As IZmqContext = ZmqContext.Create()
    Private auctionStartedAck, bidPlacedAck, auctionEventPub As ISendSocket
    Private auctionRunning As Boolean = False

    Public Sub Main()
        InitializePublishers()
        Dim subToAuctionStartedThread As New Thread(AddressOf SubscribeToAuctionStarted)
        subToAuctionStartedThread.Start()
        Dim subToBidPlacedThread As New Thread(AddressOf SubscribeToBidPlaced)
        subToBidPlacedThread.Start()
        Dim subToBidPlacedAckThread As New Thread(AddressOf SubscribeToBidChangedAck)
        subToBidPlacedAckThread.Start()
        Dim subToAuctionOverAckThread As New Thread(AddressOf SubscribeToAuctionOverAck)
        subToAuctionOverAckThread.Start()
        Dim subToAuctionRunningAckThread As New Thread(AddressOf SubscribeToAuctionRunningAck)
        subToAuctionRunningAckThread.Start()
    End Sub

    Private Sub InitializePublishers()
        auctionStartedAck = context.CreatePublishSocket()
        auctionStartedAck.Bind(Constants.AUCTION_STARTED_ACK_ADR)
        bidPlacedAck = context.CreatePublishSocket()
        bidPlacedAck.Bind(Constants.BID_PLACED_ACK_ADR)
        auctionEventPub = context.CreatePublishSocket()
        auctionEventPub.Bind(Constants.AUCTION_EVENT_PUB_ADR)
    End Sub

    Private Sub SubscribeToAuctionStarted()
        Dim auctionStartedSub As ISubscribeSocket = context.CreateSubscribeSocket()
        auctionStartedSub.Connect(Constants.AUCTION_STARTED_SUB_ADR)
        auctionStartedSub.Subscribe(System.Text.Encoding.ASCII.GetBytes(Constants.AUCTION_STARTED_TOPIC))
        Console.WriteLine("SUB: " & Constants.AUCTION_STARTED_TOPIC)

        While (True)
            Dim auctionStartedEvt As String = System.Text.Encoding.ASCII.GetString(auctionStartedSub.Receive())
            Console.WriteLine("REC: " & auctionStartedEvt)
            PublishAuctionStartedAck(auctionStartedEvt)
            Dim id As String = ParseMessage(auctionStartedEvt, "<id>", "</id>")
            PublishAuctionRunningEvent(id)
            Thread.Sleep(Constants.WAIT_TIME)
            Dim runAuctionThread As New Thread(AddressOf RunAuction)
            runAuctionThread.Start(id)
        End While
    End Sub

    Private Sub PublishAuctionRunningEvent(ByVal id As String)
        Dim auctionRunningEvt As String = String.Concat("AuctionRunning <id>", id, "</id>")
        auctionEventPub.Send(System.Text.Encoding.ASCII.GetBytes(auctionRunningEvt))
        Console.WriteLine("PUB: " & auctionRunningEvt)
    End Sub

    Private Sub RunAuction(ByVal id As String)
        Dim client As New CouchClient()
        Dim db As CouchDatabase = client.GetDatabase(Constants.DB_NAME)
        Dim document As AuctionItem = db.GetDocument(Of AuctionItem)(id)
        Dim currentBid As Double = document.Starting_Bid
        auctionRunning = True

        While (auctionRunning)
            Thread.Sleep(Constants.WAIT_TIME)

            If (currentBid > document.Estimated_Value) Then
                currentBid -= Constants.DECREMENT_AMOUNT
                Dim bidChangedEvt As String = String.Concat("BidChanged <id>", id, "</id> ", "<params>", currentBid.ToString, "</params>")
                auctionEventPub.Send(System.Text.Encoding.ASCII.GetBytes(bidChangedEvt))
                Console.WriteLine("PUB: " & bidChangedEvt)
            Else
                Dim auctionOverEvt As String = String.Concat("AuctionOver <id>", id, "</id>", " <params>No Winner</params>")
                auctionEventPub.Send(System.Text.Encoding.ASCII.GetBytes(auctionOverEvt))
                Console.WriteLine("PUB: " & auctionOverEvt)
                auctionRunning = False
                Exit Sub
            End If
        End While
    End Sub

    Private Function ParseMessage(ByVal message As String, ByVal startTag As String, ByVal endTag As String) As String
        Dim startIndex As Integer = message.IndexOf(startTag) & startTag.Length
        Dim substring As String = message.Substring(startIndex)
        Return substring.Substring(0, substring.LastIndexOf(endTag))
    End Function

    Private Sub PublishAuctionStartedAck(ByVal message As String)
        auctionStartedAck.Send(System.Text.Encoding.ASCII.GetBytes("ACK: " & message))
        Console.WriteLine("ACK SENT...")
    End Sub

    Private Sub PublishBidPlacedAck(ByVal message As String)
        bidPlacedAck.Send(System.Text.Encoding.ASCII.GetBytes("ACK: " & message))
        Console.WriteLine("ACK SENT...")
    End Sub

    Private Sub SubscribeToBidPlaced()
        Dim subscriber As ISubscribeSocket = context.CreateSubscribeSocket()
        subscriber.Connect(Constants.BID_PLACED_ADR)
        subscriber.Subscribe(System.Text.Encoding.ASCII.GetBytes(Constants.BID_PLACED_TOPIC))
        Console.WriteLine("SUB: " & Constants.BID_PLACED_TOPIC)

        While (True)
            Dim bidPlacedEvt As String = System.Text.Encoding.ASCII.GetString(subscriber.Receive())
            Console.WriteLine("REC: " + bidPlacedEvt)
            auctionRunning = False
            PublishBidPlacedAck(bidPlacedEvt)
            Dim id As String = ParseMessage(bidPlacedEvt, "<id>", "</id>")
            Dim bidderEmail As String = ParseMessage(bidPlacedEvt, "<params>", "</params>")
            Dim auctionOverEvt As String = String.Concat("AuctionOver <id>", id, "</id> ", "<params>", bidderEmail, "</params>")
            auctionEventPub.Send(System.Text.Encoding.ASCII.GetBytes(auctionOverEvt))
        End While
    End Sub

    Private Sub SubscribeToBidChangedAck()
        Dim bidChangedAckSub As ISubscribeSocket = context.CreateSubscribeSocket()
        bidChangedAckSub.Connect(Constants.BID_CHANGED_ACK_ADR)
        bidChangedAckSub.Subscribe(System.Text.Encoding.ASCII.GetBytes(Constants.BID_CHANGED_ACK_TOPIC))

        While (True)
            Console.WriteLine(System.Text.Encoding.ASCII.GetString(bidChangedAckSub.Receive()))
        End While
    End Sub

    Private Sub SubscribeToAuctionOverAck()
        Dim auctionOverAckSub As ISubscribeSocket = context.CreateSubscribeSocket()
        auctionOverAckSub.Connect(Constants.AUCTION_OVER_ACK_ADR)
        auctionOverAckSub.Subscribe(System.Text.Encoding.ASCII.GetBytes(Constants.AUCTION_OVER_ACK_TOPIC))

        While (True)
            Console.WriteLine(System.Text.Encoding.ASCII.GetString(auctionOverAckSub.Receive()))
        End While
    End Sub

    Private Sub SubscribeToAuctionRunningAck()
        Dim auctionRunningAckSub As ISubscribeSocket = context.CreateSubscribeSocket()
        auctionRunningAckSub.Connect(Constants.AUCTION_RUNNING_ACK_ADR)
        auctionRunningAckSub.Subscribe(System.Text.Encoding.ASCII.GetBytes(Constants.AUCTION_RUNNING_ACK_TOPIC))

        While (True)
            Console.WriteLine(System.Text.Encoding.ASCII.GetString(auctionRunningAckSub.Receive()))
        End While
    End Sub

End Module
