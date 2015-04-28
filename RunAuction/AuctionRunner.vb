Imports ZeroMQ
Imports ZeroMQ.Sockets
Imports System.Threading
Imports System.Configuration
Imports System.Text

' Author - Conor Hayes
' Coding Standards -> https://msdn.microsoft.com/en-us/library/h63fsef3.aspx & Work Placement

''' <summary>
''' Auction Runner
''' </summary>
''' <remarks></remarks>
Public Class AuctionRunner

    ''' <summary>
    ''' Current Auction Id
    ''' </summary>
    ''' <remarks></remarks>
    Private _currentAuctionId As String

    ''' <summary>
    ''' Broker
    ''' </summary>
    ''' <remarks></remarks>
    Private _broker As IBroker

    ''' <summary>
    ''' Bid Not Placed
    ''' </summary>
    ''' <remarks></remarks>
    Public BidNotPlaced As Boolean


    ''' <summary>
    ''' Constructor
    ''' </summary>
    ''' <param name="currentAuctionId">The current auction ID</param>
    ''' <param name="broker">Broker object</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal currentAuctionId As String, ByVal broker As IBroker)
        Me._currentAuctionId = currentAuctionId
        Me._broker = broker
    End Sub

    ''' <summary>
    ''' Run Auction
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub RunAuction()
        ' Get the item details using the ID
        Dim database As IDatabase = DatabaseFacade.GetDatabase()
        Dim item = database.getItem(_currentAuctionId)
        If (Not IsNothing(item)) Then
            ' Set the starting bid, time to wait between decrements, and the amount to decrement by
            Dim currentBid As Double = item.Starting_Bid
            Const waitTime As Integer = 10000
            Const decrementAmount As Integer = 50
            BidNotPlaced = True
            ' Run the algorithm
            While (BidNotPlaced)
                Thread.Sleep(waitTime)
                ' Check that the bid is still greater than the actual items value
                If (currentBid > item.Estimated_Value) Then
                    ' Check bid not placed is still true
                    If (BidNotPlaced) Then
                        ' Decrement the bid and inform the bidders
                        currentBid -= decrementAmount
                        _broker.PublishBidChangedEvt(_currentAuctionId, currentBid)
                    End If
                Else
                    ' End the auction
                    _broker.PublishAuctionOverEvt(_currentAuctionId, "No Winner")
                    BidNotPlaced = False
                End If
            End While
        End If
    End Sub

End Class
