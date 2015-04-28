' Author - Conor Hayes

''' <summary>
''' Database Facade
''' </summary>
''' <remarks></remarks>
Public Class DatabaseFacade

    ''' <summary>
    ''' Database
    ''' </summary>
    ''' <remarks></remarks>
    Private Shared database As IDatabase


    ''' <summary>
    ''' Get Database
    ''' </summary>
    ''' <returns>The Database</returns>
    ''' <remarks></remarks>
    Public Shared Function GetDatabase() As IDatabase
        If (Constants.DATABASE = DatabaseType.CouchDB) Then
            database = New CouchDbDatabase()
        End If
        ' Others
        Return database
    End Function

End Class
