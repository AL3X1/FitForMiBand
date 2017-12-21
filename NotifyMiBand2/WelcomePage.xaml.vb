' Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

Imports ClassesCollection
Imports Windows.ApplicationModel.Core
Imports Windows.Devices.Bluetooth
Imports Windows.Devices.Enumeration
Imports Windows.UI.Popups
''' <summary>
''' Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
''' </summary>
Public NotInheritable Class WelcomePage
    Inherits Page

    Private _readyToSave As Boolean = False

    Public Sub New()

        ' Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent()

        ' Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        Dim Heights As New List(Of CustomComboBoxItem)
        For i = 80 To 240
            Heights.Add(New CustomComboBoxItem(i.ToString & " cm", i.ToString))
        Next
        Me.cboHeight.ItemsSource = Heights
        Me.cboHeight.DisplayMemberPath = "Text"

        Dim Weights As New List(Of CustomComboBoxItem)
        For i = 30 To 170
            Weights.Add(New CustomComboBoxItem(i.ToString & " kg", i.ToString))
        Next
        Me.cboWeight.ItemsSource = Weights
        Me.cboWeight.DisplayMemberPath = "Text"

        Dim Steps As New List(Of CustomComboBoxItem)
        For i = 2000 To 50000 Step 1000
            Steps.Add(New CustomComboBoxItem(i.ToString & " steps", i.ToString))
        Next
        Me.cboSteps.ItemsSource = Steps
        Me.cboSteps.DisplayMemberPath = "Text"

        If App.LocalSettings.Values("Profile_Alias") IsNot Nothing Then
            Me.txtAlias.Text = App.LocalSettings.Values("Profile_Alias").ToString
        End If

        If App.LocalSettings.Values("Profile_Gender") IsNot Nothing Then
            cboGender.SelectedValue = App.LocalSettings.Values("Profile_Gender")
        End If

        If App.LocalSettings.Values("Profile_Height") IsNot Nothing Then
            cboHeight.SelectedItem = Heights.FirstOrDefault(Function(x) x.Value = App.LocalSettings.Values("Profile_Height").ToString)
        End If

        If App.LocalSettings.Values("Profile_Weight") IsNot Nothing Then
            cboWeight.SelectedItem = Weights.FirstOrDefault(Function(x) x.Value = App.LocalSettings.Values("Profile_Weight").ToString)
        End If

        If App.LocalSettings.Values("Profile_Steps") IsNot Nothing Then
            cboSteps.SelectedItem = Steps.FirstOrDefault(Function(x) x.Value = App.LocalSettings.Values("Profile_Steps").ToString)
        End If

        If App.LocalSettings.Values("Profile_DateOfBirth") IsNot Nothing Then
            dtpDateOfBirth.Date = CDate(App.LocalSettings.Values("Profile_DateOfBirth"))
        End If

        If App.LocalSettings.Values("Profile_Sleep") IsNot Nothing Then
            dtpSleep.Time = TimeSpan.Parse(App.LocalSettings.Values("Profile_Sleep").ToString)
        End If

    End Sub

    Private Sub cboGender_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboGender.SelectionChanged
        If cboGender.SelectedItem IsNot Nothing Then
            App.LocalSettings.Values("Profile_Gender") = cboGender.SelectedItem
        End If
    End Sub

    Private Sub cboHeight_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboHeight.SelectionChanged
        If cboHeight.SelectedItem IsNot Nothing Then
            App.LocalSettings.Values("Profile_Height") = DirectCast(cboHeight.SelectedItem, CustomComboBoxItem).Value
        End If
    End Sub

    Private Sub cboSteps_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboSteps.SelectionChanged
        If cboSteps.SelectedItem IsNot Nothing Then
            App.LocalSettings.Values("Profile_Steps") = DirectCast(cboSteps.SelectedItem, CustomComboBoxItem).Value
        End If
    End Sub

    Private Sub cboWeight_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboWeight.SelectionChanged
        If cboWeight.SelectedItem IsNot Nothing Then
            App.LocalSettings.Values("Profile_Weight") = DirectCast(cboWeight.SelectedItem, CustomComboBoxItem).Value
        End If
    End Sub

    Private Sub dtpDateOfBirth_DateChanged(sender As Object, e As DatePickerValueChangedEventArgs) Handles dtpDateOfBirth.DateChanged
        App.LocalSettings.Values("Profile_DateOfBirth") = dtpDateOfBirth.Date.ToString
    End Sub

    Private Sub dtpSleep_TimeChanged(sender As Object, e As TimePickerValueChangedEventArgs) Handles dtpSleep.TimeChanged
        App.LocalSettings.Values("Profile_Sleep") = dtpSleep.Time.ToString
    End Sub

    Private Sub txtAlias_TextChanged(sender As Object, e As TextChangedEventArgs) Handles txtAlias.TextChanged
        App.LocalSettings.Values("Profile_Alias") = Me.txtAlias.Text
    End Sub

    Private Async Sub btnAddDevice_Click(sender As Object, e As RoutedEventArgs)
        Dim _picker As DevicePicker = Nothing
        Dim _pickedDevice As DeviceInformation = Nothing
        Try
            _picker = New DevicePicker
            _picker.Filter.SupportedDeviceSelectors.Add(BluetoothLEDevice.GetDeviceSelectorFromPairingState(True))
            _pickedDevice = Await _picker.PickSingleDeviceAsync(New Rect(0, 0, Window.Current.CoreWindow.Bounds.Width, Window.Current.CoreWindow.Bounds.Height))
            If _pickedDevice IsNot Nothing Then
                App.LocalSettings.Values("DeviceId") = _pickedDevice.Id
                App.LocalSettings.Values("DeviceName") = _pickedDevice.Name

                Me.txtDeviceId.Text = _pickedDevice.Id
                Me.txtDeviceName.Text = _pickedDevice.Name

                Me.btnSave.IsEnabled = True
                _readyToSave = True
            End If

        Catch ex As Exception
            Debug.WriteLine($"Error: {ex.Message}")
            btnSave.IsEnabled = False
            _readyToSave = False
        Finally
            _picker = Nothing
            _pickedDevice = Nothing
        End Try
    End Sub

    Private Async Sub btnSave_Click(sender As Object, e As RoutedEventArgs)
        btnSave.IsEnabled = False
        Try
            If _readyToSave = True Then
                App.LocalSettings.Values("Setting_8") = False
                App.LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Battery))) = True
                App.LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Distance))) = True
                App.LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Steps))) = True
                App.LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Calories))) = True
                App.LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Heartrate))) = True

                App.CustomMiBand = New CustomMiBand
                Dim r = Await App.CustomMiBand.ConnectWithAuth()
                If r = True Then
                    Await App.CustomMiBand.UpdateOperations()
                Else
                    'Device not reachable
                End If

            End If

        Catch ex As Exception

        Finally
            btnSave.IsEnabled = True
        End Try
    End Sub
End Class
