Imports ZeroMQ
Imports ZeroMQ.Sockets
Imports DreamSeat
Imports System.Threading
Imports System.Configuration
Imports System.Text


Public Class AuctionRunner
    Private _currentAuctionId As String
    Private _auctionEventPub As ISendSocket
    Public BidNotPlaced As Boolean


    Public Sub New(ByVal currentAuctionId As String, ByVal auctionEventPub As ISendSocket)
        Me._currentAuctionId = currentAuctionId
        Me._auctionEventPub = auctionEventPub
    End Sub

    Public Sub RunAuction()
        Dim client As New CouchClient()
        Dim db As CouchDatabase = client.GetDatabase(ConfigurationManager.AppSettings("dbName"))
        Dim document As AuctionItem = db.GetDocument(Of AuctionItem)(_currentAuctionId)
        Dim currentBid As Double = document.Starting_Bid
        Const waitTime As Integer = 10000
        Const decrementAmount As Integer = 50
        BidNotPlaced = True

        While (BidNotPlaced)
            Thread.Sleep(waitTime)

            If (currentBid > document.Estimated_Value) Then
                If (BidNotPlaced) Then
                    currentBid -= decrementAmount
                    PublishBidChangedEvt(_currentAuctionId, currentBid)
                End If
            Else
                PublishAuctionOverEvt(_currentAuctionId, "No Winner")
                BidNotPlaced = False
            End If
        End While
    End Sub

End Class
