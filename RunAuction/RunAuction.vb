Imports ZeroMQ
Imports ZeroMQ.Sockets
Imports System.Threading
Imports System.Configuration
Imports System.Text

' Author - Conor Hayes
' Coding Standards -> https://msdn.microsoft.com/en-us/library/h63fsef3.aspx & Work Placement
' 0mq -> https://github.com/zeromq/netmq/

''' <summary>
''' Run Auction
''' </summary>
''' <remarks></remarks>
Public Module RunAuction

    ''' <summary>
    ''' Context
    ''' </summary>
    ''' <remarks></remarks>
    Private _context As IZmqContext = ZmqContext.Create()

    ''' <summary>
    ''' Publisher
    ''' </summary>
    ''' <remarks></remarks>
    Private _publisher As ISendSocket

    ''' <summary>
    ''' Auctions
    ''' </summary>
    ''' <remarks></remarks>
    Private _auctions As New Dictionary(Of String, AuctionRunner)


    ''' <summary>
    ''' Main
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Main()
        ' Create publisher and initialize subscribers
        _publisher = _context.CreatePublishSocket()
        _publisher.Bind(ConfigurationManager.AppSettings("auctionEventPubAddr"))
        InitializeSubscribers()
        SubscribeToAuctionStarted()
    End Sub

    ''' <summary>
    ''' Parse Message
    ''' </summary>
    ''' <param name="message">The message to parse</param>
    ''' <param name="startTag">The start delimiter</param>
    ''' <param name="endTag">The end delimiter</param>
    ''' <returns>The string required</returns>
    ''' <remarks></remarks>
    Public Function ParseMessage(ByVal message As String, ByVal startTag As String, ByVal endTag As String) As String
        Dim startIndex As Integer = message.IndexOf(startTag) + startTag.Length
        Dim substring As String = message.Substring(startIndex)
        Return substring.Substring(0, substring.LastIndexOf(endTag))
    End Function

#Region "Subscriber Code"

    ''' <summary>
    ''' Initialize Subscribers
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub InitializeSubscribers()
        ' Create all subscribers using threads
        Dim subToBidPlacedThread As New Thread(AddressOf SubscribeToBidPlaced)
        subToBidPlacedThread.Start()
        Dim subToBidPlacedAckThread As New Thread(AddressOf SubscribeToBidChangedAck)
        subToBidPlacedAckThread.Start()
        Dim subToAuctionOverAckThread As New Thread(AddressOf SubscribeToAuctionOverAck)
        subToAuctionOverAckThread.Start()
        Dim subToAuctionRunningAckThread As New Thread(AddressOf SubscribeToAuctionRunningAck)
        subToAuctionRunningAckThread.Start()
        Dim subToHeartbeatThread As New Thread(AddressOf SubscribeToHeartbeat)
        subToHeartbeatThread.Start()
    End Sub

    ''' <summary>
    ''' Subscribe To Auction Started
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub SubscribeToAuctionStarted()
        ' Create the publishers for the acknowledgement and the AuctionRunning event and bind
        Dim auctionStartedAck As ISendSocket = _context.CreatePublishSocket()
        auctionStartedAck.Bind(ConfigurationManager.AppSettings("auctionStartedAckAddr"))
        Dim auctionRunningPub As ISendSocket = _context.CreatePublishSocket()
        auctionRunningPub.Bind(ConfigurationManager.AppSettings("auctionRunningPubAddr"))
        ' Create the subscriber for AuctionStarted and connect
        Dim auctionStartedSub As ISubscribeSocket = _context.CreateSubscribeSocket()
        auctionStartedSub.Connect(ConfigurationManager.AppSettings("auctionStartedSubAddr"))
        ' Set the topic - AuctionStarted
        Dim auctionStartedTopic As String = ConfigurationManager.AppSettings("auctionStartedTopic")
        auctionStartedSub.Subscribe(System.Text.Encoding.ASCII.GetBytes(auctionStartedTopic))
        Console.WriteLine("SUB: " & auctionStartedTopic)

        While (True)
            ' Receive the event and acknowledge
            Dim auctionStartedEvt As String = Encoding.ASCII.GetString(auctionStartedSub.Receive())
            Console.WriteLine("REC: " & auctionStartedEvt)
            PublishAuctionStartedAck(auctionStartedAck, auctionStartedEvt)
            ' Extract the ID and publish an AuctionRunning event
            Dim id As String = ParseMessage(auctionStartedEvt, "<id>", "</id>")
            PublishAuctionRunningEvent(auctionRunningPub, id)
            ' Create a new AuctionRunner instance on a separate thread and run the auction (allows concurrent auctions)
            Dim auctionRunner As New AuctionRunner(id, _publisher)
            _auctions.Add(id, auctionRunner)
            Dim runAuctionThread As New Thread(AddressOf auctionRunner.RunAuction)
            runAuctionThread.Start()
        End While
    End Sub

    ''' <summary>
    ''' Subscribe To Auction Running Acknowledgement
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub SubscribeToAuctionRunningAck()
        ' Create the subscriber, connect to the address, and set the topic - ACK AuctionRunning
        Dim auctionRunningAckSub As ISubscribeSocket = _context.CreateSubscribeSocket()
        auctionRunningAckSub.Connect(ConfigurationManager.AppSettings("auctionRunningAckAddr"))
        auctionRunningAckSub.Subscribe(Encoding.ASCII.GetBytes(ConfigurationManager.AppSettings("auctionRunningAckTopic")))

        While (True)
            Console.WriteLine("REC: " & Encoding.ASCII.GetString(auctionRunningAckSub.Receive()))
        End While
    End Sub

    ''' <summary>
    ''' Subscribe To Bid Placed
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub SubscribeToBidPlaced()
        ' Create the subscriber, connect, and set the topic - BidPlaced
        Dim subscriber As ISubscribeSocket = _context.CreateSubscribeSocket()
        subscriber.Connect(ConfigurationManager.AppSettings("bidPlacedAddr"))
        Dim bidPlacedTopic As String = ConfigurationManager.AppSettings("bidPlacedTopic")
        subscriber.Subscribe(Encoding.ASCII.GetBytes(bidPlacedTopic))
        Console.WriteLine("SUB: " & bidPlacedTopic)
        ' Create the acknowledgement publisher
        Dim bidPlacedAck As ISendSocket = _context.CreatePublishSocket()
        bidPlacedAck.Bind(ConfigurationManager.AppSettings("bidPlacedAckAddr"))

        While (True)
            ' Receive and acknowledge
            Dim bidPlacedEvt As String = Encoding.ASCII.GetString(subscriber.Receive())
            Console.WriteLine("REC: " & bidPlacedEvt)
            PublishBidPlacedAck(bidPlacedAck, bidPlacedEvt)
            ' Extract the details and publish an AuctionOver event
            Dim id As String = ParseMessage(bidPlacedEvt, "<id>", "</id>")
            Dim winner As String = ParseMessage(bidPlacedEvt, "<params>", "</params>")
            PublishAuctionOverEvt(id, winner)
        End While
    End Sub

    ''' <summary>
    ''' Subscribe To Bid Changed Acknowledgement
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub SubscribeToBidChangedAck()
        Dim bidChangedAckSub As ISubscribeSocket = _context.CreateSubscribeSocket()
        ' Connect to the address and subscribe to the topic - ACK BidChanged
        bidChangedAckSub.Connect(ConfigurationManager.AppSettings("bidChangedAckAddr"))
        bidChangedAckSub.Subscribe(Encoding.ASCII.GetBytes(ConfigurationManager.AppSettings("bidChangedAckTopic")))

        While (True)
            Console.WriteLine("REC: " & Encoding.ASCII.GetString(bidChangedAckSub.Receive()))
        End While
    End Sub

    ''' <summary>
    ''' Subscribe To Auction Over Acknowledgement
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub SubscribeToAuctionOverAck()
        Dim auctionOverAckSub As ISubscribeSocket = _context.CreateSubscribeSocket()
        ' Connect to the address and subscribe to the topic - ACK AuctionOver
        auctionOverAckSub.Connect(ConfigurationManager.AppSettings("auctionOverAckAddr"))
        auctionOverAckSub.Subscribe(System.Text.Encoding.ASCII.GetBytes(ConfigurationManager.AppSettings("auctionOverAckTopic")))

        While (True)
            Dim message As String = Encoding.ASCII.GetString(auctionOverAckSub.Receive())
            Dim id As String = ParseMessage(message, "<id>", "</id>")
            Console.WriteLine("REC: " & message)
        End While
    End Sub

    ''' <summary>
    ''' Subscribe To Heartbeat
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub SubscribeToHeartbeat()
        Dim heartbeatSub As ISubscribeSocket = _context.CreateSubscribeSocket()
        ' Connect to the address and subscribe to the topic - CheckHeartbeat
        heartbeatSub.Connect(ConfigurationManager.AppSettings("heartbeatSubAddr"))
        heartbeatSub.Subscribe(System.Text.Encoding.ASCII.GetBytes(ConfigurationManager.AppSettings("checkHeartbeatTopic")))

        While (True)
            ' Receive the command and publish the response - Ok
            Console.WriteLine("REC: " & Encoding.ASCII.GetString(heartbeatSub.Receive()))
            Dim message As String = String.Concat(ConfigurationManager.AppSettings("checkHeartbeatTopicResponse"), " <params>",
                    ConfigurationManager.AppSettings("serviceName"), "</params>")
            _publisher.Send(Encoding.ASCII.GetBytes(message))
            Console.WriteLine("PUB: " & message)
        End While
    End Sub

#End Region

#Region "Publisher Code"

    ''' <summary>
    ''' Publish Auction Started Acknowledgement
    ''' </summary>
    ''' <param name="auctionStartedAck">The publisher object</param>
    ''' <param name="message">The message to acknowledge</param>
    ''' <remarks></remarks>
    Private Sub PublishAuctionStartedAck(ByVal auctionStartedAck As ISendSocket, ByVal message As String)
        Dim msg As String = "ACK " & message
        auctionStartedAck.Send(Encoding.ASCII.GetBytes(msg))
        Console.WriteLine("PUB: " & msg)
    End Sub

    ''' <summary>
    ''' Publish Auction Running Event
    ''' </summary>
    ''' <param name="auctionRunningPub">The publisher object</param>
    ''' <param name="id">The ID of the auction</param>
    ''' <remarks></remarks>
    Private Sub PublishAuctionRunningEvent(ByVal auctionRunningPub As ISendSocket, ByVal id As String)
        Dim auctionRunningEvt As String = String.Concat(
            ConfigurationManager.AppSettings("auctionRunningTopic"), " <id>", id, "</id>")
        auctionRunningPub.Send(Encoding.ASCII.GetBytes(auctionRunningEvt))
        Console.WriteLine("PUB: " & auctionRunningEvt)
    End Sub

    ''' <summary>
    ''' Publish Bid Placed Acknowledgement
    ''' </summary>
    ''' <param name="bidPlacedAckPub">The publisher object</param>
    ''' <param name="message">The message to acknowledge</param>
    ''' <remarks></remarks>
    Private Sub PublishBidPlacedAck(ByVal bidPlacedAckPub As ISendSocket, ByVal message As String)
        Dim bidPlacedAck As String = "ACK " & message
        bidPlacedAckPub.Send(Encoding.ASCII.GetBytes(bidPlacedAck))
        Console.WriteLine("PUB: " & bidPlacedAck)
    End Sub

    ''' <summary>
    ''' Publish Bid Changed Event
    ''' </summary>
    ''' <param name="id">The ID of the auction</param>
    ''' <param name="currentBid">The current bid</param>
    ''' <remarks></remarks>
    Public Sub PublishBidChangedEvt(ByVal id As String, ByVal currentBid As String)
        Dim bidChangedEvt As String = String.Concat(
            ConfigurationManager.AppSettings("bidChangedTopic"), " <id>", id,
            "</id> ", "<params>", currentBid, "</params>")
        _publisher.Send(Encoding.ASCII.GetBytes(bidChangedEvt))
        Console.WriteLine("PUB: " & bidChangedEvt)
    End Sub

    ''' <summary>
    ''' Publish Auction Over Event
    ''' </summary>
    ''' <param name="id">The ID of the auction</param>
    ''' <param name="winner">The winner of the auction</param>
    ''' <remarks></remarks>
    Public Sub PublishAuctionOverEvt(ByVal id As String, ByVal winner As String)
        Dim auctionOverEvt As String = String.Concat(ConfigurationManager.AppSettings("auctionOverTopic"), " <id>", id,
                    "</id>", " <params>", winner, "</params>")
        _publisher.Send(Encoding.ASCII.GetBytes(auctionOverEvt))
        Console.WriteLine("PUB: " & auctionOverEvt)
        ' Set BidNotPlaced of the auction to false and remove the auction
        If (_auctions.ContainsKey(id)) Then
            _auctions(id).BidNotPlaced = False
            _auctions.Remove(id)
        End If
    End Sub

#End Region
    
End Module
