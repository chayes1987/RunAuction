Imports DreamSeat.Interfaces

' Author - Conor Hayes
' Coding Standards -> https://msdn.microsoft.com/en-us/library/h63fsef3.aspx & Work Placement

''' <summary>
''' Auction Item Implements ICouchDocument
''' </summary>
''' <remarks></remarks>
Public Class AuctionItem
    Implements ICouchDocument

    ''' <summary>
    ''' Id (Inherited)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Id As String Implements ICouchDocument.Id

    ''' <summary>
    ''' Revision (Inherited)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Rev As String Implements ICouchDocument.Rev

    ''' <summary>
    ''' Starting Bid
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Starting_Bid As String

    ''' <summary>
    ''' Estimated Value
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Estimated_Value As String

End Class