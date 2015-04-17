Imports DreamSeat
Imports System.Configuration


Public Class DatabaseManager

    Public Shared Function getItem(ByVal id As String) As AuctionItem
        Dim item As AuctionItem = Nothing
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
