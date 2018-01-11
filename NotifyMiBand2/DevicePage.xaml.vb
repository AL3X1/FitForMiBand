Imports ClassesCollection
Imports Windows.Devices.Bluetooth
Imports Windows.Devices.Enumeration
Imports Windows.System.Threading

Public NotInheritable Class DevicePage
    Inherits Page

    Private mDevice As DeviceInformation
    Private mDevices As DeviceInformationCollection

    Private Async Sub DevicePage_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Dim mSelector = BluetoothLEDevice.GetDeviceSelectorFromPairingState(True)
        mDevices = Await DeviceInformation.FindAllAsync(mSelector)

        App.LocalSettings.Values("DeviceId") = Nothing
        App.LocalSettings.Values("IsAuthenticationNeeded") = True

        ' Identification sequence
        For i = 0 To mDevices.Count - 1
            If mDevices(i).Name.ToUpper = "MI BAND 2" Then
                mDevice = mDevices(i)
                App.LocalSettings.Values("DeviceId") = mDevice.Id
                Exit For
            End If
        Next

        ' Authentication sequence
        If mDevice IsNot Nothing Then
            Me.lblTitle.Text = "Device found"
            Me.lblSubTitle.Text = "Trying to authenticate on Band..."
            Me.lblDescription.Text = "Tap on Band-Button to accept requested authentication!"

            App.CustomMiBand = New CustomMiBand
            If Await App.CustomMiBand.AuthenticateAppOnDevice() Then
                Frame.Navigate(GetType(MainPage))
            Else
                Me.pbProcess.IsIndeterminate = True
                Me.lblTitle.Text = "Authentication failed!"
                Me.lblSubTitle.Text = "Try again later..."
                Me.lblDescription.Text = "Make sure you tapped on Band-Button, do not ignore requested authentication!"
            End If
        Else
            Me.lblTitle.Text = "Band not found"
            Me.lblSubTitle.Text = "Make sure you already have paired your Band with your device..."
            Me.lblDescription.Text = "Make sure that bluetooth-connection are enabled!"
        End If

    End Sub

End Class
