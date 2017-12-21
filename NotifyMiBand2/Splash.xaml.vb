' Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

Imports ClassesCollection
Imports Windows.ApplicationModel.Core
''' <summary>
''' Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
''' </summary>
Public NotInheritable Class Splash
    Inherits Page

    Private Sub Splash_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        App.CustomMiBand = New CustomMiBand
        Frame.Navigate(GetType(MainPage))
    End Sub
End Class
