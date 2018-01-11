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
            Me.pbProcess.IsIndeterminate = False
            Me.pbProcess.Maximum = 10
            Me.pbProcess.Minimum = 0

            Dim delay As TimeSpan = TimeSpan.FromSeconds(1)
            Dim delayTimer As ThreadPoolTimer = ThreadPoolTimer.CreateTimer(Async Sub()
                                                                                Await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, Sub()
                                                                                                                                                             Me.pbProcess.Value += 1
                                                                                                                                                         End Sub)
                                                                            End Sub, delay)

            App.CustomMiBand = New CustomMiBand
            If Await App.CustomMiBand.AuthenticateAppOnDevice() Then
                delayTimer.Cancel()
                ' Navigate to somewhere, everythings fine...
            Else
                ' Display Problems...
                delayTimer.Cancel()
                Me.pbProcess.IsIndeterminate = True
                Me.lblTitle.Text = "Authentication failed!"
                Me.lblSubTitle.Text = "Try again later..."
                Me.lblDescription.Text = "Make sure you tapped on Band-Button, do not ignore requested authentication!"
            End If
        End If

    End Sub

End Class
