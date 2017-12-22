Imports System.Text
Imports System.Xml
Imports System.Xml.Serialization
Imports Windows.Management.Deployment
Imports Windows.Storage.Streams

Public Class Helpers

    Public Shared Function TimeSpanToText(ByVal value As DateTime) As String
        Dim _timeRemaining As TimeSpan = DateTime.Now - CDate(value)

        If _timeRemaining.Days > 365 Then
            Return String.Format("Before {0} years", Math.Round(_timeRemaining.Days / 365))
        ElseIf _timeRemaining.Days > 31 Then
            Return String.Format("Before {0} month", Math.Round(_timeRemaining.Days / 31))
        ElseIf _timeRemaining.Days <> 0 Then
            Return String.Format("Before {0} days", _timeRemaining.Days)
        ElseIf _timeRemaining.Hours <> 0 Then
            Return String.Format("Before {0} hours", _timeRemaining.Hours)
        ElseIf _timeRemaining.Minutes <> 0 Then
            Return String.Format("Before {0} minutes", _timeRemaining.Minutes)
        Else
            Return String.Format("Before {0} seconds", _timeRemaining.Seconds)
        End If
    End Function

    Public Shared Function ToXml(value As Object, _type As Type) As String
        Dim serializer As New XmlSerializer(_type)
        Dim stringBuilder As New StringBuilder()
        Dim settings As New XmlWriterSettings() With {
        .Indent = True,
        .OmitXmlDeclaration = True
    }


        Using xmlWriter As XmlWriter = XmlWriter.Create(stringBuilder, settings)
            serializer.Serialize(xmlWriter, value)
        End Using
        Return stringBuilder.ToString()
    End Function

    Public Shared Function FromXml(xml As String, _type As Type) As Object
        Dim serializer As New XmlSerializer(_type)
        Dim deserialized As Object = Nothing
        Using stringReader As New StringReader(xml)
            deserialized = serializer.Deserialize(stringReader)
        End Using

        Return deserialized
    End Function

    Public Shared Function SplitByLength(ByVal mString As String, ByVal mSplitAt As Integer, Optional ByVal mMaxListLength As Integer = 9999) As List(Of String)
        Dim mStringList As New List(Of String)
        Dim mElement As String
        Try
            If mString.Length <= mSplitAt Then
                mStringList.Add(mString)
                Return mStringList
            End If

            For i = 0 To mString.Length Step mSplitAt
                mElement = mString.Substring(i, Math.Min(mSplitAt, mString.Length - i))
                If mElement <> String.Empty Then
                    If mStringList.Count <= mMaxListLength Then
                        mStringList.Add(mElement)
                    End If
                End If
            Next

            Return mStringList

        Catch ex As Exception
            Return New List(Of String)
        End Try
    End Function

    Public Shared Sub DebugWriter(ByVal sender As Type, ByVal message As String, Optional ByVal type As String = "ERROR")
        Debug.WriteLine($"{type} in {sender.FullName}: {message}")
    End Sub


End Class
