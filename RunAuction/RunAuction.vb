Imports ZeroMQ
Imports ZeroMQ.Sockets
Imports System.Threading
Imports System.Configuration
Imports System.Text


' Author - Conor Hayes
' Coding Standards -> https://msdn.microsoft.com/en-us/library/h63fsef3.aspx & Work Placement

''' <summary>
''' Run Auction
''' </summary>
''' <remarks></remarks>
Public Module RunAuction

    ''' <summary>
    ''' Main
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Main()
        Dim broker As IBroker = BrokerFacade.GetBroker()

        ' Initialize publishers and subscribers
        Dim subToBidPlacedThread As New Thread(AddressOf broker.SubscribeToBidPlaced)
        subToBidPlacedThread.Start()
        Dim subToBidPlacedAckThread As New Thread(AddressOf broker.SubscribeToBidChangedAck)
        subToBidPlacedAckThread.Start()
        Dim subToAuctionOverAckThread As New Thread(AddressOf broker.SubscribeToAuctionOverAck)
        subToAuctionOverAckThread.Start()
        Dim subToAuctionRunningAckThread As New Thread(AddressOf broker.SubscribeToAuctionRunningAck)
        subToAuctionRunningAckThread.Start()
        Dim subToHeartbeatThread As New Thread(AddressOf broker.SubscribeToHeartbeat)
        subToHeartbeatThread.Start()
        broker.SubscribeToAuctionStarted()
    End Sub
    
End Module
