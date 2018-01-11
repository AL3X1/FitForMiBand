' Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

Imports ClassesCollection
Imports Windows.ApplicationModel.Core
''' <summary>
''' Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
''' </summary>
Public NotInheritable Class Splash
    Inherits Page

    Private Async Sub Splash_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        App.CustomMiBand = New CustomMiBand

        If App.LocalSettings.Values("DeviceId") Is Nothing Then
            Frame.Navigate(GetType(DevicePage))
        Else
            Await App.CustomMiBand.AuthenticateAppOnDevice
            Frame.Navigate(GetType(MainPage))
        End If

    End Sub
End Class
