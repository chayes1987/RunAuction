' Author - Conor Hayes

''' <summary>
''' Broker Facade
''' </summary>
''' <remarks></remarks>
Public Class BrokerFacade

    ''' <summary>
    ''' Broker
    ''' </summary>
    ''' <remarks></remarks>
    Private Shared broker As IBroker


    ''' <summary>
    ''' Get Broker
    ''' </summary>
    ''' <returns>The Broker</returns>
    ''' <remarks></remarks>
    Public Shared Function GetBroker() As IBroker
        If (Constants.BROKER = MessageBroker.ZeroMq) Then
            broker = New ZeroMqBroker()
        End If
        ' Other Brokers may be added
        Return broker
    End Function

End Class
