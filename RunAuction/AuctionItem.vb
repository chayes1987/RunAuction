Imports DreamSeat.Interfaces

Public Class AuctionItem
    Implements ICouchDocument

    Public Property Id As String Implements ICouchDocument.Id
    Public Property Rev As String Implements ICouchDocument.Rev
    Public Property Starting_Bid As String
    Public Property Estimated_Value As String

End Class