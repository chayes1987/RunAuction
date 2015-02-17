Public Class Constants

    Public Const DB_NAME As String = "auctions"
    Public Const DECREMENT_AMOUNT As Integer = 50
    Public Const AUCTION_STARTED_SUB_ADR As String = "tcp://172.31.32.21:1011"
    Public Const AUCTION_STARTED_ACK_ADR As String = "tcp://*:1100"
    Public Const BID_PLACED_ADR As String = "tcp://172.31.32.29:1101"
    Public Const BID_PLACED_ACK_ADR As String = "tcp://*:1110"
    Public Const AUCTION_EVENT_PUB_ADR As String = "tcp://*:1111"
    Public Const AUCTION_RUNNING_ACK_ADR As String = "tcp://172.31.32.24:2000"
    Public Const BID_CHANGED_ACK_ADR As String = "tcp://172.31.32.25:2001"
    Public Const AUCTION_OVER_ACK_ADR As String = "tcp://172.31.32.28:2010"
    Public Const AUCTION_STARTED_TOPIC As String = "AuctionStarted"
    Public Const BID_PLACED_TOPIC As String = "BidPlaced"
    Public Const AUCTION_RUNNING_ACK_TOPIC = "ACK: AuctionRunning"
    Public Const BID_CHANGED_ACK_TOPIC = "ACK: BidChanged"
    Public Const AUCTION_OVER_ACK_TOPIC = "ACK: AuctionOver"
    Public Const WAIT_TIME As Integer = 10000
End Class
