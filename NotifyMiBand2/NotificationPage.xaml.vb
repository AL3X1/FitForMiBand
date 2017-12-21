' Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

Imports ClassesCollection
''' <summary>
''' Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
''' </summary>
Public NotInheritable Class NotificationPage
    Inherits Page

    Private bolLoading As Boolean = True

    Private _Result As NotificationResult
    Public Property Result() As NotificationResult
        Get
            Return _Result
        End Get
        Set(ByVal value As NotificationResult)
            _Result = value
        End Set
    End Property

    Protected Overrides Sub OnNavigatedTo(e As NavigationEventArgs)
        _Result = TryCast(e.Parameter, NotificationResult)
    End Sub

    Private Sub ToggleSwitch_Toggled(sender As Object, e As RoutedEventArgs)
        If bolLoading = False Then
            DirectCast(DirectCast(sender, ToggleSwitch).DataContext, NotificationRequest).IsOn = DirectCast(sender, ToggleSwitch).IsOn
        End If
    End Sub

    Private Sub NotificationPage_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Me.lvApps.ItemsSource = Nothing
        Me.lvApps.ItemsSource = _Result.Requests
        bolLoading = False
    End Sub
End Class
