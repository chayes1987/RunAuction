Public Class Constants

    Public Const DB_NAME As String = "auctions"
    Public Const DECREMENT_AMOUNT As Integer = 50
    Public Const AUCTION_STARTED_SUB_ADR As String = "tcp://172.31.32.21:2100"
    Public Const AUCTION_STARTED_ACK_ADR As String = "tcp://*:2300"
    Public Const BID_PLACED_ADR As String = "tcp://172.31.32.29:2900"
    Public Const BID_PLACED_ACK_ADR As String = "tcp://*:2301"
    Public Const AUCTION_EVENT_PUB_ADR As String = "tcp://*:2302"
    Public Const AUCTION_RUNNING_PUB_ADR As String = "tcp://*:2303"
    Public Const AUCTION_RUNNING_ACK_ADR As String = "tcp://172.31.32.24:2400"
    Public Const BID_CHANGED_ACK_ADR As String = "tcp://172.31.32.25:2500"
    Public Const AUCTION_OVER_ACK_ADR As String = "tcp://172.31.32.28:2800"
    Public Const AUCTION_STARTED_TOPIC As String = "AuctionStarted"
    Public Const BID_PLACED_TOPIC As String = "BidPlaced"
    Public Const AUCTION_RUNNING_ACK_TOPIC = "ACK: AuctionRunning"
    Public Const BID_CHANGED_ACK_TOPIC = "ACK: BidChanged"
    Public Const AUCTION_OVER_ACK_TOPIC = "ACK: AuctionOver"
    Public Const WAIT_TIME As Integer = 10000
End Class
