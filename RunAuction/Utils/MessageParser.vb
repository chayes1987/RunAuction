' Author - Conor Hayes

''' <summary>
''' Message Parser
''' </summary>
''' <remarks></remarks>
Public Class MessageParser

    ''' <summary>
    ''' Parse Message
    ''' </summary>
    ''' <param name="message">The message to parse</param>
    ''' <param name="startTag">The start delimiter</param>
    ''' <param name="endTag">The end delimiter</param>
    ''' <returns>The string required</returns>
    ''' <remarks></remarks>
    Public Shared Function ParseMessage(ByVal message As String, ByVal startTag As String, ByVal endTag As String) As String
        Dim startIndex As Integer = message.IndexOf(startTag) + startTag.Length
        Dim substring As String = message.Substring(startIndex)
        Return substring.Substring(0, substring.LastIndexOf(endTag))
    End Function

End Class
