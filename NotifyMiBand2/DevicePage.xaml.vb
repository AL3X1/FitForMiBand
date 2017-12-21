
' Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

Imports ClassesCollection
Imports Windows.ApplicationModel.Core
Imports Windows.Devices.Bluetooth
Imports Windows.Devices.Enumeration
''' <summary>
''' Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
''' </summary>
Public NotInheritable Class DevicePage
    Inherits Page

    Private Sub DevicePage_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        GetDeviceInformations()
    End Sub

    Private Async Sub GetDeviceInformations()
        If App.LocalSettings.Values("DeviceId") IsNot Nothing Then

            If App.LocalSettings.Values("DeviceName") IsNot Nothing Then lblTitle.Text = App.LocalSettings.Values("DeviceName").ToString
            If App.LocalSettings.Values("SoftwareRevision") IsNot Nothing Then lblVersion.Text = App.LocalSettings.Values("SoftwareRevision").ToString

            Await App.CustomMiBand.GetDeviceName()
        Else

            lblTitle.Text = ""
            lblVersion.Text = ""
        End If


    End Sub

    Private Async Sub btnAddBand_Click(sender As Object, e As RoutedEventArgs)
        Dim _picker As DevicePicker = Nothing
        Dim _pickedDevice As DeviceInformation = Nothing
        Try
            _picker = New DevicePicker
            _picker.Filter.SupportedDeviceSelectors.Add(BluetoothLEDevice.GetDeviceSelectorFromPairingState(True))
            _pickedDevice = Await _picker.PickSingleDeviceAsync(New Rect(0, 0, Window.Current.CoreWindow.Bounds.Width, Window.Current.CoreWindow.Bounds.Height))
            If _pickedDevice IsNot Nothing Then

                App.LocalSettings.Values("DeviceId") = _pickedDevice.Id
                App.LocalSettings.Values("DeviceName") = _pickedDevice.Name

                GetDeviceInformations()

            End If

        Catch ex As Exception
            Debug.WriteLine($"Error: {ex.Message}")
        Finally
            _picker = Nothing
            _pickedDevice = Nothing
        End Try
    End Sub

    Private Sub btnReset_Click(sender As Object, e As RoutedEventArgs)
        App.LocalSettings.Values("Setting_8") = True
        App.LocalSettings.Values("DeviceId") = Nothing
        Application.Current.Exit()
    End Sub
End Class
