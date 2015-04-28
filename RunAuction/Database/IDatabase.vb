' Author - Conor Hayes

''' <summary>
''' IDatabase Interface
''' </summary>
''' <remarks></remarks>
Public Interface IDatabase

    ''' <summary>
    ''' Get Item
    ''' </summary>
    ''' <param name="Id">The Auction ID</param>
    ''' <returns>The corresponding item</returns>
    ''' <remarks></remarks>
    Function GetItem(ByVal Id As String) As AuctionItem

End Interface
