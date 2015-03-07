Imports ZeroMQ
Imports ZeroMQ.Sockets
Imports System.Threading
Imports System.Configuration
Imports System.Text


Public Module RunAuction
    Private _context As IZmqContext = ZmqContext.Create()
    Private _auctionEventPub As ISendSocket
    Private _auctions As New Dictionary(Of String, AuctionRunner)


    Public Sub Main()
        _auctionEventPub = _context.CreatePublishSocket()
        _auctionEventPub.Bind(ConfigurationManager.AppSettings("auctionEventPubAddr"))
        InitializeSubscribers()
        SubscribeToAuctionStarted()
    End Sub

    Private Sub InitializeSubscribers()
        Dim subToBidPlacedThread As New Thread(AddressOf SubscribeToBidPlaced)
        subToBidPlacedThread.Start()
        Dim subToBidPlacedAckThread As New Thread(AddressOf SubscribeToBidChangedAck)
        subToBidPlacedAckThread.Start()
        Dim subToAuctionOverAckThread As New Thread(AddressOf SubscribeToAuctionOverAck)
        subToAuctionOverAckThread.Start()
        Dim subToAuctionRunningAckThread As New Thread(AddressOf SubscribeToAuctionRunningAck)
        subToAuctionRunningAckThread.Start()
    End Sub

    Private Sub SubscribeToAuctionStarted()
        Dim auctionStartedAck As ISendSocket = _context.CreatePublishSocket()
        auctionStartedAck.Bind(ConfigurationManager.AppSettings("auctionStartedAckAddr"))
        Dim auctionRunningPub As ISendSocket = _context.CreatePublishSocket()
        auctionRunningPub.Bind(ConfigurationManager.AppSettings("auctionRunningPubAddr"))

        Dim auctionStartedSub As ISubscribeSocket = _context.CreateSubscribeSocket()
        auctionStartedSub.Connect(ConfigurationManager.AppSettings("auctionStartedSubAddr"))
        Dim auctionStartedTopic As String = ConfigurationManager.AppSettings("auctionStartedTopic")
        auctionStartedSub.Subscribe(System.Text.Encoding.ASCII.GetBytes(auctionStartedTopic))
        Console.WriteLine("SUB: " & auctionStartedTopic)

        While (True)
            Dim auctionStartedEvt As String = Encoding.ASCII.GetString(auctionStartedSub.Receive())
            Console.WriteLine("REC: " & auctionStartedEvt)
            PublishAuctionStartedAck(auctionStartedAck, auctionStartedEvt)
            Dim id As String = ParseMessage(auctionStartedEvt, "<id>", "</id>")
            PublishAuctionRunningEvent(auctionRunningPub, id)
            Dim auctionRunner As New AuctionRunner(id, _auctionEventPub)
            _auctions.Add(id, auctionRunner)
            auctionRunner.RunAuction()
            'Dim runAuctionThread As New Thread(AddressOf _auctionRunner.RunAuction)
            'runAuctionThread.Start()
        End While
    End Sub

    Private Sub PublishAuctionStartedAck(ByVal auctionStartedAck As ISendSocket, ByVal message As String)
        Dim msg As String = "ACK " & message
        auctionStartedAck.Send(Encoding.ASCII.GetBytes(msg))
        Console.WriteLine("PUB: " & msg)
    End Sub

    Private Sub PublishAuctionRunningEvent(ByVal auctionRunningPub As ISendSocket, ByVal id As String)
        Dim auctionRunningEvt As String = String.Concat(ConfigurationManager.AppSettings("auctionRunningTopic"), " <id>", id, "</id>")
        auctionRunningPub.Send(Encoding.ASCII.GetBytes(auctionRunningEvt))
        Console.WriteLine("PUB: " & auctionRunningEvt)
    End Sub

    Public Function ParseMessage(ByVal message As String, ByVal startTag As String, ByVal endTag As String) As String
        Dim startIndex As Integer = message.IndexOf(startTag) + startTag.Length
        Dim substring As String = message.Substring(startIndex)
        Return substring.Substring(0, substring.LastIndexOf(endTag))
    End Function

    Private Sub SubscribeToAuctionRunningAck()
        Dim auctionRunningAckSub As ISubscribeSocket = _context.CreateSubscribeSocket()
        auctionRunningAckSub.Connect(ConfigurationManager.AppSettings("auctionRunningAckAddr"))
        auctionRunningAckSub.Subscribe(Encoding.ASCII.GetBytes(ConfigurationManager.AppSettings("auctionRunningAckTopic")))

        While (True)
            Console.WriteLine("REC: " & Encoding.ASCII.GetString(auctionRunningAckSub.Receive()))
        End While
    End Sub

    Private Sub PublishBidPlacedAck(ByVal bidPlacedAckPub As ISendSocket, ByVal message As String)
        Dim bidPlacedAck As String = "ACK " & message
        bidPlacedAckPub.Send(Encoding.ASCII.GetBytes(bidPlacedAck))
        Console.WriteLine("PUB: " & bidPlacedAck)
    End Sub

    Private Sub SubscribeToBidPlaced()
        Dim subscriber As ISubscribeSocket = _context.CreateSubscribeSocket()
        subscriber.Connect(ConfigurationManager.AppSettings("bidPlacedAddr"))
        Dim bidPlacedTopic As String = ConfigurationManager.AppSettings("bidPlacedTopic")
        subscriber.Subscribe(Encoding.ASCII.GetBytes(bidPlacedTopic))
        Console.WriteLine("SUB: " & bidPlacedTopic)

        Dim bidPlacedAck As ISendSocket = _context.CreatePublishSocket()
        bidPlacedAck.Bind(ConfigurationManager.AppSettings("bidPlacedAckAddr"))

        While (True)
            Dim bidPlacedEvt As String = Encoding.ASCII.GetString(subscriber.Receive())
            Console.WriteLine("REC: " & bidPlacedEvt)
            PublishBidPlacedAck(bidPlacedAck, bidPlacedEvt)
            Dim id As String = ParseMessage(bidPlacedEvt, "<id>", "</id>")
            Dim winner As String = ParseMessage(bidPlacedEvt, "<params>", "</params>")
            PublishAuctionOverEvt(id, winner)
        End While
    End Sub

    Private Sub SubscribeToBidChangedAck()
        Dim bidChangedAckSub As ISubscribeSocket = _context.CreateSubscribeSocket()
        bidChangedAckSub.Connect(ConfigurationManager.AppSettings("bidChangedAckAddr"))
        bidChangedAckSub.Subscribe(Encoding.ASCII.GetBytes(ConfigurationManager.AppSettings("bidChangedAckTopic")))

        While (True)
            Console.WriteLine("REC: " & Encoding.ASCII.GetString(bidChangedAckSub.Receive()))
        End While
    End Sub

    Private Sub SubscribeToAuctionOverAck()
        Dim auctionOverAckSub As ISubscribeSocket = _context.CreateSubscribeSocket()
        auctionOverAckSub.Connect(ConfigurationManager.AppSettings("auctionOverAckAddr"))
        auctionOverAckSub.Subscribe(System.Text.Encoding.ASCII.GetBytes(ConfigurationManager.AppSettings("auctionOverAckTopic")))

        While (True)
            Dim message As String = Encoding.ASCII.GetString(auctionOverAckSub.Receive())
            Dim id As String = ParseMessage(message, "<id>", "</id>")
            Console.WriteLine("REC: " & message)
        End While
    End Sub

    Public Sub PublishBidChangedEvt(ByVal id As String, ByVal currentBid As String)
        Dim bidChangedEvt As String = String.Concat(ConfigurationManager.AppSettings("bidChangedTopic"), " <id>", id,
                        "</id> ", "<params>", currentBid, "</params>")
        _auctionEventPub.Send(Encoding.ASCII.GetBytes(bidChangedEvt))
        Console.WriteLine("PUB: " & bidChangedEvt)
    End Sub

    Public Sub PublishAuctionOverEvt(ByVal id As String, ByVal winner As String)
        Dim auctionOverEvt As String = String.Concat(ConfigurationManager.AppSettings("auctionOverTopic"), " <id>", id,
                    "</id>", " <params>", winner, "</params>")
        _auctionEventPub.Send(Encoding.ASCII.GetBytes(auctionOverEvt))
        Console.WriteLine("PUB: " & auctionOverEvt)

        If (_auctions.ContainsKey(id)) Then
            _auctions(id).BidNotPlaced = False
            _auctions.Remove(id)
        End If
    End Sub

End Module
