' Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

Imports ClassesCollection

''' <summary>
''' Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
''' </summary>
Public NotInheritable Class DisplayitemsPage
    Inherits Page

    Public Sub New()

        InitializeComponent()

        If App.LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Battery))) IsNot Nothing Then tsBattery.IsOn = Convert.ToBoolean(App.LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Battery))))
        If App.LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Distance))) IsNot Nothing Then tsDistance.IsOn = Convert.ToBoolean(App.LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Distance))))
        If App.LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Steps))) IsNot Nothing Then tsSteps.IsOn = Convert.ToBoolean(App.LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Steps))))
        If App.LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Calories))) IsNot Nothing Then tsCalories.IsOn = Convert.ToBoolean(App.LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Calories))))
        If App.LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Heartrate))) IsNot Nothing Then tsHeartrate.IsOn = Convert.ToBoolean(App.LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Heartrate))))
    End Sub

    Private Async Sub btnSave_Click(sender As Object, e As RoutedEventArgs)
        pbProcessing.Visibility = Visibility.Visible
        btnSave.IsEnabled = False
        Try
            App.LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Battery))) = tsBattery.IsOn
            App.LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Distance))) = tsDistance.IsOn
            App.LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Steps))) = tsSteps.IsOn
            App.LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Calories))) = tsCalories.IsOn
            App.LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Heartrate))) = tsHeartrate.IsOn

            Await App.CustomMiBand.setDisplayItems()

            If Frame.CanGoBack Then
                Frame.GoBack()
            End If

        Catch ex As Exception
            Helpers.DebugWriter(GetType(DisplayitemsPage), ex.Message)
        Finally
            pbProcessing.Visibility = Visibility.Collapsed
            btnSave.IsEnabled = True
        End Try
    End Sub
End Class
