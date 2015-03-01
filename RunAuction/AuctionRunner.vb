Imports ZeroMQ
Imports ZeroMQ.Sockets
Imports DreamSeat
Imports System.Threading
Imports System.Configuration
Imports System.Text


Public Class AuctionRunner
    Private _currentAuctionId As String
    Private _auctionEventPub As ISendSocket
    Private _bidNotPlaced As Boolean
    Private _auctions As New Dictionary(Of String, AuctionRunner)


    Public Sub New(ByVal currentAuctionId As String, ByVal auctionEventPub As ISendSocket)
        _auctions.Add(currentAuctionId, Me)
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
        _bidNotPlaced = True

        While (_bidNotPlaced)
            Thread.Sleep(waitTime)

            If (currentBid > document.Estimated_Value) Then
                If (_bidNotPlaced) Then
                    currentBid -= decrementAmount
                    PublishBidChangedEvt(currentBid)
                End If
            Else
                _auctions.Remove(_currentAuctionId)
                PublishAuctionOverEvt("No Winner")
                _bidNotPlaced = False
            End If
        End While
    End Sub

    Public Sub BidPlaced(ByVal id As String, ByVal winner As String)
        If (_auctions.ContainsKey(id)) Then
            _auctions(id)._bidNotPlaced = False
            _auctions.Remove(id)
            PublishAuctionOverEvt(winner)
        End If
    End Sub

    Private Sub PublishBidChangedEvt(ByVal currentBid As String)
        Dim bidChangedEvt As String = String.Concat(ConfigurationManager.AppSettings("bidChangedTopic"), " <id>", _currentAuctionId,
                        "</id> ", "<params>", currentBid, "</params>")
        _auctionEventPub.Send(Encoding.ASCII.GetBytes(bidChangedEvt))
        Console.WriteLine("PUB: " & bidChangedEvt)
    End Sub

    Private Sub PublishAuctionOverEvt(ByVal winner As String)
        Dim auctionOverEvt As String = String.Concat(ConfigurationManager.AppSettings("auctionOverTopic"), " <id>", _currentAuctionId,
                    "</id>", " <params>", winner, "</params>")
        _auctionEventPub.Send(Encoding.ASCII.GetBytes(auctionOverEvt))
        Console.WriteLine("PUB: " & auctionOverEvt)
    End Sub

End Class
