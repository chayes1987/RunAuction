Imports ZeroMQ
Imports ZeroMQ.Sockets
Imports DreamSeat
Imports System.Threading

Public Class AuctionRunner
    Private _currentAuctionId As String
    Private _auctionEventPub As ISendSocket
    Public auctionRunning As Boolean = False


    Public Sub New(ByVal currentAuctionId As String, ByVal auctionEventPub As ISendSocket)
        Me._currentAuctionId = currentAuctionId
        Me._auctionEventPub = auctionEventPub
    End Sub

    Public Sub RunAuction()
        Dim client As New CouchClient()
        Dim db As CouchDatabase = client.GetDatabase(Constants.DB_NAME)
        Dim document As AuctionItem = db.GetDocument(Of AuctionItem)(_currentAuctionId)
        Dim currentBid As Double = document.Starting_Bid
        auctionRunning = True

        While (auctionRunning)
            Thread.Sleep(Constants.WAIT_TIME)

            If (currentBid > document.Estimated_Value) Then
                If (auctionRunning) Then
                    currentBid -= Constants.DECREMENT_AMOUNT
                    Dim bidChangedEvt As String = String.Concat("BidChanged <id>", _currentAuctionId,
                        "</id> ", "<params>", currentBid.ToString, "</params>")
                    _auctionEventPub.Send(System.Text.Encoding.ASCII.GetBytes(bidChangedEvt))
                    Console.WriteLine("PUB: " & bidChangedEvt)
                End If
            Else
                Dim auctionOverEvt As String = String.Concat("AuctionOver <id>", _currentAuctionId,
                    "</id>", " <params>No Winner</params>")
                _auctionEventPub.Send(System.Text.Encoding.ASCII.GetBytes(auctionOverEvt))
                Console.WriteLine("PUB: " & auctionOverEvt)
                auctionRunning = False
                Exit Sub
            End If
        End While
    End Sub

End Class
