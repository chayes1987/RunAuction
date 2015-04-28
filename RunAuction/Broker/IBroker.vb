'Author - Conor Hayes

''' <summary>
''' IBroker Interface
''' </summary>
''' <remarks></remarks>
Public Interface IBroker

    ''' <summary>
    ''' Subscribe To Auction Started
    ''' </summary>
    ''' <remarks></remarks>
    Sub SubscribeToAuctionStarted()

    ''' <summary>
    ''' Subscribe To Bid Placed
    ''' </summary>
    ''' <remarks></remarks>
    Sub SubscribeToBidPlaced()

    ''' <summary>
    ''' Subscribe To Bid Changed Acknowledgement
    ''' </summary>
    ''' <remarks></remarks>
    Sub SubscribeToBidChangedAck()

    ''' <summary>
    ''' Subscribe To Auction Over Acknowledgement
    ''' </summary>
    ''' <remarks></remarks>
    Sub SubscribeToAuctionOverAck()

    ''' <summary>
    ''' Subscribe To Auction Running Acknowledgement
    ''' </summary>
    ''' <remarks></remarks>
    Sub SubscribeToAuctionRunningAck()

    ''' <summary>
    ''' Subscribe To Heartbeat
    ''' </summary>
    ''' <remarks></remarks>
    Sub SubscribeToHeartbeat()

    ''' <summary>
    ''' Publish Auction Started Acknowledgement
    ''' </summary>
    ''' <param name="message">The message to acknowledge</param>
    ''' <remarks></remarks>
    Sub PublishAuctionStartedAck(ByVal message As String)

    ''' <summary>
    ''' Publish Auction Running Event
    ''' </summary>
    ''' <param name="id">The ID of the auction</param>
    ''' <remarks></remarks>
    Sub PublishAuctionRunningEvent(ByVal id As String)

    ''' <summary>
    ''' Publish Bid Placed Acknowledgement
    ''' </summary>
    ''' <param name="message">The message to acknowledge</param>
    ''' <remarks></remarks>
    Sub PublishBidPlacedAck(ByVal message As String)

    ''' <summary>
    ''' Publish Bid Changed Event
    ''' </summary>
    ''' <param name="id">The ID of the auction</param>
    ''' <param name="currentBid">The current bid</param>
    ''' <remarks></remarks>
    Sub PublishBidChangedEvt(ByVal id As String, ByVal currentBid As String)

    ''' <summary>
    ''' Publish Auction Over Event
    ''' </summary>
    ''' <param name="id">The ID of the auction</param>
    ''' <param name="winner">The winner of the auction</param>
    ''' <remarks></remarks>
    Sub PublishAuctionOverEvt(ByVal id As String, ByVal winner As String)

End Interface
