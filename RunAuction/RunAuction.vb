Imports DreamSeat
Imports ZeroMQ
Imports ZeroMQ.Sockets
Imports System.Threading


Public Module RunAuction
    Private Const DB_NAME As String = "auctions"
    Private context As IZmqContext = ZmqContext.Create()
    Private auctionRunning As ISendSocket

    Public Sub Main()
        Dim client As New CouchClient()
        Dim db As CouchDatabase = client.GetDatabase(DB_NAME)

        Dim subscribeToAuctionStartedThread As New Thread(AddressOf SubscribeToAuctionStarted)
        subscribeToAuctionStartedThread.Start()
    End Sub

    Private Sub SubscribeToAuctionStarted()
        Dim subscriber As ISubscribeSocket = context.CreateSubscribeSocket()
        subscriber.Connect("tcp://127.0.0.1:1001")
        Dim prefix As Byte() = System.Text.Encoding.ASCII.GetBytes("AuctionStarted")
        subscriber.Subscribe(prefix)
        Console.WriteLine("Subscribed to AuctionStarted event...")

        While (True)
            Dim message As Byte() = subscriber.Receive()
            Dim receiveStr As String = System.Text.Encoding.ASCII.GetString(message)
            Console.WriteLine(receiveStr)
        End While
    End Sub

End Module
