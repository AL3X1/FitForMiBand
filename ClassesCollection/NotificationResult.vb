Imports Windows.UI.Notifications

Public Class NotificationResult

    Private _Requests As List(Of NotificationRequest)
    Public Property Requests() As List(Of NotificationRequest)
        Get
            Return _Requests
        End Get
        Set(ByVal value As List(Of NotificationRequest))
            _Requests = value
        End Set
    End Property

    Private LocalSettings As Windows.Storage.ApplicationDataContainer = Windows.Storage.ApplicationData.Current.LocalSettings

    Public Sub New()
        _Requests = New List(Of NotificationRequest)
    End Sub

    Public Function Initialize() As Boolean
        Dim _data As NotificationResult = Nothing
        Try
            If LocalSettings.Values("NotificationResult") IsNot Nothing Then
                _data = DirectCast(Helpers.FromXml(LocalSettings.Values("NotificationResult").ToString, GetType(NotificationResult)), NotificationResult)

                _Requests = _data.Requests

                Return True
            End If

            Return False

        Catch ex As Exception
            Return False
        Finally
            _data = Nothing
        End Try
    End Function

    Public Sub Add(Appinfo As AppInfo)
        Try
            _Requests.Add(New NotificationRequest(Appinfo))

            LocalSettings.Values("NotificationResult") = Helpers.ToXml(Me, GetType(NotificationResult))

        Catch ex As Exception
        End Try
    End Sub

End Class
