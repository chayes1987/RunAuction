Imports DreamSeat
Imports System.Configuration

' Author - Conor Hayes
' Coding Standards -> https://msdn.microsoft.com/en-us/library/h63fsef3.aspx & Work Placement
' CouchDB -> https://github.com/vdaron/DreamSeat

''' <summary>
''' CouchDB Database Implements IDatabase
''' </summary>
''' <remarks></remarks>
Public Class CouchDbDatabase
    Implements IDatabase

    ''' <summary>
    ''' Get Item
    ''' </summary>
    ''' <param name="id">The ID of the auction</param>
    ''' <returns>The item matching the ID</returns>
    ''' <remarks></remarks>
    Public Function getItem(ByVal id As String) As AuctionItem Implements IDatabase.GetItem
        Dim item As AuctionItem = Nothing
        ' Connect to the database and get the item
        Try
            Dim client As New CouchClient()
            Dim db As CouchDatabase = client.GetDatabase(
                ConfigurationManager.AppSettings("dbName"))
            item = db.GetDocument(Of AuctionItem)(id)
        Catch ex As Exception
            Console.WriteLine("Error loading item")
            Return Nothing
        End Try
        Return item
    End Function

End Class
