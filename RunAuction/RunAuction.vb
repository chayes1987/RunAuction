Imports Dreamseat

Public Class RunAuction
    Private Const DB_NAME As String = "auctions"

    Public Shared Sub Main()
        Dim client As New CouchClient()
        Dim db As CouchDatabase = client.GetDatabase(DB_NAME)
        Console.WriteLine(db)
        Console.ReadLine()
    End Sub

End Class
